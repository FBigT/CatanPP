using UnityEngine;
using UnityEngine.UIElements;

public class TopBarUI : MonoBehaviour
{
    // We'll store references to the count labels only
    private Label lumberCount, woolCount, grainCount, bricksCount, oreCount, goldCount, silverCount, obsidianCount;

    void OnEnable()
    {
        // Get the root VisualElement from the UIDocument on this GameObject
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // Query each label by the name we gave them in UI Builder (e.g., "LumberCount")
        lumberCount = root.Q<Label>("LumberCount");
        woolCount = root.Q<Label>("WoolCount");
        grainCount = root.Q<Label>("GrainCount");
        bricksCount = root.Q<Label>("BricksCount");
        oreCount = root.Q<Label>("OreCount");
        goldCount = root.Q<Label>("GoldCount");
        silverCount = root.Q<Label>("SilverCount");
        obsidianCount = root.Q<Label>("ObsidianCount");

        // For testing, update with some initial values
        UpdateResourceUI(new int[] { 5, 3, 7, 2, 4, 1, 0, 0 });
    }

    public void UpdateResourceUI(int[] resources)
    {
        // In order: [Lumber, Wool, Grain, Bricks, Ore, Gold, Silver, Obsidian]
        if (lumberCount != null) lumberCount.text = resources[0].ToString();
        if (woolCount != null) woolCount.text = resources[1].ToString();
        if (grainCount != null) grainCount.text = resources[2].ToString();
        if (bricksCount != null) bricksCount.text = resources[3].ToString();
        if (oreCount != null) oreCount.text = resources[4].ToString();
        if (goldCount != null) goldCount.text = resources[5].ToString();
        if (silverCount != null) silverCount.text = resources[6].ToString();
        if (obsidianCount != null) obsidianCount.text = resources[7].ToString();
    }
}
