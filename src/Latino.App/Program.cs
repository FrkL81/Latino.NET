using System;
using System.IO;
using System.Collections.Generic;
using Latino.Core.Analysis;
using Latino.Core.Runtime;
using Latino.Core.Types;
using Latino.Core.StdLib;
using System.Linq;

class Program
{
    // ASCII Art y versión centralizados
    private const string Version = "v1.0.81";
    private const string AsciiArt = @"
 _          _   _                   _   _ _____ _____ 
| |    __ _| |_(_)_ __   ___       | \ | | ____|_   _|
| |   / _` | __| | '_ \ / _ \      |  \| |  _|   | |  
| |__| (_| | |_| | | | | (_) |  _  | |\  | |___  | |  
|_____\__,_|\__|_|_| |_|\___/  (_) |_| \_|_____| |_|  
";

    static void Main(string[] args)
    {
        // SOPORTE UTF-8 (Tildes y Emojis)
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.CancelKeyPress += (sender, e) => {
            Console.WriteLine("\n👋 Saliendo...");
            Environment.Exit(0);
        };

        var interpreter = new Interpreter();
        StdLibLoader.LoadAll(interpreter);

        // Sin argumentos → REPL
        if (args.Length == 0)
        {
            RunPrompt(interpreter);
            return;
        }

        string command = args[0];

        switch (command)
        {
            case "-v":
            case "--version":
                ShowVersion();
                break;

            case "-a":
            case "--ayuda":
            case "--help":
            case "-h":
                ShowHelp();
                break;

            case "-l":
            case "--licencia":
            case "--license":
                ShowLicense();
                break;

            case "--apoyar":
                ShowSupport();
                break;

            case "-e":
                if (args.Length < 2)
                {
                    Console.WriteLine("❌ Error: Se requiere código después de -e");
                    Console.WriteLine("Ejemplo: latino -e \"escribir('Hola')\"");
                }
                else
                {
                    string source = string.Join(" ", args.Skip(1));
                    Run(interpreter, source);
                }
                break;

            default:
                RunFile(interpreter, command);
                break;
        }
    }

    static void ShowVersion()
    {
        Console.WriteLine(AsciiArt);
        Console.WriteLine($"  Versión {Version} (Core .NET 10)");
        Console.WriteLine($"  Copyright © {DateTime.Now.Year} Franklin A. Lara P.");
        Console.WriteLine();
    }

    static void ShowHelp()
    {
        Console.WriteLine($"Latino.NET {Version} - Lenguaje de Programación en Español\n");
        
        Console.WriteLine("📖 USO:");
        Console.WriteLine("  latino [archivo.lat]          Ejecuta un archivo");
        Console.WriteLine("  latino                        Inicia el REPL interactivo");
        Console.WriteLine();
        
        Console.WriteLine("⚙️  OPCIONES:");
        Console.WriteLine("  -v, --version                 Muestra la versión");
        Console.WriteLine("  -a, --ayuda, --help, -h       Muestra esta ayuda");
        Console.WriteLine("  -l, --licencia, --license     Muestra la licencia MIT");
        Console.WriteLine("  --apoyar                      Muestra cómo apoyar el proyecto y abre el enlace");
        Console.WriteLine("  -e \"codigo\"                   Ejecuta código en línea");
        Console.WriteLine();
        
        Console.WriteLine("💡 EJEMPLOS:");
        Console.WriteLine("  latino hola.lat               # Ejecuta el archivo hola.lat");
        Console.WriteLine("  latino -e \"escribir('Hola')\"  # Ejecuta código directo");
        Console.WriteLine("  latino -e \"sis.manual()\"      # Abre el manual en PDF");
        Console.WriteLine("  latino --apoyar               # Abre la página de patrocinio");
        Console.WriteLine();
        
        Console.WriteLine("📚 DOCUMENTACIÓN:");
        Console.WriteLine("  Manual PDF: latino -e \"sis.manual()\"");
        Console.WriteLine("  Repositorio Oficial: https://github.com/FrkL81/Latino.NET");
        Console.WriteLine();
    }

    static void ShowLicense()
    {
        Console.WriteLine($"Latino.NET {Version}\n");
        
        Console.WriteLine("📜 LICENCIA MIT\n");
        
        Console.WriteLine($"Copyright (c) {DateTime.Now.Year} Franklin A. Lara P.");
        Console.WriteLine();
        
        Console.WriteLine("Se concede permiso, libre de cargos, a cualquier persona que obtenga una");
        Console.WriteLine("copia de este software y de los archivos de documentación asociados (el");
        Console.WriteLine("\"Software\"), a utilizar el Software sin restricción, incluyendo sin");
        Console.WriteLine("limitación los derechos a usar, copiar, modificar, fusionar, publicar,");
        Console.WriteLine("distribuir, sublicenciar, y/o vender copias del Software, y a permitir a");
        Console.WriteLine("las personas a las que se les proporcione el Software a hacer lo mismo,");
        Console.WriteLine("sujeto a las siguientes condiciones:");
        Console.WriteLine();
        
        Console.WriteLine("El aviso de copyright anterior y este aviso de permiso se incluirán en");
        Console.WriteLine("todas las copias o partes sustanciales del Software.");
        Console.WriteLine();
        
        Console.WriteLine("EL SOFTWARE SE PROPORCIONA \"COMO ESTÁ\", SIN GARANTÍA DE NINGÚN TIPO,");
        Console.WriteLine("EXPRESA O IMPLÍCITA, INCLUYENDO PERO NO LIMITADO A GARANTÍAS DE");
        Console.WriteLine("COMERCIALIZACIÓN, IDONEIDAD PARA UN PROPÓSITO PARTICULAR E INCUMPLIMIENTO.");
        Console.WriteLine("EN NINGÚN CASO LOS AUTORES O PROPIETARIOS DE LOS DERECHOS DE AUTOR SERÁN");
        Console.WriteLine("RESPONSABLES DE NINGUNA RECLAMACIÓN, DAÑOS U OTRAS RESPONSABILIDADES, YA SEA");
        Console.WriteLine("EN UNA ACCIÓN DE CONTRATO, AGRAVIO O CUALQUIER OTRO MOTIVO, DERIVADAS DE,");
        Console.WriteLine("FUERA DE O EN CONEXIÓN CON EL SOFTWARE O SU USO U OTRO TIPO DE ACCIONES EN");
        Console.WriteLine("EL SOFTWARE.");
        Console.WriteLine();
    }

    static void ShowSupport()
    {
        Console.WriteLine("☕ Proyecto Latino.NET — Libre, Abierto y Gratuito\n");
        Console.WriteLine("Latino.NET es un esfuerzo independiente para democratizar la programación industrial en español.");
        Console.WriteLine("Tu apoyo nos permite dedicar más horas de ingeniería, infraestructura y documentación al proyecto.\n");
        Console.WriteLine("Abriendo en tu navegador: https://frkl81.gumroad.com/l/LatinoNET\n");
        Console.WriteLine("¡Gracias por hacer posible que esta herramienta siga creciendo!\n");

        SysLib.AbrirUrl("https://frkl81.gumroad.com/l/LatinoNET");
    }

    static void RunFile(Interpreter interpreter, string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"❌ Error: El archivo '{path}' no existe.");
            Console.WriteLine($"💡 Usa 'latino --ayuda' para ver opciones disponibles.");
            Environment.ExitCode = 1;
            return;
        }

        // Cambiar contexto al directorio del script para 'incluir' relativo
        string? scriptDir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(scriptDir))
        {
            Directory.SetCurrentDirectory(scriptDir);
        }

        string source = File.ReadAllText(fullPath);
        Run(interpreter, source);
    }

    static void RunPrompt(Interpreter interpreter)
    {
        Console.WriteLine(AsciiArt);
        Console.WriteLine($"  Versión {Version} - REPL Interactivo");
        Console.WriteLine($"  Escribe 'salir' o presiona Ctrl+C para terminar.\n");
        
        while (true)
        {
            Console.Write("latino> ");
            string? line = Console.ReadLine();
            
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.Trim().ToLower() == "salir") 
            {
                Console.WriteLine("👋 ¡Hasta pronto!");
                break;
            }
            
            Run(interpreter, line, isRepl: true);
        }
    }

    static void Run(Interpreter interpreter, string source, bool isRepl = false)
    {
        try
        {
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            // Verificar errores de sintaxis antes de ejecutar
            if (parser.Errors.Count > 0)
            {
                foreach (var error in parser.Errors)
                {
                    Console.WriteLine($"❌ Error de Sintaxis [Línea {error.Token.Line}]: {error.Message}");
                }
                if (!isRepl) Environment.ExitCode = 1;
                return;
            }

            if (statements != null) 
            {
                interpreter.Interpret(statements);
            }
        }
        catch (RuntimeError ex)
        {
            string lineInfo = ex.Token != null ? $" [Línea {ex.Token.Line}]" : "";
            Console.WriteLine($"❌ Error Runtime{lineInfo}: {ex.Message}");
            if (!isRepl) Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (!isRepl) Environment.ExitCode = 1;
        }
    }
}