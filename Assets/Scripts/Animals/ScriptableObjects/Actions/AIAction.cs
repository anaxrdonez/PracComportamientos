using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UtilityAI
{
    public abstract class AIAction : ScriptableObject
    {
        public string targetTag;
        public Consideration consideration;

        public virtual void Initialize(Context context)
        {
            //optional initialization logic 
        }
        public float CalculateUtility(Context context)
        {
            return consideration.Evaluate(context);
        }

        public abstract void Execute(Context context); 
    }
}

