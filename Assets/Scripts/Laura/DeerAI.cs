// DeerAI.cs
using UnityEngine;
using System.Collections;

public class DeerAI : MonoBehaviour
{
    public float radiusX = 5f, radiusZ = 3f;
    public float walkSpeed = 2f, rotateSpeed = 120f;
    public float waitMin = 1f, waitMax = 3f;
    public float fleeSpeedMultiplier = 2f;
    public float fleeDuration = 7f;
    public float alertDuration = 5f;

    enum State { Wander, Alert, Flee }
    State state = State.Wander;

    Vector3 centerPos;
    Transform threat;
    Coroutine behaviorRoutine;

    void Start()
    {
        centerPos = transform.position;
        behaviorRoutine = StartCoroutine(WanderRoutine());
    }

    // Llamado por DeerSensor
    public void OnSensorEnter(DeerSensor.SensorType type, Transform other)
    {
        if (type == DeerSensor.SensorType.Ear && state == State.Wander)
        {
            threat = other;
            SwitchState(State.Alert);
        }
        else if ((type == DeerSensor.SensorType.LeftEye || type == DeerSensor.SensorType.RightEye))
        {
            threat = other;
            SwitchState(State.Flee);
        }
    }

    // No lo usamos para alert/flee, pero podríamos cancelar si pierde contacto
    public void OnSensorExit(DeerSensor.SensorType type, Transform other) { /* opcional */ }

    void SwitchState(State newState)
    {
        if (behaviorRoutine != null) StopCoroutine(behaviorRoutine);
        state = newState;
        if (state == State.Wander)
            behaviorRoutine = StartCoroutine(WanderRoutine());
        else if (state == State.Alert)
            behaviorRoutine = StartCoroutine(AlertRoutine());
        else if (state == State.Flee)
            behaviorRoutine = StartCoroutine(FleeRoutine());
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

    IEnumerator AlertRoutine()
    {
        float endTime = Time.time + alertDuration;
        while (Time.time < endTime && state == State.Alert)
        {
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            yield return null;
        }
        if (state == State.Alert)
            SwitchState(State.Wander);
    }

    IEnumerator FleeRoutine()
    {
        float endTime = Time.time + fleeDuration;
        while (Time.time < endTime && state == State.Flee)
        {
            Vector3 awayDir = (transform.position - threat.position).normalized;
            Vector3 target = transform.position + awayDir * radiusX; // huye lejos
            yield return RotateTowards(target);
            yield return MoveTowards(target, walkSpeed * fleeSpeedMultiplier);
        }
        if (state == State.Flee)
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
        while (Vector3.Distance(transform.position, target) > 0.1f && state == State.Wander || state == State.Flee)
        {
            // opcional: ajustar rotación en movimiento
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    Vector3 GetRandomPointInEllipse()
    {
        float t = Random.Range(0f, Mathf.PI * 2f);
        float u = Random.value + Random.value;
        float r = u > 1 ? 2f - u : u;
        float x = r * radiusX * Mathf.Cos(t);
        float z = r * radiusZ * Mathf.Sin(t);
        return new Vector3(centerPos.x + x, centerPos.y, centerPos.z + z);
    }
}
