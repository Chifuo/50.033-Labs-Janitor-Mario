using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float speed = 100;
    public float maxSpeed = 200;
    public KeyCode dashKey = KeyCode.LeftShift;
    [Min(0f)] public float dashSpeed = 400f;
    [Min(0.01f)] public float dashDuration = 0.15f;
    [Min(0f)] public float dashCooldown = 1f;
    private Rigidbody2D marioBody;
    private SpriteRenderer marioSprite;
    private bool faceRightState = true;
    private Vector2 dashDirection;
    private float dashTimeRemaining;
    private float cooldownRemaining;
    private bool isDashing;
    private Vector2 movementInput;
    private GameManager gameManager;
    private readonly HashSet<EnemyMovement> fightingGoombas = new HashSet<EnemyMovement>();
    public bool IsFightingGoomba => fightingGoombas.Count > 0;
    // For now, its not so much of a fight, more like... press space to shoo it away. 
    // In the future perhaps we can introduce broom slashing?
    public void BeginGoombaFight(EnemyMovement enemy)
    {
        fightingGoombas.Add(enemy);
        isDashing = false;
        cooldownRemaining = Mathf.Max(0f, dashCooldown);
        marioBody.linearVelocity = Vector2.zero;
    }

    public void EndGoombaFight(EnemyMovement enemy)
    {
        fightingGoombas.Remove(enemy);
    }

    private void Start()
    {
        marioSprite = GetComponent<SpriteRenderer>();
        marioBody = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
    }
    private void Update()
    {
        if (IsFightingGoomba || (gameManager != null && gameManager.IsGameOver))
            return;

        movementInput = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

        if (movementInput.x < 0f && faceRightState)
        {
            faceRightState = false;
            marioSprite.flipX = true;
        }
        if (movementInput.x > 0f && !faceRightState)
        {
            faceRightState = true;
            marioSprite.flipX = false;
        }

        if (Input.GetKeyDown(dashKey) && !isDashing && cooldownRemaining <= 0f)
        {
            dashDirection = movementInput.normalized;
            if (dashDirection == Vector2.zero)
            {
                dashDirection = faceRightState ? Vector2.right : Vector2.left;
            }

            dashTimeRemaining = Mathf.Max(0.01f, dashDuration);
            isDashing = true;
        }
    }

    private void FixedUpdate()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            marioBody.linearVelocity = Vector2.zero;
            marioBody.angularVelocity = 0f;
            isDashing = false;
            cooldownRemaining = 0f;
            return;
        }

        if (IsFightingGoomba)
        {
            marioBody.linearVelocity = Vector2.zero;
            marioBody.angularVelocity = 0f;
            return;
        }

        if (isDashing)
        {
            if (dashTimeRemaining > 0f)
            {
                marioBody.linearVelocity = dashDirection * Mathf.Max(0f, dashSpeed);
                dashTimeRemaining -= Time.fixedDeltaTime;
                return;
            }

            isDashing = false;
            marioBody.linearVelocity = Vector2.zero;
            cooldownRemaining = Mathf.Max(0f, dashCooldown);
        }
        else
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.fixedDeltaTime);
        }

        if (movementInput == Vector2.zero)
            marioBody.linearVelocity = Vector2.zero;
        else if (marioBody.linearVelocity.magnitude < maxSpeed)
            marioBody.AddForce(movementInput * speed);
    }
}
