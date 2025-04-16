using UnityEngine;
using Catan.UI.Test;        // AutoLogin
using Catan.UI;      // TopBarUI

namespace Catan.Setup
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Panels/Controllers to Toggle")]
        [SerializeField] private GameObject leftMenuUI;
        [SerializeField] private GameObject placementControllerObj;
        [SerializeField] private GameObject topBarUIObj;

        void Awake()
        {
            // start disabled until login completes
            leftMenuUI.SetActive(false);
            placementControllerObj.SetActive(false);
            topBarUIObj.SetActive(false);

            AutoLogin.OnLoginComplete += OnLoggedIn;
        }

        private void OnLoggedIn()
        {
            AutoLogin.OnLoginComplete -= OnLoggedIn;

            // now enable UI and controllers
            topBarUIObj.SetActive(true);
            placementControllerObj.SetActive(true);
            leftMenuUI.SetActive(true);

            // refresh resources once
            var topBar = topBarUIObj.GetComponent<TopBarUI>();
            if (topBar != null)
                topBar.RefreshResources();
        }
    }
}