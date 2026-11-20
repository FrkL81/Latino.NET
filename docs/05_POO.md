# CAPÍTULO 5: Programación Orientada a Objetos

## Modelar la Realidad en Código

Hasta este momento, nuestros programas se han construido bajo dos grandes enfoques: el **paradigma procedimental** (secuencias de pasos e instrucciones que ejecutan tareas directas) e introdujimos conceptos del **paradigma funcional** (funciones independientes que reciben datos de entrada y devuelven transformaciones. Se tratará en detalle en el capítulo 6). Además, en el Capítulo 3 aprendiste que los diccionarios permiten agrupar información bajo etiquetas con nombre.

Esto plantea una pregunta fundamental de arquitectura: si ya podemos representar a un usuario o una cuenta bancaria con un simple diccionario como `{"titular": "Ana", "saldo": 500}`, ¿por qué necesitamos un paradigma completo adicional?

### La Fragilidad de los Datos Desprotegidos

Un diccionario es una bolsa pasiva de claves y valores. Es una estructura sumamente útil, pero es completamente **ciega e indefensa**:

* **No protege su estado:** Cualquier línea de código externa puede corromper los datos por descuido: `cuenta["saldo"] = -999999` o incluso `cuenta["saldo"] = "mucho"`.
* **No garantiza coherencia al nacer:** Nada impide crear un diccionario incompleto al que le falte la clave `"saldo"` o el `"titular"`.
* **Separa los datos de sus acciones:** Las funciones que retiran, depositan o transfieren dinero viven dispersas por el programa, completamente desvinculadas de la estructura de datos que pretenden gobernar.

Cuando en una aplicación existen elementos con **identidad, ciclo de vida, estado interno e invariantes de negocio que proteger** (una cuenta bancaria donde el saldo no puede ser negativo, un carrito de compras con reglas de inventario o un jugador en un videojuego con salud y armadura), tratarlos como datos sueltos produce sistemas frágiles y propensos a errores silenciosos.

La **Programación Orientada a Objetos (POO)** resuelve esta fractura uniendo de forma inseparable la información (**el estado**) con las únicas operaciones autorizadas para manipularla (**el comportamiento**).

### La POO en Español Natural: La Identidad de Latino.NET

En la mayoría de los lenguajes de programación, la POO se siente como un calco forzado del inglés (`class`, `new`, `this`). En Latino.NET, la orientación a objetos fue diseñada desde sus cimientos para hablar y razonar en español fluido:

1. **Concordancia de Género al Instanciar:** El lenguaje respeta la naturaleza de tus sustantivos mediante palabras clave intercambiables: `nueva CuentaBancaria()`, `nuevo Archivo()` o el verbo universal `crear Usuario()`.
2. **Pronombres de Pertenencia:** Dentro de un objeto, accedes a sus propiedades y métodos utilizando pronombres naturales de autorreferencia: `mi.saldo`, `este.codigo` o `esta.clave`.
3. **Herencia Ontológica ("ES-UN"):** La relación de jerarquía entre clases modela el principio ontológico fundamental del mundo real mediante el verbo ser: `clase Perro es Mascota` o `clase Gerente es Empleado`.

> 💡 **Regla del Paradigma Proporcional (Mantra de Latino.NET):**  
> *"No mates moscas con cañones, ni sostengas rascacielos con mondadientes sueltos."*
>
> * Usa **procedimental** para scripts lineales, utilidades CLI y tareas directas sin persistencia de estado.
> * Usa **funcional** para procesar listas, aplicar filtros matemáticos y componer transformaciones de datos con pipelines (`|>`).  
> * Usa **POO (`clase ... fin`)** cuando existan **entidades del mundo real con identidad propia, estado que proteger y reglas de negocio que blindar**.

---

## 5.1 Clases

Una **clase** es una plantilla o molde para crear objetos. Define qué propiedades y métodos tendrán todos los objetos creados a partir de ella.

Se define con la palabra clave `clase`, seguida de un nombre y un cuerpo que termina con `fin`.

**Sintaxis Básica:**

```latino
clase NombreDeLaClase
    # Propiedades y métodos van aquí
fin
```

**Ejemplo Simple:**

```latino
clase Perro
    funcion Ladrar()
        escribir("¡Guau!")
    fin
    
    funcion Correr()
        escribir("El perro está corriendo...")
    fin
fin
```

### 5.1.1 Convención de Nombres en Objetos

Latino.NET distingue mayúsculas de minúsculas: `Ladrar` y `ladrar` son identificadores distintos, y mezclar la caja al llamar falla. La convención canónica para clases es **PascalCase** (`clase MiClase`).

> **⚠️ Excepción — mayúsculas sostenidas:** nombrar una clase en MAYÚSCULAS sostenidas (`clase API`, `clase DB`) está bloqueado intencionalmente por colisión con la regla de inmutabilidad de constantes (ADR-003): al declarar la clase, el motor lanza `No se puede reasignar la constante 'API'`. Usa PascalCase (`clase Api`, `clase Db`). Ver la sección «📌 Constantes: Valores Inmutables» en [01_Fundamentos.md](01_Fundamentos.md).

| Elemento | Convención | Ejemplo |
| :--- | :--- | :--- |
| Clase | PascalCase, con el artículo dentro del nombre | `Perro`, `ListaDeClientes` |
| Método | PascalCase | `Ladrar()`, `CalcularTotal()` |
| Constructor | PascalCase, palabra reservada | `constructor(nombre)` |
| Campo | camelCase, con `mi.` dentro y **sin prefijo desde fuera** | `mi.nombre` / `objeto.nombre` |

El artículo va **dentro** del identificador y no delante: se escribe `miLista`, no `laLista`. Es lo que hace que el español se lea sin traducir.

🎯 **Ejemplo: La convención aplicada a una clase completa**

```latino
clase Producto
    constructor(nombre, precio)
        mi.nombre = nombre
        mi.precio = precio
    fin

    funcion AplicarDescuento(porcentaje)
        mi.precio = mi.precio - (mi.precio * porcentaje / 100)
    fin

    funcion Resumen()
        retornar mi.nombre .. " cuesta " .. mi.precio
    fin
fin

miProducto = nuevo Producto("Teclado", 100)
miProducto.AplicarDescuento(20)
escribir(miProducto.Resumen())

# Salida en pantalla:
# Teclado cuesta 80
```

Los campos se declaran con `mi.` dentro de la clase, pero **se leen sin él desde fuera**: `miProducto.Resumen()` y `miProducto.nombre` funcionan, mientras que `miProducto.mi.nombre` da `El objeto de tipo 'nulo' no tiene propiedades ni métodos`.

Nota: `mi`, `este` y `esta` son **alias idénticos** del puntero de instancia dentro de la clase. Los tres funcionan igual, y ninguno existe fuera de ella. Por eso no hay plural (`mis`): `mi` es un ámbito, no un valor. El Capítulo 19 lo desarrolla.

### 5.2 Instanciación: Creando Objetos

Para crear un objeto a partir de una clase (proceso llamado "instanciación"), Latino.NET ofrece **cuatro estilos flexibles** que respetan la riqueza del idioma español.

#### Los Cuatro Estilos de Instanciación

* **1. Estilo Clásico (nueva)**

```latino
miMascota = nueva Perro()
miMascota.Ladrar()  # Salida: ¡Guau!
```

* **2. Estilo Masculino (nuevo)**

```latino
miPerro = nuevo Perro()
miPerro.Correr()    # Salida: El perro está corriendo...
```

* **3. Estilo Neutro (crear)**

```latino
mascota = crear Perro()
mascota.Ladrar()    # Salida: ¡Guau!
```

* **4. Estilo Moderno/Python (Implícito)**

```latino
perro = Perro()
perro.Correr()      # Salida: El perro está corriendo...
```

#### Filosofía de Diseño (riqueza gramatical)

Latino.NET está diseñado para aprovechar la riqueza gramatical del español. Puedes elegir el estilo que haga que tu código se lea de forma más natural:

