# CAPÍTULO 8: Organización de Proyectos (Modularidad)

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores, estudiantes y arquitectos de software que necesiten estructurar aplicaciones de mediano y gran porte, dividir responsabilidades en múltiples archivos y evitar colisiones de estado global.  
**Propósito:** Dominar la modularidad en Latino.NET: el uso de `incluir()`, resolución de rutas relativas, el patrón arquitectónico *Factory* para encapsulamiento y privacidad, prevención de dependencias circulares y organización profesional de carpetas de proyecto.

## De Escribir Código a Construir Sistemas

Escribir código que funcione es solo el primer paso en la vida de un programador; **estructurarlo para que pueda crecer sin colapsar** es lo que define la ingeniería de software. A medida que una aplicación evoluciona, mantener todo el código en un único archivo se convierte en una trampa insostenible: las variables colisionan silenciosamente, la navegación se vuelve un laberinto y cualquier modificación menor puede comprometer subsistemas enteros.

Esta evolución exige pasar de simples listas de instrucciones a **sistemas organizados en componentes autónomos**, donde cada parte tenga responsabilidades bien delimitadas y pueda desarrollarse, probarse y mantenerse de forma independiente.

Latino.NET aborda este desafío ofreciendo un sistema de modularidad ligero, intuitivo y de alto rendimiento. En este capítulo aprenderás a gobernar la complejidad dividiendo tus proyectos en módulos desacoplados mediante `incluir()`, protegiendo el estado con el patrón *Factory* y aislando dependencias.

### La Evolución Natural de un Proyecto

Todo software que sobrevive al tiempo atraviesa cuatro etapas predecibles de crecimiento:

