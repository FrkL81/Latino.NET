#!/usr/bin/env bash
# Instala la extensión de VS Code en el Codespace (postAttachCommand).
# Idempotente y NO fatal: nunca debe romper el arranque del Codespace.
VSIX="/opt/latino-ext/latino-net.vsix"
FLAG="$HOME/.latino_ext_installed"

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
