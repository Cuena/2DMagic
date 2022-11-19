using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathBarrier : MonoBehaviour
{
    public MarioAgent agent;

    private void Start()
    {
        agent = GameObject.FindWithTag("Player").GetComponent<MarioAgent>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //other.gameObject.SetActive(false);
            //GameManager.Instance.ResetLevel(3f);
            agent.LoseLevel();
            var peach = GameObject.FindWithTag("Win");
            //Destroy(peach);


        }
        else
        {
            Destroy(other.gameObject);
        }
    }

}