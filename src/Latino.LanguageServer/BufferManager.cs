using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Latino.LanguageServer;

public static class BufferManager
{
    // Diccionario seguro para guardar el código: URI del archivo -> Contenido del código
    private static readonly ConcurrentDictionary<DocumentUri, string> _buffers = new();

    public static void UpdateBuffer(DocumentUri uri, string text) => _buffers[uri] = text;
    public static string? GetBuffer(DocumentUri uri) => _buffers.TryGetValue(uri, out var text) ? text : null;
}