1. **El Script Monolítico:** Todo convive en un único archivo (`inicio.lat`). Es la etapa ideal para prototipos rápidos y scripts de terminal de menos de 100 líneas, pero sufre del «efecto bola de nieve»: un cambio en la línea 800 puede alterar una variable global declarada en la línea 50.
2. **La Extracción Funcional:** El código repetitivo se agrupa en funciones locales aplicando el principio *DRY (Don't Repeat Yourself)*. El archivo se vuelve más limpio, pero sigue siendo un cajón de sastre donde conviven modelos de datos, validaciones y lógica de interfaz.
3. **La Separación Modular:** Ciertas utilidades (cálculos matemáticos, validaciones, conexiones a bases de datos) se extraen a archivos `.lat` independientes. El archivo principal deja de ser un obrero todoterreno para convertirse en un **director de orquesta** que coordina dependencias mediante `incluir()`.
4. **El Desacoplamiento y los Patrones:** La etapa de madurez arquitectónica. Los módulos dejan de exponer variables sueltas que contaminan el espacio global y se encapsulan mediante el **Patrón Factory**: funciones que devuelven diccionarios de métodos con estado privado protegido mediante clausuras léxicas (`local`).

### Señales de que tu Proyecto Exige Modularidad

Identificar el momento exacto en que un script debe dividirse es una habilidad clave de diseño. Considera extraer código a módulos si experimentas dos o más de estos síntomas:

* Tu archivo principal supera las **200 líneas** de código continuo.
* Identificas bloques lógicos que **no tienen relación entre sí** (por ejemplo, validaciones de texto y cálculos de nómina en el mismo archivo).
* Te descubres **copiando y pegando** las mismas funciones auxiliares en tres proyectos distintos.
* Modificar una variable en una sección produce **efectos secundarios inexplicables** en otra.
* Necesitas escribir **pruebas unitarias automatizadas** para un algoritmo sin ejecutar el programa completo.

> 💡 **Principio de Responsabilidad Modular (Mantra de Latino.NET):**  
> *\"Un archivo bien diseñado hace una sola cosa y expone una interfaz limpia; el resto del mundo no necesita conocer sus secretos internos.\"*  
> La modularidad no es una ceremonia académica: es la muralla que protege a tu software contra el caos conforme el proyecto escala.

## 8.1 ¿Qué es un Módulo?

### La Analogía de la Biblioteca

**Sin modularidad (Libro Único):**

Imagina una biblioteca donde todo el conocimiento del mundo está en un solo libro gigante de 50,000 páginas. Para encontrar una receta de cocina, tienes que hojear secciones de medicina, física cuántica y poesía.

**Con modularidad (Biblioteca Organizada):**

Ahora imagina que cada tema tiene su propio libro:

* 📘 "Cocina Básica" (100 páginas)
* 📗 "Matemáticas Escolares" (150 páginas)
* 📕 "Primeros Auxilios" (80 páginas)

Cuando necesitas una receta, vas directo al libro de cocina. **Eso es modularidad.**

### En Programación

Un **módulo** en Latino.NET es simplemente un **archivo `.lat` independiente** que contiene código relacionado con un tema específico.

**Ventajas:**

1. **Organización mental:** Cada archivo tiene un propósito claro
2. **Reutilización:** Usa el mismo módulo en 10 proyectos diferentes
3. **Colaboración:** Cada persona trabaja en archivos diferentes (menos conflictos)
4. **Mantenimiento:** Cambios en un módulo no afectan a otros
5. **Testing:** Prueba cada pieza por separado

## 8.2 Tu Primer Módulo: `incluir()`

### El Comando Mágico

La función **`incluir()`** es el pegamento que une archivos. Funciona como un **copiar y pegar virtual**:

1. Lee el contenido del archivo especificado
2. Lo ejecuta **en el contexto actual** (como si estuviera escrito ahí mismo)
3. Todas las funciones y variables del módulo quedan disponibles

🎯 **Ejemplo Ultra-Simple: Constantes Compartidas**

Vamos a dividir nuestro programa en dos archivos por primera vez.

* **Paso 1: Crear el módulo de configuración**

```latino
# config.lat
# Este archivo contiene configuración global del sistema

# Constantes del negocio
IVA = 0.07
DESCUENTO_ESTUDIANTE = 0.15
MONEDA = "USD"

# Mensaje de carga (opcional, pero útil para debugging)
escribir("✅ Configuración cargada")
```

* **Paso 2: Usar el módulo en el programa principal**

```latino
# main.lat
escribir("=== CALCULADORA DE PRECIOS ===")

# Cargar el archivo de configuración
incluir("config")

# Ahora podemos usar las constantes definidas en config.lat
precioBase = 100

impuesto = precioBase * IVA
precioConImpuesto = precioBase + impuesto

escribir("Precio base:", precioBase, MONEDA)
escribir("Impuesto (7%):", impuesto, MONEDA)
escribir("Total:", precioConImpuesto, MONEDA)
```

**Salida al ejecutar `main.lat`:**

```text
=== CALCULADORA DE PRECIOS ===
✅ Configuración cargada
Precio base: 100 USD
Impuesto (7%): 7 USD
Total: 107 USD
```

### 📝 ¿Qué Pasó Aquí?

Cuando Latino.NET ejecuta `incluir("config")`:

1. Busca el archivo `config.lat` en la misma carpeta
2. Lee su contenido línea por línea
3. Ejecuta `IVA = 0.07`, `DESCUENTO_ESTUDIANTE = 0.15`, etc.
4. Ejecuta `escribir("✅ Configuración cargada")`
5. Continúa con el resto de `main.lat`

**Resultado:** Es como si hubieras escrito todo en un solo archivo, pero organizado.

## 8.3 Rutas y Búsqueda de Módulos

### La Extensión es Opcional

Latino.NET es inteligente con las extensiones:

```latino
# Todas estas formas son equivalentes
incluir("config")
incluir("config.lat")

# Latino busca automáticamente:
# 1. config.lat
# 2. config.dll (plugin .NET, veremos esto en el Capítulo 13: Guía de Plugins)
```

**Recomendación:** Omite la extensión `.lat` para mantener el código limpio.

### Rutas Relativas

Los módulos pueden estar en diferentes carpetas. Las rutas se resuelven **relativas al archivo que ejecuta `incluir()`**.

**Estructura de ejemplo:**

```text
MiProyecto/
├── main.lat
├── config.lat
└── lib/
    ├── matematicas.lat
    └── utilidades.lat
```

**Desde `main.lat`:**

```latino
# Mismo directorio
incluir("config")

# Carpeta hija 'lib'
incluir("lib/matematicas")
incluir("lib/utilidades")
```

**Si creas un archivo en `lib/` que necesita otro módulo:**

```latino
# lib/utilidades.lat
# Para incluir matematicas.lat (mismo directorio)
incluir("matematicas")

# Para incluir config.lat (directorio padre)
incluir("../config")
```

**Convenciones de rutas:**

* `"archivo"` → Mismo directorio
* `"lib/archivo"` → Carpeta hija
* `"../archivo"` → Directorio padre
* `"../../archivo"` → Dos niveles arriba

### Errores Comunes de Rutas

#### **Error 1: Archivo No Encontrado**

```latino
incluir("confg")  # ❌ Typo en el nombre
```

**Salida:**

```text
Error Runtime: No se pudo encontrar el archivo 'confg.lat'
```

**Solución:** Verifica:

1. El nombre está bien escrito
2. El archivo existe en la ruta especificada
3. La extensión es `.lat` (no `.txt` o `.latino`)

#### **Error 2: Ruta Incorrecta**

```latino
# Estructura real:
# MiProyecto/
# ├── main.lat
# └── modulos/
#     └── config.lat

# ❌ Incorrecto (busca en el mismo directorio)
incluir("config")

# ✅ Correcto (busca en carpeta 'modulos')
incluir("modulos/config")
```

#### **Error 3: Barras Invertidas en Windows**

```latino
# ❌ Funciona en Windows pero no es portable
incluir("lib\matematicas")

# ✅ Correcto (funciona en todos los sistemas)
incluir("lib/matematicas")
```

**Regla de Oro:** Siempre usa **barras diagonales** `/`, nunca invertidas `\`.

## 8.4 Módulos de Validación

🎯 **Ejemplo Práctico: Sistema de Validación de Usuarios**

Vamos a crear un ejemplo más realista: un sistema que valida datos de usuarios.

**Estructura del proyecto:**

```text
ValidadorUsuarios/
├── main.lat
└── validaciones.lat
```

* **Archivo 1: El módulo de validaciones**

```latino
# validaciones.lat
# Contiene funciones reutilizables para validar datos

funcion ValidarEmail(email)
    # Verificar que contenga @
    tieneArroba = email ~= "@"
    
    # Verificar que contenga punto después del @
    # ⚠️ doble barra: con una sola, el punto sería comodín y aceptaría
    # "usuario@ejemploxcom" como si fuera un dominio válido.
    tieneDominio = email ~= "@.+\\."
    
    retornar tieneArroba ý tieneDominio
fin

funcion ValidarEdad(edad)
    # Convertir a número si es texto
    edadNum = anumero(edad)
    
    # Verificar rango válido
    retornar edadNum >= 18 ý edadNum <= 120
fin

funcion ValidarTelefono(telefono)
    # Verificar que solo contenga dígitos, espacios y guiones
    esValido = telefono ~= "^[0-9 -]+$"
    
    # Verificar longitud mínima
    longitud = cadena.longitud(telefono)
    
    retornar esValido ý longitud >= 7
fin

escribir("📦 Módulo de validaciones cargado")
```

* **Archivo 2: El programa principal**

```latino
# main.lat
escribir("=== REGISTRO DE USUARIOS ===")
escribir("")

# Cargar las funciones de validación
incluir("validaciones")

# Solicitar datos
nombre = pedir("Nombre completo: ")      # Entrada simulada: Carlos López
email = pedir("Email: ")                 # Entrada simulada: carlos@ejemplo.com
edad = pedir("Edad: ")                   # Entrada simulada: 28
telefono = pedir("Teléfono: ")           # Entrada simulada: 6123-4567

# Validar usando las funciones del módulo
escribir("")
escribir("--- VALIDACIÓN ---")

# Email
si ValidarEmail(email)
    escribir("✅ Email válido")
sino
    escribir("❌ Email inválido (debe contener @ y dominio)")
fin

# Edad
si ValidarEdad(edad)
    escribir("✅ Edad válida")
sino
    escribir("❌ Edad inválida (debe ser entre 18 y 120)")
fin

# Teléfono
si ValidarTelefono(telefono)
    escribir("✅ Teléfono válido")
sino
    escribir("❌ Teléfono inválido (solo números, espacios y guiones)")
fin
```

**Ejecución de ejemplo:**

```text
=== REGISTRO DE USUARIOS ===

📦 Módulo de validaciones cargado
Nombre completo: Carlos López
Email: carlos@ejemplo.com
Edad: 28
Teléfono: 6123-4567

--- VALIDACIÓN ---
✅ Email válido
✅ Edad válida
✅ Teléfono válido
```

### Ventajas de Esta Estructura

1. **Reutilización:** Puedes usar `validaciones.lat` en 10 proyectos diferentes
2. **Testing:** Puedes crear un archivo `test_validaciones.lat` para probar solo las validaciones
3. **Mantenimiento:** Si cambias la lógica de validación de email, solo editas un archivo
4. **Claridad:** `main.lat` se enfoca en la lógica del programa, no en detalles técnicos

## 8.5 Debugging: Rastreando la Carga de Módulos

### Técnica: Mensajes de Carga

Es una buena práctica agregar un mensaje al final de cada módulo:

```latino
# matematicas.lat

funcion Suma(a, b)
    retornar a + b
fin

funcion Resta(a, b)
    retornar a - b
fin

# Mensaje de confirmación
escribir("✅ Módulo matematicas.lat cargado")
```

**Ventaja:** Cuando ejecutas el programa, ves exactamente qué módulos se cargaron y en qué orden.

## Arquitectura de Módulos: Guardas de Inclusión (*Include Guards*)

En Latino.NET, cada vez que ejecutas `incluir("archivo.lat")`, el motor lee el archivo desde el disco y ejecuta sus instrucciones.

Si tienes varios archivos en tu proyecto que dependen de una misma configuración o base de datos (por ejemplo, `usuarios.lat` y `ventas.lat` incluyen ambos `conexion_bd.lat`), el código de inicialización se ejecutaría múltiples veces de forma redundante.

Para evitar que operaciones pesadas (conectar a una base de datos, cargar un archivo JSON de 5 MB o inicializar pools de memoria) se repitan, aplicamos el patrón de **Guarda de Inclusión** utilizando variables globales y el operador de fusión nula `??`.

🎯 **Ejemplo Real: Módulo de Conexión SQLite Compartido**

```latino
# conexion_bd.lat
# ==================================================================
# Módulo de Base de Datos con Guarda de Inclusión
# ==================================================================
incluir("sqlite")

# 1. Comprobamos la variable centinela de forma segura.
# Si la variable global aún no existe, ?? devuelve 'falso' sin lanzar error.
_modulo_inicializado = BD_INICIALIZADA ?? falso

si no _modulo_inicializado
    # 2. Marcamos inmediatamente la bandera en el entorno GLOBAL
    global BD_INICIALIZADA = cierto

    escribir("🔄 Inicializando conexión maestra a SQLite...")
    
    # 3. Creamos la conexión y tablas una única vez
    global BD_CONEXION = sqlite.conectar("sistema.db")

    sqlite.ejecutar(BD_CONEXION, "
        CREATE TABLE IF NOT EXISTS configuracion (
            clave TEXT PRIMARY KEY,
            valor TEXT
        );
    ")

    escribir("✅ Conexión establecida y esquema verificado.")
sino
    # Este bloque es opcional (útil para depuración)
    # escribir("ℹ️ Módulo de base de datos ya activo. Reutilizando conexión.")
fin
```

* **¿Cómo se consume en la aplicación?**

Ahora, cualquier parte de tu sistema puede incluir `conexion_bd.lat` con total tranquilidad:

🎯 **Ejemplo: Consumo modular compartido y cierre higiénico de recursos**

> ⚠️ **Continuación del ejemplo anterior.** Este bloque **no se ejecuta por sí solo**: presupone que guardaste el archivo `conexion_bd.lat` del ejemplo previo en la misma carpeta. Si lo copias por separado, el motor informará `No se encontró el módulo o librería 'conexion_bd.lat'`.

```latino
# modulo_ventas.lat
incluir("conexion_bd.lat") # Inicialización única

# modulo_reportes.lat
incluir("conexion_bd.lat") # Detecta la bandera: reutiliza la conexión activa

# Operación de prueba en base de datos
sqlite.ejecutar(BD_CONEXION, "INSERT OR IGNORE INTO configuracion VALUES ('tema', 'oscuro')")

# Higiene de recursos: Cierre de conexión y eliminación del archivo de prueba
sqlite.cerrar(BD_CONEXION)
archivo.borrar("sistema.db")
escribir("🧹 Recursos liberados y base de datos de prueba eliminada.")
```

## El Comportamiento Real de la Re-Inclusión: No Hay Caché

⚠️ **Punto clave del motor:** `incluir()` **no deduplica y no cachea**. Cada llamada **vuelve a leer el archivo y re-ejecuta su contenido íntegro** en el entorno actual.
No existe un registro de "módulos ya cargados".

Verificado en v1.0.81:

| Contenido del módulo | Resultado al incluirlo 2 veces |
| :--- | :--- |
| Solo `funcion` | ✅ Sin error. La función se redefine; sigue funcionando |
| Variables reasignables | ✅ Sin error. El cuerpo **se re-ejecuta** y el valor se recalcula |
| Constante en MAYÚSCULAS | ❌ **Error: `No se puede reasignar la constante 'X'`** |

Esto explica por qué el patrón de guarda de inclusión de la sección anterior **no es una formalidad**: sin él, una re-inclusión volvería a conectar la base de datos, volvería a crear el esquema o volvería a imprimir la cabecera de inicialización.

### El efecto se acumula en cadena

Como no hay caché, un módulo alcanzado **por dos caminos distintos se ejecuta dos veces**, aunque cada archivo se incluya una sola vez:

```latino
# base.lat
base_n = (base_n ?? 0) + 1

# padre.lat
incluir("base")      # ejecuta base.lat

# hijo.lat
incluir("base")      # ejecuta base.lat OTRA VEZ
```

En un programa que incluye `padre` y después `hijo`, `base_n` vale **2**. Los tres archivos se incluyeron una vez cada uno, pero `base.lat` se ejecutó dos veces.

### Cómo hacer la re-inclusión segura

1. **Envuelve los efectos secundarios en una guarda** (sección anterior), usando el operador `??` para consultar la bandera sin lanzar error si aún no existe.
2. **Declara las constantes con `global`** en lugar de asignarlas directamente si necesitas re-asignarlas, o acepta que el módulo solo pueda includirse una vez.
3. **Funciones y helpers puros:** re-incluirlos es inofensivo; solo se redefine el mismo código. El coste es de rendimiento, no de corrección.

```latino
# seguro.lat — re-includible sin efectos duplicados
_seguro_cargado = _seguro_cargado ?? falso

si no _seguro_cargado
    global _seguro_cargado = cierto
    escribir("[carga inicial]")
fin

funcion Seguro()
    retornar "disponible"
fin
```

Incluir `seguro.lat` dos veces imprime `[carga inicial]` **una sola vez**, y
`Seguro()` sigue disponible.

> **Nota:** Esto no es un defecto del manual anterior, sino una propiedad del motor que conviene conocer: `incluir()` es "leer y ejecutar", no "cargar una vez".

## El Problema Oculto de `incluir()`

### La Contaminación del Espacio Global

En las secciones anteriores, aprendimos a dividir código en archivos usando `incluir()`. Funciona perfectamente... hasta que deja de funcionar.

Imagina este escenario real:

**Archivo: `contador.lat`**

```latino
# contador.lat
# Módulo simple para contar cosas

contador = 0

funcion Incrementar()
    contador = contador + 1
fin

funcion Obtener()
    retornar contador
fin

escribir("✅ Módulo contador cargado")
```

**Archivo: `main.lat`**

```latino
# main.lat
escribir("=== MI APLICACIÓN ===")

# Mi contador personal
contador = 100
escribir("Mi contador inicial:", contador)  # 100

# Cargo el módulo
incluir("contador")

# ¿Qué valor tiene mi contador ahora?
escribir("Mi contador después de incluir:", contador)  # ¿100? ¿0?
```

### 🚨 ¿Qué Pasó?

**Salida real:**

```text
=== MI APLICACIÓN ===
Mi contador inicial: 100
✅ Módulo contador cargado
Mi contador después de incluir: 0
```

**¡Tu variable fue sobrescrita!**

Cuando ejecutaste `incluir("contador")`, el intérprete ejecutó:

```latino
contador = 0  # ¡Esto REEMPLAZÓ tu variable!
```

Ambas variables `contador` viven en el **mismo espacio global**. No hay forma de distinguirlas. Esto se conoce como **"Contaminación del Espacio de Nombres"**.

### Riesgos en Proyectos Grandes

Ahora multiplica esto por 10 módulos diferentes:

```latino
incluir("usuarios")      # Define: id, nombre, activo
incluir("productos")     # Define: id, nombre, precio
incluir("facturas")      # Define: id, total, activo
incluir("configuracion") # Define: activo, modo
```

**Resultado:** Variables con el mismo nombre se pisan entre sí. Los bugs son **silenciosos y misteriosos**:

* Cambias `activo` en un lugar y afecta a otro módulo
* El precio de un producto misteriosamente tiene el ID de un usuario
* Tu código funciona solo hasta que incluyes el módulo en diferente orden

> **Este es el problema #1 que enfrentan principiantes al escalar proyectos.**

## 8.6 La Solución: El Patrón Factory

### Filosofía del Patrón

En lugar de ejecutar código suelto en el módulo (que contamina el espacio global), **envolvemos todo en una función** que devuelve un objeto (diccionario).

**Analogía del Mundo Real:**

* **Sin Factory (Mercado Caótico):** Todos los vendedores gritan sus productos al mismo tiempo en la misma plaza. No sabes quién vende qué.

* **Con Factory (Centro Comercial):** Cada vendedor tiene su tienda con nombre. Vas a "Zapatería López" y sabes que todo ahí es de López, no se mezcla con "Electrónica García".

### Versión Simple (Sin Privacidad)

Empecemos con la forma más básica para entender el concepto.

**Archivo: `matematicas_simple.lat`**

```latino
# matematicas_simple.lat
# Versión básica del patrón Factory

funcion CrearModuloMatematicas()
    # En lugar de variables sueltas, creamos un diccionario
    modulo = {
        "PI": 3.14159,
        "version": "1.0"
    }
    
    # Agregamos funciones al diccionario
    modulo["AreaCirculo"] = funcion(radio)
        # Aquí usamos el PI del propio módulo
        retornar modulo["PI"] * (radio ^ 2)
    fin
    
    modulo["AreaRectangulo"] = funcion(base, altura)
        retornar base * altura
    fin
    
    # Retornamos el objeto completo
    retornar modulo
fin

escribir("✅ Fábrica de Matemáticas lista")
```

**Archivo: `app.lat`**

```latino
# app.lat
escribir("=== CALCULADORA DE ÁREAS ===")

# 1. Cargar la definición de la fábrica
incluir("matematicas_simple")

# 2. CREAR el módulo (invocar la fábrica)
# ⚠️ No lo llames 'mate': esa variable SOMBREARÍA el módulo estándar `mate`
# y rompería llamadas como mate.raiz(9). Usa un nombre propio.
moduloMate = CrearModuloMatematicas()

# 3. Usar con sintaxis de punto (o corchetes)
escribir("Versión del módulo:", moduloMate["version"])
escribir("")

radio = 5
area = moduloMate["AreaCirculo"](radio)
escribir("Área del círculo (radio", radio .. "):", area)

base = 10
altura = 4
areaRect = moduloMate["AreaRectangulo"](base, altura)
escribir("Área del rectángulo:", areaRect)
```

**Salida:**

```text
=== CALCULADORA DE ÁREAS ===
✅ Fábrica de Matemáticas lista
Versión del módulo: 1.0

Área del círculo (radio 5): 78.53975
Área del rectángulo: 40
```

### 📝 ¿Qué Cambió?

**Antes (Código Suelto):**

```latino
# Variables globales contaminan
PI = 3.14159

funcion AreaCirculo(r)
    retornar PI * (r ^ 2)
fin
```

**Ahora (Patrón Factory):**

```latino
# Función que RETORNA un objeto con todo adentro
funcion CrearModuloMatematicas()
    modulo = {
        "PI": 3.14159,
        "AreaCirculo": funcion(r)…
    }
    retornar modulo
fin
```

**Ventaja:** Puedes crear **múltiples instancias** si fuera necesario:

```latino
matematicas1 = CrearModuloMatematicas()
matematicas2 = CrearModuloMatematicas()

# Son objetos independientes
matematicas1["PI"] = 3.0  # No afecta a matematicas2
```

### Versión Encapsulada (Con Privacidad Real)

Ahora elevamos el patrón al siguiente nivel usando **variables `local`** para datos verdaderamente privados.

**Archivo: `matematicas_pro.lat`**

```latino
# matematicas_pro.lat
# Versión profesional con encapsulamiento real

funcion CrearModuloMatematicas()
    # ========================================
    # SECCIÓN PRIVADA (local)
    # ========================================
    
    # Estas variables NO existen fuera de esta función
    local _PI = 3.14159265359
    local _VERSION = "2.0"
    local _cache = {}  # Para almacenar cálculos previos
    
    # Función auxiliar privada
    local _validarPositivo = funcion(numero)
        si numero <= 0
            escribir("⚠️ Advertencia: Número debe ser positivo")
            retornar falso
        fin
        retornar verdadero
    fin
    
    # ========================================
    # API PÚBLICA (lo que retornamos)
    # ========================================
    
    retornar {
        # Propiedades públicas
        "version": _VERSION,
        
        # Métodos públicos
        "AreaCirculo": funcion(radio)
            si no _validarPositivo(radio)
                retornar 0
            fin
            
            # Usar la constante privada
            local area = _PI * (radio ^ 2)
            
            # Guardar en caché (privado)
            _cache["ultimoCirculo"] = area
            
            retornar area
        fin,
        
        "AreaRectangulo": funcion(base, altura)
            si no _validarPositivo(base) ó no _validarPositivo(altura)
                retornar 0
            fin
            
            retornar base * altura
        fin,
        
        "Circunferencia": funcion(radio)
            si no _validarPositivo(radio)
                retornar 0
            fin
            
            retornar 2 * _PI * radio
        fin,
        
        "ObtenerPI": funcion()
            # Método público para leer la constante privada
            retornar _PI
        fin
    }
fin

escribir("✅ Módulo Matemáticas Pro v2.0 cargado")
```

**Archivo: `test_matematicas.lat`**

```latino
# test_matematicas.lat
escribir("=== PRUEBA DE MÓDULO MATEMÁTICAS ===")
escribir("")

# Cargar y crear módulo
incluir("matematicas_pro")
# ⚠️ Nombre propio: 'mate' está reservado por el módulo matemático estándar
moduloMate = CrearModuloMatematicas()

# Probar métodos públicos
escribir("Versión:", moduloMate["version"])
escribir("Valor de PI:", moduloMate["ObtenerPI"]())
escribir("")

# Cálculos válidos
escribir("--- Cálculos Válidos ---")
escribir("Área círculo (r=10):", moduloMate["AreaCirculo"](10))
escribir("Circunferencia (r=10):", moduloMate["Circunferencia"](10))
escribir("Área rectángulo (5x8):", moduloMate["AreaRectangulo"](5, 8))
escribir("")

# Probar validación
escribir("--- Prueba de Validación ---")
areaInvalida = moduloMate["AreaCirculo"](-5)
escribir("Área con radio negativo:", areaInvalida)
escribir("")

# Intentar acceder a datos privados
escribir("--- Prueba de Privacidad ---")
intentar
    escribir(moduloMate["_PI"])
atrapar (error)
    escribir("✅ Correcto: _PI es privado (no accesible)")
fin

intentar
    escribir(moduloMate["_cache"])
atrapar (error)
    escribir("✅ Correcto: _cache es privado (no accesible)")
fin
```

**Salida:**

```text
=== PRUEBA DE MÓDULO MATEMÁTICAS ===

✅ Módulo Matemáticas Pro v2.0 cargado
Versión: 2.0
Valor de PI: 3.14159265359

--- Cálculos Válidos ---
Área círculo (r=10): 314.159265359
Circunferencia (r=10): 62.831853072
Área rectángulo (5x8): 40

--- Prueba de Validación ---
⚠️ Advertencia: Número debe ser positivo
Área con radio negativo: 0

--- Prueba de Privacidad ---
✅ Correcto: _PI es privado (no accesible)
✅ Correcto: _cache es privado (no accesible)
```

### 📊 Comparación: Antes vs Después

| Aspecto | Código Suelto | Patrón Factory |
| :--- | :--- | :--- |
| **Variables globales** | Contaminan el espacio | Encapsuladas en objeto |
| **Privacidad** | Todo es público | Variables `local` son privadas |
| **Colisiones de nombres** | Muy probables | Imposibles (cada módulo en su objeto) |
| **Reutilización** | Una sola "instancia" | Múltiples instancias posibles |
| **Testing** | Difícil (estado compartido) | Fácil (módulos aislados) |
| **Mantenimiento** | Cambios afectan todo | Cambios controlados |

## 8.7 Estructura Profesional de Proyectos

### Convenciones de Carpetas

A medida que tu proyecto crece, organizar archivos en carpetas se vuelve esencial.

**Estructura Recomendada:**

```text
MiProyecto/
├── inicio.lat              # Punto de entrada (Main)
│
├── lib/                    # Módulos .lat propios
│   ├── matematicas.lat
│   ├── validaciones.lat
│   └── utilidades.lat
│
├── plugins/                # Librerías .dll externas (Capítulo 13: Guía de Plugins)
│   └── Latino.MySQL.dll
│
├── datos/                  # Archivos de configuración, JSON, CSV
│   ├── config.json
│   └── usuarios.csv
│
├── pruebas/                # Scripts de testing
│   ├── test_matematicas.lat
│   └── test_validaciones.lat
│
└── docs/                   # Documentación del proyecto
    └── README.txt
```

🎯 **Ejemplo Completo: Sistema de Gestión de Tareas**

Vamos a construir una aplicación multi-archivo completa usando las mejores prácticas.

**Estructura del proyecto:**

```text
GestorTareas/
├── main.lat
└── lib/
    ├── tareas.lat
    └── utilidades.lat
```

#### **Archivo 1: `lib/utilidades.lat`**

```latino
# lib/utilidades.lat
# Funciones auxiliares reutilizables

funcion CrearModuloUtilidades()
    local _separador = "=" 
    
    retornar {
        "MostrarTitulo": funcion(texto)
            local linea = ""
            desde (i = 0; i < cadena.longitud(texto); i++)
                linea ..= _separador
            fin
            
            escribir("")
            escribir(linea)
            escribir(texto)
            escribir(linea)
            escribir("")
        fin,
        
        "PedirNumero": funcion(mensaje)
            local valor = pedir(mensaje)
            retornar anumero(valor)
        fin,
        
        "Confirmar": funcion(pregunta)
            local respuesta = pedir(pregunta .. " (s/n): ")
            retornar respuesta == "s" ó respuesta == "S"
        fin
    }
fin

escribir("✅ Módulo Utilidades cargado")
```

#### **Archivo 2: `lib/tareas.lat`**

```latino
# lib/tareas.lat
# Lógica de negocio para gestión de tareas

funcion CrearModuloTareas()
    # Estado privado
    local _tareas = [ ]
    local _contadorID = 1
    
    # Funciones privadas
    local _generarID = funcion()
        idActual = _contadorID
        _contadorID = _contadorID + 1
        retornar idActual
    fin
    
    local _buscarPorID = funcion(id)
        desde (i = 0; i < lista.longitud(_tareas); i++)
            si _tareas[i]["id"] == id
                retornar i
            fin
        fin
        retornar -1  # No encontrado
    fin
    
    # API pública
    retornar {
        "Agregar": funcion(descripcion)
            tarea = {
                "id": _generarID(),
                "descripcion": descripcion,
                "completada": falso
            }
            
            lista.agregar(_tareas, tarea)
            escribir("✅ Tarea agregada con ID:", tarea["id"])
            retornar tarea["id"]
        fin,
        
        "Listar": funcion()
            si lista.longitud(_tareas) == 0
                escribir("📭 No hay tareas")
                retornar
            fin
            
            escribir("")
            escribir("LISTA DE TAREAS:")
            escribir("")
            
            desde (i = 0; i < lista.longitud(_tareas); i++)
                local tarea = _tareas[i]
                local estado = "✅"
                si no tarea["completada"]
                    estado = " "
                fin
                
                escribir("[" .. estado .. "] ID:", tarea["id"], "-", tarea["descripcion"])
            fin
            escribir("")
        fin,
        
        "Completar": funcion(id)
            local indice = _buscarPorID(id)
            
            si indice == -1
                escribir("❌ Tarea no encontrada")
                retornar falso
            fin
            
            _tareas[indice]["completada"] = verdadero
            escribir("✅ Tarea marcada como completada")
            retornar verdadero
        fin,
        
        "Eliminar": funcion(id)
            local indice = _buscarPorID(id)
            
            si indice == -1
                escribir("❌ Tarea no encontrada")
                retornar falso
            fin
            
            lista.eliminar_indice(_tareas, indice)
            escribir("🗑️  Tarea eliminada")
            retornar verdadero
        fin,
        
        "ContarPendientes": funcion()
            local contador = 0
            desde (i = 0; i < lista.longitud(_tareas); i++)
                si no _tareas[i]["completada"]
                    contador = contador + 1
                fin
            fin
            retornar contador
        fin
    }
fin

escribir("✅ Módulo Tareas cargado")
```

#### **Archivo 3: `main.lat`**

```latino
# main.lat
# Programa principal del gestor de tareas

escribir("=== GESTOR DE TAREAS ===")
escribir("Cargando módulos...")

# Cargar módulos
incluir("lib/utilidades")
incluir("lib/tareas")

# Crear instancias
util = CrearModuloUtilidades()
tareas = CrearModuloTareas()

escribir("")
escribir("¡Sistema listo!")

# Función del menú principal
funcion MostrarMenu()
    util["MostrarTitulo"]("MENÚ PRINCIPAL")
    escribir("1. Agregar tarea")
    escribir("2. Listar tareas")
    escribir("3. Completar tarea")
    escribir("4. Eliminar tarea")
    escribir("5. Ver estadísticas")
    escribir("0. Salir")
    escribir("")
    
    retornar util["PedirNumero"]("Selecciona opción: ")
fin

# Bucle principal
seguir = verdadero

mientras (seguir)
    opcion = MostrarMenu()
    
    si opcion == 1
        descripcion = pedir("Descripción de la tarea: ")  # Entrada simulada: Estudiar Latino.NET
        tareas["Agregar"](descripcion)
        
    osi opcion == 2
        tareas["Listar"]()
        
    osi opcion == 3
        idTarea = util["PedirNumero"]("ID de la tarea a completar: ")
        tareas["Completar"](idTarea)
        
    osi opcion == 4
        idTarea = util["PedirNumero"]("ID de la tarea a eliminar: ")
        si util["Confirmar"]("¿Estás seguro?")
            tareas["Eliminar"](idTarea)
        fin
        
    osi opcion == 5
        pendientes = tareas["ContarPendientes"]()
        util["MostrarTitulo"]("ESTADÍSTICAS")
        escribir("Tareas pendientes:", pendientes)
        
    osi opcion == 0
        si util["Confirmar"]("¿Salir del programa?")
            seguir = falso
            escribir("")
            escribir("👋 ¡Hasta luego!")
        fin
        
    sino
        escribir("❌ Opción inválida")
    fin
fin
```

**Ejecución de ejemplo:**

```text
=== GESTOR DE TAREAS ===
Cargando módulos...
✅ Módulo Utilidades cargado
✅ Módulo Tareas cargado

¡Sistema listo!

====================
MENÚ PRINCIPAL
====================

1. Agregar tarea
2. Listar tareas
3. Completar tarea
4. Eliminar tarea
5. Ver estadísticas
0. Salir

Selecciona opción: 1
Descripción de la tarea: Estudiar Latino.NET
✅ Tarea agregada con ID: 1

====================
MENÚ PRINCIPAL
====================
...
Selecciona opción: 2

LISTA DE TAREAS:

[ ] ID: 1 - Estudiar Latino.NET

...
```

## 8.8 Protección Contra Dependencias Circulares

### El Problema del "Ouroboros"

En mitología, el Ouroboros es una serpiente que se muerde la cola formando un círculo infinito. En programación, esto puede pasar con los módulos.

**Escenario problemático:**

```text
usuarios.lat necesita funciones de validaciones.lat
validaciones.lat necesita funciones de usuarios.lat
```

**¿Qué pasaría sin protección?**

1. `main.lat` ejecuta `incluir("usuarios")`
2. `usuarios.lat` ejecuta `incluir("validaciones")`
3. `validaciones.lat` ejecuta `incluir("usuarios")`
4. `usuarios.lat` ejecuta `incluir("validaciones")`
5. `validaciones.lat` ejecuta `incluir("usuarios")`
6. … ∞ (hasta que se agote la memoria)

Esto se conoce como **Dependencia Circular** y causa un *Stack Overflow* (desbordamiento de pila).

### Sistema de Detección de Ciclos

Latino.NET incluye un **detector automático de ciclos** que rastrea qué archivos se están cargando en este momento.

**Ejemplo de ciclo:**

**Archivo: `modulo_a.lat`**

```latino
# modulo_a.lat
escribir("Cargando módulo A...")

# Intenta cargar B
incluir("modulo_b")

funcion FuncionA()
    escribir("Soy la función A")
fin

escribir("Módulo A cargado")
```

**Archivo: `modulo_b.lat`**

```latino
# modulo_b.lat
escribir("Cargando módulo B...")

# Intenta cargar A (¡ciclo!)
incluir("modulo_a")

funcion FuncionB()
    escribir("Soy la función B")
fin

escribir("Módulo B cargado")
```

**Archivo: `main.lat`**

```latino
# main.lat
escribir("Iniciando programa...")

incluir("modulo_a")

escribir("Programa terminado")
```

**Salida al ejecutar:**

```text
Iniciando programa...
Cargando módulo A...
Cargando módulo B...
❌ Error Runtime: Dependencia circular detectada: 'modulo_a.lat' ya se está cargando.
```

**¿Qué pasó?**

1. Latino.NET mantiene una **pila de archivos en proceso de carga**
2. Cuando detecta que un archivo ya está en la pila, **lanza un error inmediatamente**

### Solución: Refactorización de Dependencias

Si encuentras un ciclo, tienes tres opciones:

#### **Opción 1: Extraer Funciones Comunes**

Crea un tercer módulo con las funciones compartidas:

```text
Antes (Ciclo):
usuarios.lat ↔ validaciones.lat

Después (Sin Ciclo):
          ↗ utilidades.lat ↖
usuarios.lat              validaciones.lat
```

**Ejemplo:**

**Archivo: `utilidades.lat`**

```latino
funcion CrearModuloUtilidades()
    retornar {
        "EsTextoVacio": funcion(texto)
            retornar texto == ""
        fin,
        
        "EsNumeroPositivo": funcion(num)
            retornar num > 0
        fin
    }
fin
```

**Archivo: `usuarios.lat`**

```latino
incluir("utilidades")
util = CrearModuloUtilidades()

funcion CrearModuloUsuarios()
    retornar {
        "ValidarNombre": funcion(nombre)
            retornar no util["EsTextoVacio"](nombre)
        fin
    }
fin
```

**Archivo: `validaciones.lat`**

```latino
incluir("utilidades")
util = CrearModuloUtilidades()

funcion CrearModuloValidaciones()
    retornar {
        "ValidarEdad": funcion(edad)
            retornar util["EsNumeroPositivo"](edad)
        fin
    }
fin
```

#### **Opción 2: Inyección de Dependencias**

En lugar de que el módulo cargue sus dependencias, las recibe como parámetro:

**Archivo: `usuarios.lat`**

```latino
# NO incluimos validaciones aquí
funcion CrearModuloUsuarios(moduloValidaciones)
    retornar {
        "Registrar": funcion(nombre, edad)
            # Usamos el módulo inyectado
            si moduloValidaciones["ValidarEdad"](edad)
                escribir("Usuario registrado:", nombre)
            sino
                escribir("Edad inválida")
            fin
        fin
    }
fin
```

**Archivo: `main.lat`**

```latino
incluir("validaciones")
incluir("usuarios")

# Creamos validaciones primero
val = CrearModuloValidaciones()

# Inyectamos validaciones en usuarios
usr = CrearModuloUsuarios(val)

# Ahora usuarios puede usar validaciones sin incluirlo
usr["Registrar"]("Carlos", 28)
```

#### **Opción 3: Reorganizar la Arquitectura**

A veces un ciclo indica un **problema de diseño**. Revisa si realmente necesitas esa dependencia mutua.

**Pregunta clave:** *"¿Este módulo realmente necesita conocer al otro, o solo necesita una interfaz específica?"*

## 8.9 Módulos .lat vs Plugins .dll

### ¿Cuál Usar y Cuándo?

Latino.NET soporta dos tipos de extensiones:

| Característica | Módulos `.lat` | Plugins `.dll` |
| :--- | :--- | :--- |
| **Lenguaje** | Latino (interpretado) | C# (compilado) |
| **Velocidad** | Normal (Tree-Walker) | Máxima (.NET 10 nativo) |
| **Facilidad** | Muy fácil de escribir | Requiere conocer C# |
| **Distribución** | Código fuente visible | Binario precompilado |
| **Debugging** | Fácil (ver código directo) | Más complejo |
| **Acceso .NET** | Limitado | Completo (TODO .NET) |
| **Acceso desde el código** | **Sin prefijo** (salvo fábrica) | **Con prefijo obligatorio**: `lapsos.ahora()` |
| **IntelliSense en VS Code** | ❌ **Sin autocompletado dinámico** | ✅ **Autocompletado, firmas y ayuda visual (hover)** |
| **Ideal para** | Lógica de negocio y modelos | Conectores BD, hardware, cálculo pesado |

### Cuándo Usar Módulos .lat

✅ **Usa `.lat` cuando:**

* La lógica es específica de tu aplicación (reglas de negocio)
* Necesitas modificar código frecuentemente (desarrollo activo)
* El rendimiento es aceptable
* Quieres que otros usuarios puedan ver/modificar el código
* Estás prototipando rápidamente

**Ejemplos:**

* Sistema de validaciones personalizado
* Lógica de cálculo de descuentos
* Generadores de reportes
* Reglas de juegos

### Cuándo Usar Plugins .dll

✅ **Usa `.dll` cuando:**

* Necesitas **máximo rendimiento** (procesamiento de millones de datos)
* Quieres acceder a **librerías .NET** (MySQL, PostgreSQL, HTTP, etc.)
* Necesitas **código nativo** (interactuar con hardware, sistema operativo)
* Quieres **proteger tu código** (distribuir binario)
* Reutilizarás la funcionalidad en múltiples proyectos

**Ejemplos:**

* Conector a base de datos
* Cliente HTTP para APIs
* Procesamiento de imágenes
* Encriptación avanzada
* Interfaz con hardware (impresoras, sensores)

### Cómo Usar Plugins .dll

La **carga** es idéntica a los módulos `.lat` (el nombre puede llevar o no extensión), pero el **acceso** se diferencia:

```latino
# Latino busca automáticamente .lat y .dll
incluir("lapsos")          # o incluir("lapsos.dll") — ambas formas funcionan

# Un plugin .dll SÍ expone prefijo (lo define [LatinoExtension] en el C#)
escribir(lapsos.ahora())
```

⚠️ **Un módulo `.lat` NO expone prefijo.** Al incluirlo, sus funciones quedan en el
**ámbito global**, tal como están escritas:

```latino
# utiles.lat contiene:  funcion AreaCirculo(radio) ... fin
incluir("utiles")

AreaCirculo(5)        # ✅ correcto: se llama sin prefijo
utiles.AreaCirculo(5) # ❌ RuntimeError: Variable no definida: 'utiles'
```

Por eso, si quieres que un `.lat` se use **con prefijo** (`mod.AreaCirculo(...)`), hay que devolver un diccionario que agrupe sus funciones. Es exactamente lo que hace el **patrón Factory** de la sección 8.6, y la forma más limpia es que el propio módulo exponga la fábrica:

**Archivo: `matematicas.lat`**

```latino
funcion AreaCirculo(radio)
    retornar 3.14159 * (radio ^ 2)
fin

funcion AreaRectangulo(base, altura)
    retornar base * altura
fin

# El módulo se devuelve a sí mismo agrupado en un diccionario
funcion CrearEspacio()
    retornar {
        "AreaCirculo": AreaCirculo,
        "AreaRectangulo": AreaRectangulo
    }
fin
```

**Archivo: `main.lat`**

```latino
incluir("matematicas")

mat = CrearEspacio()

escribir("Área del círculo (radio 5):", mat.AreaCirculo(5))
escribir("Área del rectángulo:", mat.AreaRectangulo(10, 4))
```

**Salida:**

```text
Área del círculo (radio 5): 78.53975
Área del rectángulo: 40
```

Tres notas sobre el alias:

* **`.` y `["clave"]` son equivalentes:** `mat.AreaCirculo(5)` y
  `mat["AreaCirculo"](5)` devuelven lo mismo.
* **El alias guarda una referencia, no un nombre.** Sobrevive a que el módulo se
  vuelva a incluir, porque lo que se conserva es el diccionario de funciones.
* ⚠️ **No le pongas al alias el nombre de una librería estándar.** Escribir
  `mate = CrearEspacio()` ocultaría la librería `mate` y rompería llamadas como
  `mate.raiz(9)`. Usa un nombre propio (`mat`, `moduloMate`, `mM`…).

### 🧠 La Asimetría de Herramientas: Autocompletado y Ayuda Visual

Existe una diferencia fundamental en la experiencia de desarrollo dentro de VS Code al usar un módulo `.lat` frente a un plugin `.dll`:

#### 1. Plugins `.dll` (Soporte Completo de IntelliSense)

Los plugins oficiales (`sqlite`, `lapsos`, `formulas`, `zip`, etc.) contienen **metadatos estáticos precompilados** en su interior. Cuando escribes `incluir("lapsos")`, el servidor de lenguaje (LSP) de Latino.NET inspecciona el binario en segundo plano y conoce de inmediato todas sus funciones, qué parámetros esperan y su documentación.

Por eso, al escribir el nombre del módulo seguido de un punto (`lapsos.`), el editor te muestra:

* Lista desplegable con todos los métodos disponibles.
* Plantillas de argumentos automáticas con tabulación.
* Ventana emergente con la documentación y tipos de retorno al pasar el mouse por encima (*hover*).

#### 2. Módulos `.lat` (Código Dinámico al Vuelo)

Un módulo `.lat` es código interpretado que se evalúa dinámicamente en tiempo de ejecución. El editor de código **no ejecuta tus scripts en segundo plano mientras escribes** (hacerlo provocaría congelamientos o efectos secundarios como conectar a bases de datos o crear archivos accidentales).

En consecuencia:

* Las funciones o diccionarios devueltos por un módulo `.lat` **no cuentan con autocompletado ni ayuda emergente de parámetros**.
* Debes consultar el archivo `.lat` o documentar su interfaz mediante comentarios para recordar qué funciones expone.

---

> ⚙️ **¿Por qué no veo autocompletado de un plugin en VS Code?**  
> Si escribes `incluir("sqlite")` o `incluir("lapsos")` y al poner el punto no ves la lista de funciones ni la ayuda visual, verifica estas tres causas habituales:
>
> 1. **Falta la sentencia `incluir()`:** El editor solo carga la documentación de un plugin si la directiva `incluir("nombre")` está escrita en el archivo abierto.
> 2. **Ubicación local:** El editor busca el archivo `.dll` en la misma carpeta de tu script o dentro de una subcarpeta llamada `lib/` o `plugins/`.
> 3. **Ruta global en VS Code:** Si usas los plugins globales de tu instalación de Latino.NET, debes indicarle a la extensión dónde vive tu ejecutable. Ve a **Archivo > Preferencias > Configuración**, busca **Latino** y en **Binary Path** define la ruta absoluta a tu intérprete (ejemplo: `C:\LatinoNET\latino.exe`). Con esto, el editor sabrá dónde encontrar la carpeta `lib/` maestra de plugins.
>
> **Nota:** La creación de plugins .dll se explica en detalle en el **Capítulo 13: Guía de Plugins**. Por ahora, solo necesitas saber cómo usarlos.

## 8.10 Caso de Estudio Completo: Sistema de Inventario

Vamos a construir un sistema de inventario profesional usando todas las mejores prácticas aprendidas.

### Arquitectura del Sistema

```text
SistemaInventario/
├── main.lat                    # Punto de entrada
│
├── lib/                        # Módulos de lógica
│   ├── productos.lat          # Gestión de productos
│   ├── reportes.lat           # Generación de reportes
│   └── utilidades.lat         # Funciones auxiliares
│
├── datos/                      # Archivos de configuración
│   └── config.lat             # Constantes del sistema
│
└── pruebas/                    # Scripts de testing
    └── test_productos.lat
```

### **Archivo 1: `datos/config.lat`**

```latino
# datos/config.lat
# Configuración global del sistema

funcion CargarConfiguracion()
    retornar {
        "VERSION": "1.0",
        "EMPRESA": "Mi Empresa S.A.",
        "IVA": 0.07,
        "MONEDA": "USD",
        "STOCK_MINIMO": 10,
        "STOCK_CRITICO": 5
    }
fin

escribir("✅ Configuración del sistema cargada")
```

### **Archivo 2: `lib/utilidades.lat`**

```latino
# lib/utilidades.lat
# Funciones auxiliares reutilizables

funcion CrearModuloUtilidades()
    local _ancho = 50
    
    utilidades = {
        "Separador": funcion()
            local linea = ""
            desde (i = 0; i < _ancho; i++)
                linea ..= "="
            fin
            escribir(linea)
        fin,
        
        "Titulo": funcion(texto)
            escribir("")
            utilidades["Separador"]()
            escribir(texto)
            utilidades["Separador"]()
            escribir("")
        fin,
        
        "PedirEntero": funcion(mensaje)
            local valor = pedir(mensaje)
            retornar anumero(valor)
        fin,
        
        "PedirDecimal": funcion(mensaje)
            local valor = pedir(mensaje)
            retornar anumero(valor)
        fin,
        
        "Confirmar": funcion(pregunta)
            local respuesta = pedir(pregunta .. " (s/n): ")
            retornar respuesta == "s" ó respuesta == "S"
        fin,
        
        "FormatearMoneda": funcion(cantidad, moneda)
            retornar ç("|moneda| |cantidad|")
        fin
    }
    
    retornar utilidades
fin

escribir("✅ Módulo Utilidades cargado")
```

### **Archivo 3: `lib/productos.lat`**

```latino
# lib/productos.lat
# Lógica de negocio para productos

funcion CrearModuloProductos(config)
    # Estado privado
    local _productos = [ ]
    local _siguienteID = 1
    
    # Funciones privadas
    local _generarID = funcion()
        idActual = _siguienteID
        _siguienteID = _siguienteID + 1
        retornar idActual
    fin
    
    local _buscarPorID = funcion(id)
        desde (i = 0; i < lista.longitud(_productos); i++)
            si _productos[i]["id"] == id
                retornar i
            fin
        fin
        retornar -1
    fin
    
    local _validarStock = funcion(producto)
        si producto["stock"] <= config["STOCK_CRITICO"]
            retornar "CRÍTICO"
        osi producto["stock"] <= config["STOCK_MINIMO"]
            retornar "BAJO"
        sino
            retornar "NORMAL"
        fin
    fin
    
    # API pública
    retornar {
        "Agregar": funcion(nombre, precio, stock)
            # Validaciones
            si nombre == ""
                escribir("❌ El nombre no puede estar vacío")
                retornar nulo
            fin
            
            si precio <= 0
                escribir("❌ El precio debe ser mayor a cero")
                retornar nulo
            fin
            
            si stock < 0
                escribir("❌ El stock no puede ser negativo")
                retornar nulo
            fin
            
            # Crear producto
            local producto = {
                "id": _generarID(),
                "nombre": nombre,
                "precio": precio,
                "stock": stock
            }
            
            lista.agregar(_productos, producto)
            escribir("✅ Producto agregado con ID:", producto["id"])
            
            # Advertencia de stock
            local estado = _validarStock(producto)
            si estado == "CRÍTICO"
                escribir("⚠️  ADVERTENCIA: Stock crítico")
            osi estado == "BAJO"
                escribir("⚠️  ADVERTENCIA: Stock bajo")
            fin
            
            retornar producto["id"]
        fin,
        
        "Listar": funcion()
            si lista.longitud(_productos) == 0
                escribir("📭 No hay productos registrados")
                retornar
            fin
            
            escribir("")
            imprimirf("%-5s %-25s %12s %10s %10s\n", "ID", "NOMBRE", "PRECIO", "STOCK", "ESTADO")
            escribir("----------------------------------------------------------------")
            
            desde (i = 0; i < lista.longitud(_productos); i++)
                p = _productos[i]
                estado = _validarStock(p)
                
                icono = "✅"
                si estado == "CRÍTICO"
                    icono = "🚨"
                osi estado == "BAJO"
                    icono = "⚠️ "
                fin
                
                imprimirf("%-5d %-25s %s%10.2f %10d %10s\n", 
                    p["id"], 
                    p["nombre"], 
                    config["MONEDA"] .. " ",
                    p["precio"], 
                    p["stock"],
                    icono .. " " .. estado
                )
            fin
            escribir("")
        fin,
        
        "ActualizarStock": funcion(id, cantidad)
            local indice = _buscarPorID(id)
            
            si indice == -1
                escribir("❌ Producto no encontrado")
                retornar falso
            fin
            
            local nuevoStock = _productos[indice]["stock"] + cantidad
            
            si nuevoStock < 0
                escribir("❌ No hay suficiente stock para esta operación")
                retornar falso
            fin
            
            _productos[indice]["stock"] = nuevoStock
            
            si cantidad > 0
                escribir("✅ Stock incrementado en", cantidad, "unidades")
            sino
                escribir("✅ Stock decrementado en", (cantidad * -1), "unidades")
            fin
            
            # Verificar nuevo estado
            local estado = _validarStock(_productos[indice])
            si estado == "CRÍTICO"
                escribir("⚠️  ADVERTENCIA: Stock ahora está crítico")
            osi estado == "BAJO"
                escribir("⚠️  ADVERTENCIA: Stock ahora está bajo")
            fin
            
            retornar verdadero
        fin,
        
        "ObtenerTotal": funcion()
            retornar lista.longitud(_productos)
        fin,
        
        "CalcularValorInventario": funcion()
            local total = 0
            desde (i = 0; i < lista.longitud(_productos); i++)
                p = _productos[i]
                total = total + (p["precio"] * p["stock"])
            fin
            retornar total
        fin,
        
        "ProductosConStockBajo": funcion()
            local bajos = [ ]
            desde (i = 0; i < lista.longitud(_productos); i++)
                p = _productos[i]
                si p["stock"] <= config["STOCK_MINIMO"]
                    lista.agregar(bajos, p)
                fin
            fin
            retornar bajos
        fin
    }
fin

escribir("✅ Módulo Productos cargado")
```

### **Archivo 4: `lib/reportes.lat`**

```latino
# lib/reportes.lat
# Generación de reportes del sistema

funcion CrearModuloReportes(config, util)
    retornar {
        "Inventario": funcion(productos)
            util["Titulo"]("REPORTE DE INVENTARIO")
            
            escribir("Empresa:", config["EMPRESA"])
            escribir("Versión:", config["VERSION"])
            escribir("")
            
            local total = productos["ObtenerTotal"]()
            local valor = productos["CalcularValorInventario"]()
            
            escribir("Total de productos:", total)
            escribir("Valor total inventario:", util["FormatearMoneda"](valor, config["MONEDA"]))
            escribir("")
            
            productos["Listar"]()
        fin,
        
        "StockBajo": funcion(productos)
            util["Titulo"]("PRODUCTOS CON STOCK BAJO")
            
            local bajos = productos["ProductosConStockBajo"]()
            
            si lista.longitud(bajos) == 0
                escribir("✅ Todos los productos tienen stock suficiente")
                retornar
            fin
            
            escribir("⚠️  Se encontraron", lista.longitud(bajos), "productos con stock bajo:")
            escribir("")
            
            desde (i = 0; i < lista.longitud(bajos); i++)
                p = bajos[i]
                escribir("  •", p["nombre"], "- Stock actual:", p["stock"], "unidades")
            fin
            escribir("")
        fin
    }
fin

escribir("✅ Módulo Reportes cargado")
```

### **Archivo 5: `main.lat`**

```latino
# main.lat
# Programa principal del sistema de inventario

escribir("=== SISTEMA DE INVENTARIO ===")
escribir("Cargando módulos...")
escribir("")

# Cargar configuración
incluir("datos/config")
config = CargarConfiguracion()

# Cargar módulos
incluir("lib/utilidades")
incluir("lib/productos")
incluir("lib/reportes")

# Crear instancias (inyección de dependencias)
util = CrearModuloUtilidades()
productos = CrearModuloProductos(config)
reportes = CrearModuloReportes(config, util)

escribir("")
escribir("¡Sistema listo!")
escribir("")

# Función del menú principal
funcion MostrarMenu()
    util["Titulo"]("MENÚ PRINCIPAL")
    escribir("1. Agregar producto")
    escribir("2. Listar productos")
    escribir("3. Actualizar stock")
    escribir("4. Ver reporte de inventario")
    escribir("5. Ver productos con stock bajo")
    escribir("0. Salir")
    escribir("")
    
    retornar util["PedirEntero"]("Selecciona opción: ")
fin

# Bucle principal
seguir = verdadero

mientras (seguir)
    opcion = MostrarMenu()
    
    si opcion == 1
        util["Titulo"]("AGREGAR PRODUCTO")
        nombre = pedir("Nombre del producto: ")  # Entrada simulada: Laptop Dell XPS
        precio = util["PedirDecimal"]("Precio: ")        # Entrada simulada: 1200.50
        stock = util["PedirEntero"]("Stock inicial: ")     # Entrada simulada: 3
        productos["Agregar"](nombre, precio, stock)
        
    osi opcion == 2
        util["Titulo"]("LISTADO DE PRODUCTOS")
        productos["Listar"]()
        
    osi opcion == 3
        util["Titulo"]("ACTUALIZAR STOCK")
        productos["Listar"]()
        idProducto = util["PedirEntero"]("ID del producto: ")  # Entrada simulada: 1
        cantidad = util["PedirEntero"]("Cantidad (+agregar / -quitar): ")  # Entrada simulada: 5
        productos["ActualizarStock"](idProducto, cantidad)
        
    osi opcion == 4
        reportes["Inventario"](productos)
        
    osi opcion == 5
        reportes["StockBajo"](productos)
        
    osi opcion == 0
        si util["Confirmar"]("¿Salir del sistema?")
            seguir = falso
            escribir("")
            escribir("👋 ¡Hasta luego!")
        fin
        
    sino
        escribir("❌ Opción inválida")
    fin
    
    escribir("")
    pedir("Presiona Enter para continuar...")
fin
```

**Ejecución de ejemplo:**

```text
=== SISTEMA DE INVENTARIO ===
Cargando módulos...

✅ Configuración del sistema cargada
✅ Módulo Utilidades cargado
✅ Módulo Productos cargado
✅ Módulo Reportes cargado

¡Sistema listo!

==================================================
MENÚ PRINCIPAL
==================================================

1. Agregar producto
2. Listar productos
3. Actualizar stock
4. Ver reporte de inventario
5. Ver productos con stock bajo
0. Salir

Selecciona opción: 1

==================================================
AGREGAR PRODUCTO
==================================================

Nombre del producto: Laptop Dell XPS
Precio: 1200.50
Stock inicial: 3
✅ Producto agregado con ID: 1
⚠️  ADVERTENCIA: Stock crítico

Presiona Enter para continuar...
```

## 8.11 Patrón de Archivo Manifiesto (Dependencias Globales)

A medida que tu proyecto en Latino.NET crezca, es muy probable que empieces a utilizar múltiples librerías estándar y plugins externos (como `sqlite`, `hilos`, `formulas`, `lapsos`, etc.).

Si colocas todos los comandos `incluir()` al principio de tu archivo principal, el código puede volverse ruidoso y difícil de leer. Para solucionar esto, los desarrolladores avanzados utilizan el **Patrón de Archivo Manifiesto** (similar a los *Barrel Files* en TypeScript o los *Global Usings* en C# 10).

Este patrón consiste en crear un archivo dedicado exclusivamente a cargar todas las dependencias del proyecto. Dado que la función `incluir()` de Latino.NET inyecta las librerías en el entorno global compartido, solo necesitas importar este "manifiesto" una vez en tu archivo principal.

**Ejemplo de Implementación:**

1. Crea un archivo llamado `manifiesto_deps.lat` (este será tu barril de dependencias):

    ```latino
    # archivo: manifiesto_deps.lat
    # ==========================================
    # MANIFIESTO DE DEPENDENCIAS DEL SISTEMA
    # ==========================================

    # 1. Librerías Estándar (ya cargadas globalmente, no necesitan incluir)
    # incluir("archivo")  # ← No es necesario, archivo es parte del núcleo
    incluir("lapsos")

    # 2. Plugins Oficiales
    incluir("sqlite")
    incluir("hilos")

    # 3. Módulos Internos del Proyecto (descomenta según tu estructura real)
    # incluir("modelos_datos.lat")
    # incluir("utilidades_red.lat")

    escribir("✅ Todas las dependencias cargadas correctamente.")
    ```

2. En tu archivo principal, mantén el código limpio importando solo el manifiesto:

    ```latino
    # archivo: app_principal.lat
    incluir("manifiesto_deps.lat")

    funcion IniciarApp()
        # Las librerías ya están disponibles globalmente
        horaActual = lapsos.ahora()
        dbConexion = sqlite.conectar("datos.db")
        
        escribir("Iniciando sistema a las: " .. horaActual["texto"])
        
        # ... resto de la lógica ...
        
        # Cierre de recursos: primero la conexión, luego el archivo en disco
        sqlite.cerrar(dbConexion)
        archivo.borrar("datos.db")
    fin

    IniciarApp()
    ```

**Ventajas de este patrón:**

* **Limpieza:** Tu lógica de negocio no se mezcla con la gestión de dependencias.
* **Mantenibilidad:** Si agregas un nuevo plugin al proyecto, solo lo añades al manifiesto y estará disponible para todo el sistema.
* **Tiempos de carga:** Evitas llamar a `incluir("misma_libreria")` accidentalmente en múltiples archivos.

## 8.12 Checklist de Modularización

### Lista de Verificación para Organizar Proyectos

Usa esta lista cuando estés organizando un proyecto:

**Análisis Inicial:**

* [ ] ¿El archivo principal tiene más de 200 líneas?
* [ ] ¿Hay funciones que nunca interactúan entre sí?
* [ ] ¿Hay lógica que podrías reutilizar en otros proyectos?
* [ ] ¿Te cuesta encontrar código cuando lo buscas?

**Estructura de Carpetas:**

* [ ] Creaste carpeta `lib/` para módulos propios
* [ ] Creaste carpeta `datos/` para configuraciones
* [ ] Creaste carpeta `pruebas/` para tests (opcional)
* [ ] El archivo principal se llama `main.lat` o `inicio.lat`

**Diseño de Módulos:**

* [ ] Cada módulo tiene un **propósito único y claro**
* [ ] Los nombres de archivos son **descriptivos** (`validaciones.lat`, no `utils.lat`)
* [ ] Usaste el **Patrón Factory** para encapsulación
* [ ] Las constantes privadas usan `local`
* [ ] Cada módulo tiene mensaje de carga (debugging)

**Dependencias:**

* [ ] No hay dependencias circulares
* [ ] Las rutas usan `/` (no `\`)
* [ ] Los módulos reciben dependencias como parámetros (inyección)
* [ ] Extrajiste funciones comunes a librería de utilidades

**Documentación:**

* [ ] Cada módulo tiene comentario inicial explicando su propósito
* [ ] Las funciones públicas tienen comentarios
* [ ] Existe un archivo README.txt explicando la estructura

## 8.13 Tabla de Decisión Final

### ¿Cuándo Usar Cada Estrategia?

| Situación | Estrategia Recomendada |
| :--- | :--- |
| Script de 50 líneas | **Un solo archivo** (no modularizar) |
| Script de 200-500 líneas | **Dividir en 2-3 archivos** con `incluir()` simple |
| Proyecto de 500-2000 líneas | **Patrón Factory** + carpetas `lib/` |
| Proyecto de 2000+ líneas | **Patrón Factory** + inyección de dependencias + estructura completa |
| Necesitas máximo rendimiento | **Plugin .dll** (ver Capítulo 13: Guía de Plugins) |
| Lógica reutilizable entre proyectos | **Módulo .lat** con Patrón Factory |
| Conector a base de datos | **Plugin .dll** |
| Reglas de negocio específicas | **Módulo .lat** |

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
