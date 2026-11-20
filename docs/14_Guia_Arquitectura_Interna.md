# CAPÍTULO 14: Arquitectura Interna del Motor

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores del núcleo, contribuidores de código abierto, ingenieros de sistemas y estudiantes de compiladores que deseen entender en profundidad la ingeniería interna de Latino.NET.  
**Propósito:** Explicar el ciclo de vida completo de un programa: análisis léxico con soporte de grafemas Unicode, análisis sintáctico con recuperación de errores en modo pánico, evaluación del AST en el runtime y sincronización de memoria con el Language Server Protocol (LSP).

## La Anatomía de un Intérprete Moderno

Comprender la sintaxis de un lenguaje permite escribir programas; comprender su arquitectura interna permite diseñar sistemas fiables, predecir el impacto de la memoria y contribuir a la evolución del motor.

A menudo se describe a los lenguajes de programación como cajas negras que reciben texto y generan resultados de forma casi mágica. En la práctica, un intérprete es una cadena de montaje de alta precisión donde cada etapa transforma la información sucesivamente: de caracteres crudos en el disco duro a un flujo de tokens léxicos, de tokens a un árbol sintáctico estructurado, y de ese árbol a la ejecución física sobre el procesador.

Latino.NET implementa un diseño clásico de tipo **Tree-Walker** enriquecido con optimizaciones modernas para la plataforma **.NET 10**.

### El Arte de Diseñar un Intérprete Robusto

Al construir Latino.NET, la prioridad arquitectónica no fue simplemente crear un motor que evaluara expresiones en español, sino garantizar que el sistema fuera **seguro por defecto, amigable ante fallos y fácil de mantener**:

* **Un Solo Cerebro:** Rechazamos el enfoque tradicional de construir un analizador en C# para el ejecutable y otro separado en TypeScript para el editor de código. En Latino.NET, el intérprete de consola y la extensión de Visual Studio Code comparten exactamente la misma biblioteca central (`Latino.Core.dll`), garantizando paridad absoluta entre la validación en pantalla y la ejecución real.
* **Seguridad Defensiva (*Fail-Safe*):** Los errores de sintaxis o lógica del script nunca deben provocar una caída inesperada del proceso anfitrión. El runtime desbobina la pila de llamadas (*stack unwinding*) de forma limpia y traduce las fallas a excepciones estructuradas en español.
* **Aislamiento Léxico y Rendimiento Híbrido:** Cada hilo de ejecución posee su propio entorno de memoria (`LatinoEnvironment`), mientras que las funciones críticas delegan su trabajo pesado directamente a la Base Class Library (BCL) de Microsoft.

A continuación, examinaremos el flujo completo que recorre cada línea de código desde que pulsas guardar en el editor hasta que el procesador ejecuta la instrucción.

## Pipeline de Ejecución

Latino.NET implementa un intérprete *Tree-Walker* clásico con extensiones modernas:

```text
Código Fuente (.lat)
    ↓
[1] LEXER → Tokens (con Unicode completo)
    ↓
[2] PARSER → AST (con recuperación de errores)
    ↓
[3] INTERPRETER → Ejecución directa
    ↓
[4] STDLIB → Funciones nativas (mate, sis, archivo...)
    ↓
[5] LSP → IntelliSense en VS Code
```

## 1. Lexer - Análisis Léxico

**Responsabilidad:** Convertir texto en tokens clasificados.

### Capacidades Clave

* **Unicode Total:** Soporta emojis y acentos en identificadores y strings
* **Escapes:** Procesa `\n`, `\t`, `\uXXXX` con `Regex.Unescape`
* **Notación Científica:** Maneja `1.5e-10` correctamente
* **Comentarios Híbridos:** `#`, `//` y `/* */`

### Tokens Especiales

```csharp
// Operadores compuestos de 3 caracteres
"..=" → TokenType.ConcatEqual

// Operadores de 2 caracteres
"++", "--", "+=", "**", "&&", "||", "~=" (regex match)

// Palabras clave con alias
{ "funcion", TokenType.Keyword }
{ "fun", TokenType.Keyword }      // Alias
{ "crear", TokenType.New }
{ "nueva", TokenType.New }        // Concordancia de género
{ "nuevo", TokenType.New }
```

🎯 **Ejemplo de Procesamiento**

```latino
nombre = "José\n"  // \n será convertido a salto real
x = 1.5e10         // Parseado como 15,000,000,000
emoji = "👍"       // Soportado nativamente
```

**Resultado:** Lista de `Token(Type, Text, Value?, Line, Col)`

## 2. Parser - Análisis Sintáctico

**Responsabilidad:** Construir el AST validando gramática.

### Recuperación de Errores (Panic Mode)

