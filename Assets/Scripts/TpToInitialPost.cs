using System;
using UnityEngine;

public class TpToInitialPost : MonoBehaviour
{
    public Transform tpToThisPosition;
    Rigidbody player;
    void Awake()
    {
       player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>(); 
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.position = tpToThisPosition.position;
            player.linearVelocity = Vector3.zero;          // reset linear speed
            player.angularVelocity = Vector3.zero;  // reset rotation spin
        }
    }
}
