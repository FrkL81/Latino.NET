import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient/node';

let client: LanguageClient;

// ============================================================================
// HELPERS PARA CREACIÓN DE ARCHIVOS
// ============================================================================

// Retorna la ruta raíz del workspace actual
function getWorkspaceRoot(): string | undefined {
    const folders = vscode.workspace.workspaceFolders;
    if (!folders || folders.length === 0) {
        vscode.window.showErrorMessage("Latino.NET: Por favor, abre una carpeta en VS Code primero.");
        return undefined;
    }
    return folders[0].uri.fsPath;
}

// Genera un nombre único añadiendo _1, _2 si el archivo ya existe
function getUniquePath(dir: string, baseName: string, ext: string): string {
    let currentPath = path.join(dir, `${baseName}${ext}`);
    let counter = 1;
    while (fs.existsSync(currentPath)) {
        currentPath = path.join(dir, `${baseName}_${counter}${ext}`);
        counter++;
    }
    return currentPath;
}

// Escribe el archivo forzando UTF-8 con BOM (\uFEFF) para evitar corrupción de caracteres
async function createUtf8FileAndOpen(filePath: string, content: string) {
    try {
        fs.mkdirSync(path.dirname(filePath), { recursive: true });
        // Escribir archivo con BOM UTF-8
        fs.writeFileSync(filePath, '\uFEFF' + content, 'utf8');
        
        // Abrir el archivo en el editor
        const document = await vscode.workspace.openTextDocument(filePath);
        await vscode.window.showTextDocument(document);
    } catch (err: any) {
        vscode.window.showErrorMessage(`Error al crear el archivo: ${err.message}`);
    }
}

// Escanea directorios excluyendo carpetas ruidosas
function getProjectFolders(dir: string, relativePath: string, depth: number): vscode.QuickPickItem[] {
    const EXCLUDE =['.git', 'node_modules', 'bin', 'obj', 'dist', 'out', '.vscode', '.vs'];
    let items: vscode.QuickPickItem[] =[];
    
    if (depth > 4) return items; // Límite de profundidad de búsqueda

    try {
        const entries = fs.readdirSync(dir, { withFileTypes: true });
        for (const entry of entries) {
            if (entry.isDirectory() && !EXCLUDE.includes(entry.name) && !entry.name.startsWith('.')) {
                const absPath = path.join(dir, entry.name);
                const relPath = relativePath ? `${relativePath}/${entry.name}` : entry.name;
                
                // El campo 'detail' guardará la ruta absoluta oculta
                items.push({
                    label: `$(folder) ${entry.name}`,
                    description: relPath,
                    detail: absPath 
                });
                
                // Recursión para subcarpetas
                items = items.concat(getProjectFolders(absPath, relPath, depth + 1));
            }
        }
    } catch (e) {
        // Ignorar carpetas sin permisos
    }
    return items;
}

// Muestra el menú de selección al usuario
async function pickTargetFolder(wsRoot: string): Promise<string | undefined> {
    const folderItems: vscode.QuickPickItem[] =[
        {
            label: '$(root-folder) Raíz del Proyecto',
            description: '/',
            detail: wsRoot
        },
        ...getProjectFolders(wsRoot, '', 0),
        {
            label: '$(folder-opened) Examinar carpeta del sistema...',
            description: 'Abrir ventana de Windows/Mac/Linux',
            detail: '__BROWSE__'
        }
    ];

    const selected = await vscode.window.showQuickPick(folderItems, {
        title: 'Latino.NET: ¿Dónde crear el script?',
        placeHolder: 'Selecciona la carpeta destino o escribe para buscar...',
        matchOnDescription: true
    });

    if (!selected) return undefined;

    if (selected.detail === '__BROWSE__') {
        const uris = await vscode.window.showOpenDialog({
            defaultUri: vscode.Uri.file(wsRoot),
            canSelectFiles: false,
            canSelectFolders: true,
            openLabel: 'Seleccionar Destino'
        });
        return uris && uris.length > 0 ? uris[0].fsPath : undefined;
    }

    return selected.detail; // Retorna la ruta absoluta
}

// ============================================================================
// ACTIVACIÓN DE LA EXTENSIÓN
// ============================================================================

