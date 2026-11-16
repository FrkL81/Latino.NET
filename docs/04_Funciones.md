# CAPÍTULO 4: Funciones y Procedimientos

## El Arte de Dividir y Conquistar

En los capítulos anteriores aprendiste a tomar decisiones con estructuras de control y a organizar volúmenes de información en colecciones estructuradas. Sin embargo, conforme un programa crece, surge un nuevo desafío: **la gestión de la complejidad y la reutilización del código**.

Escribir el mismo bloque de cálculo cinco veces en distintas partes de un script no solo ensucia el archivo, sino que crea una trampa de mantenimiento: si la fórmula cambia o descubres un error, estás obligado a recordar y corregir cada copia idéntica. Si olvidas una sola, el sistema queda inconsistente.

Para evitar esto aplicamos el principio clásico de la ingeniería: **dividir y conquistar**. Las funciones permiten encapsular una secuencia de instrucciones bajo un nombre descriptivo, aislando su lógica interna del resto del programa y transformando problemas gigantescos en pequeñas piezas independientes, predecibles y fáciles de probar.

### Procedimientos vs. Funciones: ¿Cuál es la Diferencia?

Aunque comúnmente se usan como sinónimos, en las ciencias de la computación y en Latino.NET responden a dos intenciones distintas:

- **Un Procedimiento:** Es una rutina orientada a la **acción y a los efectos secundarios**. Su propósito es alterar el estado del sistema: escribir en consola (`escribir`), guardar un registro en disco (`archivo.escribir`) o pausar la ejecución (`sis.dormir`). En Latino.NET, los procedimientos terminan su labor y retornan implícitamente el valor `nulo`.
- **Una Función:** Es un modelo inspirado en las **matemáticas puras**. Recibe datos de entrada (parámetros), aplica una transformación o cálculo y devuelve un resultado concreto mediante la directiva `retornar`. Una buena función minimiza sorpresas: para los mismos argumentos de entrada, siempre produce el mismo resultado predecible.

### Ciudadanas de Primera Clase (*First-Class Citizens*)

En Latino.NET las funciones no son simples etiquetas estáticas de código como en C tradicional o Pascal; son **valores de primera clase**. Esto significa que una función tiene exactamente la misma jerarquía que un número, una cadena o una lista:

1. **Se pueden asignar a variables** para crear alias o despachadores dinámicos.
2. **Se pueden almacenar dentro de listas y diccionarios** para construir tablas de comandos y librerías de utilidades.
3. **Se pueden pasar como argumentos a otras funciones** (funciones de orden superior como `lista.filtrar` o `lista.transformar`).
4. **Se pueden engendrar y retornar desde otras funciones** para crear fábricas de lógica y encapsulamiento mediante **clausuras léxicas (*closures*)**.

> 💡 **Principio de Responsabilidad Única:**  
> Una función excelente hace **una sola cosa y la hace de forma impecable**. Si al describir tu función necesitas usar la palabra *"y"* (por ejemplo: *"calcula el salario y guarda en la base de datos"*), es una señal inequívoca de que debes dividirla en dos componentes independientes.

### El Problema del Código Duplicado

Imagina que estás construyendo una calculadora. Sin funciones, tu código se vería así:

```latino
# ❌ Sin funciones: Código repetitivo y frágil
escribir("=== Operación 1 ===")
a = 10
b = 5
resultado1 = a + b
escribir("Suma: " .. a .. " + " .. b .. " = " .. resultado1)

escribir("\n=== Operación 2 ===")
c = 20
d = 8
resultado2 = c + d
escribir("Suma: " .. c .. " + " .. d .. " = " .. resultado2)

escribir("\n=== Operación 3 ===")
e = 15
f = 3
resultado3 = e + f
escribir("Suma: " .. e .. " + " .. f .. " = " .. resultado3)
```

**Problemas:**

- ❌ **15 líneas** para 3 sumas simples
- ❌ Si cambias el formato, debes editar **3 lugares**
- ❌ Copiar-pegar = **duplicar bugs**
- ❌ Imposible escalar a 100 operaciones

**Con funciones:**

```latino
# ✅ Con funciones: Limpio, reutilizable y mantenible
funcion MostrarSuma(a, b)
    resultado = a + b
    escribir("Suma: " .. a .. " + " .. b .. " = " .. resultado)
fin

escribir("=== Operación 1 ===")
MostrarSuma(10, 5)

escribir("\n=== Operación 2 ===")
MostrarSuma(20, 8)

escribir("\n=== Operación 3 ===")
MostrarSuma(15, 3)
```

**Beneficios:**

- ✅ **Código reducido**: De 15 a 10 líneas (33% menos)
- ✅ **Un solo lugar** para cambiar el formato
- ✅ **Bugs centralizados**: Arreglas una vez, funciona siempre
- ✅ **Escalable**: 1,000 operaciones = 1,000 llamadas de 1 línea

### Analogía: La Fábrica de Código

Piensa en las funciones como **máquinas en una fábrica**:

```text
┌─────────────────────────────────────┐
│       MÁQUINA: Sumar                │
├─────────────────────────────────────┤
│ Entrada: [Número A] [Número B]      │
│                                     │
│ Proceso: A + B                      │
│                                     │
│ Salida:  [Resultado]                │
└─────────────────────────────────────┘
```

**Ventajas de la fábrica:**

1. **Especialización** - Cada máquina hace UNA cosa bien
2. **Reutilización** - Usas la misma máquina 1,000 veces
3. **Mantenimiento** - Si falla, sabes dónde buscar
4. **Composición** - Conectas máquinas para procesos complejos

## 4.1 Fundamentos de Funciones

### ¿Qué Es Una Función?

Una **función** es un bloque de código reutilizable con:

1. **Nombre** - Cómo la identificas
2. **Parámetros** - Datos que recibe (entrada)
3. **Cuerpo** - Código que ejecuta (proceso)
4. **Retorno** - Dato que devuelve (salida)

**Anatomía de una función:**

```latino
┌─────────────────────────────────────┐
│  funcion NombreFuncion(parametro)   │ ← Firma (signature)
│      # Cuerpo de la función         │
│      local resultado = parametro * 2│ ← Proceso
│      retornar resultado             │ ← Retorno
│  fin                                │ ← Cierre
└─────────────────────────────────────┘
```

### Sintaxis Básica

Latino.NET ofrece **dos formas** de definir funciones:

**Forma 1: Palabra completa `funcion`**

```latino
funcion Saludar(nombre)
    escribir("¡Hola, " .. nombre .. "!")
fin

Saludar("Ana")
```

**Forma 2: Alias corto `fun`**

```latino
fun Saludar(nombre)
    escribir("¡Hola, " .. nombre .. "!")
fin

Saludar("Ana")
```

**Son idénticas.** Usa la que prefieras por legibilidad o brevedad.

### Palabras Clave de Retorno

Para devolver un valor, tienes **tres opciones**:

| Palabra | Uso | Estilo |
| :--- | :--- | :--- |
| `retornar` | Formal, explícito | Recomendado para principiantes |
| `retorno` | Intermedio | Balance legibilidad/brevedad |
| `ret` | Compacto | Para código denso |

```latino
# Las tres son equivalentes
fun Sumar1(a, b)
    retornar a + b
fin

fun Sumar2(a, b)
    retorno a + b
fin

fun Sumar3(a, b)
    ret a + b
fin

escribir(Sumar1(5, 3))  # 8
escribir(Sumar2(5, 3))  # 8
escribir(Sumar3(5, 3))  # 8
```

### Retorno Implícito: `nulo`

Si una función **no tiene `retornar`**, devuelve automáticamente `nulo`:

```latino
fun Imprimir(mensaje)
    escribir(mensaje)
    # No hay retornar
fin

resultado = Imprimir("Hola")
escribir(resultado)  # Salida: nulo
```

**Uso común:** Funciones que **hacen** algo (imprimir, guardar archivo) en lugar de **calcular** algo.

### Regla de Oro: Definir Antes de Usar

Latino.NET es un lenguaje interpretado que evalúa el código de forma secuencial. Las funciones se registran en la memoria del entorno en el instante exacto en que el flujo de ejecución pasa por su declaración `funcion` o `fun`.

Por esta razón, en el nivel raíz de un script, **no puedes invocar una función antes de su línea de definición**:

```latino
# ❌ ERROR: Invocación previa a la declaración
Saludar("Mundo")  # RuntimeError: Variable no definida: 'Saludar'

fun Saludar(nombre)
    escribir("Hola " .. nombre)
fin
```

✅ **Forma 1: Declaración lineal directa (scripts simples)**

En utilidades lineales o scripts cortos, simplemente declara la función en las líneas superiores y llámala después:

```latino
# 1. Definir primero
fun Saludar(nombre)
    escribir("Hola " .. nombre)
fin

# 2. Llamar después
Saludar("Mundo")  # ✅ Salida: Hola Mundo
```

