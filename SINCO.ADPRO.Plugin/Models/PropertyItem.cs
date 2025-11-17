using System.ComponentModel;

namespace SINCO.ADPRO.Plugin.Models
{
    /// <summary>
    /// Representa una propiedad que puede ser exportada
    /// </summary>
    public class PropertyItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyType Type { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public PropertyItem(string name, string description, PropertyType type, bool isSelectedByDefault = true)
        {
            Name = name;
            Description = description;
            Type = type;
            IsSelected = isSelectedByDefault;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Tipos de propiedades disponibles
    /// </summary>
    public enum PropertyType
    {
        BuiltIn,        // Propiedades integradas de Revit
        SharedParameter,// Parámetros compartidos
        ProjectParameter,// Parámetros de proyecto
        FamilyParameter // Parámetros de familia
    }

    /// <summary>
    /// Tipos de filtro para categorías/familias y materiales
    /// </summary>
    public enum FilterType
    {
        OnlyKeynote,          // Solo elementos con Keynote
        OnlyAssemblyCode,     // Solo elementos con AssemblyCode
        KeynoteOrAssemblyCode // Elementos con Keynote O AssemblyCode (al menos uno)
    }
}
