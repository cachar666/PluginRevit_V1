using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SINCO.ADPRO.Plugin.Views;
using System;

namespace SINCO.ADPRO.Plugin.Commands
{
    /// <summary>
    /// Comando principal que abre la ventana de extracción de cantidades
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExtractQuantitiesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Obtener el documento activo
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Verificar que hay un documento activo
                if (doc == null)
                {
                    TaskDialog.Show("Error", "No hay un documento de Revit activo.");
                    return Result.Failed;
                }

                // Crear y mostrar la ventana principal
                MainWindow window = new MainWindow(doc, uidoc);

                // Mostrar como ventana modal
                bool? result = window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", $"Error al ejecutar el comando:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
                return Result.Failed;
            }
        }
    }
}
