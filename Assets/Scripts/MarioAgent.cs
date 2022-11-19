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

    public float timeRemaining = 240;
    public float timeReward = 240;

    private bool fail = false;


    public GeneratorAgent gen;


    public override void Initialize()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        //m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;
        //spawnArea.SetActive(false);

        print(agentSettings);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
         
        // TODO: normalize positions
        sensor.AddObservation(rigidbody.transform.position.x);
        sensor.AddObservation(rigidbody.transform.position.y);

        sensor.AddObservation(velocity);

        sensor.AddObservation(grounded);
        sensor.AddObservation(jumping);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var movement = actionBuffers.DiscreteActions[0];
        var jp = actionBuffers.DiscreteActions[1];
        var direction = 0f;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // -------------------------------------------------------------- REWARD --------------------------------------------------------------------
            AddReward(-0.0000000001f * (timeReward - timeRemaining));

        }
        else
        {
            //AddReward(-99999999999.0f * Mathf.Abs(rigidbody.position.x-GameObject.FindGameObjectWithTag("Win").transform.position.x) );
            // -------------------------------------------------------------- REWARD --------------------------------------------------------------------
            AddReward(-100.0f);
            Finish();
            print("TERMINADO POR TIEMPO");
            GameManager.Instance.ResetLevel(0f);
            timeRemaining = 240;

        }

        switch (movement)
        {
           
            case 1:
                direction = -1f;
                AddReward(-1f);
                break;
            case 2:
                direction = 1f;
                AddReward(1f);
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

        //// clamp within the screen bounds
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

        rigidbody.MovePosition(position);

        // -------------------------------------------------------------- REWARD --------------------------------------------------------------------

        AddReward(agentSettings.stepReward / MaxStep);


    }

   
    public void Reset()
    {
        print("-----Reset-----");
        print(agentSettings.startingPos);
      
        rigidbody.transform.position = agentSettings.startingPos;
      
        //rigidbody.velocity = new Vector2(0f, 0f);
        velocity = new Vector2(0f, 0f);
        //rigidbody.angularVelocity = 0f;
    }
    public override void OnEpisodeBegin()
    {
        print("Reset on EPISODE BEGIN");
        Reset();

    }


    public void Finish()
    {
        EndEpisode();
        gen.EndEpisode();
    }

    public void WinLevel()
    {
        print("EPISODE FINISHED");
        // -------------------------------------------------------------- REWARD --------------------------------------------------------------------
        AddReward(agentSettings.flagReward);
        AddReward(transform.position.x - agentSettings.startingPos.x);
        print("===HA GANADO");
        // reward al generador
        gen.AddReward(-50f);
        //fail = false;
        
        Reset();
        GameManager.Instance.ResetLevel();
        Finish();
    }

    public void LoseLevel()
    {
        print("EPISODE FINISHED");
        // -------------------------------------------------------------- REWARD --------------------------------------------------------------------
        AddReward(agentSettings.dieReward);
        AddReward(rigidbody.transform.position.x - agentSettings.startingPos.x);

        // reward al generador
        //if (!fail)
        //{
        //    gen.AddReward(50f);
        //    fail = true;
        //}
        gen.AddReward(50f);

        

        Reset();
        GameManager.Instance.ResetLevel();
        Finish();
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------       MOVEMENT       -------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------------------------------------------------



    private void HorizontalMovement(float inputAxis)
    {
        // accelerate / decelerate
        //inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * agentSettings.moveSpeed, agentSettings.moveSpeed * Time.deltaTime);

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
            //AddReward(-100f);
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

        AddReward(agentSettings.stepReward / MaxStep);
    }
}