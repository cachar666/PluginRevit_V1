# Arquitectura del Proyecto SINCO ADPRO

## 📐 Visión General

```
┌─────────────────────────────────────────────────────────────┐
│                        REVIT                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              SINCO - ADPRO RIBBON                      │ │
│  │  ┌──────────────────────────────────┐                 │ │
│  │  │  Extracción de Cantidades       │                  │ │
│  │  └──────────────────────────────────┘                 │ │
│  └────────────────────────────────────────────────────────┘ │
│                           │                                 │
│                           ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │            SINCO.ADPRO.Plugin.dll                      │ │
│  │                                                         │ │
│  │  Application.cs  →  ExtractQuantitiesCommand.cs       │ │
│  │  (IExternalApplication)  (IExternalCommand)           │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
          ┌────────────────────────────────┐
          │      MainWindow (WPF)         │
          │  ┌──────────┬──────────────┐  │
          │  │Categories│  Properties  │  │
          │  │TreeView  │  CheckBoxes  │  │
          │  └──────────┴──────────────┘  │
          └────────────────────────────────┘
                           │
                           ▼
          ┌────────────────────────────────┐
          │      MainViewModel            │
          │  (MVVM Pattern)               │
          └────────────────────────────────┘
                           │
                           ▼
          ┌────────────────────────────────┐
          │  DataExtractionService        │
          │  (Extract data from Revit)    │
          └────────────────────────────────┘
                           │
                           ▼
          ┌────────────────────────────────┐
          │  ExcelExportService           │
          │  (ClosedXML)                  │
          └────────────────────────────────┘
                           │
                           ▼
                    ┌──────────┐
                    │Excel File│
                    └──────────┘
```

## 🏗️ Componentes Principales

### 1. SINCO.ADPRO.Plugin (DLL)

#### Application.cs
```csharp
IExternalApplication
├── OnStartup()
│   ├── Crear Ribbon Tab "SINCO - ADPRO"
│   ├── Crear RibbonPanel
│   └── Agregar PushButton "Extracción de Cantidades"
└── OnShutdown()
```

**Responsabilidades**:
- Inicializar el plugin cuando Revit arranca
- Crear la interfaz del Ribbon
- Registrar comandos

#### Commands/ExtractQuantitiesCommand.cs
```csharp
IExternalCommand
└── Execute()
    ├── Obtener Document actual
    ├── Crear MainWindow
    └── ShowDialog()
```

**Responsabilidades**:
- Ejecutarse cuando el usuario hace clic en el botón
- Crear y mostrar la ventana principal
- Manejar errores de ejecución

#### Models/
```
CategoryNode.cs          → Representa nodo de categoría/familia
├── Name
├── CategoryId
├── IsCategory
├── IsSelected
├── IsExpanded
└── Children (ObservableCollection)

PropertyItem.cs          → Representa propiedad exportable
├── Name
├── Description
├── Type (PropertyType enum)
└── IsSelected
```

**Responsabilidades**:
- Modelar datos para la interfaz
- Implementar INotifyPropertyChanged para data binding
- Mantener estado de selección

#### ViewModels/MainViewModel.cs
```csharp
MainViewModel
├── Categories (ObservableCollection<CategoryNode>)
├── Properties (ObservableCollection<PropertyItem>)
├── StatusMessage (string)
├── LoadCategories()
├── InitializeProperties()
├── SelectAll()
├── DeselectAll()
├── SelectAllProperties()
└── DeselectAllProperties()
```

**Responsabilidades**:
- Lógica de presentación (MVVM)
- Cargar datos del documento Revit
- Manejar selecciones del usuario
- Actualizar estado de la UI

#### Views/MainWindow.xaml
```xaml
Window
├── Header (Título y versión)
├── Main Content (Grid)
│   ├── Left Panel
│   │   ├── TreeView (Categories)
│   │   └── Buttons (Select All/Deselect All)
│   ├── Right Panel
│   │   ├── ItemsControl (Properties)
│   │   └── Buttons (Select All/Deselect All)
│   └── Status Bar
└── Footer (Action Buttons)
    ├── Export
    └── Cancel
```

