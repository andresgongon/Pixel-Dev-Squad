# Etapa 3: Componente práctico – Prácticas simuladas

**Curso:** Programación para Videojuegos (Código: 213027A)  
**Grupo:** 213027_3  

**Presentado al tutor:**  
Gonzalo Andrés Barrera Rivera  

**Entregado por los estudiantes:**
* Oscar Yesid Agudelo Ardila
* Daniel Enrique Bermejo Polo
* Germán Andrés González González
* Karem Angélica Lucía Malagón Bobadilla
* Eric Daniel Molina Martínez

**Universidad Nacional Abierta y a Distancia - UNAD**  
Escuela de Ciencias Básicas, Tecnologías e Ingenierías - ECBTI  
Octubre de 2026  

---

## Introducción

En esta tercera etapa pasamos del diseño en papel a construir el juego directamente en Unity. Tomamos todo lo que habíamos planeado en el GDD de la Etapa 2 para **Río Limpio Adventure: Misión Reciclaje** y nos enfocamos en armar una escena funcional en 2D que combinara el arte, los efectos de sonido y las reglas de accesibilidad que prometimos.

Desde el principio tuvimos claro que el juego va dirigido a niños de primaria en Colombia (entre 7 y 12 años) bajo la idea de *Games for Change*. Por eso mismo evitamos complicarnos con físicas pesadas que pudieran fallar y montamos toda la mecánica de separación de residuos sobre la interfaz de usuario (`Canvas`) en Unity 6 (`6000.3.20f1 LTS`). Así logramos un juego estable donde cada botella, cáscara o empaque que el niño clasifica en las cuatro canecas mejora el estado del río andino, mientras aplicamos en la práctica las pautas WCAG 2.2 y el diseño universal (DUA).

---

## 1. Documento Técnico del Equipo

### 1.1. Mecánicas implementadas en Unity
El ciclo de juego funciona de forma directa para que un niño de primaria lo entienda sin tener que leer manuales largos. La guardiana ambiental se ubica al lado izquierdo de la pantalla y presenta en el centro un residuo recogido del río. En la parte inferior están ubicadas las cuatro canecas del centro de acopio: **Azul (`[1]` Aprovechables)**, **Roja (`[2]` Especiales y posconsumo)**, **Verde (`[3]` Orgánicos)** y **Gris (`[4]` No aprovechables)**.

Programamos tres formas distintas de clasificar cada residuo para cumplir con la accesibilidad motora:
1. **Arrastrar y soltar (*Drag & Drop*):** El jugador hace clic sostenido sobre el residuo y lo lleva hasta la caneca. Si lo suelta por fuera, el objeto vuelve solo a su sitio inicial para que no se pierda en la pantalla.
2. **Toque a toque (*Click-to-Click*):** Pensando en los niños más pequeños o con dificultades de motricidad fina a los que les cuesta mantener presionado el botón del mouse mientras lo mueven, hicimos que baste con dar un clic sobre el residuo (que se ilumina con un borde amarillo) y luego otro clic sobre la caneca elegida.
3. **Teclado numérico (`1, 2, 3 y 4`):** También dejamos habilitadas las teclas del `1` al `4` para clasificar sin usar el mouse, además de la tecla `R` para reiniciar la partida al terminar.

Cuando el jugador acierta, la caneca muestra un resplandor dorado (*glow*), suena un acorde mayor alegre, sale un globo de texto confirmando el acierto (`¡CLASIFICACIÓN CORRECTA! ✔`) y sube la barra verde de progreso hacia la meta de `80/80`. A medida que esa barra avanza, el río pasa por tres estados: empieza contaminado al `0%`, pasa a recuperación al llegar al `50%` y termina limpio al `100%`, mostrando al final un resumen del impacto ambiental real (como los litros de agua protegidos).

Si el niño se equivoca —por ejemplo, al caer en uno de los *residuos trampa* como la servilleta sucia o el paquete engrasado—, no usamos alarmas agresivas ni límites de tiempo. Suena una gota de agua suave, se descuenta una de las 3 vidas representadas por peces rojos nativos en la esquina superior derecha y el globo central le muestra una pista corta explicándole por qué ese material va en otra caneca.

