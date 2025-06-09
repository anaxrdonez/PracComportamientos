using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // Coge la cámara activa (main o cliente)
        Camera cam = CameraSwitcher.CamaraActiva ?? Camera.main;
        if (cam == null) return;

        // Iguala la rotación del canvas a la rotación de la cámara
        transform.rotation = cam.transform.rotation;
    }
}
