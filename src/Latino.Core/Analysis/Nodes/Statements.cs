using System.Collections.Generic;

namespace Latino.Core.Analysis.Nodes;

// Base
public abstract record Statement;

// Expresión como sentencia
public record ExpressionStatement(Expression Expression) : Statement;

// Bloque de código
public record BlockStatement(List<Statement> Statements) : Statement;

// Variables (global a = 1, local a = 1)
public record VarDeclaration(Token Name, Expression? Initializer, bool IsLocal = false) : Statement;

// IF (si - osi - sino)
public record IfStatement(Expression Condition, Statement ThenBranch, Statement? ElseBranch) : Statement;

// WHILE (mientras)
public record WhileStatement(Expression Condition, Statement Body) : Statement;

// DO-WHILE (repetir ... hasta)
public record DoWhileStatement(Statement Body, Expression Condition) : Statement;

// FOR clásico (desde i=0; i<10; i++)
public record ForStatement(Statement? Initializer, Expression? Condition, Expression? Increment, Statement Body) : Statement;

// FOREACH (para elemento en lista)
public record ForeachStatement(Token Variable, Expression Iterable, Statement Body) : Statement;

// SWITCH (elegir ... caso ... defecto)
public record SwitchStatement(Expression Value, List<CaseStatement> Cases, Statement? DefaultBranch) : Statement;
public record CaseStatement(Expression Value, Statement Body); // Sub-nodo para los casos

// Saltos
public record BreakStatement(Token Keyword) : Statement;
public record ContinueStatement(Token Keyword) : Statement;
public record ReturnStatement(Token Keyword, Expression? Value) : Statement;

// Funciones
public record Parameter(Token Name, Expression? DefaultValue);

public record FunctionStatement(Token Name, List<Parameter> Params, List<Statement> Body) : Statement;

// Clases
public record ClassStatement(Token Name, VariableExpression? Superclass, List<FunctionStatement> Methods) : Statement;

// Manejo de Errores
public record TryCatchStatement(Statement TryBranch, Token? ErrorVariable, Statement CatchBranch) : Statement;

// Etiquetas y Saltos (Goto)
public record LabelStatement(Token Name) : Statement;
public record GotoStatement(Token LabelName) : Statement;

// Operadores de Flujo
public record SwapStatement(Expression Left, Expression Right) : Statement;