### 1.2. Decisiones técnicas y organización del código (100% en inglés)
Acordamos trabajar con la versión **Unity `6000.3.20f1 LTS`** usando un `Canvas` configurado en modo *Scale With Screen Size* (`1280x720`). Esta decisión técnica fue clave porque hace que los botones, los residuos y las cuatro canecas conserven su proporción y su área de toque sin importar el tamaño del monitor donde se abra el juego.

Todo el código en C# se organizó dentro de `Assets/Scripts/` siguiendo la regla obligatoria de la guía: nombres de clases, variables, métodos y todos los comentarios explicativos (de funciones, condicionales `if/else` y ciclos `for`) se escribieron completamente en **inglés**:
* **`WasteItemData.cs`:** Contiene el `enum BinCategory` con los cuatro tipos de caneca y la clase serializable `WasteItemData` donde guardamos el nombre del residuo, su sprite, si es o no un residuo trampa y el texto de la pista pedagógica.
* **`WasteItemInteractable.cs`:** Se encarga de la interacción con el residuo usando las interfaces `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler` e `IPointerClickHandler` de `UnityEngine.EventSystems`. Aquí programamos el cálculo del desplazamiento según la escala del `Canvas` y el regreso automático a la posición inicial (`ResetToHomePosition`).
* **`RecyclingBin.cs`:** Va conectado a cada una de las cuatro canecas. Recibe el evento cuando el jugador suelta o hace clic con un residuo, agranda un poco la caneca al pasar el cursor por encima (`OnPointerEnter`) y ejecuta la corrutina `AnimateGlowFeedback()` para encender y apagar suavemente el brillo dorado.
* **`GameManager.cs`:** Controla el estado general de la partida, inicializa el catálogo con los 12 residuos (`Botella`, `Banana`, `Paquete`, `Computador`, `Caja`, `Manzana`, `Servilleta`, `Llanta`, `Lata`, `Naranja`, `Papel` y `Colchon`), revisa en `Update()` si el jugador presiona las teclas `1` a `4` y compara la categoría del residuo con la de la caneca seleccionada.
* **`UIManager.cs` y `AudioManager.cs`:** Separan la interfaz y el sonido de la lógica principal. `UIManager` actualiza los textos del HUD, la barra de relleno, los iconos de los 3 peces y los cambios del fondo del río; mientras que `AudioManager` maneja dos canales (`musicSource` y `sfxSource`) para reproducir la música de fondo (`BackgroundMusic.mp3`) y los efectos de acierto (`CorrectChord.mp3`), fallo (`ErrorWaterDrop.mp3`) y clic (`SelectClick.wav`).

### 1.3. Reparto de roles entre los integrantes
Nos dividimos el trabajo en cinco frentes para que cada integrante tuviera responsabilidades claras tanto en Unity y en la documentación como en sus commits de GitHub:

| Rol en el equipo | Estudiante responsable | Aporte principal en la Etapa 3 |
| :--- | :--- | :--- |
| **Programación en C# y gestión de GitHub** | Germán Andrés González González | Creación de la estructura del proyecto en Unity, programación de los scripts en inglés (`GameManager`, `UIManager`, `RecyclingBin`, `WasteItemInteractable`, `WasteItemData`, `AudioManager`), configuración de controles, actualización del `README.md` y creación del tag `v2.0`. |
| **Arte visual, escenario e interfaz (UI)** | Eric Daniel Molina Martínez | Diseño e integración de los elementos visuales de escenario, personaje guía, contenedores y ajuste de la interfaz gráfica dentro de Unity. |
| **Catálogo de sprites de residuos y accesibilidad** | Daniel Enrique Bermejo Polo | Creación, recorte a fondo transparente y subida al repositorio de los 12 sprites de residuos (`Assets/Sprites/WasteItems/`) y validación de criterios de accesibilidad WCAG 2.2 / DUA. |
| **Integración de audio y efectos sonoros** | Oscar Yesid Agudelo Ardila | Selección, edición y subida al repositorio (`Assets/Audio/`) de la pista musical de fondo (`BackgroundMusic.mp3`) y los efectos de acierto (`CorrectChord.mp3`), error (`ErrorWaterDrop.mp3`) y selección (`SelectClick.wav`). |
| **Documentación técnica y consolidación** | Karem Angélica Lucía Malagón Bobadilla | Estructuración del documento técnico, compilación de evidencias de accesibilidad, sesión de crítica de diseño y revisión de normas APA. |

