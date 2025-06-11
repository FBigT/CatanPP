using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Dtos.GameMoveResponses;

public class VictorySceneManager : MonoBehaviour
{
    public Transform playerListParent;      // Assign PlayerListPanel here in Inspector
    public GameObject playerScorePrefab;    // Assign PlayerScoreEntry prefab here

    void Start()
    {
        var victory = VictoryDataHolder.VictoryData;
        if (victory == null || victory.players == null) return;

        foreach (var player in victory.players)
        {
            var entry = Instantiate(playerScorePrefab, playerListParent);
            var texts = entry.GetComponentsInChildren<Text>();
            texts[0].text = player.username;
            texts[1].text = player.score.ToString();
        }
    }
}
