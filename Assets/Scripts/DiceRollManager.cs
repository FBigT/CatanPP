using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class DiceRollManager : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform leftSpawnPoint, rightSpawnPoint;
    public TMP_Text resultText;

    private GameObject dice1, dice2;
    private DiceFaceDetector detector1, detector2;

    private string apiUrl = "http://localhost:8080/api/dice/roll"; // Backend endpoint (optional)

    private void OnEnable()
    {
        DiceFaceDetector.OnDiceLanded += CheckBothDiceSettled;
    }

    private void OnDisable()
    {
        DiceFaceDetector.OnDiceLanded -= CheckBothDiceSettled;
    }

    public void RollDice()
    {
        StartCoroutine(RollAndDetectDice());
    }

    private IEnumerator RollAndDetectDice()
    {
        // Spawn dice
        dice1 = Instantiate(dicePrefab, leftSpawnPoint.position, Quaternion.identity);
        dice2 = Instantiate(dicePrefab, rightSpawnPoint.position, Quaternion.identity);

        detector1 = dice1.GetComponent<DiceFaceDetector>();
        detector2 = dice2.GetComponent<DiceFaceDetector>();

        Rigidbody rb1 = dice1.GetComponent<Rigidbody>();
        Rigidbody rb2 = dice2.GetComponent<Rigidbody>();

        // Apply random force for rolling effect
        rb1.AddForce(new Vector3(5, 3, 0), ForceMode.Impulse);
        rb1.AddTorque(Random.insideUnitSphere * 15, ForceMode.Impulse);

        rb2.AddForce(new Vector3(-5, 3, 0), ForceMode.Impulse);
        rb2.AddTorque(Random.insideUnitSphere * 15, ForceMode.Impulse);

        // Wait before checking for settled dice
        yield return new WaitForSeconds(7f);
    }

    private void CheckBothDiceSettled(DiceFaceDetector settledDice)
    {
        if (detector1.HasSettled() && detector2.HasSettled())
        {
            int dice1Result = detector1.GetTopFaceValue();
            int dice2Result = detector2.GetTopFaceValue();
            int total = dice1Result + dice2Result;

            resultText.text = "You rolled: " + total;
            Debug.Log($"🎲 Final Dice Results: {dice1Result} + {dice2Result} = {total}");

            StartCoroutine(SendResultToBackend(total));

            // Keep dice longer before destroying
            Destroy(dice1, 10f);
            Destroy(dice2, 10f);
        }
    }

    private IEnumerator SendResultToBackend(int total)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, total.ToString());
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Failed to send dice result: " + request.error);
        }
        else
        {
            Debug.Log("✅ Dice result sent successfully!");
        }
    }
}
