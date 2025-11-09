# 🎯 Sistema de Cola de Misiones y Animaciones

## ✨ Nuevas Características

### 🎬 Animación de Misión Completada

Cuando completas una misión, ahora verás:

- ✅ **Pulse de escala** - El panel se agranda y vuelve a su tamaño
- ✅ **Texto "¡MISIÓN COMPLETADA!"** con estrellas
- ✅ **Cambio de color** a verde
- ✅ **Fade out** suave antes de desaparecer
- ✅ **Display de 3 segundos** antes de ocultarse

### 📋 Sistema de Cola de Misiones

Ahora puedes encadenar múltiples misiones que se ejecutarán automáticamente una tras otra.

---

## 🎮 Configuración del MissionManager

En el Inspector del GameObject "MissionManager":

### Nuevos Parámetros:

**Mission Queue Settings:**

- **Auto Start Next Mission** ☑️ - Si está marcado, inicia automáticamente la siguiente misión
- **Delay Before Next Mission** - Segundos de espera antes de iniciar la próxima (default: 2s)

---

## 🎨 Configuración de Animaciones (MissionUI)

En el GameObject que tiene el componente `MissionUI`:

**Animation Settings:**

- **Completion Scale Pulse** - Tamaño máximo del pulse (default: 1.3 = 130%)
- **Completion Animation Duration** - Duración del pulse en segundos (default: 0.5s)
- **Display Completed Time** - Tiempo que se muestra "completada" (default: 3s)
- **Completion Curve** - Curva de animación (puedes editarla en el Inspector)

---

## 📝 Cómo Usar la Cola de Misiones

### Método 1: Encolar Todas las Misiones (Automático)

En el `MissionDebugHelper`, clic derecho:

1. **📋 Queue All Available Missions**

Esto agregará TODAS las misiones de `Available Missions` del MissionManager a la cola.

### Método 2: Encolar Misiones desde Código

```csharp
MissionManager manager = MissionManager.Instance;

// Agregar una misión a la cola
manager.QueueMission(misionParaDerrotar5Enemigos);
manager.QueueMission(misionParaDerrotar10Enemigos);
manager.QueueMission(misionParaDerrotar20Enemigos);

// O agregar todas las disponibles
manager.QueueAllMissions();
```

### Método 3: Inicio Manual (sin auto-start)

Si desmarcas **Auto Start Next Mission**:

```csharp
// Iniciar la siguiente manualmente
MissionManager.Instance.StartNextMission();

// O desde el MissionDebugHelper:
// Clic derecho → ▶️ Start Next Mission in Queue
```

---

## 🔄 Flujo Automático de Misiones

Con `Auto Start Next Mission` activado:

1. **Misión 1 inicia** → "Derrota 5 enemigos"
2. Usuario mata 5 enemigos
3. **Animación de completada** (pulse + celebración)
4. **Espera 2 segundos** (configurable)
5. **Misión 2 inicia automáticamente** → "Derrota 10 enemigos"
6. Usuario mata 10 enemigos
7. Repite...

---

## 🎯 Ejemplo de Setup Completo

### Paso 1: Crear Misiones

1. Crea 3 misiones:
   - `Mission_5Enemies` (Enemies Required: 5)
   - `Mission_10Enemies` (Enemies Required: 10)
   - `Mission_20Enemies` (Enemies Required: 20)

### Paso 2: Configurar MissionManager

1. Arrastra las 3 misiones al array `Available Missions`
2. Marca ☑️ **Auto Start Next Mission**
3. **Delay Before Next Mission**: `2` segundos

### Paso 3: Iniciar

Opción A:

- En `MissionDebugHelper`: Marca ☑️ **Start Mission On Start**
- Desmarca esa opción después del primer inicio

Opción B:

- En `MissionDebugHelper`, clic derecho → **📋 Queue All Available Missions**
- Esto encola todas y empieza la primera

### Paso 4: Jugar

