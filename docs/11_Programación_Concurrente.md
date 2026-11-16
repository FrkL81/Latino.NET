# CAPÍTULO 11: Concurrencia, Multihilo y Paralelismo

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores, estudiantes, investigadores y creadores de software que necesiten acelerar el procesamiento de datos masivos, ejecutar tareas en segundo plano o evitar que sus aplicaciones se congelen ante operaciones lentas.  
**Propósito:** Dominar de forma práctica, intuitiva y segura la ejecución simultánea mediante tareas en segundo plano (`hilos`), sincronización de recursos (`mutex`), paso de mensajes (`canal`) y procesos del sistema operativo (`proceso`).

## La Orquesta Multinúcleo

Durante décadas, la velocidad de las computadoras aumentó de una forma sencilla: los fabricantes incrementaban la frecuencia de reloj de los procesadores (los gigahercios). Un programa escrito en 1990 corría automáticamente diez veces más rápido en una máquina de 1995 sin necesidad de modificar una sola línea de código.

Sin embargo, a principios del siglo XXI la física impuso un límite térmico infranqueable: los chips ya no podían disipar el calor suficiente para seguir aumentando la velocidad de un solo núcleo sin derretirse. La industria respondió transformando radicalmente la arquitectura del silicio: **multiplicar los núcleos de procesamiento**.

Hoy en día, cualquier computadora portátil, teléfono móvil o servidor en la nube cuenta con 4, 8, 16 o más núcleos de cómputo independientes. A pesar de esto, muchos lenguajes de scripting interpretados tradicionales continúan atados al pasado, confinados a un único núcleo debido a bloqueos globales del intérprete (como el célebre *GIL* de Python o Ruby).

**Latino.NET nació para la era multinúcleo.**

### Concurrencia Real al Alcance de Todos

Al ejecutarse sobre la máquina virtual de **.NET 10**, Latino.NET trasciende los modelos puramente lineales. El motor no simula hilos ni los multiplexa sobre un único canal; pone a tu disposición **concurrencia y paralelismo reales sobre el ThreadPool industrial de Microsoft**.

La concurrencia no es un lujo para servidores de alta gama; es una necesidad práctica en problemas cotidianos:

* **Descargas y peticiones web:** Descargar 20 páginas o archivos simultáneamente en lugar de esperar la suma de sus latencias en fila india.
* **Cálculo masivo de datos:** Dividir una lista de un millón de números entre los 8 núcleos de tu computadora para calcular estadísticas en una fracción del tiempo habitual.
* **Tareas en segundo plano (*Background Tasks*):** Guardar copias de seguridad o procesar registros en disco mientras el programa continúa interactuando con el usuario sin pausas ni congelamientos visuales.

### Las Cuatro Voces de la Concurrencia en Latino.NET

Para que puedas orquestar la ejecución simultánea sin caer en la maraña de errores que tradicionalmente acompaña a la programación multihilo, Latino.NET organiza su ecosistema concurrente en cuatro herramientas especializadas:

1. **Tareas y Trabajadores en Segundo Plano (`hilos`):** Despacho ágil de funciones autónomas con el patrón *Fan-Out / Fan-In* (`hilos.lanzar`, `hilos.esperarTodos`), delegando la gestión de hilos a bajo nivel al sistema operativo.
2. **Exclusión Mutua y Secciones Críticas (`mutex`):** Cerrojos de sincronización en memoria para garantizar que dos hilos no colisionen al escribir en un mismo archivo físico o recurso compartido.
3. **Paso de Mensajes sin Memoria Compartida (`canal`):** Canales seguros inspirados en la arquitectura moderna de Go y el modelo CSP (*Communicating Sequential Processes*): los hilos se comunican enviándose datos por tuberías protegidas, erradicando de raíz las condiciones de carrera.
4. **Procesos Asíncronos del Sistema (`proceso`):** Lanzamiento de binarios externos, ejecución de comandos de terminal no bloqueantes y auditoría de procesos del sistema operativo.

> 💡 **La Filosofía de la Comunicación Segura:**  
> *"No te comuniques compartiendo memoria; comparte memoria comunicándote."*  
> La concurrencia tradicional falla cuando múltiples hilos intentan modificar las mismas variables al mismo tiempo. En Latino.NET, el camino profesional y recomendado es diseñar tus programas como una red de trabajadores independientes que se pasan mensajes limpios a través de **canales (`canal`)**, garantizando sistemas rápidos, predecibles e inmunes a bloqueos.

## 🧵 11.1 Fundamentos: Concurrencia frente a Paralelismo

Antes de escribir una sola línea de código multihilo, es imperativo establecer la frontera técnica entre dos conceptos que a menudo se confunden en la literatura: **concurrencia** y **paralelismo**.

```text
┌────────────────────────────────────────────────────────────────────────┐
│                      CONCURRENCIA vs. PARALELISMO                      │
├────────────────────────────────────┬───────────────────────────────────┤
│  CONCURRENCIA (Lidiar con mucho)   │ PARALELISMO (Hacer mucho a la vez)│
├────────────────────────────────────┼───────────────────────────────────┤
│ • Estructura del programa.         │ • Ejecución física simultánea.    │
│ • Múltiples tareas progresan de    │ • Múltiples tareas corren en      │
│   forma intercalada o reactiva.    │   núcleos de CPU independientes.  │
│ • Ideal para I/O intensivo:        │ • Ideal para CPU intensivo:       │
│   esperar respuestas de red,       │   cálculo matricial, algoritmos   │
│   lectura de discos o eventos.     │   numéricos pesados o compresión. │
└────────────────────────────────────┴───────────────────────────────────┘
```

En palabras de Rob Pike (uno de los arquitectos del modelo CSP):  
> *"La concurrencia es sobre **estructurar** un programa; el paralelismo es sobre la **ejecución** simultánea. La concurrencia te permite estructurar tu solución para que pueda ser paralelizable si el hardware lo soporta."*

### La Gran Ventaja Arquitectónica: Latino.NET NO Tiene GIL

En lenguajes interpretados tradicionales como Python (CPython) o Ruby estándar, existe una barrera histórica conocida como el **GIL (*Global Interpreter Lock*)**: un cerrojo que prohíbe que más de un hilo ejecute instrucciones de bytecode al mismo tiempo, limitando todo el programa a la potencia de un solo núcleo de la CPU sin importar cuántos hilos declares.

En Latino.NET, **el GIL no existe**:

1. **Aislamiento de Intérpretes:** Cuando invocas `hilos.lanzar()`, el plugin `Latino.Plugin.Hilos` crea una instancia dedicada de la clase `Interpreter` para ese hilo específico.
2. **Ámbitos Locales Herméticos:** Cada trabajador posee su propio entorno léxico (`LatinoEnvironment`), evitando la contienda por el bloqueo de tablas de variables locales.
3. **Despacho Nativo en .NET 10:** La ejecución se delega en el subsistema de tareas de C# (`Task.Run`), permitiendo que el sistema operativo distribuya físicamente los intérpretes hijos entre los diferentes núcleos de la CPU de forma simultánea.

### El Costo Oculto de la Concurrencia: ¿Cuándo Conviene Usarla?

La concurrencia no es una varita mágica que hace que cualquier código corra instantáneamente más rápido. Lanzar un hilo conlleva un costo de ingeniería ineludible:

* **Sobrecarga de Arranque (*Spawning Overhead*):** Inicializar un nuevo hilo, reservar su pila de llamadas y preparar su entorno de memoria toma tiempo (del orden de microsegundos o milisegundos).
* **Cambio de Contexto (*Context Switching*):** Cuando hay más hilos activos que núcleos físicos, el sistema operativo debe pausar un hilo, guardar sus registros de CPU y despertar al siguiente.

#### Regla de Decisión del Arquitecto

```text
                                ¿Operación Pesada?
                                        │
                         ┌──────────────┴──────────────┐
                         ▼                             ▼
                  [ CPU Intensivo ]             [ I/O Intensivo ]
                         │                             │
            ¿Tarda más de 15-20 ms?           ¿Bloquea la terminal o
                         │                     espera red/archivos?
                   ┌─────┴─────┐                       │
                  SÍ           NO                      ▼
                   │            │             [ USAR CONCURRENCIA ]
                   ▼            ▼             (hilos.lanzar / canal)
          [ USAR PARALELISMO ] [ SECUENCIAL ]
          (Dividir en núcleos) (Un solo hilo)
```

* ❌ **Antipatrón (Sobrecarga Absurda):** Lanzar un hilo independiente para sumar dos números (`hilos.lanzar(fun() ret 2 + 2 fin)`). El tiempo necesario para crear el hilo y coordinarlo es cientos de veces mayor que la suma misma.
* ✅ **Patrón Correcto (I/O Concurrente):** Descargar tres archivos desde la web o consultar tres bases de datos simultáneamente. Mientras un hilo espera la respuesta de la red, los demás continúan trabajando sin congelar el programa.
* ✅ **Patrón Correcto (Cómputo Paralelo):** Procesar una lista de 100,000 elementos dividiéndola en 4 fragmentos homogéneos distribuidos entre los núcleos de la máquina.

### Comparativa de Modelos de Ejecución

