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
    public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction fire;

    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = new Vector2(1, 0f); //If not, the projectile won't move at the start of the game
    }

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        fire = playerControls.Player.Fire;
        fire.Enable();
        fire.performed += Fire;
    }
    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
    }


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

    private void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("Character fired");
    }
}
