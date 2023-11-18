using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Material red;
    [SerializeField] private Material green;

    [SerializeField] private GameObject flag;
    [SerializeField] private GameObject groundedFlag;

    public float speed = 0.2f;
    public float steerSpeed = 0.01f;
    private float currentRotation = 0f;

    public override void OnEpisodeBegin()
    {
        // Ripristina la trasformazione dell'oggetto alla trasformazione originale
        transform.localPosition = Vector3.zero;
    }

    public void FixedUpdate() {
        transform.position += new Vector3(0, 0, 1) * speed * Time.deltaTime;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int action = actionBuffers.DiscreteActions[0];

        switch (action)
        {
            case 0:
                // Nessuna sterzata
                Debug.Log("Nessuna sterzata");
                break;

            case 1:
                // Sterza a destra
                Debug.Log("Sterzata a destra");
                AddReward(-0.1f);
                Steer(steerSpeed);
                break;

            case 2:
                // Sterza a sinistra
                Debug.Log("Sterzata a sinistra");
                AddReward(-0.1f);
                Steer(-steerSpeed);
                break;
        }

        // ... Altri comportamenti e aggiornamenti dell'agente ...
    }

    private void Steer(float steerAmount)
    {
        currentRotation += steerSpeed * Time.deltaTime;
        transform.Rotate(0, currentRotation, 0);
        transform.position += new Vector3(1, 0, 0) * steerAmount * Time.deltaTime;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Implementa il controllo manuale durante il testing
        var discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("goal"))
        {
            SetReward(+10.0f);
            flag.GetComponent<Renderer>().material = green;
            groundedFlag.GetComponent<Renderer>().material = red;
            Debug.Log("Obiettivo raggiunto!");
            EndEpisode();
        } 
        else if (other.gameObject.CompareTag("obstacle"))
        {
            SetReward(-10.0f);
            flag.GetComponent<Renderer>().material = red;
            groundedFlag.GetComponent<Renderer>().material = red;
            Debug.Log("Ostacolo colpito!");
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider coll) {
        if (coll.CompareTag("mid-goal")) {
            Debug.Log("Obiettivo intermedio raggiunto!");
            AddReward(0.25f);
        }
    }
}
