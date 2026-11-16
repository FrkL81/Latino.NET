# CAPÍTULO 12: Programación Orientada a Datos (DOP) y Persistencia

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores, estudiantes, analistas, científicos y creadores de software que necesiten conectar APIs web, procesar archivos (CSV, JSON), consultar bases de datos (SQLite) o manipular información masiva sin la rigidez de clases intermedias.  
**Propósito:** Dominar el ciclo de vida integral de la información en Latino.NET: desde la ingesta y validación de esquemas, hasta el procesamiento tabular, la seguridad criptográfica y la persistencia transaccional en disco.

## La Información en su Estado Puro

Durante décadas, la industria promovió el dogma de que la única forma «correcta» y profesional de escribir software era envolver cada dato dentro de una clase orientada a objetos (POO). Se creaban clases para los usuarios, clases para los productos, clases intermedias (*DTOs*, *Mappers*, *Entities*) y complejas jerarquías que ataban los datos a métodos rígidos.

Sin embargo, en el mundo real de la analítica, la automatización y los servicios modernos, los datos rara vez se comportan como entidades vivas:

- Llegan como textos tabulares en **CSV** o árboles jerárquicos en **JSON**.
- Vienen acompañados de estampas de tiempo (**fechas y horas**) que deben normalizarse sin alterar la zona horaria.
- Contienen campos confidenciales que exigen **hashes de integridad y firmas criptográficas**.
- Se almacenan por millones en tablas de **SQLite** para consultas analíticas instantáneas con SQL.

La **Programación Orientada a Datos (DOP - *Data-Oriented Programming*)** propone una alternativa mucho más ágil, transparente y cercana al sentido común: **tratar a la información como ciudadana de primera clase mediante estructuras genéricas simples (`dic`, `lista`, `tupla`), separándola por completo de las funciones que la transforman**.

A lo largo de este capítulo dominarás la tubería completa de datos en Latino.NET: desde su extracción en la red y validación defensiva, hasta su almacenamiento transaccional en bases de datos relacionales y compresión en disco.

> 💡 **Regla de Oro de Entrada y Salida (Mantra de Latino.NET):**  
> *"Procesa en memoria; escribe y consulta en bloque."*  
> Las operaciones de disco y red son miles de veces más lentas que el cómputo en memoria RAM. A lo largo de este capítulo aprenderás a transformar colecciones completas en memoria mediante DOP y a persistirlas en bloque utilizando transacciones atómicas (`sqlite.transaccion`), multiplicando la velocidad de tu software por órdenes de magnitud.

## 📊 12.1 Filosofía DOP: Separar el Código de los Datos

La Programación Orientada a Datos no es una negación de la POO ni de la Programación Funcional; es una **simplificación arquitectónica deliberada** nacida de las exigencias del software moderno. Mientras que la POO modela entidades vivas con identidad interna, la DOP reconoce que los registros de negocio viajan mejor cuando no están atados a moldes preconcebidos.

En esta sección examinaremos por qué el enfoque tradicional de clases colapsa ante flujos de información heterogéneos y cuáles son los cuatro principios que rigen el tratamiento de datos puros en Latino.NET antes de ejecutar tu primera consulta o archivo.

### 12.1.1 El Colapso de la Rigidez de Clases en el Manejo de Datos

Para comprender el valor de la Programación Orientada a Datos, examinemos el problema de modelar datos externos mediante clases tradicionales (POO).

Imagina que consumes un API web o lees un archivo CSV de clientes. Bajo el paradigma estricto de clases, estás obligado a:

1. Diseñar una `clase Cliente` con diez propiedades fijas en el constructor.
2. Si el proveedor del API añade un campo nuevo (`"descuento_promocional"`), tu clase no lo reconoce: debes modificar la clase, reescribir el constructor y recompilar.
3. Si solo necesitas dos campos para generar un reporte estadístico, tienes que instanciar objetos pesados con todas sus propiedades vacías o crear una segunda clase intermedia (`ClienteResumenDto`).

En Latino.NET, **un dato es simplemente un dato**. No necesita pertenecer a una clase para tener valor:

```latino
# Enfoque DOP: El dato es un valor puro, transparente e inspeccionable
cliente = {
    "id": 1045,
    "nombre": "Elena Ramos",
    "saldo": 3200.50,
    "activo": cierto
}

# Las funciones son transformadores que operan sobre datos genéricos
funcion CalcularImpuesto(registro)
    retornar registro["saldo"] * 0.16
fin

impuesto = CalcularImpuesto(cliente)
escribir("Impuesto calculado: $" .. impuesto)
```

Si mañana el proveedor de datos añade campos nuevos (`"email"`, `"ciudad"`), el diccionario de Latino.NET los absorbe de inmediato sin romper ninguna función existente.

### 12.1.2 Los Cuatro Principios de la Programación Orientada a Datos

La arquitectura DOP de Latino.NET descansa sobre cuatro principios fundamentales:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│           LOS 4 PRINCIPIOS DE LA PROGRAMACIÓN ORIENTADA A DATOS             │
├────────────────────────────┬────────────────────────────────────────────────┤
│ 1. SEPARACIÓN CÓDIGO/DATO  │ Las funciones son lógica pura sin estado;      │
│                            │ los datos son estructuras puras sin métodos.   │
├────────────────────────────┼────────────────────────────────────────────────┤
│ 2. ESTRUCTURAS GENÉRICAS   │ La información se representa con diccionarios  │
│                            │ (`dic`), listas (`lista`) y tuplas (`tupla`).  │
├────────────────────────────┼────────────────────────────────────────────────┤
│ 3. INMUTABILIDAD Y VALOR   │ Los datos se tratan como valores inalterables; │
│                            │ las modificaciones producen nuevos registros.  │
├────────────────────────────┼────────────────────────────────────────────────┤
│ 4. ESQUEMAS EN LÍMITES     │ La validación ocurre en las fronteras del      │
│                            │ sistema (al recibir o guardar), no en clases.  │
└────────────────────────────┴────────────────────────────────────────────────┘
```

#### Principio 1: Separar Código de Datos

En Latino.NET no encapsulamos funciones dentro de diccionarios de datos. Las funciones residen en módulos o procedimientos puros; los datos residen en estructuras universales. Esto permite que una sola función (como `archivo.escribirJson` o `lista.filtrar`) pueda operar sobre cualquier conjunto de datos del sistema sin adaptadores especiales.

#### Principio 2: Representar Datos con Estructuras Genéricas

En lugar de inventar decenas de clases específicas, modelamos el universo con tres primitivas:

- **`diccionario`:** Para registros con campos identificados por nombre (`{"folio": 1, "monto": 50}`).
- **`lista`:** Para secuencias ordenadas y tablas de registros (`[reg1, reg2, reg3]`).
- **`tupla`:** Para coordenadas inmutables o registros fijos heterogéneos (`("X", 100, cierto)`).

#### Principio 3: Datos como Valores Reproducibles

Al operar sobre datos, preferimos generar nuevos resultados o proyectar vistas antes que mutar destructivamente el registro original. Cuando una copia defensiva es necesaria, nos apoyamos en `clonar()` o en el paradigma de cero copias con `tramo()`.

#### Principio 4: Esquemas Desacoplados (Validación en Fronteras)

En lugar de forzar tipos en cada variable, la validación se aplica como un contrato en los puntos de entrada (al leer de un API o archivo) y de salida (antes de escribir en SQLite). Para esto nos apoyamos en la librería `validar`.

### 12.1.3 Las Dimensiones del Dato Moderno: Estructura, Tiempo y Seguridad

Un registro de datos en Latino.NET no es solo una colección de números y cadenas; integra tres dimensiones operativas indispensables:

```text
┌────────────────────────────────────────────────────────────────────────┐
│                   ANATOMÍA DEL DATO INTEGRAL                           │
├─────────────────────┬───────────────────────┬──────────────────────────┤
│ 1. CONTENIDO        │ 2. TIEMPO             │ 3. SEGURIDAD             │
│    (Estructura)     │    (Trazabilidad)     │    (Integridad/Cifrado)  │
├─────────────────────┼───────────────────────┼──────────────────────────┤
│ Campos de negocio:  │ Marcas de auditoría   │ Firmas digitales, hashes │
│ • id, nombre, monto │ • lapsos.ahora()      │ • cadena.hash("SHA256")  │
│ • formatos JSON/CSV │ • parsing de fechas   │ • cripto.encriptar()     │
│ • filas SQLite      │ • cálculos de vigencia│ • cripto.firmar() (HMAC) │
└─────────────────────┴───────────────────────┴──────────────────────────┘
```

Esta trinidad asegura que cualquier dato procesado por tu software sea:

1. **Comprensible:** Fácil de inspeccionar y transformar en memoria.
2. **Ubicable en el tiempo:** Con marcas temporales normalizadas en ISO 8601.
3. **Inviolable:** Protegido contra manipulación accidental o fraude mediante hashes y firmas criptográficas.

### 12.1.4 Cuándo Usar DOP frente a POO

Para no caer en dogmatismos, el arquitecto de software debe saber elegir la herramienta adecuada según el problema:

| Escenario | Paradigma Recomendado | Justificación |
| :--- | :--- | :--- |
| **Ingesta de APIs REST / JSON** | ✅ **DOP (`dic` / `lista`)** | Los esquemas externos cambian sin previo aviso; los diccionarios absorben cambios sin recompilar. |
| **Pipelines ETL / CSV / Bases de Datos** | ✅ **DOP (Tablas y Registros)** | La información se procesa en lotes tabulares masivos donde las clases introducen sobrecarga innecesaria. |
| **Simuladores / Videojuegos / Motores Gráficos** | 🎯 **POO (`clase ... fin`)** | Entidades con identidad física persistente, ciclos de vida continuos y estado interno que interactúa en tiempo real. |
| **Validación de Formularios e Integración** | ✅ **DOP (`validar` + contratos)** | Separa las reglas de validación del dato mismo, permitiendo esquemas flexibles según el rol del usuario. |
| **Conexiones de Red / Drivers de Dispositivos** | 🎯 **POO (`clase ... fin`)** | Recursos con estado físico acoplado a un handle o socket del sistema operativo que debe protegerse. |

## 🌐 12.2 Ingesta y Serialización Jerárquica: JSON y APIs Web

En el ecosistema digital moderno, **JSON (*JavaScript Object Notation*)** es el estándar de facto para el intercambio de información entre aplicaciones web, microservicios, bases de datos documentales y archivos de configuración.

En lenguajes orientados a objetos tradicionales, consumir un JSON exige un proceso laborioso de *deserialización fuertemente tipada*: definir clases con nombres idénticos a los del documento, configurar serializadores y lidiar con excepciones si un campo viene como número en lugar de texto.

Bajo el paradigma **DOP** de Latino.NET, este problema desaparece: **un objeto JSON se mapea de forma transparente y directa a un `diccionario` de Latino, y un arreglo JSON se convierte en una `lista`**.

### 12.2.1 Consumo de APIs REST: De la Red a la Memoria con `web.obtener()`

La biblioteca estándar `web` incluye un cliente HTTP optimizado sobre el motor de .NET 10. Su función principal, `web.obtener(url)`, realiza peticiones HTTP GET y cuenta con **detección y deserialización automática de JSON**.

#### Estructura de la Respuesta HTTP

Al invocar `web.obtener()`, el motor retorna un diccionario con seis metadatos clave:

| Clave | Tipo | Descripción |
| :--- | :--- | :--- |
| `["estado"]` | `decimal` | Código de estado numérico devuelto por el servidor (ej: `200`, `404`, `500`). |
| `["ok"]` | `logico` | `cierto` si el código de estado está en el rango exitoso (`200` al `299`). |
| `["json"]` | `diccionario` / `lista` | **El documento parseado automáticamente.** Si la respuesta no es JSON, contiene `nulo`. |
| `["texto"]` | `cadena` | El cuerpo de la respuesta en formato de texto plano sin procesar. |
| `["url"]` | `cadena` | La URL final que respondió (tras resolver redirecciones HTTP). |
| `["encabezados"]` | `diccionario` | Diccionario con los encabezados HTTP devueltos por el servidor. |

🎯 **Ejemplo de Ingesta Directa**

```latino
escribir("Consultando API de usuarios...")

