# Referencia: Strings Predefinidos (Magic Strings)

Esta tabla documenta **todos** los valores de cadena predefinidos que las funciones nativas de Latino.NET reconocen. Úsala como referencia rápida para evitar errores de sintaxis.
> **💡 Cómo funcionan los prefijos `@` (etiquetas de autocompletado).** El prefijo con arroba es una **máscara visual del editor**, no sintaxis del lenguaje. Al escribir `"@vel: "` dentro de una cadena, el editor sugiere los valores agrupados bajo esa etiqueta, pero **al seleccionar uno inserta únicamente el valor limpio**, sin el prefijo, que es lo que el motor espera entre comillas.
>
> Es decir: la etiqueta `@vel:` sirve para filtrar y agrupar en el panel de sugerencias; **nunca se escribe en el código**. Escribir `"vel: kmh"` o `"@kmh"` hace que `mate.convertir` falle con *«Unidades no soportadas»*.
>
> ```latino
> // Correcto: la unidad desnuda, que es lo que el autocompletado inserta
> resultado = mate.convertir(100, "kmh", "ms")
> ```
>
> Lo mismo aplica a los demás prefijos de entrada: `@comp` para los modos de `cadena.es_igual()`, `@temp` para las unidades de `mate.convertir()`, `@algebra` para los tipos de `algebraLineal.norma()`, o `@sqlEjecutar` para las plantillas de `sqlite.ejecutar()`. **La etiqueta agrupa; el valor desnudo viaja.**

## 🔡 Secuencias de Escape (Lexer)

Latino.NET soporta secuencias de escape estándar dentro de cadenas de texto (usando `"` o `'`).

| Secuencia | Descripción |
| :--- | :--- |
| `\n` | Salto de línea (Newline) |
| `\r` | Retorno de carro (Carriage Return) |
| `\t` | Tabulación horizontal |
| `\\` | Barra invertida literal |
| `\"` | Comilla doble literal |
| `\'` | Comilla simple literal |
| `\b` | Retroceso (Backspace) |
| `\f` | Avance de página (Form Feed) |
| `\v` | Tabulación vertical |
| `\a` | Alerta sonora (Beep/Bell) |
| `\uXXXX` | Carácter Unicode (ej: `\u00A9` para ©) |

## 🛠️ Funciones Globales

### `tipo()` - Valores de Retorno

| Valor Retornado | Significado |
| :--- | :--- |
| `"decimal"` | Número (Latino.NET trata todo como decimal internamente) |
| `"cadena"` | Texto |
| `"logico"` | cierto / falso (valores de salida) |
| `"nulo"` | Valor nulo |
| `"lista"` | Colección de elementos `[]` |
| `"diccionario"` | Mapa de llaves/valores `{}` |
| `"tupla"` | Colección inmutable `()` |
| `"funcion"` | Función nativa o definida por usuario |
| `"pila"` | Estructura LIFO (pila.crear()) |
| `"cola"` | Estructura FIFO (cola.crear()) |
| `"conjunto"` | Conjunto único (conjunto.crear()) |
| `"desconocido"` | Definición de clase (la clase en sí, no la instancia) |
| *`"NombreClase"`* | Instancia de clase (devuelve el nombre de la clase, no `"instancia"`) |
| `"cadena"` | **También** el valor de la variable de un `atrapar (e)`: la excepción es una cadena de texto con el prefijo `[Línea N]` |
| `"cadena"` | `tipo(tipo(x))`: el nombre del tipo es texto a su vez. Admite la API de `cadena` (`cadena.longitud(tipo(42))` → `7`) |

Las quince filas están verificadas ejecutando cada caso en v1.0.81. Nota: `tipo()` sobre la **clase misma** (`tipo(Perro)`) devuelve `"desconocido"`; solo la **instancia** devuelve el nombre de la clase. Y `tipo(tipo(x))` devuelve `"cadena"`, porque el resultado de `tipo()` siempre es texto.

### `alogico()` - Semántica de Verdad

La conversión a lógico en Latino.NET se basa en la **existencia del valor**, no en coincidencia de texto:

| Valor | Resultado | Motivo |
| :--- | :--- | :--- |
| `nulo` | **falso** | Ausencia de valor |
| `""` (cadena vacía) | **falso** | No existe contenido |
| `"0"`, `"falso"`, `"hola"` | **cierto** | La cadena tiene contenido |
| `0`, `0.0`, `NaN` | **falso** | Cero o no-número |
| `1`, `-1`, `3.14` | **cierto** | Número distinto de cero |
| `falso` | **falso** | Booleano falso |
| `verdadero`/`cierto` | **cierto** | Booleano verdadero |
| `[]`, `{}`, `()` | **cierto** | El objeto existe (no es nulo) |

> **Nota:** `alogico()` NO analiza el texto de la cadena. La cadena `"0"` es verdadera porque tiene longitud > 0. Solo la cadena vacía `""` es falsa.

### `limpiar()` vs `sis.limpiar()`

1. **`limpiar([modo])` (Global):** `"pantalla"`, `"todo"`, `"cursor"`.
2. **`sis.limpiar()` (Librería `sis`):** No acepta argumentos.

## 🖨️ Sistema de Formateo (Diferencias)

### 1. `imprimirf` / `cadena.formato` (C-Style)

| Espec. | Tipo | Notas |
| :--- | :--- | :--- |
| `%s` | Cadena | |
| `%d`, `%i` | Entero | |
| `%f` | Flotante | |
| `%x`, `%X` | Hex | Minúsculas / Mayúsculas |
| `%o` | Octal | |
| `%%` | Literal | Escribe `%` |

### 2. `escribirf` / `ponerf` (Native .NET)

| Espec. | Ejemplo | Resultado |
| :--- | :--- | :--- |
| `{0}` | `escribirf("{0}", 10)` | `10` |
| `{0:C}` | `escribirf("{0:C}", 5)` | `$5.00` |
| `{0:X}` | `escribirf("{0:X}", 255)` | `FF` |
| `{0:F2}` | `escribirf("{0:F2}", 3.1415)` | `3.14` |

## 🖥️ Librería `sis` (Sistema)

### `sis.config()` - Configuración del Motor

| Clave | Tipo | Descripción | Valor Por Defecto |
| :--- | :--- | :--- | :--- |
| `"max_stack"` | Entero | Límite de profundidad de recursión | 100 |
| `"emoji_spacing"` | Entero | Auto-espaciado de emojis (0-10) | 0 |
| `"regex_timeout"` | Número | Límite de tiempo para Regex (seg) | 2.0 |

### `sis.operativo()` - Detección de Sistema Operativo

| String Válido | Sistema Detectado |
| :--- | :--- |
| `"WINDOWS"`, `"WIN32"`, `"WIN"` | Microsoft Windows |
| `"LINUX"`, `"UNIX"` | Linux / Distribuciones Unix |
| `"MAC"`, `"MACOS"`, `"OSX"`, `"APPLE"` | Apple macOS |

### `sis.fecha()` - Componentes de Fecha

| String Válido | Componente | Rango de Valores |
| :--- | :--- | :--- |
| `"seg"`, `"segundo"` | Segundo | 0 - 59 |
| `"min"`, `"minuto"` | Minuto | 0 - 59 |
| `"hora"` | Hora | 0 - 23 |
| `"dia"`, `"d_mes"` | Día del mes | 1 - 31 |
| `"mes"` | Mes | 1 - 12 |
| `"anio"`, `"año"`, `"year"` | Año | 1 - 9999 |
| `"d_semana"`, `"dia_semana"` | Día de la semana | 0 - 6 |
| `"d_año"`, `"d_anio"`, `"dia_año"` | Día del año | 1 - 366 |

## 🔤 Librería `cadena` (String)

### `cadena.estilo()` - Modos de Transformación

| Modo | Descripción |
| :--- | :--- |
| `"alternar"` | Estilo: Alterna MAY/min (SwapCase) |
| `"camel_serpiente"` | Estilo: Híbrido (hola_Mundo_Cruel) |
| `"camello"` | Estilo: Estilo camelCase (ej: holaMundo) |
| `"capitalizar"` | Estilo: Primera letra en mayúscula |
| `"constante"` | Estilo: Estilo SCREAMING_SNAKE (ej: HOLA_MUNDO) |
| `"invertir"` | Estilo: Invierte el orden de los caracteres |
| `"kebab"` | Estilo: Estilo kebab-case (ej: hola-mundo) |
| `"mayusculas"` | Estilo: Convierte todo a MAYÚSCULAS |
| `"minusculas"` | Estilo: Convierte todo a minúsculas |
| `"oracion"` | Estilo: Estilo Oración (ej: Hola mundo) |
| `"pascal"` | Estilo: Estilo PascalCase (ej: HolaMundo) |
| `"punto"` | Estilo: Estilo dot (ej: hola.mundo) |
| `"ruta"` | Estilo: Estilo ruta (detecta SO automáticamente) |
| `"ruta_unix"` | Estilo: Estilo ruta Unix (ej: hola/mundo) |
| `"ruta_windows"` | Estilo: Estilo ruta Windows (ej: hola\\mundo) |
| `"serpiente"` | Estilo: Estilo snake_case (ej: hola_mundo) |
| `"titulo"` | Estilo: Estilo Título (ej: Hola Mundo) |
| `"tren"` | Estilo: Estilo Train-Case (ej: Hola-Mundo) |

### `cadena.extraer()` - Modos de Extracción

