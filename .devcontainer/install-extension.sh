#!/usr/bin/env bash
# Instala la extension de VS Code en el Codespace (postAttachCommand).
# Idempotente y NO fatal: nunca debe romper el arranque del Codespace.
# Es una red de seguridad: el camino normal es el mecanismo nativo
# (customizations.vscode.extensions), que la instala antes de abrir el editor.
VSIX="${containerWorkspaceFolder:-$PWD}/.devcontainer/latino-net.vsix"
FLAG="$HOME/.latino_ext_installed"

# Si ya esta instalada, no reinstalar: el mecanismo nativo de devcontainer
# (customizations.vscode.extensions) suele haberla instalado antes de arrancar.
if code --list-extensions 2>/dev/null | grep -qi 'latino-net-vscode'; then
    touch "$FLAG"
    exit 0
fi

if [ -f "$VSIX" ] && [ ! -f "$FLAG" ]; then
    echo "📦 Instalando extensión Latino.NET..."
    if code --install-extension "$VSIX" --force; then
        touch "$FLAG"
        echo "✅ Extensión instalada con éxito."
    else
        echo "⚠️ No se pudo instalar la extensión automáticamente. Instálala manualmente desde $VSIX"
    fi
fi
exit 0
