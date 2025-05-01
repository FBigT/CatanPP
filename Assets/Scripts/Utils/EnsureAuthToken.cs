using UnityEngine;
using Assets.Scripts.Utils;   // LocalStorageService, SecurityUtils

/// <summary>
/// Injects a syntactically correct dummy JWT into PlayerPrefs *before* the
/// first web-request is built.  Does nothing once a real login replaces it.
/// </summary>
[DefaultExecutionOrder(-32000)]          // runs before anything else
public class EnsureAuthToken : MonoBehaviour
{
    /* header = {"alg":"none","typ":"JWT"}
       payload = {"exp":4102444800}  // 2100-01-01
       both base64url-encoded, plus Bearer prefix                          */
    const string DummyJwt =
        "Bearer eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";

    void Awake()
    {
        string stored = LocalStorageService.GetString("token");

        if (string.IsNullOrEmpty(stored) ||                // none yet
            !SecurityUtils.IsTokenValid(stored))           // or malformed/expired
        {
            Debug.Log("[EnsureAuthToken] inserting placeholder JWT");
            LocalStorageService.SetVariable("token", DummyJwt);
        }
        else
        {
            Debug.Log("[EnsureAuthToken] existing JWT is valid – nothing to do");
        }
    }
}
