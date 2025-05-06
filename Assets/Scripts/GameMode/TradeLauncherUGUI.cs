using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Assets.Scripts.GameMode
{
    [RequireComponent(typeof(Button))]
    public class TradeLauncherUGUI : MonoBehaviour
    {
        [SerializeField] private string tradeScene = "Trading";

        void Awake()
        {
            GetComponent<Button>()
                .onClick
                .AddListener(() => SceneManager.LoadScene(tradeScene));
        }
    }
}
