# CAPÍTULO 2: Control de Flujo

## El Cerebro del Programa

### ¿Por Qué Necesitas Control de Flujo?

Imagina que estás dirigiendo el tráfico en una intersección de alta congestión. No puedes limitarte a gritar a todos los conductores: *"¡Avancen todos a la vez!"*. Un sistema seguro y funcional requiere:

1. **Tomar decisiones (Bifurcaciones)** → *"Si el semáforo está en rojo, el tráfico se detiene; si está en verde, avanza."*
2. **Repetir acciones (Iteraciones)** → *"Mientras haya vehículos esperando en la fila, mantén abierta la vía."*
3. **Alterar la marcha ante imprevistos (Interrupciones)** → *"Si se aproxima una ambulancia con sirena activa, detén todo de inmediato y cede el paso prioritario."*

**Un programa de computadora funciona exactamente igual.** Sin estructuras de control de flujo, el intérprete se comporta como un autómata rígido: lee y ejecuta las instrucciones de forma inflexible de arriba hacia abajo, una tras otra, completamente ciego a las circunstancias del entorno y a la validez de los datos:

```latino
# ❌ Código estrictamente lineal (ejecución ciega)
cliente = "Carlos"
saldoDisponible = 50
montoRetiro = 120

# El programa ejecuta el descuento sin comprobar si los fondos existen:
saldoDisponible = saldoDisponible - montoRetiro

escribir("Transacción autorizada para: " .. cliente)
escribir("Dispensando efectivo: $" .. montoRetiro)
escribir("Saldo final en cuenta: $" .. saldoDisponible)
```

Si ejecutas este fragmento, el intérprete entregará alegremente $120 en efectivo y reportará un saldo absurdo de `$-70`. No ocurrió ningún error de sintaxis ni falló el motor de .NET, pero el resultado es inaceptable para la lógica del negocio.

**Las deficiencias del modelo puramente lineal:**

- **Falta de discernimiento:** ¿Qué ocurre si los fondos son insuficientes? El script no tiene la capacidad de evaluar la situación ni de negarse a dispensar el dinero.
- **Incapacidad de respuesta:** ¿Qué sucede si el usuario teclea un monto negativo o un valor no numérico? El cálculo matemático colapsa o corrompe el balance.
- **Falta de persistencia:** ¿Cómo procesamos a diez clientes consecutivos en el cajero sin copiar y pegar el mismo bloque de código diez veces?

### El Salto a la Inteligencia del Código

El control de flujo es el componente que dota de **criterio y dinamismo** al software. Permite que el intérprete examine el estado de las variables en tiempo real, elija caminos alternativos ante diferentes escenarios, repita tareas pesadas en fracciones de segundo y detenga operaciones peligrosas antes de que afecten al sistema.

A lo largo de este capítulo, aprenderás a gobernar la ejecución de tus programas en Latino.NET dominando tres pilares: **decisiones condicionales** (`si`, `osi`, `sino`), **estructuras de selección múltiple** (`elegir`, `caso`) y **bucles optimizados** (`para`, `mientras`, `repetir`).

El control de flujo se divide en 4 grandes familias:

| Categoría | Propósito | Estructuras |
| :--- | :--- | :--- |
| **🔀 Decisiones** | Ejecutar código condicionalmente | `si`, `osi`, `sino`, `?:` |
| **🔄 Bucles** | Repetir código múltiples veces | `mientras`, `repetir`, `desde`, `para` |
| **⚡ Saltos** | Alterar el flujo de bucles | `romper`, `continuar`, `ir` |
| **🎯 Selección** | Comparar contra múltiples valores | `elegir` |

## 2.1 Fundamentos de Decisiones

### La Estructura `si` - Tu Primera Decisión

La estructura `si` es la **más básica** del control de flujo. Ejecuta un bloque de código solo cuando una condición es verdadera.

**Sintaxis:**

```latino
si condicion
    # Código que se ejecuta si la condición es verdadera
fin
```

🎯 **Ejemplo: Condición de Mayoría de Edad**

```latino
edad = 20

si edad >= 18
    escribir("✅ Eres mayor de edad")
fin

escribir("Programa terminado")
```

**Salida:**

```text
✅ Eres mayor de edad
Programa terminado
```

**¿Qué pasó aquí?**

1. El programa evalúa `edad >= 18` → `20 >= 18` → `verdadero`
2. Como la condición es verdadera, ejecuta `escribir("✅ Eres mayor de edad")`
3. Continúa con el resto del código

### Agregando Alternativas: `sino`

El bloque `sino` se ejecuta **solo** cuando la condición del `si` es falsa.

```latino
temperatura = 15

si temperatura > 30
    escribir("🔥 Hace mucho calor")
sino
    escribir("❄️ El clima es fresco")
fin
```

### Múltiples Condiciones: `osi`

`osi` (contracción de "o si") te permite evaluar **condiciones adicionales** cuando la primera falla.

**⚠️ REGLA CRÍTICA:** Las condiciones se evalúan **en orden descendente**. Cuando una es verdadera, se ejecuta su bloque y **se ignoran todas las demás**, incluso si también serían verdaderas.

```latino
nota = 7.5

si nota >= 9.0
    escribir("📚 Sobresaliente")
osi nota >= 7.0    # Solo se evalúa si nota < 9.0
    escribir("📗 Notable")
osi nota >= 5.0    # Solo se evalúa si nota < 7.0
    escribir("📘 Aprobado")
sino               # Solo se ejecuta si todas las anteriores son falsas
    escribir("📕 Suspendido")
fin
```

**Salida:** `📗 Notable`

### 📝 Error Común: Orden Incorrecto

**Problema:** Si pones las condiciones en el orden equivocado, obtienes resultados incorrectos.

```latino
# ❌ INCORRECTO
nota = 8.5

si nota >= 5.0          # Esto es verdadero para 8.5
    escribir("Aprobado") # Se ejecuta esto
osi nota >= 7.0         # ¡NUNCA se evalúa!
    escribir("Notable")
osi nota >= 9.0
    escribir("Sobresaliente")
fin

# Salida incorrecta: "Aprobado" (debería ser "Notable")
```

**Razón:** Como `8.5 >= 5.0` es verdadero, se ejecuta el primer bloque y el programa **salta** el resto de condiciones.

**✅ SOLUCIÓN:** Siempre ordena de **más restrictiva a menos restrictiva**:

```latino
# ✅ CORRECTO
nota = 8.5

si nota >= 9.0      # Más restrictiva primero
    escribir("Sobresaliente")
osi nota >= 7.0     # Luego menos restrictiva
    escribir("Notable")
osi nota >= 5.0     # La más amplia al final
    escribir("Aprobado")
sino
    escribir("Suspendido")
fin
```

## Operadores de Comparación

Para escribir condiciones efectivas, necesitas dominar los operadores de comparación:

| Operador | Significado | Ejemplo | Resultado |
| :--- | :--- | :--- | :--- |
| `==` | Igual a | `5 == 5` | cierto |
| `!=` | Diferente de | `5 != 3` | cierto |
| `>` | Mayor que | `7 > 3` | cierto |
| `<` | Menor que | `3 < 7` | cierto |
| `>=` | Mayor o igual | `5 >= 5` | cierto |
| `<=` | Menor o igual | `3 <= 5` | cierto |

### Operadores Lógicos

Combina múltiples condiciones:

| Operador literal | Símbolo alternativo | Significado | Ejemplo | Resultado |
| :--- | :--- | :--- | :--- | :--- |
| `ý` (AND) | `&&` | Ambas verdaderas | `verdadero ý verdadero` | cierto |
| `ó` (OR) | `\|\|` | Al menos una verdadera | `verdadero ó falso` | cierto |
| `no` (NOT) | `!` | Invierte el valor | `!verdadero` | falso |

**Ejemplo combinado:**

```latino
edad = 25
tieneLicencia = verdadero

si edad >= 18 ý tieneLicencia
    escribir("✅ Puedes conducir")
sino
    escribir("❌ No puedes conducir")
fin
```

## Condiciones Anidadas

Puedes colocar estructuras `si` dentro de otras para lógica más compleja.

```latino
edad = 16
tienePermiso = verdadero

si edad >= 18
    escribir("✅ Mayor de edad, puede entrar")
sino
    # Anidamos otra decisión
    si tienePermiso
        escribir("⚠️ Menor de edad, pero tiene permiso parental")
    sino
        escribir("❌ No puede entrar")
    fin
fin
```

**⚠️ Advertencia:** Más de 3 niveles de anidación hace el código difícil de leer. Considera refactorizar:

```latino
# ❌ Difícil de leer (4 niveles)
si edad >= 18
    si tieneDinero
        si hayBoletos
            si noEstaLleno
                escribir("Puedes comprar")
            fin
        fin
    fin
fin

# ✅ Más legible (condiciones combinadas)
puedeComprar = edad >= 18 ý tieneDinero ý hayBoletos ý !estaLleno

si puedeComprar
    escribir("Puedes comprar")
sino
    escribir("No puedes comprar")
fin
```

## El Operador Ternario `?:`

También conocido como operador condicional, es una forma **compacta** de escribir `si/sino` que retorna un valor.

**Sintaxis:**

```latino
variable = condicion ? valor_si_verdadero : valor_si_falso
```

🎯 **Ejemplo: Operador Ternario como Expresión**

```latino
edad = 20
estado = edad >= 18 ? "Adulto" : "Menor"
escribir(estado)  # Salida: Adulto
```

**Equivalente con `si`:**

```latino
edad = 20
estado = ""

si edad >= 18
    estado = "Adulto"
sino
    estado = "Menor"
fin

escribir(estado)
```

### Cuándo Usar el Operador Ternario

**✅ Usar cuando:**

- La lógica es simple (una condición)
- Asignas un valor basado en la condición
- Quieres código compacto y legible

**❌ NO usar cuando:**

- Hay múltiples condiciones anidadas
- La lógica es compleja
- Afecta la legibilidad

**Ejemplo válido:**

```latino
# ✅ Simple y claro
esMiembro = cierto
descuento = esMiembro ? 0.10 : 0

# ✅ En contexto de función
funcion ObtenerSaludo(esManana)
    retornar esManana ? "Buenos días" : "Buenas tardes"
fin
```

Un patrón de diseño perfectamente válido y legible es la **Expresión Switch:**

```latino
# ✅ Expresión Switch (anidado vertical)
edad = 20
categoria = edad >= 65 ? "Senior" : 
            edad >= 18 ? "Adulto" : 
            edad >= 13 ? "Adolescente" : "Niño"

# ✅ Equivalente con si/osi
si edad >= 65
    categoria = "Senior"
osi edad >= 18
    categoria = "Adulto"
osi edad >= 13
    categoria = "Adolescente"
sino
    categoria = "Niño"
fin
```

**Ejemplos de abuso:**

 1. "El Espagueti Horizontal"
