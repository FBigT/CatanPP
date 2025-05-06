using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Catan.MainMenu
{
    /// <summary>Attach to the UIDocument of the main menu and set the name of the button that starts a campaign.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class CampaignLauncher : MonoBehaviour
    {
        [SerializeField] string buttonName = "CampaignButton";
        [SerializeField] string campaignScene = "Game";          // scene that contains MapGenerator & CampaignGameMode

        void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var btn = root.Q<Button>(buttonName);
            if (btn == null) { Debug.LogError($"Campaign button '{buttonName}' not found."); return; }

            btn.clicked += () => SceneManager.LoadScene(campaignScene);
        }
    }
}
