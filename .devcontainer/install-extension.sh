#!/usr/bin/env bash
# Instala la extensión de VS Code en el Codespace (postAttachCommand).
# Idempotente y NO fatal: nunca debe romper el arranque del Codespace.
VSIX="/opt/latino-ext/latino-net.vsix"
FLAG="$HOME/.latino_ext_installed"

[ -f "$VSIX" ] && [ ! -f "$FLAG" ] || exit 0

if code --install-extension "$VSIX" --force; then
    touch "$FLAG"
else
    echo "⚠️ No se pudo instalar la extensión automáticamente. Instálala manualmente desde $VSIX"
fi
exit 0
