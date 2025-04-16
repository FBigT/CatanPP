// Assets/Scripts/UI/Test/ResourceTestManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using Catan.UI;

namespace Catan.UI.Test
{
    public class ResourceTestManager : MonoBehaviour
    {
        int[] _resources = new int[8];   // 8 types, all zero

        void Awake()
        {
            var doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            root.Q<Button>("AddResourcesTestButton").clicked += OnClicked;
        }

        void OnClicked()
        {
            for (int i = 0; i < _resources.Length; i++) _resources[i]++;

            Debug.Log($"[Test] resources = {string.Join(",", _resources)}");

            // push straight to the bar
            if (TopBarUI.Instance != null)
                TopBarUI.Instance.SendMessage("SetValues", _resources);
        }
    }
}
