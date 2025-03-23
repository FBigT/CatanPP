using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceFaceDetector : MonoBehaviour
{
    private Rigidbody rb;
    private bool isSettled = false;
    private bool isOnGround = false;
    private float stillTime = 0f;
    private int finalTopFace = -1;

    private const float requiredStillTime = 1.5f; // Reduced for quicker response
    private const float minVelocityThreshold = 0.01f;
    private const float minAngularVelocityThreshold = 0.01f;

    public delegate void DiceLandedHandler(DiceFaceDetector dice);
    public static event DiceLandedHandler OnDiceLanded;

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
                    OnDiceLanded?.Invoke(this);
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
        if (collision.gameObject.CompareTag("Ground") && collision.relativeVelocity.magnitude > 0.5f)
        {
            isOnGround = true;
        }
    }

    private void IdentifyTopFace()
    {
        Transform topFace = null;
        float highestDot = -1f;

        Debug.Log($"🔍 Checking top face for {gameObject.name}");

        foreach (Transform child in transform)
        {
            float dotProduct = Vector3.Dot(child.up, Vector3.up);
            Debug.Log($"➡ Face {child.name} has dot product: {dotProduct}");

            if (dotProduct > highestDot)
            {
                highestDot = dotProduct;
                topFace = child;
            }
        }

        if (topFace != null)
        {
            DiceFaceValue faceValueScript = topFace.GetComponent<DiceFaceValue>();
            if (faceValueScript != null)
            {
                finalTopFace = faceValueScript.faceValue;
                Debug.Log($"✅ {gameObject.name} landed on face: {finalTopFace}");
            }
            else
            {
                Debug.LogError("❌ FaceValue script missing on top face!");
            }
        }
        else
        {
            Debug.LogError("❌ No valid top face detected!");
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
