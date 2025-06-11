using UnityEngine;

namespace Catan.GameMode
{
    /// <summary>Attach to the root of every Settlement / City prefab.</summary>
    public class PlayerMarker : MonoBehaviour
    {
        public int OwnerSeat;

        /// <summary>
        /// Sets color for all child renderers of the provided GameObjects.
        /// </summary>
        /// <param name="targets">GameObjects whose children will be updated</param>
        /// <param name="color">Target color to apply</param>
        public static void SetColorForObjects(GameObject[] targets, Color color)
        {
            foreach (var go in targets)
            {
                SetColorRecursively(go, color);
            }
        }

        /// <summary>
        /// Sets color for all child renderers of this prefab only.
        /// </summary>
        /// <param name="color">Target color to apply</param>
        public void SetColorForThisStructure(Color color)
        {
            SetColorRecursively(gameObject, color);
        }

        private static void SetColorRecursively(GameObject root, Color color)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_Color"))
                {
                    // Clone material to avoid affecting shared instance
                    Material newMat = new Material(rend.sharedMaterial);
                    newMat.color = color;
                    rend.material = newMat;
                }
            }
        }
    }
}
