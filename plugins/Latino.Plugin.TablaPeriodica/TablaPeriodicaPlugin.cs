using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Plugin.TablaPeriodica;

public static class TablaPeriodicaPlugin
{
    // Diccionario estático que ignora mayúsculas/minúsculas automáticamente.
    // Indexaremos los elementos tanto por su Nombre como por su Símbolo.
    private static readonly Dictionary<string, LatinoDictionary> _db = new(StringComparer.OrdinalIgnoreCase);

    // =========================================================================
    // INICIALIZACIÓN DE LA BASE DE DATOS (Rellenar por el LLM)
    // =========================================================================
static TablaPeriodicaPlugin()
{
    // Período 1
    AgregarElemento(1, "H", "Hidrógeno", 1.008, "No metal", "1s1");
    AgregarElemento(2, "He", "Helio", 4.0026, "Gas noble", "1s2");
    
    // Período 2
    AgregarElemento(3, "Li", "Litio", 6.94, "Metal alcalino", "[He] 2s1");
    AgregarElemento(4, "Be", "Berilio", 9.0122, "Metal alcalinotérreo", "[He] 2s2");
    AgregarElemento(5, "B", "Boro", 10.81, "Metaloide", "[He] 2s2 2p1");
    AgregarElemento(6, "C", "Carbono", 12.011, "No metal", "[He] 2s2 2p2");
    AgregarElemento(7, "N", "Nitrógeno", 14.007, "No metal", "[He] 2s2 2p3");
    AgregarElemento(8, "O", "Oxígeno", 15.999, "No metal", "[He] 2s2 2p4");
    AgregarElemento(9, "F", "Flúor", 18.998, "Halógeno", "[He] 2s2 2p5");
    AgregarElemento(10, "Ne", "Neón", 20.180, "Gas noble", "[He] 2s2 2p6");
    
    // Período 3
    AgregarElemento(11, "Na", "Sodio", 22.990, "Metal alcalino", "[Ne] 3s1");
    AgregarElemento(12, "Mg", "Magnesio", 24.305, "Metal alcalinotérreo", "[Ne] 3s2");
    AgregarElemento(13, "Al", "Aluminio", 26.982, "Metal del bloque p", "[Ne] 3s2 3p1");
    AgregarElemento(14, "Si", "Silicio", 28.085, "Metaloide", "[Ne] 3s2 3p2");
    AgregarElemento(15, "P", "Fósforo", 30.974, "No metal", "[Ne] 3s2 3p3");
    AgregarElemento(16, "S", "Azufre", 32.06, "No metal", "[Ne] 3s2 3p4");
    AgregarElemento(17, "Cl", "Cloro", 35.45, "Halógeno", "[Ne] 3s2 3p5");
    AgregarElemento(18, "Ar", "Argón", 39.948, "Gas noble", "[Ne] 3s2 3p6");
    
    // Período 4
    AgregarElemento(19, "K", "Potasio", 39.098, "Metal alcalino", "[Ar] 4s1");
    AgregarElemento(20, "Ca", "Calcio", 40.078, "Metal alcalinotérreo", "[Ar] 4s2");
    AgregarElemento(21, "Sc", "Escandio", 44.956, "Metal de transición", "[Ar] 3d1 4s2");
    AgregarElemento(22, "Ti", "Titanio", 47.867, "Metal de transición", "[Ar] 3d2 4s2");
    AgregarElemento(23, "V", "Vanadio", 50.942, "Metal de transición", "[Ar] 3d3 4s2");
    AgregarElemento(24, "Cr", "Cromo", 51.996, "Metal de transición", "[Ar] 3d5 4s1");
    AgregarElemento(25, "Mn", "Manganeso", 54.938, "Metal de transición", "[Ar] 3d5 4s2");
    AgregarElemento(26, "Fe", "Hierro", 55.845, "Metal de transición", "[Ar] 3d6 4s2");
    AgregarElemento(27, "Co", "Cobalto", 58.933, "Metal de transición", "[Ar] 3d7 4s2");
    AgregarElemento(28, "Ni", "Níquel", 58.693, "Metal de transición", "[Ar] 3d8 4s2");
    AgregarElemento(29, "Cu", "Cobre", 63.546, "Metal de transición", "[Ar] 3d10 4s1");
    AgregarElemento(30, "Zn", "Zinc", 65.38, "Metal de transición", "[Ar] 3d10 4s2");
    AgregarElemento(31, "Ga", "Galio", 69.723, "Metal del bloque p", "[Ar] 3d10 4s2 4p1");
    AgregarElemento(32, "Ge", "Germanio", 72.63, "Metaloide", "[Ar] 3d10 4s2 4p2");
    AgregarElemento(33, "As", "Arsénico", 74.922, "Metaloide", "[Ar] 3d10 4s2 4p3");
    AgregarElemento(34, "Se", "Selenio", 78.96, "No metal", "[Ar] 3d10 4s2 4p4");
    AgregarElemento(35, "Br", "Bromo", 79.904, "Halógeno", "[Ar] 3d10 4s2 4p5");
    AgregarElemento(36, "Kr", "Kriptón", 83.798, "Gas noble", "[Ar] 3d10 4s2 4p6");
    
    // Período 5
    AgregarElemento(37, "Rb", "Rubidio", 85.468, "Metal alcalino", "[Kr] 5s1");
    AgregarElemento(38, "Sr", "Estroncio", 87.62, "Metal alcalinotérreo", "[Kr] 5s2");
    AgregarElemento(39, "Y", "Itrio", 88.906, "Metal de transición", "[Kr] 4d1 5s2");
    AgregarElemento(40, "Zr", "Zirconio", 91.224, "Metal de transición", "[Kr] 4d2 5s2");
    AgregarElemento(41, "Nb", "Niobio", 92.906, "Metal de transición", "[Kr] 4d4 5s1");
    AgregarElemento(42, "Mo", "Molibdeno", 95.95, "Metal de transición", "[Kr] 4d5 5s1");
    AgregarElemento(43, "Tc", "Tecnecio", 98.0, "Metal de transición", "[Kr] 4d5 5s2");
    AgregarElemento(44, "Ru", "Rutenio", 101.07, "Metal de transición", "[Kr] 4d7 5s1");
    AgregarElemento(45, "Rh", "Rodio", 102.91, "Metal de transición", "[Kr] 4d8 5s1");
    AgregarElemento(46, "Pd", "Paladio", 106.42, "Metal de transición", "[Kr] 4d10");
    AgregarElemento(47, "Ag", "Plata", 107.87, "Metal de transición", "[Kr] 4d10 5s1");
    AgregarElemento(48, "Cd", "Cadmio", 112.41, "Metal de transición", "[Kr] 4d10 5s2");
    AgregarElemento(49, "In", "Indio", 114.82, "Metal del bloque p", "[Kr] 4d10 5s2 5p1");
    AgregarElemento(50, "Sn", "Estaño", 118.71, "Metal del bloque p", "[Kr] 4d10 5s2 5p2");
    AgregarElemento(51, "Sb", "Antimonio", 121.76, "Metaloide", "[Kr] 4d10 5s2 5p3");
    AgregarElemento(52, "Te", "Telurio", 127.60, "Metaloide", "[Kr] 4d10 5s2 5p4");
    AgregarElemento(53, "I", "Yodo", 126.90, "Halógeno", "[Kr] 4d10 5s2 5p5");
    AgregarElemento(54, "Xe", "Xenón", 131.29, "Gas noble", "[Kr] 4d10 5s2 5p6");
    
    // Período 6
    AgregarElemento(55, "Cs", "Cesio", 132.91, "Metal alcalino", "[Xe] 6s1");
    AgregarElemento(56, "Ba", "Bario", 137.33, "Metal alcalinotérreo", "[Xe] 6s2");
    AgregarElemento(57, "La", "Lantano", 138.91, "Lantánido", "[Xe] 5d1 6s2");
    AgregarElemento(58, "Ce", "Cerio", 140.12, "Lantánido", "[Xe] 4f1 5d1 6s2");
    AgregarElemento(59, "Pr", "Praseodimio", 140.91, "Lantánido", "[Xe] 4f3 6s2");
    AgregarElemento(60, "Nd", "Neodimio", 144.24, "Lantánido", "[Xe] 4f4 6s2");
    AgregarElemento(61, "Pm", "Prometio", 145.0, "Lantánido", "[Xe] 4f5 6s2");
    AgregarElemento(62, "Sm", "Samario", 150.36, "Lantánido", "[Xe] 4f6 6s2");
    AgregarElemento(63, "Eu", "Europio", 151.96, "Lantánido", "[Xe] 4f7 6s2");
    AgregarElemento(64, "Gd", "Gadolinio", 157.25, "Lantánido", "[Xe] 4f7 5d1 6s2");
    AgregarElemento(65, "Tb", "Terbio", 158.93, "Lantánido", "[Xe] 4f9 6s2");
    AgregarElemento(66, "Dy", "Disprosio", 162.50, "Lantánido", "[Xe] 4f10 6s2");
    AgregarElemento(67, "Ho", "Holmio", 164.93, "Lantánido", "[Xe] 4f11 6s2");
    AgregarElemento(68, "Er", "Erbio", 167.26, "Lantánido", "[Xe] 4f12 6s2");
    AgregarElemento(69, "Tm", "Tulio", 168.93, "Lantánido", "[Xe] 4f13 6s2");
    AgregarElemento(70, "Yb", "Iterbio", 173.04, "Lantánido", "[Xe] 4f14 6s2");
    AgregarElemento(71, "Lu", "Lutecio", 174.97, "Lantánido", "[Xe] 4f14 5d1 6s2");
    AgregarElemento(72, "Hf", "Hafnio", 178.49, "Metal de transición", "[Xe] 4f14 5d2 6s2");
    AgregarElemento(73, "Ta", "Tantalio", 180.95, "Metal de transición", "[Xe] 4f14 5d3 6s2");
    AgregarElemento(74, "W", "Wolframio", 183.84, "Metal de transición", "[Xe] 4f14 5d4 6s2");
    AgregarElemento(75, "Re", "Renio", 186.21, "Metal de transición", "[Xe] 4f14 5d5 6s2");
    AgregarElemento(76, "Os", "Osmio", 190.23, "Metal de transición", "[Xe] 4f14 5d6 6s2");
    AgregarElemento(77, "Ir", "Iridio", 192.22, "Metal de transición", "[Xe] 4f14 5d7 6s2");
    AgregarElemento(78, "Pt", "Platino", 195.08, "Metal de transición", "[Xe] 4f14 5d9 6s1");
    AgregarElemento(79, "Au", "Oro", 196.97, "Metal de transición", "[Xe] 4f14 5d10 6s1");
    AgregarElemento(80, "Hg", "Mercurio", 200.59, "Metal de transición", "[Xe] 4f14 5d10 6s2");
    AgregarElemento(81, "Tl", "Talio", 204.38, "Metal del bloque p", "[Xe] 4f14 5d10 6s2 6p1");
    AgregarElemento(82, "Pb", "Plomo", 207.2, "Metal del bloque p", "[Xe] 4f14 5d10 6s2 6p2");
    AgregarElemento(83, "Bi", "Bismuto", 208.98, "Metal del bloque p", "[Xe] 4f14 5d10 6s2 6p3");
    AgregarElemento(84, "Po", "Polonio", 209.0, "Metal del bloque p", "[Xe] 4f14 5d10 6s2 6p4");
    AgregarElemento(85, "At", "Astato", 210.0, "Halógeno", "[Xe] 4f14 5d10 6s2 6p5");
    AgregarElemento(86, "Rn", "Radón", 222.0, "Gas noble", "[Xe] 4f14 5d10 6s2 6p6");
    
    // Período 7
    AgregarElemento(87, "Fr", "Francio", 223.0, "Metal alcalino", "[Rn] 7s1");
    AgregarElemento(88, "Ra", "Radio", 226.0, "Metal alcalinotérreo", "[Rn] 7s2");
    AgregarElemento(89, "Ac", "Actinio", 227.0, "Actínido", "[Rn] 6d1 7s2");
    AgregarElemento(90, "Th", "Torio", 232.04, "Actínido", "[Rn] 6d2 7s2");
    AgregarElemento(91, "Pa", "Protactinio", 231.04, "Actínido", "[Rn] 5f2 6d1 7s2");
    AgregarElemento(92, "U", "Uranio", 238.03, "Actínido", "[Rn] 5f3 6d1 7s2");
    AgregarElemento(93, "Np", "Neptunio", 237.0, "Actínido", "[Rn] 5f4 6d1 7s2");
    AgregarElemento(94, "Pu", "Plutonio", 244.0, "Actínido", "[Rn] 5f6 7s2");
    AgregarElemento(95, "Am", "Americio", 243.0, "Actínido", "[Rn] 5f7 7s2");
    AgregarElemento(96, "Cm", "Curio", 247.0, "Actínido", "[Rn] 5f7 6d1 7s2");
    AgregarElemento(97, "Bk", "Berkelio", 247.0, "Actínido", "[Rn] 5f9 7s2");
    AgregarElemento(98, "Cf", "Californio", 251.0, "Actínido", "[Rn] 5f10 7s2");
    AgregarElemento(99, "Es", "Einstenio", 252.0, "Actínido", "[Rn] 5f11 7s2");
    AgregarElemento(100, "Fm", "Fermio", 257.0, "Actínido", "[Rn] 5f12 7s2");
    AgregarElemento(101, "Md", "Mendelevio", 258.0, "Actínido", "[Rn] 5f13 7s2");
    AgregarElemento(102, "No", "Nobelio", 259.0, "Actínido", "[Rn] 5f14 7s2");
    AgregarElemento(103, "Lr", "Lawrencio", 266.0, "Actínido", "[Rn] 5f14 7s2 7p1");
    AgregarElemento(104, "Rf", "Rutherfordio", 267.0, "Metal de transición", "[Rn] 5f14 6d2 7s2");
    AgregarElemento(105, "Db", "Dubnio", 268.0, "Metal de transición", "[Rn] 5f14 6d3 7s2");
    AgregarElemento(106, "Sg", "Seaborgio", 269.0, "Metal de transición", "[Rn] 5f14 6d4 7s2");
    AgregarElemento(107, "Bh", "Bohrio", 270.0, "Metal de transición", "[Rn] 5f14 6d5 7s2");
    AgregarElemento(108, "Hs", "Hassio", 269.0, "Metal de transición", "[Rn] 5f14 6d6 7s2");
    AgregarElemento(109, "Mt", "Meitnerio", 278.0, "Metal de transición", "[Rn] 5f14 6d7 7s2");
    AgregarElemento(110, "Ds", "Darmstatio", 281.0, "Metal de transición", "[Rn] 5f14 6d8 7s2");
    AgregarElemento(111, "Rg", "Roentgenio", 282.0, "Metal de transición", "[Rn] 5f14 6d9 7s2");
    AgregarElemento(112, "Cn", "Copernicio", 285.0, "Metal de transición", "[Rn] 5f14 6d10 7s2");
    AgregarElemento(113, "Nh", "Nihonio", 286.0, "Metal del bloque p", "[Rn] 5f14 6d10 7s2 7p1");
    AgregarElemento(114, "Fl", "Flerovio", 289.0, "Metal del bloque p", "[Rn] 5f14 6d10 7s2 7p2");
    AgregarElemento(115, "Mc", "Moscovio", 290.0, "Metal del bloque p", "[Rn] 5f14 6d10 7s2 7p3");
    AgregarElemento(116, "Lv", "Livermorio", 293.0, "Metal del bloque p", "[Rn] 5f14 6d10 7s2 7p4");
    AgregarElemento(117, "Ts", "Teneso", 294.0, "Halógeno", "[Rn] 5f14 6d10 7s2 7p5");
    AgregarElemento(118, "Og", "Oganesón", 294.0, "Gas noble", "[Rn] 5f14 6d10 7s2 7p6");

    // --- Propiedades de estado (fusión y ebullición en °C) ---
    AgregarPropiedades("H",  -259.16, -252.88);
    AgregarPropiedades("He", -272.20, -268.93);
    AgregarPropiedades("Li",  180.50, 1342.0);
    AgregarPropiedades("Be", 1287.0,  2470.0);
    AgregarPropiedades("B",  2075.0,  4000.0);
    AgregarPropiedades("C",  3550.0,  4827.0);
    AgregarPropiedades("N",  -210.00, -195.79);
    AgregarPropiedades("O",  -218.79, -182.96);
    AgregarPropiedades("F",  -219.67, -188.12);
    AgregarPropiedades("Ne", -248.59, -246.08);
    AgregarPropiedades("Na",   97.79,  882.94);
    AgregarPropiedades("Mg",  650.0,  1091.0);
    AgregarPropiedades("Al",  660.32, 2519.0);
    AgregarPropiedades("Si", 1414.0,  3265.0);
    AgregarPropiedades("P",    44.15,  280.5);
    AgregarPropiedades("S",   115.21,  444.72);
    AgregarPropiedades("Cl", -101.5,   -34.04);
    AgregarPropiedades("Ar", -189.35, -185.85);
    AgregarPropiedades("K",    63.38,  759.0);
    AgregarPropiedades("Ca",  842.0,  1484.0);
    AgregarPropiedades("Sc", 1541.0,  2836.0);
    AgregarPropiedades("Ti", 1668.0,  3287.0);
    AgregarPropiedades("V",  1910.0,  3407.0);
    AgregarPropiedades("Cr", 1907.0,  2671.0);
    AgregarPropiedades("Mn", 1246.0,  2061.0);
    AgregarPropiedades("Fe", 1538.0,  2861.0);
    AgregarPropiedades("Co", 1495.0,  2927.0);
    AgregarPropiedades("Ni", 1455.0,  2913.0);
    AgregarPropiedades("Cu", 1084.62, 2562.0);
    AgregarPropiedades("Zn",  419.53,  907.0);
    AgregarPropiedades("Ga",   29.76, 2229.0);
    AgregarPropiedades("Ge",  938.25, 2833.0);
    AgregarPropiedades("As",  817.0,   614.0);
    AgregarPropiedades("Se",  221.0,   685.0);
    AgregarPropiedades("Br",   -7.25,   58.8);
    AgregarPropiedades("Kr", -157.36, -153.22);
    AgregarPropiedades("Rb",   39.30,  688.0);
    AgregarPropiedades("Sr",  777.0,  1382.0);
    AgregarPropiedades("Y",  1526.0,  3345.0);
    AgregarPropiedades("Zr", 1855.0,  4409.0);
    AgregarPropiedades("Nb", 2477.0,  4744.0);
    AgregarPropiedades("Mo", 2623.0,  4639.0);
    AgregarPropiedades("Tc", 2157.0,  4265.0);
    AgregarPropiedades("Ru", 2334.0,  4150.0);
    AgregarPropiedades("Rh", 1964.0,  3695.0);
    AgregarPropiedades("Pd", 1554.9,  2963.0);
    AgregarPropiedades("Ag",  961.78, 2162.0);
    AgregarPropiedades("Cd",  321.07,  767.0);
    AgregarPropiedades("In",  156.60, 2072.0);
    AgregarPropiedades("Sn",  231.93, 2602.0);
    AgregarPropiedades("Sb",  630.63, 1587.0);
    AgregarPropiedades("Te",  449.51,  988.0);
    AgregarPropiedades("I",   113.7,   184.3);
    AgregarPropiedades("Xe", -111.75, -108.12);
    AgregarPropiedades("Cs",   28.44,  671.0);
    AgregarPropiedades("Ba",  729.0,  1897.0);
    AgregarPropiedades("La",  920.0,  3464.0);
    AgregarPropiedades("Ce",  795.0,  3443.0);
    AgregarPropiedades("Pr",  935.0,  3520.0);
    AgregarPropiedades("Nd", 1024.0,  3074.0);
    AgregarPropiedades("Pm", 1042.0,  3000.0);
    AgregarPropiedades("Sm", 1072.0,  1794.0);
    AgregarPropiedades("Eu",  826.0,  1529.0);
    AgregarPropiedades("Gd", 1312.0,  3273.0);
    AgregarPropiedades("Tb", 1356.0,  3230.0);
    AgregarPropiedades("Dy", 1407.0,  2567.0);
    AgregarPropiedades("Ho", 1461.0,  2720.0);
    AgregarPropiedades("Er", 1529.0,  2868.0);
    AgregarPropiedades("Tm", 1545.0,  1950.0);
    AgregarPropiedades("Yb",  824.0,  1196.0);
    AgregarPropiedades("Lu", 1652.0,  3402.0);
    AgregarPropiedades("Hf", 2233.0,  4603.0);
    AgregarPropiedades("Ta", 3017.0,  5458.0);
    AgregarPropiedades("W",  3422.0,  5555.0);
    AgregarPropiedades("Re", 3186.0,  5596.0);
    AgregarPropiedades("Os", 3033.0,  5012.0);
    AgregarPropiedades("Ir", 2446.0,  4428.0);
    AgregarPropiedades("Pt", 1768.3,  3825.0);
    AgregarPropiedades("Au", 1064.18, 2856.0);
    AgregarPropiedades("Hg",  -38.83,  356.73);
    AgregarPropiedades("Tl",  304.0,  1473.0);
    AgregarPropiedades("Pb",  327.46, 1749.0);
    AgregarPropiedades("Bi",  271.40, 1564.0);
    AgregarPropiedades("Po",  254.0,   962.0);
    AgregarPropiedades("At",  302.0,   337.0);
    AgregarPropiedades("Rn",  -71.0,   -61.7);
    AgregarPropiedades("Fr",   27.0,   677.0);
    AgregarPropiedades("Ra",  696.0,  1413.0);
    AgregarPropiedades("Ac", 1050.0,  3200.0);
    AgregarPropiedades("Th", 1750.0,  4820.0);
    AgregarPropiedades("Pa", 1572.0,  4000.0);
    AgregarPropiedades("U",  1135.0,  4131.0);
    AgregarPropiedades("Np",  644.0,  3902.0);
    AgregarPropiedades("Pu",  640.0,  3228.0);
    AgregarPropiedades("Am", 1176.0,  2607.0);
    AgregarPropiedades("Cm", 1340.0,  3110.0);
    AgregarPropiedades("Bk",  986.0,  2627.0);
    AgregarPropiedades("Cf",  900.0,  1472.0);
    AgregarPropiedades("Es",  860.0,  null);
    AgregarPropiedades("Fm", 1527.0,  null);
    AgregarPropiedades("Md",  827.0,  null);
    AgregarPropiedades("No",  827.0,  null);
    AgregarPropiedades("Lr", 1627.0,  null);
    AgregarPropiedades("Rf",  null,   null);
    AgregarPropiedades("Db",  null,   null);
    AgregarPropiedades("Sg",  null,   null);
    AgregarPropiedades("Bh",  null,   null);
    AgregarPropiedades("Hs",  null,   null);
    AgregarPropiedades("Mt",  null,   null);
    AgregarPropiedades("Ds",  null,   null);
    AgregarPropiedades("Rg",  null,   null);
    AgregarPropiedades("Cn",  null,   null);
    AgregarPropiedades("Nh",  null,   null);
    AgregarPropiedades("Fl",  null,   null);
    AgregarPropiedades("Mc",  null,   null);
    AgregarPropiedades("Lv",  null,   null);
    AgregarPropiedades("Ts",  null,   null);
    AgregarPropiedades("Og",  null,   null);

    // --- Isótopos estables de los primeros 36 elementos ---
    _isotopos["H"]  = new[] { 1, 2 };
    _isotopos["He"] = new[] { 3, 4 };
    _isotopos["Li"] = new[] { 6, 7 };
    _isotopos["Be"] = new[] { 9 };
    _isotopos["B"]  = new[] { 10, 11 };
    _isotopos["C"]  = new[] { 12, 13 };
    _isotopos["N"]  = new[] { 14, 15 };
    _isotopos["O"]  = new[] { 16, 17, 18 };
    _isotopos["F"]  = new[] { 19 };
    _isotopos["Ne"] = new[] { 20, 21, 22 };
    _isotopos["Na"] = new[] { 23 };
    _isotopos["Mg"] = new[] { 24, 25, 26 };
    _isotopos["Al"] = new[] { 27 };
    _isotopos["Si"] = new[] { 28, 29, 30 };
    _isotopos["P"]  = new[] { 31 };
    _isotopos["S"]  = new[] { 32, 33, 34, 36 };
    _isotopos["Cl"] = new[] { 35, 37 };
    _isotopos["Ar"] = new[] { 36, 38, 40 };
    _isotopos["K"]  = new[] { 39, 41 };
    _isotopos["Ca"] = new[] { 40, 42, 43, 44, 46, 48 };
    _isotopos["Sc"] = new[] { 45 };
    _isotopos["Ti"] = new[] { 46, 47, 48, 49, 50 };
    _isotopos["V"]  = new[] { 51 };
    _isotopos["Cr"] = new[] { 50, 52, 53, 54 };
    _isotopos["Mn"] = new[] { 55 };
    _isotopos["Fe"] = new[] { 54, 56, 57, 58 };
    _isotopos["Co"] = new[] { 59 };
    _isotopos["Ni"] = new[] { 58, 60, 61, 62, 64 };
    _isotopos["Cu"] = new[] { 63, 65 };
    _isotopos["Zn"] = new[] { 64, 66, 67, 68, 70 };
    _isotopos["Ga"] = new[] { 69, 71 };
    _isotopos["Ge"] = new[] { 70, 72, 73, 74, 76 };
    _isotopos["As"] = new[] { 75 };
    _isotopos["Se"] = new[] { 74, 76, 77, 78, 80, 82 };
    _isotopos["Br"] = new[] { 79, 81 };
    _isotopos["Kr"] = new[] { 78, 80, 82, 83, 84, 86 };
}

