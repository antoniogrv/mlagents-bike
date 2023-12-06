using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Timers;
using Random = UnityEngine.Random;

public class SpawnPerson : MonoBehaviour
{
    [SerializeField] private GameObject plane1;
    [SerializeField] private GameObject plane2;
    [SerializeField] private GameObject objectToSpawn;


    void Start()
    {
        StartCoroutine(CasualWaiting(plane1));
        StartCoroutine(CasualWaiting(plane2));
    }

    void SpawnObjectOnPlane(GameObject plane)
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
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);

        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
        // Check if MuoviPersona script is already attached, if not, attach it
        MuoviPersona muoviPersonaScript = spawnedObject.GetComponent<MuoviPersona>();
        if (muoviPersonaScript == null)
        {
            spawnedObject.AddComponent<MuoviPersona>();
        }
        spawnedObject.tag = "obstacle";
        // Aggiungi un Box Collider all'oggetto
        BoxCollider boxCollider = spawnedObject.AddComponent<BoxCollider>();

        // Puoi impostare altre proprietà del collider se necessario
        // Ad esempio, puoi impostare la grandezza del collider
        boxCollider.size = new Vector3(0.75f, 1.7f, 1f);

        // Puoi anche posizionare il collider se necessario
        // Ad esempio, puoi spostare il collider in alto di 0.5 unità rispetto al centro dell'oggetto
        boxCollider.center = new Vector3(0f, 1f, 0f);
        //Debug.Log("Aggiunto!");
    }

    IEnumerator CasualWaiting(GameObject plane)
    {
        float tempoDiAttesa = Random.Range(0f, 2f);

        // Aspetta il tempo generato casualmente
        yield return new WaitForSeconds(tempoDiAttesa);

        // Questo codice verrà eseguito dopo l'attesa casuale
        SpawnObjectOnPlane(plane);
    }

    void Update()
    {
        
    }
}
