using UnityEngine;

public class HexTile : MonoBehaviour
{
    public string resourceType;
    public int numberToken;

    public MeshRenderer renderer;
    public TextMesh numberText;

    public Material woodMat, brickMat, wheatMat, sheepMat, oreMat, desertMat;

    public void Initialize(string resource, int number)
    {
        resourceType = resource;
        numberToken = number;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (renderer == null) return;

        switch (resourceType)
        {
            case "wood": renderer.material = woodMat; break;
            case "brick": renderer.material = brickMat; break;
            case "wheat": renderer.material = wheatMat; break;
            case "sheep": renderer.material = sheepMat; break;
            case "ore": renderer.material = oreMat; break;
            case "desert": renderer.material = desertMat; break;
        }

        if (numberText != null)
            numberText.text = (numberToken > 0) ? numberToken.ToString() : "";
    }
}
