using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MarioAgent : Agent
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    public AgentSettings agentSettings;
    private Vector2 velocity;
    private float inputAxis;
    public float jumpForce => (2f * agentSettings.maxJumpHeight) / (agentSettings.maxJumpTime / 2f);
  
    public float gravity => (-2f * agentSettings.maxJumpHeight) / Mathf.Pow(agentSettings.maxJumpTime / 2f, 2f);
    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;
    public float timeRemaining;
    public float timeReward;
    public GeneratorAgent gen;
    private int curriculumStage;
    public int[] floor;

    public override void Initialize()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        timeRemaining = agentSettings.timeRemaining;
        timeReward = agentSettings.timeReward;
        curriculumStage = agentSettings.curriculumStage;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rigidbody.transform.position.y);
        sensor.AddObservation(velocity);
        sensor.AddObservation(grounded);
        sensor.AddObservation(jumping);
    }

    public void goombaReward()
    {
        AddReward(10);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var movement = actionBuffers.DiscreteActions[0];
        var jp = actionBuffers.DiscreteActions[1];
        var direction = 0f;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            AddReward(agentSettings.deathByTimeoutReward);
            Reset();
            timeRemaining = agentSettings.timeRemaining;
        }
        switch (movement)
        {
            case 1:
                direction = -1f;
                if (curriculumStage <= agentSettings.maxCurriculumMoveReward) AddReward(agentSettings.moveLeftReward);
                break;
            case 2:
                direction = 1f;
                if (curriculumStage <= agentSettings.maxCurriculumMoveReward) AddReward(agentSettings.moveRightReward);
                break;
        }

        HorizontalMovement(direction);
        
        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement(jp);
        }
        ApplyGravity();
        
        // move mario based on his velocity
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;
        
        // clamp within the screen bounds
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);
        rigidbody.MovePosition(position);
    }
    public override void OnEpisodeBegin()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        curriculumStage = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("stage", 3.0f);
        Respawn();
    }

    public float getTotalRightReward()
    {
        return (transform.position.x - agentSettings.startingPos.x) * agentSettings.totalRightMultiReward;
    }
    public void WinLevel()
    {
        AddReward(agentSettings.flagReward);
        AddReward(getTotalRightReward());
        
        // reward al generador
        gen.AddReward(agentSettings.generatorMarioWinReward);
        Reset();
    }

    public void LoseLevel()
    {
        AddReward(agentSettings.dieReward);
        AddReward(getTotalRightReward());

        gen.AddReward(agentSettings.generatorMarioLoseReward);
        Reset();
    }

    public void Respawn()
    {
        rigidbody.transform.position = agentSettings.startingPos;
        velocity = new Vector2(0f, 0f);
    }


    public void DestroyAll()
    {
        GameObject[] terr = GameObject.FindGameObjectsWithTag("terrain");
        foreach (GameObject t in terr)
        {
            GameObject.Destroy(t);
        }
        GameObject[] en = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject t in en)
        {
            GameObject.Destroy(t);
        }

        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Lose");
        foreach (GameObject s in spikes)
        {
            GameObject.Destroy(s);
        }

        GameObject[] fire = GameObject.FindGameObjectsWithTag("Fire");
        foreach (GameObject s in fire)
        {
            GameObject.Destroy(s);
        }
    }


    public void FinishEpisodes()
    {
        EndEpisode();
        gen.EndEpisode();
    }

    public void Reset()
    {
        DestroyAll();
        GameManager.Instance.ResetLevel();
        FinishEpisodes();
    }



    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------       MOVEMENT       -------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    private void HorizontalMovement(float inputAxis)
    {
        // accelerate / decelerate
        velocity.x = inputAxis * agentSettings.moveSpeed;

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
            AddReward(agentSettings.jumpReward);
        }
    }

    private void ApplyGravity()
    {
        // check if falling
        bool falling = velocity.y < 0f;
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

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[1] = 1;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // bounce off enemy head
            if (transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // stop vertical movement if mario bonks his head
            if (transform.DotTest(collision.transform, Vector2.up))
            {
                velocity.y = 0f;
            }
        }
    }


    public void MoveAgent(float direction, int jp)
    {
        HorizontalMovement(direction);

        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement(jp);
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
    }
}