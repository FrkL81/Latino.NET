# Registro de Decisiones de Arquitectura (ADR) — Latino.NET

**Última actualización:** Octubre de 2026  
**Estado general:** Activo y normativo  
**Audiencia:** Mantenedores, desarrolladores del núcleo, agentes de IA y colaboradores  

---

## 📑 Índice Cronológico de Decisiones

| ID | Título de la Decisión | Fase / Naturaleza | Estado | Fecha |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-001** | Intérprete Tree-Walker con Delegación de Fuerza Bruta en .NET 10 | Fase 1: Génesis y Runtime | **Aceptado** | Agosto 2026 |
| **ADR-002** | Filosofía de Seguridad Defensiva (*Fail-Safe*) y Protección de Pila | Fase 1: Génesis y Runtime | **Aceptado** | Agosto 2026 |
| **ADR-003** | Inmutabilidad de Bibliotecas Estándar y Constantes | Fase 1: Génesis y Runtime | **Aceptado** | Agosto 2026 |
| **ADR-004** | Codificación Universal UTF-8 y Manipulación por Grafemas | Fase 1: Génesis y Runtime | **Aceptado** | Agosto 2026 |
| **ADR-005** | Identidad Léxica: Diacríticos (`ý`/`ó`), Booleanos y Compositor `ç` | Fase 1: Génesis y Runtime | **Aceptado** | Agosto 2026 |
| **ADR-006** | Composición Funcional: Operador Pipeline (`\|>`) con Aplicación Parcial | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-007** | Sustitución Ontológica de Herencia: `hereda` $\rightarrow$ `es` | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-008** | Convención Canónica de Nomenclatura en camelCase | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-009** | Preservación del Modelo de Ámbitos Léxicos frente a la "Trampa de Python" | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-010** | Semántica de Acceso a Miembros: Permisividad en POO vs. Rigor en Diccionarios | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-011** | Semántica de Tipos e Introspección Reflexiva en POO | Fase 2: Sintaxis y Paradigmas | **Aceptado** | Septiembre 2026 |
| **ADR-012** | Política de Inclusión: Test de los 4 Filtros de Oro (Core vs. Plugins) | Fase 3: Ecosistema y Tooling | **Aceptado** | Septiembre 2026 |
| **ADR-013** | Arquitectura Híbrida del LSP y Principio de "Un Solo Cerebro" | Fase 3: Ecosistema y Tooling | **Aceptado** | Septiembre 2026 |
| **ADR-014** | Ecosistema Gráfico Satélite Unificado en Avalonia UI 11+ | Fase 3: Ecosistema y Tooling | **Aceptado** | Septiembre 2026 |
| **ADR-015** | Arquitectura de Cero Copias (*Zero-Copy*) con `tramo()` | Fase 4: Optimización y Futuro | **Aceptado** | Octubre 2026 |
| **ADR-016** | Estrategia Bifásica para Cálculo Diferencial y Científico | Fase 4: Optimización y Futuro | **Aceptado** | Octubre 2026 |
| **ADR-017** | Estrategia de Compilación Nativa (AOT vía Transpilación a C# con Roslyn) | Fase 4: Optimización y Futuro | **Aceptado** | Octubre 2026 |

---

## 🏛️ FASE 1: Génesis y Fundamentos del Motor (Agosto 2026)

### ADR-001: Intérprete Tree-Walker con Delegación de Fuerza Bruta en .NET 10

#### Contexto

Al fundar Latino.NET, se evaluó si el motor debía compilar directamente a código intermedio de .NET (`Reflection.Emit`), implementar una máquina virtual de bytecode basada en pila o mantener un árbol de ejecución directo (*Tree-Walker*).

#### Decisión

Se ratifica la arquitectura de **Tree-Walker puro** sobre el AST (`Interpreter.cs`). La velocidad del lenguaje se alcanza delegando las operaciones computacionalmente intensivas (matemáticas, ordenamiento, compresión, hashing, regex) a la **Base Class Library (BCL) de .NET 10**, ejecutadas en código nativo optimizado por el JIT.

#### Consecuencias

- **Positivas:** Simplicidad extrema para extender el motor, facilidad de depuración paso a paso, portabilidad completa y capacidad inmediata de incrustar el motor en cualquier aplicación de C# o WebAssembly.
- **Compromisos:** Los bucles manuales interpretados nodo a nodo son más lentos; el Mantra establece la obligación de delegar en funciones nativas (`para`, `lista.ordenar`, `lista.sumar`).

---

### ADR-002: Filosofía de Seguridad Defensiva (*Fail-Safe*) y Protección de Pila

#### Contexto

Un lenguaje interpretado moderno no debe permitir que errores de usuario (divisiones por cero, recursiones infinitas o tipos incompatibles) cierren abruptamente el proceso anfitrión con excepciones irrecuperables de .NET (`StackOverflowException` o caídas del host).

#### Decisión

1. **Excepciones Controladas:** El runtime intercepta fallas del sistema o de tipado y las encapsula en instancias de `RuntimeError(token, mensaje)`, 100% interceptables mediante bloques `intentar ... atrapar`.
2. **Guardián de Pila:** Se implementa un contador manual de profundidad de llamada (`CurrentDepth`) con un límite estricto (`MaxDepth = 100`, configurable en caliente con `sis.config("max_stack", n)`).
3. **Protección Anti-ReDoS:** Toda evaluación de expresiones regulares impone un timeout defensivo de 2 segundos para neutralizar congelamientos por retroceso catastrófico.

#### Consecuencias

- **Positivas:** Estabilidad de grado industrial; el motor puede ejecutar scripts no confiables en servidores sin riesgo de tumbar el proceso.

---

### ADR-003: Inmutabilidad de Bibliotecas Estándar y Constantes

#### Contexto

En lenguajes excesivamente permisivos como JavaScript o Python clásico, un script puede reasignar accidentalmente miembros nucleares (`mate.sen = nulo`), provocando fallos impredecibles en módulos ajenos.

#### Decisión

1. **Módulos del Sistema:** Los diccionarios que alojan las bibliotecas nativas (`mate`, `cadena`, `sis`, `archivo`, `lista`, etc.) se sellan antes de la ejecución mediante el método `MakeReadOnly()`. Cualquier intento de reasignar miembros arroja un `RuntimeError` protector.
2. **Constantes de Usuario:** Toda variable cuyo nombre esté escrito en mayúsculas sostenidas (`MAYUSCULAS`) queda protegida contra reasignación en el entorno (`LatinoEnvironment`).

#### Consecuencias

- **Positivas:** Integridad estructural blindada; los scripts de terceros o plugins no pueden corromper el entorno estándar.

---

### ADR-004: Codificación Universal UTF-8 y Manipulación por Grafemas

#### Contexto

El soporte nativo del español (tildes, `ñ`) y de interfaces modernas de consola (emojis) colapsa si el motor opera con conteo tradicional de bytes ASCII o unidades de 16 bits de UTF-16, partiendo caracteres complejos por la mitad.

#### Decisión

1. **Archivos:** Todos los archivos de código fuente y plantillas generadas por la extensión deben codificarse obligatoriamente en **UTF-8 con BOM (`\uFEFF`)** para blindar la lectura en terminales Windows heredadas.
2. **Manipulación de Texto:** Funciones como `cadena.longitud()`, `cadena.subcadena()`, `cadena.invertir()` y `cadena.centrar()` no operan sobre `char` ni sobre bytes; delegan en `System.Globalization.StringInfo` para computar **grupos de grafemas visualmente percibidos** (*text elements*). Un emoji compuesto por secuencias ZWJ (ej: 👨‍👩‍👧‍👦) cuenta siempre como longitud `1`.

#### Consecuencias

- **Positivas:** Inversión y recorte de cadenas inmunes a la corrupción visual de caracteres compuestos y emojis.

---

### ADR-005: Identidad Léxica: Diacríticos (`ý`/`ó`), Booleanos y Compositor `ç`

#### Contexto

En Latino clásico, las conjunciones y disyunciones utilizaban las letras `y` y `o`. Esto secuestraba identificadores matemáticos esenciales (como la coordenada cartesiana `y`). Asimismo, la interpolación de texto tradicional era verbosa y propensa a concatenaciones infinitas (`..`).

#### Decisión

1. **Lógica Diacrítica:** Los operadores lógicos en español son formalmente **`ý`** (AND) y **`ó`** (OR), liberando las letras `y` y `o` como identificadores ordinarios sin colisión léxica.
2. **Booleanos:** La salida canónica de los valores lógicos en consola y cadenas es **`cierto`** y **`falso`** (aceptando de forma laxa `verdadero` y el término culto `espurio` como alias de entrada).
3. **Interpolación Estructural:** Se consagra el carácter **`ç("|var|")`** como el compositor oficial de cadenas para navegación directa de variables y propiedades.

#### Consecuencias

- **Positivas:** Identidad hispana única, código matemático limpio (`x = 10; y = 20`) y gramática no ambigua.

---

## ⚙️ FASE 2: Sintaxis, Paradigmas y Modelo de Ejecución (Septiembre 2026)

### ADR-006: Composición Funcional: Operador Pipeline (`|>`) con Aplicación Parcial

#### Contexto

El anidamiento excesivo de llamadas a funciones (`lista.promedio(lista.filtrar(datos, ...))`) dificultaba la lectura de izquierda a derecha de las transformaciones de datos y fomentaba el uso de bucles imperativos con efectos secundarios.

#### Decisión

Se incorpora el operador pipeline **`|>`** con semántica de aplicación parcial nativa inspirada en Elixir:

1. Si el lado derecho es una función simple, el valor de la izquierda se inyecta como su único argumento: `datos |> mate.abs`.
2. Si el lado derecho es una llamada con parámetros, el valor de la izquierda se inyecta obligatoriamente como el **primer argumento**:

   ```latino
   aprobados = notas |> lista.filtrar(fun(n) ret n >= 70 fin)
   ```

#### Consecuencias

- **Positivas:** Código declarativo, lineal, sin variables intermedias mutables y con un incremento de velocidad de ejecución medido (~11% más veloz que llamadas anidadas).

---

### ADR-007: Sustitución Ontológica de Herencia: `hereda` $\rightarrow$ `es`

#### Contexto

La palabra clave histórica `hereda` representaba un calco forzado del inglés (*inherits*). En la modelación orientada a objetos en español, la relación canónica de clasificación de subclases es **"ES-UN" (*IS-A*)**: un Perro *es* un Animal.

#### Decisión

Se reemplaza formalmente la palabra clave de herencia:

```latino
# Sintaxis anterior (descartada):
clase Perro hereda Animal

# Sintaxis canónica oficial:
clase Perro es Animal
```

La palabra clave `hereda` queda removida del motor; el concepto se sigue denominando **Herencia**.

#### Consecuencias

- **Positivas:** Expresividad natural sin traducciones artificiales. Rompe de forma intencional con scripts heredados de Latino clásico que usaran `hereda`.

---

### ADR-008: Convención Canónica de Nomenclatura en camelCase

#### Contexto

Latino clásico arrastraba inconsistencias mezclando `snake_case` con llamadas en minúsculas pegadas. Se requería una regla de estilo firme para la evolución del lenguaje sin romper el pasado.

#### Decisión

Toda nueva función de la biblioteca estándar, método de extensión o plugin oficial se desarrollará en **`camelCase`** estricto (`leerJson`, `escribirCsv`, `desdeTicks`, `valorPropioDominante`). Las funciones heredadas en `snake_case` (`es_numero`, `termina_con`) se conservan exclusivamente como alias de compatibilidad histórica, pero se consideran secundarias en la documentación.

#### Consecuencias

- **Positivas:** APIs limpias, modernas y predecibles.

---

### ADR-009: Preservación del Modelo de Ámbitos Léxicos frente a la "Trampa de Python"

#### Contexto

Se evaluó si cualquier asignación `x = valor` dentro de una función debía ser local por defecto sin exigir la palabra clave `local`.

#### Decisión

**Se rechaza la asignación local forzada.** Latino.NET conserva su modelo léxico clásico:

1. Una variable declarada con `local` es estrictamente privada a su bloque.
2. Una asignación ordinaria (`x = valor`) busca hacia arriba en la cadena de entornos superiores (`_enclosing`) antes de crear una variable local.
3. El riesgo de mutar variables externas por accidente se mitiga en el tooling mediante advertencias de sombreado (*Shadowing Warnings*) en el linter/LSP, sin alterar la semántica de ejecución.

#### Consecuencias

- **Positivas:** Se preserva la programación funcional y las clausuras léxicas (*closures*) naturales. Se evita la necesidad de introducir palabras parche como `nonlocal` de Python.

---

### ADR-010: Semántica de Acceso a Miembros: Permisividad en POO vs. Rigor en Diccionarios

#### Contexto

Se detectó una asimetría deliberada en el motor (demostrada empíricamente en `test_90`): al acceder a un campo inexistente en una instancia (`objeto.campoInexistente`), el motor retorna **`nulo` en silencio**. Por el contrario, acceder a una clave inexistente en un diccionario (`dicc.claveInexistente` o `dicc["clave"]`) arroja **`RuntimeError`**. Se discutió si unificar ambos comportamientos.

#### Decisión

**Se ratifica la asimetría como decisión de diseño arquitectónico:**

1. **Instancias de Clase (Mundo Abierto / POO Dinámica):** Las instancias representan entidades cuyo estado puede ser opcional o disperso. La lectura de campos no inicializados retorna `LatinoNull.Instance` permitiendo el uso natural de operadores de fusión (`objeto.campo ?? valorPorDefecto`).
2. **Métodos de Instancia (Invocación):** Si el miembro inexistente se intenta invocar como función (`objeto.metodoInexistente()`), el motor detiene la ejecución inmediatamente con `RuntimeError: No es una función`.
3. **Diccionarios (Estructuras de Datos Estrictas):** Los diccionarios modelan tablas hash indexadas. Acceder a una clave ausente se considera un error de consulta. Para lecturas seguras con respaldo se provee la función explícita `dic.obtener(d, clave, defecto)`.

#### Consecuencias

- **Positivas:** Balance óptimo entre la flexibilidad de prototipado en clases y la robustez contra errores tipográficos en colecciones de datos.

---

### ADR-011: Semántica de Tipos e Introspección Reflexiva en POO

#### Contexto

La función `tipo()` devolvía `"desconocido"` para definiciones de clases y el nombre de la clase para instancias (ej. `"Perro"`), lo que creaba discordancias documentales.

#### Decisión

1. **Clases:** `LatinoClass.cs` sobrescribe `TypeName` para retornar obligatoriamente `"clase"`.
2. **Instancias:** Se ratifica que `tipo(instancia)` retorna el nombre de su clase generadora (`"Perro"`), favoreciendo el modelado dinámico por sobre una etiqueta genérica `"instancia"`.
3. **Jerarquía:** Para verificar relaciones de herencia polimórfica sin recurrir a comparaciones de cadenas frágiles, se crea la función nativa **`esInstancia(objeto, clase)`**.

#### Consecuencias

- **Positivas:** Introspección coherente y comprobación segura de jerarquías de herencia Top-Down.

---

## 🔌 FASE 3: Ecosistema, Modularidad y Herramientas Satélite (Septiembre 2026)

### ADR-012: Política de Inclusión: Test de los 4 Filtros de Oro (Core vs. Plugins)

#### Contexto

Para evitar que el núcleo (`Latino.Core.dll`) sufra del "síndrome de baterías agotadas" volviéndose pesado, inestable y difícil de mantener, se requería una frontera estricta entre el motor y los módulos externos.

#### Decisión

Ninguna característica se añade a `Latino.Core` a menos que supere rigurosamente los **4 Filtros de Oro**:

1. **Uso Transversal (80/20):** ¿La necesita el $\ge 80\%$ de cualquier tipo de usuario o script?
2. **Cohesión Semántica:** ¿El motor, el GC o el sistema de tipos dependen de ella para operar?
3. **Huella de Dependencias:** ¿Exige paquetes externos de NuGet o llamadas nativas complejas al SO?
4. **Portabilidad WASM:** ¿Afecta negativamente el arranque frío o la compilación a WebAssembly en el navegador?

Si la funcionalidad falla los filtros 1, 3 o 4, **debe implementarse como un plugin `.dll` independiente**.

#### Consecuencias

- **Positivas:** El Core permanece ligero, ultra rápido y listo para compilar a WebAssembly sin dependencias externas pesadas.

---

### ADR-013: Arquitectura Híbrida del LSP y Principio de "Un Solo Cerebro"

#### Contexto

Muchos entornos de desarrollo duplican la lógica del lenguaje: escriben un parser en C# para el motor y otro parser en TypeScript para el editor de código, generando desincronizaciones donde el editor marca errores que el motor sí acepta.

#### Decisión

Se consagra la arquitectura del principio **"Un Solo Cerebro"**:

1. El cliente de VS Code (`extension.ts`) es deliberadamente delgado y no analiza código.
2. El servidor de lenguaje (`Latino.LanguageServer`) está escrito en C# sobre .NET 10 y **referencia directamente al ensamblado del motor (`Latino.Core.dll`)**.
3. El LSP valida código instanciando la misma clase `Parser` del motor y autocompleta inspeccionando la memoria viva de un `Interpreter` singleton.
4. Los Magic Strings dentro de cadenas se gobiernan por la cadena de custodia: **Motor (`dic.llaves()`) $\rightarrow$ Manual (`98_`) $\rightarrow$ Tooling (`MagicStringsCatalog.cs` / Sublime)**.

#### Consecuencias

- **Positivas:** Paridad del 100% entre lo que el editor subraya y lo que el ejecutable corre. Las mejoras en el motor benefician de inmediato al editor.

---

### ADR-014: Ecosistema Gráfico Satélite Unificado en Avalonia UI 11+

#### Contexto

Se requería definir la tecnología para las herramientas visuales oficiales de desarrollo (`Latino Plugin Studio` y la interfaz del compilador AOT). Se evaluaron Windows Presentation Foundation (WPF) y Avalonia UI.

#### Decisión

**Se descarta definitivamente WPF.** Todas las aplicaciones visuales satélite del ecosistema se desarrollarán exclusivamente sobre **Avalonia UI 11+ sobre .NET 10**.

#### Consecuencias

- **Positivas:** Paridad multiplataforma absoluta y nativa en Windows, Linux y macOS sin reescribir código. Aceleración por GPU y renderizado idéntico de temas oscuros/claros.
- **Compromisos:** Se descarta el ecosistema de controles exclusivos de Windows, obligando a ceñirse a bibliotecas compatibles con XAML de Avalonia.

---

## ⚡ FASE 4: Optimización, Computación Científica y Horizonte v2.0 (Octubre 2026)

### ADR-015: Arquitectura de Cero Copias (*Zero-Copy*) con `tramo()`

#### Contexto

El procesamiento intensivo de subcadenas o sublistas en algoritmos masivos generaba presión crítica sobre el Garbage Collector de .NET al clonar y alocar memoria en cada paso.

#### Decisión

Se descarta el uso directo de `Span<T>` en el lenguaje debido a sus restricciones de ciclo de vida (no puede alojarse en el Heap ni ser capturado por closures de un lenguaje dinámico). Se adopta una abstracción de ventana virtual basada en `ArraySegment<T>` y `ReadOnlyMemory<T>`, expuesta idiomáticamente bajo la función global **`tramo(coleccion, inicio, [longitud])`** y encapsulada en el tipo `LatinoSegment`.

#### Consecuencias

- **Positivas:** Operaciones de corte y particionado en tiempo $O(1)$ y memoria $O(1)$. Las bibliotecas nativas pueden operar sobre segmentos sin clonar colecciones.

---

### ADR-016: Estrategia Bifásica para Cálculo Diferencial y Científico

#### Contexto

El usuario y el entorno de ingeniería requerían cálculo de derivadas exactas a partir de lambdas. Se discutió si manipular código fuente como texto mediante expresiones regulares, usar Álgebra Computacional (CAS) o Diferenciación Automática.

#### Decisión

**Se prohíbe el uso de Regex para cálculo simbólico** (el lenguaje de expresiones matemáticas anidadas no es regular). Se adopta una estrategia en dos etapas:

1. **Fase 1 (Runtime):** Diferenciación Automática hacia adelante utilizando **Números Duales** (`LatinoDualNumber`), permitiendo obtener derivadas exactas en un único pase de evaluación.
2. **Fase 2 (Simbólica):** Motor analítico formal sobre el AST (`Latino.Plugin.Cas`) implementando el patrón *Visitor* para emitir nuevas lambdas derivadas simplificadas algebraicamente.

#### Consecuencias

- **Positivas:** Cero errores de truncamiento $h$, velocidad máxima de ejecución y preservación de la estructura matemática formal del motor.

---

### ADR-017: Estrategia de Compilación Nativa (AOT vía Transpilación a C# con Roslyn)

#### Contexto

Para alcanzar velocidades de ejecución industriales de cara a la v2.0, se analizó si emitir código de máquina x64/ARM desde cero, generar IL dinámico con `Reflection.Emit` o transpilar a un lenguaje intermedio.

#### Decisión

Se aprueba la estrategia de **Transpilación AST $\rightarrow$ C# $\rightarrow$ Roslyn Compiler $\rightarrow$ NativeAOT de .NET 10**:

1. El motor de Latino.NET recorrerá el AST y emitirá código fuente en C# idiomático.
2. Se invocará la API de Microsoft **Roslyn** (`Microsoft.CodeAnalysis.CSharp`) en memoria para compilar el código intermedio.
3. El pipeline de publicación de .NET 10 generará el ejecutable binario de máquina nativo (*Native AOT*).

#### Consecuencias

- **Positivas:** Aprovecha décadas de optimizaciones del compilador de Microsoft (vectorización SIMD, inlining, escape analysis) con una fracción del esfuerzo que requeriría construir un backend de máquina propio.
- **Compromisos:** Requiere modelar un sistema robusto de inferencia de tipos para minimizar el empaquetado (*boxing*) excesivo al traducir tipos dinámicos a C#.

---

### 🛡️ Reglas de Gobernanza para Nuevos ADRs

1. **Inmutabilidad:** Una vez que un ADR pasa a estado **Aceptado**, no se reescribe su motivación original. Si una decisión técnica cambia en el futuro, se redacta un nuevo ADR que declare explícitamente: *"Este ADR reemplaza a ADR-XXX"*.
2. **Numeración Secuencial:** Toda nueva decisión debe registrarse con identificador incremental `ADR-018`, `ADR-019`, etc.
3. **Formato Obligatorio:** Todo registro debe incluir Título, Fecha, Estado, Contexto, Decisión y Consecuencias.

---
