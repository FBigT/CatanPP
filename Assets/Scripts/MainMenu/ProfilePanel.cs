using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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

    [Header("Status Visuals")]
    public Graphic statusIndicator;
    public Color loadingColor = Color.yellow;
    public Color loadedColor = Color.green;
    public Color errorColor = Color.red;

    private UserManager userManager;

    void Awake()
    {
        userManager = this.AddComponent<UserManager>();
        FetchProfile();
    }

    private void FetchProfile()
    {
        if (!IsUserLoggedIn())
        {
            ClearProfile();
            SetStatusColor(errorColor);
            return;
        }

        SetStatusColor(loadingColor);
        userManager.GetCurrentPlayerProfile(SetPlayerProfile, SetError);
    }

    private void ClearProfile()
    {
        username.text = "Not logged in";
        gamesWonValue.text = "-";
        gamesLostValue.text = "-";
        gamesPlayedValue.text = "-";
        turnsTakenValue.text = "-";
        resourcesGatheredValue.text = "-";
        structuresPlacedValue.text = "-";
        roadsPlacedValue.text = "-";
        skinsUnlockedValue.text = "-";
    }

    public void ReloadProfile()
    {
        Debug.Log("Reloading player profile...");
        FetchProfile();
    }

    private void SetPlayerProfile(PlayerProfile playerProfile)
    {
        username.text = playerProfile.username;
        gamesWonValue.text = playerProfile.gamesWon;
        gamesLostValue.text = playerProfile.gamesLost;
        gamesPlayedValue.text = playerProfile.gamesPlayed;
        turnsTakenValue.text = playerProfile.turnsTaken;
        resourcesGatheredValue.text = playerProfile.resourcesGathered;
        structuresPlacedValue.text = playerProfile.structuresPlaced;
        roadsPlacedValue.text = playerProfile.roadsPlaced;
        skinsUnlockedValue.text = playerProfile.skinsUnlocked;

        SetStatusColor(loadedColor);
    }

    private void SetError(string error)
    {
        Debug.LogError("Profile fetch error: " + error);
        SetStatusColor(errorColor);
    }

    private void SetStatusColor(Color color)
    {
        if (statusIndicator != null)
            statusIndicator.color = color;
    }
    private bool IsUserLoggedIn()
    {
        string token = LocalStorageService.GetString("token");
        return !string.IsNullOrEmpty(token);
    }
}
