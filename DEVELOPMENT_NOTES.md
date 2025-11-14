# Notas de Desarrollo - SINCO ADPRO

## 📝 Notas Importantes

### Compatibilidad con .NET 9 y Revit API

Revit API tradicionalmente requiere .NET Framework, pero a partir de Revit 2024+ es posible usar .NET Core/NET 9 con algunas consideraciones:

1. **Verificar compatibilidad**: Asegúrese de que su versión de Revit soporta .NET 9
2. **Dependencias**: ClosedXML y otras librerías deben ser compatibles con .NET 9
3. **Testing**: Probar exhaustivamente en todas las versiones de Revit objetivo

### Cambiar a .NET Framework (Si es necesario)

Si encuentra problemas de compatibilidad con .NET 9, puede cambiar a .NET Framework 4.8:

1. Editar `SINCO.ADPRO.Plugin.csproj`:
```xml
<TargetFramework>net48</TargetFramework>
```

2. Editar `SINCO.ADPRO.Installer.csproj`:
```xml
<TargetFramework>net48</TargetFramework>
```

### Referencias de Revit API

El paquete NuGet `Revit_All_Main_Versions_API_x64` incluye las DLLs de Revit API. Alternativamente, puede referenciar las DLLs directamente:

```xml
<ItemGroup>
  <Reference Include="RevitAPI">
    <HintPath>C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll</HintPath>
    <Private>False</Private>
  </Reference>
  <Reference Include="RevitAPIUI">
    <HintPath>C:\Program Files\Autodesk\Revit 2024\RevitAPIUI.dll</HintPath>
    <Private>False</Private>
  </Reference>
</ItemGroup>
```

**IMPORTANTE**: Establecer `<Private>False</Private>` para evitar copiar las DLLs de Revit al output.

## 🎨 Personalización

### Agregar Icono al Botón del Ribbon

1. Crear una carpeta `Resources` en el proyecto Plugin
2. Agregar una imagen PNG de 32x32 píxeles llamada `icon_32.png`
3. Establecer "Build Action" = "Content" y "Copy to Output Directory" = "Copy if newer"
4. El código en `Application.cs` ya está preparado para cargar el icono

### Cambiar Colores de la Interfaz

Los colores están definidos en los archivos XAML:

**Plugin (MainWindow.xaml)**:
- Header: `#2C3E50` (azul oscuro)
- Botón principal: `#3498DB` (azul)
- Botón éxito: `#27AE60` (verde)
- Botón secundario: `#95A5A6` (gris)

**Instalador (MainWindow.xaml)**:
- Los mismos colores para consistencia

### Agregar Más Propiedades

**Paso 1**: Agregar a la lista de propiedades en `MainViewModel.cs`:
```csharp
Properties.Add(new PropertyItem(
    "Mi Propiedad",
    "Descripción de mi propiedad",
    PropertyType.BuiltIn,
    true  // seleccionada por defecto
));
```

**Paso 2**: Agregar lógica de extracción en `DataExtractionService.cs`:
```csharp
case "Mi Propiedad":
    return GetCustomParameter(element, "NombreParametroEnRevit");
```

## 🔧 Debugging

### Debug en Revit

1. En Visual Studio, establecer configuración en "Debug"
2. En propiedades del proyecto → Debug:
   - Ejecutable: `C:\Program Files\Autodesk\Revit 2024\Revit.exe`
   - Argumentos: (vacío)
3. Establecer breakpoints
4. Presionar F5 para iniciar debug
5. Revit se abrirá con el plugin cargado

### Ver Output de Debug

Usar `System.Diagnostics.Debug.WriteLine()` o `TaskDialog.Show()` para mensajes de debug.

### Logs de Revit

Revisar los journals de Revit en:
```
%AppData%\Autodesk\Revit\Autodesk Revit [versión]\Journals\
```

## 📦 Empaquetado Avanzado

### Crear Instalador MSI con WiX

Para crear un instalador MSI profesional:

1. Instalar WiX Toolset
2. Crear proyecto WiX en la solución
3. Configurar componentes y características
4. Compilar el MSI

### Firma Digital

Para firmar el instalador:

```powershell
signtool sign /f "certificado.pfx" /p "password" /t http://timestamp.digicert.com SINCO.ADPRO.Installer.exe
```

## 🧪 Testing

### Casos de Prueba Recomendados

1. **Instalación**:
   - Instalar en Revit 2022, 2023, 2024, 2025
   - Reinstalar sobre versión existente
   - Instalar en versiones seleccionadas

2. **Funcionalidad**:
   - Abrir ventana de extracción
   - Seleccionar diferentes categorías
   - Exportar con diferentes propiedades
   - Exportar modelo vacío
   - Exportar modelo grande (>10,000 elementos)

3. **Excel**:
   - Verificar formato
   - Verificar datos correctos
   - Abrir en Excel 2016, 2019, 365

4. **Desinstalación**:
   - Desinstalar de todas las versiones
   - Verificar que no quedan archivos

## 🐛 Problemas Conocidos

### 1. Error "Could not load file or assembly ClosedXML"

**Solución**: Asegurarse de que ClosedXML.dll y todas sus dependencias están en la carpeta de instalación.

### 2. El botón no aparece en el Ribbon

**Solución**:
- Verificar que el archivo .addin tiene la ruta correcta al DLL
- Reiniciar Revit
- Verificar permisos de archivo

### 3. Error al exportar elementos sin tipo

**Solución**: El código ya maneja este caso saltando elementos sin ElementType.

## 📊 Optimizaciones Futuras

### Performance

1. **Async/Await**: Implementar carga asíncrona de categorías
2. **Lazy Loading**: Cargar familias solo cuando se expande la categoría
3. **Caché**: Cachear datos del documento para múltiples exportaciones
4. **Threads**: Usar background threads para extracción de datos

### Características

1. **Filtros**: Agregar filtros por nivel, fase, workset
2. **Plantillas**: Guardar/cargar configuraciones de exportación
3. **Programación**: Exportación automática periódica
4. **Cloud**: Subir a SharePoint/OneDrive automáticamente
5. **Comparación**: Comparar cantidades entre versiones del modelo

## 🔐 Seguridad

### Buenas Prácticas

1. **No hardcodear credenciales**: Usar Windows Credential Manager si necesario
2. **Validar inputs**: Siempre validar rutas y nombres de archivo
3. **Permisos mínimos**: El instalador requiere admin, pero el plugin no
4. **Sanitizar datos**: Limpiar caracteres especiales en nombres de archivo

## 📚 Recursos Adicionales

### Documentación de Revit API
- https://www.revitapidocs.com/
- https://thebuildingcoder.typepad.com/

### ClosedXML
- https://github.com/ClosedXML/ClosedXML
- https://closedxml.readthedocs.io/

### WPF
- https://docs.microsoft.com/en-us/dotnet/desktop/wpf/

## 🤝 Contribuciones

### Guía de Estilo

- Usar PascalCase para clases y métodos públicos
- Usar camelCase para variables privadas con underscore (_variable)
- Documentar con XML comments (///)
- Mantener métodos cortos y enfocados
- Seguir principios SOLID

### Proceso de Contribución

1. Fork del repositorio
2. Crear branch para feature (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

---

**Última actualización**: 2025
**Mantenedor**: SINCO Development Team
