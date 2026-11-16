# ⌨️ Referencia de Snippets para Sublime Text

Latino.NET incluye **1582 snippets** en Sublime Text: 562 de funciones y estructuras del lenguaje, y 1020 de Magic Strings. Los snippets están organizados por categorías lógicas que puedes activar escribiendo el prefijo y presionando la tecla `Tab`.

---

## 🏗️ Cómo funcionan

Todos los snippets siguen un patrón intuitivo. Solo escribe el **disparador (trigger)** y presiona `Tab`. Muchos permiten navegar entre parámetros usando `Tab` nuevamente.

---

## 📂 Categorías Disponibles

### 📘 Miembros de Librería (Prefijo `.`)

Estas categorías corresponden a los objetos oficiales del lenguaje. Al escribir el nombre y el punto, verás todas las funciones disponibles del módulo.

**Núcleo y uso diario:**

- `mate.` → Funciones matemáticas (48): `abs`, `raiz`, `sen`, `aleatorio`, `convertir`, etc.
- `cadena.` → Manipulación de texto (33): `longitud`, `subcadena`, `reemplazar`, `regex`, etc.
- `sis.` → Sistema, tiempo y entorno (18): `fecha`, `cwd`, `config`, `argumentos`, etc.
- `archivo.` → Gestión de archivos y directorios (23): `leer`, `escribir`, `listar`, `existe`, etc.
- `lista.` / `dic.` → Operaciones de listas y diccionarios (23 y 9).
- `cola.` / `pila.` / `conjunto.` / `tupla.` / `canal.` / `mutex.` → Estructuras de datos y sincronización.

**Módulos del plugin `formulas`** (requieren `incluir("formulas")`):

- `algebraLineal.` → Álgebra lineal (28): `cholesky`, `determinante`, `norma`, `valoresPropios`, etc.
- `calculo.` → Cálculo numérico (12): `trapecio`, `simpson13`, `romberg`, `derivada`, `gradiente`, etc.
- `interpolacion.` → Interpolación (10): `lineal`, `lagrange`, `splineCubico`, `hermite`, etc.
- `optimizacion.` → Optimización (9): `descensoGradiente`, `nelderMead`, `simplexLineal`, etc.
- `edo.` → Ecuaciones diferenciales (10): `rk4`, `adams`, `pvf`, `disparo`, etc.
- `raices.` → Raíces numéricas (7): `biseccion`, `newtonRaphson`, `brent`, `secante`, etc.
- `geometria.`, `cinematica.`, `dinamica.`, `estructuras.`, `fluidos.`, `hidraulica.`, `electricidad.`, `electromagnetismo.`, `termodinamica.`, `quimica.`, `computacion.`, `estadistica.`, `finanzas.`, `senales.`, `optica.`, `ondas.`, `astronomia.` → Ciencias, señal y ingeniería.

**Otros módulos:**

- `sqlite.` → Base de datos (12), requiere `incluir("sqlite")`.
- `tabla.` → Tabla periódica (8), requiere `incluir("tabla")`.
- `validar.` → Validadores (12): `email`, `cedula`, `url`, `telefono`, etc.
- `hilos.` → Hilos (11), requiere `incluir("hilos")`.
- `lapsos.` → Tiempos y lapsos (22), requiere `incluir("lapsos")`.
- `texto.` / `log.` / `proceso.` / `zip.` / `cripto.` / `env.` / `web.` → Texto, registro, procesos, compresión, cifrado, entorno y red.

### ⚙️ Estructuras del Lenguaje (Prefijo `:`)

Snippets para bloques de código y lógica compleja.

