using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityUtils;

namespace UtilityAI
{
    /// <summary>
    /// Contexto compartido para AIActions. Contiene referencias a componentes y datos de percepciones/estados normalizados.
    /// </summary>
    public class Context
    {
        // Componentes cacheados
        public Brain brain;
        public NavMeshAgent agent;
        public Sensor sensor;
        public Transform target;


        // Especie de la mascota (Cat/Dog) tomada desde el Brain
        public Species Species => brain.species;

        // Claves internas para datos en _data
        private static class Keys
        {
            public const string Hunger = "hunger";
            public const string Sleep = "sleep";
            public const string Bathroom = "bathroom";
            public const string Fun = "fun";
            public const string AdopterInArea = "adopterInArea";
            public const string CaretakerAvailable = "caretakerAvailable";
            public const string PenOpen = "penOpen";
            public const string Adopted = "adopted";
            public const string FoodAvailable = "foodAvailable";
            public const string ToysAvailable = "toysAvailable";
        }

        // Almacenamiento genérico de datos
        private readonly Dictionary<string, object> _data;

        public Context(Brain brain)
        {
            Preconditions.CheckNotNull(brain, nameof(brain));
            this.brain = brain;

            // Cachear componentes principales que gestiona Brain.UpdateContext
            var go = brain.gameObject;
            this.agent = go.GetOrAdd<NavMeshAgent>();
            this.sensor = go.GetOrAdd<Sensor>();

            _data = new Dictionary<string, object>();
            // No inicializar valores por defecto: los datos de necesidades y flags
            // se establecen desde PetNeeds y Sensor en Brain.UpdateContext().
        }

        #region Propiedades tipadas
        public float Hunger
        {
            get => GetData<float>(Keys.Hunger);
            set => SetData(Keys.Hunger, value);
        }
        public float Sleep
        {
            get => GetData<float>(Keys.Sleep);
            set => SetData(Keys.Sleep, value);
        }
        public float Bathroom
        {
            get => GetData<float>(Keys.Bathroom);
            set => SetData(Keys.Bathroom, value);
        }
        public float Fun
        {
            get => GetData<float>(Keys.Fun);
            set => SetData(Keys.Fun, value);
        }
        public bool AdopterInArea
        {
            get => GetData<bool>(Keys.AdopterInArea);
            set => SetData(Keys.AdopterInArea, value);
        }
        public bool CaretakerAvailable
        {
            get => GetData<bool>(Keys.CaretakerAvailable);
            set => SetData(Keys.CaretakerAvailable, value);
        }
        public bool PenOpen
        {
            get => GetData<bool>(Keys.PenOpen);
            set => SetData(Keys.PenOpen, value);
        }
        public bool Adopted
        {
            get => GetData<bool>(Keys.Adopted);
            set => SetData(Keys.Adopted, value);
        }
        public bool FoodAvailable
        {
            get => GetData<bool>(Keys.FoodAvailable);
            set => SetData(Keys.FoodAvailable, value);
        }
        public bool ToysAvailable
        {
            get => GetData<bool>(Keys.ToysAvailable);
            set => SetData(Keys.ToysAvailable, value);
        }
        #endregion

        /// <summary>
        /// Obtiene un valor tipado de _data, o default si no existe.
        /// </summary>
        public T GetData<T>(string key)
        {
            if (_data.TryGetValue(key, out var val) && val is T t)
                return t;
            return default;
        }

        /// <summary>
        /// Inserta o actualiza un dato en el contexto.
        /// </summary>
        public void SetData<T>(string key, T value)
        {
            _data[key] = value;
        }
    }
}
