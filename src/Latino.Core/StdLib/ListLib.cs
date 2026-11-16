using System;
using System.Collections.Generic;
using System.Linq;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Core.StdLib;

public static class ListLib
{
    public static void Register(Interpreter interpreter)
    {
        var methods = new Dictionary<string, NativeFunction>();

        // Helper para definir funciones con metadatos
        void Def(string name, Func<Interpreter, List<LatinoObject>, LatinoObject> logic, int arity, string desc = "", string pams = "") 
        {
            var func = new NativeFunction(logic, arity);
            if (!string.IsNullOrEmpty(desc)) func.WithDocs(desc, pams);
            methods[name] = func;
        }

        // Helper de seguridad (Casting)
        LatinoList GetList(LatinoObject obj)
        {
            if (obj is LatinoList l) return l;
            throw new RuntimeError(null!, $"Se esperaba una lista, se recibió: {obj.GetType().Name}");
        }

        // --- FUNCIONES DE MODIFICACIÓN (In-Place) ---

        Def("agregar", (i, args) => {
            var list = GetList(args[0]);
            list.Elements.Add(args[1]);
            return list;
        }, 2, "Agrega un elemento al final de la lista.", "lista, elemento");

        Def("insertar", (i, args) => {
            var list = GetList(args[0]);
            var elem = args[1];
            var index = (int)((LatinoNumber)args[2]).Value;

            // FILOSOFÍA: Resiliencia en bordes (consistente con cadena.insertar)
            // Un índice fuera de rango no es un error — simplemente insertamos
            // al inicio o al final según corresponda. El programador obtiene
            // el resultado más sensato en lugar de una excepción inesperada.
            if (index < 0) index = 0;
            if (index > list.Elements.Count) index = list.Elements.Count;

            list.Elements.Insert(index, elem);
            return list;
        }, 3, "Inserta un elemento en una posición específica. Si el índice está fuera de rango, se ajusta al inicio o al final.", "lista, elemento, indice");

        Def("eliminar", (i, args) => {
            var list = GetList(args[0]);
            list.Elements.Remove(args[1]);
            return list;
        }, 2, "Elimina la primera ocurrencia de un elemento.", "lista, elemento");

        Def("eliminar_indice", (i, args) => {
            var list = GetList(args[0]);
            var index = (int)((LatinoNumber)args[1]).Value;

            // FILOSOFÍA: Fail-Fast en borrado (opuesto deliberado a insertar)
            // Insertar en un índice inválido tiene una interpretación sensata (bordes).
            // Borrar un índice inválido NO la tiene — el elemento simplemente no existe.
            // Fallar rápido aquí ayuda al programador a detectar bugs de lógica de índices.
            if (index < 0 || index >= list.Elements.Count)
                throw new RuntimeError(null!,
                    $"Índice fuera de rango: {index}. La lista tiene {list.Elements.Count} elemento(s). " +
                    $"Índices válidos: 0 a {list.Elements.Count - 1}.");

            list.Elements.RemoveAt(index);
            return list;
        }, 2, "Elimina el elemento en el índice indicado. Lanza error si el índice está fuera de rango.", "lista, indice");

        Def("extender", (i, args) => {
            var list = GetList(args[0]);
            var other = GetList(args[1]);
            list.Elements.AddRange(other.Elements);
            return list;
        }, 2, "Agrega los elementos de la segunda lista al final de la primera.", "lista_destino, lista_origen");

        Def("invertir", (i, args) => {
            var list = GetList(args[0]);
            list.Elements.Reverse();
            return list;
        }, 1, "Invierte el orden de la lista.", "lista");

        // --- FUNCIONES DE CONSULTA ---

        Def("longitud", (i, args) => {
            return new LatinoNumber(GetList(args[0]).Elements.Count);
        }, 1, "Devuelve la cantidad de elementos.", "lista");

        Def("dimensiones", (i, args) => {
            var list = GetList(args[0]);
            int filas = list.Elements.Count;

            if (filas == 0)
                return new LatinoTuple(new List<LatinoObject> { new LatinoNumber(0), new LatinoNumber(0) });

            if (list.Elements[0] is LatinoList primeraFila)
            {
                int columnas = primeraFila.Elements.Count;
                for (int r = 1; r < filas; r++)
                {
                    if (list.Elements[r] is not LatinoList fila || fila.Elements.Count != columnas)
                        throw new RuntimeError(null!, $"La matriz es irregular. La fila {r} tiene una cantidad de columnas distinta a la fila 0.");
                }
                return new LatinoTuple(new List<LatinoObject> { new LatinoNumber(filas), new LatinoNumber(columnas) });
            }

            return new LatinoTuple(new List<LatinoObject> { new LatinoNumber(filas), new LatinoNumber(0) });

        }, 1, "Retorna una tupla (filas, columnas) con las dimensiones de una matriz. Retorna (n, 0) para vectores 1D. Lanza error si la matriz es irregular.", "matriz");

        Def("diagonal", (i, args) => {
            var list = GetList(args[0]);
            int filas = list.Elements.Count;

            if (filas == 0) return new LatinoList(new List<LatinoObject>());

            if (list.Elements[0] is not LatinoList)
                throw new RuntimeError(null!, "lista.diagonal requiere una matriz (lista de listas).");

            int cols = ((LatinoList)list.Elements[0]).Elements.Count;
            int tam  = Math.Min(filas, cols);

            var resultado = new List<LatinoObject>(tam);
            for (int r = 0; r < tam; r++)
            {
                if (list.Elements[r] is not LatinoList fila) throw new RuntimeError(null!, $"La fila {r} no es una lista.");
                if (fila.Elements.Count <= r) throw new RuntimeError(null!, $"La fila {r} no tiene suficientes columnas.");
                resultado.Add(fila.Elements[r]);
            }
            return new LatinoList(resultado);

        }, 1, "Extrae la diagonal principal de una matriz como vector.", "matriz");

        Def("columna", (i, args) => {
            var list  = GetList(args[0]);
            int j     = (int)((LatinoNumber)args[1]).Value;
            int filas = list.Elements.Count;

            if (filas == 0) return new LatinoList(new List<LatinoObject>());

            if (list.Elements[0] is not LatinoList)
                throw new RuntimeError(null!, "lista.columna requiere una matriz (lista de listas).");

            int cols = ((LatinoList)list.Elements[0]).Elements.Count;

            if (j < 0 || j >= cols)
                throw new RuntimeError(null!, $"Índice de columna fuera de rango: {j}. Índices válidos: 0 a {cols - 1}.");

            var resultado = new List<LatinoObject>(filas);
            for (int r = 0; r < filas; r++)
            {
                if (list.Elements[r] is not LatinoList fila) throw new RuntimeError(null!, $"La fila {r} no es una lista.");
                if (fila.Elements.Count <= j) throw new RuntimeError(null!, $"La fila {r} no tiene la columna {j}.");
                resultado.Add(fila.Elements[j]);
            }
            return new LatinoList(resultado);

        }, 2, "Extrae la columna j de una matriz como vector.", "matriz, j");

        Def("contiene", (i, args) => {
            return new LatinoBoolean(GetList(args[0]).Elements.Contains(args[1]));
        }, 2, "Verifica si la lista contiene el elemento.", "lista, elemento");

        Def("indice", (i, args) => {
            return new LatinoNumber(GetList(args[0]).Elements.IndexOf(args[1]));
        }, 2, "Devuelve el índice del elemento o -1.", "lista, elemento");
        
        Def("encontrar", (i, args) => { // Alias
            return new LatinoNumber(GetList(args[0]).Elements.IndexOf(args[1]));
        }, 2, "Alias de indice.", "lista, elemento");

        // --- CREACIÓN Y TRANSFORMACIÓN (Retornan nueva lista/string) ---

        Def("crear", (i, args) => {
             int n = (int)((LatinoNumber)args[0]).Value;
             var l = new List<LatinoObject>();
             for(int k=0; k<n; k++) l.Add(LatinoNull.Instance);
             return new LatinoList(l);
        }, 1, "Crea una lista de tamaño N con nulos.", "tamaño");

        Def("concatenar", (i, args) => {
            var l1 = GetList(args[0]);
            var l2 = GetList(args[1]);
            var nueva = new List<LatinoObject>(l1.Elements);
            nueva.AddRange(l2.Elements);
            return new LatinoList(nueva);
        }, 2, "Retorna una nueva lista con la unión de dos listas.", "lista1, lista2");

        Def("separar", (i, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "lista.separar() requiere al menos la lista a convertir.");
            
            var list = GetList(args[0]);
            string sep = " ";
            if (args.Count > 1) sep = args[1].ToString();
            return new LatinoString(string.Join(sep, list.Elements.Select(e => e.ToString())));
        }, -1, "Convierte la lista a texto uniendo elementos con un separador.", "lista, [separador]");

