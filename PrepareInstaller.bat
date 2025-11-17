@echo off
echo ================================================
echo Preparando paquete de instalacion SINCO ADPRO
echo ================================================
echo.

REM Crear directorio de salida
set OUTPUT_DIR=Release_PackageV3
if exist "%OUTPUT_DIR%" rmdir /S /Q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo [1/4] Copiando instalador y sus dependencias...
xcopy "SINCO.ADPRO.Installer\bin\Release\net9.0-windows\*.*" "%OUTPUT_DIR%\" /E /I /Y >nul
if errorlevel 1 (
    echo ERROR: No se pudo copiar el instalador
    pause
    exit /b 1
)

echo [2/4] Copiando plugin y dependencias...
xcopy "SINCO.ADPRO.Plugin\bin\Release\net48\*.dll" "%OUTPUT_DIR%\" /Y >nul
if errorlevel 1 (
    echo ERROR: No se pudo copiar el plugin
    pause
    exit /b 1
)

echo [3/4] Copiando carpeta Resources...
xcopy "SINCO.ADPRO.Plugin\bin\Release\net48\Resources" "%OUTPUT_DIR%\Resources\" /E /I /Y >nul
if errorlevel 1 (
    echo ERROR: No se pudo copiar Resources
    pause
    exit /b 1
)

echo [4/4] Copiando archivo .addin...
copy "SINCO.ADPRO.Plugin\SINCO.ADPRO.addin" "%OUTPUT_DIR%\" /Y >nul
if errorlevel 1 (
    echo ERROR: No se pudo copiar el archivo .addin
    pause
    exit /b 1
)

echo.
echo ================================================
echo Paquete creado exitosamente en: %OUTPUT_DIR%
echo ================================================
echo.
echo Archivos incluidos:
dir "%OUTPUT_DIR%\SINCO.ADPRO.*" /B
echo.
echo Para instalar en otro equipo:
echo 1. Copia toda la carpeta "%OUTPUT_DIR%" al otro equipo
echo 2. Ejecuta SINCO.ADPRO.Installer.exe
echo.
pause