### 1.4. Enlaces de entrega
* **Repositorio oficial en GitHub:** https://github.com/andresgongon/Pixel-Dev-Squad
* **Etiqueta de versión (Tag `v2.0`):** https://github.com/andresgongon/Pixel-Dev-Squad/releases/tag/v2.0
* **Video demostrativo (YouTube Institucional / Microsoft Stream):** `[PEGAR AQUÍ EL ENLACE DE YOUTUBE UNAD O STREAM ANTES DE ENTREGAR]`

### 1.5. Aprendizajes obtenidos durante la etapa
Montar esta primera versión funcional del juego nos dejó varias enseñanzas prácticas como grupo. Lo primero fue darnos cuenta de que acotar bien el alcance en el GDD nos salvó la entrega: haber decidido resolver todo mediante eventos de interfaz (`EventSystems`) en lugar de enredarnos con físicas complejas nos permitió concentrarnos en que los controles respondieran bien y en que el mensaje educativo sobre el reciclaje en Colombia fuera claro.

También aprendimos que la accesibilidad no es solo teoría para llenar un cuadro en el informe. Al llevar las pautas WCAG 2.2 y el DUA a Unity, vimos por qué es tan importante no depender únicamente del color en las canecas, dejar áreas de clic grandes para los niños y programar alternativas como el modo *Click-to-Click* o las teclas del `1` al `4`. Por último, trabajar todos sobre el mismo repositorio en GitHub nos enseñó a cuidar el orden de las carpetas (`Assets/Scripts`, `Assets/Sprites`, `Assets/Audio`), a no borrar los archivos `.meta` que genera Unity y a mantener el código comentado en inglés para que cualquiera del grupo pudiera entender cómo conectarse a los scripts.

---

## 2. Declaración de Uso de Inteligencia Artificial

De acuerdo con las pautas de uso ético de herramientas generativas establecidas en la guía de actividades (Paso 3), el equipo recurrió a la inteligencia artificial de forma **estrictamente auxiliar y secundaria**. Su rol se limitó a servir como un banco de generación rápida de ideas visuales y bocetos preliminares para agilizar la producción de arte 2D. 

Toda la arquitectura del proyecto, la programación completa de los scripts en C# comentados en inglés (`GameManager`, `UIManager`, `RecyclingBin`, `WasteItemInteractable`, etc.), la integración física y lógica en Unity 6, la musicalización y la implementación de las normas de accesibilidad (WCAG 2.2 / DUA) fueron desarrolladas, configuradas y probadas manualmente por los integrantes del equipo.

### Alcance puntual de la asistencia
* **Bocetos de referencia gráfica:** Se utilizó la herramienta Canva AI (Magic Media / Generador de imágenes) por parte del responsable del Rol 2 únicamente para obtener borradores iniciales de algunos residuos y siluetas base. Las imágenes generadas por IA requerían ajustes obligatorios: ninguna venía con transparencia real utilizable, tenían resoluciones dispares y carecían por completo de las pautas de accesibilidad para daltónicos. El equipo humano se encargó de recortar el canal alfa (`RGBA`), estandarizar tamaños (`512x512 px`), trazar delineados gruesos para contraste y diseñar sobre las canecas los patrones geométricos e iconografía no cromática.
* **Sin intervención en el código base:** La lógica de clasificación, el manejo de eventos táctiles/mouse (`EventSystems`), la alternancia de estados del río y el sistema de vidas fueron concebidos y programados directamente por los estudiantes.

