using UnityEngine;

public class Caneca : MonoBehaviour
{
    [Header("Código Nacional de Colores (Colombia)")]
    public TipoResiduo tipoCaneca;

    [Header("Efecto de Iluminación")]
    public SpriteRenderer glowHighlight;

    private void Start()
    {
        if (glowHighlight) glowHighlight.gameObject.SetActive(false);
    }

    public void Iluminar(bool activar)
    {
        if (glowHighlight) glowHighlight.gameObject.SetActive(activar);
    }
}
