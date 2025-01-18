using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public const float DEFAULT_MOVESPEED = 5f;
    public PlayerInputAction playerControls;
    private InputAction move;

    //Movement
    [HideInInspector]
    public float lastHorizontalVector;
    [HideInInspector]
    public float lastVerticalVector;
    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public Vector2 lastMovedVector;
    
    //References
    Rigidbody2D rb;
    PlayerStats player;

    void Awake()
    {
        playerControls = new PlayerInputAction();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = new Vector2(1,0f); //If not, the projectile won't move at the start of the game
    }

    void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
    }

    void OnDisable()
    {
        move.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        InputManagement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void InputManagement()
    {
        if(GameManager.instance.isGameOver)
        {
            return;
        }
        // float moveX = Input.GetAxisRaw("Horizontal");
        // float moveY = Input.GetAxisRaw("Vertical");

        // moveDir = new Vector2(moveX,moveY).normalized;
        moveDir = move.ReadValue<Vector2>();

        if(moveDir.x != 0)
        {
            lastHorizontalVector = moveDir.x;
            lastMovedVector = new Vector2(lastHorizontalVector, 0f); //last moved x
        }

        if(moveDir.y != 0)
        {
            lastVerticalVector = moveDir.y;
            lastMovedVector = new Vector2(0f,lastVerticalVector); //last moved y
        }

        if(moveDir.x != 0 && moveDir.y !=0)
        {
            lastMovedVector = new Vector2(lastHorizontalVector,lastVerticalVector);
        }
    }

    void Move()
    {
        if(GameManager.instance.isGameOver)
        {
            return;
        }
        rb.velocity = new Vector2(moveDir.x * DEFAULT_MOVESPEED * player.Stats.moveSpeed, moveDir.y * DEFAULT_MOVESPEED * player.Stats.moveSpeed);
    }
}
