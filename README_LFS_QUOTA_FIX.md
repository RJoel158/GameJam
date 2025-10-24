# Solución al Problema de Cuota Git LFS

## Problema Actual

El repositorio tiene **264 archivos** gestionados por Git LFS (principalmente _.fbx, _.png, _.tif, _.asset) y la cuota del servidor remoto está excedida, impidiendo que el equipo pueda hacer `git pull` o `git switch` entre ramas.

## Diagnóstico Actualizado (Post-Limpieza)

- **Total**: 264 archivos (-28 eliminados recientemente ✓)
- **Patrones LFS**: _.png, _.tif, _.asset, _.fbx
- **Carpetas principales con más archivos LFS**:
  - `Assets/StarterAssets` — 58 archivos (Environment primitives, textures, UI icons)
  - `Assets/Pro Sword and Shield Pack Basic Enemy` — 52 archivos (Animaciones)
  - `Assets/Pro Sword and Shield Pack` — 52 archivos (Animaciones)
  - `Assets/03_UsingPrefabs` — 20 archivos (Polytope duplicados + Volcanic/Enchanted models)
  - `Assets/01_Prefabs` — 11 archivos (Polytope Studio originales)
  - `Assets/AnimatedCharacters` — 8 archivos (Knight animations usados en el juego)
  - `Assets/AllSkyFree` — 7 archivos (Skybox textures)
  - `Assets/Settings` — 7 archivos (Unity pipeline assets)
  - `Assets/TextMesh Pro` — 6 archivos (Fonts & sprites)
  - `ProjectSettings/` — ~32 archivos .asset

## ✂️ Assets Seguros para Eliminar (Recomendaciones)

### 🔴 ALTA PRIORIDAD - Eliminar Ya (ahorra ~110 archivos LFS)

1. **`Assets/StarterAssets` (58 archivos)** 🎯 MAYOR IMPACTO

   - ¿Qué es?: Pack de ejemplo de Unity (cajas, rampas, grids de prueba)
   - ¿Lo usan?: NO aparece en la escena principal `OpenWorldSceneEdu.unity`
   - **Acción**: Eliminar completamente — solo es para demos/testing
   - **Comando**:
     ```bash
     git rm -r "Assets/StarterAssets"
     echo "Assets/StarterAssets/" >> .gitignore
     ```

2. **`Assets/Pro Sword and Shield Pack Basic Enemy` (52 archivos)** 🎯
   - ¿Qué es?: Animaciones duplicadas del pack principal
   - ¿Lo usan?: Tienen el mismo pack sin "Basic Enemy"
   - **Acción**: Si solo usan UN set de animaciones, eliminar el duplicado
   - **Verificar primero**: revisar qué animaciones usa el enemigo en las escenas
   - **Comando**:
     ```bash
     git rm -r "Assets/Pro Sword and Shield Pack Basic Enemy"
     echo "Assets/Pro Sword and Shield Pack Basic Enemy/" >> .gitignore
     ```

### 🟡 MEDIA PRIORIDAD - Revisar y Limpiar (ahorra ~15 archivos)

3. **`Assets/03_UsingPrefabs/Polytope Studio` (7 archivos) - DUPLICADOS CONFIRMADOS** ✂️

   - ¿Qué es?: Copia EXACTA de meshes/texturas de Polytope que ya están en `01_Prefabs`
   - **Archivos duplicados**:
     - `PT_Grass_02.fbx` (duplicado)
     - `PT_Menhir_Rock_02.fbx` (duplicado)
     - `PT_Ore_Rock_01.fbx` (duplicado)
     - `PT_Generic_Shrub_01_dead.fbx` (duplicado)
     - `PT_Fruit_Tree_01_dead.fbx` (duplicado)
     - `PT_Fruit_Tree_01_dead_cut.fbx` (duplicado)
     - `PT_Fruit_Tree_01_stump.fbx` (duplicado)
     - `PT_Grass_High_03.png` (textura duplicada)
     - `PT_Ground_Generic_03.png` (textura duplicada)
   - ✅ **Acción**: ELIMINAR — están duplicados en `01_Prefabs`
   - **Comando**:
     ```bash
     git rm -r "Assets/03_UsingPrefabs/Polytope Studio"
     ```