| Recurso / Elemento asistido | Herramienta y versión | Prompt puntual de consulta (en inglés) | Resultado preliminar de la herramienta | Trabajo técnico y posprocesamiento humano |
| :--- | :--- | :--- | :--- | :--- |
| **Bocetos de residuos (`WasteItems`)** | Google Gemini (Imagen 3) | `"Simple 2D cartoon icons of everyday trash: green plastic bottle, cardboard box, banana peel, apple core, isolated on plain background"` | Ilustraciones planas con fondo sólido y estilos visuales heterogéneos. | Selección de elementos acordes al contexto colombiano, eliminación manual del fondo a transparencia `.png`, redimensionamiento uniforme y adición de contornos para legibilidad infantil. |
| **Referencia de estación de acopio (`Backgrounds`)** | Google Gemini (Imagen 3) | `"Cartoon recycling sorting room with wide back windows showing landscape, 2D vector style"` | Una imagen estática con elementos dispersos y sin separación de capas. | Modificación manual del área de ventanales para poder programar en Unity los tres momentos de recuperación ecológica del río (sucio al 0%, transición al 50% y limpio al 100%). |
| **Boceto base de contenedores (`Bins`)** | Google Gemini (Imagen 3) | `"Four simple colored recycling bins blue red green grey, front view"` | Contenedores genéricos cuya única diferencia era el color de pintura. | Como la IA omitió cualquier criterio inclusivo, el equipo rediseñó las canecas agregando patrones en la base (franjas, rombos, puntos, ondas), etiquetas en texto y numeración de teclado `[1-4]` para usuarios con daltonismo. |

---

## 3. Reporte de Accesibilidad (WCAG 2.2 y DUA)

En el GDD de la Etapa 2 nos comprometimos a aplicar cuatro bloques de accesibilidad pensando en niños de 7 a 12 años con distintas capacidades visuales, auditivas o motrices. A continuación detallamos cómo se aplicaron técnicamente en nuestra escena de Unity:

### 3.1. Contraste de color e identificación sin depender del color (WCAG 1.4.1, 1.4.3 y 1.4.11 / DUA Principio I)
* **Verificación en WebAIM Contrast Checker:** Todos los textos del HUD superior y de las canecas utilizan tipografía blanca (`#FFFFFF`) acompañada de un componente `Outline` negro (`#141414`) y placas oscuras de respaldo (`#1F2E3D`), alcanzando una relación de contraste de **13.78:1 a 18.30:1 (Nivel AAA)**, muy por encima del mínimo de `4.5:1`. En el globo central de retroalimentación sobre fondo crema (`#FEF9E7`), usamos verde oscuro (`#1C6B2E`, contraste **6.42:1**) para los aciertos y rojo caoba oscuro (`#8C291E`, contraste **6.85:1**) para las pistas de error.
* **Soporte para usuarios con daltonismo:** Ninguna caneca depende solo del color. Cada una muestra de forma permanente su etiqueta en texto, el número de su tecla rápida (`[1] AZUL - APROVECHABLES`, `[2] ROJA - ESPECIALES`, `[3] VERDE - ORGÁNICOS`, `[4] GRIS - NO APROVECHABLE`) y soporte para patrones e iconografía distinta. Además, los 12 sprites de los residuos tienen un delineado oscuro grueso para que resalten sobre cualquier fondo.

### 3.2. Claridad tipográfica y baja carga cognitiva (WCAG 1.4.4 y 1.4.12 / DUA Principio I)
* Usamos fuente *Sans-serif* en negrita (`Bold`) con tamaño legible y frases cortas en mayúsculas sostenidas para los indicadores principales (`PROGRESO DE LIMPIEZA`, `VIDAS RESTANTES`, `NIVEL FINAL` y el nombre de cada residuo debajo de su imagen). Así evitamos saturar de texto a los niños más pequeños (7 a 9 años) que todavía están fortaleciendo su lectura.

### 3.3. Redundancia sensorial en audio y video (WCAG 1.2.2 y 1.3.3 / DUA Principio I)
* Pensando en estudiantes con discapacidad auditiva o en salones de sistemas escolares donde los computadores no tienen audífonos, **ninguna información del juego se entrega solo por sonido**:
  * **Cuando hay un acierto:** Suena el acorde mayor (`CorrectChord.mp3`) y **al mismo tiempo** se ilumina el brillo dorado (`GlowEffect`) detrás de la caneca, sube el porcentaje de la barra y aparece el globo con el texto *«¡CLASIFICACIÓN CORRECTA! ✔»*.
  * **Cuando hay un fallo:** Suena la gota de agua (`ErrorWaterDrop.mp3`) y **al mismo tiempo** cambia el indicador de los peces rojos en la esquina superior derecha y aparece en pantalla la pista escrita explicando en qué caneca debe ir ese residuo.

