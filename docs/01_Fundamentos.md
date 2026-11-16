# CAPÍTULO 1: Fundamentos

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Estudiantes, desarrolladores que inician en Latino.NET y profesionales que buscan dominar las bases léxicas, semánticas y de memoria del lenguaje.  
**Propósito:** Establecer los cimientos del pensamiento computacional en español: entrada y salida por consola, tipos de datos primitivos, variables con tipado dinámico seguro, operadores aritmético-lógicos y el compositor estructural de cadenas `ç()`.

## El Diálogo Primordial con la Máquina

Todo sistema informático, desde el script de automatización más elemental hasta la plataforma en la nube más compleja, se reduce en su núcleo a una conversación estructurada: el ser humano define datos, instruye transformaciones y la máquina entrega resultados observables.

Programar no consiste en memorizar fórmulas crípticas; es el arte de comunicar intenciones **con absoluta precisión** a un procesador que opera a velocidades extraordinarias pero que carece de intuición. Cuando esa comunicación ocurre en un idioma extranjero, el desarrollador gasta la mitad de su energía cognitiva traduciendo comandos mentales en lugar de perfeccionar el algoritmo.

Al escribir en Latino.NET, esa fricción desaparece: el código fluye con la misma claridad con la que explicas una idea a un colega, permitiéndote estructurar el pensamiento abstracto con naturalidad inmediata.

### Los Primeros Ladrillos de la Construcción

Antes de abordar arquitecturas de software complejas, es imprescindible dominar con rigor artesanal los elementos atómicos que dan vida a cualquier programa:

1. **La Salida y Entrada de Información (I/O):** La voz y el oído del software. Aprenderás a emitir mensajes formateados con precisión milimétrica (`escribir`, `escribirf`) y a capturar la interacción del usuario (`pedir`, `leer`).
2. **El ADN de la Información (Tipos Primitivos):** Cómo entiende la memoria los números de punto flotante de doble precisión, las cadenas de texto con soporte completo de grafemas Unicode (`ñ`, tildes, emojis), los valores de verdad (`cierto`, `falso`) y la ausencia deliberada de valor (`nulo`).
3. **Variables y Contenedores con Identidad:** Cómo asignar nombres limpios, comprensibles y autodocumentados a tus datos, evitando colisiones en la memoria y blindando constantes de configuración en mayúsculas sostenidas.
4. **Operadores y el Compositor `ç()`:** El arsenal de herramientas para calcular, concatenar sin ambigüedades (`..`), tomar decisiones mediante cortocircuitos lógicos acentuados (`ý`, `ó`) e interpolar estructuras complejas sin la maraña de comillas manuales.

### Dinamismo sin Caos: La Seguridad de Tipos en Latino.NET

A diferencia de lenguajes de scripting que permiten operaciones absurdas en silencio (como sumar texto con números y producir resultados corruptos), Latino.NET implementa un modelo de **tipado dinámico seguro con Smart Casting**:

* Las variables no requieren declaraciones rígidas de tipo; el intérprete infiere la naturaleza del dato al nacer.
* En operaciones matemáticas, el operador `+` es **estrictamente aritmético**: si una cadena representa un valor numérico (`"25"`), el motor lo normaliza; pero si contiene caracteres no numéricos, el sistema se detiene y avisa con claridad antes de propagar cálculos erróneos.
* La concatenación de texto posee su propio operador dedicado (`..`), erradicando de raíz las trampas lógicas clásicas de otros entornos.

> 💡 **Principio de Claridad Expresiva (Mantra de Latino.NET):**  
> *\"El código se lee diez veces más de lo que se escribe; optimiza siempre para la comprensión de quien vendrá después.\"*  
> Nombrar bien una variable, comentar el contexto de una decisión y formatear la salida en pantalla no son adornos cosméticos: son el sello de identidad del desarrollador profesional.

### 1.1 Hola Mundo: Tu Primera Conversación con la Máquina

#### ¿Por qué "Hola, Mundo"?

Desde 1978, cuando el legendario libro *"El Lenguaje de Programación C"* popularizó este ejercicio, **todo programador** empieza escribiendo un programa que simplemente dice "Hola". ¿Por qué?

Porque este ejercicio microscópico valida una cadena enorme de cosas:

1. ✅ **Tu editor de texto funciona** (puedes escribir código)
2. ✅ **El archivo se guardó correctamente** (en el lugar correcto, con el nombre correcto)
3. ✅ **El intérprete está instalado** (Latino.NET está listo)
4. ✅ **El sistema ejecuta el programa** (la máquina entiende tus órdenes)
5. ✅ **La salida aparece en pantalla** (ves el resultado)

Si ves "¡Hola, Mundo!" en tu consola, **ya eres programador**. Todo lo demás es práctica.

#### El Código Más Simple del Mundo

```latino
# mi_primer_script.lat
escribir("¡Hola, Mundo!")
```

**Desglose línea por línea:**

1. **`# mi_primer_script.lat`**  
   Esto es un **comentario** (lo veremos en un momento). La computadora lo ignora. Es una nota para usuarios que dice "este archivo se llama mi_primer_script.lat".

2. **`escribir("¡Hola, Mundo!")`**  
   Esta es una **instrucción**. Veamos sus partes:
   * **`escribir`**: Es el nombre de una **función** (una orden predefinida en Latino.NET)
   * **`()`**: Los paréntesis le dicen al intérprete "ejecuta esta función ahora"
   * **`"¡Hola, Mundo!"`**: Es el **argumento** (el dato que le pasas a la función). Las comillas indican que es texto literal.

**¿Qué hace exactamente?**

Cuando ejecutas este archivo, el intérprete de Latino.NET:

1. Lee el archivo de arriba hacia abajo
2. Ignora la línea del comentario (empieza con `#`)
3. Llega a `escribir("¡Hola, Mundo!")`
4. Busca la función `escribir` en su biblioteca interna
5. Ejecuta esa función pasándole el texto
6. La función envía el texto a la **salida estándar** (tu consola/terminal)
7. Tu pantalla muestra: `¡Hola, Mundo!`
8. El programa termina (no hay más líneas que ejecutar)

#### Tus Primeros Experimentos

Ahora que entiendes el "Hola Mundo", prueba estas variaciones para sentir el control:

* **Experimento 1: Cambiar el Mensaje**

```latino
escribir("Soy el comandante de esta máquina")
```

* **Experimento 2: Múltiples Líneas**

```latino
escribir("Primera línea")
escribir("Segunda línea")
escribir("Tercera línea")
```

💡 Observa que cada `escribir()` agrega un salto de línea automático.

* **Experimento 3: Texto con Emojis**

```latino
escribir("¡Latino funciona! 🚀")
```

💡 Latino.NET entiende Unicode, así que emojis, tildes y eñes funcionan perfectamente.

* **Experimento 4: Tu Primer Error (Intencional)**

```latino
escribir(Hola)  # ❌ Error Runtime: Variable no definida: 'Hola'
```

💡 Esto causará un **error de ejecución** porque el intérprete buscará una variable llamada `Hola` que no existe. **Los errores son parte del aprendizaje.** Léelos con calma, te dicen exactamente qué salió mal.

> ⚠️ **Nota didáctica:** Si escribes varias palabras sueltas como `escribir(Hola sin comillas)`, el analizador sintáctico (parser) arrojará un **Error de Sintaxis** antes de ejecutar el programa, porque espera un paréntesis de cierre `)` después del primer identificador.

### 💻 Consejo para el CLI (`latino -e`)

Al probar código con múltiples argumentos de cadena desde la terminal, usa **comillas simples exteriores** (`latino -e '...'` en Bash/Git Bash) o **archivos `.lat` temporales** para evitar que la shell desarme las comillas internas del código.

### 1.2 Comentarios: Notas para Tu Yo del Futuro

#### ¿Por qué Existen los Comentarios?

Imagina que escribes un programa hoy y funciona perfectamente. Regresas 6 meses después para modificarlo y piensas: *"¿Qué rayos hace esta línea?"*

Los **comentarios** son mensajes que dejas a:

* **Tu yo del futuro** (cuando hayas olvidado por qué hiciste algo)
* **Otros programadores** (colegas que lean tu código)
* **Tú mismo mientras aprendes** (para ir documentando lo que descubres)

El intérprete de Latino.NET **ignora completamente** los comentarios. Son invisibles para la máquina, pero invaluables para los usuarios.

#### Los Tres Tipos de Comentarios

**1. Comentario de Línea con `#`**

Todo lo que escribas después de `#` en una línea se ignora:

```latino
# Este comentario está solo
escribir("Hola")  # Este comentario está al lado del código
```

**2. Comentario de Línea con `//`**

Funciona exactamente igual que `#`. Es cuestión de preferencia personal:

```latino
// Otra forma de comentar
escribir("Mundo")  // También funciona al lado
```

**3. Comentario de Bloque con `/* ... */`**

Para comentarios largos que ocupan varias líneas:

```latino
/*
  Este programa fue creado el 21 de enero de 2026.
  Autor: Tu Nombre
  Propósito: Aprender los fundamentos de Latino.NET
  
  Notas importantes:
  * Este es mi primer script
  * Los comentarios de bloque son útiles para documentación extensa
*/
escribir("Código documentado es código profesional")
```

#### 📝 Trampa Común: Comentarios de Bloque NO se Anidan

Esto **NO** funciona:

```latino
/*
    Inicio del comentario
    /* Intento de meter otro comentario aquí */  <-* ¡El bloque termina aquí!
    Este texto quedará fuera del comentario y causará error
*/
```

**¿Por qué?** El intérprete ve el primer `*/` que encuentra y cierra el comentario inmediatamente.

**Solución:** Si necesitas desactivar código que ya tiene comentarios, usa `#` o `//` al inicio de cada línea:

```latino
# /*
#     Este código está desactivado
#     /* Y este comentario interno está seguro */
# */
```

#### Buenas Prácticas de Comentarios

* **✅ Comenta el "Por Qué", No el "Qué"**

**Malo (Obvio):**

```latino
edad = 25  # Asignar 25 a la variable edad
```

❌ El código ya dice exactamente eso.

**Bueno (Explica el Contexto):**

```latino
edad = 25  # Edad mínima legal para alquilar autos en este país
```

✅ Ahora entendemos **por qué** ese número.

* **✅ No Dejes Código "Muerto" Comentado**

**Malo:**

```latino
# version_antigua = 1.0
# funcion_vieja()
# calcular_mal()
version_nueva = 2.0
```

❌ Esto confunde. ¿Por qué está ahí? ¿Lo necesito?

**Bueno:**

```latino
version_nueva = 2.0  # Actualizado el 2026-01-21
```

✅ Si usas Git u otro control de versiones, el historial guarda lo antiguo. Mantén el código limpio.

* **✅ Usa Comentarios para Secciones**

En programas largos, divide visualmente el código:

```latino
# ==========================================
# SECCIÓN 1: Configuración Inicial
# ==========================================
puerto = 8080
servidor = "localhost"

# ==========================================
# SECCIÓN 2: Conexión a Base de Datos
# ==========================================
conexion = conectar(servidor, puerto)
```

