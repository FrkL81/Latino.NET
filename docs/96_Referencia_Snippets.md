# 📝 Referencia de Fragmentos de Código (Snippets)

Guía completa de los snippets integrados en la extensión VS Code de Latino.NET v1.0.81.

> **⚠️ Nota Importante:** Esta lista contiene **solo snippets de alto valor agregado**. Las funciones simples de la librería estándar (como `cadena.estilo()`, `mate.abs()`, `lista.agregar()`, etc.) ya están disponibles mediante el **autocompletado inteligente del Language Server** y no requieren snippets.

## 📖 ¿Qué son los Snippets?

Los **snippets** (fragmentos de código) son plantillas predefinidas que se autocompletan al escribir un prefijo corto. Aceleran la escritura de código repetitivo y ayudan a recordar sintaxis.

**Ejemplo:**

```text
1. Escribe: loop:mientras
2. Presiona: Tab o Enter
3. Se expande a:
   mientras [cursor aquí]
       
   fin
```

**💡 Navegación entre Placeholders:**

- `Tab` - Saltar al siguiente placeholder
- `Shift+Tab` - Regresar al placeholder anterior
- `Esc` - Salir del modo snippet

## 🗂️ Organización por Categorías

Los snippets usan prefijos jerárquicos para agruparse por funcionalidad:

| 👁️ | Categoría | Prefijo | Cantidad | Descripción |
| :--- | :--- | :--- | :--- | :--- |
| 📦 | **Condicionales** | `cond:*` | 6 | Snippets de condicionales |
| 🔧 | **Bucles** | `loop:*` | 10 | Snippets de bucles |
| ⚙️ | **Funciones** | `fn:*` | 4 | Snippets de funciones |
| 🎯 | **POO** | `poo:*` | 4 | Snippets de poo |
| ⚠️ | **Manejo de Errores** | `err:*` | 3 | Snippets de manejo de errores |
| 📦 | **Estructuras de Datos** | `data:*` | 1 | Snippets de estructuras de datos |
| 🔧 | **Funciones Avanzadas** | `adv:*` | 4 | Snippets de funciones avanzadas |
| 💻 | **Entrada/Salida** | `io:*` | 5 | Snippets de entrada/salida |
| 📂 | **Archivos** | `file:*` | 3 | Snippets de archivos |
| 🔧 | **Utilidades** | `util:*` | 4 | Snippets de utilidades |
| 🌐 | **Web** | `web:*` | 2 | Snippets de web |
| 📝 | **Plantillas de Programa** | `tpl:*` | 4 | Snippets de plantillas de programa |
| ⚡ | **Operadores Especiales** | `op:*` | 5 | Snippets de operadores especiales |
| 🔍 | **Sintaxis Alternativa** | `syntax:*` | 12 | Snippets de sintaxis alternativa |

**Total:** 67 snippets de alto valor

## 📦 Condicionales (`cond:*`)

### `cond:si` - Condicional Simple

```latino
si condicion

fin
```

### `cond:sino` - Condicional Si-Sino

```latino
si condicion

sino

fin
```

### `cond:osi` - Condicional Si-Osi-Sino

```latino
si condicion1

osi condicion2

sino

fin
```

### `cond:ternario` - Operador Ternario

```latino
condicion ? si_verdadero : si_falso
```

### `cond:elegir` - Estructura Elegir (Switch)

```latino
elegir (variable)
   caso valor1:

   caso valor2:

   defecto:

fin
```

### `cond:caso` - Caso para Elegir

```latino
caso valor:
```

## 🔧 Bucles (`loop:*`)

### `loop:mientras` - Bucle Mientras

```latino
mientras condicion

fin
```

### `loop:repetir` - Bucle Repetir-Hasta

```latino
repetir

hasta condicion
```

### `loop:desde` - Bucle Desde (For C-Style)

```latino
desde (i = 0; i < 10; i = i + 1)

fin
```

### `loop:para` - Bucle Para (Foreach)

```latino
para elemento en coleccion

fin
```

> **⚠️ Placeholders: el nombre por defecto es `coleccion`, no `lista`.**
> Este archivo refleja lo que la extensión de VS Code emite realmente. Los fragmentos de recorrido de colecciones usan `${2:coleccion}` como marcador de posición precisamente para **no** ocultar la librería `lista` de la librería estándar: una variable llamada `lista` lo eclipsa y produce `No existe el método lista.longitud()` al usarla.
>
> No cambies `coleccion` por `lista` al copiar el fragmento: perderías la librería `lista` para todo el resto del script. Nombres equivalentes y seguros: `datos`, `elementos`, `valores`.