        Def("identidad", (i, args) => {
            int n = (int)((LatinoNumber)args[0]).Value;
            if (n < 1) throw new RuntimeError(null!, $"El tamaño debe ser al menos 1 (recibido: {n}).");

            var filas = new List<LatinoObject>(n);
            for (int r = 0; r < n; r++)
            {
                var fila = new List<LatinoObject>(n);
                for (int c = 0; c < n; c++)
                    fila.Add(new LatinoNumber(r == c ? 1.0 : 0.0));
                filas.Add(new LatinoList(fila));
            }
            return new LatinoList(filas);
        }, 1, "Crea la matriz identidad n×n.", "n");

        Def("llenar", (i, args) => {
            int filas = (int)((LatinoNumber)args[0]).Value;
            int cols  = (int)((LatinoNumber)args[1]).Value;
            var valor = args[2];

            if (filas < 1) throw new RuntimeError(null!, $"Filas debe ser al menos 1 (recibido: {filas}).");
            if (cols < 1) throw new RuntimeError(null!, $"Columnas debe ser al menos 1 (recibido: {cols}).");

            var resultado = new List<LatinoObject>(filas);
            for (int r = 0; r < filas; r++)
            {
                var fila = new List<LatinoObject>(cols);
                for (int c = 0; c < cols; c++) fila.Add(valor);
                resultado.Add(new LatinoList(fila));
            }
            return new LatinoList(resultado);
        }, 3, "Crea una matriz de filas×columnas con todos los elementos iguales a valor.", "filas, columnas, valor");

