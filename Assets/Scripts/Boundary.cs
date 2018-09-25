using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour {

    private void OnTriggerExit(Collider other)
    {
        other.transform.parent.position = new Vector3(0.0f, 2.0f, 0.0f);
        other.transform.parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        other.transform.parent.GetComponent<PlayerController>().ResetDamage();
    }
}
