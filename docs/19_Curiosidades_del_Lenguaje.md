# CAPÍTULO 19: Curiosidades y Mecánicas Ocultas del Motor

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores avanzados, arquitectos de software, investigadores y contribuidores que deseen comprender a fondo el comportamiento interno del motor y runtime de Latino.NET.  
**Propósito:** Explorar los comportamientos emergentes, decisiones gramaticales no evidentes, mecánicas de ámbito y semántica de bajo nivel que definen la personalidad técnica del intérprete.

## La Capa Subterránea del Runtime

A lo largo de este manual hemos estudiado las reglas canónicas de Latino.NET: cómo estructurar clases, iterar colecciones, despachar llamadas concurrentes y manipular datos. Sin embargo, todo lenguaje de programación maduro posee una dimensión subterránea: **comportamientos emergentes que nacen de la interacción entre el analizador sintáctico (Parser), el árbol de sintaxis abstracta (AST) y la máquina virtual subyacente de .NET 10**.

Este capítulo no es una recopilación de fallos ni una lista de anécdotas triviales. Es un compendio avanzado de mecánicas verificadas empíricamente en el código fuente de `Latino.Core`. Son peculiaridades que resultan sorprendentes a primera vista para quien proviene de C#, Python o JavaScript, pero cuya comprensión revela el ingenio arquitectónico detrás del diseño del intérprete.

### El Encuentro entre un Lenguaje Dinámico y un Motor Estático

Latino.NET opera como un lenguaje dinámico sobre una máquina virtual fuertemente tipada. Esta simbiosis produce dinámicas fascinantes:

* **Insensibilidad a Saltos de Línea:** A diferencia de Python (atado a la indentación obligatoria) o de C/Java (atados a las llaves `{}`), el parser de Latino.NET utiliza delimitadores léxicos explícitos (`fin`). Esto permite colapsar bloques lógicos completos en una sola línea física para crear cláusulas de guarda ultra legibles.
* **Captura Léxica Diferenciada por Bucle:** Descubrirás por qué instanciar funciones anónimas dentro de un bucle `desde` captura la celda de memoria final (estilo C clásico), mientras que hacerlo dentro de un bucle `para` genera entornos aislados e inmutables por cada iteración.
* **Integridad Visual frente a Conteo de Bytes:** En la era de los emojis y la tipografía compleja, medir una cadena no puede reducirse a contar bytes UTF-8 o unidades UTF-16. Latino.NET delega la manipulación textual en enumeradores de grafemas visuales (*StringInfo*), garantizando que un emoji compuesto no se fracture al invertirlo o calcular su longitud.
* **Desacople Diacrítico:** Cómo la adopción de operadores acentuados (`ý`, `ó`) liberó las variables `y` y `o` para su uso matemático en coordenadas y física, eliminando una de las restricciones más molestas de los lenguajes dinámicos tradicionales.

Comprender estas mecánicas te otorgará un dominio absoluto sobre el motor, permitiéndote escribir código de alto rendimiento, diseñar pruebas unitarias avanzadas con metaprogramación y predecir con exactitud milimétrica el comportamiento de la memoria en tiempo de ejecución.

> 💡 **Principio de Transparencia del Runtime (Mantra de Latino.NET):**  
> *\"La verdadera maestría de un lenguaje comienza cuando comprendes con exactitud qué ocurre en la máquina cuando el motor guarda silencio.\"*  
> Conocer los límites, las sutilezas y la mecánica interna del árbol de ejecución es lo que separa a quien simplemente copia sintaxis de quien diseña arquitectura de software.

## 19.1 El Trío de Concordancia y los Cuatro Estilos de Instanciación

En Latino.NET, el soporte de concordancia de género gramatical en español no es un truco cosmético; es una característica de primera clase integrada en el despachador de llamadas.

### A. Los Cuatro Rostros de la Instanciación

Para dar vida a una clase, el motor expone cuatro expresiones sintácticas que convergen exactamente en el mismo nodo `CallExpression` del AST:

