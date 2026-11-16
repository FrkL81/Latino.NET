# CAPÍTULO 3: Estructuras de Datos

## Más Allá de las Variables Simples

Hasta este punto hemos trabajado exclusivamente con variables escalares: identificadores que almacenan un único valor a la vez (un número, una cadena de texto o un valor lógico). Sin embargo, el software del mundo real rara vez resuelve problemas con datos aislados. Un sistema de ventas gestiona cientos de productos en un carrito, una plataforma web administra listas de usuarios y un modelo de ingeniería procesa series de miles de coordenadas.

Intentar abordar estos escenarios con variables individuales (`producto1`, `producto2`, `producto3`... `producto100`) convertiría el código en una pesadilla imposible de mantener y de iterar. Para resolver este desafío existen las **estructuras de datos**: modelos organizados que permiten agrupar, relacionar y gobernar conjuntos de información bajo una sola variable, aplicando reglas de acceso predecibles y eficientes.

### El Catálogo de Colecciones en Latino.NET

Detrás de su sintaxis limpia en español, Latino.NET respalda cada una de sus colecciones sobre las estructuras de datos optimizadas de **.NET 10**. El lenguaje clasifica sus herramientas de almacenamiento en dos grandes familias:

1. **Estructuras Literales Nativas (Integradas en el Núcleo):**
   - **Listas (`[1, 2, 3]`)** → Secuencias ordenadas y dinámicas. Son el caballo de batalla para colecciones que crecen o se encogen y cuyos elementos se consultan por posición numérica (`0, 1, 2...`).
   - **Diccionarios (`{"clave": valor}`)** → Mapas asociativos de acceso directo. Perfectos para representar entidades complejas (objetos, registros, configuraciones) donde cada dato se localiza mediante un identificador con nombre propio.
   - **Tuplas (`(x, y, z)`)** → Registros posicionales ligeros de longitud fija. Diseñadas para empaquetar datos heterogéneos inseparables (como coordenadas cartesianas o retornos múltiples de funciones).

2. **Estructuras Especializadas (Biblioteca Estándar):**
   - **Conjuntos (`conjunto`)** → Colecciones matemáticas donde los duplicados están prohibidos y se aplican operaciones directas de unión, intersección y diferencia.
   - **Pilas (`pila`)** → Colecciones con disciplina **LIFO** (*Último en entrar, primero en salir*), indispensables para historiales de acciones, navegación hacia atrás y analizadores.
   - **Colas (`cola`)** → Colecciones con disciplina **FIFO** (*Primero en entrar, primero en salir*), diseñadas para flujos de tareas, líneas de espera y procesamiento de eventos por turnos.

> 💡 **Criterio de Selección:**  
> En programación, elegir la estructura de datos equivocada arruina el mejor de los algoritmos. A lo largo de este capítulo no solo aprenderás a crear y manipular cada contenedor, sino a desarrollar el criterio técnico para elegir exactamente la estructura adecuada para cada problema de negocio.

## 3.1 Listas

Las listas son colecciones ordenadas de elementos que pueden cambiar de tamaño. Son el tipo de colección más común y versátil en Latino.

### Características de las Listas

| Característica | Descripción |
| :--- | :--- |
| **Sintaxis Literal** | Se crean usando corchetes `[]` |
| **Índices** | El acceso a los elementos es de base 0 (el primer elemento es el índice 0) |
| **Heterogéneas** | Pueden contener elementos de diferentes tipos |
| **Mutables** | Se pueden modificar después de crearlas |

### Creación y Acceso Básico

```latino
# Lista vacía
vacia = [ ]

# Lista con elementos
mixta = [1, "dos", verdadero, nulo]

# Acceso por índice
primerElemento = mixta[0]  # 1
escribir(primerElemento)
```

> **Nota:** Los índices comienzan en 0, no en 1. El primer elemento es `mixta[0]`, el segundo `mixta[1]`, y así sucesivamente. Intentar acceder a un índice fuera del rango causará un error.

### Índices Negativos (Acceso desde el Final)

Latino.NET soporta índices negativos para acceder a elementos contando desde el final de la lista hacia atrás:

- `-1` → Último elemento
- `-2` → Penúltimo elemento
- `-n` → Primer elemento

```latino
letras = ["A", "B", "C", "D"]

# LECTURA
escribir(letras[-1])  # "D"
escribir(letras[-2])  # "C"
escribir(letras[-4])  # "A"

# ESCRITURA (Modificación)
letras[-1] = "Z"
escribir(letras)      # ["A", "B", "C", "Z"]
```

> **💡 Beneficio:** Esto es extremadamente útil para obtener el último elemento de una lista de tamaño dinámico sin tener que calcular `lista.longitud(l) - 1`.

### Funciones de la Librería `lista`

La librería `lista` provee muchas funciones para manipularlas:

| Función | Descripción |
| :--- | :--- |
| `lista.agregar(l, item)` | Añade un elemento al final de la lista |
| `lista.eliminar_indice(l, i)` | Elimina el elemento en la posición `i` |
| `lista.longitud(l)` | Devuelve el número de elementos |
| `lista.ordenar(l, [func])` | Ordena la lista (opcional: función comparadora) |
| `lista.invertir(l)` | Invierte el orden de los elementos |

### Funciones de Orden Superior

Latino soporta un estilo de programación funcional con listas. Estas funciones permiten procesar elementos de forma declarativa sin usar bucles explícitos:

```latino
numeros = [1, 2, 3, 4]

# Map: Transformar cada elemento
dobles = lista.transformar(numeros, funcion(n)
    retornar n * 2
fin)

escribir(dobles) # dobles es [2, 4, 6, 8]

# Filter: Seleccionar elementos que cumplen una condición
pares = lista.filtrar(numeros, funcion(n)
    retornar n % 2 == 0
fin)

escribir(pares) # pares es [2, 4]
```

> **Consejo:** Las funciones de orden superior (`transformar`, `filtrar`) crean nuevas listas en lugar de modificar las originales. Esto promueve la inmutabilidad y evita efectos secundarios.

### Ordenamiento Personalizado

En Latino.NET, `lista.ordenar()` ya ordena números correctamente por defecto (detecta el tipo numérico y los ordena de forma nativa). La función comparadora personalizada se utiliza para criterios avanzados, como orden inverso o estructuras complejas:

```latino
# En Latino.NET, lista.ordenar() ya ordena números correctamente por defecto:
numeros = [10, 2, 5]
lista.ordenar(numeros)
escribir(numeros)  # Salida: [2, 5, 10]

# La función comparadora personalizada se utiliza para criterios avanzados (ej: descendente):
lista.ordenar(numeros, funcion(a, b)
    retornar a > b  # Orden descendente
fin)
escribir(numeros)  # Salida: [10, 5, 2]
```

### Listas Anidadas (Matrices)

Las listas pueden contener otras listas, formando estructuras multidimensionales útiles para matrices y tablas:

```latino
# Matriz 3x3
matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
]

# Acceso a elementos anidados
escribir(matriz[1][2])  # Salida: 6

# Modificar elementos anidados
matriz[0][1] = 99  # Cambia [1, 2, 3] a [1, 99, 3]

escribir(matriz) # Salida: [[1, 99, 3], [4, 5, 6], [7, 8, 9]]
```

### Operaciones con Listas

```latino
# Concatenación
lista1 = [1, 2]
lista2 = [3, 4]
combinada = lista.concatenar(lista1, lista2)  # [1, 2, 3, 4]

escribir(ç("Lista combinada: |combinada|"))

# Verificar existencia
frutas = ["manzana", "pera", "uva"]
si lista.contiene(frutas, "pera")
    escribir("La pera existe")
fin
```

## 3.2 Diccionarios

Los diccionarios son colecciones de pares `clave: valor`. Permiten un acceso muy rápido a los valores a través de su clave única, como un directorio telefónico donde buscas un nombre y obtienes un número.

### Características de los Diccionarios

| Característica | Descripción |
| :--- | :--- |
| **Sintaxis Literal** | Se crean usando llaves `{}` con pares `clave: valor` |
| **Claves Únicas** | Cada clave debe ser única en el diccionario |
| **Acceso Rápido** | O(1) - tiempo constante para buscar por clave |
| **Claves Heterogéneas** | Soportan claves de cualquier tipo (cadena, numérico, lógico, nulo) |

🎯 **Ejemplo: Creación y Acceso a un Diccionario**

```latino
persona = {
    "nombre": "Frank",
    "rol": "Documentador",
    "edad": 19,
    "activo": verdadero
}

# Acceso por clave
escribir(persona["nombre"])  # Salida: Frank

# Modificar valor
persona["edad"] = 28
escribir(persona["edad"])  # Salida: 28
```

- **Claves Heterogéneas**

Latino soporta claves de cualquier tipo de dato en el mismo diccionario gracias a un comparador robusto:

```latino
# Claves de diferentes tipos
config = {
    "nombre": "App",        # clave cadena
    123: "numérico",         # clave numérica
    verdadero: "lógico",     # clave lógica
    nulo: "valor nulo"       # clave nula
}

escribir(config[123])        # "numérico"
escribir(config[verdadero])  # "lógico"
```

### Acceso con Punto: Lectura y sus Límites

Sobre un diccionario, el punto permite **leer** un valor de forma equivalente a los corchetes, siempre que la clave sea un **identificador alfanumérico válido**:

```latino
persona = {
    "nombre": "Frank",
    "rol": "Documentador",
    "edad": 19
}

escribir(persona.nombre)          # Salida: Frank
escribir(persona["nombre"])       # Salida: Frank  — idéntico
```

Cuando la clave **no** es un identificador válido, los corchetes son obligatorios:

```latino
config = {
    "mi-clave": 1,
    "clave con espacio": 2,
    "2inicial": 3
}

escribir(config["mi-clave"])           # 1
escribir(config["clave con espacio"])  # 2
escribir(config["2inicial"])           # 3
```

⚠️ **Límites verificados del acceso con punto.** El punto no busca «la clave más parecida»: interpreta lo que va detrás como un identificador, y falla de forma distinta según el caso:

| Código | Qué ocurre |
| :--- | :--- |
| `d.inexistente` | `RuntimeError: El diccionario no tiene la clave 'inexistente' ni existe el método dic.inexistente().` |
| `d["inexistente"]` | `RuntimeError: Clave no encontrada en diccionario: inexistente` |
| `d2 = {"mi-clave": 7}` → `d2.mi-clave` | `RuntimeError: ... la clave 'mi' ...` — lee `mi`, no `mi-clave` |
| `d3 = {"2num": 5}` → `d3.2num` | `Error de Sintaxis: Se esperaba el nombre de una propiedad o método después del punto.` |

O sea: una clave con guion, espacio o dígito inicial **requiere corchetes**, siempre.

> ⚠️ **Limitación de v1.0.81: el punto sobre diccionarios es exclusivamente de solo lectura.**
> Al asignar o modificar un valor con punto (`persona.edad = 28`) el intérprete responde `RuntimeError: Solo las instancias tienen campos.`, y ocurre igual al crear una clave nueva (`d.nuevo = 1`). La asignación directa con punto está reservada hoy a las instancias de clases (POO) y se habilitará para diccionarios en una versión posterior.
>
> **Mientras tanto, escribe siempre con corchetes:** `persona["edad"] = 28`. Es la forma que funciona en todas las versiones y evita mezclar estilos dentro del mismo código.

### Valores Dinámicos (Evaluación al Vuelo)

Una de las características más potentes de Latino.NET es que los valores dentro de un diccionario literal no tienen que ser datos estáticos. El motor evalúa cualquier expresión válida (matemáticas simples, llamadas a funciones nativas o plugins, operadores ternarios e incluso tuberías `|>`) **en el momento exacto en que se crea el diccionario**.

Esto te permite construir y procesar estructuras de datos complejas en un solo paso limpio y elegante, sin necesidad de usar variables temporales:

```latino
incluir("formulas")

radio = 5
precio_base = 120

# El diccionario calcula sus propios valores al nacer
circulo_data = {
    "radio_original": radio,
    "area_matematica": mate.pi * radio^2,                       # Operación aritmética
    "area_plugin": geometria.areaCirculo(radio)["resultado"],   # Llamada a plugin
    "doble_radio": radio |> fun(x) ret x * 2 fin,               # Pipeline y función anónima
    "es_grande": radio > 10 ? "Sí" : "No",                      # Operador ternario
    "precio_final": precio_base * 1.15                          # Operación con variables externas
}

escribir("Área: " .. circulo_data["area_matematica"])  # Salida: Área: 78.5398...
escribir("¿Es grande?: " .. circulo_data["es_grande"]) # Salida: ¿Es grande?: No
```