✅ **Forma 2: El Patrón `principal()` (código limpio y lectura natural)**

Si prefieres que la lógica principal de tu programa se lea al principio del archivo y las funciones auxiliares queden organizadas abajo, envuelve tu punto de entrada en una función `principal()` e invócala en la última línea:

```latino
funcion principal()
    # ✅ Funciona: Al ejecutarse principal(), Saludar ya fue registrada en memoria
    Saludar("Mundo")
fin

funcion Saludar(nombre)
    escribir("Hola " .. nombre)
fin

# Punto de arranque al final del archivo
principal()
```

> 🔬 **¿Por qué funciona este patrón?**  
> Las llamadas dentro del cuerpo de una función se resuelven en el momento de la ejecución (*late binding*), no durante el parseo. Como `principal()` se invoca al final del archivo, para ese momento el intérprete ya recorrió y registró todas las funciones secundarias del documento. Esto también permite que dos funciones se llamen mutuamente (recursión mutua) sin importar el orden en que fueron escritas.

✅ **Forma 3: Modularización con `incluir()` (proyectos reales)**

Cuando tu aplicación crece y deseas tener una biblioteca de funciones disponible globalmente en cualquier punto de tu código sin preocuparte por el orden de declaración, la mejor práctica de ingeniería es extraerlas a un archivo auxiliar (por ejemplo, `utilidades.lat`) y cargarlo al inicio de tu script:

```latino
# Cargar módulos al inicio del archivo
incluir("utilidades")

# Todas las funciones de utilidades.lat quedan disponibles de inmediato
Saludar("Mundo")
```

*(Aprenderás a estructurar proyectos multi-archivo, gestionar dependencias y evitar colisiones de nombres en el **Capítulo 8: Organización de Proyectos (Modularidad)**).*

## Parámetros y Argumentos

**Parámetros** = Variables en la definición  
**Argumentos** = Valores en la llamada

```latino
fun Multiplicar(a, b)  # a, b son PARÁMETROS
    ret a * b
fin

resultado = Multiplicar(5, 3)  # 5, 3 son ARGUMENTOS
```

### Múltiples Parámetros

```latino
fun calcularAreaRectangulo(base, altura)
    area = base * altura
    ret area
fin

escribir(calcularAreaRectangulo(10, 5))  # 50
```

### Sin Parámetros

🎯 **Ejemplo: Función sin parámetros para consulta del reloj del sistema**

```latino
fun ObtenerFechaActual()
    ret sis.tiempo()
fin

escribir(ObtenerFechaActual())
# Salida variable (depende del momento de ejecución y hardware):
```

### Retornar Múltiples Valores (con Diccionarios)

Latino.NET permite retornar **un solo valor**, pero puedes empaquetar múltiples datos en una estructura:

```latino
fun dividirConResto(dividendo, divisor)
    cociente = dividendo / divisor
    resto = dividendo % divisor
    
    # Retornar diccionario con ambos valores
    ret {
        "cociente": cociente,
        "resto": resto
    }
fin

# Uso
resultado = dividirConResto(17, 5)
# ⚠️ `/` es división real, no entera: 17 / 5 = 3.4 (no 3).
# Para el cociente entero usa mate.trunc(dividendo / divisor).
escribir("Cociente: " .. resultado["cociente"])  # 3.4
escribir("Resto: " .. resultado["resto"])        # 2
```

🎯 **Ejemplo Completo: Calculadora de 4 Operaciones**

```latino
# ═══════════════════════════════════════════════════════════
#  CALCULADORA BÁSICA
# ═══════════════════════════════════════════════════════════

fun Sumar(a, b)
    ret a + b
fin

fun Restar(a, b)
    ret a - b
fin

fun Multiplicar(a, b)
    ret a * b
fin

fun dividir(a, b)
    # Validación: No dividir por cero
    si b == 0
        escribir("❌ Error: División por cero")
        ret nulo
    fin
    
    ret a / b
fin

fun mostrarResultado(operacion, a, b, resultado)
    si resultado == nulo
        # No mostrar nada si hubo error
        ret
    fin
    
    escribirf("{0} {1} {2} = {3:F2}\n", a, operacion, b, resultado)
fin

# ═══════════════════════════════════════════════════════════
#  PROGRAMA PRINCIPAL
# ═══════════════════════════════════════════════════════════

num1 = 15
num2 = 3

escribir("╔════════════════════════════════════╗")
escribir("║     CALCULADORA BÁSICA             ║")
escribir("╚════════════════════════════════════╝\n")

escribirf("Operando 1: {0}\n", num1)
escribirf("Operando 2: {0}\n\n", num2)

# Realizar operaciones
mostrarResultado("+", num1, num2, Sumar(num1, num2))
mostrarResultado("-", num1, num2, Restar(num1, num2))
mostrarResultado("×", num1, num2, Multiplicar(num1, num2))
mostrarResultado("÷", num1, num2, dividir(num1, num2))

# Probar división por cero
escribir("\n--- Prueba de Error ---")
mostrarResultado("÷", num1, 0, dividir(num1, 0))
```

**Salida:**

```text
╔════════════════════════════════════╗
║     CALCULADORA BÁSICA             ║
╚════════════════════════════════════╝

Operando 1: 15
Operando 2: 3

15 + 3 = 18.00
15 - 3 = 12.00
15 × 3 = 45.00
15 ÷ 3 = 5.00

--- Prueba de Error ---
❌ Error: División por cero
```

## Validación de Aridad (Cantidad de Argumentos)

Latino.NET es **estricto** con la cantidad de argumentos. Debes pasar **exactamente** el número de parámetros definidos.

```latino
fun Saludar(nombre, apellido)
    escribir("Hola " .. nombre .. " " .. apellido)
fin

# ✅ Correcto: 2 argumentos
Saludar("Ana", "García")

# ❌ ERROR: Falta un argumento obligatorio
Saludar("Ana")
# RuntimeError: Falta el argumento obligatorio 'apellido'.

# ❌ ERROR: Exceso de argumentos
Saludar("Ana", "García", "Extra")
# RuntimeError: La función 'Saludar' esperaba máximo 2 argumentos, pero recibió 3.
```

**Beneficio:** Detecta errores temprano en lugar de comportamiento impredecible.

## Mejores Prácticas de Nombres

### Convención de Nombres

En Latino.NET, adoptamos el estilo **PascalCase** como convención nativa (primera letra mayúscula, palabras unidas con mayúsculas interiores) para funciones del usuario:

```latino
# ✅ BIEN (estilo nativo de Latino.NET)
fun CalcularImpuesto(monto)
    ret monto * 0.16
fin

fun ValidarEmail(email)
    ret cadena.contiene(email, "@")
fin
```

También puedes usar **snake_case** si lo prefieres, especialmente para funciones cortas:

```latino
# ✅ VÁLIDO (snake_case)
fun calcular_impuesto(monto)
    ret monto * 0.16
fin
```

### Nombres Descriptivos

**Usa verbos** para funciones (representan acciones):

```latino
# ✅ BIEN: Nombres claros
fun ObtenerUsuario()
fun GuardarArchivo(ruta, contenido)
fun ValidarContraseña(pass)
fun calcularPromedio(numeros)

# ❌ MAL: Nombres vagos
fun Hacer()
fun Proceso(x)
fun Temp(a, b)
fun Func1()
```

### Funciones Booleanas

Para funciones que retornan `verdadero/falso`, usa prefijos:

| Prefijo | Uso | Ejemplo |
| :--- | :--- | :--- |
| `Es` | Verifica estado | `EsMayorDeEdad(edad)` |
| `Tiene` | Verifica posesión | `TienePermisos(usuario)` |
| `Puede` | Verifica capacidad | `PuedeEditar(documento)` |
| `Debe` | Verifica obligación | `DebeActualizarse(version)` |

```latino
fun EsPar(numero)
    ret numero % 2 == 0
fin

fun TieneDescuento(cliente)
    ret cliente["tipo"] == "VIP"
fin

si EsPar(10)
    escribir("10 es par")
fin

si TieneDescuento(miCliente)
    escribir("Aplicar descuento")
fin
```

## Ejercicio Guiado: Conversor de Temperatura

Vamos a crear un sistema que convierte entre Celsius, Fahrenheit y Kelvin.

