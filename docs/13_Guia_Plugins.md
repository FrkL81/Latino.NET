# CAPÍTULO 13: Guía de Creación de Plugins (.NET y C#)

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores de C#, ingenieros de software, científicos de datos y contribuidores del ecosistema que deseen extender Latino.NET con módulos de alto rendimiento compilados.  
**Propósito:** Dominar la creación, estructuración, empaquetado y consumo de plugins externos (`.dll`) en C#: enlace con `Latino.Core`, conversión bidireccional de tipos primitivos, gestión de memoria, validación de aridad y exposición de metadatos al Language Server.

## La Fuerza Bruta del Ecosistema .NET

Uno de los mayores dilemas al diseñar un lenguaje de programación es el equilibrio entre simplicidad y rendimiento. Los lenguajes dinámicos interpretados destacan por su velocidad de desarrollo y facilidad de lectura, pero cuando se enfrentan a cargas de trabajo computacionalmente intensivas —cálculo matricial masivo, procesamiento digital de señales, conexiones directas a hardware o criptografía pesada—, el coste del recorrido dinámico de nodos se hace evidente.

En lugar de aislarse en una burbuja cerrada, Latino.NET resuelve esta disyuntiva mediante un **modelo de interoperabilidad nativa de alto rendimiento**.

Al ejecutarse directamente sobre la máquina virtual de **.NET 10**, Latino.NET no requiere complejas interfaces de funciones foráneas (*FFI*) basadas en punteros inseguros de C/C++. En este entorno, extender el lenguaje consiste en crear bibliotecas de clases estándar en C# que interactúan directamente con la jerarquía de `LatinoObject` a velocidad de código compilado de máquina.

### El Rol del Plugin en la Arquitectura Híbrida

Un plugin en Latino.NET no es un script interpretado adicional; es una biblioteca compilada (`.dll`) que se conecta en tiempo de ejecución al motor mediante reflexión eficiente:

1. **Rendimiento Industrial:** Los algoritmos pesados se ejecutan directamente sobre la Base Class Library (BCL) de .NET y el compilador JIT, logrando velocidades de ejecución órdenes de magnitud superiores al código interpretado.
2. **Acceso al Ecosistema Global:** Toda la potencia de la plataforma .NET —bases de datos (SQLite, PostgreSQL, SQL Server), redes seguras (HTTP/2, WebSockets), procesamiento de imágenes y computación científica— queda disponible para tus scripts de Latino mediante una simple sentencia `incluir()`.
3. **Desacoplamiento Profesional:** Un ingeniero de software o especialista en infraestructura puede escribir y blindar la lógica crítica en C#, exponiendo una interfaz clara y en español natural para que analistas y desarrolladores la consuman sin preocuparse por la complejidad interna de bajo nivel.

### Anatomía del Puente entre C# y Latino

Para construir una extensión sólida es necesario comprender los tres componentes que hacen posible la comunicación entre ambos mundos:

* **El Núcleo Compartido (`Latino.Core.dll`):** Es el ensamblado maestro que contiene el sistema de tipos universales (`LatinoNumber`, `LatinoString`, `LatinoList`, `LatinoDictionary`) y las interfaces de llamada (`ICallable`).
* **El Atributo de Metadatos (`[LatinoExtension]`):** Una directiva declarativa que expone métodos públicos y estáticos de C# al entorno de Latino, documentando su nombre oficial, argumentos esperados y texto de ayuda contextual.
* **El Contrato del Intérprete:** Toda función invocable desde Latino recibe una lista de argumentos (`List<LatinoObject>`) y devuelve un objeto de la jerarquía de Latino, permitiendo una conversión fluida y segura contra errores en tiempo de ejecución (*RuntimeError*).

> 💡 **Principio de Delegación Estratégica (Mantra de Latino.NET):**  
> *\"Escribe la lógica de negocio en Latino con la belleza del español; delega el trabajo pesado a C# con la potencia de .NET.\"*  
> El secreto de una aplicación industrial no radica en programarlo todo a mano en el script, sino en orquestar herramientas nativas de alto rendimiento diseñadas para resistir el paso del tiempo.

## 📋 Requisitos Previos

### Software Necesario

Antes de empezar, necesitas preparar tu taller digital:

