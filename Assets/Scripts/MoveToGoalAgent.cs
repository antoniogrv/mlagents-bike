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

    private List<Collider> colliderList;

    public float speed = 1.0f;
    public float steerSpeed = 0.01f;
    private float currentRotation = 0f;

    public float fallThreshold = -2f;

    public void Start()
    {
        colliderList = new List<Collider>();
    }

    public override void OnEpisodeBegin()
    {
        // Ripristina la trasformazione dell'oggetto alla trasformazione originale
        transform.localPosition = new Vector3(0, 0.17f, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        foreach (Collider coll in colliderList)
        {
            coll.tag = "mid-goal";
        }
        colliderList.Clear();
        currentRotation = 0f;
    }

    public void FixedUpdate() {
        // transform.position += new Vector3(0, 0, 1) * speed * Time.deltaTime;

        if (transform.position.y < fallThreshold)
        {
            Debug.Log("La moto è caduta.");
            //SetReward(-100.0f);
            // transform.localPosition = new Vector3(0, 0.17f, 0);
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int action = actionBuffers.DiscreteActions[0];

        switch (action)
        {
            case 0:
                // Nessuna sterzata
                Vector3 direction = transform.forward;
                float s = speed * Time.deltaTime;
                transform.Translate(direction * s, Space.World);
                //transform.position += new Vector3(0, 0, 1) * speed * Time.deltaTime;
                //Debug.Log("Nessuna sterzata");
                break;

            case 1:
                // Sterza a destra
                //Debug.Log("Sterzata a destra");
                AddReward(-1f);
                Steer(steerSpeed);
                break;

            case 2:
                // Sterza a sinistra
                //Debug.Log("Sterzata a sinistra");
                AddReward(-1f);
                Steer(-steerSpeed);
                break;
        }

        // ... Altri comportamenti e aggiornamenti dell'agente ...
    }

    private void Steer(float steerAmount)
    {
        //currentRotation += steerAmount * Time.deltaTime * 0.1f;
        currentRotation = steerAmount * Time.deltaTime * 100;
        transform.Rotate(0, currentRotation, 0);
        transform.position += new Vector3(2.0f, 0, 0) * steerAmount * Time.deltaTime;
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
        if (other.gameObject.CompareTag("obstacle"))
        {
            SetReward(-10.0f);
            flag.GetComponent<Renderer>().material = red;
            groundedFlag.GetComponent<Renderer>().material = red;
            //Debug.Log("Ostacolo colpito!");
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.CompareTag("goal"))
        {
            SetReward(+1000.0f);
            flag.GetComponent<Renderer>().material = green;
            groundedFlag.GetComponent<Renderer>().material = red;
            //Debug.Log("Obiettivo raggiunto!");
            EndEpisode();
        }
        if (coll.CompareTag("attraversato"))
        {
            Debug.Log("Ancora?");
            AddReward(-10f);
        }
        if (coll.CompareTag("mid-goal")) 
        {
            if (!colliderList.Contains(coll))
            {
                // Ignora l'altezza (componente Y) nella misurazione della distanza
                //Vector2 thisPosition = new Vector2(transform.position.x, transform.position.z);
                //Vector2 otherPosition = new Vector2(coll.transform.position.x, coll.transform.position.z);

                // Calcola la distanza tra i centri dei due collider (ignorando l'altezza)
                //float distance = Vector2.Distance(thisPosition, otherPosition);
                //Debug.Log("Distanza misurata: " + distance);
                
                AddReward(30.0f);

            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("mid-goal"))
        {
            if (colliderList.Count > 2)
            {
                colliderList[colliderList.Count - 2].tag = "attraversato";
            }
            
            if (!colliderList.Contains(other))
                colliderList.Add(other);
        }
    }
}
