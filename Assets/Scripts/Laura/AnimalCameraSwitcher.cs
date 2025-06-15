using UnityEngine;
using System.Collections.Generic;

public class AnimalCameraSwitcher : MonoBehaviour
{
    private List<Camera> deerCams = new List<Camera>();
    private List<Camera> tigerCams = new List<Camera>();
    private int deerIndex = -1;
    private int tigerIndex = -1;

    void Start()
    {
        // Buscar todas las cámaras hijas de los ciervos
        foreach (var deer in FindObjectsOfType<DeerAI>())
        {
            var cam = deer.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                deerCams.Add(cam);
                cam.enabled = false;
            }
        }

        // Buscar todas las cámaras hijas de los tigres
        foreach (var tiger in FindObjectsOfType<TigerAI>())
        {
            var cam = tiger.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                tigerCams.Add(cam);
                cam.enabled = false;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            CycleDeerCamera();

        if (Input.GetKeyDown(KeyCode.T))
            CycleTigerCamera();

        if (Input.GetKeyDown(KeyCode.M))
            SwitchToMainCamera();
    }

    // Desactiva absolutamente todas las cámaras antes de activar la que toque
    private void DisableAllCameras()
    {
        foreach (var cam in Camera.allCameras)
            cam.enabled = false;
    }

    private void CycleDeerCamera()
    {
        if (deerCams.Count == 0) return;

        DisableAllCameras();
        deerIndex = (deerIndex + 1) % deerCams.Count;
        deerCams[deerIndex].enabled = true;
    }

    private void CycleTigerCamera()
    {
        if (tigerCams.Count == 0) return;

        DisableAllCameras();
        tigerIndex = (tigerIndex + 1) % tigerCams.Count;
        tigerCams[tigerIndex].enabled = true;
    }

    private void SwitchToMainCamera()
    {
        DisableAllCameras();
        // Busca la cámara principal por tag
        var mainCamObj = GameObject.FindWithTag("MainCamera");
        if (mainCamObj != null)
        {
            var cam = mainCamObj.GetComponent<Camera>();
            if (cam != null)
                cam.enabled = true;
        }
        else
        {
            Debug.LogWarning("No se encontró ningún GameObject con tag 'MainCamera'");
        }
    }
}
