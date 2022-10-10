using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;





public class Discriminator : Agent
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



    private void Awake()
    {
        print("aaaaa");
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    public override void Initialize()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        print("ss");
    }


    public override void OnActionReceived(ActionBuffers actions)
    {

        print("ESTAS?");
        if (actions.DiscreteActions[0] == 1)
        {
            HorizontalMovement(1);
            grounded = rigidbody.Raycast(Vector2.down);


            if (grounded)
            {
                GroundedMovement();
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
        } else
        {
            HorizontalMovement(0);
            grounded = rigidbody.Raycast(Vector2.down);


            if (grounded)
            {
                GroundedMovement();
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
    }


    public override void OnEpisodeBegin()
    {
       GameManager.Instance.ResetLevel(2);
       print("EMPIEZA");

    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //HorizontalMovement();
        print("HOLA");
        var discrete = actionsOut.DiscreteActions;


        

        if (Input.GetAxis("Horizontal") == -1.0 || Input.GetAxis("Horizontal") == 1.0)
        {
            discrete[0] = 1;
        }else
        {
            discrete[0] = 0;
        }

        


    }


    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Win")
        {
            AddReward(100.0f);
            EndEpisode();
        }else if(collision.gameObject.tag == "Lose")
        {
            AddReward(-100.0f);
            EndEpisode();
        }
    }


    protected override void OnEnable()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

    protected override void OnDisable()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        velocity = Vector2.zero;
        jumping = false;
    }


    private void HorizontalMovement(int pressed)
    {
        // accelerate / decelerate
        if (pressed == 1)
        {
            inputAxis = Input.GetAxis("Horizontal");
            velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * Time.deltaTime);
        } else
        {
 
            velocity.x = Mathf.MoveTowards(velocity.x, moveSpeed, moveSpeed * Time.deltaTime);
        }





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

    private void GroundedMovement()
    {
        // prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        // perform jump
        if (Input.GetButtonDown("Jump"))
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
}