### 3.4. Controles alternativos y áreas de toque (WCAG 2.1.1 y 2.5.8 / DUA Principio II)
* **Triple método de control:** En `WasteItemInteractable.cs` y `GameManager.cs` dejamos funcionando al mismo tiempo el arrastre (*Drag & Drop*), la secuencia de dos clics (*Click-to-Click*) y las teclas numéricas `1, 2, 3 y 4`.
* **Tamaño de los botones (`>48x48 dp`) y ritmo libre:** Cada caneca tiene un área interactiva de `175x235 px` y el residuo activo mide `150x150 px` dentro de una resolución base de `1280x720`, superando más de tres veces el tamaño mínimo recomendado de `48x48 dp`. Tampoco pusimos un reloj que haga perder por tiempo, permitiendo que cada niño juegue y aprenda a su propio ritmo.

---

## 4. Evidencia de la Sesión de Crítica de Diseño (*Critique Session*)

Para evaluar cómo se veía y se escuchaba nuestro prototipo antes de cerrar la versión `v2.0`, hicimos una sesión grupal de crítica de diseño. Cada uno de los cinco integrantes trajo a la reunión un videojuego referente internacional para comparar el estilo de arte, el sonido o la accesibilidad, y entre todos acordamos ajustes puntuales sobre nuestra escena en Unity:

1. **Germán Andrés González González — Referente propuesto: *Alba: A Wildlife Adventure* (ustwo games).**
   * *Qué analizamos:* En *Alba* el jugador es una niña que limpia basura y rescata animales en una isla, y lo más chévere es cómo el mapa va cambiando visualmente a medida que limpias, sin usar castigos frustrantes.
   * *Qué mejoramos en nuestro juego:* Acordamos que el progreso no fuera solo subir un número de `0` a `80`, sino que el fondo del río reflejara tres estados claros (`0%` sucio, `50%` recuperándose y `100%` limpio) y que al terminar saliera una pantalla traduciendo los residuos clasificados en litros de agua protegidos.
2. **Eric Daniel Molina Martínez — Referente propuesto: *Toca Nature / Toca Life World* (Toca Boca).**
   * *Qué analizamos:* Revisamos cómo los juegos infantiles de *Toca Boca* usan ilustraciones 2D muy limpias, con bordes gruesos y colores vivos pero sin brillos exagerados que cansen la vista.
   * *Qué mejoramos en nuestro juego:* Definimos que todos los objetos interactivos (las canecas, el personaje y los residuos) mantuvieran un trazo exterior oscuro bien marcado para que los niños distingan rápido qué elementos de la pantalla se pueden tocar y cuáles son parte del fondo.
3. **Oscar Yesid Agudelo Ardila — Referente propuesto: *Stardew Valley* (ConcernedApe).**
   * *Qué analizamos:* Escuchamos cómo *Stardew Valley* maneja los sonidos de interfaz: cuando recoges un objeto suena un tono cálido y satisfactorio, y cuando haces clic en algo inválido suena un toque suave de madera o agua, nunca una alarma molesta.
   * *Qué mejoramos en nuestro juego:* Para el `AudioManager` escogimos una música de fondo tranquila (`BackgroundMusic.mp3`), un acorde mayor limpio para premiar cada acierto (`CorrectChord.mp3`) y un sonido corto de gota de agua (`ErrorWaterDrop.mp3`) cuando el niño falla, cuidando que el sonido acompañe el aprendizaje sin asustar al jugador.
