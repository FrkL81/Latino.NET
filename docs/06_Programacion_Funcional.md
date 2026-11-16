# CAPÍTULO 6: Programación Funcional

## La Elegancia de los Datos en Movimiento

A lo largo de los capítulos anteriores hemos explorado dos formas fundamentales de concebir el software. La primera, la **Programación Imperativa**, nos enseñó a dictarle a la máquina una receta secuencial paso a paso: *haz esto primero, luego aquello, incrementa este índice y salta aquí si se cumple esta condición*. La segunda, la **Programación Orientada a Objetos**, nos mostró cómo modelar el mundo encapsulando datos y comportamientos en entidades que resguardan su identidad y protegen sus reglas internas.

En este capítulo abriremos la puerta al tercer gran paradigma que late en el núcleo de Latino.NET: **la Programación Funcional (PF)**.

Lejos de ser una moda académica o una colección de sintaxis críptica, la programación funcional es un cambio de perspectiva: dejamos de preocuparnos por los estados intermedios de las variables para centrarnos en el **flujo continuo de transformación de la información**.

### El «Qué» frente al «Cómo»

La frontera que separa a la programación imperativa de la funcional no es puramente sintáctica; es esencialmente filosófica:

- **El Enfoque Imperativo se obsesiona con el *CÓMO*:** Describe la mecánica microscópica de la computadora: qué variables se inicializan, el orden exacto de los índices, los contadores manuales y la mutación constante de la memoria en cada vuelta de un bucle.
- **El Enfoque Funcional se concentra en el *QUÉ*:** Describe la intención y el destino de los datos: declara qué transformaciones matemáticas y lógicas deben atravesar las colecciones, tratando a las funciones como tuberías puras por donde los datos viajan sin alterar el entorno.

La mejor manera de experimentar este contraste es frente a un problema cotidiano de negocio. Supongamos que recibimos una lista de transacciones y requerimos filtrar únicamente aquellas que superen los $500, aplicarles el 16% de IVA y sumarlas para obtener la facturación total.

#### 1. Enfoque Imperativo (Instrucción Mecánica de Pasos)

Le indicamos a la máquina cada movimiento físico de la memoria:

```latino
ventas = [120, 650, 430, 890, 310, 720]
total = 0

desde (i = 0; i < lista.longitud(ventas); i++)
    si ventas[i] > 500
        conIva = ventas[i] * 1.16
        total = total + conIva
    fin
fin

escribir("Total: $" .. total)
# Salida: Total: $2621.6
```

#### 2. Enfoque Funcional (Declaración y Flujo de Transformación)

Declaramos la cadena de intenciones de izquierda a derecha, delegando el trabajo pesado a la biblioteca estándar de .NET 10:

```latino
ventas = [120, 650, 430, 890, 310, 720]

total = ventas
    |> lista.filtrar(funcion(v) retornar v > 500 fin)
    |> lista.transformar(funcion(v) retornar v * 1.16 fin)
    |> lista.reducir(funcion(acum, v) retornar acum + v fin, 0)

escribir("Total: $" .. total)
# Salida: Total: $2621.6
```

Ambos fragmentos entregan al centavo exactamente el mismo resultado. Sin embargo, la versión funcional se lee como una descripción en lenguaje natural: *toma las ventas, filtra las mayores a 500, aplícales el IVA y súmalas*.

Observa la ganancia inmediata de estabilidad:

- **Cero variables mutables:** No existe un acumulador `total` cambiando de valor en cada iteración.
- **Cero contadores manuales:** No hay índices `i` que puedan salirse de rango o causar desbordamientos (*off-by-one errors*).
- **Inmutabilidad total:** La lista original `ventas` permanece intacta en la memoria; el cálculo no produce efectos secundarios accidentales.

*(Nota: Si las funciones `filtrar`, `transformar`, `reducir` o el operador pipeline `|>` te resultan nuevos, no te preocupes: en las siguientes secciones desglosaremos cada una de estas herramientas paso a paso).*

### Simbiosis, no Reemplazo: La Convivencia con la POO

Antes de continuar, hay que establecer una **regla de oro: la Programación Funcional (PF) y la Programación Orientada a Objetos (POO) no son enemigas; son herramientas complementarias**. La aparición de un paradigma no invalida al otro, ni existe uno que sea absolutamente superior. En Latino.NET ambos pueden convivir en perfecta armonía arquitectónica, aprovechando las fortalezas de cada uno según las necesidades del problema:

1. **Usa POO** en los límites de tu sistema para modelar **entidades con ciclo de vida, estado interno y reglas que protejan**: conexiones a bases de datos, ventanas de interfaz gráfica, cuentas bancarias, sesiones de usuario o personajes de un videojuego.
2. **Usa Programación Funcional** en el corazón de tu lógica de negocio cuando necesites **procesar colecciones, consultar datos, realizar cálculos matemáticos o transformar información**. Su enfoque en funciones y transformaciones de datos facilita escribir código más predecible, reutilizable y seguro para la ejecución en paralelo.

El objetivo de este capítulo, en conjunto con el capítulo de POO, es darte el criterio necesario para elegir la herramienta adecuada ante cada problema concreto, o incluso combinar ambos paradigmas inteligentemente.

> 💡 **Regla de Oro del Flujo de Datos (Mantra de Latino.NET):**
>
> *«Compón transformaciones con el Pipeline y las funciones de orden superior».*
>
> Evita el «infierno de los paréntesis anidados» y el trabajo innecesario de crear listas vacías intermedias para llenarlas mediante bucles manuales. Aprovecha el operador Pipeline (`|>`) para encadenar transformaciones y permitir que los datos fluyan de forma limpia y lineal, de izquierda a derecha.

### 6.1 Funciones de Primera Clase y Anónimas (Lambdas)

En muchos lenguajes de programación, las funciones son "ciudadanos de segunda clase": solo sirven para ser declaradas en algún lugar del archivo y luego llamadas por su nombre. No puedes hacer nada más con ellas.

En Latino.NET, **las funciones son ciudadanos de primera clase** (*first-class citizens*). Esto significa que una función es un valor más en la memoria, exactamente igual que un número, una cadena de texto o un booleano. Y si es un valor como cualquier otro, entonces se aplican las mismas reglas:

- Se puede guardar en una variable.
- Se puede guardar dentro de una lista o un diccionario.
- Se puede pasar como argumento a otra función.
- Se puede retornar como resultado de otra función.

Cada uno de estos puntos abre posibilidades que son imposibles en el paradigma imperativo clásico. Vamos a explorarlos uno a uno.

#### Guardar una función en una variable

Cuando declaras una función con la palabra clave `funcion`, Latino.NET internamente crea un objeto de tipo `funcion` y lo asigna al nombre que le diste. Ese objeto puede asignarse también a otras variables, exactamente como harías con un número:

```latino
funcion SaludarFuerte(nombre)
    retornar "¡HOLA " .. cadena.mayusculas(nombre) .. "!"
fin

# Asignamos el objeto función a una nueva variable
# Nota: SIN paréntesis. No la estamos ejecutando, solo copiando la referencia.
miGrito = SaludarFuerte

escribir(miGrito("Frank"))  # Salida: ¡HOLA FRANK!
escribir(tipo(miGrito))     # Salida: funcion
```

> **⚠️ Error Común — `f` versus `f()`**
>
> Esta distinción es crítica y es la fuente de uno de los errores más frecuentes al empezar con programación funcional:
>
> ```latino
> funcion Duplicar(x) retornar x * 2 fin
>
> a = Duplicar    # a guarda el objeto función (no lo ejecuta)
> b = Duplicar(5) # b guarda el número 10 (resultado de ejecutarla)
>
> escribir(tipo(a)) # "funcion"
> escribir(tipo(b)) # "decimal"
> ```
>
> Cuando ves `Duplicar` sin paréntesis, estás hablando *sobre* la función. Cuando ves `Duplicar(5)`, la estás *ejecutando*. En programación funcional necesitarás constantemente hablar *sobre* funciones sin ejecutarlas, así que esta distinción se volverá instintiva con la práctica.

