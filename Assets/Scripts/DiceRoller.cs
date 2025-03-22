using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;  // For UI text

public class DiceRoller : MonoBehaviour
{
    public GameObject dicePrefab; // Assign in Unity
    public Transform leftSpawn, rightSpawn; // Assign in Unity
    public TMP_Text resultText; // Assign UI text field
    private string apiUrl = "http://localhost:8080/api/dice/roll";  // Backend API

    public void RollDice()
    {
        StartCoroutine(GetDiceRollFromServer());
    }

    private IEnumerator GetDiceRollFromServer()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            int diceResult = int.Parse(request.downloadHandler.text);
            Debug.Log("🎲 Dice rolled: " + diceResult);
            StartCoroutine(AnimateDiceRoll(diceResult));
        }
        else
        {
            Debug.LogError("❌ Dice roll failed: " + request.error);
        }
    }

    private IEnumerator AnimateDiceRoll(int diceResult)
    {
        // Spawn dice from both sides
        GameObject dice1 = Instantiate(dicePrefab, leftSpawn.position, Quaternion.identity);
        GameObject dice2 = Instantiate(dicePrefab, rightSpawn.position, Quaternion.identity);

        Rigidbody rb1 = dice1.GetComponent<Rigidbody>();
        Rigidbody rb2 = dice2.GetComponent<Rigidbody>();

        // Apply random force for rolling effect
        rb1.AddForce(new Vector3(5, 2, 0), ForceMode.Impulse);
        rb1.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);

        rb2.AddForce(new Vector3(-5, 2, 0), ForceMode.Impulse);
        rb2.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);

        // Wait for dice to "land"
        yield return new WaitForSeconds(2f);

        // Show the result on screen
        resultText.text = "You rolled: " + diceResult;

        // Destroy dice after some time
        Destroy(dice1, 3f);
        Destroy(dice2, 3f);
    }
}
