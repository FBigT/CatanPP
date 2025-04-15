using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Managers;
using Assets.Scripts.Enums;
using Assets.Scripts.Utils;

public class SettlementPlacementHandler : MonoBehaviour
{
    // This script should be attached to a GameObject that manages board clicks.
    // It assumes that when the player clicks on a valid placement connector, this script is triggered.
    // For simplicity, we assume a method OnBoardClick is called with the placement info.

    // Simulated method for demonstration:
    public void OnBoardClick(int tileId, int cornerIndex)
    {
        // Check if the current purchase is Settlement
        if (PurchaseManager.Instance.SelectedPurchase == PurchaseType.Settlement)
        {
            Debug.Log("Placing purchased settlement...");
            StartCoroutine(PlaceSettlement(tileId, cornerIndex));
        }
        else
        {
            Debug.Log("No settlement purchase pending.");
        }
    }

    private IEnumerator PlaceSettlement(int tileId, int cornerIndex)
    {
        // Prepare data for placement request
        WWWForm form = new WWWForm();
        form.AddField("owner", LocalStorageService.GetString("username"));
        form.AddField("tileId", tileId);
        form.AddField("cornerIndex", cornerIndex);

        // Use the same endpoint as in your PlacementController
        string endpoint = EndpointUtils.PlaceSettlement; // Ensure this matches the backend endpoint
        UnityWebRequest request = UnityWebRequest.Post(endpoint, form);

        string token = LocalStorageService.GetString("token");
        if (!string.IsNullOrEmpty(token))
            request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Settlement placement successful: " + request.downloadHandler.text);
            // Update local resource counts via TopBarUI, if backend returns new resource totals.
            // Clear the pending purchase state:
            PurchaseManager.Instance.ClearPurchase();
        }
        else
        {
            Debug.LogError("Settlement placement failed: " + request.error);
        }
    }
}