El pecado aquí no es solo el anidamiento, sino la falta de formato y la mezcla de condiciones binarias (si/no) con condiciones de estado.

    ```latino
    # ❌ Imposible saber qué ':' cierra qué '?' de un vistazo
    estado = usuario == nulo ? "Anonimo" : usuario.activo ? (usuario.rol == "admin" ? "SuperUser" : (saldo > 0 ? "Premium" : "Standard")) : "Baneado"
    ```

    *¿Qué pasa si el usuario es activo pero no es admin y su saldo es 0? Tienes que recorrer la línea con el dedo en la pantalla para saberlo.*

 2. "La Ensalada Lógica" (Condiciones Pesadas)
El abuso ocurre cuando la *condición* es tan larga que cuando llegas al `?`, ya olvidaste qué estabas evaluando.

    ```latino
    # ❌ La condición es más grande que el resultado
    accion = (archivo.existe("config.ini") && sis.usuario() == "root" || modo_debug == verdadero) ? "Editar" : "Solo Lectura"
    ```

    *Solución:* Esto debería ser un `si` normal o extraer la condición a una variable `puede_editar`.

 3. "Efectos Secundarios Ocultos" (El peor de todos)
El operador ternario debe usarse para **obtener valores**, no para **ejecutar acciones**. Usarlo para controlar el flujo del programa es un anti-patrón grave.

    ```latino
    # ❌ Usar el ternario como un 'si' comprimido para ejecutar funciones
    dummy = (intentos < 3) ? (clave == "1234" ? AbrirPuerta() : SonarAlarma()) : BloquearSistema()
    ```

    *Por qué es malo:* Estás asignando el resultado a `dummy` (basura) solo para ahorrarte escribir `si/sino`. Esto es confuso porque el ternario sugiere que estamos calculando un dato, no abriendo puertas o bloqueando sistemas.

## Ejercicio Guiado: Calculadora de Descuentos

Vamos a crear un sistema que calcule descuentos basados en múltiples condiciones.

**Requisitos:**

1. Clientes VIP obtienen 20% de descuento
2. Compras mayores a $100 obtienen 10% adicional
3. Si es día festivo, 5% adicional
4. El descuento máximo es 35%

```latino
# ═══════════════════════════════════════
#  CALCULADORA DE DESCUENTOS
# ═══════════════════════════════════════

funcion CalcularDescuento(precio, esVIP, esFestivo)
    descuento = 0
    
    # 1. Descuento base por VIP
    si esVIP
        descuento = descuento + 0.20
        escribir("✅ Descuento VIP: 20%")
    fin
    
    # 2. Descuento por monto
    si precio > 100
        descuento = descuento + 0.10
        escribir("✅ Descuento por compra mayor: 10%")
    fin
    
    # 3. Descuento por festivo
    si esFestivo
        descuento = descuento + 0.05
        escribir("✅ Descuento festivo: 5%")
    fin
    
    # 4. Aplicar tope máximo
    si descuento >= 0.35
        descuento = 0.35
        escribir("⚠️  Descuento limitado al máximo (35%)")
    fin
    
    retornar descuento
fin

funcion MostrarResumen(precio, descuento)
    montoDescuento = precio * descuento
    precioFinal = precio - montoDescuento
    
    escribir("╔════════════════════════════════╗")
    escribir("║       RESUMEN DE COMPRA        ║")
    escribir("╠════════════════════════════════╣")
    escribirf("║ Subtotal:     {0,15:C}  ║\n", precio)
    escribirf("║ Descuento:    {0,15:C}  ║\n", montoDescuento * -1)
    escribirf("║ {0,-30} ║\n", cadena.formato("(%.0f%% de descuento)", descuento * 100))
    escribir("╠════════════════════════════════╣")
    escribirf("║ TOTAL:        {0,15:C}  ║\n", precioFinal)
    escribir("╚════════════════════════════════╝")
fin

# ═══════════════════════════════════════
#  CASOS DE PRUEBA
# ═══════════════════════════════════════

escribir("=== CASO 1: Cliente Regular, $50 ===")
desc1 = CalcularDescuento(50, falso, falso)
MostrarResumen(50, desc1)

escribir("\n=== CASO 2: Cliente VIP, $150, Día Festivo ===")
desc2 = CalcularDescuento(150, verdadero, verdadero)
MostrarResumen(150, desc2)
```

**Salida esperada:**

```text
=== CASO 1: Cliente Regular, $50 ===

╔════════════════════════════════╗
║       RESUMEN DE COMPRA        ║
╠════════════════════════════════╣
║ Subtotal:              $50.00  ║
║ Descuento:              $0.00  ║
║ (0% de descuento)              ║
╠════════════════════════════════╣
║ TOTAL:                 $50.00  ║
╚════════════════════════════════╝

=== CASO 2: Cliente VIP, $150, Día Festivo ===
✅ Descuento VIP: 20%
✅ Descuento por compra mayor: 10%
✅ Descuento festivo: 5%
⚠️  Descuento limitado al máximo (35%)

╔════════════════════════════════╗
║       RESUMEN DE COMPRA        ║
╠════════════════════════════════╣
║ Subtotal:             $150.00  ║
║ Descuento:           ($52.50)  ║
║ (35% de descuento)             ║
╠════════════════════════════════╣
║ TOTAL:                 $97.50  ║
╚════════════════════════════════╝
```

## Anti-Patrones Comunes

### 1. Condiciones Redundantes

```latino
# ❌ MAL: Repetición innecesaria
si edad >= 18
    si edad >= 18  # ¡Ya lo verificaste!
        escribir("Mayor de edad")
    fin
fin

# ✅ BIEN
si edad >= 18
    escribir("Mayor de edad")
fin
```

### 2. Comparación con Booleanos

```latino
# ❌ MAL: Redundante
si esActivo == verdadero
    escribir("Activo")
fin

# ✅ BIEN: Usar directamente
si esActivo
    escribir("Activo")
fin

# ✅ BIEN: Negar con !
si !esActivo
    escribir("Inactivo")
fin
```

### 3. Bloques Vacíos

```latino
# ❌ MAL: Bloque sino vacío
si edad < 18
    escribir("Menor")
sino
    # Nada aquí
fin

# ✅ BIEN: Eliminar el sino
si edad < 18
    escribir("Menor")
fin
```

### 4. Retornos Innecesarios

```latino
# ❌ MAL
funcion EsMayor(edad)
    si edad >= 18
        retornar verdadero
    sino
        retornar falso
    fin
fin

# ✅ BIEN: Retornar la condición directamente
funcion EsMayor(edad)
    retornar edad >= 18
fin
```

## 2.2 Selección Múltiple: `elegir`

### El Problema de Muchos `osi`

Imagina que necesitas clasificar días de la semana. Con `si/osi`, el código se vuelve repetitivo:

```latino
# ❌ Funciona, pero es tedioso
dia = "miércoles"

si dia == "lunes"
    escribir("Inicio de semana")
osi dia == "martes"
    escribir("Segundo día")
osi dia == "miércoles"
    escribir("Mitad de semana")
osi dia == "jueves"
    escribir("Casi viernes")
osi dia == "viernes"
    escribir("¡Viernes!")
osi dia == "sábado"
    escribir("Fin de semana")
osi dia == "domingo"
    escribir("Descanso")
sino
    escribir("Día no válido")
fin
```

**Problemas:**

- 7 veces `dia ==` (repetitivo)
- Difícil de leer y mantener
- Fácil cometer errores de tipeo

**Solución:** La estructura `elegir` (conocida como `switch` en otros lenguajes):

```latino
# ✅ Mismo resultado, más limpio
dia = "miércoles"

elegir dia
    caso "lunes":
        escribir("Inicio de semana")
    caso "martes":
        escribir("Segundo día")
    caso "miércoles":
        escribir("Mitad de semana")
    caso "jueves":
        escribir("Casi viernes")
    caso "viernes":
        escribir("¡Viernes!")
    caso "sábado":
        escribir("Fin de semana")
    caso "domingo":
        escribir("Descanso")
    otro:
        escribir("Día no válido")
fin
```

## Sintaxis de `elegir`

```latino
elegir variable_a_evaluar
    caso valor1:
        # Código para valor1
    caso valor2:
        # Código para valor2
    caso valor3:
        # Código para valor3
    otro:
        # Código si ningún caso coincide (opcional)
fin
```

**Características clave:**

1. **Salida automática (Prohibido usar `romper` para cerrar casos)**  
   A diferencia de C, Java o C#, Latino.NET **sale automáticamente** de la estructura `elegir` tan pronto termina el código del caso coincidente.  
   - ⚠️ **No uses `romper` aquí:** La palabra clave `romper` es exclusiva de los bucles (`mientras`, `para`, `desde`, `repetir`). Si usas `romper` dentro de un `elegir`, abortarás el bucle exterior que lo contiene o provocarás un error de runtime.
   - ⚠️ **No hay casos compartidos por cascada:** Dejar un caso vacío seguido de otro (ej: `caso "A": caso "B":`) **no** hace que "A" ejecute el código de "B". El caso "A" se considerará un bloque vacío y saldrá inmediatamente sin hacer nada. Cada caso debe tener su propia lógica explícita.
2. **Los casos SON expresiones dinámicas** - A diferencia de otros lenguajes que exigen constantes, en Latino.NET cada `caso` se evalúa en tiempo de ejecución. ¡Puedes poner sumas, llamadas a funciones o variables ahí dentro!
3. **`otro` (o `defecto`) es opcional** - Si no coincide ningún caso y no hay `otro`, el programa simplemente continúa pacíficamente.
4. **Evaluación estricta (`==`)** - El motor compara el valor de `elegir` con el valor del `caso` usando igualdad estricta.

## Cuándo Usar `elegir` vs `si/osi`

| Criterio | Usar `si/osi` | Usar `elegir` |
| :--- | :--- | :--- |
| **Número de opciones** | 1-3 alternativas | 4+ alternativas |
| **Tipo de condición** | Condiciones no relacionadas | Evaluaciones contra una misma variable origen |
| **Legibilidad** | Lógica ramificada compleja | Mapas de decisión limpios y verticales |

**Ejemplos de Selección:**

```latino
# ✅ BIEN: si/osi para condiciones variadas
edad = 16
tiene_pase_vip = falso

si edad < 13
    categoria = "Niño"
osi tiene_pase_vip == verdadero
    categoria = "VIP"
sino
    categoria = "Adulto Regular"
fin

# ✅ BIEN: elegir para evaluar una misma variable contra múltiples resultados
funcion obtener_mes()
    retornar 2  # Ejemplo: febrero
fin

mes = 2  # Variable de prueba inicializada

elegir (mes)
    caso 1:
        escribir("Enero")
    caso 1 + 1:           # ¡Puedes usar matemáticas!
        escribir("Febrero")
    caso obtener_mes():   # ¡Puedes llamar funciones!
        escribir("Mes dinámico")
    otro:
        escribir("Mes no válido")
fin
```

## El Desafío: Múltiples Valores en un Caso

