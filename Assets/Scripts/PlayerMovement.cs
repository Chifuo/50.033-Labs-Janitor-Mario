using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerMovement : MonoBehaviour
{
    public Animator marioAnimator;
    public UnityEvent deathFinished = new UnityEvent();
    public UnityEvent goombaShooed = new UnityEvent();
    public float deathImpulse = 50f;
    public float deathGravityScale = 10f;
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
    private bool isDying;
    private Vector2 movementInput;
    private GameManager gameManager;
    private readonly HashSet<GoombaShoo> fightingGoombas = new HashSet<GoombaShoo>();
    public bool IsFightingGoomba => fightingGoombas.Count > 0;
    // For now, its not so much of a fight, more like... press space to shoo it away. 
    // In the future perhaps we can introduce broom slashing?
    public void BeginGoombaFight(GoombaShoo enemy)
    {
        fightingGoombas.Add(enemy);
        // Contact lets Mario shoo; physics handles the bump.
        // isDashing = false;
        // cooldownRemaining = Mathf.Max(0f, dashCooldown);
        // marioBody.linearVelocity = Vector2.zero;
    }

    public void EndGoombaFight(GoombaShoo enemy)
    {
        fightingGoombas.Remove(enemy);
    }

    public void BeginDeath()
    {
        if (isDying)
            return;

        isDying = true;
        isDashing = false;
        marioBody.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        marioAnimator.ResetTrigger("onSkid");
        marioAnimator.Play("mario-die", 0, 0f);
    }

    public void PlayDeathImpulse()
    {
        if (!isDying)
            return;

        marioBody.gravityScale = deathGravityScale;
        marioBody.linearDamping = 0f;
        marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);
    }

    public void FinishDeath()
    {
        if (!isDying)
            return;

        Time.timeScale = 0f;
        deathFinished.Invoke();
    }

    private void Start()
    {
        marioSprite = GetComponent<SpriteRenderer>();
        marioBody = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
    }
    private void Update()
    {
        if (isDying)
            return;

        marioAnimator.SetFloat("xSpeed", marioBody.linearVelocity.magnitude);

        // if (IsFightingGoomba)
        //     return;
        if (gameManager != null && gameManager.IsGameOver)
            return;

        movementInput = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

        if (movementInput.x < 0f && faceRightState)
        {
            if (marioBody.linearVelocity.x > 0.1f)
                marioAnimator.SetTrigger("onSkid");

            faceRightState = false;
            marioSprite.flipX = true;
        }

        if (movementInput.x > 0f && !faceRightState)
        {
            if (marioBody.linearVelocity.x < -0.1f)
                marioAnimator.SetTrigger("onSkid");

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
        if (isDying)
            return;

        if (gameManager != null && gameManager.IsGameOver)
        {
            marioBody.linearVelocity = Vector2.zero;
            marioBody.angularVelocity = 0f;
            isDashing = false;
            cooldownRemaining = 0f;
            return;
        }

        // Old contact freeze. Keep moving so Mario can steer away or push the goombas physically.
        // if (IsFightingGoomba)
        // {
        //     marioBody.linearVelocity = Vector2.zero;
        //     marioBody.angularVelocity = 0f;
        //     return;
        // }

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
