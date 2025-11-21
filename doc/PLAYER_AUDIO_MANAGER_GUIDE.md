# 🎵 PlayerAudioManager - Guía de Uso

## ✅ ¿Qué es?

**PlayerAudioManager** es un sistema de audio singleton que maneja **todos** los sonidos del jugador de forma centralizada y automática.

---

## 🚀 Configuración Rápida (3 pasos)

### Paso 1: Crear GameObject en la escena

1. En Unity, **click derecho en la Jerarquía** → `Create Empty`
2. Nombra el GameObject: `PlayerAudioManager`
3. **Arrastra el script** `PlayerAudioManager.cs` al GameObject

✅ **¡Listo!** El AudioManager ya está funcionando como singleton.

---

### Paso 2: Configurar los AudioClips en el Inspector

Selecciona el GameObject `PlayerAudioManager` y configura en el Inspector:

#### 🗡️ **EQUIPAMIENTO - Draw & Sheath**
- `Draw Sword Sound`: Sonido al sacar la espada (tecla 1)
- `Sheath Sword Sound`: Sonido al guardar la espada
- `Equipment Volume`: Volumen (0-1, recomendado: 0.8)

#### ⚔️ **COMBATE - Ataques**
- `Attack Sounds`: Array de clips (se reproducen aleatoriamente)
  - Puedes agregar múltiples variaciones de sonidos de espada
- `Attack Volume`: Volumen (0-1, recomendado: 0.7)
- `Attack Sound Mode`:
  - `0` = Random (completamente aleatorio)
  - `1` = Sequential (1, 2, 3... en orden)
  - `2` = Random sin repetir (recomendado)

#### 😖 **DAÑO - Recibir golpes**
- `Hurt Sounds`: Array de quejidos/gritos al recibir daño
- `Hurt Volume`: Volumen (0-1, recomendado: 0.9)

#### 🦶 **MOVIMIENTO - Pasos y Saltos**
- `Footstep Sounds`: Array de sonidos de pasos
  - Se llaman automáticamente desde **Animation Events**
- `Landing Sound`: Sonido al aterrizar después de saltar
- `Footstep Volume`: Volumen (0-1, recomendado: 0.5)

#### 🎵 **AMBIENTE - Música de fondo**
- `Ambient Sound`: Música o sonido ambiental (en loop automático)
- `Ambient Volume`: Volumen (0-1, recomendado: 0.3)

#### 🛡️ **BLOQUEO**
- `Block Sound`: Sonido al bloquear un ataque enemigo
- `Block Volume`: Volumen (0-1, recomendado: 0.8)

---

### Paso 3: ¡Ya funciona automáticamente!

No necesitas hacer nada más. El `ThirdPersonController` ya está integrado y llamará automáticamente:

✅ **Sacar/Guardar espada** → `PlayDrawSword()` / `PlaySheathSword()`  
✅ **Atacar** → `PlayAttackSound()`  
✅ **Recibir daño** → `PlayHurtSound()`  
✅ **Bloquear** → `PlayBlockSound()`  
✅ **Pasos/Aterrizar** → `PlayFootstepSound()` / `PlayLandingSound()`  
✅ **Música ambiental** → Inicia automáticamente en `Start()`

---

## 🎮 Uso Avanzado

### Controlar música ambiental desde otros scripts

```csharp
// Pausar música (ej: durante diálogo)
PlayerAudioManager.Instance.PauseAmbientMusic();

// Reanudar música
PlayerAudioManager.Instance.ResumeAmbientMusic();

// Detener completamente
PlayerAudioManager.Instance.StopAmbientMusic();

// Cambiar volumen
PlayerAudioManager.Instance.SetAmbientVolume(0.5f);
```

### Llamar sonidos manualmente desde otros scripts

```csharp
// Reproducir sonido de ataque desde cualquier lugar
PlayerAudioManager.Instance.PlayAttackSound();

// Reproducir quejido
PlayerAudioManager.Instance.PlayHurtSound();

// Reproducir bloqueo
PlayerAudioManager.Instance.PlayBlockSound();
```

---

## 🔧 Ventajas del Sistema

✅ **Singleton** - Una sola instancia en toda la escena  
✅ **Drag & Drop** - Solo arrastra el componente a un GameObject  
✅ **Automático** - Funciona sin código adicional  
✅ **DontDestroyOnLoad** - Persiste entre escenas  
✅ **Fallback** - Si no está configurado, usa el sistema antiguo  
✅ **Inspector organizado** - Headers y tooltips para fácil configuración  
✅ **Debug logs** - Mensajes en consola para verificar reproducción  

---

## 📋 Checklist de Configuración

- [ ] GameObject `PlayerAudioManager` creado en escena
- [ ] Script `PlayerAudioManager.cs` agregado al GameObject
- [ ] AudioClips asignados en el Inspector:
  - [ ] Draw Sword Sound
  - [ ] Sheath Sword Sound
  - [ ] Attack Sounds (array con al menos 1 clip)
  - [ ] Hurt Sounds (array con al menos 1 clip)
  - [ ] Footstep Sounds (array con al menos 1 clip)
  - [ ] Landing Sound
  - [ ] Ambient Sound (opcional)
  - [ ] Block Sound
- [ ] Volúmenes ajustados a tu gusto
- [ ] Probado en Play mode

---

## ⚠️ Troubleshooting

### No se escucha ningún sonido
1. Verifica que el GameObject `PlayerAudioManager` existe en la escena
2. Verifica que los AudioClips están asignados en el Inspector
3. Chequea que los volúmenes no están en 0
4. Revisa la consola por mensajes `[Audio]`

### Los pasos no suenan
- Los pasos se activan por **Animation Events** en las animaciones
- Verifica que las animaciones de caminar/correr tienen eventos `OnFootstep`
- Si no los tienen, agrégalos en el Animation Window

### "PlayerAudioManager does not exist in the current context"
- Unity está compilando. Espera unos segundos.
- Si persiste: `Edit → Preferences → External Tools → Regenerate project files`

---

## 🎯 Ejemplo de Configuración Recomendada

```
PlayerAudioManager
├─ Draw Sword Sound: sword_draw.wav
├─ Sheath Sword Sound: sword_sheath.wav
├─ Attack Sounds [3]:
│  ├─ sword_swing_1.wav
│  ├─ sword_swing_2.wav
│  └─ sword_swing_3.wav
├─ Hurt Sounds [4]:
│  ├─ hurt_1.wav
│  ├─ hurt_2.wav
│  ├─ hurt_3.wav
│  └─ hurt_4.wav
├─ Footstep Sounds [6]:
│  ├─ step_1.wav
│  ├─ step_2.wav
│  ├─ step_3.wav
│  ├─ step_4.wav
│  ├─ step_5.wav
│  └─ step_6.wav
├─ Landing Sound: landing.wav
├─ Ambient Sound: ambient_music.wav
└─ Block Sound: shield_block.wav
```

---

## 🎨 Notas Importantes

- **NO necesitas AudioSource** en el jugador - el AudioManager los crea automáticamente
- **Persiste entre escenas** - No lo destruyas ni lo dupliques
- **Compatibilidad** - Funciona con el sistema antiguo como fallback
- **Performance** - Usa `PlayClipAtPoint` para pasos (3D spatial audio)
- **Debugging** - Logs con emojis para identificar fácilmente en consola

---

**¡Disfruta de tu sistema de audio profesional!** 🎵✨