```latino
# ═══════════════════════════════════════════════════════════
#  CONVERSOR DE TEMPERATURA
# ═══════════════════════════════════════════════════════════

# Funciones de conversión individuales
fun celsiusAFahrenheit(celsius)
    ret (celsius * 9/5) + 32
fin

fun celsiusAKelvin(celsius)
    ret celsius + 273.15
fin

fun fahrenheitACelsius(fahrenheit)
    ret (fahrenheit - 32) * 5/9
fin

fun fahrenheitAKelvin(fahrenheit)
    celsius = fahrenheitACelsius(fahrenheit)  # Reutilizar función
    ret celsiusAKelvin(celsius)
fin

fun kelvinACelsius(kelvin)
    ret kelvin - 273.15
fin

fun kelvinAFahrenheit(kelvin)
    celsius = kelvinACelsius(kelvin)
    ret celsiusAFahrenheit(celsius)
fin

# Función de validación
fun temperaturaValida(temp, escala)
    # Kelvin no puede ser negativo
    si escala == "K" ý temp < 0
        ret falso
    fin
    
    # Celsius no puede ser menor a -273.15 (cero absoluto)
    si escala == "C" ý temp < -273.15
        ret falso
    fin
    
    # Fahrenheit no puede ser menor a -459.67 (cero absoluto)
    si escala == "F" ý temp < -459.67
        ret falso
    fin
    
    ret verdadero
fin

# Función de presentación
fun mostrarConversiones(valor, escalaOrigen)
    # Validar temperatura
    si !temperaturaValida(valor, escalaOrigen)
        escribir("❌ Temperatura físicamente imposible")
        ret
    fin
    
    escribir("\n╔════════════════════════════════════╗")
    escribirf("    Conversiones desde {0,3:F1}°{1}      \n", valor, escalaOrigen)
    escribir("╠════════════════════════════════════╣")
    
    # Convertir según la escala de origen
    si escalaOrigen == "C"
        f = celsiusAFahrenheit(valor)
        k = celsiusAKelvin(valor)
        escribirf("║     Fahrenheit:   {0,10:F2}°F     ║\n", f)
        escribirf("║     Kelvin:       {0,10:F2} K     ║\n", k)
    osi escalaOrigen == "F"
        c = fahrenheitACelsius(valor)
        k = fahrenheitAKelvin(valor)
        escribirf("║     Celsius:      {0,10:F2}°C     ║\n", c)
        escribirf("║     Kelvin:       {0,10:F2} K     ║\n", k)
    osi escalaOrigen == "K"
        c = kelvinACelsius(valor)
        f = kelvinAFahrenheit(valor)
        escribirf("║     Celsius:      {0,10:F2}°C     ║\n", c)
        escribirf("║     Fahrenheit:   {0,10:F2}°F     ║\n", f)
    fin
    
    escribir("╚════════════════════════════════════╝")
fin

# ═══════════════════════════════════════════════════════════
#  PRUEBAS
# ═══════════════════════════════════════════════════════════

mostrarConversiones(0, "C")      # Punto de congelación del agua
mostrarConversiones(100, "C")    # Punto de ebullición del agua
mostrarConversiones(98.6, "F")   # Temperatura corporal
mostrarConversiones(0, "K")      # Cero absoluto en Kelvin

# Probar validación
escribir("\n--- Prueba de Validación ---")
mostrarConversiones(-300, "C")   # Imposible
```

**Salida:**

```text
╔════════════════════════════════════╗
     Conversiones desde 0.0°C        
╠════════════════════════════════════╣
║  Fahrenheit:        32.00°F        ║
║  Kelvin:           273.15 K        ║
╚════════════════════════════════════╝

╔════════════════════════════════════╗
    Conversiones desde 100.0°C        
╠════════════════════════════════════╣
║  Fahrenheit:       212.00°F        ║
║  Kelvin:           373.15 K        ║
╚════════════════════════════════════╝

╔════════════════════════════════════╗
    Conversiones desde 98.6°F         
╠════════════════════════════════════╣
║  Celsius:           37.00°C        ║
║  Kelvin:           310.15 K        ║
╚════════════════════════════════════╝

╔════════════════════════════════════╗
    Conversiones desde 0.0°K         
╠════════════════════════════════════╣
║  Celsius:         -273.15°C        ║
║  Fahrenheit:      -459.67°F        ║
╚════════════════════════════════════╝

--- Prueba de Validación ---
❌ Temperatura físicamente imposible
```

**Lecciones del ejercicio:**

1. ✅ **Composición** - `FahrenheitAKelvin` usa otras dos funciones
2. ✅ **Validación centralizada** - `temperaturaValida` en un solo lugar
3. ✅ **Reutilización** - 6 conversiones con lógica compartida
4. ✅ **Nombres descriptivos** - Cada función explica qué hace

## Anti-Patrones Comunes

### 1. Funciones Demasiado Largas

```latino
# ❌ MAL: Función que hace todo (>50 líneas)
fun ProcesarPedido(pedido)
    # Validar pedido
    # ... 15 líneas
    
    # Calcular total
    # ... 10 líneas
    
    # Aplicar descuento
    # ... 10 líneas
    
    # Generar factura
    # ... 15 líneas
    
    # Enviar email
    # ... 10 líneas
fin

# ✅ BIEN: Dividir en funciones especializadas
fun ValidarPedido(pedido)
    # ... solo validación
fin

fun CalcularTotal(pedido)
    # ... solo cálculo
fin

fun GenerarFactura(pedido)
    # ... solo factura
fin

fun ProcesarPedido(pedido)
    si !ValidarPedido(pedido)
        ret falso
    fin
    
    total = CalcularTotal(pedido)
    factura = GenerarFactura(pedido)
    ret factura
fin
```

**Regla:** Si una función tiene más de **20-30 líneas**, probablemente debe dividirse.

### 2. Nombres Genéricos

```latino
# ❌ MAL
fun Hacer(a, b)
    ret a + b
fin

fun Proceso(datos)
    # ¿Qué proceso?
fin

# ✅ BIEN
fun calcularSuma(a, b)
    ret a + b
fin

fun ValidarFormatoEmail(email)
    ret cadena.contiene(email, "@")
fin
```

### 3. Efectos Secundarios Ocultos

```latino
# ❌ MAL: Modifica variables globales sin avisar
global total = 0

fun Sumar(a, b)
    total = total + a + b  # ¡Efecto secundario oculto!
    ret a + b
fin

# ✅ BIEN: Explícito en el nombre
global total = 0

fun SumarYAcumular(a, b)
    resultado = a + b
    total = total + resultado
    ret resultado
fin
```

### 4. Demasiados Parámetros

```latino
# ❌ MAL: 7 parámetros (difícil de recordar)
fun CrearUsuario(nombre, apellido, email, telefono, direccion, ciudad, pais)
    # ...
fin

# ✅ BIEN: Usar un diccionario
fun CrearUsuario(datos)
    nombre = datos["nombre"]
    email = datos["email"]
    # ...
fin

# Uso
usuario = CrearUsuario({
    "nombre": "Ana",
    "apellido": "García",
    "email": "ana@example.com",
    "telefono": "123-456",
    "direccion": "Calle 1",
    "ciudad": "Panamá",
    "pais": "PA"
})
```

**Regla:** Si tienes más de **4 parámetros**, considera agruparlos en una estructura.

## 4.2 Parámetros Flexibles

### El Problema de los Parámetros Obligatorios

Imagina una función de conexión a base de datos:

```latino
# ❌ Problema: Todos los parámetros obligatorios
fun conectarBD(host, puerto, usuario, contrasena, timeout, pool, ssl)
    # ...
fin

# Uso típico (siempre los mismos valores)
conectarBD("localhost", 3306, "root", "admin", 30, 10, falso)
conectarBD("localhost", 3306, "root", "admin", 30, 10, falso)
conectarBD("localhost", 3306, "root", "admin", 30, 10, falso)
```

**Problemas:**

- ❌ Repetir los mismos valores una y otra vez
- ❌ Fácil equivocarse en el orden
- ❌ Difícil de leer (¿qué es el `30`? ¿el `10`?)

### Parámetros Opcionales con Valores por Defecto

Latino.NET permite definir **valores predeterminados** para parámetros. Si el usuario no envía ese argumento, se usa el valor por defecto.

**Sintaxis:**

```latino
fun NombreFuncion(parametro_obligatorio, parametro_opcional = valor_defecto)
    # código
fin
```

**Ejemplo mejorado:**

```latino
# ✅ Solución: Valores por defecto para casos comunes
fun conectarBD(host, puerto = 3306, usuario = "root", contrasena = "", timeout = 30)
    escribirf("Conectando a {0}:{1}\n", host, puerto)
    escribirf("Usuario: {0}\n", usuario)
    escribirf("Timeout: {0} segundos\n", timeout)
fin

# Ahora es mucho más flexible
conectarBD("localhost")                          # Usa todos los defaults
conectarBD("192.168.1.1", 5432)                  # Solo cambia puerto
conectarBD("servidor.com", 3306, "admin", "123") # Personalizado
```

**Salida:**

```text
Conectando a localhost:3306
Usuario: root
Timeout: 30 segundos

Conectando a 192.168.1.1:5432
Usuario: root
Timeout: 30 segundos

Conectando a servidor.com:3306
Usuario: admin
Timeout: 30 segundos
```

### Regla Crítica: Orden de Parámetros

Los parámetros opcionales **siempre van al final**:

