@echo off
chcp 65001 >nul
echo ============================================
echo   SINCO ADPRO - Script de Compilación
echo ============================================
echo.

REM Verificar que estamos en el directorio correcto
if not exist "SINCO.ADPRO.sln" (
    echo ERROR: No se encuentra SINCO.ADPRO.sln
    echo Por favor, ejecute este script desde la raíz del proyecto.
    pause
    exit /b 1
)

echo [1/4] Limpiando builds anteriores...
dotnet clean -c Release >nul 2>&1

echo [2/4] Restaurando paquetes NuGet...
dotnet restore
if errorlevel 1 (
    echo ERROR: Fallo al restaurar paquetes
    pause
    exit /b 1
)

echo [3/4] Compilando Plugin...
dotnet build SINCO.ADPRO.Plugin/SINCO.ADPRO.Plugin.csproj -c Release
if errorlevel 1 (
    echo ERROR: Fallo al compilar el plugin
    pause
    exit /b 1
)

echo [4/4] Compilando Instalador...
dotnet build SINCO.ADPRO.Installer/SINCO.ADPRO.Installer.csproj -c Release
if errorlevel 1 (
    echo ERROR: Fallo al compilar el instalador
    pause
    exit /b 1
)

echo.
echo ============================================
echo   Preparando paquete de instalación...
echo ============================================
echo.

set PLUGIN_DIR=SINCO.ADPRO.Plugin\bin\Release\net9.0-windows
set INSTALLER_DIR=SINCO.ADPRO.Installer\bin\Release\net9.0-windows
set PACKAGE_DIR=Release_Package

REM Crear directorio de paquete
if exist "%PACKAGE_DIR%" rmdir /s /q "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%"

echo Copiando archivos del instalador...
copy "%INSTALLER_DIR%\SINCO.ADPRO.Installer.exe" "%PACKAGE_DIR%\" >nul
copy "%INSTALLER_DIR%\SINCO.ADPRO.Installer.dll" "%PACKAGE_DIR%\" >nul
copy "%INSTALLER_DIR%\SINCO.ADPRO.Installer.runtimeconfig.json" "%PACKAGE_DIR%\" >nul

echo Copiando archivos del plugin...
copy "%PLUGIN_DIR%\SINCO.ADPRO.Plugin.dll" "%PACKAGE_DIR%\" >nul
copy "SINCO.ADPRO.Plugin\SINCO.ADPRO.addin" "%PACKAGE_DIR%\" >nul

echo Copiando dependencias...
copy "%PLUGIN_DIR%\ClosedXML.dll" "%PACKAGE_DIR%\" >nul
copy "%PLUGIN_DIR%\DocumentFormat.OpenXml.dll" "%PACKAGE_DIR%\" 2>nul
copy "%PLUGIN_DIR%\ExcelNumberFormat.dll" "%PACKAGE_DIR%\" 2>nul
copy "%PLUGIN_DIR%\SixLabors.Fonts.dll" "%PACKAGE_DIR%\" 2>nul
copy "%PLUGIN_DIR%\XLParser.dll" "%PACKAGE_DIR%\" 2>nul

echo Copiando README...
copy "README.md" "%PACKAGE_DIR%\" >nul

echo.
echo ============================================
echo   ✓ COMPILACIÓN EXITOSA
echo ============================================
echo.
echo Archivos generados en: %PACKAGE_DIR%\
echo.
echo Archivos principales:
echo   • SINCO.ADPRO.Installer.exe  - Ejecutar como administrador
echo   • SINCO.ADPRO.Plugin.dll     - DLL del plugin
echo   • SINCO.ADPRO.addin          - Archivo de configuración
echo   • README.md                   - Documentación
echo.
echo Para instalar:
echo   1. Ejecutar SINCO.ADPRO.Installer.exe como Administrador
echo   2. Seleccionar versiones de Revit
echo   3. Clic en "Instalar"
echo.

pause
