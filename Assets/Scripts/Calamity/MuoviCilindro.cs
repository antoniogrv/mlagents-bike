using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuoviCilindro : MonoBehaviour
{
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

    }

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
}