* **Usa `nuevo` o `nueva`** según el género del sustantivo para dar naturalidad (ej: `nueva Ventana`, `nuevo Usuario`)
* **Usa el estilo implícito** (`Persona()`) si buscas máxima brevedad y estilo moderno
* **Usa `crear`** si prefieres un verbo neutro y universal

**La mejor opción es la que haga que tu código se lea mejor.**

**Ejemplo Comparativo:**

```latino
# La clase que usaremos en la comparación:
clase Ventana
    funcion Abrir()
        escribir("Ventana abierta")
    fin
fin

# Todos estos son equivalentes y correctos:
ventana1 = nueva Ventana()      # Género femenino
ventana2 = nuevo Ventana()      # También válido
ventana3 = crear Ventana()      # Neutro
ventana4 = Ventana()            # Moderno/Implícito

# Todos funcionan exactamente igual
ventana1.Abrir()
ventana2.Abrir()
ventana3.Abrir()
ventana4.Abrir()
```

### 5.3 El Constructor

El **constructor** es un método especial dentro de una clase que se ejecuta **automáticamente** cuando se crea una nueva instancia. Es el lugar ideal para inicializar las propiedades del objeto y prepararlo para su uso.

**Características del Constructor:**

* Se define con la palabra clave `constructor`
* Puede recibir parámetros
* Se ejecuta una sola vez: al crear el objeto
* No necesita `retornar` nada

**Ejemplo Básico:**

```latino
clase Saludo
    constructor(mensaje)
        escribir("Objeto creado con el mensaje: " .. mensaje)
    fin
fin

s1 = nueva Saludo("Hola Mundo")
# Salida inmediata: Objeto creado con el mensaje: Hola Mundo

s2 = crear Saludo("Bienvenidos")
# Salida inmediata: Objeto creado con el mensaje: Bienvenidos
```

**Ejemplo con Múltiples Parámetros:**

```latino
clase Persona
    constructor(nombre, edad, ciudad)
        escribir("Creando persona: " .. nombre)
        escribir("Edad: " .. edad .. ", Ciudad: " .. ciudad)
    fin
fin

juan = nuevo Persona("Juan", 30, "Panamá")
# Salida:
# Creando persona: Juan
# Edad: 30, Ciudad: Panamá
```

### 5.4 El Puntero `mi`

Dentro de una clase, la palabra clave **`mi`** se refiere a la instancia actual del objeto. Es la forma de acceder y modificar las propiedades de un objeto desde sus propios métodos, permitiendo que cada objeto mantenga su propio estado independiente.

**Concepto Clave:**

Cuando creas múltiples objetos de la misma clase, cada uno tiene sus propias propiedades. `mi` siempre apunta al objeto que está ejecutando el método en ese momento.

**Ejemplo Fundamental:**

```latino
clase Jugador
    constructor(nombre)
        # 'mi' guarda 'nombre' como una propiedad del objeto
        mi.nombre = nombre
        mi.puntuacion = 0
    fin

    funcion IncrementarPuntuacion(puntos)
        mi.puntuacion = mi.puntuacion + puntos
        escribir(mi.nombre .. " ahora tiene " .. mi.puntuacion .. " puntos.")
    fin
    
    funcion Presentarse()
        escribir("Hola, soy " .. mi.nombre)
    fin
fin

# Creamos dos jugadores diferentes
jugador1 = nueva Jugador("Ana")
jugador2 = nuevo Jugador("Carlos")

# Cada uno tiene su propio estado
jugador1.IncrementarPuntuacion(10)  # Salida: Ana ahora tiene 10 puntos.
jugador2.IncrementarPuntuacion(5)   # Salida: Carlos ahora tiene 5 puntos.
jugador1.IncrementarPuntuacion(3)   # Salida: Ana ahora tiene 13 puntos.

jugador1.Presentarse()  # Salida: Hola, soy Ana
jugador2.Presentarse()  # Salida: Hola, soy Carlos
```

**¿Por qué necesitamos `mi`?**

Sin `mi`, no habría forma de distinguir entre una variable local del método y una propiedad del objeto:

```latino
clase Ejemplo
    constructor()
        mi.dato = 100  # Propiedad del objeto (permanente)
    fin
    
    funcion Procesar()
        dato = 50      # Variable local (temporal, solo existe aquí)
        escribir(mi.dato)  # Salida: 100 (propiedad del objeto)
        escribir(dato)     # Salida: 50 (variable local)
    fin
fin
```

### 5.5 Referencia a la Instancia: Concordancia de Género

En Latino.NET, la referencia a la instancia actual tiene **tres alias intercambiables**: **`mi`**, **`este`** y **`esta`**. Esta característica única permite que el código se lea como prosa natural en español, respetando el género del objeto.

> **⚠️ Nota:** Estos pronombres solo funcionan **dentro de clases**. No existen en diccionarios literales (`{}`). Si necesitas simular objetos con diccionarios, usa el **patrón factoría con clausura** explicado en el Capítulo 3.

#### Filosofía de Diseño (concordancia de género)

La concordancia de género en el código hace que los programas sean más expresivos y naturales en español:

**`mi` (Posesivo neutro)**  
Ideal para propiedades generales y estado interno. Es el más versátil y recomendado.

```latino
clase Persona
    constructor(nombre, edad)
        mi.nombre = nombre
        mi.edad = edad
    fin
    
    funcion Saludar()
        escribir("Hola, soy " .. mi.nombre)
    fin
fin
```

**`este` (Masculino/Neutro)**  
Perfecto para objetos conceptualmente masculinos o neutros. Lee naturalmente en español.

```latino
clase Circulo
    constructor(radio)
        este.radio = radio
        este.area = este.CalcularArea()
    fin
    
    funcion CalcularArea()
        retornar 3.14159 * este.radio * este.radio
    fin
    
    funcion Describir()
        escribir("Este círculo tiene radio " .. este.radio)
    fin
fin

c = nuevo Circulo(5)
c.Describir()  # Salida: Este círculo tiene radio 5
```

**`esta` (Femenino)**  
Ideal para objetos conceptualmente femeninos. Mejora la legibilidad del código.

```latino
clase Mesa
    constructor(alto, ancho)
        esta.altura = alto
        esta.anchura = ancho
        esta.superficie = "lisa"
    fin
    
    funcion Medir()
        escribir("Esta mesa mide " .. esta.altura .. "x" .. esta.anchura .. " cm")
    fin
    
    funcion CambiarSuperficie(tipo)
        esta.superficie = tipo
        escribir("La superficie de esta mesa ahora es " .. esta.superficie)
    fin
fin

miMesa = nueva Mesa(75, 120)
miMesa.Medir()  # Salida: Esta mesa mide 75x120 cm
```

#### Uso Intercambiable

Los tres alias son completamente intercambiables y apuntan exactamente a la misma instancia. Puedes mezclarlos libremente:

```latino
clase Demostracion
    constructor()
        mi.valorPropio = 1
        este.valorActual = 2
        esta.cantidadEspecifica = 3
    fin
    
    funcion Mostrar()
        # Todos apuntan al mismo objeto
        escribir(mi.valorPropio)           # Salida: 1
        escribir(este.valorActual)         # Salida: 2
        escribir(esta.cantidadEspecifica)  # Salida: 3
    fin
fin

demo = Demostracion()
demo.Mostrar()
```

#### Regla de Seguridad: Solo Lectura

Los tres alias son **referencias de solo lectura**. No puedes reasignarlos directamente:

```latino
clase Ejemplo
    constructor()
        mi.propiedad = 1    # ✅ CORRECTO: Asignar a una propiedad
    fin
    
    funcion MetodoInvalido()
        este = nulo         # ❌ ERROR: No puedes reasignar la referencia
        esta = objeto       # ❌ ERROR: No puedes reasignar la referencia
        mi = 5              # ❌ ERROR: No puedes reasignar la referencia
    fin
    
    funcion MetodoCorrecto()
        mi.valor = 10       # ✅ CORRECTO: Modificar propiedades
        este.dato = 20      # ✅ CORRECTO: Modificar propiedades
        esta.info = 30      # ✅ CORRECTO: Modificar propiedades
    fin
fin
```

