using System;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class StackLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") =>
            methods[name] = new NativeFunction(logic, arity).WithDocs(desc, pams);

        Def("crear", args => new LatinoStack(), 0, "Crea una pila vacía.", "");
        
        Def("apilar", args => {
            S(args[0]).Push(args[1]);
            return args[0];
        }, 2, "Agrega un elemento a la cima.", "pila, elemento");
        
        Def("desapilar", args => {
            var s = S(args[0]);
            if (s.Count == 0) throw new RuntimeError(null!, "La pila está vacía.");
            return s.Pop();
        }, 1, "Extrae el elemento de la cima.", "pila");
        
        Def("cima", args => {
            var s = S(args[0]);
            if (s.Count == 0) throw new RuntimeError(null!, "La pila está vacía.");
            return s.Peek();
        }, 1, "Obtiene el elemento de la cima sin extraerlo.", "pila");
        
        Def("longitud", args => new LatinoNumber(S(args[0]).Count), 1, "Devuelve la cantidad de elementos.", "pila");

        Def("vacia", args => new LatinoBoolean(S(args[0]).Count == 0), 1, 
            "Verifica si la pila no contiene elementos.", "pila");
        
        Def("limpiar", args => {
            S(args[0]).Clear();
            return args[0];
        }, 1, "Elimina todos los elementos de la pila.", "pila");
        
        Def("alista", args => {
            var lista = new List<LatinoObject>(S(args[0]));
            // Stack itera en orden LIFO, así que invertimos para mantener orden lógico
            lista.Reverse();
            return new LatinoList(lista);
        }, 1, "Convierte la pila a una lista (fondo→cima).", "pila");

        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("pila", modulo);
    }

    private static Stack<LatinoObject> S(LatinoObject obj)
    {
        if (obj is LatinoStack s) return s.Elements;
        throw new RuntimeError(null!, $"Se esperaba una pila, se recibió: {obj.TypeName}");
    }
    
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
