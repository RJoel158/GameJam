# Solución al Problema de Cuota Git LFS

## Problema Actual
El repositorio tiene **292 archivos** gestionados por Git LFS (principalmente *.fbx, *.png, *.tif, *.asset) y la cuota del servidor remoto está excedida, impidiendo que el equipo pueda hacer `git pull` o `git switch` entre ramas.

## Archivos LFS en el Repo
- **Total**: 292 archivos
- **Patrones LFS**: *.png, *.tif, *.asset, *.fbx
- **Carpetas principales con más archivos LFS**:
  - `Assets/StarterAssets` — ~58 archivos
  - `Assets/Pro Sword and Shield Pack Basic Enemy` — ~52 archivos
  - `Assets/Pro Sword and Shield Pack` — ~52 archivos
  - `Assets/03_UsingPrefabs` — ~48 archivos
  - `Assets/01_Prefabs` — ~11 archivos
  - `Assets/AnimatedCharacters` — ~8 archivos
  - `ProjectSettings/` — ~30 archivos .asset

## Soluciones Disponibles

### Opción 1: Aumentar Cuota LFS (Recomendada - Más Rápida)

**Para GitHub:**
1. El propietario del repositorio debe ir a Settings → Billing → Git LFS Data
2. Comprar paquetes de datos adicionales (Data packs de 50GB por ~$5/mes)
3. Una vez actualizada la cuota, todos podrán hacer pull/push normalmente

**Para GitLab:**
1. Ir a Settings → Usage Quotas → Storage
2. Aumentar límites de almacenamiento según el plan
3. O considerar GitLab self-hosted si el proyecto es grande

**Ventajas:**
- Solución inmediata (minutos)
- No requiere cambios en el repo ni en el flujo de trabajo
- Todo sigue funcionando igual

**Desventajas:**
- Costo mensual
- No resuelve si el repo seguirá creciendo indefinidamente

---

### Opción 2: Migrar Assets Grandes a Almacenamiento Externo

#### Paso A: Identificar y Extraer Assets Pesados
Los packs de assets más grandes que se pueden externalizar:
- `Assets/StarterAssets` (58 archivos)
- `Assets/Pro Sword and Shield Pack` + `Basic Enemy` (104 archivos)
- `Assets/03_UsingPrefabs` (48 archivos - duplicados de Polytope)

#### Paso B: Subir a GitHub Releases o Drive
1. **Crear archivo zip de los packs:**
   ```bash
   cd Assets
   zip -r StarterAssets.zip StarterAssets/
   zip -r ProSwordShieldPack.zip "Pro Sword and Shield Pack/" "Pro Sword and Shield Pack Basic Enemy/"
   zip -r PolytopeAssets.zip 03_UsingPrefabs/
   ```

2. **Subir a GitHub Releases:**
   - En GitHub: ir a Releases → Draft a new release
   - Crear release "Assets-v1.0" y adjuntar los .zip
   - Copiar URLs de descarga

3. **O subir a Google Drive/OneDrive:**
   - Subir los .zip
   - Generar links públicos o compartidos con el equipo

#### Paso C: Crear Script de Descarga
Crear `download_assets.sh` en la raíz del repo:

```bash
#!/bin/bash
# Script para descargar assets externos

ASSETS_DIR="Assets"

echo "Descargando assets externos..."

# Descargar desde GitHub Releases (reemplazar URLs)
if [ ! -d "$ASSETS_DIR/StarterAssets" ]; then
    echo "Descargando StarterAssets..."
    curl -L -o /tmp/starter.zip "https://github.com/RJoel158/GameJam/releases/download/Assets-v1.0/StarterAssets.zip"
    unzip -q /tmp/starter.zip -d "$ASSETS_DIR/"
    rm /tmp/starter.zip
fi

if [ ! -d "$ASSETS_DIR/Pro Sword and Shield Pack" ]; then
    echo "Descargando Pro Sword and Shield Pack..."
    curl -L -o /tmp/sword.zip "https://github.com/RJoel158/GameJam/releases/download/Assets-v1.0/ProSwordShieldPack.zip"
    unzip -q /tmp/sword.zip -d "$ASSETS_DIR/"
    rm /tmp/sword.zip
fi

echo "Assets descargados. Ejecuta Unity para que importe los archivos."
```

#### Paso D: Remover del Repo y LFS
```bash
# 1. Hacer backup primero
git clone <repo-url> backup_repo

# 2. Remover carpetas del tracking
git rm -r "Assets/StarterAssets"
git rm -r "Assets/Pro Sword and Shield Pack"
git rm -r "Assets/Pro Sword and Shield Pack Basic Enemy"
git rm -r "Assets/03_UsingPrefabs"

# 3. Actualizar .gitignore
echo "Assets/StarterAssets/" >> .gitignore
echo "Assets/Pro Sword and Shield Pack/" >> .gitignore
echo "Assets/Pro Sword and Shield Pack Basic Enemy/" >> .gitignore
echo "Assets/03_UsingPrefabs/" >> .gitignore

# 4. Commit
git add .gitignore
git commit -m "Move large asset packs to external storage"

# 5. Push
git push origin main
```

