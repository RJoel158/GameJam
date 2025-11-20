# 🎵 AudioManager - Configuración y Uso

## ✅ ¿Qué se ha hecho?

Se ha separado completamente la lógica de audio del `ThirdPersonController` a un nuevo script `AudioManager.cs` usando el patrón **Singleton**.

### Archivos creados/modificados:

1. **AudioManager.cs** - Nuevo script singleton para manejar todo el audio
2. **ThirdPersonController.cs** - Refactorizado para usar AudioManager

---

## 🚀 Configuración en Unity

### Paso 1: Crear GameObject AudioManager

1. En la Jerarquía, crea un **GameObject vacío**
2. Nómbralo: `AudioManager`
3. Agrega el componente **AudioManager.cs**

### Paso 2: Configurar AudioManager en el Inspector

#### **Draw & Sheath Sounds**

- `Draw Sound`: AudioClip para sacar la espada (tecla 1)
- `Draw Sound Volume`: Volumen (0-1)
- `Sheath Sound`: AudioClip para guardar la espada
- `Sheath Sound Volume`: Volumen (0-1)
- `Draw Sword Sound`: (Legacy) Fallback si Draw/Sheath están vacíos

#### **Ambient Sound**

- `Ambient Sound`: AudioClip que sonará en loop constantemente
- `Ambient Sound Volume`: Volumen (0-1)

#### **Attack Sounds (F Key)**

- `Attack Sound Clips`: Array de clips de audio para ataques
- `Attack Sound Volume`: Volumen (0-1)
- `Sound Play Mode`:
  - `0` = **Random**: Aleatorio cada vez
  - `1` = **Sequential**: En orden 1, 2, 3...
  - `2` = **RandomNoRepeat**: Aleatorio sin repetir el anterior

#### **Footstep Sounds**

- `Landing Audio Clip`: Sonido al aterrizar
- `Footstep Audio Clips`: Array de sonidos de pasos
- `Footstep Audio Volume`: Volumen (0-1)

---

## 📋 Migración de datos del ThirdPersonController

Si ya tenías audio configurado en tu **ThirdPersonController**, necesitas:

### ⚠️ ANTES de guardar la escena:

1. **Anota o toma screenshot** de todos los AudioClips asignados en ThirdPersonController
2. Los campos que DESAPARECERÁN del ThirdPersonController:
   - `Landing Audio Clip`
   - `Footstep Audio Clips`
   - `Footstep Audio Volume`
   - `Draw Sword Sound`
   - `Draw Sword Volume`
   - `Draw Sound`
   - `Draw Sound Volume`
   - `Sheath Sound`
   - `Sheath Sound Volume`
   - `Ambient Sound`
   - `Ambient Sound Volume`
   - `F Sound Clips` → ahora `Attack Sound Clips`
   - `F Sound Volume` → ahora `Attack Sound Volume`
   - `Sound Play Mode`

### ✅ DESPUÉS de crear AudioManager:

1. **Arrastra los AudioClips** desde tu referencia/screenshot al AudioManager
2. **Configura los volúmenes** igual que antes
3. **Guarda la escena**

---

## 🎮 Cómo funciona ahora

### En el código:

```csharp
// ANTES (en ThirdPersonController)
PlayDrawSound();

// AHORA (usa singleton)
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayDrawSound();
```

### Métodos disponibles del AudioManager:

```csharp
// Sonidos de equipamiento
AudioManager.Instance.PlayDrawSound();
AudioManager.Instance.PlaySheathSound();

// Sonidos de combate
AudioManager.Instance.PlayAttackSound();

// Sonidos ambientales
AudioManager.Instance.StopAmbientSound();
AudioManager.Instance.PauseAmbientSound();
AudioManager.Instance.ResumeAmbientSound();

// Sonidos de movimiento (usados automáticamente por Animation Events)
AudioManager.Instance.PlayFootstepSound(position);
AudioManager.Instance.PlayLandingSound(position);
```

---

## 🔧 Ventajas de esta separación

✅ **Código más limpio** - ThirdPersonController solo maneja movimiento/combate  
✅ **Reutilizable** - Puedes usar AudioManager desde cualquier script  
✅ **Singleton** - Una sola instancia global en toda la escena  
✅ **No se destruye** - `DontDestroyOnLoad` mantiene el audio entre escenas  
✅ **Cooldowns automáticos** - Previene spam de sonidos (1 segundo)  
✅ **Mejor organización** - Todo el audio en un solo lugar

---

## ⚠️ Solución de problemas

### "AudioManager' does not exist in the current context"

- **Unity está compilando**. Espera unos segundos.
- Si persiste, ve a `Edit → Preferences → External Tools → Regenerate project files`

### No se escucha ningún audio

1. Verifica que **AudioManager GameObject existe** en la escena
2. Chequea que los **AudioClips están asignados** en el Inspector
3. Verifica que el **volumen no está en 0**
4. Revisa la consola por mensajes de `[AudioManager]`

### El sonido ambiental no hace loop

- Asegúrate de asignar un AudioClip en `Ambient Sound`
- El loop se configura automáticamente en el código

### Los pasos no suenan

- Los pasos se activan por **Animation Events** en las animaciones de caminar/correr
- Verifica que las animaciones tengan los eventos `OnFootstep` y `OnLand`

---

## 📝 Notas importantes

- El **AudioManager se crea automáticamente** la primera vez que se ejecuta
- Usa **2 AudioSources**:
  - Uno para efectos (Draw, Sheath, Attack)
  - Uno dedicado para el ambiente (loop)
- Los sonidos de pasos usan `AudioSource.PlayClipAtPoint` (3D spatial)
- Todos los demás son **2D** (no espaciales)

---

## 🎯 Próximos pasos recomendados

1. **Configurar el AudioManager** en la escena principal
2. **Migrar los AudioClips** del ThirdPersonController al AudioManager
3. **Probar todos los sonidos** en Play mode
4. **Ajustar volúmenes** según necesites

---

**¿Dudas?** Revisa la consola - el AudioManager loggea todos los eventos de audio con colores:

- `[AudioManager] ✓` = Éxito
- `[AudioManager] ⚠️` = Advertencia
- `[AudioManager] ❌` = Error
