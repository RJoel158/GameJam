# 🐾 Setup: Enemigo Garras (Fast Enemy)

## 📋 CHECKLIST DE CONFIGURACIÓN

### ✅ PASO 1: Configurar el Modelo FBX

1. **Selecciona `garras_model.fbx` en el Project**
2. En el Inspector, ve a la pestaña **Rig**:
   - Animation Type: **Generic** ⚠️ (usa Generic si da error con Humanoid)
   - Avatar Definition: **Create From This Model**
   - Root Node: **Deja el que aparezca automáticamente**
   - Click **Apply**

3. Ve a la pestaña **Animation**:
   - ✅ Import Animation
   - Loop Time: **según la animación** (Idle/Walk = ✅, Attack/Death = ❌)
   - Click **Apply**

---

### ✅ PASO 2: Configurar los 4 FBX de Animaciones

Ahora que tienes los 4 archivos FBX de animaciones importados (Idle, Walk, Attack, Death), **configura cada uno:**

**Para CADA archivo FBX de animación:**

1. **Selecciona el FBX** en el Project (ejemplo: `Idle.fbx`)

2. En el Inspector, ve a la pestaña **Rig**:
   - Animation Type: **Generic**
   - Avatar Definition: **Copy From Other Avatar**
   - Source: **Click en el círculo y selecciona `garras_model Avatar`** (el avatar de tu modelo principal)
   - Click **Apply**

3. Ve a la pestaña **Animation**:
   - ✅ Import Animation debe estar marcado
   - Verifica que el clip de animación aparezca
   - Loop Time: 
     - ✅ Para Idle y Walk (se repiten)
     - ❌ Para Attack y Death (se ejecutan una vez)
   - Click **Apply**

**Repite esto para los 4 archivos FBX**

⚠️ **IMPORTANTE**: Todos deben usar el mismo Avatar (`garras_model Avatar`) para que funcionen con el mismo esqueleto.

---

### ✅ PASO 3: Localizar los Clips de Animación

**¡No necesitas extraer nada!** Los FBX de Mixamo ya contienen los clips listos para usar.

Para usar las animaciones:

1. **Para cada FBX de animación** (Idle, Walk, Attack, Death):
   - **Expande el FBX** en el Project (click en la flecha ▶)
   - Dentro verás:
     - 📦 Un objeto con el nombre del FBX (el modelo)
     - ▶️ Un clip de animación (ejemplo: "mixamo.com" o el nombre de la animación)

2. **Usa directamente esos clips** en el Animator Controller (Paso 4)
   - No necesitas extraerlos
   - Simplemente arrástralos a los estados del Animator

⚠️ **NOTA**: Si los clips dicen "Read-Only" es normal, puedes usarlos así.

**Verifica que tengas:**
- ✅ Idle.fbx con su clip de animación
- ✅ Walk.fbx (o Running.fbx) con su clip
- ✅ Attack.fbx (con Claws o similar) con su clip
- ✅ Death.fbx (o Dying.fbx) con su clip

---

### ✅ PASO 4: Crear Animator Controller

1. **Click derecho en esta carpeta** → Create → Animator Controller
2. Nómbralo: **`GarrasAnimator`**
3. **Doble click** para abrirlo

4. **Crear Estados:**
   - Click derecho en grid → Create State → Empty
   - Crea: `Idle`, `Walk`, `Attack`, `Death`

5. **Asignar Animaciones:**
   - Selecciona estado `Idle` → En Inspector, Motion: Arrastra animación `Idle`
   - Repite para Walk, Attack, Death

6. **Crear Parámetros:**
   - Panel Parameters (esquina superior izquierda)
   - `+` → Float → Nombre: **`Speed`**
   - `+` → Trigger → Nombre: **`Attack`**
   - `+` → Trigger → Nombre: **`Death`**
   - `+` → Bool → Nombre: **`Dead`** (default: false)

7. **Crear Transiciones:**

**Idle ↔ Walk:**
- Idle → Walk:
  - Condition: `Speed` Greater `0.1`
  - Has Exit Time: ❌
  - Transition Duration: 0.15

- Walk → Idle:
  - Condition: `Speed` Less `0.1`
  - Has Exit Time: ❌
  - Transition Duration: 0.15

**Any State → Attack:**
- Click derecho en Any State → Make Transition → Attack
- Condition: `Attack` (trigger)
- Has Exit Time: ❌
- Transition Duration: 0.1

**Attack → Idle:**
- Has Exit Time: ✅
- Exit Time: 0.8-0.9
- Transition Duration: 0.2

