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
#    separador. Infozip/unzip la interpreta como separador y crea las
#    subcarpetas bien, pero emite un aviso y devuelve codigo 1; se tolera ese
#    1 concretamente. Un 2+ (o 127, unzip ausente) sigue abortando.
#    Python NO es via fiable aqui: en la imagen devcontainers/dotnet python3
#    esta recortado, 'import shutil' falla y zipfile lo necesita en su linea 10.
curl -fsSL "$ZIP_URL" -o "$TMP_DIR/latino.zip"
unzip -q -o "$TMP_DIR/latino.zip" -d "$TMP_DIR/x" || [ $? -le 1 ]

# 3. Localizar el binario (la estructura interna puede variar, p. ej. dist/linux/latino)
BIN="$(find "$TMP_DIR/x" -type f -name latino | head -1)"
[ -n "$BIN" ] || { echo "❌ No se encontró el binario 'latino' dentro del zip"; exit 1; }

# 4. Instalar la carpeta completa (binario + lib/ + nativos) y enlazar en el PATH
sudo rm -rf /opt/latino
sudo mkdir -p /opt/latino
sudo cp -r "$(dirname "$BIN")/." /opt/latino/
# El zip de Linux no guarda modos Unix (atributos externos en 0) y la extraccion
# puede dejar directorios sin permiso de traverse para otros (p. ej. lib/ con
# 754). Sin la 'x' de otros, File.Exists no puede resolver los plugins dentro y
# incluir() falla con "no se encontro el modulo" aunque la ruta sea absoluta.
# Se normalizan: lectura para todos y 'x' solo donde ya era un directorio.
sudo chmod -R a+rX /opt/latino
sudo chmod +x /opt/latino/latino
sudo ln -sf /opt/latino/latino /usr/local/bin/latino

# SQLitePCLRaw resuelve 'e_sqlite3' junto al plugin (lib/), no junto al
# ejecutable: el build coloca el nativo en la raiz y en Linux dlopen no lo
# encuentra desde lib/sqlite.dll. Se enlaza para que la conexion cargue.
if [ -f /opt/latino/libe_sqlite3.so ]; then
    sudo ln -sf /opt/latino/libe_sqlite3.so /opt/latino/lib/libe_sqlite3.so
fi

# Salvaguarda: el LSP busca los plugins en /opt/latino/lib
[ -d /opt/latino/lib ] || echo "⚠️ No existe /opt/latino/lib: los plugins podrían no cargarse (revisar estructura del zip)"

# 5. Dejar el VSIX dentro del workspace. El mecanismo nativo de devcontainer
#    (customizations.vscode.extensions) lo instala al crear el entorno, antes de
#    arrancar el editor: así el tema y el boton Play estan disponibles desde el
#    primer arranque y no hace falta recargar la ventana. La carpeta ya ignora
#    *.vsix en .gitignore, de modo que el binario no ensucia el repo.
VSIX_PATH="${containerWorkspaceFolder:-$PWD}/.devcontainer/latino-net.vsix"
if [ -n "$VSIX_URL" ]; then
    curl -fsSL "$VSIX_URL" -o "$VSIX_PATH"
fi

echo "✅ Instalado: $(latino -v)"