```latino
# ❌ ERROR: Opcional antes que obligatorio
fun calcular(a = 10, b)  # ¡Error de sintaxis!
    ret a + b
fin

# ✅ CORRECTO: Obligatorios primero, opcionales después
fun calcular(b, a = 10)
    ret a + b
fin
```

**Razón:** Latino.NET asigna argumentos por **posición**, no por nombre. Si los opcionales van primero, no sabría qué valor darles.

🎯 **Ejemplo: Sistema de Logging Flexible**

```latino
fun RegistrarLog(mensaje, nivel = "INFO", mostrarFecha = verdadero, miArchivo = nulo)
    # Construir línea de log
    local linea = ""
    
    # Agregar timestamp si se solicita
    si mostrarFecha
        linea = linea .. "[" .. sis.tiempo() .. "] "
    fin
    
    # Agregar nivel
    linea = linea .. nivel .. ": " .. mensaje
    
    # Mostrar en consola
    escribir(linea)
    
    # Guardar en miArchivo si se especificó
    si miArchivo != nulo
        intentar
            # Anexar al archivo: usa archivo.anexar para agregar contenido
            archivo.anexar(miArchivo, linea .. "\n")
        atrapar (err)
            escribir("⚠️  No se pudo escribir en log: " .. err)
        fin
    fin
fin

# Uso flexible
RegistrarLog("Aplicación iniciada")
RegistrarLog("Conectando a BD", "DEBUG")
RegistrarLog("Error crítico", "ERROR", verdadero, "app.log")
RegistrarLog("Proceso completado", "INFO", falso)

# Limpiar archivo temporal
archivo.borrar("app.log")
```

```text
Salida variable (depende de la fecha/hora del sistema)
[25/09/2026 2:47:14 AM] INFO: Aplicación iniciada
[25/09/2026 2:47:14 AM] DEBUG: Conectando a BD
[25/09/2026 2:47:14 AM] ERROR: Error crítico
INFO: Proceso completado
```

## Validación de Parámetros y Seguridad de Tipos

Latino.NET es un lenguaje **dinámicamente tipado**, lo que significa que una variable puede contener cualquier tipo de dato. Sin embargo, su motor matemático es **estricto y seguro** con reglas bien definidas.

A diferencia de otros lenguajes que intentan realizar operaciones extrañas (como multiplicar texto), Latino.NET lanzará un error explícito si intentas realizar operaciones matemáticas con datos que no pueden convertirse a números.

### Smart Casting: Reglas Claras de Conversión Automática

Latino.NET aplica **conversión automática (smart casting)** en situaciones específicas, siguiendo estas reglas:

| Contexto | Conversión Aplicada | Ejemplo |
| :--- | :--- | :--- |
| **Operadores aritméticos** (`+ - * / %`) | Strings numéricos → Número | `"10" * 2 = 20` |
| **Desigualdades** (`< > <= >=`) | Strings numéricos → Número | `"100" > 50` es `cierto` |
| **Booleanos en aritmética** | `verdadero`=1, `falso`=0 | `verdadero + 5 = 6` |
| **Nulo en aritmética** | `nulo` = 0 | `nulo + 5 = 5` |
| **Funciones nativas** (`rango`, `mate.raiz`) | **NO** convierten | Requieren `anumero()` |
| **Igualdad (`==`)** | **NO** convierte (estricta) | `"100" == 100` es `falso` |

### El Operador de Suma y Smart Casting

Contrario a la creencia popular, el operador `+` **NO** es polimórfico para concatenación. En Latino.NET, la concatenación de cadenas tiene su **propio operador**: `..` (doble punto).

```latino
a = 10
b = "20"
c = "Hola"

# ✅ CONCATENACIÓN (Operador correcto)
escribir(a .. b)     # Salida: 1020 (cadena de texto)
escribir("Edad: " .. a) # Salida: "Edad: 10" (Texto)

# ✅ SUMA ARITMÉTICA (Smart casting aplica)
escribir(a + b)      # Salida: 30 (Número - ¡ambos se convierten!)
escribir(a + 5)      # Salida: 15 (Número)

# ❌ ¿Qué pasa con texto no numérico?
# escribir(a + c)    # Error: No se puede convertir 'Hola' a número
```

> **⚠️ Punto Crítico:**
> El operador `+` **SIEMPRE** intenta realizar suma aritmética. Si ambos operandos (o sus versiones convertidas) son números, funciona. Si no, lanza error. La confusión surge porque `"20"` se convierte a número, no porque `+` haga concatenación.

### Comportamiento por Defecto de los Operadores

Todos los operadores aritméticos (`+`, `-`, `*`, `/`, `%`, `^`) siguen las mismas reglas de smart casting:

```latino
funcion Duplicar(numero)
    retornar numero * 2
fin

escribir(Duplicar(5))         # 10 (Correcto)
escribir(Duplicar(verdadero)) # 2  (verdadero=1)
escribir(Duplicar("10"))       # 20 (Smart casting: "10" → 10)

# ⚠️ ERROR CONTROLADO:
escribir(Duplicar("Hola"))    
# Error Runtime: No se puede convertir 'Hola' a número.
```

**¿Por qué es seguro?**
El sistema detiene la ejecución inmediatamente al encontrar un valor que no puede convertirse a número, evitando resultados incoherentes o datos corruptos.

### Validación Explícita de Tipos

Para funciones críticas que recibirán datos de fuentes externas, puedes validar explícitamente usando `tipo()`:

```latino
funcion DuplicarSeguro(n)
    # Los tipos aceptados automáticamente:
    # - "decimal": números
    # - "logico": booleanos (se convierten)
    # - "cadena": strings numéricos (con anumero() explícito)
    
    si tipo(n) == "cadena"
        n = anumero(n)
        si n == nulo
            escribir("⚠️ Error: Cadena no numérica")
            retornar nulo
        fin
    fin
    
    si tipo(n) != "decimal" ý tipo(n) != "logico"
        escribir("⚠️ Error: Tipo no soportado: " .. tipo(n))
        retornar nulo
    fin
    
    retornar n * 2
fin

escribir(DuplicarSeguro("20"))    # 40
escribir(DuplicarSeguro("Hola"))  # ⚠️ Error: Cadena no numérica + nulo
escribir(DuplicarSeguro([1,2]))   # ⚠️ Error: Tipo no soportado: lista
```

### Resumen Rápido

| Operación | Comportamiento | ¿Smart Casting? |
| :--- | :--- | :--- |
| `+ - * / % ^` | Aritmética pura | ✅ Strings numéricos, booleanos, nulo |
| `< > <= >=` | Comparación numérica | ✅ Strings numéricos |
| `== !=` | Igualdad estricta | ❌ Sin conversión |
| `..` | Concatenación | ✅ Convierte todo a texto |
| Funciones nativas | Según su implementación | ❌ Generalmente no |

**Patrón recomendado:** Valida tipos al inicio de funciones públicas o que reciban entrada del usuario.

### Tabla de Tipos en Latino.NET

| Tipo | Retorno de `tipo()` | Ejemplo |
| :--- | :--- | :--- |
| Número | `"decimal"` | `42`, `3.14` |
| Cadena | `"cadena"` | `"Hola"` |
| Booleano | `"logico"` | `verdadero`, `falso` |
| Lista | `"lista"` | `[1, 2, 3]` |
| Diccionario | `"diccionario"` | `{"a": 1}` |
| Función | `"funcion"` | `fun() ret 5 fin` |
| Nulo | `"nulo"` | `nulo` |

### Validación Robusta con Múltiples Tipos

```latino
fun calcularAreaCirculo(radio)
    # Validar tipo
    si tipo(radio) != "decimal"
        escribir("❌ Error: El radio debe ser un número")
        ret nulo
    fin
    
    # Validar rango
    si radio <= 0
        escribir("❌ Error: El radio debe ser positivo")
        ret nulo
    fin
    
    # Cálculo
    area = mate.pi * (radio ^ 2)
    ret area
fin

# Pruebas
escribir(calcularAreaCirculo(5))        # 78.53981633974483
escribir(calcularAreaCirculo("cinco"))  # ❌ Error: El radio debe ser un número + nulo
escribir(calcularAreaCirculo(-3))       # ❌ Error: El radio debe ser positivo + nulo
```

### Patrón: Función de Validación Centralizada

Para validaciones complejas, crea funciones especializadas:

