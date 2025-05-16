// Assets/Scripts/Resources/ResourceMaterialDictionary.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceMaterials", menuName = "Catan/Resource Material Dictionary")]
public class ResourceMaterialDictionary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public ResourceType type;
        public Material material;
    }

    public Entry[] entries;

    private Dictionary<ResourceType, Material> _lookup;

    public Material GetMaterial(ResourceType type)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<ResourceType, Material>();
            foreach (var entry in entries)
                _lookup[entry.type] = entry.material;
        }
        return _lookup.TryGetValue(type, out var mat) ? mat : null;
    }
}