Latino.NET (versión actual) **no soporta múltiples valores** en un solo caso:

```latino
# ❌ ESTO NO FUNCIONA
elegir mes
    caso 1, 3, 5, 7, 8, 10, 12:  # Sintaxis no válida
        escribir("31 días")
fin
```

Pero existen **3 soluciones elegantes** para este problema:

## 🎯 Solución 1: El Truco del "Switch Invertido"

En lugar de evaluar la variable, evalúas **`cierto`** y usas condiciones lógicas en los casos.

**Ventaja:** Puedes usar el operador `ó` (o su forma `||`) para múltiples valores.

```latino
mes = 2

elegir verdadero
    caso (mes == 1 ó mes == 3 ó mes == 5 ó mes == 7 ó mes == 8 ó mes == 10 ó mes == 12):
        escribir("📅 Este mes tiene 31 días")
    
    caso (mes == 4 ó mes == 6 ó mes == 9 ó mes == 11):
        escribir("📅 Este mes tiene 30 días")
    
    caso (mes == 2):
        escribir("📅 Febrero tiene 28 o 29 días")
    
    otro:
        escribir("❌ Mes inválido (debe ser 1-12)")
fin
```

**¿Cómo funciona?**

- `elegir verdadero` significa "elige el caso que sea verdadero"
- Cada `caso` evalúa una expresión booleana completa
- El primer caso verdadero se ejecuta

**Cuándo usarla:**

- Necesitas agrupar múltiples valores
- Quieres flexibilidad para condiciones complejas
- No te importa que sea menos obvio que es un "switch"

## 🎯 Solución 2: Diccionarios (Mapeo de Datos)

Si solo necesitas **mapear un valor a otro** (sin lógica compleja), un diccionario es más eficiente.

```latino
mes = 2

# Definir el mapeo como datos
diasPorMes = {
    "1": 31, "3": 31, "5": 31, "7": 31, "8": 31, "10": 31, "12": 31,
    "4": 30, "6": 30, "9": 30, "11": 30,
    "2": "28 o 29"
}

# Convertir a cadena porque las claves son strings
clave = acadena(mes)
dias = dic.obtener(diasPorMes, clave, nulo)

si dias != nulo
    escribir("📅 Este mes tiene " .. dias .. " días")
sino
    escribir("❌ Mes inválido")
fin
```

**Ventajas:**

- Muy eficiente (búsqueda O(1))
- Fácil de modificar (solo cambias el diccionario)
- Separa datos de lógica

**Cuándo usarla:**

- Mapeo simple valor → resultado
- Datos que podrían cambiar frecuentemente
- Necesitas máximo rendimiento

## 🎯 Solución 3: Listas con `contiene`

Usa listas para agrupar valores relacionados y verifica con `lista.contiene()`.

```latino
mes = 2

# Agrupar meses por característica
meses31 = [1, 3, 5, 7, 8, 10, 12]
meses30 = [4, 6, 9, 11]

si lista.contiene(meses31, mes)
    escribir("📅 Este mes tiene 31 días")
osi lista.contiene(meses30, mes)
    escribir("📅 Este mes tiene 30 días")
osi mes == 2
    escribir("📅 Febrero tiene 28 o 29 días")
sino
    escribir("❌ Mes inválido")
fin
```

**Ventajas:**

- Muy legible: "si la lista contiene el mes..."
- Fácil agregar/quitar valores
- Aprovecha funciones integradas

**Cuándo usarla:**

- Agrupaciones lógicas claras
- Valores que podrían expandirse
- Prefieres código autodocumentado

## 🎯 Solución 4: Evaluación Dinámica de Cadenas (`evaluar`)

Dado que Latino.NET evalúa las expresiones de los casos en tiempo real, puedes utilizar la potente función global `evaluar()` (o su alias `eval()`) para inyectar lógica compleja, código almacenado en bases de datos, o expresiones armadas al vuelo como si fueran un caso más.

**¿Cómo funciona?**
`evaluar("codigo")` toma un string, lo parsea y lo ejecuta al instante, retornando su resultado. Si ese resultado coincide con la variable del `elegir`, el caso se activa.

```latino
codigo_secreto = "ALFA-7"
patron_dinamico = "ALFA"

elegir (codigo_secreto)
    caso "BETA-1":
        escribir("Acceso Denegado")
        
    caso evaluar("'" .. patron_dinamico .. "-7'"):
        # El motor concatena la cadena, la evalúa como código Latino
        # y compara el resultado ("ALFA-7") con codigo_secreto.
        escribir("¡Acceso Dinámico Concedido!")
        
    otro:
        escribir("Intruso detectado")
fin
```

**Ventajas:**

- Permite construir los casos de decisión en tiempo de ejecución (ideal para motores de reglas o configuraciones leídas desde un archivo JSON/texto).

**Cuándo usarla:**

- Solo en escenarios de programación avanzada (*Metaprogramación*) donde los valores a comparar no se conocen al escribir el código, sino que se generan dinámicamente.

## Comparativa de las Soluciones para Múltiples Valores

| Aspecto | Switch Invertido | Diccionario | Lista + contiene | Evaluación Dinámica (`eval`) |
| :--- | :--- | :--- | :--- | :--- |
| **Legibilidad** | ⭐⭐⭐☆☆ | ⭐⭐⭐⭐☆ | ⭐⭐⭐⭐⭐ | ⭐⭐☆☆☆ |
| **Rendimiento** | ⭐⭐⭐☆☆ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐☆☆ | ⭐☆☆☆☆ (Lento por análisis en caliente) |
| **Flexibilidad** | ⭐⭐⭐⭐⭐ | ⭐⭐☆☆☆ | ⭐⭐⭐⭐☆ | ⭐⭐⭐⭐⭐ |
| **Caso de Uso** | Lógica múltiple | Mapeo rápido 1:1 | Agrupaciones legibles | Reglas creadas al vuelo (Avanzado) |

**Recomendación:**

- **Mapeo simple**: Diccionario
- **Lógica compleja**: Switch invertido
- **Agrupaciones semánticas**: Lista + contiene

## Proyecto: Sistema de Menú de Restaurante

Vamos a crear un sistema completo de pedidos que use `elegir` de forma profesional.

```latino
# ═══════════════════════════════════════════════════════════
#  SISTEMA DE PEDIDOS - RESTAURANTE "EL CÓDIGO SABROSO"
# ═══════════════════════════════════════════════════════════
#
# 📋 ENTRADAS SIMULADAS para ejecutar sin interacción (una por línea).
#    Secuencia verificada: pide una hamburguesa, la cobra y termina.
#    1              → Elegir "🍔 Hamburguesa" en el menú principal
#    (vacío)        → [Enter] para continuar a la siguiente pantalla
#    6              → Ir a "💰 Ver total y pagar"
#    si             → Confirmar el pago (el pago exitoso cierra el programa)
#

# Constantes de precios (datos centralizados)
PRECIO_HAMBURGUESA = 8.50
PRECIO_PIZZA = 12.00
PRECIO_ENSALADA = 6.50
PRECIO_PASTA = 10.00
PRECIO_REFRESCO = 2.50
PRECIO_JUGO = 3.00
PRECIO_AGUA = 1.50

funcion MostrarMenuPrincipal()
    escribir("\n╔════════════════════════════════════╗")
    escribir("║   EL CÓDIGO SABROSO - MENÚ         ║")
    escribir("╠════════════════════════════════════╣")
    escribir("║ 1. 🍔 Hamburguesa    ($8.50)       ║")
    escribir("║ 2. 🍕 Pizza          ($12.00)      ║")
    escribir("║ 3. 🥗 Ensalada       ($6.50)       ║")
    escribir("║ 4. 🍝 Pasta          ($10.00)      ║")
    escribir("║ 5. 🥤 Ver bebidas                  ║")
    escribir("║ 6. 💰 Ver total y pagar            ║")
    escribir("║ 7. ❌ Salir                        ║")
    escribir("╚════════════════════════════════════╝")
fin

funcion MostrarMenuBebidas()
    escribir("\n╔════════════════════════════════════╗")
    escribir("║         MENÚ DE BEBIDAS            ║")
    escribir("╠════════════════════════════════════╣")
    escribir("║ 1. 🥤 Refresco       ($2.50)       ║")
    escribir("║ 2. 🧃 Jugo           ($3.00)       ║")
    escribir("║ 3. 💧 Agua           ($1.50)       ║")
    escribir("║ 4. 🔙 Volver al menú principal     ║")
    escribir("╚════════════════════════════════════╝")
fin

funcion AgregarAlPedido(item, precio, pedido)
    # Agregar a la lista de pedido
    lista.agregar(pedido, {"item": item, "precio": precio})
    escribir("✅ " .. item .. " agregado al pedido")
fin

funcion MostrarTotal(pedido)
    escribir("\n╔════════════════════════════════════╗")
    escribir("║         TU PEDIDO                  ║")
    escribir("╠════════════════════════════════════╣")
    
    si lista.longitud(pedido) == 0
        # Texto: 24 chars. Espacios: 3 izq + 9 der = 12. Total = 36.
        escribir("║   (No has pedido nada aún)         ║")
    sino
        total = 0
        para item en pedido
            nombre = item["item"]
            precio = item["precio"]
            
            # Ajuste de columnas
            # {0,-27} -> Nombre (ancho 27, alineado izquierda)
            # {1,5:F2} -> Precio (ancho 5, alineado derecha)
            # Matemática: 1 (espacio) + 27 + 1 (espacio) + 1 ($) + 5 + 1 (espacio) = 36
            escribirf("║ {0,-27} ${1,5:F2} ║\n", nombre, precio)
            
            total = total + precio
        fin
        
        escribir("╠════════════════════════════════════╣")
        
        # Ajuste del Total
        # " TOTAL:" ocupa 7 espacios.
        # Necesitamos 22 espacios vacíos para empujar el precio a la derecha.
        # Matemática: 7 (texto) + 22 (espacios) + 1 ($) + 5 (precio) + 1 (espacio) = 36
        escribirf("║ TOTAL:                      ${0,5:F2} ║\n", total)
    fin
    
    escribir("╚════════════════════════════════════╝")
fin

funcion ProcesarPago(pedido)
    si lista.longitud(pedido) == 0
        escribir("\n⚠️  No tienes nada en tu pedido")
        retornar falso
    fin
    
    # Calcular total
    total = 0
    para item en pedido
        total = total + item["precio"]
    fin
    
    escribir("\n💳 Procesando pago...")
    sis.esperar(1000)
    
    escribir("✅ ¡Pago exitoso!")
    escribirf("   Total cobrado: ${0:F2}\n", total)
    escribir("   ¡Gracias por tu compra! 🎉")
    
    retornar verdadero
fin

funcion EjecutarRestaurante()
    pedido = [ ]
    enMenuBebidas = falso
    seguir = verdadero
    
    escribir("╔════════════════════════════════════╗")
    escribir("║  Bienvenido a El Código Sabroso    ║")
    escribir("╚════════════════════════════════════╝")
    
    mientras seguir
        # Mostrar el menú apropiado
        si enMenuBebidas
            MostrarMenuBebidas()
        sino
            MostrarMenuPrincipal()
        fin
        
        # Leer opción
        entrada = leer("\nSelecciona una opción: ")
        opcion = anumero(entrada)
        
        escribir("")  # Espacio estético
        
        # Procesar según menú activo
        si enMenuBebidas
            # MENÚ DE BEBIDAS
            elegir opcion
                caso 1:
                    AgregarAlPedido("Refresco", PRECIO_REFRESCO, pedido)
                caso 2:
                    AgregarAlPedido("Jugo", PRECIO_JUGO, pedido)
                caso 3:
                    AgregarAlPedido("Agua", PRECIO_AGUA, pedido)
                caso 4:
                    enMenuBebidas = falso
                    escribir("⬅️  Volviendo al menú principal...")
                otro:
                    escribir("❌ Opción inválida")
            fin
        sino
            # MENÚ PRINCIPAL
            elegir opcion
                caso 1:
                    AgregarAlPedido("Hamburguesa", PRECIO_HAMBURGUESA, pedido)
                caso 2:
                    AgregarAlPedido("Pizza", PRECIO_PIZZA, pedido)
                caso 3:
                    AgregarAlPedido("Ensalada", PRECIO_ENSALADA, pedido)
                caso 4:
                    AgregarAlPedido("Pasta", PRECIO_PASTA, pedido)
                caso 5:
                    enMenuBebidas = verdadero
                    escribir("➡️  Entrando al menú de bebidas...")
                caso 6:
                    MostrarTotal(pedido)
                    
                    si lista.longitud(pedido) > 0
                        confirmar = leer("\n¿Confirmar pago? (si/no): ")
                        si confirmar == "si"
                            si ProcesarPago(pedido)
                                seguir = falso
                            fin
                        sino
                            escribir("Pago cancelado, puedes seguir ordenando")
                        fin
                    fin
                caso 7:
                    escribir("👋 ¡Hasta pronto!")
                    seguir = falso
                otro:
                    escribir("❌ Opción inválida. Por favor elige 1-7")
            fin
        fin
        
        # Pausa antes de mostrar el menú de nuevo
        si seguir
            leer("\nPresiona [Enter] para continuar...")
        fin
    fin
fin

# ═══════════════════════════════════════════════════════════
#  PUNTO DE ENTRADA
# ═══════════════════════════════════════════════════════════

EjecutarRestaurante()
```

