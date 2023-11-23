using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;
using TMPro;
using System;
using System.Timers;

public class MoveToGoalCalamityAgent : Agent
{
    [SerializeField] private Material red;
    [SerializeField] private Material green;
    
    [SerializeField] private GameObject flag;
    [SerializeField] private GameObject groundedFlag;

    public int defaultTime = 11;

    private Boolean reversed = false;

    private List<Collider> colliderList;

    public float speed = 1.0f;
    public float steerSpeed = 0.01f;

    public TMP_Text timer;

    private float currentRotation = 0f;

    public float fallThreshold = -2f;

    private float midGoalReward = 50.0f; // Checkpoint lungo il percorso
    private float returnBackReward = -1000.0f; // Reward negativo quando la moto torna indietro invece di proseguire davanti
    private float steeringReward = -1.0f; // Disincentivo per la sterzata
    private float endGoalReward = 1000.0f; // Percorso completato
    private float obstacleReward = -1000.0f; // Reward negativo quando la moto urta un ostacolo

    public void Start()
    {
        colliderList = new List<Collider>();
    }

    public int GetTimer() {
        return Int32.Parse(timer.text);
    }

    public void SetTimer(string text) {
        timer.text = text;
    }

    IEnumerator TickTimer() {
        Debug.Log("Starting the timer!");
        do {
            int newTime = GetTimer() - 1;
            //Debug.Log("new time = " + newTime);
            SetTimer(newTime.ToString());
            yield return new WaitForSeconds(1);
        } while(GetTimer() > 0);

        flag.GetComponent<Renderer>().material = red;
        groundedFlag.GetComponent<Renderer>().material = red;

        EndEpisode();
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

        SetTimer(defaultTime.ToString());
        StartCoroutine("TickTimer");
    }

    public void FixedUpdate() {
        if (transform.position.y < fallThreshold)
        {
            Debug.Log("La moto è caduta.");
            EndEpisode();
            StopCoroutine("TickTimer");
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
                AddReward(steeringReward);
                Steer(steerSpeed);
                break;

            case 2:
                // Sterza a sinistra
                //Debug.Log("Sterzata a sinistra");
                AddReward(steeringReward);
                Steer(-steerSpeed);
                break;
        }

        // ... Altri comportamenti e aggiornamenti dell'agente ...
    }

    private void Steer(float steerAmount)
    {
        currentRotation = steerAmount * Time.deltaTime * 100;
        
        if (reversed)
        {
            currentRotation *= -1f;
        }

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
            AddReward(obstacleReward);
            flag.GetComponent<Renderer>().material = red;
            groundedFlag.GetComponent<Renderer>().material = red;
            //Debug.Log("Ostacolo colpito!");
            StopCoroutine("TickTimer");
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.CompareTag("puddle"))
        {
            Debug.Log("Entro nela pozzanghera");
            reversed = true;
        }
        if (coll.gameObject.CompareTag("goal"))
        {
            AddReward(endGoalReward);
            flag.GetComponent<Renderer>().material = green;
            groundedFlag.GetComponent<Renderer>().material = green;
            //Debug.Log("Obiettivo raggiunto!");
            StopCoroutine("TickTimer");
            EndEpisode();
        }
        if (coll.CompareTag("attraversato"))
        {
            Debug.Log("Ancora?");
            AddReward(returnBackReward);
            StopCoroutine("TickTimer");
            EndEpisode();
        }
        if (coll.CompareTag("mid-goal")) 
        {
            if (!colliderList.Contains(coll))
            {
                AddReward(midGoalReward);

            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("puddle"))
        {
            Debug.Log("Esco dalla pozzanghera");
            reversed = false;
        }
        if (other.CompareTag("mid-goal"))
        {
            if (colliderList.Count > 1)
            {
                colliderList[colliderList.Count - 1].tag = "attraversato";
            }
            
            if (!colliderList.Contains(other))
                colliderList.Add(other);
        }
    }
}
