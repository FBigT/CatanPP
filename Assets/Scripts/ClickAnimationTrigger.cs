using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ClickAnimationTrigger : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private string clickTriggerName = "Click";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        InputManager.OnLeftMouseClick += TriggerClick;
    }

    void OnDisable()
    {
        InputManager.OnLeftMouseClick -= TriggerClick;
    }

    void TriggerClick()
    {
        animator.SetTrigger(clickTriggerName);
    }
}