```latino
clase Ventana
    constructor(titulo)
        mi.titulo = titulo
    fin
fin

# 1. Forma Femenina (Concordancia natural con 'Ventana')
v1 = nueva Ventana("Editor")

# 2. Forma Masculina (Para sustantivos masculinos como 'Perro' o 'Archivo')
v2 = nuevo Ventana("Visor")

# 3. Forma Imperativa Neutra (Estilo procedural)
v3 = crear Ventana("Consola")

# 4. Invocación Directa (Estilo Python / Fábrica)
v4 = Ventana("Terminal")

escribir("¿Son del mismo tipo?: " .. (tipo(v1) == tipo(v4)))  # cierto
escribir("¿Son instancias únicas?: " .. (v1 == v4))             # falso
```

🔬 **Bajo el capó**

En `Parser.Expressions.cs`, las palabras clave `nueva`, `nuevo` y `crear` son tokens de tipo `TokenType.New`. Cuando el parser encuentra cualquiera de ellos, simplemente avanza y evalúa la llamada a la clase que le sigue. La cuarta forma prescinde del prefijo porque en Latino.NET **las clases son objetos invocables (`ICallable`) por derecho propio**: al llamarlas directamente como funciones, su método interno `Call()` ejecuta la cadena de constructores y retorna la nueva instancia.

> **Sobre la convención de nombres.** La tabla de convención para clases, métodos, constructores y campos vive en el **Capítulo 5, §5.1.1**. Aquí solo se registra la mecánica que la justifica: por qué no existe un plural del puntero de instancia.

### B. El Ámbito de la Instancia: Por qué no existe `mis`

Muchos desarrolladores que modelan colecciones intentan escribir instintivamente `mis.elementos = []`. En Latino.NET, **`mis` no existe ni existirá**.

```latino
clase Boveda
    constructor()
        # ✅ Correcto: El adjetivo posesivo concuerda con la instancia, no con la colección
        mi.secretos = ["clave1", "clave2"]
        este.saldo = 1000
        esta.activa = cierto
    fin
fin
```

#### 🔬 La Razón Arquitectónica

En el diseño del lenguaje:

1. `mi`, `este` y `esta` son **alias léxicos idénticos** que apuntan a la referencia de la instancia actual en memoria (equivalentes a `this` o `self`).
2. No representan pronombres gramaticales evaluados en tiempo de ejecución, sino el enlace con el entorno local de la clase (`instance.Get()` / `instance.Set()`).
3. El plural en español se expresa en el **sustantivo**, no en el puntero de ámbito. Escribir `mi.secretos` se lee en español natural como *"el campo 'secretos' de mi instancia"*.

## 19.2 Bloques en Una Línea: El Rol Estructural de `fin`

A diferencia de lenguajes como Python (que dependen de la indentación y saltos de línea) o C/Java (que dependen de llaves `{}`), Latino.NET posee una gramática **insensible a los saltos de línea**.

Esto significa que cualquier estructura de control puede colapsarse en una sola línea física, siempre y cuando se respete su delimitador canónico: la palabra clave **`fin`**.

🎯 **Ejemplo: Compactación de bloques y guardas**

```latino
# Condicional simple en una línea
si 10 > 5 escribir("Mayor") fin

# Condicional con ramas alternativas
x = -5
si x < 0 escribir("negativo") osi x == 0 escribir("cero") sino escribir("positivo") fin

# Bucles de una línea
para n en [1, 2, 3] escribir(n) fin

# Función compacta (One-Liner)
fun duplicar(v) ret v * 2 fin
escribir("duplicar(21) = " .. duplicar(21))

# Salida en pantalla:
# Mayor
# negativo
# 1
# 2
# 3
# duplicar(21) = 42
```

### El Patrón de Guarda Idiomático

En funciones de validación de negocio, esta característica permite escribir "cláusulas de guarda" ultra legibles sin anidar bloques innecesarios:

