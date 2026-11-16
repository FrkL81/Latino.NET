# ❄️ Batalla Polar — Manual del Jugador

## Versión 2.0 · Latino.NET v1.0.81

---

## 📋 Descripción

Batalla Polar es un juego de estrategia por turnos para **dos jugadores** (o un jugador contra una IA) en una misma terminal. Cada comandante controla un ejército de soldados desplegados en un plano cartesiano. El objetivo es destruir todas las tropas enemigas usando coordenadas polares antes de que el oponente destruya las tuyas.

---

## ⚙️ Requisitos e Instalación

### Requisitos

- **Latino.NET v1.0.81** o superior instalado
- **Terminal con soporte UTF-8** (recomendado: terminal integrada de VS Code o Windows Terminal)
- **Conexión a internet** solo si usas el modo IA (para consultar la API de un proveedor LLM)

### Estructura del proyecto

```text
_BatallaPolar/
├── main.lat              ← Punto de entrada
├── core/
│   ├── juego.lat         ← Controlador de la partida
│   ├── jugador.lat       ← Clase Comandante
│   ├── tablero.lat       ← Motor de renderizado
│   └── unidad.lat        ← Clase Soldado
├── utils/
│   ├── input.lat         ← Lector de teclado seguro
│   └── matematicas.lat   ← Conversión polar → cartesiano
└── modulos/
    ├── ia.lat            ← Motor de IA conectado a LLM
    └── red_dummy.lat     ← (Reservado para multijugador futuro)
```

### Cómo ejecutar

Desde la raíz del proyecto:

```bash
latino main.lat
```

O usando el botón ▶️ de la extensión Latino.NET en VS Code con `main.lat` abierto.

---

## 🎮 Configuración de la Partida

Al iniciar, el juego solicita los siguientes datos en orden:

| Pregunta | Descripción |
| -------- | ----------- |
| **Nombre del Comandante 1** | Vacío = "Comandante Azul" |
| **¿Jugador 2 es IA?** | `s` para activar modo Humano vs IA, `n` para PvP local |
| **Nombre del Comandante 2** | Solo en modo PvP. Vacío = "Comandante Rojo" |
| **Proveedor de IA** | Solo en modo IA. Groq u OpenRouter |
| **Modelo de IA** | Solo en modo IA. Ver tabla de modelos más abajo |
| **Nivel de fallo de sensores** | Solo en modo IA. Desventaja artificial (0-3) |
| **Soldados por bando** | Entre 2 y 9 |
| **¿Mostrar puntos de cuadrícula?** | `s` para ver ` · ` en celdas vacías, `n` para tablero limpio |

---

## 🤖 Modo Humano vs IA (LLM)

### Qué es

En este modo el Jugador 2 es controlado por un modelo de lenguaje (LLM) que lee el estado del tablero, decide a qué tropa enemiga apuntar y genera comentarios de rol en cada disparo.

### Proveedores soportados

El juego soporta dos proveedores de IA, ambos con **free tier** (gratuito con rate limits):

