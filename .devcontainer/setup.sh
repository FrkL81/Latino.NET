#!/usr/bin/env bash
# Configura Latino.NET dentro del contenedor (postCreateCommand).
# Descarga la última release pública y deja el runtime en /opt/latino.
set -euo pipefail

REPO="FrkL81/Latino.NET"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

echo "🚀 Configurando Latino.NET..."

# 1. Resolver la última release (sin versión fija en el script).
#    En Codespaces el token puede llegar como GITHUB_TOKEN o CODESPACES_TOKEN.
AUTH=()
if [ -n "${GITHUB_TOKEN:-}" ]; then
    AUTH=(-H "Authorization: Bearer ${GITHUB_TOKEN}")
elif [ -n "${CODESPACES_TOKEN:-}" ]; then
    AUTH=(-H "Authorization: Bearer ${CODESPACES_TOKEN}")
fi
RELEASE_JSON="$(curl -fsSL "${AUTH[@]}" "https://api.github.com/repos/${REPO}/releases/latest")"

ZIP_URL="$(grep -oE 'https://[^"]+/latino-linux[^"/]*\.zip' <<<"$RELEASE_JSON" | head -1 || true)"
VSIX_URL="$(grep -oE 'https://[^"]+/latino-net-vscode[^"/]*\.vsix' <<<"$RELEASE_JSON" | head -1 || true)"
[ -n "$ZIP_URL" ] || { echo "❌ No se encontró el zip de Linux en la última release"; exit 1; }

# 2. Descargar y descomprimir.
#    El zip de Linux se genera en Windows y guarda las rutas con '\' como
#    separador; infozip/unzip interpreta '\' como carácter de escape y falla.
#    Por eso se descomprime con Python (stdlib), que respeta el campo de nombre.
curl -fsSL "$ZIP_URL" -o "$TMP_DIR/latino.zip"
if command -v python3 >/dev/null 2>&1; then
    PY=python3
elif command -v python >/dev/null 2>&1; then
    PY=python
else
    echo "❌ Se necesita python3 o python para descomprimir el zip de Linux"; exit 1
fi
"$PY" - "$TMP_DIR/latino.zip" "$TMP_DIR/x" <<'PYEOF'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    z.extractall(sys.argv[2])
PYEOF

# 3. Localizar el binario (la estructura interna puede variar, p. ej. dist/linux/latino)
BIN="$(find "$TMP_DIR/x" -type f -name latino | head -1)"
[ -n "$BIN" ] || { echo "❌ No se encontró el binario 'latino' dentro del zip"; exit 1; }

# 4. Instalar la carpeta completa (binario + lib/ + nativos) y enlazar en el PATH
sudo rm -rf /opt/latino
sudo mkdir -p /opt/latino
sudo cp -r "$(dirname "$BIN")/." /opt/latino/
sudo chmod +x /opt/latino/latino
sudo ln -sf /opt/latino/latino /usr/local/bin/latino

# Salvaguarda: el LSP busca los plugins en /opt/latino/lib
[ -d /opt/latino/lib ] || echo "⚠️ No existe /opt/latino/lib: los plugins podrían no cargarse (revisar estructura del zip)"

# 5. Dejar el VSIX listo; se instala en postAttachCommand
if [ -n "$VSIX_URL" ]; then
    sudo mkdir -p /opt/latino-ext
    sudo curl -fsSL "$VSIX_URL" -o /opt/latino-ext/latino-net.vsix
fi

echo "✅ Instalado: $(latino -v)"
