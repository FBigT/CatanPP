    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class ResourceMapperUI : MonoBehaviour
    {
        public static ResourceMapperUI Instance;
        [Serializable]
        public class ResourceTextBinding
        {
            public string resourceName;
            public TextMeshProUGUI targetText;
        }

        public List<ResourceTextBinding> bindings;

        private Dictionary<string, int> resourceValues = new Dictionary<string, int>();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        void Start()
        {
            UpdateTextFields();
        }

        public void UpdateTextFields()
        {
            foreach (var binding in bindings)
            {
                if (binding.targetText != null && resourceValues.TryGetValue(binding.resourceName, out int value))
                {
                    binding.targetText.text = value.ToString();
                }
                else if (binding.targetText != null)
                {
                    binding.targetText.text = "0";
                }
            }
        }

        public void SetResourceValue(string name, int value)
        {
            resourceValues[name] = value;
            UpdateTextFields();
        }
    }