#### Paso E: Limpiar Historial LFS (Opcional - Libera Cuota)
⚠️ **ADVERTENCIA**: Esto reescribe el historial. Todos deben reclonar después.

```bash
# Usando git-filter-repo (instalarlo primero: pip install git-filter-repo)
git filter-repo --path "Assets/StarterAssets" --invert-paths --force
git filter-repo --path "Assets/Pro Sword and Shield Pack" --invert-paths --force
git filter-repo --path "Assets/03_UsingPrefabs" --invert-paths --force

# Force push
git push origin --force --all
```

**Nota**: En GitHub, después de esto contactar a soporte para que limpien objetos LFS huérfanos y liberen la cuota.

---

### Opción 3: Usar Git Submodules para Assets

#### Crear Repo Separado para Assets
1. **Crear nuevo repo: `GameJam-Assets`**
   ```bash
   # En GitHub/GitLab crear repo vacío GameJam-Assets
   
   # Clonar y mover assets
   git clone <GameJam-Assets-url>
   cd GameJam-Assets
   mkdir -p Assets
   
   # Copiar desde repo principal
   cp -r ../GameJam/Assets/StarterAssets Assets/
   cp -r ../GameJam/Assets/"Pro Sword and Shield Pack" Assets/
   # ... etc
   
   git add .
   git commit -m "Initial asset pack import"
   git push
   ```

2. **En repo principal, añadir como submodule:**
   ```bash
   cd GameJam
   
   # Remover carpetas actuales
   git rm -r "Assets/StarterAssets"
   # ... etc
   
   # Añadir submodule
   git submodule add <GameJam-Assets-url> Assets/ExternalAssets
   
   git commit -m "Convert large assets to submodule"
   git push
   ```

3. **Equipo clona con submodules:**
   ```bash
   git clone --recurse-submodules <repo-url>
   
   # O si ya tienen el repo:
   git submodule update --init --recursive
   ```

**Ventaja**: Assets en repo separado con su propia cuota LFS.

---

## Solución de Emergencia (Temporal)

Si necesitan trabajar **YA** mientras deciden la solución permanente:

### Para cada desarrollador:

1. **Configurar skip-smudge** (evita descargar LFS automáticamente):
   ```bash
   git config lfs.skipSmudge true
   ```

2. **Hacer pull/switch sin problemas:**
   ```bash
   git pull origin main
   git switch otra-rama
   ```

3. **Descargar SOLO los assets que necesiten trabajar:**
   ```bash
   # Ejemplo: solo necesito trabajar con Pro Sword Pack
   git lfs fetch --include="Assets/Pro Sword and Shield Pack/**"
   git lfs checkout "Assets/Pro Sword and Shield Pack/**"
   
   # O todo de Assets/Scripts (que son pequeños):
   git lfs fetch --include="Assets/Scripts/**"
   git lfs checkout "Assets/Scripts/**"
   ```

4. **Para volver a descargar todo** (cuando se resuelva la cuota):
   ```bash
   git config lfs.skipSmudge false
   git lfs fetch --all
   git lfs checkout
   ```

---

## Recomendación Final

**Para este proyecto:**

1. **Inmediato (hoy)**: 
   - Propietario compra 1-2 packs de datos LFS adicionales en GitHub (50-100GB, ~$5-10/mes)
   - Todos hacen `git lfs pull` y siguen trabajando normalmente

2. **A corto plazo (próxima semana)**:
   - Evaluar si los packs `StarterAssets` y `Pro Sword Shield` se modifican frecuentemente
   - Si NO se modifican, moverlos a GitHub Releases con script de descarga
   - Si SÍ se modifican, mantenerlos en LFS con cuota aumentada

3. **A medio plazo**:
   - Implementar política: nuevos assets grandes (>10MB) van a Releases o Drive, no a LFS
   - Documentar qué assets están externalizados y cómo descargarlos

---

## Comandos de Utilidad

### Ver cuota LFS actual
```bash
git lfs status
curl -H "Authorization: token YOUR_GITHUB_TOKEN" \
  https://api.github.com/repos/RJoel158/GameJam | grep lfs
```

### Listar archivos LFS locales
```bash
git lfs ls-files
```

### Ver tamaño total de LFS en el repo
```bash
git lfs ls-files -l | awk '{sum+=$1} END {print "Total:", sum/1024/1024, "MB"}'
```

### Limpiar caché LFS local
```bash
git lfs prune
```

---

## Contacto y Ayuda

- **Documentación Git LFS**: https://git-lfs.github.com/
- **GitHub LFS Pricing**: https://docs.github.com/en/billing/managing-billing-for-git-large-file-storage
- **GitLab LFS**: https://docs.gitlab.com/ee/topics/git/lfs/

---

## Notas para el Equipo

- El archivo `lfs_files.txt` en la raíz contiene la lista completa de 292 archivos LFS
- Usar `cat lfs_files.txt | grep "ruta_especifica"` para buscar archivos concretos
- Coordinar con el equipo antes de ejecutar operaciones que reescriban historial (filter-repo)
