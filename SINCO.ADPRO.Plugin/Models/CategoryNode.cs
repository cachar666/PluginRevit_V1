using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SINCO.ADPRO.Plugin.Models
{
    /// <summary>
    /// Representa un nodo de categoría en el árbol jerárquico
    /// </summary>
    public class CategoryNode : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded;

        public string Name { get; set; }
        public string CategoryId { get; set; }
        public bool IsCategory { get; set; } // true = Categoría, false = Familia
        public ObservableCollection<CategoryNode> Children { get; set; }
        public CategoryNode Parent { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));

                    // Propagar selección a hijos
                    if (Children != null)
                    {
                        foreach (var child in Children)
                        {
                            child.IsSelected = value;
                        }
                    }

                    // Actualizar padre
                    Parent?.UpdateSelectionState();
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public CategoryNode()
        {
            Children = new ObservableCollection<CategoryNode>();
            IsExpanded = false;
        }

        private void UpdateSelectionState()
        {
            if (Children == null || Children.Count == 0)
                return;

            bool allSelected = true;
            bool noneSelected = true;

            foreach (var child in Children)
            {
                if (child.IsSelected)
                    noneSelected = false;
                else
                    allSelected = false;
            }

            if (allSelected)
                _isSelected = true;
            else if (noneSelected)
                _isSelected = false;
            else
                _isSelected = false; // Estado indeterminado

            OnPropertyChanged(nameof(IsSelected));
            Parent?.UpdateSelectionState();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
