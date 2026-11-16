using System;
using System.Collections.Generic;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class QueueLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") =>
            methods[name] = new NativeFunction(logic, arity).WithDocs(desc, pams);

        Def("crear", args => new LatinoQueue(), 0, "Crea una cola vacía.", "");
        
        Def("encolar", args => {
            Q(args[0]).Enqueue(args[1]);
            return args[0];
        }, 2, "Agrega un elemento al final.", "cola, elemento");
        
        Def("desencolar", args => {
            var q = Q(args[0]);
            if (q.Count == 0) throw new RuntimeError(null!, "La cola está vacía.");
            return q.Dequeue();
        }, 1, "Extrae el elemento del inicio.", "cola");
        
        Def("siguiente", args => {
            var q = Q(args[0]);
            if (q.Count == 0) throw new RuntimeError(null!, "La cola está vacía.");
            return q.Peek();
        }, 1, "Obtiene el siguiente elemento sin extraerlo.", "cola");
        
        Def("longitud", args => new LatinoNumber(Q(args[0]).Count), 1, "Devuelve la cantidad de elementos.", "cola");

        Def("vacia", args => new LatinoBoolean(Q(args[0]).Count == 0), 1, 
            "Verifica si la cola no contiene elementos.", "cola");
        
        Def("limpiar", args => {
            Q(args[0]).Clear();
            return args[0];
        }, 1, "Elimina todos los elementos de la cola.", "cola");
        
        Def("alista", args => {
            var lista = new List<LatinoObject>(Q(args[0]));
            // Queue ya itera en orden FIFO (inicio→fin), no necesita Reverse
            return new LatinoList(lista);
        }, 1, "Convierte la cola a una lista (inicio→fin).", "cola");

        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("cola", modulo);
    }

    private static Queue<LatinoObject> Q(LatinoObject obj)
    {
        if (obj is LatinoQueue q) return q.Elements;
        throw new RuntimeError(null!, $"Se esperaba una cola, se recibió: {obj.TypeName}");
    }

    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
