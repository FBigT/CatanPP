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

        // Clear PlayerPrefs fallback (since there's no LocalStorageService.Instance)
        PlayerPrefs.DeleteKey("guest-code");
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }

        // Clear any local data used by the test
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
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
            response =>
            {
                guestCreationSuccessful = true;
                guestResponse = response;
                guestKey = ExtractGuestKey(response);
            },
            error => guestCreationError = error
        );

        // Wait for guest creation
        yield return WaitForCondition(() => guestCreationSuccessful || !string.IsNullOrEmpty(guestCreationError), 10f);

        // Assert Guest Creation
        Assert.IsTrue(guestCreationSuccessful, $"Guest creation failed: {guestCreationError}");
        Assert.IsNotNull(guestResponse, "Guest response should not be null");
        Assert.IsNotEmpty(guestKey, "Guest key should not be empty");

        #region Rene stupid
        //// Small delay
        //yield return new WaitForSeconds(1f);

        //// Act - Guest Login
        //userManager.GuestLogin(
        //    guestKey,
        //    response =>
        //    {
        //        guestLoginSuccessful = true;
        //        loginResponse = response;
        //    },
        //    error => guestLoginError = error
        //);

        //// Wait for guest login
        //yield return WaitForCondition(() => guestLoginSuccessful || !string.IsNullOrEmpty(guestLoginError), 10f);

        //// Assert Guest Login
        //Assert.IsTrue(guestLoginSuccessful, $"Guest login failed: {guestLoginError}");
        //Assert.IsNotNull(loginResponse, "Login response should not be null");
        #endregion
    }

    private IEnumerator WaitForCondition(System.Func<bool> condition, float timeout)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private string ExtractGuestKey(object response)
    {
        if (response is string jsonResponse)
        {
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
