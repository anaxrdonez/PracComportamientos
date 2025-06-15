using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;

namespace Pet.UtilityAI
{
    /// <summary>
    /// Controls the decision-making process of a pet using Utility AI.
    /// Evaluates actions based on their considerations and selects the best one.
    /// </summary>
    public class PetBrain : MonoBehaviour
    {
        private PetController pet;
        public bool finishedDeciding { get; set; } = false; // Indicates if the decision-making process is complete

        /// <summary>
        /// List of actions currently considered the best based on their scores.
        /// </summary>
        public Action bestAction { get; set; }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the reference to the PetController component.
        /// </summary>
        void Start()
        {
            pet = GetComponent<PetController>();
        }

        /// <summary>
        /// Called once per frame. Can be used to update behavior each frame.
        /// </summary>
        void Update()
        {
            if (bestAction is null)
            {
                // If no best action is selected, decide the best action from available actions
                DecideBestAction(pet.actionsAvailable);
            }
        }

        /// <summary>
        /// Loops through all available actions and selects the one with the highest score.
        /// </summary>
        /// <param name="actionsAvailable">List of actions to evaluate.</param>
        public void DecideBestAction(Action[] actionsAvailable)
        {
            float score = 0f;
            int nextBestActionIndex = -1;
            foreach (Action action in actionsAvailable)
            {
                float actionScore = ScoreAction(action); // Calculate the score for each action
                if (actionScore > score) // If this action has a higher score than the current best
                {
                    score = actionScore; // Update the best score
                    nextBestActionIndex = System.Array.IndexOf(actionsAvailable, action); // Get the index of the best action
                }

            }

            bestAction = actionsAvailable[nextBestActionIndex];
            finishedDeciding = true; // Mark that the decision-making is complete
        }



        /// <summary>
        /// Calculates the overall utility score of a given action by evaluating its considerations.
        /// </summary>
        /// <param name="action">The action to score.</param>
        /// <returns>The computed utility score of the action.</returns>
        public float ScoreAction(Action action)
        {
            float score = 1f; // Start with a perfect score

            foreach (Consideration consideration in action.considerations)
            {
                float considerationScore = consideration.ScoreConsideration();
                score *= considerationScore; // Multiply scores to combine utility

                if (score == 0f)
                {
                    action.score = 0f;
                    return action.score; // If any consideration scores 0, the total score is 0
                }
            }

            // Dave Mark’s "Behavioral Mathematics" approach to normalize action score
            float originalScore = score;

            // Average the score based on the number of considerations,
            // because multiplying decimal values tends to reduce the result.
            // Therefore, the more considerations there are, the smaller the final score will be.
            float modFactor = 1 - (1f / action.considerations.Length);
            float makeupValue = (1f - originalScore) * modFactor;

            action.score = originalScore + (makeupValue * originalScore);
            return action.score;
        }
    }
}