**Características del proyecto:**

✅ **Dos menús con `elegir`** - Principal y bebidas  
✅ **Gestión de estado** - Variable `enMenuBebidas` controla el flujo  
✅ **Lista de pedidos** - Acumulador de items  
✅ **Validación de entrada** - Opción `otro` para casos inválidos  
✅ **Formateo profesional** - Tablas ASCII alineadas  
✅ **Flujo completo** - Desde selección hasta pago  

## Comparación: `elegir` vs `si/osi`

Veamos el mismo problema resuelto con ambas estructuras:

**Problema:** Convertir nota numérica a letra.

### Versión con `si/osi`

```latino
nota = 85
letra = ""

si nota >= 90
    letra = "A"
osi nota >= 80
    letra = "B"
osi nota >= 70
    letra = "C"
osi nota >= 60
    letra = "D"
sino
    letra = "F"
fin

escribir("Calificación: " .. letra)
```

**Ventajas:**

- ✅ Funciona con rangos
- ✅ Fácil de entender para principiantes
- ✅ Flexible con condiciones complejas

**Desventajas:**

- ❌ Repetitivo si hay muchos casos
- ❌ Más código para valores exactos

### Versión con `elegir` (Switch Invertido)

```latino
nota = 85
letra = ""

elegir verdadero
    caso nota >= 90:
        letra = "A"
    caso nota >= 80:
        letra = "B"
    caso nota >= 70:
        letra = "C"
    caso nota >= 60:
        letra = "D"
    otro:
        letra = "F"
fin

escribir("Calificación: " .. letra)
```

**Ventajas:**

- ✅ Más compacto visualmente
- ✅ Estructura clara de opciones

**Desventajas:**

- ❌ Menos obvio que es un switch
- ❌ Requiere entender el "truco"

### Versión con `elegir` (Valores Exactos)

Si las notas fueran valores fijos:

```latino
codigo_nota = "B"  # Recibido de algún sistema
puntos = 0.0

elegir codigo_nota
    caso "A":
        puntos = 4.0
        escribir("Excelente")
    caso "B":
        puntos = 3.0
        escribir("Bueno")
    caso "C":
        puntos = 2.0
        escribir("Aceptable")
    caso "D":
        puntos = 1.0
        escribir("Mínimo")
    caso "F":
        puntos = 0
        escribir("Reprobado")
    otro:
        puntos = 0
        escribir("Código inválido")
fin

escribirf("Puntos GPA: {0:F1}\n", puntos)
```

**Ventajas:**

- ✅ Uso ideal de `elegir`
- ✅ Muy legible
- ✅ Fácil agregar casos

## Anti-Patrones con `elegir`

### 1. Usar `elegir` para Rangos (Sin Truco)

```latino
edad = 68

# ❌ ESTO NO FUNCIONA (Error de Lógica)
# El 'elegir' compara el valor (68) contra el resultado de la condición (verdadero/falso).
# 68 nunca es igual a verdadero.
elegir edad
    caso edad >= 65:
        escribir("Adulto Mayor")
    caso edad >= 18:
        escribir("Adulto")
    otro:
        escribir("Menor")
fin

# ✅ OPCIÓN A: USA si/osi PARA RANGOS (Clásico)
si edad >= 65
    escribir("Adulto Mayor")
osi edad >= 18
    escribir("Adulto")
sino
    escribir("Menor")
fin

# ✅ OPCIÓN B: EXPRESIÓN SWITCH (Operador Ternario)
# Ideal para asignar valores basados en rangos de forma compacta.
categoria = edad >= 65 ? "Adulto Mayor" : 
            edad >= 18 ? "Adulto" : 
            "Menor"

escribir("Categoría: " .. categoria)
```

### 2. Casos Duplicados

```latino
# ❌ MAL: El segundo caso 1 nunca se ejecuta
elegir opcion
    caso 1:
        escribir("Opción uno")
    caso 2:
        escribir("Opción dos")
    caso 1:  # ¡Ya existe!
        escribir("Esto nunca se ejecuta")
fin
```

### 3. Olvidar `otro` cuando es Necesario

```latino
# ⚠️ PELIGROSO: Sin validación
elegir dia_semana
    caso 1:
        escribir("Lunes")
    caso 2:
        escribir("Martes")
    # ... casos 3-7 ...
fin
# Si dia_semana = 99, no pasa nada (silencio)

# ✅ MEJOR: Con validación
elegir dia_semana
    caso 1:
        escribir("Lunes")
    # ... otros casos ...
    otro:
        escribir("❌ Día inválido (debe ser 1-7)")
fin
```

## 📊 Tabla de Decisión Final

Guía rápida para seleccionar la estructura de control ideal según la lógica que necesites implementar:

| Necesito... | Herramienta | Ejemplo |
| :--- | :--- | :--- |
| Condición única | `si` | `si edad >= 18` |
| 2-3 alternativas (Bloques) | `si/osi/sino` | `si...osi...sino` |
| Asignación rápida / Expresión | Operador Ternario | `tipo = es_vip ? "VIP" : "Normal"` |
| Comparar rangos | `si/osi` | `si nota >= 90...osi >= 80` |
| Comparar 4+ valores fijos | `elegir` | `elegir dia` |
| Lógica compleja | `si/osi` | `si edad >= 18 ý tieneLicencia` |
| Mapeo valor→resultado | Diccionario | `precios["A"]` |

## 2.3 Bucles de Iteración

### ¿Por Qué Necesitas Bucles?

Imagina que necesitas saludar a 100 personas. Sin bucles:

```latino
# ❌ La pesadilla del copiar-pegar
escribir("Hola persona 1")
escribir("Hola persona 2")
escribir("Hola persona 3")
# ... 97 líneas más ...
escribir("Hola persona 100")
```

**Problemas:**

- 100 líneas de código casi idéntico
- Si cambias el mensaje, debes editarlo 100 veces
- Imposible hacer esto con datos dinámicos

**Con bucles:**

```latino
# ✅ Elegante y mantenible
para i en rango(1, 101)
    escribir("Hola persona " .. i)
fin
```

**Los bucles te permiten:**

1. 🔄 Repetir código sin duplicarlo
2. 📊 Procesar colecciones de datos
3. 🎯 Ejecutar código hasta cumplir una condición
4. 🚀 Escalar tu programa de 10 a 10,000 elementos

## Clasificación de Bucles

Latino.NET ofrece **4 tipos de bucles**, cada uno optimizado para situaciones específicas:

| Bucle | Cuándo Usar | Itera Sobre | Dificultad |
| :--- | :--- | :--- | :--- |
| `mientras` | Condición dinámica desconocida | N/A | ⭐☆☆☆☆ |
| `repetir` | Al menos 1 vez garantizado | N/A | ⭐☆☆☆☆ |
| `para` | Colecciones completas | Listas, strings | ⭐⭐☆☆☆ |
| `desde` | Índices explícitos | N/A | ⭐⭐⭐☆☆ |

**Función auxiliar:**

- `rango()` - Genera listas numéricas para bucles `para`

## Nivel 1: Bucles Básicos (mientras, repetir)

### El Bucle `mientras`

Ejecuta código **mientras** una condición sea verdadera. La condición se evalúa **antes** de cada iteración.

**Sintaxis:**

```latino
mientras condicion
    # Código a repetir
fin
```

**Características:**

- ✅ Puede ejecutarse **0 veces** (si la condición es falsa desde el inicio)
- ✅ Ideal para situaciones donde **no sabes cuántas veces** repetirás
- ⚠️ Peligro de **bucle infinito** si la condición nunca cambia

🎯 **Ejemplo 1: Contador simple con condición previa en mientras**

```latino
contador = 0

mientras contador < 5
    escribir("Vuelta número: " .. contador)
    contador = contador + 1  # CRÍTICO: Cambiar la condición
fin

escribir("Bucle terminado")
```

**Salida:**

```text
Vuelta número: 0
Vuelta número: 1
Vuelta número: 2
Vuelta número: 3
Vuelta número: 4
Bucle terminado
```

**¿Qué pasó?**

1. Primera vuelta: `contador = 0`, `0 < 5` es verdadero → ejecuta
2. Incrementa: `contador = 1`
3. Segunda vuelta: `1 < 5` es verdadero → ejecuta
4. ...continúa hasta `contador = 5`
5. `5 < 5` es **falso** → sale del bucle

🎯 **Ejemplo 2: Validación interactiva de datos de entrada**

