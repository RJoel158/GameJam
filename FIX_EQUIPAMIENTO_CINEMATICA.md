# 🎬 Fix v2: Equipamiento Bugueado Después de Cinemática

## 🐛 Problema Actualizado

Después de la cinemática del boss:

- ❌ El jugador **no podía atacar**
- ❌ **No podía desenvainar/envainar** la espada
- ❌ Se quedaba **bugueado** al intentar equipar
- ❌ Animaciones inconsistentes

### Causa Raíz v2

**Problema:** Estábamos **reseteando** `Equipped` y `isEquipped` a `false` sin importar el estado previo del jugador.

```csharp
// ❌ ANTES (perdía el estado del arma)
playerAnimator.SetBool("Equipped", false); // Siempre false!
playerCtrl.enabled = true; // isEquipped quedaba inconsistente
```

**Resultado:**

- Si el jugador tenía la espada desenvainada ANTES de la cinemática
- Al salir, el `Animator` decía `Equipped = false` pero visualmente podía tener la espada
- El `PlayerController.isEquipped` no coincidía con el Animator
- El sistema de combate se confundía

---

## ✅ Solución v2: Guardar y Restaurar Estado

### Paso 1: Variables de Estado

Añadimos variables para **guardar** el estado antes de la cinemática:

```csharp
// Estados guardados del jugador antes de la cinemática
private bool playerWasEquipped = false;
private bool playerWasBlocking = false;
private PlayerController savedPlayerController = null;
```

### Paso 2: GUARDAR Estado (DisablePlayerControls)

**A. Guardar estado de PlayerController:**

```csharp
var playerCtrl = FindAnyObjectByType<PlayerController>();
if (playerCtrl != null)
{
    // ✅ GUARDAR antes de deshabilitar
    savedPlayerController = playerCtrl;
    playerWasEquipped = playerCtrl.isEquipped;
    playerWasBlocking = playerCtrl.isBlocking;

    Debug.Log($"Saved PlayerController: isEquipped={playerWasEquipped}");

    playerCtrl.enabled = false;
}
```

**B. Guardar estado del Animator (con redundancia):**

```csharp
var playerAnimator = FindAnyObjectByType<ThirdPersonController>()?.GetComponent<Animator>();
if (playerAnimator != null)
{
    // ✅ Guardar desde Animator si no tenemos PlayerController
    if (savedPlayerController == null)
    {
        playerWasEquipped = playerAnimator.GetBool("Equipped");
        playerWasBlocking = playerAnimator.GetBool("Block");
    }

    Debug.Log($"Saved animator: Equipped={playerWasEquipped}");

    // LUEGO resetear para la cinemática (Idle limpio)
    playerAnimator.SetBool("Equipped", false);
    playerAnimator.SetBool("Block", false);
    playerAnimator.Play("Idle", 0, 0f);
}
```

### Paso 3: RESTAURAR Estado (EnablePlayerControls)

**A. Restaurar PlayerController:**

```csharp
var playerCtrl = FindAnyObjectByType<PlayerController>();
if (playerCtrl != null)
{
    // ✅ RESTAURAR el estado guardado
    playerCtrl.isEquipped = playerWasEquipped;
    playerCtrl.isBlocking = playerWasBlocking;
    playerCtrl.isEquipping = false; // No está en medio de equipar

    Debug.Log($"Restored PlayerController: isEquipped={playerWasEquipped}");

    playerCtrl.enabled = true;
}
```

**B. Restaurar Animator:**

```csharp
var playerAnimator = FindAnyObjectByType<ThirdPersonController>()?.GetComponent<Animator>();
if (playerAnimator != null)
{
    // Reset triggers
    playerAnimator.ResetTrigger("Attack");
    playerAnimator.ResetTrigger("DrawSword");
    // ... etc

    // ✅ RESTAURAR bools guardados (NO hardcodear a false)
    playerAnimator.SetBool("Block", playerWasBlocking);
    playerAnimator.SetBool("Equipped", playerWasEquipped);
    playerAnimator.SetBool("Grounded", true);

    Debug.Log($"Restored animator: Equipped={playerWasEquipped}");

    // Reset floats
    playerAnimator.SetFloat("Speed", 0f);
    playerAnimator.SetFloat("MotionSpeed", 0f);

    // ✅ Forzar al estado correcto según equipamiento
    if (playerWasEquipped)
    {
        // Tenía espada → volver a IdleEquipped
        playerAnimator.Play("IdleEquipped", 0, 0f);
        Debug.Log("Player restored to IdleEquipped");
    }
    else
    {
        // No tenía espada → volver a Idle normal
        playerAnimator.Play("Idle", 0, 0f);
        Debug.Log("Player restored to Idle");
    }

    // Forzar actualización inmediata
    playerAnimator.Update(0f);
}
```

---

## 🔍 Comparación Antes/Después

### ❌ ANTES (Bugueado):

```csharp
// Durante cinemática
playerAnimator.SetBool("Equipped", false); // Resetear a false

// Después de cinemática
playerAnimator.SetBool("Equipped", false); // Siempre false
playerAnimator.Play("Idle", 0, 0f);        // Siempre Idle
```

**Resultado:** Si el jugador tenía la espada, se perdía el estado.

### ✅ AHORA (Funcional):

