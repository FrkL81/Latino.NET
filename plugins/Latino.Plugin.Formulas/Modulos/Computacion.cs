using System;
using System.Collections.Generic;
using Latino.Core.Types;
using Latino.Core.Runtime;

namespace Latino.Plugin.Formulas;

public static class ComputacionFormulas
{
    [LatinoExtension("computacion.entropiaShannon",
            Args = "lista_probabilidades",
            Help = "Entropía de Shannon de una fuente de información (en bits).\n\nFórmula: H = -Σ pᵢ·log₂(pᵢ)")]
        public static LatinoObject EntropiaShannon(List<LatinoObject> args)
        {
            if (args.Count == 0 || args[0] is not LatinoList lista)
                throw new RuntimeError(null!, "Requiere lista de probabilidades (deben sumar 1.0).");
            if (lista.Elements.Count < 2)
                throw new RuntimeError(null!, "Se necesitan al menos 2 símbolos para calcular entropía.");

            double suma = 0, H = 0;
            var probs = new List<double>();
            foreach (var e in lista.Elements)
            {
                if (e is not LatinoNumber num) throw new RuntimeError(null!, "Todos los elementos deben ser probabilidades (números entre 0 y 1).");
                double p = num.Value;
                if (p < 0 || p > 1) throw new RuntimeError(null!, $"Probabilidad inválida: {p}. Deben estar entre 0 y 1.");
                probs.Add(p);
                suma += p;
            }
            if (Math.Abs(suma - 1.0) > 0.001) throw new RuntimeError(null!, $"Las probabilidades deben sumar 1.0 (suma actual: {suma:F4}).");

            foreach (var p in probs)
                if (p > 0) H -= p * Math.Log(p, 2);

            double Hmax  = Math.Log(probs.Count, 2);
            double efic  = Hmax > 0 ? H / Hmax * 100 : 100;

            string descripcion = efic > 95 ? "Fuente casi aleatoria (alta entropía)" :
                                 efic > 60 ? "Entropía moderada" :
                                             "Fuente predecible (baja entropía — buena para compresión)";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("entropia_maxima_bits"),   new LatinoNumber(Hmax) },
                { new LatinoString("eficiencia_porcentaje"),  new LatinoNumber(efic) },
                { new LatinoString("descripcion"),            new LatinoString(descripcion) }
            });
            return Respuesta.Crear(H, "bits/símbolo", "H = -Σ pᵢ·log₂(pᵢ)", extras);
        }

    [LatinoExtension("computacion.probabilidadColisionHash",
            Args = "num_elementos, tamano_tabla",
            Help = "Probabilidad aproximada de al menos una colisión en una tabla hash (paradoja del cumpleaños).\n\nFórmula: P ≈ 1 - e^(-n²/(2m))")]
        public static LatinoObject ProbabilidadColisionHash(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere num_elementos (n) y tamaño de tabla hash (m).");
            double n = ((LatinoNumber)args[0]).Value;
            double m = ((LatinoNumber)args[1]).Value;
            if (n < 1 || m < 1) throw new RuntimeError(null!, "n y m deben ser al menos 1.");
            if (n > m * 2) throw new RuntimeError(null!, "Con más elementos que slots, la colisión es prácticamente segura. Aumenta el tamaño de la tabla.");
            double P = 1.0 - Math.Exp(-(n * n) / (2.0 * m));

            string riesgo = P < 0.01 ? "Riesgo muy bajo" :
                            P < 0.10 ? "Riesgo bajo" :
                            P < 0.50 ? "Riesgo moderado — considera tabla más grande" :
                                       "Riesgo alto — tabla insuficiente para este volumen";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("porcentaje"),   new LatinoNumber(P * 100) },
                { new LatinoString("riesgo"),       new LatinoString(riesgo) },
                { new LatinoString("nota"),         new LatinoString("Basado en la paradoja del cumpleaños. Válido cuando n << m.") }
            });
            return Respuesta.Crear(P, "probabilidad (0-1)", "P ≈ 1 - e^(-n²/2m)", extras);
        }

    [LatinoExtension("computacion.tiempoBusqueda",
            Args = "n_elementos, tipo_algoritmo",
            Help = "Operaciones estimadas para buscar en n elementos. Tipos: 'lineal', 'binaria', 'hash', 'arbol_b'.\n\nReferencia Big-O de cada tipo.")]
        public static LatinoObject TiempoBusqueda(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere n_elementos y tipo_algoritmo ('lineal', 'binaria', 'hash', 'arbol_b').");
            double n    = ((LatinoNumber)args[0]).Value;
            string tipo = args[1].ToString().ToLower().Trim();
            if (n < 1) throw new RuntimeError(null!, "El número de elementos debe ser al menos 1.");

            double ops;
            string bigO, descripcion;
            switch (tipo)
            {
                case "lineal":
                    ops = n; bigO = "O(n)";
                    descripcion = "Recorre elemento por elemento. Simple pero lento para n grandes.";
                    break;
                case "binaria":
                    ops = Math.Log(n, 2); bigO = "O(log n)";
                    descripcion = "Requiere datos ordenados. Muy eficiente.";
                    break;
                case "hash":
                    ops = 1; bigO = "O(1) amortizado";
                    descripcion = "Caso promedio. En el peor caso (muchas colisiones): O(n).";
                    break;
                case "arbol_b":
                    ops = Math.Log(n, 2); bigO = "O(log n)";
                    descripcion = "Eficiente para bases de datos en disco. Constante mayor que binaria en memoria.";
                    break;
                default:
                    throw new RuntimeError(null!, "Tipo inválido. Usa: 'lineal', 'binaria', 'hash', o 'arbol_b'.");
            }

            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("big_o"),        new LatinoString(bigO) },
                { new LatinoString("descripcion"),  new LatinoString(descripcion) },
                { new LatinoString("algoritmo"),    new LatinoString(tipo) }
            });
            return Respuesta.Crear(ops, "operaciones estimadas", $"Complejidad: {bigO}", extras);
        }

    [LatinoExtension("computacion.productoAnchoBandaLatencia",
            Args = "ancho_banda_bps, latencia_s",
            Help = "Producto ancho de banda × latencia (Bandwidth-Delay Product).\nIndica cuántos bits 'están en vuelo' en un enlace en todo momento.\n\nFórmula: BDP = BW · RTT")]
        public static LatinoObject ProductoAnchoBandaLatencia(List<LatinoObject> args)
        {
            if (args.Count < 2) throw new RuntimeError(null!, "Requiere ancho_banda (bps) y latencia (s, RTT completo).");
            double bw  = ((LatinoNumber)args[0]).Value;
            double rtt = ((LatinoNumber)args[1]).Value;
            if (bw <= 0 || rtt <= 0) throw new RuntimeError(null!, "El ancho de banda y la latencia deben ser positivos.");
            double bdp = bw * rtt;
            double bdpKB = bdp / 8.0 / 1024.0;

            string implicacion = bdpKB < 1    ? "Enlace de latencia baja — ventanas TCP pequeñas son suficientes" :
                                 bdpKB < 1000 ? $"Se necesita una ventana TCP de al menos {bdpKB:F1} KB para saturar el enlace" :
                                                $"Enlace de alta latencia — requiere ventana TCP de {bdpKB/1024:F1} MB (TCP tuning crítico)";
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("bdp_KB"),       new LatinoNumber(bdpKB) },
                { new LatinoString("implicacion"),  new LatinoString(implicacion) }
            });
            return Respuesta.Crear(bdp, "bits", "BDP = BW · RTT", extras);
        }

    [LatinoExtension("computacion.bitsNecesarios",
            Args = "num_valores_posibles",
            Help = "Cantidad mínima de bits necesarios para representar n valores distintos.\n\nFórmula: bits = ⌈log₂(n)⌉")]
        public static LatinoObject BitsNecesarios(List<LatinoObject> args)
        {
            if (args.Count < 1) throw new RuntimeError(null!, "Requiere el número de valores posibles (n >= 1).");
            double n = ((LatinoNumber)args[0]).Value;
            if (n < 1) throw new RuntimeError(null!, "Debe haber al menos 1 valor posible.");
            int bits = n <= 1 ? 0 : (int)Math.Ceiling(Math.Log(n, 2));
            double capacidad = Math.Pow(2, bits);
            var extras = new LatinoDictionary(new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance) {
                { new LatinoString("valores_representables"), new LatinoNumber(capacidad) },
                { new LatinoString("bits_desperdiciados"),    new LatinoNumber(capacidad - n) }
            });
            return Respuesta.Crear(bits, "bits", "bits = ⌈log₂(n)⌉", extras);
        }

}
