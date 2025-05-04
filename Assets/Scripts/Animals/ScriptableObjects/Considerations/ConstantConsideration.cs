using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    
    // This is a constant consideration that always returns the same value.
    // It is used to test the system and can be used as a placeholder for other considerations.
    [CreateAssetMenu(fileName = "ConstantConsideration", menuName = "UtilityAI/Considerations/Constant")]
    public class ConstantConsideration : Consideration
    {
        public float value;
        public override float Evaluate(Context context)
        {
            return value;
        }
    }
}