        Def("submatriz", (i, args) => {
            var list       = GetList(args[0]);
            int filaInicio = (int)((LatinoNumber)args[1]).Value;
            int filaFin    = (int)((LatinoNumber)args[2]).Value;
            int colInicio  = (int)((LatinoNumber)args[3]).Value;
            int colFin     = (int)((LatinoNumber)args[4]).Value;

            int totalFilas = list.Elements.Count;
            if (totalFilas == 0) throw new RuntimeError(null!, "No se puede extraer una submatriz de una lista vacía.");
            if (list.Elements[0] is not LatinoList) throw new RuntimeError(null!, "lista.submatriz requiere una matriz.");

            int totalCols = ((LatinoList)list.Elements[0]).Elements.Count;

            if (filaInicio < 0 || filaFin >= totalFilas || filaInicio > filaFin)
                throw new RuntimeError(null!, $"Rango de filas inválido: [{filaInicio}, {filaFin}].");
            if (colInicio < 0 || colFin >= totalCols || colInicio > colFin)
                throw new RuntimeError(null!, $"Rango de columnas inválido: [{colInicio}, {colFin}].");

            var resultado = new List<LatinoObject>(filaFin - filaInicio + 1);
            for (int r = filaInicio; r <= filaFin; r++)
            {
                if (list.Elements[r] is not LatinoList filaOrigen) throw new RuntimeError(null!, $"La fila {r} no es una lista.");
                var filaNueva = new List<LatinoObject>(colFin - colInicio + 1);
                for (int c = colInicio; c <= colFin; c++)
                    filaNueva.Add(filaOrigen.Elements[c]);
                resultado.Add(new LatinoList(filaNueva));
            }
            return new LatinoList(resultado);
        }, 5, "Extrae un bloque rectangular de una matriz (índices inclusivos).", "matriz, filaInicio, filaFin, colInicio, colFin");

        // --- FUNCIONAL ---
        