```csharp
public List<ParseError> Errors { get; } = new();

private Token Consume(TokenType type, string message)
{
    if (Check(type)) return Advance();
    
    Token current = Peek();

    // Lógica de Error Amigable
    if (type == TokenType.Identifier)
    {
        // Caso general para palabras reservadas (si, mientras, o, no...)
        // Verificamos si es una Keyword o un Operador Lógico con texto
        if (current.Type == TokenType.Keyword || 
            current.Type == TokenType.LogicAnd || 
            current.Type == TokenType.LogicOr || 
            current.Type == TokenType.LogicNot)
        {
            throw Error(current, $"'{current.Text}' es una palabra reservada, no puede usarse como variable.");
        }
    }
    
    throw Error(current, message); // Acumula en Errors[]
}
```

**Ventaja:** El LSP puede reportar **todos** los errores de sintaxis de un archivo en una sola pasada.

### Nodos AST Soportados

| Categoría | Nodos | Características |
| :--- | :--- | :--- |
| **Control** | `If`, `While`, `DoWhile`, `For`, `Foreach`, `Switch` | Anidación completa |
| **Saltos** | `Goto`, `Label`, `Break`, `Continue` | Goto manipula índice de bucle |
| **POO** | `Class`, `GetExpression`, `SetExpression` | Herencia simple |
| **Datos** | `ListLiteral`, `DictLiteral`, `TupleExpression` | Trailing commas permitidas |
| **Control Avanzado** | `SwapStatement`, `TernaryExpression`, `IndexSetExpression` | Intercambio `~`, ternario `?:`, asignación por índice |
| **Errores** | `TryCatch` | Captura `RuntimeError` y excepciones .NET |
| **Funciones** | `FunctionStatement`, `AnonymousFunction` | Closures completos |

## 3. Interpreter - Motor de Ejecución

**Responsabilidad:** Recorrer el AST y ejecutar acciones.

### Gestión de Memoria (LatinoEnvironment)

```csharp
public class LatinoEnvironment
{
    private readonly LatinoEnvironment? _enclosing; // Padre (closure)
    private readonly ConcurrentDictionary<string, LatinoObject> _values = new();

    public void Assign(Token name, LatinoObject value)
    {
        // 1. Protección de constantes (UPPER_CASE)
        if (IsConstantName(name.Text) && Exists(name.Text))
            throw new RuntimeError(name, "No se puede reasignar constante.");

        // 2. Si existe localmente, actualizar
        if (_values.ContainsKey(name.Text))
        {
            _values[name.Text] = value;
            return;
        }

        // 3. Si existe en padres (closure), actualizar arriba
        if (_enclosing != null && _enclosing.Exists(name.Text))
        {
            _enclosing.Assign(name, value);
            return;
        }

        // 4. Si es nueva, crearla AQUÍ (scope local)
        _values[name.Text] = value;
    }
}
```

**Reglas de Alcance:**

* `local x = 10` → Crea en bloque actual (shadowing)
* `global x = 10` → Crea/modifica en raíz del programa
* `x = 10` → Busca en cadena de padres, crea local si no existe

### Protección de Stack

```csharp
public int MaxDepth { get; set; } = 100; // Límite de recursión
public int CurrentDepth = 0;

// En cada llamada a función (LatinoFunction.cs):
if (interpreter.CurrentDepth >= interpreter.MaxDepth)
    throw new RuntimeError(_declaration.Name, $"Límite de recursividad excedido ({interpreter.MaxDepth}).");

interpreter.CurrentDepth++;
try
{
    // ... ejecución de la función ...
}
finally
{
    interpreter.CurrentDepth--; // Siempre decrementa, incluso si hay error
}
```

**Configurable:** `sis.config("max_stack", 250)`

### Características Avanzadas

#### 0. Fast-Path de Lambdas (Optimización)

```csharp
// En LatinoFunction.cs — Optimización para lambdas de una sola línea
if (_declaration.Body.Count == 1 && _declaration.Body[0] is ReturnStatement retStmt)
{
    if (retStmt.Value != null)
        return interpreter.EvaluateInEnv(retStmt.Value, environment);
    return LatinoNull.Instance;
}
```

**Impacto:** Las lambdas con una sola expresión (`fun(x) ret x * 2 fin`) se saltan la creación de un entorno completo y la ejecución por bloques.

> **Medición (v1.0.81, 200.000 invocaciones, 3 repeticiones):** una función de una línea tardó ~800 ms y su equivalente de tres líneas ~2.200 ms — alrededor de **2,7× más rápido**. La cifra depende del hardware y de la carga; lo verificable es que **el fast-path existe y reduce trabajo**, no una multiplier fija.

#### 1. Smart Casting (Conversión Automática)

