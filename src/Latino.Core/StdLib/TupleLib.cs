using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class TupleLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") =>
            methods[name] = new NativeFunction(logic, arity).WithDocs(desc, pams);

        Def("crear", args => new LatinoTuple(args), -1,
            "Crea una nueva tupla con los elementos proporcionados", "elementos...");
        Def("obtener", args => {
            var tuple = GetTuple(args[0]);
            var index = (int)((LatinoNumber)args[1]).Value;
            return tuple.Get(index);
        }, 2, "Obtiene el elemento en la posición especificada de la tupla", "tupla, indice");

        Def("longitud", args => new LatinoNumber(GetTuple(args[0]).Elements.Count), 1,
            "Devuelve el número de elementos en la tupla", "tupla");

        interpreter.Globals.Define("tupla", new LatinoDictionary(ConvertMap(methods), true));
    }

    private static LatinoTuple GetTuple(LatinoObject obj)
    {
        if (obj is LatinoTuple t) return t;
        throw new RuntimeError(null!, $"Se esperaba una tupla, se recibió: {obj.TypeName}");
    }

    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>();
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