# Requiere conexión a internet activa
# Petición HTTP GET segura a un endpoint REST
urlApi = "https://jsonplaceholder.typicode.com/users/1"

intentar
    respuesta = web.obtener(urlApi)
    
    si respuesta["ok"]
        # El cuerpo ya es un diccionario nativo en respuesta["json"]
        usuario = respuesta["json"]
        
        escribir("✅ Datos recibidos exitosamente:")
        escribir("   Nombre:   " .. usuario["name"])
        escribir("   Usuario:  " .. usuario["username"])
        escribir("   Correo:   " .. usuario["email"])
        escribir("   Ciudad:   " .. usuario["address"]["city"])
        escribir("   Empresa:  " .. usuario["company"]["name"])
    sino
        escribirf("❌ Error HTTP {0} al consultar el servicio.\n", respuesta["estado"])
    fin
atrapar (errorRed)
    escribir("❌ Fallo de conectividad de red: " .. errorRed)
fin
```

#### Envío de Payloads JSON con `web.enviar()`

Cuando necesitas emitir información hacia un API mediante HTTP POST, `web.enviar(url, datos)` se encarga de la serialización inversa: si el parámetro `datos` es un diccionario de Latino.NET, el motor lo transforma automáticamente a JSON y configura el encabezado `Content-Type: application/json`:

```latino
# Requiere conexión a internet activa
nuevoUsuario = {
    "nombre": "Marcos Silva",
    "rol": "Analista de Datos",
    "activo": cierto
}

# Envía el diccionario serializado como JSON en el cuerpo del POST
intentar
    respuestaEnvio = web.enviar("https://jsonplaceholder.typicode.com/users", nuevoUsuario)
    si respuestaEnvio["ok"]
        escribir("Usuario registrado con ID asignado: " .. respuestaEnvio["json"]["id"])
    sino
        escribirf("❌ Error HTTP {0} al enviar los datos.\n", respuestaEnvio["estado"])
    fin
atrapar (err)
    escribir("❌ Fallo de conectividad al emitir datos al servidor: " .. err)
fin
```

### 12.2.2 Persistencia Jerárquica en Disco: `leerJson` y `escribirJson`

Para la persistencia local de configuraciones complejas o bases de datos de documentos, la librería `archivo` ofrece serialización nativa de alto rendimiento delegando en `Utf8JsonWriter` y `JsonDocument` de .NET 10.

#### 1. Lectura de Archivos JSON (`archivo.leerJson`)

Lee el archivo desde el almacenamiento persistente y construye el árbol completo de diccionarios y listas en la memoria RAM:

```latino
# Preparación autosuficiente: creamos el archivo que vamos a leer
datosServidor = {
    "red": { "puerto": 8080 },
    "limites": { "max_conexiones": 500 }
}
archivo.escribirJson("servidor.json", datosServidor, cierto)

intentar
    configuracion = archivo.leerJson("servidor.json")
    
    puerto = configuracion["red"]["puerto"]
    maxConexiones = configuracion["limites"]["max_conexiones"]
    
    escribirf("Iniciando servicio en puerto {0} (Capacidad: {1} usuarios)\n", puerto, maxConexiones)
atrapar (e)
    escribir("Error al procesar el archivo JSON: " .. e)
fin

# Higiene: eliminar el archivo temporal
archivo.borrar("servidor.json")
```

#### 2. Escritura y Formato Legible (`archivo.escribirJson`)

La función `archivo.escribirJson(ruta, datos, [identado])` serializa diccionarios o listas a disco. El tercer parámetro booleano controla el formato:

- `cierto` (por defecto): Genera un archivo con sangría y saltos de línea (*Pretty Print*), ideal para inspección humana.
- `falso`: Genera un JSON minificado en una sola línea continua, optimizado para ahorrar espacio en disco o tráfico de red.

```latino
catalogo = [
    { "sku": "MON-4K",  "precio": 350.00, "stock": 15 },
    { "sku": "TECL-MEC", "precio": 85.50,  "stock": 40 },
    { "sku": "RAT-OPT",  "precio": 25.00,  "stock": 100 }
]

# Guardar con formato legible (indentado)
archivo.escribirJson("catalogo_legible.json", catalogo, cierto)

# Guardar compacto para producción (minificado)
archivo.escribirJson("catalogo_compacto.json", catalogo, falso)

# Comprobamos la diferencia de formato entre ambos
escribir("--- Legible ---")
escribir(archivo.leer("catalogo_legible.json"))
escribir("--- Compacto ---")
escribir(archivo.leer("catalogo_compacto.json"))

archivo.borrar("catalogo_legible.json")
archivo.borrar("catalogo_compacto.json")
```

> 💡 **Nota de Arquitectura:** El serializador JSON de Latino.NET utiliza codificación `UnsafeRelaxedJsonEscaping`. Esto significa que los caracteres hispanos (`ñ`, `á`, `é`) y los emojis (`🚀`, `📦`) se guardan directamente como caracteres legibles en UTF-8, en lugar de secuencias de escape crípticas como `\u00F1`.

### 12.2.3 Navegación Defensiva y Fusión Nula (`??`)

Al trabajar con árboles de datos JSON provenientes de fuentes externas, es frecuente encontrarse con **campos ausentes o valores nulos**. En muchos lenguajes, consultar una clave que no existe provoca una excepción fatal que aborta el programa.

En Latino.NET, el operador de fusión nula (`??`) cuenta con una característica arquitectónica clave: **captura silenciosamente los errores de claves inexistentes en el operando izquierdo**:

```latino
# Supongamos una respuesta JSON donde algunos campos de contacto son opcionales
perfilCliente = {
    "nombre": "Carlos Vega",
    "contactos": {
        "email": "carlos@empresa.com"
        # "telefono" no fue provisto por el usuario
    }
}

# ❌ Sin fusión nula: Si intentas leer perfilCliente["contactos"]["telefono"] directamente,
# el motor lanzará: RuntimeError: Clave no encontrada en diccionario: telefono

# ✅ CON FUSIÓN NULA (Navegación Defensiva en una sola línea):
# Si la clave no existe, el motor atrapa el fallo y devuelve el valor por defecto:
telefono = perfilCliente["contactos"]["telefono"] ?? "No registrado"
ciudad   = perfilCliente["direccion"]["ciudad"]   ?? "Ciudad Desconocida"
pais     = perfilCliente["pais"]                  ?? "Panamá"

escribir("Teléfono: " .. telefono)  # Imprime: No registrado
escribir("Ciudad:   " .. ciudad)    # Imprime: Ciudad Desconocida
escribir("País:     " .. pais)      # Imprime: Panamá
```

### 12.2.4 Estampado Temporal e Integridad Criptográfica del Dato

En la ingeniería de datos profesional, un registro JSON no debe considerarse completo sin certificar **cuándo se creó** y **si fue alterado durante el transporte o almacenamiento**.

Para lograr esto, se utiliza el patrón de **Sobre de Datos (*Data Envelope*)**, combinando la estructura del JSON con marcas de tiempo del plugin `lapsos` y resúmenes criptográficos con `cadena.hash()`:

```latino
incluir("lapsos")

# 1. El dato puro de negocio
transaccion = {
    "emisor": "CUENTA-001",
    "receptor": "CUENTA-894",
    "monto": 1500.00,
    "concepto": "Servicios de Auditoria"
}

# 2. Envoltura con dimensión temporal (Lapsos)
# Usamos lapsos.ahora() para estampar la fecha exacta en estándar ISO 8601
marcaTiempo = lapsos.ahora()["texto"]

sobreDeDatos = {
    "timestamp": marcaTiempo,
    "payload": transaccion
}

