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

    [SerializeField] private Material red;
    [SerializeField] private Material green;

    [SerializeField] private GameObject flag; 

    public override void OnEpisodeBegin()
    {
        // Ripristina la trasformazione dell'oggetto alla trasformazione originale
        transform.localPosition = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(targetTransform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveZ = actions.ContinuousActions[0];
        float moveX = actions.ContinuousActions[1];

        float moveSpeed = 0.4f;

        transform.position += new Vector3(moveX,0,moveZ) * Time.deltaTime * moveSpeed;

        // Converti l'angolo in un vettore di direzione
        // Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));

        // Calcola il movimento in base alle azioni ricevute
        //Vector3 movement = direction * move * moveSpeed * Time.deltaTime;

        // Applica il movimento all'oggetto
        // transform.Translate(movement);
        //transform.Rotate(0f, angle*0.1f, 0f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;

        continousActions[1] = Input.GetAxisRaw("Horizontal");
        continousActions[0] = Input.GetAxisRaw("Vertical");
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Arrivo"))
        {
            SetReward(+1f);
            Debug.Log("Complimenti");
            flag.GetComponent<Renderer>().material = green;
            EndEpisode();
        } else if (other.gameObject.CompareTag("Birillo"))
        {
            SetReward(-1f);
            Debug.Log("Ho colpito un cono o una sbarra");
            flag.GetComponent<Renderer>().material = red;
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Percorso"))
        {
            SetReward(+10f);
        }
    }
}