## Entrada y Salida: Conversaciones con la Computadora

### La Magia de la Interacción

Hasta ahora, tus programas han sido **monólogos**: la computadora habla y tú escuchas. Pero los programas realmente útiles son **diálogos**: pregunta, el usuario responde, el programa procesa y muestra resultados.

Piensa en tus aplicaciones favoritas:

* Una calculadora te **pide** números y **muestra** el resultado
* Un juego te **pregunta** tu nombre y **te saluda** personalmente
* Una app de clima **solicita** tu ubicación y **presenta** el pronóstico

Todo esto es **Entrada/Salida** (I/O por sus siglas en inglés: Input/Output). Es el puente entre el mundo del usuario y el mundo del código.

### 1.3 Salida de Datos: Hablando con el Usuario

#### La Función `escribir()` * Tu Altavoz Digital

Ya conociste `escribir()` en el "Hola Mundo", pero hay mucho más bajo el capó.

**¿Qué hace exactamente?**

1. Toma lo que le pases (texto, números, variables)
2. Lo convierte a texto si es necesario
3. Lo envía a la **salida estándar** (tu consola)
4. Agrega un **salto de línea** automático al final

**Alias disponibles:**

* `escribir()` ← Más usado
* `imprimir()` ← Alternativa clara
* `poner()` ← Estilo conciso

```latino
escribir("Línea 1")
imprimir("Línea 2")
poner("Línea 3")
# Todas hacen exactamente lo mismo
```

#### El Desafío: Combinar Texto y Variables

Imagina que tienes datos en variables y quieres construir mensajes dinámicos:

```latino
nombre = "Carlos"
edad = 28
ciudad = "Panamá"
```

¿Cómo construyes la frase: *"Carlos tiene 28 años y vive en Panamá"*?

Latino.NET te da **tres estrategias** con comportamientos diferentes. Entender cuándo usar cada una es clave.

### Estrategia 1: Concatenación Estricta (Operador `..`)

Este es el operador **oficial** para unir texto. Funciona así:

1. Convierte **todo** a texto
2. Une los pedazos **sin agregar espacios**
3. Es predecible y seguro

**Sintaxis:** `texto1 .. texto2 .. texto3`

```latino
nombre = "Carlos"
edad = 28

# Construcción manual con espacios explícitos
mensaje = "Hola, soy " .. nombre .. " y tengo " .. edad .. " años."
escribir(mensaje)
# Salida: Hola, soy Carlos y tengo 28 años.
```

**💡 Observa:**

* Los espacios (`" "`) se agregan **manualmente**
* Los números se convierten a texto automáticamente
* Es verboso pero **muy claro** en su intención

**Ejemplo de Trampa Común:**

```latino
a = 10
b = 20
resultado = a .. b
escribir(resultado)  # Salida: 1020 (¡No es 30!)
```

¿Por qué? Porque `..` **siempre concatena texto**. Convirtió los números a `"10"` y `"20"` y los pegó.

### Estrategia 2: Suma Aritmética y Smart Casting (Operador `+`)

A partir de la versión **v1.0.81**, el operador `+` es **estrictamente aritmético**. Ya no se puede usar para unir textos.

**Regla de Oro:**

* El operador `+` siempre intentará realizar una **operación matemática**.
* Si pasas un texto que parece un número (`"10"`), Latino lo convertirá automáticamente (**Smart Casting**).
* Si pasas un texto que NO es numérico (`"Hola"`), el programa lanzará un **error**.

```latino
# Caso 1: Ambos son números
a = 10
b = 20
escribir(a + b)        # Salida: 30 (Suma normal)

# Caso 2: Smart Casting (Texto numérico)
escribir("10" + 20)    # Salida: 30 (Latino convirtió "10" a número)

# Caso 3: Error (Texto no numérico)
intentar
    escribir("Total: " + a) # ❌ ERROR: No se puede sumar texto
atrapar (e)
    escribir("Error capturado!")
fin
```

**¿Por qué este cambio?**
Para evitar errores lógicos comunes. Si quieres unir textos, usa siempre el operador de concatenación `..`.

**Trampa del Pasado (Versiones anteriores):**
En versiones antiguas de Latino.NET, `"10" + 20` resultaba en `"1020"`. Ahora resulta en `30`.

**Solución para Mensajes:**
Para mezclar texto y variables, usa `..` o el Compositor `ç()`:

```latino
escribir("Total: " .. a)     # ✅ Correcto: "Total: 10"
escribir(ç("Total: |a|"))     # ✅ Elegante: "Total: 10"
```

**Solución:** Convertir explícitamente a número con `anumero()`:

```latino
# Si 'a' es texto, convertir a número antes de sumar
a = "10"
b = anumero(a) + 5  # ✅ Correcto: 15
escribir(b)
```

### Estrategia 3: Argumentos Múltiples (Uso de Coma `,`)

La forma más natural para principiantes. `escribir()` acepta múltiples valores separados por comas:

**Ventaja Mágica:** Agrega **espacios automáticos** entre cada valor.

```latino
nombre = "Latino"
version = 1.0
año = 2026

# Sin pensar en espacios ni concatenación
escribir("Lenguaje:", nombre, "versión", version, "año", año)
# Salida: Lenguaje: Latino versión 1.0 año 2026
```

**¿Cuándo usarla?**

* Para debugging rápido (imprimir múltiples variables)
* Cuando quieres espacios naturales entre valores
* Cuando no necesitas control preciso del formato

**Comparación Visual:**

```latino
edad = 28

# Con ..  (Manual, verboso)
escribir("Edad: " .. edad .. " años")

# Con ,   (Más natural, espacios automáticos)
escribir("Edad:", edad, "años")

# Ambas producen: Edad: 28 años
```

### Formato Estilo C (`imprimirf`)

Esta función está inspirada en el `printf` de C, pero en una versión simplificada. Permite inyectar valores dentro de una cadena usando marcadores de posición.

**Especificadores Soportados:**

| Marcador | Tipo de Dato | Ejemplo de Salida |
| :-- | :-- | :-- |
| `%s` | Cadena (Texto) | "Hola" |
| `%d` o `%i` | Entero (Digito) | 10 (Redondea si es decimal) |
| `%f` | Flotante (Decimal) | 10.500000 (Formato estándar) |
| `%x` | Hexadecimal en minúsculas | ff |
| `%X` | Hexadecimal en MAYÚSCULAS | FF |
| `%o` | Octal | 377 |
| `%e` | Científico | 1.05e+001 |
| `%%` | Símbolo % Literal | % |

> **Nota:** `imprimirf()` soporta modificadores de ancho y precisión estilo C: `%.2f`, `%-10s`, `%.3e`, etc. Para alineaciones complejas y formateo de tablas, considera combinarlo con `cadena.rellenar_derecha/izquierda` o el Compositor `ç()`.
>
> ⚠️ **Nota Técnica sobre ceros a la izquierda en v1.0.81:** En `imprimirf()`, modificadores como `%02d` o `%06d` rellenan con espacios debido al conversor interno. Para rellenar con ceros de forma garantizada, utiliza el formateador nativo `escribirf("{0:D2}\n", numero)` o la función de biblioteca `cadena.rellenar_izquierda(acadena(n), "0", 2)`.

**Ejemplo Básico:**

```latino
nombre = "Pi"
valor = 3.14159

imprimirf("El valor de %s es aproximadamente %f\n", nombre, valor)
# Salida: El valor de Pi es aproximadamente 3.141590
```

**Ejemplo Avanzado: Crear una Tabla Alineada**
Como `imprimirf` es básico, para alinear texto usamos las funciones de relleno (`padding`) de la librería `cadena` junto con el Compositor `ç()`.

```latino
# Función auxiliar para formatear filas
funcion fila(prod, prec)
    # Rellenar con espacios a la derecha hasta llegar a 15 caracteres
    p = cadena.rellenar_derecha(prod, " ", 15)
    retornar ç("|p| $ |prec|")
fin

escribir(fila("Producto", "Precio"))
escribir(fila("Manzanas", 2.5))
escribir(fila("Naranjas", 3.75))

# Salida:
# Producto        $ Precio
# Manzanas        $ 2.5
# Naranjas        $ 3.75
```

### 1.4 Entrada de Datos: Escuchando al Usuario

#### Las Funciones `pedir()` y `leer()` — Tus Oídos Digitales

Hasta ahora, todos nuestros programas usaban datos fijos (*hardcodeados* en el código). Sin embargo, las aplicaciones del mundo real necesitan interactuar con el usuario y recibir información en **tiempo de ejecución**.

**¿Cómo funciona la captura de datos?**

1. El programa se **detiene** en seco y cede el control a la terminal.
2. El cursor parpadea esperando que el usuario escriba.
3. El usuario teclea su respuesta y presiona **Enter**.
4. El programa captura esa entrada, la almacena en una variable y continúa la ejecución.

#### De Latino Clásico a Latino.NET: Compatibilidad y Evolución

En Latino Clásico, solicitar un dato requería dos instrucciones separadas porque `leer()` no aceptaba argumentos. En **Latino.NET** mejoramos la función clásica y añadimos un alias más natural para los hispanohablantes:

* **`pedir([mensaje])`:** Es la forma **idiomática y recomendada** en Latino.NET. Expresa con claridad la intención de solicitar un dato.
* **`leer([mensaje])`:** Es la forma clásica. Se mantiene por **100% de retrocompatibilidad**, pero fue repotenciada en Latino.NET para aceptar un mensaje directo al igual que `pedir()`.

Ambas funciones apuntan exactamente al mismo motor interno; puedes usarlas de forma indistinta según tu preferencia:

```latino
# 1. Estilo Clásico (Dos líneas * 100% compatible con versiones antiguas):
escribir("¿Cómo te llamas?")
nombre = leer()

# 2. Estilo Clásico Mejorado (Latino.NET permite prompt dentro de leer):
nombre = leer("¿Cómo te llamas? ")

# 3. Estilo Idiomático Latino.NET (Recomendado * Una sola línea expresiva):
nombre = pedir("¿Cómo te llamas? ")
```

> 💡 **Detalle de interfaz (UX):** Nota el espacio al final del texto: `"¿Cómo te llamas? "`. Ese espacio evita que lo que el usuario escriba quede pegado al signo de interrogación, manteniendo la terminal limpia y legible.

#### Regla de Oro: Toda Entrada es Siempre `cadena` (Texto)

> ⚠️ **Importante:** Este es uno de los puntos más importantes de la arquitectura del lenguaje y el origen del 90% de los errores en programadores que inician.
>
> **Todo lo que entra por la consola es TEXTO (`cadena`)**, sin importar si el usuario tecleó letras, símbolos o números.

```latino
edad = pedir("Ingresa tu edad: ")  # Entrada simulada: 25

escribir(tipo(edad))  # Salida: "cadena" (¡NO es un número!)
```