1. **El Motor (.NET SDK)**
   * Es el kit de herramientas que permite fabricar las piezas
   * [Descárgalo aquí](https://dotnet.microsoft.com/download)
   * Busca la versión **.NET 8.0 o superior** (recomendado: .NET 10)
   * Para verificar si ya lo tienes, abre una terminal y escribe:

     ```bash
     dotnet --version
     ```

     Deberías ver algo como: `10.0.0` o similar

2. **Latino.NET Instalado**
   * 📥 Descarga el archivo `.zip` desde:
     * 📥 **GitHub Releases:** <https://github.com/FrkL81/Latino.NET/releases/tag/v1.0.81>

   * Descomprímelo en una carpeta (ejemplo: `C:\LatinoNET`)
   * Dentro encontrarás una carpeta `sdk` con el archivo crucial: **`Latino.Core.dll`**
   * ¡No lo pierdas! Ese archivo es el traductor que permite que C# y Latino se entiendan

3. **Un Editor de Código (opcional pero recomendado)**
   * **Visual Studio 2022** (Community es gratis): IDE completo y potente
   * **VS Code** con extensión de C#: Ligero y rápido
   * **Rider**: Excelente pero de pago
   * O cualquier editor de texto (Notepad++, Sublime)

### Conocimientos Recomendados

* ✅ Conocimientos básicos de Latino (variables, funciones, diccionarios)
* ✅ Conceptos básicos de programación en C# (clases, métodos, tipos)
* 💡 Familiaridad con la terminal/consola de comandos
* 💡 (Opcional) Conocimientos de C# y .NET 10. NuGet no es necesario para escribir un plugin

**No te preocupes si eres principiante**: Esta guía está diseñada para llevarte paso a paso desde cero hasta crear plugins profesionales.

## 📚 Nivel 1: Tu Primer Plugin - "Hola Mundo"

Vamos a empezar con algo súper simple para entender cómo funciona todo. Crearemos un plugin que simplemente saluda.

### Paso 1.1: Crear el Proyecto

Abre tu terminal (PowerShell en Windows, Terminal en Mac/Linux) y escribe estos comandos:

```bash
# Crear una carpeta para tus experimentos
mkdir MisPrimerosPlugins
cd MisPrimerosPlugins

# Crear el proyecto de C# (Biblioteca de Clases)
dotnet new classlib -n HolaMundo
cd HolaMundo
```

**¿Qué acabas de hacer?**

* Creaste una carpeta para organizarte
* Creaste un proyecto de C# tipo "biblioteca" (que genera un archivo `.dll`)
* El nombre del proyecto es `HolaMundo`

### Paso 1.2: Conectar Latino y `C#`

Ahora tenemos un proyecto de C# vacío, pero no sabe nada de Latino. Necesitamos "presentarlos".

**Opción A: Línea de Comandos** (más rápida)

```bash
# Ajusta la ruta a donde descomprimiste Latino
dotnet add reference "C:\LatinoNET\sdk\Latino.Core.dll"
```

**Opción B: Editar manualmente** (más segura)

Abre el archivo `HolaMundo.csproj` con tu editor y agrégale esto antes del cierre `</Project>`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Referencia al cerebro de Latino -->
    <Reference Include="Latino.Core">
      <HintPath>..\..\sdk\Latino.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

⚠️ **Importante**: Ajusta la ruta `<HintPath>` para que apunte exactamente a donde tienes `Latino.Core.dll`.

### Paso 1.3: Escribir tu Primera Función

Abre el archivo `Class1.cs` que se creó automáticamente. **Borra todo** su contenido y escribe esto:

```csharp
using System;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace HolaMundo;

public class Main
{
    [LatinoExtension("saludar", Help = "Saluda al usuario", Args = "nombre")]
    public static LatinoObject Saludar(List<LatinoObject> args)
    {
        // 1. Validar que recibimos algo
        if (args.Count == 0)
            throw new RuntimeError(null, "¡Oye! saludar() necesita un nombre");

        // 2. Extraer el nombre (convertir de Latino a C#)
        string nombre = args[0].ToString();

        // 3. Crear el mensaje
        string mensaje = $"¡Hola, {nombre}! Este mensaje viene desde C#";

        // 4. Devolver a Latino (convertir de C# a Latino)
        return new LatinoString(mensaje);
    }
}
```

**Analicemos el código línea por línea:**

* **`[LatinoExtension(...)]`**: Esta es una etiqueta mágica. Le dice al motor de Latino: "¡Ey! Esta función es para ti, házla disponible con el nombre 'saludar'".
  * `"saludar"`: El nombre que usarás en Latino
  * `Help`: Descripción que aparecerá en el autocompletado del editor
  * `Args`: Los nombres de los parámetros (útil para documentación)

* **`public static LatinoObject`**: Toda función que Latino pueda llamar debe:
  * Ser `public` (pública, visible desde fuera)
  * Ser `static` (no necesita crear un objeto primero)
  * Devolver un `LatinoObject` (el tipo universal de Latino)

* **`List<LatinoObject> args`**: Los argumentos vienen en una lista. `args[0]` es el primer argumento, `args[1]` el segundo, etc.

* **`RuntimeError`**: Cuando algo sale mal, lanzas este error. Latino lo capturará con `intentar/atrapar`.

### Paso 1.4: Compilar (Convertir en DLL)

Ahora vamos a convertir tu código en una pieza que Latino puede usar:

```bash
dotnet build -c Release
```

Si ves mensajes en verde o que dicen "Build succeeded", ¡perfecto! Tu DLL está lista.

**¿Dónde está?**
Busca en: `bin/Release/net10.0/HolaMundo.dll`

### Paso 1.5: La Prueba del Fuego

Crea un archivo llamado `prueba.lat` en la **misma carpeta** donde está tu DLL:

```latino
escribir("=== Probando mi primer plugin ===")

# Cargar el plugin
incluir("HolaMundo.dll")

# Usar la función que creamos en C#
mensaje = saludar("María")
escribir(mensaje)

# Probar con otro nombre
mensaje2 = saludar("Juan Carlos")
escribir(mensaje2)
```

**Ejecuta el script:**

```bash
latino prueba.lat
```

**Salida esperada:**

```text
=== Probando mi primer plugin ===
¡Hola, María! Este mensaje viene desde C#
¡Hola, Juan Carlos! Este mensaje viene desde C#
```

🎉 **¡Felicidades!** Acabas de crear y usar tu primer plugin. Ahora entiendes el flujo básico:

1. Escribir C# con el atributo `[LatinoExtension]`
2. Compilar a DLL
3. Incluir en Latino con `incluir()`
4. Usar como cualquier función de Latino

## 📚 Nivel 2: Trabajando con Tipos de Datos

Ahora que entiendes lo básico, vamos a profundizar en cómo manejar diferentes tipos de datos entre Latino y C#.

### El Sistema de Tipos: El Puente entre Dos Mundos

Latino y C# hablan "idiomas" diferentes. Latino tiene sus propios tipos (`numero`, `cadena`, `lista`, `diccionario`), y C# tiene los suyos (`int`, `string`, `List<>`, `Dictionary<>`). Los plugins actúan como traductores.

#### Tabla de Conversión Esencial

| Tipo en Latino | Clase en C# | Conversión Latino → C# | Conversión C# → Latino |
| :--- | :--- | :--- | :--- |
| Cadena (`"texto"`) | `LatinoString` | `args[0].ToString()` | `new LatinoString("texto")` |
| Número (`42`, `3.14`) | `LatinoNumber` | `((LatinoNumber)args[0]).Value` | `new LatinoNumber(42.5)` |
| Lógico (`verdadero`/`falso`) | `LatinoBoolean` | `((LatinoBoolean)args[0]).Value` | `new LatinoBoolean(true)` |
| Lista (`[1, 2, 3]`) | `LatinoList` | `((LatinoList)args[0]).Elements` | `new LatinoList(elements)` |
| Diccionario (`{"x": 10}`) | `LatinoDictionary` | `((LatinoDictionary)args[0]).Pairs` | `new LatinoDictionary(dic)` |
| Nulo (`nulo`) | `LatinoNull` | `args[0] is LatinoNull` | `LatinoNull.Instance` |

🎯 **Ejemplo Práctico: Calculadora Básica**

Vamos a crear un plugin más útil que demuestre el manejo de números:

```csharp
using System;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Calculadora;

public class Main
{
    [LatinoExtension("multiplicar", Help = "Multiplica dos números", Args = "a, b")]
    public static LatinoObject Multiplicar(List<LatinoObject> args)
    {
        // Validar cantidad de argumentos
        if (args.Count < 2)
            throw new RuntimeError(null, "multiplicar() necesita 2 números");

        // Extraer y validar que sean números
        if (args[0] is not LatinoNumber numA || args[1] is not LatinoNumber numB)
            throw new RuntimeError(null, "Los dos argumentos deben ser números");

        // Hacer la operación en C#
        double resultado = numA.Value * numB.Value;

        // Devolver a Latino
        return new LatinoNumber(resultado);
    }

    [LatinoExtension("potencia", Help = "Eleva un número a una potencia", Args = "base, exponente")]
    public static LatinoObject Potencia(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null, "potencia() necesita 2 números");

        if (args[0] is not LatinoNumber baseNum || args[1] is not LatinoNumber expNum)
            throw new RuntimeError(null, "Los argumentos deben ser números");

        double resultado = Math.Pow(baseNum.Value, expNum.Value);
        return new LatinoNumber(resultado);
    }

    [LatinoExtension("raiz", Help = "Calcula la raíz cuadrada", Args = "numero")]
    public static LatinoObject Raiz(List<LatinoObject> args)
    {
        if (args.Count < 1)
            throw new RuntimeError(null, "raiz() necesita un número");

        if (args[0] is not LatinoNumber num)
            throw new RuntimeError(null, "El argumento debe ser un número");

        if (num.Value < 0)
            throw new RuntimeError(null, "No puedo calcular raíz de número negativo");

        return new LatinoNumber(Math.Sqrt(num.Value));
    }
}
```

**Uso en Latino:**

```latino
incluir("Calculadora.dll")

escribir(multiplicar(7, 8))      # 56
escribir(potencia(2, 10))         # 1024
escribir(raiz(144))               # 12
```

### Trabajando con Listas

Las listas son colecciones ordenadas. Veamos cómo procesarlas:

```csharp
[LatinoExtension("calc.sumarLista", Help = "Suma todos los números de una lista", Args = "lista")]
public static LatinoObject SumarLista(List<LatinoObject> args)
{
    // Validar que recibimos una lista
    if (args[0] is not LatinoList lista)
        throw new RuntimeError(null, "Se esperaba una lista");

    double suma = 0;
    
    // Recorrer cada elemento
    foreach (var elemento in lista.Elements)
    {
        // Solo sumar si es un número (ignorar texto, etc.)
        if (elemento is LatinoNumber num)
            suma += num.Value;
    }

    return new LatinoNumber(suma);
}

[LatinoExtension("calc.promedioLista", Help = "Calcula el promedio", Args = "lista")]
public static LatinoObject Promedio(List<LatinoObject> args)
{
    if (args[0] is not LatinoList lista)
        throw new RuntimeError(null, "Se esperaba una lista");

    if (lista.Elements.Count == 0)
        return new LatinoNumber(0);

    double suma = 0;
    int contador = 0;

    foreach (var elemento in lista.Elements)
    {
        if (elemento is LatinoNumber num)
        {
            suma += num.Value;
            contador++;
        }
    }

    if (contador == 0)
        throw new RuntimeError(null, "La lista no contiene números");

    return new LatinoNumber(suma / contador);
}
```

**Uso en Latino:**

```latino
incluir("Calculadora.dll")

numeros = [10, 20, 30, 40, 50]
escribir("Suma: " .. calc.sumarLista(numeros))        # 150
escribir("Promedio: " .. calc.promedioLista(numeros)) # 30
```

### Trabajando con Diccionarios

Los diccionarios son como objetos con propiedades:

```csharp
[LatinoExtension("persona.crear", Help = "Crea una persona", Args = "nombre, edad, ciudad")]
public static LatinoObject CrearPersona(List<LatinoObject> args)
{
    if (args.Count < 3)
        throw new RuntimeError(null, "Se necesita: nombre, edad, ciudad");

    // IMPORTANTE: Usar LatinoObjectComparer.Instance para que las claves funcionen
    var dic = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
    {
        [new LatinoString("nombre")] = args[0],
        [new LatinoString("edad")] = args[1],
        [new LatinoString("ciudad")] = args[2],
        [new LatinoString("activo")] = new LatinoBoolean(true),
        [new LatinoString("fecha_registro")] = new LatinoString(DateTime.Now.ToString("dd/MM/yyyy"))
    };

    return new LatinoDictionary(dic);
}

[LatinoExtension("persona.es_mayor", Help = "Verifica si es mayor de edad", Args = "persona")]
public static LatinoObject EsMayor(List<LatinoObject> args)
{
    if (args[0] is not LatinoDictionary persona)
        throw new RuntimeError(null, "Se esperaba un diccionario (persona)");

    // Extraer la edad del diccionario
    var claveEdad = new LatinoString("edad");
    
    if (!persona.Pairs.ContainsKey(claveEdad))
        throw new RuntimeError(null, "La persona no tiene el campo 'edad'");

    var edad = persona.Pairs[claveEdad];
    
    if (edad is not LatinoNumber numEdad)
        throw new RuntimeError(null, "La edad debe ser un número");

    return new LatinoBoolean(numEdad.Value >= 18);
}
```

**Uso en Latino:**

```latino
incluir("Calculadora.dll")

maria = persona.crear("María García", 25, "Madrid")
escribir("Nombre: " .. maria["nombre"])
escribir("Edad: " .. maria["edad"])
escribir("¿Es mayor? " .. persona.es_mayor(maria))  # cierto

pedro = persona.crear("Pedro López", 15, "Barcelona")
escribir("¿Pedro es mayor? " .. persona.es_mayor(pedro))  # falso
```

## 📚 Nivel 3: Organización Profesional con Namespaces

Cuando creas muchas funciones, es buena idea organizarlas en "módulos" o "librerías". Latino tiene un sistema automático para esto usando **puntos** en los nombres.

### ¿Qué son los Namespaces en Latino?

Cuando pones un punto en el nombre de una función (ej: `"mate.raiz"`), Latino automáticamente crea un objeto llamado `mate` y le agrega la función `raiz` dentro. Es como organizar archivos en carpetas.

**Sin namespace:**

```latino
raiz(16)       # ¿Raíz de qué? ¿Matemática? ¿De un árbol?
formato(x)     # ¿Formato de qué? ¿Número? ¿Fecha? ¿Texto?
```

**Con namespace:**

```latino
mate.raiz(16)     # ¡Ah! Es matemática
fecha.formato(x)  # ¡Ah! Es para fechas
```

🎯 **Ejemplo: Plugin de Matemáticas Organizado**

```csharp
using System;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace MatePlugin;

public class Main
{
    // Funciones básicas
    [LatinoExtension("mplugin.raiz", Help = "Raíz cuadrada", Args = "n")]
    public static LatinoObject Raiz(List<LatinoObject> args)
    {
        double num = ObtenerNumero(args, 0);
        
        if (num < 0)
            throw new RuntimeError(null, "No puedo calcular raíz de número negativo");
        
        return new LatinoNumber(Math.Sqrt(num));
    }

    [LatinoExtension("mplugin.potencia", Help = "Eleva a la potencia", Args = "base, exponente")]
    public static LatinoObject Potencia(List<LatinoObject> args)
    {
        double baseNum = ObtenerNumero(args, 0);
        double exp = ObtenerNumero(args, 1);
        
        return new LatinoNumber(Math.Pow(baseNum, exp));
    }

    [LatinoExtension("mplugin.aleatorio", Help = "Número aleatorio entre min y max", Args = "min, max")]
    public static LatinoObject Aleatorio(List<LatinoObject> args)
    {
        int min = (int)ObtenerNumero(args, 0);
        int max = (int)ObtenerNumero(args, 1);
        
        var random = new Random();
        return new LatinoNumber(random.Next(min, max + 1));
    }

    [LatinoExtension("mplugin.abs", Help = "Valor absoluto", Args = "n")]
    public static LatinoObject ValorAbsoluto(List<LatinoObject> args)
    {
        double num = ObtenerNumero(args, 0);
        return new LatinoNumber(Math.Abs(num));
    }

    [LatinoExtension("mplugin.redondear", Help = "Redondea un número", Args = "n, [decimales]")]
    public static LatinoObject Redondear(List<LatinoObject> args)
    {
        double num = ObtenerNumero(args, 0);
        int decimales = args.Count > 1 ? (int)ObtenerNumero(args, 1) : 0;
        
        return new LatinoNumber(Math.Round(num, decimales));
    }

    // === MÉTODO AUXILIAR (privado, no se exporta a Latino) ===
    private static double ObtenerNumero(List<LatinoObject> args, int indice)
    {
        if (indice >= args.Count)
            throw new RuntimeError(null, $"Falta el argumento #{indice + 1}");
        
        if (args[indice] is not LatinoNumber num)
            throw new RuntimeError(null, $"El argumento #{indice + 1} debe ser un número");
        
        return num.Value;
    }
}
```

**Uso en Latino:**

```latino
incluir("MatePlugin.dll")

# Ahora todas las funciones están bajo 'mplugin'
escribir(mplugin.raiz(25))              # 5
escribir(mplugin.potencia(2, 10))       # 1024
escribir(mplugin.aleatorio(1, 100))     # Ej: 42
escribir(mplugin.abs(-15))              # 15
escribir(mplugin.redondear(3.14159, 2)) # 3.14
```

### Ventajas de Usar Namespaces

✅ **Evita conflictos de nombres**: Tu función `mplugin.raiz` no choca con una función `raiz` de otro plugin
✅ **Organización clara**: Todo relacionado con matemáticas está bajo `mplugin`
✅ **Autocompletado inteligente**: En el editor, escribes `mplugin.` y ves todas las opciones
✅ **Escalabilidad**: Puedes agregar 100 funciones sin ensuciar el espacio global

## 📚 Nivel 4: Proyecto Real - Plugin "Lapsos" (Manejo de Fechas)

Ahora vamos a crear algo realmente útil: un plugin completo para manejar fechas y tiempos. Este proyecto te enseñará patrones profesionales de diseño.

> **💡 Nota Pro:** La versión de `lapsos.dll` que se incluye en la distribución oficial de Latino.NET es una versión evolucionada de este ejemplo. Incluye 13 métodos adicionales y una validación heurística avanzada (para ticks y unix) que veremos de forma simplificada aquí.

### Arquitectura del Plugin

El plugin "Lapsos" tendrá estas capacidades:

* Obtener la fecha/hora actual
* Crear fechas específicas
* Sumar/restar tiempo
* Calcular diferencias entre fechas
* Formatear fechas para mostrar
* Validaciones (años bisiestos, etc.)

### Paso 4.1: Estructura del Proyecto

```bash
mkdir Latino.Plugin.Lapsos
cd Latino.Plugin.Lapsos
dotnet new classlib
# Agregar referencia a Latino.Core.dll como aprendimos antes
```

### Paso 4.2: Los Helpers (Traductores)

Primero crearemos funciones auxiliares que convierten fechas entre .NET (`DateTime`) y Latino (diccionarios):

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Lapsos;

public class Main
{
    // ============================================
    // PARTE 1: TRADUCTORES (Métodos Privados)
    // ============================================

    /// <summary>
    /// Convierte DateTime de .NET a un diccionario de Latino
    /// </summary>
    private static LatinoDictionary ToDic(DateTime dt)
    {
        var dic = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        
        // Helper local para agregar pares clave-valor más fácilmente
        void Agregar(string clave, double valor) => 
            dic[new LatinoString(clave)] = new LatinoNumber(valor);
        
        // Componentes básicos de la fecha
        Agregar("anio", dt.Year);
        Agregar("mes", dt.Month);
        Agregar("dia", dt.Day);
        Agregar("hora", dt.Hour);
        Agregar("minuto", dt.Minute);
        Agregar("segundo", dt.Second);
        Agregar("milisegundo", dt.Millisecond);
        
        // Información adicional útil
        Agregar("dia_semana", (int)dt.DayOfWeek);  // 0=Domingo, 1=Lunes, etc.
        Agregar("dia_anio", dt.DayOfYear);          // 1-366
        
        // Representación de texto por defecto
        dic[new LatinoString("texto")] = new LatinoString(dt.ToString("dd/MM/yyyy HH:mm:ss"));

        return new LatinoDictionary(dic);
    }

    /// <summary>
    /// TRADUCTOR PROFESIONAL: Convierte LatinoObject a DateTime.
    /// Es inteligente: acepta diccionarios, cadenas de texto o números (Unix/Ticks).
    /// </summary>
    private static DateTime ToDate(LatinoObject obj)
    {
        // Caso 1: Es un diccionario (objeto interno del plugin)
        if (obj is LatinoDictionary dict)
        {
            if (dict.Pairs.TryGetValue(new LatinoString("texto"), out var textoObj))
                return DateTime.Parse(textoObj.ToString());
            
            // Fallback: si no tiene texto, intentar construirlo por componentes
            return new DateTime(
                GetComp(dict, "anio", DateTime.Now.Year), 
                GetComp(dict, "mes", 1), 
                GetComp(dict, "dia", 1));
        }
        
        // Caso 2: Es una cadena de texto (ej: resultado de archivo.fechaModificacion())
        if (obj is LatinoString str)
        {
            if (DateTime.TryParse(str.Value, out DateTime resultado))
                return resultado;
        }

        // Caso 3: Es un número (Unix Timestamp o Ticks)
        if (obj is LatinoNumber num)
        {
            long val = (long)num.Value;
            if (val > 253402300799) return new DateTime(val); // Ticks
            return new DateTime(1970, 1, 1).AddSeconds(val).ToLocalTime(); // Unix
        }
        
        throw new RuntimeError(null, $"No se pudo reconocer '{obj}' como una fecha válida.");
    }

    private static int GetComp(LatinoDictionary d, string k, int def) =>
        d.Pairs.TryGetValue(new LatinoString(k), out var v) && v is LatinoNumber n ? (int)n.Value : def;

    /// <summary>
    /// Extrae un número de la lista de argumentos de forma segura
    /// </summary>
    private static double ObtenerNum(List<LatinoObject> args, int indice)
    {
        if (indice >= args.Count)
            return 0;
        
        return args[indice] is LatinoNumber num ? num.Value : 0;
    }

    // ============================================
    // PARTE 2: FUNCIONES PÚBLICAS (Exportadas a Latino)
    // ============================================

    [LatinoExtension("lapsos.ahora", 
        Help = "Obtiene la fecha y hora actual del sistema", 
        Returns = "diccionario")]
    public static LatinoObject Ahora(List<LatinoObject> args)
    {
        return ToDic(DateTime.Now);
    }

    [LatinoExtension("lapsos.fecha", 
        Help = "Crea una fecha específica", 
        Args = "anio, mes, dia, [hora, minuto, segundo]",
        Returns = "diccionario")]
    public static LatinoObject Fecha(List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null, "lapsos.fecha() necesita al menos año, mes y día");

        try
        {
            int anio = (int)ObtenerNum(args, 0);
            int mes = (int)ObtenerNum(args, 1);
            int dia = (int)ObtenerNum(args, 2);
            
            // Parámetros opcionales de tiempo
            int hora = args.Count > 3 ? (int)ObtenerNum(args, 3) : 0;
            int minuto = args.Count > 4 ? (int)ObtenerNum(args, 4) : 0;
            int segundo = args.Count > 5 ? (int)ObtenerNum(args, 5) : 0;

            var dt = new DateTime(anio, mes, dia, hora, minuto, segundo);
            return ToDic(dt);
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null, "Fecha inválida: " + ex.Message);
        }
    }

    [LatinoExtension("lapsos.analizar", 
        Help = "Convierte texto a fecha (soporta múltiples formatos)", 
        Args = "texto_fecha",
        Returns = "diccionario")]
    public static LatinoObject Analizar(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null, "lapsos.analizar() necesita un texto con la fecha");

        string texto = args[0].ToString();

        // Intentar parsear con la configuración regional del sistema
        if (DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt))
            return ToDic(dt);
        
        // Intentar con formato invariante (ISO)
        if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return ToDic(dt);
        
        throw new RuntimeError(null, $"No se pudo reconocer la fecha: '{texto}'");
    }

    [LatinoExtension("lapsos.formato", 
        Help = "Convierte una fecha a texto con formato personalizado", 
        Args = "fecha, [formato]",
        Returns = "cadena")]
    public static LatinoObject Formato(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null, "lapsos.formato() necesita una fecha");

        DateTime dt = ToDate(args[0]);
        string formato = args.Count > 1 ? args[1].ToString() : "dd/MM/yyyy";
        
        try
        {
            return new LatinoString(dt.ToString(formato, CultureInfo.CurrentCulture));
        }
        catch
        {
            throw new RuntimeError(null, "Formato de fecha inválido");
        }
    }

    [LatinoExtension("lapsos.esBisiesto", 
        Help = "Verifica si un año es bisiesto", 
        Args = "anio_o_fecha",
        Returns = "logico")]
    public static LatinoObject EsBisiesto(List<LatinoObject> args)
    {
        if (args.Count == 0)
            throw new RuntimeError(null, "lapsos.esBisiesto() necesita un año o una fecha");

        int anio;
        
        // Aceptar número (año directamente) o diccionario (fecha)
        if (args[0] is LatinoNumber num)
        {
            anio = (int)num.Value;
        }
        else if (args[0] is LatinoDictionary dic)
        {
            var dt = ToDate(args[0]);
            anio = dt.Year;
        }
        else
        {
            throw new RuntimeError(null, "Se esperaba un número (año) o un diccionario (fecha)");
        }

        return new LatinoBoolean(DateTime.IsLeapYear(anio));
    }

    [LatinoExtension("lapsos.sumar", 
        Help = "Suma una cantidad de tiempo a una fecha", 
        Args = "fecha, cantidad, unidad",
        Returns = "diccionario")]
    public static LatinoObject Sumar(List<LatinoObject> args)
    {
        return OperarFecha(args, 1);
    }

    [LatinoExtension("lapsos.restar", 
        Help = "Resta una cantidad de tiempo a una fecha", 
        Args = "fecha, cantidad, unidad",
        Returns = "diccionario")]
    public static LatinoObject Restar(List<LatinoObject> args)
    {
        // Truco: restar es sumar negativo
        return OperarFecha(args, -1);
    }

    /// <summary>
    /// Lógica compartida para sumar/restar tiempo
    /// </summary>
    private static LatinoObject OperarFecha(List<LatinoObject> args, int factor)
    {
        if (args.Count < 3)
            throw new RuntimeError(null, "Se necesita: fecha, cantidad, unidad");

        DateTime dt = ToDate(args[0]);
        double cantidad = ObtenerNum(args, 1) * factor;
        string unidad = args[2].ToString().ToLower();

        try
        {
            DateTime nueva = unidad switch
            {
                "dia" or "dias" => dt.AddDays(cantidad),
                "mes" or "meses" => dt.AddMonths((int)cantidad),
                "anio" or "año" or "anios" or "años" => dt.AddYears((int)cantidad),
                "hora" or "horas" => dt.AddHours(cantidad),
                "minuto" or "minutos" or "min" => dt.AddMinutes(cantidad),
                "segundo" or "segundos" or "seg" => dt.AddSeconds(cantidad),
                _ => throw new RuntimeError(null, $"Unidad desconocida: '{unidad}'. Usa: dias, meses, años, horas, minutos, segundos")
            };

            return ToDic(nueva);
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null, "Error al calcular fecha: " + ex.Message);
        }
    }

    [LatinoExtension("lapsos.diferencia", 
        Help = "Calcula el tiempo transcurrido entre dos fechas", 
        Args = "fecha_inicio, fecha_fin",
        Returns = "diccionario")]
    public static LatinoObject Diferencia(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null, "lapsos.diferencia() necesita dos fechas");

        DateTime inicio = ToDate(args[0]);
        DateTime fin = ToDate(args[1]);

        TimeSpan diferencia = fin - inicio;

        var dic = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        
        void Agregar(string clave, double valor) => 
            dic[new LatinoString(clave)] = new LatinoNumber(valor);
        
        // Totales (acumulados decimales)
        Agregar("total_dias", diferencia.TotalDays);
        Agregar("total_horas", diferencia.TotalHours);
        Agregar("total_minutos", diferencia.TotalMinutes);
        Agregar("total_segundos", diferencia.TotalSeconds);
        
        // Fragmentos (unidades individuales)
        Agregar("dias", diferencia.Days);
        Agregar("horas", diferencia.Hours);
        Agregar("minutos", diferencia.Minutes);
        Agregar("segundos", diferencia.Seconds);
        
        // Signo: -1 si la fecha fin es anterior a inicio
        Agregar("signo", diferencia.Ticks < 0 ? -1 : 1);

        return new LatinoDictionary(dic);
    }
}
```

### Paso 4.3: Compilar y Probar

```bash
dotnet build -c Release
```

### Paso 4.4: Script de Prueba Completo

Crea `prueba_lapsos.lat`:

```latino
escribir("=== Plugin Lapsos: Manejo Profesional de Fechas ===\n")