#### Funciones anónimas (Lambdas)

Si una función es solo un valor, no siempre necesita un nombre formal. Podemos crearla directamente donde la necesitemos usando la palabra clave `funcion` (o su alias `fun`) seguida de los parámetros, el cuerpo y el cierre `fin`. Por no tener nombre, se llaman **funciones anónimas**, o **lambdas** en la jerga de la industria.

```latino
# Función anónima asignada a una variable
duplicador = funcion(numero)
    retornar numero * 2
fin

escribir(duplicador(10)) # Salida: 20
```

Para cuerpos simples que caben en una línea, la sintaxis queda muy compacta:

```latino
duplicador = funcion(n) retornar n * 2 fin
```

Las lambdas de una línea son la forma más frecuente en que aparecerán las funciones anónimas en el resto de este capítulo, especialmente como argumentos de `filtrar`, `transformar` y `reducir`.

#### Pasar funciones como argumentos: el patrón Callback

El verdadero poder de las funciones de primera clase aparece cuando empezamos a *pasarlas* como argumentos a otras funciones. A la función que se pasa como argumento se le llama **callback** (función de retrollamada).

Para entender por qué esto es valioso, considera el siguiente escenario de diseño:

> Estás construyendo una función que procesa una lista de salarios. Sabes que necesitarás iterar la lista y construir una nueva con los valores procesados. Pero *no sabes* de antemano qué tipo de procesamiento querrá aplicar cada usuario de tu código: a veces será un bono, a veces un descuento, a veces una conversión de moneda. Si codificas la lógica directamente, tendrás que reescribir la función cada vez que cambie la regla.

La solución funcional es separar la *mecánica* (iterar y construir la lista nueva) de la *lógica de negocio* (qué hacer con cada elemento), y pedir esa lógica como parámetro:

```latino
funcion ProcesarSalarios(listaSalarios, Transformador)
    local resultado = [ ]
    para salario en listaSalarios
        nuevoSalario = Transformador(salario)  # Ejecutamos el callback
        lista.agregar(resultado, nuevoSalario)
    fin
    retornar resultado
fin

nominasBase = [1000, 2000, 3000]

# Caso A: el usuario quiere aplicar un bono de 10%
bonoNavidad = funcion(monto) retornar monto * 1.10 fin
escribir(ProcesarSalarios(nominasBase, bonoNavidad))
# Salida: [1100, 2200, 3300.0000000000005]

# Caso B: descuento de $50. Pasamos la lambda directamente, sin guardarla
salariosNetos = ProcesarSalarios(nominasBase, funcion(monto)
    retornar monto - 50
fin)
escribir(salariosNetos)
# Salida: [950, 1950, 2950]
```

Observa la separación de responsabilidades que se logra de forma natural:

- `ProcesarSalarios` solo sabe *cómo iterar* una lista y construir otra. No sabe nada de reglas de negocio.
- `bonoNavidad` y la lambda de descuento solo saben *qué hacer* con un número. No saben nada de iteración.

Cada parte puede evolucionar, probarse y reutilizarse de forma independiente. Este patrón es exactamente la base sobre la que están construidas las funciones nativas `lista.filtrar()`, `lista.transformar()` y `lista.reducir()`, con la diferencia de que esas funciones delegan el bucle al motor interno de C#, haciéndolas considerablemente más rápidas que cualquier bucle escrito en el script. Eso es precisamente lo que veremos en la sección 6.3.

### 6.2 Clausuras (Closures) y Retención de Estado

Para entender las clausuras necesitamos hablar primero de algo que sucede invisible bajo el capó cada vez que invocas una función: el **entorno léxico**.

Cuando Latino.NET ejecuta una función, crea un espacio privado de memoria para esa función — una "burbuja" que contiene todas sus variables locales. Normalmente, cuando la función termina, esa burbuja estalla y la memoria se libera. Esto es el comportamiento estándar de casi todos los lenguajes de programación.

Pero hay una excepción: **si la función crea otra función en su interior y la retorna hacia el exterior, la burbuja no estalla**. La función interna mantiene viva la burbuja de su función padre, porque la necesita para funcionar. Esa burbuja persistente, esa referencia al entorno donde nació una función, es lo que llamamos **clausura léxica** (*lexical closure*).

Visualízalo así:

```text
┌─────────────────────────────────────┐
│  Entorno de CrearContador(0)        │
│                                     │
│  conteo = 0   ◄──────────────────┐  │
│                                  │  │
│  ┌─────────────────────────────┐ │  │
│  │  funcion()                  │ │  │
│  │    conteo = conteo + 1  ────┘ │  │
│  │    retornar conteo            │  │
│  └─────────────────────────────┘ │  │
│         │                        │  │
│         │  Esta función es       │  │
│         │  la que se retorna     │  │
└─────────┼───────────────────────────┘
          │
          ▼
    contadorA = esa función
    (lleva consigo la referencia
     al entorno donde nació)
```

La función anónima retornada *sabe dónde vive* `conteo`. No tiene una copia del valor: tiene una referencia viva al espacio de memoria original. Por eso puede leerlo y modificarlo en cada llamada.

#### Retención de estado: contadores y generadores

Este mecanismo permite crear **estado privado y persistente sin necesidad de POO**:

```latino
funcion CrearContador(inicio)
    local conteo = inicio

    retornar funcion()
        conteo = conteo + 1
        retornar conteo
    fin
fin

# Cada llamada a CrearContador genera un entorno independiente
contadorA = CrearContador(0)
contadorB = CrearContador(100)

escribir(contadorA())  # 1
escribir(contadorA())  # 2
escribir(contadorA())  # 3

escribir(contadorB())  # 101  ← su propio entorno, no interfiere con A
escribir(contadorA())  # 4    ← retomamos A, el estado se mantuvo

# Intento de acceso directo desde fuera:
# escribir(conteo)  → Error: 'conteo' no existe en este ámbito
```

`contadorA` y `contadorB` son dos funciones que viven en dos burbujas de memoria completamente separadas. Modificar el estado de una no afecta a la otra en absoluto. Esto es **encapsulamiento perfecto** sin clases, sin instancias y sin modificadores de acceso.

#### Fábrica de Funciones (*Function Factory*)

El mismo mecanismo que nos dio contadores nos permite hacer algo todavía más poderoso: **funciones que generan otras funciones especializadas**, configuradas mediante los parámetros de la función padre.

Imagina que en diferentes partes de tu aplicación necesitas multiplicar valores por distintos factores: calcular impuestos, aplicar descuentos, convertir unidades. Podrías escribir una función separada para cada caso, o podrías tener una sola fábrica que las produzca todas:

```latino
funcion HacerMultiplicador(factor)
    retornar funcion(numero)
        retornar numero * factor  # 'factor' vive en la clausura
    fin
fin

# La fábrica produce herramientas especializadas
calcularDoble     = HacerMultiplicador(2)
calcularTriple    = HacerMultiplicador(3)
aplicarIVA        = HacerMultiplicador(1.16)
convertirADolares = HacerMultiplicador(0.027)  # hipotético tipo de cambio

escribir(calcularDoble(50))       # 100
escribir(aplicarIVA(1000))        # 1160.0
escribir(convertirADolares(5000)) # 135.0
```

Cada función producida es independiente y reutilizable. `aplicarIVA` puede pasarse como callback a `ProcesarSalarios` de la sección anterior, o encadenarse en un Pipeline más adelante, o guardarse en un diccionario de transformaciones. Es una pieza.

