using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace SINCO.ADPRO.Plugin
{
    /// <summary>
    /// Clase principal de la aplicación que implementa IExternalApplication
    /// Se encarga de crear el Ribbon y registrar los comandos
    /// </summary>
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Crear el Ribbon Tab "SINCO - ADPRO"
                string tabName = "SINCO - ADPRO";

                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch (Exception)
                {
                    // El tab ya existe, no hacer nada
                }

                // Crear el Panel principal
                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Herramientas");

                // Obtener la ruta del assembly actual
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // Crear el botón "Extracción de Cantidades"
                PushButtonData buttonData = new PushButtonData(
                    "ExtraccionCantidades",
                    "Extracción de\nCantidades",
                    assemblyPath,
                    "SINCO.ADPRO.Plugin.Commands.ExtractQuantitiesCommand"
                );

                // Configurar el botón
                buttonData.ToolTip = "Extrae cantidades del modelo y las exporta a Excel";
                buttonData.LongDescription = "Permite seleccionar categorías, familias y propiedades del modelo de Revit para exportarlas a un archivo Excel con formato profesional.";

                // Intentar cargar el ícono (si existe)
                try
                {
                    string iconPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Resources", "icon_32.png");
                    if (File.Exists(iconPath))
                    {
                        buttonData.LargeImage = new BitmapImage(new Uri(iconPath));
                    }
                }
                catch
                {
                    // Si no se puede cargar el ícono, continuar sin él
                }

                // Agregar el botón al panel
                PushButton button = panel.AddItem(buttonData) as PushButton;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error al inicializar SINCO ADPRO:\n{ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Limpieza si es necesaria
            return Result.Succeeded;
        }
    }
}