### `loop:indice` - Bucle Para con Índice

```latino
desde (i = 0; i < lista.longitud(coleccion); i++)
    elemento = coleccion[i]

fin
```

### `loop:infinito` - Bucle Infinito

```latino
mientras verdadero

    # Usa 'romper' para salir
fin
```

### `loop:rango` - Bucle Para-Rango (0 a N)

```latino
para i en rango(10)
    
fin
```

### `loop:rango_if` - Bucle Para-Rango (Inicio-Fin)

```latino
para i en rango(inicio, fin)
    
fin
```

### `loop:rango_paso` - Bucle Para-Rango (con Paso)

```latino
para i en rango(inicio, fin, paso)
    
fin
```

### `loop:caracteres` - Bucle Para-Caracteres

```latino
para caracter en rango(texto)
    
fin
```

## ⚙️ Funciones (`fn:*`)

### `fn:simple` - Función Simple

```latino
funcion nombre(params)
    
fin
```

### `fn:retornar` - Función con Retorno

```latino
funcion nombre(params)
    
    retornar resultado
fin
```

### `fn:main` - Función Main

```latino
funcion principal()
    
fin

principal()
```

### `fn:anonima` - Función Anónima

```latino
mi_funcion = funcion(params)
    retornar params
fin
```

## 🎯 POO (`poo:*`)

### `poo:clase` - Clase Básica

```latino
clase Nombre
    constructor(params)
        mi.propiedad = propiedad
        
    fin
fin
```

### `poo:clase_base` - Clase Base (Heredable)

```latino
clase NombreBase
    # Regla: Las clases base usan parámetros con valores por defecto
    constructor(param = nulo)
        mi.param = param
        
    fin
fin
```

### `poo:herencia` - Clase con Herencia

```latino
clase Hija es Padre
    constructor(params)
        
    fin
fin
```

### `poo:metodo` - Método de Clase

```latino
funcion nombre(params)
    
fin
```

## 🔧 Manejo de Errores (`err:*`)

### `err:try` - Intentar-Atrapar

```latino
intentar
    
atrapar (error)
    
fin
```

### `err:validar` - Intentar-Atrapar con Validación

```latino
intentar
    resultado = operacion_peligrosa
    
atrapar (error)
    escribir("Error: " .. error)
    
fin
```

### `err:lanzar` - Lanzar Error

```latino
lanzar("Mensaje de error")
```

## 📦 Estructuras de Datos (`data:*`)

### `data:dic_vals` - Diccionario con Valores

```latino
diccionario = {
    "clave1": valor1,
    "clave2": valor2
}
```

## 🔧 Funciones Avanzadas (`adv:*`)

### `adv:mapear` - Mapear Lista

```latino
resultado = lista.transformar(coleccion, funcion(x)
    retornar x * 2
fin)
```

### `adv:filtrar` - Filtrar Lista

```latino
resultado = lista.filtrar(coleccion, funcion(x)
    retornar x > 0
fin)
```

### `adv:reducir` - Reducir Lista

```latino
resultado = lista.reducir(coleccion, funcion(acum, x)
    retornar acum + x
fin, 0)
```

### `adv:dic_mapear` - Mapear Diccionario

```latino
resultado = dic.mapear(diccionario, funcion(llave, valor)
    retornar {"llave": llave, "valor": valor * 2}
fin)
```

## 💻 Entrada/Salida (`io:*`)

### `io:escribirf` - Escribir con Formato .NET

```latino
escribirf("{0}\n", valor)
```

### `io:imprimirf` - Imprimir con Formato C

```latino
imprimirf("Valor: %.2f\n", numero)
```

### `io:compositor` - Compositor

```latino
ç("Texto |variable| masTexto")
```

### `io:leer_num` - Leer Número

```latino
numero = anumero(leer("Ingrese número: "))
```

### `io:leer_validar` - Leer con Validación

```latino
entrada = ""
repetir
    entrada = leer("Mensaje: ")
    si validar.numero(entrada)
        romper
    fin
    escribir("Entrada inválida, intente de nuevo")
hasta falso
valor = anumero(entrada)
```