**¿Por qué esta restricción?**

Reasignar `mi`, `este` o `esta` rompería la coherencia del objeto. Es como intentar decir "ahora yo soy otra persona" - no tiene sentido lógico en el contexto de POO.

🎯 **Ejemplo: Modelado de cuenta bancaria con concordancia de género e identidad**

```latino
clase CuentaBancaria
    constructor(titular, saldoInicial)
        mi.titular = titular
        mi.saldo = saldoInicial
        mi.transacciones = 0
    fin
    
    funcion Depositar(cantidad)
        mi.saldo = mi.saldo + cantidad
        mi.transacciones = mi.transacciones + 1
        escribir(mi.titular .. " depositó $" .. cantidad)
        escribir("Nuevo saldo: $" .. mi.saldo)
    fin
    
    funcion Retirar(cantidad)
        si cantidad > mi.saldo
            escribir("Fondos insuficientes para " .. mi.titular)
        sino
            mi.saldo = mi.saldo - cantidad
            mi.transacciones = mi.transacciones + 1
            escribir(mi.titular .. " retiró $" .. cantidad)
            escribir("Nuevo saldo: $" .. mi.saldo)
        fin
    fin
    
    funcion VerEstado()
        escribir("--- Estado de Cuenta ---")
        escribir("Titular: " .. mi.titular)
        escribir("Saldo: $" .. mi.saldo)
        escribir("Transacciones: " .. mi.transacciones)
    fin
fin

# Uso
cuenta = nueva CuentaBancaria("Ana López", 1000)
cuenta.Depositar(500)   # Ana López depositó $500 / Nuevo saldo: $1500
cuenta.Retirar(200)     # Ana López retiró $200 / Nuevo saldo: $1300
cuenta.Retirar(2000)    # Fondos insuficientes para Ana López
cuenta.VerEstado()      # Muestra el estado completo
```

### 5.6 Herencia

La **herencia** es un mecanismo fundamental que permite a una clase (Hija) adquirir las propiedades y métodos de otra clase (Padre). Esto fomenta la reutilización de código y la creación de jerarquías lógicas.

Para heredar, se utiliza la palabra clave **`es`** después del nombre de la clase.

**Sintaxis:**

```latino
clase ClaseHija es ClasePadre
    # Métodos y propiedades adicionales
fin
```

#### Reglas de Herencia en Latino.NET

* **1. Herencia Simple**

Una clase solo puede heredar de **una sola** clase padre. No se soporta la herencia múltiple.

```latino
# ✅ CORRECTO: Herencia simple
clase Perro es Animal
fin

# ❌ INCORRECTO: Herencia múltiple (no soportada)
# clase Perro es Animal, Mascota
# fin
```

* **2. Orden de Búsqueda de Métodos (MRO - Method Resolution Order)**

Cuando llamas a un método, el intérprete lo busca en el siguiente orden lineal:

1. En la propia **Clase Hija**
2. Si no existe, sube a la **Clase Padre**
3. Continúa subiendo hasta llegar a la clase base
4. Si no lo encuentra en ningún nivel, lanza un error

**Ejemplo Básico:**

```latino
clase Animal
    funcion Comer()
        escribir("El animal está comiendo.")
    fin
    
    funcion HacerRuido()
        escribir("Ruido genérico...")
    fin
fin

# Perro es Animal: hereda todos sus metodos y campos
clase Perro es Animal
    # Método propio (solo existe en Perro)
    funcion Ladrar()
        escribir("¡Guau!")
    fin
    
    # Sobrescritura (Override): Reemplazamos el comportamiento del padre
    funcion HacerRuido()
        escribir("¡Guau guau!")
    fin
fin

mascota = nueva Perro()

# Búsqueda de métodos:
mascota.Ladrar()      # 1. Se encuentra en Perro -> "¡Guau!"
mascota.HacerRuido()  # 2. Se encuentra en Perro (sobrescrito) -> "¡Guau guau!"
mascota.Comer()       # 3. No está en Perro, se sube a Animal -> "El animal está comiendo."
```

#### Sobrescritura de Métodos (Override)

Cuando una clase hija define un método con el mismo nombre que uno del padre, **reemplaza** completamente el comportamiento heredado:

```latino
clase Vehiculo
    # ✅ Los constructores de clases base deben tener valores por defecto
    constructor(marca = "Genérica")
        mi.marca = marca
    fin
    
    funcion Arrancar()
        escribir("El vehículo " .. mi.marca .. " está arrancando...")
    fin
    
    funcion Frenar()
        escribir("Frenando...")
    fin
fin

clase Bicicleta es Vehiculo
    constructor(marca = "Trek", tipo = "urbana")
        mi.marca = marca
        mi.tipo = tipo
    fin
    
    # Sobrescribimos Arrancar() con comportamiento específico
    funcion Arrancar()
        escribir("Pedaleando la bicicleta " .. mi.marca .. " tipo " .. mi.tipo)
    fin
    
    # Frenar() NO se sobrescribe, se hereda tal cual
fin

bici = nueva Bicicleta("Trek", "montaña")
bici.Arrancar()  # Salida: Pedaleando la bicicleta Trek tipo montaña
bici.Frenar()    # Salida: Frenando... (heredado de Vehiculo)
```

#### Encadenamiento Automático de Constructores (Auto-Chaining)

A diferencia de otros lenguajes donde debes llamar manualmente al padre usando `super()`, Latino.NET hace el trabajo pesado por ti mediante un **encadenamiento automático de arriba hacia abajo (Top-Down)**.

Cuando instancias una clase que hereda de otras, el motor buscará todos los constructores en la cadena y los ejecutará en orden: **desde el ancestro más antiguo hasta la clase hija**. Esto garantiza que el estado base del objeto se construya correctamente capa por capa.

**Regla de Resolución de Argumentos:**
Para evitar errores si los constructores piden diferente cantidad de parámetros, Latino.NET aplica una regla inteligente:

1. **Los ancestros** siempre se ejecutan **sin argumentos** (se usan para inicializar el estado por defecto).
2. **La clase instanciada** (el hijo final) es la única que recibe los **argumentos** que pasaste al crear el objeto.

**Ejemplo de Auto-Chaining:**

```latino
clase Entidad
    constructor()
        mi.origen = "Sistema Base"
        escribir("1. Creando Entidad...")
    fin
fin

clase Vehiculo es Entidad
    constructor()
        mi.ruedas = 4
        escribir("2. Ensamblando Vehículo...")
    fin
fin

clase Auto es Vehiculo
    constructor(marca, modelo)
        mi.marca = marca
        mi.modelo = modelo
        escribir("3. Pintando Auto " .. mi.marca .. " " .. mi.modelo)
    fin
fin

# Al crear el Auto, se ejecutan automáticamente los 3 constructores
miCarro = nuevo Auto("Toyota", "Corolla")

# Salida:
# 1. Creando Entidad...
# 2. Ensamblando Vehículo...
# 3. Pintando Auto Toyota Corolla

# El objeto final tiene las propiedades de todas las capas
escribir(miCarro.origen)  # Salida: Sistema Base
escribir(miCarro.ruedas)  # Salida: 4
```

