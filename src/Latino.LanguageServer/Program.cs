using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Latino.LanguageServer;
using Latino.Core.Runtime;

class Program
{
    static async Task Main(string[] args)
    {
        await Console.Error.WriteLineAsync($"[DEBUG SERVER] Args recibidos: {string.Join(", ", args)}");

        // 1. Parsear Argumentos
        string? globalPathArg = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--global-path" && i + 1 < args.Length)
            {
                globalPathArg = args[i + 1];
            }
        }

        var server = await LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(x => x
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
                )
                .WithServices(services =>
                {
                    // OPTIMIZACIÓN: Intérprete compartido (Singleton) para ahorrar memoria
                    services.AddSingleton<Interpreter>(sp => {
                        var interp = new Interpreter();
                        Latino.Core.StdLib.StdLibLoader.LoadAll(interp); // Carga inicial
                        return interp;
                    });
                    
                    services.AddSingleton<PluginManager>();
                })
                .WithHandler<TextDocumentHandler>()
                .WithHandler<CompletionHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<DocumentSymbolHandler>()
                .OnInitialize((server, request, token) =>
                {
                    // 2. Inyectar la ruta al Manager
                    if (!string.IsNullOrEmpty(globalPathArg))
                    {
                        var pm = server.Services.GetService<PluginManager>();
                        if (pm != null) pm.GlobalPath = globalPathArg;
                    }
                    return Task.CompletedTask;
                })
                .OnStarted((server, token) =>
                {
                    return Task.CompletedTask;
                })
        );

        await server.WaitForExit;
    }
}
