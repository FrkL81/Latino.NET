# CAPÍTULO 0: Introducción y Configuración

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Estudiantes, docentes, autodidactas, analistas y programadores de cualquier nivel que deseen dominar un lenguaje industrial y expresivo con sintaxis natural en español.  
**Propósito:** Proporcionar una inmersión completa al ecosistema de Latino.NET: filosofía de diseño, instalación multiplataforma, configuración del entorno en Visual Studio Code y creación de tus primeros programas ejecutables.

## La Puerta de Entrada a la Programación en Español

Aprender a programar ha sido históricamente una experiencia de doble fricción: para millones de hispanohablantes, dominar la lógica algorítmica exigía lidiar simultáneamente con una barrera idiomática artificial impuesta por el inglés técnico. Durante décadas se asumió que programar en español era solo una curiosidad académica o un juguete de iniciación incapaz de operar en entornos profesionales.

Latino.NET nace para derribar definitivamente ese mito.

Detrás de su gramática limpia, directa y elegante en español, late la infraestructura de **.NET 10**: uno de los entornos de ejecución más veloces, estables y probados en la industria del software corporativo. Latino.NET no es un simple traductor de palabras clave ni un envoltorio superficial; es un motor de lenguaje completamente nuevo, diseñado desde sus cimientos para ofrecer una experiencia accesible al estudiante sin renunciar jamás a la potencia de grado industrial que exige la ingeniería moderna.

### Un Manual para Toda la Comunidad

Tienes en tus manos el **Manual Oficial del Programador de Latino.NET**, concebido como una obra viva y progresiva de 19 capítulos (del 0 al 18), complementada con referencias técnicas de API, sintaxis y herramientas de desarrollo.

Este libro fue diseñado bajo un principio de **inclusividad arquitectónica**:

