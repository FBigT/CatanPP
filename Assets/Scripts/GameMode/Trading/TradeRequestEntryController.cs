//using Assets.Scripts.Dtos.GameMoveResponses;
//using Assets.Scripts.GameMode.Trading.Models;
//using Assets.Scripts.User;
//using Assets.Scripts.Utils;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UIElements;

//public class TradeRequestEntryController
//{
//    Label _contentLabel;
//    Button _acceptBtn, _denyBtn;
//    TradeOfferMessage _offer;

//    public TradeRequestEntryController(VisualElement root)
//    {
//        _contentLabel = root.Q<Label>("ChatContent");
//        _acceptBtn = root.Q<Button>("AcceptButton");
//        _denyBtn = root.Q<Button>("DenyButton");

//        // note: SendResponse is now async
//        _acceptBtn.clicked += () => SendResponse(true);
//        _denyBtn.clicked += () => SendResponse(false);
//    }

//    public void Bind(ChatMessage msg)
//    {
//        _offer = JsonUtility.FromJson<TradeOfferMessage>(msg.payloadJson);
//        _contentLabel.text = $"{_offer.fromUser} offers " +
//                             $"{BuildSummary(_offer.offered)} for {BuildSummary(_offer.requested)}";

//        // only the target player sees the buttons
//        string me = LocalStorageService.GetString("username");
//        bool isTarget = msg.toUser == me;
//        _acceptBtn.visible = _denyBtn.visible = isTarget;
//    }

//    // MARK: here’s the new SendResponse
//    async void SendResponse(bool accepted)
//    {
//        // 1) decide the one‐line reply
//        string reply = accepted ? "Trade accepted" : "Trade denied";

//        // 2) send it as a normal chat message
//        await WebSocketService.SendMessage(reply);

//        // 3) disable the buttons so they can’t click again
//        _acceptBtn.SetEnabled(false);
//        _denyBtn.SetEnabled(false);
//    }

//    string BuildSummary(ResourceGroup g) =>
//        string.Join(", ",
//            g.GetResourceDictionary()
//             .Where(kvp => kvp.Value > 0)
//             .Select(kvp => $"{kvp.Value} {kvp.Key}")
//        );
//}