        Def("transformar", (i, args) => {
            var list = GetList(args[0]);
            var func = args[1] as ICallable
                ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");

            // Pre-alocar capacidad exacta — el resultado siempre tiene el mismo tamaño que la entrada
            var newList  = new List<LatinoObject>(list.Elements.Count);
            // Object Pooling: reutilizar el contenedor de argumentos en cada iteración
            // Es seguro porque func.Call es síncrono y termina antes de la siguiente vuelta
            var funcArgs = new List<LatinoObject> { null! };

            foreach (var item in list.Elements)
            {
                funcArgs[0] = item;
                newList.Add(func.Call(i, funcArgs));
            }
            return new LatinoList(newList);
        }, 2, "Crea una nueva lista transformando cada elemento.", "lista, funcion");

        Def("filtrar", (i, args) => {
            var list = GetList(args[0]);
            var func = args[1] as ICallable
                ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");

            // Heurística de capacidad inicial: asume que al menos la mitad pasará el filtro
            var newList  = new List<LatinoObject>(Math.Max(4, list.Elements.Count / 2));
            var funcArgs = new List<LatinoObject> { null! };

            foreach (var item in list.Elements)
            {
                funcArgs[0] = item;
                if (i.IsTruthy(func.Call(i, funcArgs))) newList.Add(item);
            }
            return new LatinoList(newList);
        }, 2, "Crea una nueva lista con elementos que cumplen la condición.", "lista, funcion");

        // --- COMPARACIÓN LEXICOGRÁFICA ---
        Def("comparar", (i, args) => {
            var l1 = GetList(args[0]);
            var l2 = GetList(args[1]);
            
            int count = Math.Min(l1.Elements.Count, l2.Elements.Count);
            
            for (int k = 0; k < count; k++)
            {
                var o1 = l1.Elements[k];
                var o2 = l2.Elements[k];
                
                int res;
                // Intento de comparación numérica si ambos son números
                if (o1 is LatinoNumber n1 && o2 is LatinoNumber n2)
                {
                    res = n1.Value.CompareTo(n2.Value);
                }
                else
                {
                    // Fallback a comparación de cadena (Lexicográfica)
                    res = string.Compare(o1.ToString(), o2.ToString(), StringComparison.Ordinal);
                }

                if (res != 0) return new LatinoNumber(res); // -1 o 1
            }

            // Si llegamos aquí, los prefijos son iguales. Desempata la longitud.
            return new LatinoNumber(l1.Elements.Count.CompareTo(l2.Elements.Count));
        }, 2, "Compara dos listas elemento por elemento (-1, 0, 1).", "lista1, lista2");

        // --- ORDENAR ---
        Def("ordenar", (interp, args) => {
            if (args.Count == 0)
                throw new RuntimeError(null!, 
                    "lista.ordenar() requiere al menos la lista a ordenar.");
            
            var list = GetList(args[0]);
            if (args.Count > 1 && args[1] is ICallable comparador)
            {
                list.Elements.Sort((a, b) => {
                    var res = comparador.Call(interp, new List<LatinoObject> { a, b });
                    if (res is LatinoBoolean boolRes) return boolRes.Value ? -1 : 1;
                    if (res is LatinoNumber numRes) return (int)numRes.Value;
                    return 0;
                });
            }
            else
            {
                list.Elements.Sort((a, b) => {
                     if (a is LatinoNumber n1 && b is LatinoNumber n2) return n1.Value.CompareTo(n2.Value);
                     return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
                });
            }
            return list;
        }, -1, "Ordena la lista (opcionalmente con función comparadora).", "lista, [comparador]");

        // --- ESTADÍSTICAS Y AGREGACIÓN ---

