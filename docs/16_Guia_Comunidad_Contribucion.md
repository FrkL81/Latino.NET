# CAPÍTULO 16: Guía de Comunidad y Contribución

**Versión:** 1.0.81 (.NET 10)  
**Audiencia:** Desarrolladores, estudiantes, investigadores, docentes y colaboradores que deseen participar activamente en el desarrollo del motor, la biblioteca estándar, la extensión de VS Code o la documentación de Latino.NET.  
**Propósito:** Definir los valores comunitarios, el código de conducta, el protocolo formal de reporte de incidentes y pull requests, los requisitos de calidad para plugins oficiales y la ruta meritocrática para convertirse en mantenedor del proyecto.

## La Cultura del Código Abierto en Español

Latino.NET es mucho más que un repositorio en GitHub; es un **proyecto cultural y tecnológico colectivo** que busca demostrar que la ingeniería de software de vanguardia puede pensarse, documentarse y liderarse enteramente en español.

Los grandes proyectos de código abierto no se sostienen únicamente sobre líneas de código bien escritas; se fundamentan en una comunidad saludable, empática y técnicamente rigurosa. Creemos firmemente que la excelencia en la ingeniería no está reñida con la calidez humana ni con la paciencia pedagógica.

En nuestra comunidad conviven perfiles muy diversos: el estudiante de secundaria que escribe su primer bucle `mientras`, el docente universitario que rediseña su cátedra de algoritmia y el arquitecto de sistemas que optimiza árboles sintácticos en .NET 10. Todos ellos tienen un lugar legítimo en este ecosistema.

### Nuestros Tres Compromisos Fundamentales

1. **Educación Accesible con Cimientos Industriales:** No rebajamos los estándares técnicos para hacer el lenguaje accesible; elevamos la calidad de la documentación y la mentoría para que cualquier persona talentosa pueda alcanzar el nivel profesional.
2. **Meritocracia Basada en Evidencia:** En nuestra comunidad no importan los títulos formales ni las jerarquías académicas; el respeto técnico se gana a través de la calidad del código, la claridad en las revisiones (*code reviews*) y el compromiso constante con la estabilidad del sistema.
3. **Mentoría Constructiva:** Cuando revisamos una propuesta o resolvemos una duda en los foros, explicamos siempre el *por qué* de las decisiones de diseño. Corregir un error es útil; enseñar a razonar el problema transforma a un principiante en un futuro mantenedor.

A continuación se establecen las normas de convivencia, los estándares de contribución y los protocolos que garantizan la salud a largo plazo de Latino.NET.

> 💡 **Principio de Crecimiento Compartido (Mantra de Latino.NET):**  
> *\"El software libre no avanza cuando una sola persona escribe mil líneas excelentes, sino cuando mil personas encuentran un entorno seguro donde aprender a colaborar.\"*

## 📋 Código de Conducta

### Comportamientos Esperados

✅ **Mentoría Técnica de Calidad**

- Explica el *por qué* detrás del código, no solo des la solución
- Aprovecha las capacidades modernas del lenguaje (POO, manejo de errores, closures)
- Ejemplo correcto: *"El error ocurre por una desreferencia nula. Usa `intentar/atrapar` para gestionarlo de forma segura. Así se hace..."*
- Ejemplo incorrecto: *"Pon esto y arreglado, no preguntes más."*

✅ **Respeto por la Ingeniería del Software**

- Valora el esfuerzo de optimización y robustez del núcleo
- Las críticas sobre rendimiento son bienvenidas si están técnicamente argumentadas
- Reconoce que detrás de la sintaxis simple hay un motor complejo en .NET

✅ **Debate Constructivo**

- Céntrate en el código y la arquitectura, no en la persona
- Acepta múltiples paradigmas (imperativo, POO, funcional)
- Disentir sobre algoritmos es saludable y necesario

✅ **Facilitación del Aprendizaje**

- Comparte cómo funciona algo "bajo el capó" en .NET (es un aporte valioso)
- Latino.NET es un puente hacia la programación profesional, no el destino final

### Comportamientos Inaceptables

