using UnityEngine;
using System.Collections;

public class TigerAI : MonoBehaviour
{
    public float radiusX = 7f, radiusZ = 4f;
    public float walkSpeed = 2f, chaseSpeed = 4f, rotateSpeed = 120f;
    public float waitMin = 1f, waitMax = 3f;
    public float chaseDuration = 5f;

    enum State { Wander, Chase }
    State state = State.Wander;

    Vector3 centerPos;
    Transform preyTarget;
    Coroutine routine;

    void Start()
    {
        centerPos = transform.position;
        routine = StartCoroutine(WanderRoutine());
    }

    public void OnSeePrey(Transform prey)
    {
        preyTarget = prey;
        if (state != State.Chase)
            SwitchState(State.Chase);
    }

    public void OnLosePrey(Transform prey)
    {
        // Si pierde de vista antes de "atrapar", vuelve a patrullar
        if (state == State.Chase)
            SwitchState(State.Wander);
    }

    void SwitchState(State newState)
    {
        if (routine != null) StopCoroutine(routine);
        state = newState;
        if (state == State.Wander)
            routine = StartCoroutine(WanderRoutine());
        else if (state == State.Chase)
            routine = StartCoroutine(ChaseRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            Vector3 target = GetRandomPointInEllipse();
            yield return RotateTowards(target);
            yield return MoveTowards(target, walkSpeed);
            yield return new WaitForSeconds(Random.Range(waitMin, waitMax));
        }
    }

    IEnumerator ChaseRoutine()
    {
        float endTime = Time.time + chaseDuration;
        while (Time.time < endTime && state == State.Chase)
        {
            if (preyTarget == null) break;

            // 1) Girar hacia la presa
            Vector3 targetPos = preyTarget.position;
            yield return RotateTowards(targetPos);

            // 2) Moverse hacia ella
            yield return MoveTowards(targetPos, chaseSpeed);

            // 3) Comprobar “captura”
            float dist = Vector3.Distance(transform.position, preyTarget.position);
            if (dist <= 0.1f)
            {
                // Desactivar el ciervo
                preyTarget.gameObject.SetActive(false);
                preyTarget = null;
                // Volver a patrullar
                SwitchState(State.Wander);
                yield break;
            }
        }
        // Si expira el tiempo de persecución sin capturar
        if (state == State.Chase)
            SwitchState(State.Wander);
    }

    IEnumerator RotateTowards(Vector3 target)
    {
        Quaternion goal = Quaternion.LookRotation((target - transform.position).normalized, Vector3.up);
        while (Quaternion.Angle(transform.rotation, goal) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator MoveTowards(Vector3 target, float speed)
    {
        // Avanza hasta el punto o hasta que cambie de estado
        while ((state == State.Chase || state == State.Wander)
               && Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    Vector3 GetRandomPointInEllipse()
    {
        float t = Random.Range(0f, Mathf.PI * 2f);
        float u = Random.value + Random.value;
        float r = u > 1f ? 2f - u : u;
        float x = r * radiusX * Mathf.Cos(t);
        float z = r * radiusZ * Mathf.Sin(t);
        return new Vector3(centerPos.x + x, centerPos.y, centerPos.z + z);
    }
}
