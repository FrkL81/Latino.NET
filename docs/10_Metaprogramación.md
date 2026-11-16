# CAPÍTULO 10: Metaprogramación y Código Dinámico

**Audiencia:** Desarrolladores, estudiantes, analistas, investigadores y creadores de software que deseen automatizar tareas complejas, evaluar fórmulas dinámicas y flexibilizar el comportamiento de sus aplicaciones.  
**Objetivo:** Comprender la maleabilidad del motor de Latino.NET para inspeccionar estructuras, sintetizar expresiones y ejecutar código de forma segura y accesible en tiempo real.

## El Código que Crea Código

En la gran mayoría de los lenguajes de programación tradicionales existe una muralla rígida e infranqueable entre dos momentos: el **tiempo de compilación** (cuando el programa se escribe y se analiza) y el **tiempo de ejecución** (cuando el programa ya está corriendo y solo procesa datos). En esos entornos, el código fuente es una escultura de piedra congelada: no puede cambiar, no puede inspeccionarse a fondo y no puede crecer una vez puesto en marcha.

Si un analista financiero necesita que su programa aplique fórmulas fiscales cambiantes almacenadas en una base de datos, o si un investigador desea que el usuario introduzca su propia ecuación matemática en una caja de texto, los lenguajes estáticos obligan a programar engorrosos intérpretes manuales o a recompilar la aplicación entera desde cero.

En **Latino.NET**, esa barrera se disuelve por diseño.

### La Memoria Viva del Motor

Al estar construido sobre la arquitectura modular y dinámica de **.NET 10**, el motor de Latino.NET no desecha sus herramientas de análisis sintáctico una vez que inicia tu script. Componentes clave como el **Analizador Léxico (`Lexer`)**, el **Analizador Sintáctico (`Parser`)** y el **Árbol de Sintaxis Abstracta (`AST`)** residen permanentemente en la memoria del runtime, listos para ser invocados mediante instrucciones directas y en español cuando tu lógica lo requiera.

A esta capacidad se le conoce formalmente como **Metaprogramación**: la técnica de escribir código que puede examinar, generar, modificar o ejecutar otros fragmentos de código —o a sí mismo— mientras está funcionando.

### Una Capacidad al Alcance de Todos

Aunque la palabra «metaprogramación» suene como un concepto reservado a especialistas en compiladores, en la práctica cotidiana de Latino.NET representa una herramienta cotidiana y sumamente intuitiva para resolver problemas del mundo real:

- **Para quien automatiza procesos:** Evaluar reglas de negocio o fórmulas de descuento que cambian semanalmente sin tener que tocar los scripts principales.
- **Para la ciencia y la educación:** Crear calculadoras interactivas donde el usuario teclea expresiones como `mate.sen(x) * 2` y el programa las grafica instantáneamente.
- **Para el desarrollo de software y APIs:** Construir tablas de despacho que activan funciones automáticamente según comandos de texto, o validar que un registro cumpla con una estructura antes de guardarlo.

A lo largo de este capítulo descubrirás cómo gobernar este poder a través de tres dimensiones: la **introspección** (preguntarle a los datos qué son y qué contienen con `tipo()`), la **síntesis** (armar instrucciones en vivo combinando el compositor `ç()` con `evaluar()`), y la **ejecución defensiva** (evaluar código dinámico en entornos protegidos sin comprometer la estabilidad del sistema).

> 💡 **La Regla de Oro del Código Dinámico:**  
> *"La flexibilidad no debe comprometer la seguridad."*  
> Ejecutar código en caliente es una de las mayores ventajas de Latino.NET, pero evaluar entradas no confiables sin sanitización abre la puerta a inyecciones accidentales. En este capítulo aprenderás no solo a sintetizar código al vuelo, sino a aplicar listas blancas (*whitelisting*) y barreras defensivas para que tu software sea tan seguro como moldeable.

## 💻 10.1 Filosofía: El Lenguaje que se Examina a Sí Mismo

### El Código como Dato (*Code-as-Data*)

El principio fundacional de la metaprogramación es tratar las instrucciones del lenguaje no como órdenes inmutables talladas en piedra, sino como **estructuras de datos vivas y manipulables**.

En un paradigma tradicional:

- Un archivo JSON o un registro de base de datos es un **dato**.
- Un bucle `mientras` o una condición `si` es **código**.

En Latino.NET, **el código puede nacer a partir de un dato**:

1. Una regla de negocio almacenada como texto en una base de datos SQLite puede leerse en memoria.
2. Esa cadena puede ensamblarse dinámicamente mediante plantillas.
3. El motor la parsea en microsegundos y la transforma en nodos ejecutables del AST.
4. El resultado interactúa con el resto de las variables locales y de instancia sin intermediarios.

Esta capacidad convierte a Latino.NET en un lenguaje **maleable**, emparentado con la tradición reflexiva de entornos como Lisp, Ruby, Python o Smalltalk, pero respaldado por la infraestructura de alto rendimiento y gestión de memoria de .NET 10.

### Los Tres Pilares Metaprogramáticos de Latino.NET

Para dominar la metaprogramación en este lenguaje, debemos distinguir con precisión matemática sus tres pilares operativos:

```text
┌────────────────────────────────────────────────────────────────────────┐
│               METAPROGRAMACIÓN EN LATINO.NET                           │
├──────────────────┬──────────────────────┬──────────────────────────────┤
│ 1. INTROSPECCIÓN │ 2. REFLEXIÓN Y       │ 3. SÍNTESIS Y                │
│                  │    MUTABILIDAD       │    EVALUACIÓN                │
├──────────────────┼──────────────────────┼──────────────────────────────┤
│ Descubrir qué es │ Adaptar y moldear    │ Construir código al vuelo    │
│ un objeto en     │ estructuras de datos │ y ejecutarlo en memoria      │
│ tiempo real.     │ dinámicamente.       │ en tiempo de ejecución.      │
│                  │                      │                              │
│ • tipo(x)        │ • dic.llaves()       │ • evaluar() / eval()         │
│ • tipo(instancia)│ • Inyección dinámica │ • cadena.ejecutar()          │
│ • Inspección AST │   de campos en POO   │ • archivo.ejecutar()         │
└──────────────────┴──────────────────────┴──────────────────────────────┘
```

#### 1. Introspección (Lectura y Diagnóstico)

Es la capacidad de un programa de **mirarse al espejo**. Permite interrogar a cualquier valor en memoria sobre su naturaleza sin provocar fallos en tiempo de ejecución:

- Saber si una variable recibida en una función es un número (`"decimal"`), una colección (`"lista"`) o un objeto de negocio (`"Usuario"`).
- Determinar si una clave existe dentro de un diccionario antes de consultar su contenido.
- Discriminar entre funciones nativas escritas en C# y funciones anónimas (*lambdas*) creadas por el usuario en Latino.

#### 2. Reflexión y Mutabilidad Estructural (Adaptación)

Latino.NET es un lenguaje dinámico donde los esquemas de los objetos no están sellados rígidamente tras su declaración:

- Las instancias de clases creadas con `nuevo` o `nueva` permiten la **adición dinámica de atributos** en caliente, facilitando patrones como *Property Bag* o adaptadores de datos sin alterar la clase original.
- Los diccionarios (`{}`) funcionan como tablas de símbolos de primera clase que pueden transformarse, invertirse (`dic.invertir`) o mapearse (`dic.mapear`) estructuralmente en pleno vuelo.

