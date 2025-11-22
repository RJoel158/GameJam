# 🐲 Sistema de Spawn del Boss

## 📝 Descripción

Este sistema permite que el boss aparezca automáticamente cerca del jugador cuando se completa una misión específica (por ejemplo, limpiar todos los campamentos enemigos).

---

## 📁 Archivos Creados/Modificados

### Nuevos Archivos

1. ✅ `DefeatBossMission.cs` - ScriptableObject para la misión de derrotar al boss

### Archivos Modificados

1. ✅ `Boss.cs` - Agregado evento `OnBossDefeated` para el sistema de misiones
2. ✅ `BossSpawner.cs` - Mejorado para verificar misión específica y spawnear cerca del jugador

---

## 🎮 Cómo Funciona

### 1. Sistema de Misiones Previas

El `BossSpawner` escucha cuando se completa una misión:

```csharp
[Header("Mission Trigger Settings")]
public DefeatEnemiesMission requiredMission;  // La misión que debe completarse
public bool requireSpecificMission = true;     // Si debe ser una misión específica
```

### 2. Spawn Cerca del Jugador

Cuando la misión se completa:

1. ✅ Espera 3 segundos (configurable) para mostrar UI de "Misión Completada"
2. ✅ Reproduce cinemática (opcional)
3. ✅ Spawnea el boss cerca del jugador
4. ✅ Teletransporta al boss a distancia configurable del jugador
5. ✅ Hace que el boss mire al jugador
6. ✅ Activa los sistemas de combate del boss

### 3. Misión de Derrotar al Boss

Una vez derrotado el boss, puedes crear una misión que detecte esto:

```csharp
// El boss emite un evento cuando muere
Boss.OnBossDefeated?.Invoke(transform.position);

// DefeatBossMission escucha este evento
Boss.OnBossDefeated += OnBossDefeated;
```

---

## 🔧 Setup Rápido

### Paso 1: Configurar la Misión Previa

1. En el Project, encuentra o crea una misión (ej: `MisionDefeatEnemies.asset`)
2. Esta será la misión que debe completarse ANTES de que aparezca el boss

### Paso 2: Preparar el Boss en la Escena

1. **Arrastra el prefab del boss** a la escena desde `Assets/EduPrefabs/Boss`
2. **Configura todas las referencias** del boss (arma, NavMesh, etc.)
3. **DESACTIVA el GameObject del boss** en el Inspector (checkbox al lado del nombre)
   - ⚠️ **IMPORTANTE**: El boss DEBE estar desactivado al inicio

### Paso 3: Configurar el BossSpawner

1. Encuentra el GameObject con el script `BossSpawner` en tu escena
2. Configura los siguientes campos:

```
Boss Settings:
├─ Boss GameObject: [Arrastra el boss DESACTIVADO de la jerarquía]
├─ Spawn Point: [Opcional - posición durante cinemática]
└─ Instantiate Boss: ❌ (false = activa el boss existente - RECOMENDADO)

Mission Trigger Settings:
├─ Required Mission: [Arrastra la misión que debe completarse]
└─ Require Specific Mission: ✅ (true para misión específica)

Cinematic Settings:
├─ Cinematic Timeline: [Opcional - Timeline de introducción]
├─ Play Cinematic Before Spawn: ✅ (si quieres cinemática)
└─ Mission Complete Delay: 3s

Boss Teleport Settings:
├─ Teleport Distance: 8m (distancia del jugador al terminar cinemática)
└─ Teleport Height Offset: 1.5m (altura sobre el suelo)
```

### 🎬 Flujo del Sistema:

1. **Misión completada** → Espera 3s
2. **Boss se activa** en Spawn Point (o su posición actual si no hay spawn point)
3. **Cinemática se reproduce** (opcional)
4. **Boss se teletransporta** cerca del jugador (8m)
5. **Sistemas de combate activados** → ¡Pelea!

### Paso 3: Crear Misión de Derrotar al Boss (Opcional)

Si quieres una misión para derrotar al boss:

1. Click derecho en Project → `Create > Missions > Defeat Boss Mission`
2. Nombra el asset: `MisionDefeatBoss`
3. Configura:

   - Mission Name: "Derrota al Jefe"
   - Description: "Derrota al poderoso jefe para completar esta misión"

4. En el `MissionManager`, agrega esta misión a `Available Missions`

---

## 💡 Ejemplos de Uso

### Ejemplo 1: Boss Aparece Después de Limpiar 3 Campamentos

```csharp
// En el Inspector del BossSpawner:
Boss GameObject: Boss (de la jerarquía - DESACTIVADO)
Required Mission: MisionCleanCampings (requiere 3 campamentos)
Require Specific Mission: ✅
Instantiate Boss: ❌ (usa el boss de la escena)
Teleport Distance: 10m
```

### Ejemplo 2: Boss Aparece con Cinemática

```csharp
// En el Inspector del BossSpawner:
Cinematic Timeline: BossIntro_Timeline
Play Cinematic Before Spawn: ✅
Mission Complete Delay: 3s  // Tiempo para ver "Misión Completada"
Teleport Boss After Cinematic: ✅
```

### Ejemplo 3: Spawn Manual del Boss (Para Testear)

```csharp
// Encuentra el BossSpawner en la escena
BossSpawner spawner = FindObjectOfType<BossSpawner>();

// Click derecho en el componente BossSpawner → "Force Spawn Boss"
// Esto activará el boss, lo teletransportará cerca del jugador y habilitará combate
// O desde código:
spawner.ForceSpawnBoss();
```

