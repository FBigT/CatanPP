using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Dtos.GameMoveResponses;
using System.Linq;
using Assets.Scripts.Utils;

public class VictorySceneManager : MonoBehaviour
{
    public Transform playerListParent;      // Assign PlayerListPanel here in Inspector
    public GameObject playerScorePrefab;    // Assign PlayerScoreEntry prefab here

    void Start()
    {
        var victory = VictoryDataHolder.VictoryData;
        if (victory == null || victory.players == null) return;

        // Sort players by score in descending order (highest first)
        var sortedPlayers = victory.players.OrderByDescending(p => p.score).ToList();

        // Get current player's username from LocalStorage
        string currentPlayerName = LocalStorageService.GetString("username");

        // Determine the winning score
        int winningScore = sortedPlayers.First().score;

        foreach (var player in sortedPlayers)
        {
            Debug.Log($"Player data: name={player.username}, score={player.score}");

            var entry = Instantiate(playerScorePrefab, playerListParent);
            Debug.Log($"Instantiated prefab: {entry.name}");

            var texts = entry.GetComponentsInChildren<TMPro.TMP_Text>();
            if (texts.Length < 2)
            {
                Debug.LogError("Prefab missing Text components!");
                continue;
            }
            Debug.Log($"Text components found: {texts.Length}");

            // Set player name and score
            texts[0].text = player.username;
            texts[1].text = player.score.ToString();

            // Add win/lose message for current player
            if (player.username == currentPlayerName)
            {
                if (player.score == winningScore)
                {
                    texts[0].text += " - U WON!";
                    texts[0].color = Color.green; // Make winner text green
                }
                else
                {
                    texts[0].text += " - U LOST!";
                    texts[0].color = Color.red; // Make loser text red
                }
            }
            // Optionally highlight the winner(s) for all players
            else if (player.score == winningScore)
            {
                texts[0].text += " - WINNER!";
                texts[0].color = Color.yellow; // Make other winners yellow
            }
        }
    }
}