**Any State → Death:**
- Condition: `Death` (trigger) OR `Dead` (true)
- Has Exit Time: ❌
- Can Transition To Self: ❌

---

### ✅ PASO 5: Crear el Prefab del Enemigo

**⚠️ Abre tu escena principal del juego primero** (donde están tus enemigos actuales)

1. **En la Hierarchy, crea un GameObject vacío:**
   - Click derecho en Hierarchy → Create Empty
   - Nómbralo: **"FastEnemy_Garras"**

2. **Arrastra el modelo** `garras_model` **desde el Project** como hijo de FastEnemy_Garras en la Hierarchy

3. **Agregar componentes al GameObject padre:**

**A) NavMeshAgent:**
- Add Component → NavMeshAgent
- Configurar:
  - Speed: **4.5** (rápido)
  - Angular Speed: **250**
  - Acceleration: **12**
  - Stopping Distance: **1.0**
  - Radius: **0.35** (ajusta según tamaño)
  - Height: **ajusta según modelo**

**B) Animator:**
- Add Component → Animator
- Controller: Arrastra **GarrasAnimator**
- Apply Root Motion: ❌
- Avatar: Arrastra **garras_model Avatar** (del FBX garras_model)

**⚠️ IMPORTANTE - Configurar referencias del Enemy:**
- Con FastEnemy_Garras seleccionado, busca en el Inspector el componente **Fast Enemy (Script)**
- Verás campos que dicen "None":
  - **Player**: Arrastra el GameObject del jugador desde la Hierarchy
  - **Agent**: Arrastra el componente NavMeshAgent (del mismo objeto)
  - **Animator**: Arrastra el componente Animator (del mismo objeto o del hijo garras_model)

**C) CapsuleCollider:**
- Add Component → Capsule Collider
- Ajusta Center, Radius y Height al modelo
- Is Trigger: ❌

**D) Rigidbody:**
- Add Component → Rigidbody
- Use Gravity: ✅
- Is Kinematic: ❌
- Constraints: Freeze Rotation X, Y, Z

**E) FastEnemy Script:**
- Add Component → FastEnemy
- Configurar:
  - Speed Multiplier: **1.8**
  - Attack Cooldown Multiplier: **0.5**
  - Fast Enemy Health: **50**
  - Fast Enemy Damage: **8**

**⚠️ CRÍTICO - Asignar Referencias (o no funcionará):**

El script FastEnemy hereda de Enemy.cs y necesita estas referencias:

1. **Con FastEnemy_Garras seleccionado**, en el Inspector busca el componente **Fast Enemy (Script)**
2. **Llena estos campos:**
   - **Player**: Arrastra el GameObject del jugador desde la Hierarchy (busca "Player" o "PlayerArmature")
   - **Agent**: Arrastra el componente **Nav Mesh Agent** (está en el mismo FastEnemy_Garras)
   - **Animator**: Arrastra el componente **Animator** (está en el mismo FastEnemy_Garras)

3. **Verifica otros campos:**
   - Attack CD: **3** (se reducirá automáticamente por el multiplier)
   - Attack Range: **1**
   - Aggro Range: **4** (o más si quieres que detecte de lejos)
   - Ground Mask: **Everything** (o selecciona las capas de terreno)

**Sin estas referencias asignadas, el enemigo NO se moverá ni animará.**

---

### ✅ PASO 6: Crear el Arma/Garras

Como este modelo no tiene huesos/armature, crearemos el collider de ataque directamente:

1. **En la Hierarchy, click derecho en `garras_model`** (el hijo de FastEnemy_Garras)
   - Create Empty
   - Nómbralo: **"GarraWeapon"**

2. **Selecciona GarraWeapon** en el Inspector:
   - Ajusta su **Transform Position** para colocarlo frente al enemigo donde están las garras
   - Ejemplo: Position (0, 0.5, 0.5) - ajusta según tu modelo
   - En Scene view verás un pequeño ícono, muévelo donde quieras que esté el área de ataque

3. **Con GarraWeapon seleccionado**, agrégale:
   - Add Component → **Sphere Collider**
   - **Is Trigger: ✅** (muy importante)
   - Radius: **0.4** (ajusta según el tamaño de las garras)
   - En Scene view deberías ver la esfera verde del collider

4. **Add Component → Script:** Busca **GarraWeapon** (lo crearemos ahora)

⚠️ **NOTA**: Como el modelo no tiene animación de huesos, el collider estará fijo. Para mejores resultados, colócalo en el centro-frente del enemigo donde alcanzaría con sus garras.

