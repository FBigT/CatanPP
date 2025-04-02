using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CityManager : MonoBehaviour
{
    public GameObject settlementPrefab; // Assign in Unity Inspector
    public GameObject cityPrefab; // Assign in Unity Inspector
    public Button placeSettlementButton; // Assign in Unity Inspector
    public Button upgradeCityButton; // Assign in Unity Inspector
    private string apiUrl = "http://localhost:8080/api/cities";

    private Dictionary<long, GameObject> placedCities = new Dictionary<long, GameObject>();
    private long lastPlacedCityId = -1; // Stores the last placed city's ID for testing upgrade

    private void Start()
    {
        // Link buttons to functions
        if (placeSettlementButton != null)
            placeSettlementButton.onClick.AddListener(() => StartCoroutine(PlaceSettlement("Player1", new Vector3(0, 0, 0), 1, 1)));

        if (upgradeCityButton != null)
            upgradeCityButton.onClick.AddListener(() => StartCoroutine(UpgradeToCity(lastPlacedCityId)));
    }

    public IEnumerator PlaceSettlement(string owner, Vector3 position, int x, int y)
    {
        WWWForm form = new WWWForm();
        form.AddField("owner", owner);
        form.AddField("x", x);
        form.AddField("y", y);

        UnityWebRequest request = UnityWebRequest.Post(apiUrl + "/place", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("🔍 API Response: " + request.downloadHandler.text); // ✅ Log the response

            try
            {
                // ✅ Convert JSON to CityResponse object
                CityResponse cityData = JsonUtility.FromJson<CityResponse>(request.downloadHandler.text);
                long id = cityData.id;

                lastPlacedCityId = id; // ✅ Store last placed city ID

                // ✅ Spawn settlement at correct position
                GameObject settlement = Instantiate(settlementPrefab, position, Quaternion.identity);
                placedCities[id] = settlement;

                Debug.Log($"🏠 Settlement placed at ({cityData.x}, {cityData.y}) by {cityData.owner}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ Failed to parse city ID. Response: " + request.downloadHandler.text);
                Debug.LogError("Exception: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("❌ Failed to place settlement: " + request.error);
        }
    }

    public IEnumerator UpgradeToCity(long cityId)
    {
        if (cityId == -1)
        {
            Debug.LogError("❌ No settlement has been placed yet!");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "/" + cityId + "/upgrade", "");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && placedCities.ContainsKey(cityId))
        {
            GameObject oldSettlement = placedCities[cityId];
            Vector3 position = oldSettlement.transform.position;
            Destroy(oldSettlement);

            GameObject city = Instantiate(cityPrefab, position, Quaternion.identity);
            placedCities[cityId] = city;
            Debug.Log($"🏙️ Settlement upgraded to City: {cityId}");
        }
        else
        {
            Debug.LogError("❌ Failed to upgrade city: " + request.error);
        }
    }
}