```csharp
private double ToDouble(LatinoObject obj)
{
    if (obj is LatinoNumber n) return n.Value;
    if (obj is LatinoBoolean b) return b.Value ? 1.0 : 0.0;
    if (obj is LatinoString s && double.TryParse(s.Value, ...))
        return result; // "3.14" → 3.14
    if (obj is LatinoNull) return 0.0;
    throw new RuntimeError(null, "No convertible a número.");
}
```

**Ejemplo:**

```latino
x = "10" + 5  // → 15 (conversión automática)
y = "abc" + 5 // → ERROR (cadena no numérica)
```

#### 2. Igualdad Estructural (Tuplas y Listas)

```csharp
// En LatinoTuple.cs — protegido contra ciclos infinitos
[ThreadStatic] private static HashSet<object>? _activeEqualsTuple;

public override bool Equals(object? obj)
{
    if (ReferenceEquals(this, obj)) return true;
    if (obj is not LatinoTuple other || Elements.Count != other.Elements.Count) return false;

    _activeEqualsTuple ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
    if (!_activeEqualsTuple.Add(this)) return true; // Rompe el ciclo

    try {
        for (int i = 0; i < Elements.Count; i++)
            if (!Elements[i].Equals(other.Elements[i])) return false;
        return true;
    }
    finally { _activeEqualsTuple.Remove(this); }
}
```

**Resultado:**

```latino
(1, 2) == (1, 2)  # → cierto (compara valores)
[1, 2] == [1, 2]  # → cierto (compara elementos)
```

#### 3. Índices Negativos (Python-style)

```csharp
if (idxObj is LatinoList list && index is LatinoNumber n)
{
    int i = (int)n.Value;
    if (i < 0) i += list.Elements.Count; // -1 → último elemento
    
    if (i < 0 || i >= list.Elements.Count)
        throw new RuntimeError(..., "Índice fuera de rango.");
    
    return list.Elements[i];
}
```

#### 4. Soporte Unicode en Cadenas (Grafemas)

```csharp
if (idxObj is LatinoString s && index is LatinoNumber sn)
{
    var si = new StringInfo(s.Value); // Maneja emojis compuestos
    int idx = (int)sn.Value;
    if (idx < 0) idx += si.LengthInTextElements;
    
    return new LatinoString(si.SubstringByTextElements(idx, 1)); // 1 grafema completo
}
```

**Ejemplo:**

```latino
emoji = "👨‍👩‍👧‍👦"
escribir(emoji[0])  // → "👨‍👩‍👧‍👦" (familia completa, no solo 👨)
```

## 4. StdLib - Biblioteca Estándar

### Arquitectura Modular

```csharp
public static void LoadAll(Interpreter interpreter)
{
    MathLib.Register(interpreter);     // mate.*
    StringLib.Register(interpreter);   // cadena.*
    SysLib.Register(interpreter);      // sis.*
    FileLib.Register(interpreter);     // archivo.*
    ListLib.Register(interpreter);     // lista.*
    // ...
}
```

### NativeFunction - Puente C# ↔ Latino

```csharp
public class NativeFunction : LatinoObject, ICallable
{
    private readonly Func<Interpreter, List<LatinoObject>, LatinoObject> _logic;
    public int Arity { get; } // -1 para variádica
    public string Description { get; private set; } = "";
    public string ParameterNames { get; private set; } = "";

    public NativeFunction WithDocs(string desc, string paramsList = "")
    {
        Description = desc;
        ParameterNames = paramsList;
        return this; // Chainable para LSP
    }
}
```

🎯 **Ejemplo Real: mate.sen()**

```csharp
Def("sen", (i, args) => new LatinoNumber(Math.Sin(Get(args[0]))), 
    1, 
    "Seno de un ángulo (radianes).", 
    "rad");
```

**Resultado en VS Code:**

* **Autocompletado:** `mate.sen(rad)`
* **Hover:** "Seno de un ángulo (radianes)."

### Blindaje de Seguridad

```csharp
// Timeout en Regex (anti-ReDoS)
if (b.Operator.Type == TokenType.RegexMatch)
{
    try
    {
        return new LatinoBoolean(
            Regex.IsMatch(input, pattern, 
                RegexOptions.None, 
                TimeSpan.FromSeconds(2)) // TIMEOUT
        );
    }
    catch (RegexMatchTimeoutException)
    {
        throw new RuntimeError(..., "Regex excedió 2s.");
    }
}
```

### Sistema de Plugins (Extensibilidad)