> **Conexión con la siguiente sección:** La Fábrica de Funciones no es solo un patrón bonito — es la base manual de la **Aplicación Parcial** que veremos en 6.4. Cuando en esa sección construyamos piezas que encajan en el operador `|>`, estaremos usando exactamente este mismo mecanismo. Guarda mentalmente este patrón: *función que recibe una configuración y retorna una función que hace el trabajo*.

### 6.3 Funciones de Orden Superior en Colecciones

Una **función de orden superior** (*higher-order function*) es cualquier función que recibe otra función como argumento o devuelve una función como resultado. En las secciones anteriores ya construimos funciones de orden superior manualmente (`ProcesarSalarios`, `HacerMultiplicador`). Ahora vamos a usar las que Latino.NET proporciona de forma nativa para operar sobre colecciones.

El cambio mental es el siguiente:

| Enfoque imperativo | Enfoque funcional |
| :--- | :--- |
| Tú escribes el bucle | El motor itera por ti |
| Declaras una variable acumuladora | Declaras la transformación |
| Controlas el índice | Describes el resultado |
| Le dices *cómo* | Dices *qué* |

Esta delegación no es solo estética. Al confiar la iteración al motor interno de C#, el código resultante es más rápido, más seguro y considerablemente más fácil de razonar. Veamos cada herramienta.

#### Transformación — `lista.transformar()` (Map)

`lista.transformar()` recorre cada elemento de una lista, le aplica tu función, y **retorna una lista nueva** con los resultados. La lista original nunca se modifica.

```text
[1, 2, 3, 4, 5]
    │   │   │   │   │
    ▼   ▼   ▼   ▼   ▼
   x²  x²  x²  x²  x²
    │   │   │   │   │
    ▼   ▼   ▼   ▼   ▼
[1, 4, 9, 16, 25]     ← lista nueva
```

```latino
numeros  = [1, 2, 3, 4, 5]
cuadrados = lista.transformar(numeros, funcion(x) retornar x * x fin)

escribir(cuadrados) # [1, 4, 9, 16, 25]
escribir(numeros)   # [1, 2, 3, 4, 5]  ← sin cambios
```

Para diccionarios, Latino.NET ofrece dos variantes: `dic.transformarValores()` opera sobre los valores manteniendo las claves intactas, y `dic.mapear()` permite transformar tanto claves como valores:

```latino
precios = {"manzana": 2.0, "pera": 3.0, "uva": 5.0}

# Aumentar todos los precios un 10%
preciosNuevos = dic.transformarValores(precios, funcion(v) retornar v * 1.10 fin)
escribir(preciosNuevos)
# {"manzana": 2.2, "pera": 3.3, "uva": 5.5}
```

#### Transformación Anidada — FlatMap con `lista.aplanar()`

A veces la función que pasas a `transformar` no retorna un valor escalar sino una lista. En ese caso, el resultado es una **lista de listas**, que generalmente no es lo que quieres:

```latino
oraciones = ["hola mundo", "latino es genial"]

# Cada oración se convierte en una lista de palabras
listaDeListas = lista.transformar(oraciones, funcion(texto)
    retornar cadena.separar(texto, " ")
fin)

escribir(listaDeListas)
# [["hola", "mundo"], ["latino", "es", "genial"]]  ← lista de listas
```

Para resolver esto existe `lista.aplanar()`, que destruye un nivel de anidamiento y produce una lista lineal:

```latino
palabras = lista.aplanar(listaDeListas)
escribir(palabras)
# ["hola", "mundo", "latino", "es", "genial"]
```

La combinación `transformar` + `aplanar` es tan frecuente en programación funcional que en otros lenguajes tiene nombre propio: **FlatMap**. En Latino.NET se expresa como dos pasos encadenados, y como veremos en 6.5, el operador `|>` hace que esa cadena quede perfectamente legible.

#### Filtrado — `lista.filtrar()` (Filter)

`lista.filtrar()` requiere un **predicado**: una función que evalúa cada elemento y retorna `cierto` o `falso`. Solo los elementos donde el predicado retorna `cierto` pasan a la nueva lista.

```text
[15, 22, 12, 30, 18, 9]
  │   │   │   │   │   │
  ▼   ▼   ▼   ▼   ▼   ▼
 ✗   ✓   ✗   ✓   ✓   ✗    (edad >= 18?)
      │       │   │
      ▼       ▼   ▼
    [22,     30, 18]       ← lista nueva
```

```latino
edades  = [15, 22, 12, 30, 18, 9]
mayores = lista.filtrar(edades, funcion(edad) retornar edad >= 18 fin)

escribir(mayores)  # [22, 30, 18]
```

Para diccionarios, `dic.filtrar()` funciona de forma análoga, pasando clave y valor al predicado:

```latino
inventario = {"tornillo": 0, "tuerca": 45, "clavo": 0, "perno": 12}

conStock = dic.filtrar(inventario, funcion(k, v) retornar v > 0 fin)
escribir(conStock)
# {"tuerca": 45, "perno": 12}
```

#### Reducción — `lista.reducir()` (Reduce)

`lista.reducir()` es la función de orden superior más versátil, y también la que requiere más atención para entenderla bien. Toma una lista entera y la *comprime* en **un único valor final**: un número, una cadena, un diccionario, o cualquier otra cosa.

Para lograrlo, necesita una función con dos parámetros:

1. **El acumulador:** el valor que se construye iteración a iteración.
2. **El elemento actual:** el elemento de la lista que se procesa en ese paso.

Y opcionalmente, un tercer argumento: **la semilla** (el valor inicial del acumulador).

El caso más simple es la suma:

```latino
gastos = [150, 45, 200, 15]
total  = lista.reducir(gastos, funcion(acum, g) retornar acum + g fin, 0)

escribir(total)  # 410
```

Pero `reducir` no está limitado a operaciones numéricas. Aquí está su verdadero poder: puede construir *cualquier estructura* a partir de una lista. Por ejemplo, consolidar una lista de registros en un único diccionario de resumen:

```latino
ventas = [
    {"vendedor": "Ana",   "monto": 1200},
    {"vendedor": "Luis",  "monto": 800},
    {"vendedor": "Ana",   "monto": 950},
    {"vendedor": "Luis",  "monto": 600},
    {"vendedor": "Ana",   "monto": 300}
]

# Consolidar en un diccionario {vendedor: total_acumulado}
resumen = lista.reducir(ventas, funcion(acum, venta)
    local vendedor = venta["vendedor"]
    acum[vendedor] = (acum[vendedor] ?? 0) + venta["monto"]
    retornar acum
fin, { })

escribir(resumen)
# {"Ana": 2450, "Luis": 1400}
```

El acumulador comienza siendo un diccionario vacío `{}`. En cada iteración, la función busca si el vendedor ya existe en el acumulador (usando `??` para manejar el caso en que todavía no existe) y suma su venta. Al final, `reducir` ha colapsado 5 registros individuales en un resumen compacto.

> **Sobre la semilla:** Si omites el tercer argumento, `reducir` usa el primer elemento de la lista como acumulador inicial y comienza la iteración desde el segundo elemento. Esto funciona para operaciones simples como suma o máximo, pero para construcciones más complejas como el ejemplo anterior, **siempre provee una semilla explícita**. Intentar reducir una lista vacía sin semilla produce un error protector del motor.

#### Evaluación lógica — `lista.todos()` y `lista.alguno()`

A veces no queremos transformar ni filtrar los datos, sino hacerles una pregunta de validación masiva. Para eso existen dos funciones que reciben un predicado y retornan un único booleano:

- **`lista.todos()`** retorna `cierto` solo si **todos** los elementos cumplen la condición.
- **`lista.alguno()`** retorna `cierto` si **al menos uno** cumple la condición.

```latino
usuarios = [
    {"nombre": "Ana",    "activo": cierto},
    {"nombre": "Luis",   "activo": cierto},
    {"nombre": "Carlos", "activo": falso}
]

sistemaOk       = lista.todos(usuarios,  funcion(u) retornar u["activo"] fin)
requiereAtencion = lista.alguno(usuarios, funcion(u) retornar u["activo"] == falso fin)

escribir(sistemaOk)          # falso
escribir(requiereAtencion)   # cierto
```

