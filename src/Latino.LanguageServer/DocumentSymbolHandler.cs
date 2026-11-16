using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Latino.Core.Analysis;
using Latino.Core.Analysis.Nodes;

namespace Latino.LanguageServer;

public class DocumentSymbolHandler : IDocumentSymbolHandler
{
    public DocumentSymbolRegistrationOptions GetRegistrationOptions(DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentSymbolRegistrationOptions { DocumentSelector = TextDocumentSelector.ForLanguage("latino") };
    }

    public Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
    {
        var code = BufferManager.GetBuffer(request.TextDocument.Uri);
        if (code == null) return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(null);

        var symbols = new List<SymbolInformationOrDocumentSymbol>();
        
        try {
            var parser = new Parser(new Lexer(code).Tokenize());
            var statements = parser.Parse();

            foreach(var stmt in statements)
            {
                if (stmt is FunctionStatement func) {
                    var symbol = new SymbolInformation {
                        Name = func.Name.Text,
                        Kind = SymbolKind.Function,
                        Location = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Location {
                            Uri = request.TextDocument.Uri,
                            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(func.Name.Line - 1, 0, func.Name.Line - 1, 10)
                        }
                    };
                    symbols.Add(symbol);
                }
                else if (stmt is ClassStatement clazz) {
                    var symbol = new SymbolInformation {
                        Name = clazz.Name.Text,
                        Kind = SymbolKind.Class,
                        Location = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Location {
                            Uri = request.TextDocument.Uri,
                            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(clazz.Name.Line - 1, 0, clazz.Name.Line - 1, 10)
                        }
                    };
                    symbols.Add(symbol);
                }
            }
        } catch { } // Ignorar errores de parseo para el outline

        return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(new SymbolInformationOrDocumentSymbolContainer(symbols));
    }
}