# Cargar el plugin
incluir("Latino.Plugin.Lapsos.dll")

# 1. Obtener fecha actual
escribir("1. Fecha Actual:")
hoy = lapsos.ahora()
escribir("  Hoy: " .. hoy.texto)
escribir("  Año: " .. hoy.anio)
escribir("  Mes: " .. hoy.mes)
escribir("  Día del año: " .. hoy.dia_anio)

# 2. Crear fechas específicas
escribir("\n2. Crear Fechas:")
cumple = lapsos.fecha(1995, 8, 15)
escribir("  Cumpleaños: " .. lapsos.formato(cumple, "dd/MM/yyyy"))

navidad = lapsos.fecha(2025, 12, 25, 0, 0, 0)
escribir("  Navidad: " .. lapsos.formato(navidad, "dddd, dd 'de' MMMM 'de' yyyy"))

# 3. Parsear texto
escribir("\n3. Leer Fechas de Texto:")
fecha1 = lapsos.analizar("15/03/2024")
escribir("  Parseado: " .. fecha1.texto)

# 4. Validación de años bisiestos
escribir("\n4. Años Bisiestos:")
escribir("  ¿2024 es bisiesto? " .. lapsos.esBisiesto(2024))
escribir("  ¿2025 es bisiesto? " .. lapsos.esBisiesto(2025))