## 📂 Archivos (`file:*`)

### `file:leer_seguro` - Leer Archivo Seguro

```latino
intentar
    contenido = archivo.leer("archivo.txt")
    
atrapar (error)
    escribir("Error al leer archivo: " .. error)
fin
```

### `file:json_leer` - Leer JSON

```latino
intentar
    datos = archivo.leerJson("datos.json")
    
atrapar (error)
    escribir("Error al leer JSON: " .. error)
fin
```

### `file:procesar_lineas` - Procesar Líneas de Archivo

```latino
lineas = archivo.lineas("archivo.txt")
para linea en lineas
    
fin
```

## 🔧 Utilidades (`util:*`)

### `util:validar` - Validar Tipo

```latino
si tipo(variable) != "decimal"
    escribir("Error: Se esperaba decimal")
    retornar
fin
```

### `util:cronometro` - Cronómetro

```latino
inicio = sis.milisegundos()

fin = sis.milisegundos()
escribir("Tiempo: " .. (fin - inicio) .. "ms")
```

### `util:validar_email` - Validar Email

```latino
si validar.email(correo)
    
sino
    escribir("Email inválido")
fin
```

### `util:validar_pass` - Validar Contraseña Segura

```latino
si validar.contrasenaSegura(password)
    
sino
    escribir("Contraseña débil: mínimo 8 caracteres, mayúsculas, minúsculas y números")
fin
```

## 🌐 Web (`web:*`)

### `web:get` - Petición GET

```latino
intentar
    respuesta = web.obtener("https://api.ejemplo.com/datos")
    datos = respuesta["json"]  # JSON parseado automáticamente
    
atrapar (error)
    escribir("Error en petición: " .. error)
fin
```

### `web:descargar` - Descargar Archivo

```latino
intentar
    web.descargar("https://ejemplo.com/archivo.pdf", "descarga.pdf")
    escribir("Descarga completada")
    archivo.borrar("descarga.pdf")   # Higiene: no dejar artefactos en el disco
atrapar (error)
    escribir("Error al descargar: " .. error)
fin
```

## 📝 Plantillas de Programa (`tpl:*`)

### `tpl:basico` - Programa Básico

```latino
# Nombre del programa
# Autor: Tu nombre
# Fecha: ${CURRENT_YEAR}-${CURRENT_MONTH}-${CURRENT_DATE}

escribir("¡Hola, Latino.NET!")
```

### `tpl:main` - Script con Main

```latino
# Título
# Autor: Tu nombre

funcion principal()
    
fin

# Ejecutar
principal()
```

### `tpl:cli` - CLI con Argumentos

```latino
# Script CLI con argumentos

args = sis.argumentos()

si lista.longitud(args) < 1
    escribir("Uso: latino script.lat <argumento>")
    sis.salir()
fin

entrada = args[0]
```

### `tpl:robusto` - Script con Manejo de Errores Global

```latino
# Título del Script
# Autor: Tu nombre

funcion principal()
    intentar
        
    atrapar (error)
        escribir("Error fatal: " .. error)
        # v1.0.81: sis.salir()
        sis.salir()
    fin
fin

principal()
```

## ⚡ Operadores Especiales (`op:*`)

### `op:and` - AND Acentuado (ý)

```latino
ý
```

### `op:or` - OR Acentuado (ó)

```latino
ó
```

### `op:regex` - Regex Match

```latino
texto ~= "patron"
```

### `op:inc` - Incremento

```latino
variable++
```

### `op:dec` - Decremento

```latino
variable--
```

## 🔍 Sintaxis Alternativa (`syntax:*`)

### `syntax:lambda` - Lambda Corta (One-liner)

```latino
fun(x) ret x * 2 fin
```

### `syntax:dic_alineado` - Diccionario Alineado

```latino
configuracion = {
    "host"    : "localhost",
    "puerto"  : 8080,
    "debug"   : cierto
}
```

### `syntax:matriz` - Matriz 3x3 (Lista Anidada)

```latino
matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
]
```

### `syntax:pipeline` - Pipeline de Datos

```latino
# Opción 1: Pipeline encadenado (pipe operator)
resultado = datos
    |> funcion_1
    |> funcion_2
    |> funcion_3

# Opción 2: Transformación con lambda (equivalente funcional)
resultado = lista.transformar(datos, funcion(x)
    retornar x * 2
fin)
```

