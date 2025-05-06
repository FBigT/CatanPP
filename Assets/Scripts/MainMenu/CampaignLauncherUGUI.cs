// Assets/Scripts/MainMenu/CampaignLauncherUGUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Catan.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class CampaignLauncherUGUI : MonoBehaviour
    {
        [SerializeField] string campaignScene = "GameModeCampaign";

        void Awake() =>
            GetComponent<Button>().onClick.AddListener(() =>
                SceneManager.LoadScene(campaignScene));
    }
}