# 3. Serialización temporal para calcular la huella digital
archivo.escribirJson("temp_payload.json", sobreDeDatos, falso)
textoSerializado = archivo.leer("temp_payload.json")

# 4. Cálculo de integridad con Hash SHA-256
huellaSha256 = cadena.hash(textoSerializado, "SHA256")
sobreDeDatos["checksum"] = huellaSha256

# 5. Persistencia final del documento inmutable y auditado
archivo.escribirJson("transaccion_segura.json", sobreDeDatos, cierto)
archivo.borrar("temp_payload.json")

escribir("=== REGISTRO JSON AUDITADO CON ÉXITO ===")
escribir("Fecha de estampación: " .. marcaTiempo)
escribir("Huella de integridad:  " .. huellaSha256)
escribir("\nContenido guardado en disco:")
escribir(archivo.leer("transaccion_segura.json"))

# Limpieza del archivo de demostración
archivo.borrar("transaccion_segura.json")
```

**Salida en consola:**

```text
=== REGISTRO JSON AUDITADO CON ÉXITO ===
Fecha de estampación: <fecha y hora dinámica de ejecución>
Huella de integridad:  <hash SHA-256 del contenido serializado>

Contenido guardado en disco:
{
  "timestamp": "<fecha dinámica>",
  "payload": {
    "emisor": "CUENTA-001",
    "receptor": "CUENTA-894",
    "monto": 1500,
    "concepto": "Servicios de Auditoria"
  },
  "checksum": "<hash SHA-256>"
}
```

El formato JSON resuelve con maestría los árboles de datos jerárquicos y las comunicaciones en red. Sin embargo, cuando se trata de procesar cientos de miles de registros uniformes, el formato más eficiente y extendido del mundo es la tabla plana: **el formato CSV y las matrices de datos**.

## 📊 12.3 Procesamiento Tabular y Flujos Planos: CSV y Matrices

Mientras que JSON es el rey de los datos jerárquicos y las APIs, el formato **CSV (*Comma-Separated Values*)** y las **matrices bidimensionales** dominan la analítica, la ciencia de datos, las hojas de cálculo y la exportación masiva de bases de datos.

En Latino.NET, el procesamiento tabular no requiere librerías externas pesadas ni envoltorios complejos. El motor resuelve la manipulación tabular combinando dos módulos de alta velocidad:

1. **`archivo.leerCsv` / `archivo.escribirCsv`:** Ingesta y exportación de archivos planos con casteo numérico automático.
2. **Las operaciones matriciales de `lista`:** Inspección de dimensiones, extracción de vectores columna y recorte de submatrices.

### 12.3.1 Ingesta y Mapeo Automático de Registros Planos (`archivo.leerCsv`)

La función `archivo.leerCsv(ruta, [separador])` lee un archivo plano y lo transforma instantáneamente en una **`lista` de diccionarios**, donde cada fila es un diccionario cuyas claves corresponden a los encabezados de la primera línea:

```latino
# 1. Creamos un archivo CSV de prueba para simular datos reales
contenidoCsv = "id,producto,precio,unidades,activo
101,Monitor 27 Pulgadas,280.50,14,cierto
102,Teclado Mecanico,75.00,30,cierto
103,Mouse Inalambrico,22.90,0,falso
104,Cable HDMI 2.1,12.50,85,cierto"

archivo.escribir("inventario.csv", contenidoCsv)

# 2. Ingesta tabular directa
registros = archivo.leerCsv("inventario.csv", ",")

escribir("=== INVENTARIO IMPORTADO (DOP TABULAR) ===")
escribir("Total de filas leídas: " .. lista.longitud(registros))

valorTotalInventario = 0

para fila en registros
    # ¡Casteo Automático!
    # leerCsv convierte automáticamente textos numéricos a 'decimal'.
    # fila["precio"] y fila["unidades"] ya son números con los que se puede operar.
    subtotalFila = fila["precio"] * fila["unidades"]
    valorTotalInventario = valorTotalInventario + subtotalFila
    
    escribirf("  [{0}] {1,-22} | Stock: {2,3} | Subtotal: ${3,8:F2}\n", 
        fila["id"], fila["producto"], fila["unidades"], subtotalFila)
fin

escribir("---------------------------------------------------------------")
escribirf("Valorización total del inventario en almacén: ${0:N2}\n", valorTotalInventario)

# Higiene: no dejamos el CSV de prueba en el disco
archivo.borrar("inventario.csv")
```

**Salida en consola:**

```text
=== INVENTARIO IMPORTADO (DOP TABULAR) ===
Total de filas leídas: 4
  [101] Monitor 27 Pulgadas    | Stock:  14 | Subtotal: $ 3927.00
  [102] Teclado Mecanico       | Stock:  30 | Subtotal: $ 2250.00
  [103] Mouse Inalambrico      | Stock:   0 | Subtotal: $    0.00
  [104] Cable HDMI 2.1         | Stock:  85 | Subtotal: $ 1062.50
---------------------------------------------------------------
Valorización total del inventario en almacén: $7,239.50
```

> 💡 **Nota de Formatos Internacionales:** `archivo.leerCsv()` acepta cualquier delimitador en su segundo argumento opcional (como punto y coma `;` para formatos europeos o tabulaciones `\t` para archivos TSV).

### 12.3.2 Exportación y Reescritura Tabular (`archivo.escribirCsv`)

Para persistir datos tabulares o exportar el resultado de un análisis hacia Excel o Google Sheets, se utiliza `archivo.escribirCsv(ruta, listaDiccionarios, [separador])`:

- La función inspecciona el **primer diccionario de la lista** y utiliza sus claves como encabezados de columna.
- Recorre ordenadamente cada registro y emite las filas de texto correspondientes con el delimitador indicado.

```latino
# Creamos una lista de diccionarios con los resultados procesados
reporteVentas = [
    { "vendedor": "Ana Gómez",    "zona": "Norte", "total_ventas": 14200.00 },
    { "vendedor": "Luis Morales", "zona": "Sur",   "total_ventas": 9850.50 },
    { "vendedor": "Carla Ríos",   "zona": "Este",  "total_ventas": 18320.00 }
]

# Exportación directa a CSV delimitado por comas
archivo.escribirCsv("reporte_final.csv", reporteVentas, ",")

escribir("✅ Reporte exportado. Vista previa del archivo en disco:")
escribir(archivo.leer("reporte_final.csv"))

# Limpieza de archivos de prueba
archivo.borrar("reporte_final.csv")
```

### 12.3.3 Manipulación de Cuadrículas y Álgebra Tabular (`lista`)

Cuando los datos no tienen nombres de columnas sino que forman una **cuadrícula o matriz matemática bidimensional** (una lista de listas), la biblioteca estándar `lista` ofrece operaciones de corte y análisis matricial en tiempo de ejecución.

#### 1. Inspección de Dimensiones (`lista.dimensiones`)

Retorna una tupla `(filas, columnas)` con el tamaño de la matriz. Si la matriz es irregular (filas con distinta longitud), el motor lanza un `RuntimeError` defensivo garantizando integridad dimensional:

```latino
matrizDatos = [
    [ 10,  20,  30 ],
    [ 40,  50,  60 ],
    [ 70,  80,  90 ],
    [ 100, 110, 120 ]
]

dimensiones = lista.dimensiones(matrizDatos)
filas    = dimensiones[0]
columnas = dimensiones[1]

escribirf("Matriz rectangular válida: {0} filas × {1} columnas\n", filas, columnas)
# Salida: Matriz rectangular válida: 4 filas × 3 columnas
```

#### 2. Extracción de Vectores Columna (`lista.columna`)

En análisis de datos es habitual necesitar aislar una sola variable (por ejemplo, todos los valores de la segunda columna) para calcular promedios o desviaciones estándar.

`lista.columna(matriz, indiceColumna)` extrae la columna, es decir, la vertical completa como una nueva lista plana:

```latino
matrizDatos = [
    [ 10,  20,  30 ],
    [ 40,  50,  60 ],
    [ 70,  80,  90 ],
    [ 100, 110, 120 ]
]

# Extraer la columna de índice 1 (los valores centrales: 20, 50, 80, 110)
columnaCentral = lista.columna(matrizDatos, 1)

escribir("Vector columna extraído: " .. lista.separar(columnaCentral, ", "))
# Salida: Vector columna extraído: 20, 50, 80, 110

promedioColumna = lista.promedio(columnaCentral)
escribir("Promedio de la columna: " .. promedioColumna) # 65
```

#### 3. Recorte de Submatrices (*Matrix Slicing* con `lista.submatriz`)

Permite recortar una ventana rectangular de datos indicando los índices inclusivos de inicio y fin para filas y columnas:  
`lista.submatriz(matriz, filaInicio, filaFin, colInicio, colFin)`

```latino
matrizDatos = [
    [ 10,  20,  30 ],
    [ 40,  50,  60 ],
    [ 70,  80,  90 ],
    [ 100, 110, 120 ]
]

# Recortar el bloque central de filas 1 a 2, y columnas 0 a 1
subBloque = lista.submatriz(matrizDatos, 1, 2, 0, 1)

# subBloque contiene:
# [ [40, 50],
#   [70, 80] ]

escribir("Submatriz recortada:")
para fila en subBloque
    escribir("  [" .. lista.separar(fila, ", ") .. "]")
