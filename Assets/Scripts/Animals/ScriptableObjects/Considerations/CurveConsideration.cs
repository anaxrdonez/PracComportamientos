using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/CurveConsideration")]
    public class CurveConsideration : Consideration
    {
        [Tooltip("Curva que mapea el valor de la necesidad (0–1) a utilidad (0–1)")]
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Clave en el Context para leer la necesidad (float 0–1)")]
        public string contextKey;

        // Unity llama a OnEnable cuando carga el asset (incluso en el Editor)
        void OnEnable()
        {
            // Si alguien limpió la curva en el Inspector, le ponemos un default básico
            if (curve == null || curve.length == 0)
            {
                curve = AnimationCurve.Linear(0, 0, 1, 1);
            }
        }

        public override float Evaluate(Context context)
        {
            // Seguridad ante contextKey mal puesto
            if (string.IsNullOrEmpty(contextKey))
            {
                Debug.LogWarning($"[CurveConsideration] contextKey no asignada en {name}");
                return 0f;
            }

            float inputValue = context.GetData<float>(contextKey);
            float utility = curve.Evaluate(inputValue);
            return Mathf.Clamp01(utility);
        }
    }
}
