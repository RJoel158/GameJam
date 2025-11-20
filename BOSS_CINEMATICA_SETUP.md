# 🎬 SETUP CINEMÁTICA DEL BOSS - GUÍA COMPLETA

Esta guía te ayudará a configurar una cinemática profesional para la aparición del boss después de completar la misión de derrotar enemigos.

---

## 📋 REQUISITOS PREVIOS

- Unity 2021.3 o superior
- Cinemachine instalado (Window > Package Manager > Cinemachine)
- Timeline instalado (viene por defecto con Unity)
- MissionManager y DefeatEnemiesMission configurados
- Boss prefab o GameObject preparado

---

## 🎯 PARTE 1: CONFIGURACIÓN INICIAL

### 1.1 Instalar Paquetes Necesarios

1. Abre **Window > Package Manager**
2. Instala estos paquetes si no los tienes:
   - **Cinemachine** (para cámaras cinemáticas)
   - **Timeline** (debería estar instalado por defecto)

### 1.2 Importar Scripts

Los siguientes scripts ya están creados en `Assets/04_Scripts/`:

- ✅ `BossSpawner.cs` - Controla el spawn del boss
- ✅ `CinematicController.cs` - Controla la cinemática con Timeline
- ✅ `BossCinematicEvents.cs` - Eventos llamables desde Timeline

---

## 🎬 PARTE 2: CREAR EL TIMELINE

### 2.1 Crear Timeline Asset

1. En la escena **OpenWorldMerged 1**, crea un GameObject vacío:

   - Right-click en Hierarchy > Create Empty
   - Nómbralo: `BossCinematicTimeline`

2. Con el GameObject seleccionado:

   - Ve a Window > Sequencing > Timeline
   - Click en "Create" en la ventana de Timeline
   - Guarda el asset como: `Assets/00_Scenes/OpenWorldSceneMerged/BossIntro_Timeline.playable`

3. Agrega el componente **Playable Director**:
   - Debe haberse agregado automáticamente
   - Si no, Add Component > Playable Director
   - Verifica que apunte al Timeline que acabas de crear

### 2.2 Configurar Tracks en el Timeline

Ahora crearemos las pistas (tracks) para la cinemática:

#### Track 1: Cinemachine Track (Para cámaras)

1. En la ventana Timeline, click derecho > Cinemachine Track
2. Nombra el track: "Cinematic Cameras"
3. Este track controlará las cámaras durante la cinemática

#### Track 2: Animation Track (Para animaciones del boss)

1. Click derecho en Timeline > Animation Track
2. Arrastra tu Boss prefab al campo del track
3. Nombra el track: "Boss Animation"

#### Track 3: Signal Track (Para eventos)

1. Click derecho en Timeline > Signal Track
2. Nombra el track: "Events"
3. Aquí colocaremos las señales para eventos específicos

---

## 📹 PARTE 3: CONFIGURAR CÁMARAS CINEMACHINE

### 3.1 Crear Cámaras Virtuales

Crea 3-4 cámaras virtuales para diferentes ángulos:

#### Cámara 1: Wide Shot (Plano General)

1. Right-click en Hierarchy > Cinemachine > Virtual Camera
2. Nombre: `CM_BossIntro_Wide`
3. Posiciona la cámara para un plano amplio del área donde aparecerá el boss
4. Ajusta:
   - **Body**: Framing Transposer o Hard Lock to Target
   - **Aim**: Do Nothing (o apunta manualmente)
   - **Priority**: 0 (por defecto)

#### Cámara 2: Boss Close-Up (Primer Plano del Boss)

1. Crea otra Virtual Camera: `CM_BossIntro_CloseUp`
2. Posiciónala cerca del punto de spawn del boss
3. Apúntala hacia donde aparecerá el boss
4. Ajusta el FOV a 40-50 para un plano más cerrado

#### Cámara 3: Player Reaction (Reacción del Jugador)

1. Crea otra Virtual Camera: `CM_BossIntro_PlayerReaction`
2. Posiciónala mirando al jugador
3. Muestra la reacción del jugador al ver al boss

#### Cámara 4: Dramatic Angle (Ángulo Dramático)

