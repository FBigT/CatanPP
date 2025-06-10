using TMPro;
using UnityEditor;
using UnityEditor.SpeedTree.Importer;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public string resourceType;
    public int numberToken;

    public MeshRenderer hexRenderer;
    public TextMeshPro numberText;

    public Material woodMat, brickMat, wheatMat, sheepMat, oreMat, desertMat;

    public int Q;
    public int R;
    public int xCoord => Q;
    public int yCoord => R;

    public bool isWater;

    [Header("Highlighting")]
    public Material highlightMaterial;
    private Material originalMaterial;

    public void Initialize(string resource, int number, int q, int r)
    {
        resourceType = resource;
        numberToken = number;

        Q = q;
        R = r;

        UpdateVisuals();

        if (hexRenderer != null)
            originalMaterial = hexRenderer.material;
    }

    public void Highlight()
    {
        if (hexRenderer != null)
            hexRenderer.materials = new Material[] { originalMaterial, highlightMaterial };
    }

    public void ClearHighlight()
    {
        if (hexRenderer != null && originalMaterial != null)
            hexRenderer.materials = new Material[] { originalMaterial };
    }
    void UpdateVisuals()
    {
        if (hexRenderer == null) return;

        switch (resourceType)
        {
            case "wood": hexRenderer.material = woodMat; break;
            case "claypit": hexRenderer.material = brickMat; break;
            case "wheat": hexRenderer.material = wheatMat; break;
            case "pasture": hexRenderer.material = sheepMat; break;
            case "mountain": hexRenderer.material = oreMat; break;
            case "desert": hexRenderer.material = desertMat; break;
        }

        if (numberText != null)
            numberText.text = (numberToken > 0) ? numberToken.ToString() : "";
    }
}