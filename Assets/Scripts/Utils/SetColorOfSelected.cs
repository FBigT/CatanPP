using UnityEngine;
using UnityEditor;

public static class SetColorOfSelected
{
    
    /*public static void ApplyColorToSelected()
    {
        Color targetColor = Color.red; 
        SetColor(targetColor);
    }*/

    /*public static void SetColor(Color color)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material newMat = new Material(renderer.sharedMaterial);
                if (newMat.HasProperty("_Color"))
                {
                    newMat.color = color;
                    renderer.material = newMat;
                    Debug.Log($"Set color for {obj.name}");
                }
            }
        }
    }*/
}
