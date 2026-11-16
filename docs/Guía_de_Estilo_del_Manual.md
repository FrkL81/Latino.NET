# Guía de Estilo Oficial del Manual de Latino.NET

> **Versión:** 2.0 (Latino.NET v1.0.81 / .NET 10)  
> **Alcance:** Todo el contenido técnico y de referencia en `docs/`, `ejemplos/` y repositorios oficiales.  
> **Propósito:** Definir los estándares editoriales, tipográficos, estructurales y sintácticos para garantizar que el manual sea homogéneo, técnicamente riguroso, visualmente impecable y 100% compatible con analizadores sintácticos de Markdown (*linters* y parsers CommonMark).

---

## 1. Filosofía Editorial y Reglas Fundamentales

El Manual del Programador de Latino.NET no es un borrador informal; es la especificación oficial de un lenguaje de programación moderno respaldado por la arquitectura industrial de .NET 10. Todo documento debe cumplir cuatro principios:

1. **Autosuficiencia Absoluta:** Cualquier ejemplo copiado y pegado en un archivo `.lat` debe ejecutarse con éxito sin depender de variables fantasma, archivos no creados o funciones mágicas no declaradas.
2. **Determinismo y Verificabilidad:** La salida declarada en el manual debe ser la consecuencia lógica inevitable del código visible. Si un resultado depende del tiempo, la red o números aleatorios, debe advertirse explícitamente.
3. **Cero Ruido para el Analizador (*Linter-Friendly*):** La estructura del texto debe satisfacer las normas estrictas de CommonMark y analizadores como `markdownlint` sin trucos ni viñetas forzadas.
4. **Naturalidad en Español:** Promover las virtudes del lenguaje (concordancia gramatical, nombres descriptivos, acentuación lógica) sin sacrificar la precisión de la ingeniería de software.

### 1.1 Taxonomía Oficial de Desambiguación Terminológica

Para mantener la precisión técnica del manual, se establece la siguiente taxonomía oficial de términos relacionados con la ejecución y el análisis de código:

1. **`intérprete` / `motor` / `runtime`:** Para referirse al ejecutable `latino` / `latino.exe` y al sistema de ejecución en su conjunto.
2. **`parser` / `analizador sintáctico` / `lexer`:** Para referirse a la fase donde se leen las palabras clave y se detectan errores de escritura antes de ejecutar.
3. **`evaluar` / `parsear en tiempo real`:** Para la síntesis y ejecución dinámica en caliente (Capítulo 10).
4. **`compilador` / `compilado` (Intacto):** Exclusivo para .NET 10, C#, JIT, AOT, Roslyn y plugins `.dll`.

Para evitar que la palabra «módulo» se use indistintamente para capítulos, archivos `.lat`, librerías del núcleo y plugins compilados, todo documento del manual debe respetar la siguiente taxonomía:

| Concepto Real | Término Oficial a Usar | Ejemplos |
| :--- | :--- | :--- |
| **Capítulos del Manual** | **`Capítulo`** / **`Capítulos`** | *«En el Capítulo 4 aprenderás...»*, *«# CAPÍTULO 2: Control de Flujo»*. |
| **Modularidad `.lat`** | **`Módulo`** / **`Módulos`** *(Uso exclusivo)* | *«Crea un módulo en `utilidades.lat`»*, *«Patrón Factory para módulos»*, *«Organización de proyectos y modularidad»*. |
| **Librería del Núcleo (Core)** | **`Librería estándar`** / **`Librería nativa`** / **`Librería`** | *«La librería estándar `mate`»*, *«Las funciones de la librería `cadena`»*, *«La librería `sis`»*. |
| **Plugins en C# (`.dll`)** | **`Plugin`** / **`Plugins`** *(o `librería de plugin`)* | *«El plugin oficial `sqlite`»*, *«El plugin `lapsos`»*, *«Las funciones científicas del plugin `formulas`»*. |
| **Término Matemático / Físico** | **`Módulo`** *(INALTERADO)* | *«Operador módulo (`%`)»*, *«Módulo de Young»*, *«Módulo del vector»*. |
| **Código y Variables** | **Código fuente** *(INALTERADO)* | Variables como `moduloMate`, parámetros o nombres de archivo. |