fin
```

### 12.3.4 Minería y Extracción de Datos No Estructurados (`cadena.extraer`)

Con frecuencia los datos no llegan ordenados en un CSV, sino incrustados en correos electrónicos, bitácoras de texto (*logs*) o reportes sin formato.

Para transformar texto desordenado en colecciones tabulares sin escribir complejas expresiones regulares manuales, Latino.NET incorpora **`cadena.extraer(texto, modo)`**:

#### Catálogo de Modos de Extracción

| Modo | Patrón Detectado | Ejemplo de Coincidencias |
| :--- | :--- | :--- |
| `"fechas"` | Fechas en formatos ISO o slash | `2026-09-21`, `15/08/2026` |
| `"monedas"` | Valores monetarios con símbolo, **sin separador de millares** | `$150.00`, `€45.50` |
| `"emails"` | Direcciones de correo electrónico | `usuario@dominio.com` |
| `"telefonos"` | Formatos telefónicos internacionales | `+507 6000-1234`, `(555) 019-2831` |
| `"decimales"` | Números con punto flotante | `3.1416`, `0.005`, `-14.8` |
| `"digitos"` | Números enteros puros | `1045`, `892` |

> ⚠️ **Límites reales de los modos (medidos en v1.0.81):**
>
> - **`"monedas"` se corta en el separador de millares.** `$1,250.50` se extrae como `$1` y el resto se pierde. Los decimales sí se capturan completos (`$450.00`), pero **escribe los miles sin coma** si necesitas el monto íntegro.
> - **`"telefonos"` también captura fechas en formato ISO.** En un texto que contenga fechas, la lista de teléfonos las incluye al principio: para `+507 6555-8900` en un texto con dos fechas devuelve `[2026-09-18, 2026-09-21, +507 6555-8900]`. Filtra por el índice real o limpia el resultado antes de usarlo.
> - **`"monedas"` exige el símbolo.** `1250.50` a secas no coincide con ningún modo.

#### Caso Práctico de Minería de Texto

Imagina un correo de confirmación de pedido recibido en texto plano:

```latino
textoCorreo = "Confirmación de compra: El cliente juan.perez@empresa.com realizó 
un abono inicial de $450.00 el 2026-09-18 y un pago final de $1250.50 el 2026-09-21. 
Para asistencia llamar a soporte al +507 6555-8900."

# Extracción directa mediante minería de patrones
correosDetectados  = cadena.extraer(textoCorreo, "emails")
montosDetectados   = cadena.extraer(textoCorreo, "monedas")
fechasDetectadas   = cadena.extraer(textoCorreo, "fechas")
telefonosDetectados = cadena.extraer(textoCorreo, "telefonos")

escribir("=== DATOS EXTRAÍDOS DEL TEXTO NO ESTRUCTURADO ===")
escribir("Cliente identificado: " .. correosDetectados[0])
escribir("Fechas de pago:       " .. lista.separar(fechasDetectadas, " y "))
escribir("Montos de operación:  " .. lista.separar(montosDetectados, ", "))
escribir("Línea de soporte:     " .. telefonosDetectados[2])
```

**Salida en consola:**

```text
=== DATOS EXTRAÍDOS DEL TEXTO NO ESTRUCTURADO ===
Cliente identificado: juan.perez@empresa.com
Fechas de pago:       2026-09-18 y 2026-09-21
Montos de operación:  $450.00, $1250.50
Línea de soporte:     +507 6555-8900
```

Los archivos planos y las matrices permiten transformar la información a gran velocidad en la memoria de trabajo. Sin embargo, cuando los conjuntos de datos crecen hasta los millones de filas, se requieren índices de búsqueda instantáneos y garantías de integridad transaccional: **la persistencia relacional con SQLite**.

## 💾 12.4 Persistencia Relacional y Transacciones ACID (`sqlite`)

En las secciones anteriores abordamos la manipulación de árboles documentales en JSON y tablas planas en CSV. Sin embargo, cuando una aplicación maneja decenas o cientos de miles de registros relacionados, los archivos de texto colapsan:

- Modificar una sola fila exige reescribir todo el archivo en disco.
- No existen índices de búsqueda: consultar un registro obliga a recorrer la lista completa en memoria ($O(N)$).
- Si el proceso o la máquina sufren un corte de energía mientras se guarda un archivo plano, los datos quedan irremediablemente corruptos.

Para resolver este desafío sin abandonar la simplicidad de la Programación Orientada a Datos, Latino.NET incorpora el plugin oficial **`sqlite`**, integrando el motor de base de datos relacional más utilizado del planeta directamente sobre la arquitectura de **.NET 10**.

### 12.4.1 La Fusión DOP con el Modelo Relacional

En otros ecosistemas, trabajar con bases de datos relacionales suele requerir pesados marcos de mapeo objeto-relacional (*ORMs* como Entity Framework o Hibernate) que convierten filas de tablas en complejas clases de POO.

Bajo la filosofía **DOP** de Latino.NET, esta sobrecarga desaparece por completo:

- **Una fila SQL se mapea de forma nativa a un `diccionario` de Latino.**
- **El resultado de una consulta SQL es directamente una `lista` de diccionarios.**
- **Los tipos de datos se convierten bidireccionalmente sin intervención manual:** números enteros y decimales a `decimal`, textos a `cadena`, booleanos a `logico` y valores `NULL` a `nulo`.

#### Inicialización y Creación de Esquemas

```latino
incluir("sqlite")

# 1. Conexión a la base de datos (se crea el archivo si no existe)
db = sqlite.conectar("almacen_datos.db")

# 2. Creación del esquema relacional estructurado
# Usamos tipos nativos de SQLite con restricciones de integridad
sqlCrearTabla = "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)"

sqlite.ejecutar(db, sqlCrearTabla)
escribir("✅ Esquema de base de datos relacional verificado.")

# Higiene: cerramos la conexión y borramos el archivo de prueba
sqlite.cerrar(db)
archivo.borrar("almacen_datos.db")
```

### 12.4.2 Blindaje contra Inyección SQL: Consultas Parametrizadas

Uno de los errores más graves en el desarrollo de software es concatenar variables de texto directamente dentro de sentencias SQL:

```latino
# ❌ PELIGROSO (Vulnerable a Inyecciones SQL destructivas):
# Si entradaUsuario contiene: "' OR '1'='1" o "'; DROP TABLE productos; --"
entradaUsuario = "'; DROP TABLE productos; --"
sqlVulnerable = "SELECT * FROM productos WHERE codigo = '" .. entradaUsuario .. "'"
escribir(sqlVulnerable)
```

> ☠️ **`entradaUsuario` no es código malicioso: es el dato de un usuario.** La línea anterior es lo que llegaría desde un formulario web. Al concatenarlo, el motor lo interpreta como SQL y el `DROP TABLE` borra la tabla. La versión segura va justo debajo.

En Latino.NET, tanto `sqlite.ejecutar()` como `sqlite.consultar()` cuentan con soporte nativo para **parámetros nombrados**. Al pasar un diccionario de parámetros, el motor delega la sanitización en el analizador de .NET 10, neutralizando cualquier intento de inyección de código:

> 📚 **Los siguientes cinco bloques forman una sola secuencia de trabajo.** `db` se abre en el bloque de creación de esquema (el primero de la sección) y se cierra y borra en el último. Si vas a ejecutar uno suelto, añade al inicio `incluir("sqlite")` y `db = sqlite.conectar("almacen_datos.db")` (más la tabla `productos` del primer bloque).

```latino
incluir("sqlite")

# Conexión de trabajo: el esquema completo está en el primer bloque de 12.4.1
db = sqlite.conectar("almacen_datos.db")
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)")

# ✅ SEGURO: Parámetros tipados mediante diccionario
sqlInsertar = "INSERT INTO productos (codigo, nombre, precio, stock, creado_en) 
               VALUES (@cod, @nom, @precio, @stock, @fecha)"

parametrosProducto = {
    "@cod": "LAP-OLED-16",
    "@nom": "Portátil Profesional 16\"",
    "@precio": 1450.00,
    "@stock": 10,
    "@fecha": sis.tiempo("%Y-%m-%d %H:%M:%S")
}

# Inserción segura en base de datos
filasAfectadas = sqlite.ejecutar(db, sqlInsertar, parametrosProducto)

# Recuperación inmediata del ID autonumérico recién generado
idGenerado = sqlite.ultimo_id(db)
escribirf("Registro insertado con éxito. ID asignado: {0} (Filas: {1})\n", idGenerado, filasAfectadas)

# Higiene: cerramos la conexión y borramos el archivo de prueba
sqlite.cerrar(db)
archivo.borrar("almacen_datos.db")
```

> 💡 **Nota de Ergonomía:** En el diccionario de parámetros, el prefijo `@` es **opcional**: si defines `{"cod": "LAP-01"}`, el motor lo normaliza a `@cod` automáticamente antes de enviarlo a SQLite. Puedes mezclar ambos estilos en la misma llamada.
>
> Aun así, **recomendamos escribir el `@` explícito**: hace visible de un vistazo qué claves del diccionario se enlazan con marcadores del SQL, y evita confusiones al leer el código. La regla completa está en [9.17 Librería `sqlite`](09_Más_Sintaxis_Errores_Lib/9.17_plugin_sqlite.md).

### 12.4.3 Consultas Analíticas y Extracción Directa (`sqlite.consultar`)

La función `sqlite.consultar(db, sql, [parametros])` ejecuta sentencias de lectura (`SELECT`) y retorna una `lista` con todos los registros devueltos, listos para ser procesados con bucles `para` o transformaciones funcionales:

```latino
incluir("sqlite")

# Conexión de trabajo: el esquema completo está en el primer bloque de 12.4.1
db = sqlite.conectar("almacen_datos.db")
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)")

