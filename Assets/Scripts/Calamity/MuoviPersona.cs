using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuoviPersona : MonoBehaviour
{
    public float speed = 2.0f; // Adjust the speed as needed
    public GameObject nearestPlane;
    private float direction = 1.0f;
    private bool hasReversed = false;

    void Update()
    {
        if (nearestPlane == null)
        {
            Debug.LogError("Plane not assigned!");
            return;
        }

        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Check if the GameObject is beyond the bounds of the plane
        float halfPlaneWidth = nearestPlane.position.z;
        float currentX = transform.position.x;

        
    }

    void Start()
    {
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
    }
}
