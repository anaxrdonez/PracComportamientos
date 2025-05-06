using System.Collections.Generic;
using UnityEngine;

public class CicloDiaNoche : MonoBehaviour
{
    public Light luzDireccional;
    public float duracionCiclo = 60f; // Duración total del ciclo (día + noche)
    public List<Light> lucesNocturnas; // Añade aquí tus 6 point lights desde el inspector

    private float angulo = 0f;
    private bool esNoche = false;

    void Start()
    {
        // Asegura que todas las luces estén apagadas al comenzar
        CambiarLucesNocturnas(false);
    }

    void Update()
    {
        float anguloActual = luzDireccional.transform.rotation.eulerAngles.x;

        // Día: 0° a 270°, Noche: 270° a 360°
        float velocidadDia = 360f / (duracionCiclo * 0.75f);
        float velocidadNoche = 360f / (duracionCiclo * 0.25f);
        float velocidadActual = (anguloActual < 270f) ? velocidadDia : velocidadNoche;

        angulo += velocidadActual * Time.deltaTime;

        if (angulo >= 360f)
            angulo -= 360f;

        luzDireccional.transform.rotation = Quaternion.Euler(angulo, 0f, 0f);

        // Cambio de estado día/noche
        bool nuevaNoche = (angulo >= 270f && angulo < 360f);

        if (nuevaNoche != esNoche)
        {
            esNoche = nuevaNoche;
            CambiarLucesNocturnas(esNoche);
        }
    }

    void CambiarLucesNocturnas(bool encender)
    {
        foreach (Light luz in lucesNocturnas)
        {
            if (luz != null)
                luz.enabled = encender;
        }
    }
}