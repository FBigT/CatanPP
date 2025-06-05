using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplayMapper : MonoBehaviour
{
    [System.Serializable]
    public class ResourceTextBinding
    {
        public ResourceType resourceType;
        public TMP_Text textField;
    }

    [Header("Bindings")]
    [SerializeField] private List<ResourceTextBinding> bindings = new();

    private Dictionary<ResourceType, ResourceTextBinding> bindingLookup;

    void Awake()
    {
        bindingLookup = new();

        foreach (var binding in bindings)
        {
            if (binding.textField == null)
            {
                Debug.LogWarning($"Text field not assigned for {binding.resourceType}");
                continue;
            }

            bindingLookup[binding.resourceType] = binding;
        }
    }

    /// <summary>
    /// Call this when resource values are updated elsewhere.
    /// </summary>
    public void RefreshDisplay(Dictionary<ResourceType, int> resourceData)
    {
        foreach (var kvp in resourceData)
        {
            if (bindingLookup.TryGetValue(kvp.Key, out var binding))
            {
                binding.textField.text = $"{kvp.Value}";
            }
        }
    }
}
