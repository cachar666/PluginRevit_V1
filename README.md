# SINCO ADPRO - Plugin de Extracción de Cantidades para Revit

Plugin profesional para Autodesk Revit que permite extraer cantidades de elementos del modelo y exportarlas a Excel con formato profesional.

## 🎯 Características Principales

- **Interfaz Intuitiva**: Ventana WPF moderna y fácil de usar
- **Selección Flexible**: Seleccione categorías y familias específicas del modelo
- **Propiedades Personalizables**: Elija qué propiedades exportar
- **Exportación a Excel**: Archivos Excel (.xlsx) con formato profesional usando ClosedXML
- **Instalador Inteligente**: Detecta automáticamente las versiones de Revit instaladas
- **Multi-versión**: Compatible con Revit 2022, 2023, 2024, 2025 y superiores

## 📋 Requisitos

### Para Desarrollo:
- Visual Studio 2022 o superior
- .NET 9 SDK
- Autodesk Revit 2024 o superior (para desarrollo y pruebas)

### Para Usuarios Finales:
- Autodesk Revit 2022 o superior
- Windows 10/11

## 🏗️ Estructura del Proyecto

```
PluginRevit/
├── SINCO.ADPRO.Plugin/          # Proyecto principal del plugin
│   ├── Commands/                 # Comandos de Revit
│   ├── Models/                   # Modelos de datos
│   ├── ViewModels/              # ViewModels para MVVM
│   ├── Views/                    # Ventanas WPF
│   ├── Services/                 # Servicios de negocio
│   ├── Application.cs            # Clase principal IExternalApplication
│   └── SINCO.ADPRO.addin        # Archivo de configuración para Revit
│
├── SINCO.ADPRO.Installer/       # Proyecto del instalador
│   ├── Services/                 # Lógica de instalación
│   ├── MainWindow.xaml          # Interfaz del instalador
│   └── app.manifest             # Manifiesto (requiere admin)
│
└── README.md                     # Este archivo
```

## 🔨 Compilación

### Opción 1: Visual Studio
1. Abrir `SINCO.ADPRO.sln` en Visual Studio
2. Restaurar paquetes NuGet (clic derecho en solución → "Restore NuGet Packages")
3. Seleccionar configuración **Release**
4. Build → Build Solution (Ctrl+Shift+B)

### Opción 2: Línea de Comandos
```bash
# Restaurar dependencias
dotnet restore

# Compilar en Release
dotnet build -c Release

# Compilar ambos proyectos
dotnet build SINCO.ADPRO.Plugin/SINCO.ADPRO.Plugin.csproj -c Release
dotnet build SINCO.ADPRO.Installer/SINCO.ADPRO.Installer.csproj -c Release
```

## 📦 Crear el Instalador

Después de compilar el proyecto:

1. Los archivos compilados estarán en:
   - Plugin: `SINCO.ADPRO.Plugin\bin\Release\net9.0-windows\`
   - Instalador: `SINCO.ADPRO.Installer\bin\Release\net9.0-windows\`

2. Copiar los siguientes archivos del Plugin al directorio del Instalador:
   - `SINCO.ADPRO.Plugin.dll`
   - `SINCO.ADPRO.addin`
   - `ClosedXML.dll` (y todas sus dependencias)

3. Ejecutar `SINCO.ADPRO.Installer.exe` como administrador

### Script de Empaquetado (build.bat)
```batch
@echo off
echo Compilando SINCO ADPRO...

dotnet build SINCO.ADPRO.Plugin/SINCO.ADPRO.Plugin.csproj -c Release
dotnet build SINCO.ADPRO.Installer/SINCO.ADPRO.Installer.csproj -c Release

echo.
echo Copiando archivos al instalador...

set PLUGIN_DIR=SINCO.ADPRO.Plugin\bin\Release\net9.0-windows
set INSTALLER_DIR=SINCO.ADPRO.Installer\bin\Release\net9.0-windows

copy "%PLUGIN_DIR%\SINCO.ADPRO.Plugin.dll" "%INSTALLER_DIR%\"
copy "%PLUGIN_DIR%\ClosedXML.dll" "%INSTALLER_DIR%\"
copy "%PLUGIN_DIR%\DocumentFormat.OpenXml.dll" "%INSTALLER_DIR%\"
copy "SINCO.ADPRO.Plugin\SINCO.ADPRO.addin" "%INSTALLER_DIR%\"

