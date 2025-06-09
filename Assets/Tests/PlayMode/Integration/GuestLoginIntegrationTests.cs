using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class GuestLoginIntegrationTests
{
    private UserManager userManager;
    private GameObject testGameObject;

    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject("TestUserManager");
        userManager = testGameObject.AddComponent<UserManager>();

        if (LocalStorageService.Instance != null)
        {
            LocalStorageService.Instance.Clear("guest-code");
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }

        if (LocalStorageService.Instance != null)
        {
            LocalStorageService.Instance.ClearAll();
        }
    }

    [UnityTest]
    public IEnumerator CreateGuestAndLogin_ShouldSucceed()
    {
        // Arrange
        bool guestCreationSuccessful = false;
        bool guestLoginSuccessful = false;
        string guestCreationError = "";
        string guestLoginError = "";
        object guestResponse = null;
        object loginResponse = null;
        string guestKey = "";

        // Act - Create Guest
        userManager.CreateGuest(
            response => {
                guestCreationSuccessful = true;
                guestResponse = response;
                // Extract guest key - adjust based on your response structure
                guestKey = ExtractGuestKey(response);
            },
            error => guestCreationError = error
        );

        // Wait for guest creation
        float timeout = 10f;
        float elapsed = 0f;
        while (!guestCreationSuccessful && string.IsNullOrEmpty(guestCreationError) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert Guest Creation
        Assert.IsTrue(guestCreationSuccessful, $"Guest creation failed: {guestCreationError}");
        Assert.IsNotNull(guestResponse, "Guest response should not be null");
        Assert.IsNotEmpty(guestKey, "Guest key should not be empty");

        // Small delay
        yield return new WaitForSeconds(1f);

        // Act - Guest Login
        userManager.GuestLogin(
            guestKey,
            response => {
                guestLoginSuccessful = true;
                loginResponse = response;
            },
            error => guestLoginError = error
        );

        // Wait for guest login
        elapsed = 0f;
        while (!guestLoginSuccessful && string.IsNullOrEmpty(guestLoginError) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert Guest Login
        Assert.IsTrue(guestLoginSuccessful, $"Guest login failed: {guestLoginError}");
        Assert.IsNotNull(loginResponse, "Login response should not be null");

        Debug.Log($"✓ Successfully created and logged in guest with key: {guestKey}");
    }

    /// <summary>
    /// Helper method to extract guest key from response.
    /// Adjust this based on your actual response structure.
    /// </summary>
    private string ExtractGuestKey(object response)
    {
        // If you have a GuestRegisterResponse class:
        // return ((GuestRegisterResponse)response).guestKey;

        // If response is JSON string, parse it:
        if (response is string jsonResponse)
        {
            // Simple JSON parsing - replace with proper JSON library if available
            // This is a basic example - adjust for your actual JSON structure
            var startIndex = jsonResponse.IndexOf("\"guestKey\":\"") + 12;
            var endIndex = jsonResponse.IndexOf("\"", startIndex);
            if (startIndex > 11 && endIndex > startIndex)
            {
                return jsonResponse.Substring(startIndex, endIndex - startIndex);
            }
        }

        return response?.ToString() ?? "";
    }
}