❌ **Menosprecio Técnico**

- Tratar preguntas de principiantes como "pérdida de tiempo"
- Burlarse de la sintaxis en español como "inferior"

❌ **Acoso o Discriminación**

- Ataques personales, sexismo, racismo o exclusión por origen, género o capacidad técnica

❌ **Desinformación Deliberada**

- Propagar código malicioso, inseguro o que sabotee el aprendizaje
- Ejemplos que causen memory leaks intencionalmente

❌ **Sentido de Derecho (Entitlement)**

- Exigir soporte inmediato de forma grosera
- Demandar cambios en el núcleo sin argumentación técnica

### Reporte de Incidentes

Si presencias una violación al código de conducta:

1. Abre un **Issue** en el repositorio principal
2. Título: `[Reporte de Incidente] - Usuario Afectado`
3. Describe el incidente con evidencia (capturas, enlaces)

**Acción:** Los mantenedores revisarán el caso y podrán bloquear usuarios que afecten la salud de la comunidad.

## 📝 Guía de Contribución

### Tipos de Contribución

#### 1. Reportar Problemas (Issues)

**Antes de reportar:**

- [ ] Busca en issues existentes para evitar duplicados
- [ ] Verifica que usas la última versión de Latino.NET

**Plantilla de Issue - Reporte de Bug:**

```markdown
    ## Descripción del Problema
    [Descripción clara y concisa del error]

    ## Pasos para Reproducir
    1. Ejecutar el siguiente código:

          ```latino
          # Tu código aquí
          ```

    2. El error ocurre en la línea X

    ## Comportamiento Esperado

    ```text
    [Qué debería ocurrir]
    ```

    ## Comportamiento Actual

    ```text
    [Qué ocurre realmente]
    ```

    ## Entorno

    - **Versión de Latino.NET:** [ej: 1.0.81]
    - **Sistema Operativo:** [Windows 11 / Ubuntu 22.04 / macOS 14]
    - **Versión de .NET:** [dotnet --version]

    ## Logs o Mensajes de Error

    ```text
    [Pega aquí el mensaje de error completo]
    ```
```

**Plantilla de Issue - Solicitud de Función:**

```markdown
  ## Problema a Resolver

  [¿Qué necesidad o problema resuelve esta función?]

  ## Solución Propuesta

  [Describe cómo funcionaría la nueva función]

  🎯 **Ejemplo de Uso**

    ```latino
    # Código de ejemplo mostrando cómo se usaría
    ```

  ## Alternativas Consideradas

  [¿Existen otras formas de resolver esto actualmente?]

  ## Impacto

  - [ ] Esta función rompería compatibilidad con código existente
  - [ ] Requiere cambios en el LSP
  - [ ] Requiere documentación nueva
```

#### 2. Contribuir con Código (Pull Requests)

**Requisitos Mínimos:**

- [ ] Tu código compila sin errores ni warnings
- [ ] Sigue el estilo del proyecto (ver `.editorconfig` si existe)
- [ ] Incluye tests si añades funcionalidad nueva
- [ ] Actualiza la documentación relevante

**Proceso de PR:**

1. **Fork** el repositorio
2. Crea una rama descriptiva: `feature/mate-logaritmo-base-n` o `fix/lexer-unicode-bug`
3. Realiza commits atómicos con mensajes claros:

   ```text
   feat(stdlib): Añadir mate.log_base(num, base)
   
   - Implementa logaritmo en base arbitraria
   - Valida que base > 0 y base ≠ 1
   - Añade tests unitarios
   
   Assisted-by: Claude 3.7 (generación de tests)
   ```

4. Abre el Pull Request con descripción detallada
5. Responde feedback de los mantenedores

**Plantilla de Pull Request:**

