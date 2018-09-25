using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderP1 : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player2") || other.CompareTag("Player3") || other.CompareTag("Player4"))
        {
            PlayerController otherPC = other.GetComponent<Transform>().parent.GetComponent<PlayerController>();
            PlayerController thisPC = this.GetComponent<Transform>().parent.GetComponent<PlayerController>();

            float hitPower = thisPC.GetHitPower();
            int attackLevel = thisPC.GetAttackLevel();
            
            otherPC.Hit(hitPower, GetComponent<Transform>(), attackLevel);
        }
    }
}