**Responsabilidades**:
- Presentación visual
- Data binding con ViewModel
- Eventos de usuario

#### Services/DataExtractionService.cs
```csharp
DataExtractionService
├── ExtractData()
│   ├── FilteredElementCollector
│   ├── Filtrar por categorías seleccionadas
│   ├── Filtrar por familias seleccionadas
│   └── ExtractElementProperties()
└── GetPropertyValue()
    ├── BuiltInParameter
    ├── Custom Parameters
    └── Calculations
```

**Responsabilidades**:
- Extraer datos del modelo Revit
- Obtener valores de propiedades
- Manejar diferentes tipos de parámetros
- Formatear datos para exportación

#### Services/ExcelExportService.cs
```csharp
ExcelExportService
├── ExportToExcel()
│   ├── Crear Workbook (ClosedXML)
│   ├── Agregar headers con estilo
│   ├── Escribir datos
│   ├── Formatear celdas
│   ├── Ajustar columnas
│   └── Guardar archivo
└── GenerateFileName()
```

**Responsabilidades**:
- Crear archivos Excel
- Aplicar formato profesional
- Generar nombres de archivo
- Manejar errores de escritura

---

### 2. SINCO.ADPRO.Installer (EXE)

#### Services/InstallationService.cs
```csharp
InstallationService
├── DetectRevitVersions()
│   ├── Buscar en Registry
│   └── Buscar en AppData
├── Install()
│   ├── Copiar DLL a Program Files
│   ├── Copiar dependencias
│   ├── Crear .addin en cada versión
│   └── Crear desinstalador
└── Uninstall()
    ├── Eliminar .addin files
    └── Eliminar directorio de instalación
```

**Responsabilidades**:
- Detectar versiones de Revit instaladas
- Instalar plugin en versiones seleccionadas
- Desinstalar plugin
- Logging de operaciones

#### MainWindow.xaml (Installer)
```xaml
Window
├── Header (Branding)
├── Instructions
├── Versions List (CheckBoxes dinámicos)
├── Progress Bar
└── Footer
    ├── Uninstall Button
    ├── Install Button
    └── Close Button
```

**Responsabilidades**:
- Interfaz de instalación
- Mostrar versiones detectadas
- Ejecutar instalación/desinstalación
- Mostrar progreso y resultados

---

## 🔄 Flujo de Datos

### Flujo de Instalación
```
1. Usuario ejecuta Installer.exe
2. InstallationService.DetectRevitVersions()
3. UI muestra versiones detectadas
4. Usuario selecciona versiones y hace clic en "Instalar"
5. InstallationService.Install()
   ├── Copiar SINCO.ADPRO.Plugin.dll → C:\Program Files\SINCO.ADPRO\
   ├── Para cada versión seleccionada:
   │   └── Crear .addin → %AppData%\Autodesk\Revit\Addins\[versión]\
   └── Guardar log → C:\ProgramData\SINCO_ADPRO\log.txt
6. Mostrar resultado
```

### Flujo de Uso del Plugin
```
1. Usuario abre Revit
2. Revit carga SINCO.ADPRO.Plugin.dll
3. Application.OnStartup() crea Ribbon
4. Usuario hace clic en "Extracción de Cantidades"
5. ExtractQuantitiesCommand.Execute()
6. MainWindow se crea con MainViewModel
7. MainViewModel.LoadCategories() extrae datos del Document
8. Usuario selecciona categorías/familias y propiedades
9. Usuario hace clic en "Exportar"
10. MainWindow.Export_Click()
11. DataExtractionService.ExtractData() obtiene datos de elementos
12. ExcelExportService.ExportToExcel() crea archivo Excel
13. Usuario abre archivo Excel
```

---

## 🎨 Patrones de Diseño Utilizados

### 1. MVVM (Model-View-ViewModel)
```
Model           ViewModel           View
CategoryNode ←→ MainViewModel ←→  MainWindow.xaml
PropertyItem                      (Data Binding)
```

**Beneficios**:
- Separación de responsabilidades
- Testabilidad
- Data binding automático
- Mantenibilidad

