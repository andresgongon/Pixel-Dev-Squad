using UnityEngine;

public class MovimientoRio : MonoBehaviour
{
    [Header("Velocidad de Corriente del Río")]
    public float velocidadX = 0.3f;
    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (rend != null)
        {
            float offsetX = Time.time * velocidadX;
            rend.material.mainTextureOffset = new Vector2(offsetX, 0f);
        }
    }
}
