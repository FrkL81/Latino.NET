# 📚 Inventario de Librerías Estándar - Latino.NET

Este documento contiene un inventario consolidado de todas las funciones disponibles en las librerías estándar de Latino.NET.

> **💡 Manual Detallado:** Para ejemplos completos, explicaciones profundas y casos de uso, consulta la [**Referencia por Librería (API)**](95_Referencia_API.md).

## 📦 Librerías del Núcleo Estándar

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 🌐 | **Globales** | `acadena` | `valor` | Convierte un valor a su representación en cadena |
| 🌐 | **Globales** | `alogico` | `valor` | Convierte cualquier valor a lógico con semántica de tipos: nulo y 0 son falso; cadenas vacías son falso; objetos existentes son verdadero. |
| 🌐 | **Globales** | `anumero` | `valor` | Convierte cadenas y lógicos a número, devuelve nulo si no es posible |
| 🌐 | **Globales** | `clonar` | `objeto` | Realiza una copia profunda (Deep Copy) segura contra ciclos de listas, diccionarios y tuplas. |
| 🌐 | **Globales** | `escribir` | `valores...` | Escribe texto en la consola con salto de línea. |
| 🌐 | **Globales** | `escribirf` | `formato, valores...` | Escribe con formato .NET ({0}, {1:C}, {2:X}). Sin salto de línea. |
| 🌐 | **Globales** | `eval` | `codigo` | Alias de evaluar. |
| 🌐 | **Globales** | `evaluar` | `codigo` | Evalúa código Latino dinámicamente y retorna el resultado de la última expresión. |
| 🌐 | **Globales** | `imprimir` | `valores...` | Alias de escribir. |
| 🌐 | **Globales** | `imprimirf` | `formato, valores...` | Escribe con formato estilo C (`%s`, `%d`, `%.2f`), sin salto de línea. |
| 🌐 | **Globales** | `incluir` | `ruta` | Carga un script `.lat` o un plugin `.dll`. Las funciones del script quedan disponibles en el ámbito actual. |
| 🌐 | **Globales** | `lanzar` | `mensaje` | Lanza una excepción personalizada con el mensaje especificado. Útil para validaciones de lógica de negocio. Se captura con 'intentar/atrapar'. |
| 🌐 | **Globales** | `leer` | `[mensaje_opcional]` | Lee una línea de texto desde la consola con prompt opcional. |
| 🌐 | **Globales** | `limpiar` | `[modo]` | Limpia la pantalla de la consola. Modos: 'pantalla' (default), 'todo' (incluye scrollback), 'cursor' (solo resetea posición). |
| 🌐 | **Globales** | `pedir` | `[mensaje_opcional]` | Alias idiomático de leer. |
| 🌐 | **Globales** | `poner` | `valores...` | Alias de escribir. |
| 🌐 | **Globales** | `ponerf` | `formato, valores...` | Alias de escribirf. |
| 🌐 | **Globales** | `rango` | `[inicio_o_fin], [fin], [paso]` | Genera lista de números o desglosa cadena en caracteres (soporta emojis). 1 arg: rango(fin) → [0...fin-1]; 2 args: rango(inicio, fin); 3 args: rango(inicio, fin, paso). |
| 🌐 | **Globales** | `tipo` | `valor` | Devuelve el tipo de dato del valor como cadena |
| 🌐 | **Globales** | `trocar` | `coleccion, a, b` | Intercambia los valores de dos posiciones/claves dentro de una lista o diccionario. |
| 🌐 | **Globales** | `ç` | `plantilla` | El Compositor: Incrusta variables en texto (Atajo para VS Code: cc). |
| 📂 | **archivo** | `anexar` | `ruta, contenido` | Añade texto al final del archivo. |
| 📂 | **archivo** | `borrar` | `ruta` | Elimina un archivo o directorio (incluyendo contenido). |
| 📂 | **archivo** | `buscar` | `directorio, patron` | Busca archivos recursivamente usando patrón glob (*.lat, test_*.txt, etc). |
| 📂 | **archivo** | `crearCarpeta` | `ruta` | Crea un directorio (y directorios padres si faltan). |
| 📂 | **archivo** | `crear` | `ruta` | Crea un archivo vacío (falla si ya existe). |
| 📂 | **archivo** | `duplicar` | `origen, destino` | Copia un archivo (sobrescribe si existe). |
| 📂 | **archivo** | `ejecutar` | `ruta` | Ejecuta un script .lat en un entorno aislado. |
| 📂 | **archivo** | `eliminar` | `ruta` | Alias de borrar. |
| 📂 | **archivo** | `esArchivo` | `ruta` | Verifica si la ruta es un archivo (no directorio). |
| 📂 | **archivo** | `esCarpeta` | `ruta` | Verifica si la ruta es un directorio. |
| 📂 | **archivo** | `escribirCsv` | `ruta, lista_dicts, [separador]` | Escribe una lista de diccionarios en formato CSV. |
| 📂 | **archivo** | `escribirJson` | `ruta, datos, [identado]` | Escribe un diccionario o lista como JSON. |
| 📂 | **archivo** | `escribir` | `ruta, contenido` | Escribe texto en un archivo (sobrescribe si existe). |
| 📂 | **archivo** | `existe` | `ruta` | Verifica si existe un archivo o directorio. |
| 📂 | **archivo** | `extension` | `ruta` | Obtiene la extensión del archivo (ej: '.txt'). |
| 📂 | **archivo** | `fechaModificacion` | `ruta` | Obtiene la fecha de última modificación. |
| 📂 | **archivo** | `leerCsv` | `ruta, [separador]` | Lee un archivo CSV y retorna una lista de diccionarios. |
| 📂 | **archivo** | `leerJson` | `ruta` | Lee un archivo JSON y retorna diccionario o lista. |
| 📂 | **archivo** | `leer` | `ruta` | Lee el contenido completo de un archivo de texto. |
| 📂 | **archivo** | `lineas` | `ruta` | Lee un archivo y retorna lista de líneas. |
| 📂 | **archivo** | `listar` | `ruta` | Lista archivos y directorios en una ruta. |
| 📂 | **archivo** | `nombre` | `ruta` | Obtiene el nombre del archivo o directorio. |
| 📂 | **archivo** | `renombrar` | `origen, destino` | Mueve o renombra archivo/directorio. |
| 📂 | **archivo** | `rutaAbsoluta` | `ruta` | Convierte ruta relativa a absoluta. |
| 📂 | **archivo** | `tamaño` | `ruta` | Obtiene el tamaño del archivo en bytes. |
| 🔤 | **cadena** | `bytes` | `cadena` | Convierte cadena a lista de códigos UTF-16. NOTA: Emojis y caracteres fuera del BMP generan 2 códigos (par subrogado). |
| 🔤 | **cadena** | `centrar` | `texto, ancho, [relleno]` | Centra el texto en un campo de ancho especificado usando conteo de grafemas. Relleno opcional. |
| 🔤 | **cadena** | `char` | `lista_o_numero` | Convierte lista de ASCII o número a carácter. |
| 🔤 | **cadena** | `codificarBase64` | `texto` | Codifica una cadena de texto a Base64. |
| 🔤 | **cadena** | `comparar` | `cadena1, cadena2` | Compara dos cadenas y devuelve la diferencia numérica de caracteres (0 si son iguales). |
| 🔤 | **cadena** | `concatenar` | `cadena1, cadena2` | Une dos cadenas. |
| 🔤 | **cadena** | `contar` | `texto, subcadena` | Cuenta cuántas veces aparece la subcadena (sin solapamiento). |
| 🔤 | **cadena** | `contiene` | `cadena, subcadena` | Verifica si una cadena contiene otra. |
| 🔤 | **cadena** | `decodificarBase64` | `base64` | Decodifica una cadena en formato Base64 a texto. |
| 🔤 | **cadena** | `ejecutar` | `codigo` | Ejecuta código Latino contenido en una cadena (legacy, no retorna valor). |
| 🔤 | **cadena** | `eliminar` | `cadena, subcadena` | Elimina la primera ocurrencia de una subcadena. |
| 🔤 | **cadena** | `encontrar` | `cadena, subcadena` | Encuentra la posición en grafemas de una subcadena (-1 si no existe). |
| 🔤 | **cadena** | `es_alfa` | `cadena` | Verifica si la cadena contiene solo letras o números. |
| 🔤 | **cadena** | `es_igual` | `cadena1, cadena2, [modo]` | Compara dos cadenas con diferentes modos (exacta, ignora_caso, normalizada, comodin, comodin_insensible). |
| 🔤 | **cadena** | `es_numerico` | `cadena` | Verifica si la cadena es un número válido (alias de es_numero). |
| 🔤 | **cadena** | `es_numero` | `cadena` | Verifica si la cadena es un número válido. |
| 🔤 | **cadena** | `es_vacia` | `cadena` | Verifica si la cadena está vacía o es nula. |
| 🔤 | **cadena** | `esta_vacia` | `cadena` | Verifica si la cadena está vacía o es nula. |
| 🔤 | **cadena** | `estilo` | `texto, estilo` | Transforma texto a diferentes estilos (camello, pascal, serpiente, kebab, etc.). |
| 🔤 | **cadena** | `extraer` | `texto, modo` | Extrae patrones del texto. Modos: digitos, letras, decimales, emails, urls, telefonos, hashtags, menciones, monedas, fechas. |
| 🔤 | **cadena** | `formato` | `formato, valores...` | Genera una cadena con formato estilo printf (transpilado a .NET). |
| 🔤 | **cadena** | `hash` | `texto, [algoritmo]` | Calcula el hash de una cadena. Algoritmos: MD5, SHA1, SHA256 (default), SHA512. |
| 🔤 | **cadena** | `indice` | `cadena, subcadena` | Encuentra la posición en grafemas de una subcadena (-1 si no existe). |
| 🔤 | **cadena** | `inicia_con` | `cadena, prefijo` | Verifica si la cadena inicia con el texto especificado. |
| 🔤 | **cadena** | `insertar` | `cadena, subcadena, posicion` | Inserta una subcadena en la posición en grafemas especificada respetando emojis y caracteres Unicode. |
| 🔤 | **cadena** | `invertir` | `cadena` | Invierte el orden de los caracteres sin corromper emojis o caracteres especiales. |
| 🔤 | **cadena** | `longitud` | `cadena` | Devuelve el número de caracteres (grafemas) en la cadena. Los emojis cuentan como 1. |
| 🔤 | **cadena** | `mayusculas` | `cadena` | Convierte la cadena a MAYÚSCULAS. |
| 🔤 | **cadena** | `minusculas` | `cadena` | Convierte la cadena a minúsculas. |
| 🔤 | **cadena** | `nombrarNumero` | `numero` | Convierte un número a su representación cardinal en español (ignora decimales). |
| 🔤 | **cadena** | `partir` | `texto, separador, limite` | Divide el texto con límite máximo de divisiones. |
| 🔤 | **cadena** | `recortar` | `cadena` | Elimina espacios al inicio y final de la cadena. |
| 🔤 | **cadena** | `reemplazar` | `cadena, buscar, reemplazo` | Reemplaza todas las ocurrencias de un texto. |
| 🔤 | **cadena** | `reemplazarRegex` | `texto, patron, reemplazo_o_funcion, [limite]` | Reemplaza coincidencias de una expresión regular. El reemplazo puede ser texto (con $1, $2 para grupos) o una función(match, grupo1..N, indice, original). Límite opcional como 4to argumento. |
| 🔤 | **cadena** | `regex` | `cadena, patron` | Extrae todas las coincidencias de una expresión regular. |
| 🔤 | **cadena** | `regexl` | `cadena, patron` | Verifica si la cadena coincide con una expresión regular. |
| 🔤 | **cadena** | `rellenar_derecha` | `cadena, caracter, longitud` | Rellena la cadena por la derecha hasta alcanzar la longitud en grafemas especificada. |
| 🔤 | **cadena** | `rellenar_izquierda` | `cadena, caracter, longitud` | Rellena la cadena por la izquierda hasta alcanzar la longitud en grafemas especificada. |
| 🔤 | **cadena** | `separar` | `cadena, [separador]` | Divide la cadena en una lista usando un separador. |
| 🔤 | **cadena** | `subcadena` | `cadena, inicio, longitud` | Extrae una parte de la cadena sin corromper emojis o caracteres especiales. |
| 🔤 | **cadena** | `termina_con` | `cadena, sufijo` | Verifica si la cadena termina con el texto especificado. |
| 🔤 | **cadena** | `ultimo_indice` | `cadena, subcadena` | Encuentra la última posición en grafemas de una subcadena (-1 si no existe). |
| 🎫 | **cola** | `alista` | `cola` | Convierte la cola a una lista (inicio→fin). |
| 🎫 | **cola** | `crear` | n/a | Crea una cola vacía. |
| 🎫 | **cola** | `desencolar` | `cola` | Extrae el elemento del inicio. |
| 🎫 | **cola** | `encolar` | `cola, elemento` | Agrega un elemento al final. |
| 🎫 | **cola** | `limpiar` | `cola` | Elimina todos los elementos de la cola. |
| 🎫 | **cola** | `longitud` | `cola` | Devuelve la cantidad de elementos. |
| 🎫 | **cola** | `siguiente` | `cola` | Obtiene el siguiente elemento sin extraerlo. |
| 🎫 | **cola** | `vacia` | `cola` | Verifica si la cola no contiene elementos. |
| 🎯 | **conjunto** | `agregar` | `conjunto, elemento` | Agrega un elemento al conjunto y devuelve el conjunto modificado |
| 🎯 | **conjunto** | `contiene` | `conjunto, elemento` | Verifica si el conjunto contiene el elemento especificado |
| 🎯 | **conjunto** | `crear` | `elementos...` | Crea un nuevo conjunto con los elementos únicos proporcionados |
| 🎯 | **conjunto** | `diferencia` | `conjunto1, conjunto2` | Crea un nuevo conjunto con los elementos del primer conjunto que no están en el segundo |
| 🎯 | **conjunto** | `eliminar` | `conjunto, elemento` | Elimina un elemento del conjunto y devuelve el conjunto modificado |
| 🎯 | **conjunto** | `interseccion` | `conjunto1, conjunto2` | Crea un nuevo conjunto con la intersección de dos conjuntos |
| 🎯 | **conjunto** | `longitud` | `conjunto` | Devuelve el número de elementos en el conjunto |
| 🎯 | **conjunto** | `union` | `conjunto1, conjunto2` | Crea un nuevo conjunto con la unión de dos conjuntos |
| 📖 | **dic** | `agregar` | `diccionario, clave, valor` | Agrega o actualiza un valor. |
| 📖 | **dic** | `contiene` | `diccionario, clave` | Verifica si el diccionario contiene la clave especificada |
| 📖 | **dic** | `eliminar` | `diccionario, clave` | Elimina una clave del diccionario |
| 📖 | **dic** | `filtrar` | `diccionario, funcion` | Filtra pares llave-valor. La función recibe (llave, valor) y retorna verdadero para conservar. |
| 📖 | **dic** | `fusionar` | `diccionario1, diccionario2` | Crea un nuevo diccionario combinando dos. |
| 📖 | **dic** | `invertir` | `diccionario` | Intercambia llaves y valores. Si hay valores duplicados, prevalece la última llave. |
| 📖 | **dic** | `llaves` | `diccionario` | Devuelve una lista con todas las claves. |
| 📖 | **dic** | `longitud` | `diccionario` | Devuelve el número de elementos. |
| 📖 | **dic** | `mapear` | `diccionario, funcion` | Transforma llaves y valores. La función recibe (llave, valor) y retorna {llave: nuevaLlave, valor: nuevoValor}. |
| 📖 | **dic** | `obtener` | `diccionario, clave, [defecto]` | Obtiene el valor de una clave o un valor por defecto. |
| 📖 | **dic** | `transformarLlaves` | `diccionario, funcion` | Aplica una función a cada llave. Si hay colisiones, prevalece la última. |
| 📖 | **dic** | `transformarValores` | `diccionario, funcion` | Aplica una función a cada valor. Retorna nuevo diccionario con llaves originales. |
| 📖 | **dic** | `valores` | `diccionario` | Devuelve una lista con todos los valores. |
| 📜 | **lista** | `agregar` | `lista, elemento` | Agrega un elemento al final de la lista. |
| 📜 | **lista** | `aleatorio` | `lista` | Retorna un elemento aleatorio de la lista. |
| 📜 | **lista** | `alguno` | `lista, funcion` | Retorna verdadero si al menos un elemento cumple la condición. |
| 📜 | **lista** | `aplanar` | `lista` | Aplana recursivamente listas anidadas. |
| 📜 | **lista** | `columna` | `matriz, j` | Extrae la columna j de una matriz como vector. |
| 📜 | **lista** | `comparar` | `lista1, lista2` | Compara dos listas elemento por elemento (-1, 0, 1). |
| 📜 | **lista** | `concatenar` | `lista1, lista2` | Retorna una nueva lista con la unión de dos listas. |
| 📜 | **lista** | `contar` | `lista, elemento` | Cuenta cuántas veces aparece el elemento en la lista. |
| 📜 | **lista** | `contiene` | `lista, elemento` | Verifica si la lista contiene el elemento. |
| 📜 | **lista** | `crear` | `tamaño` | Crea una lista de tamaño N con nulos. |
| 📜 | **lista** | `diagonal` | `matriz` | Extrae la diagonal principal de una matriz como vector. |
| 📜 | **lista** | `dimensiones` | `matriz` | Retorna una tupla (filas, columnas) con las dimensiones de una matriz. Retorna (n, 0) para vectores 1D. Lanza error s... |
| 📜 | **lista** | `eliminar` | `lista, elemento` | Elimina la primera ocurrencia de un elemento. |
| 📜 | **lista** | `eliminar_indice` | `lista, indice` | Elimina el elemento en el índice indicado. Lanza error si el índice está fuera de rango. |
| 📜 | **lista** | `encontrar` | `lista, elemento` | Alias de indice. |
| 📜 | **lista** | `extender` | `lista_destino, lista_origen` | Agrega los elementos de la segunda lista al final de la primera. |
| 📜 | **lista** | `filtrar` | `lista, funcion` | Crea una nueva lista con elementos que cumplen la condición. |
| 📜 | **lista** | `identidad` | `n` | Crea la matriz identidad n×n. |
| 📜 | **lista** | `indice` | `lista, elemento` | Devuelve el índice del elemento o -1. |
| 📜 | **lista** | `insertar` | `lista, elemento, indice` | Inserta un elemento en una posición específica. Si el índice está fuera de rango, se ajusta al inicio o al final. |
| 📜 | **lista** | `invertir` | `lista` | Invierte el orden de la lista. |
| 📜 | **lista** | `llenar` | `filas, columnas, valor` | Crea una matriz de filas×columnas con todos los elementos iguales a valor. |
| 📜 | **lista** | `longitud` | `lista` | Devuelve la cantidad de elementos. |
| 📜 | **lista** | `ordenar` | `lista, [comparador]` | Ordena la lista (opcionalmente con función comparadora). |
| 📜 | **lista** | `particionar` | `lista, funcion` | Divide la lista en dos grupos según condición. Retorna dic con claves 'verdadero' y 'falso'. |
| 📜 | **lista** | `promedio` | `lista` | Calcula el promedio aritmético de los elementos numéricos. |
| 📜 | **lista** | `reducir` | `lista, funcion, [semilla]` | Reduce la lista a un valor usando función acumuladora f(acum, val). |
| 📜 | **lista** | `saltar` | `lista, n` | Retorna una nueva lista omitiendo los primeros N elementos. |
| 📜 | **lista** | `separar` | `lista, [separador]` | Convierte la lista a texto uniendo elementos con un separador. |
| 📜 | **lista** | `submatriz` | `matriz, filaInicio, filaFin, colInicio, colFin` | Extrae un bloque rectangular de una matriz (índices inclusivos). |
| 📜 | **lista** | `sumar` | `lista` | Calcula la suma de todos los elementos numéricos de la lista. |
| 📜 | **lista** | `todos` | `lista, funcion` | Retorna verdadero si todos los elementos cumplen la condición. |
| 📜 | **lista** | `tomar` | `lista, n` | Retorna una nueva lista con los primeros N elementos. |
| 📜 | **lista** | `transformar` | `lista, funcion` | Crea una nueva lista transformando cada elemento. |
| 📜 | **lista** | `unicos` | `lista` | Retorna una nueva lista eliminando elementos duplicados (preserva orden). |
| 📜 | **lista** | `zip` | `lista1, lista2, ...` | Combina múltiples listas en tuplas. Longitud = lista más corta. |
| 🧮 | **mate** | `abs` | `numero` | Valor absoluto de un número. |
| 🧮 | **mate** | `acos` | `num` | Arcocoseno. |
| 🧮 | **mate** | `acosh` | `num` | Arcocoseno hiperbólico. |
| 🧮 | **mate** | `aleatorio` | `min?, max?` | Genera número aleatorio. 0 args: 0.0-1.0; 1 arg: 0-Max (inclusivo); 2 args: Min-Max (inclusivo). |
| 🧮 | **mate** | `alt` | `min?, max?` | Alias de aleatorio. |
| 🧮 | **mate** | `arco` | `num` | Alias de acos. |
| 🧮 | **mate** | `asen` | `num` | Arcoseno. |
| 🧮 | **mate** | `asenh` | `num` | Arcoseno hiperbólico. |
| 🧮 | **mate** | `asin` | `num` | Alias de asen. |
| 🧮 | **mate** | `atan` | `num` | Arcotangente. |
| 🧮 | **mate** | `atan2` | `y, x` | Devuelve el ángulo (radianes) entre el eje X positivo y el punto (y, x). |
| 🧮 | **mate** | `atanh` | `num` | Arcotangente hiperbólica. |
| 🧮 | **mate** | `base` | `parte, porc_entero` | Calcula el total dado una parte y su %. |
| 🧮 | **mate** | `compuesto` | `valor_inicial, tasa, periodos, [modo]` | Calcula crecimiento/decrecimiento compuesto: V = P×(1+r)^n. Modos: 'simple'/'discreto' (default) o 'continuo'/'euler' (e^(r×n)). |
| 🧮 | **mate** | `convertir` | `valor, desde, hacia` | Convierte valores entre unidades físicas y temperaturas. |
| 🧮 | **mate** | `cos` | `rad` | Coseno de un ángulo (radianes). |
| 🧮 | **mate** | `cosh` | `num` | Coseno hiperbólico. |
| 🧮 | **mate** | `desviacion` | `lista \| numeros...` | Calcula la desviación estándar muestral. |
| 🧮 | **mate** | `epsilonMaquina` | n/a | Calcula el épsilon de máquina (la menor diferencia representable en punto flotante). Es el número positivo más pequeño ε tal que 1.0 + ε ≠ 1.0 en aritmética de la máquina. |
| 🧮 | **mate** | `esPrimo` | `numero` | Retorna verdadero si el número es primo. Máximo: 10^12 |
| 🧮 | **mate** | `exp` | `numero` | Exponencial (e^x). |
| 🧮 | **mate** | `factorial` | `numero` | Calcula el factorial de un número entero (n!). Máximo: 170. |
| 🧮 | **mate** | `frexp` | `num` | Descompone en [mantisa, exponente]. |
| 🧮 | **mate** | `interpolar` | `inicio, fin, factor` | Interpolación lineal entre dos valores. Factor 0 = inicio, 1 = fin. |
| 🧮 | **mate** | `ldexp` | `x, exp` | Carga exponente: x * 2^exp. |
| 🧮 | **mate** | `limitar` | `valor, minimo, maximo` | Limita un valor al rango [mínimo, máximo] (clamp). |
| 🧮 | **mate** | `log` | `numero` | Logaritmo natural. |
| 🧮 | **mate** | `log10` | `numero` | Logaritmo base 10. |
| 🧮 | **mate** | `max` | `lista \| numeros...` | Devuelve el valor máximo de una lista de números o argumentos sueltos. |
| 🧮 | **mate** | `mcd` | `a, b, ... \| lista` | Calcula el Máximo Común Divisor. Acepta números o lista. |
| 🧮 | **mate** | `mcm` | `a, b, ... \| lista` | Calcula el Mínimo Común Múltiplo. Acepta números o lista. |
| 🧮 | **mate** | `mediana` | `lista` | Calcula la mediana de una lista numérica (valor central). |
| 🧮 | **mate** | `min` | `lista \| numeros...` | Devuelve el valor mínimo de una lista de números o argumentos sueltos. |
| 🧮 | **mate** | `moda` | `lista` | Retorna el valor que más se repite en la lista. |
| 🧮 | **mate** | `parsearNumero` | `valor, [separador_miles]` | Parsea un número desde texto o valor numérico. Retorna {entero, decimal, texto, negativo}. Soporta múltiples formatos internacionales. |
| 🧮 | **mate** | `parte` | `total, porc_entero` | Calcula la parte de un total dado un % entero. |
| 🧮 | **mate** | `piso` | `numero` | Redondea hacia abajo al entero más cercano. |
| 🧮 | **mate** | `pitagorica` | `cateto_a, cateto_b, hipotenusa_c` | Usa string para el lado a calcular: 'a', 'b' (catetos), 'c' o 'h' (hipotenusa). |
| 🧮 | **mate** | `porc` | `parte, total` | Obtiene la proporción decimal. |
| 🧮 | **mate** | `porcentaje` | `parte, total` | Alias de porc. |
| 🧮 | **mate** | `pot` | `base, exponente` | Eleva una base a un exponente. |
| 🧮 | **mate** | `productoria` | `lista` | Multiplica todos los elementos de la lista (∏). |
| 🧮 | **mate** | `raiz` | `numero` | Raíz cuadrada. |
| 🧮 | **mate** | `raizc` | `numero` | Raíz cúbica. |
| 🧮 | **mate** | `reconstruir` | `diccionario` | Reconstruye un número desde un diccionario (inverso de parsearNumero). |
| 🧮 | **mate** | `redondear` | `numero, [decimales]` | Redondea al entero más cercano o a los decimales especificados. |
| 🧮 | **mate** | `regresion` | `lista_x, lista_y` | Realiza una regresión lineal simple. Retorna {m, b, formula}. |
| 🧮 | **mate** | `resolverPolinomio` | `coeficientes...` | Los coeficientes se pasan de mayor a menor grado: resolverPolinomio(a, b, c) = ax² + bx + c = 0 |
| 🧮 | **mate** | `rnd` | `numero, [decimales]` | Alias de redondear. |
| 🧮 | **mate** | `sen` | `rad` | Seno de un ángulo (radianes). |
| 🧮 | **mate** | `senh` | `num` | Seno hiperbólico. |
| 🧮 | **mate** | `signo` | `numero` | Retorna el signo de un número: -1 (negativo), 0 (cero), 1 (positivo). |
| 🧮 | **mate** | `sumatoria` | `lista` | Suma todos los elementos de la lista (Σ). |
| 🧮 | **mate** | `tan` | `rad` | Tangente de un ángulo (radianes). |
| 🧮 | **mate** | `tanh` | `num` | Tangente hiperbólica. |
| 🧮 | **mate** | `techo` | `numero` | Redondea hacia arriba al entero más cercano. |
| 🧮 | **mate** | `trunc` | `numero` | Trunca la parte decimal. |
| 🧮 | **mate** | `varianza` | `lista \| numeros...` | Calcula la varianza muestral de una lista de números. |
| 📥 | **pila** | `alista` | `pila` | Convierte la pila a una lista (fondo→cima). |
| 📥 | **pila** | `apilar` | `pila, elemento` | Agrega un elemento a la cima. |
| 📥 | **pila** | `cima` | `pila` | Obtiene el elemento de la cima sin extraerlo. |
| 📥 | **pila** | `crear` | n/a | Crea una pila vacía. |
| 📥 | **pila** | `desapilar` | `pila` | Extrae el elemento de la cima. |
| 📥 | **pila** | `limpiar` | `pila` | Elimina todos los elementos de la pila. |
| 📥 | **pila** | `longitud` | `pila` | Devuelve la cantidad de elementos. |
| 📥 | **pila** | `vacia` | `pila` | Verifica si la pila no contiene elementos. |
| 🖥️ | **sis** | `apoyar` | n/a | Muestra información para apoyar el proyecto y abre el enlace de patrocinio en el navegador. |
| 🖥️ | **sis** | `argumentos` | n/a | Retorna lista de argumentos pasados al script desde línea de comandos. |
| 🖥️ | **sis** | `config` | `clave, [valor]` | Configuración del motor. Válidas: max_stack, emoji_spacing, regex_timeout |
| 🖥️ | **sis** | `cwd` | n/a | Directorio actual. |
| 🖥️ | **sis** | `dirActual` | n/a | Alias de directorioActual. Retorna el directorio de trabajo actual. |
| 🖥️ | **sis** | `directorioActual` | n/a | Retorna el directorio de trabajo actual. |
| 🖥️ | **sis** | `dormir` | `segundos` | Pausa la ejecución (segundos). |
| 🖥️ | **sis** | `ejecutar` | `comando` | Ejecuta un comando del sistema operativo y retorna la salida. Lanza error si el comando falla. |
| 🖥️ | **sis** | `entorno` | `nombre_var, [valor_defecto]` | Obtiene una variable de entorno. Retorna nulo si no existe, o valor por defecto (arg2). |
| 🖥️ | **sis** | `esperar` | `ms` | Pausa la ejecución (milisegundos). |
| 🖥️ | **sis** | `fecha` | `componente` | Retorna componentes de fecha. Valores: seg/segundo, min/minuto, hora, dia/d_mes, mes, anio/año/year, d_semana/dia_semana, d_año/dia_año/d_anio. |
| 🖥️ | **sis** | `iraxy` | `x, y` | Mueve cursor. |
| 🖥️ | **sis** | `limpiar` | n/a | Limpia consola. |
| 🖥️ | **sis** | `manual` | n/a | Busca y abre el manual PDF oficial incluido en la instalación. |
| 🖥️ | **sis** | `milisegundos` | n/a | Obtiene ms del sistema. |
| 🖥️ | **sis** | `operativo` | `nombre_so` | Para información completa del sistema usa sis.plataforma(). |
| 🖥️ | **sis** | `plataforma` | n/a | Retorna un diccionario con información completa de la plataforma: SO, arquitectura, hardware, usuario, directorios y ... |
| 🖥️ | **sis** | `salir` | n/a | Termina el programa. |
| 🖥️ | **sis** | `tiempo` | `[formato]` | Retorna la fecha/hora actual. Formato opcional: %Y(año), %m(mes), %d(día), %H(hora), %M(min), %S(seg) |
| 🖥️ | **sis** | `usuario` | n/a | Usuario actual. |
| 💎 | **tupla** | `crear` | `elementos...` | Crea una nueva tupla con los elementos proporcionados |
| 💎 | **tupla** | `longitud` | `tupla` | Devuelve el número de elementos en la tupla |
| 💎 | **tupla** | `obtener` | `tupla, indice` | Obtiene el elemento en la posición especificada de la tupla |
| 🔍 | **validar** | `alfanumerico` | `texto` | Verifica si el texto contiene solo letras y números (sin símbolos). |
| 🔍 | **validar** | `analizarCedula` | `texto` | Analiza y clasifica una cédula panameña. Retorna diccionario con: valida, tipo, provincia, tomo, asiento, normalizada. |
| 🔍 | **validar** | `cedula` | `texto` | Valida formato estructural de cédula panameña (personal, extranjero, naturalizado). Acepta con o sin guiones. |
| 🔍 | **validar** | `contrasenaSegura` | `texto` | Verifica si la contraseña es segura (≥8 chars, mayúscula, minúscula, dígito, símbolo). |
| 🔍 | **validar** | `email` | `texto` | Valida formato de correo electrónico (<usuario@dominio.ext>). |
| 🔍 | **validar** | `entero` | `texto` | Verifica si el texto representa un número entero. |
| 🔍 | **validar** | `esPalindromo` | `valor` | Verifica si el valor se lee igual al derecho y al revés (ignora espacios y mayúsculas). |
| 🔍 | **validar** | `ipv4` | `texto` | Valida dirección IPv4 (ej: 192.168.1.1). |
| 🔍 | **validar** | `longitud` | `texto, min, max` | Verifica si la longitud del texto está en el rango [min, max]. |
| 🔍 | **validar** | `numero` | `texto` | Verifica si el texto representa un número válido (entero o decimal). |
| 🔍 | **validar** | `ruc` | `texto` | Valida formato básico de RUC panameño (9-15 caracteres alfanuméricos). |
| 🔍 | **validar** | `telefono` | `texto` | Valida formato telefónico flexible (soporta +, -, espacios, paréntesis). |
| 🔍 | **validar** | `url` | `texto` | Valida formato de URL (http:// o https://). |
| 🌐 | **web** | `codificarUrl` | `texto` | Codifica un string para uso en URL (URL encoding). |
| 🌐 | **web** | `decodificarUrl` | `texto` | Decodifica un string URL-encoded. |
| 🌐 | **web** | `descargar` | `url, ruta_destino` | Descarga un archivo desde una URL. Retorna {bytes, ruta, nombre}. |
| 🌐 | **web** | `enviar` | `url, [datos], [encabezados]` | Realiza una petición POST. Acepta datos (diccionario o string) y encabezados opcionales. |
| 🌐 | **web** | `obtener` | `url, [encabezados]` | Realiza una petición GET. Opcionalmente acepta diccionario de encabezados. |
| 🌐 | **web** | `solicitar` | `configuracion` | Realiza una petición HTTP personalizada. Config: {url, metodo, datos, encabezados, timeout}. |

## 🔌 Plugin: Cripto

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 🔐 | **cripto** | `claveAleatoria` | n/a | Genera una clave aleatoria segura de 256 bits codificada en Base64. |
| 🔐 | **cripto** | `desencriptar` | `cifrado, clave` | Desencripta una cadena producida por cripto.encriptar(). |
| 🔐 | **cripto** | `encriptar` | `texto, clave` | Encripta un texto con AES-256-GCM. Retorna una cadena Base64 auto-contenida |
| 🔐 | **cripto** | `firmar` | `texto, clave` | Genera una firma HMAC-SHA256 del texto usando la clave. |
| 🔐 | **cripto** | `verificar` | `texto, firma, clave` | Verifica que la firma HMAC-SHA256 corresponde al texto y la clave. |

## 🔌 Plugin: Env

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 🌍 | **env** | `escribir` | `nombre, valor` | Define una variable de entorno para el proceso actual. Si el valor es 'nulo', la variable se elimina. |
| 🌍 | **env** | `existe` | `nombre` | Retorna verdadero si la variable de entorno existe en el proceso actual, |
| 🌍 | **env** | `leer` | `nombre, [defecto]` | Lee una variable de entorno del proceso actual. |
| 🌍 | **env** | `leerArchivo` | `ruta, [prefijo]` | Parsea un archivo .env y carga sus variables en el proceso actual. |
| 🌍 | **env** | `todas` | n/a | Retorna un diccionario con todas las variables de entorno del proceso actual. |

## 🔌 Plugin: Formulas

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| ✈️ | **aerodinamica** | `numeroMach` | `velocidadObjeto, velocidadSonido` | Calcula el Número de Mach y el régimen de vuelo. Fórmula: M = v / c |
| 📦 | **algebraLineal** | `abs` | `coleccion` | Aplica valor absoluto a cada elemento del vector o matriz. |
| 📦 | **algebraLineal** | `cholesky` | `matriz_A` | Descompone A = L·Lᵀ para matrices simétricas definidas positivas. |
| 📦 | **algebraLineal** | `clip` | `coleccion, minimo, maximo` | Limita (clamp) cada elemento del vector o matriz al rango |
| 📦 | **algebraLineal** | `cruz` | `vector_a, vector_b` | Calcula el producto cruz (vectorial) de dos vectores en ℝ³. |
| 📦 | **algebraLineal** | `determinante` | `matriz_A` | Calcula el determinante de una matriz cuadrada usando factorización LU. |
| 📦 | **algebraLineal** | `dividirEscalar` | `coleccion, n` | Divide cada elemento de un vector o matriz por un escalar n. |
| 📦 | **algebraLineal** | `dividirTerminos` | `colA, colB` | Divide dos vectores o matrices elemento a elemento. |
| 📦 | **algebraLineal** | `factorizacionLU` | `matriz_A` | Descompone A = P·L·U donde: |
| 📦 | **algebraLineal** | `iguales` | `colA, colB, [tolerancia]` | Compara dos tensores elemento a elemento dentro de una tolerancia. |
| 📦 | **algebraLineal** | `invertir` | `matriz_A` | Calcula la matriz inversa A⁻¹ usando LU con pivoteo parcial. |
| 📦 | **algebraLineal** | `multiplicar` | `matriz_A, matriz_B` | Multiplica dos matrices A (m×k) y B (k×n). |
| 📦 | **algebraLineal** | `multiplicarEscalar` | `coleccion, n` | Multiplica cada elemento de un vector o matriz por un escalar n. |
| 📦 | **algebraLineal** | `multiplicarTerminos` | `colA, colB` | Producto de Hadamard: multiplica dos vectores o matrices elemento |
| 📦 | **algebraLineal** | `negativo` | `coleccion` | Invierte el signo de cada elemento del vector o matriz. |
| 📦 | **algebraLineal** | `norma` | `lista, [tipo]` | Calcula la norma de un vector o matriz. |
| 📦 | **algebraLineal** | `potenciaEscalar` | `coleccion, exp` | Eleva cada elemento del vector o matriz al exponente dado. |
| 📦 | **algebraLineal** | `punto` | `vector_a, vector_b` | Calcula el producto punto (escalar) de dos vectores de igual longitud. |
| 📦 | **algebraLineal** | `raiz` | `coleccion` | Aplica raíz cuadrada a cada elemento del vector o matriz. |
| 📦 | **algebraLineal** | `redondear` | `coleccion, [decimales]` | Redondea cada elemento del vector o matriz al número de decimales |
| 📦 | **algebraLineal** | `resolver` | `matriz_A, vector_b` | Resuelve el sistema lineal A·x = b por eliminación gaussiana con pivoteo parcial. |
| 📦 | **algebraLineal** | `restar` | `colA, colB` | Resta dos vectores o matrices elemento a elemento. |
| 📦 | **algebraLineal** | `restarEscalar` | `coleccion, n` | Resta un escalar n a cada elemento de un vector o matriz. |
| 📦 | **algebraLineal** | `sumar` | `colA, colB` | Suma dos vectores o matrices elemento a elemento. |
| 📦 | **algebraLineal** | `sumarEscalar` | `coleccion, n` | Suma un escalar n a cada elemento de un vector o matriz |
| 📦 | **algebraLineal** | `transponer` | `matriz_A` | Calcula la transpuesta Aᵀ de una matriz. |
| 📦 | **algebraLineal** | `traza` | `matriz_A` | Calcula la traza (suma de la diagonal) de una matriz cuadrada. |
| 📦 | **algebraLineal** | `valoresPropios` | `matriz_A, [max_iter], [tolerancia]` | Calcula todos los valores propios de una matriz cuadrada real |
| 📦 | **algebraLineal** | `valorPropioDominante` | `matriz_A, [tolerancia], [max_iter]` | Encuentra el valor propio dominante (mayor en módulo) y su vector propio |
| 🔭 | **astronomia** | `distanciaParalaje` | `paralajeArcsec` | Distancia a una estrella basada en su ángulo de paralaje. Fórmula: d = 1 / p |
| 🔭 | **astronomia** | `fuerzaGravitacional` | `masa1, masa2, distancia` | Fuerza de atracción gravitacional entre dos cuerpos. Fórmula: F = G·m₁·m₂ / r² |
| 🔭 | **astronomia** | `keplerTercera` | `diccionarioVariables` | Tercera Ley de Kepler. Pasa 2 de: 'T' (periodo s), 'a' (semieje m), 'M' (masa central kg). Fórmula: T² = (4π² / GM... |
| 🔭 | **astronomia** | `velocidadEscape` | `masaCuerpo, radio` | Calcula la velocidad de escape. Fórmula: v_e = √(2·G·M / r) |
| 📦 | **calculo** | `cuadraturaGauss` | `funcion, a, b, [puntos]` | Integración numérica por Cuadratura de Gauss-Legendre. |
| 📦 | **calculo** | `derivada` | `funcion, x, [h], [orden]` | Aproxima la primera derivada f'(x) por Diferencias Finitas. |
| 📦 | **calculo** | `derivadaRichardson` | `funcion, x, [h], [niveles]` | Aproxima f'(x) con alta precisión usando Extrapolación de Richardson. |
| 📦 | **calculo** | `gradiente` | `funcionNVar, listaPunto, [h]` | Calcula el gradiente numérico ∇f en un punto dado. |
| 📦 | **calculo** | `jacobiana` | `funcionVectorial, listaX, [h]` | Calcula la matriz Jacobiana J de una función vectorial F: ℝⁿ → ℝᵐ en el punto X. |
| 📦 | **calculo** | `longitudArco` | `funcion, a, b, n` | Calcula la longitud de arco de y = f(x) en [a, b] numéricamente. |
| 📦 | **calculo** | `monteCarlo` | `funcion, a, b, nMuestras, [semilla]` | Estimación de ∫ₐᵇ f(x)dx por el Método de Monte Carlo. |
| 📦 | **calculo** | `romberg` | `funcion, a, b, [maxNivel], [tolerancia]` | Integración numérica por el Método de Romberg. |
| 📦 | **calculo** | `segundaDerivada` | `funcion, x, [h]` | Aproxima la segunda derivada f''(x) por Diferencias Finitas Centrales. |
| 📦 | **calculo** | `simpson13` | `funcion, a, b, n` | Regla de Simpson 1/3 compuesta. Integra f(x) en [a, b]. |
| 📦 | **calculo** | `simpson38` | `funcion, a, b, n` | Regla de Simpson 3/8 compuesta. Integra f(x) en [a, b]. |
| 📦 | **calculo** | `trapecio` | `funcion, a, b, n` | Integración numérica por la Regla del Trapecio Compuesta. |
| 🚀 | **cinematica** | `energiaCinetica` | `masa, velocidad` | Energía cinética de un objeto en movimiento. Fórmula: Ec = ½·m·v² |
| 🚀 | **cinematica** | `energiaPotencial` | `masa, altura` | Energía potencial gravitatoria (g = 9.80665 m/s²). Fórmula: Ep = m·g·h |
| 🚀 | **cinematica** | `impulso` | `fuerza, tiempo` | Impulso de una fuerza (variación de momento lineal). Fórmula: J = F·Δt |
| 🚀 | **cinematica** | `momentoLineal` | `masa, velocidad` | Cantidad de movimiento (momento lineal). Fórmula: p = m·v |
| 🚀 | **cinematica** | `mru` | `diccionarioVariables` | Movimiento Rectilíneo Uniforme. Pasa 2 de: 'v', 'd', 't'. Fórmula: v = d / t |
| 🚀 | **cinematica** | `mruaPosicion` | `posicionInicial, velocidadInicial, aceleracion, tiempo` | Posición en MRUA (segunda ecuación cinemática). Fórmula: x = x₀ + v₀·t + ½·a·t² |
| 🚀 | **cinematica** | `mruaVelocidad` | `velocidadInicial, aceleracion, tiempo` | Velocidad final en Movimiento Rectilíneo Uniformemente Acelerado. Fórmula: v = v₀ + a·t |
| 🚀 | **cinematica** | `tiroParabolico` | `velocidadInicial, anguloGrados` | Componentes de tiro parabólico. Fórmulas: h_max = (v₀²·sin²(θ))/2g \| d_max = (v₀²·sin(2θ))/g |
| 💻 | **computacion** | `bitsNecesarios` | `num_valores_posibles` | Cantidad mínima de bits necesarios para representar n valores distintos. Fórmula: bits = ⌈log₂(n)⌉ |
| 💻 | **computacion** | `entropiaShannon` | `lista_probabilidades` | Entropía de Shannon de una fuente de información (en bits). Fórmula: H = -Σ pᵢ·log₂(pᵢ) |
| 💻 | **computacion** | `probabilidadColisionHash` | `num_elementos, tamano_tabla` | Probabilidad aproximada de al menos una colisión en una tabla hash (paradoja del cumpleaños). Fórmula: P ≈ 1 - e^(... |
| 💻 | **computacion** | `productoAnchoBandaLatencia` | `ancho_banda_bps, latencia_s` | Producto ancho de banda × latencia (Bandwidth-Delay Product). Indica cuántos bits 'están en vuelo' en un enlace en t... |
| 💻 | **computacion** | `tiempoBusqueda` | `n_elementos, tipo_algoritmo` | Operaciones estimadas para buscar en n elementos. Tipos: 'lineal', 'binaria', 'hash', 'arbol_b'. Referencia Big-O ... |
| ⚙️ | **dinamica** | `energiaMecanica` | `masa, velocidad, altura` | Energía mecánica total (cinética + potencial). Fórmula: Em = ½·m·v² + m·g·h |
| ⚙️ | **dinamica** | `newtonFuerza` | `masa, aceleracion` | Segunda Ley de Newton. Fórmula: F = m·a |
| ⚙️ | **dinamica** | `penduloSimple` | `longitud` | Período de oscilación de un péndulo simple (g = 9.80665 m/s²). Fórmula: T = 2π·√(L/g) |
| ⚙️ | **dinamica** | `peso` | `masa` | Peso de un objeto en la superficie terrestre (g = 9.80665 m/s²). Fórmula: W = m·g |
| ⚙️ | **dinamica** | `potencia` | `trabajo, tiempo` | Potencia mecánica media. Fórmula: P = W / t |
| ⚙️ | **dinamica** | `segundaLeyRotacional` | `momento_inercia, aceleracion_angular` | Segunda Ley de Newton para rotación. Fórmula: τ = I·α |
| ⚙️ | **dinamica** | `trabajo` | `fuerza, distancia, anguloGrados` | Calcula el trabajo mecánico. Fórmula: W = F · d · cos(θ) |
| 📈 | **economia** | `roi` | `gananciaNeta, costoInversion` | Calcula el Retorno de Inversión (ROI). Fórmula: ROI = (Ganancia / Costo) · 100 |
| 📦 | **edo** | `adams` | `funcionF, t0, y0, tf, n` | Resuelve dy/dt = f(t, y) con el método Adams-Bashforth-Moulton de orden 4. |
| 📦 | **edo** | `disparo` | `funcion_sistema, a, b, ya, yb, [tolerancia], [max_iter]` | Resuelve un PVF de segundo orden no lineal por el Método de Disparo. |
| 📦 | **edo** | `euler` | `funcionF, t0, y0, tf, n` | Resuelve dy/dt = f(t, y) con condición inicial y(t₀) = y₀ |
| 📦 | **edo** | `eulerImplicito` | `funcionF, funcionDfDy, t0, y0, tf, n` | Resuelve dy/dt = f(t, y) con el Método de Euler Implícito (hacia atrás). |
| 📦 | **edo** | `eulerMejorado` | `funcionF, t0, y0, tf, n` | Resuelve dy/dt = f(t, y) con el Método de Euler Mejorado (Heun). |
| 📦 | **edo** | `pvf` | `funcion_f, a, b, ya, yb, n` | Resuelve un Problema de Valor de Frontera (PVF) de segundo orden: |
| 📦 | **edo** | `rk4` | `funcionF, t0, y0, tf, n` | Resuelve dy/dt = f(t, y) con el método clásico Runge-Kutta de orden 4. |
| 📦 | **edo** | `rkf45` | `funcionF, t0, y0, tf, [tolerancia], [hInicial], [hMin], [hMax]` | Resuelve dy/dt = f(t, y) con el método Runge-Kutta-Fehlberg RKF45. |
| 📦 | **edo** | `sistemaRk4` | `funcionF, t0, listaY0, tf, n` | Resuelve un sistema de EDOs dY/dt = F(t, Y) con RK4. |
| 📦 | **edo** | `sistemaRkf45` | `funcionF, t0, listaY0, tf, [tolerancia], [hInicial], [hMin], [hMax]` | Resuelve un sistema de EDOs dY/dt = F(t, Y) con RKF45 adaptativo. |
| ⚡ | **electricidad** | `filtroRC` | `resistenciaOhm, capacitanciaF` | Frecuencia de corte (-3dB) de un filtro pasa-bajos RC. Fórmula: f_c = 1 / (2π·R·C) |
| ⚡ | **electricidad** | `leyCoulomb` | `carga1, carga2, distancia` | Fuerza electrostática entre dos cargas. Fórmula: F = k·q₁·q₂ / r² |
| ⚡ | **electricidad** | `leyOhm` | `diccionarioVariables` | Ley de Ohm. Pasa 2 valores: 'V', 'I' o 'R'. Fórmula: V = I · R |
| 🧲 | **electromagnetismo** | `campoElectrico` | `cargaC, distanciaM` | Intensidad del campo eléctrico generado por una carga puntual. Fórmula: E = k·\|q\| / r² |
| 🧲 | **electromagnetismo** | `capacitorEnergia` | `capacitanciaF, voltajeV` | Energía almacenada en un capacitor cargado. Fórmula: U = ½·C·V² |
| 🧲 | **electromagnetismo** | `inductorEnergia` | `inductanciaH, corrienteA` | Energía almacenada en el campo magnético de un inductor. Fórmula: U = ½·L·I² |
| 🧲 | **electromagnetismo** | `leyFaraday` | `numEspiras, deltaFlujoWb, deltaTiempoS` | FEM inducida por variación del flujo magnético (Ley de Faraday). Fórmula: ε = -N·ΔΦ/Δt |
| 🧲 | **electromagnetismo** | `potenciaElectrica` | `diccionarioVariables` | Potencia eléctrica. Pasa 2 de: 'V', 'I', 'R'. Fórmulas: P = V·I \| P = I²·R \| P = V²/R |
| 🧲 | **electromagnetismo** | `potencialElectrico` | `cargaC, distanciaM` | Potencial eléctrico en un punto a distancia r de una carga puntual. Fórmula: V = k·q / r |
| 🧲 | **electromagnetismo** | `resonanciaLC` | `inductanciaH, capacitanciaF` | Frecuencia de resonancia de un circuito LC. Fórmula: f₀ = 1 / (2π·√(L·C)) |
| 🧲 | **electromagnetismo** | `transformador` | `voltajePrimario, espirasPrimario, espirasSecundario` | Voltaje en el secundario de un transformador ideal. Fórmula: V₂ = V₁·(N₂/N₁) |
| 📊 | **estadistica** | `coeficienteVariacion` | `media, desviacion_estandar` | Coeficiente de Variación: dispersión relativa como porcentaje de la media. Fórmula: CV = (σ / μ) · 100 |
| 📊 | **estadistica** | `combinaciones` | `n, r` | Combinaciones sin repetición (coeficiente binomial). Fórmula: C(n,r) = n! / (r!·(n-r)!) |
| 📊 | **estadistica** | `media` | `lista_valores` | Media aritmética de una lista de valores. Fórmula: x̄ = (Σxᵢ) / n |
| 📊 | **estadistica** | `permutaciones` | `n, r` | Permutaciones sin repetición. Fórmula: P(n,r) = n! / (n-r)! |
| 📊 | **estadistica** | `puntajeZ` | `valor, media, desviacion_estandar` | Puntaje Z (estandarización): cuántas desviaciones estándar se aleja un valor de la media. Fórmula: z = (x - μ) / σ |
| 📊 | **estadistica** | `varianzaDesviacion` | `lista_valores, [es_muestra]` | Varianza y desviación estándar. es_muestra=verdadero usa N-1 (Bessel). Fórmula: σ² = Σ(xᵢ - x̄)² / N |
| 🏗️ | **estructuras** | `factorSeguridad` | `resistencia_material, tension_aplicada` | Factor de seguridad estructural. Mínimo recomendado: 1.5 (estático), 3.0 (dinámico). Fórmula: FS = σ_resistencia /... |
| 🏗️ | **estructuras** | `moduloYoung` | `diccionario_variables` | Módulo de Young (elasticidad). Omni-Solver: pasa 2 de 'E', 'sigma', 'epsilon'. Fórmula: E = σ / ε |
| 🏗️ | **estructuras** | `momentoInerciaRectangulo` | `base, altura` | Momento de inercia de una sección rectangular respecto al eje centroidal. Fórmula: I = (b·h³) / 12 |
| 🏗️ | **estructuras** | `pandeoEuler` | `modulo_young_Pa, inercia_m4, longitud_m, condicion_apoyo` | Carga crítica de pandeo de Euler para columnas. condicion_apoyo: 'pp' (ambos articulados, Le=L), 'ef' (empotrado-lib... |
| 🏗️ | **estructuras** | `tensionNormal` | `fuerza_N, area_m2` | Tensión (esfuerzo) normal en una sección transversal. Fórmula: σ = F / A |
| 🏗️ | **estructuras** | `vigaFlechaMax` | `carga_distribuida_N_m, longitud_m, modulo_young_Pa, inercia_m4` | Flecha máxima de una viga simplemente apoyada con carga distribuida uniforme. Fórmula: δ_max = (5·w·L⁴) / (384·E·I) |
| 🏗️ | **estructuras** | `vonMises` | `sigma_x_Pa, sigma_y_Pa, tau_xy_Pa` | Criterio de falla de Von Mises para materiales dúctiles (estado plano de tensiones). Fórmula: σ_v = √(σx² - σx·σy ... |
| 💰 | **finanzas** | `amortizacionCuota` | `capital, tasa_mensual, numero_cuotas` | Cuota mensual fija de un préstamo (sistema francés). Fórmula: C = P · [r(1+r)^n] / [(1+r)^n - 1] |
| 💰 | **finanzas** | `interesCompuesto` | `capital, tasa_anual, tiempo_anios, [periodos_por_anio]` | Monto final con capitalización compuesta. periodos_por_anio: 1=anual, 12=mensual, 365=diario. Fórmula: M = C · (1 ... |
| 💰 | **finanzas** | `interesSimple` | `capital, tasa_anual, tiempo_anios` | Interés simple (sin capitalización). Fórmula: I = C · r · t |
| 💰 | **finanzas** | `puntoEquilibrio` | `costos_fijos, precio_venta, costo_variable_unitario` | Punto de equilibrio: unidades mínimas para cubrir costos. Fórmula: Q = CF / (PV - CV) |
| 💰 | **finanzas** | `valorFuturo` | `valor_presente, tasa_anual, tiempo_anios` | Valor Futuro de un capital a interés compuesto anual. Fórmula: VF = VP · (1 + r)^t |
| 💰 | **finanzas** | `valorPresente` | `valor_futuro, tasa_anual, tiempo_anios` | Valor Presente: ¿cuánto vale hoy un pago futuro? Fórmula: VP = VF / (1 + r)^t |
| 📦 | **fisicaCuantica** | `energiaMasa` | `masaKg` | Equivalencia entre masa y energía (Relatividad Especial de Einstein). Fórmula: E = m·c² |
| 📦 | **fisicaCuantica** | `modeloBohr` | `numeroCuanticoN, numeroAtomicoZ` | Radio, Energía y Velocidad del electrón en un átomo hidrogenoide (Modelo de Bohr). Fórmulas: r = (n²·a₀)/Z \| E = ... |
| 💧 | **fluidos** | `reynolds` | `densidad, velocidad, diametro, viscosidad` | Calcula el número de Reynolds y su régimen. Fórmula: Re = (ρ·v·D) / μ |
| 🔬 | **formulas** | `constantes` | n/a | Devuelve un diccionario con constantes físicas universales. |
| 📦 | **geometria** | `areaCirculo` | `radio` | Calcula el área de un círculo. Fórmula: A = π · r² |
| 📦 | **geometria** | `cilindroCompleto` | `radio, altura` | Área y volumen de un cilindro. Fórmulas: A_lat = 2πrh \| A_tot = 2πr(h+r) \| V = πr²h |
| 📦 | **geometria** | `cono` | `radio, altura` | Área total y volumen de un cono recto. Fórmulas: g = √(r²+h²) \| A_tot = πr(r+g) \| V = (1/3)πr²h |
| 📦 | **geometria** | `distanciaPuntos` | `diccionario_puntos` | Distancia euclidiana entre dos puntos en 2D o 3D. Pasa: {'x1','y1','x2','y2'} o {'x1','y1','z1','x2','y2','z2'} |
| 📦 | **geometria** | `elipse` | `semieje_mayor, semieje_menor` | |
| 📦 | **geometria** | `esfera` | `radio` | Área superficial y volumen de una esfera. Fórmulas: A = 4πr² \| V = (4/3)πr³ |
| 📦 | **geometria** | `piramideRectangular` | `base_largo, base_ancho, altura` | Área total y volumen de una pirámide de base rectangular. Fórmula: V = (1/3)·A_base·h |
| 📦 | **geometria** | `poligonoRegular` | `num_lados, longitud_lado` | Área y perímetro de un polígono regular. Fórmula: A = (n·l²) / (4·tan(π/n)) |
| 📦 | **geometria** | `trapecio` | `base_mayor, base_menor, altura` | Área de un trapecio. Fórmula: A = ((B + b) / 2) · h |
| 🌊 | **hidraulica** | `bernoulli` | `diccionario_punto1, diccionario_punto2, densidad_kg_m3` | Verifica la ecuación de Bernoulli entre dos puntos. Cada punto: {'P','v','z'} Fórmula: H = P/ρg + v²/(2g) + z |
| 🌊 | **hidraulica** | `caudalManning` | `coef_manning, area_hidraulica_m2, radio_hidraulico_m, pendiente` | Caudal en canal abierto por la fórmula de Manning. Fórmula: Q = (1/n)·A·R^(2/3)·S^(1/2) |
| 🌊 | **hidraulica** | `continuidad` | `area1_m2, velocidad1_m_s, area2_m2` | Velocidad en la sección 2 por la ecuación de continuidad (flujo incompresible). Fórmula: A₁·v₁ = A₂·v₂ |
| 🌊 | **hidraulica** | `darcyWeisbach` | `factor_friccion, longitud_m, diametro_m, velocidad_m_s` | Pérdida de carga por fricción en tuberías (Darcy-Weisbach). Fórmula: h_f = f · (L/D) · v²/(2g) |
| 🌊 | **hidraulica** | `hazenWilliams` | `coef_c, radio_hidraulico_m, pendiente` | Velocidad en tuberías por Hazen-Williams (agua a temperatura normal). Coeficiente C: PVC=150, Acero nuevo=140, Hierr... |
| 🌊 | **hidraulica** | `numeroFroude` | `velocidad_m_s, profundidad_hidraulica_m` | Número de Froude para clasificar el régimen de flujo en canal abierto. Fórmula: Fr = v / √(g·D) |
| 📦 | **interpolacion** | `bilineal` | `x0, x1, y0, y1, f00, f10, f01, f11, x, y` | Interpolación bilineal en una celda rectangular 2D. |
| 📦 | **interpolacion** | `evaluarPolinomio` | `lista_coeficientes, x` | Evalúa un polinomio en x usando el Esquema de Horner (O(n), numéricamente estable). |
| 📦 | **interpolacion** | `hermite` | `lista_xs, lista_ys, lista_dys, x` | Interpolación de Hermite cúbica por tramos. |
| 📦 | **interpolacion** | `lagrange` | `lista_xs, lista_ys, x` | Interpola el valor y en x usando el polinomio de Lagrange de grado n-1. |
| 📦 | **interpolacion** | `lineal` | `x0, y0, x1, y1, x` | Interpola (o extrapola) linealmente el valor de 'y' para un punto 'x' dado, |
| 📦 | **interpolacion** | `newtonDivididas` | `lista_xs, lista_ys, x` | Interpola en x usando el polinomio de Newton con Diferencias Divididas. |
| 📦 | **interpolacion** | `regresionLineal` | `lista_xs, lista_ys` | Ajuste de la recta y = a + b·x por el método de Mínimos Cuadrados. |
| 📦 | **interpolacion** | `regresionPolinomial` | `lista_xs, lista_ys, grado` | Ajuste de un polinomio de grado k: y = c₀ + c₁x + c₂x² + ... + cₖxᵏ. |
| 📦 | **interpolacion** | `splineCubico` | `lista_xs, lista_ys, x` | Interpola en x usando un Spline Cúbico Natural. |
| 📦 | **interpolacion** | `splineCubicoLista` | `lista_xs, lista_ys, lista_x_evaluar` | Igual que interpolacion.splineCubico pero evalúa el spline en una lista |
| ∑ | **matematicas** | `logaritmoBase` | `valor, base` | Logaritmo de un número en cualquier base (cambio de base). Fórmula: log_b(x) = ln(x) / ln(b) |
| ∑ | **matematicas** | `progresionAritmetica` | `primerTermino, diferenciaComun, numTerminos` | Suma y término n-ésimo de una progresión aritmética. Fórmulas: aₙ = a₁ + (n-1)·d \| S = n·(a₁ + aₙ)/2 |
| ∑ | **matematicas** | `serieGeometrica` | `primerTermino, razon, numTerminos` | Suma de una serie geométrica finita. Fórmula: S = a·(1 - rⁿ) / (1 - r) |
| 〰️ | **ondas** | `efectoDoppler` | `frecuencia_fuente, velocidad_sonido, velocidad_observador, velocidad_fuente` | Frecuencia percibida por efecto Doppler (+ si se acercan, - si se alejan). Fórmula: f' = f·(v + v_obs) / (v + v_src) |
| 〰️ | **ondas** | `frecuenciaPeriodo` | `diccionario_variables` | Relación inversa entre frecuencia y período. Pasa 'f' o 'T'. Fórmula: f = 1 / T |
| 〰️ | **ondas** | `longitudOndaLuz` | `frecuencia` | Longitud de onda de la luz en el vacío dado su frecuencia (c = 299 792 458 m/s). Fórmula: λ = c / f |
| 〰️ | **ondas** | `velocidadOnda` | `frecuencia, longitud_onda` | Velocidad de propagación de una onda. Fórmula: v = f·λ |
| 👁️ | **optica** | `espejoEsferico` | `radio_curvatura, distancia_objeto` | Posición de la imagen en un espejo esférico. Radio positivo = cóncavo, negativo = convexo. Fórmulas: f = R/2 \| 1/... |
| 👁️ | **optica** | `lenteDelgada` | `diccionario_variables` | Ecuación de lente delgada. Pasa 2 de: 'f', 'do', 'di' (distancias en metros). Fórmula: 1/f = 1/do + 1/di |
| 👁️ | **optica** | `leySnell` | `n1, angulo1_grados, n2` | Ley de Snell-Descartes (refracción). Calcula el ángulo de refracción. Fórmula: n₁·sin(θ₁) = n₂·sin(θ₂) |
| 📦 | **optimizacion** | `bfgs` | `funcion, listaX0, [tolerancia], [maxIter]` | Minimiza f(X) con el método cuasi-Newton BFGS. |
| 📦 | **optimizacion** | `brent1D` | `funcion, a, b, [tolerancia], [maxIter]` | Minimiza f(x) en [a, b] con el Método de Brent para optimización 1D. |
| 📦 | **optimizacion** | `busquedaCuadricula` | `funcion, lista_limites_inf, lista_limites_sup, lista_n_puntos` | Evalúa f(X) en una cuadrícula regular y retorna el mejor punto encontrado. |
| 📦 | **optimizacion** | `descensoGradiente` | `funcion, listaX0, [tasa], [tolerancia], [maxIter]` | Minimiza f(X) con el Método de Descenso de Gradiente (Steepest Descent). |
| 📦 | **optimizacion** | `genetico` | `funcion, listaLimitesInf, listaLimitesSup, [tamPoblacion], [generaciones], [tolerancia], [semilla]` | Minimiza f(X) mediante un Algoritmo Genético de valor real. |
| 📦 | **optimizacion** | `nelderMead` | `funcion, listaX0, [escalaInicial], [tolerancia], [maxIter]` | Minimiza f(X) con el Método de Nelder-Mead (Simplex Downhill). |
| 📦 | **optimizacion** | `recocidoSimulado` | `funcion, listaX0, listaLimitesInf, listaLimitesSup` | Minimiza f(X) con el Método de Recocido Simulado (Simulated Annealing). |
| 📦 | **optimizacion** | `seccionAurea` | `funcion, a, b, [tolerancia], [maxIter]` | Minimiza f(x) en el intervalo [a, b] usando el Método de Sección Áurea. |
| 📦 | **optimizacion** | `simplexLineal` | `listaC, matrizA, listaB` | Resuelve un problema de Programación Lineal en forma estándar: |
| 🧪 | **quimica** | `arrhenius` | `factorPreexponencial, energiaActivacionJ, temperaturaK` | Constante de velocidad de reacción (Ecuación de Arrhenius). R = 8.314 J/(mol·K). Fórmula: k = A·e^(-Ea/RT) |
| 🧪 | **quimica** | `celdaElectroquimica` | `potencialCatodo, potencialAnodo` | Voltaje de celda electroquímica estándar. Fórmula: E°_celda = E°_catodo - E°_anodo |
| 🧪 | **quimica** | `concentracionMolar` | `molesSoluto, volumenLitros` | Concentración molar de una solución. Fórmula: M = n / V |
| 🧪 | **quimica** | `dilucion` | `diccionarioVariables` | Ley de dilución. Pasa 3 de: 'M1', 'V1', 'M2', 'V2'. Fórmula: M₁·V₁ = M₂·V₂ |
| 🧪 | **quimica** | `entalpiaReaccion` | `sumaProductosKJ, sumaReactivosKJ` | Entalpía estándar de reacción (Ley de Hess). Fórmula: ΔH°rxn = ΣΔH°f(productos) - ΣΔH°f(reactivos) |
| 🧪 | **quimica** | `hendersonHasselbalch` | `pka, concentracionBase, concentracionAcido` | pH de una solución buffer (Ecuación de Henderson-Hasselbalch). Fórmula: pH = pKa + log₁₀([A⁻]/[HA]) |
| 🧪 | **quimica** | `michaelisMenten` | `diccionarioVariables` | Cinética enzimática de Michaelis-Menten. Pasa 3 de: 'v', 'Vmax', 'Km', 'S'. Fórmula: v = (Vmax · [S]) / (Km + [S]) |
| 🧪 | **quimica** | `phEstado` | `concentracionH` | Calcula el pH y el estado de la solución. Fórmula: pH = -log₁₀[H⁺] |
| 🧪 | **quimica** | `rendimientoReaccion` | `masaObtenida, masaTeorica` | Rendimiento porcentual de una reacción química. Fórmula: R% = (m_obtenida / m_teórica) · 100 |
| 📦 | **raices** | `biseccion` | `funcion, a, b, [tolerancia], [maxIter]` | Encuentra una raíz de f(x) en el intervalo [a, b] por el método de Bisección. |
| 📦 | **raices** | `brent` | `funcion, a, b, [tolerancia], [maxIter]` | Encuentra una raíz de f(x) en [a, b] por el método de Brent. |
| 📦 | **raices** | `buscarIntervalos` | `funcion, a, b, numSubintervalos` | Escanea [a, b] dividiendo en N subintervalos y detecta dónde f cambia de signo. |
| 📦 | **raices** | `newtonRaphson` | `funcionF, funcionDf, x0, [tolerancia], [maxIter]` | Encuentra una raíz de f(x) por el método de Newton-Raphson. |
| 📦 | **raices** | `puntoFijo` | `funcionG, x0, [tolerancia], [maxIter]` | Calcula un punto fijo x' tal que g(x') = x*, por iteración x_{n+1} = g(x_n). |
| 📦 | **raices** | `regulaFalsi` | `funcion, a, b, [tolerancia], [maxIter]` | Encuentra una raíz de f(x) en [a, b] por el método de Regula Falsi. |
| 📦 | **raices** | `secante` | `funcion, x0, x1, [tolerancia], [maxIter]` | Encuentra una raíz de f(x) por el método de la Secante. |
| 📡 | **senales** | `anchoBandaModulacionAM` | `frecuencia_portadora_Hz, frecuencia_maxima_audio_Hz` | Ancho de banda de una señal AM (doble banda lateral). Fórmula: BW = 2·f_m |
| 📡 | **senales** | `capacidadCanalShannon` | `ancho_banda_Hz, snr_lineal` | Capacidad máxima teórica de un canal con ruido (Teorema de Shannon-Hartley). Fórmula: C = B·log₂(1 + SNR) |
| 📡 | **senales** | `decibelesPotencia` | `potencia_salida, potencia_entrada` | Nivel en decibeles para relación de potencias. Fórmula: dB = 10·log₁₀(P_out / P_in) |
| 📡 | **senales** | `decibelesVoltaje` | `voltaje_salida, voltaje_entrada` | Nivel en decibeles para relación de voltajes (o amplitudes). Fórmula: dBV = 20·log₁₀(V_out / V_in) |
| 📡 | **senales** | `frecuenciaNyquist` | `frecuencia_maxima_senal` | Frecuencia mínima de muestreo para evitar aliasing (Teorema de Nyquist-Shannon). Fórmula: fs_min = 2·f_max |
| 📡 | **senales** | `snr` | `potencia_senal, potencia_ruido` | Relación Señal-Ruido (Signal-to-Noise Ratio). Fórmula: SNR = 10·log₁₀(P_señal / P_ruido) |
| 🔥 | **termodinamica** | `calorEspecifico` | `masa, calorEspecifico, deltaTemperatura` | Calor absorbido o cedido por una sustancia. Fórmula: Q = m·c·ΔT |
| 🔥 | **termodinamica** | `cicloCarnot` | `temperaturaCalienteK, temperaturaFriaK` | Eficiencia máxima teórica del ciclo de Carnot. Fórmula: η = 1 - (T_fría / T_caliente) |
| 🔥 | **termodinamica** | `conduccionCalor` | `conductividad, area, deltaTemperatura, espesor` | Flujo de calor por conducción (Ley de Fourier). Fórmula: Q/t = k·A·ΔT / d |
| 🔥 | **termodinamica** | `dilatacionLineal` | `longitudInicial, coefDilatacion, deltaTemperatura` | Dilatación lineal de un sólido por cambio de temperatura. Fórmula: ΔL = L₀·α·ΔT |
| 🔥 | **termodinamica** | `entropiaCambio` | `calorJ, temperaturaK` | Cambio de entropía en un proceso reversible isotérmico. Fórmula: ΔS = Q / T |
| 🔥 | **termodinamica** | `estadoAgua` | `temperaturaCelsius` | Determina el estado del agua a 1 atm. Diagnóstico: Sólido, Líquido, Gaseoso |
| 🔥 | **termodinamica** | `gasIdeal` | `diccionarioVariables` | Ley de gases ideales. Pasa 3 de: 'P', 'V', 'n', 'T'. Fórmula: P · V = n · R · T |
| 🔥 | **termodinamica** | `stefanBoltzmann` | `temperaturaK, [emisividad], [areaM2]` | Potencia radiada por un cuerpo (Ley de Stefan-Boltzmann). σ = 5.6704×10⁻⁸ W/(m²·K⁴). Fórmula: P = ε·σ·A·T⁴ |

## 🔌 Plugin: Hilos

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 📯 | **canal** | `cerrar` | `id` | Cierra un canal. |
| 📯 | **canal** | `crear` | n/a | Crea un nuevo canal de comunicación. |
| 📯 | **canal** | `eliminar` | `id` | Cierra y devuelve un canal de la memoria liberando sus recursos. |
| 📯 | **canal** | `enviar` | `id, mensaje` | Envía un mensaje a un canal. |
| 📯 | **canal** | `recibir` | `id, [timeoutMs]` | Recibe un mensaje de un canal. |
| 🧵 | **hilos** | `dormir` | `ms` | Pausa la ejecución del hilo actual por N milisegundos. |
| 🧵 | **hilos** | `esperar` | `id, [timeoutMs]` | Espera a que un hilo termine. Con timeout, retorna nulo si no termina a tiempo (el hilo sigue vivo). |
| 🧵 | **hilos** | `esperarTodos` | `lista_ids` | Espera a que todos los hilos en la lista terminen. Retorna lista con resultados (nulo para hilos fallidos). |
| 🧵 | **hilos** | `estaVivo` | `id` | Verifica si un hilo sigue en ejecución. |
| 🧵 | **hilos** | `lanzar` | `funcion, [args...]` | Ejecuta una función en un hilo separado. |
| 🧵 | **hilos** | `limpiar` | n/a | Limpia la tabla de hilos finalizados. |
| 🔒 | **mutex** | `bloquear` | `id` | Adquiere el bloqueo de un mutex. |
| 🔒 | **mutex** | `crear` | n/a | Crea un nuevo objeto mutex. |
| 🔒 | **mutex** | `eliminar` | `id` | Elimina un mutex de la memoria. Lanza error si está bloqueado actualmente. |
| 🔒 | **mutex** | `liberar` | `id` | Libera el bloqueo de un mutex. |

## 🔌 Plugin: Lapsos

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| ⏱️ | **lapsos** | `ahora` | n/a | Fecha actual. |
| ⏱️ | **lapsos** | `analizar` | `txt` | Texto a fecha. |
| ⏱️ | **lapsos** | `comparar` | `fecha1, fecha2` | Compara dos fechas. Retorna -1 si f1<f2, 0 si son iguales, 1 si f1>f2. Acepta cadenas, objetos fecha o números. |
| ⏱️ | **lapsos** | `convertir` | `valor` | Convierte cadena, número o fecha a objeto fecha del plugin. Útil cuando trabajas con archivo.fechaModificacion(). |
| ⏱️ | **lapsos** | `desdeTicks` | `ticks` | Crea una fecha desde un valor de ticks. |
| ⏱️ | **lapsos** | `desdeUnix` | `timestamp` | Crea una fecha desde timestamp Unix (segundos desde 1970-01-01 UTC). |
| ⏱️ | **lapsos** | `diasEnMes` | `fecha` | Retorna la cantidad de días en el mes de la fecha. |
| ⏱️ | **lapsos** | `diferencia` | `f1, f2` | Diferencia entre fechas. |
| ⏱️ | **lapsos** | `esBisiesto` | `anio_o_fecha` | Es bisiesto. |
| ⏱️ | **lapsos** | `esFuturo` | `fecha` | Verifica si la fecha es posterior al momento actual. |
| ⏱️ | **lapsos** | `esPasado` | `fecha` | Verifica si la fecha es anterior al momento actual. |
| ⏱️ | **lapsos** | `esValido` | `cadena, [formato]` | Valida si una cadena representa una fecha válida según el formato. Retorna verdadero/falso sin lanzar error. |
| ⏱️ | **lapsos** | `fecha` | `a,m,d,[h,m,s]` | Crea fecha. |
| ⏱️ | **lapsos** | `finMes` | `fecha` | Retorna el último día del mes de la fecha (a las 23:59:59). |
| ⏱️ | **lapsos** | `formato` | `fecha, [fmt]` | Fecha a texto. |
| ⏱️ | **lapsos** | `hoy` | n/a | Retorna la fecha actual sin componente de hora (00:00:00). |
| ⏱️ | **lapsos** | `inicioMes` | `fecha` | Retorna el primer día del mes de la fecha (a las 00:00:00). |
| ⏱️ | **lapsos** | `restar` | `fecha, cant, unidad` | Resta tiempo. |
| ⏱️ | **lapsos** | `sumar` | `fecha, cant, unidad` | Suma tiempo. |
| ⏱️ | **lapsos** | `ticks` | `fecha` | Obtiene el valor numérico absoluto en ticks (100 nanosegundos desde 0001-01-01). |
| ⏱️ | **lapsos** | `unix` | `fecha` | Convierte fecha a timestamp Unix (segundos desde 1970-01-01 UTC). |
| ⏱️ | **lapsos** | `zonaHoraria` | n/a | Retorna el nombre de la zona horaria del sistema y el offset UTC. |

## 🔌 Plugin: Log

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 📋 | **log** | `advertencia` | `mensaje` | Registra un mensaje de advertencia. |
| 📋 | **log** | `configurar` | `archivo, [nivel], [max_mb]` | Configura el destino y nivel mínimo del log. |
| 📋 | **log** | `depurar` | `mensaje` | Registra un mensaje de depuración (máxima verbosidad). |
| 📋 | **log** | `error` | `mensaje` | Registra un mensaje de error. Siempre se escribe salvo que el nivel |
| 📋 | **log** | `info` | `mensaje` | Registra un mensaje informativo. |
| 📋 | **log** | `limpiar` | n/a | Cierra el archivo de log y libera su handle. |

## 🔌 Plugin: Proceso

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| ⚙️ | **proceso** | `ejecutar` | `cmd, [args], [timeout_ms]` | Ejecuta un comando del sistema y espera su resultado. Retorna diccionario {salida, error, codigo, timeout}. |
| ⚙️ | **proceso** | `ejecutarAsync` | `cmd, [args]` | Inicia un proceso en segundo plano y retorna un ID interno. |
| ⚙️ | **proceso** | `esperar` | `id, [timeout_ms]` | Bloquea hasta que el proceso asíncrono con el ID interno dado termine. |
| ⚙️ | **proceso** | `listar` | n/a | Lista los procesos activos del sistema operativo. |
| ⚙️ | **proceso** | `matar` | `pid` | Termina un proceso del sistema operativo por su PID real. |
| ⚙️ | **proceso** | `resultado` | `id` | Recupera el resultado de un proceso asíncrono terminado. |
| ⚙️ | **proceso** | `shell` | `cmd_string, [timeout_ms]` | Ejecuta un comando a través del shell del sistema |
| ⚙️ | **proceso** | `terminado` | `id` | Verifica si el proceso asíncrono con el ID interno dado ha terminado. |

## 🔌 Plugin: SQLite

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 💾 | **sqlite** | `cerrar` | `id` | Cierra la conexión y libera el archivo inmediatamente. |
| 💾 | **sqlite** | `conectar` | `ruta` | Abre o crea una base de datos SQLite y retorna su ID de conexión. |
| 💾 | **sqlite** | `consultar` | `id, query, [parametros]` | Ejecuta un SELECT y retorna una lista de diccionarios. |
| 💾 | **sqlite** | `ejecutar` | `id, query, [parametros]` | Ejecuta una consulta (INSERT, UPDATE, CREATE) y devuelve las filas afectadas. |
| 💾 | **sqlite** | `transaccion` | `id, funcion_cuerpo` | Ejecuta una función dentro de una transacción atómica con rollback automático. |
| 💾 | **sqlite** | `ultimo_id` | `id` | Devuelve el ID de la última fila insertada en esta conexión. |

## 🔌 Plugin: TablaPeriodica

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 📊 | **tabla** | `balancear` | `ecuacion` | Balancea una ecuación química simple (ej: 'H2 + O2 = H2O'). Retorna: {entrada, balanceada, coeficientes, verificado} |
| 📊 | **tabla** | `compararMasas` | `lista_elementos` | Compara masas molares de una lista de elementos. Retorna {elementos, mas_pesado, mas_liviano, masa_total, promedio}. |
| 📊 | **tabla** | `configuracionDetalle` | `elemento` | Expande la configuración electrónica abreviada de un elemento. Retorna: {simbolo, abreviada, expandida, total_electr... |
| 📊 | **tabla** | `datos` | `elemento` | Devuelve los datos de un elemento por su nombre o símbolo (ej: 'Oro' o 'Au'). |
| 📊 | **tabla** | `estado` | `elemento, temperatura_celsius` | Determina el estado de agregación de un elemento a una temperatura dada en °C. Retorna: {elemento, simbolo, temperat... |
| 📊 | **tabla** | `filtrar` | `campo, valor` | Filtra elementos por 'categoria', 'periodo' o 'bloque'. Retorna lista de diccionarios de elementos. |
| 📊 | **tabla** | `isotopos` | `elemento` | Retorna los isótopos estables conocidos de un elemento. Retorna: {elemento, simbolo, numero_atomico, isotopos_establ... |
| 📊 | **tabla** | `masaMolar` | `formula` | Calcula la masa molar de una fórmula química estándar (ej: 'H2O', 'CO2'). |

## 🔌 Plugin: Texto

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 📝 | **texto** | `anexar` | `id, valores...` | Añade uno o más valores al final del texto actual sin salto de línea. |
| 📝 | **texto** | `anexarLinea` | `id, [valores...]` | Añade valores opcionales seguidos de un salto de línea. |
| 📝 | **texto** | `crear` | `[capacidad_inicial]` | Crea un nuevo constructor de texto mutable y retorna su ID único. |
| 📝 | **texto** | `eliminar` | `id` | Destruye el buffer y lo elimina del registro, liberando su memoria. |
| 📝 | **texto** | `limpiar` | `id` | Vacía el contenido del buffer sin destruir el objeto ni liberar la memoria |
| 📝 | **texto** | `longitud` | `id` | Retorna la longitud actual del texto en el buffer en O(1), |
| 📝 | **texto** | `obtener` | `id` | Materializa y retorna como cadena inmutable todo el texto construido. |
| 📝 | **texto** | `reemplazar` | `id, viejo, nuevo` | Reemplaza todas las ocurrencias de 'viejo' por 'nuevo' DENTRO del buffer, |

## 🔌 Plugin: Zip

| 👁️ | Librería | Función | Parámetros | Texto de Ayuda |
| :--- | :--- | :--- | :--- | :--- |
| 🗜️ | **zip** | `agregar` | `archivo_zip, archivo_origen, [ruta_interna]` | Añade o reemplaza un archivo dentro de un zip existente (o lo crea si no existe). |
| 🗜️ | **zip** | `comprimir` | `carpeta_origen, archivo_destino` | Comprime una carpeta completa en un archivo .zip. |
| 🗜️ | **zip** | `comprimirArchivo` | `archivo_origen, archivo_destino` | Comprime un solo archivo en un .zip. |
| 🗜️ | **zip** | `contiene` | `archivo_zip, ruta_interna` | Retorna verdadero si la ruta interna existe dentro del zip. |
| 🗜️ | **zip** | `descomprimir` | `archivo_zip, carpeta_destino` | Extrae el contenido de un .zip en una carpeta. Sobreescribe archivos existentes. |
| 🗜️ | **zip** | `listar` | `archivo_zip` | Retorna una lista con las rutas internas de todos los archivos dentro del zip. |
| 🗜️ | **zip** | `cortar` | `archivo_origen, tamaño, [unidad], [dir_destino]` | Divide cualquier archivo en fragmentos del tamaño indicado. Unidad por defecto: 'MB' ('B', 'KB', 'MB', 'GB'). Retorna lista de rutas. |
| 🗜️ | **zip** | `cortarPartes` | `archivo_origen, num_partes, [dir_destino]` | Divide cualquier archivo equitativamente en un número exacto de partes. Retorna lista de rutas. |
| 🗜️ | **zip** | `unir` | `lista_partes, archivo_destino` | Une una lista de fragmentos en el archivo de destino original. Retorna cierto si tuvo éxito. |
