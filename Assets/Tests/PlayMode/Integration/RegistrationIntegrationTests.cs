using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.User;
using Assets.Scripts.Utils;

/// <summary>
/// Integration tests for the registration functionality using real network communication.
/// These tests verify that new users can be created through your Unity frontend.
/// </summary>
[TestFixture]
public class RegistrationIntegrationTests
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
        LocalStorageService.ClearAll();
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

    /// <summary>
    /// Tests user registration with valid data.
    /// </summary>
    [UnityTest]
    public IEnumerator Register_WithValidData_ShouldSucceed()
    {
        // Arrange - Create unique test user data
        string timestamp = System.DateTime.Now.Ticks.ToString();
        string testUsername = $"testuser_{timestamp}";
        string testEmail = $"test_{timestamp}@example.com";
        string testPassword = "TestPassword123!";

        var registerForm = new RegisterForm(testUsername, testEmail, testPassword);

        bool registrationSuccessful = false;
        string errorMessage = null;

        // Act - Attempt to register
        userManager.CreateUser(
            registerForm,
            () => {
                registrationSuccessful = true;
            },
            (error) => {
                errorMessage = error;
            }
        );

        // Wait for the async operation to complete
        float timeout = 10f; // Registration might take longer than login
        float elapsed = 0f;
        while (!registrationSuccessful && errorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify successful registration
        Assert.IsTrue(registrationSuccessful, $"Registration failed: {errorMessage ?? "Timeout occurred"}");
        Assert.IsNull(errorMessage, "No error should occur during successful registration");
    }

    /// <summary>
    /// Tests registration with duplicate username.
    /// </summary>
    [UnityTest]
    public IEnumerator Register_WithDuplicateUsername_ShouldFail()
    {
        // Arrange - First registration with unique data
        string timestamp = System.DateTime.Now.Ticks.ToString();
        string duplicateUsername = $"duplicate_{timestamp}";
        string firstEmail = $"first_{timestamp}@example.com";
        string secondEmail = $"second_{timestamp}@example.com";
        string password = "TestPassword123!";

        var firstRegisterForm = new RegisterForm(duplicateUsername, firstEmail, password);
        var secondRegisterForm = new RegisterForm(duplicateUsername, secondEmail, password);

        bool firstRegistrationSuccessful = false;
        bool secondRegistrationSuccessful = false;
        string firstErrorMessage = null;
        string secondErrorMessage = null;

        // Act - First registration
        userManager.CreateUser(
            firstRegisterForm,
            () => { firstRegistrationSuccessful = true; },
            (error) => { firstErrorMessage = error; }
        );

        // Wait for first registration
        float timeout = 10f;
        float elapsed = 0f;
        while (!firstRegistrationSuccessful && firstErrorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Verify first registration succeeded
        Assert.IsTrue(firstRegistrationSuccessful, $"First registration should succeed: {firstErrorMessage ?? "Timeout occurred"}");

        // Act - Second registration with same username
        userManager.CreateUser(
            secondRegisterForm,
            () => { secondRegistrationSuccessful = true; },
            (error) => { secondErrorMessage = error; }
        );

        // Wait for second registration
        elapsed = 0f;
        while (!secondRegistrationSuccessful && secondErrorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify second registration failed
        Assert.IsFalse(secondRegistrationSuccessful, "Second registration with duplicate username should fail");
        Assert.IsNotNull(secondErrorMessage, "Error message should be provided for duplicate username");
    }

    /// <summary>
    /// Tests registration with invalid email format.
    /// </summary>
    [UnityTest]
    public IEnumerator Register_WithInvalidEmail_ShouldFail()
    {
        // Arrange - Create registration data with invalid email
        string timestamp = System.DateTime.Now.Ticks.ToString();
        string testUsername = $"testuser_{timestamp}";
        string invalidEmail = "not-a-valid-email"; // Invalid email format
        string testPassword = "TestPassword123!";

        var registerForm = new RegisterForm(testUsername, invalidEmail, testPassword);

        bool registrationSuccessful = false;
        string errorMessage = null;

        // Act - Attempt to register
        userManager.CreateUser(
            registerForm,
            () => { registrationSuccessful = true; },
            (error) => { errorMessage = error; }
        );

        // Wait for the async operation to complete
        float timeout = 10f;
        float elapsed = 0f;
        while (!registrationSuccessful && errorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - Verify registration failed
        Assert.IsFalse(registrationSuccessful, "Registration with invalid email should fail");
        Assert.IsNotNull(errorMessage, "Error message should be provided for invalid email");
    }

    /// <summary>
    /// Tests registration followed by immediate login to ensure the account is fully created.
    /// </summary>
    [UnityTest]
    public IEnumerator RegisterThenLogin_ShouldCreateFunctionalAccount()
    {
        // Arrange - Create unique test user data
        string timestamp = System.DateTime.Now.Ticks.ToString();
        string testUsername = $"testuser_{timestamp}";
        string testEmail = $"test_{timestamp}@example.com";
        string testPassword = "TestPassword123!";

        var registerForm = new RegisterForm(testUsername, testEmail, testPassword);
        var loginForm = new LoginForm(testUsername, testPassword);

        bool registrationSuccessful = false;
        bool loginSuccessful = false;
        string registrationError = null;
        string loginError = null;
        LoginResponse loginResponse = null;

        // Act - Register new user
        userManager.CreateUser(
            registerForm,
            () => { registrationSuccessful = true; },
            (error) => { registrationError = error; }
        );

        // Wait for registration
        float timeout = 10f;
        float elapsed = 0f;
        while (!registrationSuccessful && registrationError == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert registration succeeded
        Assert.IsTrue(registrationSuccessful, $"Registration failed: {registrationError ?? "Timeout occurred"}");

        // Small delay to ensure account is fully created in the backend
        yield return new WaitForSeconds(1f);

        // Act - Login with newly created account
        userManager.Login(
            loginForm,
            (response) => {
                loginSuccessful = true;
                loginResponse = response;
            },
            (error) => { loginError = error; }
        );

        // Wait for login
        elapsed = 0f;
        while (!loginSuccessful && loginError == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert login succeeded
        Assert.IsTrue(loginSuccessful, $"Login with newly created account failed: {loginError ?? "Timeout occurred"}");
        Assert.IsNotNull(loginResponse, "Login response should not be null");
        Assert.IsNotEmpty(loginResponse.token, "Token should not be empty");
        Assert.AreEqual(testUsername, loginResponse.username, "Username in response should match registered username");

        // Verify authentication data was stored correctly
        string storedToken = LocalStorageService.GetString("token");
        string storedUsername = LocalStorageService.GetString("username");
        Assert.IsNotEmpty(storedToken, "Token should be stored in LocalStorage");
        Assert.AreEqual(testUsername, storedUsername, "Username should be stored in LocalStorage");
    }

    /// <summary>
    /// Tests registration with weak password (if your backend validates password strength).
    /// </summary>
    [UnityTest]
    public IEnumerator Register_WithWeakPassword_ShouldFailOrWarn()
    {
        // Arrange - Create registration data with weak password
        string timestamp = System.DateTime.Now.Ticks.ToString();
        string testUsername = $"testuser_{timestamp}";
        string testEmail = $"test_{timestamp}@example.com";
        string weakPassword = "123"; // Very weak password

        var registerForm = new RegisterForm(testUsername, testEmail, weakPassword);

        bool registrationSuccessful = false;
        string errorMessage = null;

        // Act - Attempt to register
        userManager.CreateUser(
            registerForm,
            () => { registrationSuccessful = true; },
            (error) => { errorMessage = error; }
        );

        // Wait for the async operation to complete
        float timeout = 10f;
        float elapsed = 0f;
        while (!registrationSuccessful && errorMessage == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Assert - This test depends on your backend password policy
        // If your backend enforces password strength, registration should fail
        // If not, this test documents that weak passwords are currently allowed
        if (!registrationSuccessful)
        {
            Assert.IsNotNull(errorMessage, "Error message should be provided for weak password");
            Debug.Log($"Backend correctly rejected weak password: {errorMessage}");
        }
        else
        {
            Debug.LogWarning("Backend allowed weak password - consider implementing password strength validation");
        }
    }

    /// <summary>
    /// Tests registration with empty or null fields.
    /// </summary>
    [UnityTest]
    public IEnumerator Register_WithEmptyFields_ShouldFail()
    {
        // Test cases for empty fields
        var testCases = new[]
        {
            new { username = "", email = "test@example.com", password = "TestPassword123!", description = "empty username" },
            new { username = "testuser", email = "", password = "TestPassword123!", description = "empty email" },
            new { username = "testuser", email = "test@example.com", password = "", description = "empty password" },
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var registerForm = new RegisterForm(testCase.username, testCase.email, testCase.password);
            bool registrationSuccessful = false;
            string errorMessage = null;

            // Act
            userManager.CreateUser(
                registerForm,
                () => { registrationSuccessful = true; },
                (error) => { errorMessage = error; }
            );

            // Wait for response
            float timeout = 10f;
            float elapsed = 0f;
            while (!registrationSuccessful && errorMessage == null && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Assert
            Assert.IsFalse(registrationSuccessful, $"Registration with {testCase.description} should fail");
            Assert.IsNotNull(errorMessage, $"Error message should be provided for {testCase.description}");

            Debug.Log($"✓ Correctly rejected registration with {testCase.description}: {errorMessage}");

            // Small delay between test cases
            yield return new WaitForSeconds(0.5f);
        }
    }
}