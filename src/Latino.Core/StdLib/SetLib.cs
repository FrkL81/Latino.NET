using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class SetLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") =>
            methods[name] = new NativeFunction(logic, arity).WithDocs(desc, pams);

        Def("crear", args => new LatinoSet(args), -1,
            "Crea un nuevo conjunto con los elementos únicos proporcionados", "elementos...");
        Def("agregar", args => { S(args[0]).Add(args[1]); return args[0]; }, 2,
            "Agrega un elemento al conjunto y devuelve el conjunto modificado", "conjunto, elemento");
        Def("eliminar", args => { S(args[0]).Remove(args[1]); return args[0]; }, 2,
            "Elimina un elemento del conjunto y devuelve el conjunto modificado", "conjunto, elemento");
        Def("contiene", args => new LatinoBoolean(S(args[0]).Contains(args[1])), 2,
            "Verifica si el conjunto contiene el elemento especificado", "conjunto, elemento");
        Def("union", args => {
            var newSet = new HashSet<LatinoObject>(S(args[0]), new LatinoObjectComparer());
            newSet.UnionWith(S(args[1]));
            return new LatinoSet(newSet);
        }, 2, "Crea un nuevo conjunto con la unión de dos conjuntos", "conjunto1, conjunto2");
        Def("interseccion", args => {
            var newSet = new HashSet<LatinoObject>(S(args[0]), new LatinoObjectComparer());
            newSet.IntersectWith(S(args[1]));
            return new LatinoSet(newSet);
        }, 2, "Crea un nuevo conjunto con la intersección de dos conjuntos", "conjunto1, conjunto2");
        Def("diferencia", args => {
            var newSet = new HashSet<LatinoObject>(S(args[0]), new LatinoObjectComparer());
            newSet.ExceptWith(S(args[1]));
            return new LatinoSet(newSet);
        }, 2, "Crea un nuevo conjunto con los elementos del primer conjunto que no están en el segundo", "conjunto1, conjunto2");
        Def("longitud", args => new LatinoNumber(S(args[0]).Count), 1,
            "Devuelve el número de elementos en el conjunto", "conjunto");

        interpreter.Globals.Define("conjunto", new LatinoDictionary(ConvertMap(methods), true));
    }

    private static HashSet<LatinoObject> S(LatinoObject obj)
    {
        if (obj is LatinoSet s) return s.Elements;
        throw new RuntimeError(null!, $"Se esperaba un conjunto, se recibió: {obj.TypeName}");
    }
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>();
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
