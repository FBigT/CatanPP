using System;
using UnityEngine;
using Catan.Dice;           // your existing DiceRollManager
using UnityEngine.SceneManagement;

namespace Catan.GameMode.Dice
{
    /// <summary>
    /// Listens for the two DiceFaceDetector events, computes the total once, then
    /// tells the CampaignGameMode. No changes to DiceRollManager are needed.
    /// </summary>
    public class DiceTotalHook : MonoBehaviour
    {
        public static event Action<int> OnRollTotal;

        int _sum = 0;
        int _count = 0;

        void OnEnable() => DiceFaceDetector.OnDiceLanded += OnFace;
        void OnDisable() => DiceFaceDetector.OnDiceLanded -= OnFace;

        void OnFace(DiceFaceDetector die)
        {
            _sum += die.GetTopFaceValue();
            _count += 1;

            if (_count == 2)            // both dice reported
            {
                Debug.Log($"🎲 Hook total = {_sum}");
                OnRollTotal?.Invoke(_sum);

                _sum = _count = 0;      // reset for next roll
            }
        }

        // ---- convenience -------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoSpawn()
        {
            // Ensures the hook exists once per scene.
            if (FindObjectOfType<DiceTotalHook>() == null)
            {
                var go = new GameObject("[DiceTotalHook]");
                DontDestroyOnLoad(go);
                go.AddComponent<DiceTotalHook>();
            }
        }
    }
}