**Regla de concordancia:** al sustituir por el femenino «librería», ajustar artículos y adjetivos adyacentes: *el módulo → la librería*, *del módulo → de la librería*, *al módulo → a la librería*, *este módulo → esta librería*, *un módulo → una librería*, *los módulos → las librerías*, *nuevo módulo → nueva librería*.

---

## 2. Catálogo Canónico de Emojis

Para evitar la polución visual y el uso anárquico de símbolos, se establece un catálogo unificado con funciones exclusivas. **El principio rector es la coherencia, no la uniformidad:** cada categoría de encabezado conserva un emoji estable a lo largo de todo el manual, y lo que se prohíbe es la *inconsistencia*, no la variedad temática (ver §2.4).

### 2.1 Emojis de Estado (Evaluación y Calidad)

Se utilizan **únicamente** dentro de listas, notas explicativas, comentarios de código y comparaciones de buenas/malas prácticas.  
⛔ **Prohibido utilizarlos en encabezados Markdown (`#`, `##`, `###`, etc.).**

| Emoji | Significado | Uso Exclusivo |
| :---: | :--- | :--- |
| ❌ | **Incorrecto / Error** | Código con errores de sintaxis, antipatrones, errores de sintaxis o prácticas desaconsejadas. |
| ✅ | **Correcto / Idiomático** | Solución recomendada, código funcional, buenas prácticas y validaciones aprobadas. |
| ⚠️ | **Advertencia / Limitación** | Casos límite (*edge cases*), advertencias de rendimiento, efectos secundarios o restricciones de versión. |
| 💡 | **Consejo / Nota Técnica** | Tips de productividad, atajos de teclado, detalles de implementación interna o contexto opcional. |
| 🔒 | **Seguridad / Restricción** | Palabras estrictamente reservadas, directivas de inmutabilidad y blindaje criptográfico. |

> 🚫 **Regla Estricta:** Prohibido sustituir `✅` por el glifo de texto plano `✓` o `√`. Debe mantenerse uniformidad en todo el manual.

---

### 2.2 Emojis de Categoría y Navegación

Se utilizan en **encabezados de sección (`##`, `###`)**, títulos de tablas de contenido (TOC) y metadatos estructurales.  
⛔ **Prohibido utilizarlos como indicadores de estado dentro de comentarios de código.**

| Emoji | Categoría | Uso Principal |
| :---: | :--- | :--- |
| 📦 | **Módulos y Plugins** | Estructuras de datos, paquetes externos, arquitectura modular. |
| 🎯 | **Ejemplos y Objetivos** | Bloques de ejemplo funcionales, casos de uso, metas del capítulo. |
| 📌 | **Sintaxis y Firmas** | Definiciones formales de comandos, esquemas abstractos de sintaxis. |
| 📝 | **Documentación** | Plantillas, notas de autor, snippets de código, cadenas de texto. |
| 🔧 | **Herramientas y CLI** | Configuración del runtime, comandos de terminal, parámetros del motor. |
| 📊 | **Datos y Salida** | Tablas de datos, estadísticas, análisis de memoria, benchmarking. |
| 🚀 | **Inicio y Ejecución** | Primeros pasos, tutoriales introductorios, cinemática. |
| 💻 | **Programación** | Entornos de desarrollo, Language Server (LSP), VS Code. |
| 📚 | **Referencia** | Índices completos, manuales de API, catálogos extensos. |
| 🔍 | **Validación** | Expresiones regulares, inspección de tipos, auditoría de código. |
| ⚡ | **Rendimiento** | Optimización, benchmarks de velocidad, paralelismo. |
| 🧵 | **Concurrencia** | Hilos, subprocesos, tareas asíncronas. |
| 💾 | **Persistencia** | Bases de datos relacionales, SQLite, almacenamiento transaccional. |
| 🗜️ | **Compresión** | Manejo de archivos ZIP, fragmentación y empaquetado. |
| 🔐 | **Criptografía** | Cifrado AES-GCM, firmas HMAC, hashes SHA-256. |
| 🌐 | **Web y Redes** | Clientes HTTP, servicios REST, APIs remotas. |