> **⚠️ REGLA DE ORO DE LAS CLASES BASE:**
> Como Latino.NET inicializa a los ancestros sin pasarles argumentos, **los constructores de las clases que van a ser heredadas deben usar parámetros opcionales (con valores por defecto)**. Si una clase base tiene parámetros estrictos obligatorios, el motor lanzará un error de seguridad al intentar heredar de ella.
>
> **Bien diseñado (Listo para heredar):**
> `constructor(nombre = "Desconocido", edad = 0)`
>
> **Mal diseñado (Causará error al heredar):**
> `constructor(nombre, edad)`
>
> **⚠️ REGLAS DE ORO DE LA HERENCIA EN LATINO.NET v1.0.81:**
>
> El motor **siempre ejecuta a los ancestros sin argumentos**, y los constructores parametrizados **no se heredan automáticamente**. Esto obliga a cumplir dos reglas, una en cada lado de la relación:
>
> **1. En la clase base (padre):** todo constructor debe tener **valores por defecto**, porque el motor lo invocará sin argumentos.
>
> **Bien diseñado (listo para heredar):**
> `constructor(nombre = "Desconocido", edad = 0)`
>
> **Mal diseñado (causará error al heredar):**
> `constructor(nombre, edad)`
>
> **2. En la clase derivada (hija):** si quieres instanciarla con datos (`nueva Hija("Dato")`), **la hija DEBE declarar su propio constructor**. Si no lo declara, el motor **acepta los argumentos y los descarta**: la instancia se queda con los valores por defecto del padre, sin ningún error que lo advierta.
>
> ```latino
> clase Animal
>     constructor(nombre = "Desconocido", edad = 0)
>         mi.nombre = nombre
>         mi.edad = edad
>     fin
> fin
>
> // ❌ Sin constructor propio: los argumentos se pierden EN SILENCIO
> clase PerroDefectuoso es Animal
> fin
> miPerro = nueva PerroDefectuoso("Rex", 5)
> escribir(miPerro.nombre)   // Imprime "Desconocido", no "Rex"
>
> // ✅ Con constructor propio: los datos llegan correctamente
> clase Perro es Animal
>     constructor(nombre = "Desconocido", edad = 0, raza = "Mestizo")
>         mi.nombre = nombre
>         mi.edad = edad
>         mi.raza = raza
>     fin
> fin
> miPerro2 = nueva Perro("Rex", 5, "Pastor Alemán")
> escribir(miPerro2.nombre)  // Imprime "Rex"
> ```
>
> **Cómo funciona por dentro:** al instanciar, el motor recorre la jerarquía de ancestros y solo la clase **instanciada** (la «hoja») recibe los argumentos del usuario; a los padres les pasa una lista vacía. Si la hoja no tiene constructor propio, nadie consume esos argumentos y se pierden.
>
> Como el constructor de la hoja es un `constructor` normal, **no necesitas `super(...)`** para inicializar: basta con asignar los campos que declares.
>

#### Cadenas de Herencia

Puedes crear jerarquías de múltiples niveles:

```latino
clase SerVivo
    funcion Respirar()
        escribir("Respirando...")
    fin
fin

clase Animal es SerVivo
    funcion Moverse()
        escribir("Moviéndose...")
    fin
fin

clase Mamifero es Animal
    funcion AmamantarCrias()
        escribir("Amamantando crías...")
    fin
fin

clase Perro es Mamifero
    funcion Ladrar()
        escribir("¡Guau!")
    fin
fin

# Un Perro tiene acceso a TODA la cadena de herencia
firulais = nuevo Perro()
firulais.Ladrar()          # De Perro
firulais.AmamantarCrias()  # De Mamifero
firulais.Moverse()         # De Animal
firulais.Respirar()        # De SerVivo
```

### 5.7 Reglas de Nombres (Shadowing)

En Latino.NET, la flexibilidad del lenguaje requiere que sigas una regla de oro importante para evitar conflictos entre datos y acciones.

> **⚠️ Regla de Oro:** No llames a un método igual que a una propiedad.

#### El Problema: Prioridad de Propiedades

Si una clase tiene una propiedad y un método con el mismo nombre, **la propiedad (el dato) siempre tiene prioridad sobre el método**. Esto se conoce como "Sombreado" (*Shadowing*).

**El Conflicto:**

```latino
clase Ejemplo
    constructor()
        mi.nombre = "Juan"  # Propiedad (dato: texto)
    fin
    
    funcion nombre()        # ❌ CONFLICTO: Mismo nombre que la propiedad
        retornar "Soy el método nombre"
    fin
fin

obj = nueva Ejemplo()

# Al intentar acceder a 'nombre':
escribir(obj.nombre)  # Salida: "Juan" (la propiedad)

# Al intentar llamar al método:
# obj.nombre()  # ❌ ERROR: "Juan" no es una función
```

**¿Qué sucede?**

Cuando escribes `obj.nombre`, el intérprete:

1. Busca primero si existe una propiedad llamada `nombre` → **La encuentra: "Juan"**
2. Retorna ese valor y **nunca llega a buscar el método**
3. Si intentas ejecutarlo con `()`, falla porque estás tratando de ejecutar un texto

#### La Solución: Nombres Descriptivos

Usa nombres diferentes y descriptivos. Una buena práctica es usar **verbos** para los métodos y **sustantivos** para las propiedades:

```latino
clase Persona
    constructor(nombreCompleto)
        mi.nombre = nombreCompleto  # Sustantivo (dato)
    fin
    
    funcion ObtenerNombre()         # Verbo + Sustantivo (acción)
        retornar "El nombre es: " .. mi.nombre
    fin
    
    funcion MostrarNombre()         # Verbo + Sustantivo (acción)
        escribir("Nombre: " .. mi.nombre)
    fin
    
    funcion CambiarNombre(nuevoNombre)  # Verbo + Sustantivo (acción)
        mi.nombre = nuevoNombre
        escribir("Nombre actualizado a: " .. mi.nombre)
    fin
fin

p = nueva Persona("Ana García")
escribir(p.nombre)           # Acceso directo a la propiedad: "Ana García"
escribir(p.ObtenerNombre())  # Llamada al método: "El nombre es: Ana García"
p.MostrarNombre()            # Muestra: "Nombre: Ana García"
p.CambiarNombre("Ana López") # Actualiza: "Nombre actualizado a: Ana López"
```

#### Convenciones Recomendadas

**Para Propiedades (datos):**

* Usa sustantivos simples: `nombre`, `edad`, `saldo`, `altura`
* Preferiblemente en singular

**Para Métodos (acciones):**

* Usa verbos o frases verbales: `Obtener`, `Calcular`, `Mostrar`, `Validar`
* Patrones comunes:
  * `ObtenerNombre()` / `AsignarNombre()`
  * `CalcularTotal()`
  * `MostrarDatos()`
  * `ValidarEdad()`

**Ejemplo Completo:**

```latino
clase CuentaBancaria
    constructor(titular, saldoInicial)
        # Propiedades (sustantivos)
        mi.titular = titular
        mi.saldo = saldoInicial
        mi.activa = verdadero
    fin
    
    # Métodos (verbos/acciones)
    funcion ObtenerSaldo()
        retornar mi.saldo
    fin
    
    funcion VerificarEstado()
        si mi.activa
            retornar "Cuenta activa"
        sino
            retornar "Cuenta inactiva"
        fin
    fin
    
    funcion Depositar(cantidad)
        mi.saldo = mi.saldo + cantidad
    fin
    
    funcion Retirar(cantidad)
        si cantidad <= mi.saldo
            mi.saldo = mi.saldo - cantidad
            retornar verdadero
        sino
            retornar falso
        fin
    fin
fin

cuenta = nueva CuentaBancaria("Carlos", 1000)
escribir(cuenta.titular)           # Propiedad: "Carlos"
escribir(cuenta.ObtenerSaldo())    # Método: 1000
escribir(cuenta.VerificarEstado()) # Método: "Cuenta activa"
```

### 5.8 Encapsulamiento y Privacidad

En Latino.NET, todas las propiedades y métodos de una clase son **públicos** por defecto. Esto significa que pueden ser leídos o modificados desde cualquier parte del código.

Sin embargo, el lenguaje ofrece **dos estrategias** para proteger tus datos: una basada en **convención social** y otra en **arquitectura técnica**.

#### Estrategia 1: Privacidad por Convención (El Guion Bajo)

La comunidad de Latino utiliza el prefijo **`_`** (guion bajo) para indicar que una propiedad o método es **interno** y no debería usarse desde fuera de la clase.

**El Pacto:**

> *"Si su nombre comienza con un guion bajo `_`, prometo tratarlo como privado."*

**Importante:** El lenguaje **NO** bloquea técnicamente el acceso a estas variables. La convención funciona como una **señal de advertencia** clara para otros programadores (y para tu yo del futuro).

**Ejemplo Básico:**

