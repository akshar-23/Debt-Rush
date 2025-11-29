using System;
using UnityEngine;

public class DestinationTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.isAtDestination = true;

                Debug.Log("One Player has reached the destination!");
            }
        }
    }
}
