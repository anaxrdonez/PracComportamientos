using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.UtilityAI;
using UnityEditor.ShaderKeywordFilter;

namespace Pet.Core
{
    public enum PetType
    {
        Cat,
        Dog,
    }
  
    public enum State
    {
        decide, 
        move,
        executeAction,
    }
    public class PetController : MonoBehaviour
    {
        public MoveController moveController{ get; set; }
        public PetBrain aiBrain { get; set; }

        public Stats stats { get; set; }

        public State currentSate { get; set; } = State.decide; // Initial state is to decide the next action
        [SerializeField] public Context context { get; set; } // Reference to the context for destinations

        public PetType petType;   // Se expone en el inspector para asignar Cat o Dog

        [Header("UI")]
        [SerializeField] public Billboard billboard;


        // Start is called before the first frame update
        void Start()
        {
            // Find the Context in the scene if not assigned
            if (context == null)
                context = FindObjectOfType<Context>();
            if (context == null)
                Debug.LogError("No encontré ningún Context en la escena");

            if (billboard == null)
                billboard = GetComponentInChildren<Billboard>();

           

            // Initialize components
            moveController = GetComponent<MoveController>();
            aiBrain = GetComponent<PetBrain>();
            stats = GetComponent<Stats>(); 

            if (moveController == null)
            {
                Debug.LogError("MoveController component is missing on " + gameObject.name);
            }
            if (aiBrain == null)
            {
                Debug.LogError("AIBrain component is missing on " + gameObject.name);
            }
            if (stats == null)
            {
                Debug.LogError("Stats component is missing on " + gameObject.name);
            }


        }

        // Update is called once per frame
        void Update()
        {
            PetFSM();
        }

        public void PetFSM()
        {
            if (currentSate == State.decide)
            {
                billboard.ActualizarTexto("Decidiendo..."); // Update the billboard text to indicate decision-makingES
                aiBrain.DecideBestAction(); // Decide the next best action
                if (Vector3.Distance(aiBrain.bestAction.RequiredDestination.position, this.transform.position) < 2f)
                {
                    currentSate = State.executeAction; // If the action's destination is close, execute the action
                }
                else
                {
                    currentSate = State.move; // Change state to moving


                } 
            }
             else if (currentSate == State.move)
            {
                billboard.ActualizarTexto("Caminando..."); // Update the billboard text to indicate moving
                if (Vector3.Distance(aiBrain.bestAction.RequiredDestination.position, this.transform.position) < 2f)
                {
                    currentSate = State.executeAction; // If the action's destination is close, execute the action
                }
                else
                {
                    moveController.MoveTo(aiBrain.bestAction.RequiredDestination.position); // Move towards the action's destination
                    currentSate = State.move; // Change state to moving


                }
             }

            else if (currentSate == State.executeAction)
            {
                if (aiBrain.bestAction != null)
                {
                    if(aiBrain.finishedExecutingBestAction == false)
                    {
                        aiBrain.bestAction.Execute(this); // Execute the best action
                    }

                    else if (aiBrain.finishedExecutingBestAction == true)
                    {
                        aiBrain.bestAction = null; // Reset the best action after execution
                        aiBrain.finishedExecutingBestAction = false; // Reset the flag for next action
                        currentSate = State.decide; // Go back to deciding the next action
                    }

                }
            }

        }

        


        #region Coroutines
        public void Play(int time)
        {
            StartCoroutine(PlayCoroutine(time));
        }

        private IEnumerator PlayCoroutine(int time)
        {
            Debug.Log("Playing for " + time + " seconds.");
            int counter = time;
            while (counter > 0)
            {
                Debug.Log("Time left: " + counter + " seconds.");
                counter--;
                yield return new WaitForSeconds(1f);
            }


            //logic to update things involved with playing

            Debug.Log("Finished playing.");
            stats.energy -= 60; // Updating energy after playing
            stats.boredom -= 80; // Updating boredom after playing


            aiBrain.finishedExecutingBestAction = true;
            yield break;


        }

        public void Sleep(int time)
        {
            StartCoroutine(SleepCoroutine(time));
        }

        private IEnumerator SleepCoroutine(int time)
        {
            Debug.Log("Sleeping for " + time + " seconds.");
            int counter = time;
            while (counter > 0)
            {
                Debug.Log("Time left: " + counter + " seconds.");
                counter--;
                yield return new WaitForSeconds(1f);
            }


            //logic to update energy
            stats.energy += 100; // Updating energy after sleeping
            Debug.Log("Finished sleeping.");

            aiBrain.finishedExecutingBestAction = true;
            yield break;

        }


        public void Eat(int time)
        {
            StartCoroutine(EatCoroutine(time));
        }

        private IEnumerator EatCoroutine(int time)
        {
            Debug.Log("Eating for " + time + " seconds.");
            int counter = time;
            while (counter > 0)
            {
                Debug.Log("Time left: " + counter + " seconds.");
                counter--;
                yield return new WaitForSeconds(1f);
            }


            //logic to update energy
            stats.hunger -=30; // Updating hunger after eating
            Debug.Log("Eat sleeping.");

            aiBrain.finishedExecutingBestAction = true;
            yield break;

        }


        #endregion
    }
}