---

## 🎯 Configuración de Distancia y Posición

### Posicionamiento del Boss

El sistema funciona en dos etapas:

#### 1. Durante la Cinemática (ActivateAndPositionBoss)

- Si hay `Spawn Point` asignado: Boss aparece en esa posición
- Si NO hay `Spawn Point`: Boss permanece en su posición actual en la escena

#### 2. Después de la Cinemática (TeleportBossToPlayer)

- El boss SIEMPRE se teletransporta cerca del jugador
- **Distancia**: `teleportDistance` metros del jugador (default: 8m)
- **Altura**: Altura del jugador + `teleportHeightOffset` (default: 1.5m)
- **Dirección**: Mirando hacia el jugador

```csharp
Vector3 teleportPosition = playerPos + direction * teleportDistance;
teleportPosition.y = playerPos.y + teleportHeightOffset;
```

💡 **Recomendación**: Deja el `Spawn Point` vacío si solo quieres usar la cinemática actual del boss

---

## 🐛 Troubleshooting

### El boss no aparece

✅ Verifica que:

1. `BossSpawner` está en la escena
2. `Boss GameObject` está asignado (arrastra desde la jerarquía)
3. El boss GameObject está **DESACTIVADO** al inicio
4. `Required Mission` está asignada
5. La misión realmente se completó (revisa consola)
6. `Instantiate Boss` está en **false** (para usar boss de escena)

### El boss aparece en posición incorrecta

✅ Ajusta:

- `Teleport Distance` (más cerca/lejos del jugador)
- `Teleport Height Offset` (más alto/bajo)
- Asigna un `Spawn Point` específico

### La misión de boss no se completa

✅ Verifica:

1. El boss tiene el script `Boss.cs`
2. El método `Die()` se llama cuando health <= 0
3. El evento `OnBossDefeated` se emite
4. La misión `DefeatBossMission` está activa

### La cinemática no funciona

✅ Asegúrate de:

1. El `Cinematic Timeline` está asignado
2. El Timeline tiene las pistas configuradas
3. `Play Cinematic Before Spawn` está activado
4. El Timeline usa `UnscaledGameTime` (se configura automáticamente)

---

## 📊 Eventos y Hooks

### Eventos del Boss

```csharp
// Escuchar cuando el boss es derrotado
Boss.OnBossDefeated += (Vector3 position) => {
    Debug.Log($"Boss derrotado en: {position}");
};
```

### Eventos del Mission Manager

```csharp
// Escuchar cuando se completa cualquier misión
MissionManager.Instance.OnMissionCompleted.AddListener((DefeatEnemiesMission mission) => {
    Debug.Log($"Misión completada: {mission.missionName}");
});
```

---

## 🎬 Secuencia Completa

1. **Jugador completa la misión previa** (ej: limpiar campamentos)
2. **MissionManager emite evento** `OnMissionCompleted`
3. **BossSpawner escucha el evento**
4. **Verifica si es la misión correcta**
5. **Espera 3s** (para mostrar UI de "Misión Completada")
6. **Desactiva UI y controles del jugador**
7. **Activa el boss GameObject** en su posición (o Spawn Point si está asignado)
8. **Reproduce cinemática** (opcional)
9. **Teletransporta boss cerca del jugador** (8m de distancia)
10. **Habilita combate del boss** (NavMesh, Enemy, Animator)
11. **Reactiva controles del jugador**
12. **¡Combate contra el boss!**

---

## 🔄 Ciclo de Misiones

```
Misión 1: Limpiar Campamentos (3/3)
    ↓ Completada
Trigger: BossSpawner detecta misión completada
    ↓ Espera 3s
Cinemática: Introducción del Boss
    ↓ Finaliza
Boss: Spawneado y teletransportado cerca del jugador
    ↓ Jugador derrota al boss
Misión 2: Derrotar al Boss (1/1)
    ↓ Completada
¡Juego continúa!
```

---

## 🎨 Personalización Avanzada

### Cambiar Posición de Spawn

```csharp
// En BossSpawner, modifica SpawnBoss():
spawnPosition = playerPos + player.transform.right * 5f; // Lado derecho
spawnPosition = playerPos - player.transform.forward * 8f; // Detrás
```

### Agregar Efectos de Spawn

```csharp
// En BossSpawner, método PlaySpawnEffects():
private void PlaySpawnEffects(Vector3 position)
{
    // Instantiate teleport particles
    GameObject teleportFX = Instantiate(teleportEffect, position, Quaternion.identity);

    // Play boss roar sound
    AudioSource.PlayClipAtPoint(bossRoarSound, position);

    // Camera shake
    CameraShake.Instance?.Shake(0.5f, 1f);
}
```

---

## 📝 Notas Importantes

⚠️ **El boss GameObject debe estar DESACTIVADO** al inicio de la escena

⚠️ **Usa `Instantiate Boss = false`** para activar el boss con sus configuraciones

⚠️ **El boss debe tener NavMeshAgent** si está en Fase 2 (movimiento)

⚠️ **El BossHealthBarUI debe estar en la escena** para mostrar la barra de vida

⚠️ **El Timeline debe usar UnscaledGameTime** (se configura automáticamente)

⚠️ **Los controles del jugador se desactivan** durante la cinemática

⚠️ **La UI se oculta durante la cinemática** y se restaura después

⚠️ **El boss SIEMPRE se teletransporta** cerca del jugador después de la cinemática (8m)

---

**¡Todo listo para el combate épico contra el boss!** 🎮⚔️