```latino
fun validarEmail(email)
    # Verificar tipo
    si tipo(email) != "cadena"
        ret {"valido": falso, "error": "Debe ser texto"}
    fin
    
    # Verificar longitud
    si cadena.longitud(email) < 5
        ret {"valido": falso, "error": "Email muy corto"}
    fin
    
    # Verificar formato básico
    si !(cadena.contiene(email, "@"))
        ret {"valido": falso, "error": "Falta el símbolo @"}
    fin
    
    partes = cadena.separar(email, "@")
    si lista.longitud(partes) != 2
        ret {"valido": falso, "error": "Debe tener exactamente un @"}
    fin
    
    dominio = partes[1]
    si !(cadena.contiene(dominio, "."))
        ret {"valido": falso, "error": "Dominio inválido"}
    fin
    
    ret {"valido": verdadero, "error": nulo}
fin

fun registrarUsuario(email, nombre)
    # Validar email
    validacion = validarEmail(email)
    
    si !validacion["valido"]
        escribir("❌ Email inválido: " .. validacion["error"])
        ret falso
    fin
    
    # Continuar con registro
    escribir("✅ Usuario registrado: " .. nombre)
    ret verdadero
fin

# Pruebas
registrarUsuario("ana@gmail.com", "Ana García")
registrarUsuario("email_sin_arroba", "Pedro")
registrarUsuario("doble@@arroba.com", "María")
```

**Salida:**

```text
✅ Usuario registrado: Ana García
❌ Email inválido: Falta el símbolo @
❌ Email inválido: Debe tener exactamente un @
```

## 4.3 Alcance de Variables (Scope)

### El Misterio de las Variables "Fantasma"

```latino
# Experimento
x = "Global"

fun Probar()
    x = "Local"
    escribir("Dentro: " .. x)
fin

Probar()
escribir("Fuera: " .. x)
```

**Pregunta:** ¿Qué imprime?

**Salida:**

```text
Dentro: Local
Fuera: Local
```

**Explicación:** Sin `local`, la función **modifica** la variable global. Ambas `x` son la misma.

### ¿Qué Es el Scope?

El **scope** (alcance) determina **dónde vive** una variable y **quién puede verla**.

```text
┌─────────────────────────────────────┐
│  SCOPE GLOBAL                       │ ← Visible en todo el programa
│  i = 100                            │
│                                     │
│  ┌──────────────────────────────┐   │
│  │ SCOPE FUNCIÓN A              │   │ ← Visible solo aquí
│  │ local j = 200                │   │
│  │                              │   │
│  │  ┌───────────────────────┐   │   │
│  │  │ SCOPE FUNCIÓN B       │   │   │ ← Visible solo aquí
│  │  │ local k = 300         │   │   │
│  │  │                       │   │   │
│  │  │ Puede ver: i, j, k    │   │   │
│  │  └───────────────────────┘   │   │
│  └──────────────────────────────┘   │
└─────────────────────────────────────┘
```

### Las Tres Formas de Declarar Variables

| Palabra Clave | Scope | Comportamiento |
| :--- | :--- | :--- |
| `local` | Local al bloque | Muere al salir del bloque |
| `global` | Global (raíz) | Vive todo el programa |
| *(Implícito)* | Busca hacia arriba | Si existe arriba, modifica; si no, crea local |

### 1. Variables Locales (`local`)

Existen **solo** dentro del bloque donde se declaran:

```latino
fun calcular()
    local resultado = 100
    escribir("Dentro: " .. resultado)
fin

calcular()
escribir(resultado)  # ❌ ERROR: 'resultado' no está definida
```

**Beneficio:** Evitas contaminación del scope global.

### 2. Variables Globales (`global`)

Accesibles desde **cualquier parte** del programa:

```latino
global contador = 0

fun Incrementar()
    contador = contador + 1  # Modifica la global
fin

Incrementar()
Incrementar()
escribir(contador)  # 2
```

**⚠️ Advertencia:** El abuso de globales hace el código difícil de debuguear. Úsalas solo para:

- Constantes de configuración
- Estado compartido esencial

### 3. Variables Implícitas (Sin Palabra Clave)

Si no usas `local` ni `global`, Latino.NET **busca hacia arriba**:

```latino
x = "Global"  # Variable global implícita

fun Modificar()
    x = "Modificada"  # Como 'x' existe arriba, la modifica
    escribir("Dentro: " .. x)
fin

Modificar()
escribir("Fuera: " .. x)
```

**Salida:**

```text
Dentro: Modificada
Fuera: Modificada
```

**Problema:** Podrías modificar variables sin querer.

### Comparativa Visual

```latino
# EXPERIMENTO 1: Sin local (modifica global)
unaVariable = "Original"

fun Test1()
    unaVariable = "Cambiada"
fin

Test1()
escribir(unaVariable)  # "Cambiada"

# EXPERIMENTO 2: Con local (crea nueva)
otraVariable = "Original"

fun Test2()
    local otraVariable = "Nueva"
    escribir("Dentro: " .. otraVariable)
fin

Test2()
escribir("Fuera: " .. otraVariable)
```

**Salida:**

```text
Cambiada
Dentro: Nueva
Fuera: Original
```

## Shadowing (Sombreado)

Cuando una variable local **oculta** una global con el mismo nombre:

```latino
nombre = "Global"

fun Probar()
    local nombre = "Local"  # Sombrea la global
    escribir("Dentro: " .. nombre)
fin

Probar()
escribir("Fuera: " .. nombre)
```

**Salida:**

```text
Dentro: Local
Fuera: Global
```

**Diagrama:**

```text
┌──────────────────────────────┐
│ nombre = "Global"            │ ← Global
│                              │
│  ┌───────────────────────┐   │
│  │ local nombre = "Local"│   │ ← Sombrea la global
│  │ [Global oculta aquí]  │   │
│  └───────────────────────┘   │
│                              │
│ nombre sigue siendo "Global" │
└──────────────────────────────┘
```

## Errores Comunes de Scope

### Error 1: Modificación Accidental de Globales

Si utilizas un nombre de variable común (como `total` o `i`) sin la palabra clave `local`, y esa variable casualmente ya existe fuera de tu función, **sobrescribirás el dato externo**.

```latino
# Supongamos que tienes un dato importante guardado
total = 1000 

# ❌ MAL: Sin 'local'
fun ProcesarLista(unaLista)
    # El sistema ve que 'total' existe afuera y lo usa.
    # ¡Al hacer esto borraste el 1000!
    total = 0  
    
    para item en unaLista
        total = total + item
    fin
    
    ret total
fin

escribir("Suma: " .. ProcesarLista([1, 2, 3]))  # 6
escribir("Total Global: " .. total)             # 6 (¡El 1000 se perdió!)

# ✅ BIEN: Usar 'local' protege tus datos
fun ProcesarSeguro(unaLista)
    local total = 0  # Crea una variable nueva solo para aquí
    # ... lógica ...
    ret total
fin
```

### Error 2: Modificar Global Sin Querer

```latino
# ❌ MAL: Variable global accidental
config = "produccion"

fun Testear()
    config = "desarrollo"  # ¡Modifica la global!
    # ... pruebas ...
fin

Testear()
escribir(config)  # "desarrollo" ⚠️ (cambió sin querer)

# ✅ BIEN: Usar local o ser explícito
global config = "produccion"

fun Testear()
    local config_temporal = "desarrollo"
    # ... usar config_temporal ...
fin

Testear()
escribir(config)  # "produccion" ✅
```

### Error 3: Muerte Prematura de Variables (Scope de Bloque)

En Latino.NET, las variables creadas dentro de un bloque (`si`, `mientras`) **mueren** cuando el bloque termina. No "flotan" hacia arriba como en otros lenguajes.

```latino
# ❌ MAL: Creer que la variable sobrevive
fun ObtenerUsuario(id)
    si id > 0
        # 'nombre' se crea aquí dentro (es local al 'si')
        nombre = "Usuario_" .. id  
    fin
    
    # 💥 ERROR Runtime: Variable no definida 'nombre'
    # La variable murió en el 'fin' anterior.
    retornar nombre  
fin

# ✅ BIEN: Inicializar antes
fun ObtenerUsuario(id)
    # 1. La creamos en el ámbito de la función
    local nombre = nulo
    
    si id > 0
        # 2. Como ya existe arriba, la actualizamos en lugar de crear una nueva
        nombre = "Usuario_" .. id
    fin
    
    retornar nombre # ✅ Sigue viva
fin
```

## 4.4 Funciones como Valores de Primera Clase

### El Concepto Revolucionario

En la mayoría de lenguajes clásicos (como C o Pascal), las funciones son **ciudadanos de segunda clase**: solo puedes definirlas en tu archivo y llamarlas por su nombre. No puedes hacer nada más con ellas.

**Latino.NET es diferente.** Aquí, las funciones son **valores de primera clase**, exactamente igual que los números o las cadenas de texto:

```latino
# Los números son valores que puedes guardar
x = 42
listaNumeros = [1, 2, 3]

# ¡Las funciones también son valores que puedes guardar!
f = fun(n) ret n * 2 fin
listaFunciones = [
    fun(x) ret x + 10 fin,
    fun(x) ret x * 2 fin
]
```

Esto significa que cualquier cosa que puedas hacer con un texto o un número, lo puedes hacer con una función. Veamos las aplicaciones fundamentales de este concepto.

### 1. Asignar Funciones a Variables

