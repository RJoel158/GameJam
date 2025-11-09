# 🎮 Sistema de Vida y Energía del Boss

## 📋 Descripción

El boss ahora tiene un sistema completo de vida y energía con las siguientes mecánicas:

- **Barra de Vida**: Muestra el health actual del boss
- **Barra de Energía**: Se consume con cada disparo
- **Estado Inconsciente**: Cuando se queda sin energía, el boss cae al suelo y queda temporalmente vulnerable
- **Regeneración**: Después de un tiempo, el boss se recupera y restaura su energía

---

## 🔧 Componentes Creados

### 1. **BossController.cs** (`Assets/04_Scripts/`)

Script principal que maneja la vida, energía y estados del boss.

#### Propiedades Principales:

```csharp
[Header("Health")]
public int maxHealth = 1000;           // Vida máxima del boss
private int currentHealth = 1000;      // Vida actual

[Header("Energy System")]
public float maxEnergy = 100f;         // Energía máxima
public float currentEnergy = 100f;     // Energía actual
public float energyCostPerShot = 10f;  // Coste de energía por disparo
public float energyRegenRate = 5f;     // Regeneración por segundo

[Header("Unconscious State")]
public float unconsciousDuration = 5f; // Tiempo inconsciente (segundos)
```

#### Métodos Importantes:

**`bool TryUseEnergy()`**

- Verifica si hay suficiente energía para disparar
- Si no hay energía, activa el estado inconsciente
- Retorna `true` si se pudo consumir energía

**`void BecomeUnconscious()`**

- Desactiva NavMeshAgent, God script y Animator
- Activa física (Rigidbody) para que el boss caiga
- Inicia timer de unconsciousDuration

**`void WakeUp()`**

- Restaura energía al máximo
- Reactiva componentes de combate
- Desactiva física

**`void TakeDamage(int damage)`**

- Reduce la vida del boss
- Llama a `Die()` si currentHealth <= 0

**`float GetHealthPercent()` / `float GetEnergyPercent()`**

- Retornan valores 0-1 para actualizar las barras UI

---

### 2. **BossHealthBarUI.cs** (`Assets/04_Scripts/`)

Script que controla la UI de las barras de vida y energía.

#### Configuración:

```csharp
[Header("Boss Reference")]
public BossController bossController;  // Auto-find si no se asigna

[Header("UI Elements")]
public Image healthFillImage;          // Image con fillAmount para vida
public Image energyFillImage;          // Image con fillAmount para energía

[Header("Colors")]
public Color healthColor = Color.red;
public Color energyColor = new Color(0.2f, 0.5f, 1f); // Azul
public Color lowEnergyColor = Color.yellow;
public float lowEnergyThreshold = 0.3f; // Cambiar color si energía < 30%
```

#### Funcionalidad:

- Actualiza automáticamente las barras cada frame
- Cambia el color de energía a amarillo cuando está baja
- Se oculta cuando el boss está muerto o inactivo

---

### 3. **SetupBossUI.cs** (`Assets/Editor/`)

Editor Wizard para crear la UI automáticamente.

#### Uso:

1. Ve a **Game Jam > Setup > Boss UI (Health & Energy Bars)**
2. (Opcional) Asigna un Canvas existente
3. (Opcional) Asigna el BossController
4. Click en **"Create Boss UI"**

#### Lo que Crea:

```
BossUI_Canvas (si no existe)
└── BossUI_Container
    ├── Background (panel negro semi-transparente)
    ├── BossLabel (texto "BOSS")
    ├── HealthBar
    │   ├── Background (gris)
    │   ├── Fill (rojo)
    │   └── Label ("Health")
    └── EnergyBar
        ├── Background (gris)
        ├── Fill (azul)
        └── Label ("Energy")
```

---

## 🎯 Integración con God.cs

El script `God.cs` (shooting AI del boss) fue modificado para usar energía:

### Cambios Realizados:

```csharp
// NUEVO: Referencia al BossController
private BossController bossController;

void Start()
{
    // ... código existente ...

    // Obtener referencia al BossController
    bossController = GetComponent<BossController>();
}

void CheckIfCanShoot()
{
    if (timer <= timeBtwShoot)
    {
        timer += Time.deltaTime;
    }
    else
    {
        timer = 0;

        // NUEVO: Verificar si el boss tiene energía suficiente
        bool canShoot = true;
        if (bossController != null)
        {
            canShoot = bossController.TryUseEnergy();
        }

        // Solo disparar si tiene energía
        if (canShoot)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
```

---

## 📦 Setup Completo del Boss

### Paso 1: Añadir Componentes al Boss

En el GameObject del boss (prefab "God"), añade:

1. **BossController** (script)

   - Configura maxHealth, energyCostPerShot, etc.

2. **Rigidbody** (component)

   - ✅ **Is Kinematic = true** (inicialmente)
   - ✅ **Use Gravity = true**
   - Mass = 1
   - Drag = 0
   - Angular Drag = 0.05

3. **Collider** (si no tiene)
   - CapsuleCollider o BoxCollider
   - ✅ **Is Trigger = false** (para colisionar con el suelo)

### Paso 2: Crear la UI

