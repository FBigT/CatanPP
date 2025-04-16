using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;
using Assets.Scripts.Enums;
using Catan.Managers;
using Catan.UI;

namespace Catan.Placement
{
    public class SettlementPlacementHandler : MonoBehaviour
    {
        /// <summary>
        /// Call this with tileId + cornerIndex when the player clicks a corner.
        /// </summary>
        public void OnBoardClick(int tileId, int cornerIndex)
        {
            if (PurchaseManager.Instance.SelectedPurchase != PurchaseType.Settlement)
            {
                Debug.Log("⚠️ No settlement purchase pending.");
                return;
            }

            StartCoroutine(PlaceSettlement(tileId, cornerIndex));
        }

        private IEnumerator PlaceSettlement(int tile, int corner)
        {
            var form = new WWWForm();
            form.AddField("owner", LocalStorageService.GetString("username") ?? "tester");
            form.AddField("tileId", tile);
            form.AddField("cornerIndex", corner);

            using var req = UnityWebRequest.Post(EndpointUtils.PlaceSettlement, form);
            string token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Settlement placed");
                PurchaseManager.Instance.ClearPurchase();
                TopBarUI.Instance.RefreshResources();
            }
            else
            {
                Debug.LogError($"❌ Settlement placement failed: {req.error}");
            }
        }
    }
}
