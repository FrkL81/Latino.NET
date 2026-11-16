using System.Collections.Generic;
using Latino.Core.Analysis;
using Latino.Core.Types;

namespace Latino.Core.Analysis.Nodes;

// Base para todas las expresiones (cosas que tienen un valor)
public abstract record Expression;

// Expresiones Literales (Valores fijos como 10, "hola", verdadero)
public record LiteralExpression(object? Value) : Expression;

// Expresiones Binarias (Suma, Resta, Multiplicación, Comparaciones)
// Ejemplo: 10 + 20
public record BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression;

// Expresiones Unarias (Negación, Not, Incremento Prefijo)
// Ejemplo: -5, !verdadero, ++i
public record UnaryExpression(Token Operator, Expression Right) : Expression;

// Expresiones de Sufijo (Incremento Sufijo)
// Ejemplo: i++, i--
public record PostfixExpression(Expression Left, Token Operator) : Expression;

// Expresiones de Agrupación (Paréntesis)
// Ejemplo: (10 + 2)
public record GroupingExpression(Expression Expression) : Expression;

// Uso de Variables
// Ejemplo: nombre
public record VariableExpression(Token Name) : Expression;

// Asignación de Variables
// Ejemplo: a = 10
public record AssignExpression(Token Name, Expression Value) : Expression;

// Llamada a Función
// Ejemplo: imprimir("hola")
public record CallExpression(Expression Callee, Token Paren, List<Expression> Arguments) : Expression;

// Acceso a Propiedades (Para futuras listas/diccionarios)
// Ejemplo: lista.agregar
public record GetExpression(Expression Object, Token Name) : Expression;

// Asignación a Propiedades
// Ejemplo: obj.prop = val
public record SetExpression(Expression Object, Token Name, Expression Value) : Expression;

// Acceso por Índice
// Ejemplo: lista[0]
public record IndexExpression(Expression Object, Token Bracket, Expression Index) : Expression;

// Literal de Lista
// Ejemplo: [1, "dos", verdadero]
public record ListLiteralExpression(List<Expression> Elements) : Expression;

// Literal de Tupla
// Ejemplo: (1, "dos", verdadero)
public record TupleExpression(List<Expression> Elements) : Expression;

// Literal de Diccionario
// Ejemplo: { "llave": "valor", "otra": 2 }
public record DictLiteralExpression(List<Expression> Keys, List<Expression> Values) : Expression;

// Expresión de Función Anónima
// Ejemplo: x = funcion(a, b) ... fin
public record AnonymousFunctionExpression(List<Parameter> Params, List<Statement> Body) : Expression;

// Representa: objeto[indice] = valor
public record IndexSetExpression(Expression Object, Expression Index, Expression Value) : Expression;

// Operador Ternario ( a ? b : c )
public record TernaryExpression(Expression Condition, Expression ThenBranch, Expression ElseBranch) : Expression;