        Def("sumar", (interp, args) => {
            var list = GetList(args[0]);

            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, "No se puede sumar una lista vacía.");

            double suma = 0;
            for (int i = 0; i < list.Elements.Count; i++)
            {
                var elem = list.Elements[i];
                if (elem is LatinoNumber num)
                {
                    suma += num.Value;
                }
                else
                {
                    throw new RuntimeError(null!,
                        $"El elemento en la posición {i} no es un número (tipo: {elem.GetType().Name}).");
                }
            }

            return new LatinoNumber(suma);

        }, 1, "Calcula la suma de todos los elementos numéricos de la lista.", "lista");

        Def("promedio", (interp, args) => {
            var list = GetList(args[0]);
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, "No se puede calcular el promedio de una lista vacía.");

            double suma = 0;
            for (int i = 0; i < list.Elements.Count; i++)
            {
                var elem = list.Elements[i];
                if (elem is LatinoNumber num)
                {
                    suma += num.Value;
                }
                else
                {
                    throw new RuntimeError(null!,
                        $"El elemento en la posición {i} no es un número (tipo: {elem.GetType().Name}).");
                }
            }
            return new LatinoNumber(suma / list.Elements.Count);
        }, 1, "Calcula el promedio aritmético de los elementos numéricos.", "lista");

        Def("contar", (interp, args) => {
            var list = GetList(args[0]);
            var elemento = args[1];
            int count = list.Elements.Count(e => e.Equals(elemento));
            return new LatinoNumber(count);
        }, 2, "Cuenta cuántas veces aparece el elemento en la lista.", "lista, elemento");

        // --- MANIPULACIÓN Y UTILIDAD ---

        Def("unicos", (interp, args) => {
            var list = GetList(args[0]);
            var seen = new HashSet<LatinoObject>(LatinoObjectComparer.Instance);
            var unique = new List<LatinoObject>();
            foreach (var elem in list.Elements)
            {
                if (seen.Add(elem)) unique.Add(elem);
            }
            return new LatinoList(unique);
        }, 1, "Retorna una nueva lista eliminando elementos duplicados (preserva orden).", "lista");

        Def("aleatorio", (interp, args) => {
            var list = GetList(args[0]);
            if (list.Elements.Count == 0)
                throw new RuntimeError(null!, "No se puede seleccionar un elemento aleatorio de una lista vacía.");
            
            int index = Random.Shared.Next(list.Elements.Count);
            return list.Elements[index];
        }, 1, "Retorna un elemento aleatorio de la lista.", "lista");

        Def("tomar", (interp, args) => {
            var list = GetList(args[0]);
            if (args[1] is not LatinoNumber num || num.Value < 0)
                throw new RuntimeError(null!, "El número de elementos debe ser un entero no negativo.");
            
            int n = (int)num.Value;
            var result = list.Elements.Take(n).ToList();
            return new LatinoList(result);
        }, 2, "Retorna una nueva lista con los primeros N elementos.", "lista, n");

        Def("saltar", (interp, args) => {
            var list = GetList(args[0]);
            if (args[1] is not LatinoNumber num || num.Value < 0)
                throw new RuntimeError(null!, "El número de elementos debe ser un entero no negativo.");
            
            int n = (int)num.Value;
            var result = list.Elements.Skip(n).ToList();
            return new LatinoList(result);
        }, 2, "Retorna una nueva lista omitiendo los primeros N elementos.", "lista, n");

        Def("zip", (interp, args) => {
            var listas = args.Select(GetList).ToList();
            if (listas.Count == 0) return new LatinoList(new List<LatinoObject>());

            int minLen = listas.Min(l => l.Elements.Count);
            var resultado = new List<LatinoObject>();

            for (int i = 0; i < minLen; i++)
            {
                var tupla = new List<LatinoObject>();
                foreach (var lista in listas)
                {
                    tupla.Add(lista.Elements[i]);
                }
                resultado.Add(new LatinoList(tupla));
            }
            return new LatinoList(resultado);
        }, -1, "Combina múltiples listas en tuplas. Longitud = lista más corta.", "lista1, lista2, ...");

        Def("aplanar", (interp, args) => {
            var list = GetList(args[0]);
            var resultado = new List<LatinoObject>();

            void AplanarRecursivo(LatinoList origen, List<LatinoObject> destino)
            {
                foreach (var elem in origen.Elements)
                {
                    if (elem is LatinoList subLista)
                        AplanarRecursivo(subLista, destino);
                    else
                        destino.Add(elem);
                }
            }

            AplanarRecursivo(list, resultado);
            return new LatinoList(resultado);
        }, 1, "Aplana recursivamente listas anidadas.", "lista");

        // --- PROGRAMACIÓN FUNCIONAL AVANZADA ---

        Def("reducir", (interp, args) => {
            var list = GetList(args[0]);
            if (args.Count < 2) throw new RuntimeError(null!, "Se esperaba una función para reducir.");
            var func = args[1] as ICallable ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");

            if (list.Elements.Count == 0 && args.Count < 3)
                throw new RuntimeError(null!, "No se puede reducir una lista vacía sin semilla.");

            LatinoObject acumulador;
            int inicio;

            if (args.Count == 3)
            {
                acumulador = args[2];
                inicio = 0;
            }
            else
            {
                if (list.Elements.Count == 0) throw new RuntimeError(null!, "No se puede reducir una lista vacía sin semilla.");
                acumulador = list.Elements[0];
                inicio = 1;
            }

            // Object Pooling para dos argumentos.
            // IMPORTANTE: El pool es seguro aquí porque `acumulador` se actualiza
            // con el *valor retornado* por func.Call, no con la lista pooled.
            // Las lambdas que capturen `acum` por clausura recibirán el valor
            // correcto porque el fast-path retorna el objeto evaluado, no funcArgs.
            var funcArgs = new List<LatinoObject> { null!, null! };

            for (int i = inicio; i < list.Elements.Count; i++)
            {
                funcArgs[0] = acumulador;
                funcArgs[1] = list.Elements[i];
                acumulador  = func.Call(interp, funcArgs);
            }

            return acumulador;
        }, -1, "Reduce la lista a un valor usando función acumuladora f(acum, val).", "lista, funcion, [semilla]");

        Def("todos", (interp, args) => {
            var list = GetList(args[0]);
            var func = args[1] as ICallable ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");

            var funcArgs = new List<LatinoObject> { null! };
            foreach (var elem in list.Elements)
            {
                funcArgs[0] = elem;
                var res = func.Call(interp, funcArgs);
                if (!interp.IsTruthy(res)) return new LatinoBoolean(false);
            }
            return new LatinoBoolean(true);
        }, 2, "Retorna verdadero si todos los elementos cumplen la condición.", "lista, funcion");

        Def("alguno", (interp, args) => {
            var list = GetList(args[0]);
            var func = args[1] as ICallable ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");

            var funcArgs = new List<LatinoObject> { null! };
            foreach (var elem in list.Elements)
            {
                funcArgs[0] = elem;
                var res = func.Call(interp, funcArgs);
                if (interp.IsTruthy(res)) return new LatinoBoolean(true);
            }
            return new LatinoBoolean(false);
        }, 2, "Retorna verdadero si al menos un elemento cumple la condición.", "lista, funcion");

        Def("particionar", (interp, args) => {
            var list = GetList(args[0]);
            var func = args[1] as ICallable ?? throw new RuntimeError(null!, "El segundo argumento debe ser una función.");
            
            var cumple = new List<LatinoObject>(list.Elements.Count / 2);
            var noCumple = new List<LatinoObject>(list.Elements.Count / 2);

            var funcArgs = new List<LatinoObject> { null! };
            foreach (var elem in list.Elements)
            {
                funcArgs[0] = elem;
                var resultado = func.Call(interp, funcArgs);
                if (interp.IsTruthy(resultado))
                    cumple.Add(elem);
                else
                    noCumple.Add(elem);
            }

            var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
            {
                { new LatinoString("verdadero"), new LatinoList(cumple) },
                { new LatinoString("falso"), new LatinoList(noCumple) }
            };

            return new LatinoDictionary(dict);
        }, 2, "Divide la lista en dos grupos según condición. Retorna dic con claves 'verdadero' y 'falso'.", "lista, funcion");
        
        // Registrar módulo protegido
        var modulo = new LatinoDictionary(ConvertMap(methods));
        modulo.MakeReadOnly();
        interpreter.Globals.Define("lista", modulo);
    }
    
    private static Dictionary<LatinoObject, LatinoObject> ConvertMap(Dictionary<string, NativeFunction> input) {
        var output = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach(var kv in input) output[new LatinoString(kv.Key)] = kv.Value;
        return output;
    }
}
