# 🚀 Soporte de Latino.NET para Sublime Text

Este directorio contiene los archivos necesarios para habilitar el resaltado de sintaxis, snippets inteligentes y autocompletado de strings predefinidos de Latino.NET en **Sublime Text (3 y 4)**.

## 📂 Contenido del Soporte

- `sublime/Latino.sublime-syntax`: gramática avanzada compatible con las funciones de Latino.NET v1.0.81.
- `sublime/snippets/`: colección de **1582 snippets**, repartidos en 1020 de Magic Strings (prefijo `str:`) y 562 de funciones y estructuras del lenguaje.
- `test_syntax.lat`: script de prueba para verificar que la gramática reconoce los tokens del lenguaje.

> ⚠️ **Ojo con la estructura:** la gramática y los snippets viven dentro de la subcarpeta `sublime/`, no en la raíz de este directorio. Al instalar hay que copiar el **contenido de `sublime/`**, como se indica más abajo.

---

## 🔧 Instrucciones de Instalación

### 🪟 Windows

1. Abre Sublime Text.
2. Presiona `Ctrl + Shift + P` para abrir la paleta de comandos.
3. Escribe `Browse Packages` y presiona **Enter**. Se abrirá una carpeta de Windows.
4. Crea una nueva carpeta llamada `LatinoNetSyntax` dentro de esa ubicación.
5. Abre la carpeta `sublime/` de este paquete y copia **su contenido** dentro de `LatinoNetSyntax/`:
   - Copia el archivo `Latino.sublime-syntax` a `LatinoNetSyntax/`.
   - Copia la carpeta `snippets/` completa dentro de `LatinoNetSyntax/`.

El resultado esperado es que `LatinoNetSyntax/` contenga la gramática y una carpeta `snippets/`.

### 🐧 Linux

```bash
destino=~/.config/sublime-text/Packages/User/LatinoNetSyntax
mkdir -p "$destino"
cp sublime/Latino.sublime-syntax "$destino/"
cp -r sublime/snippets "$destino/"
```

### 🍎 macOS

```bash
destino=~/Library/Application\ Support/Sublime\ Text/Packages/User/LatinoNetSyntax
mkdir -p "$destino"
cp sublime/Latino.sublime-syntax "$destino/"
cp -r sublime/snippets "$destino/"
```

---

## 💡 Cómo usar

Una vez instalado:

1. Abre cualquier archivo con extensión `.lat`.
2. Sublime Text debería detectar automáticamente el lenguaje como **Latino.NET**.
3. **Snippets de funciones y estructuras:**
   - Escribe `si:` y presiona `Tab` para una estructura condicional.
   - Escribe `mate.` y verás todas las funciones matemáticas (por ejemplo, `mate.abs`).
   - Escribe `cadena.` para las funciones de texto.
   - Escribe `#!` al inicio de un script para añadir la cabecera.
4. **Autocompletado de Magic Strings:** 🆕
    - Escribe `str:` para ver todas las categorías de strings predefinidos.
    - **Ejemplos:**

      | Trigger | Inserta | Significado |
      | :--- | :--- | :--- |
      | `str:estilo:camello` | `camello` | Estilo camelCase |
      | `str:long:m` | `m` | Metro |
      | `str:temp:celsius` | `celsius` | Temperatura |
      | `str:comp:exacta` | `exacta` | Comparación exacta |

    - **Cómo se activa el autocompletado:** los triggers van de `str:` a `str:prefijo:valor`. Sublime Text **muestra la lista de candidatos mientras escribes y la va filtrando** a medida que añades caracteres, hasta que queda solo el coincidente. Escribir `str:estilo:` muestra la lista completa de estilos, escribir `str:estilo:cam` deja los que empiezan por `cam`, y escribir el trigger entero deja un único candidato. Presiona `Tab` para insertar el que quieras.
    - **Nota importante:** los snippets insertan **el valor limpio, sin comillas y sin el prefijo**, porque son literales de datos y el motor de Latino.NET nunca procesa un prefijo con `@`. `str:temp:celsius` inserta el texto `celsius`, no `temp:celsius`. Las comillas las escribes tú, o usa el snippet ya dentro de ellas:

      ```latino
      t = mate.convertir(25, "celsius", "fahrenheit")
      ```

---

## 🛠️ Mantenimiento

La gramática y los snippets son **artefactos generados**: se corrigen en su origen y se regeneran, nunca se editan a mano. El procedimiento de regeneración y el protocolo de sincronización con el Language Server están documentados en el capítulo 14 del manual de arquitectura interna (`docs/14_Guia_Arquitectura_Interna.md`).
