using System;
using System.Threading.Tasks;
using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Enums;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class GameActionsUI : MonoBehaviour
{
    public Button rollDiceButton;
    public Button endTurnButton;

    private void Start()
    {
        if (rollDiceButton != null)
            rollDiceButton.onClick.AddListener(() => _ = OnRollDiceClicked());

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(() => _ = OnEndTurnClicked());
    }

    private async Task OnRollDiceClicked()
    {
        string username = LocalStorageService.GetString("username");
        int diceResult = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7); // 2d6 roll

        var diceDto = new DiceResultDto
        {
            username = username,
            rollResult = diceResult
        };

        var gameMoveDto = new GameMoveDto
        {
            gameMoveType = GameMoveType.DICE_ROLL,
            moveData = diceDto
        };

        Debug.Log($"[GameActionsUI] Sending DICE_ROLL: {diceResult} by {username}");
        await WebSocketService.SendGameMove(gameMoveDto);
    }

    private async Task OnEndTurnClicked()
    {
        // In a real game, you'd fetch these from game state
        var endTurnResponse = new EndTurnResponse
        {
            previousPlayerName = LocalStorageService.GetString("username"),
            currentPlayerName = "OtherPlayer",
            nextPlayerName = "NextPlayer",
            turnNumber = 1 // This should ideally come from game state
        };

        var gameMoveDto = new GameMoveDto
        {
            gameMoveType = GameMoveType.END_TURN,
            moveData = endTurnResponse
        };

        Debug.Log("[GameActionsUI] Sending END_TURN");
        await WebSocketService.SendGameMove(gameMoveDto);
    }
}