# Datos de prueba: sin inserciones, la consulta siempre devolvería 0 registros
sqlite.ejecutar(db, "INSERT INTO productos (codigo, nombre, precio, stock, creado_en) VALUES
    ('LAP-01', 'Laptop Pro', 1200.0, 5, '2026-09-01'),
    ('MON-02', 'Monitor 4K',  650.0, 8, '2026-09-01'),
    ('TEC-03', 'Teclado',     45.0, 30, '2026-09-01')")

# Consulta analítica con parámetros y ordenamiento
sqlConsulta = "SELECT id, codigo, nombre, precio, stock, (precio * stock) AS valor_almacen 
               FROM productos 
               WHERE precio >= @precioMin 
               ORDER BY valor_almacen DESC"

filtro = { "@precioMin": 500.00 }
articulos = sqlite.consultar(db, sqlConsulta, filtro)

escribir("=== INVENTARIO ALTO VALOR (CONSULTA RELACIONAL) ===")
escribir("Total de registros que cumplen el criterio: " .. lista.longitud(articulos))

para art en articulos
    # art es un diccionario puro cuyos nombres de clave son las columnas del SELECT
    escribirf("  [{0}] {1,-25} | Stock: {2,3} | Valor: ${3,10:N2}\n",
        art["codigo"], art["nombre"], art["stock"], art["valor_almacen"])
fin

# Higiene: cerramos la conexión y borramos el archivo de prueba
sqlite.cerrar(db)
archivo.borrar("almacen_datos.db")
```

### 12.4.4 El Salto Cuántico: Transacciones Atómicas ACID (`sqlite.transaccion`)

En bases de datos transaccionales rigen las propiedades **ACID** (*Atomicidad, Consistencia, Aislamiento y Durabilidad*). Cuando necesitas insertar miles de registros o transferir saldos entre cuentas, no puedes permitirte que la mitad de la operación se guarde y la otra mitad falle.

Además, existe un factor crítico de rendimiento de hardware:

- Cada vez que ejecutas `sqlite.ejecutar()` de forma individual, SQLite fuerza una **sincronización física contra el disco duro (*fsync*)**, tardando entre 10 y 30 milisegundos por sentencia. Insertar 1,000 filas una a una puede tardar más de 20 segundos.
- Al envolver las inserciones dentro de una **transacción atómica**, SQLite acumula los cambios en memoria RAM y realiza una **única sincronización en bloque al final**, reduciendo el tiempo de 20 segundos a **menos de 50 milisegundos**.

#### Anatomía de `sqlite.transaccion()`

En Latino.NET, las transacciones se gestionan mediante una función callback:

```latino
incluir("sqlite")

# Conexión de trabajo: el esquema completo está en el primer bloque de 12.4.1
db = sqlite.conectar("almacen_datos.db")
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)")

sqlite.transaccion(db, funcion(idConexion)
    # Todas las operaciones ejecutadas aquí participan en la misma transacción.
    # Si la función concluye con éxito, el motor ejecuta COMMIT automáticamente.
    # Si ocurre cualquier error o excepción, el motor ejecuta ROLLBACK y revierte todo.
fin)

# Higiene: cerramos la conexión y borramos el archivo de prueba
sqlite.cerrar(db)
archivo.borrar("almacen_datos.db")
```

#### Caso Práctico: Inserción Masiva Ultrarrápida con Reversión ante Fallos

```latino
incluir("sqlite")

# Conexión de trabajo: el esquema completo está en el primer bloque de 12.4.1
db = sqlite.conectar("almacen_datos.db")
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)")

# 1. Simulación de un lote de 500 productos generados en memoria
loteProductos = [ ]
para i en rango(1, 501)
    lista.agregar(loteProductos, {
        "codigo": ç("SKU-|i|"),
        "nombre": ç("Componente Electronico #|i|"),
        "precio": 10.0 + (i * 0.5),
        "stock": i * 2,
        "fecha": "2026-09-21"
    })
fin

escribir("Iniciando inserción atómica masiva de 500 registros...")
tiempoInicio = sis.milisegundos()

sqlLote = "INSERT INTO productos (codigo, nombre, precio, stock, creado_en) 
           VALUES (@codigo, @nombre, @precio, @stock, @fecha)"

# 2. Ejecución dentro de una transacción protegida
intentar
    sqlite.transaccion(db, funcion(conId)
        para p en loteProductos
            # Detección de anomalía intencional (simulando corte o error)
            si p["stock"] > 990
                # Provocamos un fallo de negocio deliberado
                # lanzar("Fallo crítico de verificación en lote")
            fin
            
            sqlite.ejecutar(conId, sqlLote, p)
        fin
    fin)
    
    duracion = sis.milisegundos() - tiempoInicio
    escribirf("✅ Transacción confirmada (COMMIT). Tiempo total: {0} ms\n", duracion)
atrapar (errorTransaccion)
    escribir("❌ La transacción falló y se revirtió completamente (ROLLBACK): " .. errorTransaccion)
fin

# 3. Verificación del total de registros en la base de datos
conteoFinal = sqlite.consultar(db, "SELECT COUNT(*) AS total FROM productos")[0]["total"]
escribir("Total de registros consolidados en la tabla: " .. conteoFinal)

# Higiene: cerramos la conexión y borramos el archivo de prueba
sqlite.cerrar(db)
archivo.borrar("almacen_datos.db")
```

**Salida en consola:**

```text
Iniciando inserción atómica masiva de 500 registros...
✅ Transacción confirmada (COMMIT). Tiempo total: 78 ms
Total de registros consolidados en la tabla: 500
```

> 💡 `rango(1, 501)` genera exactamente **500** valores (del 1 al 500): el segundo argumento es el límite **excluido**. Por eso el conteo es 500 y no 501.
> El tiempo en milisegundos varía según la máquina.

### 12.4.5 Cierre y Liberación Higiénica de Recursos (`sqlite.cerrar`)

En sistemas operativos como Windows, los archivos abiertos por un proceso quedan bloqueados a nivel de kernel. Si no cierras tu conexión de base de datos antes de terminar o mover el archivo `.db`, el sistema operativo impedirá borrarlo, comprimirlo o respaldarlo.

La función `sqlite.cerrar(idConexion)` realiza una limpieza en dos niveles:

1. Cierra y dispone la conexión interna en C#.
2. **Purga el pool de conexiones:** Invoca `SqliteConnection.ClearAllPools()` en .NET 10, garantizando que el archivo en disco sea liberado **inmediatamente**:

```latino
incluir("sqlite")

# Conexión de trabajo: el esquema completo está en el primer bloque de 12.4.1
db = sqlite.conectar("almacen_datos.db")
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT UNIQUE NOT NULL,
    nombre TEXT NOT NULL,
    precio REAL NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    creado_en TEXT NOT NULL
)")

# Cerramos la base de datos y liberamos el bloqueo de disco
sqlite.cerrar(db)
escribir("Base de datos cerrada y archivo liberado para operaciones del SO.")

# Ahora es completamente seguro respaldar, comprimir con zip o eliminar el archivo:
archivo.borrar("almacen_datos.db")
```

Las bases de datos relacionales garantizan la consistencia una vez que los datos ya están guardados. Sin embargo, para evitar que información corrupta, correos inválidos o documentos falsos ingresen a nuestras tablas, debemos aplicar una muralla de contención previa: **los contratos y la validación de esquemas de datos (`validar`)**.

## 🔍 12.5 Contratos y Validación de Integridad de Datos (`validar`)

En el paradigma de Programación Orientada a Datos (DOP), no encerramos los datos dentro de clases rígidas con métodos privados para protegerlos. Los datos viajan como **estructuras genéricas y abiertas** (`dic`, `lista`, `tupla`).

Sin embargo, que los datos sean abiertos no significa que el sistema deba aceptar basura. En la ingeniería de software profesional, la seguridad y la consistencia se garantizan mediante el **Principio de Validación en las Fronteras (*Boundary Validation*)**:

```text
Entrada Externa (JSON / CSV / Web)
            │
            ▼
┌──────────────────────────────────────┐
│  FRONTERA DEL SISTEMA (validar)      │ ─── ❌ Datos Inválidos ───► Abortar con error
│  • Tipos escalares y rangos          │                             o devolver reporte
│  • Formatos (emails, URLs, teléfonos)│
│  • Documentos (cédulas, RUC)         │
│  • Fechas y firmas criptográficas    │
└──────────────────────────────────────┘
            │
            ▼ ✅ Datos Saneados y Verificados
┌──────────────────────────────────────┐
│  NÚCLEO DEL PROGRAMA (DOP)           │
│  • Transformaciones puras            │
│  • Cálculos analíticos               │
│  • Persistencia en SQLite            │
└──────────────────────────────────────┘
```

Una vez que un registro cruza la frontera y supera la validación, el resto de la aplicación puede procesarlo con total confianza, sin necesidad de verificar tipos en cada línea de código.

Para construir esta muralla de contención, Latino.NET incorpora la biblioteca nativa **`validar`**, complementada con la verificación temporal de **`lapsos`** y la integridad de **`cripto`**.

### 12.5.1 Validación de Formatos y Tipos Escalares

La librería `validar` ofrece funciones nativas altamente optimizadas en C# para auditar datos primitivos:

| Función | Firma | Regla Validada |
| :--- | :--- | :--- |
| `validar.numero()` | `(texto)` | Retorna `cierto` si el texto es un número válido (entero o con decimales). |
| `validar.entero()` | `(texto)` | Retorna `cierto` si el texto representa estrictamente un número entero. |
| `validar.longitud()` | `(texto, min, max)` | Verifica si la longitud del texto se encuentra en el rango inclusivo `[min, max]`. |
| `validar.alfanumerico()` | `(texto)` | Verifica que solo contenga letras y dígitos (sin caracteres especiales ni espacios). |
| `validar.email()` | `(texto)` | Valida formato estándar de correo electrónico (`usuario@dominio.ext`). |
| `validar.url()` | `(texto)` | Valida formato de dirección web (`http://` o `https://`). |
| `validar.telefono()` | `(texto)` | Validación telefónica internacional flexible (soporta `+`, `-`, espacios y paréntesis). |
| `validar.ipv4()` | `(texto)` | Valida formato de dirección de red IPv4 (`0.0.0.0` a `255.255.255.255`). |
| `validar.contrasenaSegura()` | `(texto)` | Exige mínimo 8 caracteres, al menos 1 mayúscula, 1 minúscula, 1 dígito y 1 símbolo. |

> 🔒 **Blindaje contra ReDoS:** Todas las validaciones de expresiones regulares en `validar` están protegidas por el temporizador `sis.config("regex_timeout")` (2.0 segundos por defecto), neutralizando ataques de denegación de servicio por expresiones regulares maliciosas.

