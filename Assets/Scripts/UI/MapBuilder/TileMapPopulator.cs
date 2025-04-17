using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class TileMapPopulator : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private List<SO_Tile> tiles;

    private VisualElement tileBar;
    private SO_Tile selectedTile;

    private void Start()
    {
        var root = uiDoc.rootVisualElement;
        tileBar = root.Q<VisualElement>("TileBar");

        foreach (var tile in tiles)
        {
            CreateTileButton(tile);
        }
    }

    private void CreateTileButton(SO_Tile tile)
    {
        var button = new Button(() => OnTileSelected(tile))
        {
            text = tile.tileName,
            tooltip = tile.tileName
        };

        if (tile != null)
        {
            var icon = new Image
            {
                image = tile.icon.texture,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 32,
                    height = 32
                }
            };
            button.Add(icon);
        }

        tileBar.Add(button);
    }

    private void OnTileSelected(SO_Tile tile)
    {
        selectedTile = tile;
        Debug.Log($"Selected tile: {tile.tileName}");
    }

    public SO_Tile GetSelectedTile()
    {
        return selectedTile;
    }

    public void RefreshUI()
    {
        if (uiDoc == null || tiles == null) return;

        var root = uiDoc.rootVisualElement;
        tileBar = root.Q<VisualElement>("TileBar");

        if (tileBar == null) return;
        tileBar.Clear();

        foreach (var tile in tiles)
        {
            CreateTileButton(tile);
        }
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(TileMapPopulator))]
public class TileMapPopulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileMapPopulator populator = (TileMapPopulator)target;

        if (GUILayout.Button("Refresh Tile UI"))
        {
            populator.RefreshUI();
        }
    }
}
#endif