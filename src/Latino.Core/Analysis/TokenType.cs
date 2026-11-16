namespace Latino.Core.Analysis;

public enum TokenType
{
    // Control
    EndOfFile, Error,

    // Literales
    Identifier, String, Number,

    // Palabras Clave (Genérico para todas las keywords)
    Keyword,
    
    // Instanciación (NUEVO)
    New,

    // Operadores Matemáticos
    Plus,       // +
    Minus,      // -
    Star,       // *
    Slash,      // /
    Modulo,     // %
    Power,      // ^ o **

    // Asignación Compuesta
    PlusEqual,  // +=
    MinusEqual, // -=
    StarEqual,  // *=
    SlashEqual, // /=
    ModEqual,   // %=
    ConcatEqual,// ..=

    // Incremento / Decremento
    Increment,  // ++
    Decrement,  // --

    // Lógicos
    LogicAnd,   // &&
    LogicOr,    // ||
    LogicNot,   // !
    NullCoalescing,      // ??
    NullCoalescingEqual, // ??=

    // Comparación
    Equal,      // = (Asignación)
    EqualEqual, // ==
    NotEqual,   // !=
    Greater,    // >
    GreaterEqual, // >=
    Less,       // <
    LessEqual,    // <=
    RegexMatch,   // ~=

    // Operadores de Flujo
    Swap,       // ~
    Pipe,       // |>

    // Estructurales
    Concat,     // ..
    Dot,        // .
    Comma,      // ,
    Colon,      // :
    Question,   // ?
    Semicolon,  // ;

    // Agrupación
    LParen, RParen, // ( )
    LBrace, RBrace, // { }
    LBracket, RBracket // [ ]
}
