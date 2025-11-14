# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [1.0.0] - 2025-01-13

### Agregado
- Implementación inicial del plugin SINCO ADPRO
- Interfaz WPF para selección de categorías y familias
- Extracción de 19 propiedades predefinidas de elementos
- Exportación a Excel (.xlsx) con formato profesional
- Instalador inteligente con detección automática de versiones de Revit
- Soporte para Revit 2022, 2023, 2024, 2025 y superiores
- Sistema de logging de instalaciones
- Desinstalador automático
- Ribbon personalizado "SINCO - ADPRO" en Revit
- Botón "Extracción de Cantidades" en el ribbon
- Selección/deselección masiva de categorías y propiedades
- Validación de selecciones antes de exportar
- Nombres de archivo automáticos con fecha y nombre del proyecto
- Interfaz moderna con temas consistentes
- Documentación completa (README, DEVELOPMENT_NOTES)
- Script de compilación automatizado (build.bat)

### Características Principales
- **Categorías soportadas**: Todas las categorías de Revit con elementos
- **Familias**: Agrupación por familia dentro de cada categoría
- **Propiedades**:
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
  - SubCapítulo
  - Avance
  - Ubicación
  - Objeto

### Tecnologías
- .NET 9
- Autodesk Revit API 2024
- WPF (Windows Presentation Foundation)
- ClosedXML para exportación a Excel
- MVVM Pattern

### Instalación
- Detección automática de versiones de Revit
- Desinstalación de versiones anteriores antes de reinstalar
- Instalación en múltiples versiones simultáneamente
- Permisos de administrador
- Log de instalación en C:\ProgramData\SINCO_ADPRO\

## [Unreleased]

### Planeado
- [ ] Soporte para filtros por nivel
- [ ] Soporte para filtros por fase
- [ ] Soporte para filtros por workset
- [ ] Exportación a CSV
- [ ] Exportación a PDF
- [ ] Plantillas de exportación guardables
- [ ] Carga asíncrona de categorías para mejor performance
- [ ] Comparación de cantidades entre versiones
- [ ] Exportación automática programada
- [ ] Integración con SharePoint/OneDrive
- [ ] Iconos personalizados para el ribbon
- [ ] Temas claro/oscuro
- [ ] Soporte multiidioma (inglés/español)
- [ ] Tutorial integrado
- [ ] Actualizador automático

---

## Formato de Versiones

- **Major.Minor.Patch** (ej: 1.0.0)
  - **Major**: Cambios incompatibles con versiones anteriores
  - **Minor**: Nueva funcionalidad compatible con versiones anteriores
  - **Patch**: Correcciones de bugs compatibles con versiones anteriores

## Tipos de Cambios

- **Agregado**: Para nuevas características
- **Cambiado**: Para cambios en funcionalidad existente
- **Obsoleto**: Para características que serán removidas
- **Removido**: Para características removidas
- **Corregido**: Para corrección de bugs
- **Seguridad**: Para vulnerabilidades de seguridad