    // =========================================================================
    // TABLAS AUXILIARES ESTÁTICAS
    // =========================================================================
    private static readonly Dictionary<string, (double? fusion, double? ebullicion)>
        _propiedades = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, int[]> _isotopos
        = new(StringComparer.OrdinalIgnoreCase);

    // Lista de los 118 símbolos en orden de número atómico (para iterar sin duplicados)
    private static readonly string[] _simbolosOrdenados = {
        "H","He","Li","Be","B","C","N","O","F","Ne",
        "Na","Mg","Al","Si","P","S","Cl","Ar","K","Ca",
        "Sc","Ti","V","Cr","Mn","Fe","Co","Ni","Cu","Zn",
        "Ga","Ge","As","Se","Br","Kr","Rb","Sr","Y","Zr",
        "Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn",
        "Sb","Te","I","Xe","Cs","Ba","La","Ce","Pr","Nd",
        "Pm","Sm","Eu","Gd","Tb","Dy","Ho","Er","Tm","Yb",
        "Lu","Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg",
        "Tl","Pb","Bi","Po","At","Rn","Fr","Ra","Ac","Th",
        "Pa","U","Np","Pu","Am","Cm","Bk","Cf","Es","Fm",
        "Md","No","Lr","Rf","Db","Sg","Bh","Hs","Mt","Ds",
        "Rg","Cn","Nh","Fl","Mc","Lv","Ts","Og"
    };

