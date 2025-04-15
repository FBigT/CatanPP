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

    private string apiUrl = "http://localhost:8080/api/dice/roll";

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
        dice1 = Instantiate(dicePrefab, leftSpawnPoint.position, Quaternion.identity);
        dice2 = Instantiate(dicePrefab, rightSpawnPoint.position, Quaternion.identity);

        detector1 = dice1.GetComponent<DiceFaceDetector>();
        detector2 = dice2.GetComponent<DiceFaceDetector>();

        Rigidbody rb1 = dice1.GetComponent<Rigidbody>();
        Rigidbody rb2 = dice2.GetComponent<Rigidbody>();

        rb1.AddForce(new Vector3(Random.Range(6, 12), Random.Range(5, 8), Random.Range(-4, 4)), ForceMode.Impulse);
        rb1.AddTorque(Random.insideUnitSphere * Random.Range(20, 40), ForceMode.Impulse);

        rb2.AddForce(new Vector3(Random.Range(-6, -12), Random.Range(5, 8), Random.Range(-4, 4)), ForceMode.Impulse);
        rb2.AddTorque(Random.insideUnitSphere * Random.Range(20, 40), ForceMode.Impulse);

        yield return new WaitForSeconds(3f);
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

            // Keep your old "POST total to the server" logic:
            StartCoroutine(SendResultToBackend(total));

            // Destroy dice after some delay
            Destroy(dice1, 5f);
            Destroy(dice2, 5f);
        }
    }

    private IEnumerator SendResultToBackend(int total)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, total.ToString());
        request.timeout = 5;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Failed to send dice result: " + request.error);
        }
        else
        {
            Debug.Log("✅ Dice result sent successfully!");

            // OPTIONAL: If the server updates resources, you might want to refresh your TopBar:
            TopBarUI topBar = FindObjectOfType<TopBarUI>();
            if (topBar != null)
            {
                StartCoroutine(topBar.FetchAndUpdateResources());
            }
        }
    }
}