💡 **Consejo Profesional**: Piensa en las llaves `{}` del diccionario no solo como un contenedor de almacenamiento estático, sino como un "ejecutor de expresiones" que procesa la lógica al instante y agrupa los resultados ordenadamente bajo los nombres que tú decidas.

> **⚠️ Limitación Importante:** Los pronombres de instancia (`mi`, `este`, `esta`) **solo funcionan dentro de clases**, no dentro de diccionarios literales. Si necesitas que un método de diccionario acceda a otros métodos o datos del mismo diccionario, usa una **variable local capturada por clausura**:

```latino
# ❌ FALLA: 'este' no existe en diccionarios
persona = {
    "nombre": "Ana",
    "saludar": fun()
        escribir("Hola, soy " .. este.nombre)  # 💥 RuntimeError
    fin
}

# ✅ CORRECTO: Asignación por clave en diccionario con captura por clausura
funcion crearPersona(nombre)
    # Sin 'local': el diccionario se retorna, no es estado privado,
    # y el nombre no colisiona con ningún ámbito superior.
    persona = {
        "nombre": nombre
    }
    
    # En diccionarios, la asignación debe ser mediante corchetes:
    persona["saludar"] = fun()
        escribir("Hola, soy " .. persona.nombre)  # ✅ Acceso por clausura
    fin
    
    retornar persona
fin

p = crearPersona("Ana")
p.saludar()  # Salida: Hola, soy Ana
```

Este patrón (factoría con clausura) es la forma idiomática de crear objetos con estado en Latino.NET cuando no usas clases.

### Funciones Principales de Diccionarios

| Función | Descripción |
| :--- | :--- |
| `dic.contiene(d, k)` | Verifica si la clave existe en el diccionario |
| `dic.llaves(d)` | Devuelve una lista con todas las claves |
| `dic.valores(d)` | Devuelve una lista con todos los valores |
| `dic.obtener(d, k, v)` | Obtiene un valor o devuelve el valor por defecto |
| `dic.longitud(d)` | Cantidad de pares clave-valor |
| `dic.agregar(d, k, v)` | Agrega o actualiza un par clave-valor |

```latino
usuario = {"nombre": "Ana", "edad": 25, "activo": verdadero}

# Verificar existencia de forma segura
si dic.contiene(usuario, "edad")
    escribir("Tiene edad definida")
fin

# Obtener con valor por defecto
ciudad = dic.obtener(usuario, "ciudad", "Desconocida")  # "Desconocida"
escribir(ciudad)
laEdad = dic.obtener(usuario, "edad", nulo)  # 25
escribir(laEdad)

# Iterar sobre claves y valores
claves = dic.llaves(usuario)    # ["nombre", "edad", "activo"]
valores = dic.valores(usuario)  # ["Ana", 25, cierto]

escribir(ç("Las claves: |claves|"))
escribir(ç("Los valores: |valores|"))
```

### Diccionarios Anidados (Árboles de Datos)

Los diccionarios pueden contener otros diccionarios y listas, formando estructuras jerárquicas complejas:

```latino
organizacion = {
    "departamento": "TI",
    "gerente": {
        "nombre": "Carlos",
        "reportes": ["Ana", "Luis"]
    },
    "empleados": [
        {"nombre": "Ana", "rol": "Desarrollador"},
        {"nombre": "Luis", "rol": "Diseñador"}
    ]
}

# Acceso profundo
escribir(organizacion["gerente"]["nombre"])     # Carlos
escribir(organizacion["empleados"][1]["rol"])   # Diseñador
```

### Diccionarios como Configuraciones

```latino
config = {
    "app": {
        "nombre": "MiApp",
        "version": "1.0.0",
        "debug": verdadero
    },
    "database": {
        "host": "localhost",
        "puerto": 3306,
        "usuario": "admin"
    }
}

# Acceso seguro con valor por defecto
puerto_db = dic.obtener(config["database"], "puerto", 5432)

imprimir("Puerto de la base de datos:", puerto_db)
```

## 3.3 Tuplas

Las tuplas son colecciones ordenadas de un tamaño fijo. Una vez creadas, no se pueden modificar (son inmutables). Son útiles para agrupar datos relacionados que no deben cambiar, como coordenadas o valores de retorno múltiples.

### Características de las Tuplas

| Característica | Descripción |
| :--- | :--- |
| **Sintaxis Literal** | Se crean usando paréntesis `()` |
| **Inmutables** | No se pueden modificar después de crearlas |
| **Tamaño Fijo** | El número de elementos no puede cambiar |
| **Acceso** | Por índice `[]` o mediante librería `tupla` |

### Creación y Acceso

```latino
# Tupla de múltiples elementos
punto3d = (10, 20, 30)

# Tupla de un solo elemento (requiere coma al final)
tuplaUnitaria = (99,)

# Tupla vacía
vacia = ()

# Acceso mediante índice (Igual que listas)
coordenadas = (1024, 768)
x = coordenadas[0]  # 1024
z = coordenadas[1]  # 768

# Acceso mediante librería
x_lib = tupla.obtener(coordenadas, 0)

escribir(ç("Coordenada X: |x|\nCoordenada Z: |z|"))
escribir(ç("Coordenada X (librería): |x_lib|"))
```

### Funciones de Tuplas

| Función | Descripción |
| :--- | :--- |
| `tupla.crear(...)` | Crea una tupla con los elementos especificados |
| `tupla.obtener(t, i)` | Obtiene el elemento en la posición `i` |
| `tupla.longitud(t)` | Devuelve el número de elementos |

### Uso Práctico: Retornar Múltiples Valores

Las tuplas son ideales cuando una función necesita devolver más de un valor:

```latino
# Función que retorna cociente y residuo
funcion dividir(dividendo, divisor)
    cociente = dividendo / divisor
    residuo = dividendo % divisor
    retornar (cociente, residuo)
fin

resultado = dividir(10, 3)
coc = resultado[0]  # 3.333...
res = resultado[1]  # 1

escribir(ç("Cociente: |coc|\nResiduo: |res|"))

# Función para crear puntos 2D
funcion crearPunto(x, y)  # AHORA 'y' es una variable válida
    retornar (x, y) 
fin
p = crearPunto(5, 10)

escribir("Punto: " .. p)
```

### Comparación de Tuplas

```latino
tupla1 = (1, 2)
tupla2 = (1, 2)
tupla3 = (2, 1)

# Igualdad estructural (compara contenido)
son_iguales = (tupla1 == tupla2)  # cierto
son_diferentes = (tupla1 != tupla3)  # cierto
imprimir(ç("¿Son iguales?\nRespuesta: |son_iguales|"))
imprimir(ç("¿Son diferentes?\nRespuesta: |son_diferentes|"))
```

## 3.4 Pilas y Colas

### 3.4.1 Pilas (Stack - LIFO)

Las pilas funcionan bajo el principio "Last In, First Out" (último en entrar, primero en salir), como una pila de platos: solo puedes tomar o agregar platos desde la parte superior.

#### Características de las Pilas

| Característica | Descripción |
| :--- | :--- |
| **Principio** | LIFO (Last In, First Out) |
| **Operaciones Principales** | `apilar` (push), `desapilar` (pop) |
| **Acceso** | Solo al elemento del tope |
| **Casos de Uso** | Historial de navegación, deshacer/rehacer, parsers |

#### Funciones de Pilas

| Función | Parámetros | Descripción |
| :--- | :--- | :--- |
| `pila.crear()` | - | Crea pila vacía |
| `pila.apilar(p, item)` | `pila, elemento` | Añade al tope (push) |
| `pila.desapilar(p)` | `pila` | Extrae del tope (pop) |
| `pila.cima(p)` | `pila` | Ver tope sin extraer (peek) |
| `pila.vacia(p)` | `pila` | Verifica si está vacía |
| `pila.longitud(p)` | `pila` | Cantidad de elementos |
| `pila.limpiar(p)` | `pila` | Elimina todos los elementos |
| `pila.alista(p)` | `pila` | Convierte a lista (fondo→cima) |

#### Caso Práctico: Historial de Navegación

```latino
# Sistema de navegación web (historial)
navegacion = pila.crear()

funcion Navegar(miPila, pagina)
    pila.apilar(miPila, pagina)
    escribir("→ Navegando a: " .. pagina)
fin

funcion Retroceder(miPila)
    si pila.vacia(miPila)
        escribir("⚠ No hay historial para retroceder")
        retornar nulo
    fin
    
    actual = pila.desapilar(miPila)
    escribir("← Saliendo de: " .. actual)
    
    si !pila.vacia(miPila)
        anterior = pila.cima(miPila)
        escribir("   Volviendo a: " .. anterior)
        retornar anterior
    fin
    
    retornar nulo
fin

funcion MostrarHistorial(miPila)
    si pila.vacia(miPila)
        escribir("Historial vacío")
        retornar
    fin
    
    historial = pila.alista(miPila)
    escribir("Historial completo (más antiguo → más reciente):")
    escribir(historial)
fin

# Uso
Navegar(navegacion, "home.html")
Navegar(navegacion, "productos.html")
Navegar(navegacion, "carrito.html")
MostrarHistorial(navegacion)  # ["home.html", "productos.html", "carrito.html"]
Retroceder(navegacion)        # Vuelve a productos.html
Retroceder(navegacion)        # Vuelve a home.html

# Limpiar todo el historial
pila.limpiar(navegacion)
escribir("Historial limpio: " .. pila.vacia(navegacion))  # cierto
```

### 3.4.2 Colas (Queue - FIFO)

Las colas funcionan bajo el principio "First In, First Out" (primero en entrar, primero en salir), como una fila de supermercado: quien llega primero, es atendido primero.

#### Características de las Colas

| Característica | Descripción |
| :--- | :--- |
| **Principio** | FIFO (First In, First Out) |
| **Operaciones Principales** | `encolar` (enqueue), `desencolar` (dequeue) |
| **Acceso** | Solo al elemento del inicio |
| **Casos de Uso** | Sistemas de turnos, procesamiento de tareas, impresión |

#### Funciones de Colas

| Función | Parámetros | Descripción |
| :--- | :--- | :--- |
| `cola.crear()` | - | Crea cola vacía |
| `cola.encolar(c, item)` | `cola, elemento` | Añade al final (enqueue) |
| `cola.desencolar(c)` | `cola` | Extrae del inicio (dequeue) |
| `cola.siguiente(c)` | `cola` | Ver primero sin extraer (peek) |
| `cola.vacia(c)` | `cola` | Verifica si está vacía |
| `cola.longitud(c)` | `cola` | Cantidad de elementos |
| `cola.limpiar(c)` | `cola` | Elimina todos los elementos |
| `cola.alista(c)` | `cola` | Convierte a lista (inicio→fin) |

#### Caso Práctico: Sistema de Procesamiento por Lotes

```latino
# Sistema de tareas con cola
tareas_pendientes = cola.crear()

funcion agregarTarea(nombre, prioridad)
    tarea = {
        "nombre": nombre,
        "prioridad": prioridad,
        "estado": "pendiente"
    }
    cola.encolar(tareas_pendientes, tarea)
    escribir("✅ Tarea '" .. nombre .. "' agregada a la cola")
fin

funcion procesarSiguiente()
    si cola.vacia(tareas_pendientes)
        escribir("⚠ No hay tareas pendientes")
        retornar nulo
    fin
    
    tarea = cola.desencolar(tareas_pendientes)
    tarea["estado"] = "procesando"
    escribir("⚙ Procesando: " .. tarea["nombre"])
    retornar tarea
fin

funcion verPendientes()
    si cola.vacia(tareas_pendientes)
        escribir("No hay tareas en espera")
        retornar
    fin
    
    proximo = cola.siguiente(tareas_pendientes)
    escribir("Próximo en la fila: " .. proximo["nombre"])
    escribir("Total pendientes: " .. cola.longitud(tareas_pendientes))
fin

funcion listarTodasLasTareas()
    lista_tareas = cola.alista(tareas_pendientes)
    escribir("Tareas en cola (orden de llegada):")
    
    para tarea en lista_tareas
        escribir("  - " .. tarea["nombre"] .. " (prioridad: " .. tarea["prioridad"] .. ")")
    fin
fin

# Uso
agregarTarea("Backup BD", 1)
agregarTarea("Generar reporte", 2)
agregarTarea("Enviar emails", 3)

listarTodasLasTareas()
# Salida:
# Tareas en cola (orden de llegada):
#   - Backup BD (prioridad: 1)
#   - Generar reporte (prioridad: 2)
#   - Enviar emails (prioridad: 3)

procesarSiguiente()  # Procesa "Backup BD" primero (FIFO)
verPendientes()      # Muestra "Generar reporte" como próximo

# Limpiar todas las tareas pendientes
cola.limpiar(tareas_pendientes)
escribir("Cola limpia: " .. cola.vacia(tareas_pendientes))  # cierto
```

