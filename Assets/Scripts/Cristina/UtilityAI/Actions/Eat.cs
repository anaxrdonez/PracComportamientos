using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;

namespace Pet.UtilityAI.Actions
{
    /// <summary>
    /// Action that simulates a sleep behavior for a pet.
    /// </summary>
    [CreateAssetMenu(fileName = "Eat", menuName = "Pet/UtilityAI/Actions/Eat")]
    public class Eat : Action
    {
        public int eatTime = 3; // Duration of sleep in seconds
        /// <summary>
        /// Executes the sleep action on the given pet controller.
        /// </summary>
        public override void Execute(PetController petController)
        {
            petController.Eat(eatTime);
        }
    }
}