```csharp
interpreter.Globals.Define("incluir", new NativeFunction((interp, args) => {
    string rawName = args[0].ToString();
    
    // Buscar .lat o .dll en:
    // 1. Carpeta actual
    // 2. Carpeta del ejecutable
    // 3. Subcarpeta lib/
    
    if (finalPath.EndsWith(".dll"))
    {
        PluginLoader.Load(interp, finalPath); // Reflexión
    }
    else
    {
        // Cargar script con protección de ciclos
        if (_activeFiles.Contains(finalPath))
            throw new RuntimeError(..., "Dependencia circular.");
        
        _activeFiles.Add(finalPath);
        // ... parsear y ejecutar
        _activeFiles.Remove(finalPath);
    }
}));
```

### Política de Inclusión: ¿Qué pertenece al Core y qué a un Plugin?

Para evitar el sobrepeso del motor y prevenir la obsolescencia que sufrieron lenguajes históricos (el síndrome de las *"baterías agotadas"*), Latino.NET establece una regla de gobierno estricta desde su versión v1.0.81: **el núcleo debe permanecer esbelto, universal y altamente portable**.

Toda nueva propuesta de funcionalidad debe someterse al **Test de los 4 Filtros de Oro**:

```text
                              ¿Nueva Funcionalidad?
                                        │
                                        ▼
                ┌───────────────────────────────────────────────┐
                │ 1. ¿Es de uso transversal para el ≥80%        │─── NO ───► [ PLUGIN ]
                │    de cualquier tipo de script o usuario?     │
                └───────────────────────────────────────────────┘
                                        │ SÍ
                                        ▼
                ┌───────────────────────────────────────────────┐
                │ 2. ¿El lenguaje o sus tipos fundamentales     │─── SÍ ───► [ CORE ]
                │    dependen de ella para operar?              │
                └───────────────────────────────────────────────┘
                                        │ NO
                                        ▼
                ┌───────────────────────────────────────────────┐
                │ 3. ¿Introduce dependencias externas pesadas,  │─── SÍ ───► [ PLUGIN ]
                │    paquetes NuGet o llamadas a APIs del SO?   │
                └───────────────────────────────────────────────┘
                                        │ NO
                                        ▼
                ┌───────────────────────────────────────────────┐
                │ 4. ¿Afectaría negativamente el arranque frío  │─── SÍ ───► [ PLUGIN ]
                │    o la portabilidad en WebAssembly (WASM)?   │
                └───────────────────────────────────────────────┘
                                        │ NO
                                        ▼
                                 [ CORE / StdLib ]
```

#### Los 4 Filtros Explicados

1. **Filtro de Universalidad Transversal (Regla del 80/20):**  
   ¿Esta función la necesitará por igual un estudiante aprendiendo algoritmia, un administrador de sistemas automatizando tareas y un analista procesando datos?
   * ✅ **Al Core:** Cadenas (`cadena`), colecciones primarias (`lista`, `dic`, `tupla`), archivos locales (`archivo`), matemáticas elementales (`mate`) y tiempo básico (`sis`).
   * ❌ **A Plugin:** Álgebra de números complejos, comunicación con puertos serie/Arduino, procesamiento de imágenes o fórmulas de ingeniería avanzada.

2. **Filtro de Cohesión Semántica:**  
   ¿El analizador sintáctico, el Garbage Collector o el sistema de tipos de Latino.NET dependen de esta estructura para su propia existencia?
   * ✅ **Al Core:** Funciones de introspección (`tipo()`, `longitud()`), gestión de memoria (`tramo()`, especificación aceptada para la Fase 4 / v1.1.0+ según ADR-015, aún no disponible en v1.0.81) y operadores de conversión (`alogico()`, `anumero()`).
   * ❌ **A Plugin:** Motores de bases de datos relacionales o formatos específicos de terceros (XML, YAML).

3. **Filtro de Dependencias y Huella Binaria:**  
   ¿Requiere paquetes externos de NuGet o librerías nativas del sistema operativo?
   * `Latino.Core.dll` debe permanecer con dependencias externas mínimas o nulas. Si una característica exige referenciar librerías como `Microsoft.Data.Sqlite`, `System.IO.Ports` o motores gráficos de terceros, **debe aislarse obligatoriamente en un plugin**.

4. **Filtro de Portabilidad Universal (Criterio WASM):**  
   ¿Puede ejecutarse de forma idéntica en un servidor Linux, en un chip ARM y dentro del navegador web vía WebAssembly?
   * Si una funcionalidad depende de hilos del SO (`Thread`), sockets bloqueantes o archivos físicos de disco duro, debe vivir en un plugin (`hilos.dll`, `proceso.dll`, `sqlite.dll`). Esto garantiza que `Latino.Core` compile limpiamente a WebAssembly para el Playground oficial sin errores de plataforma.

#### Clasificación del Ecosistema