---

**Crear el script GarraWeapon.cs:**

1. En la carpeta `04_Scripts`, click derecho → Create → C# Script
2. Nómbralo: **`GarraWeapon`**
3. Abre el script y reemplaza todo con este código:
```csharp
using UnityEngine;

public class GarraWeapon : MonoBehaviour
{
    private FastEnemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<FastEnemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<StarterAssets.ThirdPersonController>();
            if (player != null && enemy != null)
            {
                player.TakeDamage(enemy.GetDamage());
                Debug.Log($"<color=red>[GarraWeapon] ¡Golpeaste al jugador por {enemy.GetDamage()} de daño!</color>");
            }
        }
    }
}
```

---

### ✅ PASO 7: Crear el Prefab Final

1. **Arrastra FastEnemy_Garras** desde Hierarchy → Esta carpeta
2. Ahora tienes el prefab listo para usar

---

### ✅ PASO 8: Añadir a Campamentos

**Opción A - Campamentos Existentes:**
1. Abre un prefab de EnemyCamp
2. Arrastra 2-3 FastEnemy_Garras junto a los enemigos normales
3. Guarda el prefab

**Opción B - Nuevo Campamento:**
1. Crea GameObject vacío: "FastEnemyCamp"
2. Add Component → EnemyCamp
3. Configura: Enemies Needed = 3
4. Coloca 3 FastEnemy_Garras como hijos

---

## 🎮 TESTING

1. **Play** en Unity
2. Acércate al campamento
3. **Verifica:**
   - ✅ Los enemigos se mueven rápido
   - ✅ Atacan más frecuentemente
   - ✅ Hacen menos daño (~8)
   - ✅ Mueren con menos vida (~50)

---

## 🐛 TROUBLESHOOTING

**Problema: No se mueve**
- ✓ Verifica que el terreno tenga NavMesh (Window → AI → Navigation → Bake)
- ✓ NavMeshAgent debe estar en el GameObject padre
- ✓ Aggro Range debe ser mayor (prueba con 10)

**Problema: Animaciones no funcionan / NullReferenceException**
- ✓ El Animator debe estar en FastEnemy_Garras (padre), NO en garras_model (hijo)
- ✓ En FastEnemy script, el campo "Animator" debe estar asignado (arrastra el Animator del mismo objeto)
- ✓ En FastEnemy script, el campo "Player" debe estar asignado
- ✓ En FastEnemy script, el campo "Agent" debe estar asignado
- ✓ Avatar del Animator debe ser "garras_model Avatar"
- ✓ Animator Controller bien asignado (GarrasAnimator)
- ✓ Transiciones configuradas correctamente en GarrasAnimator
- ✓ Parámetros con nombres exactos (Speed, Attack, Death, Dead)
- ✓ Estado Idle debe ser naranja (default state)

**Problema: Se mueve de lado**
- ✓ Verifica que el modelo esté orientado correctamente en la Scene view
- ✓ Cuando lo importaste de Tripo, el modelo puede estar rotado
- ✓ Selecciona garras_model (hijo) y rota en Y hasta que mire hacia adelante (hacia Z+)
- ✓ O ajusta la rotación del GameObject padre

**Problema: No puedo hacerle daño**
- ✓ FastEnemy_Garras debe tener Layer "Default" o "Enemy"
- ✓ Tu arma debe tener un script que llame TakeDamage(int) cuando colisiona
- ✓ Verifica que tu arma tenga collider trigger
- ✓ El enemigo debe tener CapsuleCollider (NO trigger)

**Problema: No detecta al jugador**
- ✓ Tag "Player" asignado al jugador
- ✓ Campo "Player" asignado en FastEnemy script
- ✓ Campo "Player Third Person Controller" se llena automático (si no, hay problema con el tag)

**Problema: Me hace daño pero el enemigo no hace daño visual**
- ✓ GarraWeapon tiene collider trigger
- ✓ Script GarraWeapon asignado
- ✓ GarraWeapon está posicionado frente al enemigo

---

## 📊 STATS FINALES

```
ENEMIGO NORMAL    vs    FAST ENEMY GARRAS
─────────────────────────────────────────
Vida:      100           50  (-50%)
Daño:      15            8   (-47%)
Velocidad: 2.5           4.5 (+80%)
AttackCD:  3.0s          1.5s (-50%)
```

---

**¿Necesitas ayuda?** Revisa la consola de Unity para logs con [FastEnemy] que te dirán si algo no está configurado correctamente.
