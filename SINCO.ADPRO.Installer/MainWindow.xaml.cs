using SINCO.ADPRO.Installer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SINCO.ADPRO.Installer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InstallationService _installationService;
        private List<RevitVersion> _detectedVersions;

        public MainWindow()
        {
            InitializeComponent();
            _installationService = new InstallationService();
            DetectRevitVersions();
        }

        private void DetectRevitVersions()
        {
            try
            {
                _detectedVersions = _installationService.DetectRevitVersions();

                if (_detectedVersions.Count == 0)
                {
                    NoVersionsText.Visibility = Visibility.Visible;
                    InstallButton.IsEnabled = false;
                    UninstallButton.IsEnabled = false;
                    return;
                }

                // Crear checkboxes para cada versión
                foreach (var version in _detectedVersions)
                {
                    var checkBox = new CheckBox
                    {
                        Content = CreateVersionContent(version),
                        IsChecked = version.IsSelected,
                        Margin = new Thickness(0, 5, 0, 5),
                        FontSize = 13,
                        Tag = version
                    };

                    checkBox.Checked += (s, e) => version.IsSelected = true;
                    checkBox.Unchecked += (s, e) => version.IsSelected = false;

                    VersionsPanel.Children.Add(checkBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al detectar versiones de Revit:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private StackPanel CreateVersionContent(RevitVersion version)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var nameText = new TextBlock
            {
                Text = version.DisplayName,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(nameText);

            if (version.IsInstalled)
            {
                var installedBadge = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(8, 2, 8, 2),
                    Margin = new Thickness(10, 0, 0, 0)
                };

                var installedText = new TextBlock
                {
                    Text = "Instalado",
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                };

                installedBadge.Child = installedText;
                panel.Children.Add(installedBadge);
            }

            return panel;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var selectedVersions = _detectedVersions.Where(v => v.IsSelected).ToList();

            if (selectedVersions.Count == 0)
            {
                MessageBox.Show(
                    "Por favor, seleccione al menos una versión de Revit.",
                    "Sin selección",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Desea instalar SINCO ADPRO en {selectedVersions.Count} versión(es) de Revit?\n\n" +
                string.Join("\n", selectedVersions.Select(v => $"  • {v.DisplayName}")),
                "Confirmar instalación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            PerformInstallation(selectedVersions);
        }

        private void PerformInstallation(List<RevitVersion> selectedVersions)
        {
            try
            {
                // Deshabilitar botones
                InstallButton.IsEnabled = false;
                UninstallButton.IsEnabled = false;

                // Mostrar progreso
                ProgressPanel.Visibility = Visibility.Visible;
                ProgressText.Text = "Instalando plugin...";

                // Obtener rutas de archivos
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);

                // Buscar DLL del plugin y archivo .addin
                string pluginDllPath = Path.Combine(assemblyDir, "SINCO.ADPRO.Plugin.dll");
                string addinTemplatePath = Path.Combine(assemblyDir, "SINCO.ADPRO.addin");

                // Verificar que los archivos existen
                if (!File.Exists(pluginDllPath))
                {
                    MessageBox.Show(
                        $"No se encontró el archivo del plugin:\n{pluginDllPath}\n\n" +
                        "Por favor, asegúrese de que todos los archivos están en el mismo directorio.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(addinTemplatePath))
                {
                    MessageBox.Show(
                        $"No se encontró el archivo de configuración:\n{addinTemplatePath}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Realizar instalación
                var installResult = _installationService.Install(
                    selectedVersions,
                    pluginDllPath,
                    addinTemplatePath);

                // Ocultar progreso
                ProgressPanel.Visibility = Visibility.Collapsed;

                // Mostrar resultados
                ShowResults(installResult);

                // Refrescar lista de versiones
                VersionsPanel.Children.Clear();
                DetectRevitVersions();

                // Rehabilitar botones
                InstallButton.IsEnabled = true;
                UninstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ProgressPanel.Visibility = Visibility.Collapsed;
                InstallButton.IsEnabled = true;
                UninstallButton.IsEnabled = true;

                MessageBox.Show(
                    $"Error durante la instalación:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            var installedVersions = _detectedVersions.Where(v => v.IsInstalled).ToList();

            if (installedVersions.Count == 0)
            {
                MessageBox.Show(
                    "No se encontraron instalaciones del plugin.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"¿Desea desinstalar SINCO ADPRO de todas las versiones de Revit?\n\n" +
                "Versiones afectadas:\n" +
                string.Join("\n", installedVersions.Select(v => $"  • {v.DisplayName}")),
                "Confirmar desinstalación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            PerformUninstallation();
        }

        private void PerformUninstallation()
        {
            try
            {
                // Deshabilitar botones
                InstallButton.IsEnabled = false;
                UninstallButton.IsEnabled = false;

                // Mostrar progreso
                ProgressPanel.Visibility = Visibility.Visible;
                ProgressText.Text = "Desinstalando plugin...";

                // Realizar desinstalación
                var uninstallResult = _installationService.Uninstall();

                // Ocultar progreso
                ProgressPanel.Visibility = Visibility.Collapsed;

                // Mostrar resultados
                ShowResults(uninstallResult, isUninstall: true);

                // Refrescar lista de versiones
                VersionsPanel.Children.Clear();
                DetectRevitVersions();

                // Rehabilitar botones
                InstallButton.IsEnabled = true;
                UninstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ProgressPanel.Visibility = Visibility.Collapsed;
                InstallButton.IsEnabled = true;
                UninstallButton.IsEnabled = true;

                MessageBox.Show(
                    $"Error durante la desinstalación:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowResults(InstallationResult result, bool isUninstall = false)
        {
            string action = isUninstall ? "desinstalación" : "instalación";
            string actionPast = isUninstall ? "desinstalado" : "instalado";

            if (result.Success)
            {
                MessageBox.Show(
                    $"¡{action.ToUpper().Substring(0, 1) + action.Substring(1)} completada exitosamente!\n\n" +
                    $"Versiones afectadas: {result.SuccessfulVersions.Count}\n" +
                    string.Join("\n", result.SuccessfulVersions.Select(v => $"  • Revit {v}")),
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                string message = $"La {action} se completó con algunos errores:\n\n";

                if (result.SuccessfulVersions.Count > 0)
                {
                    message += $"Exitosas ({result.SuccessfulVersions.Count}):\n";
                    message += string.Join("\n", result.SuccessfulVersions.Select(v => $"  ✓ Revit {v}"));
                    message += "\n\n";
                }

                if (result.FailedVersions.Count > 0)
                {
                    message += $"Fallidas ({result.FailedVersions.Count}):\n";
                    message += string.Join("\n", result.FailedVersions.Select(v => $"  ✗ Revit {v.Version}: {v.Error}"));
                }

                MessageBox.Show(message, "Resultado de " + action, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
