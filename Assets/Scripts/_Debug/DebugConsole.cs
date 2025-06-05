using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Enums;
using Assets.Scripts.Utils;
using Gamemode.New;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dev
{
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    public class DebugConsole : MonoBehaviour
    {
        public GameObject consolePanel;
        public InputField inputField;

        [SerializeField] KeyCode toggleKey = KeyCode.BackQuote;

        private Dictionary<string, Action<string[]>> commands;

        private void Awake()
        {
            commands = new Dictionary<string, Action<string[]>>();
            RegisterCommands();
            consolePanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }

            if (consolePanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
            {
                HandleInput(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }

        private void ToggleConsole()
        {
            consolePanel.SetActive(!consolePanel.activeSelf);

            if (consolePanel.activeSelf)
            {
                inputField.ActivateInputField();
            }
        }

        private void HandleInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(' ');
            string command = parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (commands.ContainsKey(command))
            {
                commands[command].Invoke(args);
            }
            else
            {
                Debug.LogWarning($"Unknown command: {command}");
            }
        }

        private void RegisterCommands()
        {
            commands["give"] = args =>
            {
                if (args.Length < 2) { Debug.LogWarning("Usage: give [resource] [amount]"); return; }

                string resource = args[0];
                if (!int.TryParse(args[1], out int amount))
                {
                    Debug.LogWarning("Invalid amount.");
                    return;
                }

                Debug.Log($"[DebugConsole] Giving {amount} {resource} to player.");
                // Hook into your resource system here
            };

            commands["roll"] = async args =>
            {
                if (args.Length == 0 || !int.TryParse(args[0], out int roll))
                {
                    Debug.LogWarning("Usage: roll [number]");
                    return;
                }

                var username = GameModeManager.Instance.CurrentPlayer;
                var diceDto = new DiceResultDto { username = username, rollResult = roll };
                var moveDto = new GameMoveDto(diceDto) { gameMoveType = GameMoveType.DICE_ROLL };
                await WebSocketService.SendGameMove(moveDto);
                await WebSocketService.SendMessage($"{username} (DEBUG) rolled a {roll}");

                Debug.Log($"[DebugConsole] Forced dice roll: {roll}");
            };

            commands["endturn"] = async args =>
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
                    gameMoveType = GameMoveType.END_TURN
                };

                await WebSocketService.SendGameMove(moveDto);
                await WebSocketService.SendMessage($"{prev} (DEBUG) ended turn. Now {current}'s turn.");

                Debug.Log($"[DebugConsole] Ended turn for {prev}");
            };

            // Add more commands here
        }
    }
    #endif
}
