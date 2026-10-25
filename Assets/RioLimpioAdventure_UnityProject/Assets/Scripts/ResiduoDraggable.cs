using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResiduoDraggable : MonoBehaviour
{
    [Header("Datos del Residuo")]
    public string nombreResiduo;
    public TipoResiduo tipoResiduo;
    public float velocidadFloteRio = 1.5f;

    private bool haSidoPescado = false;
    private bool estaArrastrando = false;
    private Vector3 offsetMouse;
    private Vector3 posicionOriginalPesca;

    private void Update()
    {
        // Si está en el río, se desplaza hacia la izquierda
        if (!haSidoPescado)
        {
            transform.Translate(Vector3.left * velocidadFloteRio * Time.deltaTime);

            // Si sale por la izquierda de la pantalla, se destruye y reaparece otro
            if (transform.position.x < -11f)
            {
                GameManager.Instance.spawner.GenerarSiguienteResiduo();
                Destroy(gameObject);
            }
        }
    }

    private void OnMouseDown()
    {
        haSidoPescado = true;
        estaArrastrando = true;
        posicionOriginalPesca = transform.position;

        GameManager.Instance.OnIniciarPesca();

        Vector3 mouseMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offsetMouse = transform.position - new Vector3(mouseMundo.x, mouseMundo.y, 0f);
    }

    private void OnMouseDrag()
    {
        if (estaArrastrando)
        {
            Vector3 mouseMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouseMundo.x, mouseMundo.y, 0f) + offsetMouse;
        }
    }

    private void OnMouseUp()
    {
        if (!estaArrastrando) return;
        estaArrastrando = false;

        // Comprobar con Raycast2D si cayó encima de una caneca
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Caneca"))
        {
            Caneca caneca = hit.collider.GetComponent<Caneca>();
            if (caneca != null)
            {
                GameManager.Instance.ProcesarClasificacion(tipoResiduo, caneca.tipoCaneca, caneca.transform.position);
                Destroy(gameObject);
                return;
            }
        }

        // Si se soltó fuera de una caneca válida, vuelve a la caña y normaliza expresión
        GameManager.Instance.OnSoltarPescaSinClasificar();
        transform.position = posicionOriginalPesca;
    }
}