> ℹ️ **Alcance de esta tabla:** enumera los emojis de categoría *transversal* (los que identifican el propósito de una sección de nivel superior). **No es un catálogo cerrado**: los emojis temáticos asignados a módulos, disciplinas o familias de conceptos se rigen por §2.3 y §2.4. La regla que gobierna esta tabla no es «usar solo estos emojis», sino **«cada encabezado conserva el mismo emoji a lo largo del manual»**.

---

### 2.3 Emojis Estandarizados por Librería y Plugin Oficial

Todo módulo o plugin debe identificarse siempre con el mismo emoji asignado a lo largo de todos los capítulos:

```text
Globales: 🌐          cadena: 🔤            lista: 📜             dic: 📖
tupla: 💎             conjunto: 🎯          pila: 📥              cola: 🎫
mate: 🧮              sis: 🖥️               consola: 💻           validar: 🔍
archivo: 📂           web: 🌐               lapsos: ⏱️            hilos: 🧵
sqlite: 💾            formulas: 🔬          texto: 📝             zip: 🗜️
cripto: 🔐            env: 🌍               log: 📋               proceso: ⚙️
tabla: 📊
```

*(Excepción documentada: Los submódulos científicos dentro del plugin `formulas` utilizan los emojis temáticos de sus respectivas disciplinas: 🚀 Cinemática, ⚡ Electricidad, 🧪 Química, 🔥 Termodinámica, 👁️ Óptica, 〰️ Ondas, etc.).*

---

### 2.4 Emojis Temáticos y Criterio de Coherencia

Fuera de los dos catálogos anteriores (§2.2 y §2.3), los encabezados pueden llevar un **emoji temático**:
el que evoque la materia tratada (🔢 números, 🔗 concatenación, 🌊 flujo, 🧠 lógica, 🔑 acceso,
🗝️ estructuras de retorno, 🎨 smart casting, 📐 geometría, 🏭 industrial, ⏱️ tiempo).

Esta práctica está **autorizada y extendida** en el manual. Se rige por tres reglas, y solo por estas:

1. **Estabilidad:** un encabezado dado conserva siempre el mismo emoji, en todos los capítulos donde aparece.
2. **Posición:** el emoji antecede al título, como en el resto del manual (`### 🔗 Concatenación`). No sustituye al texto: el encabezado debe seguir siendo legible sin él.
3. **No usurpación:** un emoji temático no puede ser un emoji de estado (§2.1) ni uno ya asignado a
   otro módulo en §2.3. Por eso `🌍` identifica al módulo `env` y solo se usa como encabezado en
   `9.22_plugin_env.md`; su uso temático en otro capítulo requiere un emoji distinto.

⛔ **Lo que sí se prohíbe**, y es lo único que la regla busca evitar: que un mismo encabezado cambie
de emoji entre capítulos, o que dos secciones equivalentes usen emojis distintos.

> 📌 **Consecuencia práctica:** ninguna de estas reglas obliga a reescribir el manual actual.
> Los emojis temáticos ya presentes en el manual son coherentes y estables; una auditoría que reporte su presencia como defecto está aplicando §2.2 como catálogo cerrado, lo cual esta sección deroga explícitamente.

---

## 3. Estándar Universal de Bloques de Código y Ejemplos

Para garantizar que los analizadores de Markdown (como `markdownlint`) no generen alertas y que la lectura sea fluida, se divide el código en tres categorías bien delimitadas.

### 3.1 Los Tres Tipos de Bloques de Código

#### A. Ejemplos Funcionales (Autosuficientes)

Demuestran una característica con código ejecutable completo.

