using System.Collections.Generic;
using System.Globalization;
using Latino.Core.Types;

namespace Latino.Core.Analysis;

public class Lexer
{
    private readonly string _text;
    private int _pos;
    private int _line = 1;
    private int _col = 1;

    public static readonly Dictionary<string, TokenType> Keywords = new()
    {
        // Básicos
        { "si", TokenType.Keyword }, { "fin", TokenType.Keyword },
        { "sino", TokenType.Keyword }, { "mientras", TokenType.Keyword },
        { "repetir", TokenType.Keyword }, { "funcion", TokenType.Keyword },
        { "retornar", TokenType.Keyword }, { "verdadero", TokenType.Keyword },
        { "cierto", TokenType.Keyword }, { "falso", TokenType.Keyword },
        { "espurio", TokenType.Keyword }, { "nulo", TokenType.Keyword },

        // Operadores Lógicos
        { "ý", TokenType.LogicAnd },
        { "ó", TokenType.LogicOr },
        { "no", TokenType.LogicNot },

        // Faltantes (Control)
        { "osi", TokenType.Keyword },
        { "romper", TokenType.Keyword }, { "continuar", TokenType.Keyword },
        { "hasta", TokenType.Keyword }, { "desde", TokenType.Keyword },
        { "para", TokenType.Keyword }, { "en", TokenType.Keyword },
        { "elegir", TokenType.Keyword }, { "caso", TokenType.Keyword },
        { "otro", TokenType.Keyword }, { "defecto", TokenType.Keyword },
        { "ir", TokenType.Keyword },

        // OOP
        { "clase", TokenType.Keyword }, { "constructor", TokenType.Keyword },
        { "es", TokenType.Keyword }, { "super", TokenType.Keyword },
        { "mi", TokenType.Keyword }, { "este", TokenType.Keyword }, { "esta", TokenType.Keyword },
        { "estatico", TokenType.Keyword },
        
        // Instanciación Unificada
        { "nueva", TokenType.New },
        { "crear", TokenType.New },
        { "nuevo", TokenType.New },

        // Otros
        { "global", TokenType.Keyword },
        { "local", TokenType.Keyword },
        { "intentar", TokenType.Keyword }, { "atrapar", TokenType.Keyword },

        // Alias
        { "fun", TokenType.Keyword },
        { "retorno", TokenType.Keyword }, { "ret", TokenType.Keyword }
    };