```markdown
   ## Descripción
   [¿Qué cambia este PR y por qué?]

   ## Tipo de Cambio
   - [ ] Corrección de bug (no rompe compatibilidad)
   - [ ] Nueva función (no rompe compatibilidad)
   - [ ] Cambio que rompe compatibilidad (breaking change)
   - [ ] Mejora de documentación

   ## Checklist
   - [ ] Mi código compila sin warnings
   - [ ] Añadí/actualicé tests
   - [ ] Añadí/actualicé documentación
   - [ ] Probé manualmente en Windows/Linux (marca las que apliquen)
   - [ ] Si usé IA para generar código, lo declaré en el commit

   ## Testing
   [Describe cómo probaste los cambios]

   ## Capturas (si aplica)
   [Screenshots del comportamiento nuevo/arreglado]
```

#### 3. Contribuir con Plugins

**Requisito Crítico:** Todo plugin aceptado en el ecosistema oficial **DEBE** incluir documentación completa.

**Principio — sin dependencias de terceros por defecto:**

Un plugin se escribe en **C# puro**, usando solo `System` y lo que ya trae `Latino.Core`: sin `PackageReference`, sin NuGet, sin DLLs ajenas. Es lo que hacen los plugins del núcleo y mantiene la instalación en «copiar una carpeta» y comparar una versión.

**La regla no es absoluta.** Una dependencia se admite cuando cumple alguna de estas condiciones, y solo entonces:

1. **Conector o interoperabilidad con una plataforma externa** —bases de datos, servicios o formatos que solo existen en un motor ajeno.
2. **El caso requiere una capacidad que el BCL no tiene** y que no conviene reimplementar.
3. **La dependencia es estándar, estable y con licencia permisiva** (MIT/Apache/BSD).

**El precedente:** el plugin `sqlite` del núcleo no existiría sin `Microsoft.Data.Sqlite`. La regla existe para guiar la decisión, no para excluir la realidad.

Cuando añadas una dependencia, **justifica en el PR por qué es inevitable**. Una dependencia obliga a empacar y versionar sus assemblies transitivos, y eso convierte «copiar una carpeta» en una operación de mantenimiento.

**Estructura Mínima de un Plugin:**

```text
Latino.Plugin.MiPlugin/
├── src/
│   ├── MiPlugin.cs              # Código fuente
│   └── Latino.Plugin.MiPlugin.csproj  # ⚠️ Ver <AssemblyName>
├── docs/
│   ├── MANUAL.md                # ⚠️ OBLIGATORIO
│   └── ejemplos/
│       ├── ejemplo_basico.lat
│       └── ejemplo_avanzado.lat
├── tests/
│   └── MiPluginTests.cs         # Tests unitarios
├── README.md                     # Descripción del plugin
└── LICENSE                       # Licencia (preferiblemente MIT)
```

**⚠️ CRÍTICO — Contenido del `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <!-- ⚠️ CRÍTICO: Forzar nombre del assembly para que incluir("mi_plugin") funcione -->
    <AssemblyName>mi_plugin</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="Latino.Core">
      <HintPath>path\to\Latino.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

⚠️ **Sobre las dependencias:** el ejemplo anterior no tiene ningún `PackageReference`, y ese es el caso
normal. Si tu plugin necesita uno, el principio de esta sección explica las tres condiciones bajo las que
se admite, y el plugin `sqlite` del núcleo es el precedente. Justifica en el PR por qué es inevitable.

**Contenido Obligatorio del `MANUAL.md`:**

````markdown
# Manual de Uso: [Nombre del Plugin]

## Instalación
[Cómo instalar el plugin: copiar .dll, configurar rutas, etc.]

## Funciones Disponibles

### nombre_modulo.funcion1(param1, param2)
**Descripción:** [Qué hace la función]

**Parámetros:**
- `param1` (tipo): Descripción del parámetro
- `param2` (tipo): Descripción del parámetro

**Retorna:** [Tipo y descripción del valor retornado]

**Ejemplo:**
```latino
incluir("nombre_modulo")