# 5. Aritmética de fechas
escribir("\n5. Viajes en el Tiempo:")
futuro = lapsos.sumar(hoy, 30, "dias")
escribir("  En 30 días: " .. lapsos.formato(futuro, "dd/MM/yyyy"))

pasado = lapsos.restar(hoy, 1, "año")
escribir("  Hace 1 año: " .. lapsos.formato(pasado, "dd/MM/yyyy"))

evento = lapsos.sumar(hoy, 1000, "horas")
escribir("  En 1000 horas: " .. lapsos.formato(evento, "dd/MM/yyyy HH:mm"))

# 6. Diferencias
escribir("\n6. Cálculo de Diferencias:")
meta = lapsos.fecha(2026, 1, 1, 0, 0, 0)
falta = lapsos.diferencia(hoy, meta)

escribir("  Faltan para Año Nuevo 2026:")
escribir("    " .. falta.dias .. " días")
escribir("    " .. falta.total_horas .. " horas totales")

# 7. Casos de uso real: Calcular edad
escribir("\n7. Caso Práctico - Calcular Edad:")
nacimiento = lapsos.fecha(1995, 3, 15)
edad = lapsos.diferencia(nacimiento, hoy)
escribir("  Nacido el: " .. lapsos.formato(nacimiento, "dd/MM/yyyy"))
escribir("  Edad: " .. mate.piso(edad.total_dias / 365.25) .. " años")
escribir("  (" .. mate.piso(edad.total_dias) .. " días de vida)")

