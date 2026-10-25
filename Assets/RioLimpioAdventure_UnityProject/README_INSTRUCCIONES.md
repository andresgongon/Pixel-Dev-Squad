# RÍO LIMPIO ADVENTURE - PROYECTO UNITY 2D
### Guía de Montaje Rápido en Unity

Este paquete contiene todos los scripts en C#, sprites, UI e ilustraciones para ejecutar el minijuego de limpieza de ríos y clasificación según el código de colores de Colombia (Resolución 2184):
- **Caneca Blanca**: Residuos aprovechables (Botella PET, Lata de Aluminio, Caja de Cartón).
- **Caneca Verde**: Residuos orgánicos aprovechables (Cáscara de Plátano/Banano, Resto de Manzana, Hojas de Poda).
- **Caneca Negra**: Residuos no aprovechables (Servilletas engrasadas, Empaque de papas fritas, Papel higiénico usado).

---

### Pasos para Configurar en Unity:
1. **Crear o Abrir Proyecto Unity 2D:**
   - Abre Unity Hub y crea un proyecto nuevo seleccionando la plantilla **2D** (Universal 2D o Built-in).
2. **Copiar Carpeta `Assets`:**
   - Descomprime este archivo `.zip`.
   - Arrastra la carpeta `Assets` directamente dentro de la ventana *Project* de Unity (o dentro de la carpeta raíz de tu proyecto).
3. **Ajustar Texturas a Sprites:**
   - Selecciona los PNGs dentro de `Assets/Sprites/...`.
   - En el Inspector de Unity, asegúrate de que el **Texture Type** esté configurado en `Sprite (2D and UI)`.
   - Para el sprite `RioAgua.png`, activa `Wrap Mode: Repeat` para que el script `MovimientoRio.cs` desplace la textura en bucle infinito.
4. **Armar la Escena:**
   - **Fondo:** Arrastra `FondoNaturaleza.png` a la escena.
   - **Río:** Crea un Quad o Sprite con `RioAgua.png` y agrégale el script `MovimientoRio.cs`.
   - **Personaje:** Crea un GameObject 2D con `Pescadora_Normal.png` y asigna el script `GameManager.cs`.
   - **Canecas:** Coloca 3 objetos con los sprites de las Canecas (Blanca, Verde, Negra). A cada una agrégale un `BoxCollider2D` (con *Is Trigger* activo), el Tag `"Caneca"` y el script `Caneca.cs` con su tipo asignado.
   - **Prefab Residuo:** Crea un Prefab arrastrando cualquiera de los residuos, agrega un `BoxCollider2D` y el script `ResiduoDraggable.cs`. Asígnalo al `SpawnerResiduos.cs`.
   - **UI:** Crea un Canvas con:
     - Un `Slider` para el progreso.
     - 3 `Image` con el icono `IconoPezVida.png`.
     - Paneles de Game Over y Victoria.
5. **¡Listo para Play!**
