#!/usr/bin/env bash
# Configura Latino.NET dentro del contenedor (postCreateCommand).
# Descarga la última release pública y deja el runtime en /opt/latino.
set -euo pipefail

REPO="FrkL81/Latino.NET"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

CURL=(curl -fsSL --retry 3 --retry-delay 2)

echo "🚀 Configurando Latino.NET..."

# 1. Resolver la última release (con reintento anónimo defensivo)
api_get() {
    local url="$1" tok="${GITHUB_TOKEN:-${CODESPACES_TOKEN:-}}"
    if [ -n "$tok" ] && "${CURL[@]}" -H "Authorization: Bearer ***" "$url"; then
        return 0
    fi
    "${CURL[@]}" "$url"
}
RELEASE_JSON="$(api_get "https://api.github.com/repos/${REPO}/releases/latest")"

ZIP_URL="$(grep -oE 'https://[^"]+/latino-linux[^"/]*\.zip' <<<"$RELEASE_JSON" | head -1 || true)"
VSIX_URL="$(grep -oE 'https://[^"]+/latino-net-vscode[^"/]*\.vsix' <<<"$RELEASE_JSON" | head -1 || true)"
SUMS_URL="$(grep -oE 'https://[^"]+/SHA256SUMS\.txt' <<<"$RELEASE_JSON" | head -1 || true)"
[ -n "$ZIP_URL" ] || { echo "❌ No se encontró el zip de Linux en la última release"; exit 1; }

# 2. Descargar y verificar integridad
"${CURL[@]}" "$ZIP_URL" -o "$TMP_DIR/$(basename "$ZIP_URL")"

if [ -n "$SUMS_URL" ] && "${CURL[@]}" "$SUMS_URL" -o "$TMP_DIR/SHA256SUMS.txt"; then
    ( cd "$TMP_DIR" && sha256sum -c --ignore-missing SHA256SUMS.txt ) \
        || { echo "❌ El zip descargado no coincide con SHA256SUMS.txt"; exit 1; }
else
    echo "⚠️ La release no publica SHA256SUMS.txt; se omite verificación de integridad"
fi

# Descompresión tolerando advertencias de rutas antiguas
unzip -q -o "$TMP_DIR/$(basename "$ZIP_URL")" -d "$TMP_DIR/x" || [ $? -le 1 ]

# 3. Localizar binario
BIN="$(find "$TMP_DIR/x" -type f -name latino | head -1)"
[ -n "$BIN" ] || { echo "❌ No se encontró el binario 'latino' dentro del zip"; exit 1; }

# 4. Instalar carpeta completa en /opt/latino y enlazar
sudo rm -rf /opt/latino
sudo mkdir -p /opt/latino
sudo cp -r "$(dirname "$BIN")/." /opt/latino/
sudo chmod -R a+rX /opt/latino
sudo chmod +x /opt/latino/latino
sudo ln -sf /opt/latino/latino /usr/local/bin/latino

# Enlace defensivo para sqlite en lib/
if [ -f /opt/latino/libe_sqlite3.so ] && [ ! -e /opt/latino/lib/libe_sqlite3.so ]; then
    sudo ln -sf /opt/latino/libe_sqlite3.so /opt/latino/lib/libe_sqlite3.so
fi

[ -d /opt/latino/lib ] || echo "⚠️ Advertencia: No existe /opt/latino/lib"

# 5. Descargar VSIX para el mecanismo nativo de extensiones
VSIX_PATH="${containerWorkspaceFolder:-$PWD}/.devcontainer/latino-net.vsix"
if [ -n "$VSIX_URL" ]; then
    "${CURL[@]}" "$VSIX_URL" -o "$VSIX_PATH"
fi

echo "✅ Instalado: $(latino -v)"
