using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class KillBox : MonoBehaviour
{
    [SerializeField, Tooltip("Where the entered object Will Be brought to")]
    private Transform RespawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
        //Debug.Log($"Collider {other} on {other.name} entered");
        if (player != null)
        {
            player.Teleport(RespawnPoint);
        }
        else
        {
            other.transform.position = RespawnPoint.position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (RespawnPoint != null)
        {
            Gizmos.DrawSphere(RespawnPoint.position, 0.25f);

        }
    }
}