```latino
escribir("=== VALIDACIONES BÁSICAS DE ENTRADA ===")

correoPrueba = "ingenieria@latinonet.org"
escribir("¿Email válido? " .. validar.email(correoPrueba))  # cierto

telefonoPrueba = "+507 (6000) 1234"
escribir("¿Teléfono válido? " .. validar.telefono(telefonoPrueba))  # cierto

passwordDebil = "secreto123"
escribir("¿Password seguro? " .. validar.contrasenaSegura(passwordDebil))  # falso (falta mayúscula y símbolo)
```

### 12.5.2 Documentos de Identidad y Registro Mercantil

Latino.NET incluye validadores estructurales para documentos legales e impositivos:

#### 1. Verificación de Cédulas (`validar.cedula`)

Valida la estructura de cédulas personales, extranjeros con carné de migración (`E`), panameños nacidos en el extranjero (`PE`) y naturalizados (`N`), con o sin guiones separadores:

```latino
c1 = "8-765-4321"
c2 = "PE-12-3456"
c3 = "documento_invalido"

escribir("¿Cédula 1 válida? " .. validar.cedula(c1))  # cierto
escribir("¿Cédula 2 válida? " .. validar.cedula(c2))  # cierto
escribir("¿Cédula 3 válida? " .. validar.cedula(c3))  # falso
```

#### 2. Desglose Estructural de Documentos (`validar.analizarCedula`)

Retorna un diccionario completo desglosando los componentes registrales del documento:

```latino
analisis = validar.analizarCedula("8-890-1234")

si analisis["valida"]
    escribir("=== ANÁLISIS REGISTRAL ===")
    escribir("Tipo de registro: " .. analisis["tipo"])
    escribir("Provincia:        " .. analisis["provincia"])
    escribir("Tomo:             " .. analisis["tomo"])
    escribir("Asiento:          " .. analisis["asiento"])
    escribir("Normalizada:      " .. analisis["normalizada"])
fin
```

**Salida en consola:**

```text
=== ANÁLISIS REGISTRAL ===
Tipo de registro: Panameño
Provincia:        8
Tomo:             890
Asiento:          1234
Normalizada:      8-890-1234
```

#### 3. Registro Único de Contribuyente (`validar.ruc`)

Verifica la estructura alfanumérica de registros fiscales empresariales (longitud de 9 a 15 caracteres sin guiones):

```latino
escribir("¿RUC válido? " .. validar.ruc("155689432-2-2022"))  # cierto
```

### 12.5.3 Verificación Temporal y Criptográfica (`lapsos` + `cripto`)

La integridad del dato se extiende más allá del formato del texto: involucra certificar su vigencia en el tiempo y su no manipulación física.

#### 1. Validación de Vigencia con `lapsos`

Podemos verificar si una fecha es válida y si un documento o token ya caducó:

```latino
incluir("lapsos")

# Validamos que el texto tenga una estructura de fecha real
fechaTexto = "2026-12-31"

si lapsos.esValido(fechaTexto, "yyyy-MM-dd")
    fechaVencimiento = lapsos.analizar(fechaTexto)
    
    # Comprobamos si la fecha ya quedó en el pasado
    si lapsos.esPasado(fechaVencimiento)
        escribir("⚠️ El documento se encuentra VENCIDO.")
    sino
        diferencia = lapsos.diferencia(lapsos.ahora(), fechaVencimiento)
        escribirf("Vigente. Días restantes de validez: {0:F0}\n", diferencia["total_dias"])
    fin
fin
```

#### 2. Verificación de Integridad con Firmas Digitales (`cripto`)

Cuando un registro viaja por la red o se almacena en un archivo externo, usamos una firma HMAC-SHA256 (`cripto.firmar`) para garantizar que nadie alteró los valores de las columnas:

```latino
incluir("cripto")

CLAVE_SECRETA = "ClaveSecretaDeAuditoria_2026"

# Registro crítico
datosTransaccion = "ID:8901;MONTO:45000.00;DESTINO:BANCO_CENTRAL"

# Generación de la firma HMAC
firmaDigital = cripto.firmar(datosTransaccion, CLAVE_SECRETA)

# Verificación de integridad en destino
escribir("Verificando firma digital del mensaje...")
datosIntactos = cripto.verificar(datosTransaccion, firmaDigital, CLAVE_SECRETA)
escribir("¿El registro es auténtico y no fue manipulado? " .. datosIntactos) # cierto

# Si un atacante intenta alterar el monto:
datosManipulados = "ID:8901;MONTO:99000.00;DESTINO:BANCO_CENTRAL"
intentoFraude = cripto.verificar(datosManipulados, firmaDigital, CLAVE_SECRETA)
escribir("¿Ataque aceptado? " .. intentoFraude) # falso
```

### 12.5.4 El Patrón "Guardián de Esquemas" (*Schema Guard*)

En la Programación Orientada a Datos, un error común es detener la validación al primer fallo. Los sistemas de producción deben **acumular todos los errores encontrados** para reportar al usuario una lista completa de correcciones en una sola respuesta.

A continuación, implementamos un contrato de validación de frontera desacoplado y reutilizable:

```latino
funcion ValidarRegistroCliente(registro)
    local errores = [ ]
    
    # 1. Validación de nombre
    si no dic.contiene(registro, "nombre") ó no validar.longitud(registro["nombre"] ?? "", 3, 50)
        lista.agregar(errores, "El campo 'nombre' es obligatorio y debe tener entre 3 y 50 caracteres.")
    fin
    
    # 2. Validación de email
    si no dic.contiene(registro, "email") ó no validar.email(registro["email"] ?? "")
        lista.agregar(errores, "El campo 'email' no tiene un formato de correo válido.")
    fin
    
    # 3. Validación de documento
    si no dic.contiene(registro, "cedula") ó no validar.cedula(registro["cedula"] ?? "")
        lista.agregar(errores, "El campo 'cedula' no cumple la estructura legal requerida.")
    fin
    
    # 4. Validación de límite de crédito
    saldo = registro["limite_credito"] ?? -1
    si tipo(saldo) != "decimal" ó saldo < 0
        lista.agregar(errores, "El campo 'limite_credito' debe ser un número positivo.")
    fin
    
    # Retornamos el veredicto estructurado
    retornar {
        "valido": lista.longitud(errores) == 0,
        "errores": errores
    }
fin

# Caso 1: Registro que viola múltiples contratos
clienteDefectuoso = {
    "nombre": "A",
    "email": "correo_sin_arroba",
    "cedula": "00-00",
    "limite_credito": -500
}

resultadoAuditoria = ValidarRegistroCliente(clienteDefectuoso)

si no resultadoAuditoria["valido"]
    escribir("❌ REGISTRO RECHAZADO EN LA FRONTERA:")
    para err en resultadoAuditoria["errores"]
        escribir("   • " .. err)
    fin
fin
```

**Salida en consola:**

```text
❌ REGISTRO RECHAZADO EN LA FRONTERA:
   • El campo 'nombre' es obligatorio y debe tener entre 3 y 50 caracteres.
   • El campo 'email' no tiene un formato de correo válido.
   • El campo 'cedula' no cumple la estructura legal requerida.
   • El campo 'limite_credito' debe ser un número positivo.
```

Una vez que los datos han sido estructurados, saneados y validados, nos enfrentamos al último obstáculo del almacenamiento físico: cuando los archivos de datos crecen hasta alcanzar gigabytes y desbordan la memoria o los límites de transferencia: **el particionado y la compresión con `zip`**.

## 🗜️ 12.6 Fragmentación y Manejo de Volúmenes Masivos (`zip`)

En el procesamiento de datos a escala de producción, tarde o temprano nos topamos con la barrera del tamaño físico:

- Bases de datos SQLite consolidadas (`.db`) que superan los cientos de megabytes.
- Exportaciones de bitácoras (*logs*) o archivos CSV analíticos que miden varios gigabytes.
- Restricciones de infraestructura: canales de red que abortan descargas si la conexión se interrumpe, límites de adjuntos en servicios de mensajería o cuotas de subida en nubes de almacenamiento.

Intentar cargar un archivo de 2 GB en memoria de una sola vez con `archivo.leer()` saturaría la memoria RAM y provocaría una excepción de agotamiento de memoria (*Out of Memory*).

Para mitigar este dolor sin recurrir a utilidades externas del sistema operativo, Latino.NET incorpora el plugin oficial **`zip`**, diseñado no solo para comprimir y descomprimir, sino para implementar una estrategia de **Particionado y Reensamblado de Datos (*Data Chunking & Reassembly*)** altamente eficiente en I/O.

### 12.6.1 Activación y Catálogo del Plugin `zip`

El plugin se carga mediante la directiva `incluir("zip")` y expone un conjunto de operaciones construidas directamente sobre `System.IO.Compression` de .NET 10:

```latino
incluir("zip")
```

| Función | Parámetros | Comportamiento en Runtime |
| :--- | :--- | :--- |
| `zip.comprimir()` | `origenDir, destinoZip` | Comprime una carpeta completa en un archivo `.zip` con nivel de compresión óptimo. |
| `zip.comprimirArchivo()` | `origenArch, destinoZip` | Comprime un solo archivo en un `.zip`. Sobrescribe si ya existe. |
| `zip.descomprimir()` | `origenZip, destinoDir` | Extrae el contenido completo de un archivo `.zip` en la carpeta destino. |
| `zip.listar()` | `origenZip` | Retorna una `lista` con las rutas internas de todos los archivos contenidos en el `.zip`. |
| `zip.contiene()` | `origenZip, rutaInterna` | Retorna `cierto` si el archivo o subcarpeta existe dentro del `.zip`. |
| `zip.agregar()` | `zip, archivo, [rutaInt]` | Inserta o reemplaza un archivo dentro de un `.zip` existente sin descomprimirlo. |
| `zip.cortar()` | `archivo, tam, [unid], [dir]` | Divide un archivo en fragmentos de tamaño fijo (`"B"`, `"KB"`, `"MB"`, `"GB"`). Retorna lista de rutas. |
| `zip.cortarPartes()` | `archivo, nPartes, [dir]` | Divide un archivo equitativamente en un número exacto de fragmentos. Retorna lista de rutas. |
| `zip.unir()` | `listaPartes, destinoArch` | Recombina secuencialmente una lista de partes en un único archivo idéntico al original. |

