using UnityEngine;
using System.Collections.Generic;

public class AnimalCameraSwitcher : MonoBehaviour
{
    private List<Camera> deerCams = new List<Camera>();
    private List<Camera> tigerCams = new List<Camera>();
    private int deerIndex = -1;
    private int tigerIndex = -1;

    // Estado para el toggle de los sensores
    private bool sensorsVisible = true;

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

        if (Input.GetKeyDown(KeyCode.S))
            ToggleSensorMeshes();
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

    // Alterna la visibilidad de los MeshRenderer en todos los objetos con tag "sensor"
    private void ToggleSensorMeshes()
    {
        // Cambia el estado
        sensorsVisible = !sensorsVisible;

        // Encuentra todos los objetos etiquetados como "sensor"
        var sensors = GameObject.FindGameObjectsWithTag("sensor");
        if (sensors.Length == 0)
        {
            Debug.LogWarning("No se encontraron objetos con tag 'sensor'.");
            return;
        }

        // Activa o desactiva todos sus MeshRenderer según el estado
        foreach (var obj in sensors)
        {
            var mr = obj.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = sensorsVisible;
        }

        Debug.Log($"Sensores {(sensorsVisible ? "activados" : "desactivados")}.");
    }
}
