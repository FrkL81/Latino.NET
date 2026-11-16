# Referencia de Librerías (API)

La documentación de la API de Latino.NET ha sido estructurada de forma modular para facilitar su consulta. Hemos categorizado las herramientas en **Fundamentos**, **Estructuras de Datos**, **Utilidades Core** y el **Ecosistema de Plugins**.

Haz clic en cualquier librería o plugin para ver su documentación detallada...

## 🧭 Índice Temático

### Fundamentos

Las bases del lenguaje, invocación fluida y funciones globales de inicio.

* [📝 Sintaxis de Método y Patrones Avanzados](09_Más_Sintaxis_Errores_Lib/09.00_Sintaxis_Errores_Ecosistema.md) — Invocación fluida, manejo robusto de excepciones (`intentar/atrapar/lanzar`) y patrones idiomáticos (Doble Búfer, Middleware y TDD).
* [🌐 Funciones Globales](09_Más_Sintaxis_Errores_Lib/09.01_globales.md) — Imprimir en consola, leer entradas, conversiones dinámicas de tipo (`tipo`, `alogico`, `anumero`), intercambio en memoria (`trocar`) y el compositor de cadenas (`ç`).

### Estructuras de Datos

Colecciones y tipos nativos para organizar y manipular información en memoria.

* [🔤 cadena](09_Más_Sintaxis_Errores_Lib/09.02_cadena.md) — Manipulación experta de texto con grafemas Unicode, formateo C/NET y expresiones regulares (Regex).
* [📜 lista](09_Más_Sintaxis_Errores_Lib/09.03_lista.md) — Arreglos dinámicos mutables, matrices bidimensionales y operaciones funcionales (map, filter, reduce).
* [📖 dic](09_Más_Sintaxis_Errores_Lib/09.04_dic.md) — Diccionarios clave-valor heterogéneos y transformaciones funcionales de llaves/valores.
* [💎 tupla](09_Más_Sintaxis_Errores_Lib/09.05_tupla.md) — Colecciones inmutables de longitud fija con igualdad estructural, ideales para retornos múltiples.
* [🎯 conjunto](09_Más_Sintaxis_Errores_Lib/09.06_conjunto.md) — Colecciones de elementos únicos y operaciones de la Teoría de Conjuntos (unión, intersección, diferencia).
* [📥 pila](09_Más_Sintaxis_Errores_Lib/09.07_pila.md) — Estructura LIFO (Last-In-First-Out) para algoritmos de retroceso y rastreo.
* [🎫 cola](09_Más_Sintaxis_Errores_Lib/09.08_cola.md) — Estructura FIFO (First-In-First-Out) para gestión de turnos y buffers.

### Utilidades del Sistema y Red

Herramientas integradas en el motor estándar de Latino.NET.

* [🧮 mate](09_Más_Sintaxis_Errores_Lib/09.09_mate.md) — Aritmética, trigonometría, estadística base, conversión de más de 60 unidades y solucionador algebraico de polinomios.
* [🖥️ sis](09_Más_Sintaxis_Errores_Lib/09.10_sis.md) — Información detallada de la plataforma (`sis.plataforma`), ejecución de comandos de sistema y control del intérprete.
* [💻 consola](09_Más_Sintaxis_Errores_Lib/09.11_consola.md) — Opciones de ejecución desde la terminal (CLI), parámetros de entorno y ejecución en línea (`-e`).
* [🔍 validar](09_Más_Sintaxis_Errores_Lib/09.12_validar.md) — Verificación defensiva de formatos (Emails, URLs, contraseñas y cédulas/RUC regionales).
* [📂 archivo](09_Más_Sintaxis_Errores_Lib/09.13_archivo.md) — Entrada/salida segura de archivos, manipulación de directorios, serialización JSON y tablas CSV nativas.
* [🌐 web](09_Más_Sintaxis_Errores_Lib/09.14_web.md) — Cliente HTTP nativo para consumir APIs REST, enviar peticiones con autenticación y descargar archivos.

### Ecosistema de Plugins Oficiales

Librerías de alto rendimiento compiladas sobre .NET 10. Requieren ser cargadas en tu script usando `incluir("nombre_plugin")`.

* [⏱️ lapsos](09_Más_Sintaxis_Errores_Lib/09.15_plugin_lapsos.md) — Gestión avanzada de fechas, horas, zonas horarias y aritmética temporal con precisión de ticks.
* [🧵 hilos](09_Más_Sintaxis_Errores_Lib/09.16_plugin_hilos.md) — Concurrencia real en núcleos independientes, exclusión mutua (`mutex`) y canales Go-style (`canal`).
* [💾 sqlite](09_Más_Sintaxis_Errores_Lib/09.17_plugin_sqlite.md) — Motor de base de datos relacional integrado, transaccional y con prevención de inyección SQL.
* [🔬 formulas](09_Más_Sintaxis_Errores_Lib/09.18_plugin_formulas/09.18.00_Introduccion.md) — Más de 100 fórmulas resolutoras en 26 disciplinas científicas (cálculo, álgebra lineal, física, etc.).
* [📝 texto](09_Más_Sintaxis_Errores_Lib/09.19_plugin_texto.md) — Constructor dinámico de textos gigantes altamente eficiente y seguro en hilos (`StringBuilder` con pipeline).
* [🗜️ zip](09_Más_Sintaxis_Errores_Lib/09.20_plugin_zip.md) — Compresión, descompresión y fragmentación (split & join) de archivos a nivel de bytes.
* [🔐 cripto](09_Más_Sintaxis_Errores_Lib/09.21_plugin_cripto.md) — Cifrado simétrico de nivel militar (AES-256-GCM) y firmas de autenticidad en tiempo constante (HMAC-SHA256).
* [🌍 env](09_Más_Sintaxis_Errores_Lib/09.22_plugin_env.md) — Gestión de variables de entorno y parser industrial de archivos `.env`.
* [📋 log](09_Más_Sintaxis_Errores_Lib/09.23_plugin_log.md) — Sistema de bitácoras estructurado con niveles de severidad y rotación automática de archivos en disco.
* [⚙️ proceso](09_Más_Sintaxis_Errores_Lib/09.24_plugin_proceso.md) — *OS Scripting:* Control sincrónico y asincrónico para orquestación de procesos del sistema operativo.
* [📊 tabla](09_Más_Sintaxis_Errores_Lib/09.25_plugin_tabla.md) — Tabla Periódica de los Elementos: propiedades atómicas, masas molares, balanceo de reacciones e isótopos.

### 📚 Referencias Rápidas

Índices de consulta directa. No son capítulos de aprendizaje: son tablas para buscar una firma, un fragmento o un literal concreto y copiarlo.

* [📝 Fragmentos de Código (Snippets)](96_Referencia_Snippets.md) — Los 67 snippets que emite la extensión de VS Code, con su código exacto expandido.
* [🔤 Palabras Reservadas](97_Referencia_Keywords.md) — Listado completo de palabras clave por niveles de precedencia, y cuáles están prohibidas como nombres de variable.
* [✨ Strings Predefinidos (Magic Strings)](98_Referencia_Strings_Predefinidos.md) — Todos los literales que aceptan las funciones nativas, incluidos los catálogos cerrados de `cadena.extraer()` y `cadena.hash()`.
* [📚 Inventario de Librerías Estándar](99_LIBRERIAS_ESTANDAR.md) — Inventario consolidado de todas las funciones disponibles en la librería estándar.

> **💡 Tip:** Puedes acceder al manual oficial en PDF en cualquier momento ejecutando `latino -e "sis.manual()"` desde tu terminal.
