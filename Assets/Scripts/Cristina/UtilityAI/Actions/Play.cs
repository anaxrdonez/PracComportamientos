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
            petController.Play(playTime);

        }
    }
   
}