### `syntax:fusion_nula` - Asignación Nula (Fusión)

```latino
variable ??= valor_defecto
```

### `syntax:omni` - Omni-Solver (Variables Conocidas)

```latino
resultado = modulo.funcion({
    "var1": valor1,
    "var2": valor2
})
escribir(resultado["resultado"] .. " " .. resultado["unidad"])
```

### `syntax:desestructurar` - Desestructurar Diccionario de Resultado

```latino
r = funcion(args)
valor  = r["resultado"]
unidad = r["unidad"]
extra  = r["clave_extra"]
```

### `syntax:fabrica` - Fábrica de Funciones (Clausura)

```latino
funcion HacerTransformador(config)
    retornar funcion(dato)
        retornar dato * config
    fin
fin

transformador = HacerTransformador(valor_config)
```

### `syntax:enumerar` - Iterar con Índice y Valor

```latino
desde (i = 0; i < lista.longitud(coleccion); i++)
    elemento = coleccion[i]
    # lógica
fin
```

### `syntax:recursion` - Función Recursiva

```latino
funcion Resolver(n)
    # Caso base
    si n <= 1
        retornar 1
    fin

    # Caso recursivo
    retornar n * Resolver(n - 1)
fin
```

### `syntax:acumulador` - Acumulador Seguro en Reducción

```latino
resultado = lista.reducir(coleccion, funcion(acum, item)
    clave = item["campo_agrupador"]
    acum[clave] = (acum[clave] ?? 0) + item["campo_valor"]
    retornar acum
fin, {})
```

### `syntax:desempacar` - Multiasignación desde Lista

```latino
partes   = lista_origen
primero  = partes[0]
segundo  = partes[1]
tercero  = partes[2]
```

## 📝 Tips de Uso

### 1️⃣ Explorar Snippets por Categoría

```text
loop:   → Muestra todos los bucles
time:   → Muestra funciones de fechas
web:    → Muestra peticiones HTTP
adv:    → Muestra funciones de orden superior
```

### 2️⃣ Atajos Rápidos

- `yy` → `ý` (AND)
- `oo` → `ó` (OR)
- `cc` → Compositor `ç()`

### 3️⃣ Usar Autocompletado del LSP para Funciones

```text
cadena.   → Todas las funciones de cadenas
mate.     → Todas las funciones matemáticas
lista.    → Todas las funciones de listas
lapsos.   → Todas las funciones de fechas
validar.  → Todas las validaciones
```

### 4️⃣ Conversiones de Unidades

En lugar de snippets obsoletos, usar:

```latino
# Grados a radianes
radianes = mate.convertir(45, "grados", "radianes")

# Temperatura
fahrenheit = mate.convertir(25, "celsius", "fahrenheit")

# Longitud
metros = mate.convertir(1, "millas", "m")   # ⚠️ las unidades van en PLURAL: "millas" sí, "milla" no
```

## 📊 Estadísticas

- **Total de Snippets:** 67
- **Categorías:** 14
- **Nuevos en v1.0.81:** 15
- **Eliminados (obsoletos):** 3
- **Snippets de Estructuras:** 25 (37%)
- **Snippets de Funcionalidad:** 42 (63%)
- **Atajos Rápidos:** 3 (`yy`, `oo`, `cc`)

## 🎯 Filosofía de Diseño

Los snippets en Latino.NET siguen estos principios:

1. **No duplicar el LSP** - Si una función ya se autocompleta, no necesita snippet
2. **Generar estructuras** - Multi-línea, indentación, placeholders inteligentes
3. **Combinar operaciones** - Patrones comunes, manejo de errores, validaciones
4. **Proveer atajos** - Caracteres especiales, sintaxis compleja
5. **Reflejar funcionalidad actual** - Actualizar con nuevas librerías y plugins

## 🔗 Referencias

- **Documentación Completa:** `docs/`
- **Librería Estándar:** `docs/99_LIBRERIAS_ESTANDAR.md`
- **API Detallada:** `docs/09_Más_Sintaxis_Errores_Lib/`
- **Strings Predefinidos:** `docs/98_Referencia_Strings_Predefinidos.md`

**Última actualización:** Septiembre 2026 | Latino.NET v1.0.81