#### 3. Síntesis y Evaluación Dinámica (Creación y Ejecución)

Es el grado más alto de la metaprogramación. Consiste en componer texto que representa código válido de Latino.NET (utilizando interpolación de variables o generadores de texto) e inyectarlo directamente en el pipeline del intérprete:

- El motor tokeniza, parsea y genera nodos de sintaxis abstracta en tiempo real.
- Permite construir **Motores de Reglas**, **Calculadoras de Fórmulas Matemáticas de Usuario** y **Lenguajes Específicos de Dominio (DSL)** sin necesidad de escribir un parser manual desde cero.

### La Frontera Entre la Elegancia y el Abuso

La metaprogramación otorga una libertad arquitectónica inmensa, pero exige una responsabilidad de ingeniería equivalente. En las ciencias de la computación existe un aforismo clásico:

> *"Si la depuración es el proceso de eliminar errores, programar con metaprogramación es el arte de crearlos en lugares donde nadie puede verlos."*

En Latino.NET, la metaprogramación se reserva para problemas que **no pueden resolverse elegantemente con código estático**:

- **Cuándo SÍ usarla:** Cuando las reglas de cálculo cambian frecuentemente y deben almacenarse en archivos de configuración o bases de datos; cuando se construyen analizadores de fórmulas ingresadas por usuarios; o cuando se crean frameworks y herramientas de desarrollo.
- **Cuándo NO usarla:** Para resolver flujos lógicos comunes que se resuelven con funciones de orden superior (`lista.filtrar`, `lista.transformar`), polimorfismo estándar de clases o estructuras de decisión (`elegir ... fin`).

Con estos fundamentos claros, el siguiente paso es dominar la herramienta central de la ejecución dinámica en el motor: **el espectro de evaluación en memoria**.

## ⚙️ 10.2 El Espectro de Ejecución Dinámica (De la Expresión al Sandbox)

Latino.NET ofrece tres mecanismos para ejecutar código dinámicamente en tiempo de ejecución. Aunque superficialmente parecen similares, cada uno opera en un nivel diferente de la jerarquía del motor, posee distintas reglas de aislamiento de memoria y persigue propósitos arquitectónicos concretos:

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│                      NIVELES DE EJECUCIÓN DINÁMICA                               │
├───────────────────────┬──────────────────────────┬───────────────────────────────┤
│ Herramienta           │ Unidad de Ejecución      │ Entorno de Memoria            │
├───────────────────────┼──────────────────────────┼───────────────────────────────┤
│ evaluar() / eval()    │ Expresión única o serie  │ Ámbito actual (comparte y     │
│                       │ con retorno de valor     │ modifica variables activas)   │
├───────────────────────┼──────────────────────────┼───────────────────────────────┤
│ cadena.ejecutar()     │ Bloques y sentencias     │ Ámbito actual (efectos        │
│                       │ completas (retorna nulo) │ secundarios en variables)     │
├───────────────────────┼──────────────────────────┼───────────────────────────────┤
│ archivo.ejecutar()    │ Script completo desde    │ Sandbox aislado (nuevo        │
│                       │ disco (.lat)             │ intérprete independiente)     │
└───────────────────────┴──────────────────────────┴───────────────────────────────┘
```

### 10.2.1 `evaluar(expresion)` y `eval(expresion)`: Evaluación Funcional con Retorno

La función global `evaluar()` (y su alias canónico corto `eval()`) es la herramienta predilecta para la **computación orientada a expresiones**.

#### Mecánica Interna (`RunEval`)

Cuando se invoca `evaluar(cadena)`, el motor activa el método interno `Interpreter.RunEval`:

1. Instancia un `Lexer` y un `Parser` ligeros directamente sobre la cadena de texto.
2. Construye los nodos de sintaxis abstracta (AST) correspondientes.
3. Evalúa las sentencias dentro del **entorno léxico actual** (`_environment`).
4. Si la última instrucción es una expresión, **captura y retorna su valor evaluado** al programa principal. Si el código ejecuta una sentencia `retornar`, captura dicho valor inmediatamente.

```latino
# 1. Evaluación aritmética directa
operacion = "15 * 4 + mate.pot(2, 3)"
resultado = evaluar(operacion)
escribir("Resultado: " .. resultado)  # Imprime: 68

# 2. Acceso y lectura de variables del entorno activo
base = 100
tasa = 0.18
calculo = "base + (base * tasa)"
total = evaluar(calculo)
escribir("Total con impuestos: " .. total)  # Imprime: 118
```

#### Modificación del Entorno y Efectos Secundarios

`evaluar()` no opera en una burbuja hermética. Si la cadena evaluada contiene una asignación (`=`), la variable se creará o modificará en el entorno activo:

```latino
x = 50
evaluar("x = x + 25")
escribir("Nuevo valor de x: " .. x)  # Imprime: 75

# Inyección de una nueva variable en el entorno
evaluar("variable_inyectada = 'Creada dinámicamente'")
escribir(variable_inyectada)  # Imprime: Creada dinámicamente
```

#### Control Defensivo de Errores y Diagnóstico

Dado que el código evaluado frecuentemente proviene de entradas de usuario, APIs externas o bases de datos, debemos distinguir cómo reacciona el motor ante los tres tipos de fallos posibles:

```latino
# 1. Fallo en tiempo de ejecución (Dispara 'atrapar')
expresion_riesgosa = "10 / 0"  # División por cero dinámica

intentar
    valor = evaluar(expresion_riesgosa)
    escribir("Resultado: " .. valor)
atrapar (error)
    escribir("Error capturado en evaluación dinámica: " .. error)
fin

# Salida en pantalla:
# Error capturado en evaluación dinámica: [Línea 1] División por cero.

# 2. Fallo de sintaxis al analizar (Retorna nulo en v1.0.81)
sintaxis_invalida = "10 + * 5"
resultado = evaluar(sintaxis_invalida)

si resultado == nulo
    escribir("La expresión no pudo analizarse (sintaxis inválida).")
fin

# Salida en pantalla:
# La expresión no pudo analizarse (sintaxis inválida).

# 3. Variable no definida (Dispara 'atrapar')
#    No confundir con un fallo de sintaxis: aquí la expresión se ANALIZA,
#    pero al ejecutarla el motor no encuentra el símbolo.
expresion_indefinida = "variableQueNoExiste + 1"
intentar
    valor = evaluar(expresion_indefinida)
    escribir("Resultado: " .. valor)
atrapar (error)
    escribir("Variable no definida en evaluación dinámica: " .. error)
fin