```latino
clase Usuario
    constructor(nombre)
        mi.nombre = nombre          # ✅ Pública (úsala libremente)
        mi._token = "abc123"        # ⚠️ Privada por convención
        mi._ultimoAcceso = "hoy"    # ⚠️ Privada por convención
    fin
    
    funcion _generarToken()         # ⚠️ Método privado por convención
        retornar "token_secreto_" .. mi.nombre
    fin
    
    funcion ObtenerTokenPublico()   # ✅ Método público
        # Acceso interno permitido
        retornar mi._token
    fin
    
    funcion RenovarToken()          # ✅ Método público
        # Los métodos privados se usan internamente
        mi._token = mi._generarToken()
        escribir("Token renovado")
    fin
fin

# Uso correcto
usuario = nuevo Usuario("Ana")
escribir(usuario.nombre)                # ✅ "Ana" - Acceso público correcto
escribir(usuario.ObtenerTokenPublico()) # ✅ "abc123" - Interfaz pública

# Uso desaconsejado (funciona, pero rompe el contrato)
escribir(usuario._token)                # ⚠️ Funciona, pero no recomendado
usuario._token = "hackeado"             # ⚠️ Modifica dato interno (mala práctica)
```

**¿Cuándo usar esta estrategia?**

Para la mayoría de los casos donde quieres indicar "esto es interno, pero si realmente lo necesitas, está ahí".

#### Estrategia 2: Privacidad Estricta (Patrón de Clausura)

Si necesitas que un dato sea **matemáticamente inaccesible** desde el exterior (es decir, que ni siquiera usando `objeto._variable` se pueda leer), puedes usar **variables `local`** dentro del `constructor`.

**¿Cómo funciona?**

Al declarar una variable como `local` dentro del constructor, esta variable deja de existir para el mundo exterior cuando termina la construcción. Sin embargo, las funciones que definas *dentro* de ese constructor pueden seguir viéndola y usándola. Es como un "recuerdo" privado del objeto.

* **Concepto: Clausura (Closure)**

Una clausura es un entorno cerrado donde:

1. Defines variables locales
2. Defines funciones que "recuerdan" esas variables
3. Las funciones sobreviven, pero las variables quedan ocultas

🎯 **Ejemplo 1: Privacidad estricta en clases mediante clausuras (El Diario Secreto)**

```latino
clase Diario
    constructor(mensaje)
        # Variable local: Solo existe dentro de este bloque
        local secreto = mensaje 

        # Propiedad pública: Visible para todos
        mi.titulo = "Mi Vida"

        # Método privilegiado:
        # Como se define AQUÍ, puede ver la variable 'secreto'
        mi.lectura = funcion()
            retornar "El secreto es: " .. secreto
        fin
        
        mi.ActualizarSecreto = funcion(nuevoMensaje)
            secreto = nuevoMensaje
            escribir("Secreto actualizado")
        fin
    fin
fin

# Uso
d = nueva Diario("Me gusta programar")

escribir(d.titulo)    # Salida: Mi Vida
escribir(d.lectura())    # Salida: El secreto es: Me gusta programar

# Intento de acceso directo:
escribir(d.secreto)   # Salida: nulo (¡No existe fuera del constructor!)

# Actualización controlada
d.ActualizarSecreto("Ahora me encanta Latino.NET")
escribir(d.lectura())    # Salida: El secreto es: Ahora me encanta Latino.NET
```

Aquí usamos la privacidad para proteger reglas de seguridad, impidiendo que el dato se modifique sin validación.

🎯 **Ejemplo 2: Validación interna e invariantes de seguridad (La Caja Fuerte)**

```latino
clase CajaFuerte
    constructor(claveInicial)
        # 1. Estado Privado (Solo visible dentro de este bloque)
        local _secreto = claveInicial

        # 2. Método Auxiliar Privado (Helper local)
        # Esta función solo existe aquí adentro
        local _validar = funcion(intento)
            retornar intento == _secreto
        fin

        # 3. Métodos Públicos (Expuestos en 'mi')
        # Estos métodos "recuerdan" (capturan) las variables locales
        
        mi.Abrir = funcion(clave)
            si _validar(clave)
                retornar "🔓 Caja Abierta"
            sino
                retornar "🔒 Acceso Denegado"
            fin
        fin
        
        mi.CambiarClave = funcion(actual, nuevaClave)
            si _validar(actual)
                _secreto = nuevaClave
                retornar verdadero
            fin
            retornar falso
        fin
        
        mi.VerificarClave = funcion(intento)
            retornar _validar(intento)
        fin
    fin
fin

# Uso
caja = nueva CajaFuerte("1234")

escribir(caja.Abrir("0000"))  # Salida: 🔒 Acceso Denegado
escribir(caja.Abrir("1234"))  # Salida: 🔓 Caja Abierta

# Intentos de hackeo fallidos:
escribir(caja._secreto)       # Salida: nulo (La variable es invisible)

# Cambio seguro de clave
si caja.CambiarClave("1234", "5678")
    escribir("Clave cambiada exitosamente")
fin

escribir(caja.Abrir("1234"))  # Salida: 🔒 Acceso Denegado (clave antigua)
escribir(caja.Abrir("5678"))  # Salida: 🔓 Caja Abierta (nueva clave)
```

#### Estrategia 3: Privacidad sin Clases (Patrón Factory)

Otra forma de lograr privacidad real es mediante **funciones que retornan diccionarios de métodos**. Este patrón crea clausuras donde las variables locales son verdaderamente privadas.

🎯 **Ejemplo 1: El Contador Intocable**

```latino
funcion CrearContador()
    # Variable local privada (nadie la puede tocar desde fuera)
    local _conteo = 0

    # Retornamos un objeto con métodos que acceden a _conteo
    retornar {
        "Incrementar": funcion() 
            _conteo = _conteo + 1
        fin,
        
        "Decrementar": funcion()
            _conteo = _conteo - 1
        fin,
        
        "Obtener": funcion() 
            retornar _conteo 
        fin,
        
        "Reiniciar": funcion()
            _conteo = 0
        fin
    }
fin

# Uso
c = CrearContador()
c.Incrementar()
c.Incrementar()
c.Incrementar()
escribir(c.Obtener())  # Salida: 3

c.Decrementar()
escribir(c.Obtener())  # Salida: 2

# Intento de acceso directo (causará error):
intentar
    escribir(c._conteo)
atrapar (error)
    escribir("❌ Error capturado: " .. error)
    # Salida: ❌ Error capturado: [Línea X] El diccionario no tiene la clave '_conteo' ni existe el método dic._conteo().
    escribir("La variable '_conteo' es verdaderamente inaccesible")
fin

# Nota: El manejo de errores con intentar/atrapar se verá en detalle en el capítulo 7

c.Reiniciar()
escribir(c.Obtener())  # Salida: 0
```

🎯 **Ejemplo 2: Lógica de Negocio (Cuenta Bancaria)**

```latino
funcion CrearCuenta(saldoInicial)
    local _saldo = saldoInicial
    local _historial = [ ]

    retornar {
        "Depositar": funcion(cantidad)
            _saldo = _saldo + cantidad
            escribir("Depositado: $" .. cantidad)
        fin,
        
        "Retirar": funcion(cantidad)
            si cantidad > _saldo
                escribir("Error: Fondos insuficientes.")
                retornar falso
            sino
                _saldo = _saldo - cantidad
                escribir("Retirado: $" .. cantidad)
                retornar verdadero
            fin
        fin,
        
        "VerSaldo": funcion()
            retornar _saldo
        fin,
        
        "Transferir": funcion(otraCuenta, cantidad)
            si cantidad <= _saldo
                _saldo = _saldo - cantidad
                otraCuenta.Depositar(cantidad)
                escribir("Transferencia exitosa: $" .. cantidad)
                retornar verdadero
            sino
                escribir("Error: Fondos insuficientes para transferir")
                retornar falso
            fin
        fin
    }
fin

# Uso
cuentaAna = CrearCuenta(1000)
cuentaCarlos = CrearCuenta(500)

cuentaAna.Depositar(500)       # Depositado: $500
escribir(cuentaAna.VerSaldo()) # Salida: 1500

cuentaAna.Retirar(2000)        # Error: Fondos insuficientes.
cuentaAna.Retirar(200)         # Retirado: $200

# Transferencia entre cuentas
cuentaAna.Transferir(cuentaCarlos, 300)  # Transferencia exitosa: $300
escribir(cuentaAna.VerSaldo())           # Salida: 1000
escribir(cuentaCarlos.VerSaldo())        # Salida: 800

# Intento de hackeo (causará error):
intentar
    escribir(cuentaAna._saldo)
atrapar (error)
    escribir("❌ Error capturado: " .. error)
    # Salida: ❌ Error capturado: [Línea X] El diccionario no tiene la clave '_saldo' ni existe el método dic._saldo().
    escribir("¡El saldo está completamente protegido!")
fin

# Nota: El manejo de errores con intentar/atrapar se verá en detalle en el capítulo 7
```

