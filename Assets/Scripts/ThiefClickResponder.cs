using System.Collections;
using UnityEngine;

public class ThiefClickResponder : MonoBehaviour
{
    public float squishDuration = 0.2f;
    public float squishAmount = 0.25f;
    public AnimationCurve squishCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Special Click Sequence")]
    public int requiredClicks = 5;
    public float clickWindow = 2f;
    public GameObject revealObject;
    public float revealDuration = 3f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f; //  Higher = faster turn
    public float delayBeforeReveal = 0.1f;

    [Header("SFX")]
    public SFXPlayer sfxClick;
    public SFXPlayer sfxClickCount;
    public bool playSfxOnReveal = true;

    [Header("Animator Control")]
    public Animator animator;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine squishRoutine;
    private Coroutine revealRoutine;

    private int clickCount = 0;
    private float lastClickTime;
    private bool isRevealing = false;

    void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        if (revealObject != null)
            revealObject.SetActive(false);

        if (animator != null)
            animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (isRevealing)
            return;

        sfxClick?.Play();

        if (squishRoutine != null)
            StopCoroutine(squishRoutine);
        squishRoutine = StartCoroutine(Squish());

        CountClicks();
    }

    void CountClicks()
    {
        float currentTime = Time.time;

        if (currentTime - lastClickTime <= clickWindow)
        {
            clickCount++;
        }
        else
        {
            clickCount = 1;
        }

        lastClickTime = currentTime;

        if (clickCount >= requiredClicks)
        {
            clickCount = 0;
            if (revealRoutine != null)
                StopCoroutine(revealRoutine);
            revealRoutine = StartCoroutine(RevealAndFaceCamera());
        }
    }

    IEnumerator RevealAndFaceCamera()
    {
        isRevealing = true;

        // Pause Animator
        if (animator != null)
            animator.enabled = false;

        if (delayBeforeReveal > 0f)
            yield return new WaitForSeconds(delayBeforeReveal);

        float timer = 0f;

        if (revealObject != null)
        {
            revealObject.SetActive(true);
            if (playSfxOnReveal)
                sfxClickCount?.Play();
        }

        while (timer < revealDuration)
        {
            Vector3 camPos = Camera.main.transform.position;
            Vector3 direction = camPos - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (revealObject != null)
            revealObject.SetActive(false);

        // Smoothly return to original rotation
        float returnTime = 0f;
        float returnDuration = 0.3f;
        Quaternion startRot = transform.rotation;

        while (returnTime < returnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, returnTime / returnDuration);
            returnTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;

        // Resume Animator
        if (animator != null)
            animator.enabled = true;

        isRevealing = false;
    }

    IEnumerator Squish()
    {
        float time = 0f;

        while (time < squishDuration)
        {
            float t = time / squishDuration;
            float scaleFactor = squishCurve.Evaluate(t);

            float yScale = Mathf.Lerp(1f, 1f - squishAmount, scaleFactor);
            float xzScale = Mathf.Lerp(1f, 1f + squishAmount * 0.5f, scaleFactor);

            transform.localScale = new Vector3(
                originalScale.x * xzScale,
                originalScale.y * yScale,
                originalScale.z * xzScale
            );

            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        while (time < squishDuration)
        {
            float t = time / squishDuration;
            float reverse = squishCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, reverse);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        squishRoutine = null;
    }
}
