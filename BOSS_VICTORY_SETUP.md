# CONFIGURACIÓN DEL SISTEMA DE VICTORIA

## VictoryManager - Secuencia de Victoria del Boss

Cuando el boss (God) muere, se activa una secuencia de victoria que incluye:

1. Reproducción del video de cinemática final (`CinematicaFinal.mp4`)
2. Pantalla "YOU WIN" con fade-in
3. Retorno automático al menú principal

### Setup en Unity Editor

#### 1. Crear GameObject VictoryManager

1. En la escena `FinalBoss.unity`, crear un GameObject vacío llamado `VictoryManager`
2. Agregar el componente `VictoryManager.cs`

#### 2. Configurar componentes del VictoryManager

##### Video Settings:

- **Video Path**: `Assets/00_Scenes/CinematicaFinal.mp4` (ya configurado por defecto)
- **Video Player**: Agregar componente `VideoPlayer` al mismo GameObject
- **Video Display**:
  - Crear UI Canvas si no existe
  - Crear RawImage en el Canvas llamada `VideoDisplay`
  - Escalarla a pantalla completa
  - Arrastrar la RawImage al campo `videoDisplay` del VictoryManager

##### Victory Screen:

- **You Win Image**:
  - Crear una UI Image en el Canvas llamada `YouWinImage`
  - Importar imagen "YOU WIN" o usar Text con estilo dramático
  - Configurar tamaño y posición (centrada)
  - Arrastrar al campo `youWinImage` del VictoryManager
- **You Win Fade Duration**: 1.5 segundos (por defecto)
- **You Win Display Time**: 3 segundos (por defecto)

##### Audio (Opcional):

- **Victory Music**: Arrastrar AudioClip de música de victoria si deseas
- **Victory Music Volume**: 0.8 (por defecto)

##### Scene:

- **Main Menu Scene Name**: Cambiar a `"Game"` o el nombre correcto de tu menú principal

### Código integrado

El script `God.cs` ya está modificado para llamar automáticamente al VictoryManager:

```csharp
if (health <= 0)
{
    if (VictoryManager.Instance != null)
    {
        VictoryManager.Instance.TriggerVictory();
    }
    Destroy(gameObject);
}
```

### Flujo de la secuencia

1. Player derrota al boss (God health <= 0)
2. `God.TakeDamage()` detecta muerte
3. Llama a `VictoryManager.Instance.TriggerVictory()`
4. Se reproduce el video `CinematicaFinal.mp4`
5. Cuando termina el video, aparece "YOU WIN" con fade-in
6. Después de 3 segundos, se carga automáticamente el menú principal

### Troubleshooting

- **Video no se reproduce**: Verificar que el path sea correcto y que VideoPlayer esté asignado
- **YOU WIN no aparece**: Verificar que youWinImage esté asignada y tenga alpha inicial en 0
- **No vuelve al menú**: Verificar que mainMenuSceneName coincida con el nombre exacto de la escena en Build Settings
- **VictoryManager no encontrado**: Asegurarse de que el GameObject VictoryManager existe en la escena FinalBoss

### Nombre de escena del menú

Actualmente configurado como `"MainMenu"` por defecto.
**IMPORTANTE**: Cambiar `mainMenuSceneName` en el inspector a:

- `"Game"` si la escena Game.unity es el menú principal
- O el nombre correcto de tu escena de menú principal

También asegúrate de que la escena esté agregada en:
`File > Build Settings > Scenes In Build`
