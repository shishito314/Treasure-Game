using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
  public float sight = 6f; // The sight range of the paladin
  public float attackRange = 1; // The attack range of the paladin-- he will not go further toward player than this
  public float angle = 45; // The angle of sight
  public LayerMask objectsLayers;
  public LayerMask obstaclesLayers;
  public Collider detectedObject;

  public bool isAttackable;
  public bool isSeen;
  void Update()
  {
    Collider[] colliders = Physics.OverlapSphere(transform.position, sight, objectsLayers);

    for (int i = 0; i < colliders.Length; ++i)
    {
      Collider collider = colliders[i];
      Vector3 directionToCollider = Vector3.Normalize(collider.bounds.center - transform.position);
      float angleToCollider = Vector3.Angle(transform.forward, directionToCollider);
      if (angleToCollider < angle)
      {
        if (!Physics.Linecast(transform.position, collider.bounds.center, out RaycastHit hit, obstaclesLayers))
        {
          if (collider.gameObject.CompareTag("Player")) // CHANGE TO WHATEVER THE PLAYER'S TAG IS
          {
            detectedObject = collider;
            float distance = Vector3.Distance(collider.ClosestPoint(transform.position), transform.position);
            if (distance < attackRange)
            {
              isAttackable = true;
            }
            else
            {
              isSeen = true;
              isAttackable = false;
            }
          }
        }
      }
    }
  }
}