| Nivel | Ubicación | Criterio de Inclusión | Ejemplos |
| :--- | :--- | :--- | :--- |
| **Nivel A: Núcleo (`Latino.Core`)** | Embebido en el motor | Supera los 4 filtros. Esencial para cualquier programa. | `mate`, `cadena`, `lista`, `dic`, `archivo`, `sis`, `web`. |
| **Nivel B: Plugins Oficiales** | Carpeta `plugins/` en la distribución | Herramientas de grado industrial que violan los filtros 3 o 4 pero son mantenidas por el equipo del Core. | `sqlite`, `hilos`, `lapsos`, `cripto`, `zip`, `proceso`, `formulas`. |
| **Nivel C: Plugins de Ecosistema** | Repositorios externos / Comunidad | Dominio específico, bindings de hardware o librerías experimentales. | `serial`, `servidor`, interfaces gráficas (`ui`). |

## 5. Language Server (LSP)

### Arquitectura del Servidor

```csharp
// Program.cs
await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())  // JSON-RPC desde VS Code
        .WithOutput(Console.OpenStandardOutput()) // JSON-RPC hacia VS Code
        .WithServices(services =>
        {
            // SINGLETON: Una sola instancia compartida
            services.AddSingleton<Interpreter>(sp => {
                var interp = new Interpreter();
                StdLibLoader.LoadAll(interp); // Carga ÚNICA de mate, sis, etc.
                return interp;
            });
            
            services.AddSingleton<PluginManager>();
        })
        .WithHandler<TextDocumentHandler>()   // Validación
        .WithHandler<CompletionHandler>()     // Autocompletado
        .WithHandler<HoverHandler>()          // Documentación
);
```

**Ventaja del Singleton:** El autocompletado puede inspeccionar la memoria real del intérprete (no una lista estática).

### BufferManager - Cache de Edición

```csharp
public static class BufferManager
{
    private static readonly ConcurrentDictionary<DocumentUri, string> _buffers = new();

    public static void UpdateBuffer(DocumentUri uri, string text) 
        => _buffers[uri] = text;
}
```

**Por qué existe:** VS Code envía el contenido del editor **antes de guardar**. Sin este cache, no podríamos validar código no guardado.

### TextDocumentHandler - Validación en Tiempo Real

```csharp
public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken ct)
{
    foreach (var change in request.ContentChanges)
    {
        BufferManager.UpdateBuffer(request.TextDocument.Uri, change.Text);
        _pluginManager.ScanAndLoadPlugins(request.TextDocument.Uri, change.Text);
        
        ValidateTextDocument(request.TextDocument.Uri, change.Text);
    }
}

private void ValidateTextDocument(DocumentUri uri, string text)
{
    var lexer = new Lexer(text);
    var tokens = lexer.Tokenize();
    var parser = new Parser(tokens);
    parser.Parse();
    
    // Convertir ParseError[] a Diagnostics[]
    foreach (var error in parser.Errors)
    {
        diagnostics.Add(new Diagnostic {
            Severity = DiagnosticSeverity.Error,
            Message = error.Message,
            Range = new Range(
                new Position(error.Token.Line - 1, error.Token.Column - 1), // VS Code es base-0
                new Position(error.Token.Line - 1, error.Token.Column + error.Token.Text.Length)
            )
        });
    }
    
    _router.TextDocument.PublishDiagnostics(...); // Líneas rojas en el editor
}
```

### CompletionHandler - Introspección Dinámica

```csharp
public Task<CompletionList> Handle(CompletionParams request, CancellationToken ct)
{
    var completions = new List<CompletionItem>();

    // 1. Keywords del lenguaje
    foreach (var kw in Lexer.Keywords.Keys)
        completions.Add(new CompletionItem { Label = kw, Kind = CompletionItemKind.Keyword });

    // 2. Introspección del Interpreter (librerías estándar)
    foreach (var key in _interpreter.Globals.GetKeys())
    {
        var obj = _interpreter.Globals.GetAt(key);
        
        if (obj is LatinoDictionary lib) // Es una librería (mate, sis...)
        {
            completions.Add(new CompletionItem { Label = key, Kind = CompletionItemKind.Module });
            
            // Añadir todos sus métodos
            foreach (var methodKey in lib.Pairs.Keys)
            {
                var methodObj = lib.Pairs[methodKey];
                if (methodObj is NativeFunction nf)
                {
                    // Crear snippet: mate.sen(${1:rad})
                    var pList = nf.ParameterNames.Split(',');
                    var snippetArgs = string.Join(", ", 
                        pList.Select((p, i) => $"${{{i+1}:{p.Trim()}}}"));
                    
                    completions.Add(new CompletionItem {
                        Label = $"{key}.{methodKey}",
                        InsertText = $"{key}.{methodKey}({snippetArgs})",
                        InsertTextFormat = InsertTextFormat.Snippet,
                        Documentation = nf.Description
                    });
                }
            }
        }
    }

    // 3. Plugins cargados dinámicamente
    foreach (var func in _pluginManager.GetGlobalFunctions())
    {
        completions.Add(new CompletionItem { Label = func.Name, Documentation = func.Help });
    }

    return Task.FromResult(new CompletionList(completions));
}
```