escribir("\n✅ Todas las pruebas completadas")
```

## 📚 Nivel 5: Implementación Matemática Avanzada

🎯 **Ejemplo: Plugin para Resolución de Ecuaciones Cuadráticas**

Ahora que dominas los conceptos, vamos a crear algo más matemático: un resolvedor de ecuaciones cuadráticas usando la fórmula general.

### Plugin MatePro

```csharp
using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace MatePro;

public class Calculadora
{
    [LatinoExtension("resolver_cuadratica", 
        Help = "Resuelve ecuaciones ax² + bx + c = 0 usando la fórmula general", 
        Args = "a, b, c",
        Returns = "diccionario")]
    public static LatinoObject Resolver(List<LatinoObject> args)
    {
        try
        {
            // 1. Extraer coeficientes
            if (args.Count < 3)
                throw new RuntimeError(null, "Se necesitan 3 coeficientes: a, b, c");

            double a = ((LatinoNumber)args[0]).Value;
            double b = ((LatinoNumber)args[1]).Value;
            double c = ((LatinoNumber)args[2]).Value;

            // Validar que 'a' no sea cero (si no, no es cuadrática)
            if (Math.Abs(a) < 0.0000001)
                throw new RuntimeError(null, "El coeficiente 'a' no puede ser cero");

            // 2. Calcular discriminante: b² - 4ac
            double discriminante = Math.Pow(b, 2) - (4 * a * c);

            // 3. Preparar respuesta
            var respuesta = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);

            void Agregar(string k, string v) => 
                respuesta[new LatinoString(k)] = new LatinoString(v);
            void AgregarNum(string k, double v) => 
                respuesta[new LatinoString(k)] = new LatinoNumber(v);

            // Siempre incluir el discriminante
            AgregarNum("discriminante", discriminante);

            // 4. Resolver según el discriminante
            if (discriminante > 0)
            {
                // Dos soluciones reales distintas
                double x1 = (-b + Math.Sqrt(discriminante)) / (2 * a);
                double x2 = (-b - Math.Sqrt(discriminante)) / (2 * a);
                
                AgregarNum("x1", x1);
                AgregarNum("x2", x2);
                Agregar("tipo", "Dos soluciones reales distintas");
            }
            else if (Math.Abs(discriminante) < 0.0000001)
            {
                // Una solución doble (discriminante = 0)
                double x = -b / (2 * a);
                AgregarNum("x1", x);
                AgregarNum("x2", x);
                Agregar("tipo", "Una solución doble (raíz repetida)");
            }
            else
            {
                // Soluciones complejas (discriminante < 0)
                double parteReal = -b / (2 * a);
                double parteImaginaria = Math.Sqrt(Math.Abs(discriminante)) / (2 * a);
                
                AgregarNum("parte_real", parteReal);
                AgregarNum("parte_imaginaria", parteImaginaria);
                Agregar("tipo", "Dos soluciones complejas conjugadas");
                Agregar("x1_str", $"{parteReal} + {parteImaginaria}i");
                Agregar("x2_str", $"{parteReal} - {parteImaginaria}i");
            }

            return new LatinoDictionary(respuesta);
        }
        catch (RuntimeError)
        {
            throw; // Re-lanzar errores de Latino
        }
        catch (Exception ex)
        {
            throw new RuntimeError(null, "Error en los parámetros: " + ex.Message);
        }
    }
}
```

**Uso en Latino:**

```latino
incluir("MatePro.dll")