# Salida en pantalla:
# Variable no definida en evaluación dinámica: [Línea 1] Variable no definida: 'variableQueNoExiste'.
```

> 💡 **Nota de Arquitectura sobre `[Línea 1]`:**  
> Al capturar un error de tiempo de ejecución dentro de `evaluar()`, observarás que el mensaje indica `[Línea 1]`. **Esto no es un fallo en el rastreador de líneas de tu script.**  
> Cuando el motor invoca `evaluar()`, genera un flujo de tokens independiente donde el contador de línea inicia en `1` para la cadena que se está analizando en ese instante. Si analizas un texto dinámico multilínea (con saltos `\n`), el diagnóstico reportará con exactitud la línea interna del fragmento donde ocurrió el fallo.

### 10.2.2 `cadena.ejecutar(codigo)`: Ejecución de Sentencias y Bloques

Ubicada en la biblioteca estándar de cadenas, `cadena.ejecutar(codigo)` está diseñada para procesar **código estructurado en bloque**: bucles, condicionales multilínea o definiciones de funciones completas contenidas en un texto.

#### Diferencia Arquitectónica Fundamental con `evaluar()`

A diferencia de `evaluar()`, `cadena.ejecutar()` despacha el código a través de `Interpreter.Interpret()`. En la teoría de compiladores:

- Las **expresiones** producen valores (ej. `2 + 2` produce `4`).
- Las **sentencias** ejecutan acciones y control de flujo, pero **no retornan valores** (ej. un bucle `mientras ... fin` no tiene un valor numérico).

Por esta razón, **`cadena.ejecutar()` siempre retorna `nulo`**. Su utilidad radica exclusivamente en sus **efectos secundarios** (crear funciones, modificar variables o ejecutar lógica procedimental):

```latino
# Generación de una función al vuelo dentro de una cadena
script_dinamico = "funcion SaludoDinamico(nombre)
    escribir('¡Hola ' .. nombre .. ', fui evaluada en caliente!')
fin"

# Analizar y registrar la función en el entorno
cadena.ejecutar(script_dinamico)

# La función ahora existe y es invocable de forma nativa
SaludoDinamico("Carlos")  # Imprime: ¡Hola Carlos, fui evaluada en caliente!
```

```latino
# Ejecución de lógica de bucle dinámica
acumulador = 0
bloque_bucle = "para i en rango(1, 6)
    acumulador = acumulador + i
fin"

cadena.ejecutar(bloque_bucle)
escribir("Acumulador final: " .. acumulador)  # Imprime: 15
```

### 10.2.3 `archivo.ejecutar(ruta)`: Ejecución Aislada (Sandboxing)

Cuando el código dinámico proviene de scripts externos complejos (plugins de usuario, extensiones o macros) y no se desea contaminar el entorno de memoria del programa principal, se utiliza `archivo.ejecutar(ruta)`.

#### Arquitectura del Sandbox

A nivel de código C# (`src/Latino.Core/StdLib/FileLib.cs`), `archivo.ejecutar` implementa una frontera estricta:

1. **Instancia un nuevo `Interpreter`:** Crea un motor de ejecución totalmente limpio en memoria.
2. **Carga la biblioteca estándar:** Invoca `StdLibLoader.LoadAll()` para que el script hijo tenga acceso a `mate`, `cadena`, `lista`, etc.
3. **Aislamiento de Ámbitos:** El entorno global (`Globals`) del script hijo es independiente. Las variables declaradas en el script hijo **mueren al terminar su ejecución y jamás tocan el entorno del programa padre**.
4. **Herencia de Flujos y Seguridad:** El subintérprete hereda automáticamente la salida de texto (`Out`), la entrada estándar (`In`) y el límite de profundidad de recursión (`MaxDepth`) configurado por el padre, garantizando protección contra desbordamiento de pila (*Stack Overflow*).

```latino
# 1. Creamos un script externo de prueba dinámicamente
codigo_tercero = "# Script plugin_usuario.lat
variable_del_hijo = 'Soy un dato aislado'
escribir('   [Plugin] Hola desde el sandbox aislado')
"

archivo.crearCarpeta("plugins")
archivo.escribir("plugins/plugin_usuario.lat", codigo_tercero)

# 2. Programa Principal
global configuracion_secreta = "CLAVE_MAESTRA_123"

escribir("Ejecutando script de terceros en modo seguro...")

# El script corre dentro de un nuevo intérprete aislado (Sandbox)
archivo.ejecutar("plugins/plugin_usuario.lat")

# 3. Demostración de aislamiento (las variables no se contaminan)
# 'variable_del_hijo' no existe en este entorno principal:
escribir("¿Existe variable_del_hijo en el padre? " .. (variable_del_hijo ?? "No existe"))

# Limpieza del archivo temporal
archivo.borrar("plugins")

# Salida en pantalla:
# Ejecutando script de terceros en modo seguro...
#    [Plugin] Hola desde el sandbox aislado
# ¿Existe variable_del_hijo en el padre? No existe
```

### 📊 Comparativa Técnica: ¿Cuál Herramienta Utilizar?

| Criterio | `evaluar()` / `eval()` | `cadena.ejecutar()` | `archivo.ejecutar()` |
| :--- | :--- | :--- | :--- |
| **Entrada esperada** | Cadena (Expresión o serie) | Cadena (Bloques / Sentencias) | Ruta física a archivo `.lat` |
| **Retorno** | Resultado de última expresión | Siempre `nulo` | Siempre `nulo` |
| **Ámbito léxico** | Mismo entorno del invocador | Mismo entorno del invocador | **Sandbox Aislado** (Nuevo intérprete) |
| **Rendimiento** | Ultrarrápido (microsegundos) | Rápido (parsing en memoria) | Medio (I/O de disco + carga StdLib) |
| **Caso de uso ideal** | Fórmulas, reglas matemáticas, DSLs | Inyección de funciones en runtime | Plugins externos, tareas cron aisladas |

Conociendo cómo se evalúa el código en memoria, el siguiente paso es dotar al programa de la capacidad de inspeccionar sus propios tipos y estructuras antes de ejecutarlos: **la introspección y la reflexión**.

## 🔍 10.3 Introspección y Reflexión de Tipos

La **introspección** es la habilidad de un programa para examinar el tipo, las propiedades y el estado de sus objetos en tiempo de ejecución. La **reflexión** da un paso más allá, permitiendo que el programa adapte su comportamiento y modifique la estructura de dichos objetos en pleno vuelo.

En Latino.NET, la combinación de tipado dinámico y despacho polimórfico en C# convierte la introspección en una herramienta arquitectónica de primer orden.

### 10.3.1 `tipo(x)` como Motor de Discriminación en Caliente

La función nativa `tipo(valor)` no es un simple recurso didáctico de depuración; es el **sensor de tipo fundamental del motor**. Inspecciona la propiedad virtual `TypeName` implementada en cada estructura derivada de `LatinoObject`.

#### Catálogo de Tipos Retornados en v1.0.81

| Tipo de Valor Evaluado | Retorno de `tipo(x)` | Ejemplo de Valor |
| :--- | :--- | :--- |
| **Números** | `"decimal"` | `42`, `-3.1416`, `1e5` |
| **Cadenas de texto** | `"cadena"` | `"Hola"`, `'Latino'` |
| **Lógicos / Booleanos** | `"logico"` | `cierto`, `falso` |
| **Nulo** | `"nulo"` | `nulo` |
| **Listas** | `"lista"` | `[1, 2, 3]`, `lista.crear(5)` |
| **Diccionarios** | `"diccionario"` | `{"id": 1, "activo": cierto}` |
| **Tuplas** | `"tupla"` | `(10, "A", cierto)` |
| **Conjuntos** | `"conjunto"` | `conjunto.crear(1, 2, 3)` |
| **Pilas** | `"pila"` | `pila.crear()` |
| **Colas** | `"cola"` | `cola.crear()` |
| **Funciones** | `"funcion"` | `funcion() fin`, `mate.raiz` |
| **Instancias de Clase** | *Nombre de la clase* | `"Cliente"`, `"Factura"`, `"Sensor"` |
| **Definición de Clase POO** | `"desconocido"` ⚠️ | `tipo(Cliente)` *(Peculiaridad de v1.0.81)* |
| **Definición de Función** | `"funcion"` | `tipo(Saludar)` (sin paréntesis) |
| **Excepción capturada** | `"cadena"` | `atrapar (e)` entrega una cadena |
| **`tipo(tipo(x))`** | `"cadena"` | El nombre del tipo es texto a su vez |

> ⚠️ **Nota de Arquitectura (v1.0.81):**  
> Cuando evalúas una **instancia** creada con `nuevo` o `nueva`, `tipo(instancia)` retorna el **nombre nominal de la clase generadora** (`"Cliente"`). Sin embargo, si evalúas la **plantilla de la clase en sí misma** (`tipo(Cliente)`), el motor retornará `"desconocido"` debido a que `LatinoClass.cs` hereda el valor por defecto de la clase base sin sobreescritura *(corrección planificada en el roadmap de la v2.0)*.

---

> ℹ️ **El resultado de `tipo()` es una CADENA, no un descriptor de tipo:**  
>
> A diferencia de C# (donde `x.GetType()` devuelve un objeto `System.Type`) o Python (donde `type(x)` devuelve un objeto `type`), en Latino.NET `tipo(x)` devuelve **texto plano**.
> Por eso `tipo(tipo(x))` responde `"cadena"`: el nombre del tipo es a su vez una cadena, y tiparlo devuelve el tipo de ese texto.  
>
> El valor admite la API de la librería `cadena` —`tipo(42).mayusculas()` devuelve `"DECIMAL"`— y se puede concatenar con `..`. Como con cualquier otro valor, la operación corresponde al módulo de su tipo: `cadena.longitud(tipo(42))` devuelve `7` (los siete caracteres de `"decimal"`), mientras `lista.longitud` solo acepta listas. Verificado ejecutando en v1.0.81.

#### Patrón: Enrutador Polimórfico de Argumentos

Gracias a `tipo()`, es posible construir funciones con sobrecarga dinámica que adaptan su algoritmo según la estructura recibida:

```latino
funcion Serializar(dato)
    t = tipo(dato)
    
    elegir (t)
        caso "cadena":
            retornar '"' .. dato .. '"'
            
        caso "decimal":
            retornar acadena(dato)
            
        caso "logico":
            retornar acadena(dato)
            
        caso "lista":
            elementos = [ ]
            para item en dato
                lista.agregar(elementos, Serializar(item))
            fin
            retornar "[" .. lista.separar(elementos, ", ") .. "]"
            
        caso "diccionario":
            pares = [ ]
            para llave en dic.llaves(dato)
                val_str = Serializar(dato[llave])
                lista.agregar(pares, '"' .. llave .. '": ' .. val_str)
            fin
            retornar "{" .. lista.separar(pares, ", ") .. "}"
            
        otro:
            retornar ç("<Objeto:|t|>")
    fin
