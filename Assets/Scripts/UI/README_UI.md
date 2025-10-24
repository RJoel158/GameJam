HUD de Inventario / Stats - Instrucciones de integración

1) Crear el objeto Player y añadir el componente PlayerStats
   - Selecciona tu GameObject de jugador en la escena.
   - Añade el componente `PlayerStats` (Assets/Scripts/UI/PlayerStats.cs).
   - Ajusta `playerName`, `level`, y `availablePoints` para probar.

2) Crear el Canvas y la ventana HUD
   - Crea un Canvas en la escena (si no tienes uno).
   - Dentro del Canvas crea un Panel para la ventana principal (ej: HUDPanel).
   - Asigna `InventoryHUD.hudPanel` al Panel creado.

3) Añadir textos y filas
   - Dentro del Panel crea Texts para nombre, nivel y puntos disponibles.
   - Crea 3 GameObjects hijos (uno por stat) y añade el componente `StatRow` a cada uno.
   - En cada `StatRow` asigna `labelText` (Text), `valueText` (Text) y `plusButton` (Button).
   - Establece `statType` a Strength, Agility e Intelligence respectivamente.
   - En `InventoryHUD` arrastra las 3 `StatRow` al arreglo `statRows`.

4) Consumible
   - Crea un pequeño panel para consumibles y añade `ConsumableUI`.
   - Asigna el Text al campo `countText`.

5) Conexión final
   - En `InventoryHUD` asigna `playerStats` al componente PlayerStats de tu jugador.
   - Asigna los Texts `nameText`, `levelText`, `pointsText` apropiadamente.
   - Activa/Desactiva la HUD con `InventoryHUD.ToggleHUD()` (puedes enlazarlo a una tecla usando un script pequeño o InputSystem).

6) Pruebas rápidas
   - En modo Play, selecciona el jugador y aumenta `availablePoints` desde el Inspector para probar.
   - Abre la HUD, pulsa + en las filas y observa cómo disminuyen los puntos y aumentan las estadísticas.

Notas:
 - Este sistema es intencionalmente simple y usa `UnityEngine.UI.Text`. Si usas TextMeshPro, cambia los tipos de Text a TMP_Text y añade `using TMPro;`.
 - Puedes ampliar `PlayerStats` para guardar en PlayerPrefs o en un sistema de guardado propio.