    private static void AgregarElemento(int numero, string simbolo, string nombre, double masa, string categoria, string conf)
    {
        var dict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("numero"), new LatinoNumber(numero) },
            { new LatinoString("simbolo"), new LatinoString(simbolo) },
            { new LatinoString("nombre"), new LatinoString(nombre) },
            { new LatinoString("masa"), new LatinoNumber(masa) },
            { new LatinoString("categoria"), new LatinoString(categoria) },
            { new LatinoString("configuracion"), new LatinoString(conf) }
        };

        var latinoDict = new LatinoDictionary(dict, isReadOnly: true);

        // Indexamos por Símbolo y por Nombre para facilitar la búsqueda al usuario
        _db[simbolo] = latinoDict;
        _db[nombre] = latinoDict;
    }

    private static void AgregarPropiedades(string simbolo, double? fusion, double? ebullicion)
    {
        _propiedades[simbolo] = (fusion, ebullicion);
        // Indexar también por nombre
        if (_db.TryGetValue(simbolo, out var elem))
        {
            string nombre = ((LatinoString)elem.Pairs[new LatinoString("nombre")]).Value;
            _propiedades[nombre] = (fusion, ebullicion);
        }
    }

    // =========================================================================
    // tabla.datos(elemento)
    // =========================================================================
    [LatinoExtension("tabla.datos", Args = "elemento", Help = "Devuelve los datos de un elemento por su nombre o símbolo (ej: 'Oro' o 'Au').")]
    public static LatinoObject Datos(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "Requiere el nombre o símbolo del elemento.");
        string busqueda = args[0].ToString().Trim();

        if (_db.TryGetValue(busqueda, out var datos))
        {
            return datos;
        }
        
        throw new RuntimeError(null!, $"Elemento químico no encontrado: '{busqueda}'");
    }

    // =========================================================================
    // tabla.masaMolar(formula)
    // =========================================================================
    [LatinoExtension("tabla.masaMolar", Args = "formula", Help = "Calcula la masa molar de una fórmula química estándar (ej: 'H2O', 'CO2').")]
    public static LatinoObject masaMolar(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "Requiere una fórmula química.");
        string formula = args[0].ToString().Trim();

        var regex = new Regex(@"([A-Z][a-z]*)(\d*)");
        var coincidencias = regex.Matches(formula);

        if (coincidencias.Count == 0)
            throw new RuntimeError(null!, $"La fórmula '{formula}' no contiene elementos químicos válidos.");

        double masaTotal = 0;

        foreach (Match match in coincidencias)
        {
            string simbolo = match.Groups[1].Value;
            string cantidadStr = match.Groups[2].Value;
            int cantidad = string.IsNullOrEmpty(cantidadStr) ? 1 : int.Parse(cantidadStr);

            if (!_db.TryGetValue(simbolo, out var datosElemento))
                throw new RuntimeError(null!, $"Símbolo desconocido en la fórmula: '{simbolo}'");

            var masaObj = datosElemento.Pairs[new LatinoString("masa")];
            double masa = ((LatinoNumber)masaObj).Value;

            masaTotal += masa * cantidad;
        }

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("formula"), new LatinoString(formula) },
            { new LatinoString("masa_molar_g_mol"), new LatinoNumber(masaTotal) }
        };

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // tabla.estado(elemento, temperatura_celsius)
    // =========================================================================
    [LatinoExtension("tabla.estado", Args = "elemento, temperatura_celsius", Help = "Determina el estado de agregación de un elemento a una temperatura dada en °C.\nRetorna: {elemento, simbolo, temperatura, estado, fusion_c, ebullicion_c}")]
    public static LatinoObject Estado(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "Requiere el nombre/símbolo del elemento y la temperatura en °C.");
        string busqueda = args[0].ToString().Trim();
        double temperatura = ((LatinoNumber)args[1]).Value;

        if (!_db.TryGetValue(busqueda, out var elemData))
            throw new RuntimeError(null!, $"Elemento no encontrado: '{busqueda}'");

        string nombre = ((LatinoString)elemData.Pairs[new LatinoString("nombre")]).Value;
        string simbolo = ((LatinoString)elemData.Pairs[new LatinoString("simbolo")]).Value;

        string estado;
        double? fusion = null, ebullicion = null;

        if (_propiedades.TryGetValue(simbolo, out var props))
        {
            fusion = props.fusion;
            ebullicion = props.ebullicion;

            if (fusion == null || ebullicion == null)
                estado = "Desconocido";
            else if (temperatura >= ebullicion.Value)
                estado = "Gaseoso";
            else if (temperatura >= fusion.Value)
                estado = "Líquido";
            else
                estado = "Sólido";
        }
        else
        {
            estado = "Desconocido";
        }

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("elemento"),    new LatinoString(nombre) },
            { new LatinoString("simbolo"),     new LatinoString(simbolo) },
            { new LatinoString("temperatura"), new LatinoNumber(temperatura) },
            { new LatinoString("estado"),      new LatinoString(estado) },
            { new LatinoString("fusion_c"),    fusion.HasValue ? (LatinoObject)new LatinoNumber(fusion.Value) : LatinoNull.Instance },
            { new LatinoString("ebullicion_c"),ebullicion.HasValue ? (LatinoObject)new LatinoNumber(ebullicion.Value) : LatinoNull.Instance }
        };

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // tabla.filtrar(campo, valor)
    // =========================================================================
    [LatinoExtension("tabla.filtrar", Args = "campo, valor", Help = "Filtra elementos por 'categoria', 'periodo' o 'bloque'. Retorna lista de diccionarios de elementos.")]
    public static LatinoObject Filtrar(List<LatinoObject> args)
    {
        if (args.Count < 2) throw new RuntimeError(null!, "Requiere campo (categoria/periodo/bloque) y valor.");
        string campo = args[0].ToString().Trim().ToLower();
        string valor = args[1].ToString().Trim();

        if (campo != "categoria" && campo != "periodo" && campo != "bloque")
            throw new RuntimeError(null!, $"Campo no reconocido: '{campo}'. Use 'categoria', 'periodo' o 'bloque'.");

        var resultado = new LatinoList(new List<LatinoObject>());

        foreach (string sim in _simbolosOrdenados)
        {
            if (!_db.TryGetValue(sim, out var elem)) continue;

            string numStr = ((LatinoNumber)elem.Pairs[new LatinoString("numero")]).Value.ToString();
            int numero = (int)((LatinoNumber)elem.Pairs[new LatinoString("numero")]).Value;
            string cat = ((LatinoString)elem.Pairs[new LatinoString("categoria")]).Value;
            string conf = ((LatinoString)elem.Pairs[new LatinoString("configuracion")]).Value;

            bool coincide = false;

            if (campo == "categoria")
            {
                coincide = cat.Equals(valor, StringComparison.OrdinalIgnoreCase);
            }
            else if (campo == "periodo")
            {
                if (!int.TryParse(valor, out int periodo)) continue;
                coincide = periodo switch {
                    1 => numero >= 1 && numero <= 2,
                    2 => numero >= 3 && numero <= 10,
                    3 => numero >= 11 && numero <= 18,
                    4 => numero >= 19 && numero <= 36,
                    5 => numero >= 37 && numero <= 54,
                    6 => numero >= 55 && numero <= 86,
                    7 => numero >= 87 && numero <= 118,
                    _ => false
                };
            }
            else // bloque
            {
                string bloque = ObtenerBloque(numero);
                coincide = bloque.Equals(valor, StringComparison.OrdinalIgnoreCase);
            }

            if (coincide)
                resultado.Elements.Add(elem);
        }

        return resultado;
    }

    // =========================================================================
    // tabla.compararMasas(lista_elementos)
    // =========================================================================
    [LatinoExtension("tabla.compararMasas", Args = "lista_elementos", Help = "Compara masas molares de una lista de elementos. Retorna {elementos, mas_pesado, mas_liviano, masa_total, promedio}.")]
    public static LatinoObject CompararMasas(List<LatinoObject> args)
    {
        if (args.Count == 0 || args[0] is not LatinoList lista)
            throw new RuntimeError(null!, "Requiere una lista de nombres o símbolos de elementos.");
        if (lista.Elements.Count == 0)
            throw new RuntimeError(null!, "La lista no puede estar vacía.");

        var itemsList = new List<(string nombre, string simbolo, double masa)>();

        foreach (var item in lista.Elements)
        {
            string busqueda = item.ToString().Trim();
            if (!_db.TryGetValue(busqueda, out var elem))
                throw new RuntimeError(null!, $"Elemento no encontrado: '{busqueda}'");

            string nombreEl = ((LatinoString)elem.Pairs[new LatinoString("nombre")]).Value;
            string simboloEl = ((LatinoString)elem.Pairs[new LatinoString("simbolo")]).Value;
            double masaEl = ((LatinoNumber)elem.Pairs[new LatinoString("masa")]).Value;
            itemsList.Add((nombreEl, simboloEl, masaEl));
        }

        // Ordenar de mayor a menor masa
        itemsList.Sort((a, b) => b.masa.CompareTo(a.masa));

        var elementosLista = new LatinoList(new List<LatinoObject>());
        foreach (var e in itemsList)
        {
            var d = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
            {
                { new LatinoString("nombre"),  new LatinoString(e.nombre) },
                { new LatinoString("simbolo"), new LatinoString(e.simbolo) },
                { new LatinoString("masa"),    new LatinoNumber(e.masa) }
            };
            elementosLista.Elements.Add(new LatinoDictionary(d));
        }

        var masPesado = itemsList[0];
        var masLiviano = itemsList[itemsList.Count - 1];
        double masaTotal = 0;
        foreach (var e in itemsList) masaTotal += e.masa;
        double promedio = masaTotal / itemsList.Count;

        LatinoDictionary MakeElem((string nombre, string simbolo, double masa) e)
        {
            var d = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
            {
                { new LatinoString("nombre"),  new LatinoString(e.nombre) },
                { new LatinoString("simbolo"), new LatinoString(e.simbolo) },
                { new LatinoString("masa"),    new LatinoNumber(e.masa) }
            };
            return new LatinoDictionary(d);
        }

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("elementos"),   elementosLista },
            { new LatinoString("mas_pesado"),  MakeElem(masPesado) },
            { new LatinoString("mas_liviano"), MakeElem(masLiviano) },
            { new LatinoString("masa_total"),  new LatinoNumber(masaTotal) },
            { new LatinoString("promedio"),    new LatinoNumber(promedio) }
        };

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // tabla.configuracionDetalle(elemento)
    // =========================================================================
    [LatinoExtension("tabla.configuracionDetalle", Args = "elemento", Help = "Expande la configuración electrónica abreviada de un elemento.\nRetorna: {simbolo, abreviada, expandida, total_electrones, por_capa}")]
    public static LatinoObject ConfiguracionDetalle(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "Requiere el nombre o símbolo del elemento.");
        string busqueda = args[0].ToString().Trim();

        if (!_db.TryGetValue(busqueda, out var elem))
            throw new RuntimeError(null!, $"Elemento no encontrado: '{busqueda}'");

        string simb = ((LatinoString)elem.Pairs[new LatinoString("simbolo")]).Value;
        string conf = ((LatinoString)elem.Pairs[new LatinoString("configuracion")]).Value;
        int numero = (int)((LatinoNumber)elem.Pairs[new LatinoString("numero")]).Value;

        string expandida = ExpandirConfiguracion(conf);

        // Calcular electrones por capa
        var porCapa = new Dictionary<int, int>();
        var tokenRegex = new Regex(@"(\d+)[spdf](\d+)");
        foreach (Match m in tokenRegex.Matches(expandida))
        {
            int capa = int.Parse(m.Groups[1].Value);
            int elec = int.Parse(m.Groups[2].Value);
            if (!porCapa.ContainsKey(capa)) porCapa[capa] = 0;
            porCapa[capa] += elec;
        }

        var porCapaDict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        foreach (var kv in porCapa)
            porCapaDict[new LatinoString(kv.Key.ToString())] = new LatinoNumber(kv.Value);

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("simbolo"),           new LatinoString(simb) },
            { new LatinoString("abreviada"),         new LatinoString(conf) },
            { new LatinoString("expandida"),         new LatinoString(expandida) },
            { new LatinoString("total_electrones"),  new LatinoNumber(numero) },
            { new LatinoString("por_capa"),          new LatinoDictionary(porCapaDict) }
        };

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // tabla.isotopos(elemento)
    // =========================================================================
    [LatinoExtension("tabla.isotopos", Args = "elemento", Help = "Retorna los isótopos estables conocidos de un elemento.\nRetorna: {elemento, simbolo, numero_atomico, isotopos_estables, n_isotopos}")]
    public static LatinoObject Isotopos(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "Requiere el nombre o símbolo del elemento.");
        string busqueda = args[0].ToString().Trim();

        if (!_db.TryGetValue(busqueda, out var elem))
            throw new RuntimeError(null!, $"Elemento no encontrado: '{busqueda}'");

        string nombre = ((LatinoString)elem.Pairs[new LatinoString("nombre")]).Value;
        string simb = ((LatinoString)elem.Pairs[new LatinoString("simbolo")]).Value;
        int numero = (int)((LatinoNumber)elem.Pairs[new LatinoString("numero")]).Value;

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("elemento"),       new LatinoString(nombre) },
            { new LatinoString("simbolo"),        new LatinoString(simb) },
            { new LatinoString("numero_atomico"), new LatinoNumber(numero) }
        };

        if (_isotopos.TryGetValue(simb, out var isotArr))
        {
            var lista = new LatinoList(new List<LatinoObject>());
            foreach (int a in isotArr)
                lista.Elements.Add(new LatinoNumber(a));
            res[new LatinoString("isotopos_estables")] = lista;
            res[new LatinoString("n_isotopos")] = new LatinoNumber(isotArr.Length);
        }
        else
        {
            res[new LatinoString("isotopos_estables")] = new LatinoList(new List<LatinoObject>());
            res[new LatinoString("n_isotopos")] = new LatinoNumber(0);
            res[new LatinoString("nota")] = new LatinoString("Isótopos estables no registrados para este elemento.");
        }

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // tabla.balancear(ecuacion)
    // =========================================================================
    [LatinoExtension("tabla.balancear", Args = "ecuacion", Help = "Balancea una ecuación química simple (ej: 'H2 + O2 = H2O').\nRetorna: {entrada, balanceada, coeficientes, verificado}")]
    public static LatinoObject Balancear(List<LatinoObject> args)
    {
        if (args.Count == 0) throw new RuntimeError(null!, "Requiere una ecuación química como cadena.");
        string entrada = args[0].ToString().Trim();

        if (entrada.Contains('(') || entrada.Contains(')'))
            throw new RuntimeError(null!, "tabla.balancear no soporta fórmulas con paréntesis.");

        if (!entrada.Contains('='))
            throw new RuntimeError(null!, "La ecuación debe contener '=' para separar reactivos de productos.");

        var partes = entrada.Split('=');
        if (partes.Length != 2)
            throw new RuntimeError(null!, "La ecuación debe tener exactamente un signo '='.");

        var reactivos = partes[0].Split('+').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        var productos = partes[1].Split('+').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

        var compuestosOrden = reactivos.Concat(productos).ToList();
        int nCompuestos = compuestosOrden.Count;
        int nReactivos = reactivos.Count;

        // Parsear cada compuesto en {elemento -> coef}
        var parsedList = new List<Dictionary<string, int>>();
        foreach (var comp in compuestosOrden)
            parsedList.Add(ParsearFormula(comp));

        // Recolectar todos los elementos distintos
        var elementos = parsedList.SelectMany(d => d.Keys).Distinct().OrderBy(x => x).ToList();
        int nElementos = elementos.Count;

        // Construir matriz: filas = elementos, columnas = compuestos
        // Reactivos positivos, productos negativos
        double[,] A = new double[nElementos, nCompuestos];
        for (int i = 0; i < nElementos; i++)
        {
            for (int j = 0; j < nCompuestos; j++)
            {
                int signo = j < nReactivos ? 1 : -1;
                A[i, j] = parsedList[j].TryGetValue(elementos[i], out int cnt) ? signo * cnt : 0;
            }
        }

        // Resolver sistema homogéneo A·x = 0 con x_n = 1 (última incógnita libre)
        // Usamos eliminación gaussiana sobre A^T aumentado con ceros
        // Transponer A y agregar columna de ceros para A^T * x = 0
        double[,] At = new double[nCompuestos, nElementos + 1];
        for (int i = 0; i < nCompuestos; i++)
            for (int j = 0; j < nElementos; j++)
                At[i, j] = A[j, i];

        // Forzar última variable = 1 y resolverlas
        // Para el sistema A·x=0, ponemos x_{n-1} = 1 (libre), reducimos el sistema
        int libre = nCompuestos - 1;
        // Columna independiente: A[:,libre] * (-1)
        double[] b = new double[nElementos];
        for (int i = 0; i < nElementos; i++)
            b[i] = -A[i, libre];

        // Sistema: A[:, 0..libre-1] * x[0..libre-1] = b
        int sysN = libre;
        if (sysN > nElementos)
            sysN = nElementos;  // sistema sobredeterminado — usar las primeras filas

        double[,] M = new double[sysN, sysN + 1];
        for (int i = 0; i < sysN; i++)
        {
            for (int j = 0; j < libre; j++)
                M[i, j] = i < nElementos ? A[i, j] : 0;
            M[i, sysN] = i < nElementos ? b[i] : 0;
        }

        // Eliminación gaussiana con pivoteo parcial
        for (int col = 0; col < sysN; col++)
        {
            // Buscar pivote
            int pivotRow = -1;
            double maxVal = 0;
            for (int row = col; row < sysN; row++)
            {
                if (Math.Abs(M[row, col]) > maxVal)
                {
                    maxVal = Math.Abs(M[row, col]);
                    pivotRow = row;
                }
            }
            if (pivotRow == -1 || Math.Abs(M[pivotRow, col]) < 1e-12)
                continue;

            // Intercambiar filas
            for (int k = col; k <= sysN; k++)
            {
                (M[col, k], M[pivotRow, k]) = (M[pivotRow, k], M[col, k]);
            }

            // Eliminar
            for (int row = 0; row < sysN; row++)
            {
                if (row == col || Math.Abs(M[row, col]) < 1e-12) continue;
                double factor = M[row, col] / M[col, col];
                for (int k = col; k <= sysN; k++)
                    M[row, k] -= factor * M[col, k];
            }
        }

        // Back-substitution
        double[] coefs = new double[nCompuestos];
        coefs[libre] = 1.0;
        for (int i = 0; i < sysN; i++)
        {
            if (Math.Abs(M[i, i]) < 1e-12) continue;
            coefs[i] = M[i, sysN] / M[i, i];
        }

        // Convertir a enteros: escalar por LCM de denominadores
        // Aproximar como fracciones con denominador hasta 100
        int[] nums = new int[nCompuestos];
        int[] dens = new int[nCompuestos];
        for (int i = 0; i < nCompuestos; i++)
        {
            var (num, den) = AproximarFraccion(coefs[i], 1000);
            nums[i] = num; dens[i] = den;
        }
        int lcmDen = dens[0];
        for (int i = 1; i < nCompuestos; i++)
            lcmDen = MCM(lcmDen, dens[i]);

        int[] enteros = new int[nCompuestos];
        for (int i = 0; i < nCompuestos; i++)
            enteros[i] = (int)Math.Round(coefs[i] * lcmDen);

        // Si todos son negativos, invertir signos
        if (enteros.All(x => x <= 0))
            for (int i = 0; i < nCompuestos; i++) enteros[i] = -enteros[i];

        // Si alguno es cero o negativo, fallo
        bool verificado = enteros.All(x => x > 0);

        // Verificar balance
        if (verificado)
        {
            foreach (string elSim in elementos)
            {
                int sumaR = 0, sumaP = 0;
                for (int j = 0; j < nReactivos; j++)
                    sumaR += enteros[j] * (parsedList[j].TryGetValue(elSim, out int v) ? v : 0);
                for (int j = nReactivos; j < nCompuestos; j++)
                    sumaP += enteros[j] * (parsedList[j].TryGetValue(elSim, out int v) ? v : 0);
                if (sumaR != sumaP) { verificado = false; break; }
            }
        }

        // Construir cadena balanceada
        string balanceada;
        if (verificado)
        {
            var rParte = reactivos.Select((c, i) => enteros[i] == 1 ? c : $"{enteros[i]}{c}");
            var pParte = productos.Select((c, i) => enteros[nReactivos + i] == 1 ? c : $"{enteros[nReactivos + i]}{c}");
            balanceada = string.Join(" + ", rParte) + " = " + string.Join(" + ", pParte);
        }
        else
        {
            balanceada = entrada;
        }

        // Coeficientes como diccionario
        var coefDict = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance);
        for (int i = 0; i < nCompuestos; i++)
            coefDict[new LatinoString(compuestosOrden[i])] = new LatinoNumber(enteros[i]);

        var res = new Dictionary<LatinoObject, LatinoObject>(LatinoObjectComparer.Instance)
        {
            { new LatinoString("entrada"),       new LatinoString(entrada) },
            { new LatinoString("balanceada"),    new LatinoString(balanceada) },
            { new LatinoString("coeficientes"),  new LatinoDictionary(coefDict) },
            { new LatinoString("verificado"),    new LatinoBoolean(verificado) }
        };

        if (!verificado)
            res[new LatinoString("nota")] = new LatinoString("No se encontró solución entera positiva. Verifique la ecuación.");

        return new LatinoDictionary(res);
    }

    // =========================================================================
    // HELPERS PRIVADOS
    // =========================================================================

    private static string ExpandirConfiguracion(string conf)
    {
        // Si no tiene corchete, ya es expandida
        if (!conf.Contains('['))
            return conf.Trim();

        // Extraer la parte entre corchetes
        var match = Regex.Match(conf, @"\[(\w+)\](.*)");
        if (!match.Success) return conf.Trim();

        string gasNoble = match.Groups[1].Value;
        string resto = match.Groups[2].Value.Trim();

        if (!_db.TryGetValue(gasNoble, out var gasData))
            return conf.Trim(); // No debería ocurrir

        string confGas = ((LatinoString)gasData.Pairs[new LatinoString("configuracion")]).Value;
        string expandidaGas = ExpandirConfiguracion(confGas);

        return string.IsNullOrEmpty(resto) ? expandidaGas : expandidaGas + " " + resto;
    }

    private static string ObtenerBloque(int numero)
    {
        if ((numero >= 58 && numero <= 71) || (numero >= 90 && numero <= 103))
            return "f";

        if ((numero >= 21 && numero <= 30) || 
            (numero >= 39 && numero <= 48) || 
            numero == 57 || 
            (numero >= 72 && numero <= 80) || 
            numero == 89 || 
            (numero >= 104 && numero <= 112))
            return "d";

        if ((numero >= 5 && numero <= 10) || 
            (numero >= 13 && numero <= 18) || 
            (numero >= 31 && numero <= 36) || 
            (numero >= 49 && numero <= 54) || 
            (numero >= 81 && numero <= 86) || 
            (numero >= 113 && numero <= 118))
            return "p";

        return "s";
    }

    private static Dictionary<string, int> ParsearFormula(string formula)
    {
        var resultado = new Dictionary<string, int>(StringComparer.Ordinal);
        var regex = new Regex(@"([A-Z][a-z]*)(\d*)");
        foreach (Match m in regex.Matches(formula))
        {
            string sim = m.Groups[1].Value;
            int cnt = string.IsNullOrEmpty(m.Groups[2].Value) ? 1 : int.Parse(m.Groups[2].Value);
            if (!resultado.ContainsKey(sim)) resultado[sim] = 0;
            resultado[sim] += cnt;
        }
        return resultado;
    }

    private static int MCD(int a, int b)
    {
        a = Math.Abs(a); b = Math.Abs(b);
        while (b != 0) { int t = b; b = a % b; a = t; }
        return a == 0 ? 1 : a;
    }

    private static int MCM(int a, int b)
    {
        if (a == 0 || b == 0) return 1;
        return Math.Abs(a) / MCD(a, b) * Math.Abs(b);
    }

    private static (int num, int den) AproximarFraccion(double valor, int maxDen)
    {
        if (Math.Abs(valor) < 1e-10) return (0, 1);
        int signo = valor < 0 ? -1 : 1;
        valor = Math.Abs(valor);
        int bestNum = 1, bestDen = 1;
        double bestErr = Math.Abs(valor - 1.0);
        for (int d = 1; d <= maxDen; d++)
        {
            int n = (int)Math.Round(valor * d);
            double err = Math.Abs(valor - (double)n / d);
            if (err < bestErr) { bestErr = err; bestNum = n; bestDen = d; }
            if (err < 1e-9) break;
        }
        return (signo * bestNum, bestDen);
    }
}

