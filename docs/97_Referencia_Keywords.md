# Referencia de Palabras Reservadas

Este documento lista todas las palabras que tienen un significado especial en Latino.NET.

**⚠️ REGLA DE ORO:** Nunca uses estas palabras como nombres de variables, funciones o identificadores.

## 📌 Nivel 1: Palabras Estrictamente Reservadas

**Causan ERROR DE SINTAXIS inmediato.** El programa arrojará error de sintaxis.

### Control de Flujo

| Palabra | Categoría |
| :--- | :--- |
| `si`, `sino`, `osi` | Condicionales |
| `mientras`, `repetir`, `hasta` | Bucles indeterminados |
| `desde`, `para`, `en` | Bucles determinados |
| `elegir`, `caso`, `otro`, `defecto` | Selección múltiple |
| `fin` | Cierre de bloques |
| `romper`, `continuar` | Control de bucles |
| `ir` | Salto incondicional (GoTo) |

### Funciones y Clases

| Palabra | Categoría |
| :--- | :--- |
| `funcion`, `fun` | Definición de funciones |
| `retornar`, `retorno`, `ret` | Retorno de valores |
| `clase`, `es` | Programación orientada a objetos |
| `constructor` | Inicializador de clases |
| `crear`, `nueva`, `nuevo` | Instanciación de objetos |
| `super` | ⚠️ **Reservada** — Acceso a superclase (implementación pendiente v2.0) |
| `estatico` | ⚠️ **Reservada** — Miembro de clase (implementación pendiente v2.0) |

### Manejo de Errores y Alcance

| Palabra | Categoría |
| :--- | :--- |
| `intentar`, `atrapar` | Manejo de excepciones |
| `local`, `global` | Alcance de variables |

## 🔧 Nivel 2: Operadores Literales

**Se comportan como símbolos matemáticos.** Usarlos como variables es un error grave.

| Palabra | Equivalente | Advertencia |
| :--- | :--- | :--- |
| `ý` | `&&` | Operador lógico AND (acentuado) |
| `ó` | `\|\|` | Operador lógico OR (acentuado) |
| `no` | `!` | Operador lógico NOT |

## 📌 Nivel 3: Literales de Valor

**Constantes del sistema.** No se pueden reasignar.

| Palabra | Tipo | Valor |
| :--- | :--- | :--- |
| `verdadero`, `cierto` | Booleano | `true` |
| `falso`, `espurio` | Booleano | `false` |
| `nulo` | Null | `null` |
| `mi`, `este`, `esta` | Referencia | `this` (solo lectura) |

> **⚠️ Los alias son de entrada, no de salida.** Puedes **escribir** cualquiera de las dos formas y el motor la acepta:
>
> ```latino
> escribir(verdadero)   # acepta el alias de entrada
> escribir(cierto)     # acepta la forma canónica
> ```
>
> Pero **el motor siempre responde con `cierto` y `falso`**, nunca con `verdadero` ni con `espurio`. Verificado en v1.0.81:
>
> ```latino
> a = 5 > 3
> escribir(a)          # → cierto    (nunca "verdadero")
> b = 5 < 3
> escribir(b)          # → falso     (nunca "espurio")
> espurio == falso     # → cierto    (el alias vale lo mismo)
> ```
>
> Por eso, en los ejemplos del manual la salida esperada de una condición se documenta siempre como `cierto` / `falso`, mientras que el código de entrada puede usar `verdadero` indistintamente.

## ⚡ Nivel 4: Funciones Globales

```text
acadena, alogico, anumero, ç, clonar, escribir, escribirf, eval, evaluar, imprimir, imprimirf, incluir, lanzar, leer, limpiar, pedir, poner, ponerf, rango, tipo, trocar
```

## 📚 Nivel 5: Librerías y Plugins

### Librerías del Núcleo (siempre disponibles)

```text
archivo, cadena, cola, conjunto, dic, lista, mate, pila, sis, tupla, validar, web
```

### Plugins (requieren `incluir("nombre")`)

```text
cripto, env, formulas, hilos, lapsos, log, proceso, sqlite, tabla, texto, zip
```

## 📝 Convenciones Recomendadas

| ❌ EVITAR | ✅ USAR EN SU LUGAR |
| :--- | :--- |
| `lista = [1, 2, 3]` | `miLista = [1, 2, 3]` |
| `cadena = "hola"` | `texto = "hola"` |
| `ç = "test"` | `resultado = "test"` |
| `tipo = 5` | `tipoElemento = 5` |
| `mate = 100` | `puntaje = 100` |
| `archivo = "data"` | `nombreArchivo = "data"` |
| `lapsos = 30` | `duracion = 30` |

## 🎯 Estilo de Código Recomendado

```latino
# ✅ CORRECTO - Nombres descriptivos
miLista = [1, 2, 3, 4, 5]
textoUsuario = leer("Ingresa tu nombre: ")
X = 100  # Coordenada (mayúscula)
Y = 200  # Coordenada (mayúscula)
y = 10   # Identificador válido
o = 5    # Identificador válido

# ❌ INCORRECTO - Sombreado de bibliotecas globales
lista = [1, 2, 3]  # Oculta la biblioteca lista.*
cadena = "hola"    # Oculta la biblioteca cadena.*
```

## 📖 Notas Técnicas

1. **Palabras Nivel 1-3:** Error de sintaxis garantizado
2. **Palabras Nivel 4-5:** Sombreado de nombres (shadowing)
   - El identificador local oculta el global
   - La funcionalidad global queda inaccesible en ese scope
   - No causa error, pero rompe la lógica del programa

3. **Case Sensitivity:** Latino.NET distingue mayúsculas/minúsculas
   - `ý` (operador) ≠ `y` (identificador válido)
   - `lista` (librería) ≠ `Lista` (identificador válido)

## 🔍 Tabla de Consulta Rápida

### ¿Puedo usar este nombre?

| Nombre | ¿Permitido? | Consecuencia |
| :--- | :--- | :--- |
| `contador` | ✅ Sí | Seguro |
| `resultado` | ✅ Sí | Seguro |
| `si` | ❌ No | Error de sintaxis |
| `funcion` | ❌ No | Error de sintaxis |
| `y` | ✅ Sí | Identificador seguro |
| `o` | ✅ Sí | Identificador seguro |
| `ý` | ❌ No | Operador lógico |
| `ó` | ❌ No | Operador lógico |
| `verdadero` | ❌ No | Literal constante |
| `escribir` | ⚠️ Técnicamente sí | Pierdes la función global |
| `lista` | ⚠️ Técnicamente sí | Pierdes la librería |
| `Y` | ✅ Sí | Válido (mayúscula) |
| `Lista` | ✅ Sí | Válido (mayúscula) |

**💡 Consejo Final:** Cuando dudes, usa nombres descriptivos en español sin abreviar (`numeroTotal`, `listaNombres`, `contadorVotos`). Es mejor ser explícito que causar conflictos.
