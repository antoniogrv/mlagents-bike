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
using Random = UnityEngine.Random;

public class MoveToGoalCalamityAgent : Agent
{
    [SerializeField] private Material red;
    [SerializeField] private Material green;
    
    [SerializeField] private GameObject flag;
    [SerializeField] private GameObject groundedFlag;

    public GameObject muroA;
    private List<GameObject> muroAchildren;

    public GameObject muroB;
    private List<GameObject> muroBchildren;

    public GameObject holes;
    private List<GameObject> holesChildren;
    public GameObject holePrefab;
    private List<GameObject> spawnedHoles;

    public int defaultTime = 11;

    private Boolean reversed = false;

    private List<Collider> colliderList;

    public int fossiCounter = 0;

    public float speed = 1.0f;
    public float steerSpeed = 0.01f;

    public float minSpeed = 1.0f;
    public float maxSpeed = 5.0f;
    public float accelerationStep = 0.3f;

    public TMP_Text timer;

    private float currentRotation = 0f;

    public float fallThreshold = -2f;

    private int MAX_FOSSI = 2;

    private float midGoalReward = 50.0f; // Checkpoint lungo il percorso
    private float returnBackReward = -1000.0f; // Reward negativo quando la moto torna indietro invece di proseguire davanti
    private float steeringReward = -1.0f; // Disincentivo per la sterzata
    private float endGoalReward = 1000.0f; // Percorso completato
    private float obstacleReward = -1000.0f; // Reward negativo quando la moto urta un ostacolo
    private float maxFossiReward = -1000.0f; // Reward negativo quando si colpisce il numero max di fossi
    private float fossoReward = -500.0f; // Reward negativo quando si colpisce un fosso

    public void Start()
    {
        muroAchildren = new List<GameObject>();
        muroBchildren = new List<GameObject>();

        initChildPool(muroA, muroAchildren);
        initChildPool(muroB, muroBchildren);

        colliderList = new List<Collider>();

        spawnedHoles = new List<GameObject>();

        holesChildren = new List<GameObject>();
        initHolesPool(holes, holesChildren);

        foreach (GameObject plane in holesChildren) {
            Debug.Log(plane + " è nella lista!");
        }

        //Codice per spawnare i muri
        holeAction("spawn");
        foreach (GameObject hole in spawnedHoles)
        {
            Debug.Log(hole + " è stato spawnato!");
        }
    }

    void holeAction(string action)
    {
        for (int i = 0; i < holesChildren.Count; i++)
        {
            GameObject plane = holesChildren[i];

            Renderer planeRenderer = plane.GetComponent<Renderer>();
            Vector3 planeMin = planeRenderer.bounds.min;
            Vector3 planeMax = planeRenderer.bounds.max;

            float randomX = Random.Range(planeMin.x, planeMax.x);
            float randomZ = Random.Range(planeMin.z, planeMax.z);

            Vector3 spawnPosition = new Vector3(randomX, 0.09f, randomZ);

            if (action.Equals("spawn"))
            {
                GameObject spawnedHole = Instantiate(holePrefab, spawnPosition, Quaternion.identity, plane.transform);
                spawnedHoles.Add(spawnedHole);
            } else if (action.Equals("move"))
            {
                spawnedHoles[i].transform.position = spawnPosition;
            }
        }
    }

    void initHolesPool(GameObject go, List<GameObject> array) 
    {
        Transform parentTransform = go.transform;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);
            GameObject childGameObject = childTransform.gameObject;
            array.Add(childGameObject);
        }
    }

    void initChildPool(GameObject muro, List<GameObject> array)
    {
        Transform parentTransform = gameObject.transform;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);
            GameObject childGameObject = childTransform.gameObject;

            array.Add(childGameObject);
        }
    }

    void setPoolTag(List<GameObject> array, string tag) 
    {
        foreach(GameObject obj in array)
        {
            obj.tag = tag;
        }

        Debug.Log("Settato il tag " + tag + " ad un certo pool di oggetti");
    }

    public int GetTimer() {
        return Int32.Parse(timer.text);
    }

    public void SetTimer(string text) {
        timer.text = text;
    }

    IEnumerator TickTimer() {
        //Debug.Log("Starting the timer!");
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

        //Servirà una funzione per spostare i fossi
        holeAction("move");


        // Ripristina la trasformazione dell'oggetto alla trasformazione originale
        fossiCounter = 0;
        transform.localPosition = new Vector3(0, 0.17f, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        setPoolTag(muroAchildren, "mid-goal");
        setPoolTag(muroBchildren, "mid-goal");

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
                break;

            case 1:
                // Sterza a destra
                AddReward(steeringReward);
                Steer(steerSpeed);
                break;

            case 2:
                // Sterza a sinistra
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
            StopCoroutine("TickTimer");
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.CompareTag("puddle"))
        {
            reversed = true;
        }
        if (coll.gameObject.CompareTag("fosso"))
        {
            fossiCounter = ++fossiCounter;

            Debug.Log("Numero di fossi colpiti: " + fossiCounter);

            if(fossiCounter >= MAX_FOSSI) 
            {
                AddReward(maxFossiReward);
                EndEpisode();
            } 
            else 
            {
                AddReward(fossoReward);
            }
            
        }
        if (coll.gameObject.CompareTag("goal"))
        {
            AddReward(endGoalReward);
            flag.GetComponent<Renderer>().material = green;
            groundedFlag.GetComponent<Renderer>().material = green;
            StopCoroutine("TickTimer");
            EndEpisode();
        }
        if (coll.CompareTag("attraversato"))
        {

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
            reversed = false;
        }

        if (other.CompareTag("mid-goal"))
        {
            if (other.transform.parent.gameObject.tag == "muro-a") 
            {
                setPoolTag(muroAchildren, "attraversato");
            }

            if (other.transform.parent.gameObject.tag == "muro-b") 
            {
                setPoolTag(muroBchildren, "attraversato");
            }

            if (colliderList.Count > 1)
            {
                colliderList[colliderList.Count - 1].tag = "attraversato";
            }
            
            if (!colliderList.Contains(other))
                colliderList.Add(other);
        }
    }
}
