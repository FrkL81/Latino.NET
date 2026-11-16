# 📂 Utilidad: Organizador de Archivos Inteligente

De la dependencia del Shell a la madurez de la Biblioteca Estándar Nativa.

**Proyecto:** `Utilidad_Organiza_Archivos`  
**Ecosistema:** Latino.NET (Core sobre .NET)  
**Autoría / Documentación:** Arquitectura de Software de Latino.NET  

---

## 🧭 Introducción y Propósito

Este proyecto contiene una utilidad práctica del mundo real: un **Organizador Automático de Archivos**. Su propósito es escanear una carpeta desordenada, clasificar cada archivo según su extensión (imágenes, documentos, multimedia, código fuente, ejecutables, etc.) y reubicarlo de forma limpia en una estructura jerárquica de carpetas (`Categoría/Categoría_extensión/archivo.ext`).

Más allá de su utilidad cotidiana, este repositorio sirve como un **caso de estudio arquitectónico fundamental** dentro de la distribución oficial de Latino.NET:

* **Versión 1 (`organizador_v1/organizador.lat`):** El enfoque pionero. Refleja cómo se resolvían problemas en etapas tempranas del lenguaje, cuando el motor aún no contaba con APIs nativas de I/O de archivos completas y el desarrollador se veía forzado a depender del sistema operativo subyacente (`cmd.exe`, `bash`, `dir`, `ls`, `mkdir`).
* **Versión 2 (`organizador_v2/organizador_v2.lat`):** El enfoque moderno e idiomático. Aprovecha la biblioteca estándar madura de Latino.NET (`archivo.*`), delegando la fuerza bruta directamente a las llamadas de bajo nivel de .NET.

Junto a ambos scripts se incluye el archivo de pruebas `Archivos_para_test.zip`, que contiene **100 archivos sintéticos de 0 bytes** con diversas extensiones para validar el comportamiento del script sin comprometer datos reales.

---

## 📊 Matriz Comparativa: V1 vs. V2

| Dimensión Arquitectónica | Versión 1 (`organizador.lat`) | Versión 2 (`organizador_v2.lat`) |
| :--- | :--- | :--- |
| **Mecanismo de Listado** | Llamada a Shell del SO (`dir /b` o `ls -p`) | API nativa en memoria (`archivo.listar`) |
| **Creación de Carpetas** | Subproceso de consola (`mkdir`, `mkdir -p`) | Syscall nativa de .NET (`archivo.crear_carpeta`) |
| **Extracción de Extensión** | Algoritmo manual de cadenas (`ultimo_indice` + `subcadena`) | Inspección nativa optimizada (`archivo.extension`) |
| **Filtrado de Directorios** | Filtrado frágil por texto en salida del shell | Detección binaria de atributos (`archivo.es_carpeta`) |
| **Rendimiento** | Lento (overhead de fork de procesos del SO) | Ultra-rápido (ejecución directa en memoria RAM) |
| **Seguridad** | Riesgo de inyección de comandos en rutas con comillas | Inmune a inyecciones (sin shells intermedias) |
| **Portabilidad** | Frágil (bifurcaciones `sis.operativo("WIN32")`) | 100% Multiplataforma sin código condicional |
| **Codificación (Encoding)** | Riesgo de mojibake por páginas de código de consola (CP850/1252) | UTF-8 garantizado de extremo a extremo |

---

## 🔍 Análisis Técnico del Arquitecto

### 1. Obtención del listado de archivos

#### En la Versión 1

```latino
si sis.operativo("WIN32")
    listaCruda = sis.ejecutar("dir /b /a-d")
sino
    listaCruda = sis.ejecutar("ls -p | grep -v /")
fin
items = cadena.separar(listaCruda, "\n")
```

* **El problema técnico:** Cada llamada a `sis.ejecutar` lanza un subproceso del sistema operativo (`cmd.exe /c` en Windows o `/bin/bash -c` en Unix). Esto implica crear un proceso, inicializar un intérprete de comandos, capturar descriptores de archivo (pipes de stdout/stderr), leer el texto en memoria y procesar caracteres de retorno de carro (`\r\n` vs `\n`). Si la carpeta tuviese 10,000 archivos, el buffer de texto plano de la consola y la división manual con `cadena.separar` provocarían una penalización masiva de rendimiento y memoria.

#### En la Versión 2

```latino
items = archivo.listar(directorio_actual)
```

* **La ventaja arquitectónica:** `archivo.listar()` delega directamente en `System.IO.Directory.GetFileSystemEntries` de .NET. No se genera ningún subproceso. El sistema operativo devuelve los punteros del sistema de archivos directamente a una estructura `LatinoList` en memoria. La velocidad es órdenes de magnitud superior y no depende de que comandos como `grep` o `dir` estén en el `PATH`.

---

### 2. Detección y filtrado de carpetas

#### En la Versión 1

Se delega la responsabilidad al comando del shell (`/a-d` en Windows, `grep -v /` en Unix). Si en Linux un archivo regular tiene una barra o un formato inesperado en la salida de `ls`, el parser manual falla.

#### En la Versión 2

```latino
si archivo.es_carpeta(item) o item == MI_NOMBRE
    omitidos++
    continuar
fin
```

* **La ventaja arquitectónica:** `archivo.es_carpeta()` lee los metadatos a nivel de inodo / atributos NTFS (`FileAttributes.Directory`). Es una verificación puramente binaria: no adivina basándose en el texto o la extensión, sino que consulta directamente los atributos del archivo en el sistema operativo.

---

### 3. Extracción de extensiones

#### En la Versión 1