Aunque visualmente veas un `25`, en la memoria de la máquina reside como texto (`"25"`).

#### ¿Por qué es crucial entender esto?

El operador `+` en Latino.NET es **estrictamente aritmético**. Si intentas operar directamente con la variable antes de convertirla, pueden ocurrir comportamientos inesperados o errores de tipo si la cadena contiene caracteres no numéricos:

```latino
edad = pedir("Tu edad: ")     # Entrada simulada: 25

# ❌ PELIGROSO: Confiar en el tipo dinámico
siguiente_anio = edad + 1     # Latino.NET intentará convertirlo (Smart Casting),
                              # pero si el usuario escribió "veinticinco", el programa fallará.

# ✅ PROFESIONAL Y SEGURO (Fail-Safe):
# Usamos anumero() con fusión nula (??) para garantizar un valor numérico seguro:
edad_numero = anumero(edad) ?? 0
siguiente_anio = edad_numero + 1

escribir(ç("El próximo año tendrás |siguiente_anio| años."))
```

### 🎩 La Magia del Smart Casting: cuando el texto se convierte en número

Latino.NET incorpora **Smart Casting** para hacer más natural el trabajo con datos provenientes de la entrada del usuario.

Cuando una operación aritmética recibe una cadena que representa un número, el intérprete intenta convertirla automáticamente a un valor numérico:

```latino
edad = pedir("Tu edad: ")  # Entrada simulada: 25

futuro = edad + 5
escribir(futuro)           # Salida: 30
```

Aunque `edad` comenzó siendo la cadena `"25"`, el operador `+` reconoce que se encuentra en un contexto aritmético y convierte ese valor a número antes de realizar la suma.

Lo mismo ocurre con las demás operaciones aritméticas:

```latino
edad = pedir("Tu edad: ")  # Entrada simulada: 25

meses = edad * 12
dias = edad * 365
promedio = edad / 2

escribir(meses)     # Salida: 300
escribir(dias)      # Salida: 9125
escribir(promedio)  # Salida: 12.5
```

Esto permite trabajar directamente con datos introducidos por el usuario sin tener que convertirlos manualmente en cada operación.

### ¿Y qué ocurre si el texto no representa un número?

El Smart Casting no puede convertir cualquier texto en un número.

```latino
edad = pedir("Tu edad: ")  # Entrada simulada: veinte

futuro = edad + 5
```

En este caso, `"veinte"` no representa un valor numérico válido, por lo que Latino.NET no puede realizar la operación aritmética y se producirá un error.

Esto es importante porque `+` **no es un operador de concatenación** en Latino.NET. Su función es realizar una suma aritmética.

### Concatenar texto con `..`

Cuando queremos unir textos, utilizamos el operador `..`:

```latino
nombre = pedir("¿Cómo te llamas? ")

mensaje = "Hola, " .. nombre .. "!"
escribir(mensaje)
```

Si el usuario escribe `Ana`, el resultado será:

```text
Hola, Ana!
```

La diferencia es clara:

```latino
edad = pedir("Tu edad: ")  # Entrada simulada: 25

suma = edad + 5            # Operación aritmética → 30
unTexto = edad .. " años"    # Concatenación → "25 años"

escribir(suma)
escribir(unTexto)
```

**Regla práctica:**

* `+`, `-`, `*` y `/` → operaciones aritméticas.
* `..` → concatenación de texto.
* `anumero()` → conversión explícita de una cadena numérica a número.

### Conversión explícita con `anumero()`

El Smart Casting es una ayuda conveniente, pero la práctica profesional recomendada es convertir explícitamente cualquier entrada textual a número mediante `anumero()`.

#### Soporte Amigable de Coma y Punto Decimal

Pensando en los estándares de escritura del mundo hispanohablante, en Latino.NET `anumero()` normaliza de forma segura tanto el punto como la coma decimal para valores numéricos simples:

```latino
precio1 = anumero("19.99")  # Retorna 19.99
precio2 = anumero("19,99")  # Retorna 19.99 (normalización automática de coma)
```

Si la cadena no contiene un formato numérico reconocible o presenta formatos ambiguos con múltiples separadores (como `"1,234,56"` o `"veinte"`), `anumero()` devuelve **`nulo`** sin lanzar excepciones.

#### El Patrón Defensivo Profesional (`??`)

Al combinar `anumero()` con el operador de fusión nula (`??`), garantizas que tu programa nunca opere con valores nulos o falle ante datos inválidos ingresados por el usuario:

```latino
# Si el usuario ingresa texto no numérico o vacío, 'edad' respaldará en 0
edad = anumero(pedir("Tu edad: ")) ?? 0

futuro = edad + 5
escribir(futuro)
```

🎯 **Ejemplo Completo: Calculadora de Edad**

```latino
/*
  Programa: Calculadora de Edad
  Demuestra: Entrada de usuario, Smart Casting, y salida formateada
*/

# Entrada
nombre = pedir("¿Cómo te llamas? ")    # Entrada simulada: Carlos
edad = pedir("¿Cuántos años tienes? ") # Entrada simulada: 28

# Procesamiento (Smart Casting automático en *)
meses = edad * 12
dias = edad * 365
horas = dias * 24

# Salida formateada
escribir("")  # Línea en blanco
escribir("=== RESUMEN DE VIDA ===")
escribir("Nombre:", nombre)
escribir("Edad:", edad, "años")
escribir("")
escribir("Has vivido aproximadamente:")
escribir("  •", meses, "meses")
escribir("  •", dias, "días")
escribir("  •", horas, "horas")
```

**Ejecución de ejemplo:**

```text
¿Cómo te llamas? Carlos
¿Cuántos años tienes? 28

=== RESUMEN DE VIDA ===
Nombre: Carlos
Edad: 28 años

Has vivido aproximadamente:
  • 336 meses
  • 10220 días
  • 245280 horas
```

## Tipos de Datos: El ADN de la Información

### El Universo de los Datos

Si la programación fuera un idioma, los **tipos de datos** serían el alfabeto. Todo programa manipula información, y esa información viene en diferentes "formas":

* Los **números** te permiten calcular (edad, precio, distancia)
* El **texto** te permite comunicar (nombres, mensajes, direcciones)
* Los **valores lógicos** te permiten decidir (¿está activo? ¿es mayor de edad?)
* El **nulo** te permite representar "ausencia" (¿tiene correo? → nulo)