* **Formato:** Deben ir precedidos por la etiqueta canónica en párrafo independiente:  
  `🎯 **Ejemplo: [Descripción clara y concisa en mayúscula inicial]**`
* Si en una misma sección hay una secuencia progresiva, se numeran:  
  `🎯 **Ejemplo 1: [Descripción]**`, `🎯 **Ejemplo 2: [Descripción]**`.

#### B. Bloques de Sintaxis Abstracta (Signaturas)

Muestran la estructura formal de una instrucción o comando sin valores reales (ej: `si condicion ... fin`).

* **Formato:** No llevan la etiqueta de ejemplo; se preceden por:  
  `📌 **Sintaxis:**`

#### C. Antipatrones o Código Erróneo (Contraste)

Demuestran malas prácticas, trampas comunes o código que deliberadamente produce un error para fines pedagógicos.

* **Formato:** Se etiquetan como:  
  `❌ **Antipatrón: [Qué se hace mal]**` o `⚠️ **Advertencia: [Caso límite o peligroso]**`.

#### D. Bloques bajo Encabezado de Navegación (Excepción Estructural)

Algunos bloques de código están situados bajo un encabezado Markdown que **no los titula**, sino que los organiza para la navegación y la lectura jerárquica del capítulo. En esos casos el encabezado es una pieza estructural legítima y **no debe convertirse en etiqueta de ejemplo**.

**Esta es la única excepción permitida al uso de encabezados como precedente de un bloque de código.** Aplica cuando se cumple **alguna** de estas condiciones:

* El encabezado describe una **subtarea o categoría de la sección** que agrupa varios elementos:
  `#### 1️⃣ Contadores en Bucles`, `#### Uso 1: Funciones de Librería`, `### 2. Comparación con Booleanos`.
* El encabezado es la **firma de una entrada de referencia de API** (no una demostración):
  `#### trocar(coleccion, a, b)`, `#### sisterror(codigo, mensaje)`.
* El encabezado introduce un **caso particular de la sección** y el bloque lo ilustra:
  `#### Fragmentación de Archivos`, `#### Productores y Consumidoras`.

**No es una excepción válida cuando:**

* El encabezado solo nombra el bloque de código inmediato, sin agrupar nada más. Eso sigue siendo un título de ejemplo y debe usar `🎯`.
* El encabezado lleva emojis de estado (`❌`, `✅`, `⚠️`, `💡`, `🔒`) o de módulo: esos títulos están prohibidos por el §2.

> 💡 **Criterio de discernimiento:** pregúntate *«¿este encabezado organiza el capítulo, o solo rotula este bloque?»*. Si lo organiza, es navegación y se conserva. Si solo lo rotula, es título de ejemplo y se convierte a `🎯 **Ejemplo: [...]**`.

---

### 3.2 La Regla del Espaciado Sagrado (*Linter-Friendly*)

Los analizadores sintácticos de Markdown suelen protestar cuando los bloques de código están pegados al texto anterior o anidados dentro de viñetas (`- **Ejemplo:**`) sin una indentación formal de 4 espacios.

Para resolver esto de forma limpia y estándar:

1. **Los ejemplos NO deben ser viñetas (`-`).** Son párrafos normales en negrita.
2. **Debe existir una línea en blanco obligatoria** entre el texto contextual, la etiqueta de ejemplo y el bloque delimitado por comillas invertidas.

#### 📐 Estructura Exacta en Markdown

