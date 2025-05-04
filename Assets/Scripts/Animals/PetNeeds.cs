using UnityEngine;

namespace UtilityAI
{
    /*  Componente que controlo yo para que mis bichos (gatos/perros) tengan
        sus “barras” de hambre, sueño, ganas de baño y diversión.
        Cada segundo esas barras se mueven y así la IA decide qué toca hacer. */
    public class PetNeeds : MonoBehaviour
    {
        /* ---------- Ajustes que suelo tocar en el Inspector ---------- */
        [Header("Máximos")]
        public float maxHunger = 100f;   // top de hambre (0 = lleno, 100 = me muero)
        public float maxEnergy = 100f;   // lo mismo pero para sueño
        public float maxBathroom = 100f;   // 0 = no hace falta, 100 = explota
        public float maxFun = 100f;   // 0 = aburrido, 100 = feliz

        [Header("Cuánto bajan/suben por segundo")]
        public float hungerDecayPerSec = 4f;  // cada segundo sube esto la hambre
        public float energyDecayPerSec = 2f;  // aquí baja la energía
        public float bathroomGainPerSec = 3f;  // se va llenando la vejiga
        public float funDecayPerSec = 3f;  // se va aburriendo

        /* ---------- Variables que la IA irá leyendo ---------- */
        [Header("Valores actuales (solo lectura)")]
        public float hunger;    // 0-100
        public float energy;    // 0-100
        public float bathroom;  // 0-100
        public float fun;       // 0-100

        /* ????????? Helpers en 0-1 para puntuaciones de utilidad ????????? */
        public float Hunger01 => hunger / maxHunger;
        public float Energy01 => energy / maxEnergy;
        public float Bathroom01 => bathroom / maxBathroom;
        public float Fun01 => fun / maxFun;

        /* ---------- Arranco las barras como a mí me gusta ---------- */
        void Start()
        {
            hunger = 0f;              // acaba de comer: sin hambre
            energy = maxEnergy;       // lleno de energía
            bathroom = 0f;              // vejiga vacía
            fun = maxFun * 0.7f;   // todavía quiere jugar un poco
        }

        /* ---------- Cada frame actualizo las barras ---------- */
        void Update()
        {
            float dt = Time.deltaTime;  // así todo depende del tiempo real

            hunger = Mathf.Clamp(hunger + hungerDecayPerSec * dt, 0f, maxHunger);
            energy = Mathf.Clamp(energy - energyDecayPerSec * dt, 0f, maxEnergy);
            bathroom = Mathf.Clamp(bathroom + bathroomGainPerSec * dt, 0f, maxBathroom);
            fun = Mathf.Clamp(fun - funDecayPerSec * dt, 0f, maxFun);
        }

        /* ---------- Funciones que llamo cuando hace algo ---------- */
        public void Feed(float amount)      // le doy de comer
        {
            hunger = Mathf.Clamp(hunger - amount, 0f, maxHunger);
        }

        public void Rest(float amount)      // ha dormido una siesta
        {
            energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);
        }

        public void UseBathroom()           // hizo sus cosas
        {
            bathroom = 0f;
        }

        public void Play(float amount)      // se lo ha pasado pipa
        {
            fun = Mathf.Clamp(fun + amount, 0f, maxFun);
        }
    }
}
