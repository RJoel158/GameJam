@echo off
REM Script para descargar assets externos del proyecto GameJam (Windows)
REM Ejecutar después de clonar el repo si los assets no están incluidos

setlocal enabledelayedexpansion

echo ==========================================
echo GameJam - Descarga de Assets Externos
echo ==========================================
echo.

set ASSETS_DIR=Assets
set TEMP_DIR=%TEMP%\gamejam_assets

REM Crear directorio temporal
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"

echo Verificando assets requeridos...
echo.

REM ============================================
REM CONFIGURACIÓN DE URLs
REM ============================================
REM IMPORTANTE: Reemplazar estas URLs con las URLs reales

set STARTER_ASSETS_URL=REEMPLAZAR_CON_URL_REAL
set PRO_SWORD_PACK_URL=REEMPLAZAR_CON_URL_REAL
set POLYTOPE_ASSETS_URL=REEMPLAZAR_CON_URL_REAL

REM ============================================
REM Función para descargar (requiere curl en Windows 10+)
REM ============================================

REM StarterAssets
if exist "%ASSETS_DIR%\StarterAssets" (
    echo √ StarterAssets ya existe, saltando...
) else (
    echo Descargando StarterAssets...
    curl -L -o "%TEMP_DIR%\StarterAssets.zip" "%STARTER_ASSETS_URL%"
    if !errorlevel! equ 0 (
        echo Extrayendo StarterAssets...
        powershell -command "Expand-Archive -Path '%TEMP_DIR%\StarterAssets.zip' -DestinationPath '%ASSETS_DIR%\' -Force"
        del "%TEMP_DIR%\StarterAssets.zip"
        echo √ StarterAssets descargado
    ) else (
        echo × Error descargando StarterAssets
    )
)
echo.

REM Pro Sword and Shield Pack
if exist "%ASSETS_DIR%\Pro Sword and Shield Pack" (
    echo √ Pro Sword and Shield Pack ya existe, saltando...
) else (
    echo Descargando Pro Sword and Shield Pack...
    curl -L -o "%TEMP_DIR%\ProSwordPack.zip" "%PRO_SWORD_PACK_URL%"
    if !errorlevel! equ 0 (
        echo Extrayendo Pro Sword and Shield Pack...
        powershell -command "Expand-Archive -Path '%TEMP_DIR%\ProSwordPack.zip' -DestinationPath '%ASSETS_DIR%\' -Force"
        del "%TEMP_DIR%\ProSwordPack.zip"
        echo √ Pro Sword and Shield Pack descargado
    ) else (
        echo × Error descargando Pro Sword Pack
    )
)
echo.

REM Polytope Assets
if exist "%ASSETS_DIR%\03_UsingPrefabs" (
    echo √ Polytope Assets ya existe, saltando...
) else (
    echo Descargando Polytope Assets...
    curl -L -o "%TEMP_DIR%\PolytopeAssets.zip" "%POLYTOPE_ASSETS_URL%"
    if !errorlevel! equ 0 (
        echo Extrayendo Polytope Assets...
        powershell -command "Expand-Archive -Path '%TEMP_DIR%\PolytopeAssets.zip' -DestinationPath '%ASSETS_DIR%\' -Force"
        del "%TEMP_DIR%\PolytopeAssets.zip"
        echo √ Polytope Assets descargado
    ) else (
        echo × Error descargando Polytope Assets
    )
)
echo.

REM ============================================
REM Limpieza
REM ============================================

echo Limpiando archivos temporales...
if exist "%TEMP_DIR%" rmdir /s /q "%TEMP_DIR%"

echo.
echo ==========================================
echo Descarga completada!
echo ==========================================
echo.
echo Próximos pasos:
echo 1. Abre Unity
echo 2. Unity detectará e importará los nuevos assets automáticamente
echo 3. Espera a que termine la importación antes de trabajar
echo.
echo Si algún asset falló, descárgalo manualmente desde:
echo   GitHub Releases: https://github.com/RJoel158/GameJam/releases
echo   o el enlace compartido por el equipo
echo.

pause
