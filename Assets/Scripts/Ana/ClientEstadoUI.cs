using TMPro;
using UnityEngine;

public class ClienteEstadoUI : MonoBehaviour
{
    public TextMeshProUGUI textoEstado;
    public Transform camara;

    void Start()
    {
        // Si no se asigna manualmente, intenta encontrar la cámara hija
        if (camara == null)
        {
            Camera cam = GetComponentInParent<Camera>();
            if (cam != null)
                camara = cam.transform;
        }
    }

    void Update()
    {
        if (camara != null)
        {
            // Siempre mira hacia la dirección de la cámara
            transform.LookAt(transform.position + camara.forward);
        }
    }

    public void ActualizarTexto(string nuevoTexto)
    {
        if (textoEstado != null)
            textoEstado.text = nuevoTexto;
    }
}