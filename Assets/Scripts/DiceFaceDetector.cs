using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceFaceDetector : MonoBehaviour
{
    private static readonly Dictionary<int, int> OppositeFaces = new Dictionary<int, int>
    {
        { 1, 6 }, { 2, 5 }, { 3, 4 }, { 4, 3 }, { 5, 2 }, { 6, 1 }
    };

    private Rigidbody rb;
    private bool isSettled = false;
    private bool isOnGround = false;
    private float stillTime = 0f;
    private Transform bottomFace;
    private int finalTopFace = -1;

    private const float requiredStillTime = 4f; // Dice must be still for 2 seconds
    private const float minVelocityThreshold = 0.0001f; // Dice must be almost stopped
    private const float minAngularVelocityThreshold = 0.0001f;

    public delegate void DiceLandedHandler(DiceFaceDetector dice);
    public static event DiceLandedHandler OnDiceLanded; // Notify when settled

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isOnGround && !isSettled)
        {
            if (rb.linearVelocity.magnitude < minVelocityThreshold && rb.angularVelocity.magnitude < minAngularVelocityThreshold)
            {
                stillTime += Time.deltaTime;
                if (stillTime >= requiredStillTime)
                {
                    isSettled = true;
                    IdentifyTopFace();
                    OnDiceLanded?.Invoke(this); // Notify that this dice is done
                }
            }
            else
            {
                stillTime = 0f; // Reset timer if dice moves
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

    private void IdentifyTopFace()
    {
        float lowestY = float.MaxValue;

        foreach (Transform child in transform) // Check all child faces
        {
            if (child.position.y < lowestY)
            {
                lowestY = child.position.y;
                bottomFace = child;
            }
        }

        if (bottomFace != null)
        {
            DiceFaceValue faceValueScript = bottomFace.GetComponent<DiceFaceValue>();
            if (faceValueScript != null)
            {
                finalTopFace = OppositeFaces[faceValueScript.faceValue];
                Debug.Log($"🎲 Dice {gameObject.name} landed on top face: {finalTopFace}");
            }
        }
    }

    public int GetTopFaceValue()
    {
        return finalTopFace;
    }

    public bool HasSettled()
    {
        return isSettled;
    }
}
