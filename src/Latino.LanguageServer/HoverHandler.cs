using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Latino.Core.Runtime;
using Latino.Core.StdLib;
using Latino.Core.Types;

namespace Latino.LanguageServer;

public class HoverHandler : IHoverHandler
{
    private readonly Interpreter _interpreter;
    private readonly PluginManager _pluginManager;
    
private static readonly Dictionary<string, string> _keywordDocs = new()
    {
        // --- CONTROL DE FLUJO ---
        { "si", "**Condicional SI**\n\nEjecuta un bloque de código si la condición es verdadera.\n\n`si (condicion) ... fin`" },
        { "sino", "**Condicional SINO**\n\nEjecuta un bloque alternativo si la condición del 'si' falló." },
        { "osi", "**Condicional OSI (O Si)**\n\nAgrega una condición extra si la anterior falló." },
        
        // --- BUCLES ---
        { "mientras", "**Bucle MIENTRAS**\n\nRepite el código mientras la condición sea verdadera.\n\n`mientras (condicion) ... fin`" },
        { "desde", "**Bucle DESDE**\n\nIterador numérico estilo C.\n\n`desde (i=0; i<10; i++) ... fin`" },
        { "para", "**Bucle PARA**\n\nItera sobre los elementos de una lista o colección.\n\n`para elemento en lista ... fin`" },
        { "en", "**Operador de Iteración**\n\nSe utiliza en el bucle `para` para separar la variable de la colección.\n\n`para x en lista`" },
        { "repetir", "**Bucle REPETIR**\n\nEjecuta el código al menos una vez hasta que la condición se cumpla.\n\n`repetir ... hasta (condicion)`" },
        { "hasta", "**Condición de Salida**\n\nDefine el final de un bucle `repetir`. Se detiene cuando la condición es verdadera." },
        
        // --- SALTOS ---
        { "romper", "**ROMPER**\n\nDetiene inmediatamente la ejecución del bucle actual (`mientras`, `desde`, `para` y `repetir`)." },
        { "continuar", "**CONTINUAR**\n\nSalta el resto del código actual y pasa a la siguiente iteración del bucle." },
        { "ir", "**Salto Incondicional**\n\nSalta la ejecución a una etiqueta definida.\n\n`ir etiqueta`" },
        
        // --- SELECCIÓN MÚLTIPLE ---
        { "elegir", "**Estructura ELEGIR**\n\nEvalúa una expresión y ejecuta el caso coincidente.\n\n`elegir (valor) ... fin`" },
        { "caso", "**Caso de Selección**\n\nBloque que se ejecuta si el valor coincide.\n\n`caso valor:`" },
        { "otro", "**Caso por Defecto**\n\nSe ejecuta si ningún `caso` coincidió." },
        { "defecto", "**Caso por Defecto**\n\nAlias de `otro`." },

        // --- FUNCIONES ---
        { "funcion", "**Definición de FUNCIÓN**\n\nBloque de código reutilizable.\n\n`funcion nombre(params) ... fin`" },
        { "fun", "**Definición de FUNCIÓN** (Alias)\n\nForma abreviada de `funcion`." },
        { "retornar", "**RETORNAR**\n\nDevuelve un valor desde una función y termina su ejecución." },
        { "retorno", "**RETORNAR** (Alias)\n\nSinónimo de `retornar`." },
        { "ret", "**RETORNAR** (Alias)\n\nForma abreviada de `retornar`." },

        // --- POO ---
        { "clase", "**Definición de CLASE**\n\nPlantilla para crear objetos.\n\n`clase Nombre ... fin`" },
        { "es", "**Herencia (Es-Un)**\n\nIndica la clase padre de la que hereda (relación IS-A).\n\n`clase hijo es Padre`" },
        { "constructor", "**Constructor**\n\nMétodo especial que se ejecuta automáticamente al crear una instancia.\n\n`constructor(params)`" },
        { "crear", "**Instanciación**\n\nCrea una nueva instancia de una clase.\n\n`obj = crear Clase()`" },
        { "nueva", "**Instanciación**\n\nAlias de `crear` (Para Concordancia De Género Femenino)." },
        { "nuevo", "**Instanciación**\n\nAlias de `crear` (Para Concordancia De Género Masculino)." },
        { "mi", "**Instancia Actual**\n\nReferencia al objeto actual (this) para acceder a sus propiedades." },
        { "este", "**Instancia Actual**\n\nAlias de `mi` (Para Concordancia De Género Neutro/Masculino)." },
        { "esta", "**Instancia Actual**\n\nAlias de `mi` (Para Concordancia De Género Femenino)." },
        { "estatico", "**Modificador Estático**\n\n(Reservado, no implementado aún) Indica pertenencia a la clase, no a la instancia." },

        // --- ALCANCE Y VALORES ---
        { "local", "**Alcance LOCAL**\n\nDefine una variable que solo existe en el bloque actual, protegiendo las externas." },
        { "global", "**Alcance GLOBAL**\n\nDefine o modifica una variable en la raíz del programa." },
        { "nulo", "**Valor NULO**\n\nRepresenta la ausencia de valor o referencia vacía." },
        { "cierto", "**Valor Lógico** (Principal)\n\nRepresenta la verdad (true). Es la forma canónica de salida." },
        { "verdadero", "**Valor Lógico** (Alias)\n\nAlias de compatibilidad para `cierto`. Aceptado como entrada." },
        { "falso", "**Valor Lógico** (Principal)\n\nRepresenta la falsedad (false). Es la forma canónica de salida." },
        { "espurio", "**Valor Lógico** (Alias)\n\nAlias culto para `falso`. Significa \"ilegítimo\" o \"adulterado\". Aceptado como entrada." },
        
        // --- OTROS ---
        { "intentar", "**Manejo de Errores**\n\nInicia un bloque de código propenso a fallos.\n\n`intentar ... atrapar (e) ... fin`" },
        { "atrapar", "**Captura de Error**\n\nRecibe el error si el bloque `intentar` falla.\n\n`atrapar (error)`" },
        { "fin", "**Fin de Bloque**\n\nCierra una estructura de control (si, mientras, funcion, clase, intentar)." }
    };

