using TMPro;
using UnityEditor;
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

    public void Initialize(string resource, int number, int q, int r)
    {
        resourceType = resource;
        numberToken = number;

        Q = q;
        R = r;

        UpdateVisuals();
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