Ambas funciones implementan **cortocircuito** internamente: `lista.todos()` detiene la iteración en el primer elemento que falla, y `lista.alguno()` la detiene en el primer elemento que cumple. En listas grandes esto puede suponer una diferencia de rendimiento significativa frente a iterar manualmente con un bucle y una bandera booleana.

#### Tabla de referencia rápida

| Función | Recibe | Retorna | Equivalente en otros lenguajes |
| :--- | :--- | :--- | :--- |
| `lista.transformar(l, f)` | Lista, función `(x) → y` | Lista nueva | `map` |
| `lista.aplanar(l)` | Lista de listas | Lista plana | parte de `flatMap` |
| `lista.filtrar(l, pred)` | Lista, predicado `(x) → bool` | Lista nueva (subconjunto) | `filter` |
| `lista.reducir(l, f, semilla)` | Lista, función `(acum, x) → acum`, semilla | Un valor único | `reduce` / `fold` |
| `lista.todos(l, pred)` | Lista, predicado | `cierto` / `falso` | `every` / `all` |
| `lista.alguno(l, pred)` | Lista, predicado | `cierto` / `falso` | `some` / `any` |
| `dic.filtrar(d, pred)` | Diccionario, predicado `(k, v) → bool` | Diccionario nuevo | `filter` sobre pares |
| `dic.transformarValores(d, f)` | Diccionario, función `(v) → v2` | Diccionario nuevo | `mapValues` |

### 6.4 Aplicación Parcial y Currificación

Antes de explicar el mecanismo, expliquemos el problema que resuelve.

Considera esta situación: tienes la función `lista.filtrar()` que acepta dos argumentos — una lista y un predicado — y quieres usarla dentro de un Pipeline. El Pipeline, como veremos en 6.5, funciona pasando el resultado de la izquierda como **único argumento** a la función de la derecha. Pero `lista.filtrar()` necesita dos argumentos. Hay una incompatibilidad de forma:

```text
lo que el Pipeline ofrece:    un valor  →  [función]
lo que lista.filtrar necesita: (lista, predicado)  ←  dos argumentos
```

Necesitamos una forma de **pre-configurar** `lista.filtrar` con el predicado de antemano, para que en el momento en que llegue la lista por el Pipeline, la función ya sepa qué filtrar. En esencia, necesitamos convertir una función de dos argumentos en una función de un solo argumento. Eso es exactamente lo que hacen la **aplicación parcial** y la **currificación**.

#### Currificación

La **currificación** (*currying*, en honor al matemático Haskell Curry) es el proceso de transformar una función que recibe múltiples argumentos en una cadena de funciones que reciben **un argumento cada una**:

```text
f(a, b, c)   →   f(a)(b)(c)
```

Cada llamada en la cadena retorna una nueva función que espera el siguiente argumento. La función final se ejecuta cuando se provee el último.

Latino.NET no hace currificación de forma automática o implícita, pero podemos construirla manualmente usando exactamente el patrón de Fábrica de Funciones que vimos en 6.2. Tomemos nuestra función de suma:

```latino
# Versión tradicional: dos argumentos simultáneos
funcion Sumar(a, b)
    retornar a + b
fin

# Versión currificada: un argumento a la vez
funcion SumarCurrificado(a)
    retornar funcion(b)
        retornar a + b   # 'a' vive en la clausura
    fin
fin
```

La versión currificada se usa así:

```latino
# Llamada completa: proveemos ambos argumentos en secuencia
resultado = SumarCurrificado(3)(7)
escribir(resultado)  # 10
```

#### Aplicación Parcial

La **aplicación parcial** es cuando tomamos la función currificada y *fijamos* solo algunos de sus argumentos, guardando el resultado como una función nueva especializada:

```latino
# Fijamos el primer argumento (a = 10)
# Obtenemos una función nueva que solo espera 'b'
SumarDiez = SumarCurrificado(10)

escribir(SumarDiez(5))   # 15
escribir(SumarDiez(20))  # 30
escribir(SumarDiez(100)) # 110
```

`SumarDiez` es ahora una función especializada y reutilizable, con `a = 10` fijo para siempre en su clausura. No duplicamos código, no usamos variables globales: solo fijamos un argumento y obtuvimos una nueva herramienta.

#### El patrón práctico: construir piezas para el Pipeline

La aplicación parcial no es solo un ejercicio matemático. Su uso más práctico en Latino.NET es exactamente el que anticipamos al inicio de esta sección: **convertir funciones de múltiples argumentos en funciones de un argumento** que encajen en el operador `|>`.

El patrón consiste en crear una función envolvente que recibe la *configuración* y retorna una función que solo espera los *datos*:

```latino
# Envolvemos lista.filtrar para pre-configurar el predicado
funcion FiltrarCon(predicado)
    retornar funcion(datos)
        retornar lista.filtrar(datos, predicado)
    fin
fin

# Envolvemos lista.transformar para pre-configurar la transformación
funcion TransformarCon(transformacion)
    retornar funcion(datos)
        retornar lista.transformar(datos, transformacion)
    fin
fin

# Envolvemos lista.reducir para pre-configurar la función y la semilla
funcion ReducirCon(funReduccion, semilla)
    retornar funcion(datos)
        retornar lista.reducir(datos, funReduccion, semilla)
    fin
fin
```

Estas tres funciones envolventes siguen exactamente el mismo patrón: reciben la *configuración* primero y retornan una función que solo necesita los *datos*. Ahora podemos crear piezas especializadas y nombradas:

```latino
# Creamos piezas especializadas usando aplicación parcial
soloActivos   = FiltrarCon(funcion(u) retornar u["activo"] fin)
extraerSalario = TransformarCon(funcion(u) retornar u["salario"] fin)
sumarTotal     = ReducirCon(funcion(acum, s) retornar acum + s fin, 0)
```

Cada pieza es una función de **un solo argumento**: los datos. No saben de dónde vendrán esos datos. Solo saben qué hacer cuando lleguen. Eso las hace perfectamente intercambiables, reutilizables y, como veremos a continuación, perfectamente compatibles con el Pipeline.

> **Conexión con 6.2:** Si reconoces el patrón de `FiltrarCon`, `TransformarCon` y `ReducirCon` como Fábricas de Funciones, es porque lo son. La aplicación parcial *es* la Fábrica de Funciones aplicada con un propósito específico: producir piezas de aridad 1 para componer. El Capítulo 6 lleva tres secciones construyendo hacia este momento.

### 6.5 El Operador Pipeline (`|>`)

Con las piezas de aridad 1 que construimos en 6.4, estamos listos para el operador que le da forma visible a la programación funcional en Latino.NET.

#### El problema: el infierno de los paréntesis

Cuando aplicamos varias transformaciones sucesivas a un mismo dato usando la notación tradicional de llamadas a función, el código se anida de adentro hacia afuera:

```latino
# Tomar "-42", convertirlo a número, sacar su valor absoluto, convertirlo a cadena
resultado = acadena(mate.abs(anumero("-42")))
```

Para leer este código debes encontrar el paréntesis más interno, entender qué hace, luego subir un nivel, luego otro. El flujo de datos va de derecha a izquierda y de adentro hacia afuera, que es exactamente lo opuesto a cómo las personas leemos y pensamos. Con cuatro transformaciones ya es incómodo; con ocho es ilegible.

#### La solución: el operador `|>`

El operador Pipeline (`|>`) invierte esta lógica. **Toma el resultado de la expresión a su izquierda y lo pasa como único argumento a la función de su derecha**, permitiendo que el flujo de datos se lea de izquierda a derecha, en el mismo orden en que ocurre:

```latino
resultado = "-42" |> anumero |> mate.abs |> acadena

escribir(resultado)  # "42"
```

Ahora el código se lee exactamente como pensamos: *toma "-42", conviértelo a número, saca el valor absoluto, conviértelo a cadena*. El dato fluye hacia adelante, paso a paso, como agua por una tubería.

> **⚙️ Detalle técnico:** El motor de Latino.NET exige que la expresión a la derecha del `|>` evalúe a un objeto `ICallable` (una función). El operador inyectará el valor izquierdo como argumento a esa función. Si la expresión derecha no es una función, el motor lanzará un error en tiempo de ejecución.

#### El Pipeline con múltiples líneas

Para cadenas más largas, Latino.NET permite distribuir el Pipeline en varias líneas. Cada paso queda en su propia línea, formando una lista visual de transformaciones:

```latino
resultado = datos
    |> paso1
    |> paso2
    |> paso3
    |> paso4
```

Esta forma es la más legible cuando el Pipeline tiene más de tres pasos, y es la que usaremos en los ejemplos que siguen.

🎯 **Ejemplo Completo: Pipeline + Colecciones + Aplicación Parcial**

Aquí es donde todo el Capítulo 6 converge. Vamos a calcular la nómina total de los empleados activos de una empresa, pasando los datos por una cadena de transformaciones.

Primero, los datos:

```latino
empleados = [
    {"nombre": "Ana",    "activo": cierto,  "salario": 2000},
    {"nombre": "Luis",   "activo": falso,   "salario": 1500},
    {"nombre": "Carlos", "activo": cierto,  "salario": 3000},
    {"nombre": "Marta",  "activo": cierto,  "salario": 2500},
    {"nombre": "Pedro",  "activo": falso,   "salario": 1800}
]
```

Segundo, creamos nuestras piezas usando las funciones envolventes de 6.4. Cada pieza tiene un nombre que describe su intención:

```latino
soloActivos    = FiltrarCon(funcion(e) retornar e["activo"] fin)
extraerSalario = TransformarCon(funcion(e) retornar e["salario"] fin)
sumarTotal     = ReducirCon(funcion(acum, s) retornar acum + s fin, 0)
```

Tercero, ensamblamos el Pipeline. Los datos entran por la izquierda y el resultado sale por la derecha:

```latino
# Definiciones de funciones auxiliares (sección 6.4)
funcion FiltrarCon(predicado)
    retornar funcion(datos)
        retornar lista.filtrar(datos, predicado)
    fin
fin

funcion TransformarCon(transformacion)
    retornar funcion(datos)
        retornar lista.transformar(datos, transformacion)
    fin
fin

funcion ReducirCon(funReduccion, semilla)
    retornar funcion(datos)
        retornar lista.reducir(datos, funReduccion, semilla)
    fin
fin

# Creación de piezas especializadas
soloActivos    = FiltrarCon(funcion(e) retornar e["activo"] fin)
extraerSalario = TransformarCon(funcion(e) retornar e["salario"] fin)
sumarTotal     = ReducirCon(funcion(acum, s) retornar acum + s fin, 0)

# Datos de entrada
empleados = [
    {"nombre": "Ana",    "activo": cierto,  "salario": 2000},
    {"nombre": "Luis",   "activo": falso,   "salario": 1500},
    {"nombre": "Carlos", "activo": cierto,  "salario": 3000},
    {"nombre": "Marta",  "activo": cierto,  "salario": 2500},
    {"nombre": "Pedro",  "activo": falso,   "salario": 1800}
]

# Pipeline: los datos fluyen de izquierda a derecha
totalNomina = empleados
    |> soloActivos     # [Ana(2000), Carlos(3000), Marta(2500)]
    |> extraerSalario  # [2000, 3000, 2500]
    |> sumarTotal      # 7500

escribir("Nómina total: $" .. totalNomina)
# Salida: Nómina total: $7500
```

Cada línea del Pipeline tiene una responsabilidad única y un nombre que la documenta. Si mañana la regla de negocio cambia y también quieres incluir a los empleados con contrato temporal, solo modificas `soloActivos`. El resto del Pipeline no se toca.

> **¿Por qué no simplemente `empleados |> lista.filtrar(...)`?**
> El operador Pipeline inyecta el valor izquierdo como **primer argumento** de la función derecha. Si la función requiere parámetros adicionales, estos se pasan entre paréntesis directamente:
>
> ```latino
> totalNomina = empleados
>     |> lista.filtrar(fun(e) ret e["activo"] fin)
>     |> lista.transformar(fun(e) ret e["salario"] fin)
>     |> lista.reducir(fun(acum, s) ret acum + s fin, 0)
> ```
>
> Las funciones auxiliares (`FiltrarCon`, `TransformarCon`) son un excelente ejercicio para construir **Fábricas de Funciones**, pero no son obligatorias para usar `|>`.

#### Un Pipeline sobre datos de texto

El Pipeline no es exclusivo de listas de diccionarios. Funciona igual de bien para cualquier flujo de transformaciones:

```latino
# Contar cuántas palabras únicas tiene un texto
texto = "la casa es grande y la casa tiene jardin y el jardin es grande"

contarUnicas = texto
    |> funcion(t) retornar cadena.separar(t, " ") fin  # lista de palabras
    |> funcion(palabras)                                # eliminar duplicados
        retornar lista.unicos(palabras)
    fin
    |> lista.longitud                                   # contar

escribir("Palabras únicas: " .. contarUnicas)
```

#### Los límites del Pipeline: cuándo no usarlo

El Pipeline es una herramienta, no un dogma. Hay situaciones donde forzarlo empeora el código:

**1. Cuando necesitas bifurcaciones.** El Pipeline es una tubería recta. Si en algún punto del flujo necesitas tomar decisiones (`si/sino`), la tubería debe romperse. Es más honesto usar variables intermedias:

```latino
# ❌ Forzado e ilegible
resultado = datos |> pasoA |> funcion(d) si condicion retornar pasoB(d) sino retornar pasoC(d) fin fin

# ✅ Claro y mantenible
parcial = datos |> pasoA
si condicion
    resultado = parcial |> pasoB
sino
    resultado = parcial |> pasoC
fin
```

**2. Cuando hay efectos secundarios en el medio.** Si una de las funciones de la cadena escribe en una base de datos, imprime en pantalla o modifica una variable externa, el Pipeline oculta ese efecto secundario dentro de lo que parece un flujo de transformación pura. Eso hace el código difícil de depurar. Los efectos secundarios deben vivir fuera de la cadena, no dentro.

**3. Cuando la cadena es demasiado larga.** Un Pipeline de tres a cinco pasos es el punto óptimo de legibilidad. Si tienes diez pasos encadenados y hay un error, será difícil identificar en cuál paso exacto ocurrió. Cuando la cadena supera los cinco pasos, considera dividirla en dos Pipelines con un nombre intermedio que explique qué se logró en la primera mitad.

```latino
# Dos Pipelines nombrados son más debuggeables que uno de diez pasos
datosLimpios  = datosRaw   |> normalizar |> eliminarNulos |> validar
resultadoFinal = datosLimpios |> calcular  |> formatear    |> serializar
```

### 6.6 Recursión como Alternativa Funcional a los Bucles

En la programación imperativa, la iteración se expresa con bucles: `desde`, `mientras`, `para`. Todos ellos tienen algo en común — requieren una variable que muta en cada ciclo (`i = i + 1`, `contador = contador + 1`). En el paradigma funcional estricto esa mutación es exactamente lo que se quiere evitar.

La alternativa es la **recursión**: una función que resuelve un problema llamándose a sí misma con una versión *reducida* del mismo problema, hasta llegar a un caso tan simple que puede resolverse directamente sin llamarse de nuevo.

No es solo una curiosidad teórica. Hay clases enteras de problemas — estructuras en árbol, datos anidados de profundidad desconocida, algoritmos de división y conquista — donde la recursión es la solución natural y un bucle imperativo sería artificialmente torpe.