Un patrón muy común: pedir datos hasta que sean válidos.

```latino
# Entrada simulada: abc, luego -5, luego 150, luego 25 (la última es válida)
funcion PedirEdadValida()
    edad = -1  # Valor inicial inválido
    
    mientras edad < 0 ó edad > 120
        entrada = leer("Ingresa tu edad (0-120): ")
        edad = anumero(entrada)
        
        si edad == nulo
            escribir("❌ Eso no es un número")
            edad = -1  # Forzar otra vuelta
        osi edad < 0 ó edad > 120
            escribir("❌ Edad inválida. Debe estar entre 0 y 120")
        fin
    fin
    
    escribir("✅ Edad válida: " .. edad)
    retornar edad
fin

# Uso
miEdad = PedirEdadValida()
```

**Interacción ejemplo (entradas simuladas para reproducir el flujo):**

```text
Ingresa tu edad (0-120): abc          ← entrada: "abc"
❌ Eso no es un número
Ingresa tu edad (0-120): -5           ← entrada: "-5"
❌ Edad inválida. Debe estar entre 0 y 120
Ingresa tu edad (0-120): 150          ← entrada: "150"
❌ Edad inválida. Debe estar entre 0 y 120
Ingresa tu edad (0-120): 25           ← entrada: "25"
✅ Edad válida: 25
```

### El Peligro del Bucle Infinito

Si la condición **nunca** se vuelve falsa, el bucle nunca termina:

```latino
# ❌ BUCLE INFINITO - ¡NO EJECUTAR!
x = 10

mientras x > 5
    escribir("x vale: " .. x)
    # ¡OLVIDO FATAL! No hay x = x - 1
fin

# Este código imprimirá "x vale: 10" eternamente
```

**Cómo prevenirlo:**

1. ✅ Siempre modifica la variable de la condición dentro del bucle
2. ✅ Usa un contador de seguridad si es necesario
3. ✅ Considera usar `romper` como escape de emergencia

```latino
# ✅ Bucle con contador de seguridad
# Simulación: `condicionCompleja` es un marcador de posición; sustitúyelo
# por la condición real de tu algoritmo.
intentos = 0
MAX_INTENTOS = 100
condicionCompleja = verdadero # ← SIMULACIÓN: sustitúyelo por tu condición real

mientras condicionCompleja ý intentos < MAX_INTENTOS
    # Hacer algo
    intentos = intentos + 1
fin

si intentos >= MAX_INTENTOS
    escribir("⚠️ Se alcanzó el límite de intentos")
fin
```

### El Bucle `repetir...hasta`

Similar a `mientras`, pero la condición se evalúa **después** de cada iteración. Esto garantiza que el código se ejecute **al menos 1 vez**.

**Sintaxis:**

```latino
repetir
    # Código a repetir (se ejecuta al menos 1 vez)
hasta condicion_de_salida
```

> ⚠️ **`repetir` es la única excepción a la regla del `fin`.** Todas las demás estructuras ejecutables (`si`, `mientras`, `desde`, `para`, `elegir`, funciones, clases) se cierran con `fin`. El bucle `repetir` **no**: su delimitador terminal es `hasta condicion`, que ya cumple la doble función de cerrar el bloque y declarar la condición de salida.
>
> Esto significa que escribir `hasta condicion` seguido de `fin` es un **error de sintaxis**:
>
> ```latino
> # ❌ Incorrecto: sobra el 'fin'
> repetir
>     escribir("hola")
> hasta contador >= 3
> fin
> ```
>
> ```latino
> # ✅ Correcto: 'hasta' cierra el bucle
> repetir
>     escribir("hola")
> hasta contador >= 3
> ```
>
> No lo confundas con `mientras`, que sí exige `fin` porque su condición va **antes** del cuerpo:
>
> | Bucle | Condición | Delimitador terminal |
> | :--- | :--- | :--- |
> | `mientras` | antes del cuerpo | `fin` |
> | `repetir` | después del cuerpo | `hasta condicion` |

### Diferencia Clave: `mientras` vs `repetir`

```latino
# EXPERIMENTO 1: mientras con condición falsa
contador = 10

mientras contador < 5
    escribir("Esto NUNCA se imprime")
    contador = contador + 1
fin
# Salida: (nada)

# EXPERIMENTO 2: repetir con condición falsa
contador = 10

repetir
    escribir("Esto se imprime UNA vez")
    contador = contador + 1
hasta contador >= 5
# Salida: "Esto se imprime UNA vez"
```

**Regla mnemotécnica:**

- **`mientras`** = "¿Puedo entrar?" (verifica antes)
- **`repetir`** = "Entra, luego pregunto" (verifica después)

🎯 **Ejemplo 3: Menú interactivo con ejecución garantizada en repetir**

Caso de uso perfecto para `repetir`: mostrar un menú al menos una vez.

```latino
# Entrada simulada: 1 (sumar), 10, 5, [Enter] para pausar, 5 (salir), [Enter]
# Salida esperada: "Resultado: 10 + 5 = 15"
funcion MostrarMenuCalculadora()
    opcion = 0
    
    repetir
        escribir("\n╔════════════════════════════╗")
        escribir("║    CALCULADORA BÁSICA      ║")
        escribir("╠════════════════════════════╣")
        escribir("║ 1. Sumar                   ║")
        escribir("║ 2. Restar                  ║")
        escribir("║ 3. Multiplicar             ║")
        escribir("║ 4. Dividir                 ║")
        escribir("║ 5. Salir                   ║")
        escribir("╚════════════════════════════╝")
        
        entrada = leer("\nElige una opción (1-5): ")
        opcion = anumero(entrada)
        
        si opcion == nulo
            escribir("❌ Ingresa un número válido")
            opcion = 0  # Forzar que continúe
        osi opcion >= 1 ý opcion <= 4
            # Realizar operación
            a = anumero(leer("Primer número: "))
            b = anumero(leer("Segundo número: "))
            
            elegir opcion
                caso 1:
                    escribirf("Resultado: {0} + {1} = {2}\n", a, b, a + b)
                caso 2:
                    escribirf("Resultado: {0} - {1} = {2}\n", a, b, a - b)
                caso 3:
                    escribirf("Resultado: {0} × {1} = {2}\n", a, b, a * b)
                caso 4:
                    si b != 0
                        escribirf("Resultado: {0} ÷ {1} = {2:F2}\n", a, b, a / b)
                    sino
                        escribir("❌ No se puede dividir por cero")
                    fin
            fin
            
            leer("\nPresiona [Enter] para continuar...")
        osi opcion == 5
            escribir("👋 ¡Hasta pronto!")
        sino
            escribir("❌ Opción inválida")
        fin
        
    hasta opcion == 5
fin

# Ejecutar
MostrarMenuCalculadora()
```

**Interacción ejemplo:**

```text
╔════════════════════════════╗
║    CALULADORA EN LATINO    ║
╠════════════════════════════╣
║ 1. Sumar                   ║
║ 2. Restar                  ║
║ 3. Multiplicar             ║
║ 4. Dividir                 ║
║ 5. Salir                   ║
╚════════════════════════════╝

Elige una opción (1-5): 1
Primer número: 10
Segundo número: 5
Resultado: 10 + 5 = 15

Presiona [Enter] para continuar...

╔════════════════════════════╗
║    CALULADORA EN LATINO    ║
║ 5. Salir                   ║
╚════════════════════════════╝

Elige una opción (1-5): 5
👋 ¡Hasta pronto!
```

## Comparativa: `mientras` vs `repetir`

| Aspecto | `mientras` | `repetir...hasta` |
| :--- | :--- | :--- |
| **Evalúa condición** | Antes de ejecutar | Después de ejecutar |
| **Ejecuciones mínimas** | 0 veces | 1 vez |
| **Mejor para** | Validaciones previas | Menús, input del usuario |
| **Condición** | "mientras sea verdadero" | "hasta que sea verdadero" |

**Ejemplo visual:**

```latino
# mientras: Puede no ejecutarse nunca
x = 100
mientras x < 10
    escribir("Nunca verás esto")
fin

# repetir: Se ejecuta al menos 1 vez
x = 100
repetir
    escribir("Esto se imprime una vez")
hasta x < 10
```

## Nivel 2: Iteración de Colecciones (para, rango)

### El Bucle `para` (Foreach)

El bucle `para` es **la forma natural** de iterar en Latino.NET. Recorre cada elemento de una colección automáticamente.

**Sintaxis:**

```latino
para elemento en coleccion
    # Usar 'elemento' en cada vuelta
fin
```

**Características:**

- ✅ No necesitas índices manualmente
- ✅ Código más legible y limpio
- ✅ Funciona con listas, cadenas (vía `rango`) y números
- ⚠️ No modifiques la colección mientras iteras

### Iteración de Listas

El caso más común y natural:

```latino
frutas = ["manzana", "banana", "cereza", "durazno"]

para fruta en frutas
    escribir("Me gusta la " .. fruta)
fin
```

**Salida:**

```text
Me gusta la manzana
Me gusta la banana
Me gusta la cereza
Me gusta la durazno
```

🎯 **Ejemplo 4: Procesamiento de calificaciones con bucle para**

```latino
funcion CalcularPromedio(calificaciones)
    suma = 0
    cantidad = 0
    
    para nota en calificaciones
        suma = suma + nota
        cantidad = cantidad + 1
    fin
    
    promedio = suma / cantidad
    retornar promedio
fin

# Uso
notas = [8.5, 9.0, 7.5, 8.0, 9.5]
resultado = CalcularPromedio(notas)

escribirf("Promedio: {0:F2}\n", resultado)
# Salida: Promedio: 8.50
```

## La Función `rango()` - Generador Universal

`rango()` es una **función global multiuso** que genera listas automáticamente:

1. 🔢 **Listas numéricas** (secuencias, índices, rangos)
2. 🔤 **Listas de caracteres** desde cadenas (soporta emojis)
3. 🔄 **Control de bucles** (retrocompatibilidad con Latino)

> **⚠️ Memoria (v1.0.81):** «generador» aquí no significa perezoso: `rango()` **materializa una `LatinoList` completa en memoria RAM** en el momento de la llamada, con un objeto numérico por elemento. Costo medido en el equipo de desarrollo: **~100-130 MB por cada 3 millones de elementos** (`rango(3000000)` alcanzó un pico de 129 MB). En bucles masivos, considera este factor antes de generar rangos de millones de elementos.

### Modo 1: Generación de Números

El límite superior (`fin`) es siempre **exclusivo**.

**Sintaxis:**

```latino
rango(fin)               # [0, 1, 2, ... fin-1]
rango(inicio, fin)       # [inicio, ... fin-1]
rango(inicio, fin, paso) # Saltos de N en N
```

**Ejemplos:**

