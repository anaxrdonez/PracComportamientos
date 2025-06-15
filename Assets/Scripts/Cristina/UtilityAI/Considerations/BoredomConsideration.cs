using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.UtilityAI;
using Pet.Core;

namespace Pet.UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "BoredomConsideration", menuName = "Pet/UtilityAI/Considerations/Boredom Consideration")]

    public class BoredomConsideration : Consideration
    {
        [SerializeField] private AnimationCurve responseCurve; // Curve to define how boredom affects the score

        public override float ScoreConsideration(PetController pet)
        {

            score = responseCurve.Evaluate(Mathf.Clamp01(pet.stats.boredom / 100)); // Evaluate the curve based on the boredom level
            return score; // Return the calculated score           }
        }
    }
}

