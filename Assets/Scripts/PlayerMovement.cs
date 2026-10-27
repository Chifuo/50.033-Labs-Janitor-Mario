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
    public float speed;
    public float maxSpeed;
    [Min(0f)] public float dashSpeed;
    [Min(0.01f)] private float dashDuration = 0.15f;
    [Min(0f)] private float dashCooldown = 1f;
    private Rigidbody2D marioBody;
    private SpriteRenderer marioSprite;
    private bool faceRightState = true;
    private Vector2 dashDirection;
    private float dashTimeRemaining;
    private float cooldownRemaining;
    private bool isDashing;
    private bool isDying;
    public bool isSpedup;
    private Vector2 movementInput;
    private GameManager manager;
    private JanitorMario.FSM.MarioStateController mainFsm;
    public bool GameIsOver => manager != null && manager.IsGameOver;
    public JanitorMario.FSM.JanitorRankProgress RankProgress => manager != null ? manager.RankProgress : null;
    public JanitorMario.FSM.BuffProgress BuffProgress => manager != null ? manager.BuffProgress : null;
    private readonly HashSet<GoombaShoo> fightingGoombas = new HashSet<GoombaShoo>();
    public bool IsFightingGoomba => fightingGoombas.Count > 0;
    public void BeginGoombaFight(GoombaShoo enemy)
    {
        fightingGoombas.Add(enemy);
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
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;
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

        deathFinished.Invoke();
    }
    public void SetSpeedUp(bool active)
    {
        bool wasSpedUp = isSpedup;
        isSpedup = active;
        speed = active ? 200f : 100f;
        maxSpeed = active ? 400f : 200f;
        dashSpeed = 400f;
        if (wasSpedUp && !active)
            manager.playerEndSpedup.Invoke();
    }

    private void Awake()
    {
        marioSprite = GetComponent<SpriteRenderer>();
        marioBody = GetComponent<Rigidbody2D>();
        mainFsm = GetComponent<JanitorMario.FSM.MarioStateController>();
    }

    private void HandleGameOver()
    {
        // A running FSM starts death through its Fired setup action.
        if (mainFsm == null || !mainFsm.ControlsGameplay)
            BeginDeath();
    }

    private void Start()
    {
        isDying = false;
        manager = GameManager.instance;
        manager.gameOver.AddListener(HandleGameOver);
        SetSpeedUp(false);

    }
    private void OnDestroy()
    {
        if (manager == null)
            return;
        manager.gameOver.RemoveListener(HandleGameOver);
    }
    private void Update()
    {
        if (isDying)
            return;

        marioAnimator.SetFloat("xSpeed", marioBody.linearVelocity.magnitude);

        if (GameIsOver)
            return;

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

    }

    public void Move(Vector2 value)
    {
        movementInput = Vector2.ClampMagnitude(value, 1f);
    }

    public void Dash()
    {
        if (!isActiveAndEnabled || isDying || GameIsOver ||
            isDashing || cooldownRemaining > 0f)
            return;

        dashDirection = movementInput.normalized;
        if (dashDirection == Vector2.zero)
            dashDirection = faceRightState ? Vector2.right : Vector2.left;

        dashTimeRemaining = Mathf.Max(0.01f, dashDuration);
        isDashing = true;
    }

    public void Shoo()
    {
        if (!isActiveAndEnabled || isDying || GameIsOver)
            return;

        if (mainFsm != null && mainFsm.ControlsGameplay)
        {
            mainFsm.Shoo();
            return;
        }

        ShooContact(1f);
    }

    public void ShooContact(float strength)
    {
        if (!isActiveAndEnabled || isDying || GameIsOver)
            return;

        // A successful shoo removes the Goomba from this set.
        foreach (GoombaShoo enemy in new List<GoombaShoo>(fightingGoombas))
        {
            if (enemy != null)
                enemy.TryShoo(this, strength);
        }
    }
    private void FixedUpdate()
    {
        if (isDying)
            return;

        if (GameIsOver)
        {
            marioBody.linearVelocity = Vector2.zero;
            marioBody.angularVelocity = 0f;
            isDashing = false;
            cooldownRemaining = 0f;
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