```markdown
Párrafo contextual o explicativo que introduce la necesidad técnica.

🎯 **Ejemplo: Cálculo de la hipotenusa**

```latino
catetoA = 3
catetoB = 4
hipotenusa = mate.raiz(catetoA^2 + catetoB^2)
escribir("Hipotenusa: " .. hipotenusa)  # Salida: 5
```

```text
Párrafo posterior opcional que analiza el resultado obtenido.
```

---

### 3.3 Tratamiento de Párrafos Contextuales Previos

¿Qué hacer cuando una sección ya tiene un párrafo explicativo que describe lo que hace el código?

1. **Evitar la redundancia:** No escribas un párrafo diciendo *"A continuación calculamos el promedio de una lista:"* seguido de una etiqueta que diga *"Ejemplo: Calcular el promedio de una lista"*.
2. **Regla de Integración:**
   * **Opción 1 (Párrafo contextual amplio):** Mantén la explicación teórica como párrafo y usa la etiqueta para dar un título técnico breve al bloque:
     > La función `cadena.recortar()` elimina tanto espacios como tabulaciones al inicio y final de una cadena:
     >
     > 🎯 **Ejemplo: Sanitización de entradas con espacios en blanco**
     >
     > ```latino
     > entrada = "   usuario_admin   "
     > limpia = cadena.recortar(entrada)
     > # ⚠️ ç() no ejecuta llamadas a funciones: |cadena.longitud(limpia)|
     > # imprimiría "<funcion nativa>". Resuelve el valor antes de interpolar.
     > lon = cadena.longitud(limpia)
     > escribir(ç("Longitud saneada: |lon|"))
     >
   * **Opción 2 (Párrafo breve):** Si el texto anterior era solo una línea introductoria, suprímelo y transfórmalo directamente en la etiqueta del ejemplo:

     > 🎯 **Ejemplo: Concatenación de colecciones mediante `lista.concatenar`**
     >
     > ```latino
     > listaA = [1, 2]
     > listaB = [3, 4]
     > unida = lista.concatenar(listaA, listaB)
     > escribir(unida)  # Salida: [1, 2, 3, 4]
     > ```

### 3.4 Geometría de Código y Límite de Ancho de Columna (Maquetado PDF)

Para evitar que herramientas de maquetado tipográfico (Adobe InDesign) o visores en pantallas reducidas apliquen saltos de línea automáticos que fracturen identificadores, palabras clave o cadenas literales:

1. **Límites de Ancho de Columna:**
   * **Recomendado:** Máximo **72 a 76 caracteres** por línea física de código.
   * **Límite crítico absoluto:** Ninguna línea en un bloque cercado debe superar los **80 caracteres** (incluyendo sangría de espacios).
2. **Puntos de Corte Canónicos en Latino.NET (`.lat`):**
   * **En pipelines (`|>`):** El salto se realiza **antes** del operador, iniciando la siguiente línea con sangría de 4 espacios.
   * **En concatenaciones (`..`):** Cortar **antes** del operador (estilo *leading operator*), permitiendo identificar visualmente la continuación en el margen izquierdo.
   * **En consultas SQL y textos multilínea:** Aprovechar las cadenas multilínea nativas (`\"...\"`) cortando tras las cláusulas maestras (`SELECT`, `FROM`, `WHERE`, `JOIN`, `VALUES`).
   * **En colecciones (`[]`, `{}`):** Estructurar verticalmente listas o diccionarios largos (apertura en línea inicial, un elemento por renglón sangrado a 4 espacios y cierre en su propia línea).
   * **En firmas y llamadas con múltiples argumentos:** Abrir paréntesis, quebrar un parámetro por línea y cerrar el paréntesis al nivel de la cabecera.
   * **En operadores lógicos (`ý`, `ó`):** Envolver la expresión entre paréntesis y quebrar **antes** del operador con sangría protectora.
   * **En el operador ternario (`? :`):** En expresiones largas, estructurar como tabla vertical alineando el separador `:` debajo del indicador `?`.
3. **⛔ Regla Negativa Absoluta del Compositor `ç()`:**
   * **Terminantemente prohibido quebrar líneas DENTRO de los delimitadores `|...|`**. El texto literal de la plantilla puede tener saltos de línea, pero la expresión incrustada debe ser atómica y residir en un único renglón físico.