    public HoverHandler(PluginManager pluginManager, Interpreter interpreter)
    {
        _interpreter = interpreter; // Inyectado, ya viene con StdLib cargada
        _pluginManager = pluginManager;
    }

    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions { DocumentSelector = TextDocumentSelector.ForLanguage("latino") };
    }

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var code = BufferManager.GetBuffer(request.TextDocument.Uri);
        if (code == null) return Task.FromResult<Hover?>(null);

        // 1. Obtener la "palabra completa" (incluyendo puntos, ej: "mate.sen" o "lapsos.ahora")
        string word = GetWordAtPosition(code, request.Position);
        if (string.IsNullOrEmpty(word)) return Task.FromResult<Hover?>(null);

        // 2. NUEVO: Buscar primero en plugins
        var pluginFunc = _pluginManager.FindFunction(word);
        if (pluginFunc != null)
        {
            string formattedHelp = MarkdownHelper.FormatHelp(pluginFunc.Help);
            string markdown = $"{formattedHelp}\n\n";
            
            // CORRECCIÓN: Usar bloque de código latino para coloreado sintáctico
            if (!string.IsNullOrEmpty(pluginFunc.Args))
            {
                markdown += $"```latino\n{word}({pluginFunc.Args})\n```";
            }
            else
            {
                markdown += $"```latino\n{word}()\n```";
            }

            // Mejorar la presentación visual del tipo de retorno
            if (!string.IsNullOrEmpty(pluginFunc.Returns) && pluginFunc.Returns != "nulo")
            {
                markdown += $"\n**Retorna:** `{pluginFunc.Returns}`";
            }

            return Task.FromResult<Hover?>(new Hover {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent {
                        Kind = MarkupKind.Markdown,
                        Value = markdown
                    }
                )
            });
        }

        // 3. Resolver el objeto en la librería estándar (Soporte para anidamiento)
        LatinoObject? currentObj = null;
        
        var parts = word.Split('.');
        currentObj = _interpreter.Globals.GetAt(parts[0]);

        // Navegamos hacia abajo (ej: .sen)
        if (currentObj != null && parts.Length > 1)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                if (currentObj is LatinoDictionary dict)
                {
                    var keyToFind = parts[i];
                    var found = false;
                    
                    foreach(var pair in dict.Pairs)
                    {
                        if (pair.Key.ToString() == keyToFind)
                        {
                            currentObj = pair.Value;
                            found = true;
                            break;
                        }
                    }
                    if (!found) { currentObj = null; break; }
                }
                else
                {
                    currentObj = null;
                    break;
                }
            }
        }

        // 4. Generar documentación si es función nativa de la librería estándar
        if (currentObj is NativeFunction nf && !string.IsNullOrEmpty(nf.Description))
        {
            string formattedDesc = MarkdownHelper.FormatHelp(nf.Description);
            
            return Task.FromResult<Hover?>(new Hover {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent {
                        Kind = MarkupKind.Markdown,
                        Value = $"{formattedDesc}\n\n```latino\n{word}({nf.ParameterNames})\n```"
                    }
                )
            });
        }

        // 5. Búsqueda Estática (Keywords)
        if (_keywordDocs.TryGetValue(word, out string? doc) && doc != null)
        {
            return Task.FromResult<Hover?>(new Hover {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent { Kind = MarkupKind.Markdown, Value = doc }
                )
            });
        }
        // NUEVO: Fallback para capturar "mi" dentro de "mi.propiedad"
        else if (parts.Length > 1 && _keywordDocs.TryGetValue(parts[0], out string? docFallback) && docFallback != null)
        {
            return Task.FromResult<Hover?>(new Hover {
                Contents = new MarkedStringsOrMarkupContent(
                    new MarkupContent { Kind = MarkupKind.Markdown, Value = docFallback }
                )
            });
        }

        return Task.FromResult<Hover?>(null);
    }

    private string GetWordAtPosition(string text, Position position)
    {
        var lines = text.Split('\n');
        if (position.Line >= lines.Length) return "";
        
        var line = lines[position.Line];
        if (position.Character >= line.Length) return "";
        
        // Buscar hacia atrás desde la posición actual
        int start = position.Character;
        while (start > 0 && IsWordChar(line[start - 1]))
        {
            start--;
        }
        
        // Buscar hacia adelante desde la posición actual
        int end = position.Character;
        while (end < line.Length && IsWordChar(line[end]))
        {
            end++;
        }
        
        return line.Substring(start, end - start);
    }
    
    private bool IsWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.';
    }
}