### MagicStringsCatalog - Proveedor de Strings Predefinidos en Cadenas

A partir de la versión v1.0.81, el Language Server ofrece autocompletado contextual **dentro de cadenas literales**, delimitadas por comillas dobles o simples. Es la única superficie donde el autocompletado se aparta de la gramática del lenguaje: dentro de una cadena no hay palabras clave ni nombres de función que sugerir, y el valor correcto suele ser un dato de dominio.

El mecanismo reside en dos piezas. `CompletionHandler` detecta si el cursor está dentro de una cadena y, si lo está, cede el turno a `MagicStringsCatalog`.

```csharp
// CompletionHandler.cs - Detección de contexto de comillas
string textBeforeCursor = lineText.Substring(0, charIdx);

// Heurística simple: ¿estamos dentro de comillas en esta línea?
int doubleQuotesCount = textBeforeCursor.Count(c => c == '"');
int singleQuotesCount = textBeforeCursor.Count(c => c == '\'');

bool insideDouble = doubleQuotesCount % 2 != 0;
bool insideSingle = singleQuotesCount % 2 != 0;

if (insideDouble || insideSingle)
{
    // Estamos dentro de una cadena -> Ofrecer Catálogo

    // Calculamos el rango de la palabra actual para un reemplazo limpio
    var start = charIdx;
    while (start > 0 && !char.IsWhiteSpace(lineText[start - 1]) && lineText[start - 1] != '"' && lineText[start - 1] != '\'')
    {
        start--;
    }

    var range = new Range(new Position(lineIdx, start), new Position(lineIdx, charIdx));

    foreach (var item in MagicStringsCatalog.Items)
    {
        completions.Add(new CompletionItem {
            Label = item.Label,
            InsertText = item.InsertText,
            FilterText = item.FilterText,
            Detail = item.Detail,
            Kind = item.Kind,
            Preselect = item.Preselect,
            Documentation = item.Documentation,
            TextEdit = new TextEdit {
                NewText = item.InsertText ?? item.Label,
                Range = range
            }
        });
    }

    return Task.FromResult(new CompletionList(completions));
}
```

El conteo de comillas es una **heurística de línea**, no un análisis léxico: no distingue la comilla de apertura de una comilla dentro de un literal ya cerrado, ni sabe si la línea pertenece a un comentario. Es una decisión aceptable porque un error de detección solo produce una lista de sugerencias fuera de contexto; nunca produce un `InsertText` inválido, porque el `TextEdit` siempre reemplaza un rango ya calculado sobre el texto real.

#### Anatomía Técnica: la Máscara Visual (Label) frente al Valor Real (InsertText)

Un error común es suponer que los prefijos con `@` son cadenas que el motor ejecuta. **El motor de Latino.NET jamás procesa un prefijo con `@`**: escribir `"vel: kmh"` hace que `mate.convertir` falle con *Unidades no soportadas o incompatibles*, y escribir `"@kmh"` produce exactamente el mismo error. El prefijo pertenece al editor, no al lenguaje.

La arquitectura se divide en dos roles estrictos:

* **La etiqueta (`Label`):** máscara de interfaz usada por el editor para agrupar, ordenar y filtrar las sugerencias cuando el usuario escribe `@` (por ejemplo, `@vel: kmh` o `@cholesky: L`).
* **El texto insertado (`InsertText`):** el valor real y limpio que el editor escribe en el archivo al aceptar la sugerencia (por ejemplo, `kmh` o `L`).

```text
[ Usuario escribe:  mate.convertir(100, "@v
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│ El editor muestra el menú flotante (Labels):                 │
│   • @vel: kmh   (Velocidad: Kilómetros por hora)            │
│   • @vel: mph   (Velocidad: Millas por hora)                 │
│   • @vel: ms    (Velocidad: Metros por segundo)              │
└──────────────────────────────────────────────────────────────┘
                         │
               [ Usuario presiona ENTER ]
                         │
                         ▼
[ Código resultante:  mate.convertir(100, "kmh", "ms") ]
  El prefijo '@vel:' desaparece; el motor solo recibe 'kmh'.
```

El `TextEdit` cierra el contrato: aunque un editor ignorase el campo `InsertText`, el `NewText` del reemplazo es el mismo valor limpio. No hay dos caminos que puedan divergir.

#### Los Dos Catálogos: unificados en ambas superficies

