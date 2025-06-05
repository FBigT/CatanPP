using Assets.Scripts.Dtos;
using Assets.Scripts.Utils;
using Gamemode.New;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollButtonHandler : MonoBehaviour
{
    public Button rollButton;

    private void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
    }

    private async void OnRollClicked()
    {
        var username = GameModeManager.Instance.CurrentPlayer;
        int roll = Random.Range(2, 13);  // Catan-style 2d6

        var diceDto = new DiceResultDto
        {
            username = username,
            rollResult = roll
        };

        var moveDto = new GameMoveDto(diceDto)
        {
            gameMoveType = Assets.Scripts.Enums.GameMoveType.DICE_ROLL
        };

        await WebSocketService.SendGameMove(moveDto);

        await WebSocketService.SendMessage($"{username} rolled a {roll}");
    }
}
