using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WinBarrier : MonoBehaviour
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
            var peach = GameObject.FindWithTag("Win");
            Destroy(peach);
            agent.WinLevel();
        }
    }

}