La forma más básica de aprovechar esto es guardar una función existente dentro de una nueva variable. Observa que al asignarla **no usamos paréntesis**, ya que no queremos ejecutarla, solo queremos referenciarla.

```latino
# Definimos la función normalmente
fun Saludar(nombre)
    ret "Hola, " .. nombre
fin

# Asignamos la función a una variable (¡sin paréntesis!)
miFuncion = Saludar

# Llamamos a la función a través de la variable
escribir(miFuncion("Ana"))  # Salida: Hola, Ana
```

### 2. Funciones Anónimas (Lambdas)

Si una función es simplemente un valor (como el número `5` o el texto `"Hola"`), no siempre es necesario darle un nombre formal en el código. A estas funciones sin nombre se les conoce como **Funciones Anónimas** o *Lambdas*.

Son perfectas para operaciones rápidas o para guardar lógica directamente en variables y estructuras de datos.

**Sintaxis:**

```latino
fun(parametros) ret expresion fin
```

**Comparación:**

```latino
# Función nombrada (Tradicional)
fun Duplicar(x)
    ret x * 2
fin
resultado1 = Duplicar(5)

# Función anónima asignada a una variable
duplicador = fun(x) ret x * 2 fin
resultado2 = duplicador(5)

# Ambos dan como resultado 10
```

### 3. Guardar en Estructuras: El Patrón "Dispatch Table"

Dado que las funciones son valores y las funciones anónimas nos permiten escribirlas en una sola línea, podemos guardar funciones dentro de **Diccionarios**. Esto nos permite crear uno de los patrones de diseño más limpios y poderosos de la programación: el **Dispatch Table** (Tabla de Despacho).

**El Problema:**
Imagina un sistema con múltiples comandos. Normalmente usarías un bloque `si/osi/sino` gigante:

```latino
# ❌ MAL: Condicional gigante y difícil de mantener
fun Procesar(comando, a, b)
    si comando == "sumar"
        ret a + b
    osi comando == "restar"
        ret a - b
    osi comando == "multiplicar"
        ret a * b
    # ... docenas de líneas más ...
    fin
fin
```

**La Solución Elegante:**
Creamos un diccionario donde las *claves* son los nombres de los comandos, y los *valores* son funciones anónimas.

```latino
# ✅ BIEN: Calculadora usando Dispatch Table
operaciones = {
    "sumar":       fun(a, b) ret a + b fin,
    "restar":      fun(a, b) ret a - b fin,
    "multiplicar": fun(a, b) ret a * b fin,
    "dividir":     fun(a, b) ret a / b fin,
    "potencia":    fun(a, b) ret a ^ b fin
}

fun calcular(operacion, a, b)
    # 1. Verificamos si la operación existe en el diccionario
    si !dic.contiene(operaciones, operacion)
        escribir("❌ Operación desconocida: " .. operacion)
        ret nulo
    fin
    
    # 2. Recuperamos la función del diccionario
    funcionOperacion = operaciones[operacion]
    
    # 3. La ejecutamos y retornamos el resultado
    ret funcionOperacion(a, b)
fin

# Uso limpio y directo
escribir(calcular("sumar", 10, 5))       # Salida: 15
escribir(calcular("multiplicar", 10, 5)) # Salida: 50
escribir(calcular("raiz", 10, 2))        # Salida: ❌ Operación desconocida
```

**Ventajas del Dispatch Table:**

- ✅ Adiós a las sentencias `si/osi/sino` kilométricas.
- ✅ Facilidad para agregar nuevos comandos (solo agregas una línea al diccionario).
- ✅ Puedes agregar o eliminar operaciones dinámicamente mientras el programa se ejecuta.

> **💡 Hacia el Paradigma Funcional:**
> Hemos visto que puedes asignar funciones a variables y guardarlas en diccionarios. Pero, ¿qué pasa si **pasas una función como parámetro** a otra función (Callbacks)? ¿O si haces que una función **retorne una nueva función** (Closures)?
>
> Esas capacidades avanzadas abren la puerta a un mundo completamente nuevo: el **Paradigma Funcional**. Aprenderemos a dominar estas técnicas a fondo en el **Capítulo 6: Programación Funcional**.

## 4.5 Recursividad

### La Función Que Se Llama a Sí Misma

**Recursividad** es cuando una función se llama a sí misma para resolver un problema dividiéndolo en subproblemas más pequeños.

```latino
fun Contar(n)
    si n == 0
        escribir("¡Despegue!")
    sino
        escribir(n)
        Contar(n - 1)  # ¡Se llama a sí misma!
    fin
fin

Contar(3)
```

**Salida:**

```text
3
2
1
¡Despegue!
```

### Analogía: Las Muñecas Rusas (Matrioshka)

Imagina un conjunto de muñecas rusas, una dentro de otra:

```text
┌──────────────────────────┐
│    Muñeca 5 (Grande)     │
│  ┌────────────────────┐  │
│  │   Muñeca 4         │  │
│  │ ┌────────────────┐ │  │
│  │ │   Muñeca 3     │ │  │
│  │ │ ┌────────────┐ │ │  │
│  │ │ │   Muñeca 2 │ │ │  │
│  │ │ │ ┌────────┐ │ │ │  │
│  │ │ │ │ Núcleo │ │ │ │  │ ← Caso base
│  │ │ │ └────────┘ │ │ │  │
│  │ │ └────────────┘ │ │  │
│  │ └────────────────┘ │  │
│  └────────────────────┘  │
└──────────────────────────┘
```

**Proceso:**

1. **Abrir** (dividir): Abres la muñeca grande, encuentras una más pequeña
2. **Abrir** (dividir): Abres esa, encuentras otra más pequeña
3. **Continuar** hasta llegar al núcleo (caso base)
4. **Cerrar** (resolver): Empiezas a cerrar desde dentro hacia afuera

**La recursividad funciona igual:**

1. Divide el problema en uno más pequeño
2. Resuelve el problema pequeño (llamándose a sí misma)
3. Cuando llegas al caso más simple (caso base), empiezas a "regresar"

## Anatomía de una Función Recursiva

Toda función recursiva tiene **dos partes obligatorias**:

```latino
fun FuncionRecursiva(parametro)
    # 1. CASO BASE (Condición de parada)
    si parametro cumple_condicion_simple
        ret resultado_directo
    fin
    
    # 2. CASO RECURSIVO (Llamada a sí misma)
    ret FuncionRecursiva(parametro_reducido)
fin
```

**Componentes:**

| Parte | Descripción | Si falta... |
| :--- | :--- | :--- |
| **Caso Base** | Condición de parada | ¡Bucle infinito! Stack overflow |
| **Caso Recursivo** | Llamada a sí misma con problema más pequeño | No es recursiva |
| **Reducción** | Cada llamada debe acercar al caso base | ¡Bucle infinito! |

🎯 **Ejemplo 1: Factorial (Nivel Básico)**

El factorial de un número: `5! = 5 × 4 × 3 × 2 × 1 = 120`

### Pensamiento Recursivo

```text
factorial(5) = 5 × factorial(4)
             = 5 × (4 × factorial(3))
             = 5 × (4 × (3 × factorial(2)))
             = 5 × (4 × (3 × (2 × factorial(1))))
             = 5 × (4 × (3 × (2 × 1)))
             = 120
```

### Implementación

```latino
fun Factorial(n)
    # CASO BASE: factorial de 0 o 1 es 1
    si n <= 1
        ret 1
    fin
    
    # CASO RECURSIVO: n × factorial(n-1)
    ret n * Factorial(n - 1)
fin

# Pruebas
escribir("5! = " .. Factorial(5))   # 120
escribir("3! = " .. Factorial(3))   # 6
escribir("0! = " .. Factorial(0))   # 1
```

### Diagrama de Llamadas (Stack)

```text
Factorial(5)
  │
  ├─ necesita 5 × Factorial(4)
  │              │
  │              ├─ necesita 4 × Factorial(3)
  │              │              │
  │              │              ├─ necesita 3 × Factorial(2)
  │              │              │              │
  │              │              │              ├─ necesita 2 × Factorial(1)
  │              │              │              │              │
  │              │              │              │              └─ ret 1 ✅
  │              │              │              └─ ret 2 × 1 = 2 ✅
  │              │              └─ ret 3 × 2 = 6 ✅
  │              └─ ret 4 × 6 = 24 ✅
  └─ ret 5 × 24 = 120 ✅
```

🎯 **Ejemplo 2: Suma de Lista (Nivel Básico-Intermedio)**

Sumar todos los elementos de una lista recursivamente.

```latino
fun SumarLista(miLista)
    # CASO BASE: miLista vacía = 0
    si lista.longitud(miLista) == 0
        ret 0
    fin
    
    # CASO RECURSIVO: primer elemento + suma del resto nativo
    primer_elemento = miLista[0]
    resto = lista.saltar(miLista, 1)
    
    retornar primer_elemento + SumarLista(resto)
fin

# Prueba
numeros = [5, 3, 8, 2]
escribir("Suma: " .. SumarLista(numeros))  # 18
```