fin

registro = {
    "folio": 1045,
    "activo": cierto,
    "etiquetas": ["urgente", "factura"]
}

escribir(Serializar(registro))
# Salida: {"folio": 1045, "activo": cierto, "etiquetas": ["urgente", "factura"]}
```

### 10.3.2 Inspección Estructural de Colecciones en Caliente

En arquitecturas orientadas a servicios (APIs REST, ingesta de archivos JSON o bases de datos SQLite), los datos suelen recibirse como diccionarios con esquemas inciertos. La introspección permite auditar su contenido antes de procesarlos.

#### Métodos Clave de Inspección de Diccionarios

- `dic.contiene(d, clave)`: Retorna `cierto` si la llave existe, evitando excepciones en runtime.
- `dic.llaves(d)`: Extrae una `lista` con todas las claves registradas para iterar o auditar.
- `dic.valores(d)`: Extrae la lista de todos los valores almacenados.

#### Patrón: Validador Dinámico de Esquemas (*Schema Guard*)

Podemos escribir un contrato que valide si un objeto dinámico cumple con los tipos requeridos antes de persistirlo en disco o base de datos:

```latino
funcion ValidarEsquema(datos, esquemaEsperado)
    para campo en dic.llaves(esquemaEsperado)
        # 1. Verificar presencia de la propiedad
        si no dic.contiene(datos, campo)
            lanzar(ç("Falta el campo obligatorio: '|campo|'"))
        fin
        
        # 2. Verificar correspondencia estricta de tipo
        tipoReal = tipo(datos[campo])
        tipoRequerido = esquemaEsperado[campo]
        
        si tipoReal != tipoRequerido
            lanzar(ç("Campo '|campo|' inválido. Esperado: |tipoRequerido|, Recibido: |tipoReal|"))
        fin
    fin
    retornar cierto
fin

# Definición del esquema estricto
ESQUEMA_USUARIO = {
    "nombre": "cadena",
    "edad": "decimal",
    "activo": "logico"
}

# Carga de datos dinámica
peticion_valida = { "nombre": "Lucía", "edad": 29, "activo": cierto }
peticion_invalida = { "nombre": "Marcos", "edad": "treinta", "activo": cierto }

intentar
    ValidarEsquema(peticion_valida, ESQUEMA_USUARIO)
    escribir("✅ Petición 1 aprobada")
    
    ValidarEsquema(peticion_invalida, ESQUEMA_USUARIO)
atrapar (error)
    escribir("❌ Fallo de validación: " .. error)
    # Salida: Fallo de validación: Campo 'edad' inválido. Esperado: decimal, Recibido: cadena
fin
```

### 10.3.3 Mutabilidad Dinámica de Estado en POO (*Property Bags*)

En Latino.NET, las instancias de clases creadas con `nuevo` o `nueva` no poseen una estructura de memoria sellada en C#.

#### Arquitectura Interna de `LatinoInstance.cs`

Cada instancia almacena su estado en un diccionario interno de C#:

```csharp
private readonly Dictionary<string, LatinoObject> _fields = new();
```

Cuando se ejecuta una lectura `instancia.campo`:

1. El motor busca primero en `_fields`.
2. Si no existe, busca el método correspondiente en `_klass`.
3. Si tampoco existe, retorna silenciosamente `LatinoNull.Instance` (no lanza error).

Cuando se ejecuta una asignación `instancia.campo = valor`:

1. El motor inyecta o actualiza la clave en `_fields`.

#### Adición Dinámica de Campos en Tiempo de Ejecución

Esto permite utilizar el patrón **Bolsa de Propiedades (*Property Bag*)**: extender objetos con metadatos contextuales sin tener que modificar la clase base:

```latino
clase RegistroVenta
    constructor(folio, monto)
        mi.folio = folio
        mi.monto = monto
    fin
fin

# Instancia estándar
venta = nueva RegistroVenta("FAC-001", 1500)
escribir(ç("Venta: |venta.folio| - Subtotal: $|venta.monto|"))

# Inyección reflexiva de nuevos campos en caliente (sin tocar la clase)
venta.fechaHora = "2026-09-19 20:30"
venta.autorizadoPor = "admin_finanzas"
venta.tasaDescuento = 0.10

escribir("Metadatos inyectados dinámicamente:")
escribir("  Autorización: " .. venta.autorizadoPor)
escribir("  Fecha: " .. venta.fechaHora)
```

#### Objetos Malleables con Diccionarios y Clausuras

Si un problema requiere objetos con comportamiento dinámico pero sin la ceremonia de declarar una `clase ... fin`, se pueden componer tablas de funciones utilizando diccionarios literales y clausuras léxicas:

```latino
funcion CrearServicioNotificaciones(canal)
    # Variables privadas capturadas por clausura
    local prefijo = "[" .. cadena.mayusculas(canal) .. "]"
    
    retornar {
        "canal": canal,
        "enviar": funcion(mensaje)
            escribir(prefijo .. " " .. mensaje)
        fin
    }