`MagicStringsCatalog.cs` y los snippets de Sublime Text están unificados y ofrecen **ambas familias**: los Magic Strings de entrada (unidades, estilos, regex, plantillas SQL) y los Magic Strings de retorno estructurado de funciones.

El catálogo maestro del Language Server suma **1020 ítems sobre 209 prefijos**, repartidos así:

| Familia | Valores | Prefijos | Contenido |
| :--- | ---: | ---: | :--- |
| Entrada | 191 | 36 | Unidades de `mate.convertir`, estilos de `cadena.estilo`, modos de comparación, algoritmos de hash, patrones de regex, plantillas SQL, formatos y claves de configuración. |
| Retorno | 829 | 173 | Claves de los diccionarios que devuelven las 173 funciones auditadas, etiquetadas con la librería que las produce. |
| **Total** | **1020** | **209** | |

Esta unificación no fue automática. Hasta la unificación, el catálogo contenía solo las claves de retorno porque una regeneración previa lo había sobrescrito con una tabla del manual que cubría únicamente las funciones auditadas. Los 191 valores de entrada se recuperaron del historial de Git y **se verificaron uno a uno ejecutando la función correspondiente** contra el motor de v1.0.81, porque tres familias declaraban valores que el motor nunca aceptó: `@algebra` era el caso más claro, con `norma_1`, `norma_2` y `norma_inf` en lugar de `1`, `frobenius` e `inf`.

Una consecuencia práctica de la unificación: un usuario de VS Code y uno de Sublime Text reciben hoy exactamente las mismas sugerencias dentro de las cadenas, y ambos catálogos se regeneran desde la misma fuente de verdad, que es el apartado siguiente.

#### Tipología Dual de Magic Strings

Ambas familias comparten la misma anatomía `Label` más `InsertText`, pero se orientan en direcciones opuestas.

**Entrada y configuración.** El autor elige el valor y la función lo consume.

| Etiqueta | Valor insertado | Dónde se usa |
| :--- | :--- | :--- |
| `@vel: kmh` | `kmh` | `mate.convertir(100, "kmh", "ms")` |
| `@comp: ignora_caso` | `ignora_caso` | `cadena.es_igual(a, b, "ignora_caso")` |
| `@tabla: categoria` | `categoria` | `tabla.filtrar(t, "categoria")` |
| `@sqlEjecutar: crear_tabla` | `crear_tabla` | `sqlite.ejecutar(plantilla)` |

**Salida y retorno.** La función produce la clave y el autor la indexa.

| Etiqueta | Valor insertado | Dónde se usa |
| :--- | :--- | :--- |
| `@cholesky: L` | `L` | `res["L"]` de `algebraLineal.cholesky` |
| `@bernoulli: resultado` | `resultado` | `datos["resultado"]` |
| `@michaelisMenten: unidad` | `unidad` | `r["unidad"]` de `quimica.michaelisMenten` |

#### Protocolo de Sincronización (Cadena de Custodia)

Para evitar discrepancias entre el motor y los editores, cualquier modificación sigue este orden estricto de autoridad:

```text
  [1] MOTOR (Verdad Absoluta)
      • Atributos [LatinoExtension] y helper Respuesta.Crear() en C#.
      • Validación empírica obligatoria: dic.llaves().
                 ↓
  [2] MANUAL (Especificación Canónica)
      • docs/98_Referencia_Strings_Predefinidos.md.
                 ↓
  [3] TOOLING (Implementación en Editores)
      • VS Code: src/Latino.LanguageServer/MagicStringsCatalog.cs.
      • Sublime Text: Sublime-Text-support/sublime/snippets/*.sublime-snippet.
```

**Regla inquebrantable:** jamás se genera un snippet ni se añade un ítem a `MagicStringsCatalog.cs` sin verificar antes que la función y sus claves existen en el motor mediante ejecución real. La verificación se hace con `dic.llaves()` y no leyendo el código de la función, porque el conjunto de claves reales puede no coincidir con el que aparenta el código: `quimica.michaelisMenten` devuelve siempre `resultado`, `unidad`, `formula`, `incognita`, `saturacion_pct` y `regimen`, sea cual sea la incógnita despejada, y ese detalle no se deduce leyendo la función.

El orden importa porque en una tarea de documentación el motor no se modifica. Si un snippet y el motor discrepan, el snippet es el que está mal: el manual es la autoridad y el motor es la verdad.

### HoverHandler - Documentación Contextual