| Proveedor | Características | URL para API Key |
| --------- | --------------- | ---------------- |
| **Groq** | Ultra-rápido, modelos de alta calidad | [console.groq.com](https://console.groq.com) |
| **OpenRouter** | Múltiples modelos, router automático | [openrouter.ai](https://openrouter.ai) |

### API Key

Para usar el modo IA necesitas una clave de API. El juego te la pedirá automáticamente si no la tienes configurada.

**Cómo obtenerla:**

1. Crea una cuenta gratuita en el proveedor elegido
2. Genera una API Key
2. Pégala cuando el juego te la solicite

El juego crea automáticamente un archivo `.env` en la raíz del proyecto para guardar tu clave de forma segura.

> ⚠️ **No compartas el archivo `.env` ni lo subas a repositorios públicos (GitHub, etc.).** Trátalo como una contraseña. Si usas Git, agrégalo a tu `.gitignore`.

### Modelos disponibles

**Groq (recomendado):**

| Opción | Modelo | Característica |
| ------ | ------ | -------------- |
| 1 | `qwen/qwen3.6-27b` | Razonamiento, excelente en español |
| 2 | `qwen/qwen3.8-27b` | Más reciente, soporta imagen |
| 3 | `openai/gpt-oss-120b` | Gran capacidad |
| 4 | `openai/gpt-oss-20b` | Razonamiento eficiente |

**OpenRouter (alternativa):**

| Opción | Modelo | Característica |
| ------ | ------ | -------------- |
| 1 | `openrouter/free` | Router automático (recomendado) |
| 2 | `nvidia/nemotron-3-nano-30b-a3b:free` | Modelo NVIDIA |
| 3 | `meta-llama/llama-3.2-3b-instruct:free` | Modelo Meta |

> **Nota:** Todos estos modelos están disponibles en el **free tier** de sus respectivos proveedores (con rate limits). No necesitas configurar un método de pago para usarlos. Verifica la disponibilidad actualizada en: https://console.groq.com/docs/models o https://openrouter.ai/models

> **⚠️ Advertencia sobre OpenRouter:** Algunos modelos gratuitos de OpenRouter tienen filtros de seguridad más estrictos. Si recibes mensajes como "User Safety: unsafe" o "Violence", es un **falso positivo** del filtro de seguridad del modelo. Esto ocurre porque el prompt del juego usa vocabulario militar ("artillería", "destruir", "despiadado") que algunos modelos interpretan erróneamente como violencia real. **Solución:** Usa Groq para una mejor experiencia, o prueba con otros modelos de OpenRouter.

> **Nota sobre modelos razonadores:** Qwen3 y algunos otros modelos usan cadena de pensamiento interna (`<think>...</think>`). El juego la filtra automáticamente antes de mostrar la respuesta.

### Niveles de fallo de sensores

| Nivel | Descripción | Efecto |
| ----- | ----------- | ------ |
| 0 | Sin fallos | La IA dispara con precisión perfecta |
| 1 | Bajo | Desvío de distancia aleatorio: 0 o +1 |
| 2 | Medio | Desvío de distancia: -1, 0 o +1 |
| 3 | Alto | Desvío de distancia: -1, 0 o +1 **y** ángulo: ±10° |

### Intercomunicador de radio

Durante tu turno (cuando juegas contra la IA), puedes enviar un mensaje de radio al Comandante enemigo. La IA lo leerá y responderá de forma dramática en su siguiente transmisión. Es solo roleplay — no afecta la mecánica del juego.

---

## 🗺️ El Tablero

Campo de batalla de **21×21 celdas** (radio 10). El centro es el origen `(0, 0)`.

```text
       [B] BANDO AZUL         BANDO ROJO [R]
      ---------------------------------------
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
       ─  ─  ─  ─  ─  ─  ┼  ─  ─  ─  ─  ─  ─
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
       ·  ·  ·  ·  ·  ·  │  ·  ·  ·  ·  ·  ·
      ---------------------------------------
```

### Leyenda de símbolos

| Símbolo | Significado |
| ------- | ----------- |
| `[B]` | Soldado Azul — vivo |
| `[R]` | Soldado Rojo — vivo |
| `{X}` | Soldado destruido |
| `(1)`…`(9)` | Impacto en agua — número de veces disparado en esa celda |
| `(+)` | Zona con 10+ impactos |
| ` · ` | Celda vacía (modo con puntos) |
| `   ` | Celda vacía (modo sin puntos) |
| ` ┼ ` | Origen (0, 0) |
| ` │ ` | Eje vertical |
| ` ─ ` | Eje horizontal |

### Distribución de tropas

- **Jugador 1** despliega en la mitad **izquierda** (x: -10 a -1)
- **Jugador 2** despliega en la mitad **derecha** (x: 1 a 10)
- Las posiciones son **aleatorias** en cada partida
- El **fuego amigo** es posible — tus propias tropas pueden ser destruidas

---

## 🎲 Sistema de Turnos

### HUD de estado

Cada turno muestra una línea de estado compacta:

```text
🎖️  Turno 4 | Comandante Azul 🟦 | Vidas: [B] 6 vs [R] 4 | Disparos: 3
─────────────────────────────────────────────────────────────────
```

### Iniciativa (inicio de partida)

Ambos jugadores lanzan un dado (1–6). El **menor** va primero y ese valor determina los disparos del primer turno. En empate se relanza automáticamente.

### Tirada de turno

Al inicio de cada turno, el jugador activo lanza un dado que determina cuántos disparos tiene ese turno (1–6).

- **Humano:** presiona `[Enter]` para confirmar y comenzar a disparar
- **IA:** pausa automática de 2.5 segundos

### Flujo de un turno completo

```text
1. Tablero + HUD
2. [Solo PvE] Mensaje de radio opcional al enemigo IA
3. Por cada disparo:
   a. Ingresar ángulo (0–360°) y distancia (0–14)
   b. El motor calcula coordenadas cartesianas
   c. Se resuelve colisión
   d. Tablero actualizado + HUD
   e. [Humano] Enter para continuar | [IA] pausa 3 segundos
4. Cambio de turno → tirada de disparos del siguiente jugador
```

---

## 🧭 Sistema de Disparo

### Parámetros

| Parámetro | Rango | Descripción |
| --------- | ----- | ----------- |
| **Ángulo** | 0° – 360° | Dirección del disparo |
| **Distancia** | 0 – 14 | Alcance. La diagonal del tablero mide ≈14.1 — ninguna celda queda fuera de alcance |

### Referencia rápida de ángulos

```text
            90°  (arriba)
             ↑
   135° ←  ──┼──  → 45°
             │
   180° ←  ──┼──  → 0° / 360°  (derecha)
             │
   225° ←  ──┼──  → 315°
             ↓
            270°  (abajo)
```

### Ejemplos

| Ángulo | Distancia | Resultado | Zona |
| ------ | --------- | --------- | ---- |
| 0° | 8 | (8, 0) | Eje derecho |
| 90° | 5 | (0, 5) | Eje arriba |
| 180° | 9 | (-9, 0) | Eje izquierdo |
| 45° | 7 | (5, 5) | Diagonal |
| 33° | 11 | (9, 6) | Primer cuadrante |
| 225° | 10 | (-7, -7) | Tercer cuadrante |

### Resultados posibles

| Resultado | Descripción |
| --------- | ----------- |
| `💥 ¡BLANCO CONFIRMADO!` | Tropa enemiga destruida — aparece `{X}` en el tablero |
| `☠️  ¡FUEGO AMIGO!` | Tropa propia destruida — ¡cuidado con los ángulos! |
| `💨 Agua` | Sin impacto — aparece `(n)` en el tablero |

---

## 🏆 Condición de Victoria

La partida termina cuando **todas las tropas de un bando son destruidas**. El reporte final muestra ganador, duración, precisión de cada jugador y el MVP. En modo IA, el Comandante enemigo transmite un mensaje de cierre dramático.

---

## 💡 Consejos Tácticos

- **Lee los `(n)`** — indican dónde ya disparaste. Evita repetir la misma zona.
- **Fuego amigo** — tus tropas están en x negativa (Azul) o positiva (Rojo). Un disparo a 0° o 180° con distancia alta puede alcanzarlas.
- **Dado alto = más disparos** — si sacas 5 o 6, aprovecha para barrer una zona sistemáticamente.
- **Distancia máxima 14** — con el ángulo correcto alcanzas cualquier esquina del tablero.
- **Modo sin puntos** — la opción `n` en cuadrícula hace los `{X}` y `(n)` más visibles sobre el fondo vacío.
- **Contra la IA** — los niveles 2 y 3 de fallo hacen que la IA pierda entre ±1 unidad de distancia y hasta ±10° de ángulo. Aun así, conoce exactamente dónde están tus tropas.

---

## 🚀 Extendiendo el Proyecto

Este es un proyecto de **aprendizaje**. Te invitamos a extenderlo:

### Implementar tu propio proveedor de IA

El motor de IA (`modulos/ia.lat`) está diseñado para ser extensible. Puedes agregar proveedores como:

- **OpenAI** (GPT-4, GPT-3.5)
- **Anthropic** (Claude)
- **Google** (Gemini)
- **Mistral**
- **Ollama** (modelos locales)

Solo necesitas:
1. Agregar el proveedor en el menú de selección
2. Configurar URL, headers y timeout en `llamar_api`
3. Añadir la lista de modeles disponibles

### Otras ideas de extensión

- **Multijugador en red** — Usa el plugin `hilos` y `web` para partidas online
- **Más tipos de unidades** — Tanques, aviones, submarinos con diferentes alcances
- **Mapas personalizados** — Terreno con obstáculos, ríos, montañas
- **Modo campaña** — Misiones progresivas con dificultad creciente
- **Estadísticas persistentes** — Guardar récords en SQLite

---

## 📝 Notas Finales

- *Desarrollado en Latino.NET v1.0.81 · Proyecto de ejemplo multiarchivo con integración LLM*
- *Los modelos y precios de los proveedores pueden cambiar. Verifica siempre la disponibilidad actualizada en los sitios oficiales.*
