using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;

namespace Pet.UtilityAI.Actions
{
    /// <summary>
    /// Action that simulates a sleep behavior for a pet.
    /// </summary>
    [CreateAssetMenu(fileName = "Sleep", menuName = "Pet/UtilityAI/Actions/Sleep")]
    public class Sleep : Action
    {
        public int sleepTime = 10; // Duration of sleep in seconds
        /// <summary>
        /// Executes the sleep action on the given pet controller.
        /// </summary>
        public override void Execute(PetController petController)
        {
            petController.billboard?.ActualizarTexto("Durmiendo...");

            petController.Sleep(sleepTime);
        }

        public override void SetDestination(PetController petController)
        {
            switch (petController.petType)
            {
                case PetType.Cat:
                    RequiredDestination = petController.context.catBed.transform;
                    break;
                case PetType.Dog:
                    RequiredDestination = petController.context.dogBed.transform;
                    break;
            }
            petController.moveController.destination = RequiredDestination; // Update the move controller's destination to the bed's position
        }
    }
}


