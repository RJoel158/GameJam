#!/bin/bash
# Script para analizar y limpiar assets LFS no usados
# Ejecutar desde la raíz del repo GameJam

set -e

echo "=============================================="
echo "Análisis de Assets LFS - GameJam"
echo "=============================================="
echo ""

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para verificar si existe
check_exists() {
    local path=$1
    if [ -d "$path" ] || [ -f "$path" ]; then
        echo -e "${GREEN}✓${NC} $path"
        return 0
    else
        echo -e "${RED}✗${NC} $path (ya eliminado)"
        return 1
    fi
}

# Función para obtener tamaño
get_folder_size() {
    local path=$1
    if [ -d "$path" ]; then
        du -sh "$path" 2>/dev/null | awk '{print $1}'
    else
        echo "0"
    fi
}

echo "Estado actual de carpetas LFS principales:"
echo ""

echo -e "${YELLOW}🔴 ALTA PRIORIDAD - Eliminar:${NC}"
if check_exists "Assets/StarterAssets"; then
    SIZE=$(get_folder_size "Assets/StarterAssets")
    echo "   Tamaño: $SIZE (58 archivos LFS)"
    echo "   Comando: git rm -r 'Assets/StarterAssets'"
fi

if check_exists "Assets/Pro Sword and Shield Pack Basic Enemy"; then
    SIZE=$(get_folder_size "Assets/Pro Sword and Shield Pack Basic Enemy")
    echo "   Tamaño: $SIZE (52 archivos LFS)"
    echo "   Comando: git rm -r 'Assets/Pro Sword and Shield Pack Basic Enemy'"
fi

echo ""
echo -e "${YELLOW}🟡 REVISAR - Posibles duplicados:${NC}"
if check_exists "Assets/03_UsingPrefabs"; then
    SIZE=$(get_folder_size "Assets/03_UsingPrefabs")
    echo "   Tamaño: $SIZE (20 archivos LFS)"
    echo "   Contiene: Polytope duplicados + Enchanted/Volcanic models"
fi

echo ""
echo -e "${GREEN}🟢 MANTENER - Assets esenciales:${NC}"
check_exists "Assets/AnimatedCharacters"
check_exists "Assets/01_Prefabs"
check_exists "Assets/Pro Sword and Shield Pack"
check_exists "Assets/Scripts"

echo ""
echo "=============================================="
echo "Análisis de escenas"
echo "=============================================="
echo ""

SCENE_FILES=$(find Assets/00_Scenes -name "*.unity" 2>/dev/null)
echo "Escenas encontradas:"
echo "$SCENE_FILES"
echo ""

echo "=============================================="
echo "Recomendaciones"
echo "=============================================="
echo ""

echo -e "${YELLOW}Eliminar ahora (libera ~110 archivos LFS):${NC}"
echo ""
echo "git rm -r 'Assets/StarterAssets'"
echo "git rm -r 'Assets/Pro Sword and Shield Pack Basic Enemy'"
echo "echo 'Assets/StarterAssets/' >> .gitignore"
echo "echo 'Assets/Pro Sword and Shield Pack Basic Enemy/' >> .gitignore"
echo "git commit -m 'Remove unused assets to reduce LFS quota'"
echo "git push"
echo ""
echo -e "${GREEN}Resultado esperado: 264 → ~154 archivos LFS (reducción del 42%)${NC}"
echo ""

# Buscar archivos potencialmente no usados en escenas
echo "=============================================="
echo "Archivos potencialmente no referenciados"
echo "=============================================="
echo ""

if [ -d "Assets/03_UsingPrefabs" ]; then
    echo "Revisar manualmente estos modelos en 03_UsingPrefabs:"
    ls -1 Assets/03_UsingPrefabs/*.fbx 2>/dev/null | head -n 10 || echo "  (ninguno en raíz)"
    ls -1d Assets/03_UsingPrefabs/*/ 2>/dev/null | head -n 5 || echo "  (ninguna carpeta)"
fi

echo ""
echo "Para verificar si un asset se usa en las escenas:"
echo "  grep -r 'NombreDelAsset' Assets/00_Scenes/*.unity"
echo ""