```latino
funcion ObtenerExtension(nombreArchivo)
    puntoIndex = cadena.ultimo_indice(nombreArchivo, ".")
    si puntoIndex == -1 retornar nulo fin
    largoTotal = cadena.longitud(nombreArchivo)
    ext = cadena.subcadena(nombreArchivo, puntoIndex + 1, largoTotal - puntoIndex)
    retornar cadena.minusculas(ext)
fin
```

* **El problema técnico:** Para cada archivo se invocan 3 funciones de manipulación de cadenas sobre el intérprete. En archivos complejos (como `archivo.tar.gz` o nombres con puntos múltiples como `documento.final.v2.pdf`), el manejo manual de índices de subcadena requiere un control riguroso de desplazamientos (base 0 vs longitud) y genera strings temporales innecesarios en memoria.

#### En la Versión 2

```latino
ext_cruda = archivo.extension(item)
ext = cadena.minusculas(cadena.reemplazar(ext_cruda, ".", ""))
```

* **La ventaja arquitectónica:** `archivo.extension()` utiliza la implementación canónica y probada de `System.IO.Path.GetExtension`. Resuelve edge-cases del sistema de archivos (como archivos sin nombre que inician con punto `.gitignore`, rutas absolutas o nombres con puntos sucesivos) de forma estandarizada.

---

### 4. Creación de directorios y seguridad

#### En la Versión 1

```latino
si sis.operativo("WIN32")
     sis.ejecutar(ç("if not exist \"|ruta|\" mkdir \"|ruta|\""))
sino
     sis.ejecutar(ç("mkdir -p \"|ruta|\""))
fin
```

* **El peligro latente:** **Inyección de comandos de Shell.** Si una categoría o extensión contuviera caracteres especiales, comillas dobles mal escapadas o símbolos como `&`, `|` o `;`, el shell del sistema operativo intentaría interpretarlos como sentencias arbitrarias. Además, lanzar una llamada al sistema por cada archivo para verificar la existencia de su carpeta destino degrada el tiempo de ejecución dramáticamente.

#### En la Versión 2

```latino
archivo.crear_carpeta(carpeta_destino)
```

* **La ventaja arquitectónica:** `archivo.crear_carpeta()` invoca la API `Directory.CreateDirectory`. Esta función es **idempotente por naturaleza**: si la carpeta ya existe, no hace nada; si no existe, crea recursivamente todos los directorios padres necesarios. No pasa por ningún shell, por lo que es inmune a ataques de inyección y miles de veces más rápida.

---

## 🧪 Banco de Pruebas: `Archivos_para_test.zip`

El paquete incluye `Archivos_para_test.zip`, un conjunto de **100 archivos de prueba sintéticos** con tamaño de **0 bytes**.

### ¿Por qué archivos de 0 bytes?

1. **Seguridad:** Permite ejecutar pruebas de estrés y movimiento de archivos sin riesgo de pérdida de información personal.
2. **Eficiencia de almacenamiento:** 100 archivos reales de audio, video o binarios ocuparían cientos de megabytes. Un archivo de 0 bytes solo crea la entrada en el directorio (metadatos), permitiendo distribuir un ZIP de apenas unos cuantos kilobytes.
3. **Validación de cobertura:** Incluye extensiones pertenecientes a todas las categorías definidas en la configuración (`.pdf`, `.jpg`, `.mp4`, `.zip`, `.lat`, `.py`, `.json`, etc.) y archivos sin categoría definida para probar el contador de archivos omitidos.

---

## 🚀 Instrucciones de Ejecución

### Paso 1: Preparar el entorno de prueba

Descomprime el contenido de `Archivos_para_test.zip` en la misma carpeta donde vas a ejecutar el script (o copia los scripts a la carpeta descomprimida).

### Paso 2: Ejecutar la Versión 1

```bash
cd organizador_v1
latino organizador.lat
```

### Paso 3: Ejecutar la Versión 2 (Recomendada)

```bash
cd organizador_v2
latino organizador_v2.lat
```

### Estructura resultante esperada

Tras la ejecución, los archivos dispersos se reorganizarán automáticamente:

```text
.
├── organizador_v2.lat
├── Imágenes/
│   ├── Imágenes_jpg/
│   │   └── foto_01.jpg
│   └── Imágenes_png/
│       └── captura.png
├── Documentos/
│   ├── Documentos_pdf/
│   │   └── reporte.pdf
│   └── Documentos_docx/
│       └── contrato.docx
├── Videos/
│   └── Videos_mp4/
│       └── clip.mp4
└── Codigo_Latino/
    └── Codigo_Latino_lat/
        └── modulo.lat
```

---

## 💡 Lecciones de Diseño para el Desarrollador de Latino.NET

1. **Aplica la Regla de Oro #2: "Delega la fuerza bruta a la biblioteca nativa".**  
   Nunca recurras a `sis.ejecutar` para operaciones que el lenguaje ya provee de forma nativa (`archivo.*`, `cadena.*`, `lista.*`). Cada llamada al sistema operativo crea overhead y fragmenta la portabilidad.
2. **Escribe código independiente de la plataforma.**  
   Las verificaciones de tipo `si sis.operativo("WIN32")` deben reservarse únicamente para casos extremos de interoperabilidad de bajo nivel. El módulo `archivo` de Latino.NET normaliza internamente las barras diagonales (`/` vs `\`) y las peculiaridades del sistema de archivos en Windows, Linux y macOS.
3. **Favorece la inmutabilidad y la seguridad contra inyecciones.**  
   Mover archivos a través de las APIs nativas del lenguaje garantiza que nombres con espacios, comillas o caracteres Unicode (tildes, eñes, emojis) se procesen de forma exacta y segura sin requerir secuencias de escape complejas en el shell.
