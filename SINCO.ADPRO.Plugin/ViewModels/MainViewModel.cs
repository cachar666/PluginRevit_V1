using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SINCO.ADPRO.Plugin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SINCO.ADPRO.Plugin.ViewModels
{
    /// <summary>
    /// ViewModel principal para la ventana de extracción de cantidades
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Document _document;
        private readonly UIDocument _uidocument;
        private string _statusMessage;
        private FilterType _selectedFilter;

        public ObservableCollection<CategoryNode> Categories { get; set; }
        public ObservableCollection<PropertyItem> Properties { get; set; }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public FilterType SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    LoadCategories(); // Recargar categorías con el nuevo filtro
                }
            }
        }

        public MainViewModel(Document document, UIDocument uidocument)
        {
            _document = document;
            _uidocument = uidocument;
            Categories = new ObservableCollection<CategoryNode>();
            Properties = new ObservableCollection<PropertyItem>();
            _selectedFilter = FilterType.OnlyKeynote; // Filtro por defecto

            InitializeProperties();
            LoadCategories();
        }

        /// <summary>
        /// Inicializa la lista de propiedades predefinidas
        /// </summary>
        private void InitializeProperties()
        {
            Properties.Add(new PropertyItem("ID Elemento", "ID único del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Nombre del Elemento", "Nombre del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Categoría", "Categoría del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Familia y Tipo", "Familia y tipo del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Assembly Code", "Código de ensamblaje", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Keynote", "Nota clave", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Type Mark", "Marca de tipo", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Descripción", "Descripción del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Comentarios Tipo", "Comentarios del tipo", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Nivel", "Nivel del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Área", "Área del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Altura", "Altura del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Longitud", "Longitud del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Volumen", "Volumen del elemento", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("Densidad", "Densidad del material", PropertyType.BuiltIn, true));
            Properties.Add(new PropertyItem("SubCapítulo", "Subcapítulo", PropertyType.ProjectParameter, true));
            Properties.Add(new PropertyItem("Avance", "Estado de avance", PropertyType.ProjectParameter, true));
            Properties.Add(new PropertyItem("Ubicación", "Ubicación del elemento", PropertyType.ProjectParameter, true));
            Properties.Add(new PropertyItem("Objeto", "Tipo de objeto", PropertyType.ProjectParameter, true));
        }

        /// <summary>
        /// Carga las categorías y familias del documento (solo elementos visibles en la vista activa)
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                StatusMessage = "Cargando categorías de la vista activa...";
                Categories.Clear();

                // Obtener la vista activa
                View activeView = _uidocument.ActiveView;
                if (activeView == null)
                {
                    StatusMessage = "No hay una vista activa";
                    return;
                }

                // Crear colector solo con elementos visibles en la vista activa
                FilteredElementCollector collector = new FilteredElementCollector(_document, activeView.Id)
                    .WhereElementIsNotElementType();

                // Agrupar elementos por categoría
                var elementsByCategory = collector
                    .Where(e => e.Category != null)
                    .GroupBy(e => e.Category.Name)
                    .OrderBy(g => g.Key);

                foreach (var categoryGroup in elementsByCategory)
                {
                    string categoryName = categoryGroup.Key;

                    // Crear nodo de categoría
                    CategoryNode categoryNode = new CategoryNode
                    {
                        Name = categoryName,
                        CategoryId = categoryGroup.First().Category.Id.ToString(),
                        IsCategory = true,
                        IsMaterial = false,
                        IsSelected = true
                    };

                    // Agrupar por familia dentro de la categoría
                    var elementsByFamily = categoryGroup
                        .Where(e => e.GetTypeId() != ElementId.InvalidElementId)
                        .Select(e =>
                        {
                            ElementType type = _document.GetElement(e.GetTypeId()) as ElementType;
                            return new { Element = e, Type = type };
                        })
                        .Where(x => x.Type != null)
                        .GroupBy(x => x.Type.FamilyName)
                        .OrderBy(g => g.Key);

                    foreach (var familyGroup in elementsByFamily)
                    {
                        if (string.IsNullOrEmpty(familyGroup.Key))
                            continue;

                        // Obtener el primer tipo para verificar Keynote y AssemblyCode
                        var firstType = familyGroup.First().Type;
                        string keynote = GetParameterValue(firstType, BuiltInParameter.KEYNOTE_PARAM);
                        string assemblyCode = GetParameterValue(firstType, BuiltInParameter.UNIFORMAT_CODE);

                        bool hasKeynote = !string.IsNullOrWhiteSpace(keynote);
                        bool hasAssemblyCode = !string.IsNullOrWhiteSpace(assemblyCode);

                        // Aplicar filtro - SIEMPRE filtrar
                        if (!PassesFilter(hasKeynote, hasAssemblyCode))
                            continue;

                        CategoryNode familyNode = new CategoryNode
                        {
                            Name = familyGroup.Key,
                            CategoryId = categoryNode.CategoryId,
                            IsCategory = false,
                            IsMaterial = false,
                            IsSelected = true,
                            Parent = categoryNode,
                            HasKeynote = hasKeynote,
                            HasAssemblyCode = hasAssemblyCode,
                            Keynote = keynote,
                            AssemblyCode = assemblyCode
                        };

                        categoryNode.Children.Add(familyNode);
                    }

                    // Solo agregar categorías que tengan familias que pasen el filtro
                    if (categoryNode.Children.Count > 0)
                    {
                        Categories.Add(categoryNode);
                    }
                }

                int totalFamilies = Categories.Sum(c => c.Children.Count);
                StatusMessage = $"Vista activa: {activeView.Name} - {Categories.Count} categorías con {totalFamilies} familias";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar categorías: {ex.Message}";
            }
        }

        /// <summary>
        /// Verifica si un elemento pasa el filtro seleccionado
        /// </summary>
        private bool PassesFilter(bool hasKeynote, bool hasAssemblyCode)
        {
            switch (SelectedFilter)
            {
                case FilterType.OnlyKeynote:
                    return hasKeynote;
                case FilterType.OnlyAssemblyCode:
                    return hasAssemblyCode;
                case FilterType.KeynoteOrAssemblyCode:
                    return hasKeynote || hasAssemblyCode; // Al menos uno
                default:
                    return false;
            }
        }

        /// <summary>
        /// Obtiene el valor de un parámetro
        /// </summary>
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

        /// <summary>
        /// Selecciona todas las categorías y familias
        /// </summary>
        public void SelectAll()
        {
            foreach (var category in Categories)
            {
                category.IsSelected = true;
            }
        }

        /// <summary>
        /// Deselecciona todas las categorías y familias
        /// </summary>
        public void DeselectAll()
        {
            foreach (var category in Categories)
            {
                category.IsSelected = false;
            }
        }

        /// <summary>
        /// Selecciona todas las propiedades
        /// </summary>
        public void SelectAllProperties()
        {
            foreach (var property in Properties)
            {
                property.IsSelected = true;
            }
        }

        /// <summary>
        /// Deselecciona todas las propiedades
        /// </summary>
        public void DeselectAllProperties()
        {
            foreach (var property in Properties)
            {
                property.IsSelected = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