#### Diferencia Clave: Pila vs Cola

```latino
# PILA (LIFO) - Último en entrar, primero en salir
p = pila.crear()
pila.apilar(p, "A")
pila.apilar(p, "B")
pila.apilar(p, "C")
escribir(pila.desapilar(p))  # → "C" (último agregado)

# COLA (FIFO) - Primero en entrar, primero en salir
c = cola.crear()
cola.encolar(c, "A")
cola.encolar(c, "B")
cola.encolar(c, "C")
escribir(cola.desencolar(c))  # → "A" (primero agregado)
```

> **💡 Tip:** Usa `pila.vacia()` y `cola.vacia()` antes de `desapilar()` o `desencolar()` para evitar errores al intentar extraer de estructuras vacías.

## 3.5 Mutabilidad y Referencias

Al trabajar con datos en Latino.NET, es vital entender cómo se almacenan y comparten en la memoria. Esto afecta directamente cómo debes manipular los datos y evitar errores comunes.

> **💡 Solución Nativa:** La función global `clonar(objeto)` realiza una copia profunda (Deep Copy) segura contra ciclos, superior a las funciones manuales de copiado que se muestran a continuación. Úsala siempre que necesites duplicar una estructura sin riesgo de efectos espejo.

### Tipos Mutable vs Inmutable

Existen dos comportamientos fundamentales:

1. **Mutables (Por Referencia):** Si asignas la variable a otra, ambas apuntan al **mismo objeto**. Si cambias una, cambias la otra.

2. **Inmutables (Por Valor):** No se pueden modificar internamente una vez creados. Si intentas cambiarlos, en realidad estás creando uno nuevo.

### Tabla de Referencia de Mutabilidad

| Tipo de Dato | Comportamiento | ¿Qué pasa si hago `b = a`? |
| :--- | :--- | :--- |
| **Lista** | Mutable (Referencia) | `a` y `b` son la **MISMA** lista |
| **Diccionario** | Mutable (Referencia) | `a` y `b` comparten las claves |
| **Conjunto/Pila/Cola** | Mutable (Referencia) | `a` y `b` comparten los datos |
| **Tupla** | Inmutable (Valor) | `b` es una copia idéntica, nadie puede modificarla |
| **Cadena** | Inmutable (Valor) | Modificar crea una nueva cadena |
| **Número/Lógico** | Inmutable (Valor) | Son valores puros |

### El "Efecto Espejo" (Referencias)

Este es un error común al trabajar con listas o diccionarios. El signo `=` no crea una copia de los datos, solo crea un "alias" o acceso directo a los mismos datos:

```latino
# Caso MUTABLE (Lista) - ¡CUIDADO!
lista_A = [10, 20]
lista_B = lista_A       # No es una copia, es la misma lista!

lista_B[0] = 999        # Modificamos la lista B
escribir(lista_A[0])    # Salida: 999
# La lista A cambió porque A y B son el mismo objeto
```

⚠️ **Advertencia:** Este comportamiento se aplica a todas las estructuras mutables: listas, diccionarios, conjuntos, pilas y colas.

### Crear Copias Independientes

Para evitar el efecto espejo, debes crear copias manuales de las estructuras:

```latino
# Para listas: crear nueva lista y copiar elementos
funcion copiarLista(original)
    local copia = [ ]
    para elemento en original
        lista.agregar(copia, elemento)
    fin
    retornar copia
fin

lista_A = [1, 2, 3]
lista_B = copiarLista(lista_A)
lista_B[0] = 99
escribir(lista_A[0])  # Sigue siendo 1

# Para diccionarios
funcion copiarDiccionario(original)
    local copia = { }
    claves = dic.llaves(original)
    para clave en claves
        copia[clave] = original[clave]
    fin
    retornar copia
fin
```

> **⚠️ ¿Por qué ese `local` no sobra?** Porque **una asignación dentro de una función crea una variable global si el nombre ya existe fuera**, y sobrescribe la de fuera. Verificado:
>
> ```latino
> copia = "mi valor global"
> funcion f(original)
>     copia = [ ]        # sin local: sobrescribe la global de arriba
>     retornar copia
> fin
> f([1, 2, 3])
> escribir(copia)       # → []  ❌ se perdió "mi valor global"
> ```
>
> El `[]` es el valor de `copia` **dentro** de la función: la asignación sobreescribe la global en cuanto se ejecuta, y el argumento `original` nunca llegó a usarse.
> Con `local`, la global queda intacta. Aquí `local` **no es un "efecto Lua"**: es lo que impide que la función corrompa el estado de quien la llama.

### Comportamiento Inmutable

Los tipos inmutables son seguros. No tienen "efecto espejo" porque no se pueden modificar por dentro, solo se pueden reemplazar por completo:

```latino
# Caso INMUTABLE (Tupla o Cadena)
tupla_A = (10, 20)
tupla_B = tupla_A       # Apuntan a lo mismo, pero...

# tupla_B[0] = 999      # ERROR: No se puede modificar una tupla
tupla_B = (999, 20)     # Esto crea una TUPLA NUEVA en B

escribir(tupla_A[0])    # Salida: 10
# La tupla A queda intacta
```

> **Consejo:** Si necesitas una copia independiente de una lista o diccionario para poder modificarla sin afectar a la original, debes usar mecanismos de clonado manual (como crear una lista nueva y agregarle los elementos de la anterior).

## 3.6 Conversiones entre Estructuras

### De Diccionario a Lista de Pares

```latino
usuario = {"nombre": "Ana", "edad": 25}

# Obtener claves y valores por separado
claves = dic.llaves(usuario)    # ["nombre", "edad"]
valores = dic.valores(usuario)  # ["Ana", 25]

# Crear lista de tuplas manualmente
pares = [ ]
para k en claves
    lista.agregar(pares, (k, usuario[k]))
fin

# pares = [("nombre", "Ana"), ("edad", 25)]
escribir(pares)
```

### De Lista a Diccionario

```latino
# Supongamos que tienes una lista de tuplas (clave, valor)
pares = [("nombre", "Ana"), ("edad", 25), ("activo", verdadero)]

funcion listaADiccionario(lista_pares)
    local resultado = { }
    para par en lista_pares
        clave = par[0]
        valor = par[1]
        resultado[clave] = valor
    fin
    retornar resultado
fin

diccionario = listaADiccionario(pares)
# {"nombre": "Ana", "edad": 25, "activo": verdadero}
imprimir(diccionario)
```

## 3.6.5 Complejidad Temporal: ¿Qué Tan Rápido Es?

Cuando trabajas con estructuras de datos, es importante entender qué tan rápido se ejecutan las operaciones, especialmente cuando manejas muchos datos. La **complejidad temporal** nos dice cuánto tiempo toma una operación en función del tamaño de los datos.

### Notación Big O (Simplificada)

| Notación | Nombre | Descripción | Ejemplo |
| :--- | :--- | :--- | :--- |
| **O(1)** | Constante | Siempre toma el mismo tiempo, sin importar cuántos datos haya | Acceder a `dic["clave"]` o `lista[5]` |
| **O(n)** | Lineal | El tiempo crece proporcionalmente a la cantidad de datos | Buscar un elemento en una lista con `lista.contiene()` |
| **O(log n)** | Logarítmico | Muy eficiente, crece muy lento | Búsqueda binaria (no disponible en Latino base) |
| **O(n²)** | Cuadrático | Muy lento para datos grandes, evítalo si puedes | Bucle dentro de otro bucle |

> **💡 En términos simples:** O(1) es como buscar un número en tu agenda telefónica usando la tecla de búsqueda directa. O(n) es como leer toda la agenda línea por línea hasta encontrarlo.

### Tabla de Rendimiento por Estructura

| Estructura | Acceso | Búsqueda | Inserción (final) | Inserción (inicio) | Eliminación |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Lista** | O(1) por índice | O(n) | O(1) | O(n) | O(n) |
| **Diccionario** | O(1) por clave | O(1) por clave | O(1) | N/A | O(1) |
| **Conjunto** | N/A | O(1) | O(1) | N/A | O(1) |
| **Pila** | O(1) solo tope | N/A | O(1) | N/A | O(1) del tope |
| **Cola** | O(1) solo inicio | N/A | O(1) | N/A | O(1) del inicio |
| **Tupla** | O(1) por índice | O(n) | Inmutable | Inmutable | Inmutable |

🎯 **Ejemplos Prácticos de Rendimiento**

```latino
# LENTO: Buscar en lista (O(n) - recorre toda la lista)
lista_grande = rango(100000)  # 100,000 elementos
si lista.contiene(lista_grande, 99999)  # Revisa elemento por elemento
    escribir("Encontrado")  # Esto es LENTO para listas grandes
fin

# RÁPIDO: Buscar en diccionario (O(1) - acceso directo)
dic_grande = { }
para i en rango(100000)
    dic_grande[i] = verdadero
fin

si dic.contiene(dic_grande, 99999)  # Acceso instantáneo
    escribir("Encontrado")  # Esto es RÁPIDO sin importar el tamaño
fin

# LENTO: Insertar al inicio de lista (O(n) - mueve todos los elementos)
miLista = [1, 2, 3, 4, 5]
lista.insertar(miLista, 999, 0)  # Inserta 999 en índice 0

# RÁPIDO: Insertar al final de lista (O(1) - solo añade)
lista.agregar(miLista, 999)  # Solo agrega al final
```

> **⚠️ Regla de Oro:** Si necesitas buscar datos **frecuentemente** por un identificador (nombre, ID, etc.), usa un **diccionario**, no una lista. La diferencia en velocidad es enorme con datos grandes.

## 3.6.6 Tabla de Decisión: ¿Qué Estructura Usar?

Esta tabla te ayudará a elegir la estructura correcta según tu caso de uso:

| ¿Qué Necesitas? | Usa Esto | Razón |
| :--- | :--- | :--- |
| Guardar datos en orden de llegada | **Lista** | Mantiene el orden de inserción |
| Buscar datos por nombre/ID rápidamente | **Diccionario** | Acceso O(1) por clave |
| Eliminar duplicados automáticamente | **Conjunto** | No permite duplicados |
| Histórico reversible (deshacer/rehacer) | **Pila** | LIFO - último cambio primero |
| Procesar tareas en orden de llegada | **Cola** | FIFO - primera tarea primero |
| Datos que nunca cambian | **Tupla** | Inmutable, segura, eficiente |
| Verificar si algo existe (sí/no) | **Conjunto** o **Diccionario** | Ambos son O(1) |
| Agrupar datos relacionados | **Diccionario** | Estructura clave-valor |
| Datos numéricos ordenados | **Lista** | Usa `lista.ordenar()` |

🎯 **Ejemplos de Decisión por Caso de Uso**

