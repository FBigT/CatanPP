using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(Transform))]
public class CursorController3D : MonoBehaviour
{
    [System.Serializable]
    public class CursorModeData
    {
        public string name;
        public CursorMode mode;
        public Sprite sprite;
        public Vector3 localPositionOffset;
        public Vector3 localRotationEuler;
        public AudioClip sfx;

        [Header("Animation")]
        public string clickTriggerName = "Click";

        [Header("Transition")]
        [Tooltip("Delay in seconds before applying this cursor mode.")]
        public float transitionDelay = 0f;
    }

    public enum CursorMode
    {
        Idle,
        Placing
    }

    [Header("Cursor Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("SFX")]
    [SerializeField] private SFXPlayer sfxPlayer;

    [Header("Settings")]
    [SerializeField] private float cursorYLevel = 0.05f;
    [SerializeField] private CursorMode previewMode = CursorMode.Idle;

    [Header("Cursor Modes")]
    [SerializeField] private List<CursorModeData> cursorModes = new();

    private Dictionary<CursorMode, CursorModeData> modeLookup;
    private CursorMode currentMode;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        RebuildLookup();
        SetCursorMode(currentMode);
    }

    private void OnEnable()
    {
        InputManager.OnLeftMouseClick += TriggerClickAnimation;
        InputManager.OnLeftMouseClick += TriggerClickSFX;
    }

    private void OnDisable()
    {
        InputManager.OnLeftMouseClick -= TriggerClickAnimation;
        InputManager.OnLeftMouseClick -= TriggerClickSFX;
    }

    private void Update()
    {
        UpdateCursorPosition();
    }

    private void OnValidate()
    {
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        modeLookup = new();
        foreach (var data in cursorModes)
        {
            if (!modeLookup.ContainsKey(data.mode))
                modeLookup.Add(data.mode, data);
        }
    }

    private void UpdateCursorPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, cursorYLevel, 0));
        if (plane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter);
        }
    }

    public void SetCursorMode(CursorMode mode, bool useTransitionDelay = false)
    {
        if (!modeLookup.TryGetValue(mode, out var data))
        {
            Debug.LogWarning($"Cursor mode {mode} not found.");
            return;
        }

        // Cancel any ongoing transition
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        if (useTransitionDelay && data.transitionDelay > 0f)
        {
            transitionCoroutine = StartCoroutine(ApplyModeWithDelay(data));
        }
        else
        {
            ApplyMode(data);
            currentMode = data.mode;

            if (animator != null)
                animator.Play(data.mode.ToString());
        }
    }

    private IEnumerator ApplyModeWithDelay(CursorModeData data)
    {
        yield return new WaitForSeconds(data.transitionDelay);

        ApplyMode(data);
        currentMode = data.mode;

        if (animator != null)
            animator.Play(data.mode.ToString());
    }

    private void ApplyMode(CursorModeData data)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = data.sprite;

        if (visualRoot != null)
        {
            visualRoot.localPosition = data.localPositionOffset;
            visualRoot.localRotation = Quaternion.Euler(data.localRotationEuler);
        }
    }

    private void ApplyPreviewMode(CursorMode mode)
    {
        if (modeLookup == null || !modeLookup.TryGetValue(mode, out var data)) return;
        ApplyMode(data);
    }

    private void TriggerClickAnimation()
    {
        if (!modeLookup.TryGetValue(currentMode, out var data)) return;

        if (animator != null && !string.IsNullOrEmpty(data.clickTriggerName))
            animator.SetTrigger(data.clickTriggerName);
    }

    private void TriggerClickSFX()
    {
        if (sfxPlayer != null && modeLookup.TryGetValue(currentMode, out var data) && data.sfx != null)
        {
            sfxPlayer.PlayClip(data.sfx);
        }
    }

    public CursorMode GetCurrentMode() => currentMode;
}