**¿Por qué el Error Demuestra Privacidad Real?**

El hecho de que Latino.NET lance un error informando que la clave no existe en el diccionario es **exactamente la prueba** de que la variable local es inaccesible. No es que retorne `nulo` (que indicaría que existe pero está vacía), es que **literalmente no existe** en el objeto retornado.

Esto contrasta con la convención del guion bajo (`mi._token`), donde la propiedad SÍ existe y es accesible, pero por convención social "prometemos" no tocarla.

#### Tabla Comparativa Actualizada

| Estrategia | Accesibilidad | Comportamiento al Intentar Acceso | Uso Recomendado |
| :--- | :--- | :--- | :--- |
| **Convención `_`** | Técnicamente accesible | Retorna el valor (sin error) | Mayoría de casos, señal de "no tocar" |
| **Clausura en Clases** | Matemáticamente inaccesible | Retorna `nulo` (sin error) | Datos sensibles en clases |
| **Patrón Factory** | Matemáticamente inaccesible | Lanza error de runtime | Lógica de negocio sin herencia |

**¿Cuándo usar cada una?**

* **Convención de guion bajo:** Para la mayoría de casos donde quieres indicar "esto es interno, pero el lenguaje me permite acceder si es necesario". Acceso directo funciona pero no es recomendado.

* **Patrón de clausura en clases:** Cuando necesitas protección real de datos sensibles (contraseñas, tokens, claves) dentro de una jerarquía de clases. El acceso directo a la variable cerrada devuelve `nulo` sin lanzar error, así que **no necesitas `intentar/atrapar`**: el dato sigue protegido, solo que invisible.

* **Patrón Factory:** Cuando necesitas privacidad estricta pero no requieres herencia ni jerarquías complejas. Ideal para módulos independientes. Aquí el acceso directo **sí causa error de runtime**, porque el objeto es un diccionario y un diccionario **no devuelve `nulo` ante una clave ausente**: lanza `El diccionario no tiene la clave 'X' ni existe el método dic.X()`.

> **⚠️ No confundas los dos errores.** El comportamiento ante un acceso fallido depende del **tipo de objeto**, no del patrón de privacidad:
>
> | Tipo de objeto | Acceso a un nombre inexistente | Motivo |
> | :--- | :--- | :--- |
> | Instancia de clase (`new`) | Retorna `nulo` en silencio | Las clases heredan el acceso permisivo de Latino |
> | Diccionario | **Lanza error de runtime** | Las claves deben existir |
>
> Verificado ejecutando los dos casos. Si escribes `intentar/atrapar` para "proteger" una propiedad de clase, el bloque nunca se disparará.

### 5.9 Sobrecarga de Métodos

**⚠️ Advertencia Importante:**

Latino.NET sigue el modelo de los lenguajes dinámicos modernos (como JavaScript o Python): **no existe la sobrecarga de métodos** por firma (overloading).

Si defines un método dos veces con el mismo nombre dentro de una clase, **la segunda definición sobrescribe y borra a la primera**, independientemente de cuántos parámetros tenga cada una.

#### Comportamiento de Sobrescritura en Latino.NET

```latino
clase Calculadora
    # ❌ INCORRECTO: Esta definición se perderá
    funcion Sumar(a, b)
        retornar a + b
    fin
    
    # ❌ INCORRECTO: Esta sobrescribe a la anterior
    funcion Sumar(a, b, c)
        retornar a + b + c
    fin
fin

calc = nueva Calculadora()

# Error Runtime: Falta el argumento obligatorio 'c'.
# (solo existe la segunda versión)
# resultado = calc.Sumar(10, 20)
```

**¿Qué sucedió?**

El intérprete solo "recuerda" la última definición de `Sumar`. La primera versión (con 2 parámetros) fue completamente reemplazada.

#### Implementación Mediante Parámetros Opcionales

Para lograr el mismo efecto (un método que acepte diferente cantidad de argumentos), debes usar **parámetros opcionales** con valores por defecto.

```latino
clase CalculadoraInteligente
    # ✅ CORRECTO: Un solo método flexible
    funcion Sumar(a, b = 0, c = 0)
        retornar a + b + c
    fin
fin

calc = nueva CalculadoraInteligente()

escribir(calc.Sumar(5))           # Salida: 5 (b=0, c=0)
escribir(calc.Sumar(5, 3))        # Salida: 8 (c=0)
escribir(calc.Sumar(5, 3, 2))     # Salida: 10 (todos los argumentos)
```

#### Patrón Avanzado: Validación de Argumentos

Puedes hacer que un método sea aún más flexible usando validación interna:

```latino
clase Operaciones
    funcion Multiplicar(a, b = nulo, c = nulo)
        # Validamos cuántos argumentos reales recibimos
        si b == nulo
            # Solo un argumento: elevar al cuadrado
            retornar a * a
        osi c == nulo
            # Dos argumentos: multiplicación simple
            retornar a * b
        sino
            # Tres argumentos: multiplicación triple
            retornar a * b * c
        fin
    fin
fin

op = nueva Operaciones()

escribir(op.Multiplicar(5))        # Salida: 25 (5²)
escribir(op.Multiplicar(5, 3))     # Salida: 15
escribir(op.Multiplicar(5, 3, 2))  # Salida: 30
```

#### Ventajas de este Enfoque

1. **Compatibilidad:** Funciona consistentemente en todos los casos
2. **Claridad:** Un solo método con múltiples formas de uso
3. **Mantenimiento:** Más fácil de entender y modificar
4. **Consistencia:** Alineado con la filosofía de lenguajes dinámicos

### 5.10 Polimorfismo y Duck Typing