```latino
# Caso 1: Sistema de inventario (búsquedas frecuentes)
# ❌ MAL: Lista (búsqueda lenta O(n))
inventario_lista = [
    {"sku": "A001", "nombre": "Laptop", "stock": 10},
    {"sku": "A002", "nombre": "Mouse", "stock": 50}
]
# Para buscar "A001", debe revisar toda la lista

# ✅ BIEN: Diccionario (búsqueda rápida O(1))
inventario_dic = {
    "A001": {"nombre": "Laptop", "stock": 10},
    "A002": {"nombre": "Mouse", "stock": 50}
}
producto = inventario_dic["A001"]  # Acceso instantáneo

# Caso 2: Registro de eventos (orden cronológico)
# ✅ BIEN: Lista (mantiene orden)
eventos = [ ]
lista.agregar(eventos, {"tipo": "login", "hora": "10:00"})
lista.agregar(eventos, {"tipo": "compra", "hora": "10:15"})

# Caso 3: Lista de usuarios únicos (sin duplicados)
# ✅ BIEN: Conjunto
usuarios_activos = conjunto.crear()
conjunto.agregar(usuarios_activos, "ana@mail.com")
conjunto.agregar(usuarios_activos, "ana@mail.com")  # Se ignora (ya existe)
conjunto.agregar(usuarios_activos, "luis@mail.com")
# usuarios_activos tiene solo 2 elementos

# Caso 4: Coordenadas geográficas (datos fijos)
# ✅ BIEN: Tupla (inmutable)
ubicacion_oficina = (40.7128, -74.0060)  # Latitud, Longitud
# ubicacion_oficina[0] = 0  # ERROR: No se puede modificar
```

## 3.6.7 Patrones Comunes de Iteración

Dominar estos patrones te hará más productivo al trabajar con estructuras de datos.

### Patrón 1: Iterar con Índice

```latino
# Cuando necesitas el índice además del valor
frutas = ["manzana", "pera", "uva"]

# ⚠️ El compositor ç() NO resuelve índices dinámicos: |frutas[i]| se
# imprimiría literalmente. Extrae el elemento a una variable primero.
para i en rango(lista.longitud(frutas))
    numFruta = i + 1
    frutaActual = frutas[i]
    escribir(ç("Fruta #|numFruta|: |frutaActual|"))
fin

# Salida:
# Fruta #1: manzana
# Fruta #2: pera
# Fruta #3: uva
```

### Patrón 2: Iterar con Transformación

```latino
# Convertir todos los elementos de una lista
precios = [10, 20, 30]

precios_con_iva = lista.transformar(precios, funcion(precio)
    retornar precio * 1.16  # 16% de IVA
fin)

escribir(precios_con_iva)  # [11.6, 23.2, 34.8]
```

### Patrón 3: Acumulación (Suma, Concatenación, etc.)

```latino
# Sumar todos los elementos de una lista
numeros = [1, 2, 3, 4, 5]

suma = 0
para num en numeros
    suma = suma + num
fin

escribir("Total: " .. suma)  # Total: 15

# O usando reducir (programación funcional)
total = lista.reducir(numeros, funcion(acumulador, actual)
    retornar acumulador + actual
fin, 0)

escribir("Total (reducir): " .. total)  # Total (reducir): 15
```

### Patrón 4: Buscar y Filtrar

```latino
# Encontrar el primer elemento que cumple una condición
usuarios = [
    {"nombre": "Ana", "edad": 25},
    {"nombre": "Luis", "edad": 17},
    {"nombre": "Maria", "edad": 30}
]

# Buscar primer adulto (≥18)
funcion encontrarAdulto(usuarios)
    para usuario en usuarios
        si usuario["edad"] >= 18
            retornar usuario
        fin
    fin
    retornar nulo
fin

adulto = encontrarAdulto(usuarios)
escribir(adulto["nombre"])  # Ana

# Filtrar todos los adultos
adultos = lista.filtrar(usuarios, funcion(u)
    retornar u["edad"] >= 18
fin)

escribir(lista.longitud(adultos))  # 2 (Ana y Maria)
```

### Patrón 5: Agrupar por Criterio

```latino
# Agrupar productos por categoría
productos = [
    {"nombre": "Laptop", "categoria": "Electrónica"},
    {"nombre": "Mesa", "categoria": "Muebles"},
    {"nombre": "Mouse", "categoria": "Electrónica"}
]

funcion agruparPorCategoria(productos)
    local grupos = { }
    
    para producto en productos
        categoria = producto["categoria"]
        
        # Si la categoría no existe, crear lista vacía
        si !dic.contiene(grupos, categoria)
            grupos[categoria] = [ ]
        fin
        
        # Agregar producto a su categoría
        lista.agregar(grupos[categoria], producto["nombre"])
    fin
    
    retornar grupos
fin

agrupados = agruparPorCategoria(productos)
escribir(agrupados)
# {
#   "Electrónica": ["Laptop", "Mouse"],
#   "Muebles": ["Mesa"]
# }
```

### Patrón 6: Construcción Incremental

```latino
# Construir estructura compleja paso a paso
funcion construirReporte(datos)
    reporte = {
        "total": 0,
        "items": [ ],
        "estadisticas": { }
    }
    
    # Procesar datos
    para item en datos
        reporte["total"] = reporte["total"] + item["precio"]
        lista.agregar(reporte["items"], item["nombre"])
    fin
    
    # Calcular estadísticas
    reporte["estadisticas"]["cantidad"] = lista.longitud(datos)
    reporte["estadisticas"]["promedio"] = reporte["total"] / lista.longitud(datos)
    
    retornar reporte
fin

datos = [
    {"nombre": "Producto A", "precio": 100},
    {"nombre": "Producto B", "precio": 200}
]

reporte = construirReporte(datos)
escribir(reporte)
# {
#   "total": 300,
#   "items": ["Producto A", "Producto B"],
#   "estadisticas": {"cantidad": 2, "promedio": 150}
# }
```

### Patrón 7: Iterar Diccionarios

```latino
# Iterar sobre claves y valores de un diccionario
configuracion = {
    "host": "localhost",
    "puerto": 8080,
    "debug": verdadero
}

# Método 1: Iterar solo claves
claves = dic.llaves(configuracion)
para clave en claves
    valor = configuracion[clave]
    escribir(clave .. " = " .. valor)
fin

# Método 2: Iterar claves y valores juntos
funcion iterarDiccionario(diccionario)
    claves = dic.llaves(diccionario)
    para clave en claves
        valor = diccionario[clave]
        escribir(ç("  |clave|: |valor|"))
    fin
fin

escribir("Configuración:")
iterarDiccionario(configuracion)
# Salida:
# Configuración:
#   host: localhost
#   puerto: 8080
#   debug: verdadero
```

> **💡 Consejo Profesional:** Aprende estos patrones de memoria. Son como "bloques de Lego" que puedes combinar para resolver problemas más complejos. El 90% del código que escribas usará alguna variación de estos patrones.

## 3.6.8 Errores Comunes y Cómo Evitarlos

### Error 1: Modificar Lista Durante Iteración

```latino
# ❌ MAL: Eliminar elementos mientras iteras
numeros = [1, 2, 3, 4, 5]

para num en numeros
    si num % 2 == 0
        lista.eliminar(numeros, num)  # ¡PELIGRO!
    fin
fin
# Puede saltar elementos o causar errores

# ✅ BIEN: Crear nueva lista con los elementos deseados
numeros = [1, 2, 3, 4, 5]

impares = lista.filtrar(numeros, funcion(n)
    retornar n % 2 != 0
fin)

escribir(impares)  # [1, 3, 5]
```

### Error 2: No Validar Índices

```latino
# ❌ MAL: Acceso sin validar
funcion obtenerPrimero(miLista)
    retornar miLista[0]  # Falla si la lista está vacía
fin

# ✅ BIEN: Validar antes de acceder
funcion obtenerPrimeroSeguro(miLista)
    si lista.longitud(miLista) == 0
        retornar nulo
    fin
    retornar miLista[0]
fin

vacia = [ ]
escribir(obtenerPrimeroSeguro(vacia))  # nulo (sin error)
```

### Error 3: Asumir Que una Clave Existe

```latino
# ❌ MAL: Acceso directo sin validar
usuario = {"nombre": "Ana"}
escribir(usuario["email"])  # Error si "email" no existe

# ✅ BIEN: Usar dic.obtener() con valor por defecto
email = dic.obtener(usuario, "email", "sin-email@example.com")
escribir(email)  # "sin-email@example.com"

# O validar primero
si dic.contiene(usuario, "email")
    escribir(usuario["email"])
sino
    escribir("Email no disponible")
fin
```

### Error 4: Confundir Copia con Referencia

```latino
# ❌ MAL: Creer que = copia la lista
original = [1, 2, 3]
copia = original     # ¡NO es una copia!
copia[0] = 999

escribir(original)   # [999, 2, 3] - ¡Original modificada!

# ✅ BIEN: Crear copia real
original = [1, 2, 3]
copia = [ ]
para elemento en original
    lista.agregar(copia, elemento)
fin

copia[0] = 999
escribir(original)   # [1, 2, 3] - Original intacta
```

### Error 5: Confundir la Comparación de Estructuras

En Latino.NET, el operador `==` evalúa **igualdad estructural profunda** (*Value Semantics*). Dos listas, diccionarios o tuplas son iguales si tienen la misma longitud y sus elementos internos coinciden valor por valor:

```latino
lista1 = [1, 2, 3]
lista2 = [1, 2, 3]

si lista1 == lista2
    escribir("Son iguales")  # ✅ Salida: Son iguales (compara contenido)
fin
```

> **💡 Diferencia con Latino:** En el intérprete original, `==` en listas comparaba referencias de memoria. Latino.NET implementa igualdad estructural profunda por defecto.

**Comparación personalizada:** Cuando necesites lógica distinta (por ejemplo, comparar sin distinguir mayúsculas/minúsculas o ignorar orden), define tu propio comparador:

```latino
# ✅ Comparar listas elemento por elemento con lógica personalizada
funcion listasIguales(a, b)
    si lista.longitud(a) != lista.longitud(b)
        retornar falso
    fin
    para i en rango(lista.longitud(a))
        si a[i] != b[i]
            retornar falso
        fin
    fin
    retornar verdadero
fin
```

> **⚠️ Advertencia:** Los 5 errores anteriores son las causas más comunes de bugs en programas Latino. Si los evitas, ahorrarás horas de debugging.

## 3.7 Mejores Prácticas

Seguir estas recomendaciones te ayudará a escribir código más limpio, eficiente y mantenible.

### 3.7.1 Elige la Estructura Correcta

La elección de la estructura de datos correcta puede hacer la diferencia entre código rápido y código lento, entre código limpio y código complejo.

**Guía Rápida de Selección:**

| Si Necesitas... | Primera Opción | Segunda Opción | Evita |
| :--- | :--- | :--- | :--- |
| Orden de inserción | Lista | Cola | Conjunto |
| Búsqueda rápida por ID | Diccionario | - | Lista |
| Sin duplicados | Conjunto | - | Lista manual |
| Deshacer/Rehacer | Pila | - | Lista |
| Procesar en orden FIFO | Cola | Lista | Pila |
| Datos inmutables | Tupla | - | Lista |
| Agrupar por categoría | Diccionario de listas | - | Lista única |
| Contadores/Frecuencias | Diccionario | - | Lista |

**Ejemplos de Selección Correcta:**

```latino
# ✅ CORRECTO: Contador de palabras (Diccionario)
funcion contarPalabras(texto)
    palabras = cadena.separar(texto, " ")
    contadores = { }
    
    para palabra en palabras
        cuenta_actual = dic.obtener(contadores, palabra, 0)
        contadores[palabra] = cuenta_actual + 1
    fin
    
    retornar contadores
fin

resultado = contarPalabras("hola mundo hola")
escribir(resultado)  # {"hola": 2, "mundo": 1}

# ❌ INCORRECTO: Intentar lo mismo con listas (lento y complejo)
# Requiere buscar en toda la lista cada vez = O(n²)

# ✅ CORRECTO: Historial de comandos (Pila)
historial = pila.crear()

funcion ejecutarComando(comando)
    pila.apilar(historial, comando)
    escribir("Ejecutando: " .. comando)
    # ... lógica del comando
fin

funcion deshacerUltimo()
    si pila.vacia(historial)
        escribir("No hay nada que deshacer")
        retornar
    fin
    
    ultimo = pila.desapilar(historial)
    escribir("Deshaciendo: " .. ultimo)
fin

ejecutarComando("crear archivo")
ejecutarComando("editar texto")
deshacerUltimo()  # Deshace "editar texto"

# ✅ CORRECTO: Cola de impresión (Cola)
cola_impresion = cola.crear()

funcion agregarDocumento(nombre)
    cola.encolar(cola_impresion, nombre)
    escribir("Documento '" .. nombre .. "' en cola de impresión")
fin

funcion imprimirSiguiente()
    si cola.vacia(cola_impresion)
        escribir("No hay documentos pendientes")
        retornar
    fin
    
    doc = cola.desencolar(cola_impresion)
    escribir("Imprimiendo: " .. doc)
fin
```