#### Estructura obligatoria: caso base y caso recursivo

Toda función recursiva bien diseñada tiene exactamente dos partes. Sin las dos, la función está incompleta:

**El caso base** es la condición de parada. Es el problema tan pequeño o simple que puede resolverse directamente, sin llamarse de nuevo. Si olvidas el caso base, la función se llama a sí misma indefinidamente.

**El caso recursivo** es la llamada a sí misma con un problema *estrictamente más pequeño o reducido*. La palabra clave es "estrictamente": cada llamada debe acercarse al caso base, nunca alejarse.

```latino
funcion Factorial(n)
    # Caso base: el problema más pequeño posible
    si n <= 1
        retornar 1
    fin

    # Caso recursivo: n se reduce en cada llamada
    retornar n * Factorial(n - 1)
fin

escribir(Factorial(5))  # 120
```

Para entender qué ocurre internamente, es útil ver cómo el motor *expande* y luego *colapsa* las llamadas:

```text
Factorial(5)
  └─ 5 * Factorial(4)
           └─ 4 * Factorial(3)
                    └─ 3 * Factorial(2)
                             └─ 2 * Factorial(1)
                                      └─ 1        ← caso base, empieza el colapso
                             └─ 2 * 1 = 2
                    └─ 3 * 2 = 6
           └─ 4 * 6 = 24
  └─ 5 * 24 = 120
```

Cada nivel de sangría en ese diagrama es una llamada que ocupa espacio físico en la memoria del programa. Esto es central para entender la advertencia que viene a continuación.

#### Donde la recursión brilla: estructuras anidadas

Los bucles tradicionales son buenos para listas planas. Donde la recursión aplasta a los bucles es al navegar estructuras de profundidad desconocida: JSONs anidados, árboles de categorías, sistemas de archivos, expresiones matemáticas.

Considera el problema de sumar todos los números de una estructura que puede tener cualquier nivel de anidamiento:

```latino
datosComplejos = [1, [2, 3], [[4], 5], [[[6]]]]
# Respuesta esperada: 21
```

Con un bucle imperativo, necesitarías saber de antemano cuántos niveles hay para anidar los bucles. Con recursión, el problema se resuelve de forma natural porque la función puede llamarse a sí misma para cada subestructura que encuentre:

```latino
funcion SumarTodo(elemento)
    # Caso base A: es un número, lo retornamos directamente
    si tipo(elemento) == "decimal"
        retornar elemento
    fin

    # Caso base B / Recursivo: es una lista, sumamos sus partes
    si tipo(elemento) == "lista"
        retornar lista.reducir(elemento, funcion(acum, sub)
            retornar acum + SumarTodo(sub)  # llamada recursiva por cada elemento
        fin, 0)
    fin

    retornar 0  # cualquier otro tipo no contribuye a la suma
fin

datosComplejos = [1, [2, 3], [[4], 5], [[[6]]]]
escribir(SumarTodo(datosComplejos))  # 21
```

La función no sabe cuántos niveles tiene la estructura. No necesita saberlo. Simplemente pregunta: *¿es un número? retórnalo. ¿es una lista? suma sus partes aplicando la misma lógica a cada una*. La profundidad se maneja sola.

#### 📝 Límite de recursión y el Call Stack

Cada llamada recursiva reserva un espacio en la **pila de llamadas** (*call stack*) del programa — la misma estructura que vimos en el diagrama de expansión del Factorial. Esa pila tiene un tamaño finito. Si la recursión es demasiado profunda, la pila se desborda y el programa termina abruptamente con un error de tipo *StackOverflow*.

Para proteger al programa, **Latino.NET limita la profundidad de recursión a 100 llamadas por defecto**. Si tu función supera ese límite, el motor lanza un `RuntimeError` atrapable antes de que ocurra el desbordamiento real.

Puedes verificar este comportamiento:

```latino
funcion ProbarLimite(n)
    retornar ProbarLimite(n + 1)  # sin caso base: recursión infinita
fin

intentar
    ProbarLimite(0)
atrapar(e)
    escribir("Motor detuvo la recursión: " .. e)
    # Salida: Motor detuvo la recursión: [Línea 1] Límite de recursividad excedido (100).
fin
```

Si tu problema legítimamente requiere más profundidad, puedes ajustar el límite:

```latino
sis.config("max_stack", 5000)
```

> **Úsalo con criterio.** Elevar el límite no elimina el riesgo de desbordamiento — solo lo desplaza. Si tu algoritmo requiere 10,000 llamadas recursivas sobre una lista plana, la recursión probablemente no es la herramienta correcta para ese caso. Un bucle `desde` o `lista.reducir()` será más eficiente y más seguro.

#### TCO: por qué Latino.NET no lo implementa

En la teoría de lenguajes funcionales existe una optimización llamada *Tail Call Optimization* (TCO). Un compilador con TCO detecta cuando la llamada recursiva es la **última operación** de la función (sin ningún cálculo pendiente después de ella) y en lugar de apilar un nuevo frame, reutiliza el frame actual. Esto convierte la recursión de cola en algo equivalente a un bucle en consumo de memoria.

**Latino.NET no implementa TCO**, porque se ejecuta sobre el runtime de .NET cuyo modelo de pila no permite ese tipo de reutilización de frames de forma transparente. Es importante que lo sepas para no asumir que una recursión "bien escrita" puede ser infinitamente profunda. El límite de pila siempre aplica.

#### Cuándo usar recursión y cuándo no

| Situación | Recomendación |
| :--- | :--- |
| Estructura de árbol o anidamiento de profundidad desconocida | ✅ Recursión — es la solución natural |
| Algoritmos de división y conquista (merge sort, búsqueda binaria) | ✅ Recursión — el problema se divide limpiamente |
| Contar del 1 al N, sumar una lista plana, iterar un arreglo | ❌ Usa `lista.reducir()` o un bucle — más eficiente y sin riesgo de stack |
| Procesamiento de más de ~80 niveles de profundidad | ⚠️ Evalúa si ajustar `max_stack` o reformular con una pila explícita |

### 6.7 Buenas Prácticas: Pureza, Transparencia e Inmutabilidad

Las secciones anteriores cubrieron las herramientas. Esta sección cubre la filosofía que hace que esas herramientas funcionen correctamente a escala. No son reglas arbitrarias: cada una tiene una razón técnica concreta detrás.

#### 1. Funciones puras

Una **función pura** cumple dos condiciones sin excepción:

1. Dado el mismo conjunto de argumentos, **siempre retorna el mismo resultado**.
2. No produce **efectos secundarios**: no modifica variables externas, no escribe en disco, no imprime en pantalla, no altera el estado del programa fuera de su propio scope.

```latino
global tasaIVA = 0.16

# ❌ Función IMPURA — depende de una variable global que puede cambiar en cualquier momento
funcion PrecioConIVAImpuro(precio)
    retornar precio * (1 + tasaIVA)
fin

# ✅ Función PURA — todo lo que necesita entra por los parámetros
funcion PrecioConIVA(precio, tasa)
    retornar precio * (1 + tasa)
fin
```

La función impura puede retornar resultados distintos para el mismo `precio` dependiendo de cuándo se llama, si otro hilo modificó `tasaIVA`, o si alguien en otra parte del código decidió cambiar la variable global. La función pura siempre retorna el mismo resultado para los mismos argumentos, sin importar nada más.

#### 2. Transparencia referencial

La transparencia referencial es la consecuencia directa de tener funciones puras, y es la propiedad que hace que el código funcional sea fácil de **razonar, depurar y probar**.

Una función es **referencialmente transparente** si puedes reemplazar cualquier llamada a esa función por su valor de retorno y el programa se comporta exactamente igual. Es decir: `f(x)` puede sustituirse por su resultado dondequiera que aparezca, sin efectos sobre el resto del programa.

