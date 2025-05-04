using UnityEngine;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    private List<Camera> clienteCams = new List<Camera>();
    private Camera mainCam;
    private int camIndex = -1;

    void Start()
    {
        mainCam = Camera.main;
    }

    public void RegistrarCliente(Camera clienteCam)
    {
        if (!clienteCams.Contains(clienteCam))
        {
            clienteCams.Add(clienteCam);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CambiarCamaraCliente();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ActivarMainCam();
        }
    }

    void CambiarCamaraCliente()
    {
        if (clienteCams.Count == 0) return;

        DesactivarTodas();

        camIndex = (camIndex + 1) % clienteCams.Count;
        clienteCams[camIndex].enabled = true;
    }

    void ActivarMainCam()
    {
        DesactivarTodas();
        if (mainCam != null) mainCam.enabled = true;
        camIndex = -1;
    }

    void DesactivarTodas()
    {
        if (mainCam != null) mainCam.enabled = false;
        foreach (var cam in clienteCams)
        {
            if (cam != null)
                cam.enabled = false;
        }
    }

    public void DesregistrarCliente(Camera cam)
    {
        if (clienteCams.Contains(cam))
        {
            if (camIndex >= clienteCams.IndexOf(cam)) camIndex--;
            clienteCams.Remove(cam);
        }

        if (clienteCams.Count == 0)
        {
            ActivarMainCam();
        }
    }

}
