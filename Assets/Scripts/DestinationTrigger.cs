using System;
using UnityEngine;

public class DestinationTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && !player.isAtDestination)
            {
                bool isFirst = true;

                // Check if any other player has already reached the destination
                if (GameManager.Instance != null)
                {
                    foreach (var p in GameManager.Instance.players)
                    {
                        if (p != null)
                        {
                            var pc = p.GetComponent<PlayerController>();
                            if (pc != null && pc.isAtDestination)
                            {
                                isFirst = false;
                                break;
                            }
                        }
                    }
                }

                player.isAtDestination = true;

                if (isFirst)
                {
                    player.reachedDestinationFirst = true;
                    Debug.Log($"Player {player.playerNumber} reached the destination first!");
                }
                else
                {
                    Debug.Log("One Player has reached the destination!");
                }
            }
        }
    }
}
