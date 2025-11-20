# 🚀 SETUP RÁPIDO DE CINEMÁTICA DEL BOSS - 2 MÉTODOS

Ahora tienes **2 formas súper fáciles** de configurar la cinemática del boss:

---

## ⚡ MÉTODO 1: SETUP ULTRA RÁPIDO (RECOMENDADO PARA EMPEZAR)

Este método crea solo lo básico: el boss aparece después de la misión, **sin cinemática** (la puedes agregar después).

### Pasos:

1. **En Unity**, crea un GameObject vacío en la escena:

   - Right-click en Hierarchy > Create Empty
   - Nómbralo: `SetupHelper`

2. **Agrega el script** `QuickBossCinematicSetup.cs`:

   - Selecciona `SetupHelper`
   - Add Component > Quick Boss Cinematic Setup

3. **Configura en el Inspector**:

   - `Boss Prefab`: Arrastra tu prefab del boss
   - `Boss Spawn Position`: Ajusta la posición donde aparecerá (ej: 0, 0, 50)
   - (Opcional) Agrega AudioClips si los tienes

4. **Click derecho en el script** en el Inspector:
   - Selecciona: **🚀 SETUP COMPLETO (1 CLICK)**
5. **¡LISTO!** 🎉

### Resultado:

- ✅ Boss aparece cuando completes la misión
- ✅ Se spawns en la posición que definiste
- ⏭️ Sin cinemática (puedes agregar después con el Método 2)

### Comandos Útiles (click derecho en el script):

- **🧪 TEST: Forzar Spawn del Boss** - Testear sin completar misión
- **ℹ️ Mostrar Info del Setup** - Ver estado de la configuración
- **🗑️ Limpiar Setup** - Borrar todo y empezar de nuevo

---

## 🎬 MÉTODO 2: SETUP COMPLETO CON CINEMÁTICA (AVANZADO)

Este método crea **TODO**: Timeline, cámaras Cinemachine, UI, eventos, etc.

### Pasos:

1. **En Unity**, ve al menú:

   ```
   Tools > Boss Cinematic Setup Wizard
   ```

2. **Se abrirá una ventana**. Configura:

   - **Boss Prefab**: Arrastra tu boss prefab
   - **Boss Spawn Position**: Posición donde aparecerá
   - **Player GameObject**: Se detecta automáticamente (o arrástralo)
   - **Cinematic Duration**: Duración de la cinemática (default: 12 segundos)
   - ✅ Todas las casillas marcadas

3. **Audio Opcional**:

   - Boss Roar Sound
   - Ground Impact Sound
   - Dramatic Music

4. **Click en el botón verde**:

   ```
   🚀 CREAR SETUP COMPLETO
   ```

5. **¡LISTO!** Se creará automáticamente:
   - ✅ BossSpawner
   - ✅ BossCinematicTimeline
   - ✅ 4 Cámaras Cinemachine
   - ✅ Letterbox UI (barras negras)
   - ✅ Timeline Signals

### Siguiente paso después del wizard:

1. Selecciona `BossCinematicTimeline` en la Hierarchy
2. Abre **Window > Sequencing > Timeline**
3. Verás el timeline vacío con las pistas creadas
4. **Arrastra las cámaras** al track "Cinemachine Track":

   - Busca en Hierarchy: `BossCinematicCameras`
   - Arrastra cada cámara al timeline en diferentes tiempos:
     ```
     0s-3s:   CM_BossIntro_Wide
     3s-5s:   CM_BossIntro_CloseUp
     5s-7s:   CM_BossIntro_Dramatic
     7s-9s:   CM_BossIntro_PlayerReaction
     ```

5. **Agrega Signal Emitters** en el track "Events":
   - Click derecho en el track > Add Signal Emitter
   - En el segundo 2: Conecta a `BossCinematicEvents.SpawnBoss()`
   - En el segundo 4: Conecta a `BossCinematicEvents.PlayBossRoar()`
   - etc.

---

## 🔀 ¿CUÁL MÉTODO USAR?

### Usa **MÉTODO 1** si:

- ✅ Quieres probar rápido
- ✅ No necesitas cinemática aún
- ✅ Quieres configurar manualmente después
- ✅ Eres principiante con Unity Timeline

### Usa **MÉTODO 2** si:

- ✅ Quieres la cinemática épica completa
- ✅ Ya conoces Unity Timeline
- ✅ Quieres todo configurado de una vez
- ✅ Vas a pulir la presentación del boss

