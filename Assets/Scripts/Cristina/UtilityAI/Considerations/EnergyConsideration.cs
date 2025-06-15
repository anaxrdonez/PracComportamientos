using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;
using Pet.UtilityAI;

namespace Pet.UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "EnergyConsideration", menuName = "Pet/UtilityAI/Considerations/EnergyConsideration")]
    public class EnergyConsideration : Consideration
    {
        [SerializeField] private AnimationCurve responseCurve;

        public override float ScoreConsideration(PetController pet)
        {
            score = responseCurve.Evaluate(Mathf.Clamp01(pet.stats.energy / 100)); // Evaluate the curve based on the hunger level
            return score; // Return the calculated score        }
        }
    }
}