1. Dale Play
2. Mata enemigos y completa misiones
3. Verás las animaciones y las misiones se encadenarán automáticamente

---

## 🛠️ Comandos de Debug

En el componente `MissionDebugHelper`, clic derecho:

| Comando                             | Descripción                            |
| ----------------------------------- | -------------------------------------- |
| **Start Mission**                   | Inicia la primera misión de la lista   |
| **📋 Queue All Available Missions** | Encola todas las misiones disponibles  |
| **▶️ Start Next Mission in Queue**  | Inicia manualmente la siguiente misión |
| **ℹ️ Show Queue Info**              | Muestra cuántas misiones hay en cola   |
| **🔍 DIAGNOSTICO COMPLETO**         | Chequea todo el sistema                |
| **🧪 Test: Simular Matar Enemigo**  | Simula matar un enemigo para testing   |

---

## 📊 Verificar Estado de la Cola

```csharp
MissionManager manager = MissionManager.Instance;

// ¿Cuántas misiones hay en cola?
int count = manager.GetQueueCount();
Debug.Log($"Misiones en cola: {count}");

// ¿Hay una misión activa?
bool hasActive = manager.HasActiveMission();
Debug.Log($"Misión activa: {hasActive}");

// Limpiar la cola
manager.ClearQueue();
```

---

## 🎨 Personalizar Animaciones

### Cambiar el Efecto de Pulse

En el Inspector de `MissionUI`:

- **Completion Scale Pulse**:
  - `1.0` = Sin pulse
  - `1.3` = Crece 30% (default)
  - `1.5` = Crece 50% (más dramático)
  - `2.0` = Crece 100% (muy exagerado)

### Cambiar la Velocidad

- **Completion Animation Duration**:
  - `0.3s` = Muy rápido
  - `0.5s` = Normal (default)
  - `1.0s` = Lento, dramático

### Cambiar Tiempo de Display

- **Display Completed Time**:
  - `1s` = Rápido
  - `3s` = Normal (default)
  - `5s` = Largo, para leer todo

### Editar la Curva

Haz clic en **Completion Curve** en el Inspector:

- Predefinidos: Linear, EaseIn, EaseOut, EaseInOut
- O edita manualmente la curva para efectos custom

---

## 🐛 Solución de Problemas

### La animación no se ve

- ✅ Verifica que `Completion Scale Pulse` > 1.0
- ✅ Asegúrate de que el panel sea visible cuando se completa

### Las misiones no se encadenan

- ✅ Verifica que **Auto Start Next Mission** esté marcado
- ✅ Usa **ℹ️ Show Queue Info** para ver si hay misiones en cola
- ✅ Revisa la consola para logs en cyan

### La siguiente misión empieza muy rápido/lento

- ✅ Ajusta **Delay Before Next Mission** en MissionManager

### Quiero control manual

- ✅ Desmarca **Auto Start Next Mission**
- ✅ Usa `MissionManager.Instance.StartNextMission()` cuando quieras

---

## 💡 Ideas Avanzadas

### Misión con Recompensa

```csharp
// Al completar una misión, dar recompensa antes de la siguiente
void OnMissionCompleted(DefeatEnemiesMission mission)
{
    GiveReward(mission);
    // La siguiente misión iniciará automáticamente después del delay
}
```

### Diferentes Delays por Misión

```csharp
// En MissionManager, puedes ajustar el delay dinámicamente
manager.delayBeforeNextMission = 5f; // Más tiempo antes de la siguiente
```

### Saltar Misión

```csharp
// Cancelar la actual y pasar a la siguiente
MissionManager.Instance.CancelCurrentMission();
MissionManager.Instance.StartNextMission();
```

---

## 📝 Resumen

✅ Animación de completado con pulse, celebración y fade
✅ Sistema de cola para encadenar misiones
✅ Inicio automático o manual
✅ Totalmente configurable desde el Inspector
✅ Debug helpers para testing fácil

¡Disfruta tu sistema de misiones mejorado! 🎮✨
