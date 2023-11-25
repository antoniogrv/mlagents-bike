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

    public float minSpeed = 1.0f;
    public float maxSpeed = 5.0f;
    public float accelerationStep = 0.3f;

    public TMP_Text timer;

    private float currentRotation = 0f;

    public float fallThreshold = -2f;

    private float[] midGoalCurveReward;
    private GameObject[] curveObject;
    private float midGoalReward = 50.0f; // Checkpoint lungo il percorso
    private float returnBackReward = -1000.0f; // Reward negativo quando la moto torna indietro invece di proseguire davanti
    private float steeringReward = -1.0f; // Disincentivo per la sterzata
    private float endGoalReward = 1000.0f; // Percorso completato
    private float obstacleReward = -1000.0f; // Reward negativo quando la moto urta un ostacolo

    void Start()
    {
        curveObject = new GameObject[6];

        for (int i = 100; i < 106; i++)
        {
            GameObject tempCurveObject = GameObject.Find("Mid-goal (" + i + ")");
            if (tempCurveObject != null)
            {
                curveObject[i - 100] = tempCurveObject;
            }
            else
            {
                Debug.Log("Oggetto non trovato con il nome: " + "Mid-goal (" + i + ")");
            }
        }

        midGoalCurveReward = new float[3];
        colliderList = new List<Collider>();
    }

    public int GetTimer() {
        return Int32.Parse(timer.text);
    }

    public void SetTimer(string text) {
        timer.text = text;
    }

    public void SetCurveGoalReward(float midGoalCurve, float midGoalCurve2, float midGoalCurve3){
        midGoalCurveReward[0] = midGoalCurve;
        midGoalCurveReward[1] = midGoalCurve2;
        midGoalCurveReward[2] = midGoalCurve3;

        //Debug.Log("mid-goal-curve: " + midGoalCurveReward[0]);
        //Debug.Log("mid-goal-curve2: " + midGoalCurveReward[1]);
        //Debug.Log("mid-goal-curve3: " + midGoalCurveReward[2]);
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

        for (int i = 100; i <= 106; i++)
        {
            if(i == 100 || i == 103)
                curveObject[i - 100].tag = "mid-goal-curve";
            if(i == 101 || i == 104)
                curveObject[i - 100].tag = "mid-goal-curve-2";
            if(i == 102 || i == 105)
                curveObject[i - 100].tag = "mid-goal-curve-3";
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

        int accelerate = actionBuffers.DiscreteActions[1];

        switch (accelerate)
        {
            case 0:
                if (speed < maxSpeed) speed += accelerationStep;
                break;
            case 1:
                break;
            case 2:
                if (speed > minSpeed) speed -= accelerationStep;
                break;
        }

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

        discreteActions[1] = 1;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[1] = 0;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[1] = 2;
        }

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
        if (coll.CompareTag("mid-goal-curve"))
        {
            if (!colliderList.Contains(coll))
            {
                AddReward(midGoalCurveReward[0]);
            }
        }
        if (coll.CompareTag("mid-goal-curve-2"))
        {
            if (!colliderList.Contains(coll))
            {
                AddReward(midGoalCurveReward[1]);
            }
        }
        if (coll.CompareTag("mid-goal-curve-3"))
        {
            if (!colliderList.Contains(coll))
            {
                AddReward(midGoalCurveReward[2]);
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
        if (other.CompareTag("mid-goal") || (other.CompareTag("mid-goal-curve") || other.CompareTag("mid-goal-curve-2") || other.CompareTag("mid-goal-curve-3") ) )
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
