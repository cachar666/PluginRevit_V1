using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SINCO.ADPRO.Plugin.Services
{
    /// <summary>
    /// Servicio para exportar datos a Excel usando ClosedXML
    /// </summary>
    public class ExcelExportService
    {
        /// <summary>
        /// Exporta los datos a un archivo Excel
        /// </summary>
        public void ExportToExcel(
            List<Dictionary<string, string>> data,
            List<string> columnHeaders,
            string filePath,
            string projectName)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Cantidades");

                    // Configurar título
                    worksheet.Cell(1, 1).Value = "EXTRACCIÓN DE CANTIDADES - SINCO ADPRO";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, columnHeaders.Count).Merge();
                    worksheet.Range(1, 1, 1, columnHeaders.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Información del proyecto
                    worksheet.Cell(2, 1).Value = $"Proyecto: {projectName}";
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Range(2, 1, 2, columnHeaders.Count).Merge();

                    worksheet.Cell(3, 1).Value = $"Fecha de extracción: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                    worksheet.Cell(3, 1).Style.Font.Italic = true;
                    worksheet.Range(3, 1, 3, columnHeaders.Count).Merge();

                    // Fila vacía
                    int currentRow = 5;

                    // Encabezados de columnas
                    for (int i = 0; i < columnHeaders.Count; i++)
                    {
                        var cell = worksheet.Cell(currentRow, i + 1);
                        cell.Value = columnHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196); // Azul corporativo
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    currentRow++;

                    // Datos
                    foreach (var row in data)
                    {
                        for (int i = 0; i < columnHeaders.Count; i++)
                        {
                            string columnName = columnHeaders[i];
                            string value = row.ContainsKey(columnName) ? row[columnName] : "";

                            var cell = worksheet.Cell(currentRow, i + 1);
                            cell.Value = value;
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // Alineación según el tipo de dato
                            if (IsNumericColumn(columnName))
                            {
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else
                            {
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            }
                        }

                        // Alternar color de filas para mejor legibilidad
                        if (currentRow % 2 == 0)
                        {
                            worksheet.Range(currentRow, 1, currentRow, columnHeaders.Count)
                                .Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
                        }

                        currentRow++;
                    }

                    // Ajustar ancho de columnas
                    worksheet.Columns().AdjustToContents();

                    // Establecer ancho mínimo y máximo
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width < 10)
                            column.Width = 10;
                        if (column.Width > 50)
                            column.Width = 50;
                    }

                    // Congelar encabezados
                    worksheet.SheetView.FreezeRows(5);

                    // Agregar autofiltro
                    worksheet.Range(5, 1, currentRow - 1, columnHeaders.Count).SetAutoFilter();

                    // Guardar el archivo
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar a Excel: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determina si una columna contiene datos numéricos
        /// </summary>
        private bool IsNumericColumn(string columnName)
        {
            string[] numericColumns = { "Área", "Altura", "Longitud", "Volumen", "Densidad", "ID Elemento" };
            return numericColumns.Any(nc => columnName.IndexOf(nc, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Genera un nombre de archivo sugerido basado en el proyecto y la fecha
        /// </summary>
        public static string GenerateFileName(string projectName, string location)
        {
            string sanitizedProjectName = SanitizeFileName(projectName);
            string sanitizedLocation = SanitizeFileName(location);
            string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string fileName = $"Cantidades_{sanitizedProjectName}";

            if (!string.IsNullOrEmpty(sanitizedLocation))
            {
                fileName += $"_{sanitizedLocation}";
            }

            fileName += $"_{date}.xlsx";

            return fileName;
        }

        /// <summary>
        /// Sanitiza un nombre de archivo removiendo caracteres inválidos
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Sin_Nombre";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = new string(fileName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());

            return string.IsNullOrWhiteSpace(sanitized) ? "Sin_Nombre" : sanitized;
        }
    }
}