4. **Puntos de Corte en Lenguajes Auxiliares:**
   * **Terminal y Shell (Bash/CLI):** Usar siempre la barra invertida `\` al final del renglón para quebrar comandos extensos.
   * **C# (.NET):** Quebrar tras comas de argumentos, tras operadores LINQ o en asignaciones concatenadas con `+`.
   * **JSON:** Formato tabular vertical estricto con sangría de 2 o 4 espacios; prohibido incluir arreglos u objetos densos en una sola línea horizontal.

---

---

## 4. Reglas de Calidad e Invariantes Técnicas para el Código

Todo bloque de código en el manual debe cumplir con las siguientes reglas de ingeniería:

### 4.1 Autosuficiencia e Inicialización de Variables

* Prohibido utilizar variables que no hayan sido creadas en el mismo bloque de código.
* Si un ejemplo utiliza una lista o diccionario, debe inicializarse arriba (`datos = [ ]` o `config = {}`).
* Si se llama a una función propia o auxiliar, la función debe estar declarada dentro del mismo bloque.

### 4.2 Salidas Declaradas y Coherencia Lógica

* Si se indica un comentario `# Salida:` o se añade un bloque de texto con el resultado esperado, **el código visible debe contener las funciones `escribir()`, `escribirf()` o `imprimirf()` que efectivamente lo produzcan**.
* Prohibidas las «salidas mágicas» o matemáticamente incoherentes con las funciones estándar.

### 4.3 Control del No Determinismo

Funciones que retornan valores dependientes del entorno, reloj o red jamás deben declararse con salidas fijas sin calificar:

* `sis.tiempo()`, `lapsos.ahora()`, `sis.fecha()`
* `mate.aleatorio()`
* Hashes generados sobre estampas de tiempo dinámicas
* Mediciones de rendimiento con `sis.milisegundos()`

**Directiva:** En estos casos, la salida esperada o el comentario en código debe indicar explícitamente:

```latino
# Salida variable (depende del momento de ejecución y hardware):
```

### 4.4 Documentación de Entradas Interactivas

Cuando un ejemplo utilice `pedir()` o `leer()`, **es obligatorio documentar la entrada simulada en la misma línea**:

```latino
nombre = pedir("Ingresa tu nombre: ")  # Entrada simulada: Carlos
edad = anumero(pedir("Tu edad: "))      # Entrada simulada: 28
```

### 4.5 Higiene de Recursos y Efectos Secundarios

Cualquier ejemplo que abra conexiones a bases de datos SQLite o cree archivos en disco debe mostrar cómo liberar y limpiar los recursos al terminar:

* Cerrar conexiones: `sqlite.cerrar(db)`
* Limpiar archivos temporales creados para la prueba: `archivo.borrar("archivo_prueba.txt")`
* Liberar cerrojos de sincronización: `mutex.eliminar(m)`

### 4.6 Rutas Estrictamente Relativas

* **Terminantemente prohibido** utilizar rutas absolutas como `C:/Users/...` o rutas al sistema operativo como `C:/Windows/System32/...`.
* Usar siempre rutas relativas limpias: `./datos/config.json` o `datos.txt`.

---

## 5. Estilo de Programación Idiomático en Latino.NET

Para que el lector aprenda el estilo correcto del lenguaje, los ejemplos del manual deben seguir estas directrices:

### 5.1 Ámbito Léxico y el Uso Correcto de `local`

Latino.NET **no es Lua**. Las variables declaradas dentro de una función o bloque son **locales por defecto** si no existen en un ámbito superior. No satures los ejemplos con `local` en cada línea cotidiana.

**Reserva `local` exclusivamente para:**

1. **Privacidad estricta en POO (Constructores):** Ocultar datos de la instancia mediante clausuras (`local _clave = 123`).
2. **Encapsulamiento en el Patrón Fábrica (Módulos):** Estado interno que no se exporta en el diccionario devuelto.
3. **Sombreado explícito (*Shadowing*):** Cuando ya existe una variable global con ese nombre (ej. `total` o `config`) y deseas crear una nueva variable temporal sin alterar el valor exterior.
4. **Supervivencia de variables en bloques (`si`, `mientras`):** Inicializar una variable antes de entrar al bloque para que siga viva después de `fin`.

