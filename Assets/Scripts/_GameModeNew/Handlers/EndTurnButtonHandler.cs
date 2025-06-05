using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Utils;
using Gamemode.New;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonHandler : MonoBehaviour
{
    public Button endTurnButton;

    private void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private async void OnEndTurnClicked()
    {
        var prev = GameModeManager.Instance.CurrentPlayer;

        GameModeManager.Instance.StartNextTurn();

        var current = GameModeManager.Instance.CurrentPlayer;
        int turnNum = GameModeManager.Instance.CurrentTurn;

        var endTurnDto = new EndTurnResponse
        {
            previousPlayerName = prev,
            currentPlayerName = prev,
            nextPlayerName = current,
            turnNumber = turnNum
        };

        var moveDto = new GameMoveDto(endTurnDto)
        {
            gameMoveType = Assets.Scripts.Enums.GameMoveType.END_TURN
        };

        await WebSocketService.SendGameMove(moveDto);

        await WebSocketService.SendMessage($"{prev} ended their turn. It’s now {current}’s turn.");
    }
}