**Pensamiento:**

```text
SumarLista([5, 3, 8, 2])
= 5 + SumarLista([3, 8, 2])
= 5 + (3 + SumarLista([8, 2]))
= 5 + (3 + (8 + SumarLista([2])))
= 5 + (3 + (8 + (2 + SumarLista([]))))
= 5 + (3 + (8 + (2 + 0)))
= 18
```

🎯 **Ejemplo 3: Fibonacci (Nivel Intermedio)**

La secuencia de Fibonacci: `0, 1, 1, 2, 3, 5, 8, 13, 21...`

Cada número es la suma de los dos anteriores: `F(n) = F(n-1) + F(n-2)`

```latino
fun Fibonacci(n)
    # CASOS BASE
    si n == 0
        ret 0
    osi n == 1
        ret 1
    fin
    
    # CASO RECURSIVO: suma de los dos anteriores
    ret Fibonacci(n - 1) + Fibonacci(n - 2)
fin

# Generar secuencia
escribir("Secuencia de Fibonacci:")
desde (i = 0; i <= 10; i = i + 1)
    escribir("F(" .. i .. ") = " .. Fibonacci(i))
fin
```

**Salida:**

```text
Secuencia de Fibonacci:
F(0) = 0
F(1) = 1
F(2) = 1
F(3) = 2
F(4) = 3
F(5) = 5
F(6) = 8
F(7) = 13
F(8) = 21
F(9) = 34
F(10) = 55
```

### Análisis: Ineficiencia de Fibonacci

```text
Fibonacci(5)
    ├─ Fibonacci(4)
    │   ├─ Fibonacci(3)
    │   │   ├─ Fibonacci(2)
    │   │   │   ├─ Fibonacci(1) → 1
    │   │   │   └─ Fibonacci(0) → 0
    │   │   └─ Fibonacci(1) → 1
    │   └─ Fibonacci(2)  ← ¡DUPLICADO!
    │       ├─ Fibonacci(1) → 1
    │       └─ Fibonacci(0) → 0
    └─ Fibonacci(3)  ← ¡DUPLICADO!
        ├─ Fibonacci(2)  ← ¡TRIPLICADO!
        │   ├─ Fibonacci(1) → 1
        │   └─ Fibonacci(0) → 0
        └─ Fibonacci(1) → 1
```

**Problema:** Calcula los mismos valores múltiples veces. `Fibonacci(50)` tomaría años.

**Solución:** Usar iteración o memoización (guardar resultados calculados).

## Recursividad vs Iteración

### Mismo Problema, Dos Enfoques

**Problema:** Contar de N a 0

#### Versión Recursiva

```latino
fun ContarRecursivo(n)
    si n < 0
        ret
    fin
    
    escribir(n)
    ContarRecursivo(n - 1)
fin

ContarRecursivo(5)
```

#### Versión Iterativa

```latino
fun ContarIterativo(n)
    mientras n >= 0
        escribir(n)
        n = n - 1
    fin
fin

ContarIterativo(5)
```

**Ambos producen:**

```text
5
4
3
2
1
0
```

### Tabla Comparativa

| Aspecto | Recursividad | Iteración |
| :--- | :--- | :--- |
| **Legibilidad** | ⭐⭐⭐⭐⭐ Elegante para problemas naturalmente recursivos | ⭐⭐⭐☆☆ Más verbosa |
| **Rendimiento** | ⭐⭐☆☆☆ Llamadas de función tienen costo | ⭐⭐⭐⭐⭐ Más rápida |
| **Memoria** | ⭐⭐☆☆☆ Consume stack por llamada | ⭐⭐⭐⭐⭐ Memoria constante |
| **Facilidad** | ⭐⭐⭐⭐☆ Natural para algunos problemas | ⭐⭐⭐☆☆ Requiere más lógica |
| **Límites** | ⭐⭐☆☆☆ Stack overflow si profunda | ⭐⭐⭐⭐⭐ Sin límites prácticos |

### ¿Cuándo Usar Cada Una?

**✅ Usa Recursividad cuando:**

- El problema es naturalmente recursivo (árboles, divide y conquista)
- La profundidad es pequeña (< 100 niveles)
- La legibilidad es crítica
- Ejemplos: recorrer directorios, parsers, algoritmos de backtracking

**✅ Usa Iteración cuando:**

- Necesitas máximo rendimiento
- La profundidad puede ser muy grande
- El problema es una simple repetición
- Ejemplos: procesar listas grandes, loops de juegos, cálculos numéricos

🎯 **Ejemplo Comparativo: Potencia**

```latino
# ═══════════════════════════════════════════════════════════
#  CALCULAR POTENCIA: RECURSIVO VS ITERATIVO
# ═══════════════════════════════════════════════════════════

# VERSIÓN RECURSIVA
fun PotenciaRecursiva(base, exponente)
    # Caso base: cualquier número^0 = 1
    si exponente == 0
        ret 1
    fin
    
    # Caso recursivo: base × base^(exponente-1)
    ret base * PotenciaRecursiva(base, exponente - 1)
fin

# VERSIÓN ITERATIVA
fun PotenciaIterativa(base, exponente)
    local resultado = 1
    
    desde (i = 0; i < exponente; i = i + 1)
        resultado = resultado * base
    fin
    
    ret resultado
fin

# Comparación
escribir("=== Recursiva ===")
escribir("2^5 = " .. PotenciaRecursiva(2, 5))
escribir("3^4 = " .. PotenciaRecursiva(3, 4))

escribir("\n=== Iterativa ===")
escribir("2^5 = " .. PotenciaIterativa(2, 5))
escribir("3^4 = " .. PotenciaIterativa(3, 4))
```

**Salida:**

```text
=== Recursiva ===
2^5 = 32
3^4 = 81

=== Iterativa ===
2^5 = 32
3^4 = 81
```

## Stack Overflow y Límites

### ¿Qué Es el Stack Overflow?

Cada llamada recursiva consume memoria en el **stack (pila)**. Si llamas demasiadas veces, la pila se llena:

```text
┌─────────────────────┐
│ Factorial(1000)     │ ← Más reciente
├─────────────────────┤
│ Factorial(999)      │
├─────────────────────┤
│ Factorial(998)      │
├─────────────────────┤
│      ...            │
├─────────────────────┤
│ Factorial(2)        │
├─────────────────────┤
│ Factorial(1) ← BASE │
└─────────────────────┘
       STACK
    (Memoria limitada)
```

**Cuando se llena:** `RuntimeError: Límite de recursividad excedido`

### Límite Por Defecto en Latino.NET

Latino.NET tiene un **límite de seguridad de 100 niveles** por defecto para evitar crashes.

```latino
fun Profunda(n)
    escribir(n)
    Profunda(n + 1)  # ¡Sin caso base! Infinita
fin

intentar
    Profunda(1)
atrapar (err)
    escribir("Error: " .. err)
fin
```

**Salida:**

```text
1
2
3
...
99
100
Error: [Línea 1] Límite de recursividad excedido (100).
```

### Configurar el Límite con `sis.config`

Para algoritmos legítimamente profundos, puedes aumentar el límite:

```latino
# Consultar límite actual
limite_actual = sis.config("max_stack")
escribir("Límite actual: " .. limite_actual)

# Aumentar para cálculo profundo
sis.config("max_stack", 500)

intentar
    resultado = Factorial(250)
    escribir("✅ Factorial(250) calculado")
atrapar (err)
    escribir("❌ Error: " .. err)
fin

# Restaurar límite original
sis.config("max_stack", limite_actual)
```

### Límites Físicos del Stack

`sis.config` mueve el límite de software, pero **no puede aumentar la memoria física (Stack)** de tu computadora.

Si configuras un límite excesivamente alto (ej: 5000) y tu función es compleja, es posible que el programa se cierre abruptamente (*Crash*) antes de que Latino.NET pueda avisarte.

**Reglas:**

- ✅ Aumenta el límite en incrementos pequeños solo cuando sea estrictamente necesario
- ✅ Para iteraciones masivas (miles de veces), usa siempre bucles (`mientras`, `desde`) en lugar de recursividad
- ✅ Si necesitas más de 1000 niveles, reconsidera tu algoritmo

🎯 **Ejemplo Avanzado: Búsqueda Binaria Recursiva**

Buscar un elemento en una lista **ordenada** dividiéndola a la mitad repetidamente.

