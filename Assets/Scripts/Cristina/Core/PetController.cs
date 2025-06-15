using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.UtilityAI;

namespace Pet.Core
{
    public class PetController : MonoBehaviour
    {
        public MoveController moveController{ get; set; }
        public PetBrain aiBrain { get; set; }
        public Action[] actionsAvailable;

        public Stats stats { get; set; }


        // Start is called before the first frame update
        void Start()
        {
            moveController = GetComponent<MoveController>();
            aiBrain = GetComponent<PetBrain>();
            if (moveController == null)
            {
                Debug.LogError("MoveController component is missing on " + gameObject.name);
            }
            if (aiBrain == null)
            {
                Debug.LogError("AIBrain component is missing on " + gameObject.name);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (aiBrain != null && !aiBrain.finishedDeciding)
            {
                aiBrain.finishedDeciding = false; // Reset the decision flag
                aiBrain.bestAction.Execute(this); // Execute the best action
            }
        }

        public void OnFinishedAction()
        {
            aiBrain.DecideBestAction(actionsAvailable); // Decide the next best action
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

            //Decide next action

            OnFinishedAction();

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
            OnFinishedAction();



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

            OnFinishedAction();

        }


        #endregion
    }
}