echo.
echo Compilación completada!
echo Instalador disponible en: %INSTALLER_DIR%
pause
```

## 🚀 Instalación para Usuarios

1. Descargar el instalador `SINCO.ADPRO.Installer.exe`
2. Ejecutar como **Administrador** (clic derecho → "Ejecutar como administrador")
3. Seleccionar las versiones de Revit donde desea instalar
4. Clic en "Instalar"
5. El plugin aparecerá en Revit bajo la pestaña **"SINCO - ADPRO"**

## 💡 Uso del Plugin

1. Abrir un proyecto en Revit
2. Ir a la pestaña **"SINCO - ADPRO"**
3. Clic en el botón **"Extracción de Cantidades"**
4. En la ventana:
   - **Izquierda**: Seleccionar categorías y familias a exportar
   - **Derecha**: Seleccionar propiedades a incluir
5. Clic en **"Exportar"**
6. Elegir ubicación y nombre del archivo Excel
7. ¡Listo! El archivo Excel se generará con todos los datos

## 📊 Propiedades Disponibles

El plugin puede extraer las siguientes propiedades:

- ID Elemento
- Nombre del Elemento
- Categoría
- Familia y Tipo
- Assembly Code
- Keynote
- Type Mark
- Descripción
- Comentarios Tipo
- Nivel
- Área
- Altura
- Longitud
- Volumen
- Densidad
- SubCapítulo (parámetro personalizado)
- Avance (parámetro personalizado)
- Ubicación (parámetro personalizado)
- Objeto (parámetro personalizado)

## 🔧 Desarrollo y Extensión

### Agregar Nuevas Propiedades

1. Editar `ViewModels/MainViewModel.cs` → método `InitializeProperties()`
2. Editar `Services/DataExtractionService.cs` → método `GetPropertyValue()`

### Cambiar Estilos de Excel

Editar `Services/ExcelExportService.cs` → método `ExportToExcel()`

### Personalizar Ribbon

Editar `Application.cs` → método `OnStartup()`

## 🐛 Solución de Problemas

### El plugin no aparece en Revit
- Verificar que el archivo `.addin` esté en `%AppData%\Autodesk\Revit\Addins\[versión]\`
- Verificar que la ruta del DLL en el archivo `.addin` sea correcta
- Revisar el log de Revit en `%AppData%\Autodesk\Revit\Autodesk Revit [versión]\Journals\`

### Error al exportar a Excel
- Verificar que ClosedXML.dll esté en el mismo directorio que el plugin
- Verificar que tiene permisos de escritura en la carpeta destino
- Revisar el log en `C:\ProgramData\SINCO_ADPRO\log.txt`

### El instalador no detecta Revit
- Verificar que Revit esté correctamente instalado
- Revisar que existe la carpeta `%AppData%\Autodesk\Revit\Addins\`
- Ejecutar el instalador como Administrador

## 📝 Logs

Los logs de instalación se guardan en:
```
C:\ProgramData\SINCO_ADPRO\log.txt
```

## 🔄 Desinstalación

### Opción 1: Usar el Instalador
1. Ejecutar `SINCO.ADPRO.Installer.exe` como Administrador
2. Clic en "Desinstalar"

### Opción 2: Manual
1. Eliminar archivos `.addin` de `%AppData%\Autodesk\Revit\Addins\[versión]\`
2. Eliminar carpeta `C:\Program Files\SINCO.ADPRO\`

## 📜 Licencia

Copyright © 2025 SINCO - Sistemas Integrados de Construcción

## 👥 Soporte

Para soporte técnico o reportar problemas, contactar a:
- Email: soporte@sinco.com
- Web: www.sinco.com

## 🎯 Roadmap

- [ ] Soporte para más propiedades personalizadas
- [ ] Exportación a otros formatos (CSV, PDF)
- [ ] Filtros avanzados de elementos
- [ ] Plantillas de exportación guardables
- [ ] Exportación automática programada

---

**Versión**: 1.0.0
**Fecha**: 2025
**Autor**: SINCO Development Team
