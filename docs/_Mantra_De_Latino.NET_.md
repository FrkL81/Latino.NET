# El Mantra de Latino.NET

## 10 Reglas de Oro

Latino.NET está diseñado para ser **fácil de aprender, pero industrial al ejecutar**. Detrás de su sintaxis amigable en español, late la máquina virtual de .NET 10.

Para sacar el máximo provecho del lenguaje y escribir código que sobreviva al tiempo, el equipo de Arquitectura ha definido las **10 Reglas de Oro**. Sigue estos principios y tu software será rápido, seguro, legible y profesional.

### 1. El Formato es Sagrado (UTF-8)

**"Guarda siempre tus archivos con codificación UTF-8".**

Latino.NET habla tu idioma: soporta caracteres hispanos (`ñ`, `á`), operadores lógicos acentuados (`ý`, `ó`) y emojis (`🚀`) de forma nativa. Si tu editor guarda en ANSI u otra codificación obsoleta, el analizador léxico (lexer) verá caracteres corruptos y el programa fallará. Configura tu entorno (VS Code u otros) para usar UTF-8 siempre.

### 2. Delega la Fuerza Bruta

**"Si existe una función nativa en la biblioteca o en un plugin, úsala".**

No reinventes la rueda escribiendo algoritmos pesados a mano en Latino. El motor de Latino.NET delega el trabajo pesado a C# y .NET 10, lo que significa que las funciones de biblioteca (`mate`, `cadena`, `lista`, `archivo`, `sqlite`, `lapsos`) se ejecutan **prácticamente a velocidad nativa**, reduciendo el costo del intérprete a un mínimo imperceptible.

