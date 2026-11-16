using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class DictLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        Def("contiene", (i, args) => new LatinoBoolean(GetDict(args[0]).Pairs.ContainsKey(args[1])), 2,
            "Verifica si el diccionario contiene la clave especificada", "diccionario, clave");
        
        Def("eliminar", (i, args) => { 
            var dict = GetDict(args[0]);
            if (dict.IsReadOnly) throw new RuntimeError(null!, "El diccionario es de solo lectura.");
            dict.Pairs.Remove(args[1]); 
            return args[0]; 
        }, 2, "Elimina una clave del diccionario", "diccionario, clave");
        
        Def("llaves", (i, args) => new LatinoList(GetDict(args[0]).Pairs.Keys.ToList()), 1,
            "Devuelve una lista con todas las claves.", "diccionario");
        
        Def("valores", (i, args) => new LatinoList(GetDict(args[0]).Pairs.Values.ToList()), 1,
            "Devuelve una lista con todos los valores.", "diccionario");
        
        methods["vals"] = methods["valores"];
        
        Def("longitud", (i, args) => new LatinoNumber(GetDict(args[0]).Pairs.Count), 1,
            "Devuelve el número de elementos.", "diccionario");
        
        Def("fusionar", (i, args) => {
            var d1 = GetDict(args[0]);
            var d2 = GetDict(args[1]);
            var newDict = new Dictionary<LatinoObject, LatinoObject>(d1.Pairs, LatinoObjectComparer.Instance);
            foreach (var kvp in d2.Pairs)
            {
                newDict[kvp.Key] = kvp.Value;
            }
            return new LatinoDictionary(newDict);
        }, 2, "Crea un nuevo diccionario combinando dos.", "diccionario1, diccionario2");

        Def("obtener", (i, args) => {
            if (args.Count < 2)
                throw new RuntimeError(null!, 
                    "dic.obtener() requiere al menos 2 argumentos: diccionario y clave.");
            
            if (GetDict(args[0]).Pairs.TryGetValue(args[1], out var value))
            {
                return value;
            }
            return args.Count > 2 ? args[2] : LatinoNull.Instance;
        }, -1, "Obtiene el valor de una clave o un valor por defecto.", "diccionario, clave, [defecto]");

        Def("agregar", (i, args) => {
            var dict = GetDict(args[0]);
            var key = args[1];
            var val = args[2];
            
            if (dict.IsReadOnly) throw new RuntimeError(null!, "El diccionario es de solo lectura.");
            
            dict.Pairs[key] = val;
            return LatinoNull.Instance;
        }, 3, "Agrega o actualiza un valor.", "diccionario, clave, valor");

        // =====================================================================
        // PAQUETE 4: DICCIONARIOS FUNCIONALES
        // =====================================================================

        Def("invertir", (interp, args) => {
            var dict = GetDict(args[0]);
            var invertido = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            foreach (var kvp in dict.Pairs)
            {
                // El valor se convierte en llave, la llave en valor
                // Si hay valores duplicados, el último sobrescribe
                invertido[kvp.Value] = kvp.Key;
            }
            
            return new LatinoDictionary(invertido);
        }, 1, "Intercambia llaves y valores. Si hay valores duplicados, prevalece la última llave.", "diccionario");

        Def("transformarValores", (interp, args) => {
            var dict = GetDict(args[0]);
            var func = args[1] as ICallable 
                ?? throw new RuntimeError(null!, 
                    "El segundo argumento de dic.transformarValores() debe ser una función.");
            
            var transformado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            foreach (var kvp in dict.Pairs)
            {
                var nuevoValor = func.Call(interp, new List<LatinoObject> { kvp.Value });
                transformado[kvp.Key] = nuevoValor;
            }
            
            return new LatinoDictionary(transformado);
        }, 2, "Aplica una función a cada valor. Retorna nuevo diccionario con llaves originales.", "diccionario, funcion");

        Def("filtrar", (interp, args) => {
            var dict = GetDict(args[0]);
            var func = args[1] as ICallable 
                ?? throw new RuntimeError(null!, 
                    "El segundo argumento de dic.filtrar() debe ser una función.");
            
            var filtrado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            foreach (var kvp in dict.Pairs)
            {
                var resultado = func.Call(interp, new List<LatinoObject> { kvp.Key, kvp.Value });
                
                if (interp.IsTruthy(resultado))
                {
                    filtrado[kvp.Key] = kvp.Value;
                }
            }
            
            return new LatinoDictionary(filtrado);
        }, 2, "Filtra pares llave-valor. La función recibe (llave, valor) y retorna verdadero para conservar.", "diccionario, funcion");

        Def("transformarLlaves", (interp, args) => {
            var dict = GetDict(args[0]);
            var func = args[1] as ICallable 
                ?? throw new RuntimeError(null!, 
                    "El segundo argumento de dic.transformarLlaves() debe ser una función.");
            
            var transformado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            foreach (var kvp in dict.Pairs)
            {
                var nuevaLlave = func.Call(interp, new List<LatinoObject> { kvp.Key });
                transformado[nuevaLlave] = kvp.Value; 
            }
            
            return new LatinoDictionary(transformado);
        }, 2, "Aplica una función a cada llave. Si hay colisiones, prevalece la última.", "diccionario, funcion");

        Def("mapear", (interp, args) => {
            var dict = GetDict(args[0]);
            var func = args[1] as ICallable 
                ?? throw new RuntimeError(null!, 
                    "El segundo argumento de dic.mapear() debe ser una función.");
            
            var mapeado = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
            
            foreach (var kvp in dict.Pairs)
            {
                var resultado = func.Call(interp, new List<LatinoObject> { kvp.Key, kvp.Value });
                
                if (resultado is not LatinoDictionary resultDict)
                    throw new RuntimeError(null!, 
                        "dic.mapear() requiere que la función retorne un diccionario con llaves 'llave' y 'valor'.");
                
                var llaveKey = new LatinoString("llave");
                var valorKey = new LatinoString("valor");
                
                if (!resultDict.Pairs.ContainsKey(llaveKey) || !resultDict.Pairs.ContainsKey(valorKey))
                    throw new RuntimeError(null!, 
                        "dic.mapear() requiere que el diccionario retornado tenga las llaves 'llave' y 'valor'.");
                
                var nuevaLlave = resultDict.Pairs[llaveKey];
                var nuevoValor = resultDict.Pairs[valorKey];
                
                mapeado[nuevaLlave] = nuevoValor;
            }
            
            return new LatinoDictionary(mapeado);
        }, 2, "Transforma llaves y valores. La función recibe (llave, valor) y retorna {llave: nuevaLlave, valor: nuevoValor}.", "diccionario, funcion");

        interpreter.Globals.Define("dic", new LatinoDictionary(ConvertMap(methods), true));
    }

    private static LatinoDictionary GetDict(LatinoObject obj)
    {
        if (obj is LatinoDictionary d) return d;
        throw new RuntimeError(null!, $"Se esperaba un diccionario, se recibió: {obj.GetType().Name}");
    }
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
