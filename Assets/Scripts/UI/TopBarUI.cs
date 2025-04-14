using UnityEngine;
using UnityEngine.UIElements;

public class TopBarUI : MonoBehaviour
{
    // Declare the labels so we can update their texts later
    private Label lumberLabel, woolLabel, grainLabel, bricksLabel, oreLabel, goldLabel, silverLabel, obsidianLabel;

    // Called when the script instance is being loaded
    void OnEnable()
    {
        // Get the root VisualElement from the UIDocument on this GameObject
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // Query for each label by the names we set in UI Builder
        lumberLabel = root.Q<Label>("Lumber");
        woolLabel = root.Q<Label>("Wool");
        grainLabel = root.Q<Label>("Grain");
        bricksLabel = root.Q<Label>("Bricks");
        oreLabel = root.Q<Label>("Ore");
        goldLabel = root.Q<Label>("Gold");
        silverLabel = root.Q<Label>("Silver");
        obsidianLabel = root.Q<Label>("Obsidian");

        // Load a custom emoji-capable font from the Resources folder.
        // Ensure the font is located at Assets/Resources/Fonts/NotoColorEmoji.ttf
        Font emojiFont = Resources.Load<Font>("Fonts/NotoColorEmoji");
        if (emojiFont != null)
        {
            // Create a new FontDefinition and assign the loaded font.
            FontDefinition fontDef = new FontDefinition();
            fontDef.font = emojiFont;

            // Assign the FontDefinition to each label so they render using the custom font
            lumberLabel.style.unityFontDefinition = fontDef;
            woolLabel.style.unityFontDefinition = fontDef;
            grainLabel.style.unityFontDefinition = fontDef;
            bricksLabel.style.unityFontDefinition = fontDef;
            oreLabel.style.unityFontDefinition = fontDef;
            goldLabel.style.unityFontDefinition = fontDef;
            silverLabel.style.unityFontDefinition = fontDef;
            obsidianLabel.style.unityFontDefinition = fontDef;
        }
        else
        {
            Debug.LogWarning("Emoji font not found. Make sure 'NotoColorEmoji.ttf' is in Assets/Resources/Fonts.");
        }

        // For testing, update the labels with some initial values.
        UpdateResourceUI(new int[] { 5, 3, 7, 2, 4, 1, 0, 0 });
    }

    // A public method to update the resource UI; you can call this as needed during gameplay.
    public void UpdateResourceUI(int[] resources)
    {
        // Order: [Lumber, Wool, Grain, Bricks, Ore, Gold, Silver, Obsidian]
        if (lumberLabel != null) lumberLabel.text = $"🌲 {resources[0]}";
        if (woolLabel != null) woolLabel.text = $"🐑 {resources[1]}";
        if (grainLabel != null) grainLabel.text = $"🌾 {resources[2]}";
        if (bricksLabel != null) bricksLabel.text = $"🧱 {resources[3]}";
        if (oreLabel != null) oreLabel.text = $"🪨 {resources[4]}";
        if (goldLabel != null) goldLabel.text = $"💰 {resources[5]}";
        if (silverLabel != null) silverLabel.text = $"🥈 {resources[6]}";
        if (obsidianLabel != null) obsidianLabel.text = $"🪵 {resources[7]}";
    }
}
