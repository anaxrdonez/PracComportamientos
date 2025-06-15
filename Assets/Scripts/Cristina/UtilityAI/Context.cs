using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pet.Core
{

    public enum DestinationType
    {
        restDog,
        foodDog,
        restCat,
        foodCat,
        
    }
    public class Context : MonoBehaviour
    {
        public GameObject dogFood;
        public GameObject catFood;

        public GameObject dogBed;
        public GameObject catBed;

        public float MinDistance = 5f;
        public Dictionary<DestinationType, List<Transform>> Destinations { get; private set; }

        private void Start()
        {
            List<Transform> dogBedDestination = new List<Transform>() { dogBed.transform };
            List<Transform> catBedDestination = new List<Transform>() { catBed.transform };

            List<Transform> dogFoodDestination = new List<Transform>() { dogFood.transform };
            List<Transform> catFoodDestination = new List<Transform>() { catFood.transform };


            Destinations = new Dictionary<DestinationType, List<Transform>>()
            {
                { DestinationType.restDog, dogBedDestination},
                { DestinationType.foodCat, catFoodDestination },
                { DestinationType.restCat, catBedDestination },
                { DestinationType.foodDog, dogFoodDestination }
            };
        }

        

    }
}