---

## 🧪 TESTING

### Probar sin completar la misión:

**Método 1:**

1. Click derecho en `QuickBossCinematicSetup` script
2. Selecciona: **🧪 TEST: Forzar Spawn del Boss**

**Método 2:**

1. Encuentra `BossSpawner` en Hierarchy
2. En el Inspector, click derecho en `BossSpawner` script
3. Selecciona: **Force Spawn Boss**

### Probar el Timeline solo:

1. Selecciona `BossCinematicTimeline`
2. Abre Window > Sequencing > Timeline
3. Click en Play ▶️ en la ventana de Timeline
4. Observa la cinemática en la Scene view

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### "Boss Prefab no asignado"

- Asegúrate de arrastrar el prefab del boss en el campo correspondiente

### "MissionManager no encontrado"

- Verifica que tengas el MissionManager en tu escena
- El BossSpawner se conectará automáticamente cuando esté presente

### "El boss no aparece al completar la misión"

- Abre la Console (Ctrl+Shift+C) y busca logs
- Verifica que el MissionManager esté en la escena
- Usa "Mostrar Info del Setup" para ver el estado

### "Las cámaras no cambian"

- Verifica que la Main Camera tenga un Cinemachine Brain
- Asegúrate de que las cámaras estén en el Cinemachine Track del Timeline

### "Quiero empezar de nuevo"

- **Método 1**: Click derecho > **🗑️ Limpiar Setup**
- **Método 2**: En el Wizard, click en **🗑️ Remove All Boss Cinematic Objects**

---

## 📊 COMPARACIÓN DE MÉTODOS

| Característica       | Método 1 (Rápido) | Método 2 (Completo) |
| -------------------- | ----------------- | ------------------- |
| Boss Spawn           | ✅                | ✅                  |
| Cinemática           | ❌                | ✅                  |
| Cámaras Cinemachine  | ❌                | ✅ (4 cámaras)      |
| Timeline             | ❌                | ✅                  |
| Letterbox UI         | ❌                | ✅                  |
| Signals              | ❌                | ✅                  |
| Tiempo de setup      | 30 segundos       | 5 minutos           |
| Configuración manual | Poca              | Media               |

---

## 🎓 PASOS SIGUIENTES

### Después del Método 1:

1. Completa una misión para ver al boss aparecer
2. Si te gusta, agrega cinemática con Método 2
3. Ajusta la posición del spawn point si es necesario

### Después del Método 2:

1. Ajusta los tiempos de las cámaras en el Timeline
2. Agrega efectos de partículas al `BossCinematicEvents`
3. Conecta los Signal Emitters a los métodos
4. Testea la cinemática completa
5. Ajusta audio y efectos visuales

---

## 💡 TIPS PRO

1. **Visualización del Spawn Point**:

   - Con el `QuickBossCinematicSetup` en la escena, verás una esfera roja en Scene view mostrando dónde aparecerá el boss

2. **Testing Rápido**:

   - Usa los comandos de Context Menu (click derecho) para testear sin jugar la escena completa

3. **Combinar Métodos**:

   - Puedes empezar con Método 1 y luego ejecutar Método 2 para agregar la cinemática

4. **Backup**:
   - Guarda tu escena antes de ejecutar cualquier wizard

---

## 📝 ARCHIVOS CREADOS

### Método 1:

- `BossSpawner` GameObject en la escena
- `BossSpawnPoint` como hijo del spawner

### Método 2:

- `BossSpawner` GameObject
- `BossCinematicTimeline` GameObject con PlayableDirector
- `BossCinematicCameras` con 4 cámaras virtuales
- `CinematicUI` con Letterbox Bars
- Timeline Asset en `Assets/00_Scenes/OpenWorldSceneMerged/`
- Signals en `Assets/04_Scripts/Signals/`

---

## ✅ CHECKLIST FINAL

Antes de testear en juego, verifica:

- [ ] Boss Prefab asignado
- [ ] Spawn Position configurada
- [ ] MissionManager en la escena
- [ ] Al menos una misión configurada
- [ ] (Si usaste Método 2) Timeline tiene cámaras asignadas
- [ ] (Si usaste Método 2) Signals conectados a eventos

---

¡Ya estás listo para tener una aparición épica del boss! 🎬🎮

**Creado para**: GameJam Project  
**Scripts**: QuickBossCinematicSetup.cs, BossCinematicSetupWizard.cs  
**Fecha**: Noviembre 2025