escribir("=== Resolvedor de Ecuaciones Cuadráticas ===\n")

# Caso 1: Dos soluciones reales (x² - 5x + 6 = 0)
# Soluciones: x = 2 y x = 3
escribir("Caso 1: x² - 5x + 6 = 0")
r1 = resolver_cuadratica(1, -5, 6)
escribir("  Tipo: " .. r1["tipo"])
si dic.contiene(r1, "x1")
    escribir("  x1 = " .. r1["x1"])
    escribir("  x2 = " .. r1["x2"])
fin

# Caso 2: Una solución doble (x² - 4x + 4 = 0)
# Solución: x = 2 (doble)
escribir("\nCaso 2: x² - 4x + 4 = 0")
r2 = resolver_cuadratica(1, -4, 4)
escribir("  Tipo: " .. r2["tipo"])
escribir("  x = " .. r2["x1"])

# Caso 3: Soluciones complejas (x² + 2x + 5 = 0)
escribir("\nCaso 3: x² + 2x + 5 = 0")
r3 = resolver_cuadratica(1, 2, 5)
escribir("  Tipo: " .. r3["tipo"])
si dic.contiene(r3, "x1_str")
    escribir("  x1 = " .. r3["x1_str"])
    escribir("  x2 = " .. r3["x2_str"])
