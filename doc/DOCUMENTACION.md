# DOCUMENTACION DEL PROYECTO GAMEJAM

## Descripción General
Este proyecto es un juego desarrollado en Unity para un GameJam. El repositorio contiene todos los assets, scripts y configuraciones necesarias para ejecutar y modificar el juego.

## Estructura de Carpetas
- **Assets/**: Contiene todos los recursos del juego (escenas, prefabs, materiales, scripts, modelos, animaciones, etc).
  - `00_Scenes/`: Escenas principales del juego.
  - `01_Prefabs/`: Prefabs utilizados en el juego.
  - `02_Materials/`: Materiales y texturas.
  - `03_UsingPrefabs/`: Prefabs y assets adicionales.
  - `04_Scripts/`: Scripts de lógica y control.
  - `05_EduModels/`, `06_EnemyAnimations/`, `07_PlayerAnimations/`: Modelos y animaciones.
  - Subcarpetas para assets específicos (AllSkyFree, AnimatedCharacters, etc).
- **ProjectSettings/**: Configuración del proyecto Unity.
- **Packages/**: Dependencias y paquetes utilizados.

## Documentación de Pruebas Realizadas
### Pruebas de Occlusion Culling
- Se activó y configuró Occlusion Culling en la escena principal.
- Se verificó que los objetos fuera del campo de visión de la cámara no se renderizan.
- Se comprobó el rendimiento en diferentes áreas del mapa.

### Bugs Encontrados
1. **Algunos objetos no desaparecen correctamente al estar fuera de la vista**
   - Causa: No tienen marcada la opción "Static".
   - Solución en progreso: Marcar los objetos relevantes como "Static" en el Inspector.

2. **Problemas de renderizado con objetos dinámicos**
   - Causa: Occlusion Culling solo afecta objetos estáticos.
   - Solución en progreso: Implementar culling manual para objetos dinámicos usando scripts.

3. **Artefactos visuales al mover la cámara rápidamente**
   - Causa: Parámetros de Occlusion Culling demasiado estrictos.
   - Solución en progreso: Ajustar "Smallest Occluder" y "Smallest Hole" para mejorar la precisión.

4. **Bug de colisión al saltar contra objetos con Capsule Collider**
   - Descripción: Al saltar y chocar contra un objeto con Capsule Collider, la fuerza en el eje X se incrementa de forma inesperada.
   - Causa: El cálculo de la física puede estar sumando fuerzas de rebote lateral por la forma del collider.
   - Solución propuesta: Limitar la fuerza en X al detectar colisión durante el salto, o cambiar el tipo de collider a Box Collider para superficies planas.

5. **El collider de ataque no detecta correctamente los impactos**
   - Descripción: El collider de ataque (por ejemplo, de la espada) no registra correctamente los impactos con enemigos.
   - Causa: El collider puede estar mal posicionado, o el método de detección (OnTriggerEnter/OnCollisionEnter) no está configurado correctamente.
   - Solución propuesta: Revisar la posición y tamaño del collider de ataque, asegurarse de que ambos objetos tengan los tags y capas correctas, y que el método de detección sea el adecuado (usar triggers para armas).

## Soluciones en Progreso

### Soluciones Detalladas

1. **Marcar objetos como "Static" para Occlusion Culling**
    - Revisar todos los objetos del escenario en el Inspector y activar la casilla "Static" para los que no se mueven.
    - Realizar pruebas de rendimiento tras el cambio y verificar que los objetos fuera de la vista se ocultan correctamente.

2. **Culling manual para objetos dinámicos**
    - Implementar un script que utilice `GeometryUtility.CalculateFrustumPlanes(Camera.main)` y `Renderer.bounds` para desactivar objetos dinámicos fuera del frustum.
    - Ejemplo:
       ```csharp
       void Update() {
             Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
             foreach (var obj in dynamicObjects) {
                   obj.SetActive(GeometryUtility.TestPlanesAABB(planes, obj.GetComponent<Renderer>().bounds));
             }
       }
       ```

3. **Ajuste de parámetros de Occlusion Culling**
    - Probar diferentes valores de "Smallest Occluder" y "Smallest Hole" en la ventana de Occlusion Culling.
    - Hacer "Bake" tras cada ajuste y comparar el resultado visual y de rendimiento.

4. **Limitar fuerza lateral en Capsule Collider**
    - En el script de movimiento del jugador, limitar la fuerza en el eje X al detectar colisión con Capsule Collider:
       ```csharp
       void OnCollisionEnter(Collision col) {
             if (col.collider is CapsuleCollider) {
                   Vector3 velocity = rb.velocity;
                   velocity.x = Mathf.Clamp(velocity.x, -maxSideForce, maxSideForce);
                   rb.velocity = velocity;
             }
       }
       ```
    - Alternativamente, usar Box Collider para superficies planas donde no se requiera rebote lateral.

5. **Revisar y ajustar colliders de ataque**
    - Verificar que el collider de ataque esté correctamente posicionado y dimensionado en el prefab del arma.
    - Usar `OnTriggerEnter(Collider other)` para detectar impactos y asegurarse de que el enemigo tenga el tag/capa correcta.
    - Ejemplo:
       ```csharp
       void OnTriggerEnter(Collider other) {
             if (other.CompareTag("Enemy")) {
                   // Aplicar daño
             }
       }
       ```
    - Comprobar que ambos colliders tengan la opción "Is Trigger" activada si se usan triggers.

### Documentación de Bugs

- Para cada bug detectado, se recomienda:
   1. Documentar el contexto y los pasos para reproducirlo.
   2. Registrar el comportamiento esperado y el observado.
   3. Anotar la solución aplicada o en progreso, con referencias a scripts o configuraciones modificadas.
   4. Realizar pruebas posteriores para validar la corrección y actualizar la documentación.

## Notas y Recomendaciones
- Mantener actualizada la documentación de pruebas y bugs.
- Usar prefabs y assets organizados en las carpetas correspondientes.
- Revisar la configuración de física y renderizado para optimizar el rendimiento.
