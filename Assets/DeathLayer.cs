using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            //Debug.Log("Collided with player");
            // player executes code 
            other.GetComponent<PlayerController>().HandleDeath();

        }
    }
}
