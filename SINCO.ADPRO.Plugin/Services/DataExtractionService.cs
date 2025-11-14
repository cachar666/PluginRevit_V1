using Autodesk.Revit.DB;
using SINCO.ADPRO.Plugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SINCO.ADPRO.Plugin.Services
{
    /// <summary>
    /// Servicio para extraer datos de elementos de Revit
    /// </summary>
    public class DataExtractionService
    {
        private readonly Document _document;

        public DataExtractionService(Document document)
        {
            _document = document;
        }

        /// <summary>
        /// Extrae datos de los elementos según las categorías y propiedades seleccionadas
        /// </summary>
        public List<Dictionary<string, string>> ExtractData(
            IEnumerable<CategoryNode> selectedCategories,
            IEnumerable<PropertyItem> selectedProperties)
        {
            var results = new List<Dictionary<string, string>>();

            try
            {
                // Obtener las familias seleccionadas agrupadas por categoría
                var selectedFamilies = selectedCategories
                    .Where(c => c.IsCategory && c.IsSelected)
                    .SelectMany(c => c.Children.Where(f => f.IsSelected))
                    .ToList();

                // Obtener las categorías sin hijos seleccionadas
                var selectedCategoriesOnly = selectedCategories
                    .Where(c => c.IsCategory && c.IsSelected && !c.Children.Any(f => f.IsSelected))
                    .ToList();

                // Crear colector de elementos
                FilteredElementCollector collector = new FilteredElementCollector(_document)
                    .WhereElementIsNotElementType();

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

                    // Verificar por familia
                    if (selectedFamilies.Any(f => f.Parent.Name == categoryName && f.Name == familyName))
                    {
                        isSelected = true;
                    }
                    // Verificar por categoría completa
                    else if (selectedCategoriesOnly.Any(c => c.Name == categoryName))
                    {
                        isSelected = true;
                    }

                    if (!isSelected)
                        continue;

                    // Extraer propiedades del elemento
                    var elementData = ExtractElementProperties(element, elementType, selectedProperties);
                    results.Add(elementData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al extraer datos: {ex.Message}", ex);
            }

            return results;
        }

        /// <summary>
        /// Extrae las propiedades de un elemento específico
        /// </summary>
        private Dictionary<string, string> ExtractElementProperties(
            Element element,
            ElementType elementType,
            IEnumerable<PropertyItem> properties)
        {
            var data = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                string value = GetPropertyValue(element, elementType, prop.Name);
                data[prop.Name] = value;
            }

            return data;
        }

        /// <summary>
        /// Obtiene el valor de una propiedad específica
        /// </summary>
        private string GetPropertyValue(Element element, ElementType elementType, string propertyName)
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
                // Buscar en parámetros de instancia
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue)
                {
                    return param.AsValueString() ?? param.AsString() ?? "";
                }

                // Buscar en parámetros de tipo
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
                // Intentar diferentes parámetros de altura
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
                // Intentar diferentes parámetros de longitud
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
                // Intentar obtener la densidad del material
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
                            // La densidad no está directamente disponible en la API básica
                            // Retornar el nombre del material
                            return material.Name;
                        }
                    }
                }
            }
            catch { }
            return "";
        }
    }
}
