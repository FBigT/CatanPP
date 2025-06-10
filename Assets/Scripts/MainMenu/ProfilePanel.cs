using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    public TMP_Text username;
    public TMP_Text gamesWonValue;
    public TMP_Text gamesPlayedValue;
    public TMP_Text gamesLostValue;
    public TMP_Text turnsTakenValue;
    public TMP_Text resourcesGatheredValue;
    public TMP_Text structuresPlacedValue;
    public TMP_Text roadsPlacedValue;
    public TMP_Text skinsUnlockedValue;
    public GameObject mainPanel;

    public Button forgetButton;

    void Awake(){
        UserManager userManager = this.AddComponent<UserManager>();
        userManager.GetCurrentPlayerProfile(SetPlayerProfile, SetError);

        forgetButton.onClick.AddListener(() => userManager.ForgetCurrentUser(OnForgetSuccess, SetError));
    }

    private void SetPlayerProfile(PlayerProfile playerProfile) { 
        username.text = playerProfile.username;
        gamesWonValue.text = playerProfile.gamesWon; 
        gamesLostValue.text = playerProfile.gamesLost; 
        gamesPlayedValue.text = playerProfile.gamesPlayed; 
        turnsTakenValue.text = playerProfile.turnsTaken; 
        resourcesGatheredValue.text = playerProfile.resourcesGathered; 
        structuresPlacedValue.text = playerProfile.structuresPlaced; 
        roadsPlacedValue.text = playerProfile.roadsPlaced;
        skinsUnlockedValue.text = playerProfile.skinsUnlocked;
    }

    private void SetError(string error) { 
        Debug.LogError(error);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
            mainPanel.SetActive(true);
        }
    }

    private void OnForgetSuccess() {
        LocalStorageService.ClearAll();
        SceneManager.LoadScene("Login");
    }
}