| Paradigma | Recurso Limitante | Comportamiento en Latino.NET | Herramienta Clave |
| :--- | :--- | :--- | :--- |
| **Secuencial** | Tiempo lineal | Las instrucciones corren en orden estricto de arriba abajo en un solo hilo. | Bucle `para`, pipeline `\|>` |
| **Concurrente (I/O)** | Red, Disco, Periféricos | Múltiples tareas avanzan intercaladas sin congelar la terminal ni la UI. | `hilos.lanzar()`, `canal` |
| **Paralelo (CPU)** | Núcleos del Procesador | Múltiples hilos computan fracciones de un cálculo masivo al mismo tiempo. | `hilos.esperarTodos()` |
| **Asíncrono de SO** | Procesos Externos | Binarios externos corren de forma no bloqueante a nivel del sistema operativo. | `proceso.ejecutarAsync()` |

Con estos fundamentos teóricos claros, estamos listos para encender el motor y aprender a crear, monitorear y gobernar nuestros primeros trabajadores en segundo plano: **el plugin `hilos`**.

## 🧵 11.2 El Modelo de Tareas y Trabajadores (`hilos`)

El plugin `hilos` constituye la columna vertebral de la ejecución en paralelo en Latino.NET. Proporciona una interfaz orientada a **Tareas y Trabajadores (*Worker Tasks*)**: despacha funciones en segundo plano sobre el *ThreadPool* de .NET 10, captura automáticamente sus valores de retorno y permite sincronizar la recolección de resultados sin lidiar manualmente con la complejidad de sockets o hilos del sistema operativo.

### 11.2.1 Carga y Activación del Plugin

Al tratarse de un plugin oficial de la distribución estándar, se incorpora al script mediante la directiva `incluir()`:

```latino
incluir("hilos")
```

Al cargarse, el plugin registra en el entorno global el objeto de biblioteca `hilos`, sellado y protegido contra modificaciones accidentales en tiempo de ejecución.

### 11.2.2 Despacho de Tareas con `hilos.lanzar()`

Para ejecutar una función en segundo plano, se utiliza la función `hilos.lanzar()`:

```latino
idHilo = hilos.lanzar(funcionObjetivo, [argumento1, argumento2, ...])
```

#### Mecánica Interna del Lanzamiento

Cuando invocas `hilos.lanzar()`:

1. **Instanciación del Intérprete Hijo:** El motor crea un nuevo `Interpreter` completamente aislado en memoria para el trabajador.
2. **Carga de Bibliotecas:** El subintérprete hereda automáticamente los flujos estándar de entrada y salida (`In`, `Out`), la profundidad máxima de pila (`MaxDepth`), y recarga la biblioteca estándar (`mate`, `cadena`, `archivo`...) junto con el propio plugin `hilos`.
3. **Despacho sobre el ThreadPool:** Se delega la ejecución de la función en una `Task<LatinoObject>` administrada por .NET 10, asignándole de inmediato un núcleo físico o lógico disponible.
4. **Retorno del Identificador (GUID):** `hilos.lanzar()` no bloquea el script; retorna inmediatamente una cadena de texto única de 36 caracteres (un UUID/GUID, ej: `"4f8a12e9-7b3c-..."`) que sirve como recibo o puntero para rastrear, consultar y esperar el resultado del hilo.

```latino
incluir("hilos")

# Procedimiento que simula un trabajo intensivo
funcion TareaCalculo(nombre, limite)
    local suma = 0
    para i en rango(limite)
        suma = suma + i
    fin
    escribir("   [" .. nombre .. "] Cómputo finalizado con éxito.")
    retornar suma
fin

escribir("1. Despachando tarea en segundo plano...")

# Lanzamos la función pasando sus dos argumentos
id = hilos.lanzar(TareaCalculo, "Worker-Alpha", 1000000)

# El hilo principal continúa de inmediato sin congelarse
escribir("2. El script principal sigue ejecutando instrucciones.")
escribir("   ID del hilo asignado por el motor: " .. id)

# Esperamos el resultado del trabajador
resultado = hilos.esperar(id)
escribir("3. Resultado recuperado del hilo: " .. resultado)
```

**Salida en consola:**

```text
1. Despachando tarea en segundo plano...
2. El script principal sigue ejecutando instrucciones.
   ID del hilo asignado por el motor: d6f882a1-5c3b-4b21-a189-9e8c4e09f112
   [Worker-Alpha] Cómputo finalizado con éxito.
3. Resultado recuperado del hilo: 499999500000
```

### 11.2.3 Sincronización y Espera: `hilos.esperar()`

El identificador devuelto por `hilos.lanzar()` permite recuperar el valor retornado por la función en segundo plano mediante `hilos.esperar()`:

```latino
valorRetornado = hilos.esperar(idHilo, [timeoutMs])
```

#### Modo 1: Espera Indefinida (Bloqueante)

Si solo pasas el `idHilo`, el hilo invocador se pausará pacíficamente hasta que la tarea en segundo plano concluya y retorne su valor. Al terminar:

1. El motor recupera el valor retornado por la función.
2. Elimina automáticamente el registro del hilo de la tabla interna de tareas activas para evitar fugas de memoria (*memory leaks*).
3. Si el hilo secundario sufrió una excepción no controlada, el subintérprete la captura en su frontera, escribe un diagnóstico en la consola y **el hilo principal NO se cae**: `hilos.esperar()` retorna `nulo`. ⚠️ No pongas un `intentar ... atrapar` esperando capturar el fallo del hilo secundario: no se propagará. Ver [11.7.3](#1173-resiliencia-de-trabajadores-aislamiento-de-excepciones).

#### Modo 2: Espera con Tiempo Límite (*Timeout*)

Si pasas un segundo argumento numérico en milisegundos (`timeoutMs`), el motor esperará como máximo ese lapso de tiempo:

* **Si el hilo termina antes de vencer el plazo:** Retorna su valor y libera el registro normalmente.
* **Si el plazo expira sin que el hilo termine:** **Retorna `nulo` inmediatamente**, pero **el hilo secundario NO se detiene**: continúa trabajando en segundo plano y su identificador se conserva intacto en la tabla interna para que puedas volver a consultarlo o esperarlo más adelante.

```latino
incluir("hilos")

funcion DescargaPesada()
    # Simula una operación de red de 2 segundos (2000 ms)
    hilos.dormir(2000)
    retornar "DATOS_DESCARGADOS_OK"
fin

id = hilos.lanzar(DescargaPesada)

# Intentamos esperar con un timeout impaciente de 500 ms
intento1 = hilos.esperar(id, 500)

si intento1 == nulo
    escribir("⏳ El hilo aún no termina (timeout de 500 ms alcanzado).")
    escribir("   El programa puede hacer otras tareas mientras tanto...")
fin

# Ahora esperamos el tiempo necesario sin timeout para recolectar el dato
resultadoFinal = hilos.esperar(id)
escribir("✅ Recibido: " .. resultadoFinal)
```

**Salida en consola:**

```text
⏳ El hilo aún no termina (timeout de 500 ms alcanzado).
   El programa puede hacer otras tareas mientras tanto...
✅ Recibido: DATOS_DESCARGADOS_OK
```

### 11.2.4 El Patrón Fan-Out / Fan-In: `hilos.esperarTodos()`

Uno de los patrones más importantes de la computación paralela es el **Fan-Out / Fan-In** (*Despliegue y Recolección*):

* **Fan-Out:** Tomar un problema masivo, dividirlo en $N$ fragmentos homogéneos y lanzar $N$ hilos simultáneamente sobre los núcleos de la CPU.
* **Fan-In:** Sincronizar un punto de reunión donde el hilo principal espera a que **todos** los trabajadores terminen y consolida sus resultados en una sola lista.

En Latino.NET, este patrón se implementa con la función nativa `hilos.esperarTodos(listaIds)`:

```latino
listaResultados = hilos.esperarTodos(listaDeIdentificadores)
```

#### Caso Práctico: Procesamiento Paralelo en 4 Núcleos

```latino
incluir("hilos")

# Procedimiento que procesará un bloque numérico
funcion SumarRango(nombreLote, inicio, final)
    local suma = 0
    para i en rango(inicio, final)
        suma = suma + i
    fin
    retornar {"lote": nombreLote, "suma": suma}
fin

escribir("=== DIVISIÓN PARALELA (FAN-OUT) ===")
tiempoInicio = sis.milisegundos()

# Lanzamos 4 hilos para procesar 40 millones de números en paralelo
hilosDespachados = [ ]

lista.agregar(hilosDespachados, hilos.lanzar(SumarRango, "Lote-1", 0,         10000000))
lista.agregar(hilosDespachados, hilos.lanzar(SumarRango, "Lote-2", 10000000, 20000000))
lista.agregar(hilosDespachados, hilos.lanzar(SumarRango, "Lote-3", 20000000, 30000000))
lista.agregar(hilosDespachados, hilos.lanzar(SumarRango, "Lote-4", 30000000, 40000000))

escribir("4 hilos trabajando simultáneamente en los núcleos de la CPU...")

# Punto de reunión (FAN-IN): Espera hasta que los 4 finalicen
resultados = hilos.esperarTodos(hilosDespachados)

duracion = sis.milisegundos() - tiempoInicio
escribir("=== RECOLECCIÓN CONCLUIDA (FAN-IN) ===")

granTotal = 0
para lote en resultados
    granTotal = granTotal + lote["suma"]
    escribirf("  Resultado de {0}: {1}\n", lote["lote"], lote["suma"])
fin

escribir("-------------------------------------------------")
escribir("Suma global consolidada: " .. granTotal)
escribir("Tiempo total de cómputo en paralelo: " .. duracion .. " ms")
```

> 💡 **Nota de Rendimiento:** `hilos.esperarTodos()` libera automáticamente de la memoria del motor todos los identificadores de la lista, devuelva cada hilo un resultado exitoso o `nulo` en caso de falla.

### 11.2.5 Monitoreo y Mantenimiento del Ciclo de Vida

El plugin incluye tres funciones complementarias indispensables para la supervisión y control higiénico de los trabajadores:

#### 1. `hilos.estaVivo(idHilo)`

Retorna `cierto` si el hilo secundario aún se encuentra en ejecución o pendiente de procesar, y `falso` si ya concluyó su tarea o si el identificador no existe.

```latino
si hilos.estaVivo(id)
    escribir("El trabajador sigue ocupado...")
sino
    escribir("El trabajador ya completó su ciclo.")
fin
```

#### 2. `hilos.dormir(milisegundos)`

Detiene la ejecución del hilo que lo invoca durante el tiempo especificado. A diferencia de `sis.dormir()` (que acepta segundos fraccionarios), `hilos.dormir()` opera con precisión de milisegundos enteros delegando directamente en `Thread.Sleep()` del sistema operativo.

```latino
hilos.dormir(250) # Pausa de un cuarto de segundo en el hilo actual
```

#### 3. `hilos.limpiar()`

En servicios de larga duración (daemons, servidores web o scripts de consola que corren indefinidamente), los hilos que terminan sin que se invoque explícitamente `hilos.esperar()` acumulan metadatos en la tabla interna de tareas del plugin.

Invocando `hilos.limpiar()`, el motor escanea la tabla de hilos, purga de memoria todos los que hayan finalizado y retorna el número de tareas recolectadas:

```latino
tareasLimpiadas = hilos.limpiar()
escribir("Se liberaron " .. tareasLimpiadas .. " tareas finalizadas de la memoria.")
```

El modelo de tareas resuelve brillantemente el cómputo independiente y el procesamiento por lotes. Sin embargo, surge un desafío inevitable cuando dos o más hilos necesitan interactuar con un mismo recurso externo (como un archivo o un saldo bancario): **la exclusión mutua y los cerrojos de sincronización (`mutex`)**.

## 11.3 Exclusión Mutua y Secciones Críticas (`mutex`)

En la sección anterior vimos que cada hilo lanzado con `hilos.lanzar()` posee su propia memoria local aislada. Sin embargo, en el desarrollo de software real, los hilos rara vez trabajan en un vacío absoluto: casi siempre deben converger para interactuar con **recursos externos compartidos**:

* Escribir en un mismo archivo de texto o bitácora de disco.
* Modificar registros en una base de datos local SQLite.
* Enviar comandos a un puerto de comunicación o periférico.
* Imprimir salidas formateadas en la consola sin que los textos se solapen o mezclen.

Cuando dos o más hilos intentan leer, modificar o escribir sobre un mismo recurso simultáneamente, se desata el problema más peligroso de la computación concurrente: **la condición de carrera (*Race Condition*)**.

Para dominar este problema, Latino.NET incorpora el plugin **`hilos`**, que proporciona exclusión mutua (`mutex`).

### 11.3.1 La Condición de Carrera y la Sección Crítica

Imagina que dos hilos intentan registrar ventas simultáneamente leyendo y actualizando un archivo que almacena el saldo de una caja:

```text
Hilo A lee saldo: $1000
                                Hilo B lee saldo: $1000
Hilo A suma venta de $200
Hilo A guarda saldo: $1200
                                Hilo B suma venta de $300
                                Hilo B guarda saldo: $1300 💥 (¡La venta de A se perdió!)
```

El resultado final es `$1300` en lugar de `$1500`: **los datos se han corrompido silenciosamente**.

La porción de código donde se accede y manipula un recurso compartido se denomina **Sección Crítica**. La regla fundamental de la concurrencia establece que **únicamente un hilo a la vez puede estar dentro de una sección crítica**.

### 11.3.2 Ciclo de Vida del Mutex en Latino.NET

El plugin `mutex` (incluido automáticamente al hacer `incluir("hilos")`) implementa cerrojos de exclusión mutua de alto rendimiento delegando en la clase `Monitor` de .NET 10.

```latino
incluir("hilos")

# 1. Creación del cerrojo (retorna un identificador único GUID)
miCerrojo = mutex.crear()

# 2. Adquisición del bloqueo (Sección Crítica)
mutex.bloquear(miCerrojo)

# --- INICIO DE LA SECCIÓN CRÍTICA ---
# Solo el hilo que ejecutó bloquear() puede estar aquí.
# Si otro hilo intenta bloquear(), se pausará hasta que este hilo libere.
escribir("Operación atómica sobre recurso compartido...")
# --- FIN DE LA SECCIÓN CRÍTICA ---

# 3. Liberación del bloqueo
mutex.liberar(miCerrojo)

# 4. Destrucción del cerrojo al terminar la aplicación
mutex.eliminar(miCerrojo)
```

#### Anatomía de las Funciones Nativas de `mutex`

| Función | Parámetros | Comportamiento en Runtime |
| :--- | :--- | :--- |
| `mutex.crear()` | *Ninguno* | Registra un objeto de bloqueo en memoria y retorna su `id` único. |
| `mutex.bloquear(id)` | `idMutex` | Adquiere el cerrojo exclusivo. Si otro hilo lo retiene, pausa la ejecución hasta que quede libre. |
| `mutex.liberar(id)` | `idMutex` | Cede el cerrojo. Si el hilo actual no es el propietario del bloqueo, lanza un `RuntimeError` defensivo. |
| `mutex.eliminar(id)` | `idMutex` | Libera el objeto de memoria. Si el cerrojo está bloqueado actualmente por algún hilo, **rechaza la eliminación lanzando un error**. |

### 11.3.3 Patrón Idiomático: Liberación Segura sin `finally`

En Latino.NET rige la **Invariante Técnica #9: no existe la palabra clave `finally` ni `finalmente`**.

Esto plantea un desafío vital para el arquitecto de software:  
*¿Qué ocurre si dentro de la sección crítica ocurre un error inesperado (por ejemplo, disco lleno o división por cero) antes de llegar a `mutex.liberar()`?*

Si el error interrumpe el flujo, el cerrojo quedará bloqueado para siempre en la memoria de la máquina virtual, provocando que todos los demás hilos se congelen eternamente esperando un recurso que jamás se liberará (*Orphaned Lock / Deadlock*).

#### El Patrón de Doble Liberación Defensiva

En Latino.NET, toda sección crítica propensa a fallos debe protegerse envolviendo la lógica en `intentar ... atrapar`:

```latino
funcion SeccionCriticaSegura(idMutex, rutaArchivo, lineaTexto)
    # 1. Adquirir el cerrojo
    mutex.bloquear(idMutex)
    
    # 2. Proteger la ejecución crítica
    intentar
        archivo.anexar(rutaArchivo, lineaTexto .. "\n")
        
        # Liberación obligatoria en caso de ÉXITO
        mutex.liberar(idMutex)
        retornar cierto
    atrapar (error)
        # Liberación obligatoria en caso de ERROR antes de propagar
        mutex.liberar(idMutex)
        
        # Propagar el error o registrar el fallo
        lanzar("Fallo crítico en archivo compartido: " .. error)
    fin
fin
```

> 💡 **Regla de Oro:** Siempre que adquieras un cerrojo con `mutex.bloquear()`, asegúrate de que exista una ruta de ejecución en el bloque `atrapar` que invoque `mutex.liberar()`.

### 11.3.4 Caso Práctico: Escritura Concurrente en Archivo de Bitácora

En el siguiente ejemplo completo y ejecutable, lanzamos **3 hilos en paralelo** para escribir 15 líneas en un mismo archivo de registro compartido. Gracias al `mutex`, las líneas se graban limpiamente sin colisiones de I/O ni archivos dañados:

```latino
incluir("hilos")

# 1. Inicialización de recursos compartidos
RUTA_LOG = "auditoria_concurrente.txt"
archivo.escribir(RUTA_LOG, "=== INICIO DE REGISTRO DE EVENTOS ===\n")

# Creamos el cerrojo que protegerá el archivo de disco
cerrojoArchivo = mutex.crear()

# 2. Subrutina que ejecutarán los trabajadores concurrentes
funcion RegistrarEventos(nombreTrabajador, totalEventos, idMutex, rutaDestino)
    para i en rango(1, totalEventos + 1)
        # Pausa aleatoria para desordenar los hilos intencionalmente
        hilos.dormir(50)
        
        estampa = sis.tiempo("%H:%M:%S")
        mensaje = ç("[|estampa|] [|nombreTrabajador|] Evento de transaccion #|i|")
        
        # --- SECCIÓN CRÍTICA BLINDADA ---
        mutex.bloquear(idMutex)
        intentar
            archivo.anexar(rutaDestino, mensaje .. "\n")
            mutex.liberar(idMutex)
        atrapar (e)
            mutex.liberar(idMutex)
            escribir("Error en hilo: " .. e)
        fin
    fin
    retornar ç("Trabajador |nombreTrabajador| finalizo sus registros.")
fin

escribir("Lanzando 3 hilos concurrentes escribiendo en el mismo archivo...")

# 3. Lanzamos los tres trabajadores sobre los núcleos de la CPU
h1 = hilos.lanzar(RegistrarEventos, "HILO_ALFA",  5, cerrojoArchivo, RUTA_LOG)
h2 = hilos.lanzar(RegistrarEventos, "HILO_BETA",  5, cerrojoArchivo, RUTA_LOG)
h3 = hilos.lanzar(RegistrarEventos, "HILO_GAMMA", 5, cerrojoArchivo, RUTA_LOG)

# 4. Esperamos la recolección de los tres hilos
reportes = hilos.esperarTodos([h1, h2, h3])

para rep en reportes
    escribir("  ✅ " .. rep)
fin

# 5. Destrucción segura del cerrojo
mutex.eliminar(cerrojoArchivo)

escribir("\n=== CONTENIDO FINAL DEL ARCHIVO (SIN CORRUPCIÓN) ===")
contenido = archivo.leer(RUTA_LOG)
escribir(contenido)

# Limpieza del archivo de prueba
archivo.borrar(RUTA_LOG)
```

**Salida en consola:**

```text
Lanzando 3 hilos concurrentes escribiendo en el mismo archivo...
  ✅ Trabajador HILO_ALFA finalizo sus registros.
  ✅ Trabajador HILO_BETA finalizo sus registros.
  ✅ Trabajador HILO_GAMMA finalizo sus registros.

# Salida variable (depende del momento de ejecución y hardware):
=== CONTENIDO FINAL DEL ARCHIVO (SIN CORRUPCIÓN) ===
=== INICIO DE REGISTRO DE EVENTOS ===
[10:15:02] [HILO_ALFA] Evento de transaccion #1
[10:15:02] [HILO_BETA] Evento de transaccion #1
[10:15:02] [HILO_GAMMA] Evento de transaccion #1
[10:15:03] [HILO_BETA] Evento de transaccion #2
[10:15:03] [HILO_ALFA] Evento de transaccion #2
...
```

### 11.3.5 Prevención de Interbloqueos (*Deadlocks*)

El riesgo más insidioso al trabajar con exclusión mutua es el **Interbloqueo (*Deadlock*)**: una parálisis total del sistema provocada cuando dos o más hilos quedan esperando mutuamente por recursos retenidos por el otro.

```text
┌────────────────────────────────────────────────────────┐
│              EL ABRAZO MORTAL (DEADLOCK)               │
├────────────────────────────────────────────────────────┤
│ Hilo A tiene Cerrojo 1  ─────►  Quiere Cerrojo 2       │
│                                       ▲                │
│                                       │ (Esperando)    │
│                                       ▼                │
│ Hilo B tiene Cerrojo 2  ─────►  Quiere Cerrojo 1       │
│                                                        │
│ RESULTADO: Ambos hilos se congelan para siempre.       │
└────────────────────────────────────────────────────────┘
```

#### La Regla de Adquisición Jerárquica Ordenada

Para prevenir deadlocks matemáticamente sin necesidad de algoritmos complejos, aplica siempre esta regla arquitectónica:

> 🔒 **Si tu programa necesita adquirir más de un mutex simultáneamente, todos los hilos del sistema deben adquirirlos exactamente en el mismo orden jerárquico (por ejemplo: siempre el Mutex A antes que el Mutex B).**

Al forzar un orden global de adquisición, ningún hilo podrá retener un recurso secundario mientras disputa el primero, eliminando la posibilidad física del ciclo de bloqueo.

Los mutexes proporcionan un control granular excelente sobre recursos del sistema operativo. Sin embargo, compartir memoria y sincronizar bloqueos manuales suele ser propenso a errores. La ciencia de la computación moderna resolvió esto con un enfoque infinitamente más limpio y desacoplado: **los canales de comunicación (`canal`)**.

## 🎫 11.4 Comunicación por Paso de Mensajes: Canales (`canal`)

En la sección anterior vimos cómo proteger la memoria compartida mediante exclusión mutua (`mutex`). Sin embargo, coordinar cerrojos manuales en sistemas con decenas de hilos puede convertirse en una pesadilla de ingeniería: un solo bloqueo olvidado provoca corrupción de datos, y un cerrojo cruzado paraliza el sistema en un interbloqueo (*deadlock*).

Para evitar estos dolores de cabeza, la ciencia de la computación moderna adoptó un paradigma alternativo formulado originalmente por C.A.R. Hoare en 1978: **CSP (*Communicating Sequential Processes*)**, popularizado a nivel mundial por el lenguaje **Go**.

La máxima de este paradigma es contundente:
> 🔒 **«No te comuniques compartiendo memoria; comparte memoria comunicándote.»**

En Latino.NET, este modelo se implementa a través del plugin oficial **`hilos`**, que proporciona canales.

### 11.4.1 La Filosofía de los Canales en Latino.NET

Un **Canal** es un conducto unidireccional y seguro para hilos (*Thread-Safe Queue*) que conecta productores de datos con consumidores de datos:

```text
┌──────────────┐                                           ┌──────────────┐
│ HILO 1       │                                           │ HILO 2       │
│ (Productor)  │ ────► canal.enviar(ch, dato) ────► [CANAL] ────► canal.recibir(ch) ────►│ (Consumidor) │
└──────────────┘                                           └──────────────┘
```

#### Ventajas Arquitectónicas del Canal sobre el Mutex

1. **Cero Cerrojos Manuales:** El canal administra internamente la exclusión mutua utilizando `BlockingCollection` en .NET 10. No necesitas llamar a `bloquear()` ni `liberar()`.
2. **Desacoplamiento Temporal:** El productor puede enviar un mensaje y continuar su trabajo de inmediato sin esperar a que el consumidor esté listo.
3. **Seguridad contra Condiciones de Carrera:** Los datos viajan como mensajes aislados de un hilo a otro, eliminando la posibilidad física de que dos hilos editen la misma variable al mismo tiempo.

### 11.4.2 Anatomía del Plugin `canal`

Al incluir el plugin de concurrencia (`incluir("hilos")`), el objeto global `canal` queda automáticamente habilitado con cinco operaciones primitivas:

| Función | Parámetros | Comportamiento en Runtime |
| :--- | :--- | :--- |
| `canal.crear()` | *Ninguno* | Instancia una cola segura entre hilos en memoria y retorna su `id` único (GUID). |
| `canal.enviar(id, dato)` | `idCanal, dato` | Inserta un dato en la cola del canal de forma atómica. Si el canal fue cerrado previamente, lanza un `RuntimeError`. |
| `canal.recibir(id, [ms])` | `idCanal, [timeout]` | Extrae el siguiente dato disponible en orden FIFO. Bloquea el hilo hasta que llegue un dato o venza el timeout. |
| `canal.cerrar(id)` | `idCanal` | Marca el canal como cerrado: no acepta nuevos envíos, pero permite a los consumidores drenar los mensajes restantes. |
| `canal.eliminar(id)` | `idCanal` | Cierra el canal, espera a que los consumidores reaccionen y destruye la cola liberando su memoria RAM. |

### 11.4.3 El Patrón Productor-Consumidor Clásico

El caso de uso fundamental de un canal es coordinar dos hilos con roles especializados:

1. **El Productor:** Lee datos, captura métricas o consulta APIs en bucle y los deposita en el canal con `canal.enviar()`. Al terminar, clausura la transmisión con `canal.cerrar()`.
2. **El Consumidor:** Permanece escuchando el canal con `canal.recibir()`. Procesa cada elemento tan pronto llega. Cuando el canal se vacía y se cierra, `canal.recibir()` retorna **`nulo`**, señal inequívoca de que debe terminar su ciclo.

#### Código Ejecutable Completo

```latino
incluir("hilos")

# 1. Creación del canal de comunicación
tuberia = canal.crear()

# 2. Subrutina del Productor
funcion Productor(idCanal, cantidadItems)
    escribir("   [Productor] Iniciando generacion de paquetes...")
    para i en rango(1, cantidadItems + 1)
        # Simulamos tiempo de captura de datos
        hilos.dormir(80)
        
        paquete = ç("PAQUETE_DATOS_#|i|")
        canal.enviar(idCanal, paquete)
        escribir("   [Productor] Enviado: " .. paquete)
    fin
    
    # Señalizamos que no habra mas datos
    canal.cerrar(idCanal)
    escribir("   [Productor] Transmision cerrada.")
    retornar cierto
fin

# 3. Subrutina del Consumidor
funcion Consumidor(idCanal, nombreConsumidor)
    escribir("   [" .. nombreConsumidor .. "] A la espera de paquetes...")
    local procesados = 0
    
    mientras cierto
        # Espera bloqueante pacifica hasta que llegue un mensaje
        item = canal.recibir(idCanal)
        
        # Si el canal esta cerrado y ya no quedan datos, recibir() retorna nulo
        si item == nulo
            escribir("   [" .. nombreConsumidor .. "] Canal vacio y cerrado. Finalizando.")
            romper
        fin
        
        # Procesamiento del dato recibido
        procesados = procesados + 1
        escribirf("   [{0}] Procesando con exito: {1}\n", nombreConsumidor, item)
    fin
    
    retornar procesados
fin

escribir("Iniciando arquitectura Productor-Consumidor...")

# 4. Lanzamos ambos trabajadores en hilos independientes
idProductor  = hilos.lanzar(Productor, tuberia, 4)
idConsumidor = hilos.lanzar(Consumidor, tuberia, "TRABAJADOR_BETA")

# 5. Esperamos la culminacion de ambos hilos
hilos.esperar(idProductor)
totalProcesado = hilos.esperar(idConsumidor)

# 6. Destruccion higienica del canal
canal.eliminar(tuberia)

escribir("===========================================")
escribir("Flujo concurrente completado exitosamente.")
escribir("Total de paquetes procesados: " .. totalProcesado)
```

**Salida en consola:**

```text
Iniciando arquitectura Productor-Consumidor...
   [Consumidor] A la espera de paquetes...
   [Productor] Iniciando generacion de paquetes...
   [Productor] Enviado: PAQUETE_DATOS_#1
   [TRABAJADOR_BETA] Procesando con exito: PAQUETE_DATOS_#1
   [Productor] Enviado: PAQUETE_DATOS_#2
   [TRABAJADOR_BETA] Procesando con exito: PAQUETE_DATOS_#2
   [Productor] Enviado: PAQUETE_DATOS_#3
   [TRABAJADOR_BETA] Procesando con exito: PAQUETE_DATOS_#3
   [Productor] Enviado: PAQUETE_DATOS_#4
   [TRABAJADOR_BETA] Procesando con exito: PAQUETE_DATOS_#4
   [Productor] Transmision cerrada.
   [TRABAJADOR_BETA] Canal vacio y cerrado. Finalizando.
===========================================
Flujo concurrente completado exitosamente.
Total de paquetes procesados: 4
```

### 11.4.4 Recepción No Bloqueante con Tiempo Límite (*Timeouts*)

En muchas aplicaciones de red o interfaces interactivas, un consumidor no puede permitirse quedar congelado indefinidamente si el productor deja de emitir datos o sufre una caída de conexión.

Para estos escenarios, `canal.recibir(idCanal, timeoutMs)` acepta un segundo parámetro en milisegundos:

* Si llega un dato antes del vencimiento: lo retorna inmediatamente.
* Si el tiempo expira sin mensajes: **retorna `nulo`** sin trabar el hilo, permitiendo ejecutar tareas de mantenimiento, registrar advertencias o reintentar.

```latino
incluir("hilos")

canalSensores = canal.crear()

# Intentamos leer con un timeout estricto de 300 milisegundos
lectura = canal.recibir(canalSensores, 300)

si lectura == nulo
    escribir("⚠️ Alerta: El sensor no envio telemetria en 300 ms. Verificando hardware...")
sino
    escribir("Telemetria recibida: " .. lectura)
fin

canal.eliminar(canalSensores)
```

### 11.4.5 Arquitectura de Piscina de Trabajadores (*Worker Pool*)

Una de las mayores fortalezas de los canales en Latino.NET es que soportan de forma nativa la concurrencia de **múltiples consumidores leyendo del mismo canal** sin colisiones ni duplicación de trabajo (*Work Stealing*).

Si lanzas tres hilos consumidores escuchando la misma tubería:

1. El productor deposita 100 tareas en el canal.
2. Cada consumidor que queda libre extrae la siguiente tarea de la fila automáticamente.
3. Ninguna tarea se procesa dos veces.
4. Cuando el canal se cierra, los tres consumidores detectan el `nulo` y se apagan en orden.

```latino
incluir("hilos")

colaTrabajo = canal.crear()

funcion Trabajador(idCanal, idNumero)
    local tareasHechas = 0
    mientras cierto
        tarea = canal.recibir(idCanal)
        si tarea == nulo romper fin
        
        # Procesa la tarea asignada
        hilos.dormir(40)
        tareasHechas = tareasHechas + 1
    fin
    retornar {"trabajador": idNumero, "total": tareasHechas}
fin

# Despachamos 3 trabajadores compitiendo por la misma cola
w1 = hilos.lanzar(Trabajador, colaTrabajo, 1)
w2 = hilos.lanzar(Trabajador, colaTrabajo, 2)
w3 = hilos.lanzar(Trabajador, colaTrabajo, 3)

# El hilo principal inyecta 15 tareas en el canal
para i en rango(1, 16)
    canal.enviar(colaTrabajo, ç("TAREA_#|i|"))
fin

# Cerramos el canal para indicar que no hay mas trabajo
canal.cerrar(colaTrabajo)

# Esperamos a que los tres trabajadores concluyan
balance = hilos.esperarTodos([w1, w2, w3])

escribir("=== DISTRIBUCIÓN DE CARGA DEL WORKER POOL ===")
para b en balance
    escribirf("  Obrero {0} completo {1} tareas.\n", b["trabajador"], b["total"])
fin

canal.eliminar(colaTrabajo)
```

**Salida típica:**

```text
=== DISTRIBUCIÓN DE CARGA DEL WORKER POOL ===
  Obrero 1 completo 5 tareas.
  Obrero 2 completo 5 tareas.
  Obrero 3 completo 5 tareas.
```

Los canales y los hilos internos permiten aprovechar al máximo los recursos de la máquina virtual de Latino.NET. Sin embargo, en ocasiones el sistema necesita interactuar con el entorno exterior: ejecutar scripts en otros lenguajes, compilar binarios o consultar utilidades del sistema operativo sin bloquear la ejecución. Para ello, entra en juego **el plugin de procesos asíncronos (`proceso`)**.

## ⚙️ 11.5 Procesos Asíncronos del Sistema Operativo (`proceso`)

Hasta este punto hemos explorado la concurrencia que ocurre **dentro de la máquina virtual de Latino.NET**: hilos internos despachados sobre el ThreadPool, cerrojos de exclusión mutua (`mutex`) y canales de mensajes en memoria (`canal`).

Sin embargo, en la ingeniería de sistemas moderna, un programa rara vez vive aislado: con frecuencia debe orquestar herramientas externas del sistema operativo:

* Ejecutar comandos de control de versiones (`git`).
* Transcodificar video o audio con herramientas como `ffmpeg`.
* Lanzar compilaciones, contenedores `docker` o scripts legados en Python, Bash o PowerShell.
* Realizar diagnósticos de red extensos (`ping`, `traceroute`).

Si ejecutas un comando externo de forma síncrona ordinaria (por ejemplo, con `sis.ejecutar()`), **el hilo principal de Latino.NET se congela por completo** hasta que el programa externo concluya. Si ese proceso tarda 30 segundos, tu aplicación dejará de responder a eventos, no procesará canales y parecerá colgada.

Para gobernar el paralelismo a nivel del sistema operativo sin bloquear la ejecución, Latino.NET incorpora el plugin oficial **`proceso`**.

### 11.5.1 Concurrencia de Procesos frente a Concurrencia de Hilos

Es indispensable comprender la frontera arquitectónica entre ambos enfoques:

```text
┌────────────────────────────────────────────────────────────────────────┐
│                   HILOS INTERNOS vs. PROCESOS DEL SO                   │
├───────────────────────────────────┬────────────────────────────────────┤
│ HILOS (`hilos.lanzar`)            │ PROCESOS (`proceso.ejecutarAsync`) │
├───────────────────────────────────┼────────────────────────────────────┤
│ • Corre dentro de Latino.NET.     │ • Binario externo independiente    │
│ • Comparte recursos del proceso.  │   del sistema operativo.           │
│ • Comunicación ultrarrápida       │ • Espacio de memoria protegido por │
│   mediante canales en RAM.        │   el kernel del SO.                │
│ • Tareas de código Latino.NET.    │ • Orquestación de herramientas CLI │
│                                   │   y utilidades externas.           │
└───────────────────────────────────┴────────────────────────────────────┘
```

### 11.5.2 Carga y Operaciones del Plugin `proceso`

El plugin se activa mediante `incluir("proceso")`:

```latino
incluir("proceso")
```

#### Catálogo de Funciones de Concurrencia de Procesos

| Función | Parámetros | Comportamiento en Runtime |
| :--- | :--- | :--- |
| `proceso.ejecutar(cmd, [args], [ms])` | `cmd, [args], [timeout]` | Ejecución síncrona protegida con timeout (defecto: 30s). Retorna `{salida, error, codigo, timeout}`. |
| `proceso.shell(cmdString, [ms])` | `cmdString, [timeout]` | Ejecuta a través de `/bin/bash -c` o `cmd.exe /C`. Soporta tuberías (`\|`) y redirecciones (`>`). |
| `proceso.ejecutarAsync(cmd, [args])` | `cmd, [args]` | **Lanza el proceso en segundo plano.** Retorna inmediatamente un `id` interno numérico. |
| `proceso.terminado(id)` | `idInterno` | Sondeo no bloqueante: retorna `cierto` si el proceso externo ya finalizó. |
| `proceso.resultado(id)` | `idInterno` | Recupera `{salida, error, codigo}` si terminó; retorna `nulo` si sigue corriendo. |
| `proceso.esperar(id, [ms])` | `idInterno, [timeout]` | Pausa el script hasta que el proceso termine o venza el timeout. Retorna el diccionario de salida. |
| `proceso.listar()` | *Ninguno* | Retorna lista de diccionarios de procesos del SO: `[{pid, nombre, memoria_mb}, ...]`. |
| `proceso.matar(pid)` | `pidReal` | Termina el proceso del sistema operativo con ese PID. Usa un PID propio; finalizar procesos de otros programas requiere privilegios elevados y no es portable. Envía `SIGKILL` en Unix o `TerminateProcess` en Windows. |

### 11.5.3 Ejecución Asíncrona No Bloqueante (`proceso.ejecutarAsync`)

La función `proceso.ejecutarAsync()` despacha el comando al sistema operativo e inicia la captura en segundo plano de los flujos de salida estándar (`STDOUT`) y error (`STDERR`).

A diferencia de las funciones síncronas, retorna de inmediato un **identificador interno de tarea**:

```latino
incluir("proceso")
incluir("hilos")

# Comando portable para simular una tarea de 2 segundos
# En Windows: ping a localhost; en Linux/Mac: sleep
esWindows = sis.operativo("WINDOWS")
comando   = esWindows ? "ping" : "sleep"
argumentos = esWindows ? ["-n", "3", "127.0.0.1"] : ["2"]

escribir("1. Despachando proceso externo en segundo plano...")

# El proceso arranca pero NO congela a Latino.NET
idTarea = proceso.ejecutarAsync(comando, argumentos)

escribir("2. El script de Latino.NET sigue libre ejecutando código.")
escribir("   ID de tarea asignado por el gestor de procesos: " .. idTarea)

# 3. Bucle de sondeo reactivo (Polling)
contadorVueltas = 0
mientras no proceso.terminado(idTarea)
    hilos.dormir(400) # Pausa de 400 ms en el hilo de Latino
    contadorVueltas = contadorVueltas + 1
    escribirf("   [Latino.NET activo] Trabajando... vuelta de reloj #{0}\n", contadorVueltas)
fin

escribir("3. El comando externo ha finalizado. Recuperando salida...")

# 4. Recuperación del resultado consolidado
resultado = proceso.resultado(idTarea)

escribir("Código de salida del proceso: " .. resultado["codigo"])
escribir("Longitud de la salida capturada: " .. cadena.longitud(resultado["salida"]) .. " caracteres.")
```

**Salida en consola:**

```text
1. Despachando proceso externo en segundo plano...
2. El script de Latino.NET sigue libre ejecutando código.
   ID de tarea asignado por el gestor de procesos: 1
   [Latino.NET activo] Trabajando... vuelta de reloj #1
   [Latino.NET activo] Trabajando... vuelta de reloj #2
   [Latino.NET activo] Trabajando... vuelta de reloj #3
   [Latino.NET activo] Trabajando... vuelta de reloj #4
   [Latino.NET activo] Trabajando... vuelta de reloj #5
3. El comando externo ha finalizado. Recuperando salida...
Código de salida del proceso: 0
Longitud de la salida capturada: 215 caracteres.
```

> ⚠️ **Regla de Memoria en `proceso.resultado()`:**  
> Por higiene de recursos, al invocar `proceso.resultado(id)` o `proceso.esperar(id)`, el motor entrega los datos y **purga automáticamente el registro interno**. Una segunda llamada con el mismo identificador retornará `nulo`.

### 11.5.4 Argumentos Seguros: Listas frente a Cadenas

`proceso.ejecutar()` y `proceso.ejecutarAsync()` admiten dos formas de especificar los argumentos:

1. **Como cadena única (`"archivo copia.txt destino"`):** El motor separa los argumentos por espacios en blanco. Si una ruta contiene espacios (ej. `"C:\Mis Documentos\archivo.txt"`), el sistema operativo la interpretará erróneamente como dos parámetros distintos.
2. **Como lista estructurada (`["archivo copia.txt", "destino"]`):** **La forma profesional y recomendada**. El motor de Latino.NET encapsula automáticamente con comillas dobles los argumentos que contienen espacios en blanco, garantizando que el sistema operativo reciba cada parámetro intacto.

```latino
incluir("proceso")

# ✅ FORMA SEGURA: Cada elemento de la lista es un argumento independiente
argumentos = [
    "checkout",
    "-b",
    "feature/nueva-arquitectura con espacios"
]

# El motor construye: git checkout -b "feature/nueva-arquitectura con espacios"
id = proceso.ejecutarAsync("git", argumentos)
proceso.esperar(id)
```

### 11.5.5 Ejecución por Shell con Tuberías del Sistema (`proceso.shell`)

Cuando necesitas invocar construcciones exclusivas de la consola del sistema operativo —como encadenar comandos con tuberías (`|`), redireccionar salidas a archivos (`>`), o expandir variables de entorno del shell (`$HOME` o `%USERPROFILE%`)— debes utilizar `proceso.shell()`:

```latino
incluir("proceso")

# Comando con tubería de shell (Pipe nativo de Bash o CMD)
comandoShell = sis.operativo("WINDOWS")
    ? "dir . | findstr /I lat"
    : "ls . | grep -E '\\.lat$'"

escribir("Ejecutando pipeline a través del shell nativo del SO...")
res = proceso.shell(comandoShell, 10000) # Timeout de 10 segundos

si res["timeout"]
    escribir("⚠️ El comando excedió el tiempo límite y fue abortado.")
sino
    escribir("Código de salida: " .. res["codigo"])
    escribir("Salida capturada:")
    escribir(res["salida"])
fin
```

> 🔒 **Advertencia de Seguridad (Inyección de Shell):**  
> `proceso.shell()` invoca directamente el intérprete de comandos (`cmd.exe` o `/bin/bash`). **Jamás concatenes texto ingresado por un usuario no confiable dentro de un comando shell**, ya que un atacante podría inyectar comandos maliciosos mediante caracteres como `;`, `&&` o `|`. Para entradas dinámicas externas, utiliza siempre `proceso.ejecutar(cmd, listaArgumentos)`.

### 11.5.6 Inspección y Terminación de Procesos del SO (`listar` y `matar`)

El plugin `proceso` no solo controla los procesos que él mismo crea; también puede actuar como un monitor de telemetría del sistema operativo:

#### 1. Auditoría de Memoria con `proceso.listar()`

Retorna una lista de diccionarios con todos los procesos activos en la máquina, su PID del sistema operativo y su consumo de memoria RAM en megabytes:

```latino
incluir("proceso")

procesos = proceso.listar()
escribir("Total de procesos activos en el SO: " .. lista.longitud(procesos))

escribirf("\n{0,-8} | {1,-30} | {2,10}\n", "PID", "NOMBRE", "RAM (MB)")
escribir("-----------------------------------------------------")

# Mostramos los primeros 5 procesos encontrados
para i en rango(mate.min(5, lista.longitud(procesos)))
    p = procesos[i]
    escribirf("{0,-8} | {1,-30} | {2,10:F1}\n", p["pid"], p["nombre"], p["memoria_mb"])
fin
```

#### 2. Terminación de Procesos con `proceso.matar(pid)`

Si un proceso en segundo plano se descontrola o queda en bucle infinito, puedes terminarlo mediante su PID real del sistema operativo. Recuerda que esta función actúa sobre procesos del sistema, no sobre los identificadores internos de `proceso.ejecutarAsync`: necesita privilegios elevados para afectar a programas de otros usuarios, y su comportamiento no es idéntico en todos los sistemas:

```latino
incluir("proceso")

# ⚠️ IMPORTANTE: nunca uses un PID inventado en un ejemplo copiable.
# Si ese número existe, estarías matando el proceso de otro usuario.
# La forma segura es un PID que sabes que no existe, dentro de un 'intentar'.
pidHuerfano = 999999   # por encima del rango habitual de PIDs del sistema

intentar
    # Termina el proceso inmediatamente vía kernel
    proceso.matar(pidHuerfano)
    escribir("Proceso neutralizado con éxito.")
atrapar (error)
    escribir("No se pudo terminar el proceso (quizá ya terminó o falta permiso): " .. error)
fin
```

Hemos dominado las cuatro fuerzas de la concurrencia en Latino.NET: tareas paralelas (`hilos`), cerrojos de memoria (`mutex`), canales desacoplados (`canal`) y procesos externos (`proceso`).

En la siguiente sección uniremos todas estas piezas para construir **tres arquitecturas concurrentes completas de nivel industrial**.

## 🏗️ 11.6 Patrones de Arquitectura Concurrente en la Práctica

Conocer las funciones nativas es apenas el primer paso. La verdadera maestría de un ingeniero de software radica en saber **ensamblar esas primitivas para resolver problemas de arquitectura a gran escala**.

En esta sección estudiaremos tres patrones concurrentes canónicos, implementados mediante código 100% autosuficiente y ejecutable en Latino.NET v1.0.81.

### 11.6.1 Patrón 1: Procesamiento Masivo en Paralelo (Fan-Out / Fan-In)

**El Problema:**  
Imagina que recibes un lote de archivos de datos (o particiones de un archivo masivo divididas con `zip.cortar()`) y necesitas analizar sus registros, calcular sumas y obtener promedios. Si los procesas uno tras otro en un solo hilo, tardarás la suma del tiempo de cada archivo ($T_1 + T_2 + T_3$).

**La Solución:**

* **Fan-Out (Despliegue):** Lanzar un hilo independiente para cada archivo, permitiendo que la CPU procese los tres lotes simultáneamente en núcleos distintos.
* **Fan-In (Consolidación):** Sincronizar un único punto de reunión con `hilos.esperarTodos()`, recopilar las métricas individuales y totalizarlas en un reporte final.

🎯 **Ejemplo 1: Procesamiento masivo en paralelo (Patrón Fan-Out / Fan-In)**

```latino
incluir("hilos")

# 1. Preparar archivos de prueba para simular lotes de datos
archivo.escribir("lote_1.txt", "10\n20\n30\n40\n50")
archivo.escribir("lote_2.txt", "100\n200\n300")
archivo.escribir("lote_3.txt", "5\n15\n25\n35\n45\n55")

# 2. Subrutina que ejecutará cada trabajador
funcion AnalizarLote(rutaArchivo, idLote)
    lineas = archivo.lineas(rutaArchivo)
    suma = 0
    cantidad = lista.longitud(lineas)
    
    para linea en lineas
        suma = suma + anumero(linea)
    fin
    
    retornar {
        "id": idLote,
        "archivo": rutaArchivo,
        "cantidad": cantidad,
        "suma": suma,
        "promedio": cantidad > 0 ? (suma / cantidad) : 0
    }
fin

escribir("=== INICIANDO PROCESAMIENTO PARALELO (FAN-OUT) ===")
archivos = ["lote_1.txt", "lote_2.txt", "lote_3.txt"]
idsHilos = [ ]

para i en rango(lista.longitud(archivos))
    # Despachamos cada archivo a un hilo independiente
    id = hilos.lanzar(AnalizarLote, archivos[i], i + 1)
    lista.agregar(idsHilos, id)
fin

# 3. Punto de sincronización y recolección (FAN-IN)
resultados = hilos.esperarTodos(idsHilos)

escribir("\n=== CONSOLIDACIÓN DE RESULTADOS (FAN-IN) ===")
sumaTotal = 0
elementosTotales = 0

para r en resultados
    escribirf("Lote {0} ({1}): {2} registros | Suma: {3} | Promedio: {4:F2}\n",
        r["id"], r["archivo"], r["cantidad"], r["suma"], r["promedio"])
    sumaTotal = sumaTotal + r["suma"]
    elementosTotales = elementosTotales + r["cantidad"]
fin

escribir("---------------------------------------------------------")
escribirf("Total Registros: {0} | Gran Total: {1}\n", elementosTotales, sumaTotal)

# Limpieza higiénica de archivos temporales
para arch en archivos
    archivo.borrar(arch)
fin
```

**Salida en consola:**

```text
=== INICIANDO PROCESAMIENTO PARALELO (FAN-OUT) ===

=== CONSOLIDACIÓN DE RESULTADOS (FAN-IN) ===
Lote 1 (lote_1.txt): 5 registros | Suma: 150 | Promedio: 30.00
Lote 2 (lote_2.txt): 3 registros | Suma: 600 | Promedio: 200.00
Lote 3 (lote_3.txt): 6 registros | Suma: 180 | Promedio: 30.00
---------------------------------------------------------
Total Registros: 14 | Gran Total: 930
```

### 11.6.2 Patrón 2: Piscina de Consumidores Desacoplada (Worker Pool con Canales)

**El Problema:**  
Tienes una fuente continua de tareas (por ejemplo, peticiones web entrantes o transacciones financieras) y no quieres lanzar un nuevo hilo por cada tarea individual (porque crear miles de hilos satura el sistema operativo).

**La Solución:**  
Crear un **número fijo de trabajadores persistentes** (por ejemplo, 2 o 4 obreros) que compitan de manera segura por las tareas que caen en un `canal`. Los obreros se reparten el trabajo automáticamente sin colisiones:

🎯 **Ejemplo 2: Piscina de consumidores desacoplada (Patrón Worker Pool)**

```latino
incluir("hilos")

canalTareas = canal.crear()

funcion Trabajador(idCanal, idWorker)
    local completadas = 0
    mientras cierto
        # Espera pasiva hasta que haya una tarea disponible
        tarea = canal.recibir(idCanal)
        
        # Si el canal está cerrado y ya no quedan elementos, salir del bucle
        si tarea == nulo
            romper
        fin
        
        # Simular procesamiento de la tarea
        hilos.dormir(30)
        completadas = completadas + 1
    fin
    retornar {"worker": idWorker, "procesadas": completadas}
fin

# 1. Lanzar 2 trabajadores en hilos independientes
w1 = hilos.lanzar(Trabajador, canalTareas, "Alpha")
w2 = hilos.lanzar(Trabajador, canalTareas, "Beta")

# 2. El hilo principal actúa como Productor inyectando 8 tareas
escribir("Productor enviando 8 tareas a la cola de trabajo...")
para i en rango(1, 9)
    canal.enviar(canalTareas, ç("TAREA-#|i|"))
fin

# 3. Cerrar el canal (señal de fin de trabajo)
canal.cerrar(canalTareas)

# 4. Esperar a que ambos obreros terminen de drenar el canal
reportes = hilos.esperarTodos([w1, w2])

escribir("=== BALANCE DE CARGA ENTRE TRABAJADORES ===")
para rep en reportes
    escribirf("  Trabajador {0} procesó {1} tareas de forma equilibrada.\n", 
        rep["worker"], rep["procesadas"])
fin

canal.eliminar(canalTareas)
```

**Salida en consola:**

```text
Productor enviando 8 tareas a la cola de trabajo...
=== BALANCE DE CARGA ENTRE TRABAJADORES ===
  Trabajador Alpha procesó 4 tareas de forma equilibrada.
  Trabajador Beta procesó 4 tareas de forma equilibrada.
```

### 11.6.3 Patrón 3: Bitácora Asíncrona Centralizada (Logger Concurrente)

**El Problema:**  
En aplicaciones empresariales concurrentes, las operaciones de disco son miles de veces más lentas que la memoria RAM. Si 5 hilos de negocio intentan escribir en el disco cada vez que registran una acción, se disputarán el cerrojo del archivo o sufrirán cuellos de botella de I/O.

**La Solución:**  
Un hilo dedicado exclusivamente a escribir en disco (**el hilo Logger**) que escucha un `canal`. Los hilos de negocio depositan sus mensajes en el canal en microsegundos y continúan su camino sin esperar al disco:

🎯 **Ejemplo 3: Bitácora asíncrona centralizada mediante canales (Patrón Logger)**

```latino
incluir("hilos")

RUTA_LOG = "sistema_asincrono.log"
canalLogs = canal.crear()

# 1. Hilo dedicado exclusivamente al I/O de disco
funcion ServicioLogger(idCanal, rutaDestino)
    archivo.escribir(rutaDestino, "=== BITÁCORA CENTRALIZADA DE AUDITORÍA ===\n")
    local totalLineas = 0
    
    mientras cierto
        mensaje = canal.recibir(idCanal)
        si mensaje == nulo
            romper # Canal cerrado y drenado
        fin
        
        archivo.anexar(rutaDestino, mensaje .. "\n")
        totalLineas = totalLineas + 1
    fin
    
    retornar totalLineas
fin

# Lanzamos el servicio de logging en segundo plano
idLogger = hilos.lanzar(ServicioLogger, canalLogs, RUTA_LOG)

# 2. Hilos de lógica de negocio que emiten logs sin congelarse
funcion OperacionNegocio(idCanal, nombreModulo, repeticiones)
    para i en rango(1, repeticiones + 1)
        hilos.dormir(20) # Simulación de trabajo
        estampa = sis.tiempo("%H:%M:%S")
        registro = ç("[|estampa|] [|nombreModulo|] Transacción #|i| completada.")
        
        # Enviar al canal en memoria (operación instantánea en RAM)
        canal.enviar(idCanal, registro)
    fin
    retornar cierto
fin

escribir("Lanzando módulos de Facturación e Inventario en paralelo...")
t1 = hilos.lanzar(OperacionNegocio, canalLogs, "FACTURACIÓN", 3)
t2 = hilos.lanzar(OperacionNegocio, canalLogs, "INVENTARIO",  3)

# Esperamos que los hilos de negocio concluyan
hilos.esperarTodos([t1, t2])

# Notificamos al logger que ya no habrá más entradas
canal.cerrar(canalLogs)

# Esperamos que el logger termine de vaciar la cola al archivo
lineasEscritas = hilos.esperar(idLogger)

canal.eliminar(canalLogs)

escribir("Servicio finalizado. Total de líneas persistidas: " .. lineasEscritas)
escribir("\n=== CONTENIDO FINAL DEL ARCHIVO EN DISCO ===")
escribir(archivo.leer(RUTA_LOG))

# Limpieza
archivo.borrar(RUTA_LOG)
```

**Salida en consola:**

```text
Lanzando módulos de Facturación e Inventario en paralelo...
Servicio finalizado. Total de líneas persistidas: 6

=== CONTENIDO FINAL DEL ARCHIVO EN DISCO ===
=== BITÁCORA CENTRALIZADA DE AUDITORÍA ===
[10:30:15] [FACTURACIÓN] Transacción #1 completada.
[10:30:15] [INVENTARIO] Transacción #1 completada.
[10:30:15] [FACTURACIÓN] Transacción #2 completada.
[10:30:15] [INVENTARIO] Transacción #2 completada.
[10:30:15] [FACTURACIÓN] Transacción #3 completada.
[10:30:15] [INVENTARIO] Transacción #3 completada.
```

Estos tres patrones constituyen las piedras angulares del software concurrente moderno.

En la siguiente y última sección, cerraremos el capítulo repasando **las trampas, invariantes y reglas de oro** para blindar tus programas concurrentes contra errores silenciosos.

## 🛡️ 11.7 Trampas, Invariantes y Reglas de Oro de Concurrencia

La programación concurrente y paralela es una de las herramientas más potentes del software moderno, pero también la que más castiga los descuidos de diseño. Los errores en entornos multihilo suelen ser no deterministas: ocurren una vez cada mil ejecuciones, son casi imposibles de reproducir en depuradores paso a paso y pueden pasar desapercibidos durante semanas hasta colapsar un servidor en producción.

Para cerrar este capítulo con maestría, analizaremos las tres trampas críticas del desarrollo concurrente y estableceremos las **Reglas de Oro inquebrantables de Latino.NET**.

### 11.7.1 La Ilusión de la Memoria Compartida (Aislamiento por Diseño)

En lenguajes como C, C++ o Java, todos los hilos comparten por defecto el mismo espacio de memoria global. Si un hilo modifica una variable, el otro ve el cambio de inmediato (lo que suele derivar en condiciones de carrera catastróficas).

**En Latino.NET, los hilos NO comparten sus variables locales ni su entorno léxico.**

Cuando invocas `hilos.lanzar(MiFuncion)`, el motor crea una instancia independiente de `Interpreter` para ese hilo. Aunque el script padre tenga una variable llamada `contador`, el hilo secundario **no puede verla ni modificarla directamente**:

```latino
incluir("hilos")

# Variable en el entorno del hilo principal
contadorGlobal = 0

funcion TareaHijo()
    # ❌ ERROR CONCEPTUAL:
    # Esta asignación modifica únicamente una variable local interna del intérprete hijo.
    # Jamás alterará 'contadorGlobal' en el hilo principal.
    contadorGlobal = contadorGlobal + 100
    retornar contadorGlobal
fin

id = hilos.lanzar(TareaHijo)
hilos.esperar(id)

escribir("Valor de contadorGlobal en el padre: " .. contadorGlobal)
# Salida: Valor de contadorGlobal en el padre: 0 (¡No cambió!)
```

#### El Puente Correcto

Para comunicar datos entre hilos existen exclusivamente tres vías seguras:

1. **Entrada:** Pasar los datos como argumentos en `hilos.lanzar(funcion, arg1, arg2)`.
2. **Salida:** Retornar el valor desde la función del hilo y recogerlo con `hilos.esperar(id)`.
3. **Flujo Continuo:** Enviar y recibir mensajes a través de un `canal`.

### 11.7.2 La Anatomía del Interbloqueo (*Deadlock*) y su Neutralización

Un **Interbloqueo (*Deadlock*)** ocurre cuando dos o más hilos quedan congelados para siempre, esperando mutuamente por recursos retenidos por el otro.

#### El Caso Clásico de Bloqueo Cruzado

```latino
# HILO A:
mutex.bloquear(cerrojo1)
mutex.bloquear(cerrojo2) # 💥 Se congela esperando que B libere cerrojo2

# HILO B:
mutex.bloquear(cerrojo2)
mutex.bloquear(cerrojo1) # 💥 Se congela esperando que A libere cerrojo1
```

#### Técnicas Defensivas para Neutralizar Deadlocks

1. **Adquisición Jerárquica Global:** Si tu programa requiere dos o más cerrojos, **todos los hilos deben adquirirlos exactamente en el mismo orden** (siempre `cerrojo1` antes que `cerrojo2`).
2. **Canales con Tiempo Límite (*Timeouts*):** Al esperar mensajes en un canal donde el emisor podría fallar, utiliza siempre `canal.recibir(ch, timeoutMs)` en lugar de la espera bloqueante infinita.
3. **Liberación Defensiva:** Envuelve siempre las secciones críticas dentro de bloques `intentar ... atrapar` para que una excepción nunca deje un `mutex` bloqueado en la memoria.

### 11.7.3 Resiliencia de Trabajadores: Aislamiento de Excepciones

En muchas plataformas, si un hilo secundario sufre una excepción no controlada (por ejemplo, una división por cero o un archivo no encontrado), todo el proceso del sistema operativo se cae inmediatamente (*Crash* general).

**Latino.NET implementa Aislamiento Defensivo de Excepciones en el plugin `hilos`:**

Mira lo que ocurre internamente en `src/Latino.Plugin.Hilos/HilosPlugin.cs`:

* Si una función dentro de un hilo secundario arroja un `RuntimeError`, el subintérprete captura la falla en su frontera.
* Emite un diagnóstico en la salida estándar identificando los primeros 8 caracteres del GUID del hilo afectado:  
  `[Error en Hilo 4f8a12e9 Lín 12]: División por cero.`
* **El hilo principal NO se cae**: continúa su ejecución pacíficamente.
* La tarea se marca como finalizada y retorna `nulo` (`LatinoNull.Instance`).

```latino
incluir("hilos")

funcion TareaDefectuosa()
    hilos.dormir(100)
    # Provocamos un fallo intencional en el hilo secundario
    resultado = 10 / 0 
    retornar resultado
fin

escribir("1. Lanzando hilo con error intencional...")
id = hilos.lanzar(TareaDefectuosa)

# 2. El hilo principal sigue vivo y operativo
escribir("2. El programa principal no se congeló ni crasheó.")

# 3. Recolectamos el resultado con seguridad
valor = hilos.esperar(id)

si valor == nulo
    escribir("3. El trabajador falló de forma aislada y devolvió nulo.")
fin

escribir("4. La aplicación concluyó normalmente sin caerse.")
```

**Salida en consola:**

```text
1. Lanzando hilo con error intencional...
2. El programa principal no se congeló ni crasheó.

[Error en Hilo d8a1c4e2]: División por cero.
3. El trabajador falló de forma aislada y devolvió nulo.
4. La aplicación concluyó normalmente sin caerse.
```

### 📋 Las 5 Reglas de Oro de la Concurrencia en Latino.NET

Para diseñar arquitecturas concurrentes robustas y libres de fallos, mantén presente este decálogo de ingeniería:

1. **Prefiere Canales sobre Mutexes:**  
   Siempre que sea posible, comunica tus hilos mediante `canal.enviar()` y `canal.recibir()`. Es un modelo declarativo, libre de condiciones de carrera y matemáticamente superior a la sincronización manual por cerrojos.
2. **Usa Mutex solo para Recursos Físicos Reales:**  
   Reserva `mutex` exclusivamente para serializar accesos a recursos externos no concurrentes: escribir en un archivo plano de texto, ejecutar transacciones en SQLite o imprimir tablas alineadas en la consola.
3. **Todo Mutex Bloqueado DEBE Liberarse en `atrapar`:**  
   Dado que no existe `finally` en Latino.NET, cualquier sección crítica protegida con `mutex.bloquear(m)` debe contar con una llamada a `mutex.liberar(m)` tanto al final de la lógica exitosa como dentro del bloque `atrapar (e)`.
4. **Respeta la Sobrecarga de Creación de Hilos:**  
   No lances hilos para operaciones que tarden menos de 10 milisegundos. Para cálculos masivos, utiliza el patrón **Fan-Out / Fan-In** dividiendo el trabajo en un número de fragmentos proporcional a los núcleos lógicos de la máquina (`sis.plataforma()["procesadores"]`).
5. **Cierra los Canales al Terminar la Producción:**  
   El productor siempre debe invocar `canal.cerrar(ch)` tan pronto termine de emitir datos. Esto permite que los consumidores detecten el valor `nulo` y finalicen sus bucles ordenadamente sin quedar esperando para siempre.

## 📝 resumen del capítulo

En este capítulo hemos desbloqueado el verdadero potencial multinúcleo de Latino.NET sobre .NET 10:

* **Concurrencia sin GIL:** Latino.NET no posee bloqueos globales de intérprete; ejecuta tareas independientes sobre el ThreadPool y los núcleos reales del procesador.
* **El plugin `hilos`:** Permite despachar funciones en segundo plano (`lanzar`), recuperar sus resultados de forma síncrona o con timeout (`esperar`) y sincronizar lotes masivos en paralelo con `esperarTodos()`.
* **El plugin `mutex`:** Proporciona exclusión mutua para proteger secciones críticas sobre archivos y periféricos compartidos, evitando condiciones de carrera.
* **El plugin `canal`:** Implementa el modelo CSP de paso de mensajes mediante colas seguras entre hilos (*Thread-Safe*), ideal para arquitecturas de Productores y Consumidores sin memoria compartida.
* **El plugin `proceso`:** Permite orquestar binarios y utilidades externas del sistema operativo de forma asíncrona y no bloqueante con `proceso.ejecutarAsync()`.