```latino
# Dígitos del 0 al 9
digitos = rango(10)
escribir(digitos)  # [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]

# Años de una década
decada2020 = rango(2020, 2030)
escribir(decada2020)  # [2020, 2021, 2022, 2023, 2024, 2025, 2026, 2027, 2028, 2029]

# Números impares del 1 al 19
impares = rango(1, 20, 2)
escribir(impares)  # [1, 3, 5, 7, 9, 11, 13, 15, 17, 19]

# Cuenta regresiva (paso negativo)
regresiva = rango(10, 0, -1)
escribir(regresiva)  # [10, 9, 8, 7, 6, 5, 4, 3, 2, 1]
```

### Modo 2: Iteración de Cadenas

Puedes iterar una cadena de texto directamente sin necesidad de convertirla manualmente. `rango()` se encarga de desglosar el texto en caracteres de forma inteligente (respetando emojis y tildes).

**Sintaxis Directa (Recomendada):**

```latino
texto = "Aé🚀"

# El bucle recibe la cadena directamente
para caracter en rango(texto)
    escribir("Carácter: " .. caracter)
fin
```

**Salida:**

```text
Carácter: A
Carácter: é
Carácter: 🚀
```

#### Soporte Completo de Unicode

`rango()` maneja correctamente **grafemas complejos** (emojis, diacríticos combinados):

```latino
# Emoji simple
emojis = rango("🚀💻🎨")
escribir(emojis)  # ["🚀", "💻", "🎨"]

# Emoji compuesto (familia = 1 grafema visual)
familia = rango("A👨‍👩‍👧‍👦B")
escribir(familia)  # ["A", "👨‍👩‍👧‍👦", "B"]  ← 3 elementos
escribir(lista.longitud(familia))  # 3

# Espacios y puntuación incluidos
texto = rango("¡Hola, 世界!")
escribir(texto)  # ["¡", "H", "o", "l", "a", ",", " ", "世", "界", "!"]
```

**⚠️ Nota Técnica:** `rango()` respeta límites de grafemas Unicode. Los emojis compuestos (como 👨‍👩‍👧‍👦) se mantienen como **un solo carácter** en la lista resultante.

🎯 **Ejemplo 2: Contador de Vocales**

```latino
funcion ContarVocales(texto)
    vocales = ["a", "e", "i", "o", "u", "A", "E", "I", "O", "U"]
    contador = 0
    
    # Iteración directa sin conversión manual
    para letra en rango(texto)
        si lista.contiene(vocales, letra)
            contador = contador + 1
        fin
    fin
    
    retornar contador
fin

# Uso
frase = "Hola mundo desde Latino"
total = ContarVocales(frase)

escribir("La frase '" .. frase .. "' tiene " .. total .. " vocales")
# Salida: La frase 'Hola mundo desde Latino' tiene 9 vocales
```

🎯 **Ejemplo 3: Análisis de Texto**

```latino
funcion AnalizarTexto(texto)
    letras = rango(texto)
    
    escribir("=== ANÁLISIS DE TEXTO ===")
    escribir("Total de caracteres: " .. lista.longitud(letras))
    
    espacios = 0
    numeros = 0
    
    para char en letras
        si char == " "
            espacios = espacios + 1
        fin
        
        # Verificar si es dígito (0-9)
        si lista.contiene(rango("0123456789"), char)
            numeros = numeros + 1
        fin
    fin
    
    escribir("Espacios: " .. espacios)
    escribir("Dígitos: " .. numeros)
fin

# Uso
AnalizarTexto("Latino 2025 es genial!")
```

**Salida:**

```text
=== ANÁLISIS DE TEXTO ===
Total de caracteres: 22
Espacios: 3
Dígitos: 4
```

### Modo 3: Control de Bucles (Retrocompatibilidad)

Combinar `rango()` con `para` emula el bucle clásico de Latino:

```latino
# Repetir 5 veces
para i en rango(5)
    escribir("Vuelta: " .. i)
fin
```

**Salida:**

```text
Vuelta: 0
Vuelta: 1
Vuelta: 2
Vuelta: 3
Vuelta: 4
```

**✅ Portabilidad 100%** - Código de Latino funciona sin cambios:

```latino
# Código original de Latino
para i en rango(5)
    escribir(i)
fin

# Funciona EXACTAMENTE IGUAL en Latino.NET
```

🎯 **Ejemplo 4: Cuenta Regresiva**

```latino
escribir("🚀 Cuenta regresiva para despegue...")

para t en rango(10, 0, -1)
    escribirf("{0}...\n", t)
    sis.esperar(500)  # Esperar 500ms
fin

escribir("🔥 ¡DESPEGUE!")
```

**Salida:**

```text
🚀 Cuenta regresiva para despegue...
10...
9...
8...
...
1...
🔥 ¡DESPEGUE!
```

🎯 **Ejemplo 5: Acceso por Índice (Listas Paralelas)**

Cuando tienes dos listas relacionadas y necesitas acceder a ambas simultáneamente:

```latino
nombres = ["Ana", "Beto", "Carla"]
edades  = [22, 30, 28]

# Generar índices: 0, 1, 2
para i en rango(lista.longitud(nombres))
    escribir(nombres[i] .. " tiene " .. edades[i] .. " años")
fin
```

**Salida:**

```text
Ana tiene 22 años
Beto tiene 30 años
Carla tiene 28 años
```

🎯 **Ejemplo 6: Procesamiento por Lotes (Batch)**

```latino
usuarios = ["Ana", "Beto", "Carla", "Dante", "Elena", "Fabio"]
total = lista.longitud(usuarios)
tamano_lote = 2

escribir("📦 Procesando usuarios en lotes de " .. tamano_lote)

# Saltamos de 2 en 2: 0, 2, 4
para i en rango(0, total, tamano_lote)
    fin_lote = i + tamano_lote
    
    # No pasarnos del total
    si fin_lote > total
        fin_lote = total
    fin
    
    escribirf("\n--- Lote {0} ---\n", (i / tamano_lote) + 1)
    
    # Procesar este lote
    para j en rango(i, fin_lote)
        escribir("  ✉️  Enviando email a: " .. usuarios[j])
    fin
fin
```

**Salida:**

```text
📦 Procesando usuarios en lotes de 2

--- Lote 1 ---
  ✉️  Enviando email a: Ana
  ✉️  Enviando email a: Beto

--- Lote 2 ---
  ✉️  Enviando email a: Carla
  ✉️  Enviando email a: Dante

--- Lote 3 ---
  ✉️  Enviando email a: Elena
  ✉️  Enviando email a: Fabio
```

### Iteración de Diccionarios

Los diccionarios **no se pueden iterar directamente** con `para`. Debes decidir si quieres las **llaves** o los **valores**.

#### Opción 1: Iterar Llaves

```latino
usuario = {
    "nombre": "Ana",
    "edad": 25,
    "ciudad": "Panamá"
}

llaves = dic.llaves(usuario)  # Retorna ["nombre", "edad", "ciudad"]

para clave en llaves
    valor = usuario[clave]
    escribir(clave .. ": " .. valor)
fin
```

**Salida:**

```text
nombre: Ana
edad: 25
ciudad: Panamá
```

#### Opción 2: Iterar Valores

```latino
usuario = {
    "nombre": "Ana",
    "edad": 25,
    "admin": verdadero
}

valores = dic.valores(usuario)  # Retorna ["Ana", 25, verdadero]

para val en valores
    escribir("Dato: " .. val)
fin
```

**Salida:**

```text
Dato: Ana
Dato: 25
Dato: verdadero
```

🎯 **Ejemplo 7: Tabla de Contactos**

```latino
funcion MostrarContactos(contactos)
    escribir("\n╔═══════════════════════════════════════╗")
    escribir("║          LISTA DE CONTACTOS           ║")
    escribir("╠═══════════════════════════════════════╣")
    
    llaves = dic.llaves(contactos)
    
    para nombre en llaves
        telefono = contactos[nombre]
        escribirf("║ {0,-20} {1,15}  ║\n", nombre, telefono)
    fin
    
    escribir("╚═══════════════════════════════════════╝")
fin

# Uso
agenda = {
    "Carlos": "507-1234-5678",
    "María": "507-8765-4321",
    "Pedro": "507-5555-9999"
}

MostrarContactos(agenda)
```

**Salida:**

```text
╔═══════════════════════════════════════╗
║          LISTA DE CONTACTOS           ║
╠═══════════════════════════════════════╣
║ Carlos                 507-1234-5678  ║
║ María                  507-8765-4321  ║
║ Pedro                  507-5555-9999  ║
╚═══════════════════════════════════════╝
```

#### Opción 3: Patrón de Enumeración (Claves Numéricas)

Si estructuras tu diccionario utilizando **números secuenciales** como claves (similar a una lista, pero con índices personalizados), puedes usar la función `rango()` para iterar directamente.

Esto es ideal para menús o catálogos donde los IDs son números.

```latino
# Diccionario indexado numéricamente
menu = {
    1: "Nueva Partida",
    2: "Cargar Juego",
    3: "Opciones",
    4: "Salir"
}

escribir("--- MENÚ PRINCIPAL ---")

# Generamos las claves del 1 al 5 (el 5 es el límite exclusivo)
para i en rango(1, 5)
    # Acceso directo usando el número generado
    item = menu[i]
    
    escribir(i .. ". " .. item)
fin
```

**Salida:**

```text
--- MENÚ PRINCIPAL ---
1. Nueva Partida
2. Cargar Juego
3. Opciones
4. Salir
```

> **Consejo:** Este patrón funciona porque en Latino.NET los diccionarios soportan claves de cualquier tipo. Asegúrate de que el `rango` que generes coincida con las claves que definidas, o se producirá un error de ejecución (`RuntimeError`) por clave no encontrada.

### Limitación: Tuplas

Las tuplas tienen una estructura interna diferente. En Latino.NET v1.0, no puedes usar `para` directamente con tuplas.

```latino
# ⚠️ En v1.0.81, el bucle 'para' solo itera sobre listas.
# Si pasas una tupla, el motor no arrojará un error visible,
# pero ignorará el bucle por completo:
mi_tupla = (10, 20, 30)

para item en mi_tupla  # Se omite en silencio (no se ejecuta)
    escribir(item)
fin

# ✅ SOLUCIÓN IDIOMÁTICA: Recorrer por índice con rango() y tupla.longitud()
mi_tupla = (10, 20, 30)
longitud = tupla.longitud(mi_tupla)

para i en rango(longitud)
    valor = tupla.obtener(mi_tupla, i)
    escribir("Elemento: " .. valor)
fin
```

**Salida:**

```text
Elemento: 10
Elemento: 20
Elemento: 30
```

## 🎯 Cuándo Usar Cada Forma

### Usa `para elemento en lista` cuando

- ✅ Solo necesitas los valores, no los índices
- ✅ Quieres código limpio y legible
- ✅ No vas a modificar la colección

### Usa `para i en rango(longitud)` cuando

- ✅ Necesitas el índice actual
- ✅ Trabajas con listas paralelas (mismo índice, diferentes listas)
- ✅ Necesitas modificar elementos in-place
- ✅ Procesas datos en lotes/bloques