fin

notificadorSMS = CrearServicioNotificaciones("sms")
notificadorEmail = CrearServicioNotificaciones("email")

# Invocación dinámica a través de la tabla de despacho
notificadorSMS["enviar"]("Código de seguridad: 4920")
notificadorEmail["enviar"]("Su factura mensual está lista")

# Salida:
# [SMS] Código de seguridad: 4920
# [EMAIL] Su factura mensual está lista
```

Con las capacidades de introspección y adaptación de objetos dominadas, el siguiente nivel es la **síntesis activa de código**: utilizar plantillas para generar expresiones de Latino.NET y ejecutarlas en memoria.

## 🧩 10.4 Síntesis Dinámica de Código: La Unión de `ç()` y `evaluar()`

En los capítulos anteriores conocimos al **Compositor de Cadenas `ç()`** como un interpolador estructural diseñado para presentar datos en pantalla de forma limpia. Sin embargo, en el ámbito de la metaprogramación, `ç()` asume un rol mucho más potente: se convierte en el **motor de plantillas de código fuente (*Code Template Engine*)** de Latino.NET.

Al combinar la capacidad de ensamblado estructural de `ç()` con la capacidad de análisis en caliente de `evaluar()`, es posible sintetizar y ejecutar algoritmos completos sobre la marcha.

### 10.4.1 La Regla de Oro de la Síntesis con `ç()`

Para generar código de forma idiomática y sin errores, debemos recordar la **Invariante Técnica #5 del motor**:

- **Dentro de `| |`:** Únicamente se permite navegación estructural directa de variables simples (`|nombre|`), propiedades (`|obj.campo|`) e índices (`|arr[0]|`).
- **Prohibido dentro de `| |`:** Operaciones aritméticas (`|a + b|`) o llamadas a funciones (`|mate.raiz(x)|`).

Esta restricción, lejos de ser una limitación, encaja de forma natural con la síntesis de código:

```text
┌───────────────────────────────────────────────────────────────────────────┐
│                           PIPELINE DE SÍNTESIS                            │
├───────────────────────────────────────────────────────────────────────────┤
│  1. Variables con metadatos:    campo = "precio", op = ">=", umbral = 100 │
│                                                                           │
│  2. Plantilla con ç():          ç("producto.|campo| |op| |umbral|")       │
│     (Resuelve variables         │                                         │
│      dentro de | |)             ▼                                         │
│                                 "producto.precio >= 100"                  │
│                                                                           │
│  3. Análisis con evaluar():  evaluar("producto.precio >= 100")            │
│     (Tokeniza, parsea y         │                                         │
│      resuelve la aritmética)    ▼                                         │
│                                 cierto / falso                            │
└───────────────────────────────────────────────────────────────────────────┘
```

El Compositor `ç()` no resuelve la lógica; se limita a **ensamblar el texto de la instrucción**. Es `evaluar()` quien toma ese texto, lo convierte en nodos del AST (en este caso, un `BinaryExpression(TokenType.GreaterEqual)`) y ejecuta la operación en memoria.

### 10.4.2 Patrón: Constructor Dinámico de Expresiones (*Expression Builder*)

Imagina un sistema de consulta donde los filtros no están prefijados en el código fuente, sino que un usuario selecciona campos, operadores relacionales y valores límite desde una interfaz gráfica o un archivo de configuración.

En lugar de escribir decenas de bloques `si / sino` para cada combinación posible, construimos y evaluamos la expresión directamente:

```latino
# Objeto de negocio activo en el entorno
producto = {
    "nombre": "Monitor 4K",
    "precio": 350,
    "stock": 12,
    "categoria": "electronica"
}

funcion EvaluarCondicion(objeto, campo, operador, valorLimite)
    # ✅ FIX: Declaramos la variable local antes de entrar al 'si'
    local plantilla = ""
    
    si tipo(valorLimite) == "cadena"
        plantilla = ç("objeto['|campo|'] |operador| '|valorLimite|'")
    sino
        plantilla = ç("objeto['|campo|'] |operador| |valorLimite|")
    fin
    
    escribir("Código generado: " .. plantilla)
    retornar evaluar(plantilla)
fin

# Pruebas dinámicas
c1 = EvaluarCondicion(producto, "precio", ">=", 300)
escribir("¿Cumple condición de precio? " .. c1)

c2 = EvaluarCondicion(producto, "stock", "<", 5)
escribir("¿Requiere reabastecimiento urgente? " .. c2)

c3 = EvaluarCondicion(producto, "categoria", "==", "electronica")
escribir("¿Es del departamento correcto? " .. c3)
```

### 10.4.3 Síntesis de Funciones en Caliente (*Lambdas Sintéticas*)

La metaprogramación no se limita a evaluar expresiones escalares; permite **fabricar nuevas funciones anónimas** basadas en parámetros que solo se conocen en tiempo de ejecución.

Al ensamblar la definición de una función dentro de una cadena y pasarla a `evaluar()`, el motor genera un nodo `AnonymousFunctionExpression` y retorna un objeto `LatinoFunction` de pleno derecho, capaz de ser invocado o pasado como argumento a tuberías (`|>`):

```latino
funcion FabricarEscalador(multiplicador, offset)
    # Sintetizamos la declaración de una función anónima
    codigoFuncion = ç("(funcion(x)
        retornar (x * |multiplicador|) + |offset|
    fin)")
    
    # Analizar la función en memoria
    retornar evaluar(codigoFuncion)
fin

# Generamos dos transformadores con lógica fija interna
convertirCelsiusAFahrenheit = FabricarEscalador(1.8, 32)
calcularPrecioConMargen     = FabricarEscalador(1.15, 5)

# Invocación directa de las funciones generadas
tempF = convertirCelsiusAFahrenheit(20)
precioFinal = calcularPrecioConMargen(100)

escribir("20°C en Fahrenheit: " .. tempF)         # Imprime: 68
escribir("Precio con margen ($): " .. precioFinal) # Imprime: 120
```

> 💡 **Nota de Arquitectura: Funciones Anónimas dentro de `evaluar()`**  
> Cuando sintetices funciones anónimas directas con `evaluar()`, debes envolver la declaración entre paréntesis: `evaluar("(fun(x) ret x * 2 fin)")`.  
> Si omites los paréntesis, el parser interpretará que la instrucción es una declaración formal de función y esperará un nombre (`funcion MiNombre()`), provocando un fallo de análisis sintáctico. Los paréntesis le indican al parser que debe tratar la definición como una expresión de valor.

### 10.4.4 Integración con el Operador Tubería (`|>`)

Las funciones sintetizadas dinámicamente son compatibles con el operador Pipeline (`|>`). Esto permite insertar filtros o transformaciones generadas en caliente dentro de un flujo funcional de datos:

```latino
datos = [ 10, 20, 30, 40, 50, 60, 70, 80 ]

# Un parámetro leído de configuración externa
umbral_corte = 35

# Generamos dinámicamente el predicado de filtrado
predicado = evaluar(ç("(fun(n) ret n > |umbral_corte| fin)"))

# El pipeline ejecuta la lambda sintetizada sin ninguna fricción
resultado = datos
    |> lista.filtrar(predicado)
    |> lista.promedio()

