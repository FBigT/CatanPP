using Assets.Scripts.User;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    void Awake(){
        UserManager userManager = this.AddComponent<UserManager>();
        userManager.GetCurrentPlayerProfile(SetPlayerProfile, SetError);
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
    }

    private void SetError(string error) { 
        Debug.LogError(error);
    }
}