En lenguajes estáticos (como C# o Java), para que dos objetos sean intercambiables, deben heredar de la misma clase o implementar la misma interfaz.

En Latino.NET, utilizamos el concepto de **Duck Typing** ("Tipado de Pato"):

> *"Si camina como un pato y hace cuac como un pato, entonces es un pato."*

Esto significa que **no necesitas declarar tipos**. Si pasas un objeto a una función y ese objeto tiene los métodos necesarios, funcionará.

🎯 **Ejemplo Básico de Duck Typing**

```latino
clase Pato
    funcion Sonido()
        retornar "¡Cuac cuac!"
    fin
    
    funcion Caminar()
        retornar "Caminando como pato..."
    fin
fin

clase Perro
    funcion Sonido()
        retornar "¡Guau guau!"
    fin
    
    funcion Caminar()
        retornar "Caminando como perro..."
    fin
fin

clase Robot
    funcion Sonido()
        retornar "Beep boop"
    fin
    
    funcion Caminar()
        retornar "Moviendo servomotores..."
    fin
fin

# Esta función acepta CUALQUIER cosa que tenga un método 'Sonido'
funcion HacerRuido(animal)
    escribir(animal.Sonido())
fin

# Esta función acepta CUALQUIER cosa que tenga 'Sonido' y 'Caminar'
funcion RealizarAcciones(entidad)
    escribir(entidad.Caminar())
    escribir(entidad.Sonido())
fin

# Uso polimórfico
miPato = nueva Pato()
miPerro = nuevo Perro()
miRobot = nuevo Robot()

HacerRuido(miPato)   # Salida: ¡Cuac cuac!
HacerRuido(miPerro)  # Salida: ¡Guau guau!
HacerRuido(miRobot)  # Salida: Beep boop

escribir("--- Realizando acciones ---")
RealizarAcciones(miPato)
# Salida:
# Caminando como pato...
# ¡Cuac cuac!

RealizarAcciones(miRobot)
# Salida:
# Moviendo servomotores...
# Beep boop
```

#### Polimorfismo con Herencia

Cuando combinas herencia con polimorfismo, obtienes código muy flexible:

```latino
clase Forma
    funcion CalcularArea()
        retornar 0  # Método base genérico
    fin
    
    funcion Describir()
        escribir("Soy una forma genérica")
    fin
fin

clase Rectangulo es Forma
    constructor(ancho, alto)
        mi.ancho = ancho
        mi.alto = alto
    fin
    
    funcion CalcularArea()
        retornar mi.ancho * mi.alto
    fin
    
    funcion Describir()
        escribir("Soy un rectángulo de " .. mi.ancho .. "x" .. mi.alto)
    fin
fin

clase Circulo es Forma
    constructor(radio)
        mi.radio = radio
    fin
    
    funcion CalcularArea()
        retornar 3.14159 * mi.radio * mi.radio
    fin
    
    funcion Describir()
        escribir("Soy un círculo de radio " .. mi.radio)
    fin
fin

# Función polimórfica: acepta cualquier Forma
funcion MostrarInformacion(forma)
    forma.Describir()
    escribir("Área: " .. forma.CalcularArea())
    escribir("---")
fin

# Uso
rect = nuevo Rectangulo(5, 3)
circ = nuevo Circulo(4)

MostrarInformacion(rect)
# Salida:
# Soy un rectángulo de 5x3
# Área: 15
# ---

MostrarInformacion(circ)
# Salida:
# Soy un círculo de radio 4
# Área: 50.26544
# ---
```

### 5.11 Verificación de Tipos (`tipo`)

A veces necesitas saber exactamente qué clase de objeto tienes, especialmente cuando usas polimorfismo.

La función global **`tipo()`** devuelve una cadena que identifica el objeto.

#### Valores de Retorno

* **Para tipos primitivos:** `"decimal"`, `"cadena"`, `"logico"`
* **Para objetos:** Devuelve el **nombre de la clase** (ej: `"Perro"`, `"CuentaBancaria"`)

> **⚠️ Nota Técnica (v1.0.81):** `tipo()` debe usarse sobre **instancias**, no sobre identificadores de clase. Si ejecutas `tipo(MiClase)` (pasando la clase en sí), el motor devuelve `"desconocido"` porque `LatinoClass` carece de override de `TypeName` y hereda el valor por defecto de `LatinoObject`. Siempre pasa la **instancia** al verificar el tipo.

**Ejemplo Básico:**

```latino
clase Animal
    funcion Comer()
        escribir("El animal está comiendo.")
    fin
fin

clase Perro es Animal
    funcion Ladrar()
        escribir("¡Guau!")
    fin
fin

clase Gato es Animal
    funcion Maullar()
        escribir("¡Miau!")
    fin
fin

# Creamos instancias
p = nuevo Perro()
g = nuevo Gato()
numero = 42
texto = "Hola"

# Verificación de tipos
escribir("Tipo de p: " .. tipo(p))        # Salida: Perro
escribir("Tipo de g: " .. tipo(g))        # Salida: Gato
escribir("Tipo de numero: " .. tipo(numero))  # Salida: decimal
escribir("Tipo de texto: " .. tipo(texto))    # Salida: cadena

# Uso condicional
si tipo(p) == "Perro"
    escribir("¡Es un canino!")
    p.Ladrar()
fin
```

#### Uso Práctico: Función Polimórfica Inteligente

```latino
clase Perro
    funcion HacerRuido()
        escribir("¡Guau!")
    fin
    
    funcion Trucos()
        escribir("El perro da la patita")
    fin
fin

clase Gato
    funcion HacerRuido()
        escribir("¡Miau!")
    fin
    
    funcion Trucos()
        escribir("El gato ignora tus órdenes")
    fin
fin

clase Pajaro
    funcion HacerRuido()
        escribir("¡Pío pío!")
    fin
fin

funcion InteractuarConAnimal(animal)
    # Primero hacemos el ruido (todos lo tienen)
    animal.HacerRuido()
    
    # Luego verificamos el tipo para acciones específicas
    local tipoAnimal = tipo(animal)
    
    si tipoAnimal == "Perro"
        escribir("¡Es un perro! Probemos un truco:")
        animal.Trucos()
    osi tipoAnimal == "Gato"
        escribir("¡Es un gato! Intentemos un truco:")
        animal.Trucos()
    sino
        escribir("No sé qué trucos hace un " .. tipoAnimal)
    fin
fin

# Uso
perro = nuevo Perro()
gato = nuevo Gato()
pajaro = nuevo Pajaro()

InteractuarConAnimal(perro)
# Salida:
# ¡Guau!
# ¡Es un perro! Probemos un truco:
# El perro da la patita

escribir("---")

InteractuarConAnimal(gato)
# Salida:
# ¡Miau!
# ¡Es un gato! Intentemos un truco:
# El gato ignora tus órdenes

escribir("---")

InteractuarConAnimal(pajaro)
# Salida:
# ¡Pío pío!
# No sé qué trucos hace un Pajaro
```

### 5.12 Composición y Delegación (Composición sobre Herencia)

En la Programación Orientada a Objetos tradicional, el error de diseño más frecuente es **abusar de la herencia para reutilizar código**. Cuando una clase hereda de otra únicamente para aprovechar dos o tres funciones, se crea un acoplamiento rígido conocido como el *problema de la clase base frágil*: cualquier cambio en el padre puede quebrar silenciosamente el comportamiento de todos los hijos.

Latino.NET establece un mandato arquitectónico claro: **Herencia Simple Estricta** (una clase solo puede extender de una única superclase mediante `es`). Para combinar múltiples comportamientos y reutilizar lógica sin jerarquías rígidas, el lenguaje promueve oficialmente la regla de oro del diseño moderno:

> **"Prefiere la composición sobre la herencia"** (*Composition over Inheritance*).

#### 🎯 Herencia frente a Composición

Para saber qué herramienta usar, responde a la naturaleza de la relación entre tus objetos:

| Criterio | Herencia (`clase Hija es Padre`) | Composición (`mi.pieza = nueva Pieza()`) |
| :--- | :--- | :--- |
| **Relación semántica** | **"ES UN"** (*is-a*) — Taxonomía estricta. | **"TIENE UN"** o **"USA UN"** (*has-a* / *uses-a*) — Ensamblaje. |
| **Ejemplo conceptual** | Un `Perro` **es un** `Animal`. Un `Círculo` **es una** `Figura`. | Un `Auto` **tiene un** `Motor`. Un `Heroe` **tiene un** `Arma`. |
| **Acoplamiento** | **Fuerte:** La subclase depende de la estructura interna del padre. | **Débil:** La clase anfitriona solo interactúa con la interfaz pública de la pieza. |
| **Constructores** | **Encadenados:** Los ancestros se ejecutan en cadena jerárquica. | **Aislados:** La pieza se recibe o ensambla completa sin arrastrar dependencias. |
| **Flexibilidad en runtime** | **Estática:** No puedes cambiar de padre en mitad del programa. | **Dinámica:** Puedes cambiar la pieza interna en caliente en cualquier momento. |

#### 🔧 La Técnica de Delegación

La **Composición** consiste en guardar una instancia dentro de otra (`mi.motor = motor`). La **Delegación** es el paso siguiente: la clase exterior expone métodos propios que llaman a los métodos del componente interno, ofreciendo una fachada simple y controlada hacia el exterior.

#### 🎯 Ejemplo: Auto y Motor (Composición y Sustitución en Caliente)

Un `Auto` no "es un" `Motor`; un `Auto` **tiene un** `Motor`. Al ensamblarlo como pieza, podemos delegar el encendido y además reemplazar el motor por otro más potente en tiempo de ejecución:

```latino
clase Motor
    constructor(tipo, potencia)
        mi.tipo = tipo
        mi.potencia = potencia
        mi.encendido = falso
    fin
    
    funcion Encender()
        si mi.encendido
            retornar "El motor ya está encendido"
        sino
            mi.encendido = cierto
            retornar ç("Run run... ¡Motor |mi.tipo| listo! (|mi.potencia| HP)")
        fin
    fin
    
    funcion Apagar()
        mi.encendido = falso
        retornar "Motor apagado"
    fin
fin

clase Auto
    constructor(marca, modelo, motor)
        mi.marca = marca
        mi.modelo = modelo
        
        # Composición: El auto guarda la pieza interna (Inyección de dependencia)
        mi.motor = motor
    fin
    
    # Delegación: El Auto ofrece la acción 'Arrancar', delegando en 'mi.motor'
    funcion Arrancar()
        escribir(ç("Girando la llave del |mi.marca| |mi.modelo|..."))
        local mensaje = mi.motor.Encender()
        escribir(mensaje)
    fin
    
    funcion Apagar()
        local mensaje = mi.motor.Apagar()
        escribir(mensaje)
    fin
    
    # Sustitución en caliente: cambiar la pieza en tiempo de ejecución
    funcion InstalarMotor(nuevoMotor)
        mi.motor = nuevoMotor
        escribir(ç("Nuevo motor |nuevoMotor.tipo| instalado en el |mi.modelo|"))
    fin
fin

# 1. Creamos la pieza y ensamblamos el auto
motorV6 = nueva Motor("V6", 300)
miCarro = nueva Auto("Toyota", "Corolla", motorV6)

miCarro.Arrancar()
# Salida:
# Girando la llave del Toyota Corolla...
# Run run... ¡Motor V6 listo! (300 HP)

miCarro.Arrancar()
# Salida:
# Girando la llave del Toyota Corolla...
# El motor ya está encendido

miCarro.Apagar()
# Salida: Motor apagado

# 2. Reemplazo de la pieza en caliente (sin herencia ni jerarquías rígidas)
motorDeportivo = nueva Motor("V8 BiTurbo", 550)
miCarro.InstalarMotor(motorDeportivo)
# Salida: Nuevo motor V8 BiTurbo instalado en el Corolla

miCarro.Arrancar()
# Salida:
# Girando la llave del Toyota Corolla...
# Run run... ¡Motor V8 BiTurbo listo! (550 HP)
```

#### 🎮 Ejemplo Complejo: Sistema Modular de Videojuego

En sistemas más avanzados (como inventarios, combate o interfaces gráficas), la composición brilla al permitir que una entidad combine múltiples capacidades independientes sin caer en la maraña de clases abstractas o herencia múltiple:

```latino
clase Arma
    constructor(nombre, danio)
        mi.nombre = nombre
        mi.danio = danio
    fin
    
    funcion Atacar()
        retornar ç("¡Ataque con |mi.nombre| causa |mi.danio| puntos de daño!")
    fin
fin

clase Inventario
    constructor()
        mi.articulos = [ ]
    fin
    
    funcion Guardar(item)
        lista.agregar(mi.articulos, item)
        escribir(ç("Guardado en bolsa: |item|"))
    fin
    
    funcion Listar()
        retornar lista.separar(mi.articulos, ", ")
    fin
fin

clase Heroe
    constructor(nombre)
        mi.nombre = nombre
        mi.salud = 100
        
        # Composición Múltiple: El héroe TIENE un arma y TIENE un inventario
        mi.arma = nueva Arma("Espada Corta", 15)
        mi.bolsa = nueva Inventario()
    fin
    
    # Delegamos el combate al objeto Arma
    funcion Atacar()
        escribir(ç("|mi.nombre| entra en combate:"))
        escribir("  " .. mi.arma.Atacar())
    fin
    
    # Equipar una pieza nueva sustituyendo la referencia interna
    funcion EquiparArma(nuevaArma)
        mi.arma = nuevaArma
        escribir(ç("|mi.nombre| empuña ahora: |nuevaArma.nombre|"))
    fin
    
    # Delegamos la recolección al objeto Inventario
    funcion Recoger(item)
        mi.bolsa.Guardar(item)
    fin
    
    funcion Inspeccionar()
        escribir(ç("=== Ficha de |mi.nombre| ==="))
        escribir(ç("Salud: |mi.salud|/100"))
        escribir(ç("Arma actual: |mi.arma.nombre| (|mi.arma.danio| daño)"))
        items = mi.bolsa.Listar()
        escribir(ç("Bolsa: [|items|]"))
    fin
fin

# Uso del sistema compuesto
aventurero = nueva Heroe("Rodrigo")
aventurero.Inspeccionar()
# Salida:
# === Ficha de Rodrigo ===
# Salud: 100/100
# Arma actual: Espada Corta (15 daño)
# Bolsa: []

aventurero.Atacar()
# Salida:
# Rodrigo entra en combate:
#   ¡Ataque con Espada Corta causa 15 puntos de daño!

# Recolectar ítems y cambiar de equipamiento en caliente
aventurero.Recoger("Poción Roja")
aventurero.Recoger("Antorcha")
# Salida:
# Guardado en bolsa: Poción Roja
# Guardado en bolsa: Antorcha

armaEpica = nueva Arma("Hacha de Obsidiana", 65)
aventurero.EquiparArma(armaEpica)
# Salida: Rodrigo empuña ahora: Hacha de Obsidiana

aventurero.Atacar()
# Salida:
# Rodrigo entra en combate:
#   ¡Ataque con Hacha de Obsidiana causa 65 puntos de daño!
```

#### 💡 Ventajas de la Composición en Latino.NET

1. **Inmunidad al Problema de Constructores en Cascada:** A diferencia de la herencia (donde los ancestros deben coincidir estrictamente con sus valores por defecto), la clase anfitriona recibe o inicializa sus piezas ya terminadas sin preocuparse por la cadena de llamadas del padre.
2. **Sustitución en Caliente (*Duck Typing*):** Cualquier objeto que implemente las funciones esperadas puede asignarse como pieza interna en tiempo de ejecución sin exigir heredar de una clase base común.
3. **Mantenimiento y Pruebas Unitarias:** Cada componente (`Motor`, `Arma`, `Inventario`) puede probarse de forma aislada en sus propios scripts de prueba sin necesidad de instanciar la clase contenedora entera.
4. **Reutilización Transversal:** Un mismo componente (`Inventario` o `Motor`) puede ser reutilizado por clases completamente distintas (`Auto`, `Lancha`, `Avion`) que no comparten ningún lazo familiar.

## Los Cuatro Pilares de la POO son soportados por Latino.NET

### Concretamente Tenemos

1. **Abstracción**
   * Definir **Clases** con `clase`
   * Crear objetos con cuatro estilos de instanciación
   * Usar **constructores** para inicializar

2. **Encapsulamiento**
   * Proteger datos con **convención `_`**
   * Privacidad estricta con **variables `local`**
   * **Patrón Factory** para clausuras

3. **Herencia**
   * Reutilizar código con `es`
   * Entender el **orden de resolución** (MRO)
   * **Sobrescribir métodos** del padre

4. **Polimorfismo**
   * Tratar diferentes objetos de la misma forma
   * **Duck Typing** en acción
   * Verificar tipos con `tipo()`

### Conceptos Avanzados

✅ Referencias a la instancia: `mi`, `este`, `esta`  
✅ Reglas de **Shadowing** (propiedades vs métodos)  
✅ **Parámetros opcionales** en lugar de sobrecarga  
✅ **Composición** como alternativa a herencia múltiple  

## Dominando esto tienes el poder para

* Estructurar aplicaciones profesionales y ordenadas
* Crear jerarquías lógicas de clases
* Proteger datos sensibles con encapsulamiento
* Escribir código reutilizable y mantenible
* Combinar comportamientos complejos con composición

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
