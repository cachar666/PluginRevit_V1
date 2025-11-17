using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SINCO.ADPRO.Plugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SINCO.ADPRO.Plugin.Services
{
    /// <summary>
    /// Servicio para extraer datos de elementos de Revit (solo vista activa)
    /// </summary>
    public class DataExtractionService
    {
        private readonly Document _document;
        private readonly UIDocument _uidocument;

        public DataExtractionService(Document document, UIDocument uidocument)
        {
            _document = document;
            _uidocument = uidocument;
        }

        /// <summary>
        /// Extrae datos de los elementos según las categorías y propiedades seleccionadas
        /// Incluye elementos de la vista activa Y materiales como registros separados
        /// </summary>
        public List<Dictionary<string, string>> ExtractData(
            IEnumerable<CategoryNode> selectedCategories,
            IEnumerable<PropertyItem> selectedProperties,
            FilterType filterType)
        {
            var results = new List<Dictionary<string, string>>();

            try
            {
                // Obtener la vista activa
                View activeView = _uidocument.ActiveView;
                if (activeView == null)
                {
                    throw new Exception("No hay una vista activa");
                }

                // Obtener las familias seleccionadas
                var selectedFamilies = selectedCategories
                    .Where(c => c.IsCategory && c.IsSelected)
                    .SelectMany(c => c.Children.Where(f => f.IsSelected && !f.IsMaterial))
                    .ToList();

                // Obtener las categorías sin hijos seleccionadas
                var selectedCategoriesOnly = selectedCategories
                    .Where(c => c.IsCategory && c.IsSelected && !c.Children.Any(f => f.IsSelected))
                    .ToList();

                // Crear colector solo con elementos visibles en la vista activa
                FilteredElementCollector collector = new FilteredElementCollector(_document, activeView.Id)
                    .WhereElementIsNotElementType();

                // Conjunto para rastrear materiales únicos procesados
                HashSet<ElementId> processedMaterials = new HashSet<ElementId>();
                Dictionary<ElementId, MaterialData> materialQuantities = new Dictionary<ElementId, MaterialData>();

                // PASO 1: Extraer elementos y acumular datos de materiales
                foreach (var element in collector)
                {
                    if (element.Category == null)
                        continue;

                    string categoryName = element.Category.Name;
                    ElementType elementType = _document.GetElement(element.GetTypeId()) as ElementType;

                    if (elementType == null)
                        continue;

                    string familyName = elementType.FamilyName;

                    // Verificar si el elemento pertenece a una familia seleccionada
                    bool isSelected = false;

                    if (selectedFamilies.Any(f => f.Parent.Name == categoryName && f.Name == familyName))
                    {
                        isSelected = true;
                    }
                    else if (selectedCategoriesOnly.Any(c => c.Name == categoryName))
                    {
                        isSelected = true;
                    }

                    if (!isSelected)
                        continue;

                    // Extraer propiedades del elemento
                    var elementData = ExtractElementProperties(element, elementType, selectedProperties, null);
                    results.Add(elementData);

                    // Acumular datos de materiales de este elemento
                    AccumulateMaterialData(element, elementType, materialQuantities);
                }

                // PASO 2: Agregar registros de materiales que cumplan con el filtro
                foreach (var materialData in materialQuantities.Values)
                {
                    Material material = _document.GetElement(materialData.MaterialId) as Material;
                    if (material == null)
                        continue;

                    // Obtener Keynote y AssemblyCode del material
                    string keynote = GetParameterValue(material, BuiltInParameter.KEYNOTE_PARAM);
                    string assemblyCode = GetParameterValue(material, BuiltInParameter.UNIFORMAT_CODE);

                    bool hasKeynote = !string.IsNullOrWhiteSpace(keynote);
                    bool hasAssemblyCode = !string.IsNullOrWhiteSpace(assemblyCode);

                    // Aplicar filtro
                    if (!PassesFilter(hasKeynote, hasAssemblyCode, filterType))
                        continue;

                    // Crear registro de material
                    var materialRecord = CreateMaterialRecord(material, materialData, selectedProperties, keynote, assemblyCode);
                    results.Add(materialRecord);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al extraer datos: {ex.Message}", ex);
            }

            return results;
        }

        /// <summary>
        /// Acumula datos de materiales de un elemento (volumen, área)
        /// </summary>
        private void AccumulateMaterialData(Element element, ElementType elementType, Dictionary<ElementId, MaterialData> materialQuantities)
        {
            try
            {
                // Obtener geometría del elemento
                Options geoOptions = new Options();
                geoOptions.DetailLevel = ViewDetailLevel.Fine;
                geoOptions.ComputeReferences = false;
                GeometryElement geoElement = element.get_Geometry(geoOptions);

                if (geoElement != null)
                {
                    foreach (GeometryObject geoObj in geoElement)
                    {
                        ProcessGeometryObject(geoObj, materialQuantities);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Procesa un objeto de geometría para extraer materiales
        /// </summary>
        private void ProcessGeometryObject(GeometryObject geoObj, Dictionary<ElementId, MaterialData> materialQuantities)
        {
            if (geoObj is Solid solid && solid.Volume > 0)
            {
                foreach (Face face in solid.Faces)
                {
                    ElementId matId = face.MaterialElementId;
                    if (matId != null && matId != ElementId.InvalidElementId)
                    {
                        if (!materialQuantities.ContainsKey(matId))
                        {
                            materialQuantities[matId] = new MaterialData { MaterialId = matId };
                        }

                        // Acumular área y volumen
                        materialQuantities[matId].TotalArea += face.Area;
                        materialQuantities[matId].TotalVolume += solid.Volume / solid.Faces.Size; // Aproximado
                    }
                }
            }
            else if (geoObj is GeometryInstance instance)
            {
                GeometryElement instGeometry = instance.GetInstanceGeometry();
                if (instGeometry != null)
                {
                    foreach (GeometryObject instObj in instGeometry)
                    {
                        ProcessGeometryObject(instObj, materialQuantities);
                    }
                }
            }
        }

        /// <summary>
        /// Crea un registro de material para el Excel
        /// </summary>
        private Dictionary<string, string> CreateMaterialRecord(
            Material material,
            MaterialData data,
            IEnumerable<PropertyItem> properties,
            string keynote,
            string assemblyCode)
        {
            var record = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                string value = "";

                switch (prop.Name)
                {
                    case "ID Elemento":
                        value = material.Id.ToString();
                        break;
                    case "Nombre del Elemento":
                        value = material.Name;
                        break;
                    case "Categoría":
                        value = "Materiales";
                        break;
                    case "Familia y Tipo":
                        value = "Materiales";
                        break;
                    case "Assembly Code":
                        value = assemblyCode;
                        break;
                    case "Keynote":
                        value = keynote;
                        break;
                    case "Área":
                        value = FormatArea(data.TotalArea);
                        break;
                    case "Volumen":
                        value = FormatVolume(data.TotalVolume);
                        break;
                    default:
                        value = "";
                        break;
                }

                record[prop.Name] = value;
            }

            return record;
        }

        /// <summary>
        /// Verifica si un elemento pasa el filtro seleccionado
        /// </summary>
        private bool PassesFilter(bool hasKeynote, bool hasAssemblyCode, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.OnlyKeynote:
                    return hasKeynote;
                case FilterType.OnlyAssemblyCode:
                    return hasAssemblyCode;
                case FilterType.KeynoteOrAssemblyCode:
                    return hasKeynote || hasAssemblyCode;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Extrae las propiedades de un elemento específico
        /// </summary>
        private Dictionary<string, string> ExtractElementProperties(
            Element element,
            ElementType elementType,
            IEnumerable<PropertyItem> properties,
            string materialName = null)
        {
            var data = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                string value = GetPropertyValue(element, elementType, prop.Name, materialName);
                data[prop.Name] = value;
            }

            return data;
        }

        /// <summary>
        /// Obtiene el valor de una propiedad específica
        /// </summary>
        private string GetPropertyValue(Element element, ElementType elementType, string propertyName, string materialName = null)
        {
            try
            {
                switch (propertyName)
                {
                    case "ID Elemento":
                        return element.Id.ToString();

                    case "Nombre del Elemento":
                        return element.Name ?? "";

                    case "Categoría":
                        return element.Category?.Name ?? "";

                    case "Familia y Tipo":
                        return elementType != null ? $"{elementType.FamilyName} - {elementType.Name}" : "";

                    case "Assembly Code":
                        return GetParameterValue(elementType, BuiltInParameter.UNIFORMAT_CODE);

                    case "Keynote":
                        return GetParameterValue(elementType, BuiltInParameter.KEYNOTE_PARAM);

                    case "Type Mark":
                        return GetParameterValue(elementType, BuiltInParameter.ALL_MODEL_TYPE_MARK);

                    case "Descripción":
                        return GetParameterValue(elementType, BuiltInParameter.ALL_MODEL_DESCRIPTION);

                    case "Comentarios Tipo":
                        return GetParameterValue(elementType, BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);

                    case "Nivel":
                        return GetLevel(element);

                    case "Área":
                        return GetArea(element);

                    case "Altura":
                        return GetHeight(element);

                    case "Longitud":
                        return GetLength(element);

                    case "Volumen":
                        return GetVolume(element);

                    case "Densidad":
                        return GetDensity(element);

                    case "SubCapítulo":
                    case "Avance":
                    case "Ubicación":
                    case "Objeto":
                        return GetCustomParameter(element, propertyName);

                    default:
                        return GetCustomParameter(element, propertyName);
                }
            }
            catch
            {
                return "";
            }
        }

        private string GetParameterValue(Element element, BuiltInParameter parameter)
        {
            try
            {
                Parameter param = element?.get_Parameter(parameter);
                if (param != null && param.HasValue)
                {
                    return param.AsValueString() ?? param.AsString() ?? "";
                }
            }
            catch { }
            return "";
        }

        private string GetCustomParameter(Element element, string parameterName)
        {
            try
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue)
                {
                    return param.AsValueString() ?? param.AsString() ?? "";
                }

                ElementType type = _document.GetElement(element.GetTypeId()) as ElementType;
                if (type != null)
                {
                    param = type.LookupParameter(parameterName);
                    if (param != null && param.HasValue)
                    {
                        return param.AsValueString() ?? param.AsString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        private string GetLevel(Element element)
        {
            try
            {
                Parameter levelParam = element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                if (levelParam != null && levelParam.HasValue)
                {
                    ElementId levelId = levelParam.AsElementId();
                    Level level = _document.GetElement(levelId) as Level;
                    return level?.Name ?? "";
                }
            }
            catch { }
            return "";
        }

        private string GetArea(Element element)
        {
            try
            {
                Parameter param = element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                if (param != null && param.HasValue)
                    return param.AsValueString() ?? "";
            }
            catch { }
            return "";
        }

        private string GetHeight(Element element)
        {
            try
            {
                Parameter param = element.get_Parameter(BuiltInParameter.GENERIC_HEIGHT);
                if (param == null || !param.HasValue)
                    param = element.LookupParameter("Altura");
                if (param == null || !param.HasValue)
                    param = element.LookupParameter("Height");

                if (param != null && param.HasValue)
                    return param.AsValueString() ?? "";
            }
            catch { }
            return "";
        }

        private string GetLength(Element element)
        {
            try
            {
                Parameter param = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                if (param == null || !param.HasValue)
                    param = element.get_Parameter(BuiltInParameter.GENERIC_WIDTH);
                if (param == null || !param.HasValue)
                    param = element.LookupParameter("Longitud");
                if (param == null || !param.HasValue)
                    param = element.LookupParameter("Length");

                if (param != null && param.HasValue)
                    return param.AsValueString() ?? "";
            }
            catch { }
            return "";
        }

        private string GetVolume(Element element)
        {
            try
            {
                Parameter param = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                if (param != null && param.HasValue)
                    return param.AsValueString() ?? "";
            }
            catch { }
            return "";
        }

        private string GetDensity(Element element)
        {
            try
            {
                ElementType type = _document.GetElement(element.GetTypeId()) as ElementType;
                if (type != null)
                {
                    Parameter materialParam = type.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                    if (materialParam != null && materialParam.HasValue)
                    {
                        ElementId materialId = materialParam.AsElementId();
                        Material material = _document.GetElement(materialId) as Material;
                        if (material != null)
                        {
                            return material.Name;
                        }
                    }
                }
            }
            catch { }
            return "";
        }

        private string FormatArea(double areaInSquareFeet)
        {
            if (areaInSquareFeet == 0)
                return "";
            return $"{areaInSquareFeet:F2} ft²";
        }

        private string FormatVolume(double volumeInCubicFeet)
        {
            if (volumeInCubicFeet == 0)
                return "";
            return $"{volumeInCubicFeet:F2} ft³";
        }
    }

    /// <summary>
    /// Datos acumulados de un material
    /// </summary>
    internal class MaterialData
    {
        public ElementId MaterialId { get; set; }
        public double TotalArea { get; set; }
        public double TotalVolume { get; set; }
    }
}