* **Si estás dando tus primeros pasos en la informática:** Encontrarás explicaciones claras, libres de jerga innecesaria, ejemplos ejecutables desde el primer minuto y una rampa de acceso suave que te permitirá concentrarte en lo verdaderamente importante: aprender a estructurar el pensamiento lógico y resolver problemas.
* **Si ya tienes experiencia en otros lenguajes (Python, JavaScript, C#, Java):** Descubrirás un entorno multiparadigma real que integra programación imperativa, orientación a objetos con concordancia de género, pipelines funcionales (`|>`) y extensiones nativas en C# sin la pesadez de configuraciones complejas.

### Los Cuatro Pilares del Ecosistema

A lo largo de este compendio dominarás las cuatro dimensiones que convierten a Latino.NET en una herramienta versátil para el aula, el laboratorio científico y el servidor de producción:

1. **Sintaxis Natural y Ergonomía:** Estructuras de control y datos que se leen en español fluido, respetando acentos lógicos (`ý`, `ó`), caracteres propios (`ñ`) y plantillas estructurales (`ç`).
2. **Robustez y Resiliencia (*Fail-Safe*):** Manejo profesional de excepciones (`intentar / atrapar / lanzar`), defensas contra recursión descontrolada y memoria administrada por el recolector de basura de .NET.
3. **Alto Rendimiento por Delegación:** Algoritmos pesados de ordenamiento, criptografía, compresión y matemáticas que corren a velocidad nativa de máquina.
4. **Tooling de Primera Clase:** Soporte oficial para Visual Studio Code con el protocolo estándar de servidores de lenguaje (*LSP*), autocompletado en tiempo real, diagnósticos y plantillas interactivas.

> 💡 **Principio de Accesibilidad Universal (Mantra de Latino.NET):**  
> *\"La lógica no tiene fronteras lingüísticas; tu lenguaje de programación tampoco debería tenerlas.\"*  
> Latino.NET es una rampa de despegue diseñada para que pienses como programador en tu propio idioma y construyas software con los mismos estándares de calidad que rigen la industria global.

## 0.1 ¿Qué es Latino.NET?

### La Evolución del Código en Español

**Latino.NET** es más que un lenguaje de programación; es un **puente cultural y tecnológico**.

Es una reimplementación moderna del lenguaje **Latino** (creado por Primitivo Roman Montero en 2015), construida completamente desde cero sobre la plataforma industrial **.NET 10** de Microsoft.

**No es un traductor.** No es un "wrapper" del Latino original. Es un **motor de lenguaje completamente nuevo**, diseñado con arquitectura profesional, que respeta la sintaxis que la comunidad hispanohablante ya conoce y ama.

### El Problema que Resuelve

La programación es una habilidad universal, pero su puerta de entrada ha estado históricamente cerrada por una barrera lingüística: **el inglés**.

Para millones de hispanohablantes, aprender lógica de programación implica luchar simultáneamente contra dos frentes:

1. Entender **el algoritmo** (la lógica del problema)
2. Traducir mentalmente **comandos en inglés** (`while`, `print`, `foreach`, `try/catch`)

**Latino.NET elimina esa fricción** permitiendo escribir código con sintaxis natural en español:

```latino
# En lugar de esto (JavaScript):
while (count < 10) {
    console.log("Conteo: " + count);
    count++;
}

# Escribes esto (Latino.NET):
conteo = 0
mientras (conteo < 10)
    escribir("Conteo: " .. conteo)
    conteo++
fin
```

*La diferencia parece cosmética, pero el impacto cognitivo es enorme, especialmente en etapas tempranas del aprendizaje.*

### ¿Y el inglés?

Seamos claros: **Latino.NET no compite con el inglés; te libera para que lo aprendas sin prisa.**

La industria del software —documentación, comunidades, ofertas laborales, frameworks— opera en inglés. Negarlo sería irresponsable. Pero exigirle a un principiante que domine *dos* idiomas simultáneamente (el de la lógica y el de Shakespeare) es una barrera innecesaria que ha alejado a millones de personas talentosas de la programación.

Nuestra postura es pragmática:

* **Aprende a pensar como programador en tu idioma.** La lógica, los algoritmos y la resolución de problemas no pertenecen a ninguna lengua.
* **Incorpora el inglés técnico a tu ritmo.** Una vez que dominas los conceptos, traducir `mientras` a `while` es trivial. Hacerlo al revés —entender `while` sin saber qué es un bucle— es innecesariamente difícil.

Latino.NET es una **rampa de acceso**, no un destino final. Te quita la fricción inicial para que, cuando decidas dar el salto al inglés (y lo harás), ya tengas una base sólida sobre la cual construir.

### ¿Por Qué Ahora? ¿Por Qué .NET?

**El momento es el correcto** por tres razones técnicas:

1. **.NET 10 es multiplataforma y gratuito**
   * Corre en Windows, Linux y macOS sin modificaciones
   * El runtime es de código abierto y está respaldado por Microsoft
   * Permite compilar ejecutables autocontenidos (no requiere que el usuario instale .NET)

2. **La tecnología de compilación moderna lo permite**
   * Herramientas como Roslyn y el Language Server Protocol (LSP) hacen posible crear editores inteligentes sin reinventar la rueda
   * Native AOT permite compilar a código nativo para máximo rendimiento

3. **El ecosistema .NET es vasto**
   * Puedes extender Latino.NET con librerías escritas en C#, F# o VB.NET
   * Acceso a bases de datos, APIs web, machine learning, todo disponible vía plugins

## 0.2 Filosofía de Diseño

### "Respetar el Pasado, Construir el Futuro"

Latino.NET nace de una idea sencilla: **un lenguaje puede evolucionar sin perder aquello que lo hizo especial**.

Nuestro principio rector, **"Respetar el Pasado, Construir el Futuro"**, representa la relación entre Latino y Latino.NET. Latino es nuestro **lenguaje mentor**: el proyecto que nos inspiró, cuya sintaxis y filosofía sirven como punto de partida, y sin el cual Latino.NET simplemente no existiría.

Por eso, Latino.NET no pretende reemplazar ni desmerecer a Latino. Es, ante todo, **un homenaje y una evolución técnica de ese legado**, construido desde cero sobre .NET para explorar hasta dónde puede llegar aquella idea original.

1. **Respetar el Pasado**

   Conservamos aquello que define la experiencia de programar en Latino y que forma parte de su identidad:

   * La sintaxis natural en español (`si`, `mientras`, `funcion`)
   * Las palabras clave (`verdadero`, `falso`, `nulo`)
   * La estructura fundamental del lenguaje
   * Las convenciones y conceptos que la comunidad reconoce

   Nuestro objetivo es mantener una alta compatibilidad con los programas existentes, para que quienes conocen Latino puedan sentirse en casa al utilizar Latino.NET.

2. **Construir el Futuro**

   Sobre esa base conocida, Latino.NET incorpora una arquitectura moderna que permite ampliar considerablemente las posibilidades del lenguaje:

   ✅ **Robustez**

   * Manejo de errores recuperable (`intentar/atrapar`)
   * Protección contra desbordamientos de memoria
   * Detección de ciclos infinitos en módulos
   * Arquitectura preparada para escenarios de uso más exigentes

   ✅ **Programación Multiparadigma**

   * Programación imperativa y estructurada
   * Programación orientada a objetos mediante clases, herencia y polimorfismo
   * Programación funcional con funciones de orden superior, lambdas, closures y operaciones sobre colecciones

   Cada paradigma se implementa de forma nativa dentro del lenguaje, manteniendo la simplicidad de su sintaxis.

   ✅ **Interoperabilidad**

   * Sistema de plugins (`.dll`) para extender el lenguaje con C#, F# o VB.NET
   * Acceso al amplio ecosistema de .NET
   * Integración con bibliotecas y tecnologías existentes del entorno .NET

   ✅ **Experiencia de Desarrollador (DX)**

   * Language Server Protocol para autocompletado inteligente
   * Diagnósticos en tiempo real mientras escribes
   * Snippets y documentación contextual
   * Herramientas de desarrollo integradas con Visual Studio Code

**Latino.NET mira hacia adelante sin olvidar de dónde viene.**

La sintaxis que aprendimos de Latino sigue siendo reconocible; el motor que la ejecuta ha sido reconstruido para permitir nuevas posibilidades.

**Respetamos el pasado porque forma parte de nuestra identidad. Construimos el futuro porque queremos que esa idea siga viva.**

## 0.3 Características Principales

### Paradigma Multidisciplinar

Latino.NET no te encasilla en un solo estilo de programación. Puedes usar el paradigma que mejor se adapte a tu problema:

🔹 **Imperativo/Estructurado**

```latino
contador = 0
mientras (contador < 5)
    escribir("Iteración: " .. contador)
    contador++
fin
```

🔹 **Orientado a Objetos (POO)**

```latino
clase Perro
    constructor(nombre)
        mi.nombre = nombre
    fin
    
    funcion Ladrar()
        escribir(mi.nombre .. " dice: ¡Guau!")
    fin
fin

firulais = nueva Perro("Firulais")
firulais.Ladrar()  # Salida: Firulais dice: ¡Guau!
```

🔹 **Funcional**

```latino
numeros = [1, 2, 3, 4, 5]

# Map: Duplicar cada número
duplicados = lista.transformar(numeros, funcion(n)
    retornar n * 2
fin)

# Filter: Números mayores que 5
filtrados = lista.filtrar(duplicados, funcion(n)
    retornar n > 5
fin)

escribir(filtrados)  # [6, 8, 10]
```

### Sistema de Tipos Dinámico pero Seguro

**Flexibilidad sin caos:**

```latino
# No necesitas declarar tipos
edad = 25           # Número
nombre = "Carlos"   # Texto
activo = verdadero  # Lógico

# Puedes cambiar el tipo de una variable
edad = "veinticinco"  # Ahora es texto
```

**Pero con protección:**

```latino
# Las librerías validan tipos
lista.agregar("texto", 5)  # ❌ Error: Primer argumento debe ser lista

# El motor detecta operaciones inválidas
resultado = "hola" / 2     # ❌ Error Runtime: No se puede dividir texto
```

### Gestión de Memoria Automática

Latino.NET utiliza el **Garbage Collector de .NET**, uno de los más avanzados del mundo.

**Ventajas:**

* No necesitas liberar memoria manualmente (no hay `free()` ni `delete`)
* Maneja referencias circulares sin problemas
* Optimizado para procesos de larga duración

**Ejemplo de referencia circular que NO causa fuga:**

```latino
# Lista que se contiene a sí misma
miLista = [1, 2, 3]
miLista.agregar(miLista)  # ¡Lista dentro de lista!

# El GC de .NET detecta esto y lo limpia correctamente
escribir(miLista)  # No cuelga el programa
```

### Extensibilidad Nativa (Plugins)

Esta es la característica que distingue a Latino.NET de otros lenguajes educativos.

**Puedes cargar librerías .NET compiladas:**

```latino
# Ejemplo conceptual: plugins de terceros
# (Latino.NET incluye SQLite como motor de bases de datos integrado)
# incluir("plugins/Latino.MySQL.dll")  # ← Plugin ficticio, no incluido en la distribución
# conexion = mysql.conectar("localhost", "usuario", "clave")

# Alternativa real con el plugin SQLite incluido:
incluir("sqlite")
db = sqlite.conectar("mi_base.db")

# Higiene: cerrar la conexión y borrar el archivo que acabamos de crear
sqlite.cerrar(db)
archivo.borrar("mi_base.db")
```

Esto permite:

* **Rendimiento nativo** para operaciones pesadas
* **Acceso a bases de datos** (MySQL, PostgreSQL, SQL Server)
* **APIs web, hardware, criptografía** - todo el ecosistema .NET

## 0.4 Ventajas Competitivas

### ¿Por Qué Elegir Latino.NET?

| Aspecto | Latino.NET | Otros Lenguajes Educativos |
| :--- | :--- | :--- |
| **Idioma** | Español nativo | Inglés o traducción superficial |
| **Rendimiento** | Compilado JIT (.NET) | Intérprete sin optimizaciones JIT |
| **Robustez** | Manejo de errores profesional | Manejo de errores limitado |
| **Extensibilidad** | Plugins .NET | Limitado o inexistente |
| **Herramientas** | LSP, autocompletado, diagnósticos | Texto plano o básico |
| **Escalabilidad** | De scripts a sistemas complejos | Solo scripts pequeños |

### Casos de Uso Ideales

✅**Educación**

* Enseñar algoritmos y estructuras de datos sin la barrera del inglés
* Introducir POO con sintaxis familiar
* Preparar estudiantes para lenguajes industriales (C#, Java)

✅ **Scripting y Automatización**

* Procesamiento de archivos CSV, JSON, texto
* Tareas de sistema operativo (renombrar archivos, backups)
* Cálculos matemáticos y estadísticos

✅ **Prototipado Rápido**

* Probar lógica de negocio antes de implementar en C#
* Lenguaje "pegamento" para orquestar librerías .NET
* Crear herramientas internas de empresa

❌ **Cuándo NO usarlo**

* Aplicaciones de alto rendimiento crítico (juegos 3D, trading de alta frecuencia)
* Desarrollo web frontend (usa JavaScript/TypeScript)
* Sistemas operativos o drivers (usa C/Rust)

## 0.5 Descarga del Intérprete

### ¿Qué Vas a Instalar?

El **intérprete de Latino.NET** (también llamado "motor" o "runtime") es un ejecutable autocontenido que:

* **NO requiere** que instales .NET en tu computadora
  * Pesa aproximadamente **70-80 MB** (incluye todo lo necesario)
  * Es un **archivo único** (en Windows: `latino.exe`, en Linux/Mac: `latino`)
  * Funciona desde el momento en que lo descargas

**Versión actual:** v1.0.81

### Dónde Descargar

Dirígete a la página oficial de lanzamientos en GitHub:

👉 **[https://github.com/FrkL81/Latino.NET/releases/latest](https://github.com/FrkL81/Latino.NET/releases/latest)**

En la sección **Assets** encontrarás los archivos comprimidos para cada sistema operativo:

* 🪟 **Windows:** `latino-win-v1.0.81.zip`
* 🐧 **Linux:** `latino-linux-v1.0.81.zip`
* 🍎 **macOS:** (Requiere compilación manual, ver sección correspondiente)

## 0.6 Instalación en Windows

### Paso 1: Descargar el Archivo (win)

1. Haz clic en `latino-win-v1.0.81.zip` para descargarlo
2. Guarda el archivo en tu carpeta de **Descargas**

### Paso 2: Descomprimir (win)

* **Opción A: Crear Carpeta Dedicada (Recomendado)**

   1. Abre el **Explorador de Archivos**
   2. Navega a tu disco `C:\`
   3. Crea una carpeta llamada `LatinoNET` (ruta final: `C:\LatinoNET`)
   4. Haz clic derecho sobre el archivo descargado → **Extraer todo...**
   5. Cambia la ruta de destino a `C:\LatinoNET`
   6. Haz clic en **Extraer**

* **Opción B: Cualquier Ubicación**

Puedes descomprimir en cualquier carpeta, pero **recuerda la ruta completa** porque la necesitarás después.

Ejemplos válidos:

* `C:\Programas\LatinoNET`
* `D:\Herramientas\Latino`
* `C:\Users\TuNombre\Latino`

### Paso 3: Configurar el PATH (Variable de Entorno)

#### ¿Qué es el PATH y Por Qué es Importante?

El **PATH** es una lista de carpetas donde Windows busca programas ejecutables.

**Sin PATH configurado:**

```cmd
C:\Users\Carlos> latino hola.lat
'latino' no se reconoce como un comando interno o externo...
```

**Con PATH configurado:**

```cmd
C:\Users\Carlos> latino hola.lat
¡Hola Mundo!
```

#### Cómo Configurar el PATH

1. **Abrir Configuración de Variables de Entorno**

   * Presiona la tecla `Windows`
   * Escribe: **"variables de entorno"**
   * Selecciona: **"Editar las variables de entorno del sistema"**
   * Se abrirá la ventana de **Propiedades del sistema**

2. **Acceder a Variables de Entorno**

   * Haz clic en el botón **"Variables de entorno..."** (esquina inferior derecha)
   * Se abrirá una nueva ventana con dos secciones

3. **Editar la Variable PATH**

   * En la sección de abajo (**Variables del sistema**), busca la variable llamada **Path**
   * Selecciónala y haz clic en **Editar...**
   * Se abrirá la ventana de edición del Path

4. **Agregar la Ruta de Latino.NET**

   * Haz clic en el botón **Nuevo**
   * Escribe la ruta completa donde descomprimiste Latino.NET

  **Ejemplo si usaste `C:\LatinoNET`:**

  ```text
  C:\LatinoNET
  ```

  **Ejemplo si usaste otra ubicación:**

  ```text
  C:\Users\TuNombre\Latino
  ```

* Haz clic en **Aceptar** en todas las ventanas abiertas

### Paso 4: Reiniciar el Sistema

**⚠️ Importante:** Los cambios en las variables de entorno **NO se aplican inmediatamente**.

Debes **reiniciar tu computadora** para que Windows reconozca la nueva ruta.

### Paso 5: Verificar la Instalación

Después del reinicio:

1. Presiona `Windows + R`
2. Escribe `cmd` y presiona Enter
3. En la ventana negra (Command Prompt), escribe:

```cmd
latino -v
```

**Salida esperada:**

```text
 _          _   _                   _   _ _____ _____ 
| |    __ _| |_(_)_ __   ___       | \ | | ____|_   _|
| |   / _` | __| | '_ \ / _ \      |  \| |  _|   | |  
| |__| (_| | |_| | | | | (_) |  _  | |\  | |___  | |  
|_____\__,_|\__|_|_| |_|\___/  (_) |_| \_|_____| |_|  

  Versión v1.0.81 (Core .NET 10)
  Copyright © 2026 Franklin A. Lara P.
```

Si ves el arte ASCII y la versión, **¡la instalación fue exitosa!** 🎉

### Solución de Problemas en Windows

#### Problema 1: "latino no se reconoce como comando"

**Causas posibles:**

1. **No reiniciaste el sistema** → Reinicia y vuelve a intentar
2. **Ruta incorrecta en PATH** → Verifica que la ruta coincida exactamente con donde está el archivo `latino.exe`
3. **Escribiste mal la ruta** → Asegúrate de no tener espacios extra o errores tipográficos

**Verificación manual:**

```cmd
# Navega a la carpeta donde está Latino
cd C:\LatinoNET

# Ejecuta directamente
latino.exe -v
```

Si funciona aquí pero no desde cualquier ubicación, el problema es el PATH.

#### Problema 2: "Windows protegió tu PC"

Al intentar ejecutar `latino.exe` por primera vez, Windows SmartScreen puede bloquearlo.

**Solución:**

1. Haz clic en **"Más información"**
2. Haz clic en **"Ejecutar de todas formas"**

Esto es normal para ejecutables descargados de internet. Latino.NET no está firmado digitalmente (aún), pero es 100% seguro.

#### Problema 3: Conflicto con Latino

Si ya tenías instalada la versión clásica de Latino (escrita en C), ambas versiones usan el comando `latino`.

**Verificar cuál versión estás ejecutando:**

```cmd
latino -v
```

  > Si ves `Latino 1.4.x` → Estás usando la versión clásica (antigua)
  > Si ves `Latino.NET v1.0.81` → Estás usando la versión nueva (correcta)

* **Solución 1: Desinstalar Latino**

Elimina la versión clásica (C) y deja solo Latino.NET (.NET 10).

* **Solución 2: Priorizar Latino.NET en el PATH**

1. Abre de nuevo la edición de Variables de Entorno
2. En la lista del **Path**, selecciona la ruta de Latino.NET
3. Usa los botones **Subir** para moverla **arriba** de la ruta del Latino
4. Windows buscará primero en esa carpeta

* **Solución 3: Renombrar el Ejecutable**

Cambia el nombre de `latino.exe` a `latinonet.exe`. Así podrás tener ambas versiones:

* `latino` ejecuta la versión clásica
* `latinonet` ejecuta Latino.NET

### Opción Alternativa: Sin Configurar PATH

Si no quieres lidiar con variables de entorno, puedes usar Latino.NET sin configurar el PATH:

**1. Desde la carpeta de instalación:**

```cmd
cd C:\LatinoNET
latino.exe mi_script.lat
```

**2. Con ruta completa:**

```cmd
C:\LatinoNET\latino.exe C:\MisScripts\hola.lat
```

**3. Configurar solo en VS Code:**

Se detalla en la sección 0.11 de este mismo documento.

## 0.7 Instalación en Linux

### Paso 1: Descargar el Archivo (linux)

Descarga `latino-linux-v1.0.81.zip` desde la página de releases.

O usa `wget` desde la terminal:

```bash
cd ~/Descargas
wget https://github.com/FrkL81/Latino.NET/releases/download/v1.0.81/latino-linux-v1.0.81.zip
```

### Paso 2: Descomprimir (linux)

```bash
# Crear una carpeta dedicada y extraer allí el paquete
mkdir -p ~/latino
unzip ~/Descargas/latino-linux-v1.0.81.zip -d ~/latino
```

### Paso 3: Instalar Globalmente

* **Opción A: Instalación Global (con sudo)**

Esta es la forma recomendada para que todos los usuarios del sistema puedan usar Latino.NET:

```bash
# Crear un enlace simbólico al binario (NO moverlo)
sudo ln -sf ~/latino/latino /usr/local/bin/latino
```

* **Opción B: Instalación de Usuario (sin sudo)**

Si no tienes permisos de administrador o prefieres instalarlo solo para tu usuario:

```bash
# Crear carpeta local de binarios (si no existe)
mkdir -p ~/.local/bin

# Crear el enlace en tu PATH personal
ln -sf ~/latino/latino ~/.local/bin/latino

# Agregar al PATH (solo si no está ya)
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

> **¿Por qué un enlace simbólico?** El motor resuelve sus plugins en la carpeta `lib/`
> que acompaña al binario. Si se mueve solo el ejecutable
> (`sudo mv dist/linux/latino /usr/local/bin/`), `lib/` se queda atrás y los plugins
> dejan de cargar. El enlace simbólico mantiene el binario y su carpeta `lib/` juntos,
> de modo que el motor siempre resuelve sus dependencias.

### Paso 4: Verificar la Instalación

Abre una **nueva terminal** y ejecuta:

```bash
latino -v
```

**Salida esperada:**

```text
 _          _   _                   _   _ _____ _____ 
| |    __ _| |_(_)_ __   ___       | \ | | ____|_   _|
| |   / _` | __| | '_ \ / _ \      |  \| |  _|   | |  
| |__| (_| | |_| | | | | (_) |  _  | |\  | |___  | |  
|_____\__,_|\__|_|_| |_|\___/  (_) |_| \_|_____| |_|  

  Versión v1.0.81 (Core .NET 10)
  Copyright © 2026 Franklin A. Lara P.
```

### Solución de Problemas en Linux

#### Problema 1: "command not found"

**Causas:**

1. No abriste una terminal **nueva** después de la instalación
2. El PATH no está configurado correctamente (solo en instalación de usuario)
3. El archivo no tiene permisos de ejecución

**Verificación:**

```bash
# Comprobar si el archivo existe
ls -l /usr/local/bin/latino

# O si usaste instalación de usuario
ls -l ~/.local/bin/latino

# Comprobar si tiene permisos de ejecución (debe mostrar 'x')
# Ejemplo de salida correcta:
# -rwxr-xr-x 1 usuario grupo 75M fecha latino
```

**Solución:**

```bash
# Si falta el permiso de ejecución
chmod +x /ruta/al/latino

# Si el PATH no está configurado (instalación de usuario)
echo $PATH  # Verifica si ~/.local/bin está en la lista

# Si no está, agrégalo
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

#### Problema 2: "Permission denied"

Si obtienes este error al intentar ejecutar `latino`:

```bash
# Asegúrate de dar permisos de ejecución
chmod +x /usr/local/bin/latino

# O con sudo si es necesario
sudo chmod +x /usr/local/bin/latino
```

#### Problema 3: "No such file or directory" (dependencias faltantes)

En algunas distribuciones minimalistas, pueden faltar librerías básicas:

```bash
# Instalar dependencias necesarias (Debian/Ubuntu)
sudo apt-get install libicu-dev

# Para Fedora/CentOS
sudo dnf install icu

# Para Arch Linux
sudo pacman -S icu
```

## 0.8 Instalación en macOS

### Nota Importante para Usuarios de Mac

> ⚠️ **Nota:** Debido a las políticas de seguridad de Apple...

Latino.NET **no está firmado** (requiere una cuenta de desarrollador de Apple de $99/año).

**Por esta razón, recomendamos compilar tu propia versión nativa.**

### Requisitos Previos

Necesitas tener instalado el **.NET SDK 10**:

1. Ve a: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Descarga el **SDK** (no el Runtime) para macOS
3. Sigue el instalador

**Verificar instalación:**

```bash
dotnet --version
# Debe mostrar: 10.x.x
```

### Paso 1: Clonar el Repositorio

```bash
# Instalar Git si no lo tienes
# En macOS con Homebrew:
brew install git

# Clonar el repositorio
git clone https://github.com/FrkL81/Latino.NET.git
cd Latino.NET
```

### Paso 2: Compilar según tu Procesador

**Para Macs con Apple Silicon (M1, M2, M3, M4):**

```bash
dotnet publish src/Latino.App/Latino.App.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained \
    -o ./bin_mac
```

**Para Macs con Procesador Intel:**

```bash
dotnet publish src/Latino.App/Latino.App.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained \
    -o ./bin_mac
```

**¿Cómo saber qué procesador tengo?**

1. Haz clic en el menú  (esquina superior izquierda)
2. Selecciona **"Acerca de este Mac"**
3. Si dice **"Chip Apple M1/M2/M3/M4"** → Usa `osx-arm64`
4. Si dice **"Procesador Intel"** → Usa `osx-x64`

La compilación tomará 1-2 minutos.

### Paso 3: Instalar Globalmente (macOS)

```bash
# Mover el binario a una ruta global
sudo mv ./bin_mac/latino /usr/local/bin/

# Dar permisos de ejecución
sudo chmod +x /usr/local/bin/latino
```

### Paso 4: Verificar la Instalación (macOS)

Abre una **nueva terminal** y ejecuta:

```bash
latino -v
```

**Salida esperada:**

```text
 _          _   _                   _   _ _____ _____ 
| |    __ _| |_(_)_ __   ___       | \ | | ____|_   _|
| |   / _` | __| | '_ \ / _ \      |  \| |  _|   | |  
| |__| (_| | |_| | | | | (_) |  _  | |\  | |___  | |  
|_____\__,_|\__|_|_| |_|\___/  (_) |_| \_|_____| |_|  

  Versión v1.0.81 (Core .NET 10)
  Copyright © 2026 Franklin A. Lara P.
```

### Solución de Problemas en macOS

#### Problema 1: "latino cannot be opened because the developer cannot be verified"

Si intentas ejecutar el binario descargado (no compilado), macOS lo bloqueará.

**Solución:**

1. Haz clic derecho sobre el archivo `latino`
2. Selecciona **"Abrir"**
3. Confirma que quieres abrirlo
4. O compila desde el código fuente (recomendado)

#### Problema 2: "dotnet: command not found"

El SDK de .NET no está instalado o no está en el PATH.

**Solución:**

```bash
# Verificar si .NET está instalado
which dotnet

# Si no está, descárgalo desde:
# https://dotnet.microsoft.com/download
```

#### Problema 3: Error de compilación "The framework 'Microsoft.NETCore.App', version '10.0.0' was not found"

La versión del SDK no es la correcta.

**Solución:**

```bash
# Verificar versión instalada
dotnet --version

# Debe ser 10.x.x
# Si es menor, descarga la versión correcta
```

## 0.9 Instalación de la Extensión para Visual Studio Code

### ¿Por Qué Necesitas la Extensión?

Técnicamente, puedes programar en Latino.NET usando cualquier editor de texto (Notepad, Vim, Nano). Pero estarías renunciando a herramientas que transforman la experiencia:

**Sin extensión (Editor de texto plano):**

* ❌ Todo el texto es del mismo color
* ❌ No sabes si hay errores hasta ejecutar
* ❌ No hay sugerencias de código
* ❌ Tienes que escribir todo manualmente

**Con extensión (Editor inteligente):**

* ✅ **Resaltado de sintaxis** (palabras clave, cadenas, comentarios con colores)
* ✅ **Diagnósticos en tiempo real** (errores subrayados mientras escribes)
* ✅ **Autocompletado (IntelliSense)** (sugiere funciones y variables)
* ✅ **Snippets** (plantillas de código con `Tab`)
* ✅ **Documentación al pasar el mouse** (tooltips explicativos)

**La extensión usa el Language Server Protocol (LSP)**, la misma tecnología que usan lenguajes profesionales como TypeScript, Python y Rust.

### Requisito Previo: Visual Studio Code

Si no lo tienes instalado:

1. Ve a: [https://code.visualstudio.com/](https://code.visualstudio.com/)
2. Descarga la versión para tu sistema operativo
3. Instálalo siguiendo el asistente

**Alternativas compatibles:**

La extensión `.vsix` funciona en cualquier editor basado en VS Code:

* **Devin** (fork de VS Code)
* **Kiro** (fork de VS Code)
* **Antigravity IDE** (fork de VS Code)
* **VSCodium** (versión open-source de VS Code)

## 0.10 Descarga de la Extensión

### Paso 1: Obtener el Archivo .vsix

Dirígete a la página de lanzamientos:

👉 **[https://github.com/FrkL81/Latino.NET/releases/latest](https://github.com/FrkL81/Latino.NET/releases/latest)**

En la sección **Assets**, descarga:

📦 `latino-net-vscode-1.0.81.vsix`

Guarda el archivo en una ubicación que recuerdes (ej: Descargas).

### Paso 2: Instalar la Extensión

#### Método A: Interfaz Gráfica (Recomendado)

1. **Abre Visual Studio Code**

2. **Ve a la vista de Extensiones**
   * Presiona `Ctrl + Shift + X` (Windows/Linux)
   * O `Cmd + Shift + X` (macOS)
   * O haz clic en el ícono de cuadrados en la barra lateral izquierda

3. **Accede al menú de instalación**
   * Haz clic en el ícono de **tres puntos** (`...`) en la parte superior derecha del panel de extensiones
   * Selecciona **"Instalar desde VSIX..."**

4. **Selecciona el archivo**
   * Navega hasta donde descargaste `latino-net-vscode-1.0.81.vsix`
   * Selecciónalo y haz clic en **Abrir**

5. **Confirmar instalación**
   * VS Code mostrará un mensaje: *"Instalando extensión..."*
   * Luego: *"La extensión se instaló correctamente"*
   * Haz clic en **"Recargar Ahora"** si te lo pide

#### Método B: Línea de Comandos

Si prefieres la terminal:

**Windows (PowerShell):**

```powershell
cd C:\Users\TuNombre\Descargas
code --install-extension latino-net-vscode-1.0.81.vsix
```

**Linux/macOS:**

```bash
cd ~/Descargas
code --install-extension latino-net-vscode-1.0.81.vsix
```

**Salida esperada:**

```text
Installing extension 'latino-net-vscode-1.0.81.vsix'...
Extension 'latino-net-vscode-1.0.81.vsix' was successfully installed.
```

### Paso 3: Verificar la Instalación

1. Abre la vista de Extensiones (`Ctrl + Shift + X`)
2. En el cuadro de búsqueda, escribe: **Latino**
3. Deberías ver la extensión instalada con el nombre **"Latino.NET"**

**Indicadores de instalación correcta:**

* ✅ Aparece en la lista de extensiones instaladas
* ✅ Tiene un botón de **engranaje** (configuración) visible
* ✅ **NO** tiene un botón "Instalar" (porque ya está instalada)

## 0.11 Configuración de la Extensión

### Escenario 1: PATH Configurado (Configuración Automática)

Si configuraste el PATH correctamente (sección 0.6 de este documento), **no necesitas hacer nada más**. La extensión encontrará automáticamente el comando `latino`.

**Prueba:**

1. Crea un archivo de prueba llamado `test.lat`
2. Escribe: `escribir("Hola")`
3. Si ves colores en la sintaxis, **¡funciona!**

### Escenario 2: PATH No Configurado (Configuración Manual)

Si decidiste **NO** configurar el PATH (o si la extensión no encuentra el ejecutable), necesitas decirle a VS Code dónde está `latino.exe`.

#### Paso a Paso

1. **Abrir Configuración de VS Code**
   * Presiona `Ctrl + ,` (Windows/Linux)
   * O `Cmd + ,` (macOS)
   * O ve a: **Archivo > Preferencias > Configuración**

2. **Buscar la Configuración de Latino**
   * En el cuadro de búsqueda de la configuración, escribe: **Latino**
   * Encontrarás una sección llamada **"Latino > Binary: Path"**

3. **Especificar la Ruta del Ejecutable**

   **En Windows:**

   ```text
   C:\LatinoNET\latino.exe
   ```

   **En Linux:**

   ```text
   /usr/local/bin/latino
   ```

   O si usaste instalación de usuario:

   ```text
   /home/tunombre/.local/bin/latino
   ```

   **En macOS:**

   ```text
   /usr/local/bin/latino
   ```

4. **Guardar la Configuración**
   * VS Code guarda automáticamente los cambios
   * Cierra y vuelve a abrir VS Code para que surta efecto

### Verificar que el Language Server Funciona

1. **Crear un archivo de prueba:**
   * En VS Code, presiona `Ctrl + N` (nuevo archivo)
   * Guárdalo como `prueba.lat`

2. **Escribir código con error intencional:**

   ```latino
   escribir("Hola Mundo"
   ```

3. **Observar el diagnóstico:**
   * Deberías ver una **línea ondulada roja** bajo la última línea
   * Al pasar el mouse, aparecerá un mensaje: *"Se esperaba ')'"*

4. **Corregir el error:**

   ```latino
   escribir("Hola Mundo")
   ```

5. **Probar autocompletado:**
   * Escribe `escri` y presiona `Ctrl + Espacio`
   * Deberías ver sugerencias como `escribir`, `imprimirf`, etc.

Si todo esto funciona, **¡el Language Server está activo!** 🎉

## 0.12 Características de la Extensión

### 1. Resaltado de Sintaxis

Cada elemento del código tiene un color específico:

```latino
# Comentarios en verde/gris
escribir("Texto en naranja")  # Función en azul, cadena en naranja
numero = 42                    # Variable en blanco, número en verde
si verdadero                   # Palabras clave en morado
    escribir("Lógico en azul")
fin
```

### 2. Diagnósticos en Tiempo Real

Errores comunes detectados **mientras escribes**:

* **Error: Paréntesis sin cerrar**

```latino
escribir("Hola"  # ❌ Línea ondulada roja
```

* **Error: Palabra clave mal escrita**

```latino
si edad > 18
    escribir("Mayor")
fiin  # ❌ Subrayado: "Se esperaba 'fin'"
```

* **Error: Falta operador**

```latino
x = 10 20  # ❌ "Se esperaba operador entre expresiones"
```

### 3. Autocompletado (IntelliSense)

La extensión sugiere código mientras escribes.

🎯 **Ejemplo 1: Funciones de Librería**

```latino
# Escribe "mat" y presiona Ctrl+Espacio
mat
# Aparecen sugerencias:
# - mate.abs
# - mate.acos
# - mate.acosh
# - mate.aleatorio ...
```

🎯 **Ejemplo 2: Palabras Clave**

```latino
# Escribe "si" y presiona Tab
si
# Se autocompletará la estructura:
si condicion
    
fin
```

🎯 **Ejemplo 3: Variables Definidas**

```latino
nombreCompleto = "Carlos López"

# Luego, al escribir "nom" y presionar Ctrl+Espacio
# Sugiere: nombreCompleto
```

### 4. Snippets Jerárquicos y Estructuras de Código

La extensión incluye más de **60 plantillas de código** organizadas mediante prefijos funcionales. Al escribir el prefijo y presionar `Tab`, se despliega la estructura completa con cursores interactivos:

| Categoría | Prefijo | Atajo Rápido | Expansión |
| :--- | :--- | :--- | :--- |
| **Condicionales** | `cond:si` | `si` + Tab | Bloque `si ... fin` |
| **Condicionales** | `cond:sino` | - | Bloque `si ... sino ... fin` |
| **Condicionales** | `cond:osi` | - | Bloque `si ... osi ... sino ... fin` |
| **Condicionales** | `cond:elegir` | - | Estructura switch-case `elegir ... fin` |
| **Bucles** | `loop:para` | `para` + Tab | Bucle rey `para elemento en coleccion` |
| **Bucles** | `loop:mientras` | `mientras` + Tab | Bucle condicional `mientras ... fin` |
| **Bucles** | `loop:desde` | `desde` + Tab | Bucle contador estilo C |
| **Bucles** | `loop:rango` | - | Iteración numérica rápida con `rango()` |
| **Funciones** | `fn:simple` | `funcion` + Tab | Declaración de función `funcion nombre() ... fin` |
| **Funciones** | `fn:main` | - | Estructura de script con función `principal()` |
| **POO** | `poo:clase` | `clase` + Tab | Definición de clase con constructor |
| **POO** | `poo:herencia` | - | Declaración de clase hija (`es`) |
| **Errores** | `err:try` | `intentar` + Tab | Bloque seguro `intentar ... atrapar (e) ... fin` |
| **Avanzados** | `syntax:pipeline` | - | Flujo funcional con tubería (`\|>`) |
| **Operadores** | `op:and` | `yy` + Tab | Operador lógico `ý` |
| **Operadores** | `op:or` | `oo` + Tab | Operador lógico `ó` |
| **Operadores** | `io:compositor` | `cc` + Tab | Plantilla de interpolación `ç("... \|var\| ...")` |

> 💡 **Nota:** La referencia completa de todos los snippets está detallada en la [Referencia de Snippets](96_Referencia_Snippets.md).

### 5. Documentación Contextual al Pasar el Mouse (Hover)

Al colocar el cursor sobre cualquier palabra clave, función nativa (`mate.raiz`, `cadena.longitud`) o función de plugins científicos (`formulas`), aparecerá una tarjeta explicativa con su sintaxis, parámetros esperados, tipo de retorno y fórmulas matemáticas formateadas con precisión.

### 6. Magic Strings (Autocompletado Dentro de Cadenas)

A partir de la versión **v1.0.81**, la extensión incorpora un catálogo inteligente de cadenas mágicas. Cuando tu cursor esté dentro de comillas (`""` o `''`), escribe el símbolo `@` para desplegar sugerencias automáticas validadas por el motor:

* `@estilo:` Sugiere convenciones de texto para `cadena.estilo()` (`camello`, `serpiente`, `kebab`, `pascal`, `titulo`).
* `@long:`, `@masa:`, `@temp:` Sugiere unidades físicas para `mate.convertir()`.
* `@hash:` Sugiere algoritmos de cifrado (`sha256`, `md5`, `sha512`).
* `@sqlEjecutar:`, `@sqlConsultar:` Inserta plantillas de sentencias SQL preparadas con parámetros seguros (`@param`).

### 7. Generador de Archivos y Proyectos (Scaffolding UTF-8)

Para evitar errores de codificación (tildes y emojis rotos) en sistemas Windows, la extensión incluye generadores nativos que guardan automáticamente los archivos con codificación **UTF-8 con BOM**:

Abre la Paleta de Comandos (`Ctrl + Shift + P` o `Cmd + Shift + P` en Mac) y escribe **Latino: Nuevo**:

* **Nuevo archivo vacío:** Crea un script `.lat` en blanco en la carpeta que elijas.
* **Nuevo módulo simple:** Esqueleto modular basado en el patrón *Factory*.
* **Nuevo módulo de clases:** Plantilla orientada a objetos con constructor y métodos.
* **Nuevo proyecto CRUD CLI:** Aplicación de consola completa conectada a una base de datos SQLite.
* **Nuevo Proyecto Profesional:** Genera la arquitectura recomendada de carpetas (`lib/`, `clases/`, `datos/`, `plugins/`, `pruebas/`, `docs/`) y un punto de entrada `inicio.lat`.

### 8. El "Easter Egg" Hispano: Atajo de Teclado `Ctrl + Alt + Ñ`

Para crear un nuevo archivo de Latino.NET a la velocidad del pensamiento sin tocar el ratón, presiona:

* 🪟 / 🐧 **`Ctrl + Alt + Ñ`** (Windows / Linux)
* 🍎 **`Cmd + Option + Ñ`** (macOS)

*(En configuraciones de teclado en inglés, este comando se mapea por hardware a `ctrl+alt+oem_3` o puedes reasignarlo desde `Ctrl + K, Ctrl + S` buscando `latino.newFile`).*

### 9. Botón de Ejecución Integrada (Play ▶)

No necesitas abrir la terminal y escribir comandos manualmente cada vez que quieras probar un script. Al abrir cualquier archivo `.lat`, aparecerá un botón verde de **Play (▶)** en la esquina superior derecha del editor. Al hacer clic, se abrirá la terminal integrada y ejecutará el archivo actual inmediatamente.

### 10. Temas Visuales Oficiales y Plegado de Código

La extensión incluye dos temas diseñados específicamente para el contraste sintáctico de Latino.NET:

* 🌙 **Latino.NET Dark:** Fondo azul abismo (`#161925`) optimizado para sesiones largas.
* ☀️ **Latino.NET Light:** Fondo blanco puro de alto contraste para lectura diurna y presentaciones.

*(Cámbialos en cualquier momento con `Ctrl + K, Ctrl + T` buscando `Latino.NET`).*

Además, dispones de **Plegado de Código (Code Folding)** en todas las estructuras del lenguaje cerradas con `fin` (`si`, `mientras`, `para`, `desde`, `funcion`, `clase`, `elegir`, `intentar`). La única excepción es `repetir...hasta`, que se cierra con su propia condición y **no lleva `fin`**.

## 0.13 Tu Primer Programa Completo

### Paso 1: Crear un Proyecto

1. **Crear una carpeta para tus scripts:**

   **Windows:**

   ```text
   C:\MisScripts\Latino
   ```

   **Linux/macOS:**

   ```text
   ~/MisScripts/Latino
   ```

2. **Abrir la carpeta en VS Code:**
   * **Archivo > Abrir Carpeta...**
   * Selecciona la carpeta que creaste
   * Haz clic en **Seleccionar carpeta**

### Paso 2: Crear el Archivo

1. Haz clic en el ícono de **Nuevo Archivo** (o presiona `Ctrl + N`)
2. Guárdalo como `hola_mundo.lat`
3. VS Code detectará automáticamente que es un archivo Latino (por la extensión `.lat`)

### Paso 3: Escribir el Código

Copia y pega el siguiente programa:

```latino
# ============================================
# MI PRIMER PROGRAMA EN LATINO.NET
# ============================================

# Constante (MAYÚSCULAS = inmutable)
NOMBRE_PROGRAMA = "Calculadora de Edad"
VERSION = "1.0"

# Función para calcular edad en días
funcion CalcularEdadEnDias(años)
    # Validación
    si años < 0
        retornar -1  # Código de error
    fin
    
    # Cálculo aproximado (365 días por año; ámbito local por defecto)
    dias = años * 365
    retornar dias
fin

# Función para mostrar banner
funcion MostrarBanner()
    escribir("==========================================")
    escribir("  " .. NOMBRE_PROGRAMA)
    escribir("  Versión: " .. VERSION)
    escribir("==========================================")
    escribir("")
fin

# ============================================
# PROGRAMA PRINCIPAL
# ============================================

# Mostrar encabezado
MostrarBanner()

# Solicitar datos al usuario con prompt integrado
nombre = pedir("Ingresa tu nombre: ")        # Entrada simulada: Carlos
edad = pedir("Ingresa tu edad en años: ")    # Entrada simulada: 28

# Convertir texto a número
edadNumerica = anumero(edad)

# Calcular días vividos
dias = CalcularEdadEnDias(edadNumerica)

# Validar resultado
si dias == -1
    escribir("❌ Error: La edad no puede ser negativa")
sino
    # Mostrar resultado usando el Compositor
    mensaje = ç("👋 Hola |nombre|, has vivido aproximadamente |dias| días!")
    escribir("")
    escribir(mensaje)
    escribir("")
    
    # Dato curioso
    si dias > 10000
        escribir("🎉 ¡Wow! Has superado los 10,000 días de vida")
    fin
fin

escribir("Gracias por usar " .. NOMBRE_PROGRAMA)
```

### Paso 4: Ejecutar el Programa

#### Opción A: Desde VS Code (Recomendado)

1. **Abrir la terminal integrada:**
   * Presiona `` Ctrl + ` `` (tecla acento grave)
   * O ve a: **Terminal > Nueva Terminal**

2. **Ejecutar el programa:**

   ```bash
   latino hola_mundo.lat
   ```

3. **Interactuar con el programa:**

   ```text
   ==========================================
     Calculadora de Edad
     Versión: 1.0
   ==========================================
   
   Ingresa tu nombre: Carlos
   Ingresa tu edad en años: 28
   
   👋 Hola Carlos, has vivido aproximadamente 10220 días!
   
   🎉 ¡Wow! Has superado los 10,000 días de vida
   Gracias por usar Calculadora de Edad
   ```

#### Opción B: Desde la Terminal del Sistema

**Windows (CMD o PowerShell):**

```cmd
cd C:\MisScripts\Latino
latino hola_mundo.lat
```

**Linux/macOS:**

```bash
cd ~/MisScripts/Latino
latino hola_mundo.lat
```

### Paso 5: Experimentar y Aprender

Ahora que tienes todo configurado, prueba estas modificaciones:

* **Experimento 1: Error Intencional**

Elimina el paréntesis de cierre en la línea 15:

```latino
funcion CalcularEdadEnDias(años
```

Observa cómo VS Code subraya el error **antes de ejecutar**.

* **Experimento 2: Usar Autocompletado**

En una nueva línea, escribe:

```latino
mat
```

Presiona `Ctrl + Espacio` y explora las funciones disponibles en la librería `mate`.

* **Experimento 3: Crear una Clase**

Escribe `clase` y presiona `Tab`. Observa cómo se expande la plantilla completa.

## 0.14 Modo Interactivo (REPL)

Latino.NET incluye un **REPL** (Read-Eval-Print Loop) para experimentar sin crear archivos.

### Cómo Usarlo

Abre una terminal y ejecuta:

```bash
latino
```

**Sin argumentos**, se abrirá el modo interactivo oficial:

```text
 _          _   _                   _   _ _____ _____ 
| |    __ _| |_(_)_ __   ___       | \ | | ____|_   _|
| |   / _` | __| | '_ \ / _ \      |  \| |  _|   | |  
| |__| (_| | |_| | | | | (_) |  _  | |\  | |___  | |  
|_____\__,_|\__|_|_| |_|\___/  (_) |_| \_|_____| |_|  

  Versión v1.0.81 - REPL Interactivo
  Escribe 'salir' o presiona Ctrl+C para terminar.

latino> 
```

**Prueba comandos en vivo:**

```latino
latino> x = 10
latino> y = 20
latino> escribir(x + y)
30
latino> escribir("Hola desde REPL")
Hola desde REPL
latino> escribir(mate.raiz(16))
4
latino> salir
👋 ¡Hasta pronto!
```

> ⚠️ **Importante:** El REPL **no imprime automáticamente** el resultado de expresiones sueltas (a diferencia de Python). Para ver el valor de una expresión, envuélvela en `escribir(...)`.
>
> * **Salir:** Escribe `salir` (sin paréntesis) o presiona `Ctrl+C`.
> * Al salir verás: `👋 ¡Hasta pronto!`

**Ventajas del REPL:**

* ✅ Probar expresiones rápidamente
* ✅ Verificar sintaxis
* ✅ Experimentar con funciones de librerías
* ✅ Calculadora científica instantánea

## 0.15 Recursos Adicionales

### Carpeta de Ejemplos

El repositorio incluye una carpeta `/ejemplos` con scripts funcionales. **Estos ejemplos se incluyen en el paquete de distribución** (no requieren descarga adicional).

```text
ejemplos\
├── Batalla_Polar\
│   ├── core\
│   │   ├── juego.lat
│   │   ├── jugador.lat
│   │   ├── tablero.lat
│   │   └── unidad.lat
│   ├── modulos\
│   │   ├── ia.lat
│   │   └── red_dummy.lat
│   ├── utils\
│   │   ├── input.lat
│   │   └── matematicas.lat
│   ├── main.lat
│   └── MANUAL_Batalla_Polar.md
├── Crea_abre_elimina\
│   ├── a_hola_archivo.lat
│   └── b_limpiar_saludo.lat
├── Ecosistema\
│   ├── Ej_formulas\
│   │   ├── 01_AlgebraLineal.lat
│   │   ├── 02_Geometria.lat
│   │   ├── 03_Estadistica.lat
│   │   ├── 04_Optimizacion.lat
│   │   ├── 05_Edo.lat
│   │   ├── 06_Electricidad_Magnetismo_Optica.lat
│   │   ├── 07_Fisica_Basica.lat
│   │   ├── 08_Astronomia_Termodinamica.lat
│   │   ├── 09_Ingenieria_Civil.lat
│   │   └── 10_Quimica_Finanzas.lat
│   ├── Ej_archivo.lat
│   ├── Ej_cadena.lat
│   ├── Ej_cola.lat
│   ├── Ej_conjunto.lat
│   ├── Ej_cripto.lat
│   ├── Ej_dic.lat
│   ├── Ej_env.lat
│   ├── Ej_globales.lat
│   ├── Ej_hilos.lat
│   ├── Ej_lapsos.lat
│   ├── Ej_lista.lat
│   ├── Ej_log.lat
│   ├── Ej_mate.lat
│   ├── Ej_pila.lat
│   ├── Ej_proceso.lat
│   ├── Ej_sis.lat
│   ├── Ej_sqlite.lat
│   ├── Ej_tabla.lat
│   ├── Ej_texto.lat
│   ├── Ej_tupla.lat
│   ├── Ej_validar.lat
│   ├── Ej_web.lat
│   └── Ej_zip.lat
├── Modulos_De_Usuario\
│   ├── geometria.lat
│   └── main.lat
├── Utilidad_Administra_Contactos\
│   ├── AdministradorContactosV2.1.lat
│   ├── reporte_contactos.csv
│   └── reporte_contactos.md
├── Utilidad_Organiza_Archivos\
│   ├── organizador_v1\
│   │   └── organizador.lat
│   ├── organizador_v2\
│   │   └── organizador_v2.lat
│   └── README.md
├── 01_hola_mundo.lat
├── 02_aritmetica_basica.lat
├── 03_comentarios_se_ignoran.lat
├── 04_variables_y_nombres.lat
├── 05_operadores_de_Latino.NET.lat
├── 06_tipos_y_estructuras.lat
├── 07_toma_decisiones.lat
├── 08_nuestros_bucles.lat
├── 09_funciones_y_PF.lat
├── 10_algo_de_POO.lat
├── 11_sin_comillas_extra.lat
├── 12_atrapando_errores.lat
├── 13_el_alcance.lat
├── 14_patrones_avanzados.lat
├── 15_regex_y_patrones.lat
├── 16_json_y_configuracion.lat
├── 17_cliente_http_rest.lat
├── 18_errores_y_validaciones.lat
├── 19_closures_y_funcional.lat
├── 20_script_cli_argumentos.lat
├── 21_datos_csv_a_sqlite.lat
├── 22_respaldo_automatizado.lat
├── 23_mini_framework_pruebas.lat
├── 24_texto_estructurado.lat
├── ASCII_Graphic.lat
├── chat_Groq.lat
└── chat_OpenRouter.lat
```

**Nota:** La carpeta `ejemplos/` se incluye en el paquete de distribución de Latino.NET. No requiere descarga adicional.

### Documentación del Manual

Este es el **Capítulo 0** de un compendio de **19 capítulos** (numerados del 0 al 18):

| Capítulo | Tema |
| :--- | :--- |
| **0** | Introducción y Configuración (Este capítulo) |
| **1** | Fundamentos (Variables, Tipos, Operadores) |
| **2** | Control de Flujo (si, mientras, desde, para) |
| **3** | Estructuras de Datos (Listas, Diccionarios, Conjuntos) |
| **4** | Funciones (Declaración, Parámetros, Clausuras) |
| **5** | POO (Clases, Herencia, Encapsulamiento) |
| **6** | Programación Funcional (Lambdas, Pipeline, Orden Superior) |
| **7** | Herramientas del Lenguaje (Archivos, Red, Sistema) |
| **8** | Modularidad y Extensibilidad (incluir, Plugins) |
| **9** | Referencia API (Librerías estándar completas) |
| **10** | Metaprogramación y Código Dinámico |
| **11** | Programación Concurrente (Hilos, Canales) |
| **12** | DOP y Persistencia (Orientación a Datos, SQLite) |
| **13** | Guía de Plugins (Crear extensiones en C#) |
| **14** | Arquitectura Interna (Cómo funciona el motor) |
| **15** | Mantenimiento de la Extensión (VS Code) |
| **16** | Guía de Comunidad y Contribución |
| **17** | Casos de Estudio: Ingeniería Industrial |
| **18** | Un Intérprete Rápido (Diseño Core) |
| **19** | Curiosidades y Mecánicas Ocultas del Motor |
| … | *(Además, existen referencias complementarias en el repositorio oficial: `96_Referencia_Snippets.md`, `97_Referencia_Keywords.md` y `98_Referencia_Strings_Predefinidos.md`)* |

### Dónde Obtener Ayuda

**GitHub Issues:**
👉 [https://github.com/FrkL81/Latino.NET/issues](https://github.com/FrkL81/Latino.NET/issues)

* Reportar bugs
* Solicitar características
* Compartir feedback

**GitHub Discussions:**
👉 [https://github.com/FrkL81/Latino.NET/discussions](https://github.com/FrkL81/Latino.NET/discussions)

* Hacer preguntas
* Compartir scripts
* Ayudar a otros usuarios
