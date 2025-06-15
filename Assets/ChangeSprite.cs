using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSprite : MonoBehaviour
{
    [Header("Componentes UI")]
    public Image imageStatus;
    public Transform camara;

    void Awake()
    {
        // Si no se asignó el Image, lo busca en los hijos automáticamente
        if (imageStatus == null)
        {
            imageStatus = GetComponentInChildren<Image>();
            if (imageStatus == null)
                Debug.LogWarning($"[{name}] ChangeSprite: no se encontró ningún Image en los hijos.");
        }
    }

    void Start()
    {
        // Si no se asigna manualmente la cámara, intenta encontrar la cámara en el padre
        if (camara == null)
        {
            Camera cam = GetComponentInParent<Camera>();
            if (cam != null)
                camara = cam.transform;
            else
                Debug.LogWarning($"[{name}] ChangeSprite: no se encontró ninguna Cámara en los padres.");
        }
    }

    void Update()
    {
        if (camara != null)
        {
            // Siempre gira la UI hacia la cámara para que sea legible
            transform.LookAt(transform.position + camara.rotation * Vector3.forward,
                              camara.rotation * Vector3.up);
        }
    }

    public void ActualizarSprite(Sprite newSprite)
    {
        if (imageStatus != null)
        {
            imageStatus.sprite = newSprite;
            Debug.Log($"[{name}] ChangeSprite: sprite actualizado a {newSprite?.name ?? "null"}.");
        }
        else
        {
            Debug.LogError($"[{name}] ChangeSprite: imageStatus es null, no se puede actualizar el sprite.");
        }
    }
}
