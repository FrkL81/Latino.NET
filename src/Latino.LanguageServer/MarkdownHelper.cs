namespace Latino.LanguageServer;

public static class MarkdownHelper
{
    /// <summary>
    /// Formatea el texto de ayuda inteligentemente.
    /// Si contiene saltos de línea (\n) o fórmulas matemáticas ($), lo retorna crudo.
    /// Si es texto plano simple, lo envuelve en negritas.
    /// </summary>
    public static string FormatHelp(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // Detección de Markdown complejo o LaTeX
        bool esComplejo = text.Contains('\n') || text.Contains('$') || text.Contains('#');
        
        return esComplejo ? text : $"**{text}**";
    }
}