- `cond:` → Estructuras condicionales (`si`, `osi`, `sino`, `ternario`).
- `loop:` → Todo tipo de bucles (`para`, `mientras`, `repetir`, `desde`, `rango`).
- `poo:` → Programación Orientada a Objetos (`clase`, `constructor`, `herencia`, `nuevo`).
- `fn:` → Definición de funciones (`simple`, `anonima`, `retornar`, `main`).
- `err:` → Manejo de excepciones y validaciones (`intentar/atrapar`, `validar`).
- `io:` → Entrada y salida de datos rápida (`poner`, `leer`, interpolación `cc`).
- `op:` → Operadores rápidos y expresiones comunes (`inc`, `dec`, `yy`, `oo`).

### 🔤 Magic Strings (Prefijo `str:`)

Estos 1020 snippets insertan **valores de datos** dentro de cadenas, no código. Su mecánica es particular: el disparador tiene tres partes (`str:prefijo:valor`) pero lo que se inserta es **solo el valor limpio**, sin prefijo y sin comillas.

Sublime Text **muestra la lista de candidatos mientras escribes y la va filtrando** a medida que añades caracteres, hasta que queda el coincidente. Escribir `str:vel:` muestra todas las unidades de velocidad; escribir `str:vel:km` deja las que empiezan por `km`; escribir `str:vel:kmh` deja una sola. Presiona `Tab` para insertar.

| Categoría | Prefijo | Qué aporta |
| :--- | :--- | :--- |
| Unidades | `str:vel:`, `str:temp:`, `str:masa:`, `str:vol:`, `str:pres:`, `str:area:`, `str:long:`, `str:en:`, `str:dig:`, `str:ang:` | Unidades para `mate.convertir()` |
| Estilos | `str:estilo:` | Estilos de `cadena.estilo()` |
| Comparación | `str:comp:` | Modos de `cadena.es_igual()` |
| Hash | `str:hash:` | Algoritmos de `cadena.hash()` |
| Regex | `str:regex:` | Patrones para `cadena.extraer()` |
| Formatos | `str:fmt:` | Especificadores de formato |
| Plantillas SQL | `str:sqlEjecutar:`, `str:sqlConsultar:` | Consultas y sentencias listas para usar |
| Tabla | `str:tabla:` | Campos de `tabla.filtrar()` |
| Sistema | `str:sis:`, `str:config:`, `str:fecha:`, `str:lapsos:` | Opciones de `sis.*` |
| Números | `str:mcd:`, `str:mcm:`, `str:factorial:`, `str:compuesto:` | Operaciones numéricas de `mate.*` |
| Colecciones | `str:contiene:`, `str:fusionar:`, `str:llaves:`, `str:valores:`, `str:mapear:`, `str:filtrar:`, `str:transformarLlaves:`, `str:transformarValores:` | Operaciones de `dic.*` y `lista.*` |
| Procesos | `str:ejecutarAsync:` | Estados de `proceso.ejecutarAsync()` |
| Álgebra | `str:algebra:` | Tipos de norma de `algebraLineal.norma()` |
| Retorno | `str:<función>:` | Claves de los diccionarios que devuelven las funciones (por ejemplo, `str:cholesky:L` → `L`) |

> 📖 El catálogo completo, con los 209 prefijos y su valor real, está en `docs/98_Referencia_Strings_Predefinidos.md`.

### 🌍 Funciones y Keywords Globales (Sin prefijo)

Palabras clave y funciones de primer nivel que no requieren pertenecer a un objeto.

- **Conversión:** `acadena`, `alogico`, `anumero`, `tipo`.
- **Control:** `romper`, `continuar`, `incluir`, `retornar`.
- **Ámbito:** `local`, `global`.
- **Utilidades:** `#!` (Crea la cabecera Shebang), `manual`.

---

## 🎯 Recomendación

Te recomendamos escribir el nombre de una librería (por ejemplo: `mate.`) y navegar por la lista de autocompletado de Sublime Text para descubrir todas las funciones disponibles y sus descripciones rápidas.

> ⚠️ **Nota sobre Plugins:** Para utilizar las funciones de la librería `lapsos.`, es obligatorio añadir el comando `incluir("lapsos")` al inicio de tu código, de forma similar a los `using` en C# o `import` en otros lenguajes.
