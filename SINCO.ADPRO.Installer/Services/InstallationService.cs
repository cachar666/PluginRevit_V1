using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SINCO.ADPRO.Installer.Services
{
    /// <summary>
    /// Servicio para manejar la instalación y desinstalación del plugin
    /// </summary>
    public class InstallationService
    {
        private const string PLUGIN_NAME = "SINCO.ADPRO";
        private const string ADDIN_FILENAME = "SINCO.ADPRO.addin";
        private const string DLL_FILENAME = "SINCO.ADPRO.Plugin.dll";
        private const string LOG_DIRECTORY = @"C:\ProgramData\SINCO_ADPRO";
        private const string LOG_FILENAME = "log.txt";

        private readonly StringBuilder _logBuilder = new StringBuilder();

        /// <summary>
        /// Detecta las versiones de Revit instaladas en el sistema
        /// </summary>
        public List<RevitVersion> DetectRevitVersions()
        {
            var versions = new List<RevitVersion>();

            // Buscar en el registro de Windows
            string[] registryPaths = new[]
            {
                @"SOFTWARE\Autodesk\Revit",
                @"SOFTWARE\WOW6432Node\Autodesk\Revit"
            };

            foreach (string registryPath in registryPaths)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (key != null)
                        {
                            foreach (string subKeyName in key.GetSubKeyNames())
                            {
                                // Buscar versiones numéricas (2022, 2023, 2024, 2025, etc.)
                                if (int.TryParse(subKeyName, out int year) && year >= 2020)
                                {
                                    string addinPath = GetAddinPath(year.ToString());
                                    if (Directory.Exists(addinPath))
                                    {
                                        versions.Add(new RevitVersion
                                        {
                                            Year = year.ToString(),
                                            DisplayName = $"Autodesk Revit {year}",
                                            AddinPath = addinPath,
                                            IsInstalled = IsPluginInstalled(addinPath)
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error al buscar en registro {registryPath}: {ex.Message}");
                }
            }

            // Método alternativo: buscar directamente en AppData
            string baseAddinPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Autodesk", "Revit", "Addins");

            if (Directory.Exists(baseAddinPath))
            {
                foreach (string versionDir in Directory.GetDirectories(baseAddinPath))
                {
                    string versionName = Path.GetFileName(versionDir);
                    if (int.TryParse(versionName, out int year) && year >= 2020)
                    {
                        if (!versions.Any(v => v.Year == versionName))
                        {
                            versions.Add(new RevitVersion
                            {
                                Year = versionName,
                                DisplayName = $"Autodesk Revit {versionName}",
                                AddinPath = versionDir,
                                IsInstalled = IsPluginInstalled(versionDir)
                            });
                        }
                    }
                }
            }

            return versions.OrderByDescending(v => v.Year).ToList();
        }

        /// <summary>
        /// Obtiene la ruta de addins para una versión de Revit
        /// </summary>
        private string GetAddinPath(string version)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Autodesk", "Revit", "Addins", version);
        }

        /// <summary>
        /// Verifica si el plugin ya está instalado
        /// </summary>
        private bool IsPluginInstalled(string addinPath)
        {
            string addinFile = Path.Combine(addinPath, ADDIN_FILENAME);
            return File.Exists(addinFile);
        }

        /// <summary>
        /// Instala el plugin en las versiones seleccionadas de Revit
        /// </summary>
        public InstallationResult Install(
            List<RevitVersion> selectedVersions,
            string pluginDllPath,
            string addinTemplatePath)
        {
            var result = new InstallationResult();

            try
            {
                Log("=== INICIANDO INSTALACIÓN ===");
                Log($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log($"DLL Source: {pluginDllPath}");
                Log($"Addin Template: {addinTemplatePath}");
                Log("");

                // Crear directorio de instalación principal
                string installBaseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    PLUGIN_NAME);

                Directory.CreateDirectory(installBaseDir);
                Log($"Directorio de instalación: {installBaseDir}");

                // Copiar DLL principal
                string targetDllPath = Path.Combine(installBaseDir, DLL_FILENAME);
                File.Copy(pluginDllPath, targetDllPath, true);
                Log($"DLL copiada a: {targetDllPath}");

                // Copiar dependencias
                string sourceDllDir = Path.GetDirectoryName(pluginDllPath);
                CopyDependencies(sourceDllDir, installBaseDir);

                // Instalar en cada versión de Revit
                foreach (var version in selectedVersions)
                {
                    Log($"\nInstalando en Revit {version.Year}...");

                    try
                    {
                        // Desinstalar versión anterior si existe
                        if (version.IsInstalled)
                        {
                            Log($"  - Detectada instalación anterior, desinstalando...");
                            UninstallFromVersion(version);
                        }

                        // Crear directorio de addins si no existe
                        Directory.CreateDirectory(version.AddinPath);

                        // Crear archivo .addin con la ruta correcta
                        string addinContent = File.ReadAllText(addinTemplatePath);
                        addinContent = addinContent.Replace("{ASSEMBLY_PATH}", targetDllPath);

                        string targetAddinPath = Path.Combine(version.AddinPath, ADDIN_FILENAME);
                        File.WriteAllText(targetAddinPath, addinContent);

                        Log($"  - Addin creado: {targetAddinPath}");
                        result.SuccessfulVersions.Add(version.Year);
                    }
                    catch (Exception ex)
                    {
                        Log($"  - ERROR: {ex.Message}");
                        result.FailedVersions.Add((version.Year, ex.Message));
                    }
                }

                // Crear desinstalador
                CreateUninstaller(installBaseDir);

                // Guardar log
                SaveLog();

                result.Success = result.FailedVersions.Count == 0;
                result.LogContent = _logBuilder.ToString();

                Log("\n=== INSTALACIÓN COMPLETADA ===");
            }
            catch (Exception ex)
            {
                Log($"\nERROR CRÍTICO: {ex.Message}");
                result.Success = false;
                result.LogContent = _logBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Copia las dependencias necesarias (ClosedXML, etc.)
        /// </summary>
        private void CopyDependencies(string sourceDir, string targetDir)
        {
            try
            {
                string[] dependencies = new[]
                {
                    "ClosedXML.dll",
                    "ClosedXML.Parser.dll",
                    "DocumentFormat.OpenXml.dll",
                    "DocumentFormat.OpenXml.Framework.dll",
                    "ExcelNumberFormat.dll",
                    "SixLabors.Fonts.dll",
                    "RBush.dll",
                    "System.Buffers.dll",
                    "System.Memory.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "Microsoft.Bcl.HashCode.dll",
                    "Xceed.Wpf.Toolkit.dll",
                    "Xceed.Wpf.AvalonDock.dll",
                    "Xceed.Wpf.AvalonDock.Themes.Aero.dll",
                    "Xceed.Wpf.AvalonDock.Themes.Metro.dll",
                    "Xceed.Wpf.AvalonDock.Themes.VS2010.dll"
                };

                foreach (string dependency in dependencies)
                {
                    string sourcePath = Path.Combine(sourceDir, dependency);
                    if (File.Exists(sourcePath))
                    {
                        string targetPath = Path.Combine(targetDir, dependency);
                        File.Copy(sourcePath, targetPath, true);
                        Log($"  - Dependencia copiada: {dependency}");
                    }
                }

                // Copiar carpeta Resources (contiene el logo)
                string sourceResourcesDir = Path.Combine(sourceDir, "Resources");
                if (Directory.Exists(sourceResourcesDir))
                {
                    string targetResourcesDir = Path.Combine(targetDir, "Resources");
                    Directory.CreateDirectory(targetResourcesDir);

                    foreach (string file in Directory.GetFiles(sourceResourcesDir))
                    {
                        string fileName = Path.GetFileName(file);
                        string targetPath = Path.Combine(targetResourcesDir, fileName);
                        File.Copy(file, targetPath, true);
                        Log($"  - Recurso copiado: Resources/{fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"  - Error al copiar dependencias: {ex.Message}");
            }
        }

        /// <summary>
        /// Desinstala el plugin de todas las versiones de Revit
        /// </summary>
        public InstallationResult Uninstall()
        {
            var result = new InstallationResult();

            try
            {
                Log("=== INICIANDO DESINSTALACIÓN ===");
                Log($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log("");

                var versions = DetectRevitVersions();

                foreach (var version in versions.Where(v => v.IsInstalled))
                {
                    Log($"Desinstalando de Revit {version.Year}...");

                    try
                    {
                        UninstallFromVersion(version);
                        result.SuccessfulVersions.Add(version.Year);
                    }
                    catch (Exception ex)
                    {
                        Log($"  - ERROR: {ex.Message}");
                        result.FailedVersions.Add((version.Year, ex.Message));
                    }
                }

                // Eliminar directorio de instalación
                string installBaseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    PLUGIN_NAME);

                if (Directory.Exists(installBaseDir))
                {
                    try
                    {
                        Directory.Delete(installBaseDir, true);
                        Log($"Directorio de instalación eliminado: {installBaseDir}");
                    }
                    catch (Exception ex)
                    {
                        Log($"No se pudo eliminar el directorio: {ex.Message}");
                    }
                }

                SaveLog();

                result.Success = result.FailedVersions.Count == 0;
                result.LogContent = _logBuilder.ToString();

                Log("\n=== DESINSTALACIÓN COMPLETADA ===");
            }
            catch (Exception ex)
            {
                Log($"\nERROR CRÍTICO: {ex.Message}");
                result.Success = false;
                result.LogContent = _logBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Desinstala el plugin de una versión específica
        /// </summary>
        private void UninstallFromVersion(RevitVersion version)
        {
            string addinFile = Path.Combine(version.AddinPath, ADDIN_FILENAME);

            if (File.Exists(addinFile))
            {
                File.Delete(addinFile);
                Log($"  - Archivo .addin eliminado: {addinFile}");
            }
        }

        /// <summary>
        /// Crea un script de desinstalación
        /// </summary>
        private void CreateUninstaller(string installDir)
        {
            try
            {
                string uninstallerPath = Path.Combine(installDir, "Uninstall.bat");
                string uninstallerContent = $@"@echo off
echo Desinstalando SINCO ADPRO...
""{System.Reflection.Assembly.GetExecutingAssembly().Location}"" /uninstall
pause
";
                File.WriteAllText(uninstallerPath, uninstallerContent);
                Log($"Desinstalador creado: {uninstallerPath}");
            }
            catch (Exception ex)
            {
                Log($"Error al crear desinstalador: {ex.Message}");
            }
        }

        /// <summary>
        /// Agrega un mensaje al log
        /// </summary>
        private void Log(string message)
        {
            _logBuilder.AppendLine(message);
        }

        /// <summary>
        /// Guarda el log en disco
        /// </summary>
        private void SaveLog()
        {
            try
            {
                Directory.CreateDirectory(LOG_DIRECTORY);
                string logPath = Path.Combine(LOG_DIRECTORY, LOG_FILENAME);

                // Agregar separador si el archivo ya existe
                if (File.Exists(logPath))
                {
                    File.AppendAllText(logPath, "\n\n" + new string('=', 80) + "\n\n");
                }

                File.AppendAllText(logPath, _logBuilder.ToString());
            }
            catch
            {
                // Si no se puede guardar el log, no hacer nada
            }
        }
    }

    /// <summary>
    /// Representa una versión de Revit instalada
    /// </summary>
    public class RevitVersion
    {
        public string Year { get; set; }
        public string DisplayName { get; set; }
        public string AddinPath { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsSelected { get; set; } = true;
    }

    /// <summary>
    /// Resultado de la instalación/desinstalación
    /// </summary>
    public class InstallationResult
    {
        public bool Success { get; set; }
        public List<string> SuccessfulVersions { get; set; } = new List<string>();
        public List<(string Version, string Error)> FailedVersions { get; set; } = new List<(string, string)>();
        public string LogContent { get; set; }
    }
}
