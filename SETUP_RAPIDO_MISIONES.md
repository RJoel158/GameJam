# 🎯 Guía Rápida de Setup - Sistema de Misiones

## ✅ Archivos Creados (Todos en Assets/04_Scripts/)

1. ✅ `DefeatEnemiesMission.cs` - ScriptableObject de misión
2. ✅ `MissionManager.cs` - Gestor de misiones (Singleton)
3. ✅ `MissionUI.cs` - Interfaz de usuario opcional
4. ✅ `MissionDebugHelper.cs` - Herramientas de debug
5. ✅ `MissionStarter.cs` - Iniciar misiones fácilmente
6. ✅ `Enemy.cs` - Modificado (evento OnEnemyDefeated)

---

## 🚀 Setup en 3 Pasos

### PASO 1: Crear una Misión

```
Unity Editor:
1. Project Window → Clic derecho
2. Create > Missions > Defeat Enemies Mission
3. Nombrar: "Mission_Defeat5Enemies"
4. En Inspector:
   - Mission Name: "Defeat 5 Enemies"
   - Enemies Required: 5
```

### PASO 2: Añadir MissionManager a la Escena

```
Unity Editor:
1. Hierarchy → Clic derecho → Create Empty
2. Nombrar: "MissionManager"
3. Add Component → MissionManager
4. Arrastrar tu misión al array "Available Missions"
```

### PASO 3: Iniciar la Misión

```
Opción A - Automático (Recomendado para testing):
1. En "MissionManager" GameObject
2. Add Component → MissionDebugHelper
3. Marcar: "Start Mission On Start"
4. Mission Index To Start: 0

Opción B - Manual desde código:
1. Crear GameObject vacío → "MissionController"
2. Add Component → MissionStarter
3. Arrastrar tu misión a "Mission To Start"
4. Marcar: "Start On Awake"
```

---

## 🎮 Probar el Sistema

### 1. Press Play

- Verás en consola: "Mission Started: Defeat 5 Enemies"

### 2. Derrota Enemigos

- Cada derrota mostrará en consola: "Enemy defeated at position: (X, Y, Z)"
- El progreso se actualiza automáticamente: "Progress: 1/5", "2/5", etc.

### 3. Completa la Misión

- Cuando derrotas 5 enemigos: "Mission Completed!"
- En consola verás TODAS las posiciones guardadas

### 4. Ver Posiciones en Scene View

- En Scene View (con Gizmos ON):
  - Esferas rojas = donde derrotaste enemigos
  - Líneas verticales para mejor visualización
  - Números de enemigo

---

## 📊 Inspeccionar Datos en Tiempo Real

Durante el juego:

1. Project Window → Selecciona tu misión
2. Inspector mostrará datos en vivo:
   - ✅ Is Active: true/false
   - ✅ Is Completed: true/false
   - ✅ Enemies Defeated: 0, 1, 2, 3...
   - ✅ Defeat Positions: Lista de Vector3

---

## 🎨 Añadir UI (Opcional)

Si quieres ver progreso en pantalla:

### Setup Rápido UI

```
1. GameObject → UI → Canvas
2. Dentro del Canvas:
   - UI → Panel (nombrar "MissionPanel")
   - UI → Text - TextMeshPro (x3):
     - MissionNameText
     - MissionDescriptionText
     - ProgressText
   - UI → Slider (nombrar "ProgressSlider")

3. En Canvas:
   - Add Component → MissionUI
   - Arrastrar referencias a los campos correspondientes
```

---

## 🔧 Comandos de Debug

Selecciona GameObject con MissionDebugHelper → Clic derecho en componente:

| Comando                    | Acción                                |
| -------------------------- | ------------------------------------- |
| **Start Mission**          | Inicia la misión                      |
| **Cancel Current Mission** | Cancela misión actual                 |
| **Show Mission Progress**  | Muestra progreso detallado en consola |
| **Reset Current Mission**  | Reinicia misión (útil para testing)   |

---

## 💾 Los Datos se Guardan Automáticamente

✅ Las posiciones de enemigos derrotados se guardan en el ScriptableObject
✅ Puedes acceder a ellos desde cualquier script
✅ Los datos persisten entre sesiones de Unity Editor

### Acceder desde código:

```csharp
MissionManager manager = MissionManager.Instance;
DefeatEnemiesMission mission = manager.currentMission;

// Obtener lista de posiciones
List<Vector3> positions = mission.defeatPositions;

// Usar las posiciones para lo que necesites
foreach (Vector3 pos in positions)
{
    Debug.Log($"Enemy defeated at: {pos}");
    // Spawner objetos, efectos, etc.
}
```

---

## 🎯 Crear Otros Tipos de Misión

Para misiones diferentes (recolectar objetos, explorar zonas, etc.):

1. Crea nueva clase que herede de `ScriptableObject`
2. Añade tus propios campos (objetivos, recompensas, etc.)
3. Suscríbete a eventos que necesites
4. Implementa lógica de CheckProgress() y Complete()

Usa `DefeatEnemiesMission.cs` como plantilla.

---

## ❓ Troubleshooting

### No veo progreso

- ✅ Verifica que MissionManager esté en escena
- ✅ Verifica que la misión esté iniciada (consola debe decir "Mission Started")
- ✅ Verifica que estás derrotando enemigos (health <= 0)

### Los enemigos no cuentan

- ✅ Verifica que el script Enemy.cs tenga el evento OnEnemyDefeated
- ✅ El enemigo debe llamar al método Die() cuando health <= 0

### No aparece UI

- ✅ Verifica que MissionUI tenga referencias asignadas
- ✅ Verifica que Canvas esté en Screen Space - Overlay
- ✅ Verifica que MissionPanel esté visible

---

## 📖 Documentación Completa

Ver `SISTEMA_MISIONES_README.md` para documentación detallada y ejemplos avanzados.

---

**🎉 ¡Sistema Listo para Usar!**

Presiona Play, derrota 5 enemigos, y verás el sistema en acción.
