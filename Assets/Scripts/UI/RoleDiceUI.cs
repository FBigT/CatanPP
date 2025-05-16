using UnityEngine;
using UnityEngine.UIElements;

public class RoleDiceUI : MonoBehaviour
{
    private Button btnRoleDice;
    private ListView chatContainer;
    private VisualElement root;
    private readonly Color diceTextColor = new Color(0.9f, 0.7f, 0.2f);

    private readonly System.Collections.Generic.List<VisualElement> chatMessages = new();

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        btnRoleDice = root.Q<Button>("btnDiceRole");
        chatContainer = root.Q<ListView>("ChatContainer");

        if (btnRoleDice != null)
            btnRoleDice.clicked += OnRollDice;

        if (chatContainer != null)
        {
            chatContainer.makeItem = () => new Label();
            chatContainer.bindItem = (element, i) =>
            {
                var label = element as Label;
                if (label != null && i < chatMessages.Count)
                {
                    var item = chatMessages[i];
                    label.text = (item as Label)?.text;
                    label.style.color = item.resolvedStyle.color;
                }
            };
            chatContainer.itemsSource = chatMessages;
        }
    }

    void OnRollDice()
    {
        int die1 = Random.Range(1, 7);
        int die2 = Random.Range(1, 7);
        int total = die1 + die2;

        string message = $"You rolled: <b>{die1}</b> + <b>{die2}</b> = <b>{total}</b>";

        var msgLabel = new Label(message)
        {
            style =
            {
                color = diceTextColor,
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 12
            }
        };

        chatMessages.Add(msgLabel);
        chatContainer.Rebuild();

        Debug.Log(message);
    }
}