- ❌ *Lento (Intérprete paso a paso):* Ordenar una lista con bucles manuales o sumar elementos uno por uno.
- ✅ *Rápido (Velocidad nativa C#):* Usar `lista.ordenar(datos)` y `lista.sumar(datos)`.

### 3. El Bucle Rey es `para`

**"Prefiere siempre `para` sobre `desde` al recorrer colecciones".**

El bucle `para` está optimizado a bajo nivel mediante enumeradores directos en .NET y es **el doble de rápido** y mucho más seguro que el bucle `desde`.

**Guía de elección de bucles:**

- ✅ `para elemento en coleccion` → Iterar listas, diccionarios, cadenas y series numéricas generadas con `rango()` (**PRIMERA OPCIÓN SIEMPRE**).
- ⚠️ `desde (i = 0; i < n; i++)` → Solo cuando requieras saltos matemáticos exóticos sobre el índice que no resuelva `rango()`.
- ⚠️ `mientras` y `repetir` → Reservados para bucles condicionales impulsados por eventos externos o valores centinela (ej. esperar la respuesta de un usuario o de la red).

### 4. Piensa en Flujos de Datos (`|>`)

**"Compón transformaciones con el Pipeline y funciones de orden superior".**

Evita el "infierno de los paréntesis" y el desgaste de crear listas vacías para llenarlas manualmente con un `si` dentro de un bucle. El operador Pipeline (`|>`) pasa el resultado de la izquierda como **primer argumento** a la función de la derecha, permitiendo que tus datos fluyan de izquierda a derecha sin mutar las colecciones originales.

```latino
# ❌ Difícil de leer (anidado de adentro hacia afuera) y con bucles manuales
numeros = [ 85, 40, 90, 65, 70, 95 ]
aprobados = [ ]
para n en numeros
    si n >= 70
        lista.agregar(aprobados, n)
    fin
fin
promedio = lista.promedio(aprobados)

# ✅ Flujo funcional idiomático (lineal, sin efectos secundarios)
promedio = [ 85, 40, 90, 65, 70, 95 ]
    |> lista.filtrar(funcion(n) retornar n >= 70 fin)
    |> lista.promedio()

# Nota: También puedes usar las formas cortas: fun(n) ret n >= 70 fin
```

### 5. Domina la Nulidad y el Despacho (`??`, `??=`)

**"No le temas al nulo; gobiérnalo con cortocircuito seguro y convierte escaleras condicionales en tablas directas".**

Evita escribir bloques condicionales verbosos solo para comprobar si una variable, parámetro opcional o clave de diccionario existe. Utiliza los operadores de fusión nula de Latino.NET para proveer valores de respaldo por defecto en una sola expresión concisa y segura.

```latino
# ❌ Verbosidad propensa a errores
si ajustes["puerto"] == nulo
    puerto = 8080
sino
    puerto = ajustes["puerto"]
fin

# ✅ Fusión Nula (??): Conciso, seguro y elegante
puerto = ajustes["puerto"] ?? 8080

# ✅ Asignación Nula (??=): Inicializa la variable solo si actualmente es nula
sesion["rol"] ??= "invitado"
```

#### El Patrón de Despacho: De la Escalera $O(N)$ a la Tabla Directa $O(1)$

Cuando tienes múltiples opciones fijas, encadenar `si / osi / osi / sino` obliga al intérprete a recorrer nodo por nodo cada condición en tiempo lineal **$O(N)$**. Si tienes 10 casos, en el peor de los escenarios el motor interpretará 10 comparaciones una tras otra en el AST.

En su lugar, aprovecha que los diccionarios de Latino.NET delegan su búsqueda directamente en las tablas hash nativas de **.NET 10** en tiempo constante **$O(1)$** (instantáneo en nanosegundos). Combina un diccionario con el operador `??` para crear tablas de despacho ultrarrápidas, legibles y mantenibles:

```latino
# ❌ Lento y propenso a desorden: Escalera interpretada O(N)
si opcion == "guardar"
    accion = "Guardando archivo..."
osi opcion == "abrir"
    accion = "Abriendo archivo..."
osi opcion == "cerrar"
    accion = "Cerrando archivo..."
sino
    accion = "Opción desconocida"
fin

# ✅ Rápido, limpio y elegante: Despacho hash directo O(1) con ??
mensajes = {
    "guardar": "Guardando archivo...",
    "abrir":   "Abriendo archivo...",
    "cerrar":  "Cerrando archivo..."
}
accion = mensajes[opcion] ?? "Opción desconocida"
```

> 💡 **Despacho de Acciones (Comportamiento Dinámico):**  
> En Latino.NET las funciones son ciudadanos de primera clase. Puedes almacenar pequeñas funciones anónimas (`fun() ... fin`) dentro del diccionario para ejecutar acciones distintas en una sola línea, eliminando ramas condicionales complejas:

```latino
operaciones = {
    "+": fun(a, b) ret a + b fin,
    "-": fun(a, b) ret a - b fin,
    "*": fun(a, b) ret a * b fin,
    "/": fun(a, b) ret b == 0 ? "División por cero" : a / b fin
}

# Resuelve la acción en O(1) con función de respaldo segura
calcular = dic.obtener(operaciones, "+", fun(a, b) ret "Operador inválido" fin)
escribir(calcular(10, 5)) # 15

# Resuelve la acción en O(1) directamente desde el diccionario
escribir(operaciones["*"](10, 5)) # 50
```

### 6. Ámbito Limpio: Conoce tu Entorno y Protege con `local`

**"Latino.NET crea variables locales por defecto si son nuevas, pero muta los ámbitos superiores si el nombre ya existe; usa `local` para blindar tu código."**

En Latino.NET las variables nuevas que nacen dentro de una función son locales de forma natural. Sin embargo, antes de crear una variable, el motor busca hacia arriba en la cadena de ámbitos. Reserva y prioriza la palabra clave `local` para dos propósitos arquitectónicos indispensables:

1. **Sombreado Explícito ante Colisión Real (*Shadowing*):** Si una función necesita utilizar un identificador que ya existe en el ámbito superior (por ejemplo, una variable global `total`), antepón `local` para declarar una variable fresca en el ámbito hijo y evitar mutar la variable externa.
2. **Privacidad Real en POO y Fábricas:** En constructores de clases y módulos, declara variables con `local` para crear datos estrictamente privados y matemáticamente inaccesibles desde el exterior mediante clausuras léxicas.

🎯 **Ejemplo: Sombreado explícito con variable externa colisionante**

```latino
# Variable preexistente en el ámbito superior
suma = 1000

funcion calcular_promedio(numeros)
    local suma = 0  # Sombreado intencional: no modifica la variable 'suma' externa
    para n en numeros
        suma = suma + n
    fin
    retornar suma / lista.longitud(numeros)
fin

escribir("Promedio: " .. calcular_promedio([10, 20, 30]))  # Salida: 20
escribir("Suma externa intacta: " .. suma)                  # Salida: 1000

# ✅ Privacidad estricta en POO (Patrón Bóveda)
clase Boveda
    constructor(clave)
        local _secreto = clave   # Oculto: inaccesible e invisible desde fuera
        mi.verificar = funcion(intento) retornar intento == _secreto fin
    fin
fin
```

> 💡 **Regla de Bloque:** Las estructuras de control (`si`, `mientras`, `para`) crean entornos hijos. Si una variable calculada dentro de un bloque condicional debe ser utilizada después de su `fin`, declárala e inicialízala antes de entrar a él.

### 7. Elige el Paradigma Proporcional

**"No mates moscas con cañones, ni sostengas rascacielos con mondadientes sueltos".**

Latino.NET es un lenguaje multiparadigma real. La maestría del desarrollador consiste en aplicar la herramienta adecuada para la complejidad del problema:

1. **Usa Procedural** para scripts de consola lineales, tareas cron, utilidades CLI y transformaciones directas donde no hay persistencia de estado.
2. **Usa Funcional** para procesar colecciones, consultar datos, aplicar filtros matemáticos y construir pipelines sin efectos secundarios.
3. **Usa POO (`clase ... fin`)** cuando existan **entidades con identidad, estado interno que proteger e invariantes de negocio** (ej: una cuenta bancaria, una conexión de red, un carrito de compras o un juego). Apóyate en los pronombres de concordancia (`mi`, `este`, `esta`) para que tu modelo de objetos se lea en español natural.

### 8. Claridad con el Compositor (`ç`)

**"Usa el Compositor para navegar estructuras; resuelve la lógica fuera de él".**

El compositor `ç("... |expresion| ...")` es un interpolador estructural de alta velocidad diseñado para inspeccionar datos de forma limpia sin la maraña visual de concatenar con `..`.

- **Qué sí resuelve dentro de `| |`:** Navegación estructural directa: variables simples (`|nombre|`), propiedades por punto (`|usuario.edad|`, `|esta.saldo|`), claves literales de diccionario (`|config['puerto']|`, `|contadores["completada"]|`) e índices constantes de colecciones (`|items[0]|`).
- **Qué no resuelve dentro de `| |`:** Operaciones aritméticas (`|a + b|`), comparaciones, llamadas a funciones (`|mate.abs(x)|`) ni claves e índices dinámicos (`|contadores[estado]|`, `|items[indice]|`).

> **Claves e índices dinámicos:** cuando la clave o el índice depende de una variable, resuélvelo antes del compositor con acceso directo o con la función apropiada (`dic.obtener`, `lista.indice`, etc.) y pasa al `ç()` el valor simple ya obtenido.

```latino
precio = 100

# ❌ Incorrecto: operación no resuelta o impresa literalmente:
escribir(ç("Resultado: |precio * 1.16|"))

# ❌ Incorrecto: el compositor no evalúa una clave dinámica:
contadores = {"pendiente": 2}
estado = "pendiente"
escribir(ç("Pendientes: |contadores[estado]|"))

# ✅ Correcto: resuelve la operación antes o utiliza formateo numérico:
total = precio * 1.16
escribir(ç("Subtotal: $|precio| -> Total con IVA: $|total|"))

# ✅ Correcto: resuelve la clave dinámica antes y entrega un valor simple:
cuenta = dic.obtener(contadores, estado, 0)
escribir(ç("  |estado|: |cuenta|"))

# 💡 Para reportes tabulares o dinero con decimales fijos, prefiere escribirf():
escribirf("Total con IVA: ${0:N2}\n", total)
```

### 9. Agrupa las Operaciones de Entrada/Salida

**"Procesa en memoria; escribe y consulta en bloque".**

Las operaciones de disco (archivos), red (HTTP) y bases de datos son **miles de veces más lentas** que las operaciones en memoria RAM.

- ❌ *Lento:* Abrir y escribir en un archivo línea por línea dentro de un bucle de mil iteraciones, o ejecutar `sqlite.ejecutar()` mil veces de forma individual.
- ✅ *Rápido:* Acumular los datos en una lista en memoria, unirlos con `lista.separar()` y escribirlos en disco en una sola operación. En bases de datos, envuelve siempre las inserciones masivas dentro de `sqlite.transaccion()`.
- ✅ *En Consola:* Separa la recolección de entradas (`pedir`) de la presentación de tablas formateadas (`escribirf`), evitando solapamientos visuales.

### 10. No Adivines, Mide

**"La optimización prematura es la raíz de todos los males".**

Antes de reescribir un algoritmo haciéndolo críptico con la excusa de que sea "más rápido", mide con precisión dónde está el cuello de botella real utilizando el cronómetro de alta resolución del sistema:

🎯 **Ejemplo: Medición de rendimiento en milisegundos**

```latino
inicio = sis.milisegundos()

# Simulación de proceso a evaluar
datos = rango(10000)
lista.ordenar(datos)

tiempo_total = sis.milisegundos() - inicio
escribir(ç("⏱️ Tiempo de ejecución real: |tiempo_total| ms"))
# Salida variable (depende del momento de ejecución y hardware):
```

## La Filosofía Final

Latino.NET es un **Lenguaje Dinámico sobre un Motor Estático**.

Cuando escribes código puramente interpretado (bucles manuales carácter por carácter, algoritmos reinventados a mano), dependes exclusivamente del recorrido de nodos del AST. Cuando escribes con **estilo idiomático** (delegando a la biblioteca estándar, aprovechando `para`, usando funciones de orden superior y operadores modernos), el motor delega la carga pesada al compilador JIT de **.NET 10**.

**La diferencia entre un script lento y una aplicación de grado industrial no está en Latino.NET; está en cómo usas sus herramientas.**

¡Escribe con la belleza del español, ejecuta con la potencia de .NET!

### 📊 Datos Reales Medidos en Latino.NET (v1.0.81 sobre .NET 10)

> **⚠️ Cómo leer estas cifras:** son **relativas al hardware y a la carga** de la máquina donde se midieron. El cociente entre dos implementaciones se mantiene estable, pero los valores absolutos cambian con el procesador, la memoria disponible y los procesos en ejecución. Lo verificable es que **el trabajo delegado a .NET reduce trabajo**, no un multiplicador fijo: las cifras orientan, no predicen el rendimiento de tu máquina.
> Para una medición con condiciones declaradas (iteraciones y repeticiones), ver el capítulo de arquitectura interna (`14_Guia_Arquitectura_Interna.md`).

- `lista.ordenar()` es **~750 veces más rápido** que un algoritmo de burbuja implementado manualmente en Latino.
- `lista.sumar()` nativo es **~25 veces más rápido** que acumular valores con un bucle `desde`.
- El bucle `para` es **~2 veces más rápido** que `desde` para iterar cualquier colección.
- El estilo Funcional (`lista.filtrar` + `lista.transformar`) tiene un **rendimiento idéntico** al de un bucle imperativo manual, con la ventaja de otorgar inmutabilidad y legibilidad inmediata.
- El operador pipeline `|>` con aplicación parcial nativa es **~11% más veloz** que encadenar llamadas anidadas con asignaciones intermedias.

### 📝 Nota Técnica: Emojis y la Geometría en Terminales Heterogéneas

En Latino.NET fomentamos el diseño de interfaces de consola (CLI) modernas y expresivas con soporte nativo de emojis (🚀, 📊, ⚙️). Sin embargo, la renderización visual de caracteres Unicode en entornos de terminal presenta desafíos físicos de alineación que todo desarrollador debe conocer:

1. **La Naturaleza de las Celdas Monoespaciadas:**  
   Tradicionalmente, las terminales asumen que cada carácter ocupa exactamente **1 celda** de ancho. Los emojis y símbolos extendidos son glifos complejos que, dependiendo de la consola, la fuente instalada y el sistema operativo, pueden interpretarse como de **1 celda** o de **2 celdas** (ancho doble).
2. **Consolas Modernas vs. Consolas Heredadas (*Legacy*):**  
   - En emuladores de terminal modernos con soporte completo para fuentes de ancho variable y símbolos extendidos (*Windows Terminal*, *iTerm2*, terminales nativas de Linux actuales), el cálculo visual es exacto por defecto (ancho `0`).
   - En terminales heredadas, consolas integradas con motores ligeros o sesiones remotas por SSH, ciertos glifos pueden superponerse o quebrar la alineación tabular de reportes generados con `escribirf()`.
3. **El Adaptador de Compatibilidad de Latino.NET:**  
   Para que tus aplicaciones puedan ejecutarse de forma impecable sin importar la antigüedad de la consola del usuario final, Latino.NET incorpora un ajuste de compensación geométrica:

    ```latino
    # Por defecto es 0 (óptimo para terminales modernas)
    # Ajusta a 1 o 2 para estabilizar la cuadrícula en consolas heredadas
    sis.config("emoji_spacing", 2)
    ```

Esta directiva (Opcional) asegura que tu software mantenga una presentación profesional y tablas perfectamente cuadradas en cualquier ecosistema de terminal.
