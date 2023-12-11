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
    public float speed = 1.25f;
    public float cylinderSpeed = 1.25f;
    public GameObject nearestPlane;
    public GameObject nearestCylinder;

    private float direction = -1.0f;
    private bool hasReversed = false;
    
    private Vector3 start;
    private Vector3 end;

    private Collider myCollider;

    void Update()
    {
        transform.Translate(Vector3.back * direction * speed * Time.deltaTime);
        nearestCylinder.transform.Translate(Vector3.down * direction * cylinderSpeed * Time.deltaTime);

        if ((transform.position.z < start.z && hasReversed) || (transform.position.z > end.z && !hasReversed))
        {
            hasReversed = !hasReversed;
            transform.rotation *= Quaternion.Euler(0, 180, 0);
            //Debug.Log("LA PERSONA SI E' FERMATA A :" + transform.position.z);
            if (hasReversed)
                nearestCylinder.transform.position = new Vector3(transform.position.x, nearestCylinder.transform.position.y, transform.position.z - (end.z - start.z) + 2.15f);
            else
                nearestCylinder.transform.position = new Vector3(transform.position.x, nearestCylinder.transform.position.y, transform.position.z + (end.z - start.z) - 2.15f);
            cylinderSpeed *= -1.0f;

        }
    }

    void Start()
    {

        Vector3 currentScale = transform.localScale;

        float proportionalIncrease = 0.22f;
        Vector3 newScale = currentScale * (1.0f + proportionalIncrease);

        transform.localScale = newScale;

        Rigidbody myRigidbody = GetComponent<Rigidbody>();


        if (myRigidbody == null)
        {
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

        GameObject[] cylinders = GameObject.FindGameObjectsWithTag("cylinder");

        if (cylinders.Length == 0)
        {
            return;
        }

        float closestDistanceCylinder = Mathf.Infinity;

        foreach (GameObject cylinder in cylinders)
        {
            float distance = Vector3.Distance(transform.position, cylinder.transform.position);

            if (distance < closestDistanceCylinder)
            {
                closestDistanceCylinder = distance;
                nearestCylinder = cylinder;
            }
        }
        if (nearestCylinder == null)
        {
            return;
        }

        transform.SetParent(nearestPlane.transform);
        nearestCylinder.transform.SetParent(nearestPlane.transform);

    }
    /*

    private void OnTriggerEnter(Collider other)
    {
        Transform otherTransform = other.gameObject.transform;

        otherTransform.position = new Vector3(
            otherTransform.position.x,
            otherTransform.position.y + 2.20f,
            otherTransform.position.z
        );
    }

    private void OnTriggerExit(Collider other)
    {
        Transform otherTransform = other.gameObject.transform;

        otherTransform.position = new Vector3(
            otherTransform.position.x,
            otherTransform.position.y - 2.20f,
            otherTransform.position.z
        );
    }
    */
}
