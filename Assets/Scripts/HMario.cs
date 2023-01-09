using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HMario : Agent
{
   
    private new Rigidbody2D rigidbody;
    private new Camera camera;
    private new Collider2D collider;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;

    public float maxJumpHeight = 8f;
    public float maxJumpTime = 2f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    public float timeRemaining = 240;
    public float timeReward = 240;
    public override void Initialize()
    {

        // Cache the agent rb
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var movement = actionBuffers.DiscreteActions[0];
        var jp = actionBuffers.DiscreteActions[1];
        var direction = 0f;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            AddReward(-0.0000000001f*(timeReward - timeRemaining)); 

        } 
        else
        {
            AddReward(-99999999999.0f);
            EndEpisode();
            GameManager.Instance.ResetLevel(0f);
            timeRemaining = 240;

        }

        switch (movement)
        {
            case 1:
                direction = -1f;
                break;
            case 2:
                direction = 1f;
                AddReward(200.0f);
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
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[1] = 1;
        }

        var jp = discreteActionsOut[1];
        var direction = 0f;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            AddReward(-0.0000000001f * (timeReward - timeRemaining));

        }
        else
        {
            AddReward(-100.0f);
            EndEpisode();
            GameManager.Instance.ResetLevel(0f);
            timeRemaining = 240;
        }

        switch (discreteActionsOut[0])
        {
            case 1:
                direction = -1f;
                break;
            case 2:
                direction = 1f;
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
            AddReward(-1.0f);
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

    public override void OnEpisodeBegin()
    {
        rigidbody.velocity = new Vector2(0f, 0f);
        rigidbody.angularVelocity = 0f;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Win"))
        {
            AddReward(9999999999999.0f);
            EndEpisode();
            GameManager.Instance.ResetLevel(0f);
        }
        else if (collision.CompareTag("Lose"))
        {
            AddReward(-99999999999.0f);
            EndEpisode();
            GameManager.Instance.ResetLevel(0f);
        }
    }

}