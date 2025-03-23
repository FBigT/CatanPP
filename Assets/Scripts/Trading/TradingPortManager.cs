using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TradingPortManager : MonoBehaviour
{
    public GameObject tradingPortPrefab; // Assign the prefab in Unity
    public List<Vector3> placeholderPositions; // Temporary positions
    public Button spawnButton; // Assign a UI Button in Unity

    private List<GameObject> spawnedPorts = new List<GameObject>();

    private void Start()
    {
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(SpawnPorts);
        }

        // Uncomment the next line if you want it to spawn automatically at game start
        // SpawnPorts();
    }

    public void SpawnPorts()
    {
        if (tradingPortPrefab == null || placeholderPositions.Count == 0)
        {
            Debug.LogError("⚠️ No trading port prefab or positions assigned!");
            return;
        }

        foreach (Vector3 position in placeholderPositions)
        {
            GameObject portInstance = Instantiate(tradingPortPrefab, position, Quaternion.identity);
            portInstance.name = "TradingPort";
            spawnedPorts.Add(portInstance);
        }

        Debug.Log("✅ Trading Ports Spawned!");
    }
}
