using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.User;
using Assets.Scripts.Utils;

/// <summary>
/// Integration tests for the login functionality using real network communication.
/// These tests verify that your Unity frontend can properly communicate with your Java backend.
/// </summary>
[TestFixture]
public class LoginIntegrationTests
{
    private UserManager userManager;
    private GameObject testGameObject;

    [SetUp]
    public void SetUp()
    {
        // Create a test GameObject to attach UserManager component
        testGameObject = new GameObject("TestUserManager");
        userManager = testGameObject.AddComponent<UserManager>();

        // Clean up any previous authentication data
        LocalStorageService.Clear("token");
        LocalStorageService.Clear("refresh-token");
        LocalStorageService.Clear("username");
        LocalStorageService.Clear("guest-code");
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }

        // Clean up any test data
        LocalStorageService.ClearAll();
    }

    #region old
    /*
         /// <summary>
    /// Tests the login process with valid credentials.
    /// </summary>
    [UnityTest]
    public IEnumerator Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange - Use test account credentials
        var loginForm = new LoginForm("testuser", "testpassword");  // Replace with actual test account
        bool loginSuccessful = false;
        string errorMessage = null;
        LoginResponse response = null;

        // Act - Attempt to login
        userManager.Login(
            loginForm,
            (loginResponse) => {
                loginSuccessful = true;
                response = loginResponse;
            },
            (error) => {
                loginSuccessful = false;
                errorMessage = error;
            }
        );

        // Wait for the async operation to complete (adjust timeout as needed)
        float timeout = 5f;
        float elapsed = 0f;
        while (!loginSuccessful && errorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify successful login
        Assert.IsTrue(loginSuccessful, $"Login failed: {errorMessage ?? "Timeout occurred"}");
        Assert.IsNotNull(response, "Response should not be null");
        Assert.IsNotEmpty(response.token, "Token should not be empty");
        Assert.IsNotEmpty(response.refreshToken, "Refresh token should not be empty");

        // Verify token was stored correctly
        string storedToken = LocalStorageService.GetString("token");
        string storedUsername = LocalStorageService.GetString("username");
        Assert.IsNotEmpty(storedToken, "Token should be stored in LocalStorage");
        Assert.AreEqual(loginForm.username, storedUsername, "Username should be stored in LocalStorage");
    }
     */
    #endregion

    /// <summary>
    /// Tests the login process with invalid credentials.
    /// </summary>
    [UnityTest]
    public IEnumerator Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange - Use invalid credentials
        var loginForm = new LoginForm("nonexistentuser", "wrongpassword");
        bool loginSuccessful = false;
        string errorMessage = null;

        // Act - Attempt to login
        userManager.Login(
            loginForm,
            (loginResponse) => {
                loginSuccessful = true;
            },
            (error) => {
                errorMessage = error;
            }
        );

        // Wait for the async operation to complete
        float timeout = 5f;
        float elapsed = 0f;
        while (errorMessage == null && !loginSuccessful && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify login failed as expected
        Assert.IsFalse(loginSuccessful, "Login should fail with invalid credentials");
        Assert.IsNotNull(errorMessage, "An error message should be provided");

        // Verify no token was stored
        string storedToken = LocalStorageService.GetString("token");
        Assert.IsNull(storedToken, "No token should be stored after failed login");
    }

    /// <summary>
    /// Tests the guest login process.
    /// </summary>
    [UnityTest]
    public IEnumerator GuestLogin_ShouldGenerateGuestCodeAndLogin()
    {
        // Arrange
        bool guestCreationSuccessful = false;
        bool guestLoginSuccessful = false;
        string guestCreationError = null;
        string guestLoginError = null;
        GuestRegisterResponse guestResponse = null;
        LoginResponse loginResponse = null;

        // Act - Create guest account
        userManager.CreateGuest(
            (response) => {
                guestCreationSuccessful = true;
                guestResponse = response;
            },
            (error) => {
                guestCreationError = error;
            }
        );

        // Wait for guest creation
        float timeout = 5f;
        float elapsed = 0f;
        while (!guestCreationSuccessful && guestCreationError == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert guest creation was successful
        Assert.IsTrue(guestCreationSuccessful, $"Guest creation failed: {guestCreationError ?? "Timeout occurred"}");
        Assert.IsNotNull(guestResponse, "Guest response should not be null");
        Assert.IsNotEmpty(guestResponse.guestKey, "Guest key should not be empty");

        // Act - Login with guest code
        userManager.GuestLogin(
            guestResponse.guestKey,
            (response) => {
                guestLoginSuccessful = true;
                loginResponse = response;
            },
            (error) => {
                guestLoginError = error;
            }
        );

        // Reset timeout for guest login
        elapsed = 0f;
        while (!guestLoginSuccessful && guestLoginError == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert guest login was successful
        Assert.IsTrue(guestLoginSuccessful, $"Guest login failed: {guestLoginError ?? "Timeout occurred"}");
        Assert.IsNotNull(loginResponse, "Login response should not be null");
        Assert.IsNotEmpty(loginResponse.token, "Token should not be empty");
    }

    #region old
    /// <summary>
    /// Tests token refresh functionality.
    /// </summary>
    /*
    [UnityTest]
    public IEnumerator RefreshToken_WithValidRefreshToken_ShouldProvideNewAccessToken()
    {
        // Arrange - First login to get a refresh token
        var loginForm = new LoginForm("testuser", "testpassword");  // Replace with actual test account
        bool loginSuccessful = false;
        LoginResponse initialResponse = null;

        userManager.Login(
            loginForm,
            (response) => {
                loginSuccessful = true;
                initialResponse = response;
            },
            (error) => { loginSuccessful = false; }
        );

        // Wait for login
        float timeout = 5f;
        float elapsed = 0f;
        while (!loginSuccessful && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Verify initial login succeeded
        Assert.IsTrue(loginSuccessful, "Initial login should succeed");
        Assert.IsNotNull(initialResponse, "Initial response should not be null");

        // Store the refresh token
        string refreshToken = initialResponse.refreshToken;

        // Clear token to simulate expiration
        LocalStorageService.Clear("token");

        // Act - Attempt to refresh the token
        bool refreshSuccessful = false;
        LoginResponse refreshResponse = null;

        userManager.RefreshToken(
            refreshToken,
            (response) => {
                refreshSuccessful = true;
                refreshResponse = response;
            },
            (error) => { refreshSuccessful = false; }
        );

        // Wait for refresh
        elapsed = 0f;
        while (!refreshSuccessful && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify refresh succeeded
        Assert.IsTrue(refreshSuccessful, "Token refresh should succeed");
        Assert.IsNotNull(refreshResponse, "Refresh response should not be null");
        Assert.IsNotEmpty(refreshResponse.token, "New token should not be empty");
        Assert.AreNotEqual(initialResponse.token, refreshResponse.token, "New token should be different from the old token");

        // Verify new token was stored
        string storedToken = LocalStorageService.GetString("token");
        Assert.IsNotEmpty(storedToken, "New token should be stored in LocalStorage");
    }
    */
    #endregion
}