### Usa `rango(texto)` cuando

- ✅ Necesitas procesar caracteres individuales
- ✅ Trabajas con emojis o Unicode complejo
- ✅ Quieres convertir cadena a lista de caracteres

## Resumen de Bucles: Niveles 1 y 2

| Bucle | Mejor Para | Ejemplo |
| :--- | :--- | :--- |
| `mientras` | Condición dinámica (0+ veces) | Validar input del usuario |
| `repetir` | Al menos 1 vez garantizado | Menús interactivos |
| `para` | Recorrer colecciones completas | Procesar lista de productos |
| `rango()` | Generar secuencias numéricas | Tablas de multiplicar, coordenadas |

## Nivel 3: Bucles con Control Explícito (desde)

### El Bucle `desde` (For Clásico)

El bucle `desde` te da **control total** sobre el contador. Es el bucle más poderoso, pero también el más complejo.

**Sintaxis:**

```latino
desde (inicializador; condición; incremento)
    # Código a repetir
fin
```

**Estructura:**

- **Inicializador**: Se ejecuta **una vez** al inicio (`i = 0`)
- **Condición**: Se evalúa **antes de cada vuelta** (`i < 10`)
- **Incremento**: Se ejecuta **al final de cada vuelta** (`i = i + 1`)

🎯 **Ejemplo Básico: Contador de 0 a 4**

```latino
desde (i = 0; i < 5; i = i + 1)
    escribir("Vuelta número: " .. i)
fin
```

**Salida:**

```text
Vuelta número: 0
Vuelta número: 1
Vuelta número: 2
Vuelta número: 3
Vuelta número: 4
```

**¿Qué pasó paso a paso?**

1. **Inicialización**: `i = 0` (solo una vez)
2. **Vuelta 1**:
   - Evalúa condición: `0 < 5` → verdadero
   - Ejecuta: `escribir("Vuelta número: 0")`
   - Incrementa: `i = 0 + 1` → `i = 1`
3. **Vuelta 2**:
   - Evalúa condición: `1 < 5` → verdadero
   - Ejecuta: `escribir("Vuelta número: 1")`
   - Incrementa: `i = 1 + 1` → `i = 2`
4. ...continúa hasta `i = 5`
5. **Final**:
   - Evalúa condición: `5 < 5` → **falso**
   - Sale del bucle

## Variaciones del Bucle `desde`

### 1. Contar Hacia Atrás

```latino
escribir("Cuenta regresiva:")

desde (i = 5; i > 0; i = i - 1)
    escribir(i .. "...")
    sis.esperar(500)
fin

escribir("¡Despegue! 🚀")
```

**Salida:**

```text
Cuenta regresiva:
5...
4...
3...
2...
1...
¡Despegue! 🚀
```

### 2. Incrementos Diferentes

```latino
# De 2 en 2 (números pares)
escribir("Números pares del 0 al 10:")
desde (i = 0; i <= 10; i = i + 2)
    escribir(i)
fin

# De 5 en 5
escribir("\nDe 5 en 5:")
desde (i = 0; i <= 50; i = i + 5)
    escribir(i)
fin
```

**Salida:**

```text
Números pares del 0 al 10:
0
2
4
6
8
10

De 5 en 5:
0
5
10
15
20
25
30
35
40
45
50
```

### 3. Múltiples Variables (Patrón Avanzado)

Aunque el bucle `desde` solo permite un inicializador formal, puedes manejar múltiples variables manualmente:

```latino
# Inicializar fuera
i = 0
j = 10

desde (; i < 5; i = i + 1)
    escribir("i: " .. i .. ", j: " .. j)
    j = j - 2  # Decrementar manualmente la segunda variable
fin
```

**Utilizando `para + rango()`**

```latino
# Inicializar fuera
j = 10

para i en rango(5)
    escribir("i: " .. i .. ", j: " .. j)
    j = j - 2  # Decrementar manualmente la segunda variable
fin
```

**Salida:**

```text
i: 0, j: 10
i: 1, j: 8
i: 2, j: 6
i: 3, j: 4
i: 4, j: 2
```

## Comparativa: `desde` vs `para + rango()`

| Criterio | `desde` (Estilo C) | `para + rango()` (Estilo Moderno) |
| :--- | :--- | :--- |
| **Legibilidad** | ⭐⭐☆☆☆ (Verboso) | ⭐⭐⭐⭐⭐ (Limpio) |
| **Uso Común** | Solo casos específicos | 95% de los casos |
| **Paso/Salto** | Manual (`i+=2`) | Automático (`rango(0,10,2)`) |
| **Condición** | **Dinámica** (Se re-evalúa siempre) | **Estática** (La lista se crea al inicio) |
| **Rendimiento** | Ligeramente más rápido (sin listas) | Crea una lista en memoria |

**Regla de oro:**

- **Usa `para + rango()`** para el 95% de casos (más claro)
- **Usa `desde`** solo cuando necesites control especial

🎯 **Ejemplo de Cuándo SÍ Usar `desde`**

Esto es lo único que `para` no puede hacer fácilmente: **Cambiar la meta mientras corres.**

**Ejemplo donde `desde` es necesario:**

```latino
limite = 10
# Usamos 'desde' porque queremos cambiar el límite sobre la marcha
desde (i = 0; i < limite; i++)
    escribir(i)
    si i == 5
        limite = 20  # ¡El bucle se extiende dinámicamente!
    fin
fin
```

- Si usaras `rango(limite)`, la lista `[0..9]` se crearía al principio y cambiar `limite` después no afectaría a la lista ya creada.

## 📝 Antipatrones con `desde`

### 1. Usar `desde` para Iteración Simple

```latino
# ❌ Verboso innecesariamente
desde (i = 0; i < 10; i = i + 1)
    escribir(i)
fin

# ✅ Más limpio
para i en rango(10)
    escribir(i)
fin
```

### 2. Condición Mal Escrita

```latino
# ❌ BUCLE INFINITO: Condición siempre verdadera
desde (i = 0; i < 10; i = i - 1)  # ¡i NUNCA llega a 10!
    escribir(i)
fin

# ✅ Correcto
desde (i = 0; i < 10; i = i + 1)
    escribir(i)
fin
```

### 3. Olvidar el Incremento

```latino
# ❌ ERROR: Sin incremento
desde (i = 0; i < 10; )  # ¡i nunca cambia!
    escribir(i)
fin

# ✅ Correcto
desde (i = 0; i < 10; i = i + 1)
    escribir(i)
fin
```

### Comparación Visual de los Bucles

**Imprimir números del 0 al 4 con cada bucle:**

```latino
# ========== MIENTRAS ==========
i = 0
mientras i < 5
    escribir(i)
    i = i + 1
fin

# ========== REPETIR ==========
i = 0
repetir
    escribir(i)
    i = i + 1
hasta i >= 5

# ========== PARA + RANGO ==========
para i en rango(5)
    escribir(i)
fin

# ========== DESDE ==========
desde (i = 0; i < 5; i = i + 1)
    escribir(i)
fin
```

**Todos producen la misma salida:**

```text
0
1
2
3
4
```

## 📊 Comparativa Completa de Todos los Bucles

### Tabla de Decisión

| Necesito... | Herramienta | Dificultad | Ejemplo |
| :--- | :--- | :--- | :--- |
| **Recorrer una lista completa** | `para` | ⭐ | Procesar productos |
| **N repeticiones exactas** | `para` + `rango()` | ⭐ | Imprimir 10 veces |
| **Condición lógica simple** | `mientras` | ⭐⭐ | Esperar conexión |
| **Ejecutar al menos 1 vez** | `repetir` | ⭐⭐ | Menú interactivo |
| **Salida anticipada compleja** | `mientras` + `romper` | ⭐⭐⭐ | Buscar hasta encontrar |
| **Modificar el índice/meta** | `desde` | ⭐⭐⭐⭐ | Saltos dinámicos |

**¿Cuál usar?**

- **`para + rango(5)`** → ✅ **Más limpio y legible** (recomendado)
- **`desde`** → Solo si necesitas algo especial
- **`mientras`/`repetir`** → Solo si la condición es dinámica

## 2.4 Control de Flujo Avanzado

### El Problema: Bucles Rígidos

Con lo que hemos visto hasta ahora, los bucles son **todo o nada**:

```latino
# ❌ Problema: Buscar un elemento
nombres = ["Ana", "Beto", "Carla", "Dante", "Elena"]
objetivo = "Carla"
encontrado = falso

para nombre en nombres
    si nombre == objetivo
        encontrado = verdadero
        # ¿Cómo detengo el bucle AHORA?
        # El bucle sigue recorriendo todos los elementos innecesariamente
    fin
fin

si encontrado
    escribir("✅ Encontrado")
fin
```

**Problema:** Seguimos iterando incluso después de encontrar lo que buscamos. En una lista de 1,000 elementos, esto es ineficiente.

**Solución:** Las sentencias de **control de flujo** te permiten alterar el comportamiento de bucles en tiempo real.

## Las Herramientas de Control

| Sentencia | Efecto | Mejor Para |
| :--- | :--- | :--- |
| `romper` | Sale **inmediatamente** del bucle | Búsquedas, detección de condiciones |
| `continuar` | Salta a la **siguiente iteración** | Filtrado, saltar elementos inválidos |
| `ir` (goto) | Salta a una **etiqueta** específica | Casos muy raros (máquinas de estado) |

## `romper` - Salida Inmediata

La sentencia `romper` **termina el bucle** instantáneamente, sin importar si quedan iteraciones pendientes.

**Sintaxis:**

```latino
mientras condicion
    # código
    si otra_condicion
        romper  # Sale del bucle ahora
    fin
    # más código
fin
# Continúa aquí después del romper
```

🎯 **Ejemplo 5: Búsqueda eficiente y detención anticipada con romper**

```latino
funcion BuscarElemento(unaLista, objetivo)
    encontrado = falso
    posicion = -1
    
    para i en rango(lista.longitud(unaLista))
        si unaLista[i] == objetivo
            encontrado = verdadero
            posicion = i
            romper  # ¡ENCONTRADO! No seguir buscando
        fin
    fin
    
    si encontrado
        escribir("✅ Encontrado en posición: " .. posicion)
    sino
        escribir("❌ No encontrado")
    fin
    
    retornar posicion
fin

# Uso
numeros = [10, 20, 30, 40, 50, 60, 70, 80, 90]
BuscarElemento(numeros, 40)
# Salida: ✅ Encontrado en posición: 3
# Solo iteró 4 veces en lugar de 9
```

🎯 **Ejemplo 2: Validación con Reintentos Limitados**