```latino
funcion procesarPago(monto, saldo)
    si monto <= 0    retornar "Monto inválido"    fin
    si monto > saldo retornar "Fondos agotados"   fin
    
    # Flujo principal limpio sin anidamientos:
    retornar "Pago procesado con éxito"
fin
```

🔬 **Bajo el capó**

El `Lexer` de Latino.NET ignora los espacios en blanco y los saltos de línea (`\n`, `\r`) para efectos gramaticales. Para el analizador sintáctico, una línea con `si a escribir(a) fin` produce exactamente el mismo árbol `IfStatement` que un bloque repartido en cuatro líneas con indentación. El token `fin` es la única barrera que le indica al parser dónde termina el cuerpo del bloque.

## 19.3 El Operador Ternario: Cortocircuito y Evaluación como Guarda

El operador ternario `condicion ? si_cierto : si_falso` tiene dos propiedades que pocos desarrolladores aprovechan al máximo: **asociatividad por la derecha** y **cortocircuito garantizado**.

🎯 **Ejemplo: Encadenamiento sin paréntesis y protección contra división por cero**

```latino
# 1. Encadenamiento fluido (Árbol de decisión ternario)
puntuacion = 85
categoria = puntuacion >= 90 ? "Sobresaliente" :
            puntuacion >= 80 ? "Notable" :
            puntuacion >= 70 ? "Aprobado" : "Reprobado"

escribir("Resultado: " .. categoria)  # Notable

# 2. Cortocircuito como guarda matemática
divisor = 0
# Si divisor es 0, la rama (100 / divisor) JAMÁS se evalúa:
seguro = (divisor != 0) ? (100 / divisor) : 0
escribir("Cálculo seguro: " .. seguro)  # 0
```

🔬 **Bajo el capó**

En `Interpreter.Evaluate(TernaryExpression)`, el motor evalúa primero la condición. Si el resultado es evaluado como verdadero mediante `IsTruthy()`, invoca `Evaluate(ThenBranch)` y retorna inmediatamente. La rama alternativa `ElseBranch` no es visitada por el intérprete. Esto convierte al ternario en un escudo protector contra errores de ejecución (*runtime crashes*) como divisiones por cero o accesos a punteros nulos.

## 19.4 Loop Capture: La Trampa de `desde` frente a la Aislamiento de `para`

Este es uno de los comportamientos más sutiles y profundos del lenguaje. Al crear funciones anónimas (clausuras o *closures*) dentro de un bucle, **el tipo de bucle elegido cambia radicalmente el resultado de la captura de variables**.

🎯 **Ejemplo: Comparación de captura léxica**

```latino
# EXPERIMENTO A: Captura con bucle C-Style 'desde'
closuresDesde = [ ]
desde (i = 0; i < 3; i++)
    lista.agregar(closuresDesde, fun() ret i fin)
fin

escribir("--- Con bucle 'desde' ---")
para fn en closuresDesde
    escribir(fn())
fin

# EXPERIMENTO B: Captura con bucle 'para .. en'
closuresPara = [ ]
para k en [0, 1, 2]
    lista.agregar(closuresPara, fun() ret k fin)
fin

escribir("--- Con bucle 'para' ---")
para fn en closuresPara
    escribir(fn())
fin

# ==============================================================================
# SALIDA REAL EN CONSOLA:
# --- Con bucle 'desde' ---
# 3
# 3
# 3
# --- Con bucle 'para' ---
# 0
# 1
# 2
# ==============================================================================
```

### ¿Por qué ocurre esto?

* **En el bucle `desde` (Modelo clásico C / JS `var`):**  
  El entorno léxico de la variable `i` se instancia **una sola vez** antes de iniciar las iteraciones. Cada función anónima captura una referencia a la *misma celda de memoria*. Cuando los closures se ejecutan más tarde, todos leen el valor con el que terminó la celda al salir del bucle: `3`.