1. Usa el wizard: **Game Jam > Setup > Boss UI (Health & Energy Bars)**
2. O crea manualmente:
   - Canvas en modo ScreenSpaceOverlay
   - Panel contenedor con las barras
   - Añade script `BossHealthBarUI`

### Paso 3: Configurar Referencias

En `BossHealthBarUI`:

- Asigna `bossController` → el GameObject del boss
- Asigna `healthFillImage` → la Image del fill de vida
- Asigna `energyFillImage` → la Image del fill de energía
- Asigna `bossUIContainer` → el GameObject contenedor

### Paso 4: Tag del Suelo

Asegúrate de que el suelo tenga el tag **"Ground"** para detectar colisiones:

1. Selecciona el GameObject del suelo/terrain
2. En el Inspector, Tag → "Ground"

---

## 🎮 Flujo de Combate

### Ciclo Normal:

1. Boss dispara → consume energía (10 por disparo)
2. Energía se regenera lentamente (5/segundo)
3. Barra de energía cambia a amarillo cuando < 30%

### Agotamiento de Energía:

1. **Energía llega a 0** → `BecomeUnconscious()` se activa
2. Boss cae al suelo (física activada)
3. **Colisiona con el suelo** → log de debug
4. **Timer cuenta 5 segundos** → `WakeUp()` se llama
5. Boss recupera energía al máximo y vuelve al combate

### Muerte del Boss:

1. Vida llega a 0 → `Die()` se activa
2. Desactiva NavMeshAgent, Animator, colliders
3. UI del boss se oculta automáticamente

---

## 🔍 Debug y Testing

### Logs Importantes:

```
[BossController] Boss has become unconscious! (Yellow)
[Boss] Hit the ground while unconscious (Yellow)
[BossController] Boss is waking up and restoring energy (Green)
[BossController] Boss has died! (Red)
```

### Gizmos:

En Scene View, con el boss seleccionado:

- **Esfera Verde** = Boss activo
- **Esfera Roja** = Boss inconsciente

### Testing en Play Mode:

**Probar Energía:**

```csharp
// En Inspector, reduce energyCostPerShot a 5
// Reduce maxEnergy a 30
// Observa cómo se agota más rápido
```

**Probar Estado Inconsciente:**

```csharp
// Establece unconsciousDuration = 2f para pruebas rápidas
// Observa el ciclo completo de caída y despertar
```

**Probar Muerte:**

```csharp
// En BossController, establece currentHealth = 50
// Golpea al boss y verifica que muere correctamente
```

---

## ⚙️ Valores Recomendados

### Boss Fácil:

```csharp
maxHealth = 500
maxEnergy = 100
energyCostPerShot = 5
energyRegenRate = 8
unconsciousDuration = 6
```

### Boss Normal:

```csharp
maxHealth = 1000
maxEnergy = 100
energyCostPerShot = 10
energyRegenRate = 5
unconsciousDuration = 5
```

### Boss Difícil:

```csharp
maxHealth = 2000
maxEnergy = 150
energyCostPerShot = 8
energyRegenRate = 3
unconsciousDuration = 3
```

---

## 🐛 Troubleshooting

### La UI no aparece:

✅ Verifica que `bossUIContainer.SetActive(true)` esté llamado
✅ Revisa que el Canvas esté en modo ScreenSpaceOverlay
✅ Asegúrate de que `bossController` no sea null

### El boss no cae cuando está inconsciente:

✅ Verifica que Rigidbody existe y `isKinematic` está configurado
✅ Asegúrate de que `Use Gravity = true`
✅ Revisa que no haya un NavMeshAgent bloqueando el movimiento

### La energía no se regenera:

✅ Verifica que `energyRegenRate > 0`
✅ Asegúrate de que el boss no está inconsciente (no regenera mientras duerme)
✅ Revisa que Time.deltaTime no esté congelado

### El boss no despierta:

✅ Verifica que `unconsciousDuration` no sea muy alto
✅ Revisa los logs para ver si `WakeUp()` se llama
✅ Asegúrate de que la coroutine no se interrumpe

---

## 📝 Próximos Pasos

### Mejoras Opcionales:

1. **Efectos Visuales:**

   - Partículas cuando el boss cae inconsciente
   - Efecto de polvo al colisionar con el suelo
   - Glow o shader cuando la energía está baja

2. **Audio:**

   - Sonido de "sin energía"
   - Sonido de impacto con el suelo
   - Música de boss fight

3. **UI Avanzada:**

   - Animaciones en las barras
   - Números mostrando vida/energía actual
   - Icono de estado (normal/inconsciente/crítico)

4. **Balanceo:**
   - Ajustar valores según testing con jugadores
   - Diferentes fases del boss (más agresivo con menos vida)
   - Regeneración de energía más rápida en fases finales

---

## 📚 Referencias de Scripts

- `BossController.cs` - Sistema de vida/energía/estados
- `BossHealthBarUI.cs` - UI de barras
- `God.cs` - IA de disparo (modificado para energía)
- `BossSpawner.cs` - Spawning y cinemática
- `SetupBossUI.cs` - Wizard de setup

---

**¡Sistema listo para usar!** 🎉

Para activar todo:

1. Ejecuta el wizard de UI
2. Añade BossController al prefab del boss
3. Configura el tag "Ground"
4. ¡Playtest y ajusta valores!
