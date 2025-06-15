using TMPro;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public TextMeshProUGUI textoEstado;
    public TextMeshProUGUI statsText;


    void LateUpdate()
    {
        // Coge la cámara activa (main o cliente)
        Camera cam = CameraSwitcher.CamaraActiva ?? Camera.main;
        if (cam == null) return;

        // Iguala la rotación del canvas a la rotación de la cámara
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }

    public void UpdateStatsText(int energy, int hunger, int boredom)
    {
        if (statsText != null)  statsText.text = $"Energy: {energy}\nHunger: {hunger}\nBoredom: {boredom}";
    }

    public void ActualizarTexto(string nuevoTexto)
    {
        if (textoEstado != null)
            textoEstado.text = nuevoTexto;
    }
}