4. **`Assets/03_UsingPrefabs/Enchanted_Luminance_*` (5 archivos) - USADO EN ESCENA** ⚠️

   - ¿Qué es?: Modelo 3D (parece castillo/estructura mágica)
   - ✅ **SE USA**: Aparece en `OpenWorldSceneMerged.unity`
   - **Acción**: ❌ NO ELIMINAR (se usa en el juego)

5. **`Assets/03_UsingPrefabs/Volcanic_Fortress_*` (5 archivos) - USADO EN ESCENA** ⚠️

   - ¿Qué es?: Modelo 3D (fortaleza volcánica)
   - ✅ **SE USA**: Aparece en `OpenWorldSceneMerged.unity`
   - **Acción**: ❌ NO ELIMINAR (se usa en el juego)

6. **`Assets/03_UsingPrefabs/PT_Grass_03.png` (1 archivo) - DUPLICADO** ✂️

   - ¿Qué es?: Textura de grass (391KB)
   - Duplicado de `01_Prefabs/Polytope Studio/.../PT_Grass_03.png`
   - ✅ **Acción**: ELIMINAR si no está en prefabs únicos de 03_UsingPrefabs
   - **Comando**:
     ```bash
     git rm "Assets/03_UsingPrefabs/PT_Grass_03.png"
     ```

7. **`Assets/AllSkyFree` (7 archivos)**
   - ¿Qué es?: Pack de skyboxes (Cold Night)
   - ¿Lo usan?: Si solo usan 1 skybox, pueden eliminar el resto
   - **Acción**: Revisar cuál skybox usa la escena y eliminar los demás

### 🟢 MANTENER - Assets Esenciales

✅ **`Assets/AnimatedCharacters` (8 archivos)** — Animaciones del knight player
✅ **`Assets/01_Prefabs` (11 archivos)** — Polytope assets usados en el mundo
✅ **`Assets/Pro Sword and Shield Pack` (52 archivos)** — SI es el pack principal usado
✅ **`Assets/TextMesh Pro` (6 archivos)** — UI text system
✅ **`Assets/Settings` (7 archivos)** — Configuración Unity pipeline
✅ **`ProjectSettings/` (32 archivos)** — Configuración proyecto Unity
✅ **Archivos raíz** (8 archivos): knight models, sword, shield, club — usados en juego

## Soluciones Disponibles

### 🚀 Plan de Acción Recomendado (Limpieza Segura)

**Fase 1: Eliminar Duplicados en 03_UsingPrefabs (GRATIS - reduce ~10 archivos)**

```bash
# Eliminar Polytope duplicados (7 archivos LFS)
git rm -r "Assets/03_UsingPrefabs/Polytope Studio"

# Eliminar textura duplicada
git rm "Assets/03_UsingPrefabs/PT_Grass_03.png"

# Commit
git commit -m "Remove duplicate Polytope assets from 03_UsingPrefabs"
git push origin merge-openWorld-PlayerMovement
```

**Resultado**: 264 → ~256 archivos LFS

**Fase 2: Eliminar Pack Duplicado de Animaciones (reduce ~52 archivos)**

```bash
# SOLO si confirmas que el enemigo NO usa estas animaciones
git rm -r "Assets/Pro Sword and Shield Pack Basic Enemy"
echo "Assets/Pro Sword and Shield Pack Basic Enemy/" >> .gitignore

git commit -m "Remove duplicate enemy animation pack"
git push origin merge-openWorld-PlayerMovement
```

**Resultado**: ~256 → ~204 archivos LFS

