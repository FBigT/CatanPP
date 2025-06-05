using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;

public class UIBlockerManager : MonoBehaviour
{
    public GameObject darkenPanel;
    public GameObject loginPanel;
    public GameObject registerPanel;

    private Image panelImage;
    private Graphic raycastTarget;

    void Awake()
    {
        if (darkenPanel != null)
        {
            panelImage = darkenPanel.GetComponent<Image>();
            raycastTarget = darkenPanel.GetComponent<Graphic>();
        }
    }

    void Update()
    {
        UpdateBlocker();
    }

    private void UpdateBlocker()
    {
        bool userLoggedIn = IsUserLoggedIn();
        bool loginActive = loginPanel != null && loginPanel.activeInHierarchy;
        bool registerActive = registerPanel != null && registerPanel.activeInHierarchy;

        bool shouldShow = !userLoggedIn || loginActive || registerActive;

        if (darkenPanel != null)
        {
            darkenPanel.SetActive(shouldShow);

            if (panelImage != null)
            {
                var color = panelImage.color;
                color.a = shouldShow ? 0.5f : 0f;
                panelImage.color = color;
            }

            if (raycastTarget != null)
            {
                raycastTarget.raycastTarget = shouldShow;
            }
        }
    }

    private bool IsUserLoggedIn()
    {
        string token = LocalStorageService.GetString("token");
        return !string.IsNullOrEmpty(token);
    }
}
