using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Latino.Core.Analysis;

namespace Latino.LanguageServer;

public class TextDocumentHandler : ITextDocumentSyncHandler
{
    private readonly ILanguageServerFacade _router;
    private readonly ILogger<TextDocumentHandler> _logger;
    private readonly PluginManager _pluginManager;

    public TextDocumentHandler(
        ILanguageServerFacade router, 
        ILogger<TextDocumentHandler> logger,
        PluginManager pluginManager)
    {
        _router = router;
        _logger = logger;
        _pluginManager = pluginManager;
    }

    public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[LSP] Archivo abierto: {request.TextDocument.Uri}");
        
        // 1. Guardar en memoria
        BufferManager.UpdateBuffer(request.TextDocument.Uri, request.TextDocument.Text);
        
        // 2. NUEVO: Escanear y cargar plugins
        _pluginManager.ScanAndLoadPlugins(request.TextDocument.Uri, request.TextDocument.Text);
        
        // 3. Validar (Lógica existente)
        ValidateTextDocument(request.TextDocument.Uri, request.TextDocument.Text);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        foreach (var change in request.ContentChanges)
        {
            // En modo Full sync, el cambio trae todo el texto
            BufferManager.UpdateBuffer(request.TextDocument.Uri, change.Text);
            
            // NUEVO: Re-escanear plugins en cada cambio
            _pluginManager.ScanAndLoadPlugins(request.TextDocument.Uri, change.Text);
            
            ValidateTextDocument(request.TextDocument.Uri, change.Text);
        }
        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    // --- Capabilities & Registration ---

    public void SetCapability(TextSynchronizationCapability capability) 
    {
    }

    TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentChangeRegistrationOptions { 
            DocumentSelector = TextDocumentSelector.ForLanguage("latino"), 
            SyncKind = Change 
        };
    }

    TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentOpenRegistrationOptions { 
            DocumentSelector = TextDocumentSelector.ForLanguage("latino") 
        };
    }

    TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentCloseRegistrationOptions { 
            DocumentSelector = TextDocumentSelector.ForLanguage("latino") 
        };
    }

    TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSaveRegistrationOptions { 
            DocumentSelector = TextDocumentSelector.ForLanguage("latino"),
            IncludeText = true
        };
    }

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, "latino");
    }

    // --- LÓGICA DE VALIDACIÓN ---

    private void ValidateTextDocument(DocumentUri uri, string text)
    {
        var diagnostics = new List<Diagnostic>();

        try
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Tokenize();
            
            // Verificar errores léxicos primero
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Error)
                {
                    _logger.LogWarning($"[LSP] Error Léxico encontrado: {token.Text}");
                    diagnostics.Add(new Diagnostic {
                        Severity = DiagnosticSeverity.Error,
                        Message = $"Carácter inesperado: '{token.Text}'",
                        Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                            new Position(token.Line - 1, token.Column - 1),
                            new Position(token.Line - 1, token.Column)
                        )
                    });
                }
            }

            var parser = new Parser(tokens);
            parser.Parse();
            
            // AHORA LEEMOS LOS ERRORES ACUMULADOS DEL PARSER
            foreach (var error in parser.Errors)
            {
                var line = Math.Max(0, error.Token.Line - 1);
                var col = Math.Max(0, error.Token.Column - 1);
                var length = Math.Max(1, error.Token.Text?.Length ?? 1);

                diagnostics.Add(new Diagnostic {
                    Severity = DiagnosticSeverity.Error,
                    Message = error.Message,
                    Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                        new Position(line, col),
                        new Position(line, col + length)
                    ),
                    Source = "Latino Parser"
                });
            }
            
            if (parser.Errors.Count == 0)
            {
                _logger.LogInformation("[LSP] Análisis completado sin errores.");
            }
            else
            {
                _logger.LogWarning($"[LSP] Se detectaron {parser.Errors.Count} errores de sintaxis.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"[LSP] Excepción Crítica: {ex.Message}");
        }

        _router.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams {
            Uri = uri,
            Diagnostics = new Container<Diagnostic>(diagnostics)
        });
    }
}
