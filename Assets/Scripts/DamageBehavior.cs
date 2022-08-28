using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    To use:
    1. Put this script on any object that would damage the player
    2. set the damage equal to how much damage you would like the object to deal to the player
    3. make the collider on the object a trigger
*/

public class DamageBehavior : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private PlayerBehavior player;
    public int damage;

    private bool canDamage = true;
    // private int collisions = 0;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerBehavior>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == PLAYER_TAG) {
            if (canDamage == true) {
                player.Damage(damage);
                canDamage = false;
            }
            // ++collisions;
            // Debug.Log(collisions);
        }
    }
    void OnTriggerExit(Collider other) {
        // if (other.tag == PLAYER_TAG) {
        //     if (--collisions == 0) {
                canDamage = true;
            // }
            // Debug.Log(collisions);
        // }
    }
}