```csharp
public Task<Hover?> Handle(HoverParams request, CancellationToken ct)
{
    string word = GetWordAtPosition(code, request.Position); // "mate.sen"
    
    // 1. Buscar en plugins primero
    var pluginFunc = _pluginManager.FindFunction(word);
    if (pluginFunc != null)
        return CreateHover(pluginFunc.Help, word, pluginFunc.Args);
    
    // 2. Navegar en librería estándar (drill-down)
    var parts = word.Split('.'); // ["mate", "sen"]
    var currentObj = _interpreter.Globals.GetAt(parts[0]);
    
    for (int i = 1; i < parts.Length; i++)
    {
        if (currentObj is LatinoDictionary dict)
        {
            currentObj = dict.Pairs[new LatinoString(parts[i])];
        }
    }
    
    if (currentObj is NativeFunction nf)
    {
        return CreateHover(nf.Description, word, nf.ParameterNames);
    }
    
    // 3. Keywords estáticas
    if (_keywordDocs.TryGetValue(word, out string doc))
        return CreateHover(doc);
}
```

### PluginManager - Carga Dinámica de DLLs

```csharp
public void ScanAndLoadPlugins(DocumentUri documentUri, string sourceCode)
{
    // Regex: incluir("lapsos") o incluir('tiempo.dll')
    var pattern = @"incluir\s*\(\s*[""']([^""']+)[""']\s*\)";
    var matches = Regex.Matches(sourceCode, pattern);

    foreach (Match match in matches)
    {
        string rawName = match.Groups[1].Value;
        if (rawName.EndsWith(".lat")) continue; // Solo DLLs
        
        string dllPath = ResolveDllPath(documentUri, rawName);
        if (File.Exists(dllPath)) LoadPlugin(dllPath);
    }
}

private void LoadPlugin(string dllPath)
{
    var assembly = Assembly.LoadFrom(dllPath);
    
    foreach (var type in assembly.GetTypes())
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = method.GetCustomAttribute<LatinoExtensionAttribute>();
            if (attr != null)
            {
                var funcMetadata = new FunctionMetadata {
                    Name = attr.FunctionName,    // "lapsos.ahora"
                    Help = attr.Help,             // "Obtiene fecha actual"
                    Args = attr.Args              // "formato"
                };
                
                // Clasificar: ¿Global o Módulo?
                if (funcMetadata.Name.Contains('.'))
                {
                    var parts = funcMetadata.Name.Split('.', 2);
                    _modules[parts[0]].Methods[parts[1]] = funcMetadata;
                }
                else
                {
                    _globalFunctions[funcMetadata.Name] = funcMetadata;
                }
            }
        }
    }
}
```

## Principios de Diseño Clave

### 1. Cerebro Único

* **Parser**, **Interpreter** y **LSP** usan el **mismo** `Latino.Core.dll`
* Garantiza paridad absoluta entre intérprete y editor

### 2. Blindaje Total

* **I/O:** Excepciones de disco → `RuntimeError`
* **Regex:** Timeout de 2s para evitar ReDoS
* **Stack:** Límite configurable de recursión
* **Casting:** Helpers `Get()` validan tipos antes de convertir

### 3. Inmutabilidad de StdLib

```csharp
mateObj.MakeReadOnly(); // Impide mate.sen = "hacked"
```

### 4. Extensibilidad Sin Recompilación

* Plugins se cargan con Reflexión
* `[LatinoExtension]` inyecta metadatos al LSP automáticamente
* Namespaces automáticos: `lapsos.ahora` crea objeto `lapsos` en memoria

### 5. Núcleo Minimalista, Ecosistema Extensible

* `Latino.Core.dll` se mantiene deliberadamente pequeño y enfocado.
* Se prefiere la modularidad de plugins oficiales sobre la sobrecarga de la biblioteca estándar.
* Toda nueva característica debe justificar su presencia en el Core superando el Test de los 4 Filtros de Oro.

## Limitaciones Conocidas

1. **No hay compilación a bytecode** (intérprete puro)
2. **Goto solo funciona dentro del mismo bloque** (no saltos inter-función)
3. **Sin optimización de tail-call** (recursión limitada a MaxDepth)
4. **Herencia simple** (no múltiple)

## Checklist de Contribución

Antes de enviar un PR:

* [ ] **Test de los 4 Filtros de Oro:** Si el PR añade código a `Latino.Core`, justifica por qué no puede ser un plugin
* [ ] Nuevas funciones tienen **documentación** (`WithDocs()`)
* [ ] Excepciones .NET se convierten a `RuntimeError`
* [ ] Tests cubren casos límite (índices negativos, unicode, etc.)
* [ ] Lexer actualizado si añades palabras reservadas
* [ ] LSP actualizado si añades construcciones sintácticas
* [ ] **Magic Strings:** Si la función devuelve un diccionario estructurado o consume opciones fijas en cadena, registra el par `Label`/`InsertText` en `98_Referencia_Strings_Predefinidos.md` y sincroniza `MagicStringsCatalog.cs` (VS Code) o los snippets (Sublime Text)

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-16