escribir("Promedio de datos filtrados dinámicamente: " .. resultado)
# Salida: Promedio de datos filtrados dinámicamente: 60
```

Con la capacidad de ensamblar y evaluar código mediante plantillas, estamos listos para estudiar **casos de uso de nivel industrial**: motores de reglas de negocio conectados a bases de datos y analizadores de fórmulas de usuario.

## 🏭 10.5 Casos Prácticos de Metaprogramación Industrial

Para apreciar el verdadero alcance de la metaprogramación, debemos salir de los ejemplos de juguete y examinar problemas reales de ingeniería de software. A continuación, exploraremos tres arquitecturas industriales donde la evaluación dinámica y la introspección resuelven problemas que con código estático serían rígidos y difíciles de mantener.

### 10.5.1 Caso 1: Motor de Reglas de Negocio en Base de Datos (SQLite + `evaluar`)

**El Problema:**  
En aplicaciones bancarias, de comercio electrónico o seguros, las políticas de negocio (autorización de créditos, reglas de descuento, alertas de fraude) cambian constantemente. Si las reglas están cableadas en el código fuente con cadenas de `si / sino`, cada cambio comercial exige modificar el código, compilar, hacer pruebas de regresión y desplegar un nuevo release.

**La Solución Metaprogramática:**  
Almacenar las condiciones como expresiones de texto en una tabla SQLite. El motor consulta las reglas activas de la base de datos y las evalúa en tiempo real contra los objetos de negocio activos en memoria:

```latino
incluir("sqlite")

# 1. Inicialización de la base de datos de políticas
db = sqlite.conectar("politicas_credito.db")

sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS politicas (
    id INTEGER PRIMARY KEY,
    nombre TEXT NOT NULL,
    condicion TEXT NOT NULL,
    tasa_aprobada REAL NOT NULL
)")

# Limpiar tabla previa e insertar reglas comerciales dinámicas
sqlite.ejecutar(db, "DELETE FROM politicas")

