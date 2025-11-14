using Autodesk.Revit.DB;
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
        private string _statusMessage;

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

        public MainViewModel(Document document)
        {
            _document = document;
            Categories = new ObservableCollection<CategoryNode>();
            Properties = new ObservableCollection<PropertyItem>();

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
        /// Carga las categorías y familias del documento
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                StatusMessage = "Cargando categorías...";

                // Obtener todas las categorías con elementos en el documento
                Dictionary<string, CategoryNode> categoryDict = new Dictionary<string, CategoryNode>();

                FilteredElementCollector collector = new FilteredElementCollector(_document)
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

                        CategoryNode familyNode = new CategoryNode
                        {
                            Name = familyGroup.Key,
                            CategoryId = categoryNode.CategoryId,
                            IsCategory = false,
                            IsSelected = true,
                            Parent = categoryNode
                        };

                        categoryNode.Children.Add(familyNode);
                    }

                    // Solo agregar categorías que tengan familias
                    if (categoryNode.Children.Count > 0)
                    {
                        Categories.Add(categoryNode);
                    }
                }

                StatusMessage = $"Cargadas {Categories.Count} categorías con {Categories.Sum(c => c.Children.Count)} familias";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al cargar categorías: {ex.Message}";
            }
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