### 3.7.2 Consideraciones de Rendimiento

#### Reglas de Oro

1. **Evita búsquedas en listas grandes** → Usa diccionarios
2. **No insertes al inicio de listas** → Usa colas o inserta al final
3. **No copies estructuras en cada iteración** → Modifica en su lugar o usa referencias
4. **Valida una vez, no en cada iteración** → Mueve validaciones fuera de bucles

#### Tabla de Costos de Operaciones

| Operación | Lista | Diccionario | Conjunto | Pila | Cola |
| :--- | :--- | :--- | :--- | :--- | :--- |
| Acceso | O(1) índice | O(1) clave | - | O(1) tope | O(1) inicio |
| Búsqueda | O(n) | O(1) | O(1) | - | - |
| Inserción final | O(1) | O(1) | O(1) | O(1) | O(1) |
| Inserción inicio | O(n) ⚠️ | N/A | N/A | N/A | N/A |
| Eliminación | O(n) | O(1) | O(1) | O(1) | O(1) |

🎯 **Ejemplos de Optimización**

```latino
# ❌ LENTO: Buscar en lista repetidamente O(n²)
usuarios = [
    {"id": 1, "nombre": "Ana"},
    {"id": 2, "nombre": "Luis"},
    {"id": 3, "nombre": "Maria"}
]

pedidos = [
    {"usuario_id": 2, "total": 100},
    {"usuario_id": 1, "total": 200},
    {"usuario_id": 3, "total": 150}
]

# Esto busca en toda la lista de usuarios por cada pedido
para pedido en pedidos
    para usuario en usuarios
        si usuario["id"] == pedido["usuario_id"]
            escribir(usuario["nombre"] .. " gastó $" .. pedido["total"])
        fin
    fin
fin

# ✅ RÁPIDO: Indexar primero con diccionario O(n)
usuarios_por_id = { }
para usuario en usuarios
    usuarios_por_id[usuario["id"]] = usuario
fin

# Ahora búsqueda es O(1) por cada pedido
para pedido en pedidos
    usuario = usuarios_por_id[pedido["usuario_id"]]
    escribir(usuario["nombre"] .. " gastó $" .. pedido["total"])
fin
```

```latino
# ❌ LENTO: Copiar lista en cada iteración
funcion filtrarPorCategoria(productos, categoria)
    resultado = [ ]
    
    para producto en productos
        # Crear diccionario temporal en cada iteración (lento)
        info = {
            "nombre": producto["nombre"],
            "precio": producto["precio"]
        }
        
        si producto["categoria"] == categoria
            lista.agregar(resultado, info)
        fin
    fin
    
    retornar resultado
fin

# ✅ RÁPIDO: Solo copiar lo necesario
funcion filtrarPorCategoriaRapido(productos, categoria)
    # Primero filtrar, luego transformar
    filtrados = lista.filtrar(productos, funcion(p)
        retornar p["categoria"] == categoria
    fin)
    
    # Solo crear diccionarios para elementos filtrados
    retornar lista.transformar(filtrados, funcion(p)
        retornar {"nombre": p["nombre"], "precio": p["precio"]}
    fin)
fin
```

### 3.7.3 Patrones Recomendados

#### Patrón 1: Validación Defensiva

Siempre valida entradas antes de procesarlas:

```latino
funcion procesarPedido(pedido)
    # Validar tipo
    si tipo(pedido) != "diccionario"
        escribir("Error: Se esperaba un diccionario")
        retornar nulo
    fin
    
    # Validar campos requeridos
    si !dic.contiene(pedido, "items") || !dic.contiene(pedido, "cliente")
        escribir("Error: Faltan campos requeridos")
        retornar nulo
    fin
    
    # Validar tipos internos
    items = pedido["items"]
    si tipo(items) != "lista"
        escribir("Error: 'items' debe ser una lista")
        retornar nulo
    fin
    
    # Procesar de forma segura
    total = 0
    para item en items
        precio = dic.obtener(item, "precio", 0)
        total = total + anumero(precio)
    fin
    
    retornar total
fin

# Uso seguro
pedido_valido = {
    "cliente": "Ana",
    "items": [
        {"nombre": "Laptop", "precio": 1000},
        {"nombre": "Mouse", "precio": 20}
    ]
}

pedido_invalido = {"cliente": "Luis"}  # Falta "items"

escribir(procesarPedido(pedido_valido))    # 1020
escribir(procesarPedido(pedido_invalido))  # Error: Faltan campos requeridos
```

#### Patrón 2: Valores por Defecto con `dic.obtener()`

```latino
# Configuración con valores por defecto
funcion conectarBaseDatos(config)
    host = dic.obtener(config, "host", "localhost")
    puerto = dic.obtener(config, "puerto", 5432)
    usuario = dic.obtener(config, "usuario", "admin")
    timeout = dic.obtener(config, "timeout", 30)
    
    escribir(ç("Conectando a |host|:|puerto| como |usuario| (timeout: |timeout|s)"))
    # ... lógica de conexión
fin

# Uso con configuración parcial
config_minima = {"host": "192.168.1.100"}
conectarBaseDatos(config_minima)
# → Conectando a 192.168.1.100:5432 como admin (timeout: 30s)

config_completa = {
    "host": "db.example.com",
    "puerto": 3306,
    "usuario": "root",
    "timeout": 60
}
conectarBaseDatos(config_completa)
# → Conectando a db.example.com:3306 como root (timeout: 60s)
```

#### Patrón 3: Construcción Incremental (Builder)

```latino
# Constructor de reportes paso a paso
funcion ReporteBuilder()
    local reporte = {
        "titulo": "Sin título",
        "datos": [ ],
        "metadatos": { },
        "formato": "texto"
    }
    
    # Retornar objeto con métodos
    builder = {
        "titulo": funcion(t)
            reporte["titulo"] = t
            retornar builder  # Para encadenar
        fin,
        
        "agregarDato": funcion(d)
            lista.agregar(reporte["datos"], d)
            retornar builder
        fin,
        
        "formato": funcion(f)
            reporte["formato"] = f
            retornar builder
        fin,
        
        "construir": funcion()
            # Agregar metadatos automáticos
            reporte["metadatos"]["cantidad"] = lista.longitud(reporte["datos"])
            reporte["metadatos"]["fecha"] = sis.tiempo("%Y-%m-%d")
            retornar reporte
        fin
    }
    
    retornar builder
fin

# Uso encadenado
reporte = ReporteBuilder()
reporte.titulo("Ventas Mensuales")
reporte.agregarDato({"mes": "Enero", "total": 1000})
reporte.agregarDato({"mes": "Febrero", "total": 1500})
reporte.formato("JSON")
reporte = reporte.construir()

escribir(reporte)
# {
#   "titulo": "Ventas Mensuales",
#   "datos": [...],
#   "metadatos": {"cantidad": 2, "fecha": "<fecha-dinamica>"},
#   "formato": "JSON"
# }
# Nota: la fecha se genera con sis.tiempo("%Y-%m-%d"), por lo que su valor
# corresponde al día en que ejecutes el ejemplo (salida variable).
```

#### Patrón 4: Cache/Memoización

```latino
# Evitar cálculos repetidos
cache_fibonacci = { }

funcion fibonacci(n)
    # Verificar si ya está calculado
    si dic.contiene(cache_fibonacci, n)
        retornar cache_fibonacci[n]
    fin
    
    # Calcular
    # La variable debe existir ANTES del condicional: una variable creada
    # dentro de un si/sino muere al cerrarse el bloque.
    resultado = nulo
    si n <= 1
        resultado = n
    sino
        resultado = fibonacci(n - 1) + fibonacci(n - 2)
    fin
    
    # Guardar en cache
    cache_fibonacci[n] = resultado
    retornar resultado
fin

# Primera llamada: calcula todo
escribir(fibonacci(10))  # Tarda un poco

# Segunda llamada: instantáneo (usa cache)
escribir(fibonacci(10))  # Inmediato
```

### 3.7.4 Caso Práctico: Gestión de Tareas

El siguiente ejemplo completo integra las mejores prácticas aprendidas:

Aplicando todas las mejores prácticas:

```latino
/*
Sistema completo de gestión de tareas que demuestra:
- Validación defensiva
- Uso correcto de estructuras
- Manejo de errores
- Patrones funcionales
*/

# Estado global del sistema
tareas_por_id = { }
contador_id = 0
categorias = conjunto.crear()

# Crear nueva tarea
funcion crearTarea(titulo, categoria)
    # Validación
    si tipo(titulo) != "cadena" || cadena.longitud(titulo) == 0
        escribir("Error: Título inválido")
        retornar nulo
    fin
    
    # Generar ID único
    contador_id = contador_id + 1
    id = "TASK-" .. contador_id
    
    # Crear tarea
    tarea = {
        "id": id,
        "titulo": titulo,
        "categoria": categoria,
        "estado": "pendiente",
        "fecha_creacion": sis.tiempo("%Y-%m-%d %H:%M")  # Salida variable (depende del momento de ejecución y hardware):
    }
    
    # Registrar
    tareas_por_id[id] = tarea
    conjunto.agregar(categorias, categoria)
    
    escribir(ç("✅ Tarea creada: |id|"))
    retornar id
fin

# Obtener tareas por estado
funcion obtenerPorEstado(estado)
    tareas = dic.valores(tareas_por_id)
    
    retornar lista.filtrar(tareas, funcion(t)
        retornar t["estado"] == estado
    fin)
fin

# Obtener tareas por categoría
funcion obtenerPorCategoria(categoria)
    tareas = dic.valores(tareas_por_id)
    
    retornar lista.filtrar(tareas, funcion(t)
        retornar t["categoria"] == categoria
    fin)
fin

# Cambiar estado de tarea
funcion cambiarEstado(id, nuevo_estado)
    # Validar existencia
    si !dic.contiene(tareas_por_id, id)
        escribir(ç("Error: Tarea |id| no existe"))
        retornar falso
    fin
    
    # Validar estado
    estados_validos = ["pendiente", "en_progreso", "completada"]
    si !lista.contiene(estados_validos, nuevo_estado)
        escribir(ç("Error: Estado '|nuevo_estado|' no válido"))
        retornar falso
    fin
    
    # Actualizar
    tareas_por_id[id]["estado"] = nuevo_estado
    escribir(ç("✅ Tarea |id| → |nuevo_estado|"))
    retornar verdadero
fin

# Estadísticas
funcion generarEstadisticas()
    tareas = dic.valores(tareas_por_id)
    
    # Contar por estado
    contadores = { }
    para tarea en tareas
        estado = tarea["estado"]
        cuenta_actual = dic.obtener(contadores, estado, 0)
        contadores[estado] = cuenta_actual + 1
    fin
    
    # Mostrar reporte
    escribir("\n=== ESTADÍSTICAS ===")
    total_tareas = lista.longitud(tareas)
    escribir(ç("Total de tareas: |total_tareas|"))
    total_categorias = conjunto.longitud(categorias)
    escribir(ç("Categorías: |total_categorias|"))
    
    escribir("\nPor estado:")
    estados = dic.llaves(contadores)
    para estado en estados
        cuenta = dic.obtener(contadores, estado, 0)
        escribir(ç("  |estado|: |cuenta|"))
    fin
fin

# === USO DEL SISTEMA ===

# Crear tareas
crearTarea("Implementar login", "Backend")
crearTarea("Diseñar dashboard", "Frontend")
crearTarea("Escribir tests", "Testing")
crearTarea("Revisar código", "Backend")

# Cambiar estados
cambiarEstado("TASK-1", "completada")
cambiarEstado("TASK-2", "en_progreso")

# Consultas
escribir("\nTareas pendientes:")
pendientes = obtenerPorEstado("pendiente")
para tarea en pendientes
    escribir(ç("  - |tarea['titulo']| (|tarea['categoria']|)"))
fin

escribir("\nTareas de Backend:")
backend = obtenerPorCategoria("Backend")
para tarea en backend
    escribir(ç("  - |tarea['titulo']| [|tarea['estado']|]"))
fin

# Estadísticas finales
generarEstadisticas()

# Salida esperada:
# ✅ Tarea creada: TASK-1
# ✅ Tarea creada: TASK-2
# ✅ Tarea creada: TASK-3
# ✅ Tarea creada: TASK-4
# ✅ Tarea TASK-1 → completada
# ✅ Tarea TASK-2 → en_progreso
#
# Tareas pendientes:
#   - Escribir tests (Testing)
#   - Revisar código (Backend)
#
# Tareas de Backend:
#   - Implementar login [completada]
#   - Revisar código [pendiente]
#
# === ESTADÍSTICAS ===
# Total de tareas: 4
# Categorías: 3
#
# Por estado:
#   completada: 1
#   en_progreso: 1
#   pendiente: 2
```

