using UnityEngine;
using System.Collections.Generic;

public class SpawnerResiduos : MonoBehaviour
{
    [System.Serializable]
    public struct InfoResiduo
    {
        public string nombre;
        public TipoResiduo tipo;
        public Sprite sprite;
    }

    [Header("Lista de 9 Residuos según Normativa")]
    public List<InfoResiduo> catalogoResiduos = new List<InfoResiduo>();

    [Header("Prefab Base con ResiduoDraggable y SpriteRenderer")]
    public GameObject prefabResiduoBase;

    [Header("Punto de Aparición (Margen derecho del río)")]
    public Transform puntoSpawn;

    private int indiceActual = 0;
    private bool juegoTerminado = false;

    private void Start()
    {
        // Mezclar aleatoriamente los 9 residuos
        MezclarCatalogo();
        GenerarSiguienteResiduo();
    }

    public void MezclarCatalogo()
    {
        for (int i = 0; i < catalogoResiduos.Count; i++)
        {
            InfoResiduo temp = catalogoResiduos[i];
            int rnd = Random.Range(i, catalogoResiduos.Count);
            catalogoResiduos[i] = catalogoResiduos[rnd];
            catalogoResiduos[rnd] = temp;
        }
    }

    public void GenerarSiguienteResiduo()
    {
        if (juegoTerminado || indiceActual >= catalogoResiduos.Count)
        {
            // Si superó la lista, volver a mezclar
            indiceActual = 0;
            MezclarCatalogo();
        }

        if (prefabResiduoBase == null || puntoSpawn == null) return;

        InfoResiduo info = catalogoResiduos[indiceActual];
        indiceActual++;

        // Variación aleatoria de altura en el canal del río
        Vector3 pos = puntoSpawn.position + new Vector3(0f, Random.Range(-0.8f, 0.8f), 0f);

        GameObject nuevo = Instantiate(prefabResiduoBase, pos, Quaternion.identity);
        ResiduoDraggable script = nuevo.GetComponent<ResiduoDraggable>();
        SpriteRenderer sr = nuevo.GetComponent<SpriteRenderer>();

        script.nombreResiduo = info.nombre;
        script.tipoResiduo = info.tipo;
        if (sr && info.sprite)
        {
            sr.sprite = info.sprite;
        }

        // Agregar o ajustar BoxCollider2D
        BoxCollider2D col = nuevo.GetComponent<BoxCollider2D>();
        if (col == null) col = nuevo.AddComponent<BoxCollider2D>();
    }

    public void DetenerSpawner()
    {
        juegoTerminado = true;
    }
}