    public Lexer(string text) => _text = text;

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < _text.Length)
        {
            char c = Current();

            // 1. Ignorar Espacios
            if (char.IsWhiteSpace(c)) { Advance(); continue; }

            // 2. Comentarios (Soporte Híbrido: #, // y /* */)
            if (c == '#' || (c == '/' && Peek() == '/')) { ReadLineComment(); continue; }
            if (c == '/' && Peek() == '*') { ReadBlockComment(); continue; }

            // 3. Tipos de Datos
            if (char.IsDigit(c)) { tokens.Add(ReadNumber()); continue; }
            if (char.IsLetter(c) || c == '_') { tokens.Add(ReadIdent()); continue; }
            if (c == '"' || c == '\'') { tokens.Add(ReadString(c)); continue; }

            // 4. Símbolos
            var sym = ReadSymbol();
            if (sym != null) { tokens.Add(sym); continue; }

            // Error
            tokens.Add(new Token(TokenType.Error, c.ToString(), null, _line, _col));
            Advance();
        }
        tokens.Add(new Token(TokenType.EndOfFile, "\0", null, _line, _col));
        return tokens;
    }

    // --- Métodos Auxiliares ---

    private char Current() => _pos >= _text.Length ? '\0' : _text[_pos];
    private char Peek() => _pos + 1 >= _text.Length ? '\0' : _text[_pos + 1];
    private char PeekNext() => _pos + 2 >= _text.Length ? '\0' : _text[_pos + 2];

    private void Advance() {
        if (Current() == '\n') { _line++; _col = 0; }
        _pos++; _col++;
    }

    private void ReadLineComment() {
        while (Current() != '\n' && Current() != '\0') Advance();
    }

    private void ReadBlockComment() {
        Advance(); Advance(); // Consumir /*
        while (!(Current() == '*' && Peek() == '/') && Current() != '\0') Advance();
        Advance(); Advance(); // Consumir */
    }

    private Token ReadNumber()
    {
        int start = _pos;
        int col = _col;
        
        while (char.IsDigit(Current())) Advance();

        // Parte Decimal
        if (Current() == '.' && char.IsDigit(Peek()))
        {
            Advance();
            while (char.IsDigit(Current())) Advance();
        }

        // Parte Exponente (Notación Científica)
        if (Current() == 'e' || Current() == 'E')
        {
            // Verificamos si lo que sigue es un número o signo
            char next = Peek();
            if (char.IsDigit(next) || next == '+' || next == '-')
            {
                Advance(); // Consumir 'e'
                if (Current() == '+' || Current() == '-') Advance(); // Consumir signo
                while (char.IsDigit(Current())) Advance(); // Consumir exponente
            }
        }

        string text = _text[start.._pos];
        
        // Parsear usando CultureInfo.InvariantCulture para asegurar que el punto sea decimal
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            return new Token(TokenType.Number, text, new LatinoNumber(value), _line, col);
        }

        return new Token(TokenType.Error, text, null, _line, col);
    }

    private Token ReadString(char quote) {
        int col = _col;
        Advance(); // Consumir apertura
        int start = _pos;
        
        while (Current() != quote && Current() != '\0') {
            if (Current() == '\\') {
                Advance(); // Consumir barra
                if (Current() != '\0') Advance(); // Consumir caracter escapado (saltarlo)
            } else {
                Advance();
            }
        }
        
        if (Current() == '\0') return new Token(TokenType.Error, "Cadena sin cerrar", null, _line, col);

        string raw = _text[start.._pos];
        Advance(); // Consumir cierre
        
        try {
            string processed = System.Text.RegularExpressions.Regex.Unescape(raw);
            return new Token(TokenType.String, processed, new LatinoString(processed), _line, col);
        } catch {
            return new Token(TokenType.String, raw, new LatinoString(raw), _line, col);
        }
    }

    private Token ReadIdent() {
        int start = _pos, col = _col;
        while (char.IsLetterOrDigit(Current()) || Current() == '_') Advance();
        string txt = _text[start.._pos];
        // Determina si es palabra clave o variable
        return new Token(Keywords.TryGetValue(txt, out var type) ? type : TokenType.Identifier, txt, null, _line, col);
    }

    private Token? ReadSymbol()
    {
        int col = _col;
        char c = Current();
        char next = Peek();

        // Operadores de 3 caracteres
        if (c == '.' && next == '.') {
            if (PeekNext() == '=') { Advance(); Advance(); Advance(); return NewToken(TokenType.ConcatEqual, "..=", col); }
            Advance(); Advance(); return NewToken(TokenType.Concat, "..", col);
        }

        // Operadores de 2 caracteres
        switch (c)
        {
            case '+':
                if (next == '+') { Advance(); Advance(); return NewToken(TokenType.Increment, "++", col); }
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.PlusEqual, "+=", col); }
                break;
            case '?':
                if (next == '?') {
                    if (PeekNext() == '=') { Advance(); Advance(); Advance(); return NewToken(TokenType.NullCoalescingEqual, "??=", col); }
                    Advance(); Advance(); return NewToken(TokenType.NullCoalescing, "??", col);
                }
                break;
            case '-':
                if (next == '-') { Advance(); Advance(); return NewToken(TokenType.Decrement, "--", col); }
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.MinusEqual, "-=", col); }
                break;
            case '*':
                if (next == '*') { Advance(); Advance(); return NewToken(TokenType.Power, "**", col); } // Alias potencia
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.StarEqual, "*=", col); }
                break;
            case '/':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.SlashEqual, "/=", col); }
                break;
            case '%':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.ModEqual, "%=", col); }
                break;
            case '=':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.EqualEqual, "==", col); }
                break;
            case '!':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.NotEqual, "!=", col); }
                break;
            case '>':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.GreaterEqual, ">=", col); }
                break;
            case '<':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.LessEqual, "<=", col); }
                break;
            case '~':
                if (next == '=') { Advance(); Advance(); return NewToken(TokenType.RegexMatch, "~=", col); }
                // Si no es ~=, entonces es Swap (~)
                Advance();
                return NewToken(TokenType.Swap, "~", col);
            case '&':
                if (next == '&') { Advance(); Advance(); return NewToken(TokenType.LogicAnd, "&&", col); }
                break;
            case '|':
                if (next == '|') { Advance(); Advance(); return NewToken(TokenType.LogicOr, "||", col); }
                if (next == '>') { Advance(); Advance(); return NewToken(TokenType.Pipe, "|>", col); }
                // Si no es || ni |>, es un error (Latino no soporta bitwise OR)
                throw new System.Exception($"Carácter inesperado '|' en línea {_line}, columna {col}. ¿Querías usar '||' (OR lógico) o '|>' (pipe)?");
        }

        // Si no fue ?? o ??=, pero es un ? simple (operador ternario)
        if (c == '?') { Advance(); return NewToken(TokenType.Question, "?", col); }

        // Operadores simples de 1 caracter
        Advance();
        return c switch
        {
            '+' => NewToken(TokenType.Plus, "+", col),
            '-' => NewToken(TokenType.Minus, "-", col),
            '*' => NewToken(TokenType.Star, "*", col),
            '/' => NewToken(TokenType.Slash, "/", col),
            '%' => NewToken(TokenType.Modulo, "%", col),
            '^' => NewToken(TokenType.Power, "^", col),
            '=' => NewToken(TokenType.Equal, "=", col),
            '!' => NewToken(TokenType.LogicNot, "!", col),
            '(' => NewToken(TokenType.LParen, "(", col),
            ')' => NewToken(TokenType.RParen, ")", col),
            '{' => NewToken(TokenType.LBrace, "{", col),
            '}' => NewToken(TokenType.RBrace, "}", col),
            '[' => NewToken(TokenType.LBracket, "[", col),
            ']' => NewToken(TokenType.RBracket, "]", col),
            '.' => NewToken(TokenType.Dot, ".", col),
            ',' => NewToken(TokenType.Comma, ",", col),
            ':' => NewToken(TokenType.Colon, ":", col),
            ';' => NewToken(TokenType.Semicolon, ";", col),
            '>' => NewToken(TokenType.Greater, ">", col),
            '<' => NewToken(TokenType.Less, "<", col),
            _ => null
        };
    }

    private Token NewToken(TokenType type, string text, int col) => new Token(type, text, null, _line, col);
}