**⚠️ NO ELIMINAR:**

- `Assets/StarterAssets` — contiene ThirdPersonController que USAN
- `Assets/03_UsingPrefabs/Enchanted_Luminance_*` — usado en OpenWorldSceneMerged
- `Assets/03_UsingPrefabs/Volcanic_Fortress_*` — usado en OpenWorldSceneMerged

**Fase 3: Si aún necesitan más espacio**

- Opción A: Comprar 1 pack LFS adicional (50GB/$5/mes) — todos trabajan normalmente
- Opción B: Mover packs de animaciones a almacenamiento externo (ver abajo)

---

### Opción 1: Aumentar Cuota LFS (Más Rápida después de limpieza)

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

**Assets recomendados para externalizar (después de eliminar StarterAssets):**

- ~~`Assets/StarterAssets` (58 archivos)~~ — ELIMINAR directamente, no externalizar
- `Assets/Pro Sword and Shield Pack` (52 archivos) — SI se usa poco, externalizar
- ~~`Assets/Pro Sword and Shield Pack Basic Enemy`~~ — ELIMINAR si es duplicado
- `Assets/03_UsingPrefabs` (20 archivos restantes) — Revisar y limpiar

#### Paso B: Subir a GitHub Releases o Drive

1. **Crear archivo zip de los packs que SÍ se usan:**

   ```bash
   cd Assets
   # Solo si deciden externalizar animaciones que se usan
   zip -r ProSwordShieldPack.zip "Pro Sword and Shield Pack/"
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

### Ejecutar análisis automatizado

```bash
./analyze_lfs.sh
```

---

## 🎯 Resumen Ejecutivo - Estado Actual

**Después del escaneo actualizado:**

- **Total archivos LFS**: 264 (-28 desde el escaneo anterior ✓)
- **Espacio en disco**: ~338 MB total
  - StarterAssets: 86 MB (58 archivos) 🔴 ELIMINAR
  - 03_UsingPrefabs: 243 MB (20 archivos) 🟡 REVISAR
  - Pro Sword Pack Basic Enemy: 9.4 MB (52 archivos) 🔴 ELIMINAR
  - Otros esenciales: ~100 MB (mantener)

**Acción inmediata recomendada:**

```bash
# Copiar y pegar estos comandos (liberan 110 archivos LFS):
git rm -r 'Assets/StarterAssets'
git rm -r 'Assets/Pro Sword and Shield Pack Basic Enemy'
echo 'Assets/StarterAssets/' >> .gitignore
echo 'Assets/Pro Sword and Shield Pack Basic Enemy/' >> .gitignore
git commit -m 'Remove unused assets to reduce LFS quota'
git push origin merge-openWorld-PlayerMovement
```

**Resultado esperado**: 264 → ~154 archivos LFS (**reducción del 42%**)

**Después de la limpieza:**

- Si aún hay problemas de cuota → Comprar 1 pack LFS ($5/mes)
- O externalizar `Assets/Pro Sword and Shield Pack` a GitHub Releases

---

## Contacto y Ayuda

- **Documentación Git LFS**: https://git-lfs.github.com/
- **GitHub LFS Pricing**: https://docs.github.com/en/billing/managing-billing-for-git-large-file-storage
- **GitLab LFS**: https://docs.gitlab.com/ee/topics/git/lfs/

---

## Notas para el Equipo

- El archivo `lfs_files_new.txt` en la raíz contiene la lista actualizada de 264 archivos LFS
- Usar `cat lfs_files_new.txt | grep "ruta_especifica"` para buscar archivos concretos
- Script `analyze_lfs.sh` ejecuta análisis automático y muestra recomendaciones
- Coordinar con el equipo antes de ejecutar operaciones que reescriban historial (filter-repo)
- **Escenas del proyecto**: `OpenWorldSceneEdu.unity` y `OpenWorldSceneMerged.unity`
