using UnityEngine;

public class PredatorAttack : MonoBehaviour
{
    [Tooltip("Tag que identifica a la presa")]
    public string preyTag = "Deer";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(preyTag))
        {
            // “Matar” al ciervo
            other.gameObject.SetActive(false);
        }
    }
}
