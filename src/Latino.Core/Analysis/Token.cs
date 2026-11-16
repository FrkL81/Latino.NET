namespace Latino.Core.Analysis;

public record Token(TokenType Type, string Text, object? Value, int Line, int Column);