```latino
# ✅ Código idiomático limpio (sin local redundante)
funcion calcularPromedio(numeros)
    suma = 0
    para n en numeros
        suma = suma + n
    fin
    retornar suma / lista.longitud(numeros)
fin
```

### 5.2 Concatenación frente a Aritmética

* El operador `+` es **estrictamente aritmético**. Jamás lo utilices para unir texto.
* La concatenación se realiza exclusivamente con el operador `..` (o `..=` para asignación acumulativa).
* Para mezclar variables con texto en mensajes legibles, utiliza el compositor `ç("... |variable| ...")`.

### 5.3 Operadores Lógicos Estrictos

* Utiliza siempre los operadores lógicos válidos: `ý` o `&&` para AND; `ó` o `||` para OR; `no` o `!` para NOT.
* Las palabras `y` y `o` sin tilde son identificadores de variables válidos (útiles para coordenadas matemáticas `x, y` u objetos `o`), no operadores.

### 5.4 Conversión de Tipos

* La función global canónica para convertir a texto es **`acadena()`**. No invoques `cadena()` como función, ya que `cadena` representa la biblioteca de utilidades de texto.
* La conversión a número se realiza mediante **`anumero()`**.

### 5.5 Placeholders: Nunca un nombre de librería estándar

⚠️ **Regla:** en plantillas, esquemas y snippets con marcadores de posición, el
nombre de la variable de trabajo **nunca puede coincidir con una librería
estándar** (`lista`, `mate`, `cadena`, `dic`, `pila`, `cola`, `conjunto`,
`tupla`, `sis`).

El motivo es concreto: en Latino.NET, una variable **oculta la librería**. La
asignación no da error y el fallo aparece al usarlo, con un mensaje engañoso:

```latino
lista = [10, 20, 30]   # ← la variable se llama 'lista' y eso ya basta
lista.longitud(lista)   # ❌ No existe el método lista.longitud()

# Renombrar la variable basta: la librería vuelve a estar disponible
datos = [10, 20, 30]
lista.longitud(datos)   # ✅ 3
```

**Usa nombres que no colisionen:** `coleccion`, `datos`, `elementos`, `valores`,
`nombres`. Son tan claros como `lista` y no rompen nada. ⚠️ Evita el prefijo `mi`:
es la palabra reservada de POO para instancias (`mi.propiedad`).

> **Aplicado en la extensión.** Los 6 marcadores `${2:lista}` de
> `extension/snippets/latino.code-snippets` se sustituyeron por `${2:coleccion}`,
> que es la convención que ya usaban los snippets modernos del mismo archivo.
> A día de hoy ese archivo tiene **0 colisiones** y `coleccion` es su nombre dominante.

**Y recuerda quién emite el texto.** Si el documento es una referencia de lo que
expande la extensión, **no lo "corrijas" por adelantado**: quedaría mintiendo sobre
lo que la extensión realmente emite. El defecto se corrige en el origen y el
documento se actualiza después.

---

## 6. Formato de Tablas en Markdown

Para asegurar la correcta interpretación de las tablas en navegadores, GitHub y editores de texto:

1. Toda tabla debe incluir una cabecera clara.
2. La línea divisoria entre la cabecera y el cuerpo debe utilizar alineación explícita a la izquierda mediante dos puntos:

   ```markdown
   | Comando | Tipo | Descripción |
   | :--- | :--- | :--- |
   | `acadena()` | Global | Convierte un valor a texto. |
   ```

3. No dejes celdas vacías; si un parámetro es opcional o no aplica, indícalo como `nulo`, `Opcional` o `-`.

---

## 7. Lista de Verificación Rápida para Revisores

Antes de dar por finalizado un módulo o admitir un Pull Request, valida los siguientes puntos:

* [ ] **Sin encabezados de ejemplo:** Ningún ejemplo usa `##`, `###` ni `####` como título **salvo cuando el encabezado cumple la excepción estructural del §3.1-D** (organiza el capítulo en lugar de rotular el bloque).
* [ ] **Etiqueta uniforme:** Los ejemplos ejecutables comienzan con `🎯 **Ejemplo: [Descripción]**`.
* [ ] **Líneas en blanco:** Existe una línea vacía antes y después de cada bloque de código delimitado con comillas triples.
* [ ] **Código autosuficiente:** Todas las variables, listas y módulos requeridos están definidos en el bloque.
* [ ] **Sin rutas absolutas:** Solo se emplean rutas relativas portables (`./`).
* [ ] **Limpieza de archivos:** Los scripts que crean archivos `.db` o `.txt` los eliminan al finalizar su ejecución.
* [ ] **Salidas variables calificadas:** Salidas con `sis.tiempo()`, fechas o hashes dinámicos llevan el comentario `# Salida variable`.
* [ ] **Entradas simuladas:** Cada llamada a `pedir()` o `leer()` incluye `# Entrada simulada: ...`.
* [ ] **Uso correcto de `acadena()`:** No se invoca `cadena(valor)` como conversor.
* [ ] **Placeholders sin colisión:** En plantillas y snippets, ninguna variable de
      trabajo se llama como una librería estándar (`lista`, `mate`, `cadena`, `pila`…). Se
      prefieren `miLista`, `datos`, `coleccion` (§5.5).
* [ ] **Prosa en una sola línea física:** cada párrafo se escribe como UNA línea, sin cortes manuales. El ancho lo decide el editor o el exportador a PDF; cortar a ~95 caracteres parte la oración en un punto arbitrario y obliga a maquetar a mano. Al unir prosa existente, nunca dentro de un bloque de código.
* [ ] **Emojis estandarizados:** Solo se utiliza el set canónico (`❌`, `✅`, `⚠️`, `💡`, `🔒`) y nunca glifos como `✓`.
* [ ] **Límite de columna respetado:** Ninguna línea en bloques de código supera los 80 caracteres (ideal ≤ 76) para garantizar una maquetación limpia en InDesign sin saltos arbitrarios.

---

> **Historial de Versiones del Documento:**
>
> * **v2.0 (2026-09):** Sustitución integral de la antigua `guia-emojis.md`. Incorporación del estándar universal de etiquetado de ejemplos `🎯`, solución a advertencias de linters Markdown, reglas de higiene de efectos secundarios e integración de la semántica de ámbito léxico refinada de Latino.NET v1.0.81.
>
> * **v2.0.1 (2026-09):** Añadida la categoría **§3.1-D — Bloques bajo Encabezado de Navegación**, única excepción permitida al uso de encabezados Markdown como precedente de un bloque de código. Precisa el criterio de discernimiento y actualiza el checklist del §7 en consecuencia. Evita que auditorías futuras conviertan subtítulos de navegación (`#### Uso 1: ...`) en etiquetas de ejemplo, destruyendo la estructura del capítulo.
>
> * **v2.0.2 (2026-09):** Añadida la sección **§5.5 — Placeholders: Nunca un nombre de módulo**, con el caso verificado de `${2:lista}` en la extensión y la regla de no "corregir por adelantado" un documento que espeja lo que emite la extensión. Nuevo punto en la checklist de §7, que precisa el criterio de discernimiento y recoge la única excepción permitida: los encabezados Markdown pueden preceder a un bloque de código cuando solo lo titulan. Evita que auditorías futuras conviertan subtítulos de navegación (`#### Uso 1: ...`) en etiquetas de ejemplo, destruyendo la estructura del capítulo.
>
> * **v2.0.3 (2026-11):** Incorporación de la sección **§3.4 — Geometría de Código y Límite de Ancho de Columna**, definiendo los límites de 72-80 caracteres y los puntos de corte canónicos en Latino.NET (pipelines, concatenación, SQL, colecciones y prohibición de cortes dentro del compositor `ç()`) para garantizar un maquetado limpio en InDesign sin fractura de tokens.
