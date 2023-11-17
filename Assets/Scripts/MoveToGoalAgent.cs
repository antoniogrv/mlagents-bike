using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin!");
        // Ripristina la trasformazione dell'oggetto alla trasformazione originale
        transform.position = new Vector3(-15f,-0.15f,1.5f);
        transform.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(targetTransform.position);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.ContinuousActions[0];
        float angle = actions.ContinuousActions[1];

        float moveSpeed = 0.2f;

        // Converti l'angolo in un vettore di direzione
        Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));

        // Calcola il movimento in base alle azioni ricevute
        Vector3 movement = direction * move * moveSpeed * Time.deltaTime;

        // Applica il movimento all'oggetto
        transform.Translate(movement);
        transform.Rotate(0f, angle*0.1f, 0f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[1] = Input.GetAxisRaw("Horizontal");
        continousActions[0] = Input.GetAxisRaw("Vertical");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall> (out Wall wall))
        {
            SetReward(+1f);
            Debug.Log("Ho colpito il muro, fratellì!");
            EndEpisode();
        } else if (!other.TryGetComponent<Plane>(out Plane plane))
        {
            SetReward(-1f);
            Debug.Log("Ho colpito un cono o una sbarra");
            EndEpisode();
        }
    }
}
