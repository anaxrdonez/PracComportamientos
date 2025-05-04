using UnityEngine;

namespace UtilityAI
{
    public abstract class Consideration : ScriptableObject
    {
        public abstract float Evaluate(Context context); //evaluate how desirable or useful a specific factor
        // is for the AI at any given time. To evaluate the consideration, we need to pass in a context object, that is what we know about the world at the time of evaluation.
    }
}

