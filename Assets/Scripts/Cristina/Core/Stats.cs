using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pet.Core
{
    public class Stats : MonoBehaviour
    {
        private int _energy;
        public int energy
        {
            get { return _energy; }
            set
            {
                _energy = Mathf.Clamp(value, 0, 100);
                OnStatValueChanged?.Invoke();
            }
        }

        private int _hunger;
        public int hunger
        {
            get { return _hunger; }
            set
            {
                _hunger = Mathf.Clamp(value, 0, 100);
                OnStatValueChanged?.Invoke();
            }
        }

        private int _boredom;
        public int boredom
        {
            get { return _boredom; }
            set
            {
                _boredom = Mathf.Clamp(value, 0, 100);
                OnStatValueChanged?.Invoke();
            }
        }

        [SerializeField] private float timeToDecreaseHunger = 6f;
        [SerializeField] private float timeToDecreaseEnergy = 5f;
        [SerializeField] private float timeToDecreaseBoredom = 7f;
        private float timeLeftEnergy;
        private float timeLeftHunger;
        private float timeLeftBoredom;

        [SerializeField] private Billboard billboard;

        public delegate void StatValueChangedHandler();
        public event StatValueChangedHandler OnStatValueChanged;


        void Awake()
        {
            // Si no lo asignaste en el Inspector, busca un Billboard en los hijos
            if (billboard == null)
            {
                billboard = GetComponentInChildren<Billboard>();
            }
            if (billboard == null)
            {
                Debug.LogError($"{name}: Stats necesita un componente Billboard asignado o en hijos.");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            hunger = Random.Range(20, 80);
            energy = Random.Range(20, 80);
            boredom = Random.Range(10, 100);

            // Inicializamos los temporizadores para que comiencen a contar
            timeLeftHunger = timeToDecreaseHunger;
            timeLeftEnergy = timeToDecreaseEnergy;
            timeLeftBoredom = timeToDecreaseBoredom;


        }

        private void OnEnable()
        {
            OnStatValueChanged += UpdateDisplayText;
        }

        private void OnDisable()
        {
            OnStatValueChanged -= UpdateDisplayText;
        }

        private void Update()
        {
            UpdateEnergy();
            UpdateHunger();
            UpdateBoredom();
        }

        public void UpdateHunger()
        {
            if (timeLeftHunger > 0)
            {
                timeLeftHunger -= Time.deltaTime;
                return;
            }

            timeLeftHunger = timeToDecreaseHunger;
            hunger += 1;
        }

        public void UpdateEnergy()
        {
            if (timeLeftEnergy > 0)
            {
                timeLeftEnergy -= Time.deltaTime;
                return;
            }

            timeLeftEnergy = timeToDecreaseEnergy;
            energy -= 1;
        }

        public void UpdateBoredom()
        {
            if (timeLeftBoredom > 0)
            {
                timeLeftBoredom -= Time.deltaTime;
                return;
            }

            timeLeftBoredom = timeToDecreaseBoredom;
            boredom -= 1;
        }

        void UpdateDisplayText()
        {
            if (billboard == null)
                return;  // Ya avisamos en Awake, aquí simplemente salimos si no está

            billboard.UpdateStatsText(energy, hunger, boredom);
        }

    }
}