### 2. Service Layer
```
Services/
├── DataExtractionService  → Lógica de extracción
└── ExcelExportService     → Lógica de exportación
```

**Beneficios**:
- Reutilización de código
- Facilita testing
- Separación de concerns

### 3. Observer Pattern (INotifyPropertyChanged)
```csharp
public class CategoryNode : INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));  // Notifica cambio
        }
    }
}
```

**Beneficios**:
- Actualización automática de UI
- Sincronización de datos

---

## 📊 Diagrama de Clases Simplificado

```
┌─────────────────────┐
│   Application       │
│  (Revit Plugin)     │
└──────────┬──────────┘
           │ creates
           ▼
┌─────────────────────┐
│ExtractQuantities    │
│     Command         │
└──────────┬──────────┘
           │ opens
           ▼
┌─────────────────────┐      ┌──────────────────┐
│   MainWindow        │ uses │  MainViewModel   │
│    (WPF View)       │◄─────┤   (Logic)        │
└─────────────────────┘      └────────┬─────────┘
                                      │ uses
                             ┌────────┴────────┐
                             ▼                 ▼
                  ┌──────────────────┐  ┌────────────────┐
                  │DataExtraction    │  │ExcelExport     │
                  │   Service        │  │  Service       │
                  └──────────────────┘  └────────────────┘
```

---

## 🔌 Dependencias Externas

### NuGet Packages

**SINCO.ADPRO.Plugin**:
```xml
<PackageReference Include="Revit_All_Main_Versions_API_x64" Version="2024.0.0" />
<PackageReference Include="ClosedXML" Version="0.104.2" />
<PackageReference Include="Extended.Wpf.Toolkit" Version="4.6.1" />
```

**SINCO.ADPRO.Installer**:
```xml
<PackageReference Include="Extended.Wpf.Toolkit" Version="4.6.1" />
```

### Dependencias Transitivas (de ClosedXML):
- DocumentFormat.OpenXml
- ExcelNumberFormat
- SixLabors.Fonts
- XLParser

---

## 📁 Estructura de Archivos en Disco

### Después de la Instalación
```
C:\Program Files\SINCO.ADPRO\
├── SINCO.ADPRO.Plugin.dll
├── ClosedXML.dll
├── DocumentFormat.OpenXml.dll
└── [otras dependencias]

%AppData%\Autodesk\Revit\Addins\2024\
├── SINCO.ADPRO.addin        ← Apunta a C:\Program Files\SINCO.ADPRO\

%AppData%\Autodesk\Revit\Addins\2025\
├── SINCO.ADPRO.addin        ← Apunta a C:\Program Files\SINCO.ADPRO\

C:\ProgramData\SINCO_ADPRO\
└── log.txt
```

---

## 🚀 Puntos de Extensión

### 1. Agregar Nueva Propiedad
```
1. MainViewModel.InitializeProperties()
2. DataExtractionService.GetPropertyValue()
```

### 2. Agregar Nuevo Formato de Exportación
```
1. Crear nuevo servicio (ej: CsvExportService)
2. Llamar desde MainWindow.Export_Click()
```

### 3. Agregar Filtros
```
1. Agregar UI en MainWindow.xaml
2. Agregar lógica en MainViewModel
3. Modificar DataExtractionService.ExtractData()
```

---

## 🔒 Seguridad y Permisos

### Instalador
- Requiere permisos de **Administrador** (UAC)
- Escribe en `C:\Program Files\`
- Escribe en `C:\ProgramData\`

### Plugin
- Ejecuta con permisos del usuario de Revit
- Solo lee del modelo de Revit
- Escribe archivos Excel con permisos del usuario

---

## 📈 Performance

### Optimizaciones Implementadas
- ✅ FilteredElementCollector (eficiente)
- ✅ Carga diferida de familias (lazy loading)
- ✅ Data binding eficiente con ObservableCollection

### Optimizaciones Futuras
- ⏳ Async/Await para operaciones largas
- ⏳ Background threads para extracción
- ⏳ Caché de datos del documento
- ⏳ Paginación para modelos grandes

---

**Versión del documento**: 1.0
**Última actualización**: Enero 2025