1. Crea otra Virtual Camera: `  `
2. Posiciónala en un ángulo bajo mirando hacia arriba
3. Da sensación épica y amenazante

### 3.2 Agregar Cámaras al Timeline

1. En la ventana Timeline, en el track "Cinematic Cameras"
2. Arrastra cada cámara virtual a diferentes momentos:

   ```
   0s-3s:    Wide Shot (mostrar área)
   3s-5s:    Boss Close-Up (boss aparece)
   5s-7s:    Dramatic Angle (boss rugiendo)
   7s-9s:    Player Reaction (mostrar jugador)
   9s-11s:   Wide Shot (vista general de batalla)
   ```

3. Ajusta los tiempos según tu preferencia

---

## 🎮 PARTE 4: CONFIGURAR EL BOSS SPAWNER

### 4.1 Crear GameObject BossSpawner

1. En Hierarchy, crea un GameObject vacío:

   - Nombre: `BossSpawner`
   - Posición: 0, 0, 0

2. Add Component > **BossSpawner** script

3. Configura los campos:

   **Boss Settings:**

   - `Boss Prefab`: Arrastra tu boss prefab aquí
   - `Spawn Point`: Crea un Transform vacío donde aparecerá el boss
   - `Instantiate Boss`: ✅ True (si quieres instanciar) o ❌ False (si ya está en escena)

   **Cinematic Settings:**

   - `Cinematic Timeline`: Arrastra el GameObject `BossCinematicTimeline`
   - `Play Cinematic Before Spawn`: ✅ True
   - `Spawn Delay`: 1.0s

   **Camera Settings:**

   - `Player Camera`: Arrastra la cámara del jugador (o déjalo vacío)
   - `Disable Player Controls`: ✅ True

   **Audio Settings:**

   - `Boss Appear Sound`: Arrastra un AudioClip épico (opcional)

### 4.2 Crear Spawn Point

1. Crea un GameObject vacío:

   - Nombre: `BossSpawnPoint`
   - Posiciónalo donde quieres que aparezca el boss
   - El gizmo te mostrará una esfera roja y una flecha azul

2. Arrastra este GameObject al campo `Spawn Point` del BossSpawner

---

## 🎯 PARTE 5: CONFIGURAR EVENTOS DEL TIMELINE

### 5.1 Crear GameObject para Eventos

1. En el GameObject `BossCinematicTimeline`:

   - Add Component > **BossCinematicEvents** script

2. Configura los campos:
   - `Boss Spawner`: Arrastra el GameObject BossSpawner
   - `Boss`: Se asignará automáticamente cuando spawne
   - `Enable Camera Shake`: ✅ True
   - `Shake Intensity`: 0.5
   - `Shake Duration`: 0.3

### 5.2 Agregar Eventos al Timeline

En el track "Events" (Signal Track):

1. **Segundo 2**: Spawn Boss

   - Crea un Signal Emitter
   - Conecta a `BossCinematicEvents.SpawnBoss()`

2. **Segundo 3**: Ground Impact Effect

   - Signal Emitter
   - Conecta a `BossCinematicEvents.PlayGroundImpactEffect()`

3. **Segundo 4**: Boss Roar

   - Signal Emitter
   - Conecta a `BossCinematicEvents.PlayBossRoar()`

4. **Segundo 5**: Dramatic Music

   - Signal Emitter
   - Conecta a `BossCinematicEvents.PlayDramaticMusic()`

5. **Segundo 10**: Enable Boss AI
   - Signal Emitter
   - Conecta a `BossCinematicEvents.EnableBossAI()`

### 5.3 Configurar Signals (Señales)

Si no sabes usar Signals en Timeline:

1. En Unity, ve a: **Assets > Create > Signals**
2. Crea estas señales:

   - `Signal_SpawnBoss`
   - `Signal_GroundImpact`
   - `Signal_BossRoar`
   - `Signal_DramaticMusic`
   - `Signal_EnableBossAI`

3. En cada Signal Emitter en el Timeline:
   - Asigna la señal correspondiente
   - Ve al GameObject `BossCinematicTimeline`
   - En el Inspector, agrega Signal Receivers
   - Conecta cada señal a su método correspondiente en `BossCinematicEvents`

---

