using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MarioAgent : Agent
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    public float flagReward;
    public float stepReward;
    public float dieReward;

    public Vector2 startingPos;

    public override void Initialize()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        //m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;
        //spawnArea.SetActive(false);
    }

  


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var dojump = 0;
        var movement = actionBuffers.DiscreteActions[0];
        var jumping = actionBuffers.DiscreteActions[1];
        var direction = 0f;

        switch (movement)
        {
            case 0:
               break;
            case 1:
                direction = -1f;
                AddReward(-1f);
                break;
            case 2:
                direction = 1f;
                AddReward(1f);
                break;

        }

        if (jumping == 1)
        {
            AddReward(-1f);
            dojump = 1;
        }
        else
        {
            AddReward(1f);
            dojump = 0;
        }

        MoveAgent(direction, dojump);
        
    }

    public void Reset()
    {
        rigidbody.position = startingPos;
        rigidbody.velocity = new Vector2(0f, 0f);
        rigidbody.angularVelocity = 0f;
    }
    public override void OnEpisodeBegin()
    {
        print("Reset on EPISODE BEGIN");
        Reset();

    }

    public void WinLevel()
    {
        print("EPISODE FINISHED");
        AddReward(flagReward);
        EndEpisode();
        //GameManager.Instance.ResetLevel(1f);
    }

    public void LoseLevel()
    {
        print("EPISODE FINISHED");
        AddReward(dieReward);
        EndEpisode();
        //GameManager.Instance.ResetLevel(1f);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------       MOVEMENT       -------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    public void MoveAgent(float direction, int dojump)
    {
        HorizontalMovement(direction);

        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement(dojump);
        }

        ApplyGravity();

        // move mario based on his velocity
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;

        //// clamp within the screen bounds
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

        rigidbody.MovePosition(position);

        AddReward(stepReward / MaxStep);
    }

    private void HorizontalMovement(float inputAxis)
    {
        // accelerate / decelerate
        //inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * Time.deltaTime);

        // check if running into a wall
        if (rigidbody.Raycast(Vector2.right * velocity.x))
        {
            velocity.x = 0f;
        }

        // flip sprite to face direction
        if (velocity.x > 0f)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (velocity.x < 0f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);

        }
    }


    private void GroundedMovement(int jp)
    {
        // prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        // perform jump
        if (jp == 1)
        {
            velocity.y = jumpForce;
            jumping = true;
        }

    }

    private void ApplyGravity()
    {
        // check if falling
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;

        // apply gravity and terminal velocity
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }

        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[1] = 1;
        }
    }


    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    /// 


    //public Vector2 GetRandomSpawnPos()
    //{
    //    var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x,
    //        m_SpawnAreaBounds.extents.x);
    //    var randomPosY = Random.Range(-m_SpawnAreaBounds.extents.z,
    //        m_SpawnAreaBounds.extents.z);

    //    var randomSpawnPos = spawnArea.transform.position +
    //        new Vector2(randomPosX, randomPosY);
    //    return randomSpawnPos;
    //}


}