4. **Daniel Enrique Bermejo Polo — Referente propuesto: *Duolingo ABC* y *Dumb Ways to Die*.**
   * *Qué analizamos:* Vimos cómo estos juegos mantienen la atención de los niños variando los objetos y metiendo pequeños retos de atención donde, si te equivocas, el juego te explica rápido la regla para que no vuelvas a fallar en el siguiente intento.
   * *Qué mejoramos en nuestro juego:* En lugar de poner solo 3 o 4 basuras repetidas, armamos un lote de **12 residuos variados** (`Botella`, `Caja`, `Lata`, `Papel`, `Banana`, `Manzana`, `Naranja`, `Computador`, `Llanta`, `Colchon`, `Paquete` y `Servilleta`) metiendo *residuos trampa* (como la servilleta sucia que parece de papel reciclable pero va en la caneca gris) y conectando cada uno con una pista corta en el globo de texto.
5. **Karem Angélica Lucía Malagón Bobadilla — Referente propuesto: *Hue* (Fiddlesticks) y *Among Us* (modo accesible de cables).**
   * *Qué analizamos:* Revisamos cómo *Among Us* corrigió su minijuego de cables agregándole formas y símbolos a cada color porque las personas daltónicas no podían jugarlo, y cómo *Hue* usa texturas para diferenciar colores.
   * *Qué mejoramos en nuestro juego:* Como en nuestra idea inicial las cuatro canecas tenían exactamente el mismo dibujo y solo cambiaban de color, acordamos ponerles etiqueta explícita en mayúsculas, el número de su tecla rápida (`[1]` a `[4]`), iconos distintos y patrones geométricos para que cualquier niño pueda clasificar sin depender únicamente del color.

**Conclusión grupal de la sesión:** Comparar nuestro prototipo con estos cinco juegos nos ayudó a pulir detalles que al principio habíamos pasado por alto, logrando que el arte de los 12 residuos, los sonidos de acierto/error y los controles accesibles funcionen como un solo conjunto coherente para niños de primaria.

---

## Referencias

* CAST. (2018). *Universal Design for Learning (UDL) Guidelines version 2.2*. https://udlguidelines.cast.org/
* CG Spectrum. (2024). *Game Development Career Pathways | CG Spectrum*. https://www.cgspectrum.com/career-pathways/game-development
* Codigofacilito. (2021). *Qué es y cómo hacer un Pull Request* [Video]. YouTube. https://www.youtube.com/watch?v=Zqft6yNRuNs
* De Paz Nuñez, R. (2022). *Trabajo Colaborativo usando GitHub y Visual Studio 2022* [Video]. YouTube. https://www.youtube.com/watch?v=nRh0wqwaV_A
* Games for Change. (2024). *Plataforma de referencia de videojuegos y experiencias inmersivas para el impacto social*. https://www.gamesforchange.org/
* KnowledgeHut upGrad. (2023). *What is Version Control? Git & GitHub Tutorial for Beginners* [Video]. YouTube. https://www.youtube.com/watch?v=eu1KW274sX8
* Ministerio de Ambiente y Desarrollo Sostenible. (2019). *Resolución 2184 de 2019: Por la cual se modifica la resolución 668 de 2016 sobre uso racional de bolsas plásticas y se adoptan otras disposiciones (Código de colores unificado para la separación de residuos en la fuente)*. República de Colombia.
* Normas APA actualizadas 7ª edición. (2020). *Portada*. https://normas-apa.org/estructura/portada/
* Stratvert, K. (2022). *Git and GitHub Tutorial for Beginners* [Video]. YouTube. https://www.youtube.com/watch?v=tRZGeaHPoaw
* Unity Learn. (2019). *Los elementos básicos de Unity*. https://learn.unity.com/course/unity-basics-1
* Unity Learn. (2020). *Guía de recorrido por el Kit de juego en 3D*. https://learn.unity.com/tutorial/guia-de-recorrido-por-el-kit-de-juego-en-3d?language=es
* Velazquez, A. V. (2014). *Etapas del desarrollo cognitivo de Piaget*. ResearchGate. https://www.researchgate.net/publication/327219515_Etapas_del_desarrollo_cognitivo_de_Piaget
* World Wide Web Consortium (W3C). (2023). *Web Content Accessibility Guidelines (WCAG) 2.2*. https://www.w3.org/TR/WCAG22/
* WebAIM. (2024). *Contrast Checker*. https://webaim.org/resources/contrastchecker/