### 12.6.2 Estrategias de Particionado de Datos (*Chunking*)

Latino.NET ofrece dos estrategias complementarias para fragmentar volúmenes pesados según el requisito de infraestructura:

```text
┌────────────────────────────────────────────────────────────────────────┐
│                   ESTRATEGIAS DE PARTITIONING EN LATINO                │
├────────────────────────────────────┬───────────────────────────────────┤
│ zip.cortar(archivo, tamaño, unid)  │ zip.cortarPartes(archivo, n)      │
├────────────────────────────────────┼───────────────────────────────────┤
│ • Partición por TAMAÑO FIJO.       │ • Partición por NÚMERO DE PARTES. │
│ • "Divide en pedazos de 10 MB".    │ • "Divide en exactamente 4 partes │
│ • Genera tantas partes como sean   │    homogéneas".                   │
│   necesarias.                      │ • Distribuye los bytes residuales │
│ • Ideal para respetar cuotas de red│   de forma equitativa.            │
│   o límites de almacenamiento.     │ • Ideal para dividir trabajo entre│
│                                    │   N hilos de procesamiento.       │
└────────────────────────────────────┴───────────────────────────────────┘
```

#### Nomenclatura Estandarizada

Ambas funciones generan fragmentos con una extensión secuencial de tres dígitos:

- `datos_pesados.csv.part001`
- `datos_pesados.csv.part002`
- `datos_pesados.csv.part003`

#### Memoria Eficiente con Búferes de 80 KB

A nivel de implementación en C# (`ZipPlugin.cs`), el motor utiliza un búfer de transferencia de **80 KB (81,920 bytes)**. Este valor fue elegido deliberadamente: es el tamaño óptimo de E/S que se mantiene justo por debajo del umbral del *Large Object Heap (LOH)* de .NET (85,000 bytes). Como resultado, **fragmentar un archivo de 500 MB consume prácticamente cero memoria RAM adicional** y no sobrecarga al Garbage Collector.

### 12.6.3 Reensamblado Íntegro y Verificación Criptográfica (`zip.unir`)

La función `zip.unir(listaPartes, destino)` toma la lista de fragmentos y los fusiona secuencialmente en un nuevo archivo idéntico byte por byte.

En la ingeniería de datos, **ninguna transferencia o partición se considera completa sin una verificación de integridad**. Al combinar `zip.cortar()`, `zip.unir()` y `cadena.hash()`, garantizamos matemáticamente que el archivo final no sufrió ninguna corrupción:

```latino
incluir("zip")

# 1. Creamos un archivo de datos simulado
RUTA_ORIGINAL = "datos_maestros.csv"
contenidoPrueba = "id,transaccion,monto,fecha\n"
para i en rango(1, 1001)
    monto = i * 2.5
    contenidoPrueba = contenidoPrueba .. ç("|i|,TRANSACCION_REGISTRO_MASIVO,|monto|,2026-09-21\n")
fin
archivo.escribir(RUTA_ORIGINAL, contenidoPrueba)

escribir("Archivo original creado. Tamaño: " .. archivo.tamaño(RUTA_ORIGINAL) .. " bytes.")

# 2. Calculamos la huella digital SHA-256 antes de fragmentar
hashOriginal = cadena.hash(archivo.leer(RUTA_ORIGINAL), "SHA256")
escribir("Huella SHA-256 original: " .. hashOriginal)

# 3. Fragmentamos el archivo en 3 partes equitativas
escribir("\nFragmentando archivo en 3 partes...")
partesGeneradas = zip.cortarPartes(RUTA_ORIGINAL, 3)

para p en partesGeneradas
    escribirf("  📦 Fragmento generado: {0} ({1} bytes)\n", p, archivo.tamaño(p))
fin

# 4. Reensamblado en un nuevo archivo
RUTA_RECONSTRUIDA = "datos_reensamblados.csv"
escribir("\nReensamblando fragmentos con zip.unir()...")
zip.unir(partesGeneradas, RUTA_RECONSTRUIDA)

# 5. Auditoría Criptográfica de Integridad
hashFinal = cadena.hash(archivo.leer(RUTA_RECONSTRUIDA), "SHA256")
escribir("Huella SHA-256 final:    " .. hashFinal)

si hashOriginal == hashFinal
    escribir("✅ INTEGRIDAD CERTIFICADA: El archivo se reconstruyó con cero pérdidas.")
sino
    escribir("❌ CORRUPCIÓN DETECTADA: Los hashes no coinciden.")
fin

# 6. Limpieza de archivos de prueba
archivo.borrar(RUTA_ORIGINAL)
archivo.borrar(RUTA_RECONSTRUIDA)
para parte en partesGeneradas
    archivo.borrar(parte)
fin
```

**Salida en consola:**

```text
Archivo original creado. Tamaño: 56910 bytes.
Huella SHA-256 original: 7a82dc43ef10...

Fragmentando archivo en 3 partes...
  📦 Fragmento generado: datos_maestros.csv.part001 (18970 bytes)
  📦 Fragmento generado: datos_maestros.csv.part002 (18970 bytes)
  📦 Fragmento generado: datos_maestros.csv.part003 (18970 bytes)

Reensamblando fragmentos con zip.unir()...
Huella SHA-256 final:    7a82dc43ef10...
✅ INTEGRIDAD CERTIFICADA: El archivo se reconstruyó con cero pérdidas.
```

### 12.6.4 Compresión y Empaquetado de Datos en Reposo

Cuando los datos necesitan almacenarse a largo plazo (respaldos diarios de bases de datos SQLite o históricos de ventas en CSV), la compresión en formato `.zip` reduce el espacio en disco en un **60% a 85%**.

```latino
incluir("zip")

archivo.escribir("respaldo_ventas.csv", "id,monto\n1,100.0\n2,250.0\n")

# Comprimir un archivo pesado individual (ej: base de datos o CSV)
zip.comprimirArchivo("respaldo_ventas.csv", "historico_2026.zip")

# Inspección de contenido sin descomprimir
escribir("Archivos dentro del ZIP:")
para item en zip.listar("historico_2026.zip")
    escribir("  • " .. item)
fin

# Comprobación de presencia de un archivo específico
si zip.contiene("historico_2026.zip", "respaldo_ventas.csv")
    escribir("✅ El archivo requerido está presente en el comprimido.")
fin

# Extracción completa en carpeta destino
zip.descomprimir("historico_2026.zip", "datos_restaurados")

# Limpieza al final
archivo.borrar("respaldo_ventas.csv")
archivo.borrar("historico_2026.zip")
archivo.borrar("datos_restaurados")
```

Hemos dominado la totalidad de las piezas que gobiernan los datos en Latino.NET: estructuras abiertas (`dic`/`lista`), árboles documentales en JSON, tablas planas en CSV, álgebra matricial, persistencia relacional en SQLite, contratos con `validar`, marcas de tiempo con `lapsos`, seguridad con `cripto` y particionado con `zip`.

En la sección final uniremos todas estas herramientas en una sola sinfonía de ingeniería: **el diseño y construcción de un pipeline ETL industrial completo**.

## 🏭 12.7 Caso Maestro de Ingeniería: El Pipeline ETL Completo

Para concluir este capítulo con maestría, uniremos todas las herramientas estudiadas en una sola arquitectura industrial: la construcción de un **Pipeline ETL (*Extract, Transform, Load*)**.

En las empresas modernas, un pipeline ETL es el corazón del flujo de información:

1. **Extracción (Extract):** Ingesta de fuentes externas crudas (archivos CSV sucios, llamadas a APIs web o volcados de transacciones).
2. **Transformación (Transform):** Limpieza de campos, filtrado de registros defectuosos con contratos de `validar`, normalización de marcas temporales con `lapsos` y generación de resúmenes criptográficos con `cadena.hash()`.
3. **Carga (Load):** Inyección masiva de alta velocidad en una base de datos relacional `sqlite` bajo una transacción atómica protegida contra caídas.
4. **Auditoría (Export):** Generación de un reporte ejecutivo consolidado en formato **JSON** listo para el consumo de analistas o tableros de control.

```text
┌────────────────────────────────────────────────────────────────────────┐
│                   PIPELINE ETL INDUSTRIAL EN LATINO                    │
├────────────────────────────────────────────────────────────────────────┤
│ 1. EXTRACT    Archivo plano sucio ("ventas_crudo.csv")                 │
│               └── Leído con archivo.leerCsv()                          │
│                                                                        │
│ 2. TRANSFORM  • Validación en frontera con validar.email / longitud    │
│               • Estampado temporal ISO con lapsos.ahora()              │
│               • Resumen criptográfico de fila con cadena.hash(SHA256)  │
│               • Separación: válidos vs. rechazados                     │
│                                                                        │
│ 3. LOAD       • Inyección masiva en SQLite ("data_warehouse.db")       │
│               └── Protegida con sqlite.transaccion() (ACID)            │
│                                                                        │
│ 4. AUDIT      • Emisión de balance ejecutivo en JSON                   │
│               └── Persistido con archivo.escribirJson()                │
└────────────────────────────────────────────────────────────────────────┘
```

### Código Completo del Pipeline ETL

A continuación se presenta la implementación integral, 100% autosuficiente y ejecutable en Latino.NET v1.0.81:

