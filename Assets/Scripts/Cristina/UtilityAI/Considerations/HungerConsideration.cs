using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.UtilityAI;
using Pet.Core;


namespace Pet.UtilityAI.Considerations
{

    [CreateAssetMenu(fileName = "HungerConsideration", menuName = "Pet/UtilityAI/Considerations/Hunger Consideration")]
    public class HungerConsideration : Consideration
    {
        [SerializeField] private AnimationCurve responseCurve; // Curve to define how hunger affects the score

        public override float ScoreConsideration(PetController pet)
        {
            score = responseCurve.Evaluate(Mathf.Clamp01(pet.stats.hunger/100)); // Evaluate the curve based on the hunger level
            return score; // Return the calculated score
        }
    }

}
 