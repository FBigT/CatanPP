using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Assets.Scripts.Utils;
using Catan.UI;

namespace Catan.Dice
{
    public class DiceRollManager : MonoBehaviour
    {
        [Header("Dice Prefab & Spawn Points")]
        public GameObject dicePrefab;
        public Transform leftSpawnPoint, rightSpawnPoint;

        [Header("UI")]
        public TMP_Text resultText;

        private DiceFaceDetector detector1, detector2;
        private const string RollUrl = "http://localhost:8080/api/dice/roll";

        private void OnEnable()
            => DiceFaceDetector.OnDiceLanded += OnDiceLanded;

        private void OnDisable()
            => DiceFaceDetector.OnDiceLanded -= OnDiceLanded;

        public void RollDice()
            => StartCoroutine(RollAndSend());

        private IEnumerator RollAndSend()
        {
            var d1 = Instantiate(dicePrefab, leftSpawnPoint.position, Quaternion.identity);
            var d2 = Instantiate(dicePrefab, rightSpawnPoint.position, Quaternion.identity);

            detector1 = d1.GetComponent<DiceFaceDetector>();
            detector2 = d2.GetComponent<DiceFaceDetector>();

            var rb1 = d1.GetComponent<Rigidbody>();
            var rb2 = d2.GetComponent<Rigidbody>();

            rb1.AddForce(new Vector3(Random.Range(6, 12), Random.Range(5, 8), Random.Range(-4, 4)), ForceMode.Impulse);
            rb1.AddTorque(Random.insideUnitSphere * Random.Range(20, 40), ForceMode.Impulse);
            rb2.AddForce(new Vector3(Random.Range(-6, -12), Random.Range(5, 8), Random.Range(-4, 4)), ForceMode.Impulse);
            rb2.AddTorque(Random.insideUnitSphere * Random.Range(20, 40), ForceMode.Impulse);

            yield return new WaitUntil(() =>
                detector1 != null && detector2 != null
                && detector1.HasSettled() && detector2.HasSettled());

            int v1 = detector1.GetTopFaceValue();
            int v2 = detector2.GetTopFaceValue();
            int total = v1 + v2;
            resultText.text = $"You rolled: {total}";

            Debug.Log($"🎲 {v1}+{v2} = {total}");
            StartCoroutine(SendResult());

            Destroy(d1, 5f);
            Destroy(d2, 5f);
        }

        private void OnDiceLanded(DiceFaceDetector _)
        {
            // no-op: handled by coroutine
        }

        private IEnumerator SendResult()
        {
            using var req = UnityWebRequest.Get(RollUrl);
            string token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Dice sent");
                TopBarUI.Instance.RefreshResources();
            }
            else
            {
                Debug.LogError($"❌ Dice send error: {req.error}");
            }
        }
    }
}
