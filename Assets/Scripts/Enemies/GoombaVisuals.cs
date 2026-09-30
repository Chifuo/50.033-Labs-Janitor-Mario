using System.Collections;
using UnityEngine;

public class GoombaVisuals : MonoBehaviour
{
    [SerializeField] private Animator goombaAnimator;
    [SerializeField] private GameObject alert;
    [SerializeField] private Transform visual;
    [SerializeField] private float hopHeight = 0.6f;
    [SerializeField] private float hopDuration = 0.3f;
    private GoombaShoo shoo;
    private Rigidbody2D enemyBody;
    private SpriteRenderer enemySprite;
    private Vector3 visualRestPosition;
    private Coroutine hopRoutine;

    public void Initialize(Rigidbody2D body, GoombaShoo shooSource)
    {
        Unsubscribe();
        shoo = shooSource;
        if (isActiveAndEnabled)
            Subscribe();
        enemyBody = body;
        enemySprite = GetComponentInChildren<SpriteRenderer>();
        if (goombaAnimator == null)
            goombaAnimator = GetComponentInChildren<Animator>();
        if (visual != null)
            visualRestPosition = visual.localPosition;
    }

    public void UpdateFacing(Vector2 velocity)
    {
        if (enemySprite != null && Mathf.Abs(velocity.x) > 0.01f)
            enemySprite.flipX = velocity.x > 0f;
    }

    private void LateUpdate()
    {
        // Walking up/down, both speeds check
        if (goombaAnimator != null && enemyBody != null)
            goombaAnimator.SetFloat("xSpeed", enemyBody.linearVelocity.magnitude);
    }

    private void ShowShooFeedback()
    {
        if (alert != null)
        {
            alert.SetActive(true);
            CancelInvoke(nameof(HideAlert));
            Invoke(nameof(HideAlert), 1f);
        }

        if (visual != null)
        {
            if (hopRoutine != null)
                StopCoroutine(hopRoutine);
            hopRoutine = StartCoroutine(Hop());
        }
    }

    private void HideAlert()
    {
        if (alert != null)
            alert.SetActive(false);
    }

    private IEnumerator Hop()
    {
        float duration = Mathf.Max(0.01f, hopDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float height = Mathf.Sin(progress * Mathf.PI) * hopHeight;

            visual.localPosition =
                visualRestPosition + Vector3.up * height;

            elapsed += Time.deltaTime;
            yield return null;
        }

        visual.localPosition = visualRestPosition;
        hopRoutine = null;
    }

    public void ResetFeedback()
    {
        CancelInvoke(nameof(HideAlert));
        HideAlert();
        if (hopRoutine != null)
        {
            StopCoroutine(hopRoutine);
            hopRoutine = null;
            if (visual != null)
                visual.localPosition = visualRestPosition;
        }
    }

    private void OnEnable() => Subscribe();
    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (shoo == null)
            return;
        // Avoid adding the same listener twice during setup.
        shoo.Shooed -= ShowShooFeedback;
        shoo.Shooed += ShowShooFeedback;
    }

    private void Unsubscribe()
    {
        if (shoo != null)
            shoo.Shooed -= ShowShooFeedback;
    }
}
