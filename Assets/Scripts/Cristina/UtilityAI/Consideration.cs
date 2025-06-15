using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;


namespace Pet.UtilityAI
{

    /// <summary>
    /// Base class for Considerations in a Utility AI system.
    /// </summary>
    public abstract class Consideration : ScriptableObject
    {
        public string Name;

        private float _score;
        public float score
        {
            get { return score; }
            set { this._score = Mathf.Clamp01(value); }
        }

        public virtual void Awake()
        {
            score = 0.0f; // Default weight
        }


        // Al contrario de las acciones, las consideraciones se evaluan cada una a su manera
        // las acciones te valoran todas igual
        public abstract float ScoreConsideration(PetController pet);




    }

}