resultado = nombre_modulo.funcion1(10, "texto")
escribir(resultado)  # Salida esperada: ...
```

**Errores Comunes:**

- `RuntimeError: ...` → Solución: ...

### [Repetir para cada función]

## Casos de Uso Completos

[Ejemplos reales de uso del plugin]

## Limitaciones Conocidas

[Qué NO puede hacer el plugin]

## Troubleshooting

[Problemas comunes y soluciones]
````

**Calidad de Código del Plugin:**

✅ **Arquitectura Limpia**

- Código organizado, legible y siguiendo buenas prácticas de .NET
- Nombres descriptivos (no `func1`, `tmp`, `x`)

✅ **Integración Correcta**

- Uso apropiado de `[LatinoExtension]` con metadatos:

  ```csharp
  [LatinoExtension("modulo.funcion", 
      Help = "Descripción clara",
      Args = "param1, param2",
      Returns = "tipo_retorno")]
  ```

✅ **Resiliencia**

- Manejo proactivo de errores (lanza `RuntimeError` descriptivos)
- Validación de parámetros antes de usarlos
- No debe crashear el intérprete en ningún escenario

❌ **Lo que NO se acepta:**

- Plugins "Hola Mundo" o de suma de dos números
- Código sin documentación
- DLLs sin código fuente disponible
- Plugins que requieran dependencias no documentadas

## Convertirse en Mantenedor

### Filosofía: Meritocracia Basada en Evidencia

No exigimos títulos universitarios ni certificaciones. En Open Source, **el código habla más fuerte que el currículum**. Buscamos desarrolladores que demuestren capacidad técnica y alineación con nuestra filosofía.

### Requisitos para Postularse

#### 1. Competencia Técnica Demostrable

Proporciona enlaces a tu perfil público (GitHub, GitLab) que demuestren:

- [ ] **Historial de actividad:** Contribuciones constantes y claras
- [ ] **Higiene de Git:** Commits atómicos, mensajes descriptivos, manejo de ramas y resolución de conflictos
- [ ] **Colaboración:** Participación en code reviews, issues, discusiones técnicas

#### 2. Espíritu Open Source

Debes tener al menos **un proyecto público** (tuyo o como colaborador principal) que:

- [ ] Esté activamente mantenido o en desarrollo claro
- [ ] Use licencia permisiva (preferiblemente **MIT**, como Latino.NET)
- [ ] Tenga documentación de calidad (`README` estructurado, guías de uso)

#### 3. La Prueba Técnica: Plugin Funcional

**Debes crear y publicar un plugin (.dll) para Latino.NET** que demuestre que comprendes la arquitectura interna y sabes explotar C# dentro de Latino.

**Requisitos del Plugin de Prueba:**

✅ **Complejidad Funcional:**

- ❌ No sirve: "Hola Mundo", suma de números
- ✅ Sí sirve: Conector a base de datos (SQLite), wrapper de API externa, procesamiento de imágenes, utilidades de sistema avanzadas

✅ **Documentación Completa:**

- Manual de uso (`MANUAL.md`) con todas las funciones documentadas
- Ejemplos de código funcionales
- Guía de instalación

- Arquitectura limpia y organizada
- Integración correcta con la API de Latino.NET
- Manejo robusto de errores
- **Dependencias justificadas:** si el plugin declara alguna dependencia externa, explica en el README **por qué es inevitable** y por qué no cabe un port propio. La dependencia no es un defecto, pero una dependencia sin explicación sí lo es.

✅ **Publicación:**

- Código fuente disponible (GitHub/GitLab)
- Binario (.dll) disponible para descarga
- Tests unitarios incluidos

### Responsabilidades del Mantenedor

Al obtener acceso de escritura al repositorio, asumes los siguientes compromisos:

#### 1. Guardián de la Estabilidad

- Priorizar solidez del núcleo sobre features innecesarias
- Analizar rigurosamente cambios que rompan compatibilidad backward

#### 2. Revisión Constructiva y Responsable

**Tu criterio es irremplazable:**

- Aunque uses IA para análisis preliminar, **tú eres el responsable final**
- Debes ejecutar tests unitarios antes de aprobar
- Valida que el código siga patrones de diseño del proyecto
- Verifica que viene acompañado de documentación

**Checklist de Revisión de PR:**

- [ ] El código compila sin errores
- [ ] Los tests pasan
- [ ] Sigue el estilo del proyecto
- [ ] La funcionalidad está documentada
- [ ] No introduce vulnerabilidades de seguridad
- [ ] No degrada el rendimiento sin justificación

#### 3. Documentación Continua

- Cualquier cambio aprobado debe tener su documentación actualizada
- Si un PR no incluye documentación, solicítala antes de aprobar

#### 4. Embajador del Código de Conducta

- Sé ejemplo de trato profesional y rigor técnico
- Paciencia con principiantes, firmeza con estándares de calidad

#### 5. Transparencia en el Uso de IA

**Fomenta la declaración de asistencia por IA:**

- Incentiva a contribuidores a declarar en commits si usaron IA:

  ```text
  feat(core): Añadir soporte para operador ternario
  
  Implementa sintaxis: condicion ? verdadero : falso
  
  Assisted-by: Claude 3.7 (generación de tests)
  ```

- **Criterio Propio:** La IA nunca debe tener el "voto decisivo"
- Valida que código generado por IA sea seguro, eficiente y mantenible

#### 6. Gestión de Issues y PRs

**Etiquetado de Issues:**

- `bug` → Error confirmado que requiere corrección
- `enhancement` → Solicitud de mejora o nueva función
- `documentation` → Mejoras a la documentación
- `good first issue` → Ideal para nuevos contribuidores
- `needs-docs` → PR aprobado condicionalmente, requiere documentación
- `breaking-change` → Rompe compatibilidad con versiones anteriores

**Tiempos de Respuesta:**

- Issues críticos (crashes): Responder en 48 horas
- PRs: Primera revisión en 7 días
- Issues de documentación: Clasificar en 72 horas

## 🚀 Proceso de Postulación a Mantenedor

Si cumples los requisitos y deseas dedicar tiempo a mejorar Latino.NET:

1. **Prepara tu Portafolio:**
   - Perfil de GitHub/GitLab actualizado
   - Proyecto Open Source representativo
   - Plugin de prueba funcional y documentado

2. **Abre un Issue:**
   - Título: `[Postulación a Mantenedor] - Tu Nombre / Usuario`
   - Cuerpo:

     ```markdown
     ## Información Personal
     - **Nombre:** [Tu nombre]
     - **Usuario GitHub:** [@tuusuario]
     - **Ubicación:** [País/Región - opcional]
     
     ## Portafolio
     - **Perfil:** https://github.com/tuusuario
     - **Proyecto Open Source:** [enlace + breve descripción]
     - **Plugin de Prueba:** [enlace al repositorio]
     
     ## Motivación
     [¿Por qué quieres ser mantenedor de Latino.NET?]
     
     ## Disponibilidad
     [¿Cuántas horas semanales puedes dedicar?]
     
     ## Áreas de Interés
     - [ ] Core (Lexer/Parser/Interpreter)
     - [ ] StdLib (Librerías estándar)
     - [ ] LSP (Extensión VS Code)
     - [ ] Documentación
     - [ ] Gestión de comunidad
     ```

3. **Revisión del Equipo:**
   - El equipo revisará tu código, plugin y trayectoria
   - Podrían solicitar una videollamada técnica
   - Decisión en 2-3 semanas

## 📚 Recursos Adicionales

- **Manual de Latino.NET:** `docs/Manual_De_Latino.NET_vX.X.XX.pdf`
- **Arquitectura Interna:** `docs/Arquitectura_Interna.md`
- **Manual de la Extensión:** `docs/MANUAL_MANTENIMIENTO_v2_PARTE1.md` y `PARTE2.md`
- **Ejemplos de Código:** `ejemplos/`

## 🙏 Atribución

Este documento se inspira en:

- [Contributor Covenant](https://www.contributor-covenant.org/) (Código de Conducta)
- [GitHub Community Guidelines](https://docs.github.com/en/site-policy/github-terms/github-community-guidelines)
- Prácticas de proyectos .NET (Roslyn, ASP.NET Core)

Adaptado para reflejar los estándares de ingeniería y enfoque profesional de Latino.NET.

**Última actualización:** Septiembre 2026  
**Versión del documento:** 2.0

> **Versión:** Latino.NET v1.0.81 | Última actualización: 2026-09-16
