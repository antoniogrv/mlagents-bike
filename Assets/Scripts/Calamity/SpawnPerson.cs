using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Timers;

public class SpawnPerson : MonoBehaviour
{
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject objectToSpawn;

    void Start()
    {
        SpawnObjectOnPlane();
    }

    void SpawnObjectOnPlane()
    {
        if (plane == null)
        {
            return;
        }

        // Get the bounds of the plane

        Renderer planeRenderer = plane.GetComponent<Renderer>();

        if (planeRenderer == null)
        {
            return;
        }

        Bounds planeBounds = planeRenderer.bounds;

        float randomX = UnityEngine.Random.Range(planeBounds.min.x, planeBounds.max.x);
        float randomZ = UnityEngine.Random.Range(planeBounds.min.z, planeBounds.max.z);

        float spawnY = plane.transform.position.y;

        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);
        Quaternion spawnRotation = Quaternion.Euler(0f, 90f, 0f);

        objectToSpawn.AddComponent<MuoviPersona>();

        Instantiate(objectToSpawn, spawnPosition, spawnRotation);
    }

    void Update()
    {
        
    }
}
