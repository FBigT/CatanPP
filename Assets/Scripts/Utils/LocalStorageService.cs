using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class LocalStorageService
    {
        public static void SetVariable(string name, string value) { 
            PlayerPrefs.SetString(name, SecurityUtils.Encrypt(value));
        }

        public static void SetVariable(string name, int value)
        {
            PlayerPrefs.SetString(name, SecurityUtils.Encrypt(value.ToString()));
        }

        public static void SetVariable(string name, float value)
        {
            PlayerPrefs.SetString(name, SecurityUtils.Encrypt(value.ToString()));
        }

        #nullable enable
        public static string? GetString(string name) { 
            if (string.IsNullOrEmpty(name) || !PlayerPrefs.HasKey(name)) return null;
            
            return SecurityUtils.Decrypt(PlayerPrefs.GetString(name));
        }

        public static int? GetInt(string name)
        {
            if (string.IsNullOrEmpty(name) || !PlayerPrefs.HasKey(name)) return null;

            string stringValue = SecurityUtils.Decrypt(PlayerPrefs.GetString(name));
            if (string.IsNullOrEmpty(stringValue) || !int.TryParse(stringValue, out int value)) return null;
            return value;
        }

        public static float? GetFloat(string name)
        {
            if (string.IsNullOrEmpty(name) || !PlayerPrefs.HasKey(name)) return null;

            string stringValue = SecurityUtils.Decrypt(PlayerPrefs.GetString(name));
            if (string.IsNullOrEmpty(stringValue) || !float.TryParse(stringValue, out float value)) return null;
            return value;
        }
#nullable disable

        public static void Clear() {
            PlayerPrefs.DeleteAll();
        }
    }
}