fin
```

## 🔧 Herramientas y Patrones Profesionales

### Clase Helper Reutilizable

Para evitar repetir código, crea una clase con métodos auxiliares:

```csharp
public static class LatinoHelpers
{
    /// <summary>
    /// Extrae un número de forma segura con valor por defecto
    /// </summary>
    public static double GetNumber(List<LatinoObject> args, int index, double defaultValue = 0)
    {
        if (index >= args.Count)
            return defaultValue;
        
        return args[index] is LatinoNumber num ? num.Value : defaultValue;
    }

    /// <summary>
    /// Extrae un texto de forma segura
    /// </summary>
    public static string GetString(List<LatinoObject> args, int index, string defaultValue = "")
    {
        if (index >= args.Count)
            return defaultValue;
        
        return args[index]?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Extrae un booleano de forma segura
    /// </summary>
    public static bool GetBoolean(List<LatinoObject> args, int index, bool defaultValue = false)
    {
        if (index >= args.Count)
            return defaultValue;
        
        return args[index] is LatinoBoolean b ? b.Value : defaultValue;
    }

    /// <summary>
    /// Extrae una lista o lanza error
    /// </summary>
    public static LatinoList GetList(List<LatinoObject> args, int index)
    {
        if (index >= args.Count || args[index] is not LatinoList list)
            throw new RuntimeError(null, $"Se esperaba una lista en la posición {index + 1}");
        
        return list;
    }

    /// <summary>
    /// Extrae un diccionario o lanza error
    /// </summary>
    public static LatinoDictionary GetDictionary(List<LatinoObject> args, int index)
    {
        if (index >= args.Count || args[index] is not LatinoDictionary dic)
            throw new RuntimeError(null, $"Se esperaba un diccionario en la posición {index + 1}");
        
        return dic;
    }

    /// <summary>
    /// Valida la cantidad mínima de argumentos
    /// </summary>
    public static void ValidateArgCount(List<LatinoObject> args, int minimum, string functionName)
    {
        if (args.Count < minimum)
            throw new RuntimeError(null, 
                $"{functionName}() requiere al menos {minimum} argumento(s), se recibieron {args.Count}");
    }
}
```

**Uso de los Helpers:**

```csharp
[LatinoExtension("mi_funcion", Args = "a, b, [c], [texto]")]
public static LatinoObject MiFuncion(List<LatinoObject> args)
{
    // Validar cantidad mínima
    LatinoHelpers.ValidateArgCount(args, 2, "mi_funcion");
    
    // Obtener parámetros requeridos
    double a = LatinoHelpers.GetNumber(args, 0);
    double b = LatinoHelpers.GetNumber(args, 1);
    
    // Obtener parámetros opcionales con valores por defecto
    double c = LatinoHelpers.GetNumber(args, 2, 10.0);
    string texto = LatinoHelpers.GetString(args, 3, "Sin texto");
    
    // ... tu lógica aquí
}
```

## 📦 Compilación y Distribución

### Compilar en Modo Release

Siempre compila en modo Release para producción:

```bash
dotnet build -c Release
```

El archivo DLL estará en: `bin/Release/net10.0/TuPlugin.dll`

### Opciones de Distribución

#### 1. Instalación Local (por proyecto)

Copia la DLL junto a tu script `.lat`:

```text
MiProyecto/
├── app.lat
└── MiPlugin.dll
```

**Uso:**

```latino
incluir("MiPlugin.dll")
```

#### 2. Instalación Global

Copia la DLL en la raíz del ejecutable de Latino.NET o en su subcarpeta `lib/`:

**Windows:**

```powershell
# Si Latino.NET está instalado en C:\LatinoNET
copy MiPlugin.dll C:\LatinoNET\lib\
```

**Linux/Mac:**

```bash
# Junto al binario en /usr/local/bin o su subcarpeta lib/
sudo cp MiPlugin.dll /usr/local/bin/lib/
```

**Uso** (desde cualquier lugar):

```latino
incluir("MiPlugin.dll")
```

#### 3. Distribución como Paquete

Crea un archivo `README.md` junto a tu DLL:

```markdown
# Plugin MiPlugin para Latino

## Instalación
1. Copia `MiPlugin.dll` a la carpeta de tu proyecto Latino
2. O instala globalmente en `%LATINO_HOME%/plugins/`

## Uso
```latino
incluir("MiPlugin")

resultado = mi_funcion(10, 20)
escribir(resultado)
```

## Funciones Disponibles

* `mi_funcion(a, b)`: Descripción...
* `otra_funcion(x)`: Descripción...

## 📋 Checklist de Calidad

Antes de publicar tu plugin, verifica:

### Código

* [ ] Todos los métodos públicos tienen `[LatinoExtension]`
* [ ] Cada función tiene `Help` descriptivo
* [ ] Los parámetros están documentados en `Args`
* [ ] El tipo de retorno está en `Returns` (opcional pero útil)
* [ ] Validación robusta de argumentos (cantidad y tipo)
* [ ] Manejo de errores con `RuntimeError`
* [ ] Métodos auxiliares son `private`

### Organización

* [ ] Funciones relacionadas usan namespace (`"modulo.funcion"`)
* [ ] Nombres descriptivos en español (o tu idioma target)
* [ ] Sin funciones con más de 5 parámetros
* [ ] Código comentado en puntos clave

### Testing

* [ ] Probado con argumentos válidos
* [ ] Probado con argumentos inválidos
* [ ] Probado con casos límite (números negativos, cero, vacíos)
* [ ] Compilado en modo Release
* [ ] Sin warnings del compilador

### Documentación

* [ ] README con instrucciones de instalación
* [ ] Ejemplos de uso de cada función
* [ ] Licencia clara (si es open source)
* [ ] Información de contacto/soporte

## 📚 Buenas Prácticas

**✅ Buenas prácticas recomendadas:**

* **Validar siempre los argumentos**: Tipo y cantidad
* **Usar namespaces**: Mantiene el código organizado
* **Mensajes de error claros**: En español, descriptivos
* **Documentar con atributos**: Help, Args, Returns
* **Crear helpers reutilizables**: DRY (Don't Repeat Yourself)
* **Usar `LatinoObjectComparer.Instance`**: Para diccionarios
* **Manejar excepciones**: Convertir a `RuntimeError`
* **Valores por defecto**: Para parámetros opcionales

### 📦 Dependencias: el Principio

**Un plugin se escribe en C# puro.** Usa `System` y lo que ya trae `Latino.Core`, y nada más:
sin `PackageReference`, sin NuGet, sin DLLs ajenas. Es la forma en que están escritos los plugins del
núcleo, y mantiene la instalación en «copiar una carpeta».

**La regla no es absoluta.** Una dependencia externa se admite cuando cumple alguna de estas
condiciones, y solo entonces:

1. **Conector o interoperabilidad con una plataforma externa** —bases de datos, servicios o formatos que solo existen en un motor ajeno.
2. **El caso requiere una capacidad que el BCL no tiene** y que no conviene reimplementar.
3. **La dependencia es estándar, estable y con licencia permisiva** (MIT/Apache/BSD).

**El precedente:** el plugin `sqlite` del núcleo no existiría sin `Microsoft.Data.Sqlite`. La regla está
para guiar la decisión, no para excluir la realidad.

Añadir una dependencia **no es gratis**: obliga a empacar y versionar sus assemblies transitivos, y la
instalación deja de ser copiar una carpeta. Antes de añadirla, pregúntate si cabe un port propio de lo
que necesitas, y justifica en el PR por qué la dependencia es inevitable.

**❌ Prácticas desaconsejadas:**

* **Más de 5 parámetros**: Difícil de usar y recordar
* **Excepciones de .NET directas**: Usar `RuntimeError`
* **Nombres en inglés**: Si tu audiencia habla español
* **Modificar estado global**: Sin documentarlo claramente
* **Dependencias sin justificar**: Añadidas por comodidad, sin cumplir las tres condiciones del principio
* **Asumir tipos**: Siempre validar con `is`
* **Ignorar casos límite**: Null, cero, vacíos

## 🔬 Temas Avanzados

### Acceder al Intérprete

Para casos especiales, puedes usar la firma completa:

```csharp
[LatinoExtension("obtener_version")]
public static LatinoObject ObtenerVersion(Interpreter interpreter, List<LatinoObject> args)
{
    // Leer variable global de Latino
    var version = interpreter.Globals.GetAt("VERSION");
    return version ?? new LatinoString("desconocida");
}
```

### Trabajar con Archivos

```csharp
[LatinoExtension("archivo.leer", Args = "ruta")]
public static LatinoObject LeerArchivo(List<LatinoObject> args)
{
    string ruta = args[0].ToString();
    
    try
    {
        string contenido = System.IO.File.ReadAllText(ruta);
        return new LatinoString(contenido);
    }
    catch (Exception ex)
    {
        throw new RuntimeError(null, $"Error al leer archivo: {ex.Message}");
    }
}
```

### Plugins Políglotas

Latino corre sobre .NET CLR, así que puedes usar:

* **Visual Basic .NET**: Sintaxis más amigable para principiantes
* **F#**: Potente para matemáticas y ciencia de datos
* **Cualquier lenguaje .NET**

El proceso es idéntico: creas una biblioteca de clases, referencias `Latino.Core.dll` y usas `[LatinoExtension]`.
