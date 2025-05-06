// Assets/Scripts/UI/TopBarUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Assets.Scripts.Utils;
using Catan.GameMode;

namespace Catan.UI
{
    public class TopBarUI : MonoBehaviour
    {
        public static TopBarUI Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Called throughout the game to update your HUD.</summary>
        public void RefreshResources() => StartCoroutine(FetchAndUpdate());

        IEnumerator FetchAndUpdate()
        {
            var req = UnityWebRequest.Get(EndpointUtils.GetResources);
            if (LocalStorageService.GetString("token") is string t && t != "")
                req.SetRequestHeader("Authorization", t);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[TopBarUI] backend unavailable: using local ({req.error})");
                if (CampaignGameMode.Instance != null)
                    SetValues(CampaignGameMode.Instance.CurrentPlayer.Resources);
                else
                    SetValues(new int[8]);
            }
            else
            {
                var payload = JsonUtility.FromJson<ResourceGroup>(req.downloadHandler.text);
                SetValues(payload.ToArray());
            }
        }

        // called via SendMessage from elsewhere
        void SetValues(int[] v)
        {
            // find every label with class “resource-value” and write it
            var doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            var labels = root.Query<Label>(className: "resource-value").ToList();
            for (int i = 0; i < labels.Count && i < v.Length; i++)
                labels[i].text = v[i].ToString();
        }

        [System.Serializable]
        public class ResourceGroup
        {
            public int lumber, wool, grain, bricks, ore, gold, silver, obsidian;
            public int[] ToArray() => new[] { lumber, wool, grain, bricks, ore, gold, silver, obsidian };
        }
    }
}