* **En el bucle `para .. en` (Modelo moderno C# / ES6 `let`):**  
  En cada vuelta de la colección, el motor crea un `LatinoEnvironment` **fresco e independiente** para el cuerpo del bucle (`loopEnv = new LatinoEnvironment(_environment)`). La variable `k` nace y muere en cada iteración; por lo tanto, cada closure captura una variable distinta e inmutable congelada en su propio instante de tiempo.

> 💡 **Regla de Arquitectura:** Si vas a instanciar lambdas o callbacks que dependan del índice dentro de un bucle, **utiliza siempre `para i en rango(...)`**. Evitarás el clásico problema de captura de iterador.

## 19.5 Coerciones Numéricas Seguras y Normalización Decimal

Latino.NET busca ser un lenguaje amigable, idiomático y predecible. Al interactuar con conversiones de tipos primitivos (`anumero`, `alogico`), el motor aplica reglas de diseño para proteger al programador contra corrupciones silenciosas de datos.

### A. Normalización Inteligente y Fail-Safe en `anumero("1,5")`

En muchos países hispanohablantes, la coma `,` se utiliza culturalmente como separador decimal. En el estándar internacional de programación, el punto `.` es la norma. Para tender un puente natural entre ambos mundos sin comprometer la integridad matemática, Latino.NET implementa **normalización contextual segura**:

```latino
escribir(anumero("42"))        # 42
escribir(anumero("3.14"))      # 3.14
escribir(anumero("  100 "))    # 100 (descarta espacios automáticamente)

# ✅ NORMALIZACIÓN TRANSPARENTE:
escribir(anumero("1,5"))       # Imprime 1.5 (detecta la coma decimal hispana)
escribir(anumero("-0,75"))     # Imprime -0.75

# 🛡️ PROTECCIÓN CONTRA AMBIGÜEDAD (FAIL-SAFE):
escribir(anumero("1,234,56"))  # Imprime nulo (formato ambiguo/inválido)
escribir(anumero("1.234.56"))  # Imprime nulo (múltiples puntos inválidos)
```

🔬 **Bajo el capó**

En `src/Latino.Core/StdLib/StdLibLoader.cs`, la función nativa `anumero` aplica una regla quirúrgica antes de delegar en el conversor de coma flotante:

1. **Detección unívoca:** Si la cadena contiene **exactamente una única coma y ningún punto** (`texto.Count(c => c == ',') == 1 && !texto.Contains('.')`), el motor normaliza internamente la coma a punto decimal (`texto.Replace(',', '.')`). Esto hace que entradas como `"1,5"` se procesen limpiamente como `"1.5"`.
2. **Rechazo de miles permisivos:** La conversión se ejecuta mediante `double.TryParse` con `System.Globalization.NumberStyles.Float` bajo `CultureInfo.InvariantCulture`.
3. **Fail-Safe contra ambigüedades:** Al no incluir `NumberStyles.AllowThousands`, el motor se niega rotundamente a interpretar comas múltiples o desubicadas como separadores de miles. Si recibe una entrada ambigua como `"1,234,56"`, no produce un valor numérico distorsionado: la conversión falla de inmediato devolviendo `nulo`.

> 💡 **Regla de Arquitectura:** `anumero()` normaliza números decimales directos con coma (`"1,5"` $\rightarrow$ `1.5`), pero protege el sistema devolviendo `nulo` ante formatos combinados de moneda o miles. Si procesas archivos bancarios o CSV con notaciones complejas (como `"1.234,56"` o `"$ 1,234.56"`), limpia previamente la cadena usando la librería `cadena` antes de la conversión:
>
> ```latino
> entrada = "1.234,56"
> # Quitar separador de miles y unificar coma decimal a punto:
> limpio = cadena.reemplazar(cadena.reemplazar(entrada, ".", ""), ",", ".")
> valorReal = anumero(limpio) # 1234.56 garantizado
> ``

### B. La Filosofía de Verdad en `alogico()`

A diferencia de Python o JavaScript (donde el string `"0"` se evalúa a veces como falso según el contexto), en Latino.NET la identidad lógica de las cadenas es estrictamente existencial:

```latino
escribir(alogico(""))      # falso  (cadena vacía = ausencia de contenido)
escribir(alogico(" "))     # cierto (contiene un espacio = existe contenido)
escribir(alogico("0"))     # cierto (es un carácter '0' = existe contenido)
escribir(alogico("falso")) # cierto (es una palabra = existe contenido)
```

En Latino.NET, una cadena de texto representa un dato. **La única cadena vacía de verdad es la que no tiene longitud (`""`)**. Para evaluar semántica de formularios humanos (donde `"falso"`, `"no"` o `"0"` deban ser tratados como falsos), se debe utilizar una función de validación de negocio y no la coerción primitiva.

## 19.6 Variables Homófonas y el Desacople Léxico: `y` y `o` frente a `ý` y `ó`

En la inmensa mayoría de lenguajes de programación, las palabras clave para las operaciones lógicas de conjunción y disyunción secuestran identificadores naturales. En Python o Latino clásico, intentar nombrar una variable como `y` (la coordenada cartesiana por excelencia) colapsaba con un error sintáctico porque el analizador léxico la reservaba exclusivamente para el operador `AND`.

En Latino.NET se resolvió este conflicto de forma brillante mediante **desacople diacrítico**.

🎯 **Ejemplo: Coexistencia de coordenadas y operadores lógicos**

```latino
# En geometría, física y videojuegos, 'x' e 'y' son universales:
x = 100
y = 250
o = 0  # Origen o inclinación

# 'y' y 'o' son IDENTIFICADORES (variables ordinarias).
# 'ý' y 'ó' son OPERADORES LÓGICOS (conjunción y disyunción):
si (x > 50) ý (y == 250) ó (o != 0)
    escribir("Coordenada válida procesada correctamente")
fin

# También conviven con los símbolos tradicionales C-Style:
si (x > 50) && (y == 250) || (o != 0)
    escribir("Equivalencia simbólica C-Style confirmada")
fin

# Salida en pantalla:
# Coordenada válida procesada correctamente
# Equivalencia simbólica C-Style confirmada
```

🔬 **Bajo el capó**

En `src/Latino.Core/Analysis/Lexer.cs`:

1. Los identificadores `y` y `o` fueron removidos de la tabla `Keywords`. Cuando el lector léxico analiza la letra `y`, la categoriza legítimamente como `TokenType.Identifier`.
2. Los caracteres acentuados `ý` y `ó` están registrados directamente como `TokenType.LogicAnd` y `TokenType.LogicOr`.
3. Esto permite que el árbol sintáctico construya un nodo `BinaryExpression` donde la variable `y` opera como operando sin ambigüedad gramatical:

   ```text
   BinaryExpression (TokenType.LogicAnd)
   ├── Left:  BinaryExpression (==) -> VariableExpression("y"), Literal(10)
   └── Right: BinaryExpression (==) -> VariableExpression("o"), Literal(20)
   ```

## 19.7 Despacho Dinámico y Monkey Patching: Funciones Mutables vs. Módulos Blindados

En Latino.NET las funciones de usuario son **ciudadanos de primera clase (*First-Class Citizens*)**. Una función declarada con `funcion Nombre() ... fin` no es un bloque estático grabado en piedra; es una variable en el entorno léxico que apunta a un objeto invocable (`LatinoFunction`).

Esto permite técnicas avanzadas como **Monkey Patching**, decoradores dinámicos y sustitución de dependencias (*Mocking*) para pruebas unitarias.

🎯 **Ejemplo: Reasignación de funciones e inmutabilidad de módulos**

```latino
# 1. Función de usuario mutable
funcion ObtenerPrecio()
    retornar 100
fin

# Guardamos una captura de la referencia original
Original = ObtenerPrecio

# Sobrescribimos la función en caliente (Monkey Patching)
ObtenerPrecio = funcion()
    # Inyectamos lógica: precio original con 15% de recargo
    retornar Original() * 1.15
fin

escribir("Precio con recargo: " .. ObtenerPrecio())  # 114.99999999999999
escribir("Original intacta:     " .. Original())        # 100

# 2. Las librerías estándar nativas están BLINDADAS contra mutación:
intentar
    mate.sen = funcion(x) retornar 0 fin
atrapar (e)
    escribir("Protección nativa: " .. e)
fin

# Salida en pantalla:
# Precio con recargo: 114.99999999999999
# Original intacta:     100
# Protección nativa: [Línea 15] Solo las instancias tienen campos.
```

🔬 **Bajo el capó**

* **La captura por referencia:** Cuando asignas `Original = ObtenerPrecio`, la variable `Original` guarda un puntero directo al objeto `LatinoFunction` existente. Cuando reasignas `ObtenerPrecio = funcion() ...`, estás cambiando el destino del identificador en el `LatinoEnvironment`, no mutando el objeto en memoria. Por eso `Original()` sigue llamando a la primera versión.
* **El seguro de inmutabilidad:** Todas las librerías estándar (`mate`, `cadena`, `sis`, `archivo`, etc.) son instancias de `LatinoDictionary` creadas durante el arranque (`StdLibLoader.cs`). Antes de exponerlos a `Globals`, el motor ejecuta `modulo.MakeReadOnly()`. Cualquier intento de reasignar o borrar sus funciones es interceptado inmediatamente en C# lanzando un `RuntimeError` defensivo.

## 19.8 Autopoiesis: Código que se Autogenera y Autocarga en Tiempo Real

Latino.NET es un lenguaje dinámico con capacidad de **metaprogramación autopoietica**: un programa puede escribir código fuente en disco, incluirlo dentro de su propio árbol de ejecución en caliente y consumirlo al instante.

🎯 **Ejemplo: Generación dinámica y consumo inmediato**

```latino
# Definimos el nombre del script temporal
archivoDinamico = "calculador_generado.lat"

# Sintetizamos código en una cadena de texto
codigoFuente = "
funcion CalcularImpuestoDinamico(base)
    # Tasa calculada en caliente al momento de la generación
    retornar base * 0.18
fin
"

# 1. Escribir el nuevo módulo en disco
archivo.escribir(archivoDinamico, codigoFuente)

# 2. Cargar el módulo en el intérprete activo
incluir(archivoDinamico)

# 3. La función 'CalcularImpuestoDinamico' ahora existe en el entorno global:
total = CalcularImpuestoDinamico(500)
escribir("Impuesto calculado dinámicamente: " .. total)  # 90

# 4. Limpieza defensiva del entorno
archivo.borrar(archivoDinamico)

# Salida en pantalla:
# Impuesto calculado dinámicamente: 90
```

🔬 **Bajo el capó**

Cuando se ejecuta `incluir(archivoDinamico)`:

1. El motor lee el texto recién creado y lo pasa por el pipeline completo: `Lexer` $\rightarrow$ `Parser` $\rightarrow$ `Statements`.
2. El intérprete evalúa esas sentencias sobre el **mismo `Globals`** del proceso en ejecución.
3. Los símbolos declarados en el archivo pasan a formar parte del entorno léxico global de inmediato.
4. El posterior borrado físico del archivo (`archivo.borrar`) no afecta al programa: las funciones generadas ya residen en la memoria RAM como objetos de primera clase.

## 19.9 La Ilusión del Carácter: Grafemas, Pares Subrogados y la Realidad Unicode

En ciencias de la computación, uno de los errores más comunes es asumir que **un carácter visible en pantalla equivale a un elemento de memoria**. En la mayoría de los lenguajes basados en UTF-16 (como JavaScript, Java o C# nativo), los emojis y caracteres especiales rompen las funciones de cadena porque ocupan múltiples posiciones (*surrogate pairs* o pares subrogados).

En Latino.NET, la librería `cadena` fue diseñada bajo el principio de **integridad visual**: opera sobre **elementos de texto (grafemas perceived)** y no sobre posiciones crudas de memoria.

🎯 **Ejemplo: La diferencia entre bytes y grafemas visuales**

```latino
# 1. Cadena que combina caracteres ASCII, acentos y emojis compuestos:
texto = "A👨‍👩‍👧B"

# Medición de longitud visual (lo que ve el usuario)
longitudVisual = cadena.longitud(texto)

# Inspección de códigos de memoria UTF-16 subyacentes
bytesInternos = cadena.bytes(texto)

escribir("Texto analizado: " .. texto)
escribir("Longitud percibida (cadena.longitud): " .. longitudVisual)
escribir("Unidades de memoria UTF-16 (cadena.bytes): " .. lista.longitud(bytesInternos))
escribir("Lista de códigos UTF-16: " .. bytesInternos)

# 2. Inversión segura:
invertido = cadena.invertir(texto)
escribir("Invertido sin corromper: " .. invertido)

# ==============================================================================
# SALIDA REAL EN CONSOLA:
# Texto analizado: A👨‍👩‍👧B
# Longitud percibida (cadena.longitud): 3
# Unidades de memoria UTF-16 (cadena.bytes): 10
# Lista de códigos UTF-16: [65, 55357, 56424, 8205, 55357, 56425, 8205, 55357, 56423, 66]
# Invertido sin corromper: B👨‍👩‍👧A
# ==============================================================================
```

### ¿Por qué la longitud es 3 pero hay 10 códigos?

El emoji de la familia (`👨‍👩‍👧`) parece un solo símbolo, pero en el estándar Unicode es una secuencia compleja compuesta por:

1. `Hombre` (`55357, 56424` — par subrogado)
2. `ZWJ (Zero Width Joiner)` (`8205` — unión invisible)
3. `Mujer` (`55357, 56425` — par subrogado)
4. `ZWJ` (`8205` — unión invisible)
5. `Niña` (`55357, 56423` — par subrogado)

En otros lenguajes, hacer un `substring(0, 2)` o `reverse()` rompería los pares subrogados a la mitad, emitiendo signos de interrogación corruptos (`??`).

🔬 **Bajo el capó**

En `src/Latino.Core/StdLib/StringLib.cs`, funciones como `longitud()`, `subcadena()`, `invertir()`, `rellenar_izquierda()` y `centrar()` no utilizan el indexador `.Length` ni `.Substring()` de C#. En su lugar, delegan en `System.Globalization.StringInfo` y su iterador `GetTextElementEnumerator()`.

Latino.NET trata cada secuencia de grafemas indivisibles como una unidad atómica. Para el programador de Latino.NET:

* `texto[0]` es `"A"`.
* `texto[1]` es la familia completa `"👨‍👩‍👧"`.
* `texto[2]` es `"B"`.

## 19.10 Referencias Cruzadas del Manual

Para profundizar en los fundamentos arquitectónicos que dan origen a estas curiosidades, consulta los capítulos especializados correspondientes:

| Mecánica o Curiosidad | Capítulo de Profundización | Aspecto Clave a Consultar |
| :--- | :--- | :--- |
| **Convención de nombres de POO** | `docs/05_POO.md` §5.1.1 | Tabla de convención y el por qué de `mi` / `este` / `esta` |
| **Herencia y Constructores** | `docs/05_POO.md` | Relación *ES-UN*, cascada de inicialización y duck-typing. |
| **Gramática y Variables** | `docs/01_Fundamentos.md` | Nomenclatura, tipos primitivos y operadores diacríticos (`ý`, `ó`). |
| **Entornos y Closures** | `docs/04_Funciones.md` | Cadena de ámbitos (`LatinoEnvironment`), clausuras y paso de parámetros. |
| **Metaprogramación e Inspección** | `docs/10_Metaprogramación.md` | Evaluación dinámica con `evaluar()` y análisis de AST. |
| **Manejo de Errores e Inmutabilidad** | `docs/09_Más_Sintaxis_Errores_Lib/9.0_Sintaxis_Errores_Ecosistema.md` | Estructuras `intentar/atrapar` y protección `IsReadOnly`. |
| **Arquitectura de Ejecución** | `docs/14_Guia_Arquitectura_Interna.md` | El Tree-Walker, gestión de memoria y delegación en .NET 10. |