## 🎨 PARTE 6: CONFIGURAR EFECTOS VISUALES (OPCIONAL)

### 6.1 Añadir Particle Effects

1. Crea particle systems para:

   - Boss appear effect (humo, energía, etc.)
   - Ground impact (polvo, rocas)

2. Asígnalos en el `BossCinematicEvents`:
   - `Boss Appear Effect`
   - `Ground Impact Effect`

### 6.2 Añadir Camera Shake

1. En cualquier cámara Cinemachine, Add Component:

   - **Cinemachine Impulse Source**

2. Configura:

   - Amplitude Gain: 1.0
   - Frequency Gain: 1.0
   - Duration: 0.3s

3. Esto permitirá el shake cuando el boss golpee el suelo

---

## 🔊 PARTE 7: CONFIGURAR AUDIO

### 7.1 Boss Audio

En el `BossCinematicEvents`, asigna:

- `Boss Roar Sound`: Rugido épico del boss
- `Ground Impact Sound`: Sonido de impacto
- `Dramatic Music`: Música dramática de fondo

### 7.2 Timeline Audio Track (Alternativa)

También puedes usar un Audio Track en el Timeline:

1. En Timeline: Right-click > Audio Track
2. Arrastra AudioClips directamente al track
3. Ajusta volumen y fade in/out

---

## 🔗 PARTE 8: CONECTAR CON MISSION MANAGER

### 8.1 Configurar MissionManager

1. Encuentra el GameObject `MissionManager` en la escena
2. En el Inspector, en la sección **Events**:
   - Expande `On Mission Completed`
   - Click en `+` para agregar un nuevo evento
   - Arrastra el GameObject `BossSpawner`
   - Selecciona: `BossSpawner > PlayCinematic()`
   - ⚠️ **NOTA**: El BossSpawner ya está suscrito al evento, pero puedes usar UnityEvents también

### 8.2 Verificar Configuración

El flujo completo debe ser:

```
Misión Completada
   ↓
OnMissionCompleted Event (MissionManager)
   ↓
BossSpawner.OnMissionCompleted() escucha el evento
   ↓
Inicia Cinemática (Timeline)
   ↓
Boss Spawns (desde Timeline Signal)
   ↓
Efectos y Audio
   ↓
Cinemática termina
   ↓
Controles del jugador se restauran
   ↓
¡Boss Fight!
```

---

## ✅ PARTE 9: TESTING Y DEBUGGING

### 9.1 Probar el Timeline Solo

1. Selecciona `BossCinematicTimeline` en Hierarchy
2. En la ventana Timeline, presiona Play ▶️
3. Verifica que las cámaras cambien correctamente
4. Verifica que los eventos se disparen en el momento correcto

### 9.2 Probar el Spawn del Boss

1. En el BossSpawner, Right-click en el script
2. Selecciona: **Force Spawn Boss**
3. Verifica que el boss aparezca en el punto correcto

### 9.3 Probar la Integración Completa

1. Play en Unity ▶️
2. Completa la misión de derrotar enemigos
3. Observa que la cinemática se active automáticamente
4. Verifica que:
   - ✅ Se desactiven los controles del jugador
   - ✅ Las cámaras cambien
   - ✅ El boss aparezca
   - ✅ Los efectos se reproduzcan
   - ✅ Los controles se reactiven al final

### 9.4 Debugging

Si algo no funciona, revisa los logs:

```csharp
// Busca en la Console estos mensajes:
[BossSpawner] Subscribed to MissionManager.OnMissionCompleted
[BossSpawner] Mission 'X' completed! Preparing boss spawn...
[BossSpawner] Starting boss introduction cinematic...
[CinematicController] Starting cinematic...
[BossCinematicEvents] Spawning boss from Timeline event...
[BossSpawner] Boss instantiated at (x, y, z)
[CinematicController] Cinematic ended, restoring scene...
```

---

## 🎨 PARTE 10: MEJORAS OPCIONALES

### 10.1 Letterbox Bars (Barras Negras Cinemáticas)

1. Crea un Canvas UI:

   - Right-click Hierarchy > UI > Canvas
   - Nombre: `CinematicUI`

