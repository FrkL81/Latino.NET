using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Latino.Core.Runtime;
using Latino.Core.StdLib;
using Latino.Core.Types;

namespace Latino.LanguageServer;

public class CompletionHandler : ICompletionHandler
{
    private readonly Interpreter _interpreter;
    private readonly PluginManager _pluginManager;

    public CompletionHandler(PluginManager pluginManager, Interpreter interpreter)
    {
        _interpreter = interpreter; // Inyectado, ya viene con StdLib cargada
        _pluginManager = pluginManager;
    }

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("latino"),
            TriggerCharacters = new Container<string>(".", "\"", "'", "@")
        };
    }

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var completions = new List<CompletionItem>();

        // --- NUEVO: DETECCIÓN DE CONTEXTO DE CADENA (Magic Strings) ---
        string? buffer = BufferManager.GetBuffer(request.TextDocument.Uri);
        if (buffer != null)
        {
            var lines = buffer.Split('\n');
            int lineIdx = request.Position.Line;
            if (lineIdx < lines.Length)
            {
                string lineText = lines[lineIdx];
                int charIdx = request.Position.Character;
                
                // Asegurar que el índice está dentro de los límites
                if (charIdx > lineText.Length) charIdx = lineText.Length;
                
                string textBeforeCursor = lineText.Substring(0, charIdx);
                
                // Heurística simple: ¿Estamos dentro de comillas en esta línea?
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

                    var range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                        new Position(lineIdx, start),
                        new Position(lineIdx, charIdx)
                    );

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
            }
        }

        // 1. Palabras Clave (Keywords del Lexer)
        foreach (var kw in Latino.Core.Analysis.Lexer.Keywords.Keys)
        {
            completions.Add(new CompletionItem { Label = kw, Kind = CompletionItemKind.Keyword });
        }

        // 2. INTROSPECCIÓN DINÁMICA DEL ENTORNO (Librerías estándar)
        foreach(var key in _interpreter.Globals.GetKeys())
        {
             var obj = _interpreter.Globals.GetAt(key);
             
             if (obj is LatinoDictionary lib)
             {
                 // ES UNA LIBRERÍA (ej: "mate", "sis")
                 completions.Add(new CompletionItem { 
                     Label = key, 
                     Kind = CompletionItemKind.Module,
                     Detail = "Librería Estándar"
                 });

                 // Agregar todos sus métodos (ej: "mate.sen")
                 foreach(var methodKey in lib.Pairs.Keys)
                 {
                     string methodName = methodKey.ToString();
                     var methodObj = lib.Pairs[methodKey];
                     string fullLabel = $"{key}.{methodName}";
                     
                     CompletionItem item;

                     if (methodObj is NativeFunction nf)
                     {
                         string? documentation = !string.IsNullOrEmpty(nf.Description) ? nf.Description : null;
                         string detail = $"Método de {key}";
                         string insertText = fullLabel;
                         InsertTextFormat insertFormat = InsertTextFormat.PlainText;

                         if (!string.IsNullOrEmpty(nf.ParameterNames))
                         {
                             insertFormat = InsertTextFormat.Snippet;
                             var pList = nf.ParameterNames.Split(',').Select(p => p.Trim()).ToList();
                             var snippetArgs = string.Join(", ", pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                             insertText = $"{fullLabel}({snippetArgs})";
                             detail += $"({nf.ParameterNames})";
                         }

                         item = new CompletionItem { 
                            Label = fullLabel, 
                            Kind = CompletionItemKind.Method,
                            Detail = detail,
                            Documentation = documentation != null ? new StringOrMarkupContent(new MarkupContent { Kind = MarkupKind.Markdown, Value = MarkdownHelper.FormatHelp(documentation) }) : null,
                            InsertText = insertText,
                            InsertTextFormat = insertFormat
                         };
                     }
                     else
                     {
                         item = new CompletionItem { 
                            Label = fullLabel, 
                            Kind = CompletionItemKind.Method,
                            Detail = $"Método de {key}",
                            InsertText = fullLabel
                         };
                     }

                     completions.Add(item);
                 }
             }
             else
             {
                 // ES UNA FUNCIÓN GLOBAL (ej: "escribir", "incluir")
                 CompletionItem item;

                 if (obj is NativeFunction nf)
                 {
                    string? documentation = !string.IsNullOrEmpty(nf.Description) ? nf.Description : null;
                     string detail = "Función Global";
                     string insertText = key;
                     InsertTextFormat insertFormat = InsertTextFormat.PlainText;

                     if (!string.IsNullOrEmpty(nf.ParameterNames))
                     {
                         insertFormat = InsertTextFormat.Snippet;
                         var pList = nf.ParameterNames.Split(',').Select(p => p.Trim()).ToList();
                         var snippetArgs = string.Join(", ", pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                         insertText = $"{key}({snippetArgs})";
                         detail += $"({nf.ParameterNames})";
                     }

                     item = new CompletionItem { 
                         Label = key, 
                         Kind = CompletionItemKind.Function,
                         Detail = detail,
                         Documentation = documentation != null ? new StringOrMarkupContent(new MarkupContent { Kind = MarkupKind.Markdown, Value = MarkdownHelper.FormatHelp(documentation) }) : null,
                         InsertText = insertText,
                         InsertTextFormat = insertFormat
                     };
                 }
                 else
                 {
                     item = new CompletionItem { 
                         Label = key, 
                         Kind = CompletionItemKind.Function,
                         Detail = "Función Global",
                         InsertText = key
                     };
                 }

                 completions.Add(item);
             }
        }

        // 3. NUEVO: AGREGAR FUNCIONES DE PLUGINS
        // 3.1 Funciones globales de plugins
        foreach (var func in _pluginManager.GetGlobalFunctions())
        {
            string detail = "Función de Plugin";
            string insertText = func.Name;
            InsertTextFormat insertFormat = InsertTextFormat.PlainText;

            if (!string.IsNullOrEmpty(func.Args))
            {
                insertFormat = InsertTextFormat.Snippet;
                var pList = func.Args.Split(',').Select(p => p.Trim()).ToList();
                var snippetArgs = string.Join(", ", pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                insertText = $"{func.Name}({snippetArgs})";
                detail += $"({func.Args})";
            }

            completions.Add(new CompletionItem
            {
                Label = func.Name,
                Kind = CompletionItemKind.Function,
                Detail = detail,
                Documentation = !string.IsNullOrEmpty(func.Help) ? new StringOrMarkupContent(new MarkupContent { Kind = MarkupKind.Markdown, Value = MarkdownHelper.FormatHelp(func.Help) }) : null,
                InsertText = insertText,
                InsertTextFormat = insertFormat
            });
        }

        // 3.2 Módulos de plugins (ej: "lapsos")
        foreach (var module in _pluginManager.GetModules())
        {
            // Agregar el módulo en sí
            completions.Add(new CompletionItem
            {
                Label = module.Name,
                Kind = CompletionItemKind.Module,
                Detail = "Módulo de Plugin"
            });

            // Agregar todos los métodos del módulo (ej: "lapsos.ahora")
            foreach (var method in module.Methods.Values)
            {
                string detail = $"Método de {module.Name}";
                string insertText = method.Name; // Ya contiene el prefijo completo
                InsertTextFormat insertFormat = InsertTextFormat.PlainText;

                if (!string.IsNullOrEmpty(method.Args))
                {
                    insertFormat = InsertTextFormat.Snippet;
                    var pList = method.Args.Split(',').Select(p => p.Trim()).ToList();
                    var snippetArgs = string.Join(", ", pList.Select((p, i) => $"${{{i+1}:{p}}}"));
                    insertText = $"{method.Name}({snippetArgs})";
                    detail += $"({method.Args})";
                }

                completions.Add(new CompletionItem
                {
                    Label = method.Name,
                    Kind = CompletionItemKind.Method,
                    Detail = detail,
                    Documentation = !string.IsNullOrEmpty(method.Help) ? new StringOrMarkupContent(new MarkupContent { Kind = MarkupKind.Markdown, Value = MarkdownHelper.FormatHelp(method.Help) }) : null,
                    InsertText = insertText,
                    InsertTextFormat = insertFormat
                });
            }
        }

        return Task.FromResult(new CompletionList(completions));
    }
}
