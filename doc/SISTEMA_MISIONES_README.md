# Sistema de Misiones - Derrotar Enemigos

Sistema completo de misiones para Unity donde la misión principal es derrotar X enemigos y guardar sus posiciones usando ScriptableObjects.

## 📁 Archivos Creados

- `DefeatEnemiesMission.cs` - ScriptableObject que define una misión de derrotar enemigos
- `MissionManager.cs` - Singleton que gestiona las misiones activas
- `MissionUI.cs` - Componente de UI para mostrar progreso de misión
- `MissionDebugHelper.cs` - Herramienta de debug para probar misiones
- `Enemy.cs` - Modificado para emitir evento cuando muere (con posición)

## 🚀 Setup Rápido

### 1. Crear una Misión (ScriptableObject)

1. En Unity, clic derecho en Project → `Create > Missions > Defeat Enemies Mission`
2. Nombra el asset, por ejemplo: `Mission_Defeat5Enemies`
3. En el Inspector, configura:
   - **Mission Name**: "Defeat 5 Enemies"
   - **Description**: "Defeat 5 enemies to complete this mission"
   - **Enemies Required**: 5

### 2. Configurar MissionManager en la Escena

1. Crea un GameObject vacío en la escena: `GameObject > Create Empty`
2. Renómbralo a "MissionManager"
3. Añade el componente `MissionManager`
4. En el Inspector:
   - Arrastra tu misión creada al array `Available Missions`

### 3. Configurar UI (Opcional pero recomendado)

#### Opción A: UI Simple de TextMeshPro

1. Crea un Canvas: `GameObject > UI > Canvas`
2. Dentro del Canvas crea:

   - Panel para la misión: `GameObject > UI > Panel`
   - Textos: `GameObject > UI > Text - TextMeshPro` (para nombre, descripción, progreso)
   - Slider: `GameObject > UI > Slider` (para barra de progreso)

3. Añade componente `MissionUI` al Canvas
4. Asigna las referencias en el Inspector

#### Opción B: Sin UI

Si no quieres UI, simplemente NO añadas el componente MissionUI. Las misiones funcionarán igual y verás el progreso en la consola.

### 4. Añadir Helper de Debug (Recomendado)

1. En el GameObject "MissionManager" (o crea uno nuevo), añade `MissionDebugHelper`
2. Marca `Start Mission On Start` si quieres que la misión inicie automáticamente
3. Configura `Mission Index To Start` (0 para la primera misión)

### 5. ¡Jugar!

1. Presiona Play
2. La misión iniciará automáticamente (si configuraste el helper)
3. Derrota enemigos para ver el progreso
4. En Scene View verás marcadores rojos en las posiciones donde derrotaste enemigos

## 🎮 Cómo Usar

### Iniciar Misión Desde Script

```csharp
// Obtener el manager
MissionManager manager = MissionManager.Instance;

// Iniciar por índice
manager.StartMission(0);

// O iniciar una misión específica
manager.StartMission(miMision);
```

### Iniciar Misión Desde Inspector

1. Selecciona el GameObject con `MissionDebugHelper`
2. Clic derecho en el componente → `Start Mission`

### Ver Progreso

En la consola verás:

- Cuando inicia la misión
- Cada vez que derrotas un enemigo (con su posición)
- Cuando completas la misión (con todas las posiciones)

En Scene View (con Gizmos activados):

- Esferas rojas en cada posición de derrota
- Líneas verticales para mejor visibilidad
- Números de enemigo

## 📊 Acceder a los Datos de la Misión

### Desde Script

```csharp
MissionManager manager = MissionManager.Instance;
DefeatEnemiesMission currentMission = manager.currentMission;

// Obtener progreso
int enemiesDefeated = currentMission.enemiesDefeated;
int enemiesRequired = currentMission.enemiesRequired;
float percentage = currentMission.GetProgressPercentage();

// Obtener posiciones guardadas
List<Vector3> positions = currentMission.defeatPositions;

// Verificar si está completada
bool completed = currentMission.isCompleted;
```

### Desde el Inspector

1. Durante el juego, selecciona el asset de la misión en Project
2. Verás los valores actualizándose en tiempo real:
   - `Enemies Defeated`
   - `Is Active`
   - `Is Completed`
   - `Defeat Positions` (lista de Vector3)

## 🔧 Personalización

### Crear Otros Tipos de Misión

Para crear un tipo diferente de misión:

1. Crea una nueva clase que herede de `ScriptableObject`
2. Define tus propios campos (objetivos, recompensas, etc.)
3. Suscríbete a los eventos que necesites (por ejemplo, `Enemy.OnEnemyDefeated`)
4. Implementa la lógica de progreso y completado

Ejemplo básico:

```csharp
[CreateAssetMenu(menuName = "Missions/Custom Mission")]
public class CustomMission : ScriptableObject
{
    public string missionName;
    public bool isCompleted;

    public void StartMission()
    {
        // Tu lógica aquí
        Enemy.OnEnemyDefeated += OnEnemyDefeated;
    }

    void OnEnemyDefeated(Vector3 position)
    {
        // Tu lógica de progreso
    }
}
```

### Modificar UI

Edita `MissionUI.cs` para cambiar:

- Colores
- Animaciones
- Posición
- Estilo de visualización

## 🐛 Debug y Testing

### Comandos del Helper (clic derecho en componente)

- **Start Mission** - Inicia la misión configurada
- **Cancel Current Mission** - Cancela la misión actual
- **Show Mission Progress** - Muestra progreso detallado en consola
- **Reset Current Mission** - Reinicia la misión (útil para testing)

### Visualización de Gizmos

En `MissionManager` y `MissionDebugHelper` hay Gizmos que muestran las posiciones donde derrotaste enemigos. Asegúrate de tener Gizmos activados en Scene View.

## ⚙️ Características

✅ Sistema basado en ScriptableObjects (datos persistentes entre sesiones de editor)
✅ Soporte para múltiples tipos de misión
✅ Sistema de eventos desacoplado
✅ UI opcional y personalizable
✅ Herramientas de debug integradas
✅ Visualización en Scene View
✅ Fácil de extender para nuevos tipos de misión

## 📝 Notas Importantes

- Las posiciones se guardan en **espacio mundial** (world space)
- Los datos de la misión se mantienen en el ScriptableObject incluso al salir de Play Mode
- Para "resetear" una misión entre tests, usa el botón **Reset Current Mission** del helper
- El sistema usa un singleton para MissionManager (solo debe haber uno en la escena)

## 🎯 Próximos Pasos Sugeridos

1. Crear diferentes tipos de misión (recolección, exploración, etc.)
2. Añadir sistema de recompensas
3. Guardar misiones completadas en un archivo persistente
4. Añadir misiones en cadena (quest chains)
5. Sistema de diálogos para entregar/completar misiones

## 💡 Ejemplo de Uso Completo

```csharp
public class GameController : MonoBehaviour
{
    public DefeatEnemiesMission missionToStart;

    void Start()
    {
        // Esperar 2 segundos y empezar misión
        Invoke(nameof(StartFirstMission), 2f);
    }

    void StartFirstMission()
    {
        MissionManager.Instance.StartMission(missionToStart);
    }
}
```

---

**¡Listo para usar!** 🎮