Latino.NET está diseñado para que trabajes con datos de forma **natural e intuitiva**. No necesitas declarar tipos explícitamente (como en C# o Java). El lenguaje **infiere** automáticamente qué tipo de dato tienes.

### 1.5 Los Tipos Fundamentales

En Latino.NET, todos los valores derivan de una clase base llamada `LatinoObject`. Pero no te preocupes por los detalles técnicos ahora. Lo importante es entender **qué representa cada tipo** y **cómo usarlo**.

#### Nulo (`nulo`)

**Concepto:** Representa la **ausencia intencional de valor**.

No es lo mismo que:

* Cero (`0`) → Es un número
* Texto vacío (`""`) → Es texto, pero sin caracteres
* Falso (`falso`) → Es un valor lógico

`nulo` significa: *"Aquí debería haber algo, pero no hay nada"*.

**Casos de Uso:**

```latino
# Variable que aún no tiene valor
email = nulo

# Más adelante en el programa
si email == nulo
    escribir("El usuario no proporcionó email")
sino
    escribir("Email registrado:", email)
fin
```

**En Objetos:**

```latino
usuario = {
    "nombre": "Carlos",
    "telefono": nulo  # No tiene teléfono registrado
}

si usuario["telefono"] == nulo
    escribir("Contactar por email")
fin
```

**Nota Técnica:** Intentar acceder a una clave inexistente en un diccionario lanza `RuntimeError`. Para evitarlo, usa `dic.contiene()` o el operador `??` (que funciona tanto con variables como con claves):

```latino
persona = {"nombre": "Ana"}
# ✅ Forma segura con ?? (funciona con claves):
escribir(persona["edad"] ?? "Edad no especificada")

# ✅ Alternativa con dic.contiene:
si dic.contiene(persona, "edad")
    escribir(persona["edad"])
sino
    escribir("Edad no especificada")
fin
```

#### Tipo Lógico (`cierto`, `falso`)

**Concepto:** Representa **valores de verdad** binarios.

En Latino.NET, la salida canónica e identidad primordial del lenguaje son **`cierto`** y **`falso`** (así se imprimen en consola y se evalúan en inspecciones). Para facilitar la transición y lectura natural, el motor incluye compatibilidad con alias de entrada:

* **Verdadero:** Forma canónica **`cierto`** (admite el alias común **`verdadero`**).
* **Falso:** Forma canónica **`falso`** (admite el alias **`espurio`**, sinónimo de ilegítimo o adulterado).

```latino
# Formas canónicas oficiales
activo = cierto
suspendido = falso

# Uso de alias de entrada (equivalencia semántica total)
esMayor = verdadero   # El motor lo almacena y muestra como 'cierto'
esInvalido = espurio  # El motor lo almacena y muestra como 'falso'

escribir(activo == esMayor)       # Salida: cierto
escribir(suspendido == esInvalido) # Salida: cierto
escribir(esMayor)                 # Salida: cierto (imprime siempre la forma canónica)
```

**¿Cuándo usarlos?**

En **estructuras de control** (decisiones):

```latino
llueve = verdadero

si llueve
    escribir("Lleva paraguas")
sino
    escribir("Disfruta el sol")
fin
```

**En Validaciones:**

```latino
edad = 17
esMayor = edad >= 18  # esMayor contiene: falso

si esMayor
    escribir("Puede votar")
sino
    escribir("Aún no puede votar")
fin
```

#### 🔢 Numérico (Números de Punto Flotante)

**Concepto:** Representa **cualquier número**, ya sea entero o decimal.

A diferencia de lenguajes como C++ o Java, Latino.NET **no distingue** entre enteros y decimales. Todo número es un `decimal` de doble precisión (64 bits).

**Ejemplos:**

```latino
edad = 28              # Entero
precio = 19.99         # Decimal
temperatura = -5.5     # Negativo
distancia = 1000000    # Grande
pequeno = 0.0001       # Muy pequeño

# Notación científica (soportada de forma nativa en el código)
luz = 3e8              # 300000000.0 (3 × 10⁸ m/s)
masaElectron = 9.1e-31 # 9.1 × 10⁻³¹
```

**Operaciones Básicas:**

```latino
a = 10
b = 3

suma = a + b           # 13
resta = a * b          # 7
multiplicacion = a * b # 30
division = a / b       # 3.333...
modulo = a % b         # 1 (el resto)
potencia = a ^ 2       # 100
```

**⚠️ Cuidado con la División por Cero:**

```latino
resultado = 10 / 0     # Error Runtime: División por cero
```

**Precisión de Decimales:**

Los números de punto flotante tienen **límites de precisión**:

```latino
x = 0.1 + 0.2
escribir(x)  # Salida: 0.30000000000000004

# Esto es normal en TODOS los lenguajes (JavaScript, Python, C#)
# Se debe a cómo se representa en binario
```

**Solución:** Redondea cuando muestres al usuario:

```latino
imprimirf("%.2f\n", x)  # Salida: 0.30
```

#### 📝 Cadenas (Texto)

**Concepto:** Secuencias de caracteres usadas para representar **texto**.

**Definición:** Usa comillas dobles (`"`) o simples (`'`):

```latino
nombre = "Carlos"
ciudad = 'Panamá'
frase = "Texto con 'comillas simples' adentro"
otra = 'Texto con "comillas dobles" adentro'
```

**Cadenas Vacías:**

```latino
vacio = ""     # Texto sin caracteres (pero existe)
nada = nulo    # Ausencia total de valor

escribir(vacio == nulo)  # Salida: falso (son diferentes)
```

### Secuencias de Escape: Caracteres Especiales

Algunos caracteres no se pueden escribir directamente. Usamos **secuencias de escape** (empiezan con `\`):

| Secuencia | Significado |
| :-- | :-- |
| `\n` | Salto de línea (Enter) |
| `\t` | Tabulación (Tab) |
| `\"` | Comilla doble literal |
| `\'` | Comilla simple literal |
| `\\` | Barra invertida literal |
| `\uXXXX` | Carácter Unicode (hexadecimal) |

**Ejemplos:**

```latino
# Salto de línea
escribir("Primera línea\nSegunda línea")
# Salida:
# Primera línea
# Segunda línea

# Tabulación
escribir("Nombre:\tCarlos")
# Salida: Nombre:    Carlos

# Comillas dentro de texto
frase = "Él dijo: \"Hola\""
escribir(frase)  # Salida: Él dijo: "Hola"

# Unicode (corazón)
escribir("Te quiero \u2764")  # Salida: Te quiero ❤
```

### 📌 Cadenas de Texto Multilínea

En Latino.NET, las cadenas delimitadas por comillas dobles (`"..."`) o simples (`'...'`) pueden extenderse a lo largo de **múltiples líneas físicas** en el código fuente sin necesidad de caracteres especiales de escape ni sintaxis compleja de *heredoc*.

#### Preservación Literal de Caracteres

El motor almacena la cadena **exactamente como fue escrita**, preservando de forma literal:

1. Todos los saltos de línea intermedios.
2. Todas las tabulaciones y sangrías de cada renglón.
3. Los saltos de línea iniciales o finales si las comillas de apertura o cierre están en renglones separados.

#### 🎯 Ejemplo: Consultas SQL y Bloques Preformateados

Las cadenas multilínea son la forma más limpia de redactar consultas a bases de datos o plantillas de texto en Latino.NET:

```latino
incluir("sqlite")

db = sqlite.conectar(":memory:")

# Cadena multilínea directa para definir una tabla SQL:
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS contactos (
    id       INTEGER PRIMARY KEY,
    nombre   TEXT NOT NULL,
    telefono TEXT,
    activo   INTEGER DEFAULT 1
)")

escribir("✅ Tabla de contactos creada en memoria exitosamente.")
sqlite.cerrar(db)
```

#### ⚠️ La Trampa de la Indentación Visual

Cuando sangras el texto para que se vea alineado con tu código, **esos espacios de sangría formarán parte real del contenido de la cadena**:

```latino
# ❌ Cuidado: la sangría del código se imprimirá en la salida
menu = "
    1. Iniciar sesión
    2. Registrarse
    3. Salir
"

escribir(menu)
# Se imprimirá con los 4 espacios a la izquierda de cada opción
```

#### ¿Cómo evitar sangrías o saltos no deseados?

Dependiendo de la necesidad de tu programa, dispones de tres técnicas idiomáticas:

##### 1. Uso de `cadena.recortar()`

Si dejas las comillas en su propio renglón, se creará un salto de línea en blanco al principio y al final. Puedes limpiarlos con `cadena.recortar()` (elimina espacios y saltos en los extremos globales de la cadena):

```latino
unTexto = "
Hola, mundo
"

textoLimpio = cadena.recortar(unTexto)
escribir(textoLimpio) # Imprime "Hola, mundo" sin líneas vacías arriba ni abajo
```

##### 2. Pegar el texto al margen izquierdo

Si requieres un bloque multilínea sin sangría interna, escribe el contenido pegado a la columna cero:

```latino
aviso = "AVISO IMPORTANTE:
El servidor entrará en mantenimiento
a las 22:00 UTC."

escribir(aviso)
```

##### 3. Concatenación explícita con `..` (Máximo control)

Si prefieres que tu código fuente conserve su sangría visual en el editor sin contaminar el texto con espacios, utiliza el operador de concatenación `..`:

```latino
consulta = "SELECT id, nombre "
    .. "FROM usuarios "
    .. "WHERE activo = 1"
```

### 🎨 Acceso Inteligente a Caracteres

Aquí es donde Latino.NET brilla. La mayoría de los lenguajes tratan las cadenas como **arrays de bytes**, pero Latino entiende **grafemas** (lo que el ojo ve como "un carácter").

**Sintaxis:** `texto[índice]`

**Índices positivos** (empiezan en 0):

```latino
saludo = "Hola"

escribir(saludo[0])  # Salida: H
escribir(saludo[1])  # Salida: o
escribir(saludo[2])  # Salida: l
escribir(saludo[3])  # Salida: a
```

**Índices negativos** (desde el final, estilo Python):

```latino
saludo = "Hola"

escribir(saludo[-1])  # Salida: a (último)
escribir(saludo[-2])  # Salida: l (penúltimo)
escribir(saludo[-3])  # Salida: o
escribir(saludo[-4])  # Salida: H (primero desde el final)
```

### 🚀 El Superpoder: Emojis y Acentos

**¿Por qué es un superpoder?**

En lenguajes como Java o C# clásico, un emoji como 🚀 ocupa **2 posiciones** en memoria (se llama "par sustituto"). Esto rompe el acceso por índice:

```java
// En Java (comportamiento problemático)
String unTexto = "Latino🚀";
char c = texto.charAt(6);  // Devuelve MEDIO emoji (basura)
```

**En Latino.NET:**

```latino
unTexto = "Latino🚀"

escribir(unTexto[6])   # Salida: 🚀 (¡completo!)
escribir(unTexto[-1])  # Salida: 🚀 (funciona con negativos)
```

Latino.NET usa **StringInfo de .NET** internamente, que entiende grafemas completos.

**Más ejemplos:**

```latino
mensaje = "¡Hola! 👋😊"

escribir(mensaje[0])   # Salida: ¡
escribir(mensaje[6])   # Salida: (espacio)
escribir(mensaje[7])   # Salida: 👋 (emoji completo)
escribir(mensaje[8])   # Salida: 😊 (otro emoji completo)
escribir(mensaje[-1])  # Salida: 😊 (último)
```

### Casos de Uso Prácticos

* **1. Validar Extensión de Archivo**

```latino
nombreArchivo = "documento.pdf"

# Verificar si termina en punto
si nombreArchivo[-4] == "."
    extension = nombreArchivo[-3] .. nombreArchivo[-2] .. nombreArchivo[-1]
    escribir("Extensión:", extension)  # Salida: pdf
fin
```

**Forma aún mejor (usando función de cadenas):**

```latino
nombreArchivo = "documento.pdf"
si nombreArchivo[-1] == "f" ý nombreArchivo[-2] == "d" ý nombreArchivo[-3] == "p"
    escribir("Es un PDF")
fin
```

* **2. Verificar si Texto Está Entre Paréntesis**

```latino
expresion = "(x + y)"

si expresion[0] == "(" ý expresion[-1] == ")"
    escribir("Expresión agrupada correctamente")
sino
    escribir("Faltan paréntesis")
fin
```

* **3. Extraer Iniciales de un Nombre**

```latino
nombre = "Ángel"
apellido = "López"

iniciales = nombre[0] .. apellido[0]
escribir(iniciales)  # Salida: ÁL (respeta la tilde)
```

Un **palíndromo** es una palabra que se lee igual de adelante hacia atrás (ej: "reconocer", "somos").

**Estrategia:**

* Comparamos el primer carácter con el último
* Comparamos el segundo con el penúltimo
* Y así sucesivamente hasta la mitad

🎯 **Ejemplo: Detector de palíndromos mediante índices simétricos**

```latino
funcion esPalindromo(unTexto)
    longitud = cadena.longitud(unTexto)
    mitad = longitud / 2
    
    # Recorremos solo hasta la mitad
    desde (i = 0; i < mitad; i++)
        # Comparamos espejo:
        # i=0 → texto[0] vs texto[-1]
        # i=1 → texto[1] vs texto[-2]
        # i=2 → texto[2] vs texto[-3]
        si texto[i] != texto[-(i + 1)]
            retornar falso
        fin
    fin
    
    retornar verdadero
fin

# Pruebas
escribir("reconocer:", esPalindromo("reconocer"))  # reconocer: cierto
escribir("somos:", esPalindromo("somos"))          # somos: cierto
escribir("anita:", esPalindromo("anita"))          # anita: falso
escribir("latino:", esPalindromo("latino"))        # latino: falso
```

**¿Por qué `-(i + 1)` y no `-i`?**

Porque los índices negativos empiezan en `-1` (el último), no en `-0`:

* `-1` = último
* `-2` = penúltimo
* `-3` = antepenúltimo

Entonces para comparar:

* `i = 0` → comparamos `[0]` con `[-1]`
* `i = 1` → comparamos `[1]` con `[-2]`
* `i = 2` → comparamos `[2]` con `[-3]`

La fórmula es: `[i]` vs `[-(i+1)]`

### Advertencia Técnica: Longitud vs Visualización

Existe una diferencia entre **ver** una cadena y **medirla**:

* **`texto[i]`**: Accede a **grafemas** (lo que ves)
* **`cadena.longitud(unTexto)`**: Mide **grafemas** (lo que ves visualmente)

```latino
mensaje = "Hola 🌍"

# Visualmente: H-o-l-a-espacio-🌍 = 6 símbolos
# La función cuenta grafemas, no unidades UTF-16

escribir(cadena.longitud(mensaje))  # Salida: 6

# Pero al acceder:
escribir(mensaje[5])  # Salida: 🌍 (correcto, grafema completo)
```

**¿Cuándo importa esto?**

* Al calcular posiciones exactas en archivos
* Al trabajar con APIs que esperan bytes
* Al hacer validaciones de longitud máxima

**Regla Práctica:**

* Para **mostrar** al usuario → Usa índices (`texto[i]`)
* Para **validar longitud** → Usa `cadena.longitud()`

## Variables, Constantes y el Compositor

### El Arte de Nombrar Cosas

Uno de los aspectos más subestimados de la programación es **elegir buenos nombres**. Un programa con nombres descriptivos se lee como una historia; uno con nombres crípticos se lee como un acertijo.

> *"Hay dos cosas difíciles en ciencias de la computación: invalidación de caché y nombrar cosas."* — Phil Karlton

Las variables son **cajas con etiquetas** donde guardas información. La etiqueta (el nombre) debe decirte claramente **qué contiene** sin necesidad de buscar en el código.

### 1.6 Variables: Contenedores con Identidad

#### ¿Qué es una Variable?

Una variable es un **espacio en memoria** con un **nombre simbólico** que apunta a un valor. Ese valor puede cambiar durante la ejecución del programa (por eso se llama "variable").

**Analogía:** Imagina una caja de cartón con una etiqueta. Puedes:

* Poner cosas dentro (asignar valor)
* Ver qué hay dentro (leer valor)
* Cambiar el contenido (reasignar)
* La etiqueta siempre es la misma, pero el contenido puede cambiar

```latino
# Creación (primera asignación)
edad = 25

# Lectura (usar el valor)
escribir(edad)  # Salida: 25

# Reasignación (cambiar el contenido)
edad = 26
escribir(edad)  # Salida: 26
```

### 📜 Reglas de Nombres en Latino.NET

#### Reglas Obligatorias de Nomenclatura

* **1. Debe empezar con letra o guion bajo (`_`)**

```latino
# ✅ Válidos
nombre = "Carlos"
_privado = 100
usuario2 = "Ana"

# ❌ Inválidos
2usuario = "Error"   # No puede empezar con número
@variable = 10       # No puede empezar con símbolo
```

* **2. No puede contener espacios**

```latino
# ❌ Inválido
mi variable = 10

# ✅ Válido (usa camelCase o guion bajo)
miVariable = 10
mi_variable = 10
```

* **3. Latino distingue mayúsculas de minúsculas**

```latino
edad = 25
Edad = 30
EDAD = 35

escribir(edad)   # Salida: 25
escribir(Edad)   # Salida: 30
escribir(EDAD)   # Salida: 35
# Son tres variables diferentes
```

### 📌 Palabras Reservadas: Liberación de 'y' y 'o'

Latino.NET tiene **palabras reservadas** que el lenguaje usa para su sintaxis. **No puedes usarlas como nombres de variables.**

**Cambio importante en v1.0:**

Anteriormente, `y` y `o` eran operadores lógicos y no podían usarse como variables. Ahora, para liberar estos nombres tan comunes (especialmente para matemáticas y objetos), los operadores se han movido a sus versiones acentuadas:

* `ý` = AND (&&)
* `ó` = OR (||)

**Esto significa que ahora puedes hacer:**

```latino
# ✅ AHORA ES VÁLIDO Y RECOMENDADO
x = 10
y = 20    # Perfecto para coordenadas
o = { "a": 1 } # Perfecto para representar "objeto"
```

**¿Por qué este cambio?**

Porque en el mundo de los gráficos y la física, `y` es una coordenada indispensable. Obligar a usar `Y` (mayúscula) o `coord_y` restaba naturalidad al idioma. Con `ý` y `ó`, mantenemos el español pero liberamos las variables más valiosas.

### 🎯 Convención Latino.NET: NO Uses Letras Sueltas

En Latino.NET promovemos **código legible y autodocumentado**. Las letras sueltas dificultan el mantenimiento.

🎯 **Ejemplo No Recomendado (Confuso e Ilegible)**

```latino
# ¿Qué demonios significan estas letras?
a = 10
b = 20
c = a + b
d = c * 2
e = d / 3
```

Un mes después, ni tú recordarás qué hace este código.

🎯 **Ejemplo Recomendado (Descriptivo y Claro)**

```latino
precioBase = 10
impuesto = 20
precioConImpuesto = precioBase + impuesto
precioDoble = precioConImpuesto * 2
precioFinal = precioDoble / 3
```

Ahora el código se lee como una receta, no como un enigma.

### 🔢 Excepciones Permitidas (Convenciones Universales)

Hay **contextos específicos** donde letras sueltas son aceptables porque tienen significado universal:

#### 1️⃣ Contadores en Bucles

```latino
# ✅ Aceptable: 'i' es universalmente entendido como contador
desde (i = 0; i < 10; i++)
    escribir(i)
fin

# Para bucles anidados
desde (i = 0; i < 3; i++)
    desde (j = 0; j < 3; j++)
        escribir(i, j)
    fin
fin
```

**Nota:** Si el bucle es complejo, usa nombres descriptivos:

```latino
# ❌ Confuso en lógica compleja
desde (i = 0; i < usuarios; i++)
    desde (j = 0; j < productos; j++)
        # ¿Qué es i? ¿Qué es j?
    fin
fin

# ✅ Mejor
desde (indiceUsuario = 0; indiceUsuario < usuarios; indiceUsuario++)
    desde (indiceProducto = 0; indiceProducto < productos; indiceProducto++)
        # Ahora es obvio qué hace cada uno
    fin
fin
```

#### 2️⃣ Coordenadas (¡Usa x, y con libertad!)

```latino
# ✅ Ahora puedes usar minúsculas sin miedo
x = 100
y = 200
z = 50
```

**Nota Histórica:** En versiones anteriores, usar `y` en minúscula causaba errores por ser un operador. Si ves tutoriales viejos que insisten en usar `Y` mayúscula, es por esta razón. En Latino.NET ya no es necesario.

#### 3️⃣ Variables Matemáticas Estándar

```latino
# En contextos matemáticos/científicos
a = 1  # Coeficiente en ecuación cuadrática: ax² + bx + c
b = -5
c = 6

discriminante = b^2 * 4*a*c
```

**Pero incluso aquí, si puedes, sé más claro:**

```latino
# ✅ Mejor
coeficienteA = 1
coeficienteB = -5
terminoIndependiente = 6
```

### ✍️ Variables en Español: Tildes y Eñes

Latino.NET **soporta completamente** caracteres Unicode en nombres de variables. Puedes usar **tildes**, **eñes** y otros caracteres especiales:

```latino
# ✅ Totalmente válido
año = 2026
niño = "Carlitos"
día = "Lunes"
operación = "Suma"
Ángel = "Nombre propio"

# Incluso emojis (aunque no es recomendable)
🚀 = "cohete"  # Técnicamente funciona, pero no lo hagas
```

**Requisito Técnico:**

Para que esto funcione correctamente:

1. Tu archivo `.lat` debe estar guardado en **UTF-8**
2. Tu consola/terminal debe soportar UTF-8 (en Windows: `chcp 65001`)

### Convenciones de Nomenclatura

Latino.NET recomienda **camelCase** para variables (primera palabra en minúscula, siguientes con mayúscula inicial):

```latino
# ✅ Recomendado (camelCase)
nombreCompleto = "Carlos López"
edadActual = 28
saldoDisponible = 1500.50
estaActivo = verdadero

# También válido (snake_case, estilo Python)
nombre_completo = "Carlos López"
edad_actual = 28
saldo_disponible = 1500.50
esta_activo = verdadero

# ❌ No recomendado (confuso)
NombreCompleto = "Carlos"  # Parece nombre de clase
NOMBRE_COMPLETO = "Carlos"  # Parece constante
```

### 📌 Constantes: Valores Inmutables

Una **constante** es una variable que **no puede cambiar** después de su primera asignación.

#### Regla de Inmutabilidad

En Latino.NET, si un nombre está **completamente en MAYÚSCULAS**, se trata como constante:

```latino
# Declaración de constantes
PI = 3.14159
MAX_INTENTOS = 3
URL_BASE = "https://api.ejemplo.com"
VELOCIDAD_LUZ = 299792458
```

**¿Qué pasa si intentas cambiarlas?**

```latino
PI = 3.14159

# Intento de reasignación
PI = 4.0  # ❌ Error Runtime: No se puede reasignar una constante
```

El intérprete lanzará un error inmediatamente.

🎯 **Ejemplo Completo: Calculadora de Área**

```latino
/*
  Calculadora de Círculo
  Demuestra: Constantes, variables descriptivas, y buenas prácticas
*/

# Constante matemática (MAYÚSCULAS)
PI = 3.14159265359

# Variables descriptivas (camelCase)
radio = 5
diametro = radio * 2
circunferencia = 2 * PI * radio
area = PI * radio^2

# Salida formateada
escribir("=== CÍRCULO ===")
imprimirf("Radio: %.2f unidades\n", radio)
imprimirf("Diámetro: %.2f unidades\n", diametro)
imprimirf("Circunferencia: %.2f unidades\n", circunferencia)
imprimirf("Área: %.2f unidades²\n", area)
```

**Salida:**

```text
=== CÍRCULO ===
Radio: 5.00 unidades
Diámetro: 10.00 unidades
Circunferencia: 31.42 unidades
Área: 78.54 unidades²
```

### 📏 Inicialización Obligatoria

En Latino.NET, **debes asignar un valor inicial** a una variable antes de usarla:

```latino
# ❌ Error: Variable no inicializada
escribir(miVariable)  # Error: Variable 'miVariable' no definida

# ✅ Correcto
miVariable = 10
escribir(miVariable)  # Salida: 10
```

**Buena Práctica:** Inicializa con valores por defecto si es necesario:

```latino
contador = 0        # Entero en cero
nombre = ""         # Texto vacío
activo = falso      # Booleano por defecto
datos = nulo        # Sin valor inicial
```

#### Declaración Múltiple y el Uso del Punto y Coma (`;`)

En Latino.NET, los saltos de línea y los puntos y comas (`;`) son completamente **opcionales** para el motor. El motor es lo suficientemente inteligente como para leer una instrucción tras otra sin necesidad de delimitadores estrictos.

Esto te permite declarar e inicializar múltiples variables en una sola línea. Sin embargo, para mantener el código legible, **se recomienda encarecidamente utilizar el punto y coma (`;`) como separador visual** cuando agrupes instrucciones.

**Ejemplos:**

```latino
# ✅ Válido, pero confuso a simple vista:
pos = 0 neg = 0 ceros = 0

# 🏆 Recomendado (Código Limpio con separador visual):
pos = 0; neg = 0; ceros = 0;
```

**Consideraciones y Buenas Prácticas:**

1. **Agrupación Semántica:** Inicializa variables en la misma línea **solo** si están estrechamente relacionadas (por ejemplo: contadores de un mismo bucle, coordenadas `x = 0; y = 0`, o un grupo de variables que inician en `nulo`).
2. **Claridad sobre Compresión:** Nunca sacrifiques la legibilidad por ahorrar líneas de código. No utilices esta técnica para mezclar llamadas a funciones complejas u operaciones matemáticas largas en una sola línea.
3. **Punto y coma al final de la línea:** Si solo hay una instrucción por línea (como es habitual), colocar el `;` al final es opcional y depende enteramente del gusto del programador. Personalmente no lo recomiendo, es pura recarga visual sin ser algo necesario.

### 1.7 El Compositor `ç()`: Plantillas Inteligentes

Cuando construyes mensajes dinámicos, el operador `..` se vuelve tedioso y difícil de leer:

```latino
# Forma tradicional (difícil de leer)
nombre = "Carlos"
edad = 28
ciudad = "Panamá"

mensaje = "Hola, soy " .. nombre .. ", tengo " .. edad .. " años y vivo en " .. ciudad .. "."
```

El **Compositor** de cadenas `ç()` resuelve esto permitiendo **incrustar variables directamente** en el texto.

#### Sintaxis Básica

Encierra el nombre de la variable entre barras verticales `|variable|`:

```latino
nombre = "Carlos"
edad = 28
ciudad = "Panamá"

mensaje = ç("Hola, soy |nombre|, tengo |edad| años y vivo en |ciudad|.")
escribir(mensaje)
# Salida: Hola, soy Carlos, tengo 28 años y vivo en Panamá.
```

**Ventajas:**

1. **Legibilidad:** El texto se lee como aparecerá en pantalla
2. **Menos errores:** No hay que contar comillas y operadores `..`
3. **Mantenimiento:** Fácil ver qué variables se usan

#### Atajo de Productividad

La extensión oficial de VS Code incluye **snippets** para el compositor:

* Escribe `cc` y presiona `Tab` → se expande a `ç("")`
* Escribe `comp` y presiona `Tab` → se expande a `ç("")`

#### 💡 Cadenas Multilínea en el Compositor `ç()`

El compositor estructural `ç()` soporta plantillas multilínea con total naturalidad, permitiendo incrustar variables en cualquier línea del bloque:

```latino
usuario = "Carlos"
rol = "Administrador"

reporte = ç("
==============================
FICHA DE ACCESO
==============================
Usuario: |usuario|
Rol:     |rol|
Estado:  Activo
")

escribir(cadena.recortar(reporte))
```

> 🔒 **Regla del Compositor en Multilínea:**  
> Las expresiones dentro de `|...|` deben resolverse en una sola línea física. No dividas una etiqueta con un salto de línea (por ejemplo, `|usuario\n.nombre|` es inválido).

#### Navegación Profunda (Estructuras Complejas)

El Compositor no solo lee variables simples, puede **navegar** dentro de objetos y listas:

**Acceso a Propiedades:**

```latino
usuario = {
    "nombre": "Admin",
    "email": "admin@ejemplo.com",
    "edad": 30
}

mensaje = ç("Usuario: |usuario['nombre']| (|usuario['email']|)")
escribir(mensaje)
# Salida: Usuario: Admin (admin@ejemplo.com)
```

**Acceso a Listas:**

```latino
colores = ["Rojo", "Verde", "Azul"]

mensaje = ç("El primer color es |colores[0]|")
escribir(mensaje)
# Salida: El primer color es Rojo
```

**Anidamiento Complejo:**

```latino
datos = {
    "empresa": "TechCorp",
    "empleados": [
        {"nombre": "Ana", "cargo": "CEO"},
        {"nombre": "Luis", "cargo": "CTO"}
    ]
}

mensaje = ç(
    "En |datos['empresa']|, |datos['empleados'][0]['nombre']| es |datos['empleados'][0]['cargo']|"
    )
    
escribir(mensaje)
# Salida: En TechCorp, Ana es CEO
```

#### Limitaciones Importantes

* **No Hay Límite de Profundidad (aclaración verificada)**

A diferencia de lo que se supone en algunos casos, el Compositor **no tiene un tope de niveles**: navega tan profundo como la estructura lo permita. Todo esto funciona y está verificado ejecutando:

```latino
datos = {
    "empresa": "Acme",
    "empleados": [
        {"nombre": "Luis", "cargo": "Dev"},
        {"nombre": "Ana",  "cargo": "Chef"}
    ],
    "matriz": [[1, 2], [3, 4]]
}

escribir(ç("|datos['empleados'][1]['nombre']|"))   # Ana   (3 niveles)
escribir(ç("|datos['matriz'][1][0]|"))             # 3     (doble índice)
escribir(ç("|datos.empresa|"))                     # Acme  (punto en vez de corchetes)
```

El punto y los corchetes son **equivalentes** en cualquier nivel, y los índices numéricos se pueden encadenar. Lo que **no** se admite no es la profundidad, sino lo siguiente.

* **Los Índices Dinámicos NO Se Resuelven (el límite real)**

Este es el límite que realmente marca la frontera del Compositor: **la clave o el índice debe ser literal**. No se evalúa como expresión, así que un índice que dependa de una variable se deja sin resolver y aparece **en la salida tal cual**:

```latino
frutas = ["manzana", "naranja", "uva"]
i = 1

# ❌ NO resuelve: el índice i es una variable
unTexto = ç("La fruta es |frutas[i]|")
escribir(unTexto)
# Salida: La fruta es |frutas[i]|   (literal, no "naranja")

# ✅ Solución: calcula el valor antes y usa una referencia simple
elegida = frutas[i]
unTexto = ç("La fruta es |elegida|")
escribir(unTexto)
# Salida: La fruta es naranja
```

Lo mismo aplica a las claves dinámicas de diccionarios: `|usuario[clave]|` no funciona si `clave` es una variable. La clave debe escribirse literal
(`|usuario['nombre']|`).

* **1. Solo Referencias, No Expresiones**

El Compositor busca **rutas de acceso**, no evalúa código:

```latino
a = 10
b = 5

# ❌ No funciona (no evalúa operaciones)
unTexto = ç("La suma es |a + b|")
escribir(unTexto)
# Salida: La suma es |a + b| (literal, no calculado)

# ✅ Solución: Calcula primero
suma = a + b
unTexto = ç("La suma es |suma|")
escribir(unTexto)
# Salida: La suma es 15
```

* **2. Manejo de Errores (Silencioso)**

Si una variable no existe, el texto `|...|` permanece intacto:

```latino
nombre = "Carlos"

mensaje = ç("Hola |nombre|, tu edad es |edad|")
escribir(mensaje)
# Salida: Hola Carlos, tu edad es |edad|
# (edad no existe, se mantiene literal)
```

Esto es **intencional** para facilitar el debugging.

#### 💼 Caso de Uso: Generación de SQL

El Compositor es ideal para construir consultas o código donde las comillas harían el código ilegible:

```latino
unaTabla = "usuarios"
idUsuario = 42
estado = "activo"

query = ç("SELECT * FROM |unaTabla| WHERE id = |idUsuario| AND estado = '|estado|'")
escribir(query)
# Salida: SELECT * FROM usuarios WHERE id = 42 AND estado = 'activo'
```

Sin el compositor:

```latino
query = "SELECT * FROM " .. unaTabla .. " WHERE id = " .. idUsuario .. " AND estado = '" .. estado .. "'"
# Difícil de leer y propenso a errores
```

## Operadores: Las Herramientas del Lenguaje

### El Lenguaje de las Máquinas

Si las variables son **cajas con etiquetas**, los operadores son las **herramientas** que usas para manipular su contenido. Son los símbolos que le dicen a la computadora qué hacer con los datos:

* **Sumar** precios (`+`)
* **Comparar** edades (`>`, `<`)
* **Combinar** condiciones (`ý`, `ó`)
* **Acceder** a propiedades (`.`)
* **Proteger** contra valores nulos (`??`)

Los operadores son el vocabulario matemático y lógico de Latino.NET. Dominarlos es como aprender los verbos de un idioma: son la acción que hace que todo funcione.

### 📌 Nota Tipográfica: Ligaduras de Código en la Documentación

A lo largo de este manual (y especialmente en su versión maquetada en PDF), observarás que ciertos operadores de dos o más caracteres se visualizan como **un único símbolo gráfico continuo** (por ejemplo, verás una flecha estilizada en lugar de una barra y un signo mayor, o un signo de desigualdad matemática en vez de una exclamación e igual).

Este efecto visual se conoce como **Ligaduras Tipográficas** (*Font Ligatures*).

#### ¿Qué son las ligaduras y cómo se escriben?

Las fuentes modernas diseñadas para programadores combinan automáticamente pares o tríos de caracteres contiguos en un glifo más legible para descansar la vista.

⚠️ **Regla fundamental para el teclado:**  
**En tu teclado no necesitas buscar ningún símbolo especial.** Las ligaduras son puramente estéticas a nivel de pantalla o papel. Al escribir código en tu editor, **debes teclear siempre los caracteres estándar individuales uno tras otro**.

#### Tabla de Operadores y su Representación Visual

En la siguiente tabla puedes comparar cómo debes escribir cada operador en tu teclado frente a cómo lo verás renderizado en los bloques de código de este manual:

| Operador en Latino.NET | Lo que escribes en tu teclado | Cómo se ve con ligaduras (PDF) | Significado / Uso |
| :-- | :-- | :-- | :-- |
| **Tubería (Pipeline)** | `\|>` | ▷ / ⯈ | Pasa el dato izquierdo como primer argumento |
| **Igualdad** | `==` | 🟰 / == continuo | Comparación de igualdad |
| **Desigualdad** | `!=` | ≠ | Comparación de no igual / distinto |
| **Menor o igual** | `<=` | ≤ | Comparación relacional menor o igual |
| **Mayor o igual** | `>=` | ≥ | Comparación relacional mayor o igual |
| **Coincidencia Regex** | `~=` | ≃ / ≈= | Coincidencia de expresión regular |
| **Concatenación** | `..` | ‥ | Unión de cadenas de texto |
| **Concatenación asignación** | `..=` | ‥= | Concatena y reasigna |
| **Fusión nula** | `??` | ?? compacto | Respaldo ante valores nulos |
| **Fusión nula asignación** | `??=` | ??= continuo | Asigna solo si la variable es nula |
| **Potencia alternativa** | `**` | ** compacto / estrella doble | Operador alternativo de exponenciación (alias de `^`) |
| **Incremento** | `++` | ++ continuo | Aumenta en 1 |
| **Decremento** | `--` | ―― continuo | Disminuye en 1 |
| **Y Lógico (AND)** | `&&` | && estilizado | Operador lógico conjuntivo |
| **O Lógico (OR)** | `\|\|` | ‖ / dos barras unidas | Operador lógico disyuntivo |
| **Asignaciones compuestas** | `+=`, `-=`, `*=`, `/=`, `%=` | 🠚 barra continua | Operación aritmética y reasignación |
| **Comentario de línea** | `//` | // inclinado continuo | Comentario alternativo de código |
| **Comentario multilínea** | `/*` y `*/` | /\* y \*/ unidos | Apertura y cierre de bloque de comentario |

> 📖 **Recordatorio para la lectura:**  
> A partir de este capítulo y en todos los ejemplos posteriores de la documentación oficial, se mantendrá el renderizado con ligaduras activadas para garantizar una presentación visual profesional y limpia. Ten siempre presente esta tabla como referencia si algún glifo te resulta desconocido.

### 1.8 Operadores Aritméticos

Estos son los operadores que realizan **cálculos matemáticos**. Funcionan exactamente como en matemáticas escolares.

#### Operadores Básicos

```latino
a = 10
b = 3

suma = a + b           # 13
resta = a * b          # 7
multiplicacion = a * b # 30
division = a / b       # 3.333...
```

#### Módulo (Resto de la División) `%`

Devuelve el **residuo** de dividir un número entre otro. Es extremadamente útil para:

* Detectar números pares/impares
* Ciclos repetitivos (cada N elementos)
* Validaciones (múltiplos de algo)

```latino
resto = 10 % 3
escribir(resto)  # Salida: 1 (10 ÷ 3 = 3 con residuo 1)

# Casos comunes
escribir(10 % 2)  # 0 (par)
escribir(11 % 2)  # 1 (impar)
escribir(15 % 5)  # 0 (múltiplo de 5)
```

* **Uso Práctico: Detector de Pares**

```latino
numero = anumero(pedir("Ingresa un número: "))    # Entrada simulada: 14

si numero % 2 == 0
    escribir("Es par")
sino
    escribir("Es impar")
fin
```

#### Potencia `^` y `**`

Eleva una base a un exponente. Latino.NET admite tanto el circunflejo tradicional (`^`) como el doble asterisco (`**`), este último familiar para desarrolladores provenientes de Python o JavaScript:

```latino
# Ambas formas son exactamente equivalentes
cuadrado1 = 5^2        # 25 (5²)
cuadrado2 = 5**2       # 25 (5²)

cubo = 2**3            # 8 (2³)
raizCuadrada = 9**0.5  # 3.0 (√9)
```

🎯 **Ejemplo: Cálculo de Áreas**

```latino
# Área de un cuadrado
lado = 5
area = lado^2
escribir("Área del cuadrado:", area)  # 25

# Volumen de un cubo
volumen = lado^3
escribir("Volumen del cubo:", volumen)  # 125
```

#### 📝 Cuidado: División por Cero

Este es el error matemático más común:

```latino
# ❌ Esto causará un error fatal
resultado = 10 / 0  # Error Runtime: División por cero
```

* **Solución: Validar antes de dividir**

```latino
divisor = 0

si divisor != 0
    resultado = 10 / divisor
    escribir(resultado)
sino
    escribir("Error: No se puede dividir por cero")
fin
```

### 🔢 Operadores de Asignación Compuesta

Son atajos que combinan una operación con asignación. Hacen el código más conciso y legible.

#### Sintaxis: `variable operador= valor`

```latino
contador = 10

# Forma larga
contador = contador + 5

# Forma corta (igual resultado)
contador += 5
```

#### Todos los Atajos Disponibles

```latino
x = 10

x += 5   # x = x + 5  → 15
x -= 3   # x = x * 3  → 12
x *= 2   # x = x * 2  → 24
x /= 4   # x = x / 4  → 6
x %= 4   # x = x % 4  → 2
x ..= " texto"  # x = x .. " texto"
```

**Para Cadenas (concatenación con `..=`):**

```latino
mensaje = "Hola"
mensaje ..= " Mundo"  # mensaje = mensaje .. " Mundo"
escribir(mensaje)     # Salida: Hola Mundo
```

* **Uso Práctico: Acumulador de Puntos**

```latino
puntaje = 0

escribir("Inicio:", puntaje)  # 0

puntaje += 10   # Primer nivel completado
escribir("Nivel 1:", puntaje)  # 10

puntaje += 25   # Segundo nivel
escribir("Nivel 2:", puntaje)  # 35

puntaje -= 5    # Penalización
escribir("Con penalización:", puntaje)  # 30
```

### ⬆️⬇️ Incremento y Decremento

Atajos especiales para sumar o restar 1 (muy común en contadores).

#### Prefijo vs Sufijo

**Prefijo (`++i`):** Incrementa **primero**, luego devuelve el valor

```latino
contador = 5
resultado = ++contador
escribir(contador)   # 6
escribir(resultado)  # 6 (devolvió el valor YA incrementado)
```

**Sufijo (`i++`):** Devuelve el valor **original**, luego incrementa

```latino
contador = 5
resultado = contador++
escribir(contador)   # 6 (se incrementó)
escribir(resultado)  # 5 (devolvió el valor ANTES de incrementar)
```

**Regla Mnemotécnica:**

* `++antes` → Cambia antes de usar
* `despues++` → Usa primero, cambia después

**Ejemplo Comparativo:**

```latino
# Prefijo
x = 10
escribir(++x)  # Salida: 11 (incrementa a 11, luego imprime)
escribir(x)    # Salida: 11

# Sufijo
y = 10
escribir(y++)  # Salida: 10 (imprime 10, luego incrementa)
escribir(y)    # Salida: 11
```

* **Uso Típico: Bucles**

```latino
contador = 0

mientras contador < 5
    escribir("Iteración:", contador)
    contador++  # Aquí no importa si es prefijo o sufijo
fin
```

### 🔗 Concatenación de Cadenas

#### Operador `..` (Concatenación)

Siempre une como texto, sin agregar espacios:

```latino
nombre = "Carlos"
apellido = "López"

# Sin espacio automático
nombreCompleto = nombre .. apellido
escribir(nombreCompleto)  # Salida: CarlosLópez

# Con espacio manual
nombreCompleto = nombre .. " " .. apellido
escribir(nombreCompleto)  # Salida: Carlos López

# Convierte números automáticamente
frase = "El valor es " .. 42
escribir(frase)  # Salida: El valor es 42
```

### 🧠 Operadores Lógicos

Estos operadores trabajan con valores de verdad (`verdadero`/`falso`) y son fundamentales para **tomar decisiones** en el código.

#### Y Lógico (`ý` / `&&`)

Devuelve `verdadero` **solo si AMBAS** condiciones son verdaderas:

```latino
edad = 20
tieneLicencia = verdadero

# Puede manejar si tiene 18+ Y tiene licencia
puedeConducir = edad >= 18 ý tieneLicencia
escribir(puedeConducir)  # cierto

# Sintaxis alternativa (mismo resultado)
puedeConducir = edad >= 18 && tieneLicencia
```

**Tabla de Verdad:**

| Condición A | Condición B | A `ý` B |
| :-- | :-- | :-- |
| cierto | cierto | cierto |
| verdadero | falso | falso |
| falso | verdadero | falso |
| falso | falso | falso |

🎯 **Ejemplo Práctico: Validación de Acceso**

```latino
edad = pedir("Tu edad: ")                        # Entrada simulada: 20
tieneBoleto = pedir("¿Tienes boleto? (si/no): ") # Entrada simulada: si

esAdulto = anumero(edad) >= 18
tienePase = tieneBoleto == "si"

puedeEntrar = esAdulto ý tienePase

si puedeEntrar
    escribir("✅ Acceso permitido")
sino
    escribir("❌ Acceso denegado")
fin
```

#### O Lógico (`ó` / `||`)

Devuelve `verdadero` si **AL MENOS UNA** condición es verdadera:

```latino
esFinDeSemana = verdadero
esFeriado = falso

puedeDescansar = esFinDeSemana ó esFeriado
escribir(puedeDescansar)  # cierto

# Sintaxis alternativa
puedeDescansar = esFinDeSemana || esFeriado
```

**Tabla de Verdad:**

| Condición A | Condición B | A `ó` B |
| :-- | :-- | :-- |
| cierto | cierto | cierto |
| cierto | falso | cierto |
| falso | cierto | cierto |
| falso | falso | falso |

🎯 **Ejemplo: Descuentos en Tienda**

```latino
esEstudiante = verdadero
esAdultoMayor = falso

tieneDescuento = esEstudiante ó esAdultoMayor

si tieneDescuento
    escribir("💰 Aplica descuento del 20%")
sino
    escribir("Precio regular")
fin
```

#### Negación (`no` / `!`)

Invierte el valor lógico:

```latino
activo = verdadero
inactivo = no activo  # falso

escribir(activo)    # cierto
escribir(inactivo)  # falso

# Sintaxis alternativa
inactivo = !activo
```

* **Uso Práctico: Validar Campos Vacíos**

```latino
nombre = pedir("Tu nombre: ")  # Entrada simulada: ""

estaVacio = nombre == ""
noEstaVacio = no estaVacio

si noEstaVacio
    escribir("Bienvenido,", nombre)
sino
    escribir("Error: Debes ingresar tu nombre")
fin
```

#### Combinaciones Complejas

Puedes combinar múltiples operadores lógicos:

```latino
edad = 25
tieneLicencia = verdadero
tieneSeguro = falso

# Puede conducir si:
# * Tiene 18+ años Y
# * Tiene licencia Y
# * Tiene seguro
puedeConducirLegal = edad >= 18 ý tieneLicencia ý tieneSeguro

escribir(puedeConducirLegal)  # falso (le falta el seguro)
```

**Con Paréntesis para Claridad:**

```latino
esEstudiante = verdadero
esEmpleado = falso
edad = 20

# Descuento si:
# (Es estudiante O empleado) Y es menor de 25
tieneDescuento = (esEstudiante ó esEmpleado) ý edad < 25
escribir(tieneDescuento)  # cierto
```

### 🛡️ Operadores de Nulidad y Condicionales

Latino.NET incluye operadores modernos para hacer tu código más seguro y corto al trabajar con valores que podrían no existir (`nulo`) o tomar decisiones rápidas.

#### Fusión Nula (`??`)

Devuelve el valor de la izquierda si existe (no es nulo). Si es nulo, devuelve el valor de la derecha por defecto.

```latino
# Si 'usuario' es nulo, usará "Invitado"
nombre = usuario ?? "Invitado"

# Muy útil al leer diccionarios:
config = {}
puerto = config["puerto"] ?? 8080
escribir(puerto) # Salida: 8080 (porque la clave no existía)
```

#### La Fusión Nula no necesita defensiva previa

A diferencia de JavaScript o Python, donde consultar una variable que nunca se declaró detiene el programa (`ReferenceError` / `NameError`), en Latino.NET `??` resuelve la pregunta *«¿existe?»* por ti. El motor intercepta la ausencia de la variable antes de evaluarla y aplica el valor por defecto:

```latino
# 'config_puerto' no fue declarada en ninguna parte: no crashea
puerto = config_puerto ?? 8080
escribir(puerto)             # Salida: 8080

# Igual de seguro con una clave ausente de un diccionario
ajustes = { "tema": "oscuro" }
puerto = ajustes["puerto"] ?? 8080
escribir(puerto)             # Salida: 8080

# Y si sí existe, gana el valor real
ajustes = { "puerto": 3000 }
puerto = ajustes["puerto"] ?? 8080
escribir(puerto)             # Salida: 3000
```

En lugar del `typeof x !== 'undefined' ? x : defecto` de JavaScript o el `try/except NameError` de Python, escribes una línea. Es una decisión de diseño deliberada del motor, no un accidente.

> **Comportamiento seguro de `??`:**  
> El operador de fusión nula provee valores por defecto de respaldo cuando una variable no ha sido definida o una clave de diccionario no existe. A diferencia de un bloque `intentar`, **no oculta errores lógicos de cálculo**: si la expresión izquierda falla por un error real de ejecución (como una división por cero o un índice inválido), el motor emitirá el `RuntimeError` correspondiente para no enmascarar fallos de programación.

#### Asignación de Fusión Nula (`??=`)

Asigna un valor a una variable o propiedad **solo si actualmente es nula** (o no existe). Si ya tiene un valor, lo respeta y no hace nada.

```latino
configuracion = {}

# Como 'tema' no existe, lo crea y le asigna "oscuro"
configuracion["tema"] ??= "oscuro"

# Como 'tema' ya es "oscuro" (no nulo), IGNORA esta línea:
configuracion["tema"] ??= "claro" 

escribir(configuracion["tema"]) # Salida: oscuro
```

#### Operador Ternario (`? :`)

Es como un `si ... sino` comprimido en una sola línea. Evalúa una condición: si es cierta, devuelve el primer valor; si es falsa, devuelve el segundo.

```latino
# Sintaxis: condicion ? valor_si_cierto : valor_si_falso
edad = 20
estado = edad >= 18 ? "Adulto" : "Menor"
escribir(estado) # Salida: Adulto
```

### ⚖️ Operadores Relacionales

Comparan dos valores y devuelven `verdadero` o `falso`.

```latino
a = 10
b = 20

# Igualdad (estricta, NO convierte tipos)
escribir(a == b)     # falso
escribir("10" == 10) # falso (tipos diferentes)

# Diferente
escribir(a != b)     # cierto

# Menor que (sí convierte strings numéricos)
escribir(a < b)      # cierto
escribir("50" < 100) # cierto (smart casting)

# Menor o igual
escribir(a <= b)     # cierto

# Mayor que
escribir(a > b)      # falso

# Mayor o igual
escribir(a >= b)     # falso
```

**Comparación de Cadenas:**

```latino
nombre1 = "Ana"
nombre2 = "Carlos"

# Orden alfabético (lexicográfico)
escribir(nombre1 < nombre2)  # cierto ("A" viene antes que "C")

# Comparación numérica con Smart Casting (al menos un operando es número):
escribir("100" > 50)         # cierto (Smart Casting convierte "100" a 100)
escribir(anumero("100") > anumero("50")) # cierto (conversión explícita recomendada)

# Comparación alfabética (lexicográfica entre dos cadenas):
escribir("100" > "50")       # falso (el carácter "1" va antes que "5" en el alfabeto)
```

#### Coincidencia con Expresión Regular `~=`

Este operador especial verifica si una cadena **coincide** con un patrón (expresión regular):

```latino
email = "usuario@ejemplo.com"

# Verificar si contiene "@"
tieneArroba = email ~= "@"
escribir(tieneArroba)  # cierto

# Verificar formato de email simple
# ⚠️ doble barra: envía "\." al motor Regex para que el punto sea LITERAL.
# Con una sola barra, "." sería comodín y aceptaría "usuario@ejemploxcom".
esEmailValido = email ~= ".+@.+\\..+"
escribir(esEmailValido)  # cierto
```

**Nota:** Las expresiones regulares son un tema avanzado. Por ahora, solo necesitas saber que `~=` verifica patrones de texto.

### 🌊 Operadores de Flujo

Estos operadores especiales mejoran la forma en que fluyen los datos en tu programa, haciendo el código más limpio y fácil de leer.

#### Intercambio (Swap) `~`

Permite intercambiar valores en una sola línea, de forma limpia y directa, sin necesidad de declarar una variable temporal en la memoria.

**Poder extendido:** A diferencia de otros lenguajes, el operador `~` en Latino.NET es de primer nivel. Funciona perfectamente tanto con variables simples como con **índices de listas, claves de diccionarios y propiedades de objetos**.

```latino
# 1. Variables simples
a = 10
b = 20
a ~ b             # 'a' ahora es 20, 'b' es 10

# 2. Índices de listas
arr = ["Rojo", "Azul", "Verde"]
arr[0] ~ arr[2]
escribir(arr)     # ["Verde", "Azul", "Rojo"]

# 3. Diccionarios y Propiedades de Clases
config = {"tema": "oscuro", "previo": "claro"}
config["tema"] ~ config["previo"]

# En el contexto de un método de clase (fragmento ilustrativo):
clase Punto
    funcion intercambiar()
        mi.coordenada_x ~ mi.coordenada_y
    fin
fin
```

#### Tubería (Pipeline) `|>`

El operador Pipeline toma el resultado de la izquierda y lo inyecta automáticamente como el **primer argumento** de la función de la derecha. Es ideal para encadenar operaciones y leer el código de izquierda a derecha de forma natural.

**1. Inyección Simple (Sin argumentos extra):**

```latino
# Sin Pipeline (Anidado * Difícil de leer):
resultado = acadena(mate.redondear(mate.abs(-42.7)))

# Con Pipeline (Lineal * Flujo natural):
resultado = -42.7 |> mate.abs |> mate.redondear |> acadena
```

**2. Llamadas Parciales (Con argumentos extra):**
Si la función de la derecha requiere más argumentos, los colocas normalmente. El Pipeline inyectará el dato de la izquierda en la **primera posición**, empujando los demás a la derecha.

```latino
# mate.pot(base, exponente)
# El 10 entra como la 'base', el 3 queda como 'exponente'
resultado = 10 |> mate.pot(3)  
escribir(resultado) # 1000
```

**3. Inyección en Métodos de Instancia / Colecciones:**
Funciona perfectamente con métodos de diccionarios, listas o clases.

```latino
miLista = [1, 2, 3]

# Inyecta el 4 como el elemento a agregar en la lista
4 |> miLista.agregar()

escribir(miLista) # [1, 2, 3, 4]
```

### 🔑 Operador de Acceso a Miembros (`.`)

El punto (`.`) es el **puente** entre un contenedor y su contenido. Se usa para:

1. Acceder a **funciones de librerías**
2. Acceder a **propiedades de objetos**
3. Llamar a **métodos de objetos**

**Léelo como:** "de", "dentro de", o "perteneciente a"

#### Uso 1: Funciones de Librería

```latino
# La constante PI "de" la librería mate
valorPi = mate.pi
escribir(valorPi)  # 3.14159...

# La función raíz cuadrada "de" mate
raiz = mate.raiz(25)
escribir(raiz)  # 5.0

# Longitud "de" la librería cadena
unTexto = "Hola"
tamano = cadena.longitud(unTexto)
escribir(tamano)  # 4
```

#### Uso 2: Propiedades de Objetos

```latino
persona = {
    "nombre": "Carlos",
    "edad": 28
}

# Acceso con punto (si la clave es un identificador válido)
escribir(persona.nombre)  # Carlos
escribir(persona.edad)    # 28
```

#### Uso 3: Métodos en POO

```latino
clase Persona
    constructor(nombre)
        mi.nombre = nombre
    fin
    
    funcion saludar()
        escribir("Hola, soy " .. mi.nombre)
    fin
fin

# El método "saludar" pertenece a "usuario"
usuario = nueva Persona("Ana")
usuario.saludar()  # Salida: Hola, soy Ana
```

### 🎯 Precedencia de Operadores

Cuando hay múltiples operadores en una expresión, Latino.NET los evalúa en un **orden específico** (como en matemáticas):

**Orden (de mayor a menor prioridad):**

1. **Agrupación, Llamadas y Accesos:** `( )`, `[ ]`, `.`
2. **Unarios (Prefijo y Sufijo):** `no` / `!`, `-` (negación), `++`, `--`
3. **Potencia:** `^`
4. **Multiplicación, División, Módulo:** `*`, `/`, `%`
5. **Suma, Resta:** `+`, `-`
6. **Relacionales:** `<`, `>`, `<=`, `>=`
7. **Concatenación:** `..`
8. **Igualdad y Regex:** `==`, `!=`, `~=`
9. **Y Lógico (AND):** `ý` / `&&`
10. **O Lógico (OR):** `ó` / `||`
11. **Fusión Nula:** `??`
12. **Tubería (Pipeline):** `|>`
13. **Ternario:** `? :`
14. **Asignación:** `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `..=`, `??=`

**Ejemplo sin Paréntesis:**

```latino
resultado = 5 + 3 * 2
escribir(resultado)  # 11 (no 16)
# Primero: 3 * 2 = 6
# Luego: 5 + 6 = 11
```

**Ejemplo con Paréntesis:**

```latino
resultado = (5 + 3) * 2
escribir(resultado)  # 16
# Primero: 5 + 3 = 8
# Luego: 8 * 2 = 16
```

**Buena Práctica:** Usa paréntesis para **claridad**, aunque no sean técnicamente necesarios. Observa cómo combinamos los operadores lógicos correctos (`ý`, `ó`):

```latino
# Menos claro
valido = edad >= 18 ý tieneLicencia ó esInstructor

# Más claro (misma lógica, mejor legibilidad)
valido = (edad >= 18 ý tieneLicencia) ó esInstructor
```

🎯 **Ejemplo: Calculadora de Índice de Masa Corporal con operadores modernos**

```latino
# Calculadora de Índice de Masa Corporal
escribir("=== CALCULADORA DE IMC ===\n")

# Entrada de datos interactiva con valores simulados
nombre = pedir("Tu nombre: ") ?? "Usuario"          # Entrada simulada: Eric
edad = anumero(pedir("Tu edad: ")) ?? 0             # Entrada simulada: 28
peso = anumero(pedir("Tu peso (kg): "))             # Entrada simulada: 77
altura = anumero(pedir("Tu altura (m): "))          # Entrada simulada: 1.75

# Validación
si peso == nulo ó altura == nulo
    escribir("❌ Error: peso y altura deben ser números válidos")
sino
    si peso <= 0 ó altura <= 0
        escribir("❌ Error: peso y altura deben ser mayores a cero")
    sino
        imc = peso / (altura ^ 2)
        
        # Clasificación con operador ternario anidado
        clasificacion = imc < 18.5 ? "Bajo peso" :
                        imc < 25 ? "Normal" :
                        imc < 30 ? "Sobrepeso" : "Obesidad"
        
        # Salida formateada
        esMayor = edad >= 18 ? "Sí" : "No"
        
        escribir("\n📊 RESULTADOS")
        escribir("═" .. cadena.rellenar_derecha("", "═", 20))
        escribir("Nombre:", nombre)
        escribir("Edad:", edad, "años (Mayor de edad:", esMayor, ")")
        escribir("IMC:", imc, "-", clasificacion)
        
        # Recomendación con operadores lógicos
        si imc < 18.5 ó imc >= 30
            escribir("⚠️ Recomendación: Consulta a un especialista")
        sino
            escribir("✅ Mantén tu estilo de vida saludable")
        fin
    fin
fin
```

**Salida de Ejemplo:**

```text
=== CALCULADORA DE IMC ===

Tu nombre: Eric
Tu edad: 28
Tu peso (kg): 77
Tu altura (m): 1.75

📊 RESULTADOS
═════════════════════
Nombre: Eric
Edad: 28 años (Mayor de edad: Sí)
IMC: 25.142857142857142 * Sobrepeso
✅ Mantén tu estilo de vida saludable
```
