using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Pet.Core
{
    public class MoveController : MonoBehaviour
    {
        private NavMeshAgent agent;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void MoveTo(Vector3 destination)
        {
            if (agent != null)
            {
                agent.SetDestination(destination);
            }
            else
            {
                Debug.LogError("NavMeshAgent component is missing on " + gameObject.name);
            }
        }
    }
}
