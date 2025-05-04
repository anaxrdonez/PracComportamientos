using UnityEngine;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Referencias de cámaras")]
    private List<Camera> clienteCams = new List<Camera>();
    private Camera mainCam;
    private int camIndex = -1;

    [Header("Objetos según modo de cámara")]
    public GameObject clienteModeObject;  // Asignar en el Inspector
    public GameObject mainModeObject;     // Asignar en el Inspector

    void Start()
    {
        mainCam = Camera.main;

        // Estado inicial: si arranca en mainCam, mostramos sólo mainModeObject
        clienteModeObject?.SetActive(false);
        mainModeObject?.SetActive(true);
    }

    public void RegistrarCliente(Camera clienteCam)
    {
        if (!clienteCams.Contains(clienteCam))
            clienteCams.Add(clienteCam);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            CambiarCamaraCliente();

        if (Input.GetKeyDown(KeyCode.M))
            ActivarMainCam();
    }

    void CambiarCamaraCliente()
    {
        if (clienteCams.Count == 0) return;

        DesactivarTodas();

        camIndex = (camIndex + 1) % clienteCams.Count;
        clienteCams[camIndex].enabled = true;

        // Toggle de GameObjects
        clienteModeObject?.SetActive(true);
        mainModeObject?.SetActive(false);
    }

    void ActivarMainCam()
    {
        DesactivarTodas();

        if (mainCam != null)
            mainCam.enabled = true;

        camIndex = -1;

        // Toggle de GameObjects
        clienteModeObject?.SetActive(false);
        mainModeObject?.SetActive(true);
    }

    void DesactivarTodas()
    {
        if (mainCam != null)
            mainCam.enabled = false;
        foreach (var cam in clienteCams)
            if (cam != null)
                cam.enabled = false;
    }

    public void DesregistrarCliente(Camera cam)
    {
        if (!clienteCams.Contains(cam)) return;

        if (camIndex >= clienteCams.IndexOf(cam))
            camIndex--;
        clienteCams.Remove(cam);

        if (clienteCams.Count == 0)
            ActivarMainCam();
    }
}
