using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;

using Assets.Scripts.Utils; // EndpointUtils, LocalStorageService

public class TopBarUI : MonoBehaviour
{
    private Label lumberCount, woolCount, grainCount, bricksCount, oreCount, goldCount, silverCount, obsidianCount;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        lumberCount = root.Q<Label>("LumberCount");
        woolCount = root.Q<Label>("WoolCount");
        grainCount = root.Q<Label>("GrainCount");
        bricksCount = root.Q<Label>("BricksCount");
        oreCount = root.Q<Label>("OreCount");
        goldCount = root.Q<Label>("GoldCount");
        silverCount = root.Q<Label>("SilverCount");
        obsidianCount = root.Q<Label>("ObsidianCount");

        // Fetch at startup
        StartCoroutine(FetchAndUpdateResources());
    }

    public IEnumerator FetchAndUpdateResources()
    {
        string endpoint = EndpointUtils.GetResources; // e.g. http://localhost:8080/api/game/resources
        UnityWebRequest request = UnityWebRequest.Get(endpoint);

        string token = LocalStorageService.GetString("token");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // The response is a ResourceGroup JSON: { "lumber": int, "wool": int, etc. }
            string json = request.downloadHandler.text;
            ResourceGroup rg = JsonUtility.FromJson<ResourceGroup>(json);

            int[] array = new int[]
            {
                rg.lumber, rg.wool, rg.grain, rg.bricks,
                rg.ore, rg.gold, rg.silver, rg.obsidian
            };
            UpdateResourceUI(array);
        }
        else
        {
            Debug.LogError("Failed to fetch resources: " + request.error);
        }
    }

    private void UpdateResourceUI(int[] resources)
    {
        if (lumberCount != null) lumberCount.text = resources[0].ToString();
        if (woolCount != null) woolCount.text = resources[1].ToString();
        if (grainCount != null) grainCount.text = resources[2].ToString();
        if (bricksCount != null) bricksCount.text = resources[3].ToString();
        if (oreCount != null) oreCount.text = resources[4].ToString();
        if (goldCount != null) goldCount.text = resources[5].ToString();
        if (silverCount != null) silverCount.text = resources[6].ToString();
        if (obsidianCount != null) obsidianCount.text = resources[7].ToString();
    }

    [System.Serializable]
    public class ResourceGroup
    {
        public int lumber;
        public int wool;
        public int grain;
        public int bricks;
        public int ore;
        public int gold;
        public int silver;
        public int obsidian;
    }
}