sqlite.ejecutar(db, "INSERT INTO politicas (nombre, condicion, tasa_aprobada) VALUES 
    ('Cliente Preferencial', 'solicitud.score >= 750 ý solicitud.ingresoMensual > 4000', 0.085),
    ('Cliente Estándar',     'solicitud.score >= 650 ý solicitud.antiguedadLaboralMeses >= 12', 0.125),
    ('Riesgo Alto con Aval', 'solicitud.score < 650 ý solicitud.tieneGarante == cierto', 0.160)")

# 2. Entidad de negocio recibida en memoria (solicitud actual)
solicitud = {
    "cliente": "Roberto Méndez",
    "score": 780,
    "ingresoMensual": 4500,
    "antiguedadLaboralMeses": 36,
    "tieneGarante": falso
}

escribir("Evaluando solicitud de crédito para: " .. solicitud.cliente)

# 3. Consulta y evaluación dinámica en caliente
politicas = sqlite.consultar(db, "SELECT * FROM politicas ORDER BY id ASC")
creditoAprobado = falso

para regla en politicas
    # La variable 'solicitud' está en el ámbito activo.
    # evaluar() analiza la condición de la base de datos y la resuelve directamente.
    cumple = evaluar(regla["condicion"])
    
    si cumple
        escribir("✅ APROBADO bajo la política: " .. regla["nombre"])
        escribirf("   Tasa de interés asignada: {0:P2}\n", regla["tasa_aprobada"])
        creditoAprobado = cierto
        romper # Política de mejor tasa asignada
    fin
fin

si no creditoAprobado
    escribir("❌ RECHAZADO: La solicitud no cumplió ninguna política de crédito vigente.")
fin

sqlite.cerrar(db)

# Higiene: cerrar la conexión NO borra el archivo .db del disco
archivo.borrar("politicas_credito.db")
```

**Beneficio de Arquitectura:**  
El equipo de riesgos puede agregar, modificar o apagar políticas comerciales directamente en la base de datos SQL sin necesidad de reiniciar el servidor ni recompilar una sola línea de código de Latino.NET.

### 10.5.2 Caso 2: Evaluador y Tabulador de Fórmulas Matemáticas de Usuario

**El Problema:**  
En software científico, simuladores o herramientas de análisis de ingeniería, el usuario final necesita ingresar funciones matemáticas arbitrarias (como $f(x) = \sin(x) \cdot e^{-0.2x}$) para generar reportes, calcular raíces o graficar curvas. Crear un analizador sintáctico matemático manual desde cero en cada proyecto requiere cientos de líneas de código y suele ser lento.

**La Solución Metaprogramática:**  
Delegar el análisis léxico y sintáctico al motor interno de Latino.NET a través de `evaluar()`. El usuario ingresa la fórmula como texto; el programa la valida y genera una tabla de valores en microsegundos:

```latino
escribir("=============================================")
escribir("  TABULADOR CIENTÍFICO DE FUNCIONES f(x)")
escribir("=============================================")

# Fórmula ingresada por el usuario o leída de un archivo de parámetros
# En un programa interactivo usarías: formulaUsuario = pedir("Ingrese f(x): ")
formulaUsuario = "mate.sen(x) * mate.exp(-0.2 * x)"

escribir("Función cargada: f(x) = " .. formulaUsuario)
escribir("")

# Parámetros del barrido
x_inicio = 0.0
x_fin    = 5.0
paso     = 0.5

# Encabezado formateado
escribirf("{0,-8} | {1,-15}\n", "x", "f(x)")
escribir("-------------------------")

# Validación previa de la fórmula con x=0 antes de entrar al bucle.
#
# ⚠️ `evaluar()` falla de TRES maneras distintas:
#    · error de SINTAXIS   ("2 +")       -> retorna nulo en SILENCIO, no lanza
#    · variable INDEFINIDA ("foo + 1")  -> lanza excepción capturable
#    · error de RUNTIME    ("10 / 0")   -> lanza excepción capturable
#
# ⚠️ Y `intentar/atrapar` crea su propio ámbito: una variable declarada DENTRO
#    no existe fuera. Por eso `test` se inicializa antes del bloque.
test = nulo
intentar
    x = 0
    test = evaluar(formulaUsuario)
atrapar (e)
    escribir("❌ La fórmula no se puede evaluar: " .. e)
    sis.salir()
fin

# El error de sintaxis no lanza excepción: hay que comprobar el retorno nulo.
si test == nulo
    escribir("❌ La fórmula contiene errores de sintaxis.")
    sis.salir()
fin

# Bucle de tabulación numérica
x_actual = x_inicio
mientras (x_actual <= x_fin)
    # Colocamos 'x' en el ámbito léxico activo
    x = x_actual
    
    # evaluar() resuelve la expresión matemática completa en el entorno
    y = evaluar(formulaUsuario)
    
    escribirf("{0,-8:F2} | {1,-15:F5}\n", x_actual, y)
    x_actual = x_actual + paso
fin

escribir("-------------------------")
escribir("Tabulación completada con éxito.")
```

**Salida en consola:**

```text
=============================================
  TABULADOR CIENTÍFICO DE FUNCIONES f(x)
=============================================
Función cargada: f(x) = mate.sen(x) * mate.exp(-0.2 * x)

x        | f(x)           
-------------------------
0.00     | 0.00000        
0.50     | 0.43380        
1.00     | 0.68894        
1.50     | 0.73896        
2.00     | 0.60952        
2.50     | 0.36299        
3.00     | 0.07745        
3.50     | -0.17419       
4.00     | -0.34005       
4.50     | -0.39743       
5.00     | -0.35277       
-------------------------
Tabulación completada con éxito.
```

### 10.5.3 Caso 3: Despacho Dinámico de Comandos (*Dynamic Action Dispatcher*)

**El Problema:**  
En herramientas CLI, bots de mensajería o controladores de APIs, se reciben peticiones como cadenas de texto (`"iniciar"`, `"pausar"`, `"detener"`, `"reiniciar"`). La aproximación tradicional consiste en gigantescas estructuras `elegir / caso` o decenas de bloques `si / osi / sino`, que crecen descontroladamente y violan el principio de responsabilidad única.

**La Solución Metaprogramática:**  
Construir el identificador de la acción dinámicamente y despachar la llamada a través de una tabla de funciones o mediante invocación reflexiva:

```latino
# 1. Definición de servicios y operaciones disponibles
ControladorServidor = {
    "accion_iniciar": funcion(parametros)
        escribir("🚀 Iniciando servicios en puerto: " .. (parametros["puerto"] ?? 8080))
    fin,
    
    "accion_pausar": funcion(parametros)
        escribir("⏸️ Pausando tráfico entrante...")
    fin,
    
    "accion_detener": funcion(parametros)
        escribir("🛑 Deteniendo servicios y cerrando sockets de red.")
    fin
}

# 2. Despachador genérico reflexivo
funcion DespacharPeticion(controlador, nombreComando, parametros)
    nombreMetodo = "accion_" .. cadena.minusculas(nombreComando)
    
    # Validamos si la acción existe en la tabla mediante introspección
    si no dic.contiene(controlador, nombreMetodo)
        escribir(ç("⚠️ Comando desconocido: '|nombreComando|'. Acción ignorada."))
        retornar falso
    fin
    
    # Invocamos la función directamente desde la tabla de despacho
    funcionAccion = controlador[nombreMetodo]
    funcionAccion(parametros)
    retornar cierto
fin

# 3. Simulación de eventos recibidos externamente
DespacharPeticion(ControladorServidor, "iniciar", {"puerto": 9000})
DespacharPeticion(ControladorServidor, "pausar", {})
DespacharPeticion(ControladorServidor, "destruir", {}) # Comando no registrado
DespacharPeticion(ControladorServidor, "detener", {})

# Salida:
# 🚀 Iniciando servicios en puerto: 9000
# ⏸️ Pausando tráfico entrante...
# ⚠️ Comando desconocido: 'destruir'. Acción ignorada.
# 🛑 Deteniendo servicios y cerrando sockets de red.
```

Como hemos visto, la metaprogramación transforma problemas complejos en soluciones concisas y desacopladas. Sin embargo, evaluar código en caliente conlleva riesgos inherentes de estabilidad y seguridad que todo desarrollador debe saber gobernar: **el blindaje y las defensas contra el abuso de `eval`**.

## 🛡️ 10.6 Blindaje, Seguridad y Rendimiento: "El Lado Oscuro de Eval"

En la ingeniería de lenguajes de programación existe una advertencia universal compartida por JavaScript, Python, Ruby y Lisp: **"Eval is Evil" (La evaluación dinámica puede ser peligrosa)**.

Ejecutar cadenas de texto en tiempo de ejecución otorga un poder formidable, pero si se utiliza sin disciplina arquitectónica, abre dos brechas críticas: **vulnerabilidades de inyección de código (*Code Injection*)** y **degradación del rendimiento**.

En esta sección aprenderás las técnicas defensivas de Latino.NET para blindar tus aplicaciones y la matriz de decisión para saber cuándo metaprogramar y cuándo abstenerse.

### 10.6.1 El Peligro de la Inyección de Código (*Code Injection*)

Cuando utilizas `evaluar(texto)`, el motor de Latino.NET no distingue si la cadena fue escrita por ti o por un atacante: **la ejecuta con todos los privilegios del proceso del sistema operativo**.

Imagina un servicio web que recibe una fórmula matemática de un usuario y la ejecuta a ciegas:

```latino
# ❌ CÓDIGO PELIGROSO (Vulnerable a Inyección de Código)
formulaUsuario = pedir("Introduce tu fórmula: ")
# Si el usuario introduce: "2 + 2" -> Funciona.
# Pero si un atacante introduce: "2 + 2; archivo.borrar('datos_criticos.db'); 0"
# O introduce: "sis.salir()"
resultado = evaluar(formulaUsuario) 
```

#### Técnica Defensiva: Validación con Lista Blanca (*Whitelisting*)

Jamás debes evaluar texto proveniente de fuentes no confiables (entradas HTTP, archivos subidos por clientes o formularios) sin **sanitizarlo previamente mediante expresiones regulares**.

> ⚠️ **Trampa común: las clases de caracteres NO validan palabras.** En una regex, `[...]` es un conjunto de **caracteres individuales**, no de palabras. Escribir `"[... mate.sen cos tan ...]"` no autoriza la función `mate.sen`: autoriza cualquier combinación arbitraria de las letras `m`, `a`, `t`, `e`, `.`, `s`, `n`, `c`, `o`, `r`, `z`, `p`, `i`… Veamos qué acepta un patrón así:
>
> | Entrada | ¿Pasa el filtro? | ¿Debería? |
> | :--- | :--- | :--- |
> | `x + 1` | ✅ acepta | ✅ sí |
> | `mate.sen(x) * 2` | ✅ acepta | ✅ sí |
> | `si x` | ⚠️ **acepta** | ❌ no: es una palabra clave del lenguaje |
> | `zzz` | ⚠️ **acepta** | ❌ no: no es nada |
> | `m.a.t.e.s.e.n` | ⚠️ **acepta** | ❌ no: no es una llamada a función |
> | `x; sis.salir()` | ✅ rechaza | ✅ correcto (por el `;`) |
>
> El resultado no es una lista blanca: es un alfabeto de letras sueltas. Para autorizar **identificadores completos** hay que usar **alternancia de grupos** `(?:a|b|c)`, que exige que la secuencia entera coincida.

**La versión correcta** usa un grupo no capturante por token, exigiendo que cada elemento sea un token completo:

```latino
funcion EvaluarMatematicaSegura(formulaTexto, xValor)
    # Lista blanca con ALTERNANCIA ESTRICTA por token.
    # Cada alternativa debe ser un token COMPLETO, no letras sueltas:
    #   (?:mate\.)?     → prefijo opcional de la librería
    #   (?:sen|cos|...) → una sola función, escrita entera
    patronSeguro = "^(?:[0-9]|[+\\-*/^(),. ]|x|(?:mate\\.)?(?:sen|cos|tan|exp|pot|raiz|pi|e))+$"

    si no (formulaTexto ~= patronSeguro)
        lanzar("Violación de seguridad: La fórmula contiene identificadores o caracteres prohibidos.")
    fin

    # Exponer únicamente la variable necesaria en el ámbito
    x = xValor

    # Evaluación envuelta en bloque defensivo
    intentar
        retornar evaluar(formulaTexto)
    atrapar (error)
        lanzar("Error de sintaxis en fórmula matemática: " .. error)
    fin
fin
```

**Salida verificada** del filtro (ejecutado con `latino whitelista.lat`):

```text
ACEPTADAS (todas legítimas):
  OK       x + 1
  OK       mate.sen(x) * 2
  OK       exp(x)
  OK       2 * pi
  OK       raiz(x^2)
  OK       cos(x) + sen(x)

RECHAZADAS (todas maliciosas o inválidas):
  OK (bloqueado)  x + 1; sis.salir()
  OK (bloqueado)  si x
  OK (bloqueado)  zzz
  OK (bloqueado)  m.a.t.e.s.e.n
  OK (bloqueado)  f()
  OK (bloqueado)  miVar
  OK (bloqueado)  secretx
  OK (bloqueado)  archivo.leer
```

```latino
# Prueba segura
intentar
    res1 = EvaluarMatematicaSegura("mate.sen(x) * 2", 1.5)
    escribir("Resultado seguro: " .. res1)

    # Intento malicioso de inyección
    res2 = EvaluarMatematicaSegura("x + 1; sis.salir()", 1.5)
atrapar (alerta)
    escribir("🛡️ Ataque neutralizado: " .. alerta)
fin

# Salida:
# Resultado seguro: 1.994989973208109
# 🛡️ Ataque neutralizado: [Línea 9] Violación de seguridad: La fórmula contiene identificadores o caracteres prohibidos.
```

> 💡 **Una lista blanca nunca sustituye al principio de mínimo privilegio.** Este regex filtra la *forma* del texto, no su *significado*. Para confiar de verdad en la entrada, combina tres capas:
>
> 1. **Lista blanca de tokens** (este ejemplo) para rechazar lo evidente.
> 2. **Lista negra de palabras clave del lenguaje** — `si`, `sino`, `mientras`, `para`, `funcion`, `retornar`, `lanzar` — porque una lista blanca por caracteres nunca los detecta.
> 3. **Entorno aislado**: evalúa en un proceso aparte, con un timeout y sin acceso a archivos ni red.
>
> Para expresiones matemáticas, la opción más segura no es una regex: es **parsear la fórmula token a token** y construir un árbol de evaluación propio, donde solo se acepten operandos numéricos y operadores conocidos por construcción.

### 10.6.2 Blindaje de Pila y Recursos con `sis.config()`

Un riesgo recurrente al analizar código en caliente es la **recursión infinita o DoS (Denegación de Servicio)**. Un script dinámico malicioso o defectuoso podría intentar agotar la memoria de la máquina virtual llamándose recursivamente sin caso base.

Latino.NET incorpora en su núcleo controles de protección mediante `sis.config()`:

```latino
# 1. Limitar la profundidad de recursión máxima de la pila (Default: 100)
# Si una función dinámica supera este límite, el motor detiene la ejecución
# con un RuntimeError atrapable antes de sufrir un Stack Overflow en C#.
sis.config("max_stack", 25)

# 2. Configurar el tiempo límite para operaciones de expresiones regulares (Default: 2.0s)
# Protege contra ataques ReDoS (Regular Expression Denial of Service)
sis.config("regex_timeout", 1.0)

codigoPeligroso = "funcion Bomba(n)
    retornar Bomba(n + 1)
fin
Bomba(1)"

intentar
    cadena.ejecutar(codigoPeligroso)
atrapar (errorPila)
    escribir("🛡️ Pila protegida por el motor: " .. errorPila)
    # Salida: 🛡️ Pila protegida por el motor: [Línea 16] Error al ejecutar cadena: Límite de recursividad excedido (25).
fin
```

### 10.6.3 Rendimiento: Analizar en Bucle vs. Sintetizar una Lambda

Cada invocación a `evaluar()` o `cadena.ejecutar()` involucra:

1. Instanciación del `Lexer` y tokenización de la cadena.
2. Construcción de los nodos del árbol sintáctico en el `Parser`.
3. Recorrido del AST por el `Interpreter`.

Si ejecutas `evaluar("expresion")` un millón de veces dentro de un bucle `mientras` o `para`, estarás obligando al motor a **re-tokenizar y re-parsear el mismo texto un millón de veces**.

❌ **Antipatrón: Evaluación dinámica dentro de bucles masivos**

```latino
# ❌ Ineficiente: Reconstruye y parsea el AST en cada una de las 10,000 iteraciones
para x en rango(10000)
    y = evaluar("x * 2") 
fin
```

✅ **Patrón Idiomático: Analizar una Sola Vez, Ejecutar Múltiples Veces (Rápido)**

Sintetiza una función anónima (lambda) **una sola vez fuera del bucle**, guarda la referencia en una variable y luego invócala a velocidad de ejecución pura de máquina:

🎯 **Ejemplo: Análisis previo de funciones para ejecución a velocidad nativa**

```latino
# ✅ Óptimo: Parsea una sola vez fuera del bucle como lambda
funcionMultiplicadora = evaluar("(funcion(x) retornar x * 2 fin)")

y = 0
para x en rango(10000)
    y = funcionMultiplicadora(x)
fin

escribir("Resultado: " .. y)
```

### 📊 Matriz de Decisión Arquitectónica: ¿Metaprogramar o no?

Antes de recurrir a `evaluar()` o `cadena.ejecutar()`, consulta esta matriz de ingeniería:

| Escenario | ¿Usar Metaprogramación? | Alternativa Idiomática Recomendada |
| :--- | :--- | :--- |
| **Fórmulas dinámicas de usuario** | ✅ **SÍ (`evaluar`)** | Sanitizar previamente con expresiones regulares. |
| **Reglas de negocio desde SQLite / JSON** | ✅ **SÍ (`evaluar`)** | Aislar las variables expuestas en un diccionario. |
| **Plugins de terceros en disco** | ✅ **SÍ (`archivo.ejecutar`)** | Correr en sandbox aislado para no contaminar variables. |
| **Mapear/Filtrar colecciones de datos** | ❌ **NO** | Usar funciones de orden superior: `lista.transformar()` y `lista.filtrar()`. |
| **Elegir entre 5 o 10 acciones conocidas** | ❌ **NO** | Usar tablas de despacho en diccionarios: `controlador["accion"](args)`. |
| **Calcular expresiones conocidas** | ❌ **NO** | Escribir la matemática directamente en Latino: `y = mate.sen(x) * 2`. |
| **Transformar texto con variables** | ❌ **NO** | Usar el Compositor `ç("Hola \|nombre\|")` directamente. |

## 📝 resumen del capítulo

La metaprogramación en Latino.NET v1.0.81 demuestra la madurez del motor:

1. **`tipo()`** ofrece introspección en caliente tanto para tipos primitivos (`"decimal"`, `"cadena"`, `"diccionario"`) como para clases de POO (devolviendo el nombre nominal de la clase generadora).
2. **`evaluar()` / `eval()`** evalúa expresiones en tiempo real dentro del entorno activo y retorna el resultado de la última expresión calculada.
3. **`cadena.ejecutar()`** analiza bloques de sentencias procedimentales y definiciones de funciones contenidas en texto, retornando siempre `nulo`.
4. **`archivo.ejecutar()`** ofrece un sandbox limpio que ejecuta scripts externos en una nueva instancia del intérprete sin contaminar la memoria del programa padre.
5. **El Compositor `ç()`** funciona como un motor de plantillas de código para sintetizar lógica dinámica sin la maraña de concatenaciones manuales con `..`.
6. **Seguridad primero:** Toda evaluación dinámica debe blindarse con validación de listas blancas, control de pila (`max_stack`) y captura defensiva de excepciones (`intentar ... atrapar`).