```latino
incluir("sqlite")
incluir("lapsos")

escribir("================================================================")
escribir("     PIPELINE ETL DE AUDITORÍA Y CARGA MASIVA DE VENTAS        ")
escribir("================================================================")

tiempoInicio = sis.milisegundos()

# ----------------------------------------------------------------------
# FASE 1: EXTRACCIÓN (EXTRACT)
# Simulamos un archivo CSV externo recibido de un sistema de facturación
# Nota: Incluye registros válidos y registros intencionalmente corruptos
# ----------------------------------------------------------------------
RUTA_CSV_CRUDO = "ventas_crudo.csv"
contenidoCrudo = "folio,cliente_email,monto,divisa
FAC-1001,carlos.mendoza@empresa.com,1250.50,USD
FAC-1002,correo_invalido_sin_arroba,300.00,USD
FAC-1003,lucia.ramos@banco.org,4800.00,USD
FAC-1004,marcos.vega@tienda.com,-50.00,USD
FAC-1005,elena.santos@consultoria.net,890.00,USD"

archivo.escribir(RUTA_CSV_CRUDO, contenidoCrudo)
escribir("1. [EXTRACT] Archivo CSV recibido e ingresado a memoria.")

registrosCrudos = archivo.leerCsv(RUTA_CSV_CRUDO, ",")
escribir("   Total de filas extraídas: " .. lista.longitud(registrosCrudos))

# ----------------------------------------------------------------------
# FASE 2: TRANSFORMACIÓN Y VALIDACIÓN (TRANSFORM)
# ----------------------------------------------------------------------
escribir("\n2. [TRANSFORM] Iniciando saneamiento de datos y validación...")

registrosSaneados   = [ ]
registrosRechazados = [ ]
volumenMonetarioValido = 0
marcaTiempoProceso = lapsos.ahora()["texto"]

para fila en registrosCrudos
    local esValido = cierto
    local motivoFallo = ""
    
    # Regla 1: Formato de correo corporativo
    si no validar.email(fila["cliente_email"])
        esValido = falso
        motivoFallo = "Formato de correo electrónico inválido"
    # Regla 2: Monto numérico estrictamente positivo
    osi fila["monto"] <= 0
        esValido = falso
        motivoFallo = "El monto de la transacción debe ser mayor a cero"
    fin
    
    si esValido
        # Calculamos la huella digital SHA-256 de la transacción para auditoría
        datosFilaTexto = ç("|fila['folio']|;|fila['cliente_email']|;|fila['monto']|")
        hashFila = cadena.hash(datosFilaTexto, "SHA256")
        
        # Enriquecimiento del registro (DOP)
        fila["hash_integridad"] = hashFila
        fila["procesado_en"]    = marcaTiempoProceso
        volumenMonetarioValido  = volumenMonetarioValido + fila["monto"]
        
        lista.agregar(registrosSaneados, fila)
    sino
        # Aislamos el registro rechazado para la bitácora de anomalías
        fila["motivo_rechazo"] = motivoFallo
        fila["fecha_rechazo"]  = marcaTiempoProceso
        lista.agregar(registrosRechazados, fila)
    fin
fin

escribirf("   Registros saneados listos para carga: {0}\n", lista.longitud(registrosSaneados))
escribirf("   Registros anómalos detectados y aislados: {0}\n", lista.longitud(registrosRechazados))

# ----------------------------------------------------------------------
# FASE 3: CARGA ATÓMICA EN BASE DE DATOS (LOAD)
# ----------------------------------------------------------------------
escribir("\n3. [LOAD] Conectando a SQLite e iniciando carga transaccional...")

RUTA_DB = "data_warehouse.db"
db = sqlite.conectar(RUTA_DB)

# Esquema relacional del almacén de datos
sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS ventas_consolidadas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    folio TEXT UNIQUE NOT NULL,
    cliente_email TEXT NOT NULL,
    monto REAL NOT NULL,
    divisa TEXT NOT NULL,
    hash_integridad TEXT NOT NULL,
    procesado_en TEXT NOT NULL
)")

sqlInsertar = "INSERT INTO ventas_consolidadas 
    (folio, cliente_email, monto, divisa, hash_integridad, procesado_en)
    VALUES (@folio, @cliente_email, @monto, @divisa, @hash_integridad, @procesado_en)"

# Inyección protegida bajo transacción ACID
intentar
    sqlite.transaccion(db, funcion(idConexion)
        para registro en registrosSaneados
            sqlite.ejecutar(idConexion, sqlInsertar, registro)
        fin
    fin)
    escribir("   ✅ Carga transaccional completada con éxito (COMMIT ejecutado).")
atrapar (errorCarga)
    escribir("   ❌ La carga falló; se realizó reversión completa (ROLLBACK): " .. errorCarga)
fin

# Verificamos la persistencia en disco
conteoEnBaseDatos = sqlite.consultar(db, "SELECT COUNT(*) AS total FROM ventas_consolidadas")[0]["total"]
sqlite.cerrar(db)

# ----------------------------------------------------------------------
# FASE 4: AUDITORÍA Y EXPORTACIÓN DE REPORTE (AUDIT & EXPORT)
# ----------------------------------------------------------------------
escribir("\n4. [AUDIT] Generando reporte ejecutivo en JSON...")

tiempoFin = sis.milisegundos()
duracionTotal = tiempoFin - tiempoInicio

reporteEjecutivo = {
    "pipeline": "ETL_VENTAS_DIARIAS",
    "ejecutado_en": marcaTiempoProceso,
    "duracion_ms": duracionTotal,
    "metricas": {
        "total_extraidos": lista.longitud(registrosCrudos),
        "total_cargados": lista.longitud(registrosSaneados),
        "total_rechazados": lista.longitud(registrosRechazados),
        "volumen_monetario_usd": volumenMonetarioValido,
        "registros_en_db": conteoEnBaseDatos
    },
    "anomalias": registrosRechazados
}

RUTA_REPORTE_JSON = "reporte_auditoria_etl.json"
archivo.escribirJson(RUTA_REPORTE_JSON, reporteEjecutivo, cierto)

# Salida variable (depende del momento de ejecución y hardware):
escribir("================================================================")
escribir("✅ PIPELINE ETL CONCLUIDO CON ÉXITO")
escribir("Reporte persistido en: " .. RUTA_REPORTE_JSON)
escribir("Duración total de ejecución: " .. duracionTotal .. " ms")
escribir("================================================================")

# Mostrar contenido del reporte final
escribir("\nContenido del Reporte JSON de Auditoría:")
escribir(archivo.leer(RUTA_REPORTE_JSON))

# Limpieza higiénica de archivos de prueba temporales
archivo.borrar(RUTA_CSV_CRUDO)
archivo.borrar(RUTA_DB)
archivo.borrar(RUTA_REPORTE_JSON)
```

**Salida Real en Consola:**

Al ejecutar el pipeline completo en Latino.NET v1.0.81, el sistema muestra:

```text
================================================================
     PIPELINE ETL DE AUDITORÍA Y CARGA MASIVA DE VENTAS        
================================================================
1. [EXTRACT] Archivo CSV recibido e ingresado a memoria.
   Total de filas extraídas: 5

2. [TRANSFORM] Iniciando saneamiento de datos y validación...
   Registros saneados listos para carga: 3
   Registros anómalos detectados y aislados: 2

3. [LOAD] Conectando a SQLite e iniciando carga transaccional...
   ✅ Carga transaccional completada con éxito (COMMIT ejecutado).

4. [AUDIT] Generando reporte ejecutivo en JSON...
================================================================
✅ PIPELINE ETL CONCLUIDO CON ÉXITO
Reporte persistido en: reporte_auditoria_etl.json
Duración total de ejecución: 48 ms
================================================================

Contenido del Reporte JSON de Auditoría:
{
  "pipeline": "ETL_VENTAS_DIARIAS",
  "ejecutado_en": "2026-09-21 18:10:00",
  "duracion_ms": 48,
  "metricas": {
    "total_extraidos": 5,
    "total_cargados": 3,
    "total_rechazados": 2,
    "volumen_monetario_usd": 6940.5,
    "registros_en_db": 3
  },
  "anomalias": [
    {
      "folio": "FAC-1002",
      "cliente_email": "correo_invalido_sin_arroba",
      "monto": 300,
      "divisa": "USD",
      "motivo_rechazo": "Formato de correo electrónico inválido",
      "fecha_rechazo": "2026-09-21 18:10:00"
    },
    {
      "folio": "FAC-1004",
      "cliente_email": "marcos.vega@tienda.com",
      "monto": -50,
      "divisa": "USD",
      "motivo_rechazo": "El monto de la transacción debe ser mayor a cero",
      "fecha_rechazo": "2026-09-21 18:10:00"
    }
  ]
}
```

## 📝 resumen del capítulo

En este capítulo hemos dominado la arquitectura de datos moderna en Latino.NET:

1. **La Filosofía DOP:** Los datos son valores transparentes desacoplados de la lógica, modelados mediante diccionarios (`dic`), listas (`lista`) y tuplas (`tupla`).
2. **Árboles Jerárquicos en JSON:** Ingesta y consumo directo de APIs REST con `web.obtener()`, y serialización en disco con `archivo.leerJson()` y `archivo.escribirJson()`.
3. **Navegación Defensiva (`??`):** El operador de fusión nula absorbe claves inexistentes o campos opcionales en una sola expresión limpia sin excepciones.
4. **Flujos Tabulares Planos (CSV y Matrices):** Mapeo de archivos planos con casteo numérico automático vía `archivo.leerCsv()`, exportación con `archivo.escribirCsv()`, y operaciones de recorte matricial (`dimensiones`, `columna`, `submatriz`).
5. **Persistencia Relacional Transaccional (`sqlite`):** Consultas parametrizadas seguras contra inyección SQL (`@param`) y aceleración atómica de alta velocidad con `sqlite.transaccion()`.
6. **Contratos en Frontera (`validar`):** Muralla de validación defensiva para correos, teléfonos, cédulas, RUC y tipos escalares antes de procesar o persistir.
7. **La Trinidad del Dato Integral:**
   - *Estructura:* Tablas y documentos genéricos.
   - *Tiempo (`lapsos`):* Trazabilidad temporal normalizada en ISO 8601.
   - *Seguridad (`cripto` / `cadena.hash`):* Huellas de integridad SHA-256 y cifrado AES-256-GCM.
8. **Volúmenes Masivos (`zip`):** Estrategias de fragmentación en bloques de 80 KB (`zip.cortar`) y recombinación íntegra (`zip.unir`) para superar las limitaciones de memoria RAM y cuotas de red.
