using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MuoviPersona : MonoBehaviour
{
    public float speed = 2.0f; // Adjust the speed as needed
    public GameObject nearestPlane;
    private float direction = -1.0f;
    private bool hasReversed = false;
    //private float lenght = 2.5f;
    private Vector3 start;
    private Vector3 end;
    private float zSaved;

    private MoveToGoalCalamityAgent agentScript;

    void Update()
    {
        transform.Translate(Vector3.back * direction * speed * Time.deltaTime);
        //if (transform.position.z < start.z || transform.position.z > end.z) direction *= -1f;
        if ((transform.position.z < start.z && hasReversed) || (transform.position.z > end.z && !hasReversed))
        {
            Debug.Log("Cambiando direzione!");
            hasReversed = !hasReversed;
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    void Start()
    {
        GameObject moto = GameObject.Find("Moto");

        if(moto != null)
        {
            agentScript = moto.GetComponent<MoveToGoalCalamityAgent>();
        }

        if(agentScript == null) 
        {
            Debug.Log("script non trovato");
        }

        GameObject[] planes = GameObject.FindGameObjectsWithTag("plane");

        if (planes.Length == 0)
        {
            Debug.LogError("Manca il piano");
            return;
        }

        float closestDistance = Mathf.Infinity;

        foreach (GameObject plane in planes)
        {
            float distance = Vector3.Distance(transform.position, plane.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPlane = plane;
            }
        }
        if (nearestPlane == null)
        {
            Debug.LogError("Plane not assigned!");
            return;
        }

        Renderer planeRenderer = nearestPlane.GetComponent<Renderer>();

        if (planeRenderer == null)
        {
            return;
        }

        Bounds planeBounds = planeRenderer.bounds;

        start = new Vector3(transform.position.x, transform.position.y, planeBounds.min.z);
        end = new Vector3(transform.position.x, transform.position.y, planeBounds.max.z);

        zSaved = transform.position.z;
        InvokeRepeating("CheckPosition", 0f, 0.2f);
    }

    private void CheckPosition()
    {
        if(zSaved > transform.position.z)
        {
            if (agentScript != null) 
            {
                agentScript.SetCurveGoalReward(25f, 50f, 75f);
            }
        }
        if(zSaved < transform.position.z)
        {
            if (agentScript != null) 
            {
                agentScript.SetCurveGoalReward(75f, 50f, 25f);
            }
        }
        zSaved = transform.position.z;
    }
}
