using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum TipoResiduo
{
    Blanco,  // Aprovechables limpios (plástico, latas, cartón)
    Verde,   // Orgánicos aprovechables (cáscaras, restos de comida)
    Negro    // No aprovechables (servilletas sucias, paquetes de snacks, papel usado)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración del Nivel")]
    public int totalObjetosParaGanar = 9;
    public int vidasIniciales = 3;
    private int vidasRestantes;
    private int residuosClasificadosCorrectos = 0;

    [Header("UI - Progreso y Vidas")]
    public Slider sliderProgreso;
    public TextMeshProUGUI textoProgreso;
    public Image[] iconosPecesVidas; // 3 peces
    public GameObject panelGameOver;
    public GameObject panelVictoria;
    public TextMeshProUGUI textoFeedback;

    [Header("Personaje y Expresiones")]
    public SpriteRenderer personajeRenderer;
    public Sprite spriteNormal;
    public Sprite spriteFuerzaPesca;
    public Sprite spriteFeliz;
    public Sprite spriteTriste;

    [Header("Generador de Residuos")]
    public SpawnerResiduos spawner;

    private Coroutine coroutineEstadoPersonaje;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        vidasRestantes = vidasIniciales;
        residuosClasificadosCorrectos = 0;
        
        if (panelGameOver) panelGameOver.SetActive(false);
        if (panelVictoria) panelVictoria.SetActive(false);
        if (textoFeedback) textoFeedback.text = "";

        ActualizarUI();
    }

    public void OnIniciarPesca()
    {
        CambiarExpresion(spriteFuerzaPesca, 0f); // Se queda en fuerza mientras jala
    }

    public void OnSoltarPescaSinClasificar()
    {
        CambiarExpresion(spriteNormal, 0f);
    }

    public void ProcesarClasificacion(TipoResiduo residuoTipo, TipoResiduo canecaTipo, Vector3 posicionCaneca)
    {
        if (residuoTipo == canecaTipo)
        {
            // Acierto
            residuosClasificadosCorrectos++;
            CambiarExpresion(spriteFeliz, 1.5f);
            MostrarFeedback("¡Excelente! Clasificación Correcta +1", Color.green);
            ActualizarUI();

            if (residuosClasificadosCorrectos >= totalObjetosParaGanar)
            {
                GanarJuego();
            }
            else
            {
                spawner.GenerarSiguienteResiduo();
            }
        }
        else
        {
            // Error
            vidasRestantes--;
            CambiarExpresion(spriteTriste, 1.5f);
            MostrarFeedback("¡Caneca incorrecta! Código Colombia: Blanco (Aprovechables), Verde (Orgánicos), Negro (No aprovechables)", Color.red);
            ActualizarUI();

            if (vidasRestantes <= 0)
            {
                GameOver();
            }
            else
            {
                spawner.GenerarSiguienteResiduo();
            }
        }
    }

    private void CambiarExpresion(Sprite nuevoSprite, float duracionRestaurar)
    {
        if (personajeRenderer == null || nuevoSprite == null) return;

        if (coroutineEstadoPersonaje != null)
            StopCoroutine(coroutineEstadoPersonaje);

        personajeRenderer.sprite = nuevoSprite;

        if (duracionRestaurar > 0)
        {
            coroutineEstadoPersonaje = StartCoroutine(RestaurarSpriteNormalDespuesDe(duracionRestaurar));
        }
    }

    private IEnumerator RestaurarSpriteNormalDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (personajeRenderer && spriteNormal)
        {
            personajeRenderer.sprite = spriteNormal;
        }
    }

    private void ActualizarUI()
    {
        if (sliderProgreso)
        {
            sliderProgreso.value = (float)residuosClasificadosCorrectos / totalObjetosParaGanar;
        }

        if (textoProgreso)
        {
            int porcentaje = Mathf.RoundToInt(((float)residuosClasificadosCorrectos / totalObjetosParaGanar) * 100f);
            textoProgreso.text = $"{residuosClasificadosCorrectos}/{totalObjetosParaGanar} ({porcentaje}%)";
        }

        if (iconosPecesVidas != null)
        {
            for (int i = 0; i < iconosPecesVidas.Length; i++)
            {
                if (iconosPecesVidas[i] != null)
                {
                    iconosPecesVidas[i].enabled = (i < vidasRestantes);
                }
            }
        }
    }

    private void MostrarFeedback(string mensaje, Color color)
    {
        if (textoFeedback)
        {
            textoFeedback.text = mensaje;
            textoFeedback.color = color;
            CancelInvoke(nameof(LimpiarFeedback));
            Invoke(nameof(LimpiarFeedback), 3f);
        }
    }

    private void LimpiarFeedback()
    {
        if (textoFeedback) textoFeedback.text = "";
    }

    private void GanarJuego()
    {
        if (panelVictoria) panelVictoria.SetActive(true);
        if (spawner) spawner.DetenerSpawner();
    }

    private void GameOver()
    {
        if (panelGameOver) panelGameOver.SetActive(true);
        if (spawner) spawner.DetenerSpawner();
    }

    public void ReiniciarNivel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
