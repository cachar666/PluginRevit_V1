# 🚀 Guía de Inicio Rápido - SINCO ADPRO

## Para Desarrolladores

### 1. Compilar el Proyecto

**Opción A - Usando el script (Recomendado)**
```batch
# Ejecutar desde la raíz del proyecto
build.bat
```

**Opción B - Usando dotnet CLI**
```bash
dotnet restore
dotnet build -c Release
```

**Opción C - Visual Studio**
1. Abrir `SINCO.ADPRO.sln`
2. Clic derecho en solución → "Restore NuGet Packages"
3. Build → Build Solution (Ctrl+Shift+B)

### 2. Archivos Generados

Después de compilar, los archivos estarán en:
```
Release_Package/
├── SINCO.ADPRO.Installer.exe     ← Ejecutar como admin
├── SINCO.ADPRO.Plugin.dll
├── SINCO.ADPRO.addin
├── ClosedXML.dll
└── [otras dependencias]
```

### 3. Instalar en Revit

1. Ir a la carpeta `Release_Package`
2. Clic derecho en `SINCO.ADPRO.Installer.exe`
3. Seleccionar "Ejecutar como administrador"
4. Seleccionar versiones de Revit
5. Clic en "Instalar"

### 4. Usar el Plugin

1. Abrir Revit
2. Buscar pestaña "SINCO - ADPRO" en el Ribbon
3. Clic en "Extracción de Cantidades"
4. Seleccionar categorías/familias (izquierda)
5. Seleccionar propiedades (derecha)
6. Clic en "Exportar"

---

## Para Usuarios Finales

### Requisitos
- Windows 10 o superior
- Autodesk Revit 2022 o superior
- Permisos de Administrador

### Instalación

1. **Descargar** el instalador `SINCO.ADPRO.Installer.exe`

2. **Ejecutar como Administrador**
   - Clic derecho en el archivo
   - Seleccionar "Ejecutar como administrador"

3. **Seleccionar versiones**
   - Marcar las versiones de Revit donde desea instalar
   - El instalador detecta automáticamente las versiones instaladas

4. **Instalar**
   - Clic en "Instalar"
   - Esperar confirmación

### Uso Básico

#### Paso 1: Abrir el Plugin
![Ribbon](https://via.placeholder.com/600x100.png?text=SINCO+-+ADPRO+Ribbon)

En Revit, buscar la pestaña **"SINCO - ADPRO"** y hacer clic en **"Extracción de Cantidades"**

#### Paso 2: Seleccionar Datos

**Panel Izquierdo - Categorías y Familias**
- ✓ Muros
  - ✓ Muro Básico
  - ✓ Muro Cortina
- ✓ Puertas
  - ✓ Puerta Simple
- ✓ Ventanas
  - ✓ Ventana Fija

**Panel Derecho - Propiedades**
- ✓ ID Elemento
- ✓ Nombre del Elemento
- ✓ Categoría
- ✓ Familia y Tipo
- ✓ Nivel
- ✓ Área
- ... (todas seleccionadas por defecto)

#### Paso 3: Exportar
1. Clic en **"Exportar"**
2. Elegir ubicación y nombre del archivo
3. ¡Listo! El archivo Excel se genera automáticamente

### Resultado

El archivo Excel incluirá:
```
┌─────────────────────────────────────────────────┐
│ EXTRACCIÓN DE CANTIDADES - SINCO ADPRO         │
│ Proyecto: Mi Proyecto                           │
│ Fecha: 13/01/2025 15:30:00                     │
├──────┬──────────┬───────────┬──────────────────┤
│ ID   │ Nombre   │ Categoría │ Familia y Tipo   │
├──────┼──────────┼───────────┼──────────────────┤
│ 1234 │ Muro 1   │ Muros     │ Básico - 20cm    │
│ 1235 │ Puerta 1 │ Puertas   │ Simple - 0.90m   │
│ ...  │ ...      │ ...       │ ...              │
└──────┴──────────┴───────────┴──────────────────┘
```

---

## Solución de Problemas Rápida

### ❌ El plugin no aparece en Revit

**Solución**:
1. Cerrar Revit completamente
2. Verificar que el archivo está en:
   `%AppData%\Autodesk\Revit\Addins\[versión]\SINCO.ADPRO.addin`
3. Abrir Revit de nuevo
4. Revisar el journal de Revit si persiste el problema

### ❌ Error al exportar

**Solución**:
1. Verificar que seleccionó al menos una categoría
2. Verificar que tiene permisos de escritura en la carpeta destino
3. Asegurarse de que el archivo no está abierto en Excel

### ❌ El instalador no detecta Revit

**Solución**:
1. Verificar que Revit está instalado
2. Ejecutar el instalador como Administrador
3. Verificar que existe la carpeta:
   `%AppData%\Autodesk\Revit\Addins\`

### ❌ Faltan dependencias

**Solución**:
1. Asegurarse de que todos los archivos del instalador están juntos:
   - SINCO.ADPRO.Installer.exe
   - SINCO.ADPRO.Plugin.dll
   - SINCO.ADPRO.addin
   - ClosedXML.dll
   - (y otras DLLs)
2. No mover archivos individualmente

---

## Desinstalación

### Método 1: Usar el Instalador
1. Ejecutar `SINCO.ADPRO.Installer.exe` como Administrador
2. Clic en "Desinstalar"
3. Confirmar

### Método 2: Manual
1. Eliminar archivos `.addin` de:
   `%AppData%\Autodesk\Revit\Addins\[versión]\`
2. Eliminar carpeta:
   `C:\Program Files\SINCO.ADPRO\`

---

## Logs y Diagnóstico

### Log de Instalación
```
C:\ProgramData\SINCO_ADPRO\log.txt
```

### Journals de Revit
```
%AppData%\Autodesk\Revit\Autodesk Revit [versión]\Journals\
```

---

## Contacto y Soporte

📧 Email: soporte@sinco.com
🌐 Web: www.sinco.com
📖 Documentación completa: Ver README.md

---

## Atajos de Teclado (Dentro del Plugin)

- `Ctrl + A`: Seleccionar todas las categorías
- `Ctrl + D`: Deseleccionar todas las categorías
- `Enter`: Exportar (cuando el botón está enfocado)
- `Esc`: Cancelar

---

## Tips y Trucos

### 💡 Tip 1: Exportar solo una categoría
Deseleccionar todas las categorías y luego seleccionar solo la deseada.

### 💡 Tip 2: Nombre de archivo automático
El nombre de archivo sugerido incluye:
- Nombre del proyecto
- Ubicación (si está disponible)
- Fecha y hora actual

Ejemplo: `Cantidades_EdificioA_BuenosAires_20250113_153000.xlsx`

### 💡 Tip 3: Propiedades personalizadas
El plugin busca automáticamente propiedades personalizadas como:
- SubCapítulo
- Avance
- Ubicación
- Objeto

Si no existen en su modelo, aparecerán vacías en Excel.

### 💡 Tip 4: Excel formateado
El archivo Excel incluye:
- Encabezados con color
- Filas alternadas para mejor legibilidad
- Columnas auto-ajustadas
- Filtros automáticos
- Encabezados congelados

---

**Versión**: 1.0.0
**Última actualización**: Enero 2025
