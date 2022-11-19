using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SkyReward : MonoBehaviour
{
    public MarioAgent agent;
    private bool activated = false;
    
    private void Start()
    {
        agent = GameObject.FindWithTag("Player").GetComponent<MarioAgent>();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            print("??? HA SALIDO");
            //other.gameObject.SetActive(false);

            if (activated == false)
            {
                agent.AddReward(5f * Math.Sign(agent.velocity.x));
                activated = true;
            }
            
            
            
            
        }
      
    }


}