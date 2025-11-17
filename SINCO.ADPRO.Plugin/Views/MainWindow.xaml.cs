using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using SINCO.ADPRO.Plugin.Models;
using SINCO.ADPRO.Plugin.Services;
using SINCO.ADPRO.Plugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SINCO.ADPRO.Plugin.Views
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Document _document;
        private readonly UIDocument _uidocument;
        private readonly MainViewModel _viewModel;

        public MainWindow(Document document, UIDocument uidocument)
        {
            InitializeComponent();

            _document = document;
            _uidocument = uidocument;

            _viewModel = new MainViewModel(_document, _uidocument);
            DataContext = _viewModel;
        }

        private void SelectAllCategories_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAll();
        }

        private void DeselectAllCategories_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DeselectAll();
        }

        private void SelectAllProperties_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllProperties();
        }

        private void DeselectAllProperties_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DeselectAllProperties();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar selecciones
                var selectedCategories = _viewModel.Categories
                    .Where(c => c.IsSelected || c.Children.Any(f => f.IsSelected))
                    .ToList();

                if (selectedCategories.Count == 0)
                {
                    MessageBox.Show(
                        "Por favor, seleccione al menos una categoría o familia para exportar.",
                        "Sin selección",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var selectedProperties = _viewModel.Properties
                    .Where(p => p.IsSelected)
                    .ToList();

                if (selectedProperties.Count == 0)
                {
                    MessageBox.Show(
                        "Por favor, seleccione al menos una propiedad para exportar.",
                        "Sin selección",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Obtener información del proyecto
                string projectName = _document.Title;
                string projectLocation = "";

                try
                {
                    ProjectInfo projectInfo = _document.ProjectInformation;
                    if (projectInfo != null)
                    {
                        Parameter locationParam = projectInfo.LookupParameter("Project Address");
                        if (locationParam != null && locationParam.HasValue)
                        {
                            projectLocation = locationParam.AsString();
                        }
                    }
                }
                catch { }

                // Generar nombre de archivo sugerido
                string suggestedFileName = ExcelExportService.GenerateFileName(projectName, projectLocation);

                // Mostrar diálogo para guardar archivo
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar archivo de cantidades",
                    FileName = suggestedFileName
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                string filePath = saveDialog.FileName;

                // Mostrar mensaje de progreso
                _viewModel.StatusMessage = "Extrayendo datos del modelo...";

                // Extraer datos (elementos + materiales de la vista activa)
                DataExtractionService extractionService = new DataExtractionService(_document, _uidocument);
                List<Dictionary<string, string>> data = extractionService.ExtractData(
                    selectedCategories,
                    selectedProperties,
                    _viewModel.SelectedFilter);

                if (data.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontraron elementos que coincidan con los criterios de selección.",
                        "Sin datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    _viewModel.StatusMessage = "No se encontraron datos para exportar.";
                    return;
                }

                _viewModel.StatusMessage = $"Exportando {data.Count} elementos a Excel...";

                // Exportar a Excel
                ExcelExportService excelService = new ExcelExportService();
                List<string> columnHeaders = selectedProperties.Select(p => p.Name).ToList();

                excelService.ExportToExcel(data, columnHeaders, filePath, projectName);

                _viewModel.StatusMessage = $"Exportación completada: {data.Count} elementos exportados.";

                // Preguntar si desea abrir el archivo
                var result = MessageBox.Show(
                    $"Se exportaron {data.Count} elementos exitosamente.\n\n¿Desea abrir el archivo ahora?",
                    "Exportación exitosa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"No se pudo abrir el archivo automáticamente:\n{ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                // Cerrar la ventana
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error durante la exportación:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Convertidor para cambiar el peso de la fuente según si es categoría o familia
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCategory && isCategory)
                return FontWeights.Bold;
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
