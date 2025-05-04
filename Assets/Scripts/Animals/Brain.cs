using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UtilityAI
{
    // Especies para elegir el tipo de baño apropiado
    public enum Species { Cat, Dog }

    [RequireComponent(typeof(NavMeshAgent), typeof(Sensor))]
    public class Brain : MonoBehaviour
    {
        [Header("Lista de acciones (ScriptableObjects)")]
        public List<AIAction> actions = new List<AIAction>();

        [Header("Parámetros generales")]
        public Species species;                          // Gato o Perro
        public float replanInterval = 0.25f;             // Cada cuánto replantear (segundos)

        // refs cacheadas
        public Context context;
        private PetNeeds needs;
        private NavMeshAgent agent;
        private Sensor sensor;

        private AIAction currentAction;
        private float nextReplanTime;

        void Awake()
        {
            // instancio contexto y cacheo componentes
            context = new Context(this);
            needs = GetComponent<PetNeeds>();
            agent = GetComponent<NavMeshAgent>();
            sensor = GetComponent<Sensor>();

            // datos constantes
            context.SetData("agent", agent);
            context.SetData("species", species);

            // inicializo cada acción
            foreach (var a in actions)
                a.Initialize(context);
        }

        void Update()
        {
            UpdateContext();

            // replantear si toca o no hay acción actual
            if (Time.time >= nextReplanTime || currentAction == null)
            {
                nextReplanTime = Time.time + replanInterval;
                currentAction = SelectBestAction();
            }

            // ejecuto la acción elegida
            currentAction?.Execute(context);
        }

        void UpdateContext()
        {
            // necesidades normalizadas: 0 = cubierta, 1 = urgente
            context.SetData("hunger", needs.Hunger01);
            context.SetData("sleep", 1f - needs.Energy01);
            context.SetData("bathroom", needs.Bathroom01);
            context.SetData("fun", 1f - needs.Fun01);

            // pull-percepciones desde Sensor (tags configurados en Sensor.targetTags)
            bool hasFood = sensor.GetClosestTarget("Food") != null;
            bool hasToy = sensor.GetClosestTarget("Toy") != null;
            bool hasAdopter = sensor.GetClosestTarget("Adopter") != null;
            bool hasCaretaker = sensor.GetClosestTarget("Caretaker") != null;

            context.SetData("foodAvailable", hasFood);
            context.SetData("toysAvailable", hasToy);
            context.SetData("adopterInArea", hasAdopter);
            context.SetData("caretakerAvailable", hasCaretaker);

            // penOpen y adopted pueden venir de otros sistemas y ajustarse en Context externamente
        }

        AIAction SelectBestAction()
        {
            if (actions == null || actions.Count == 0)
                return null;

            AIAction best = null;
            float bestU = float.MinValue;

            foreach (var a in actions)
            {
                float u = a.CalculateUtility(context);
                if (u > bestU)
                {
                    bestU = u;
                    best = a;
                }
            }
            return best;
        }
    }
}