export function activate(context: vscode.ExtensionContext) {
    const outputChannel = vscode.window.createOutputChannel("Latino Client");
    outputChannel.appendLine(`[${new Date().toLocaleTimeString()}] Activando extensión Latino.NET (v1.0.81)...`);

    // --- 1. PREPARACIÓN DE ARGUMENTOS GLOBALES ---
    const config = vscode.workspace.getConfiguration('latino');
    let binaryPath = config.get<string>('binaryPath');
    let extraArgs: string[] =[];

    if (binaryPath && binaryPath.trim() !== "") {
        let cleanPath = binaryPath.replace(/['"]/g, '').trim();
        try {
            let globalDir = cleanPath;
            if (fs.existsSync(cleanPath)) {
                const stat = fs.statSync(cleanPath);
                if (stat.isFile()) globalDir = path.dirname(cleanPath);
            } else if (path.extname(cleanPath) !== "") {
                globalDir = path.dirname(cleanPath);
            }
            if (globalDir && globalDir !== ".") {
                extraArgs.push('--global-path', globalDir);
            }
        } catch (e) {
            outputChannel.appendLine(`[Error] Fallo al procesar binaryPath: ${e}`);
        }
    }

    // --- 2. LOCALIZAR SERVIDOR LSP ---
    const prodServerPath = context.asAbsolutePath(path.join('server', 'Latino.LanguageServer.dll'));
    const devServerPath = context.asAbsolutePath(path.join('..', 'src', 'Latino.LanguageServer', 'bin', 'Debug', 'net10.0', 'Latino.LanguageServer.dll'));

    let serverPath = prodServerPath;
    if (!fs.existsSync(serverPath)) {
        if (fs.existsSync(devServerPath)) {
            serverPath = devServerPath;
        } else {
            vscode.window.showErrorMessage("Error Fatal: No se encuentra Latino.LanguageServer.dll");
            return;
        }
    }

    // --- 3. INICIAR CLIENTE LSP ---
    let serverOptions: ServerOptions = {
        run: { command: 'dotnet', args:[serverPath, ...extraArgs] },
        debug: { command: 'dotnet', args: [serverPath, ...extraArgs] }
    };

    let clientOptions: LanguageClientOptions = {
        documentSelector:[{ scheme: 'file', language: 'latino' }],
        outputChannel: outputChannel
    };

    try {
        client = new LanguageClient('latinoLanguageServer', 'Latino Language Server', serverOptions, clientOptions);
        client.start();
    } catch (e) {}

    // ============================================================================
    // REGISTRO DE COMANDOS (RUN & GENERADORES)
    // ============================================================================

    // Comando: Ejecutar
    let runCommand = vscode.commands.registerCommand('latino.run', () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showWarningMessage("No hay ningún archivo abierto para ejecutar.");
            return;
        }
        const filePath = editor.document.fileName;
        const terminalName = "Latino Run";
        let terminal = vscode.window.terminals.find(t => t.name === terminalName);
        if (!terminal) terminal = vscode.window.createTerminal(terminalName);
        terminal.show();

        let executable = "latino";
        if (binaryPath && binaryPath.trim() !== "") executable = binaryPath.replace(/['"]/g, '').trim();

        if (process.platform === 'win32') {
            terminal.sendText(`& "${executable}" "${filePath}"`);
        } else {
            terminal.sendText(`"${executable}" "${filePath}"`);
        }
    });

    // Comando: Nuevo Archivo Vacío (Selector de Carpeta + Input Box)
    let cmdNewFile = vscode.commands.registerCommand('latino.newFile', async () => {
        const wsRoot = getWorkspaceRoot();
        if (!wsRoot) return;

        // 1. Preguntar dónde guardar
        const targetDir = await pickTargetFolder(wsRoot);
        if (!targetDir) return; // Usuario canceló

        // 2. Deducir nombre sugerido
        let baseName = "archivo";
        let counter = 1;
        while (fs.existsSync(path.join(targetDir, `${baseName}_${counter}.lat`))) {
            counter++;
        }
        let defaultName = `${baseName}_${counter}.lat`;

        // 3. Preguntar el nombre del archivo
        const relativeTarget = path.relative(wsRoot, targetDir) || 'Raíz';
        const fileName = await vscode.window.showInputBox({
            title: `Nuevo script en: ${relativeTarget}`,
            prompt: "Nombre del archivo (ej: mis_funciones)",
            value: defaultName,
            valueSelection: [0, defaultName.length - 4]
        });

        if (!fileName) return;

        // 4. Validar y crear
        const finalName = fileName.endsWith('.lat') ? fileName : `${fileName}.lat`;
        const filePath = path.join(targetDir, finalName);

        if (fs.existsSync(filePath)) {
            vscode.window.showWarningMessage(`Latino.NET: El archivo '${finalName}' ya existe.`);
            return;
        }

        const content = `# Script Latino.NET\n# Creado el: ${new Date().toISOString().split('T')[0]}\n\n`;
        await createUtf8FileAndOpen(filePath, content);
    });

    // Comando: Nuevo Módulo Simple
    let cmdNewModule = vscode.commands.registerCommand('latino.newModule', async () => {
        const wsRoot = getWorkspaceRoot();
        if (!wsRoot) return;
        const modDir = path.join(wsRoot, "lib");
        const filePath = getUniquePath(modDir, "modulo", ".lat");
        const content = `# Módulo Latino.NET (Patrón Factory)\n\nfuncion CrearModulo()\n    // Variables privadas\n    local version = "1.0"\n\n    retornar {\n        "version": version,\n        "saludar": funcion(nombre)\n            escribir(ç("Hola |nombre| desde el módulo."))\n        fin\n    }\nfin\n`;
        await createUtf8FileAndOpen(filePath, content);
    });

    // Comando: Nueva Clase
    let cmdNewClass = vscode.commands.registerCommand('latino.newClass', async () => {
        const wsRoot = getWorkspaceRoot();
        if (!wsRoot) return;
        const modDir = path.join(wsRoot, "clases");
        const filePath = getUniquePath(modDir, "Clase", ".lat");
        const content = `clase MiClase\n    // El constructor inicializa el estado de la instancia\n    constructor(nombre = "Desconocido")\n        mi.nombre = nombre\n    fin\n\n    funcion mostrar()\n        escribir(ç("Instancia de MiClase: |mi.nombre|"))\n    fin\nfin\n`;
        await createUtf8FileAndOpen(filePath, content);
    });

    // Comando: Nuevo Proyecto CRUD
    let cmdNewCrud = vscode.commands.registerCommand('latino.newCrudProject', async () => {
        const wsRoot = getWorkspaceRoot();
        if (!wsRoot) return;
        
        const projDir = wsRoot;
        const filePath = path.join(projDir, "principal.lat");
        
        const content = `/*\n  Proyecto CRUD CLI Básico\n  Latino.NET con SQLite\n*/\nincluir("sqlite")\n\nglobal db = nulo\n\nfuncion iniciar()\n    db = sqlite.conectar("datos.db")\n    sqlite.ejecutar(db, "CREATE TABLE IF NOT EXISTS items (id INTEGER PRIMARY KEY, nombre TEXT)")\nfin\n\nfuncion menu()\n    mientras verdadero\n        limpiar("pantalla")\n        escribir("=== SISTEMA CRUD ===")\n        escribir("1. Agregar Item")\n        escribir("2. Listar Items")\n        escribir("0. Salir")\n        \n        op = pedir("\\nOpción: ")\n        \n        elegir(op)\n            caso "1":\n                n = pedir("Nombre: ")\n                si n != ""\n                    sqlite.ejecutar(db, "INSERT INTO items (nombre) VALUES (@n)", {"n": n})\n                fin\n            caso "2":\n                items = sqlite.consultar(db, "SELECT * FROM items")\n                para item en items\n                    escribirf("[{0}] {1}\\n", item["id"], item["nombre"])\n                fin\n                pedir("\\nPresiona Enter...")\n            caso "0":\n                sqlite.cerrar(db)\n                romper\n        fin\n    fin\nfin\n\nintentar\n    iniciar()\n    menu()\natrapar (e)\n    escribir("Error crítico: " .. e)\n    si db != nulo sqlite.cerrar(db) fin\nfin\n`;
        
        await createUtf8FileAndOpen(filePath, content);
    });

    // Comando: Nuevo Proyecto Profesional
    let cmdNewProfessionalProject = vscode.commands.registerCommand('latino.newProfessionalProject', async () => {
        const wsRoot = getWorkspaceRoot();
        if (!wsRoot) return;
        
        const projDir = wsRoot;
        
        // Crear estructura de carpetas recomendada
        const folders = ["lib", "clases", "plugins", "datos", "pruebas", "docs"];
        for (const folder of folders) {
            fs.mkdirSync(path.join(projDir, folder), { recursive: true });
        }
        
        // Archivo 1: inicio.lat (Punto de entrada)
        const mainPath = path.join(projDir, "inicio.lat");
        const mainContent = `/*\n  Punto de entrada principal del proyecto\n  Creado el: ${new Date().toISOString().split('T')[0]}\n*/\n\n// Importar plugins oficiales\nincluir("lapsos")\n\n// Importar módulos propios desde la carpeta lib/\n// incluir("lib/utilidades")\n\nfuncion principal()\n    limpiar("pantalla")\n    escribir("=== Proyecto Profesional Latino.NET ===")\n    escribir("Estructura base generada correctamente.")\nfin\n\n// Iniciar programa\nintentar\n    principal()\natrapar (e)\n    escribir("Error fatal: " .. e)\nfin\n`;
        await createUtf8FileAndOpen(mainPath, mainContent);
        
        // Archivo 2: README.md en docs/
        const readmePath = path.join(projDir, "docs", "README.md");
        const readmeContent = `# Mi Proyecto Latino.NET\n\n## Estructura\n- \`inicio.lat\`: Punto de entrada (Main).\n- \`lib/\`: Módulos .lat funcionales y utilidades.\n- \`clases/\`: Definiciones de clases (POO).\n- \`plugins/\`: Librerías .dll externas.\n- \`datos/\`: Archivos de configuración (JSON, CSV).\n- \`pruebas/\`: Scripts de testing.\n- \`docs/\`: Documentación del proyecto.\n`;
        fs.writeFileSync(readmePath, '\uFEFF' + readmeContent, 'utf8');

        // Archivo 3: .editorconfig en la raíz para forzar UTF-8
        const editorConfigPath = path.join(projDir, ".editorconfig");
        const editorConfigContent = `root = true\n\n[*.lat]\ncharset = utf-8\nindent_style = space\nindent_size = 4\n`;
        fs.writeFileSync(editorConfigPath, editorConfigContent, 'utf8');
        
        vscode.window.showInformationMessage("Proyecto Profesional generado con éxito.");
    });

    context.subscriptions.push(runCommand, cmdNewFile, cmdNewModule, cmdNewClass, cmdNewCrud, cmdNewProfessionalProject);
}

export function deactivate(): Thenable<void> | undefined {
    return client ? client.stop() : undefined;
}