```csharp
// Durante cinemática
playerWasEquipped = playerCtrl.isEquipped;  // GUARDAR
playerAnimator.SetBool("Equipped", false);  // Resetear temporalmente

// Después de cinemática
playerCtrl.isEquipped = playerWasEquipped;         // RESTAURAR
playerAnimator.SetBool("Equipped", playerWasEquipped); // RESTAURAR

if (playerWasEquipped)
    playerAnimator.Play("IdleEquipped", 0, 0f); // Estado correcto
else
    playerAnimator.Play("Idle", 0, 0f);         // Estado correcto
```

**Resultado:** El jugador sale con la espada en el mismo estado que entró.

---

## 🎯 Estados del Animator

### Estados Posibles:

1. **`Idle`** - Sin espada, idle normal
2. **`IdleEquipped`** - Con espada desenvainada, idle con espada

### Flujo Correcto:

```
ANTES CINEMÁTICA:
  playerWasEquipped = true
  Estado: "IdleEquipped"
  ↓
DURANTE CINEMÁTICA:
  Estado: "Idle" (temporal, para cinemática limpia)
  ↓
DESPUÉS CINEMÁTICA:
  Restaurar playerWasEquipped = true
  Estado: "IdleEquipped" ← CORRECTO!
```

---

## 📋 Checklist de Verificación

Después de aplicar el fix, verifica:

### Con Espada Desenvainada:

1. ✅ Desenvainar espada ANTES de completar misión
2. ✅ Completar misión → cinemática se activa
3. ✅ Durante cinemática, jugador aparece sin espada (normal)
4. ✅ Después de cinemática, **jugador tiene espada desenvainada**
5. ✅ Puede **atacar inmediatamente**
6. ✅ Puede **envainar con R**
7. ✅ Puede **desenvainar nuevamente con R**

### Sin Espada:

1. ✅ Completar misión SIN desenvainar
2. ✅ Después de cinemática, **jugador NO tiene espada**
3. ✅ Puede **desenvainar con R**
4. ✅ Puede **atacar después de desenvainar**

---

## 🧪 Testing Detallado

### Test 1: Con Espada

```
1. Desenvainar espada (R)
2. Completar misión de enemigos
3. Esperar cinemática
4. Verificar: ¿Espada visible? → SÍ ✅
5. Probar ataque → Funciona ✅
6. Envainar (R) → Funciona ✅
7. Desenvainar (R) → Funciona ✅
```

### Test 2: Sin Espada

```
1. NO desenvainar (espada en espalda)
2. Completar misión
3. Esperar cinemática
4. Verificar: ¿Espada en espalda? → SÍ ✅
5. Desenvainar (R) → Funciona ✅
6. Atacar → Funciona ✅
```

### Test 3: Durante Animación de Desenvainar

```
1. Presionar R (empezar a desenvainar)
2. INMEDIATAMENTE completar misión
3. Cinemática interrumpe
4. Verificar estado después: Debería estar correcto ✅
```

---

## 🐛 Bugs Solucionados

### Bug 1: No Podía Atacar

**Causa:** `Equipped = false` pero visualmente tenía espada  
**Fix:** Restaurar `Equipped` al valor guardado

### Bug 2: No Podía Desenvainar

**Causa:** `isEquipped` inconsistente entre PlayerController y Animator  
**Fix:** Restaurar ambos al mismo valor guardado

### Bug 3: Animación Bugueada al Desenvainar

**Causa:** Estado Idle incorrecto para el equipamiento actual  
**Fix:** Usar `IdleEquipped` vs `Idle` según `playerWasEquipped`

---

## 📝 Logs de Debug

Al ejecutar, verás en consola:

```
Durante DisablePlayerControls:
[BossSpawner] Saved PlayerController state: isEquipped=True, isBlocking=False
[BossSpawner] Saved animator state: Equipped=True, Blocking=False

Durante EnablePlayerControls:
[BossSpawner] Restored PlayerController state: isEquipped=True
[BossSpawner] Restored animator state: Equipped=True, Blocking=False
[BossSpawner] Player restored to IdleEquipped state
```

Si ves `isEquipped=False` pero el jugador tenía espada, hay un problema.

---

## 🔧 Archivos Modificados

**`BossSpawner.cs`**

- Añadidas variables: `playerWasEquipped`, `playerWasBlocking`, `savedPlayerController`
- Modificado: `DisablePlayerControls()` - Guarda estado
- Modificado: `EnablePlayerControls()` - Restaura estado

---

## 💡 Notas Importantes

### ¿Por qué Guardar Dos Veces?

```csharp
// 1. Desde PlayerController
playerWasEquipped = playerCtrl.isEquipped;

// 2. Desde Animator (si playerCtrl es null)
if (savedPlayerController == null)
    playerWasEquipped = playerAnimator.GetBool("Equipped");
```

**Razón:** Redundancia. Si por alguna razón no encontramos PlayerController, podemos leer desde el Animator. Es un fallback de seguridad.

### ¿Por qué Play("IdleEquipped")?

Si simplemente hacemos `SetBool("Equipped", true)`, el Animator tiene que transicionar desde Idle → IdleEquipped. Esto puede tomar frames y causar animaciones intermedias extrañas.

Con `Play("IdleEquipped", 0, 0f)`, **saltamos directamente** al estado correcto, sin transiciones.

---

**Fix aplicado en:** `BossSpawner.cs`  
**Versión:** v2 (Restauración de Estado)  
**Fecha:** 2025-11-09  
**Estado:** ✅ Completado - Sistema de Equipamiento Funcional
