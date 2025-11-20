# ⚡ Setup Rápido - Boss con Vida y Energía

## 🎯 Pasos Rápidos (5 minutos)

### 1️⃣ Setup del Boss (2 min)

1. **Abrir el wizard de componentes:**
   - Ve a: `Game Jam > Setup > Boss Components (Health & Energy)`
2. **Seleccionar el boss:**
   - Arrastra el prefab/GameObject del boss al campo "Boss GameObject"
   - El wizard mostrará qué componentes tiene y cuáles faltan
3. **Click en "Setup Boss Components"**
   - Esto añadirá automáticamente:
     - ✅ BossController (sistema de vida/energía)
     - ✅ Rigidbody (para caer cuando esté inconsciente)
     - ✅ Collider (si no tiene)
     - ✅ Configuración correcta

### 2️⃣ Setup de la UI (2 min)

1. **Abrir el wizard de UI:**
   - Ve a: `Game Jam > Setup > Boss UI (Health & Energy Bars)`
2. **(Opcional) Seleccionar Canvas existente**
   - Si quieres usar un Canvas existente, arrástralo al campo
   - Si no, se creará uno nuevo automáticamente
3. **Click en "Create Boss UI"**
   - Esto creará:
     - ✅ Panel con fondo
     - ✅ Barra de vida (roja)
     - ✅ Barra de energía (azul)
     - ✅ Script BossHealthBarUI configurado

### 3️⃣ Tag del Suelo (30 seg)

1. **Selecciona el GameObject del suelo/terrain**
2. **En el Inspector, cambia el Tag a "Ground"**
   - Si "Ground" no existe:
     - Click en Tag → "Add Tag..."
     - Click en el + y escribe "Ground"
     - Vuelve al suelo y asigna el tag

### 4️⃣ Test! (30 seg)

1. **Entra en Play Mode**
2. **Completa la misión de enemigos**
3. **El boss aparecerá después de la cinemática**
4. **Observa cómo:**
   - Las barras aparecen arriba
   - El boss dispara y consume energía (barra azul baja)
   - La energía se regenera lentamente
   - Cuando se queda sin energía, cae inconsciente
   - Después de 5 segundos, despierta y sigue luchando

---

## 🎮 Valores Recomendados

Los valores por defecto están en `BossController`:

```
Vida Máxima: 1000
Energía Máxima: 100
Coste por Disparo: 10 (10 disparos hasta quedar inconsciente)
Regeneración: 5/segundo (tarda 20 segundos en llenar desde 0)
Tiempo Inconsciente: 5 segundos
```

### Para ajustar la dificultad:

**Boss más fácil:**

- Reduce `maxHealth` a 500
- Reduce `energyCostPerShot` a 5
- Aumenta `unconsciousDuration` a 8

**Boss más difícil:**

- Aumenta `maxHealth` a 2000
- Aumenta `energyCostPerShot` a 15
- Reduce `unconsciousDuration` a 3

---

## 🐛 ¿Problemas?

### La UI no aparece:

✅ Asegúrate de que el boss esté activo en la escena
✅ Verifica que `BossHealthBarUI` tenga la referencia a `bossController`

### El boss no cae cuando se queda sin energía:

✅ Verifica que el suelo tenga el tag "Ground"
✅ Asegúrate de que el boss tiene Rigidbody

### La energía no se consume:

✅ Verifica que `God.cs` tenga la integración con BossController
✅ Revisa los logs de consola para ver si hay errores

---

## 📚 Documentación Completa

Para más detalles, ver: `BOSS_HEALTH_ENERGY_SYSTEM.md`

---

**¡Listo para jugar!** 🎉
