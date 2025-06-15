using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;


namespace Pet.UtilityAI.Actions
{

    [CreateAssetMenu(fileName = "Play", menuName = "Pet/UtilityAI/Actions/Play")]
    public class Play : Action
    {
        int playTime = 5;
        public override void Execute(PetController petController)
        {
            petController.billboard?.ActualizarTexto("Jugando...");

            petController.Play(playTime);

        }

        public override void SetDestination(PetController petController)
        {
            RequiredDestination = petController.transform;

        }
    }
   
}