```latino
# ═══════════════════════════════════════════════════════════
#  BÚSQUEDA BINARIA RECURSIVA
# ═══════════════════════════════════════════════════════════

fun BusquedaBinariaRecursiva(miLista, objetivo, inicio, final)
    # CASO BASE 1: No encontrado (rango inválido)
    si inicio > final
        ret -1
    fin
    
    # Calcular punto medio
    medio = (inicio + final) / 2
    medio = mate.piso(medio)  # Asegurar entero
    
    # CASO BASE 2: Encontrado en el medio
    si miLista[medio] == objetivo
        ret medio
    fin
    
    # CASO RECURSIVO 1: Buscar en mitad izquierda
    si miLista[medio] > objetivo
        ret BusquedaBinariaRecursiva(miLista, objetivo, inicio, medio - 1)
    fin
    
    # CASO RECURSIVO 2: Buscar en mitad derecha
    ret BusquedaBinariaRecursiva(miLista, objetivo, medio + 1, final)
fin

# Wrapper para facilitar uso
fun BuscarBinario(miLista, objetivo)
    ret BusquedaBinariaRecursiva(miLista, objetivo, 0, lista.longitud(miLista) - 1)
fin

# ═══════════════════════════════════════════════════════════
#  PRUEBAS
# ═══════════════════════════════════════════════════════════

numeros_ordenados = [2, 5, 8, 12, 16, 23, 38, 45, 56, 67, 78]

escribir("Lista: " .. numeros_ordenados)
escribir("")

# Buscar varios elementos
objetivos = [23, 5, 100, 2, 78]

para obj en objetivos
    posicion = BuscarBinario(numeros_ordenados, obj)
    
    si posicion != -1
        escribirf("✅ {0} encontrado en posición {1}\n", obj, posicion)
    sino
        escribirf("❌ {0} no encontrado\n", obj)
    fin
fin
```

**Salida:**

```text
Lista: [2, 5, 8, 12, 16, 23, 38, 45, 56, 67, 78]

✅ 23 encontrado en posición 5
✅ 5 encontrado en posición 1
❌ 100 no encontrado
✅ 2 encontrado en posición 0
✅ 78 encontrado en posición 10
```

**Ventaja:** En una lista de 1,000,000 elementos, búsqueda binaria necesita **máximo 20 comparaciones** vs 500,000 promedio de búsqueda lineal.

## Anti-Patrones de Recursividad

### 1. Olvidar el Caso Base

```latino
# ❌ MAL: Sin caso base (infinito)
fun Mal(n)
    ret n + Mal(n - 1)  # ¡Nunca para!
fin

# ✅ BIEN: Con caso base
fun Bien(n)
    si n == 0
        ret 0
    fin
    ret n + Bien(n - 1)
fin
```

### 2. Reducción Incorrecta

```latino
# ❌ MAL: No se acerca al caso base
fun Mal(n)
    si n == 0
        ret 0
    fin
    ret n + Mal(n + 1)  # ¡n aumenta en lugar de disminuir!
fin

# ✅ BIEN: Reducción correcta
fun Bien(n)
    si n == 0
        ret 0
    fin
    ret n + Bien(n - 1)  # n disminuye hacia 0
fin
```

### 3. Usar Recursividad Cuando Iteración Es Mejor

```latino
# ❌ MAL: Recursividad para suma simple (ineficiente)
fun SumarHasta(n)
    si n == 0
        ret 0
    fin
    ret n + SumarHasta(n - 1)
fin

# ✅ BIEN: Iteración (más eficiente)
fun SumarHasta(n)
    suma = 0
    desde (i = 1; i <= n; i = i + 1)
        suma = suma + i
    fin
    ret suma
fin

# ✅ MEJOR: Fórmula matemática (instantáneo)
fun SumarHasta(n)
    ret (n * (n + 1)) / 2
fin
```

## 4.6 Patrones de Diseño con Funciones

Los **patrones de diseño** son soluciones probadas a problemas comunes. Con funciones como valores de primera clase, Latino.NET permite implementar patrones poderosos de forma elegante.

## Patrón 1: Dispatch Table (Tabla de Despacho)

### El Problema

Imagina un sistema con 20 comandos diferentes:

```latino
# ❌ MAL: Switch gigante
fun ProcesarComando(cmd, args)
    si cmd == "crear"
        # 20 líneas
    osi cmd == "leer"
        # 15 líneas
    osi cmd == "actualizar"
        # 25 líneas
    osi cmd == "eliminar"
        # 18 líneas
    # ... 16 comandos más
    sino
        escribir("Comando desconocido")
    fin
fin
# Total: 300+ líneas de si/osi/sino
```

**Problemas:**

- ❌ Código difícil de leer
- ❌ Agregar comando = modificar función gigante
- ❌ No puedes agregar comandos en runtime

### La Solución: Dispatch Table

```latino
# ✅ BIEN: Diccionario de funciones
comandos = {
    "crear": fun(args) # lógica de crear fin,
    "leer": fun(args) # lógica de leer fin,
    "actualizar": fun(args) # lógica de actualizar fin,
    "eliminar": fun(args) # lógica de eliminar fin
}

fun ProcesarComando(cmd, args)
    si dic.contiene(comandos, cmd)
        comandos[cmd](args)
    sino
        escribir("❌ Comando desconocido")
    fin
fin
```

**Ventajas:**

- ✅ Código limpio y organizado
- ✅ Fácil agregar comandos (solo modificar diccionario)
- ✅ Puedes agregar comandos dinámicamente
- ✅ Fácil probar cada comando independientemente

🎯 **Ejemplo Completo: Sistema de Calculadora Científica**

```latino
# ═══════════════════════════════════════════════════════════
#  CALCULADORA CIENTÍFICA CON DISPATCH TABLE
# ═══════════════════════════════════════════════════════════

# Operaciones básicas
operaciones = {
    "suma": fun(a, b) ret a + b fin,
    "resta": fun(a, b) ret a - b fin,
    "multiplicacion": fun(a, b) ret a * b fin,
    "division": fun(a, b)
        si b == 0
            escribir("❌ División por cero")
            ret nulo
        fin
        ret a / b
    fin,
    "potencia": fun(a, b) ret a ^ b fin,
    "raiz": fun(a, b) ret a ^ (1/b) fin,
    "modulo": fun(a, b) ret a % b fin
}

# Funciones trigonométricas (usando mate.*)
funcionesCientificas = {
    "seno": fun(x) ret mate.sen(x) fin,
    "coseno": fun(x) ret mate.cos(x) fin,
    "tangente": fun(x) ret mate.tan(x) fin,
    "log": fun(x) ret mate.log(x) fin,
    "abs": fun(x) ret mate.abs(x) fin,
    "piso": fun(x) ret mate.piso(x) fin,
    "techo": fun(x) ret mate.techo(x) fin
}

fun calcular(operacion, valores)
    # Verificar si es operación binaria
    si dic.contiene(operaciones, operacion)
        si lista.longitud(valores) < 2
            escribir("❌ Se necesitan 2 valores")
            ret nulo
        fin
        ret operaciones[operacion](valores[0], valores[1])
    fin
    
    # Verificar si es función científica
    si dic.contiene(funcionesCientificas, operacion)
        si lista.longitud(valores) < 1
            escribir("❌ Se necesita 1 valor")
            ret nulo
        fin
        ret funcionesCientificas[operacion](valores[0])
    fin
    
    escribir("❌ Operación desconocida: " .. operacion)
    ret nulo
fin

# Agregar operación dinámicamente
fun AgregarOperacion(nombre, MiFuncion)
    operaciones[nombre] = MiFuncion
    escribir("✅ Operación '" .. nombre .. "' agregada")
fin

# ═══════════════════════════════════════════════════════════
#  PRUEBAS
# ═══════════════════════════════════════════════════════════

escribir("=== Calculadora Científica ===\n")

# Operaciones básicas
escribirf("10 + 5 = {0}\n", calcular("suma", [10, 5]))
escribirf("10 - 5 = {0}\n", calcular("resta", [10, 5]))
escribirf("10 × 5 = {0}\n", calcular("multiplicacion", [10, 5]))
escribirf("10 ÷ 2 = {0}\n", calcular("division", [10, 2]))
escribirf("2 ^ 8 = {0}\n", calcular("potencia", [2, 8]))

escribir("\n=== Funciones Científicas ===\n")
escribirf("abs(-15) = {0}\n", calcular("abs", [-15]))
escribirf("piso(3.7) = {0}\n", calcular("piso", [3.7]))
escribirf("techo(3.2) = {0}\n", calcular("techo", [3.2]))

# Agregar operación personalizada
escribir("\n=== Operación Dinámica ===")
AgregarOperacion("promedio", fun(a, b) ret (a + b) / 2 fin)
escribirf("promedio(10, 20) = {0}\n", calcular("promedio", [10, 20]))
```

**Salida:**

```text
=== Calculadora Científica ===

10 + 5 = 15
10 - 5 = 5
10 × 5 = 50
10 ÷ 2 = 5
2 ^ 8 = 256

=== Funciones Científicas ===

abs(-15) = 15
piso(3.7) = 3
techo(3.2) = 4

=== Operación Dinámica ===
✅ Operación 'promedio' agregada
promedio(10, 20) = 15
```

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