## 3.8 Debugging de Estructuras de Datos

Saber cómo inspeccionar y depurar estructuras de datos te ahorrará horas de frustración.

### 3.8.1 Inspección Básica

```latino
# Función universal de inspección
funcion inspeccionar(estructura, nombre)
    escribir(ç("\n=== INSPECCIÓN: |nombre| ==="))
    # ⚠️ ç() no ejecuta llamadas a funciones: |tipo(estructura)| imprimiría
    # "<funcion nativa>". Resuelve el valor antes de interpolarlo.
    tipoDato = tipo(estructura)
    escribir(ç("Tipo: |tipoDato|"))
    
    si tipo(estructura) == "lista"
        escribir("Longitud: " .. lista.longitud(estructura))
        escribir(ç("Contenido: |estructura|"))
        
        si lista.longitud(estructura) > 0
            escribir(ç("Primer elemento: |estructura[0]|"))
            ultimoElem = estructura[lista.longitud(estructura) - 1]
            escribir(ç("Último elemento: |ultimoElem|"))
        fin
        
    osi tipo(estructura) == "diccionario"
        claves = dic.llaves(estructura)
        escribir("Cantidad de claves: " .. lista.longitud(claves))
        escribir(ç("Claves: |claves|"))
        escribir(ç("Contenido: |estructura|"))
        
    osi tipo(estructura) == "conjunto"
        cant = conjunto.longitud(estructura)
        escribir("Tamaño: " .. cant)
        escribir("Elementos: " .. estructura)
        
    sino
        escribir(ç("Valor: |estructura|"))
    fin
    
    escribir("=" .. cadena.rellenar_derecha("", "=", 40))
fin

# Uso
datos = {
    "usuarios": ["Ana", "Luis", "Maria"],
    "activos": 3,
    "config": {"debug": verdadero}
}

inspeccionar(datos, "Sistema")
inspeccionar(datos["usuarios"], "Lista de Usuarios")
inspeccionar(datos["config"], "Configuración")
```

### 3.8.2 Validación de Estructura

```latino
# Validar que un diccionario tenga la estructura esperada
funcion validarEstructura(obj, esquema)
    errores = [ ]
    
    # Verificar que es un diccionario
    si tipo(obj) != "diccionario"
        lista.agregar(errores, "Se esperaba un diccionario")
        retornar errores
    fin
    
    # Verificar cada campo del esquema
    campos = dic.llaves(esquema)
    para campo en campos
        tipo_esperado = esquema[campo]
        
        # Verificar existencia
        si !dic.contiene(obj, campo)
            lista.agregar(errores, ç("Falta campo: '|campo|'"))
            continuar
        fin
        
        # Verificar tipo
        valor = obj[campo]
        tipo_actual = tipo(valor)
        
        si tipo_actual != tipo_esperado
            lista.agregar(errores, 
                ç("Campo '|campo|': se esperaba |tipo_esperado|, se recibió |tipo_actual|"))
        fin
    fin
    
    retornar errores
fin

# Definir esquema
esquema_usuario = {
    "nombre": "cadena",
    "edad": "decimal",
    "activo": "logico"
}

# Validar objetos
usuario_valido = {"nombre": "Ana", "edad": 25, "activo": verdadero}
usuario_invalido = {"nombre": "Luis", "edad": "30"}  # edad es cadena, falta activo

errores1 = validarEstructura(usuario_valido, esquema_usuario)
escribir("Errores usuario válido: " .. lista.longitud(errores1))

errores2 = validarEstructura(usuario_invalido, esquema_usuario)
escribir("Errores usuario inválido:")
para error en errores2
    escribir(ç("  - |error|"))
fin
# - Campo 'edad': se esperaba decimal, se recibió cadena
# - Falta campo: 'activo'
```

### 3.8.3 Tracing de Modificaciones

```latino
# Wrapper para rastrear cambios en diccionarios
funcion crearDiccionarioRastreado(nombre)
    local datos = { }
    local historial = [ ]

    retornar {
        "establecer": funcion(clave, valor)
            valor_anterior = dic.obtener(datos, clave, nulo)
            datos[clave] = valor
            
            registro = {
                "operacion": "establecer",
                "clave": clave,
                "valor_anterior": valor_anterior,
                "valor_nuevo": valor,
                "timestamp": sis.tiempo("%H:%M:%S")
            }
            
            lista.agregar(historial, registro)
            escribir(ç("[|nombre|] |clave| = |valor|"))
        fin,
        
        "obtener": funcion(clave)
            retornar dic.obtener(datos, clave, nulo)
        fin,
        
        "eliminar": funcion(clave)
            si dic.contiene(datos, clave)
                valor = datos[clave]
                dic.eliminar(datos, clave)
                
                registro = {
                    "operacion": "eliminar",
                    "clave": clave,
                    "valor": valor,
                    "timestamp": sis.tiempo("%H:%M:%S")
                }
                
                lista.agregar(historial, registro)
                escribir(ç("[|nombre|] Eliminado: |clave|"))
            fin
        fin,
        
        "historial": funcion()
            retornar historial
        fin,
        
        "datos": funcion()
            retornar datos
        fin
    }
fin

# Uso
config = crearDiccionarioRastreado("Config")
config.establecer("host", "localhost")
config.establecer("puerto", 8080)
config.establecer("puerto", 3000)  # Cambio
config.eliminar("host")

escribir("\nHistorial de cambios:")
cambios = config.historial()
para cambio en cambios
    escribir(ç("  [|cambio['timestamp']|] |cambio['operacion']| |cambio['clave']|"))
fin
```

**⚠️ `local` aquí no es decorativo: es imprescindible.**
Sin los dos `local`, las asignaciones `datos = { }` y `historial = [ ]` **sobrescriben las variables globales del mismo nombre** que ya existen en capítulos anteriores de este archivo. Verificado ejecutando:

```latino
datos = { "usuarios": ["Ana", "Luis", "Maria"] }   # global previa
config = crearDiccionarioRastreado("Config")          # sin local
# -> datos ahora vale  {} : la lista "usuarios" se ha perdido
```

Es el caso de uso **legítimo** de `local` en una fábrica: el estado interno pertenece a la clausura de la fábrica y no debe filtrarse al ámbito superior.
Un alias directo (`datos = datos`) no serviría, porque seguiría siendo global.

### 3.8.4 Detección de Fugas de Memoria

```latino
# Detectar referencias circulares o estructuras grandes
funcion analizarMemoria(estructura, nombre, profundidad)
    si profundidad > 10
        escribir(ç("⚠ ADVERTENCIA: Profundidad excesiva en '|nombre|'"))
        retornar
    fin
    
    tipo_actual = tipo(estructura)
    
    si tipo_actual == "lista"
        tamaño = lista.longitud(estructura)
        
        si tamaño > 1000
            escribir(ç("⚠ Lista grande: '|nombre|' tiene |tamaño| elementos"))
        fin
        
        # Analizar elementos anidados
        para i en rango(mate.min(tamaño, 5))  # Solo primeros 5
            analizarMemoria(estructura[i], ç("|nombre|[|i|]"), profundidad + 1)
        fin
        
    osi tipo_actual == "diccionario"
        claves = dic.llaves(estructura)
        cantidad = lista.longitud(claves)
        
        si cantidad > 100
            escribir(ç("⚠ Diccionario grande: '|nombre|' tiene |cantidad| claves"))
        fin
        
        # Analizar valores anidados
        para i en rango(mate.min(cantidad, 5))  # Solo primeras 5
            clave = claves[i]
            analizarMemoria(estructura[clave], ç("|nombre|.|clave|"), profundidad + 1)
        fin
    fin
fin

# Detectar uso de memoria
datos_grandes = {
    "items": rango(5000),  # Lista grande
    "config": {"debug": verdadero},
    "anidado": {
        "nivel1": {
            "nivel2": {
                "nivel3": { }
            }
        }
    }
}

analizarMemoria(datos_grandes, "datos_grandes", 0)
# ⚠ Lista grande: 'datos_grandes.items' tiene 5000 elementos
```

> **💡 Consejo de Debugging:** Cuando tengas un bug relacionado con estructuras de datos, sigue estos pasos:
>
> 1. Usa `inspeccionar()` para ver el estado actual
> 2. Usa `validarEstructura()` para verificar integridad
> 3. Usa rastreo de modificaciones para ver cómo llegó a ese estado
> 4. Usa `analizarMemoria()` para detectar problemas de rendimiento

## 3.9 Técnicas Avanzadas

Esta sección cubre patrones y técnicas profesionales para trabajar con estructuras de datos complejas.

### 3.9.1 Estructuras Compuestas (Nested Structures)

Las estructuras del mundo real suelen combinar múltiples tipos de datos anidados. Aquí aprenderás a manejarlas eficientemente.

#### Patrón: Árbol de Datos (JSON-like)

```latino
# Sistema de gestión de organización con múltiples niveles
organizacion = {
    "nombre": "TechCorp",
    "fundacion": 2020,
    "departamentos": [
        {
            "nombre": "Ingeniería",
            "gerente": {
                "nombre": "Carlos Ruiz",
                "id": "EMP001",
                "contacto": {
                    "email": "carlos@techcorp.com",
                    "telefono": "+1-555-0100"
                }
            },
            "equipos": [
                {
                    "nombre": "Backend",
                    "miembros": ["Ana", "Luis", "Pedro"],
                    "tecnologias": conjunto.crear()
                },
                {
                    "nombre": "Frontend",
                    "miembros": ["Maria", "Juan"],
                    "tecnologias": conjunto.crear()
                }
            ]
        },
        {
            "nombre": "Ventas",
            "gerente": {
                "nombre": "Sofia Martinez",
                "id": "EMP002",
                "contacto": {
                    "email": "sofia@techcorp.com",
                    "telefono": "+1-555-0200"
                }
            },
            "equipos": [ ]
        }
    ]
}

# Agregar tecnologías a equipos
conjunto.agregar(organizacion["departamentos"][0]["equipos"][0]["tecnologias"], "Python")
conjunto.agregar(organizacion["departamentos"][0]["equipos"][0]["tecnologias"], "PostgreSQL")
conjunto.agregar(organizacion["departamentos"][0]["equipos"][1]["tecnologias"], "React")
conjunto.agregar(organizacion["departamentos"][0]["equipos"][1]["tecnologias"], "TypeScript")

# Función para navegar estructuras profundas de forma segura
funcion obtenerValorProfundo(estructura, ruta, valorDefecto)
    /*
    Obtiene valores de estructuras anidadas de forma segura
    ruta: lista de claves/índices ["departamentos", 0, "gerente", "nombre"]
    */
    actual = estructura
    
    para paso en ruta
        si tipo(actual) == "diccionario"
            si !dic.contiene(actual, paso)
                retornar valorDefecto
            fin
            actual = actual[paso]
            
        osi tipo(actual) == "lista"
            indice = anumero(paso)
            si indice == nulo || indice >= lista.longitud(actual)
                retornar valorDefecto
            fin
            actual = actual[indice]
            
        sino
            retornar valorDefecto
        fin
    fin
    
    retornar actual
fin

# Uso seguro de navegación profunda
gerente_ingenieria = obtenerValorProfundo(
    organizacion,
    ["departamentos", 0, "gerente", "nombre"],
    "No asignado"
)
escribir("Gerente de Ingeniería: " .. gerente_ingenieria)  # Carlos Ruiz

# Intentar obtener algo que no existe
gerente_marketing = obtenerValorProfundo(
    organizacion,
    ["departamentos", 2, "gerente", "nombre"],
    "No existe"
)
escribir("Gerente de Marketing: " .. gerente_marketing)  # No existe
```

