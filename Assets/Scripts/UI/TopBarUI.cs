// Assets/Scripts/UI/TopBarUI.cs
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.Networking;
using Assets.Scripts.Utils;
using System.Collections.Generic;            // ←  List<T>
namespace Catan.UI
{
    public class TopBarUI : MonoBehaviour
    {
        // ── Singleton (lightweight) ───────────────────────────────────────────
        public static TopBarUI Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _doc = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            // every label that has the class “resource‑value”
            _valueLabels = _root.Query<Label>(className: "resource-value").ToList();
        }

        // ── Public API you can call from buttons / managers ──────────────────
        public void RefreshResources() => StartCoroutine(FetchAndUpdate());

        // ── Implementation ────────────────────────────────────────────────────
        IEnumerator FetchAndUpdate()
        {
            var req = UnityWebRequest.Get(EndpointUtils.GetResources);
            if (LocalStorageService.GetString("token") is string t && t != "")
                req.SetRequestHeader("Authorization", $"Bearer {t}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[TopBarUI] fallback to zeros ({req.error})");
                SetValues(new int[8]);        // 8 zeros
            }
            else
            {
                var payload = JsonUtility.FromJson<ResourceGroup>(req.downloadHandler.text);
                SetValues(payload.ToArray());
            }
        }

        void SetValues(int[] v)
        {
            for (int i = 0; i < _valueLabels.Count && i < v.Length; i++)
                _valueLabels[i].text = v[i].ToString();
        }

        // ── helpers / fields ──────────────────────────────────────────────────
        [System.Serializable]
        public class ResourceGroup
        {
            public int lumber, wool, grain, bricks, ore, gold, silver, obsidian;
            public int[] ToArray() => new[]
            {
                lumber, wool, grain, bricks, ore, gold, silver, obsidian
            };
        }

        UIDocument _doc;
        VisualElement _root;
        List<Label> _valueLabels;
    }
}
