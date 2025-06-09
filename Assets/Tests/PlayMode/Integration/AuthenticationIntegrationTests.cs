using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
// REMOVED: using Assets.Scripts.User;
// REMOVED: using Assets.Scripts.Utils;

[TestFixture]
public class AuthenticationIntegrationTests
{
    private UserManager userManager;
    private GameObject testGameObject;

    [SetUp]
    public void SetUp()
    {
        // Create a test GameObject to host UserManager
        testGameObject = new GameObject("TestUserManager");
        userManager = testGameObject.AddComponent<UserManager>();

        // Clear any existing tokens using safe fallback approach
        ClearStorageData();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }

        // Clean up storage
        ClearStorageData();
    }

    // Helper method with fallback storage clearing
    private void ClearStorageData()
    {
        try
        {
            // Try LocalStorageService if it exists
            var storageServiceType = System.Type.GetType("LocalStorageService");
            if (storageServiceType != null)
            {
                var instanceProperty = storageServiceType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var clearMethod = storageServiceType.GetMethod("Clear");
                        var clearAllMethod = storageServiceType.GetMethod("ClearAll");

                        clearMethod?.Invoke(instance, new object[] { "token" });
                        clearMethod?.Invoke(instance, new object[] { "refresh-token" });
                        clearMethod?.Invoke(instance, new object[] { "username" });
                        clearMethod?.Invoke(instance, new object[] { "guest-code" });

                        clearAllMethod?.Invoke(instance, null);
                    }
                }
            }
        }
        catch
        {
            // Fallback to PlayerPrefs if LocalStorageService not available
            PlayerPrefs.DeleteKey("token");
            PlayerPrefs.DeleteKey("refresh-token");
            PlayerPrefs.DeleteKey("username");
            PlayerPrefs.DeleteKey("guest-code");
            PlayerPrefs.DeleteAll();
        }
    }

    [UnityTest]
    public IEnumerator RegisterAndLogin_ValidCredentials_ShouldSucceed()
    {
        // Arrange
        string testUsername = SimpleTestUtils.CreateUniqueUsername();
        string testEmail = SimpleTestUtils.CreateUniqueEmail();
        string testPassword = SimpleTestUtils.GetTestPassword();

        bool registrationSuccessful = false;
        bool loginSuccessful = false;
        string registrationError = "";
        string loginError = "";
        object loginResponse = null; // Use object to avoid type dependency

        // Act - Registration
        // Adjust method signature based on your actual UserManager implementation
        userManager.CreateUser(
            testUsername, testEmail, testPassword,
            () => registrationSuccessful = true,
            error => registrationError = error
        );

        // Wait for registration with timeout
        yield return WaitForCondition(() => registrationSuccessful || !string.IsNullOrEmpty(registrationError), 10f);

        // Assert Registration
        Assert.IsTrue(registrationSuccessful, $"Registration failed: {registrationError}");

        // Small delay for backend processing
        yield return new WaitForSeconds(1f);

        // Act - Login
        userManager.Login(
            testUsername, testPassword,
            response => {
                loginSuccessful = true;
                loginResponse = response;
            },
            error => loginError = error
        );

        // Wait for login with timeout
        yield return WaitForCondition(() => loginSuccessful || !string.IsNullOrEmpty(loginError), 10f);

        // Assert Login
        Assert.IsTrue(loginSuccessful, $"Login failed: {loginError}");
        Assert.IsNotNull(loginResponse, "Login response should not be null");

        SimpleTestUtils.LogTestSuccess($"Registered and logged in user: {testUsername}");
    }

    [UnityTest]
    public IEnumerator Login_InvalidCredentials_ShouldFail()
    {
        // Arrange
        string invalidUsername = "nonexistentuser";
        string invalidPassword = "wrongpassword";
        bool loginSuccessful = false;
        string loginError = "";

        // Act
        userManager.Login(
            invalidUsername, invalidPassword,
            response => loginSuccessful = true,
            error => loginError = error
        );

        // Wait for response with timeout
        yield return WaitForCondition(() => loginSuccessful || !string.IsNullOrEmpty(loginError), 10f);

        // Assert
        Assert.IsFalse(loginSuccessful, "Login should fail with invalid credentials");
        Assert.IsNotEmpty(loginError, "Error message should be provided for failed login");

        SimpleTestUtils.LogTestSuccess($"Correctly rejected invalid credentials: {loginError}");
    }

    [UnityTest]
    public IEnumerator Register_DuplicateUsername_ShouldFail()
    {
        // Arrange
        string duplicateUsername = "duplicateuser_" + System.DateTime.Now.Ticks;
        string password = "password123";

        bool firstRegistrationSuccessful = false;
        bool secondRegistrationSuccessful = false;
        string firstRegistrationError = "";
        string secondRegistrationError = "";

        // Act - First registration
        userManager.CreateUser(
            duplicateUsername, $"{duplicateUsername}_first@test.com", password,
            () => firstRegistrationSuccessful = true,
            error => firstRegistrationError = error
        );

        // Wait for first registration
        yield return WaitForCondition(() => firstRegistrationSuccessful || !string.IsNullOrEmpty(firstRegistrationError), 10f);

        Assert.IsTrue(firstRegistrationSuccessful, $"First registration should succeed: {firstRegistrationError}");

        // Small delay
        yield return new WaitForSeconds(1f);

        // Act - Second registration with same username
        userManager.CreateUser(
            duplicateUsername, $"{duplicateUsername}_second@test.com", password,
            () => secondRegistrationSuccessful = true,
            error => secondRegistrationError = error
        );

        // Wait for second registration
        yield return WaitForCondition(() => secondRegistrationSuccessful || !string.IsNullOrEmpty(secondRegistrationError), 10f);

        // Assert
        Assert.IsFalse(secondRegistrationSuccessful, "Second registration with duplicate username should fail");
        Assert.IsNotEmpty(secondRegistrationError, "Error message should be provided for duplicate username");

        SimpleTestUtils.LogTestSuccess($"Correctly rejected duplicate username: {secondRegistrationError}");
    }

    // Helper method for waiting with timeout
    private IEnumerator WaitForCondition(System.Func<bool> condition, float timeout)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}