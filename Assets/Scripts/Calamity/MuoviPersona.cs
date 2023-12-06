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

    private Collider myCollider;

    void Update()
    {
        transform.Translate(Vector3.back * direction * speed * Time.deltaTime);
        //if (transform.position.z < start.z || transform.position.z > end.z) direction *= -1f;
        if ((transform.position.z < start.z && hasReversed) || (transform.position.z > end.z && !hasReversed))
        {
            //Debug.Log("Cambiando direzione!");
            hasReversed = !hasReversed;
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    void Start()
    {

        Vector3 currentScale = transform.localScale;

        // Calculate the proportional increase (15% bigger)
        float proportionalIncrease = 0.22f;
        Vector3 newScale = currentScale * (1.0f + proportionalIncrease);

        // Apply the new scale to the GameObject
        transform.localScale = newScale;

        Rigidbody myRigidbody = GetComponent<Rigidbody>();


        if (myRigidbody == null)
        {
            // Add a Rigidbody component to the GameObject
            myRigidbody = gameObject.AddComponent<Rigidbody>();
            myRigidbody.isKinematic = true;
        }


        GameObject[] planes = GameObject.FindGameObjectsWithTag("plane");

        if (planes.Length == 0)
        {
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

    }

    private void OnTriggerEnter(Collider other)
    {
        Transform otherTransform = other.gameObject.transform;

        // Raise the object in height by 2.20 units
        otherTransform.position = new Vector3(
            otherTransform.position.x,
            otherTransform.position.y + 2.20f,
            otherTransform.position.z
        );
    }

    private void OnTriggerExit(Collider other)
    {
        Transform otherTransform = other.gameObject.transform;

        // Raise the object in height by 2.20 units
        otherTransform.position = new Vector3(
            otherTransform.position.x,
            otherTransform.position.y - 2.20f,
            otherTransform.position.z
        );
    }
}