2. Dentro del Canvas, crea 2 Images negras:

   - Top Bar: Altura 100px, anclada arriba
   - Bottom Bar: Altura 100px, anclada abajo
   - Color: Negro (R:0, G:0, B:0, A:255)

3. Agrupa en un GameObject:

   - Nombre: `LetterboxBars`
   - Desactívalo por defecto

4. En `CinematicController`:
   - Asigna `LetterboxBars` al campo correspondiente
   - ✅ `Show Letterbox`: True

### 10.2 Skip Cinematic (Saltar Cinemática)

1. Crea un botón UI o escucha la tecla ESC:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        CinematicController controller = FindAnyObjectByType<CinematicController>();
        controller?.SkipCinematic();
    }
}
```

### 10.3 Subtítulos

1. Crea un TextMeshPro Text en el Canvas
2. En Timeline, usa un Control Track
3. Activa/desactiva el texto en momentos específicos
4. O usa el método `ShowMessage()` de `BossCinematicEvents`

---

## 📊 RESUMEN DE CONFIGURACIÓN

### GameObjects Necesarios:

```
Scene Hierarchy:
├── MissionManager (ya existe)
├── BossSpawner (nuevo)
│   └── BossSpawnPoint (nuevo, child o separado)
├── BossCinematicTimeline (nuevo)
│   ├── Playable Director
│   ├── BossCinematicEvents
│   └── (opcional) Particle Systems
├── Cinemachine Cameras (nuevas)
│   ├── CM_BossIntro_Wide
│   ├── CM_BossIntro_CloseUp
│   ├── CM_BossIntro_PlayerReaction
│   └── CM_BossIntro_Dramatic
└── (opcional) CinematicUI
    └── LetterboxBars
```

### Scripts Configurados:

- ✅ `BossSpawner.cs` - En GameObject BossSpawner
- ✅ `CinematicController.cs` - (opcional, si quieres control extra)
- ✅ `BossCinematicEvents.cs` - En GameObject BossCinematicTimeline

---

## 🐛 TROUBLESHOOTING (Solución de Problemas)

### El boss no aparece:

- ✅ Verifica que `BossSpawner` esté suscrito al evento
- ✅ Revisa que el prefab esté asignado
- ✅ Verifica los logs en la Console

### La cinemática no inicia:

- ✅ Verifica que `Cinematic Timeline` esté asignado en BossSpawner
- ✅ Asegúrate de que `Play Cinematic Before Spawn` esté en True
- ✅ Verifica que el Timeline tenga un Playable Director

### Las cámaras no cambian:

- ✅ Verifica que las Virtual Cameras tengan diferentes priorities durante el Timeline
- ✅ Asegúrate de que el Cinemachine Brain esté en la Main Camera
- ✅ Verifica que las cámaras estén en el Cinemachine Track del Timeline

### Los controles no se reactivan:

- ✅ Verifica que el Timeline termine correctamente
- ✅ Revisa el método `OnTimelineStopped()` en CinematicController
- ✅ Asegúrate de que no haya errores en la Console

### Los eventos no se disparan:

- ✅ Verifica que los Signals estén creados
- ✅ Asegúrate de que los Signal Receivers estén configurados
- ✅ Verifica que `BossCinematicEvents` tenga las referencias correctas

---

## 🎓 SIGUIENTE PASO

Una vez configurado todo, puedes:

1. **Ajustar el timing** - Experimenta con la duración de cada cámara
2. **Añadir más efectos** - Partículas, luces, post-processing
3. **Mejorar el audio** - Música épica, sonidos ambientales
4. **Crear variaciones** - Diferentes cinemáticas para diferentes bosses
5. **Pulir animaciones** - Si el boss tiene animaciones especiales

---

## 📝 NOTAS FINALES

- Los scripts usan muchos logs de debug (en color) para facilitar el testing
- Puedes comentar los `Debug.Log()` una vez que todo funcione
- Experimenta con los tiempos del Timeline para lograr el mejor efecto dramático
- Los Gizmos te ayudarán a visualizar el spawn point del boss en el editor

¡Buena suerte con tu cinemática épica del boss! 🎬🎮

---

**Creado para:** GameJam Project  
**Versión:** 1.0  
**Fecha:** Noviembre 2025
