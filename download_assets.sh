#!/bin/bash
# Script para descargar assets externos del proyecto GameJam
# Ejecutar después de clonar el repo si los assets no están incluidos

set -e

ASSETS_DIR="Assets"
TEMP_DIR="/tmp/gamejam_assets"

echo "=========================================="
echo "GameJam - Descarga de Assets Externos"
echo "=========================================="
echo ""

# Crear directorio temporal
mkdir -p "$TEMP_DIR"

# Función para descargar y extraer
download_and_extract() {
    local name=$1
    local url=$2
    local target_dir=$3
    
    if [ -d "$target_dir" ]; then
        echo "✓ $name ya existe, saltando..."
        return
    fi
    
    echo "Descargando $name..."
    local zip_file="$TEMP_DIR/${name}.zip"
    
    if curl -L -f -o "$zip_file" "$url"; then
        echo "Extrayendo $name..."
        unzip -q "$zip_file" -d "$ASSETS_DIR/"
        rm "$zip_file"
        echo "✓ $name descargado"
    else
        echo "✗ Error descargando $name"
        echo "  URL: $url"
        echo "  Por favor descarga manualmente y coloca en: $target_dir"
    fi
    echo ""
}

# ============================================
# CONFIGURACIÓN DE URLs
# ============================================
# IMPORTANTE: Reemplazar estas URLs con las URLs reales de GitHub Releases
# o enlaces de Google Drive/OneDrive donde estén subidos los assets

# Ejemplo formato GitHub Releases:
# STARTER_ASSETS_URL="https://github.com/RJoel158/GameJam/releases/download/Assets-v1.0/StarterAssets.zip"

# Ejemplo formato Google Drive (link directo de descarga):
# STARTER_ASSETS_URL="https://drive.google.com/uc?export=download&id=XXXXXXXXXXXXX"

STARTER_ASSETS_URL="REEMPLAZAR_CON_URL_REAL"
PRO_SWORD_PACK_URL="REEMPLAZAR_CON_URL_REAL"
POLYTOPE_ASSETS_URL="REEMPLAZAR_CON_URL_REAL"

# ============================================
# Descargar cada pack
# ============================================

echo "Verificando assets requeridos..."
echo ""

# StarterAssets
download_and_extract \
    "StarterAssets" \
    "$STARTER_ASSETS_URL" \
    "$ASSETS_DIR/StarterAssets"

# Pro Sword and Shield Pack
download_and_extract \
    "ProSwordShieldPack" \
    "$PRO_SWORD_PACK_URL" \
    "$ASSETS_DIR/Pro Sword and Shield Pack"

# Polytope Assets (03_UsingPrefabs)
download_and_extract \
    "PolytopeAssets" \
    "$POLYTOPE_ASSETS_URL" \
    "$ASSETS_DIR/03_UsingPrefabs"

# ============================================
# Limpieza
# ============================================

echo ""
echo "Limpiando archivos temporales..."
rm -rf "$TEMP_DIR"

echo ""
echo "=========================================="
echo "Descarga completada!"
echo "=========================================="
echo ""
echo "Próximos pasos:"
echo "1. Abre Unity"
echo "2. Unity detectará e importará los nuevos assets automáticamente"
echo "3. Espera a que termine la importación antes de trabajar"
echo ""
echo "Si algún asset falló, descárgalo manualmente desde:"
echo "  GitHub Releases: https://github.com/RJoel158/GameJam/releases"
echo "  o el enlace compartido por el equipo"
echo ""