#### Patrón: Índices Múltiples

Crear índices secundarios para búsquedas rápidas por diferentes criterios:

```latino
# Sistema de biblioteca con múltiples índices
biblioteca = {
    "libros": [ ],
    "indice_isbn": { },      # ISBN -> libro
    "indice_autor": { },     # autor -> [libros]
    "indice_genero": { }     # genero -> [libros]
}

funcion agregarLibro(isbn, titulo, autor, genero, año)
    libro = {
        "isbn": isbn,
        "titulo": titulo,
        "autor": autor,
        "genero": genero,
        "año": año
    }
    
    # Agregar a lista principal
    lista.agregar(biblioteca["libros"], libro)
    
    # Índice por ISBN (1 a 1)
    biblioteca["indice_isbn"][isbn] = libro
    
    # Índice por autor (1 a muchos)
    si !dic.contiene(biblioteca["indice_autor"], autor)
        biblioteca["indice_autor"][autor] = [ ]
    fin
    lista.agregar(biblioteca["indice_autor"][autor], libro)
    
    # Índice por género (1 a muchos)
    si !dic.contiene(biblioteca["indice_genero"], genero)
        biblioteca["indice_genero"][genero] = [ ]
    fin
    lista.agregar(biblioteca["indice_genero"][genero], libro)
    
    escribir(ç("✅ Libro agregado: |titulo|"))
fin

funcion buscarPorISBN(isbn)
    retornar dic.obtener(biblioteca["indice_isbn"], isbn, nulo)
fin

funcion buscarPorAutor(autor)
    retornar dic.obtener(biblioteca["indice_autor"], autor, [ ])
fin

funcion buscarPorGenero(genero)
    retornar dic.obtener(biblioteca["indice_genero"], genero, [ ])
fin

# Agregar libros
agregarLibro("978-0-123456-78-9", "El Principito", "Saint-Exupéry", "Infantil", 1943)
agregarLibro("978-0-234567-89-0", "1984", "George Orwell", "Distopía", 1949)
agregarLibro("978-0-345678-90-1", "Rebelión en la granja", "George Orwell", "Distopía", 1945)

# Búsquedas O(1) por ISBN
libro = buscarPorISBN("978-0-123456-78-9")
si libro != nulo
    escribir(ç("Encontrado: |libro['titulo']|"))
fin

# Búsquedas O(1) por autor
libros_orwell = buscarPorAutor("George Orwell")
escribir("\nLibros de Orwell: " .. lista.longitud(libros_orwell))
para libro en libros_orwell
    escribir(ç("  - |libro['titulo']| (|libro['año']|)"))
fin

# Búsquedas O(1) por género
distopias = buscarPorGenero("Distopía")
escribir("\nDistopías disponibles: " .. lista.longitud(distopias))
```

### 3.9.2 Serialización y Persistencia

Convertir estructuras de datos a formato de texto para guardarlas en archivos o enviarlas por red.

#### Serialización Simple (CSV-like)

```latino
# Convertir lista de diccionarios a formato CSV
funcion serializarCSV(registros, columnas)
    si lista.longitud(registros) == 0
        retornar ""
    fin
    
    lineas = [ ]
    
    # Encabezado
    encabezado = cadena.separar(columnas, ",")
    lista.agregar(lineas, columnas)
    
    # Datos
    para registro en registros
        valores = [ ]
        para columna en encabezado
            valor = dic.obtener(registro, columna, "")
            lista.agregar(valores, acadena(valor))
        fin
        
        linea = cadena.concatenar(valores[0], "")
        para i en rango(1, lista.longitud(valores))
            linea = linea .. "," .. valores[i]
        fin
        lista.agregar(lineas, linea)
    fin
    
    # Unir con saltos de línea
    resultado = lineas[0]
    para i en rango(1, lista.longitud(lineas))
        resultado = resultado .. "\n" .. lineas[i]
    fin
    
    retornar resultado
fin

# Deserializar CSV a lista de diccionarios
funcion deserializarCSV(texto)
    lineas = cadena.separar(texto, "\n")
    
    si lista.longitud(lineas) < 2
        retornar [ ]
    fin
    
    # Primera línea es el encabezado
    encabezado = cadena.separar(lineas[0], ",")
    registros = [ ]
    
    # Procesar datos
    para i en rango(1, lista.longitud(lineas))
        valores = cadena.separar(lineas[i], ",")
        registro = { }
        
        para j en rango(lista.longitud(encabezado))
            si j < lista.longitud(valores)
                registro[encabezado[j]] = valores[j]
            fin
        fin
        
        lista.agregar(registros, registro)
    fin
    
    retornar registros
fin

# Uso
usuarios = [
    {"nombre": "Ana", "edad": 25, "ciudad": "Madrid"},
    {"nombre": "Luis", "edad": 30, "ciudad": "Barcelona"},
    {"nombre": "Maria", "edad": 28, "ciudad": "Valencia"}
]

# Serializar
csv = serializarCSV(usuarios, "nombre,edad,ciudad")
escribir("CSV generado:")
escribir(csv)
escribir("")

# Guardar en archivo
archivo.escribir("usuarios.csv", csv)

# Cargar desde archivo
csv_cargado = archivo.leer("usuarios.csv")

# Deserializar
usuarios_recuperados = deserializarCSV(csv_cargado)
escribir("Usuarios recuperados: " .. lista.longitud(usuarios_recuperados))

para usuario en usuarios_recuperados
    escribir(ç("  - |usuario['nombre']| (|usuario['edad']| años) de |usuario['ciudad']|"))
fin

# Limpiar archivo temporal
archivo.borrar("usuarios.csv")
```

#### Serialización JSON-like

```latino
# Convertir estructura a formato JSON-like (string)
funcion aJSON(valor, nivel)
    tipo_valor = tipo(valor)
    indentacion = cadena.rellenar_derecha("", "  ", nivel)
    
    si tipo_valor == "cadena"
        retornar ç('"|valor|"')
        
    osi tipo_valor == "decimal"
        retornar acadena(valor)
        
    osi tipo_valor == "logico"
        retornar valor ? "true" : "false"
        
    osi tipo_valor == "nulo"
        retornar "null"
        
    osi tipo_valor == "lista"
        si lista.longitud(valor) == 0
            retornar "[]"
        fin
        
        resultado = "[\n"
        para i en rango(lista.longitud(valor))
            elemento = valor[i]
            resultado = resultado .. indentacion .. "  " .. aJSON(elemento, nivel + 1)
            
            si i < lista.longitud(valor) - 1
                resultado = resultado .. ","
            fin
            resultado = resultado .. "\n"
        fin
        resultado = resultado .. indentacion .. "]"
        retornar resultado
        
    osi tipo_valor == "diccionario"
        claves = dic.llaves(valor)
        
        si lista.longitud(claves) == 0
            retornar "{}"
        fin
        
        resultado = "{\n"
        para i en rango(lista.longitud(claves))
            clave = claves[i]
            val = valor[clave]
            
            resultado = resultado .. indentacion .. ç('  "|clave|": ') .. aJSON(val, nivel + 1)
            
            si i < lista.longitud(claves) - 1
                resultado = resultado .. ","
            fin
            resultado = resultado .. "\n"
        fin
        resultado = resultado .. indentacion .. "}"
        retornar resultado
        
    sino
        retornar '"' .. acadena(valor) .. '"'
    fin
fin

# Función wrapper
funcion serializar(estructura)
    retornar aJSON(estructura, 0)
fin

# Uso
datos = {
    "nombre": "TechCorp",
    "activo": verdadero,
    "empleados": 150,
    "departamentos": ["Ingeniería", "Ventas", "RRHH"],
    "configuracion": {
        "debug": falso,
        "puerto": 8080,
        "hosts": ["localhost", "192.168.1.1"]
    }
}

json = serializar(datos)
escribir("Estructura serializada a JSON:")
escribir(json)

# Guardar en archivo
archivo.escribir("datos.json", json)
escribir("\n✅ Guardado en datos.json")

# Cargar desde archivo
json_cargado = archivo.leer("datos.json")
escribir("\n✅ Cargado desde archivo")
escribir("Contenido:")
escribir(json_cargado)

# Limpiar
archivo.borrar("datos.json")
```

### 3.9.3 Optimización de Memoria

Técnicas para reducir el consumo de memoria en estructuras grandes.

#### Técnica 1: Compartir Referencias (Flyweight Pattern)

```latino
# En lugar de duplicar datos, compartir referencias
# ❌ MAL: Duplicar datos (consume mucha memoria)
productos_duplicados = [ ]

para i en rango(1000)
    producto = {
        "id": i,
        "categoria": "Electrónica",  # Se duplica 1000 veces
        "descripcion": "Producto genérico de electrónica",  # Se duplica 1000 veces
        "precio": 100
    }
    lista.agregar(productos_duplicados, producto)
fin

# ✅ BIEN: Compartir datos comunes (ahorra memoria)
categorias_cache = {
    "electronica": {
        "nombre": "Electrónica",
        "descripcion": "Producto genérico de electrónica",
        "impuesto": 0.16
    }
}

productos_optimizados = [ ]

para i en rango(1000)
    producto = {
        "id": i,
        "categoria_ref": "electronica",  # Solo referencia
        "precio": 100
    }
    lista.agregar(productos_optimizados, producto)
fin

# Acceso a datos compartidos
funcion obtenerProductoCompleto(producto)
    categoria = categorias_cache[producto["categoria_ref"]]
    
    retornar {
        "id": producto["id"],
        "precio": producto["precio"],
        "categoria": categoria["nombre"],
        "descripcion": categoria["descripcion"],
        "impuesto": categoria["impuesto"]
    }
fin

producto_ejemplo = obtenerProductoCompleto(productos_optimizados[0])
escribir(producto_ejemplo)
```

#### Técnica 2: Lazy Loading (Carga Perezosa)

```latino
# Cargar datos solo cuando se necesitan
cache_usuarios = { }

funcion cargarUsuario(id)
    # Verificar si ya está en cache
    si dic.contiene(cache_usuarios, id)
        escribir(ç("✅ Usuario |id| desde cache"))
        retornar cache_usuarios[id]
    fin
    
    # Simular carga desde base de datos (lento)
    escribir(ç("⏳ Cargando usuario |id| desde BD..."))
    sis.esperar(100)  # Simular latencia
    
    usuario = {
        "id": id,
        "nombre": "Usuario " .. id,
        "datos": rango(100)  # Datos grandes
    }
    
    # Guardar en cache
    cache_usuarios[id] = usuario
    escribir(ç("✅ Usuario |id| cargado y cacheado"))
    
    retornar usuario
fin

# Primera carga: lenta
usuario1 = cargarUsuario(123)  # Carga desde BD

# Segunda carga: rápida (cache)
usuario1_bis = cargarUsuario(123)  # Desde cache

# Tercera carga: nueva, lenta
usuario2 = cargarUsuario(456)  # Carga desde BD
```

#### Técnica 3: Compresión de Listas Grandes

```latino
# Comprimir listas de números en rangos
funcion comprimirRangos(numeros)
    /*
    Convierte [1,2,3,4,7,8,9,15] en [(1,4), (7,9), (15,15)]
    Ahorra memoria para listas grandes con secuencias continuas
    */
    si lista.longitud(numeros) == 0
        retornar [ ]
    fin
    
    # Ordenar primero
    lista.ordenar(numeros, funcion(a, b)
        retornar anumero(a) < anumero(b)
    fin)
    
    rangos = [ ]
    inicio = anumero(numeros[0])
    fin_rango = inicio
    
    para i en rango(1, lista.longitud(numeros))
        actual = anumero(numeros[i])
        
        si actual == fin_rango + 1
            # Continúa el rango
            fin_rango = actual
        sino
            # Termina el rango y empieza uno nuevo
            lista.agregar(rangos, (inicio, fin_rango))
            inicio = actual
            fin_rango = actual
        fin
    fin
    
    # Agregar último rango
    lista.agregar(rangos, (inicio, fin_rango))
    
    retornar rangos
fin

# Descomprimir rangos
funcion descomprimirRangos(rangos)
    numeros = [ ]
    
    para rango_tupla en rangos
        inicio = rango_tupla[0]
        fin_rango = rango_tupla[1]

        para num en rango(inicio, fin_rango + 1)
            lista.agregar(numeros, num)
        fin
    fin
    
    retornar numeros
fin

# Uso
numeros_grandes = [1,2,3,4,5,10,11,12,100,101,102,103,104,105,200]
escribir("Original: " .. lista.longitud(numeros_grandes) .. " elementos")

comprimido = comprimirRangos(numeros_grandes)
escribir("Comprimido: " .. lista.longitud(comprimido) .. " rangos")
escribir("Rangos: " .. comprimido)
# → [(1, 5), (10, 12), (100, 105), (200, 200)]

# Recuperar datos originales
recuperado = descomprimirRangos(comprimido)
escribir("Recuperado: " .. lista.longitud(recuperado) .. " elementos")
escribir("Igual al original: " .. (lista.longitud(recuperado) == lista.longitud(numeros_grandes)))
```