| Modo | Descripción |
| :--- | :--- |
| `"decimales"` | Regex: Extraer números con punto |
| `"digitos"` | Regex: Extraer números enteros |
| `"emails"` | Regex: Extraer correos electrónicos |
| `"fechas"` | Regex: Extraer fechas |
| `"hashtags"` | Regex: Extraer hashtags (#...) |
| `"letras"` | Regex: Extraer solo texto |
| `"menciones"` | Regex: Extraer menciones (@...) |
| `"monedas"` | Regex: Extraer valores monetarios |
| `"telefonos"` | Regex: Extraer números de teléfono |
| `"urls"` | Regex: Extraer enlaces web |

> **Son 10 modos y la lista es cerrada.** No hay comodines ni Alias: un modo desconocido falla con `Modo de extracción desconocido: 'X'. Modos válidos: digitos, letras, decimales, emails, urls, telefonos, hashtags, menciones, monedas, fechas`.
> Los nombres se comparan **sin distinguir mayúsculas**, pero no admiten acentos (`telefonos`, no `teléfonos`).

### `cadena.hash()` - Algoritmos Soportados

| Algoritmo | Descripción |
| :--- | :--- |
| `"sha256"` | Hash: Algoritmo SHA-256 (Seguro) — **es el valor por defecto** |
| `"md5"` | Hash: Algoritmo MD5 (Rápido) |
| `"sha1"` | Hash: Algoritmo SHA-1 (Legacy) |
| `"sha512"` | Hash: Algoritmo SHA-512 (Máximo) |

> **Insensible a mayúsculas/minúsculas:** `"SHA256"`, `"sha256"` y `"Sha256"` son equivalentes. Si se omite el segundo argumento, se usa **SHA-256**.
> La lista completa son **cuatro** algoritmos; cualquier otro da `Algoritmo de hash no soportado: 'X'. Usa: MD5, SHA1, SHA256, SHA512.`

### `cadena.es_igual()` - Modos de Comparación

| Modo | Descripción |
| :--- | :--- |
| `"exacta"` | Comparación: Comparación exacta (default) |
| `"ignora_caso"` | Comparación: Ignora diferencias MAY/min |
| `"normalizada"` | Comparación: Ignora acentos y MAY/min |
| `"comodin"` | Comparación: Usa comodines (* y ?) |
| `"comodin_insensible"` | Comparación: Comodines + ignora MAY/min |

## Especificadores de Formato

### 1. C-Style (`imprimirf` / `cadena.formato`)

| Especificador | Tipo | Descripción |
| :--- | :--- | :--- |
| `%s` | Cadena | Texto |
| `%d` o `%i` | Entero | Número entero decimal |
| `%.2f` | Decimal | Flotante con precisión (ej: 2 decimales) |
| `%x` | Hex | Hexadecimal minúsculas |
| `%X` | Hex | Hexadecimal MAYÚSCULAS |
| `%o` | Octal | Número octal |
| `%c` | Carácter | Carácter individual |
| `%e` | Científica | Notación científica |
| `%g` | General | Formato general (auto) |
| `%%` | Literal | Símbolo `%` literal |

### 2. .NET Nativo (`escribirf` / `ponerf`)

> **Nota:** `ponerf` es un **alias directo** de `escribirf`. Ambas comparten la misma implementación y estilo de formato .NET Nativo. No existe una versión "C-Style" de `ponerf`.

| Especificador | Tipo | Descripción |
| :--- | :--- | :--- |
| `{0}` | Argumento | Argumento básico (posición 0) |
| `{0:C}` | Moneda | Formato moneda ($5.00) |
| `{0:X}` | Hex | Hexadecimal |
| `{0:F2}` | Decimal | Flotante con 2 dígitos |
| `{0:N2}` | Número | Número con separador de miles |
| `{0,-15}` | Alineación | Alineado a la izquierda (15 espacios) |
| `{0,15}` | Alineación | Alineado a la derecha (15 espacios) |

> **Nota:** `escribirf` **no** agrega salto de línea automático; requiere `\n`.

### 3. Fecha y Hora (`sis.tiempo`)

| Especificador | Componente | Rango |
| :--- | :--- | :--- |
| `%Y` | Año 4 dígitos | 1-9999 |
| `%y` | Año 2 dígitos | 0-99 |
| `%m` | Mes | 01-12 |
| `%d` | Día del mes | 01-31 |
| `%H` | Hora 24h | 00-23 |
| `%I` | Hora 12h | 01-12 |
| `%M` | Minuto | 00-59 |
| `%S` | Segundo | 00-59 |
| `%p` | AM/PM | AM / PM |

> **Nota:** No confundir el `%d` de fecha (día) con el `%d` numérico de `imprimirf`.

## `mate.convertir()` - Unidades Disponibles

| Categoría | Unidades disponibles |
| :--- | :--- |
| **Ángulo** | `"grados"`, `"radianes"` |
| **Área** | `"m2"`, `"km2"`, `"ha"`, `"acre"`, `"fanega"` |
| **Digital** | `"byte"`, `"kb"`, `"mb"`, `"gb"`, `"tb"` |
| **Energía** | `"j"`, `"caloria"`, `"kcal"`, `"btu"`, `"ev"` |
| **Longitud** | `"m"`, `"km"`, `"cm"`, `"mm"`, `"pulg"`, `"pies"`, `"yd"`, `"millas"`, `"milla_nautica"`, `"vara"`, `"legua"`, `"codo"`, `"palmo"`, `"ua"`, `"anio_luz"` |
| **Masa** | `"kg"`, `"g"`, `"mg"`, `"lb"`, `"oz"`, `"t"`, `"ton_corta"`, `"arroba"`, `"quintal"`, `"grano"`, `"quilate"` |
| **Presión** | `"pa"`, `"psi"`, `"bar"`, `"atm"`, `"torr"` |
| **Temperatura** | `"celsius"`, `"fahrenheit"`, `"kelvin"` |
| **Velocidad** | `"kmh"`, `"mph"`, `"nudo"`, `"mach"`, `"ms"` |
| **Volumen** | `"l"`, `"ml"`, `"gl"`, `"gl_uk"`, `"pinta"`, `"barril"`, `"taza"` |

## `algebraLineal.norma()` - Tipos de Norma

| Modo | Descripción |
| :--- | :--- |
| `"1"` | Norma L1 (Suma de valores absolutos en vectores / Columna máxima en matrices) |
| `"2"` o `"euclidiana"` | Norma L2 Euclidiana (defecto en vectores) |
| `"frobenius"` o `"f"` | Norma de Frobenius (defecto en matrices) |
| `"inf"` o `"∞"` | Norma infinito (Máximo absoluto en vectores / Fila máxima en matrices) |

## 📊 Librería `tabla` (Periódica)

| Filtro | Descripción |
| :--- | :--- |
| `"categoria"` | Tabla: Filtro: Categoría química |
| `"periodo"` | Tabla: Filtro: Período (1-7) |
| `"bloque"` | Tabla: Filtro: Bloque (s, p, d, f) |

## ⏱️ Plugin `lapsos` (Tiempo)

| Unidad | Descripción |
| :--- | :--- |
| `"dia"` o `"dias"` | Días |
| `"mes"` o `"meses"` | Meses |
| `"anio"`, `"ano"`, `"año"` o `"años"` | Años |
| `"hora"` o `"horas"` | Horas |
| `"min"` o `"minutos"` | Minutos |
| `"seg"` o `"segundos"` | Segundos |

> **Nota sobre las claves de las funciones «omni-solucionadoras».** La tabla de cada función lista **todas** las claves
> que puede devolver. Las funciones que reciben un diccionario con **3 de 4** valores y calculan la cuarta no son una
> excepción: medido contra el motor, `quimica.michaelisMenten` devuelve sus seis claves (`resultado`, `unidad`, `formula`,
> `incognita`, `saturacion_pct` y `regimen`) con cualquiera de las cuatro incógnitas, sin condiciones. Se verificó con `v`,
> `Vmax`, `Km` y `S`, y el conjunto de claves resultó idéntico en los cuatro casos.

## 📥 Índice de Magic Strings de Entrada

El catálogo de magic strings tiene **dos familias**. Este índice cubre la primera: los **valores fijos que el autor elige** y que una función consume como argumento de cadena. La segunda son las **claves de retorno**, documentadas en la sección siguiente.

Es un índice de navegación, no una duplicación: **cada familia remite a la sección donde sus valores ya están detallados**. Todos los conteos se verificaron ejecutando cada función en Latino.NET v1.0.81.

El motor nunca ejecuta el prefijo `@`: la etiqueta pertenece al editor y el valor va desnudo entre comillas, como en `mate.convertir(1, "kmh", "ms")`.

| Prefijo | Valores | Lo consume | Detalle en |
| :--- | ---: | :--- | :--- |
| `@algebra` | 3 | `algebraLineal.norma()` | `algebraLineal.norma()` — Tipos de Norma |
| `@ang` | 2 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@area` | 5 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@comp` | 5 | `cadena.es_igual()` | Secuencias de Escape (Lexer) |
| `@compuesto` | 2 | `mate.compuesto()` | Diccionarios de Retorno por Librería |
| `@config` | 3 | `sis.config()` | `sis.config()` — Claves de Configuración |
| `@contiene` | 1 | `cadena.contiene()` | Diccionarios de Retorno por Librería |
| `@dig` | 5 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@ejecutarAsync` | 5 | `hilos.ejecutarAsync()` | Diccionarios de Retorno por Librería |
| `@en` | 5 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@estilo` | 18 | `cadena.estilo()` | `cadena.estilo()` — Estilos Disponibles |
| `@factorial` | 2 | `mate.factorial()` | Diccionarios de Retorno por Librería |
| `@fecha` | 11 | `sis.fecha()` | `sis.fecha()` — Componentes de Fecha |
| `@filtrar` | 3 | `tabla.filtrar()` | Diccionarios de Retorno por Librería |
| `@fmt` | 20 | `sis.escribir()` | Diccionarios de Retorno por Librería |
| `@fusionar` | 1 | `dic.fusionar()` | Diccionarios de Retorno por Librería |
| `@hash` | 4 | `cadena.hash()` | `cadena.hash()` — Algoritmos |
| `@lapsos` | 6 | `lapsos.esperar()` | Plugin `lapsos` — Guiones de Tiempo |
| `@llaves` | 1 | `dic.llaves()` | Diccionarios de Retorno por Librería |
| `@long` | 15 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@mapear` | 2 | `dic.mapear()` | Diccionarios de Retorno por Librería |
| `@masa` | 11 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@mcd` | 2 | `mate.mcd()` | Diccionarios de Retorno por Librería |
| `@mcm` | 2 | `mate.mcm()` | Diccionarios de Retorno por Librería |
| `@pres` | 5 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@regex` | 10 | `cadena.extraer()` | Secuencias de Escape (Lexer) |
| `@sis` | 6 | `sis.limpiar()` | Secuencias de Escape (Lexer) |
| `@sqlConsultar` | 8 | `sqlite.consultar()` | Plantillas y Snippets SQL |
| `@sqlEjecutar` | 7 | `sqlite.ejecutar()` | Plantillas y Snippets SQL |
| `@tabla` | 3 | `tabla.filtrar()` | Librería `tabla` — Filtros |
| `@temp` | 3 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@transformarLlaves` | 1 | `dic.transformarLlaves()` | Diccionarios de Retorno por Librería |
| `@transformarValores` | 1 | `dic.transformarValores()` | Diccionarios de Retorno por Librería |
| `@valores` | 1 | `dic.valores()` | Diccionarios de Retorno por Librería |
| `@vel` | 5 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |
| `@vol` | 7 | `mate.convertir()` | `mate.convertir()` — Unidades Disponibles |

**Total: 36 prefijos y 191 valores de entrada.**

### Correcciones respecto al catálogo anterior

Tres familias del catálogo heredado declaraban valores que el motor no acepta. Se verificaron contra la ejecución real antes de documentarlas:

| Prefijo | Valor obsoleto | Valor real | Comprobación |
| :--- | :--- | :--- | :--- |
| `@algebra` | `norma_1`, `norma_2` y `norma_inf` | `1`, `frobenius` e `inf` | `algebraLineal.norma()` solo admite esos tres tipos de norma. |
| `@descensoGradiente` | `f_x` | `tasa_final` | Devuelve `optimo`, `f_optimo`, `convergio`, `iteraciones`, `error_estimado`, `formula` y `tasa_final`. |
| `@seccionAurea` | `f_x` | Retirado | Devuelve `optimo`, `f_optimo`, `convergio`, `iteraciones`, `error_estimado` y `formula`. |

`@descensoGradiente` y `@seccionAurea` se reagruparon con las claves de retorno porque enumeran lo que una función **produce**, no un valor que el autor le pase. `@pitagorica` se reagrupó por la misma razón: `mate.pitagorica()` devuelve un diccionario con la clave `c`, y `a`, `b` y `formula` no son claves suyas.

## Diccionarios de Retorno por Librería

### Álgebra Lineal

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@resolver` | `"solucion"` | solucion |
| `@resolver` | `"norma_residuo"` | norma_residuo |
| `@resolver` | `"n"` | n |
| `@resolver` | `"formula"` | Expresión matemática |
| `@factorizacionLU` | `"L"` | L |
| `@factorizacionLU` | `"U"` | U |
| `@factorizacionLU` | `"P"` | P |
| `@factorizacionLU` | `"determinante"` | determinante |
| `@factorizacionLU` | `"n"` | n |
| `@factorizacionLU` | `"formula"` | Expresión matemática |
| `@cholesky` | `"L"` | L |
| `@cholesky` | `"Lt"` | Lt |
| `@cholesky` | `"determinante"` | determinante |
| `@cholesky` | `"n"` | n |
| `@cholesky` | `"formula"` | Expresión matemática |
| `@invertir` | `"inversa"` | inversa |
| `@invertir` | `"error_frobenius"` | error_frobenius |
| `@invertir` | `"n"` | n |
| `@invertir` | `"formula"` | Expresión matemática |
| `@determinante` | `"resultado"` | resultado |
| `@determinante` | `"n"` | n |
| `@determinante` | `"formula"` | Expresión matemática |
| `@valorPropioDominante` | `"valor_propio"` | valor_propio |
| `@valorPropioDominante` | `"vector_propio"` | vector_propio |
| `@valorPropioDominante` | `"iteraciones"` | iteraciones |
| `@valorPropioDominante` | `"convergio"` | convergio |
| `@valorPropioDominante` | `"error_estimado"` | Error estimado del valor propio |
| `@valorPropioDominante` | `"formula"` | Expresión matemática |
| `@valoresPropios` | `"valores_propios"` | valores_propios |
| `@valoresPropios` | `"n"` | n |
| `@valoresPropios` | `"formula"` | Expresión matemática |
| `@valoresPropios` | `"nota"` | Nota explicativa |
| `@norma` | `"resultado"` | resultado |
| `@norma` | `"formula"` | Expresión matemática |
| `@norma` | `"tipo"` | Tipo de norma aplicada |
| `@multiplicar` | `"resultado"` | resultado |
| `@multiplicar` | `"formula"` | Expresión matemática |
| `@multiplicar` | `"filas"` | Número de filas del resultado |
| `@multiplicar` | `"columnas"` | Número de columnas del resultado |
| `@transponer` | `"resultado"` | Matriz transpuesta Aᵀ |
| `@transponer` | `"filas"` | Número de filas del resultado |
| `@transponer` | `"columnas"` | Número de columnas del resultado |
| `@transponer` | `"formula"` | Expresión matemática |

### Cálculo

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@romberg` | `"resultado"` | resultado |
| `@romberg` | `"errorEstimado"` | errorEstimado |
| `@romberg` | `"nivelesUsados"` | nivelesUsados |
| `@romberg` | `"formula"` | Expresión matemática |
| `@romberg` | `"convergio"` | Si el método alcanzó la tolerancia |
| `@romberg` | `"evaluacionesF"` | Número de evaluaciones de la función |
| `@trapecio` | `"resultado"` | resultado |
| `@trapecio` | `"errorEstimado"` | errorEstimado |
| `@trapecio` | `"nSubintervalos"` | nSubintervalos |
| `@trapecio` | `"h"` | h |
| `@trapecio` | `"formula"` | Expresión matemática |
| `@simpson13` | `"resultado"` | Valor de la integral |
| `@simpson13` | `"errorEstimado"` | Error estimado de la aproximación |
| `@simpson13` | `"nSubintervalos"` | Número de subintervalos usados |
| `@simpson13` | `"h"` | Tamaño del subintervalo |
| `@simpson13` | `"formula"` | Expresión matemática |
| `@simpson38` | `"resultado"` | Valor de la integral. **Exige que `n` sea múltiplo de 3** |
| `@simpson38` | `"nSubintervalos"` | Número de subintervalos usados |
| `@simpson38` | `"h"` | Tamaño del subintervalo |
| `@simpson38` | `"formula"` | Expresión matemática |
| `@cuadraturaGauss` | `"resultado"` | Valor de la integral |
| `@cuadraturaGauss` | `"puntosUsados"` | Puntos de cuadratura usados (2, 3, 4 o 5) |
| `@cuadraturaGauss` | `"exactoHastaGrado"` | Grado de polinomio integrado exactamente |
| `@cuadraturaGauss` | `"formula"` | Expresión matemática |
| `@monteCarlo` | `"resultado"` | resultado |
| `@monteCarlo` | `"errorEstimado"` | errorEstimado |
| `@monteCarlo` | `"formula"` | Expresión matemática |
| `@monteCarlo` | `"nMuestras"` | Número de muestras usadas |
| `@monteCarlo` | `"intervaloConfianza95"` | Intervalo de confianza del 95% |
| `@derivada` | `"resultado"` | resultado |
| `@derivada` | `"hUsado"` | hUsado |
| `@derivada` | `"formula"` | Expresión matemática |
| `@derivada` | `"ordenError"` | Orden del error del método |
| `@derivada` | `"metodo"` | Método numérico empleado |
| `@segundaDerivada` | `"resultado"` | resultado |
| `@segundaDerivada` | `"hUsado"` | hUsado |
| `@segundaDerivada` | `"formula"` | Expresión matemática |
| `@segundaDerivada` | `"ordenError"` | Orden del error del método |
| `@derivadaRichardson` | `"resultado"` | resultado |
| `@derivadaRichardson` | `"errorEstimado"` | errorEstimado |
| `@derivadaRichardson` | `"formula"` | Expresión matemática |
| `@derivadaRichardson` | `"nivelesUsados"` | Niveles de refinamiento usados |
| `@gradiente` | `"gradiente"` | gradiente |
| `@gradiente` | `"norma"` | norma |
| `@gradiente` | `"formula"` | Expresión matemática |
| `@gradiente` | `"nVariables"` | Número de variables |
| `@gradiente` | `"hUsado"` | Paso h empleado |
| `@jacobiana` | `"jacobiana"` | La matriz jacobiana |
| `@jacobiana` | `"filas_m"` | Número de filas (derivadas parciales) |
| `@jacobiana` | `"columnas_n"` | Número de columnas (variables) |
| `@jacobiana` | `"h_usado"` | Paso finito usado en la derivación |
| `@jacobiana` | `"formula"` | Expresión matemática |
| `@longitudArco` | `"resultado"` | resultado |
| `@longitudArco` | `"formula"` | Expresión matemática |
| `@longitudArco` | `"n_subintervalos"` | Número de subintervalos |

### Cinemática

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@mru` | `"resultado"` | resultado |
| `@mru` | `"incognita"` | incognita (v, d, o t) |
| `@mru` | `"formula"` | Expresión matemática |
| `@mru` | `"unidad"` | Unidad del resultado |
| `@mruaVelocidad` | `"resultado"` | resultado |
| `@mruaVelocidad` | `"formula"` | Expresión matemática |
| `@mruaVelocidad` | `"unidad"` | Unidad del resultado |
| `@mruaPosicion` | `"resultado"` | resultado |
| `@mruaPosicion` | `"formula"` | Expresión matemática |
| `@mruaPosicion` | `"unidad"` | Unidad del resultado |
| `@tiroParabolico` | `"altura_maxima"` | altura_maxima |
| `@tiroParabolico` | `"alcance_maximo"` | alcance_maximo |
| `@tiroParabolico` | `"tiempo_vuelo"` | tiempo_vuelo |
| `@tiroParabolico` | `"formula"` | Expresión matemática |

### Diccionario

| Función | Tipo que retorna | Contenido |
| :--- | :--- | :--- |
| `@mapear` | `diccionario` | Una entrada por elemento, con las claves que construya tu función |
| `@llaves` | `lista` | Las claves, en una lista plana. **No** devuelve un diccionario |
| `@valores` | `lista` | Los valores, en una lista plana. **No** devuelve un diccionario |
| `@contiene` | `logico` | `cierto` o `falso`. **No** devuelve un diccionario |
| `@fusionar` | `diccionario` | Un diccionario nuevo con las claves combinadas. **No** envuelve en `"resultado"` |
| `@transformarValores` | `diccionario` | Mismo diccionario con los valores transformados |
| `@filtrar` | `diccionario` | Solo las entradas cuyo valor pase la función que recibe |
| `@transformarLlaves` | `diccionario` | Mismo diccionario con las claves transformadas |

### Dinámica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@energiaMecanica` | `"resultado"` | resultado |
| `@energiaMecanica` | `"energia_cinetica"` | energia_cinetica |
| `@energiaMecanica` | `"energia_potencial"` | energia_potencial |
| `@energiaMecanica` | `"formula"` | Expresión matemática |
| `@energiaMecanica` | `"unidad"` | Unidad del resultado |
| `@penduloSimple` | `"resultado"` | resultado |
| `@penduloSimple` | `"frecuencia"` | frecuencia |
| `@penduloSimple` | `"formula"` | Expresión matemática |
| `@penduloSimple` | `"unidad"` | Unidad del resultado |
| `@segundaLeyRotacional` | `"resultado"` | resultado |
| `@segundaLeyRotacional` | `"formula"` | Expresión matemática |
| `@segundaLeyRotacional` | `"unidad"` | Unidad del resultado |
| `@newtonFuerza` | `"resultado"` | Fuerza en Newtons (F = m·a) |
| `@newtonFuerza` | `"unidad"` | N |
| `@newtonFuerza` | `"formula"` | Expresión matemática |

### EDOs

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@rk4` | `"trayectoria"` | trayectoria |
| `@rk4` | `"y_final"` | y_final |
| `@rk4` | `"t_final"` | t_final |
| `@rk4` | `"h"` | h |
| `@rk4` | `"n_pasos"` | n_pasos |
| `@rk4` | `"formula"` | Expresión matemática |
| `@rkf45` | `"trayectoria"` | trayectoria |
| `@rkf45` | `"y_final"` | y_final |
| `@rkf45` | `"t_final"` | t_final |
| `@rkf45` | `"pasos_aceptados"` | Pasos aceptados |
| `@rkf45` | `"pasos_rechazados"` | Pasos rechazados |
| `@rkf45` | `"formula"` | Expresión matemática |
| `@rkf45` | `"completo"` | Si la integración alcanzó el tiempo final |
| `@euler` | `"trayectoria"` | trayectoria |
| `@euler` | `"y_final"` | y_final |
| `@euler` | `"t_final"` | t_final |
| `@euler` | `"h"` | h |
| `@euler` | `"n_pasos"` | n_pasos |
| `@euler` | `"formula"` | Expresión matemática |
| `@eulerMejorado` | `"trayectoria"` | trayectoria |
| `@eulerMejorado` | `"y_final"` | y_final |
| `@eulerMejorado` | `"t_final"` | t_final |
| `@eulerMejorado` | `"h"` | h |
| `@eulerMejorado` | `"n_pasos"` | n_pasos |
| `@eulerMejorado` | `"formula"` | Expresión matemática |
| `@eulerImplicito` | `"trayectoria"` | trayectoria |
| `@eulerImplicito` | `"y_final"` | y_final |
| `@eulerImplicito` | `"t_final"` | t_final |
| `@eulerImplicito` | `"h"` | h |
| `@eulerImplicito` | `"n_pasos"` | n_pasos |
| `@eulerImplicito` | `"formula"` | Expresión matemática |
| `@adams` | `"trayectoria"` | trayectoria |
| `@adams` | `"y_final"` | y_final |
| `@adams` | `"t_final"` | t_final |
| `@adams` | `"h"` | h |
| `@adams` | `"n_pasos"` | n_pasos |
| `@adams` | `"formula"` | Expresión matemática |
| `@pvf` | `"solucion"` | solucion |
| `@pvf` | `"n_nodos"` | n_nodos |
| `@pvf` | `"h"` | h |
| `@pvf` | `"formula"` | Expresión matemática |
| `@disparo` | `"solucion"` | solucion |
| `@disparo` | `"pendiente_inicial"` | pendiente_inicial |
| `@disparo` | `"iteraciones"` | iteraciones |
| `@disparo` | `"convergio"` | convergio |
| `@disparo` | `"formula"` | Expresión matemática |
| `@disparo` | `"error_frontera"` | Error en la condición de frontera |
| `@sistemaRk4` | `"trayectoria"` | trayectoria |
| `@sistemaRk4` | `"y_final"` | y_final |
| `@sistemaRk4` | `"t_final"` | t_final |
| `@sistemaRk4` | `"h"` | h |
| `@sistemaRk4` | `"n_pasos"` | n_pasos |
| `@sistemaRk4` | `"formula"` | Expresión matemática |
| `@sistemaRk4` | `"dimension"` | Dimensión del sistema |
| `@sistemaRkf45` | `"trayectoria"` | trayectoria |
| `@sistemaRkf45` | `"y_final"` | y_final |
| `@sistemaRkf45` | `"t_final"` | t_final |
| `@sistemaRkf45` | `"formula"` | Expresión matemática |
| `@sistemaRkf45` | `"pasos_aceptados"` | Pasos aceptados por el control adaptativo |
| `@sistemaRkf45` | `"pasos_rechazados"` | Pasos rechazados por el control adaptativo |
| `@sistemaRkf45` | `"dimension"` | Dimensión del sistema |
| `@sistemaRkf45` | `"completo"` | Si la integración alcanzó el tiempo final |

### Electricidad

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@leyOhm` | `"resultado"` | resultado |
| `@leyOhm` | `"unidad"` | unidad |
| `@leyOhm` | `"incognita"` | incognita (V, I, o R) |
| `@leyOhm` | `"formula"` | Expresión matemática |
| `@potenciaElectrica` | `"resultado"` | resultado |
| `@potenciaElectrica` | `"unidad"` | unidad |
| `@potenciaElectrica` | `"formula_usada"` | formula_usada |
| `@potenciaElectrica` | `"formula"` | Expresión matemática |
| `@leyCoulomb` | `"resultado"` | resultado |
| `@leyCoulomb` | `"unidad"` | unidad |
| `@leyCoulomb` | `"formula"` | Expresión matemática |
| `@campoElectrico` | `"resultado"` | resultado |
| `@campoElectrico` | `"unidad"` | unidad |
| `@campoElectrico` | `"formula"` | Expresión matemática |
| `@capacitorEnergia` | `"resultado"` | resultado |
| `@capacitorEnergia` | `"unidad"` | unidad |
| `@capacitorEnergia` | `"carga_almacenada_C"` | carga_almacenada_C |
| `@capacitorEnergia` | `"formula"` | Expresión matemática |
| `@inductorEnergia` | `"resultado"` | resultado |
| `@inductorEnergia` | `"unidad"` | unidad |
| `@inductorEnergia` | `"formula"` | Expresión matemática |
| `@resonanciaLC` | `"resultado"` | resultado |
| `@resonanciaLC` | `"unidad"` | unidad |
| `@resonanciaLC` | `"periodo_s"` | periodo_s |
| `@resonanciaLC` | `"omega_rad_s"` | omega_rad_s |
| `@resonanciaLC` | `"formula"` | Expresión matemática |
| `@leyFaraday` | `"resultado"` | resultado |
| `@leyFaraday` | `"unidad"` | unidad |
| `@leyFaraday` | `"nota"` | nota |
| `@leyFaraday` | `"formula"` | Expresión matemática |
| `@transformador` | `"resultado"` | resultado |
| `@transformador` | `"unidad"` | unidad |
| `@transformador` | `"relacion_transformacion"` | relacion_transformacion |
| `@transformador` | `"tipo"` | tipo |
| `@transformador` | `"formula"` | Expresión matemática |
| `@filtroRC` | `"resultado"` | resultado |
| `@filtroRC` | `"unidad"` | unidad |
| `@filtroRC` | `"constante_tiempo_s"` | constante_tiempo_s |
| `@filtroRC` | `"rango"` | rango |
| `@filtroRC` | `"nota"` | nota |
| `@filtroRC` | `"formula"` | Expresión matemática |

### Escape

| Prefijo | Secuencia | Significado |
| :--- | :--- | :--- |
| `@esc` | `"\\n"` | Salto de línea |
| `@esc` | `"\\r"` | Retorno de carro |
| `@esc` | `"\\t"` | Tabulación |
| `@esc` | `"\\'"` | Comilla simple |
| `@esc` | `"\\\\"` | Barra invertida |
| `@esc` | `"\\b"` | Retroceso (Backspace) |
| `@esc` | `"\\f"` | Avance de página |
| `@esc` | `"\\v"` | Tabulación vertical |
| `@esc` | `"\\a"` | Alerta sonora |
| `@esc` | `"\\u"` | Carácter Unicode (ej: `\\u00A9` para ©) |

### Estadística

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@media` | `"resultado"` | resultado |
| `@media` | `"unidad"` | unidades |
| `@media` | `"formula"` | Expresión matemática |
| `@varianzaDesviacion` | `"media"` | media |
| `@varianzaDesviacion` | `"varianza"` | varianza |
| `@varianzaDesviacion` | `"desviacion_estandar"` | desviacion estandar |
| `@varianzaDesviacion` | `"n"` | número de elementos (tamaño muestral) |
| `@varianzaDesviacion` | `"formula"` | Expresión matemática |
| `@puntajeZ` | `"resultado"` | resultado |
| `@puntajeZ` | `"interpretacion"` | interpretacion |
| `@puntajeZ` | `"unidad"` | σ |
| `@puntajeZ` | `"formula"` | Expresión matemática |
| `@coeficienteVariacion` | `"resultado"` | resultado |
| `@coeficienteVariacion` | `"nivel"` | nivel |
| `@coeficienteVariacion` | `"unidad"` | % |
| `@coeficienteVariacion` | `"formula"` | Expresión matemática |
| `@combinaciones` | `"resultado"` | resultado |
| `@combinaciones` | `"formula"` | Expresión matemática |
| `@combinaciones` | `"unidad"` | Unidad del resultado |
| `@permutaciones` | `"resultado"` | resultado |
| `@permutaciones` | `"unidad"` | permutaciones |
| `@permutaciones` | `"formula"` | Expresión matemática |

### Estructuras

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@tensionNormal` | `"resultado"` | resultado |
| `@tensionNormal` | `"unidad"` | unidad |
| `@tensionNormal` | `"formula"` | Expresión matemática |
| `@moduloYoung` | `"resultado"` | resultado |
| `@moduloYoung` | `"unidad"` | unidad |
| `@moduloYoung` | `"incognita"` | incognita (E, sigma, o epsilon) |
| `@moduloYoung` | `"formula"` | Expresión matemática |
| `@factorSeguridad` | `"resultado"` | resultado |
| `@factorSeguridad` | `"diagnostico"` | diagnostico |
| `@factorSeguridad` | `"formula"` | Expresión matemática |
| `@factorSeguridad` | `"unidad"` | Unidad del resultado |
| `@vigaFlechaMax` | `"resultado"` | resultado |
| `@vigaFlechaMax` | `"limite_L_300_m"` | limite_L_300_m |
| `@vigaFlechaMax` | `"estado"` | estado |
| `@vigaFlechaMax` | `"formula"` | Expresión matemática |
| `@vigaFlechaMax` | `"unidad"` | Unidad del resultado |
| `@momentoInerciaRectangulo` | `"inercia_xx"` | inercia_xx |
| `@momentoInerciaRectangulo` | `"inercia_yy"` | inercia_yy |
| `@momentoInerciaRectangulo` | `"modulo_resistente_xx"` | modulo_resistente_xx |
| `@momentoInerciaRectangulo` | `"formula"` | Expresión matemática |
| `@pandeoEuler` | `"resultado"` | resultado |
| `@pandeoEuler` | `"longitud_efectiva_m"` | longitud_efectiva_m |
| `@pandeoEuler` | `"condicion"` | condicion |
| `@pandeoEuler` | `"advertencia"` | advertencia |
| `@pandeoEuler` | `"formula"` | Expresión matemática |
| `@pandeoEuler` | `"unidad"` | Unidad del resultado |
| `@vonMises` | `"resultado"` | resultado |
| `@vonMises` | `"unidad"` | unidad |
| `@vonMises` | `"uso"` | uso |
| `@vonMises` | `"nota"` | nota |
| `@vonMises` | `"formula"` | Expresión matemática |

### Astronomía

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@velocidadEscape` | `"resultado"` | resultado |
| `@velocidadEscape` | `"unidad"` | unidad |
| `@velocidadEscape` | `"formula"` | Expresión matemática |
| `@fuerzaGravitacional` | `"resultado"` | resultado |
| `@fuerzaGravitacional` | `"unidad"` | unidad |
| `@fuerzaGravitacional` | `"formula"` | Expresión matemática |
| `@distanciaParalaje` | `"resultado"` | resultado |
| `@distanciaParalaje` | `"distancia_anios_luz"` | distancia_anios_luz |
| `@distanciaParalaje` | `"formula"` | Expresión matemática |
| `@distanciaParalaje` | `"unidad"` | Unidad del resultado |
| `@keplerTercera` | `"resultado"` | resultado |
| `@keplerTercera` | `"incognita"` | incognita (T, a, o M) |
| `@keplerTercera` | `"formula"` | Expresión matemática |
| `@keplerTercera` | `"unidad"` | Unidad del resultado |

### Aerodinámica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@numeroMach` | `"resultado"` | resultado |
| `@numeroMach` | `"regimen_vuelo"` | regimen_vuelo |
| `@numeroMach` | `"formula"` | Expresión matemática |
| `@numeroMach` | `"unidad"` | Unidad del resultado |

### Computación

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@entropiaShannon` | `"resultado"` | resultado |
| `@entropiaShannon` | `"entropia_maxima_bits"` | entropia_maxima_bits |
| `@entropiaShannon` | `"eficiencia_porcentaje"` | eficiencia_porcentaje |
| `@entropiaShannon` | `"descripcion"` | descripcion |
| `@entropiaShannon` | `"formula"` | Expresión matemática |
| `@entropiaShannon` | `"unidad"` | Unidad del resultado |
| `@probabilidadColisionHash` | `"resultado"` | resultado |
| `@probabilidadColisionHash` | `"porcentaje"` | porcentaje |
| `@probabilidadColisionHash` | `"riesgo"` | riesgo |
| `@probabilidadColisionHash` | `"nota"` | nota |
| `@probabilidadColisionHash` | `"formula"` | Expresión matemática |
| `@probabilidadColisionHash` | `"unidad"` | Unidad del resultado |
| `@tiempoBusqueda` | `"resultado"` | resultado |
| `@tiempoBusqueda` | `"big_o"` | big_o |
| `@tiempoBusqueda` | `"descripcion"` | descripcion |
| `@tiempoBusqueda` | `"algoritmo"` | algoritmo |
| `@tiempoBusqueda` | `"formula"` | Expresión matemática |
| `@tiempoBusqueda` | `"unidad"` | Unidad del resultado |
| `@productoAnchoBandaLatencia` | `"resultado"` | resultado |
| `@productoAnchoBandaLatencia` | `"bdp_KB"` | bdp_KB |
| `@productoAnchoBandaLatencia` | `"implicacion"` | implicacion |
| `@productoAnchoBandaLatencia` | `"formula"` | Expresión matemática |
| `@productoAnchoBandaLatencia` | `"unidad"` | Unidad del resultado |
| `@bitsNecesarios` | `"resultado"` | resultado |
| `@bitsNecesarios` | `"valores_representables"` | valores_representables |
| `@bitsNecesarios` | `"bits_desperdiciados"` | bits_desperdiciados |
| `@bitsNecesarios` | `"formula"` | Expresión matemática |
| `@bitsNecesarios` | `"unidad"` | Unidad del resultado |

### Economía

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@roi` | `"resultado"` | resultado |
| `@roi` | `"unidad"` | unidad |
| `@roi` | `"formula"` | Expresión matemática |

### Finanzas

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@interesSimple` | `"resultado"` | resultado |
| `@interesSimple` | `"monto_final"` | monto_final |
| `@interesSimple` | `"interes_generado"` | interes_generado |
| `@interesSimple` | `"capital_inicial"` | capital_inicial |
| `@interesSimple` | `"formula"` | Expresión matemática |
| `@interesSimple` | `"unidad"` | Unidad del resultado |
| `@interesCompuesto` | `"resultado"` | resultado |
| `@interesCompuesto` | `"interes_generado"` | interes_generado |
| `@interesCompuesto` | `"capital_inicial"` | capital_inicial |
| `@interesCompuesto` | `"formula"` | Expresión matemática |
| `@interesCompuesto` | `"unidad"` | Unidad del resultado |
| `@valorPresente` | `"resultado"` | resultado |
| `@valorPresente` | `"formula"` | Expresión matemática |
| `@valorPresente` | `"unidad"` | Unidad del resultado |
| `@valorFuturo` | `"resultado"` | resultado |
| `@valorFuturo` | `"formula"` | Expresión matemática |
| `@valorFuturo` | `"unidad"` | Unidad del resultado |
| `@amortizacionCuota` | `"resultado"` | Cuota fija periódica |
| `@amortizacionCuota` | `"numero_cuotas"` | Número de cuotas del préstamo |
| `@amortizacionCuota` | `"total_pagado"` | Suma de todas las cuotas |
| `@amortizacionCuota` | `"total_intereses"` | Suma de los intereses pagados |
| `@amortizacionCuota` | `"unidad"` | moneda/mes |
| `@amortizacionCuota` | `"formula"` | Expresión matemática |
| `@puntoEquilibrio` | `"resultado"` | resultado |
| `@puntoEquilibrio` | `"margen_contribucion"` | margen_contribucion |
| `@puntoEquilibrio` | `"ingreso_equilibrio"` | ingreso_equilibrio |
| `@puntoEquilibrio` | `"formula"` | Expresión matemática |
| `@puntoEquilibrio` | `"unidad"` | Unidad del resultado |

### Física Cuántica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@modeloBohr` | `"radio_orbital_m"` | radio_orbital_m |
| `@modeloBohr` | `"energia_eV"` | energia_eV |
| `@modeloBohr` | `"velocidad_tangencial_ms"` | velocidad_tangencial_ms |
| `@modeloBohr` | `"formula"` | Expresión matemática |
| `@energiaMasa` | `"resultado"` | resultado |
| `@energiaMasa` | `"unidad"` | unidad |
| `@energiaMasa` | `"formula"` | Expresión matemática |

### Fluidos

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@reynolds` | `"resultado"` | resultado |
| `@reynolds` | `"regimen"` | regimen |
| `@reynolds` | `"formula"` | Expresión matemática |
| `@reynolds` | `"unidad"` | Unidad del resultado |

### Geometría

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@areaCirculo` | `"resultado"` | resultado |
| `@areaCirculo` | `"unidad"` | unidad |
| `@areaCirculo` | `"formula"` | Expresión matemática |
| `@cilindroCompleto` | `"area_lateral"` | area_lateral |
| `@cilindroCompleto` | `"area_total"` | area_total |
| `@cilindroCompleto` | `"volumen"` | volumen |
| `@cilindroCompleto` | `"formula"` | Expresión matemática |
| `@esfera` | `"area_superficial"` | area_superficial |
| `@esfera` | `"volumen"` | volumen |
| `@esfera` | `"formula"` | Expresión matemática |
| `@cono` | `"generatriz"` | generatriz |
| `@cono` | `"area_base"` | area_base |
| `@cono` | `"area_lateral"` | area_lateral |
| `@cono` | `"area_total"` | area_total |
| `@cono` | `"volumen"` | volumen |
| `@cono` | `"formula"` | Expresión matemática |
| `@piramideRectangular` | `"area_base"` | area_base |
| `@piramideRectangular` | `"area_lateral"` | area_lateral |
| `@piramideRectangular` | `"area_total"` | area_total |
| `@piramideRectangular` | `"volumen"` | volumen |
| `@piramideRectangular` | `"formula"` | Expresión matemática |
| `@elipse` | `"area"` | area |
| `@elipse` | `"perimetro"` | perimetro |
| `@elipse` | `"excentricidad"` | excentricidad |
| `@elipse` | `"formula"` | Expresión matemática |
| `@poligonoRegular` | `"area"` | area |
| `@poligonoRegular` | `"perimetro"` | perimetro |
| `@poligonoRegular` | `"apotema"` | apotema |
| `@poligonoRegular` | `"angulo_interior"` | angulo_interior |
| `@poligonoRegular` | `"formula"` | Expresión matemática |
| `@distanciaPuntos` | `"resultado"` | resultado |
| `@distanciaPuntos` | `"unidad"` | unidad |
| `@distanciaPuntos` | `"formula"` | Expresión matemática |

### Hidráulica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@bernoulli` | `"resultado"` | resultado |
| `@bernoulli` | `"carga_punto_1_m"` | carga_punto_1_m |
| `@bernoulli` | `"carga_punto_2_m"` | carga_punto_2_m |
| `@bernoulli` | `"diferencia_carga_m"` | diferencia_carga_m |
| `@bernoulli` | `"es_ideal"` | es_ideal |
| `@bernoulli` | `"formula"` | Expresión matemática |
| `@bernoulli` | `"unidad"` | Unidad del resultado |
| `@continuidad` | `"resultado"` | resultado |
| `@continuidad` | `"unidad"` | unidad |
| `@continuidad` | `"formula"` | Expresión matemática |
| `@continuidad` | `"caudal_m3_s"` | Caudal en metros cúbicos por segundo |
| `@continuidad` | `"efecto"` | Efecto sobre el caudal al cambiar el área |
| `@numeroFroude` | `"resultado"` | resultado |
| `@numeroFroude` | `"unidad"` | unidad |
| `@numeroFroude` | `"formula"` | Expresión matemática |
| `@numeroFroude` | `"regimen"` | Régimen de flujo calculado |
| `@caudalManning` | `"resultado"` | resultado |
| `@caudalManning` | `"unidad"` | unidad |
| `@caudalManning` | `"formula"` | Expresión matemática |
| `@caudalManning` | `"velocidad_media_m_s"` | Velocidad media en m/s |
| `@darcyWeisbach` | `"resultado"` | resultado |
| `@darcyWeisbach` | `"unidad"` | unidad |
| `@darcyWeisbach` | `"formula"` | Expresión matemática |
| `@darcyWeisbach` | `"presion_perdida_Pa"` | Pérdida de carga en pascales |
| `@darcyWeisbach` | `"nota"` | Nota explicativa |
| `@hazenWilliams` | `"resultado"` | resultado |
| `@hazenWilliams` | `"unidad"` | unidad |
| `@hazenWilliams` | `"formula"` | Expresión matemática |
| `@hazenWilliams` | `"material_referencia"` | Material de referencia usado |
| `@hazenWilliams` | `"nota"` | Nota explicativa |

### Interpolación

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@lagrange` | `"resultado"` | Valor interpolado |
| `@lagrange` | `"grado_polinomio"` | Grado del polinomio ajustado |
| `@lagrange` | `"es_extrapolacion"` | `cierto` si el punto cae fuera del rango |
| `@lagrange` | `"formula"` | Expresión matemática |
| `@newtonDivididas` | `"resultado"` | Valor interpolado |
| `@newtonDivididas` | `"grado_polinomio"` | Grado del polinomio ajustado |
| `@newtonDivididas` | `"coeficientes_dd"` | Coeficientes de diferencias divididas |
| `@newtonDivididas` | `"es_extrapolacion"` | `cierto` si el punto cae fuera del rango |
| `@newtonDivididas` | `"formula"` | Expresión matemática |
| `@splineCubico` | `"resultado"` | Valor interpolado |
| `@splineCubico` | `"derivada_en_x"` | Primera derivada en el punto evaluado |
| `@splineCubico` | `"tramo_usado"` | Índice del tramo del spline que se aplicó |
| `@splineCubico` | `"es_extrapolacion"` | `cierto` si el punto cae fuera del rango |
| `@splineCubico` | `"formula"` | Expresión matemática |
| `@regresionLineal` | `"pendiente"` | pendiente |
| `@regresionLineal` | `"intercepto"` | intercepto |
| `@regresionLineal` | `"r_cuadrado"` | r_cuadrado |
| `@regresionLineal` | `"error_estandar"` | error_estandar |
| `@regresionLineal` | `"residuos"` | residuos |
| `@regresionLineal` | `"calidad"` | calidad |
| `@regresionLineal` | `"formula"` | Expresión matemática |
| `@regresionPolinomial` | `"coeficientes"` | Coeficientes del polinomio ajustado |
| `@regresionPolinomial` | `"grado"` | Grado del polinomio ajustado |
| `@regresionPolinomial` | `"r_cuadrado"` | r_cuadrado |
| `@regresionPolinomial` | `"formula"` | Expresión matemática |
| `@regresionPolinomial` | `"nota"` | Cómo evaluar el polinomio resultante |

### Lapsos

| Prefijo | Campo | Valor |
| :--- | :--- | :--- |
| `@ahora` | `"anio"` | Año (número) |
| `@ahora` | `"mes"` | Mes 1-12 |
| `@ahora` | `"dia"` | Día del mes |
| `@ahora` | `"hora"` | Hora 0-23 |
| `@ahora` | `"minuto"` | Minuto 0-59 |
| `@ahora` | `"segundo"` | Segundo 0-59 |
| `@ahora` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@analizar` | `"anio"` | Año (número) |
| `@analizar` | `"mes"` | Mes 1-12 |
| `@analizar` | `"dia"` | Día del mes |
| `@analizar` | `"hora"` | Hora 0-23 |
| `@analizar` | `"minuto"` | Minuto 0-59 |
| `@analizar` | `"segundo"` | Segundo 0-59 |
| `@analizar` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@sumar` | `"anio"` | Año (número) |
| `@sumar` | `"mes"` | Mes 1-12 |
| `@sumar` | `"dia"` | Día del mes |
| `@sumar` | `"hora"` | Hora 0-23 |
| `@sumar` | `"minuto"` | Minuto 0-59 |
| `@sumar` | `"segundo"` | Segundo 0-59 |
| `@sumar` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@restar` | `"anio"` | Año (número) |
| `@restar` | `"mes"` | Mes 1-12 |
| `@restar` | `"dia"` | Día del mes |
| `@restar` | `"hora"` | Hora 0-23 |
| `@restar` | `"minuto"` | Minuto 0-59 |
| `@restar` | `"segundo"` | Segundo 0-59 |
| `@restar` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@hoy` | `"anio"` | Año (número) |
| `@hoy` | `"mes"` | Mes 1-12 |
| `@hoy` | `"dia"` | Día del mes |
| `@hoy` | `"hora"` | Hora 0-23 |
| `@hoy` | `"minuto"` | Minuto 0-59 |
| `@hoy` | `"segundo"` | Segundo 0-59 |
| `@hoy` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@desdeTicks` | `"anio"` | Año (número) |
| `@desdeTicks` | `"mes"` | Mes 1-12 |
| `@desdeTicks` | `"dia"` | Día del mes |
| `@desdeTicks` | `"hora"` | Hora 0-23 |
| `@desdeTicks` | `"minuto"` | Minuto 0-59 |
| `@desdeTicks` | `"segundo"` | Segundo 0-59 |
| `@desdeTicks` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@desdeUnix` | `"anio"` | Año (número) |
| `@desdeUnix` | `"mes"` | Mes 1-12 |
| `@desdeUnix` | `"dia"` | Día del mes |
| `@desdeUnix` | `"hora"` | Hora 0-23 |
| `@desdeUnix` | `"minuto"` | Minuto 0-59 |
| `@desdeUnix` | `"segundo"` | Segundo 0-59 |
| `@desdeUnix` | `"texto"` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |
| `@diferencia` | `"dias"` | Días del fragmento |
| `@diferencia` | `"horas"` | Horas del fragmento |
| `@diferencia` | `"minutos"` | Minutos del fragmento |
| `@diferencia` | `"segundos"` | Segundos del fragmento |
| `@diferencia` | `"milisegundos"` | Milisegundos del fragmento |
| `@diferencia` | `"total_dias"` | Total de días (acumulado) |
| `@diferencia` | `"dias_totales"` | Alias de `total_dias` |
| `@diferencia` | `"total_horas"` | Total de horas (acumulado) |
| `@diferencia` | `"horas_totales"` | Alias de `total_horas` |
| `@diferencia` | `"total_minutos"` | Total de minutos (acumulado) |
| `@diferencia` | `"total_segundos"` | Total de segundos (acumulado) |
| `@diferencia` | `"signo"` | `-1` si la 1.ª fecha es posterior, `1` si es anterior |

> **Estas funciones NO devuelven un diccionario con la clave `"resultado"`** (medido en v1.0.81): devuelven escalares o un diccionario `fecha`. No escribas `resultado["resultado"]`.
>
| Prefijo | Tipo de retorno | Qué obtienes |
| :--- | :--- | :--- |
| `@formato` | `cadena` | La fecha como texto según el patrón indicado |
| `@zonaHoraria` | `cadena` | Identificador de la zona horaria local |
| `@esBisiesto` | `logico` | `cierto` si el año es bisiesto |
| `@esValido` | `logico` | `cierto` si la fecha existe en el calendario |
| `@esFuturo` | `logico` | `cierto` si la fecha es posterior a ahora |
| `@esPasado` | `logico` | `cierto` si la fecha es anterior a ahora |
| `@diasEnMes` | `decimal` | Número de días del mes (28/29/30/31) |
| `@comparar` | `decimal` | `-1` anterior, `0` igual, `1` posterior |
| `@ticks` | `decimal` | Fecha en *ticks* desde .NET |
| `@unix` | `decimal` | Fecha como *Unix timestamp* (segundos) |
| `@inicioMes` | `diccionario` `fecha` | El día 1 del mes |
| `@finMes` | `diccionario` `fecha` | El último día del mes |
| `@convertir` | `diccionario` `fecha` | La fecha leída del patrón indicado |
| `@fecha` | `diccionario` `fecha` | Fecha construida desde sus componentes |

> **Estructura del diccionario `fecha`** (las mismas siete claves en todas las funciones que lo devuelven):
>
| Clave | Tipo | Significado |
| :--- | :--- | :--- |
| `anio` | `decimal` | Año |
| `mes` | `decimal` | Mes 1-12 |
| `dia` | `decimal` | Día del mes |
| `hora` | `decimal` | Hora 0-23 |
| `minuto` | `decimal` | Minuto 0-59 |
| `segundo` | `decimal` | Segundo 0-59 |
| `texto` | `cadena` | Fecha y hora como cadena `AAAA-MM-DD HH:MM:SS` |

### Log

| Prefijo | Nivel | Uso |
| :--- | :--- | :--- |
| `@log` | `"depurar"` | Diagnóstico profundo |
| `@log` | `"info"` | Estado general (default) |
| `@log` | `"advertencia"` | Situaciones extrañas |
| `@log` | `"error"` | Fallos críticos |
| `@log` | `"consola"` | Imprimir en consola |

### Matemáticas

> Las funciones de `mate` que devuelven un número van aparte: la columna «Clave de retorno» no aplica a ellas porque no son diccionarios.

| Función | Retorna | Valor / Notas |
| :--- | :--- | :--- |
| `@mcd` | `decimal` | Máximo común divisor |
| `@mcm` | `decimal` | Mínimo común múltiplo |
| `@factorial` | `decimal` | Factorial |
| `@compuesto` | `decimal` | Interés compuesto. Firma: `(valor_inicial, tasa, periodos, [modo])` |

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@parsearNumero` | `"entero"`, `"decimal"`, `"negativo"`, `"texto"` | Parseo tolerante; lanza si el texto no es numérico |
| `@serieGeometrica` | `"resultado"`, `"unidad"`, `"formula"` | Serie geométrica |
| `@serieGeometrica` | `"suma_infinita"` | Suma de la serie infinita, si converge |
| `@progresionAritmetica` | `"formula"`, `"suma"`, `"termino_n"`, `"media"` | Progresión aritmética |
| `@logaritmoBase` | `"resultado"`, `"unidad"`, `"formula"` | Logaritmo en base arbitraria |
| `@pitagorica` | Siempre la clave `c` | Exige 3 argumentos: los dos catetos y el lado a calcular (`"a"`, `"b"`, `"c"`, `"h"`; otro valor es error). El tercer argumento se valida pero **la clave devuelta es siempre `c`**, que contiene la hipotenusa |

### Optimización

| Función | Clave de Retorno |
| :--- | :--- |
| `@seccionAurea` | `"optimo"`, `"f_optimo"`, `"convergio"`, `"iteraciones"`, `"error_estimado"`, `"formula"` |
| `@descensoGradiente` | `"optimo"`, `"f_optimo"`, `"convergio"`, `"iteraciones"`, `"error_estimado"`, `"formula"`, `"tasa_final"` |

### Plataforma

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@plat` | `"so"` | Nombre del SO |
| `@plat` | `"version"` | Versión del SO |
| `@plat` | `"arquitectura"` | Arquitectura CPU |
| `@plat` | `"dotnet"` | Versión .NET |
| `@plat` | `"nombre_maquina"` | Nombre del equipo |
| `@plat` | `"bits"` | Bits del proceso |
| `@plat` | `"es_64bits"` | ¿Es 64 bits? |
| `@plat` | `"procesadores"` | Núcleos lógicos |
| `@plat` | `"memoria_proceso"` | RAM usada (MB) |
| `@plat` | `"usuario"` | Usuario actual |
| `@plat` | `"directorio_actual"` | Carpeta actual |
| `@plat` | `"directorio_home"` | Carpeta Home |
| `@plat` | `"separador_rutas"` | Separador \\ o / |
| `@plat` | `"salto_linea"` | Salto de línea nativo |
| `@plat` | `"tiempo_sistema"` | Uptime (ms) |

### Plugin

| Prefijo | Nombre | Librería |
| :--- | :--- | :--- |
| `@plugin` | `"formulas"` | Fórmulas científicas y matemáticas |
| `@plugin` | `"sqlite"` | Base de datos SQLite |
| `@plugin` | `"hilos"` | Concurrencia y multihilo |
| `@plugin` | `"lapsos"` | Manejo de tiempo y fechas |
| `@plugin` | `"zip"` | Compresión de archivos |
| `@plugin` | `"cripto"` | Criptografía y Hashes |
| `@plugin` | `"env"` | Variables de entorno (.env) |
| `@plugin` | `"log"` | Registro de eventos (Logging) |
| `@plugin` | `"proceso"` | Ejecución de procesos OS |
| `@plugin` | `"texto"` | Constructores de texto mutables |
| `@plugin` | `"tabla"` | Tabla Periódica de Elementos |

### Proceso

> Las cuatro funciones de resultado (`ejecutar`, `shell`, `esperar`, `resultado`) devuelven **exactamente las mismas cuatro claves** (medido en v1.0.81): `salida`, `error`, `codigo`, `timeout`. `ejecutarAsync` es la excepción: ver su fila.
>
| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@ejecutar` | `"salida"` | Salida estándar del proceso |
| `@ejecutar` | `"error"` | Salida de error, si la hubo |
| `@ejecutar` | `"codigo"` | Código de salida |
| `@ejecutar` | `"timeout"` | Indicador de tiempo agotado |
| `@shell` | `"salida"` | Salida del comando de shell |
| `@shell` | `"error"` | Salida de error, si la hubo |
| `@shell` | `"codigo"` | Código de salida |
| `@shell` | `"timeout"` | Indicador de tiempo agotado |
| `@esperar` | `"salida"` | Salida estándar (bloquea hasta terminar) |
| `@esperar` | `"error"` | Salida de error, si la hubo |
| `@esperar` | `"codigo"` | Código de salida |
| `@esperar` | `"timeout"` | Indicador de tiempo agotado |
| `@resultado` | `"salida"` | Salida del proceso ya finalizado |
| `@resultado` | `"codigo"` | Código de salida |
| `@resultado` | `"error"` | Salida de error, si la hubo |
| `@resultado` | `"timeout"` | Indicador de tiempo agotado |
| `@ejecutarAsync` | *(no devuelve diccionario)* | Retorna el **id interno del proceso** como `decimal`. Pasa ese id a `esperar()` o `resultado()`. |
| `@listar` | `"pid"` | pid |
| `@listar` | `"nombre"` | nombre |
| `@listar` | `"memoria_mb"` | memoria_mb |

### Química

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@phEstado` | `"resultado"` | resultado |
| `@phEstado` | `"estado"` | estado |
| `@phEstado` | `"formula"` | Expresión matemática |
| `@phEstado` | `"unidad"` | Unidad del resultado |
| `@dilucion` | `"resultado"` | resultado |
| `@dilucion` | `"incognita"` | incognita |
| `@dilucion` | `"formula"` | Expresión matemática |
| `@dilucion` | `"unidad"` | Unidad del resultado |
| `@concentracionMolar` | `"resultado"` | Concentración molar (mol/L) |
| `@concentracionMolar` | `"unidad"` | mol/L |
| `@concentracionMolar` | `"formula"` | Expresión matemática |
| `@rendimientoReaccion` | `"resultado"` | resultado |
| `@rendimientoReaccion` | `"calidad"` | calidad |
| `@rendimientoReaccion` | `"masa_perdida"` | masa_perdida |
| `@rendimientoReaccion` | `"formula"` | Expresión matemática |
| `@rendimientoReaccion` | `"unidad"` | Unidad del resultado |
| `@arrhenius` | `"resultado"` | resultado |
| `@arrhenius` | `"nota"` | nota |
| `@arrhenius` | `"formula"` | Expresión matemática |
| `@arrhenius` | `"unidad"` | Unidad del resultado |
| `@celdaElectroquimica` | `"resultado"` | resultado |
| `@celdaElectroquimica` | `"tipo_celda"` | tipo_celda |
| `@celdaElectroquimica` | `"espontanea"` | espontanea |
| `@celdaElectroquimica` | `"formula"` | Expresión matemática |
| `@celdaElectroquimica` | `"unidad"` | Unidad del resultado |
| `@hendersonHasselbalch` | `"resultado"` | resultado |
| `@hendersonHasselbalch` | `"ratio_base_acido"` | ratio_base_acido |
| `@hendersonHasselbalch` | `"estado_buffer"` | estado_buffer |
| `@hendersonHasselbalch` | `"formula"` | Expresión matemática |
| `@hendersonHasselbalch` | `"unidad"` | Unidad del resultado |
| `@entalpiaReaccion` | `"resultado"` | resultado |
| `@entalpiaReaccion` | `"tipo_reaccion"` | tipo_reaccion |
| `@entalpiaReaccion` | `"formula"` | Expresión matemática |
| `@entalpiaReaccion` | `"unidad"` | Unidad del resultado |
| `@michaelisMenten` | `"resultado"` | resultado |
| `@michaelisMenten` | `"incognita"` | incognita |
| `@michaelisMenten` | `"saturacion_pct"` | saturacion_pct |
| `@michaelisMenten` | `"regimen"` | regimen |
| `@michaelisMenten` | `"formula"` | Expresión matemática |
| `@michaelisMenten` | `"unidad"` | Unidad del resultado |

### Raíces

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@biseccion` | `"resultado"` | resultado |
| `@biseccion` | `"iteraciones"` | iteraciones |
| `@biseccion` | `"convergio"` | convergio |
| `@biseccion` | `"error_estimado"` | error_estimado |
| `@biseccion` | `"formula"` | Expresión matemática |
| `@secante` | `"resultado"` | resultado |
| `@secante` | `"iteraciones"` | iteraciones |
| `@secante` | `"convergio"` | convergio |
| `@secante` | `"error_estimado"` | error_estimado |
| `@secante` | `"formula"` | Expresión matemática |
| `@newtonRaphson` | `"resultado"` | resultado |
| `@newtonRaphson` | `"iteraciones"` | iteraciones |
| `@newtonRaphson` | `"convergio"` | convergio |
| `@newtonRaphson` | `"error_estimado"` | error_estimado |
| `@newtonRaphson` | `"formula"` | Expresión matemática |
| `@brent` | `"resultado"` | resultado |
| `@brent` | `"iteraciones"` | iteraciones |
| `@brent` | `"convergio"` | convergio |
| `@brent` | `"error_estimado"` | error_estimado |
| `@brent` | `"formula"` | Expresión matemática |
| `@brent` | `"f_en_raiz"` | Valor de la función en la raíz encontrada |

### Tabla

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@datos` | `"numero"` | Número atómico |
| `@datos` | `"simbolo"` | Símbolo químico |
| `@datos` | `"nombre"` | Nombre del elemento |
| `@datos` | `"masa"` | Masa atómica |
| `@datos` | `"categoria"` | Categoría química (p. ej. `"Metal de transición"`) |
| `@datos` | `"configuracion"` | Configuración electrónica |
| `@filtrar` | *(retorna una `lista` de diccionarios, no un diccionario)* | Lista plana de elementos que cumplen el filtro |
| `@estado` | `"elemento"` | Nombre del elemento |
| `@estado` | `"simbolo"` | Símbolo químico |
| `@estado` | `"temperatura"` | Temperatura evaluada (°C) |
| `@estado` | `"estado"` | Estado físico: Sólido / Líquido / Gaseoso / Desconocido |
| `@estado` | `"fusion_c"` | Punto de fusión (°C), o nulo si se desconoce |
| `@estado` | `"ebullicion_c"` | Punto de ebullición (°C), o nulo si se desconoce |
| `@configuracionDetalle` | `"simbolo"` | Símbolo químico |
| `@configuracionDetalle` | `"abreviada"` | Configuración abreviada (p. ej. `[Ar] 3d6 4s2`) |
| `@configuracionDetalle` | `"expandida"` | Configuración extendida |
| `@configuracionDetalle` | `"total_electrones"` | Número total de electrones |
| `@configuracionDetalle` | `"por_capa"` | electrones por capa |
| `@isotopos` | `"elemento"` | Nombre del elemento |
| `@isotopos` | `"simbolo"` | Símbolo químico |
| `@isotopos` | `"numero_atomico"` | Número atómico |
| `@isotopos` | `"isotopos_estables"` | Lista de isótopos estables |
| `@isotopos` | `"n_isotopos"` | Número de isótopos estables |
| `@masaMolar` | `"formula"` | formula |
| `@masaMolar` | `"masa_molar_g_mol"` | masa_molar_g_mol |
| `@balancear` | `"entrada"` | entrada |
| `@balancear` | `"balanceada"` | balanceada |
| `@balancear` | `"coeficientes"` | coeficientes |
| `@balancear` | `"verificado"` | verificado |
| `@compararMasas` | `"elementos"` | elementos |
| `@compararMasas` | `"mas_pesado"` | mas_pesado |
| `@compararMasas` | `"mas_liviano"` | mas_liviano |
| `@compararMasas` | `"masa_total"` | masa_total |
| `@compararMasas` | `"promedio"` | promedio |

### Termodinámica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@gasIdeal` | `"resultado"` | resultado |
| `@gasIdeal` | `"incognita"` | incognita |
| `@gasIdeal` | `"unidad"` | unidad |
| `@gasIdeal` | `"formula"` | Expresión matemática |
| `@estadoAgua` | `"resultado"` | resultado |
| `@estadoAgua` | `"estado_fisico"` | estado_fisico |
| `@estadoAgua` | `"formula"` | Expresión matemática |
| `@estadoAgua` | `"unidad"` | Unidad del resultado |
| `@calorEspecifico` | `"resultado"` | resultado |
| `@calorEspecifico` | `"unidad"` | unidad |
| `@calorEspecifico` | `"formula"` | Expresión matemática |
| `@calorEspecifico` | `"proceso"` | Descripción del proceso térmico (Absorcion/Cesion de calor) |
| `@entropiaCambio` | `"resultado"` | resultado |
| `@entropiaCambio` | `"unidad"` | unidad |
| `@entropiaCambio` | `"formula"` | Expresión matemática |
| `@entropiaCambio` | `"proceso"` | Proceso fisico al que aplica |
| `@cicloCarnot` | `"resultado"` | resultado |
| `@cicloCarnot` | `"unidad"` | unidad |
| `@cicloCarnot` | `"formula"` | Expresión matemática |
| `@cicloCarnot` | `"eficiencia_porcentaje"` | Eficiencia térmica en porcentaje |
| `@cicloCarnot` | `"nota"` | Nota explicativa |
| `@conduccionCalor` | `"resultado"` | resultado |
| `@conduccionCalor` | `"unidad"` | unidad |
| `@conduccionCalor` | `"formula"` | Expresión matemática |
| `@stefanBoltzmann` | `"resultado"` | resultado |
| `@stefanBoltzmann` | `"unidad"` | unidad |
| `@stefanBoltzmann` | `"formula"` | Expresión matemática |
| `@dilatacionLineal` | `"resultado"` | resultado |
| `@dilatacionLineal` | `"unidad"` | unidad |
| `@dilatacionLineal` | `"formula"` | Expresión matemática |
| `@dilatacionLineal` | `"longitud_final"` | Longitud tras la dilatacion |

### Ondas

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@velocidadOnda` | `"resultado"` | resultado |
| `@velocidadOnda` | `"unidad"` | unidad |
| `@velocidadOnda` | `"formula"` | Expresión matemática |
| `@frecuenciaPeriodo` | `"resultado"` | resultado |
| `@frecuenciaPeriodo` | `"incognita"` | incognita (f o T) |
| `@frecuenciaPeriodo` | `"formula"` | Expresión matemática |
| `@frecuenciaPeriodo` | `"unidad"` | Unidad del resultado |
| `@efectoDoppler` | `"resultado"` | resultado |
| `@efectoDoppler` | `"frecuencia_fuente"` | frecuencia_fuente |
| `@efectoDoppler` | `"delta_f"` | delta_f |
| `@efectoDoppler` | `"formula"` | Expresión matemática |
| `@efectoDoppler` | `"unidad"` | Unidad del resultado |
| `@longitudOndaLuz` | `"resultado"` | resultado |
| `@longitudOndaLuz` | `"espectro"` | espectro |
| `@longitudOndaLuz` | `"formula"` | Expresión matemática |
| `@longitudOndaLuz` | `"unidad"` | Unidad del resultado |

### Óptica

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@leySnell` | `"resultado"` | resultado |
| `@leySnell` | `"fenomeno"` | fenomeno (Reflexión Total Interna) |
| `@leySnell` | `"formula"` | Expresión matemática |
| `@leySnell` | `"unidad"` | Unidad del resultado |
| `@lenteDelgada` | `"resultado"` | resultado |
| `@lenteDelgada` | `"incognita"` | incognita (f, do, o di) |
| `@lenteDelgada` | `"aumento"` | aumento |
| `@lenteDelgada` | `"formula"` | Expresión matemática |
| `@espejoEsferico` | `"resultado"` | resultado |
| `@espejoEsferico` | `"unidad"` | unidad |
| `@espejoEsferico` | `"tipo_espejo"` | tipo_espejo |
| `@espejoEsferico` | `"tipo_imagen"` | tipo_imagen |
| `@espejoEsferico` | `"formula"` | Expresión matemática |
| `@espejoEsferico` | `"focal"` | Distancia focal |
| `@espejoEsferico` | `"distancia_imagen"` | Distancia de la imagen |
| `@espejoEsferico` | `"aumento"` | Aumento lateral |

### Señales

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@decibelesPotencia` | `"resultado"` | resultado |
| `@decibelesPotencia` | `"tipo"` | tipo (Ganancia/Atenuación) |
| `@decibelesPotencia` | `"formula"` | Expresión matemática |
| `@decibelesPotencia` | `"unidad"` | Unidad del resultado |
| `@decibelesVoltaje` | `"resultado"` | resultado |
| `@decibelesVoltaje` | `"unidad"` | unidad |
| `@decibelesVoltaje` | `"formula"` | Expresión matemática |
| `@frecuenciaNyquist` | `"resultado"` | resultado |
| `@frecuenciaNyquist` | `"frecuencia_nyquist_Hz"` | frecuencia_nyquist_Hz |
| `@frecuenciaNyquist` | `"nota"` | nota |
| `@frecuenciaNyquist` | `"formula"` | Expresión matemática |
| `@frecuenciaNyquist` | `"unidad"` | Unidad del resultado |
| `@snr` | `"resultado"` | resultado |
| `@snr` | `"calidad_senal"` | calidad_senal |
| `@snr` | `"formula"` | Expresión matemática |
| `@snr` | `"unidad"` | Unidad del resultado |
| `@capacidadCanalShannon` | `"resultado"` | resultado |
| `@capacidadCanalShannon` | `"capacidad_Mbps"` | capacidad_Mbps |
| `@capacidadCanalShannon` | `"nota"` | nota |
| `@capacidadCanalShannon` | `"formula"` | Expresión matemática |
| `@capacidadCanalShannon` | `"unidad"` | Unidad del resultado |
| `@anchoBandaModulacionAM` | `"resultado"` | Ancho de banda AM (Hz) |
| `@anchoBandaModulacionAM` | `"frecuencia_superior_Hz"` | Frecuencia superior de la banda |
| `@anchoBandaModulacionAM` | `"frecuencia_inferior_Hz"` | Frecuencia inferior de la banda |
| `@anchoBandaModulacionAM` | `"unidad"` | Hz |
| `@anchoBandaModulacionAM` | `"formula"` | Expresión matemática |

### Tipo

| Prefijo | Nombre devuelto | Tipo del valor |
| :--- | :--- | :--- |
| `@tipo` | `"decimal"` | Numérico |
| `@tipo` | `"cadena"` | Texto |
| `@tipo` | `"logico"` | Lógico |
| `@tipo` | `"nulo"` | Valor nulo |
| `@tipo` | `"lista"` | Lista |
| `@tipo` | `"diccionario"` | Diccionario |
| `@tipo` | `"tupla"` | Tupla |
| `@tipo` | `"funcion"` | Función |
| `@tipo` | `"desconocido"` | Definición de clase |
| `@tipo` | `NombreClase` | Instancia de clase (ej: "Perro") |

### Validar

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@analizarCedula` | `"valida"` | valida |
| `@analizarCedula` | `"tipo"` | tipo |
| `@analizarCedula` | `"normalizada"` | normalizada |
| `@analizarCedula` | `"provincia"` | provincia |
| `@analizarCedula` | `"tomo"` | tomo |
| `@analizarCedula` | `"asiento"` | asiento |

### Web

| Función | Clave de Retorno | Descripción |
| :--- | :--- | :--- |
| `@obtener` | `"estado"` | estado |
| `@obtener` | `"ok"` | ok |
| `@obtener` | `"texto"` | texto |
| `@obtener` | `"json"` | json |
| `@obtener` | `"url"` | url |
| `@obtener` | `"encabezados"` | encabezados |
| `@enviar` | `"estado"` | estado |
| `@enviar` | `"ok"` | ok |
| `@enviar` | `"texto"` | texto |
| `@enviar` | `"json"` | json |
| `@enviar` | `"url"` | url |
| `@enviar` | `"encabezados"` | encabezados |
| `@solicitar` | `"estado"` | estado |
| `@solicitar` | `"ok"` | ok |
| `@solicitar` | `"texto"` | texto |
| `@solicitar` | `"json"` | json |
| `@solicitar` | `"url"` | url |
| `@solicitar` | `"encabezados"` | encabezados |
| `@descargar` | `"bytes"` | bytes |
| `@descargar` | `"ruta"` | ruta |
| `@descargar` | `"nombre"` | nombre |

## 🗄️ Plantillas y Snippets SQL para el Editor (`@sqlConsultar` / `@sqlEjecutar`)

*Estas cadenas no son retornos del motor, sino plantillas automáticas provistas por el Language Server (LSP) en VS Code para acelerar la escritura de consultas en el plugin `sqlite`.*

### Consultas (`@sqlConsultar`)

| Plantilla | Descripción |
| :--- | :--- |
| `"SELECT * FROM tabla"` | Leer todos los registros |
| `"SELECT * FROM tabla WHERE id = @id"` | Leer registro por id |
| `"SELECT * FROM tabla WHERE columna LIKE @patron"` | Buscar por texto parcial |
| `"SELECT * FROM tabla ORDER BY columna DESC LIMIT 10"` | Últimos 10 registros ordenados |
| `"SELECT COUNT(*) AS total FROM tabla"` | Contar total de registros |
| `"SELECT t1.*, t2.nombre FROM tabla1 t1 INNER JOIN tabla2 t2 ON t1.fk_id = t2.id"` | Consulta con INNER JOIN (relación obligatoria) |
| `"SELECT t1.*, t2.nombre FROM tabla1 t1 LEFT JOIN tabla2 t2 ON t1.fk_id = t2.id"` | Consulta con LEFT JOIN (relación opcional) |
| `"SELECT categoria, COUNT(*) AS cantidad FROM tabla GROUP BY categoria"` | Agrupar y contar por categoría |

### Ejecuciones (`@sqlEjecutar`)

| Plantilla | Descripción |
| :--- | :--- |
| `"CREATE TABLE IF NOT EXISTS tabla (id INTEGER PRIMARY KEY AUTOINCREMENT, nombre TEXT NOT NULL)"` | Crear tabla con clave primaria |
| `"INSERT INTO tabla (columna1, columna2) VALUES (@val1, @val2)"` | Insertar registro con parámetros |
| `"UPDATE tabla SET columna = @valor WHERE id = @id"` | Actualizar registro por id |
| `"DELETE FROM tabla WHERE id = @id"` | Borrar registro por id |
| `"DROP TABLE IF EXISTS tabla"` | Eliminar tabla si existe |
| `"ALTER TABLE tabla ADD COLUMN nueva_col TEXT"` | Agregar columna a tabla existente |
| `"CREATE INDEX IF NOT EXISTS idx_columna ON tabla(columna)"` | Crear índice para optimizar búsquedas |

## 🔍 Autocompletado Inteligente en VS Code (LSP)

La extensión oficial de Latino.NET para VS Code facilita enormemente el trabajo con **Magic Strings** (cadenas predefinidas). Cuando necesitas acceder a claves específicas de retorno de funciones, simplemente:

1. **Escribe el símbolo arroba `@` dentro de comillas** (simples o dobles)
2. **VS Code desplegará automáticamente** todas las categorías mágicas disponibles
3. **Sigue escribiendo el prefijo** (ej: `@geo`) para filtrar las propiedades exactas de ese capítulo
4. **Selecciona la clave deseada** y VS Code la insertará automáticamente, evitando errores de tipeo

### 🎯 Prefijos Disponibles

**Categorías de unidades y estilos:**
**`@estilo:`**, **`@temp:`**, **`@long:`**, **`@masa:`**, **`@area:`**, **`@vol:`**, **`@pres:`**, **`@vel:`**, **`@en:`**, **`@dig:`**, **`@ang:`**, **`@hash:`**, **`@regex:`**, **`@comp:`**, **`@fmt:`**, **`@esc:`**

**Funciones científicas y matemáticas:**
**`@adams:`**, **`@ahora:`**, **`@amortizacionCuota:`**, **`@analizar:`**, **`@analizarCedula:`**, **`@anchoBandaModulacionAM:`**, **`@areaCirculo:`**, **`@arrhenius:`**, **`@balancear:`**, **`@bernoulli:`**, **`@biseccion:`**, **`@bitsNecesarios:`**, **`@brent:`**, **`@calorEspecifico:`**, **`@campoElectrico:`**, **`@capacidadCanalShannon:`**, **`@capacitorEnergia:`**, **`@caudalManning:`**, **`@celdaElectroquimica:`**, **`@cholesky:`**, **`@cicloCarnot:`**, **`@cilindroCompleto:`**, **`@coeficienteVariacion:`**, **`@combinaciones:`**, **`@comparar:`**, **`@compararMasas:`**, **`@compuesto:`**, **`@conduccionCalor:`**, **`@config:`**, **`@configuracionDetalle:`**, **`@cono:`**, **`@contiene:`**, **`@continuidad:`**, **`@convertir:`**, **`@cuadraturaGauss:`**, **`@darcyWeisbach:`**, **`@datos:`**, **`@decibelesPotencia:`**, **`@decibelesVoltaje:`**, **`@derivada:`**, **`@derivadaRichardson:`**, **`@descargar:`**, **`@descensoGradiente:`**, **`@desdeTicks:`**, **`@desdeUnix:`**, **`@determinante:`**, **`@diasEnMes:`**, **`@diferencia:`**, **`@dilatacionLineal:`**, **`@dilucion:`**, **`@disparo:`**, **`@distanciaParalaje:`**, **`@distanciaPuntos:`**, **`@efectoDoppler:`**, **`@ejecutar:`**, **`@ejecutarAsync:`**, **`@elipse:`**, **`@energiaMasa:`**, **`@energiaMecanica:`**, **`@entalpiaReaccion:`**, **`@entropiaCambio:`**, **`@entropiaShannon:`**, **`@enviar:`**, **`@esBisiesto:`**, **`@esFuturo:`**, **`@esPasado:`**, **`@esValido:`**, **`@esc:`**, **`@esfera:`**, **`@espejoEsferico:`**, **`@esperar:`**, **`@estado:`**, **`@estadoAgua:`**, **`@euler:`**, **`@eulerImplicito:`**, **`@eulerMejorado:`**, **`@factorSeguridad:`**, **`@factorial:`**, **`@factorizacionLU:`**, **`@fecha:`**, **`@filtroRC:`**, **`@finMes:`**, **`@formato:`**, **`@frecuenciaNyquist:`**, **`@frecuenciaPeriodo:`**, **`@fuerzaGravitacional:`**, **`@fusionar:`**, **`@gasIdeal:`**, **`@gradiente:`**, **`@hazenWilliams:`**, **`@hendersonHasselbalch:`**, **`@hoy:`**, **`@inductorEnergia:`**, **`@inicioMes:`**, **`@interesCompuesto:`**, **`@interesSimple:`**, **`@invertir:`**, **`@isotopos:`**, **`@jacobiana:`**, **`@keplerTercera:`**, **`@lagrange:`**, **`@lapsos:`**, **`@lenteDelgada:`**, **`@leyCoulomb:`**, **`@leyFaraday:`**, **`@leyOhm:`**, **`@leySnell:`**, **`@listar:`**, **`@llaves:`**, **`@log:`**, **`@longitudArco:`**, **`@longitudOndaLuz:`**, **`@mapear:`**, **`@masaMolar:`**, **`@mcd:`**, **`@mcm:`**, **`@media:`**, **`@michaelisMenten:`**, **`@modeloBohr:`**, **`@moduloYoung:`**, **`@momentoInerciaRectangulo:`**, **`@monteCarlo:`**, **`@mru:`**, **`@mruaPosicion:`**, **`@mruaVelocidad:`**, **`@multiplicar:`**, **`@newtonDivididas:`**, **`@newtonFuerza:`**, **`@newtonRaphson:`**, **`@norma:`**, **`@numeroFroude:`**, **`@numeroMach:`**, **`@obtener:`**, **`@pandeoEuler:`**, **`@parsearNumero:`**, **`@penduloSimple:`**, **`@permutaciones:`**, **`@phEstado:`**, **`@piramideRectangular:`**, **`@pitagorica:`**, **`@plat:`**, **`@plugin:`**, **`@poligonoRegular:`**, **`@potenciaElectrica:`**, **`@probabilidadColisionHash:`**, **`@productoAnchoBandaLatencia:`**, **`@progresionAritmetica:`**, **`@puntajeZ:`**, **`@puntoEquilibrio:`**, **`@pvf:`**, **`@regresionLineal:`**, **`@regresionPolinomial:`**, **`@rendimientoReaccion:`**, **`@resolver:`**, **`@resonanciaLC:`**, **`@restar:`**, **`@resultado:`**, **`@reynolds:`**, **`@rk4:`**, **`@rkf45:`**, **`@roi:`**, **`@romberg:`**, **`@secante:`**, **`@seccionAurea:`**, **`@segundaDerivada:`**, **`@segundaLeyRotacional:`**, **`@serieGeometrica:`**, **`@shell:`**, **`@simpson13:`**, **`@sis:`**, **`@sistemaRk4:`**, **`@sistemaRkf45:`**, **`@snr:`**, **`@solicitar:`**, **`@splineCubico:`**, **`@sqlConsultar:`**, **`@sqlEjecutar:`**, **`@stefanBoltzmann:`**, **`@sumar:`**, **`@tabla:`**, **`@tensionNormal:`**, **`@ticks:`**, **`@tiempoBusqueda:`**, **`@tipo:`**, **`@tiroParabolico:`**, **`@transformador:`**, **`@transformarLlaves:`**, **`@transformarValores:`**, **`@transponer:`**, **`@trapecio:`**, **`@trapecio:`**, **`@unix:`**, **`@valorFuturo:`**, **`@valorPresente:`**, **`@valorPropioDominante:`**, **`@valores:`**, **`@valoresPropios:`**, **`@varianzaDesviacion:`**, **`@velocidadEscape:`**, **`@velocidadOnda:`**, **`@vigaFlechaMax:`**, **`@vonMises:`**, **`@zonaHoraria:`**

> Nota: Además, cada función científica compleja tiene su propio prefijo (ej: **`@romberg:`**, **`@euler:`**, **`@tiroParabolico:`** *).*
>
> Ejemplo: Escribir `"@areaCirculo:"` mostrará todas las claves de geometría como `"resultado"`, `"unidad"`, `"formula"`, etc.
