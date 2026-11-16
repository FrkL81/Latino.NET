# CAPÍTULO 7: Herramientas Esenciales del Programador

## El Taller del Software Profesional

Cualquiera puede escribir un programa que funcione cuando todo sale bien: cuando el usuario introduce exactamente los datos esperados, la red responde a tiempo, los archivos existen y la memoria es infinita. A este escenario ideal se le conoce en ingeniería como el *camino feliz* (*the happy path*).

Sin embargo, el mundo real es caótico y hostil:

- Los discos duros se llenan o pierden permisos de escritura.
- Los usuarios introducen texto donde se esperaban números.
- Los servidores remotos se caen o tardan demasiado en responder.
- Las tablas en terminales se desalinean si no se controlan anchos fijos y decimales.
- Los scripts colisionan entre sí cuando las variables globales se nombran sin disciplina.

La verdadera diferencia entre un programador casual y un desarrollador profesional no radica en conocer la sintaxis básica, sino en **cómo prepara su código para resistir cuando las cosas salen mal** y **cómo pule la presentación de sus datos para que el resultado sea confiable, legible y mantenible**.

Este capítulo es tu taller de artesanía técnica: una colección condensada de convenciones, trampas no evidentes y herramientas de ingeniería que a cualquier programador le tomaría meses de ensayo y error descubrir por su cuenta.

### Los Cuatro Pilares de la Robustez

Para elevar tus scripts al estándar de producción, Latino.NET integra herramientas nativas organizadas en cuatro dimensiones esenciales:

1. **Precisión en la Presentación (7.1):** El texto sin formatear produce reportes amateur. Dominarás el sistema dual de formateo de Latino.NET: el estilo de alto nivel de **.NET** (`escribirf`) para alineaciones tabulares y monedas, y la precisión clásica de **C** (`imprimirf`) para ingeniería y notación científica.
2. **Interacción con el Entorno y el Sistema Operativo (7.2):** Tu software no vive en el vacío. Aprenderás a gobernar la librería `sis` para consultar directorios de trabajo, detectar plataformas en caliente (Windows, Linux, macOS), ajustar parámetros de seguridad del motor (`max_stack`) y emitir códigos de salida estándar (`sis.salir`) para integraciones con Docker y terminales.
3. **Resiliencia Defensiva ante Fallos (7.3):** En Latino.NET, un error de usuario no debe colapsar el proceso. Dominarás el blindaje defensivo con `intentar ... atrapar`, los patrones de reintento (*retry logic*), la propagación de fallos (*bubbling*) y la emisión de excepciones personalizadas de negocio con `lanzar()`.
4. **Validación, Memoria y Disciplina Léxica (7.4 – 7.7):** Completarás tu caja de herramientas con el motor de expresiones regulares protegido contra ataques ReDoS (`~=`, `cadena.regex`), las reglas oficiales de nomenclatura limpia, la gestión de alias de bibliotecas y el intercambio de memoria de alto rendimiento mediante el operador `~` y `trocar()`.

> 💡 **Principio de Resiliencia Industrial (Mantra de Latino.NET):**  
> *"El código aficionado asume que el entorno es perfecto; el software profesional se diseña para cuando el mundo falla."*  
> No esperes a que un script falle en producción para protegerlo. Formatea tus salidas, valida tus entradas, captura las contingencias predecibles y entrega siempre estados de salida claros al sistema anfitrión.

## 7.1 Salida Formateada de Datos

### ¿Por qué necesitas formateo?

Cuando empezaste a programar, probablemente escribías cosas así:

```latino
nombre = "Ana"
saldo = 1250.756
escribir("Hola " .. nombre .. ", tu saldo es: " .. saldo)
# Salida: Hola Ana, tu saldo es: 1250.756
```

Esto funciona, pero tiene problemas:

1. **No controlas los decimales** → El saldo muestra 3 decimales cuando solo necesitas 2
2. **No puedes alinear columnas** → Imposible hacer tablas legibles
3. **No hay formato de moneda** → Falta el símbolo `$` y las comas de miles
4. **Código difícil de leer** → Muchos `..` encadenados

El **formateo de cadenas** resuelve todos estos problemas. Es como tener una plantilla donde defines *cómo* quieres que se vean tus datos, y Latino.NET se encarga del resto.

### Los Dos Sistemas de Formateo

Latino.NET te ofrece **dos sabores** de formateo, cada uno con sus ventajas:

