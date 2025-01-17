using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // References
    private Animator am;
    private PlayerMovement pm;
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        am = GetComponent<Animator>();
        pm = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimatorParameters();
        UpdateSpriteDirection();
    }

    void UpdateAnimatorParameters()
    {
        // Cập nhật tham số "Move" dựa trên tốc độ di chuyển
        bool isMoving = pm.moveDir.magnitude > 0.01f; // Kiểm tra nhân vật đang di chuyển
        am.SetBool("Move", isMoving);

        // Cập nhật hướng di chuyển
        am.SetFloat("Horizontal", pm.moveDir.x);
        am.SetFloat("Vertical", pm.moveDir.y);
    }

    void UpdateSpriteDirection()
    {
        // Lật hướng sprite theo hướng di chuyển
        if (pm.lastHorizontalVector < 0)
        {
            sr.flipX = true;
        }
        else if (pm.lastHorizontalVector > 0)
        {
            sr.flipX = false;
        }
    }

    public void SetAnimatorController(RuntimeAnimatorController c)
    {
        if (!am) am = GetComponent<Animator>();
        am.runtimeAnimatorController = c;
    }
}
