using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pet.Core;

namespace Pet.UtilityAI
{

    public abstract class Action : ScriptableObject
    {
        public string actionName;
        private float _score;

        public float score;
        public float Score
        {
            get { return _score; }
            set { this._score = Mathf.Clamp01(value); }
        }

        public Consideration[] considerations;
        

        public virtual void Awake()
        {
            score = 0f;
        }

        public abstract void Execute(PetController petController);


    }
}