| Característica | `escribirf` / `ponerf` | `imprimirf` |
| :--- | :--- | :--- |
| **Estilo** | Moderno (.NET/C#) | Clásico (C/Java) |
| **Sintaxis** | `{0}`, `{1:F2}` | `%s`, `%.2f` |
| **Moneda/Porcentaje** | ✅ Automático | ❌ Manual |
| **Curva de aprendizaje** | ⭐⭐☆☆☆ Suave | ⭐⭐⭐☆☆ Requiere memorizar |
| **Mejor para** | Reportes, UI, principiantes | Veteranos de C, logs técnicos |

> **💡 Recomendación:** Si es tu primera vez con formateo, empieza con `escribirf`. Si vienes de C, C++ o Java, `imprimirf` te resultará familiar.

**Importante:** Ninguna de estas funciones agrega salto de línea automático. Si lo necesitas, añade `\n` al final del texto.

## Método 1: Estilo .NET (`escribirf` / `ponerf`)

Esta es la forma **moderna y poderosa** de formatear. Usa el motor de Microsoft .NET, que maneja automáticamente cosas como separadores de miles, símbolos de moneda y formatos regionales.

> **Nota:** `ponerf` es un alias exacto de `escribirf`. Úsalos indistintamente.

### Sintaxis Básica (Estilo .NET)

La estructura es: **`{índice[,alineación][:formato]}`**

```latino
# Lo más simple: placeholders numerados
escribirf("Hola {0}, tienes {1} mensajes\n", "Carlos", 5)
# Salida: Hola Carlos, tienes 5 mensajes
```

**Reglas de oro:**

1. Los índices empiezan en `0` (primer argumento después del formato)
2. Puedes reutilizar índices: `"{0} y {0}"` usa el mismo valor dos veces
3. El orden de los índices no importa: `"{1} antes que {0}"` es válido

### Formatos Numéricos Esenciales

#### Decimales Fijos (`F`)

Controla cuántos decimales mostrar. Útil para dinero o mediciones.

```latino
pi = 3.14159265359

escribirf("Con 2 decimales: {0:F2}\n", pi)   # 3.14
escribirf("Con 5 decimales: {0:F5}\n", pi)   # 3.14159
escribirf("Sin decimales:   {0:F0}\n", pi)   # 3
```

#### Números con Separadores (`N`)

Agrega comas (o puntos según tu región) cada 3 dígitos.

```latino
grande = 1234567.89

escribirf("{0:N}\n", grande)     # 1,234,567.89 (default: 2 decimales)
escribirf("{0:N0}\n", grande)    # 1,234,568 (sin decimales, redondea)
```

#### Moneda (`C`)

Formato automático con símbolo de moneda local.

```latino
precio = 49.99

escribirf("Precio: {0:C}\n", precio)
# En LATAM: $49.99
# En Europa: 49,99 €
# (Depende de la configuración regional del sistema)
```

#### Porcentaje (`P`)

Convierte decimales a porcentajes (multiplica por 100 y agrega `%`).

```latino
descuento = 0.15
tasa = 0.0325

escribirf("Descuento: {0:P0}\n", descuento)  # 15%
escribirf("Interés: {0:P2}\n", tasa)         # 3.25%
```

#### Hexadecimal (`X`)

Convierte números a base 16 (útil para colores RGB o debugging).

```latino
color = 255

escribirf("Decimal: {0}, Hex: {0:X}\n", color)  # Decimal: 255, Hex: FF
escribirf("Hex minúscula: {0:x}\n", color)      # ff
```

### Alineación y Tablas

Puedes reservar espacio fijo para que las columnas se alineen perfectamente.

**Sintaxis:** `{índice,ancho}` donde:

- `ancho` positivo → Alinea a la **derecha**
- `ancho` negativo → Alinea a la **izquierda**

```latino
escribir("--- Reporte de Ventas ---")
escribirf("| {0,-15} | {1,10} |\n", "Producto", "Precio")
escribirf("| {0,-15} | {1,10:C} |\n", "Laptop", 899.99)
escribirf("| {0,-15} | {1,10:C} |\n", "Mouse", 25.50)
escribirf("| {0,-15} | {1,10:C} |\n", "Teclado", 45.00)

# Salida:
# | Producto        |     Precio |
# | Laptop          |    $899.99 |
# | Mouse           |     $25.50 |
# | Teclado         |     $45.00 |
```

### Combinando Formato y Alineación

Puedes usar ambos al mismo tiempo: `{índice,ancho:formato}`

```latino
items = [
    ["CPU", 350.00],
    ["RAM 16GB", 120.50],
    ["SSD 1TB", 180.00]
]

escribir("=== Factura de Compra ===")
total = 0

para item en items
    nombre = item[0]
    precio = item[1]
    
    escribirf("| {0,-12} | {1,10:C} |\n", nombre, precio)
    total = total + precio
fin

escribirf("| {0,-12} | {1,10:C} |\n", "TOTAL", total)

# Salida:
# | CPU          |    $350.00 |
# | RAM 16GB     |    $120.50 |
# | SSD 1TB      |    $180.00 |
# | TOTAL        |    $650.50 |
```

### Ejercicio Guiado: Calculadora de Propinas

```latino
funcion MostrarPropina(cuenta, porcentaje)
    propina = cuenta * porcentaje
    total = cuenta + propina
    
    escribir("\n--- Desglose de Cuenta ---")
    escribirf("Subtotal:  {0,10:C}\n", cuenta)
    escribirf("Propina:   {0,10:C} ({1:P0})\n", propina, porcentaje)
    escribirf("           {0,10}\n", "----------")  # Línea divisoria
    escribirf("Total:     {0,10:C}\n", total)
fin

# Prueba
MostrarPropina(45.80, 0.18)

# Salida:
# --- Desglose de Cuenta ---
# Subtotal:      $45.80
# Propina:        $8.24 (18%)
#                ----------
# Total:         $54.04
```

## Método 2: Estilo C (`imprimirf`)

Esta función usa la sintaxis clásica de `printf` de C. Es perfecta si ya programaste en C, C++, Java o PHP, porque los códigos son idénticos.

### Sintaxis Básica (Estilo C)

La estructura es: **`%[banderas][ancho][.precisión]tipo`**

```latino
imprimirf("Hola %s, tienes %d años\n", "Luis", 25)
# Salida: Hola Luis, tienes 25 años
```

### Especificadores de Tipo

| Código | Tipo | Descripción | Ejemplo |
| :--- | :--- | :--- | :--- |
| `%s` | String | Cadena de texto | `"Hola"` |
| `%d` / `%i` | Integer | Número entero (trunca decimales) | `42` |
| `%f` | Float | Número decimal (6 decimales por defecto) | `3.141593` |
| `%x` / `%X` | Hex | Hexadecimal (minúscula/mayúscula) | `ff` / `FF` |
| `%o` | Octal | Base 8 | `12` |
| `%e` | Scientific | Notación científica | `1.5e+002` |
| `%%` | Literal | El símbolo `%` | `%` |

### Control de Precisión

#### Decimales en Flotantes (`.N`)

```latino
pi = 3.14159265359

imprimirf("Default: %f\n", pi)       # 3.141593 (6 decimales)
imprimirf("Dos decimales: %.2f\n", pi)  # 3.14
imprimirf("Sin decimales: %.0f\n", pi)  # 3
```

#### Relleno de Enteros

```latino
dia = 5
mes = 9

# ⚠️ En v1.0.81, imprimirf con %02d rellena con espacios debido al transpilador interno.
# ✅ Forma recomendada con formato nativo .NET (escribirf):
escribirf("Fecha: {0:D2}/{1:D2}/2026\n", dia, mes)
# Salida: Fecha: 05/09/2026

# ✅ Alternativa mediante función de biblioteca y conversión canónica:
diaStr = cadena.rellenar_izquierda(acadena(dia), "0", 2)
mesStr = cadena.rellenar_izquierda(acadena(mes), "0", 2)
escribir("Fecha: " .. diaStr .. "/" .. mesStr .. "/2026")
```

### Alineación con Ancho

```latino
escribir("--- Tabla de Multiplicar (Estilo C) ---")
imprimirf("| %5s | %5s | %6s |\n", "A", "B", "A×B")

desde (i = 1; i <= 3; i += 1)
    imprimirf("| %5d | %5d | %6d |\n", i, 7, i * 7)
fin

# Salida:
# |     A |     B |    A×B |
# |     1 |     7 |      7 |
# |     2 |     7 |     14 |
# |     3 |     7 |     21 |
```

**Alineación a la izquierda:** Usa el flag `-` antes del ancho.

```latino
imprimirf("| %-10s | %8s |\n", "Nombre", "Edad")
imprimirf("| %-10s | %8d |\n", "Ana", 28)
imprimirf("| %-10s | %8d |\n", "Roberto", 45)

# Salida:
# | Nombre     |     Edad |
# | Ana        |       28 |
# | Roberto    |       45 |
```

### Casos Especiales

#### Hexadecimal y Octal

```latino
num = 255

imprimirf("Decimal: %d\n", num)    # 255
imprimirf("Hex (min): %x\n", num)  # ff
imprimirf("Hex (may): %X\n", num)  # FF
imprimirf("Octal: %o\n", num)      # 377
```

#### Notación Científica

```latino
grande = 123456.789

imprimirf("Estándar: %f\n", grande)      # 123456.789000
imprimirf("Científico: %e\n", grande)    # 1.234568e+005
imprimirf("Científico (2 dec): %.2e\n", grande)  # 1.23e+005
```

## Comparación Lado a Lado

Aunque ambas funciones sirven para formatear texto, tienen filosofías diferentes que afectan el resultado visual. El siguiente ejemplo ilustra las diferencias al trabajar con datos financieros.

**Problema:** Mostrar un reporte de ventas con moneda, porcentaje y alineación.

```latino
vendedor = "María González"
ventas = 15234.50
comision_porcentaje = 0.08
comision = ventas * comision_porcentaje

escribir("\n══════════ ESTILO .NET ══════════")
escribirf("Vendedor: {0}\n", vendedor)
escribirf("Ventas:   {0,12:C}\n", ventas)
escribirf("Comisión: {0,12:P0}\n", comision_porcentaje)
escribirf("Ganancia: {0,12:C}\n", comision)

escribir("\n══════════ ESTILO C ══════════")
imprimirf("Vendedor: %s\n", vendedor)
imprimirf("Ventas:   $%12.2f\n", ventas)
imprimirf("Comisión: %11.0f%%\n", comision_porcentaje * 100)
imprimirf("Ganancia: $%12.2f\n", comision)
```

**Salida (ambos métodos):**

```text
══════════ ESTILO .NET ══════════
Vendedor: María González
Ventas:     $15,234.50
Comisión:          8 %
Ganancia:    $1,218.76

══════════ ESTILO C ══════════
Vendedor: María González
Ventas:   $    15234.50
Comisión:           8%
Ganancia: $     1218.76
```

- **Análisis de las Diferencias**

1. **Formato Inteligente vs. Matemático:**
    - **Estilo .NET (`escribirf`):** Entiende el contexto semántico. Al usar `:C` (Currency), agrega automáticamente el símbolo de moneda y los **separadores de miles** (`15,234.50`), haciendo el número más legible.
    - **Estilo C (`imprimirf`):** Trata el valor como un número matemático puro. El formato `%f` nunca agrega comas de miles (`15234.50`), lo cual es preferible para exportar datos (CSV) pero menos legible para el usuario.

2. **Alineación y Símbolos:**
    - **Estilo .NET:** Alinea el bloque completo (Símbolo + Número). El signo `$` se mantiene pegado a la cifra.
    - **Estilo C:** En el ejemplo, el signo `$` es texto fijo y el número se alinea por separado. Esto crea un hueco visual entre el signo y el número (`$    15234.50`).

**Observaciones:**

- `escribirf` maneja automáticamente el símbolo `$` y las comas
- `imprimirf` requiere que calcules el porcentaje manualmente (`* 100`)
- Para escapar `%` en `imprimirf`, debes usar `%%`

## ¿Cuándo Usar Cada Una?

### Usar `escribirf`

Usa **`escribirf` (Estilo .NET)** para reportes, interfaces de usuario y datos financieros donde la legibilidad y las convenciones regionales son importantes. Por ejemplo si:

- ✅ Necesitas formato de **moneda** o **porcentaje** automático
- ✅ Estás aprendiendo (sintaxis más intuitiva)
- ✅ Trabajas con reportes empresariales o interfaces de usuario
- ✅ Quieres código que funcione en diferentes regiones sin cambios

### Usar `imprimirf`

Usa **`imprimirf` (Estilo C)** para datos científicos, logs técnicos o exportación de archivos donde necesitas el número crudo y control exacto sobre los dígitos. Por ejemplo si:

- ✅ Ya dominas C, C++, Java o PHP (te sentirás en casa)
- ✅ Necesitas **hexadecimal** u **octal** frecuentemente
- ✅ Estás portando código antiguo que ya usa `printf`
- ✅ Prefieres control manual total sobre el formato

## 7.2 Información del Sistema (`sis`)

### ¿Qué es la librería `sis`?

Cuando tu programa se ejecuta, no está solo en el vacío. Vive dentro de un **sistema operativo** (Windows, Linux, macOS) que le proporciona recursos: memoria, acceso a archivos, hora del reloj, información del usuario, etc.

La librería `sis` (Sistema) es tu **puente de comunicación** con ese entorno. Te permite:

- 🕐 Obtener la fecha y hora actual
- 👤 Saber quién está ejecutando el programa
- 💻 Detectar el sistema operativo
- 📂 Conocer desde qué carpeta se ejecuta el código
- ⏸️ Pausar la ejecución temporalmente
- 🚪 Terminar el programa de forma controlada
- ⚙️ Ajustar parámetros internos del motor

**¿Por qué es importante?** Porque cada sistema es diferente. Un script que funciona en tu computadora podría comportarse distinto en la de tu cliente si no consultas el entorno correctamente.

## Funciones Básicas de Consulta

### `sis.tiempo()` - Fecha y Hora Actual

Retorna la fecha y hora del sistema como una **cadena de texto**.

```latino
ahora = sis.tiempo()
escribir("Ejecutado el: " .. ahora)
# Salida variable (depende de la fecha/hora del sistema)
```

**Formato regional:** La salida depende de la configuración de tu sistema operativo. En Latinoamérica verás `DD/MM/YYYY`, en EE.UU. `MM/DD/YYYY`.

#### Caso de Uso: Marca de Agua en Reportes

```latino
funcion GenerarReporte()
    escribir("╔════════════════════════════════════╗")
    escribir("║     REPORTE DE VENTAS MENSUAL      ║")
    escribir("╠════════════════════════════════════╣")
    escribir("║ ... datos del reporte ...          ║")
    escribir("╠════════════════════════════════════╣")
    escribirf("║ Generado: {0,-24} ║\n", sis.tiempo())
# Salida variable (depende de la fecha/hora del sistema)
    escribir("╚════════════════════════════════════╝")
fin

GenerarReporte()
```

### `sis.usuario()` - Nombre del Usuario

Retorna el nombre de la cuenta con la que se inició sesión en el sistema.

```latino
quien = sis.usuario()
escribir("¡Bienvenido, " .. quien .. "!")
# En mi PC: ¡Bienvenido, carlos_admin!
```

#### Caso de Uso: Personalización de Bienvenida

```latino
funcion MostrarBanner()
    usuario = sis.usuario()
    hora = sis.tiempo()
    # Salida variable (depende de la fecha/hora del sistema)
    
    escribir("")
    escribir("═══════════════════════════════════════")
    escribirf("  Usuario: {0}\n", usuario)
    escribirf("  Sesión iniciada: {0}\n", hora)
    escribir("═══════════════════════════════════════")
    escribir("")
fin

MostrarBanner()
```

### `sis.cwd()` - Directorio Actual

CWD significa **"Current Working Directory"** (Directorio de Trabajo Actual). Es la carpeta desde donde se ejecutó el programa.

```latino
carpeta = sis.cwd()
escribir("Ejecutando desde: " .. carpeta)
# Salida variable (ejemplo en Windows: C:\...\tu_proyecto | Linux: /home/usuario/tu_proyecto)
```

**¿Por qué es útil?** Porque si tu programa necesita leer archivos relativos (ej: `"config.txt"`), saber la ruta base te ayuda a construir rutas absolutas correctas.

#### Caso de Uso: Validación de Instalación

```latino
funcion VerificarInstalacion()
    ruta_actual = sis.cwd()
    
    # Verificar que estamos en la carpeta correcta
    si cadena.contiene(ruta_actual, "ProyectoLatino")
        escribir("✅ Instalación detectada correctamente")
        escribir("   Ruta: " .. ruta_actual)
    sino
        escribir("⚠️  ADVERTENCIA: No estás en la carpeta del proyecto")
        escribir("   Ruta actual: " .. ruta_actual)
        escribir("   Ruta esperada: .../ProyectoLatino/")
    fin
fin

VerificarInstalacion()
```

### `sis.operativo(nombre)` - Detección del Sistema

Verifica si el programa se está ejecutando en un sistema operativo específico. Retorna `verdadero` o `falso`.

**Valores reconocidos** (los nueve identificadores de la lista blanca, agrupados por familia):

- `"WINDOWS"`, `"WIN32"`, `"WIN"` → Windows (cualquier versión)
- `"LINUX"`, `"UNIX"` → Linux/Unix
- `"MAC"`, `"MACOS"`, `"OSX"`, `"APPLE"` → macOS

```latino
si sis.operativo("WIN32")
    escribir("Sistema: Windows")
osi sis.operativo("LINUX")
    escribir("Sistema: Linux")
osi sis.operativo("OSX")
    escribir("Sistema: macOS")
fin
```

> **⚠️ Nota técnica:** `sis.operativo()` acepta solo identificadores de la lista blanca (`WINDOWS`, `WIN32`, `WIN`, `LINUX`, `UNIX`, `MAC`, `MACOS`, `OSX`, `APPLE`). Si se pasa cualquier otro valor (como `"ANDROID"`), el motor lanza `RuntimeError: Sistema operativo desconocido: '...'`. El bloque `sino` jamás se alcanza como "desconocido", porque el error aborta la ejecución.

#### Caso de Uso: Rutas Multiplataforma

Los sistemas usan diferentes separadores de ruta (`\` en Windows, `/` en Linux/Mac). Detectar el SO te permite adaptarte:

```latino
funcion ObtenerSeparador()
    si sis.operativo("WIN32")
        retornar "\\"  # Windows usa barra invertida
    sino
        retornar "/"   # Linux/Mac usan barra normal
    fin
fin

funcion ConstruirRuta(carpeta, archivo)
    sep = ObtenerSeparador()
    retornar carpeta .. sep .. archivo
fin

# Uso
ruta = ConstruirRuta("datos", "clientes.csv")
escribir("Ruta generada: " .. ruta)
# Windows: datos\clientes.csv
# Linux:   datos/clientes.csv
```

## Funciones de Control de Ejecución

### `sis.esperar(milisegundos)` - Pausar el Programa

Detiene la ejecución por el tiempo especificado en **milisegundos** (1 segundo = 1000 ms).

```latino
escribir("Procesando")
sis.esperar(1000)  # Pausa de 1 segundo
escribir("Procesando.")
sis.esperar(1000)
escribir("Procesando..")
sis.esperar(1000)
escribir("¡Listo!")
```

#### Caso de Uso: Barra de Progreso Simulada

```latino
funcion MostrarProgreso(etapas)
    total = lista.longitud(etapas)
    
    desde (i = 0; i < total; i += 1)
        etapa = etapas[i]
        
        # 1. Calculamos la fracción (0.25, 0.50, 0.75, 1.00)
        # Nota: Convertimos a decimal implícitamente
        fraccion = (i + 1) / total
        
        # 2. Usamos formato de Porcentaje (:P0)
        # {0,5}  -> Alineación a la derecha
        # (ancho 5 para que "100%" quepa bien)
        # :P0    -> Formato Porcentaje con 0 decimales
        escribirf("[{0,5:P0}] {1}\n", fraccion, etapa)
        
        sis.esperar(800)
    fin
    
    escribir("\n✅ Proceso completado")
fin

# Uso
tareas = [
    "Validando credenciales...",
    "Conectando a base de datos...",
    "Cargando configuración...",
    "Iniciando servicios..."
]

MostrarProgreso(tareas)
```

**Salida:**

```text
[ 25%] Validando credenciales...
[ 50%] Conectando a base de datos...
[ 75%] Cargando configuración...
[100%] Iniciando servicios...

✅ Proceso completado
```

### `sis.salir([codigo])` - Terminar el Programa

Finaliza la ejecución del script inmediatamente. De manera opcional, acepta un código numérico entero para comunicar el estado de salida al sistema operativo o entorno anfitrión (scripts de terminal, Docker, Makefiles o pipelines de CI/CD).

> 💡 **Convención de códigos de salida:**
>
> - `0` (predeterminado si se omite el argumento): Indica que el programa finalizó con **éxito** normal.
> - `1` o superior: Indica que ocurrió un **error** o que se abortó por precondiciones fallidas. Las herramientas de terminal (como Bash, CMD o PowerShell) leen este valor para decidir si continúan ejecutando tareas encadenadas.

```latino
funcion ValidarLicencia(clave)
    si clave == "ABC-123-XYZ"
        escribir("✅ Licencia válida")
        retornar verdadero
    sino
        escribir("❌ Licencia inválida o expirada")
        escribir("   Contacte a soporte@empresa.com")
        # Termina el programa reportando código de error 1 al sistema operativo
        sis.salir(1)
    fin
fin

# Si la validación falla, el programa termina aquí con código 1
ValidarLicencia("CLAVE-INCORRECTA")

# Este código NUNCA se ejecutará si la clave es inválida
escribir("Iniciando aplicación principal...")
```

## Configuración Avanzada del Motor

### `sis.config(clave, [valor])` - Ajustar Parámetros Internos

Latino.NET permite modificar ciertos límites del intérprete en tiempo de ejecución. Actualmente soporta una clave:

#### `"max_stack"` - Límite de Recursividad

**¿Qué es?** Define cuántas veces puede una función llamarse a sí misma (profundidad) antes de que Latino.NET detenga la ejecución por precaución.

**Valor por defecto:** `100` (Optimizado para seguridad).

**¿Cuándo aumentarlo?** Solo si recibes el error:
`RuntimeError: Se ha excedido el límite de recursividad (100).`
Y estás seguro de que tu algoritmo es correcto pero necesita un poco más de espacio.

🎯 **Ejemplo: Ajustando el Límite**

El siguiente ejemplo muestra cómo el límite por defecto protege el programa, y cómo ampliarlo controladamente.

```latino
funcion Factorial(n)
    si n <= 1
        retornar 1
    sino
        retornar n * Factorial(n - 1)
    fin
fin

# CASO 1: Límite por defecto (100)
# Intentamos calcular Factorial de 130 (sobrepasa el límite)
escribir("=== Intento 1: Límite Estándar ===")
intentar
    resultado = Factorial(130)
    escribir("Éxito: " .. resultado)
atrapar (err)
    # Esto es BUENO: El guardián detuvo el proceso antes de que colapsara
    escribir("✅ Error controlado capturado: " .. err)
fin

# CASO 2: Aumentar el límite
# Lo subimos lo suficiente para que quepa nuestro cálculo (130),
# pero no tanto como para agotar la memoria del sistema.
escribir("\n=== Intento 2: Aumentando Límite a 200 ===")
sis.config("max_stack", 200)

intentar
    resultado = Factorial(130)
    escribir("✅ Cálculo exitoso con nuevo límite.")
    # escribir("Resultado: " .. resultado) # Es un número muy grande
atrapar (err)
    escribir("❌ Error: " .. err)
fin
```

**⚠️ Advertencia sobre `max_stack`:**

- **No lo aumentes sin razón.** Un límite alto puede congelar tu programa si hay un bucle infinito accidental.
- **Evalúa tu algoritmo primero.** A veces es mejor cambiar a una solución iterativa (con bucles) en vez de recursiva.
- **Restaura el valor después.** Si lo modificas para una tarea específica, devuélvelo a `100` al terminar:

```latino
# Guardar límite original
limiteOriginal = sis.config("max_stack")

# Aumentar temporalmente
sis.config("max_stack", 1000)

# ... ejecutar tarea pesada ...

# Restaurar
sis.config("max_stack", limiteOriginal)
```

🎯 **Ejemplo: Recursividad y Seguridad**

Este ejemplo demuestra cómo funciona el sistema de seguridad y qué pasa si lo desactivas imprudentemente.

```latino
funcion Descenso(nivel)
    si nivel <= 0 retornar verdadero fin
    retornar Descenso(nivel - 1)
fin

escribir("--- PRUEBA DE ESTABILIDAD ---")

# CASO 1: ZONA SEGURA (Dentro del límite)
# El límite es 100, pedimos 50. Todo bien.
intentar
    Descenso(50)
    escribir("✅ 1. Nivel 50: Seguro.")
atrapar (e)
    escribir("❌ 1. Fallo inesperado: " .. e)
fin

# CASO 2: EL GUARDIÁN (Límite Lógico)
# El límite es 100, pedimos 105.
# El intérprete nos detiene ANTES de que se acabe la memoria real.
intentar
    Descenso(105)
    escribir("❌ 2. Fallo: El guardián no actuó.")
atrapar (e)
    escribir("✅ 2. Guardián activo: " .. e)
    # Salida esperada: Se ha excedido el límite de recursividad (100).
fin

/*
# CASO 3: ZONA DE PELIGRO (El Crash Real)
# ⚠️ ADVERTENCIA: Esto cerrará el programa.
# Aumentamos el límite lógico a un número exagerado (10,000).
# La memoria física se acabará MUCHO antes de llegar a 10,000.

escribir("⚠️ 3. Entrando en zona de peligro...")
sis.config("max_stack", 10000) 

intentar
    # Esto provocará un 'StackOverflowException' del sistema operativo.
    # NO se puede atrapar con 'intentar/atrapar'.
    Descenso(5000) 
    escribir("¿Sobreviví?")
atrapar (e)
    escribir("Error atrapado: " .. e)
fin
*/
```

> **⚠️ Advertencia de Arquitectura (Límite Físico):**
> `sis.config` mueve el límite de software, pero **no puede aumentar la memoria física** (Stack) de tu computadora.
>
> Si configuras un límite excesivamente alto (ej: 5000) y tu función es compleja, es posible que el programa se cierre abruptamente (*Crash*) antes de que Latino.NET pueda avisarte.
>
> - **Regla general:** Aumenta el límite en incrementos pequeños solo cuando sea estrictamente necesario.
> - **Mejor práctica:** Para iteraciones masivas (miles de veces), usa siempre bucles (`mientras`, `desde`) en lugar de recursividad.

💀 **Nota sobre el Caso 3:**

Si ejecutas el Caso 3, verás que el programa termina repentinamente (posiblemente mostrando un mensaje de sistema Stack overflow.). Esto demuestra por qué nunca debes aumentar max_stack arbitrariamente si no estás seguro de la capacidad de tu máquina.

## 7.3 Manejo de Errores: `intentar / atrapar`

### La Filosofía: "Prepararse para lo Peor"

Imagina que construyes un puente. No solo calculas cuánto peso puede soportar en condiciones ideales. También consideras:

- ¿Qué pasa si hay un terremoto?
- ¿Qué pasa si el viento es más fuerte de lo esperado?
- ¿Qué pasa si un camión sobrecargado intenta cruzar?

**Programar es igual.** No basta con que tu código funcione cuando todo sale bien. Debes preguntarte:

- ¿Qué pasa si el usuario escribe texto donde esperabas un número?
- ¿Qué pasa si el archivo que intentas leer no existe?
- ¿Qué pasa si la conexión a internet falla a mitad de una descarga?

Si no manejas estos escenarios, tu programa **crasheará** (se cerrará abruptamente con un mensaje de error feo). El usuario perderá su trabajo, quedará confundido, y probablemente nunca vuelva a usar tu aplicación.

El bloque `intentar/atrapar` es tu **red de seguridad**. Te permite decir: *"Voy a intentar esto peligroso, pero si falla, tengo un plan B"*.

## Sintaxis Obligatoria

En Latino.NET, **no puedes usar `intentar` sin `atrapar`**. Van siempre juntos, como las dos caras de una moneda.

```latino
intentar
    # Código que podría fallar
    resultado = 10 / 0
atrapar (error)
    # Código que se ejecuta si hubo error
    escribir("Ocurrió un problema: " .. error)
fin
```

**Reglas de oro:**

1. El bloque `intentar` va primero
2. Le sigue **inmediatamente** el bloque `atrapar`
3. La variable entre paréntesis (`error`) contiene el mensaje del error
4. Puedes nombrar la variable como quieras: `(e)`, `(err)`, `(problema)`

## ¿Qué Errores Puedes Atrapar?

Latino.NET protege tu programa contra errores de **tiempo de ejecución** (Runtime Errors). Son los que ocurren cuando el código ya está corriendo, no durante la fase de escritura.

### 1. Errores Matemáticos

La pesadilla de todo programador: la división por cero.

```latino
funcion DividirSeguro(a, b)
    intentar
        resultado = a / b
        retornar resultado
    atrapar (e)
        escribir("⚠️  No se puede dividir por cero")
        retornar nulo
    fin
fin

# Uso
escribir(DividirSeguro(10, 2))   # 5
escribir(DividirSeguro(10, 0))   # nulo (con advertencia)
```

### 2. Errores de Archivos (I/O)

Los archivos son impredecibles: pueden no existir, estar bloqueados por otro programa, o tener permisos restringidos.

```latino
intentar
    contenido = archivo.leer("informe_secreto.txt")
    escribir("Archivo cargado exitosamente")
atrapar (e)
    escribir("❌ No se pudo leer el archivo")
    escribir("   Razón: " .. e)
    # Ejemplo de salida:
    # Salida real: ❌ No se pudo leer el archivo: [Línea X] Archivo no encontrado: 'informe_secreto.txt'
fin
```

### 3. Índices Fuera de Rango

Intentar acceder a una posición que no existe en listas o cadenas.

```latino
lista_corta = ["A", "B", "C"]

intentar
    elemento = lista_corta[10]  # Solo hay 3 elementos (0, 1, 2)
    escribir(elemento)
atrapar (e)
    escribir("❌ Índice inválido: " .. e)
    # Salida real: ❌ Índice inválido: [Línea 4] Índice de lista fuera de rango: 10. La lista tiene 3 elementos.
fin
```

**Prevención vs Reacción:**

```latino
miLista = [10, 20, 30]
indice = 5

# ❌ MALO: Confiar ciegamente (aquí el script ABORTA con un error)
# elemento = miLista[indice]

# ✅ BUENO: Validar primero
si indice >= 0 ý indice < lista.longitud(miLista)
    elemento = miLista[indice]
    escribir("Elemento: " .. elemento)
sino
    escribir("Índice fuera de rango")
fin

# ✅ TAMBIÉN BUENO: Capturar el error
intentar
    elemento = miLista[indice]
    escribir("Elemento: " .. elemento)
atrapar (e)
    escribir("Índice inválido: " .. e)
fin
```

### 4. Tipos Incorrectos (Type Mismatch)

Las funciones de las librerías estándar validan los datos. Si envías el tipo equivocado, te avisan.

```latino
intentar
    # lista.agregar espera (lista, elemento)
    lista.agregar("Esto no es una lista", 42)
atrapar (e)
    escribir("❌ Error de tipo: " .. e)
    # Salida: Se esperaba una lista como primer argumento
fin
```

## ¿Qué NO Puedes Atrapar?

Existen errores que ocurren **antes** de que el programa se ejecute. Estos son errores de **tiempo de análisis** y detendrán el programa antes de que llegue a tu bloque `intentar`.

### Errores de Sintaxis

Código mal escrito que el intérprete no puede entender.

```latino
# ⚠️ No es ejecutable: el error de sintaxis es deliberado y es lo que
# este ejemplo enseña. El bloque 'atrapar' nunca llega a alcanzarse.
intentar
    si (x < 10  # Falta cerrar paréntesis
        escribir("Menor")
    fin
atrapar (e)
    # Este bloque NUNCA se alcanza
fin

# Error del parser:
# SyntaxError: Expected ')' at line 2
```

### Errores Gramaticales

Usar palabras reservadas donde no deberías.

```latino
# ⚠️ Este bloque NO es ejecutable: el error de sintaxis que muestra es
# deliberado y es justamente lo que el ejemplo quiere demostrar.
mi = 10  # ❌ 'mi' es palabra reservada → Error de análisis

pra i en lista  # ❌ 'para' mal escrito → Error de análisis
```

**Regla simple:** Si el código ni siquiera empieza a correr, `intentar/atrapar` no te salvará.

## Patrones de Uso

### Patrón 1: El Silenciador (Ignorar Errores)

A veces, no te importa si algo falla. Solo quieres que el programa continúe.

```latino
# Intentar eliminar un archivo temporal
# Si no existe, no importa
intentar
    archivo.eliminar("temp_cache.log")
atrapar (e)
    # Silencio intencional
fin

escribir("Programa continuando normalmente...")
```

**⚠️ Advertencia:** Usa esto con cuidado. Silenciar errores puede ocultar problemas reales.

### Patrón 2: El Logger (Registrar y Continuar)

Registra el error en un archivo de log para revisión posterior, pero no detengas el programa.

```latino
funcion RegistrarError(mensaje)
    intentar
        timestamp = sis.tiempo()
        # Salida variable (depende de la fecha/hora del sistema)
        # Anexar al archivo: usa archivo.anexar para agregar contenido
        entrada = timestamp .. " - ERROR: " .. mensaje .. "\n"
        archivo.anexar("errores.log", entrada)
    atrapar (e)
        # Si ni siquiera podemos escribir el log, imprimir en consola
        escribir("⚠️  No se pudo escribir en log: " .. e)
    fin
fin

funcion ProcesarArchivos(lista_archivos)
    local procesados = 0
    local errores = 0
    
    para archivo_nombre en lista_archivos
        intentar
            contenido = archivo.leer(archivo_nombre)
            # ... procesar contenido ...
            procesados = procesados + 1
            escribir("✅ " .. archivo_nombre)
        atrapar (e)
            RegistrarError("Fallo al procesar " .. archivo_nombre .. ": " .. e)
            errores = errores + 1
            escribir("❌ " .. archivo_nombre)
        fin
    fin
    
    escribir("\n═══════════════════════════════════")
    escribirf("Procesados: {0}\n", procesados)
    escribirf("Errores:    {0}\n", errores)
    escribir("═══════════════════════════════════")
fin

# Preparación: dos archivos válidos y uno que realmente no existe
archivo.escribir("datos1.txt", "Registro A")
archivo.escribir("datos2.txt", "Registro B")

# Uso
archivos = ["datos1.txt", "datos2.txt", "archivo_inexistente.txt"]
ProcesarArchivos(archivos)

# Higiene: no dejamos residuos
archivo.borrar("datos1.txt")
archivo.borrar("datos2.txt")
archivo.borrar("errores.log")
```

**Salida verificada** (ejecutado con `latino logger.lat`):

```text
✅ datos1.txt
✅ datos2.txt
❌ archivo_inexistente.txt

═══════════════════════════════════
Procesados: 2
Errores:    1
═══════════════════════════════════
```

> 💡 **El tercer archivo es inexistente a propósito.** Ese es el punto del patrón: el logger registra el fallo en `errores.log` y **el programa continúa**. Al terminar puedes revisar el log con `archivo.leer("errores.log")`, que contendrá una línea por cada fallo con esta forma:
>
> ```text
> <fecha-hora-dinámica> - ERROR: Fallo al procesar archivo_inexistente.txt: [Línea N] Archivo no encontrado: 'archivo_inexistente.txt'
> ```
>
> El timestamp viene de `sis.tiempo()`, así que cambia en cada ejecución.
>
> 💡 **Limpieza en el ejemplo.** El patrón escribe en `errores.log` cada vez que falla algo. En un programa real ese archivo es exactamente lo que quieres conservar; en un ejemplo ejecutable hay que borrarlo para no dejar residuos en el directorio del lector.

### Patrón 3: El Recuperador (Valores por Defecto)

Si algo falla, usar un valor seguro para continuar.

```latino
funcion CargarConfiguracion(ruta)
    intentar
        contenido = archivo.leer(ruta)
        valor = anumero(contenido)
        
        si valor == nulo
            escribir("⚠️  Archivo no contiene un número válido")
            retornar 0  # Valor por defecto
        fin
        
        escribir("✅ Configuración cargada: " .. valor)
        retornar valor
        
    atrapar (error)
        escribir("⚠️  No se pudo cargar configuración: " .. error)
        escribir("   Usando valor por defecto (0)")
        retornar 0
    fin
fin

# Uso
timeout = CargarConfiguracion("timeout.cfg")
escribir("Timeout configurado en: " .. timeout .. " segundos")
```

### Patrón 4: El Reintentador (Retry Logic)

Intentar una operación varias veces antes de rendirse.

```latino
funcion ConectarConReintentos(servidor, max_intentos)
    intentos = 0
    
    mientras intentos < max_intentos
        intentos = intentos + 1
        
        escribirf("Intento {0}/{1} conectando a {2}...\n", 
                  intentos, max_intentos, servidor)
        
        intentar
            # Simular conexión (en la realidad usarías una librería de red)
            si servidor == "servidor_caido.com"
                # Forzar error para demostración
                resultado = 10 / 0
            fin
            
            escribir("✅ Conexión exitosa")
            retornar verdadero
            
        atrapar (e)
            escribir("❌ Falló: " .. e)
            
            si intentos < max_intentos
                escribir("   Reintentando en 2 segundos...")
                sis.esperar(2000)
            sino
                escribir("   Máximo de intentos alcanzado")
            fin
        fin
    fin
    
    retornar falso
fin

# Uso
exito = ConectarConReintentos("servidor_caido.com", 3)

si exito
    escribir("\n=== Aplicación Iniciada ===")
sino
    escribir("\n⚠️  No se pudo iniciar. Verifique su conexión.")
    # Terminar el programa reportando fallo (código 1) al shell
    sis.salir(1)
fin
```

> 💡 Integración con scripts y CI/CD:
>Al pasar sis.salir(1), el proceso notifica inmediatamente un estado de fallo a la terminal, lo que permite que herramientas externas (como Bash, PowerShell, Docker o GitHub Actions) detengan la ejecución encadenada de tareas ante un fallo de conexión. Consulta la sección 7.2 Información del Sistema (sis) para más detalles sobre códigos de retorno.

## Propagación de Errores (Bubbling)

Si ocurre un error dentro de una función que no tiene `intentar/atrapar`, el error **"sube"** (se propaga) hasta encontrar un bloque que lo capture.

```latino
funcion Nivel3()
    escribir("   Nivel 3: Haciendo cálculo...")
    resultado = 100 / 0  # ¡ERROR AQUÍ!
    retornar resultado
fin

funcion Nivel2()
    escribir("  Nivel 2: Llamando a Nivel3...")
    retornar Nivel3()  # El error pasa a través de aquí
fin

funcion Nivel1()
    escribir(" Nivel 1: Llamando a Nivel2...")
    retornar Nivel2()  # Y a través de aquí
fin

# El error finalmente se captura aquí
intentar
    escribir("Nivel 0: Iniciando proceso...")
    resultado = Nivel1()
    escribir("Resultado: " .. resultado)
atrapar (err)
    escribir("\n❌ Error capturado en el nivel superior:")
    escribir("   " .. err)
    escribir("\n   El error se originó en Nivel3, pero")
    escribir("   se propagó hasta encontrar un try/catch")
fin
```

**Salida:**

```text
Nivel 0: Iniciando proceso...
 Nivel 1: Llamando a Nivel2...
  Nivel 2: Llamando a Nivel3...
   Nivel 3: Haciendo cálculo...

❌ Error capturado en el nivel superior:
   [Línea X] División por cero.
   
   El error se originó en Nivel3, pero
   se propagó hasta encontrar un try/catch
```

**Lección:** No necesitas `try/catch` en TODAS las funciones. Puedes capturar errores en puntos estratégicos de alto nivel.

## Cuándo NO Usar intentar / atrapar

### 1. Para Control de Flujo Normal

```latino
diccionario = {"edad": 30}

# ❌ MALO: Usar errores para lógica normal
usuario = "sin asignar"
intentar
    usuario = diccionario["usuario"]
atrapar (e)
    usuario = "Anónimo"
fin

# ✅ BUENO: Validar primero
si dic.contiene(diccionario, "usuario")
    usuario = diccionario["usuario"]
sino
    usuario = "Anónimo"
fin
escribir("Usuario: " .. usuario)
```

**Razón:** Los errores son "costosos" (lentos). Las validaciones con `si` son mucho más rápidas.

### 2. Para Errores Predecibles

```latino
# Simula lo que escribe un usuario en un formulario
entrada_usuario = "34"

# ❌ MALO: Sabemos que el usuario puede meter basura
intentar
    edad = anumero(entrada_usuario)
atrapar (e)
    escribir("Por favor ingrese un número")
fin

# ✅ BUENO: Validar con lógica clara
edad = anumero(entrada_usuario)
si edad == nulo
    escribir("Por favor ingrese un número válido")
    edad = 0
fin
escribir("Edad validada: " .. edad)
```

### 3. Para Ocultar Bugs

```latino
# ❌ TERRIBLE: Esconder problemas reales
intentar
    # ... 500 líneas de código ...
atrapar (e)
    # Silencio total
fin

# ✅ BUENO: Capturar selectivamente
intentar
    archivo.leer("opcional.txt")  # Solo este archivo es opcional
atrapar (e)
    escribir("Archivo opcional no encontrado, continuando...")
fin

# ... resto del código SIN try/catch ...
```

**Regla:** Si no sabes cómo manejar un error, déjalo explotar. Es mejor que crashee y lo arregles, a que falle silenciosamente y no sepas por qué.

## 📊 Checklist de Manejo de Errores

Antes de dar por terminada una función crítica, pregúntate:

- [ ] ¿Qué puede salir mal? (Lista todos los escenarios)
- [ ] ¿Puedo prevenir el error con validación? (Hazlo primero)
- [ ] ¿Tengo un plan si el error ocurre? (No silencies sin razón)
- [ ] ¿Estoy registrando el error? (Log o consola)
- [ ] ¿El usuario entiende qué pasó? (Mensaje claro)
- [ ] ¿El programa puede continuar? (O debe detenerse)

## Lanzar Errores Personalizados (`lanzar`)

Hasta ahora, hemos visto cómo atrapar errores que genera el sistema (como divisiones por cero o archivos faltantes). Pero, ¿qué pasa si quieres generar un error tú mismo porque algo no cumple con la lógica de tu negocio?

Para esto existe la función global **`lanzar()`**.

### ¿Para qué sirve?

Sirve para detener la ejecución y enviar un mensaje de error que puede ser capturado por un bloque `intentar/atrapar`. Es ideal para:

- **Validar entradas de usuario** (ej: formato de texto incorrecto).
- **Verificar pre-condiciones** (ej: asegurar que una lista no esté vacía).
- **Asegurar lógica de negocio** (ej: saldo insuficiente o permisos denegados).

🎯 **Ejemplo: Validación Financiera**

```latino
funcion RetirarDinero(saldo, monto)
    si monto < 0
        lanzar("No puedes retirar montos negativos")
    fin
    
    si monto > saldo
        lanzar("Fondos insuficientes. Saldo actual: " .. saldo)
    fin
    
    retornar saldo - monto
fin

intentar
    saldo_nuevo = RetirarDinero(100, 500)
    escribir("Saldo actualizado: " .. saldo_nuevo)
atrapar (e)
    escribir("🛑 OPERACIÓN CANCELADA: " .. e)
fin

# Salida:
# 🛑 OPERACIÓN CANCELADA: [Línea 7] Fondos insuficientes. Saldo actual: 100
```

### Ventajas de usar `lanzar()`

1. **Semántica Universal**: Es el estándar (`throw` / `raise`) que facilita la comunicación entre programadores.
2. **Cohesión Lingüística**: Forma un flujo natural con `intentar` y `atrapar`.
3. **Propagación Automática**: Un error lanzado con `lanzar()` "burbujea" hacia arriba hasta encontrar un capturador, igual que un error nativo.
4. **Múltiples Argumentos**: Puedes pasar varios valores y se concatenarán automáticamente en el mensaje final.

```latino
# Este bloque lanza la excepción a propósito (para demostrar el mensaje)
lanzar("Error fatal en ID:", 1024, "Estado:", "Inactivo")
# Genera: Error fatal en ID: 1024 Estado: Inactivo
```

## 7.4 Expresiones Regulares (Regex)

### ¿Qué Problema Resuelven?

Imagina que tienes que validar 1,000 correos electrónicos escritos por usuarios. Necesitas verificar que:

- Tengan un `@` en el medio
- Tengan al menos un punto después del `@`
- No tengan espacios ni caracteres raros
- El dominio sea válido (`.com`, `.org`, etc.)

**Opción 1 (El camino doloroso):**

```latino
funcion ValidarEmail(email)
    # Buscar @
    si !(cadena.contiene(email, "@"))
        retornar falso
    fin
    
    # Dividir por @
    partes = cadena.separar(email, "@")
    si lista.longitud(partes) != 2
        retornar falso
    fin
    
    # Validar que hay punto después de @
    dominio = partes[1]
    si !(cadena.contiene(dominio, "."))
        retornar falso
    fin
    
    # ... 50 líneas más de validaciones ...
fin
```

**Opción 2 (Con Regex, el camino profesional):**

```latino
funcion ValidarEmail(email)
    patron = "^[\\w\\.-]+@[\\w\\.-]+\\.\\w+$"
    retornar email ~= patron
fin
```

**Una línea.** Eso es el poder de Regex.

### ¿Qué Son las Expresiones Regulares?

Las **Expresiones Regulares** (Regex/RegExp) son un **mini-lenguaje dentro del lenguaje** diseñado específicamente para:

1. **Buscar patrones** → "Encuentra todas las fechas en este documento"
2. **Validar formatos** → "¿Este texto es un número de teléfono válido?"
3. **Extraer información** → "Dame todos los enlaces de esta página web"
4. **Reemplazar inteligentemente** → "Cambia todos los números por su versión en palabras"

**Latino.NET usa el motor Regex de .NET**, uno de los más potentes del mundo, con soporte para:

- Grupos de captura
- Lookaheads/Lookbehinds
- Unicode completo
- Opciones de compilación

## Seguridad: Timeout Automático

Algunos patrones Regex pueden ser **peligrosos**. Por ejemplo, el patrón `(a+)+$` con el texto `"aaaaaaaaaaaaaaaaaX"` puede tardar **minutos** en fallar (esto se llama "ReDoS" - Regex Denial of Service).

**Protección de Latino.NET:**
Todas las operaciones Regex tienen un **timeout de 2 segundos**. Si un patrón tarda más, recibirás este error:

```latino
intentar
    resultado = "texto" ~= "(a+)+"
atrapar (e)
    escribir("Error: " .. e)
    # Salida: Regex timeout (2s)
fin
```

**Esto te protege de:**

- Patrones maliciosos de usuarios
- Bugs en tus propios patrones
- Congelamiento del programa

## Las 3 Herramientas Regex de Latino.NET

Latino.NET te ofrece **tres niveles** de trabajo con Regex, desde lo más simple hasta lo más poderoso.

| Nivel | Herramienta | Propósito | Retorna |
| :--- | :--- | :--- | :--- |
| **1️⃣ Básico** | Operador `~=` | Validación rápida (verdadero/falso) | `verdadero` o `falso` |
| **2️⃣ Intermedio** | `cadena.regexl()` | Validación funcional | `verdadero` o `falso` |
| **3️⃣ Avanzado** | `cadena.regex()` | Extracción de coincidencias | Lista de textos |

## 🎯 NIVEL 1: Operador de Coincidencia (`~=`)

El operador `~=` es **la forma más simple** de usar Regex. Es perfecto para condiciones `si`.

### Sintaxis

```latino
# Firma de la sintaxis (no es un programa ejecutable completo)
texto ~= "patron"
```

- **Lado izquierdo:** El texto a analizar
- **Lado derecho:** El patrón Regex (siempre entre comillas)

🎯 **Ejemplo: Validar Contraseña**

```latino
funcion ContraseñaSegura(pass)
    # Debe tener al menos 8 caracteres, una mayúscula y un número
    tiene_longitud = cadena.longitud(pass) >= 8
    tiene_mayuscula = pass ~= "[A-Z]"
    tiene_numero = pass ~= "\\d"
    
    retornar tiene_longitud ý tiene_mayuscula ý tiene_numero
fin

# Pruebas
escribir(ContraseñaSegura("abc123"))        # falso (no mayúscula)
escribir(ContraseñaSegura("Abc123"))        # falso (muy corta)
escribir(ContraseñaSegura("Abc12345"))      # cierto
```

### Casos de Uso Comunes

#### 1. Detectar si hay números

```latino
texto = "El año es 2026"
si texto ~= "\\d"
    escribir("Contiene números")
fin
```

#### 2. Verificar formato de fecha

```latino
fecha = "22/01/2026"
si fecha ~= "^\\d{2}/\\d{2}/\\d{4}$"
    escribir("Formato válido: DD/MM/YYYY")
sino
    escribir("Formato inválido")
fin
```

#### 3. Detectar correo electrónico (simple)

```latino
email = "usuario@dominio.com"
si email ~= "@.*\\."
    escribir("Parece un correo válido")
fin
```

## 🎯 NIVEL 2: Función Lógica (`cadena.regexl`)

Es **idéntico** al operador `~=`, pero en formato de función. Úsalo cuando:

- Quieras ser más explícito en el código
- Necesites pasar el patrón como variable
- Prefieras el estilo funcional

### Sintaxis (`cadena.regexl`)

```latino
# Firma de la sintaxis (no es un programa ejecutable completo)
cadena.regexl(texto, patron)
```

🎯 **Ejemplo: Validador de Formulario**

```latino
funcion ValidarFormulario(datos)
    # Patrones centralizados
    patron_nombre = "^[A-Za-zÁÉÍÓÚáéíóúÑñ ]{2,50}$"
    patron_telefono = "^\\d{3}-\\d{3}-\\d{4}$"
    patron_codigo_postal = "^\\d{5}$"
    
    errores = []
    
    # Validar nombre
    si !cadena.regexl(datos["nombre"], patron_nombre)
        lista.agregar(errores, "Nombre inválido (solo letras, 2-50 caracteres)")
    fin
    
    # Validar teléfono
    si !cadena.regexl(datos["telefono"], patron_telefono)
        lista.agregar(errores, "Teléfono debe ser: XXX-XXX-XXXX")
    fin
    
    # Validar código postal
    si !cadena.regexl(datos["codigo_postal"], patron_codigo_postal)
        lista.agregar(errores, "Código postal debe tener 5 dígitos")
    fin
    
    retornar errores
fin

# Uso
formulario = {
    "nombre": "Carlos García",
    "telefono": "507-123-5678",
    "codigo_postal": "12345"
}

errores = ValidarFormulario(formulario)

si lista.longitud(errores) == 0
    escribir("✅ Formulario válido")
sino
    escribir("❌ Errores encontrados:")
    para error en errores
        escribir("   - " .. error)
    fin
fin
```

## 🎯 NIVEL 3: Extracción (`cadena.regex`)

Esta es la herramienta **más poderosa**. En lugar de devolver `verdadero/falso`, te da **todas las coincidencias** encontradas en el texto.

### Sintaxis (`cadena.regex`)

```latino
# Firma de la sintaxis (no es un programa ejecutable completo)
lista_resultados = cadena.regex(texto, patron)
```

🎯 **Ejemplo Básico: Extraer Números**

```latino
texto = "Tengo 3 gatos, 2 perros y 15 peces"
numeros = cadena.regex(texto, "\\d+")

escribir("Números encontrados:")
para num en numeros
    escribir("  - " .. num)
fin

# Salida:
# - 3
# - 2
# - 15
```

🎯 **Ejemplo Avanzado: Extraer Información Estructurada**

```latino
funcion ExtraerContactos(texto)
    # Patrón: nombre@dominio.com
    patron_email = "[\\w\\.-]+@[\\w\\.-]+\\.\\w+"
    
    # Patrón: 3 dígitos - 4 dígitos - 4 dígitos
    # (Coincide con: 507-1234-5678)
    patron_telefono = "\\(?\\d{3}\\)?[- ]?\\d{4}[- ]?\\d{4}"
    
    emails = cadena.regex(texto, patron_email)
    telefonos = cadena.regex(texto, patron_telefono)
    
    retornar {
        "emails": emails,
        "telefonos": telefonos
    }
fin

# Uso
documento = "
Contacto 1: juan.perez@empresa.com - Tel: 507-1234-5678
Contacto 2: maria.lopez@ejemplo.org - Tel: (507) 9876-5432
Soporte técnico: soporte@tech.com - Tel: 507 5555 1234
"

contactos = ExtraerContactos(documento)

escribir("=== Emails Encontrados ===")
para email en contactos["emails"]
    escribir("  📧 " .. email)
fin

escribir("\n=== Teléfonos Encontrados ===")
para tel en contactos["telefonos"]
    escribir("  📞 " .. tel)
fin
```

**Salida:**

```text
=== Emails Encontrados ===
  📧 juan.perez@empresa.com
  📧 maria.lopez@ejemplo.org
  📧 soporte@tech.com

=== Teléfonos Encontrados ===
  📞 507-1234-5678
  📞 (507) 9876-5432
  📞 507 5555 1234
```

## 📚 Sintaxis de Regex: Guía Progresiva

### Regla de Oro: La Doble Barra

En Latino.NET, las cadenas usan `\` como carácter de escape (`"\n"` = salto de línea). Pero Regex **también** usa `\` para sus propios símbolos (`\d` = dígito).

**Problema:** Si escribes `"\d"`, Latino interpreta `\d` como un escape desconocido.

**Solución:** Usa **doble barra** `"\\d"` para que Latino envíe `\d` al motor Regex.

| Símbolo Regex | En Latino | Significado |
| :--- | :--- | :--- |
| `\d` | `"\\d"` | Un dígito |
| `\w` | `"\\w"` | Una letra/número/_ |
| `\s` | `"\\s"` | Un espacio |
| `\.` | `"\\."` | Un punto literal |
| `\\` | `"\\\\"` | Una barra invertida literal |

### 1️⃣ Caracteres Literales

Los caracteres normales coinciden consigo mismos.

```latino
"gato" ~= "gato"  # cierto
"gato" ~= "perro"     # falso
"gato" ~= "Gato"      # falso (case-sensitive)
```

### 2️⃣ Metacaracteres Básicos

| Símbolo | Significado | Ejemplo | Coincide |
| :--- | :--- | :--- | :--- |
| `.` | Cualquier carácter | `"g.to"` | "gato", "g3to", "g@to" |
| `^` | Inicio de línea | `"^Hola"` | "Hola mundo" (✅), "Dice Hola" (❌) |
| `$` | Fin de línea | `"fin$"` | "El fin" (✅), "fin del mundo" (❌) |
| `\|` | O (alternativa) | `"gato\|perro"` | "gato" o "perro" |

**Ejemplo combinado:**

```latino
# Validar que una línea SOLO contenga "SI" o "NO"
respuesta = "SI"
si respuesta ~= "^(SI\|NO)$"
    escribir("Respuesta válida")
fin
```

### 3️⃣ Clases de Caracteres

#### Clases Predefinidas

| Clase | Significado | Equivalente a |
| :--- | :--- | :--- |
| `\\d` | Dígito | `[0-9]` |
| `\\D` | NO dígito | `[^0-9]` |
| `\\w` | Palabra (letra, número, _) | `[a-zA-Z0-9_]` |
| `\\W` | NO palabra | `[^a-zA-Z0-9_]` |
| `\\s` | Espacio en blanco | Espacio, Tab, Salto |
| `\\S` | NO espacio | Cualquier cosa visible |

🎯 **Ejemplo: Detectar si hay solo letras**

```latino
texto = "Hola"
si texto ~= "^\\w+$"
    escribir("Solo caracteres de palabra")
fin
```

#### Clases Personalizadas `[...]`

Define tu propio conjunto de caracteres válidos.

```latino
# Solo vocales
"aeiou" ~= "[aeiou]"  # cierto (tiene 'a')

# Rango de números
"5" ~= "[0-9]"  # cierto

# Rango de letras
"m" ~= "[a-z]"  # cierto

# Combinación
"B" ~= "[A-Za-z]"  # cierto

# Negación (TODO excepto números)
"a" ~= "[^0-9]"  # cierto
```

🎯 **Ejemplo: Validar código hexadecimal**

```latino
codigo = "A3F"
si codigo ~= "^[0-9A-Fa-f]+$"
    escribir("Código hex válido")
fin
```

### 4️⃣ Cuantificadores (Repetición)

Controlan **cuántas veces** debe aparecer el patrón anterior.

| Símbolo | Cantidad | Ejemplo | Coincide |
| :--- | :--- | :--- | :--- |
| `*` | 0 o más | `"ja*"` | "j", "ja", "jaaaa" |
| `+` | 1 o más | `"ja+"` | "ja", "jaaaa" (NO "j") |
| `?` | 0 o 1 (opcional) | `"colou?r"` | "color", "colour" |
| `{n}` | Exactamente n | `"\\d{4}"` | "2026" |
| `{n,}` | n o más | `"\\d{2,}"` | "12", "123", "1234" |
| `{n,m}` | Entre n y m | `"\\d{2,4}"` | "12", "123", "1234" |

**Ejemplos prácticos:**

```latino
# Código postal de 5 dígitos
"12345" ~= "^\\d{5}$"  # cierto

# Teléfono (3 dígitos - 4 dígitos)
"123-4567" ~= "^\\d{3}-\\d{4}$"  # cierto

# Contraseña de 8-16 caracteres
"MiClave123" ~= "^.{8,16}$"  # cierto

# URL (opcional www)
"ejemplo.com" ~= "^(www\\.)?.*\\.com$"  # cierto
"www.ejemplo.com" ~= "^(www\\.)?.*\\.com$"  # cierto
```

### 5️⃣ Grupos y Captura `(...)`

Los paréntesis agrupan partes del patrón.

```latino
# Agrupar para alternancia
"gato" ~= "^(gato|perro)$"  # cierto
"perro" ~= "^(gato|perro)$"  # cierto

# Agrupar con cuantificadores
"jajaja" ~= "^(ja)+$"  # cierto (repite el grupo)
```

### 6️⃣ Escapar Caracteres Especiales

Si necesitas buscar un carácter que es especial en Regex (como `.`, `*`, `+`), debes escaparlo.

```latino
# Buscar un punto literal
"3.14" ~= "\\."  # cierto

# Buscar un signo +
"2+2" ~= "\\+"  # cierto

# Buscar paréntesis literales
"(2026)" ~= "\\(\\d{4}\\)"  # cierto
```

## 📝 Ejercicios Prácticos

### Ejercicio 1: Validador de Fecha (DD/MM/YYYY)

```latino
funcion ValidarFecha(fecha)
    # Patrón: 2 dígitos / 2 dígitos / 4 dígitos
    patron = "^\\d{2}/\\d{2}/\\d{4}$"
    retornar fecha ~= patron
fin

# Pruebas
escribir(ValidarFecha("22/01/2026"))    # cierto
escribir(ValidarFecha("1/1/2026"))      # falso (falta padding)
escribir(ValidarFecha("22-01-2026"))    # falso (guion en vez de /)
```

### Ejercicio 2: Extraer URLs de un Texto

```latino
funcion ExtraerURLs(texto)
    # Patrón simple para http(s)://dominio.com
    patron = "https?://[\\w\\.-]+\\.[a-z]{2,}"
    retornar cadena.regex(texto, patron)
fin

# Uso
articulo = "
Visita https://latino-lang.org para más info.
También puedes ir a http://docs.ejemplo.com
o revisar https://github.com/latino-lang
"

urls = ExtraerURLs(articulo)

escribir("URLs encontradas:")
para url en urls
    escribir("  🔗 " .. url)
fin
```

**Salida:**

```text
URLs encontradas:
  🔗 https://latino-lang.org
  🔗 http://docs.ejemplo.com
  🔗 https://github.com
```

### Ejercicio 3: Validar Tarjeta de Crédito (Básico)

```latino
funcion ValidarTarjeta(numero)
    # Eliminar espacios y guiones
    limpio = cadena.reemplazar(numero, " ", "")
    limpio = cadena.reemplazar(limpio, "-", "")
    
    # 16 dígitos, empezando con 4, 5 o 6
    patron = "^[456]\\d{15}$"
    
    retornar limpio ~= patron
fin

# Pruebas
escribir(ValidarTarjeta("4111 1111 1111 1111"))  # cierto (Visa)
escribir(ValidarTarjeta("5500 0000 0000 0004"))  # cierto (Mastercard)
escribir(ValidarTarjeta("3782 822463 10005"))    # falso (Amex, 15 dígitos)
```

### Ejercicio 4: Censurar Palabras

```latino
funcion CensurarTexto(texto, palabras_prohibidas)
    resultado = texto

    para palabra en palabras_prohibidas
        asteriscos = cadena.rellenar_derecha("", "*", cadena.longitud(palabra))
        resultado = cadena.reemplazar(resultado, palabra, asteriscos)
    fin

    retornar resultado
fin

# Uso
mensaje = "Este mensaje contiene contenido inapropiado y spam"
prohibidas = ["spam", "inapropiado"]

limpio = CensurarTexto(mensaje, prohibidas)
escribir(limpio)
# Salida: Este mensaje contiene contenido *********** y ****
```

## 📖 Cheat Sheet: Patrones Comunes

### Validación de Datos

| Caso de Uso | Patrón | Ejemplo |
| :--- | :--- | :--- |
| **Email** | `^[\\w\\.-]+@[\\w\\.-]+\\.\\w+$` | `user@domain.com` |
| **Teléfono (507-XXXX-XXXX)** | `^\\d{3}-\\d{4}-\\d{4}$` | `507-1234-5678` |
| **Código Postal (5 dígitos)** | `^\\d{5}$` | `12345` |
| **URL** | `^https?://[\\w\\.-]+` | `https://ejemplo.com` |
| **IPv4** | `^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$` | `192.168.1.1` |
| **Fecha DD/MM/YYYY** | `^\\d{2}/\\d{2}/\\d{4}$` | `22/01/2026` |
| **Hora 24h (HH:MM)** | `^([01]\\d\|2[0-3]):[0-5]\\d$` | `14:30` |
| **Hexadecimal** | `^[0-9A-Fa-f]+$` | `A3F` |
| **Contraseña fuerte** | `^(?=.*[A-Z])(?=.*\\d).{8,}$` | `Abc12345` |

### Extracción de Datos

| Caso de Uso | Patrón | Descripción |
| :--- | :--- | :--- |
| **Todos los números** | `\\d+` | Uno o más dígitos |
| **Palabras** | `\\w+` | Secuencias de letras/números |
| **Emails** | `[\\w\\.-]+@[\\w\\.-]+` | Direcciones de correo |
| **Menciones (@usuario)** | `@\\w+` | Menciones estilo Twitter |
| **Hashtags** | `#\\w+` | Hashtags |
| **Precios ($XX.XX)** | `\\$\\d+\\.\\d{2}` | Montos en dólares |

## 🔧 Errores Comunes y Soluciones

### Error 1: Olvidar la Doble Barra

```latino
# ❌ INCORRECTO
"123" ~= "\d+"           # Latino intenta interpretar \d

# ✅ CORRECTO
"123" ~= "\\d+"          # Latino envía \d al motor Regex
```

### Error 2: Anclas Faltantes

```latino
# ❌ PROBLEMA: Acepta más de lo esperado
"abc123xyz" ~= "\\d+"  # cierto (encuentra 123 en el medio)

# ✅ SOLUCIÓN: Usar ^ y $ para validación estricta
"abc123xyz" ~= "^\\d+$"  # falso (no es SOLO números)
"123" ~= "^\\d+$"  # cierto
```

### Error 3: No Escapar Puntos

```latino
# ❌ INCORRECTO: El punto es "cualquier carácter"
"192.168.1.1" ~= "\\d+.\\d+"  # Coincide con "192X168Y1Z1"

# ✅ CORRECTO: Escapar el punto literal
"192.168.1.1" ~= "\\d+\\.\\d+"  # Solo coincide con punto real
```

### Error 4: Cuantificadores Codiciosos

```latino
# Problema: .* es "codicioso" (toma todo lo posible)
html = "<div>Hola</div><div>Mundo</div>"
tags = cadena.regex(html, "<div>.*</div>")
# Resultado: ["<div>Hola</div><div>Mundo</div>"]  (¡todo!)

# Solución: Usar .*? (no codicioso)
tags = cadena.regex(html, "<div>.*?</div>")
# Resultado: ["<div>Hola</div>", "<div>Mundo</div>"]  (separados)
```

## 🔗 Recursos Adicionales

Latino.NET usa el motor estándar de .NET, así que cualquier recurso de Regex para C# es 100% compatible:

- **[Regex101](https://regex101.com/)** - Probador interactivo (selecciona "C#" como flavor)
- **[Documentación .NET Regex](https://learn.microsoft.com/es-es/dotnet/standard/base-types/regular-expressions)** - Referencia oficial completa
- **[RegexOne](https://regexone.com/)** - Tutorial interactivo para principiantes
- **[Debuggex](https://www.debuggex.com/)** - Visualizador de patrones (modo JavaScript similar)

## 7.5 Buenas Prácticas y Convenciones

### ¿Por Qué Importan los Estándares?

Imagina que estás leyendo un libro donde:

- Cada capítulo usa un tipo de letra diferente
- Algunos párrafos están justificados a la izquierda, otros a la derecha
- Los nombres de personajes cambian de mayúsculas a minúsculas aleatoriamente
- No hay espacios consistentes entre líneas

Técnicamente, el libro es legible. Pero tu cerebro tardaría **3 veces más** en entenderlo, y probablemente abandonarías en el capítulo 2.

**El código es igual.** Dos programas pueden hacer exactamente lo mismo, pero uno será:

- 10 veces más fácil de leer
- 5 veces más rápido de mantener
- 100% menos frustrante para otros desarrolladores

Los **estándares de código** no son caprichos estéticos. Son **herramientas de comunicación** entre personas que escriben y leen código.

## La Filosofía de Latino.NET

Latino.NET adopta las **convenciones de C# (.NET)** por tres razones estratégicas:

1. **Coherencia con la plataforma** → Latino.NET corre sobre .NET, usar su estándar facilita la integración
2. **Familiaridad para desarrolladores modernos** → Millones de programadores ya conocen este estilo
3. **Tooling maduro** → Analizadores de código, formateadores y linters de .NET funcionan con estas reglas

**Principio rector:** *"El código se lee 10 veces más de lo que se escribe, optimiza para la lectura."*

## 📋 Tabla de Convenciones Oficial

| Elemento | Convención | Descripción | ✅ Correcto | ❌ Incorrecto |
| :--- | :--- | :--- | :--- | :--- |
| **Clases** | `PascalCase` | Primera letra mayúscula, sin espacios | `Usuario` | `usuario`, `USUARIO` |
| **Funciones de usuario** | `PascalCase` | Primera letra mayúscula | `CalcularTotal()` | `calcular_total()`, `calcularTotal()` |
| **Métodos de clases** | `PascalCase` | Primera letra mayúscula | `CalcularTotal()` | `calcularTotal()`, `calcular_total()` |
| **Variables** | `camelCase` | Primera minúscula, siguientes mayúsculas | `nombreCompleto` | `NombreCompleto`, `nombre_completo` |
| **Constantes** | `UPPER_CASE` | Todo mayúsculas con guiones bajos | `MAX_INTENTOS` | `maxIntentos`, `MaxIntentos` |
| **Archivos** | `snake_case` | Minúsculas con guiones bajos | `mi_programa.lat` | `MiPrograma.lat`, `mi-programa.lat` |
| **Privados (Advertencia)** | `_camelCase` | Prefijo guion bajo | `_datosInternos` | `datosInternos` |

## 🎯 Reglas Detalladas

> 📝 **Sobre los ejemplos de esta sección:** son **esquemas ilustrativos**, no programas ejecutables. Muestran la *forma* que deben tener los nombres (clases, funciones, archivos) y los antipatrones que debes evitar. Copiarlos en un archivo `.lat` y ejecutarlos no funcionará, porque faltan las declaraciones, los cuerpos de las funciones y los datos de contexto.

### 1. Clases: `PascalCase`

Las clases representan **conceptos** o **entidades**. Su nombre debe ser un sustantivo claro.

```latino
# ✅ BIEN
clase Usuario
clase GestorDeArchivos
clase CalculadoraImpuestos

# ❌ MAL
clase usuario              # Minúscula
clase gestor_archivos      # Snake case
clase CALCULADORA          # Todo mayúsculas
```

**Reglas adicionales:**

- Usa singular: `Usuario`, no `Usuarios`
- Evita abreviaturas oscuras: `GestorArchivo`, no `GesArc`
- Si el nombre es compuesto, une las palabras: `CuentaBancaria`, no `Cuenta_Bancaria`

### 2. Métodos y Funciones: `PascalCase`

Los métodos son **acciones**, deben empezar con un verbo.

```latino
# ✅ BIEN
funcion CalcularDescuento(precio, porcentaje)
funcion ValidarEmail(email)
funcion ObtenerUsuarios()

# ❌ MAL
funcion calcularDescuento()    # Minúscula inicial
funcion VALIDAR_EMAIL()        # Mayúsculas sostenidas
funcion usuarios()             # No es verbo (sustantivo solo)
```

**Verbos comunes:**

- `Calcular`, `Validar`, `Obtener`, `Guardar`, `Eliminar`
- `Crear`, `Actualizar`, `Buscar`, `Verificar`, `Procesar`

**Métodos booleanos (retornan verdadero/falso):**

- Prefijos: `Es`, `Tiene`, `Puede`, `Debe`
- Ejemplos: `EsValido()`, `TienePermisos()`, `PuedeEditar()`

### 3. Variables: `camelCase`

Las variables almacenan **datos**, deben ser descriptivas.

```latino
# ✅ BIEN
nombreUsuario = "Carlos"
edadMinima = 18
totalConImpuesto = 125.50

# ❌ MAL
NombreUsuario = "Carlos"    # PascalCase (parece clase)
edad_minima = 18            # Snake case
t = 125.50                  # Abreviatura críptica
```

**Guías:**

- Usa nombres completos: `temperatura`, no `temp` (excepto en loops: `i`, `j`)
- Variables booleanas: Usa prefijos `es`, `tiene`, `debe`
  - `esValido = verdadero`
  - `tienePermiso = falso`

### 4. Constantes: `UPPER_CASE`

Las constantes son valores que **nunca cambian**. Su nombre grita "¡NO ME MODIFIQUES!".

```latino
# ✅ BIEN
PI = 3.14159
MAX_INTENTOS = 3
TIMEOUT_SEGUNDOS = 30
RUTA_BASE = "/home/usuario/app"

# ❌ MAL
pi = 3.14159                # Parece variable
MaxIntentos = 3             # Parece clase
timeout_Segundos = 30       # Mezcla de estilos
```

**⚠️ Regla crítica:** Una vez asignadas, **NUNCA** reasignes constantes en tu código:

```latino
# ⚠️ El primer par de líneas NO es ejecutable: la reasignación de la
# constante es el error que el ejemplo quiere provocar.
MAX_USUARIOS = 100
# ... 200 líneas después ...
MAX_USUARIOS = 200  # ❌ Contradicción: el motor lanza
                    #    "No se puede reasignar la constante"

# ✅ Si necesitas cambiar el valor, no es constante
limiteUsuarios = 100
# ... más tarde ...
limiteUsuarios = 200  # OK, es variable
```

### 5. Archivos: `snake_case`

Los nombres de archivo siguen la convención Unix: minúsculas separadas por guiones bajos.

```latino
# ✅ BIEN
mi_programa.lat
utilidades_matematicas.lat
gestor_base_datos.lat

# ❌ MAL
MiPrograma.lat              # PascalCase
mi-programa.lat             # Guiones (problemático en algunos sistemas)
miPrograma.lat              # camelCase
```

**Razones:**

- **Compatibilidad:** Algunos sistemas de archivos distinguen mayúsculas, otros no
- **Legibilidad:** Es más fácil leer `gestor_archivos` que `GestorArchivos` en listados de terminal
- **Convención universal:** Python, Ruby, Rust y la mayoría de lenguajes modernos usan este estilo

### 6. Miembros Privados: `_camelCase` (Advertencia)

El prefijo `_` indica que una variable o método es **interno** de la clase y no debe usarse desde fuera.

```latino
clase CuentaBancaria
    constructor(saldo_inicial)
        mi._saldo = saldo_inicial        # Privado
        mi._historial = []               # Privado
    fin
    
    funcion Depositar(cantidad)
        mi._saldo = mi._saldo + cantidad
        mi._RegistrarTransaccion("DEPOSITO", cantidad)
    fin
    
    funcion _RegistrarTransaccion(tipo, cantidad)  # Privado
        transaccion = {"tipo": tipo, "cantidad": cantidad}
        lista.agregar(mi._historial, transaccion)
    fin
fin
```

> **⚠️ Importante:** En Latino.NET, el prefijo `_` es una **convención visual**. El lenguaje no bloquea físicamente el acceso directo por punto (`objeto._saldo` sigue siendo accesible), pero actúa como un pacto de no agresión: indica que es un miembro interno y que manipularlo desde fuera puede romper la lógica del objeto.
>
> > 💡 **¿Necesitas privacidad estricta y blindada?**  
> > Si requieres que un dato sea **matemáticamente inaccesible** desde el exterior (donde ni siquiera conociendo el nombre se pueda leer o alterar desde fuera del objeto), consulta el **Capítulo 5: Programación Orientada a Objetos (§5.8)** para implementar el **Patrón Bóveda** mediante clausuras léxicas con variables `local` dentro del constructor.

🎯 **Ejemplo Completo: Sistema Bancario**

Aplicando todas las convenciones en un programa realista:

```latino
# Archivo: sistema_bancario.lat

# ═══════════════════════════════════════════════════════════
#  SISTEMA BANCARIO - Ejemplo de Convenciones
# ═══════════════════════════════════════════════════════════

# CONSTANTES GLOBALES (UPPER_CASE)
MAX_RETIRO_DIARIO = 10000
TASA_INTERES_ANUAL = 0.05
COMISION_TRANSFERENCIA = 2.50

# CLASE (PascalCase)
clase CuentaBancaria
    # CONSTRUCTOR
    constructor(numeroCuenta, titular)
        # Variables de instancia (camelCase)
        mi.numeroCuenta = numeroCuenta
        mi.titular = titular
        
        # Miembros privados (_camelCase)
        mi._saldo = 0
        mi._transacciones = []
        mi._intentosRetiroHoy = 0
    fin
    
    # MÉTODOS PÚBLICOS (PascalCase + Verbo)
    funcion Depositar(cantidad)
        # Variables locales (camelCase)
        esValido = cantidad > 0
        
        si esValido
            mi._saldo = mi._saldo + cantidad
            mi._RegistrarTransaccion("DEPOSITO", cantidad)
            
            escribirf("✅ Depósito exitoso: ${0:F2}\n", cantidad)
            retornar verdadero
        sino
            escribir("❌ Error: La cantidad debe ser positiva")
            retornar falso
        fin
    fin
    
    funcion Retirar(cantidad)
        # Validaciones paso a paso
        excedeLimite = cantidad > MAX_RETIRO_DIARIO
        saldoInsuficiente = cantidad > mi._saldo
        
        si saldoInsuficiente
            escribir("❌ Error: Fondos insuficientes")
            retornar falso
        osi excedeLimite
            escribirf("❌ Error: Límite diario ${0:F2}\n", MAX_RETIRO_DIARIO)
            retornar falso
        sino
            mi._saldo = mi._saldo - cantidad
            mi._intentosRetiroHoy = mi._intentosRetiroHoy + 1
            mi._RegistrarTransaccion("RETIRO", cantidad)
            
            escribirf("✅ Retiro exitoso: ${0:F2}\n", cantidad)
            retornar verdadero
        fin
    fin
    
    funcion ObtenerSaldo()
        retornar mi._saldo
    fin
    
    funcion MostrarResumen()
        escribir("\n╔════════════════════════════════════╗")
        escribir("║       RESUMEN DE CUENTA            ║")
        escribir("╠════════════════════════════════════╣")
        escribirf("║ Titular:  {0,-24} ║\n", mi.titular)
        escribirf("║ Cuenta:   {0,-24} ║\n", mi.numeroCuenta)
        escribirf("║ Saldo:    ${0,23:F2} ║\n", mi._saldo)
        escribir("╚════════════════════════════════════╝")
    fin
    
    # MÉTODOS PRIVADOS (_PascalCase)
    funcion _RegistrarTransaccion(tipo, cantidad)
        transaccion = {
            "tipo": tipo,
            "cantidad": cantidad,
            "fecha": sis.tiempo(),
            "saldo_resultante": mi._saldo
        }
        
        lista.agregar(mi._transacciones, transaccion)
    fin
    
    funcion _TienePermisoRetiro()
        # Método interno de validación
        retornar mi._intentosRetiroHoy < 3
    fin
fin

# ═══════════════════════════════════════════════════════════
#  FUNCIÓN PRINCIPAL (PascalCase)
# ═══════════════════════════════════════════════════════════

funcion EjecutarDemo()
    escribir("╔════════════════════════════════════════╗")
    escribir("║    SISTEMA BANCARIO v1.0               ║")
    escribir("╚════════════════════════════════════════╝\n")
    
    # Variables locales (camelCase)
    cuentaAna = nueva CuentaBancaria("12345", "Ana García")
    montoInicial = 5000
    montoRetiro = 1000
    
    # Operaciones
    cuentaAna.Depositar(montoInicial)
    cuentaAna.Retirar(montoRetiro)
    
    # Mostrar estado final
    cuentaAna.MostrarResumen()
fin

# ═══════════════════════════════════════════════════════════
#  PUNTO DE ENTRADA
# ═══════════════════════════════════════════════════════════

EjecutarDemo()
```

**Observaciones del código:**

1. ✅ Todas las clases en `PascalCase`: `CuentaBancaria`
2. ✅ Todos los métodos en `PascalCase`: `Depositar`, `MostrarResumen`
3. ✅ Variables en `camelCase`: `cuentaAna`, `montoInicial`, `esValido`
4. ✅ Constantes en `UPPER_CASE`: `MAX_RETIRO_DIARIO`
5. ✅ Miembros privados con `_`: `_saldo`, `_RegistrarTransaccion`
6. ✅ Variables booleanas descriptivas: `esValido`, `excedeLimite`

## Anti-Patrones: Lo Que NO Hacer

### 1. Nombres Crípticos o Vagos

```latino
# ❌ TERRIBLE
funcion p(x, y)
    z = x + y
    retornar z
fin

# ✅ EXCELENTE
funcion CalcularTotal(subtotal, impuesto)
    total = subtotal + impuesto
    retornar total
fin
```

**Excepciones permitidas:**

- Contadores de loops: `i`, `j`, `k`
- Variables matemáticas universales: `x`, `y` (en contexto claro)

### 2. Abreviaturas No Estándar

```latino
# ❌ MAL
clase GstUsr    # ¿Gestor Usuario? ¿Guest User?
funcion CalcImp()  # ¿Calcula impuestos o importa?

# ✅ BIEN
clase GestorUsuarios
funcion CalcularImpuesto()
```

**Abreviaturas aceptadas universalmente:**

- `Id` (Identificador): `usuarioId`
- `Url` (Uniform Resource Locator): `urlBase`
- `Html` (HyperText Markup Language): `contenidoHtml`

### 3. Mezcla de Idiomas

```latino
# ❌ CONFUSO
clase UserGestor
funcion DeleteUsuario()
variable nombreUser

# ✅ CONSISTENTE (Todo español)
clase GestorUsuarios
funcion EliminarUsuario()
variable nombreUsuario
```

### 4. Notación Húngara (Obsoleta)

```latino
# ❌ ANTIGUO (estilo C de los 90s)
strNombre = "Carlos"
intEdad = 30
boolActivo = verdadero

# ✅ MODERNO
nombre = "Carlos"
edad = 30
activo = verdadero
```

**Por qué está obsoleto:** Los lenguajes modernos tienen sistemas de tipos y editores inteligentes que muestran el tipo. Prefijar el tipo es redundante y hace el código más difícil de leer.

### 5. Comentarios Obvios

```latino
# Esquema ilustrativo (no ejecutable: '...' representa código omitido)
# ❌ RUIDO
# Incrementar i en 1
i = i + 1

# ❌ REDUNDANTE
funcion CalcularTotal()
    # Esta función calcula el total
    # ...
fin

# ✅ ÚTIL (Explica el POR QUÉ, no el QUÉ)
# Usamos tolerancia de 0.01 para evitar errores de redondeo flotante
# (el valor absoluto vive en la librería estándar 'mate': mate.abs)
si mate.abs(diferencia) < 0.01
```

## 📝 Checklist de Código Limpio

Antes de dar por terminado tu archivo `.lat`, verifica:

### Nombres

- [ ] ¿Las clases están en `PascalCase`?
- [ ] ¿Las funciones empiezan con verbos y están en `PascalCase`?
- [ ] ¿Las variables están en `camelCase` y son descriptivas?
- [ ] ¿Las constantes están en `UPPER_CASE`?
- [ ] ¿Los miembros privados tienen prefijo `_`?

### Legibilidad

- [ ] ¿Podrías entender el código en 6 meses sin recordar qué hace?
- [ ] ¿Los nombres tienen al menos 3 caracteres (excepto `i`, `j`)?
- [ ] ¿Evitaste abreviaturas oscuras?
- [ ] ¿El código está en un solo idioma (español)?

### Estructura

- [ ] ¿Las funciones hacen UNA cosa y la hacen bien?
- [ ] ¿Ninguna función tiene más de 50 líneas? (señal de que debe dividirse)
- [ ] ¿Los bloques `si/mientras/para` están correctamente indentados?

### Comentarios

- [ ] ¿Los comentarios explican el POR QUÉ, no el QUÉ?
- [ ] ¿Eliminaste comentarios obsoletos o código comentado?

## Guía de Buenas Prácticas de Nomenclatura

> *"Solo hay dos cosas difíciles en Ciencias de la Computación: invalidación de caché y nombrar cosas."*  
> — Phil Karlton

### Guía de Nombres para Conceptos Difíciles

| Concepto | ❌ Nombre Vago | ✅ Nombre Claro |
| :--- | :--- | :--- |
| Una lista de usuarios | `datos` | `listaUsuarios` |
| Resultado de validación | `ok` | `esValido` |
| Contador de intentos | `c` | `intentosFallidos` |
| Fecha de vencimiento | `fecha` | `fechaVencimiento` |
| Precio sin impuesto | `precio` | `precioSinImpuesto` |
| ID temporal | `temp` | `idTemporal` |

### Técnica: La Prueba de Lectura en Voz Alta

Lee tu código como si fuera prosa. Si suena natural, está bien nombrado:

```latino
# ✅ Se lee naturalmente
si usuarioEstaAutenticado ý tienePermisosAdmin
    MostrarPanelControl()
fin

# ❌ Suena robótico
si usrAuth y prmAdm
    ShowPanel()
fin
```

## 🌍 Integración con el Ecosistema Latino

### Convenciones en Librerías Estándar

Las librerías de Latino.NET ya siguen estos estándares:

```latino
# Librerías estándar en minúsculas
cadena.longitud()
lista.agregar()
sis.tiempo()

# Funciones globales en camelCase o PascalCase
escribir()
anumero()
tipo()

# Constantes en UPPER_CASE (si las hubiera en futuras versiones)
PI
MAX_STACK
```

## 7.6 Referencias a Librerías (Alias y Atajos)

A medida que tus programas crecen, escribir nombres largos de librerías repetidamente puede volverse tedioso:

```latino
# Catálogo de llamadas repetidas (ilustrativo, no ejecutable: 'fecha',
# 'inicio' y 'fin' serían datos declarados antes)
lapsos.ahora()
lapsos.formato(fecha, "dd/MM/yyyy")
lapsos.sumar(inicio, 30, "dias")
lapsos.diferencia(inicio, fin)
```

Latino.NET te permite crear **referencias cortas** (alias) a cualquier librería u objeto, haciendo tu código más limpio y legible.

### ¿Cómo Funciona Esto?

En Latino.NET, las librerías son **objetos normales** almacenados en variables globales. Cuando escribes:

```latino
incluir("lapsos")
```

El sistema hace internamente algo equivalente a:

```latino
# Esquema interno — no es código escribible.
# Solo ilustra qué hay detrás de incluir("lapsos").
lapsos = <objeto con funciones de tiempo>
```

Dado que `lapsos` es simplemente una variable global que apunta a un objeto, puedes crear **otra variable** que apunte al mismo objeto:

```latino
incluir("lapsos")

# Creamos una segunda referencia al mismo objeto
f = lapsos

# Ahora 'f' y 'lapsos' apuntan a lo mismo
escribir(f.ahora())      # ✅ Funciona
escribir(lapsos.ahora()) # ✅ También funciona
```

**Concepto clave:** No estás "copiando" la librería. Estás creando una **segunda etiqueta** que apunta al mismo contenido en memoria.

**Analogía:** Es como tener dos controles remotos para el mismo televisor. Ambos controlan el mismo aparato.

#### Sintaxis: Cargar y Referenciar

Para crear un alias de una librería externa (plugin), sigue este orden:

- **Paso 1: Cargar la librería**

```latino
incluir("lapsos")  # Carga el plugin en memoria
```

- **Paso 2: Crear la referencia corta**

```latino
f = lapsos  # 'f' ahora es un atajo a 'lapsos'
```

- **Paso 3: Usar el atajo**

```latino
# Continuación del ejemplo del alias (requiere incluir("lapsos") y f = lapsos)
hoy = f.ahora()
escribir(f.formato(hoy, "dd/MM/yyyy"))
```

#### 📝 Error Común: Asignación Directa

**❌ Incorrecto:**

```latino
f = incluir("lapsos")  # f será 'nulo'
```

**¿Por qué falla?**

La función `incluir()` realiza una **acción** (cargar el archivo), pero **no retorna nada** (`nulo`). La librería queda disponible como variable global con su nombre original.

**✅ Correcto:**

```latino
incluir("lapsos")  # Carga la librería (disponible como 'lapsos')
f = lapsos         # Ahora sí creamos el alias
```

#### Alias para Librerías Integradas

No necesitas `incluir()` para las librerías que vienen con Latino.NET (como `mate`, `cadena`, `lista`). Están siempre disponibles:

```latino
# Alias para Matemáticas
m = mate

escribir(m.pi)          # 3.141592653589793
escribir(m.pot(2, 3))   # 8
escribir(m.raiz(16))    # 4

# Alias para Cadenas
s = cadena

texto = "hola mundo"
escribir(s.mayusculas(texto))  # "HOLA MUNDO"
escribir(s.longitud(texto))    # 10

# Alias para Listas
l = lista

numeros = [1, 2, 3]
l.agregar(numeros, 4)
escribir(numeros)  # [1, 2, 3, 4]
```

**Ventaja:** Si tu editor de código tiene autocompletado largo, escribir `m.` es mucho más rápido que `mate.`.

#### Caso Práctico: Sistema de Gestión de Fechas

Imagina una rutina o servicio que calcula fechas de vencimiento. Usar un alias hace el código más compacto y profesional:

**Sin alias (Verboso):**

```latino
incluir("lapsos")

hoy = lapsos.ahora()
vencimiento = lapsos.sumar(hoy, 30, "dias")
diferencia = lapsos.diferencia(hoy, vencimiento)
formato = lapsos.formato(vencimiento, "dd/MM/yyyy")
```

**Con alias (Limpio):**

```latino
incluir("lapsos")
f = lapsos  # 'f' de fecha/tiempo

hoy = f.ahora()
vencimiento = f.sumar(hoy, 30, "dias")
diferencia = f.diferencia(hoy, vencimiento)
formato = f.formato(vencimiento, "dd/MM/yyyy")
```

**Ejemplo completo:**

```latino
# Cargar y crear alias
incluir("lapsos")
f = lapsos

# Fecha actual
hoy = f.ahora()

# Calcular vencimiento (30 días después)
vencimiento = f.sumar(hoy, 30, "dias")

# Formato de visualización
fmt = "dd/MM/yyyy"

# Reporte
escribir("=== CÁLCULO DE VENCIMIENTO ===")
escribir("Hoy:        " .. f.formato(hoy, fmt))
escribir("Vence en:   " .. f.formato(vencimiento, fmt))
escribir("")

# Validación adicional
año = 2028
si f.esBisiesto(año)
    escribir("✅ El año " .. año .. " es bisiesto (366 días)")
sino
    escribir("📅 El año " .. año .. " NO es bisiesto (365 días)")
fin

# Días transcurridos
# 'info_diferencia' es un Diccionario completo
info_diferencia = f.diferencia(hoy, vencimiento)

# Accedemos a la clave específica que necesitamos
dias = info_diferencia["total_dias"]

escribir("")
escribir("Faltan " .. dias .. " días para el vencimiento")
```

**Salida:** (ejemplo ilustrativo, varía según fecha de ejecución)

```text
=== CÁLCULO DE VENCIMIENTO ===
Hoy:        28/01/2026
Vence en:   27/02/2026

✅ El año 2028 es bisiesto (366 días)

Faltan 30 días para el vencimiento
```

#### Múltiples Referencias al Mismo Objeto

Puedes crear tantas referencias como necesites. Todas apuntan al mismo objeto:

```latino
incluir("lapsos")

# Tres nombres para la misma librería
tiempo = lapsos
fecha = lapsos
t = lapsos

# Todos funcionan igual
escribir(tiempo.ahora())
escribir(fecha.ahora())
escribir(t.ahora())

# Todos muestran la misma hora (mismo objeto)
```

#### Cuándo Usar Alias

**✅ Usa alias cuando:**

- El nombre de la librería es largo (`transformaciones_geometricas` → `geo`)
- Escribes código muy denso con muchas llamadas a la misma librería
- Quieres que el código sea más "visual" (ej: `db` para base de datos, `http` para web)
- Estás portando código de otro lenguaje con convenciones diferentes

**❌ NO uses alias si:**

- El nombre original ya es corto (`mate`, `sis`, `io`)
- El código es pequeño (menos de 50 líneas)
- Trabajas en equipo y nadie más conoce tu alias (reduce legibilidad)

#### Convenciones Recomendadas

Si decides usar alias, sigue estas convenciones para mantener el código legible:

| Librería Original | Alias Recomendado | Razón |
| :--- | :--- | :--- |
| `lapsos` | `f` o `tiempo` | "f" de fecha, o explícito |
| `matematicas_avanzadas` | `mat` | Común en ciencia |
| `base_datos_sql` | `db` | Estándar universal |
| `cliente_http` | `http` | Descriptivo |
| `graficos_2d` | `g2d` | Compacto y claro |

**Regla de oro:** El alias debe ser **obvio** o **documentado**. Si alguien más va a leer tu código, agrega un comentario:

```latino
incluir("lapsos")
fechas = lapsos  # Alias descriptivo para funciones de fecha/hora
```

#### Nota Técnica: Referencias vs Copias

Este comportamiento es posible gracias al **tipado dinámico** de Latino.NET. Cuando escribes:

```latino
x = mate
```

No estás duplicando el objeto `mate` en memoria. Estás creando una segunda **referencia** (puntero) al mismo objeto. Esto significa:

1. **Sin costo de memoria extra** (ambas variables apuntan al mismo lugar)
2. **Cambios compartidos** (si modificaras el objeto, afectaría a ambas referencias)
3. **Comparación por referencia:** `x == mate` es `verdadero` (son el mismo objeto)

```latino
a = mate
b = mate

escribir(a == b)  # cierto (ambos apuntan al mismo objeto)
```

#### En resumen

✅ Las librerías en Latino.NET son **objetos en variables globales**  
✅ Puedes crear **referencias cortas** (alias) asignando la variable a otro nombre  
✅ `incluir()` **no retorna nada**, debes crear el alias después de cargar  
✅ Los alias **no duplican memoria**, son referencias al mismo objeto  
✅ Usa alias para **mejorar legibilidad** en código denso  
✅ Documenta alias no obvios con **comentarios**  

**Ejemplo de Buena Práctica:**

```latino
# ============================================
# MÓDULO: Procesamiento de Datos con Fechas
# Alias usado: 'f' para funciones de tiempo
# ============================================

incluir("lapsos")
f = lapsos

funcion CalcularVencimientos(fechaInicio, diasCredito)
    vencimiento = f.sumar(fechaInicio, diasCredito, "dias")
    retornar f.formato(vencimiento, "dd/MM/yyyy")
fin

# Uso
hoy = f.ahora()
resultado = CalcularVencimientos(hoy, 30)
escribir("Fecha de vencimiento: " .. resultado)
```

## 7.7 Intercambio de Valores en Memoria: `~` y `trocar()`

En la mayoría de los lenguajes de programación tradicionales, para intercambiar el contenido de dos variables se requiere recurrir a la clásica y verbosa variable auxiliar:

```latino
# ❌ El estilo antiguo e incómodo de otros lenguajes
temporal = a
a = b
b = temporal
```

Otros entornos como Python recurren a la asignación múltiple (`a, b = b, a`), lo que obliga a la máquina virtual a instanciar y desempacar una tupla intermedia en la memoria RAM.

**Latino.NET resuelve este problema de raíz ofreciendo dos mecanismos nativos de intercambio (*In-Place Swap*):** el operador de flujo **`~`** para cualquier referencia directa, y la función de biblioteca **`trocar()`** para manipular posiciones y claves dentro de colecciones.

| Mecanismo | Nivel de Operación | Sintaxis | Ventaja Principal |
| :--- | :--- | :--- | :--- |
| **`~` (Operador)** | Sentencia sobre cualquier referencia asignable | `a ~ b` | **Versatilidad:** funciona con variables, casillas de listas, claves de diccionarios y propiedades de clases. |
| **`trocar()` (Función)** | Función global sobre colecciones completas | `trocar(col, a, b)` | **Ergonomía:** admite índices negativos (`-1`), claves de diccionarios y devuelve la colección para encadenamiento. |

### El Operador de Intercambio `~`

El operador `~` realiza el intercambio de valores en una sola línea de código, sin variables temporales visibles:

```latino
a = 10
b = 20

a ~ b

escribir("a vale: " .. a)   # 20
escribir("b vale: " .. b)   # 10
```

#### Intercambio Polimórfico de Referencias

La verdadera potencia del operador `~` en Latino.NET es que no se limita a variables simples; **opera sobre cualquier destino de asignación válido en el lenguaje**:

```latino
# 1. Intercambio entre índices de una lista
colores = ["Rojo", "Azul", "Verde"]
colores[0] ~ colores[2]
escribir(colores)    # ["Verde", "Azul", "Rojo"]

# 2. Intercambio entre claves de un diccionario
config = {"tema_activo": "oscuro", "tema_previo": "claro"}
config["tema_activo"] ~ config["tema_previo"]
escribir(config)     # {tema_activo: claro, tema_previo: oscuro}
```

Funciona de manera idéntica dentro de métodos en la Programación Orientada a Objetos, utilizando los pronombres de instancia (`mi`, `este`, `esta`):

```latino
clase Punto2D
    constructor(x = 0, y = 0)
        mi.x = x
        mi.y = y
    fin

    funcion invertirCoordenadas()
        # Intercambio directo entre propiedades de la instancia
        mi.x ~ mi.y
    fin
fin

p = nuevo Punto2D(5, 95)
p.invertirCoordenadas()

escribirf("Punto invertido -> X: {0}, Y: {1}\n", p.x, p.y)  # X: 95, Y: 5
```

### La Función Global `trocar()`

Mientras que el operador `~` opera cuando ya tienes ambas referencias identificadas, la función nativa `trocar(coleccion, a, b)` está diseñada para **operar dentro de colecciones completas**, aplicando validaciones de tipo y modificaciones directamente en el sitio (*in-place*).

```latino
trocar(lista, indiceA, indiceB)         # Para listas (índices numéricos)
trocar(diccionario, claveA, claveB)     # Para diccionarios (claves existentes)
```

#### Listas e Índices Negativos (Estilo Moderno)

En listas, los argumentos segundo y tercero deben ser números enteros. `trocar()` soporta nativamente **índices negativos** calculados desde el final (`-1` representa el último elemento, `-2` el penúltimo):

```latino
valores = [10, 20, 30, 40, 50]

# Intercambiamos el primer elemento (0) con el último (-1) sin calcular la longitud
trocar(valores, 0, -1)
escribir(valores)     # [50, 20, 30, 40, 10]
```

Esto permite invertir una colección sin memoria auxiliar:

```latino
numeros = [1, 2, 3, 4, 5, 6]
mitad = mate.techo(numeros.longitud() / 2)

para k en rango(mitad)
    trocar(numeros, k, -k - 1)
fin

escribir("Lista invertida in-place: " .. numeros)  # [6, 5, 4, 3, 2, 1]
```

#### Diccionarios: Intercambio de Valores entre Claves

Al operar sobre diccionarios, el segundo y tercer argumento corresponden a las claves cuyos valores se transferirán mutuamente:

```latino
preferencias = {"idioma_primario": "es", "idioma_secundario": "en"}
trocar(preferencias, "idioma_primario", "idioma_secundario")

escribir("Idioma primario ahora es: " .. preferencias["idioma_primario"]) # en
```

> ⚠️ **Validación Estricta:** Ambas claves deben existir previamente en el diccionario. Si intentas intercambiar una clave inexistente, el motor lanzará:  
> `RuntimeError: Una o ambas claves no existen en el diccionario.`

### Retorno de la Misma Colección (Encadenamiento)

`trocar()` devuelve la colección que recibió, y la modifica en el sitio. Esto permite recuperar el valor en una expresión encadenada:

```latino
origen = [1, 2, 3]
resultado = trocar(origen, 0, 1)

escribir("Intercambio: " .. resultado)   # [2, 1, 3]
```

> ⚠️ **Ojo: `trocar()` no crea una copia.** Si necesitas conservar la colección original, clónala antes con `clonar()`:
>
> ```latino
> original = [1, 2, 3]
> copiaSegura = clonar(original)
> trocar(copiaSegura, 0, 2)
>
> escribir(original)    # [1, 2, 3]  ← intacto
> escribir(copiaSegura)  # [3, 2, 1]  ← el que sí cambió
> ```

### Inmutabilidad de la Biblioteca Estándar

Uno de los principios de diseño fundamentales de Latino.NET es la **protección estricta de las librerías nativas**. Los módulos como `sis`, `mate`, `cadena` o `archivo` son diccionarios marcados como de solo lectura.

Cualquier intento de mutarlos con `trocar()` o el operador `~` será bloqueado por el motor:

```latino
# ❌ Operación rechazada por el motor
trocar(sis, "salir", "dormir")
# Error Runtime: El diccionario es de solo lectura.
```

### ⚡ Nota sobre Rendimiento

`~` y `trocar()` resuelven el mismo problema, pero **no tienen el mismo coste interno**:

- El operador `~` se implementa como una sentencia que **evalúa ambos lados y luego realiza dos asignaciones genéricas** (`PerformAssignment` sobre cada lado).
- `trocar()` intercambia los elementos **directamente** dentro de la lista, en una sola operación, tras validar los índices.

En bucles densos ambos son muy rápido comparados con un intercambio clásico, y su diferencia práctica es marginal. **Elige por ergonomía, no por rendimiento:**

| Necesitas… | Usa |
| :--- | :--- |
| Intercambiar variables, propiedades o dos posiciones concretas que ya tienes | `a ~ b` |
| Índices negativos, claves de diccionario o el valor devuelto en una expresión | `trocar(col, a, b)` |

> 📊 *Medición orientativa en v1.0.81 (20 000 intercambios sobre una lista de 20 000 elementos): `trocar()` ≈ 62–78 ms, `~` ≈ 94–110 ms en la primera pasada. Tras el calentamiento del JIT ambos convergen en torno a 31 ms. Estos valores son indicativos y dependen del hardware; no los tomes como referencia absoluta.*

### Diagnóstico de Errores Frecuentes

| Código Incorrecto | Diagnóstico del Motor | Causa Raíz |
| :--- | :--- | :--- |
| `trocar(l, 0)` | `Esta función requiere al menos 3 argumento(s)...` | Aridad insuficiente (faltan argumentos). |
| `trocar(l, 0, 99)` | `Índices fuera de rango en trocar().` | El índice supera la longitud de la lista. |
| `trocar(l, "0", "1")` | `Para listas, los índices a trocar deben ser numéricos.` | Se pasaron cadenas en lugar de números. |
| `trocar(d, "a", "z")` | `Una o ambas claves no existen en el diccionario.` | Una de las claves no ha sido inicializada. |
| `trocar(mate, 0, 1)` | `El diccionario es de solo lectura.` | Intento de alterar una librería nativa protegido. |
| `trocar("texto", 0, 1)` | `trocar() solo soporta listas y diccionarios.` | Las cadenas son inmutables; no admiten swap in-place. |

### ⚡ Trampa Crítica: El Sombreado de Nombres de Librerías

Al manipular listas o diccionarios, **jamás nombres a tu variable con el mismo nombre que una librería de la biblioteca estándar**:

```latino
# ❌ ANTIPATRÓN GRAVE:
lista = [1, 2, 3, 4, 5]
escribir(lista.longitud())
# Error Runtime: No existe el método lista.longitud().
```

#### ¿Por qué ocurre este error tan desconcertante?

Cuando ejecutas un método sobre una lista (como `.longitud()`), el motor recurre a su despachador interno, que **busca la librería `"lista"` en el ámbito global** para encontrar el método.

Si declaraste una variable llamada `lista = [...]`, **has ocultado la librería nativa con tu propia variable**. El motor ya no encuentra las funciones del sistema y el método falla.

```latino
# ✅ FORMA IDIOMÁTICA Y SEGURA:
numeros = [1, 2, 3, 4, 5]
escribir(numeros.longitud()) # 5 (funciona a la perfección)
```

Lo mismo aplica para identificadores como `dic`, `cadena`, `mate`, `sis` o `archivo`.

### Diferencia Visual: `~` frente a `~=`

Dado que la sección 7.4 cubre las expresiones regulares, es vital no confundir estos operadores en tus lecturas de código:

| Símbolo | Naturaleza | Propósito | Ejemplo |
| :--- | :--- | :--- | :--- |
| **`~`** | Operador de Flujo | Intercambio de dos valores | `a ~ b` *(invierte valores)* |
| **`~=`** | Operador Relacional | Coincidencia de texto con Expresión Regular | `correo ~= "^[a-z]+@"` *(evalúa a lógico)* |
| **`\| ... \|`** | Delimitadores de Texto | Interpolación estructural en el Compositor | `ç("Hola \|nombre\|")` *(formatea cadena)* |

### 📋 Checklist del Desarrollador para Intercambios

- [ ] ¿Necesito intercambiar dos variables sueltas o propiedades de un objeto? → Usa **`a ~ b`**.
- [ ] ¿Necesito intercambiar extremos de listas con índices negativos (`-1`)? → Usa **`trocar(col, 0, -1)`**.
- [ ] ¿Quiero intercambiar valores en un diccionario sin crear copias? → Usa **`trocar(dict, "k1", "k2")`**.
- [ ] ¿Verifiqué que mi variable no se llame `lista` o `dic`? → Evita colisiones con las librerías.
- [ ] ¿Necesito conservar la colección original intacta? → Clónala antes con `clonar()`; `trocar()` modifica *in-place*.

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-06