```latino
# Entrada simulada: password123 (intento 1, falla), luego Latino2026 (intento 2, acierta)
funcion PedirContrasenaConLimite()
    MAX_INTENTOS = 3
    CONTRASENA_CORRECTA = "Latino2026"
    
    desde (intento = 1; intento <= MAX_INTENTOS; intento = intento + 1)
        escribirf("Intento {0}/{1}\n", intento, MAX_INTENTOS)
        entrada = leer("Ingresa la contraseña: ")
        
        si entrada == CONTRASENA_CORRECTA
            escribir("✅ ¡Acceso concedido!")
            romper  # Salir porque ya fue exitoso
        sino
            restantes = MAX_INTENTOS - intento
            si restantes > 0
                escribirf("❌ Incorrecta. Te quedan {0} intentos\n\n", restantes)
            sino
                escribir("❌ Cuenta bloqueada por seguridad")
            fin
        fin
    fin
fin

# Uso
PedirContrasenaConLimite()
```

**Salida (ejemplo de éxito en 2do intento):**

```text
Intento 1/3
Ingresa la contraseña: password123
❌ Incorrecta. Te quedan 2 intentos

Intento 2/3
Ingresa la contraseña: Latino2026
✅ ¡Acceso concedido!
```

## `continuar` - Saltar Iteración

La sentencia `continuar` **salta** el resto del código de la vuelta actual y pasa **inmediatamente a la siguiente iteración**.

**Diferencia clave con `romper`:**

- **`romper`**: Sale del bucle completamente
- **`continuar`**: Solo salta esta vuelta, el bucle continúa

🎯 **Ejemplo 3: Filtrar Números Pares**

```latino
numeros = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

escribir("Solo números impares:")

para num en numeros
    # Si es par (módulo 2 es 0), saltar esta vuelta
    si num % 2 == 0
        continuar  # Salta el escribir()
    fin
    
    escribir("  " .. num)
fin
```

**Salida:**

```text
Solo números impares:
  1
  3
  5
  7
  9
```

🎯 **Ejemplo 4: Procesar Solo Datos Válidos**

```latino
funcion ProcesarCalificaciones(notas)
    suma = 0
    validas = 0
    
    escribir("📊 Procesando calificaciones...")
    
    para nota en notas
        # Saltar calificaciones fuera de rango
        si nota < 0 ó nota > 10
            escribir("⚠️  Calificación inválida: " .. nota .. " (ignorada)")
            continuar  # No sumar ni contar esta
        fin
        
        # Código que solo se ejecuta para notas válidas
        suma = suma + nota
        validas = validas + 1
        escribir("✅ Registrada: " .. nota)
    fin
    
    si validas > 0
        promedio = suma / validas
        escribirf("\n📈 Promedio: {0:F2} ({1} notas válidas)\n", promedio, validas)
    sino
        escribir("\n❌ No hay notas válidas")
    fin
fin

# Uso
calificaciones = [8.5, 15, 7.0, -2, 9.5, 6.0, 12]
ProcesarCalificaciones(calificaciones)
```

**Salida:**

```text
📊 Procesando calificaciones...
✅ Registrada: 8.5
⚠️  Calificación inválida: 15 (ignorada)
✅ Registrada: 7.0
⚠️  Calificación inválida: -2 (ignorada)
✅ Registrada: 9.5
✅ Registrada: 6.0
⚠️  Calificación inválida: 12 (ignorada)

📈 Promedio: 7.75 (4 notas válidas)
```

## Combinando `romper` y `continuar`

Puedes usar ambos en el mismo bucle para lógica compleja:

```latino
funcion BuscarPrimerPar(numeros, limite)
    encontrado = falso
    
    escribir("Buscando el primer número par...")
    
    para num en numeros
        # Saltar números mayores al límite (ignorar)
        si num > limite
            escribirf("  Saltando {0} (mayor a {1})\n", num, limite)
            continuar
        fin
        
        # Verificar si es par
        si num % 2 == 0
            escribir("✅ ¡Encontrado! " .. num)
            encontrado = verdadero
            romper  # Detener búsqueda
        sino
            escribir("  " .. num .. " es impar, continuar...")
        fin
    fin
    
    si !encontrado
        escribir("❌ No se encontró ningún par válido")
    fin
fin

# Uso
numeros = [15, 7, 3, 20, 8, 25, 4]
BuscarPrimerPar(numeros, 10)
```

**Salida:**

```text
Buscando el primer número par...
  Saltando 15 (mayor a 10)
  7 es impar, continuar...
  3 es impar, continuar...
  Saltando 20 (mayor a 10)
✅ ¡Encontrado! 8
```

## Bucles Anidados: `romper` Solo Sale del Más Interno

**⚠️ Importante:** `romper` solo afecta al bucle donde se encuentra. Para salir de múltiples niveles, necesitas estrategias adicionales.

```latino
# Ejemplo: Búsqueda en matriz
matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
]

objetivo = 5
encontrado = falso

para fila en matriz
    para elemento en fila
        si elemento == objetivo
            escribir("✅ Encontrado: " .. elemento)
            encontrado = verdadero
            romper  # Solo sale del bucle interno (fila)
        fin
    fin
    
    # ¿Cómo salir también del bucle externo?
    si encontrado
        romper  # Ahora sí salimos del bucle de filas
    fin
fin
```

**Salida:**

```text
✅ Encontrado: 5
```

**Alternativa con Bandera:**

```latino
matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
]

objetivo = 5
encontrado = falso

para fila en matriz
    si encontrado  # Verificar bandera al inicio
        romper
    fin
    
    para elemento en fila
        si elemento == objetivo
            escribir("✅ Encontrado: " .. elemento)
            encontrado = verdadero
            romper
        fin
    fin
fin
```

## 2.5 Saltos y GoTo: `ir` (Advertencia)

Latino soporta la sentencia `ir` (goto), que transfiere el control a una **etiqueta** definida.

**⚠️ ADVERTENCIA CRÍTICA:** El uso de `ir` está **altamente desaconsejado** en programación moderna. Puede crear "código espagueti" imposible de mantener.

**Sintaxis:**

```latino
etiqueta:
    # código

ir etiqueta  # Salta a la etiqueta
```

### ¿Por Qué Evitar `ir`?

- **Problema 1: Flujo Impredecible**

```latino
# ❌ Código confuso con goto
x = 0

inicio:
    x = x + 1
    escribir(x)
    
    si x < 5
        ir inicio
    fin
    
# ¿Cuántas veces se ejecuta? Difícil de seguir
```

**Solución:** Usa un bucle normal:

```latino
# ✅ Equivalente claro
x = 0

mientras x < 5
    x = x + 1
    escribir(x)
fin
```

- **Problema 2: Depuración Imposible**

Con `ir`, el flujo puede saltar a cualquier parte, haciendo imposible rastrear bugs.

```latino
# ❌ Pesadilla de depuración
si condicion1
    ir etiqueta_a
fin

# 100 líneas de código...

etiqueta_a:
    si condicion2
        ir etiqueta_b
    fin

# 200 líneas más...

etiqueta_b:
    # ¿Cómo llegaste aquí?
fin
```

### Casos Legítimos (Muy Raros)

Hay **exactamente 2 situaciones** donde `ir` podría ser aceptable:

#### 1. Salir de Múltiples Bucles Anidados

```latino
# ✅ Uso legítimo: Salir de 3 bucles
matriz = [
    [[1, 2], [3, 4]],
    [[5, 6], [7, 8]]
]
objetivo = 5
encontrado = falso

desde (i = 0; i < lista.longitud(matriz); i = i + 1)
    desde (j = 0; j < lista.longitud(matriz[i]); j = j + 1)
        desde (k = 0; k < lista.longitud(matriz[i][j]); k = k + 1)
            si matriz[i][j][k] == objetivo
                escribir("Encontrado en: " .. i .. "," .. j .. "," .. k)
                ir salir_todos  # Salir de los 3 bucles
            fin
        fin
    fin
fin

salir_todos:
escribir("Búsqueda terminada")
```

**Alternativa mejor (con bandera):**

```latino
# ✅ MÁS CLARO: Usando bandera con datos de prueba
matriz = [[[1, 2], [3, 4]], [[5, 6], [7, 8]]]
objetivo = 5
encontrado = falso

desde (i = 0; i < 2 ý !encontrado; i = i + 1)
    desde (j = 0; j < 2 ý !encontrado; j = j + 1)
        desde (k = 0; k < 2; k = k + 1)
            si matriz[i][j][k] == objetivo
                escribir("Encontrado")
                encontrado = verdadero
                romper
            fin
        fin
    fin
fin
```

#### 2. Máquinas de Estado Simples

```latino
# Ejemplo: Procesador de comandos
# Entrada simulada: ayuda, info, salir (los comandos válidos del ejemplo)
funcion ProcesarComandos()
    estado = "inicio"
    
    inicio:
        comando = leer("CMD> ")
        
        si comando == "ayuda"
            ir mostrar_ayuda
        osi comando == "salir"
            ir salir
        osi comando == "info"
            ir mostrar_info
        sino
            escribir("Comando desconocido")
            ir inicio
        fin
    
    mostrar_ayuda:
        escribir("Comandos: ayuda, info, salir")
        ir inicio
    
    mostrar_info:
        escribir("Sistema de comandos v1.0")
        ir inicio
    
    salir:
        escribir("Adiós")
fin
```

**Alternativa mejor (con elegir):**

```latino
# ✅ MÁS MANTENIBLE
# Entrada simulada: ayuda, info, salir (los comandos válidos del ejemplo)
funcion ProcesarComandos()
    seguir = verdadero
    
    mientras seguir
        comando = leer("CMD> ")
        
        elegir comando
            caso "ayuda":
                escribir("Comandos: ayuda, info, salir")
            caso "info":
                escribir("Sistema de comandos v1.0")
            caso "salir":
                escribir("Adiós")
                seguir = falso
            otro:
                escribir("Comando desconocido")
        fin
    fin
fin
```

## Anti-Patrones de Control de Flujo

### 1. Usar `romper` para Control Normal

```latino
# ❌ MAL: Romper por condición normal
i = 0
mientras verdadero  # Bucle infinito artificial
    escribir(i)
    i = i + 1
    si i >= 10
        romper
    fin
fin

# ✅ BIEN: Condición en el mientras
i = 0
mientras i < 10
    escribir(i)
    i = i + 1
fin
```

### 2. Abusar de `continuar`

```latino
# ❌ MAL: Lógica invertida confusa
para num en numeros
    si num < 0
        continuar
    fin
    si num > 100
        continuar
    fin
    si num % 2 == 1
        continuar
    fin
    
    # Solo llegamos aquí si pasa 3 condiciones
    escribir(num)
fin

# ✅ BIEN: Condición positiva clara
para num en numeros
    si num >= 0 ý num <= 100 ý num % 2 == 0
        escribir(num)
    fin
fin
```

### 3. `ir` para Lógica Normal

```latino
# ❌ TERRIBLE: goto para bucle
contador = 0

bucle:
    escribir(contador)
    contador = contador + 1
    si contador < 5
        ir bucle
    fin

# ✅ EXCELENTE: Bucle normal
desde (contador = 0; contador < 5; contador = contador + 1)
    escribir(contador)
fin
```

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