```latino
funcion PrecioConIVA(precio, tasa)
    retornar precio * (1 + tasa)
fin

# La llamada se puede sustituir por su valor y nada cambia
total = PrecioConIVA(100, 0.16) + PrecioConIVA(200, 0.16)
escribir(total)                      # 347.99999999999994

# Equivalente a sustituir cada llamada por su resultado:
escribir(116.0 + 232.0)              # 348.0
```

¿Ves la sutileza? Los dos valores *representan* lo mismo, pero en punto flotante dan bits ligeramente distintos: `347.99999999999994` frente a `348.0`. La sustitución es válida **semánticamente** —el programa se comporta igual—, pero si necesitas igualdad exacta entre ambos resultados, primero redondea:

```latino
funcion PrecioConIVA(precio, tasa)
    retornar precio * (1 + tasa)
fin

total = PrecioConIVA(100, 0.16) + PrecioConIVA(200, 0.16)
escribir(mate.redondear(total, 2))       # 348
escribir(116.0 + 232.0)                  # 348
```

Esto tiene una consecuencia práctica muy concreta: **las funciones puras son triviales de probar**. No necesitas levantar una base de datos, no necesitas configurar un entorno, no necesitas preocuparte por el orden en que se ejecutan los tests. Solo pasas argumentos y verificas la salida:

```latino
# La función pura que vamos a someter a prueba
funcion PrecioConIVA(precio, tasa)
    retornar precio * (1 + tasa)
fin

# Test de una función pura: tan simple como esto
funcion TestPrecioConIVA()
    # ⚠️ Comparamos con redondeo: ver la nota de punto flotante más abajo
    si mate.redondear(PrecioConIVA(100, 0.16), 2) != 116.0
        escribir("❌ FALLO: precio base 100 con 16% IVA debería dar 116")
        retornar falso
    fin
    si mate.redondear(PrecioConIVA(0, 0.16), 2) != 0.0
        escribir("❌ FALLO: precio base 0 siempre debe dar 0")
        retornar falso
    fin
    escribir("✅ PrecioConIVA: todos los casos pasan")
    retornar cierto
fin

TestPrecioConIVA()
```

> ⚠️ **Punto flotante: el detalle que hace fallar tests "obvios".** Una función pura siempre retorna lo mismo para los mismos argumentos, pero eso no significa que retorne un número *limpio*. `100 * (1 + 0.16)` da `115.99999999999999` en punto flotante IEEE 754, no `116.0`. Si comparas eso directamente con `!= 116.0`, el test falla siempre — y no por un error de la función, sino por la representación binaria de los decimales.
>
> Por eso los tests de valores numéricos se escriben con tolerancia:
>
> ```latino
> # Opción A: redondear a los decimales significativos
> mate.redondear(PrecioConIVA(100, 0.16), 2) == 116.0
>
> # Opción B: comparar la diferencia contra una tolerancia
> mate.abs(PrecioConIVA(100, 0.16) - 116.0) < 0.0001
> ```
>
> Esto **no contradice la transparencia referencial**: la función sigue siendo pura. Es una propiedad del tipo `número`, no de la función.

Prueba esto con una función impura que depende de `tasaIVA` global y verás de inmediato el problema: el resultado del test depende del estado del programa en el momento en que se ejecuta. Con funciones puras, el test es determinista siempre.

#### 3. La mutación silenciosa: el bug más difícil de encontrar

Considera este código. Tómate un momento para predecir qué imprime:

```latino
funcion AgregarBono(salarios, bono)
    para i en rango(lista.longitud(salarios))
        salarios[i] = salarios[i] + bono  # ← mutación directa
    fin
    retornar salarios
fin

nominasOriginales = [1000, 2000, 3000]
nominasConBono    = AgregarBono(nominasOriginales, 500)

escribir(nominasConBono)      # [1500, 2500, 3500] — esperado
escribir(nominasOriginales)   # ¿?
```

Si esperabas que `nominasOriginales` siguiera siendo `[1000, 2000, 3000]`, la respuesta es: **también es `[1500, 2500, 3500]`**. La función modificó la lista original porque en Latino.NET las listas se pasan por referencia. `AgregarBono` no trabajó sobre una copia — trabajó directamente sobre la misma lista en memoria.

Este tipo de bug es particularmente peligroso porque `nominasOriginales` en ningún momento recibió una asignación explícita. El cambio fue silencioso y a distancia. En un programa grande con múltiples funciones operando sobre los mismos datos, rastrear de dónde vino la mutación puede tomar horas.

La versión funcional no modifica la lista original. En su lugar, **retorna una lista nueva**:

```latino
funcion AgregarBonoPuro(salarios, bono)
    # lista.transformar siempre retorna una lista nueva
    retornar lista.transformar(salarios, funcion(s) retornar s + bono fin)
fin

nominasOriginales = [1000, 2000, 3000]
nominasConBono    = AgregarBonoPuro(nominasOriginales, 500)

escribir(nominasConBono)      # [1500, 2500, 3500]
escribir(nominasOriginales)   # [1000, 2000, 3000] ← intacta
```

La regla práctica: **cuando necesites "modificar" una colección dentro de una función, crea una nueva en lugar de alterar la original**. Las funciones nativas de Latino.NET (`lista.transformar`, `lista.filtrar`, `lista.reducir`) siguen este principio por diseño — ninguna de ellas toca la colección que reciben.

#### 4. Retornos seguros con `??` y `??=`

En un Pipeline o en una cadena de transformaciones funcionales, un valor `nulo` inesperado puede romper toda la cadena de golpe. El operador de **fusión nula (`??`)** es la herramienta para manejar esa situación de forma elegante:

```latino
# Sin fusión nula: si "puerto" no existe, el acceso al diccionario retorna nulo
# y la operación que sigue puede fallar
funcion ObtenerConfig(cfg)
    retornar {
        "puerto":   cfg["puerto"]   ?? 8080,
        "host":     cfg["host"]     ?? "localhost",
        "timeout":  cfg["timeout"]  ?? 30,
        "debug":    cfg["debug"]    ?? falso
    }
fin

# Con una configuración parcial, la función igual produce valores válidos
miConfig = ObtenerConfig({"host": "192.168.1.1"})
escribir(miConfig)
# {"puerto": 8080, "host": "192.168.1.1", "timeout": 30, "debug": falso}
```

El operador `??=` es su complemento: asigna un valor a una variable *solo si esa variable actualmente es nula*, dejándola intacta si ya tiene un valor:

```latino
# Inicialización diferida: solo asigna si aún es nulo
cache = nulo

# Simula la lectura del disco para que el ejemplo sea autocontenido
funcion CargarDesdeArchivo()
    escribir("  (disco: leyendo datos...)")
    retornar {"items": 3, "listo": cierto}
fin

funcion ObtenerDatos()
    cache ??= CargarDesdeArchivo()  # Solo carga una vez; las siguientes llamadas reutilizan
    retornar cache
fin

# Primera llamada: sí lee del disco
escribir("primera llamada:")
datos1 = ObtenerDatos()
escribir("items: " .. datos1["items"])
# Segunda llamada: NO vuelve a leer del disco
escribir("segunda llamada (NO vuelve a leer del disco):")
datos2 = ObtenerDatos()
escribir("items: " .. datos2["items"])
```

```text
primera llamada:
  (disco: leyendo datos...)
items: 3
segunda llamada (NO vuelve a leer del disco):
items: 3
```

En el contexto funcional, `??` es especialmente valioso dentro de `lista.reducir()` cuando construyes diccionarios acumulativos, como vimos en el ejemplo de ventas por vendedor en 6.3: `acum[vendedor] = (acum[vendedor] ?? 0) + monto`.

## 📚 Anexo: Hoja de Referencia de Funciones de Orden Superior