### 3.9.4 Patrones Avanzados de Programación Funcional

#### Patrón: Pipeline de Transformaciones

```latino
# Encadenar múltiples transformaciones
funcion pipeline(datos, transformaciones)
    resultado = datos
    
    para transformacion en transformaciones
        resultado = transformacion(resultado)
    fin
    
    retornar resultado
fin

# Definir transformaciones
filtrarActivos = funcion(usuarios)
    retornar lista.filtrar(usuarios, funcion(u)
        retornar dic.obtener(u, "activo", falso)
    fin)
fin

extraerNombres = funcion(usuarios)
    retornar lista.transformar(usuarios, funcion(u)
        retornar u["nombre"]
    fin)
fin

ordenarAlfabeticamente = funcion(nombres)
    lista.ordenar(nombres, funcion(a, b)
        retornar a < b
    fin)
    retornar nombres
fin

convertirMayusculas = funcion(nombres)
    retornar lista.transformar(nombres, funcion(n)
        retornar cadena.mayusculas(n)
    fin)
fin

# Datos de entrada
usuarios = [
    {"nombre": "maria", "activo": verdadero},
    {"nombre": "ana", "activo": verdadero},
    {"nombre": "luis", "activo": falso},
    {"nombre": "carlos", "activo": verdadero}
]

# Pipeline: filtrar → extraer → ordenar → mayúsculas
resultado = pipeline(usuarios, [
    filtrarActivos,
    extraerNombres,
    ordenarAlfabeticamente,
    convertirMayusculas
])

escribir("Resultado del pipeline:")
escribir(resultado)
# → ["ANA", "CARLOS", "MARIA"]
```

#### Patrón: Memoización con Límite de Cache

```latino
# Cache con límite de tamaño (evita crecimiento infinito)
funcion crearCacheLimitado(limite)
    local cache = { }
    local orden = [ ]  # Rastrear orden de acceso (LRU)
    
    retornar funcion(clave, calculador)
        # Si está en cache, retornar
        si dic.contiene(cache, clave)
            escribir(ç("✅ Cache hit: |clave|"))
            retornar cache[clave]
        fin
        
        # Calcular valor
        escribir(ç("⏳ Calculando: |clave|"))
        valor = calculador(clave)
        
        # Si cache está lleno, eliminar el más antiguo (FIFO simple)
        si lista.longitud(orden) >= limite
            mas_antiguo = orden[0]
            lista.eliminar_indice(orden, 0)
            dic.eliminar(cache, mas_antiguo)
            escribir(ç("♻ Eliminado del cache: |mas_antiguo|"))
        fin
        
        # Agregar al cache
        cache[clave] = valor
        lista.agregar(orden, clave)
        
        retornar valor
    fin
fin

# Función costosa (simulada)
funcion calcularCostoso(n)
    sis.esperar(100)  # Simular cálculo lento
    retornar n * n
fin

# Crear cache con límite de 3 elementos
cacheLimitado = crearCacheLimitado(3)

# Usar cache
escribir(cacheLimitado(5, calcularCostoso))    # Calcula
escribir(cacheLimitado(10, calcularCostoso))   # Calcula
escribir(cacheLimitado(5, calcularCostoso))    # Cache hit
escribir(cacheLimitado(15, calcularCostoso))   # Calcula
escribir(cacheLimitado(20, calcularCostoso))   # Calcula, elimina 5 (FIFO)
escribir(cacheLimitado(10, calcularCostoso))   # Cache hit (10 sigue en caché)
```

### 3.9.5 Proyecto Completo: Sistema de Cache Multinivel

Implementación de un sistema de cache profesional con múltiples niveles de almacenamiento.

```latino
/*
Sistema de Cache Multinivel
- L1: Cache en memoria (rápido, limitado)
- L2: Cache en "disco" simulado (más lento, mayor capacidad)
- L3: Fuente de datos original (muy lento)
*/

funcion SistemaCacheMultinivel()
    # Nivel 1: Cache en memoria (límite 5 elementos)
    local cache_l1 = { }
    local l1_orden = [ ]
    local l1_limite = 5
    
    # Nivel 2: Cache en disco simulado (límite 20 elementos)
    local cache_l2 = { }
    local l2_orden = [ ]
    local l2_limite = 20
    
    # Estadísticas
    local stats = {
        "l1_hits": 0,
        "l2_hits": 0,
        "l3_accesos": 0,
        "total_accesos": 0
    }
    
    cache = {
        "obtener": funcion(clave, fuente)
            stats["total_accesos"] = stats["total_accesos"] + 1
            
            # Buscar en L1 (más rápido)
            si dic.contiene(cache_l1, clave)
                stats["l1_hits"] = stats["l1_hits"] + 1
                escribir(ç("✅ L1 Cache Hit: |clave|"))
                retornar cache_l1[clave]
            fin
            
            # Buscar en L2 (medio)
            si dic.contiene(cache_l2, clave)
                stats["l2_hits"] = stats["l2_hits"] + 1
                escribir(ç("✅ L2 Cache Hit: |clave|"))
                valor = cache_l2[clave]
                
                # Promover a L1
                cache["promoverL1"](clave, valor)
                retornar valor
            fin
            
            # Cargar desde fuente (lento)
            stats["l3_accesos"] = stats["l3_accesos"] + 1
            escribir(ç("⏳ L3 Miss: Cargando |clave| desde fuente..."))
            valor = fuente(clave)
            
            # Guardar en ambos niveles
            cache["promoverL1"](clave, valor)
            cache["guardarL2"](clave, valor)
            
            retornar valor
        fin,
        
        "promoverL1": funcion(clave, valor)
            # Si está lleno, eliminar el más antiguo
            si lista.longitud(l1_orden) >= l1_limite
                antiguo = l1_orden[0]
                lista.eliminar_indice(l1_orden, 0)
                
                # Degradar a L2
                cache["guardarL2"](antiguo, cache_l1[antiguo])
                dic.eliminar(cache_l1, antiguo)
            fin
            
            cache_l1[clave] = valor
            lista.agregar(l1_orden, clave)
        fin,
        
        "guardarL2": funcion(clave, valor)
            # Si está lleno, eliminar el más antiguo
            si lista.longitud(l2_orden) >= l2_limite
                antiguo = l2_orden[0]
                lista.eliminar_indice(l2_orden, 0)
                dic.eliminar(cache_l2, antiguo)
                escribir(ç("♻ L2 eviction: |antiguo|"))
            fin
            
            cache_l2[clave] = valor
            lista.agregar(l2_orden, clave)
        fin,
        
        "estadisticas": funcion()
            total = stats["total_accesos"]
            
            escribir("\n=== ESTADÍSTICAS DE CACHE ===")
            escribir(ç("Total de accesos: |total|"))
            porcL1 = mate.redondear(stats["l1_hits"] / total * 100)
            porcL2 = mate.redondear(stats["l2_hits"] / total * 100)
            porcL3 = mate.redondear(stats["l3_accesos"] / total * 100)
            escribir(ç("L1 Hits: |stats['l1_hits']| (|porcL1|%)"))
            escribir(ç("L2 Hits: |stats['l2_hits']| (|porcL2|%)"))
            escribir(ç("L3 Accesos: |stats['l3_accesos']| (|porcL3|%)"))
            local l1_size = lista.longitud(l1_orden)
            escribir(ç("L1 Size: |l1_size|/|l1_limite|"))
            local l2_size = lista.longitud(l2_orden)
            escribir(ç("L2 Size: |l2_size|/|l2_limite|"))
        fin,
        
        "limpiar": funcion()
            cache_l1 = { }
            l1_orden = [ ]
            cache_l2 = { }
            l2_orden = [ ]
            stats = {
                "l1_hits": 0,
                "l2_hits": 0,
                "l3_accesos": 0,
                "total_accesos": 0
            }
            escribir("✅ Cache limpiado")
        fin
    }
    
    retornar cache
fin

# Simular fuente de datos lenta (base de datos, API, etc.)
funcion cargarDesdeBD(id)
    sis.esperar(200)  # Simular latencia
    retornar {
        "id": id,
        "nombre": "Usuario " .. id,
        "datos": rango(10)
    }
fin

# === DEMO DEL SISTEMA ===

cache = SistemaCacheMultinivel()

escribir("=== Prueba de Sistema de Cache Multinivel ===\n")

# Primera ronda: Todo desde L3
escribir("--- Primera Ronda ---")
cache.obtener(1, cargarDesdeBD)
cache.obtener(2, cargarDesdeBD)
cache.obtener(3, cargarDesdeBD)

# Segunda ronda: L1 hits
escribir("\n--- Segunda Ronda (L1 hits) ---")
cache.obtener(1, cargarDesdeBD)
cache.obtener(2, cargarDesdeBD)

# Tercera ronda: Llenar L1, forzar degradación a L2
escribir("\n--- Tercera Ronda (Llenando L1) ---")
cache.obtener(4, cargarDesdeBD)
cache.obtener(5, cargarDesdeBD)
cache.obtener(6, cargarDesdeBD)
cache.obtener(7, cargarDesdeBD)  # Esto degrada 2 a L2 (FIFO)

# Cuarta ronda: L2 hit
escribir("\n--- Cuarta Ronda (L2 hit) ---")
cache.obtener(1, cargarDesdeBD)  # Debe estar en L2, promocionar a L1

# Estadísticas finales
cache.estadisticas()

# Salida esperada:
# === Prueba de Sistema de Cache Multinivel ===
# 
# --- Primera Ronda ---
# ⏳ L3 Miss: Cargando 1 desde fuente...
# ⏳ L3 Miss: Cargando 2 desde fuente...
# ⏳ L3 Miss: Cargando 3 desde fuente...
# 
# --- Segunda Ronda (L1 hits) ---
# ✅ L1 Cache Hit: 1
# ✅ L1 Cache Hit: 2
# 
# --- Tercera Ronda (Llenando L1) ---
# ⏳ L3 Miss: Cargando 4 desde fuente...
# ⏳ L3 Miss: Cargando 5 desde fuente...
# ⏳ L3 Miss: Cargando 6 desde fuente...
# ⏳ L3 Miss: Cargando 7 desde fuente...
# 
# --- Cuarta Ronda (L2 hit) ---
# ✅ L2 Cache Hit: 1
# 
# === ESTADÍSTICAS DE CACHE ===
# Total de accesos: 10
# L1 Hits: 2 (20%)
# L2 Hits: 1 (10%)
# L3 Accesos: 7 (70%)
# L1 Size: 5/5
# L2 Size: 10/20
```

> **⚠️ ¿Por qué L2 llega a 10 si solo hubo 7 accesos a la fuente?**
> Porque un **fallo en L3 guarda en los dos niveles a la vez**: el código llama a `promoverL1` *y* a `guardarL2` en cada acierto de fuente (7 claves).
>
> | Origen de las entradas en L2 | Cantidad |
> | :--- | ---: |
> | Fallos en L3 (cada uno escribe en L1 **y** en L2) | 7 |
> | Degradaciones: elementos expulsados de L1 al llenarse (clave 7) | 3 |
> | **Total residente en L2** | **10** |
>
> El valor `1/20` que aparece en versiones previas de este ejemplo era **imposible**: la entrada que se acaba de promover desde L2 sigue contando en `l2_orden` (el código no la elimina de ahí al promocionarla), de modo que el mínimo observable ronda los 10 elementos.

---

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
