using Xunit;
using System.IO;
using System;
using Microsoft.Extensions.Logging.Abstractions;
using Latino.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Latino.Tests;

public class PluginLogicTests
{
    [Fact]
    public void TestGlobalPathResolution()
    {
        // 1. Setup: Crear estructura de carpetas falsa
        string fakeGlobalDir = Path.Combine(Path.GetTempPath(), "LatinoGlobal_" + Guid.NewGuid());
        Directory.CreateDirectory(fakeGlobalDir);
        
        string fakeDllName = "libreria_global.dll";
        string fakeDllPath = Path.Combine(fakeGlobalDir, fakeDllName);
        File.WriteAllText(fakeDllPath, "dummy content"); // Crear archivo vacío

        try
        {
            // 2. Instanciar Manager (con Logger mudo)
            var manager = new PluginManager(NullLogger<PluginManager>.Instance);
            
            // 3. Inyectar Ruta Global
            manager.GlobalPath = fakeGlobalDir;

            // 4. Probar resolución
            // Simulamos que estamos editando un archivo en OTRA carpeta
            string userScriptUri = "file:///C:/Users/Dev/script.lat"; 
            
            // ResolveDllPath ahora es público
            string resolved = manager.ResolveDllPath(DocumentUri.Parse(userScriptUri), fakeDllName);
            
            Assert.Equal(Path.GetFullPath(fakeDllPath), Path.GetFullPath(resolved));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(fakeGlobalDir)) Directory.Delete(fakeGlobalDir, true);
        }
    }
}