Latino.NET es un lenguaje multiparadigma con un soporte profundo para la programación funcional. Cualquier función en el núcleo o en los plugins que reciba una función anónima (Lambda) como argumento se considera una Función de Orden Superior.

A continuación, tienes el catálogo maestro de todas las Funciones de Orden Superior disponibles en el ecosistema, indicando exactamente qué **firma** (argumentos) inyecta el motor en tu función.

### 📜 Librería `lista` (Funcional Clásica)

| Función | Firma del Callback (Lambda) | Qué hace con la función que le pasas |
| :--- | :--- | :--- |
| `lista.transformar(l, fn)` | `fn(elemento)` | Ejecuta `fn` por cada ítem y reemplaza el valor con lo que retornes (Map). |
| `lista.filtrar(l, fn)` | `fn(elemento)` | Ejecuta `fn` por cada ítem; si retorna `cierto`, lo conserva en la nueva lista. |
| `lista.reducir(l, fn, [ac])` | `fn(acumulador, elemento)` | Ejecuta `fn` y guarda el retorno en el acumulador para la siguiente vuelta. |
| `lista.todos(l, fn)` | `fn(elemento)` | Evalúa `fn` y retorna `cierto` solo si **todos** los ítems cumplen la condición. |
| `lista.alguno(l, fn)` | `fn(elemento)` | Evalúa `fn` y retorna `cierto` si **al menos uno** cumple la condición. |
| `lista.particionar(l, fn)` | `fn(elemento)` | Ejecuta `fn` y separa la lista en dos grupos: `verdadero` y `falso`. |
| `lista.ordenar(l, [fn])` | `fn(item_a, item_b)` | Usa la `fn` como árbitro para saber quién va primero al ordenar. |

### 📖 Librería `dic` (Transformación de Mapas)

| Función | Firma del Callback (Lambda) | Qué hace con la función que le pasas |
| :--- | :--- | :--- |
| `dic.filtrar(d, fn)` | `fn(llave, valor)` | Evalúa `fn` y conserva el par solo si retorna `cierto`. |
| `dic.transformarLlaves(d, fn)` | `fn(llave)` | Evalúa `fn` y cambia el nombre de la llave en el nuevo diccionario. |
| `dic.transformarValores(d, fn)` | `fn(valor)` | Evalúa `fn` y cambia el valor asociado a la llave. |
| `dic.mapear(d, fn)` | `fn(llave, valor)` | Espera que `fn` retorne un nuevo diccionario `{"llave": k, "valor": v}`. |

### 🔤 Librería `cadena` (Transformación Dinámica)

| Función | Firma del Callback (Lambda) | Qué hace con la función que le pasas |
| :--- | :--- | :--- |
| `cadena.reemplazarRegex(...)` | `fn(match, grupo1...N, indice, original)` | Ejecuta `fn` cada vez que Regex encuentra un patrón y sustituye el texto con lo que retornes. |

### 🔬 Plugin `formulas` (Matemáticas y Ciencias)

Este plugin delega el trabajo pesado de las matemáticas de nivel universitario a lambdas de Latino.NET.

| Librería y Funciones | Firma del Callback | Uso de la Lambda |
| :--- | :--- | :--- |
| **Cálculo (Integrales):** `trapecio`, `simpson13`, `simpson38`, `cuadraturaGauss`, `monteCarlo`, `romberg`, `longitudArco` | `fn(x)` | Representa la función $f(x)$ a integrar en un intervalo $[a, b]$. |
| **Cálculo (Derivadas 1D):** `derivada`, `segundaDerivada`, `derivadaRichardson` | `fn(x)` | Representa la función $f(x)$ a derivar en un punto específico. |
| **Cálculo (Varias Variables):** `gradiente`, `jacobiana` | `fn(vector)` | Recibe una lista (vector coordenado). Retorna un escalar (gradiente) o una lista (jacobiana). |
| **Búsqueda de Raíces:** `biseccion`, `secante`, `brent`, `puntoFijo`, `regulaFalsi`, `buscarIntervalos` | `fn(x)` | Representa la función $f(x)$ donde se buscará $f(x) = 0$. |
| **Newton-Raphson:** `raices.newtonRaphson(f, df, x0)` | `f(x)`, `df(x)` | Exige DOS funciones: la función principal y su derivada analítica. |
| **Ecuaciones Diferenciales (1D):** `euler`, `eulerMejorado`, `rk4`, `rkf45`, `adams` | `fn(t, y)` | Representa la EDO de la forma $\frac{dy}{dt} = f(t, y)$. |
| **Sistemas de EDOs (Vectores):** `sistemaRk4`, `sistemaRkf45`, `disparo` | `fn(t, vector_Y)` | Recibe el tiempo y una lista de estado. Debe retornar la lista de derivadas. |
| **EDOs Implícitas / PVF:** `eulerImplicito`, `pvf` | `f(t,y)`, `dfdy(t,y)`, `f(x,y,yp)` | Exigen lambdas de múltiples argumentos para la resolución de fronteras o rigidez. |
| **Optimización (1D):** `seccionAurea`, `brent1D` | `fn(x)` | Función unimodal $f(x)$ a la que se le buscará el mínimo. |
| **Optimización Global/ND:** `descensoGradiente`, `bfgs`, `nelderMead`, `genetico`, `recocidoSimulado`, `busquedaCuadricula` | `fn(vector)` | Función de costo multidimensional a minimizar. Recibe lista, retorna escalar. |

### ⚙️ Plugins del Ecosistema (Callbacks Operativos)

| Función | Firma del Callback (Lambda) | Qué hace con la función que le pasas |
| :--- | :--- | :--- |
| `hilos.lanzar(fn, [args...])` | `fn(args...)` | Ejecuta tu función aislada en un núcleo (Thread) distinto del procesador. |
| `sqlite.transaccion(db, fn)` | `fn(id_conexion)` | Ejecuta tu función. Si falla, hace un *Rollback* automático; si tiene éxito, hace *Commit*. |

### Cierre del Capítulo: ¿Cuándo usar qué paradigma?

Dominar un paradigma es saber cuándo aplicarlo y cuándo no. Aquí está la guía de decisión para elegir entre los tres pilares de Latino.NET según la naturaleza del problema:

| Criterio del problema | Imperativo | POO | Funcional |
| :--- | :--- | :--- | :--- |
| Script corto o algoritmo puntual | ✅ | — | — |
| Entidad con ciclo de vida y estado mutable | — | ✅ | — |
| Jerarquía de tipos o herencia | — | ✅ | — |
| Transformación o filtrado de colecciones | — | — | ✅ |
| Cálculo matemático o científico | ✅ | — | ✅ |
| Código que se ejecutará en múltiples hilos | ⚠️ | ⚠️ | ✅ |
| Necesitas probar la lógica fácilmente | — | ⚠️ | ✅ |
| Flujo de datos en varias etapas (pipeline) | — | — | ✅ |
| Procesamiento de estructuras anidadas | — | — | ✅ |

La columna de **Concurrencia** merece una nota especial. Cuando múltiples hilos de ejecución operan simultáneamente sobre datos compartidos, la mutación de estado se convierte en un problema técnico grave: dos hilos pueden leer y modificar el mismo dato al mismo tiempo, produciendo resultados impredecibles. Las funciones puras que operan sobre datos inmutables eliminan este riesgo por diseño, porque no hay estado compartido que corromper. Como aprenderás en el capítulo de Hilos y Concurrencia, la pureza funcional pasa de ser una virtud filosófica a ser una garantía técnica.

Los tres paradigmas no son mutuamente excluyentes. En un programa real y maduro los encontrarás mezclados: clases POO que modelan las entidades del dominio, funciones puras que implementan las reglas de negocio, y Pipelines que orquestan el flujo de datos entre capas. La señal de que has dominado este capítulo no es que uses programación funcional en todo — es que sabes exactamente cuándo y por qué usarla.

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
