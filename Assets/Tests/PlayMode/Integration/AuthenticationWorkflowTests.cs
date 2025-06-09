using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


/// <summary>
/// Comprehensive integration tests demonstrating full authentication workflows.
/// These tests use the TestUtilities to show cleaner, more maintainable test patterns.
/// </summary>
[TestFixture]
public class AuthenticationWorkflowTests
{
    private GameObject testGameObject;
    private UserManager userManager;

    [SetUp]
    public void SetUp()
    {
        (testGameObject, userManager) = TestSetup.SetupCleanTestEnvironment();
    }

    [TearDown]
    public void TearDown()
    {
        TestSetup.CleanupTest(testGameObject);
    }

    /// <summary>
    /// Tests the complete registration -> login workflow.
    /// This is a key integration test that verifies the entire user onboarding process.
    /// </summary>
    [UnityTest]
    public IEnumerator CompleteRegistrationLoginWorkflow_ShouldSucceed()
    {
        // Arrange - Create test user data
        var registerForm = TestDataBuilder.CreateValidRegisterForm("workflow_test");
        var loginForm = TestDataBuilder.CreateLoginFormFromRegister(registerForm);

        var registrationResult = new AsyncOperationResult<bool>();
        var loginResult = new AsyncOperationResult<LoginResponse>();

        // Act & Assert - Registration
        yield return userManager.RegisterAsync(registerForm, registrationResult);

        Assert.IsTrue(registrationResult.IsComplete, "Registration should complete within timeout");
        Assert.IsTrue(registrationResult.IsSuccessful, $"Registration should succeed: {registrationResult.ErrorMessage}");

        // Act & Assert - Login with newly created account
        yield return userManager.LoginAsync(loginForm, loginResult);

        Assert.IsTrue(loginResult.IsComplete, "Login should complete within timeout");
        Assert.IsTrue(loginResult.IsSuccessful, $"Login should succeed: {loginResult.ErrorMessage}");

        // Verify the complete authentication state
        TestAssertions.AssertValidLoginResponse(loginResult.Result, registerForm.username);
        TestAssertions.AssertTokensStoredCorrectly(registerForm.username);

        Debug.Log($"✓ Successfully completed registration and login for user: {registerForm.username}");
    }

    /// <summary>
    /// Tests the guest registration and login workflow.
    /// </summary>
    [UnityTest]
    public IEnumerator GuestWorkflow_ShouldCreateAndLoginGuest()
    {
        // Arrange
        var guestCreationResult = new AsyncOperationResult<GuestRegisterResponse>();
        var guestLoginResult = new AsyncOperationResult<LoginResponse>();

        // Act & Assert - Create guest
        yield return userManager.CreateGuestAsync(guestCreationResult);

        Assert.IsTrue(guestCreationResult.IsComplete, "Guest creation should complete within timeout");
        Assert.IsTrue(guestCreationResult.IsSuccessful, $"Guest creation should succeed: {guestCreationResult.ErrorMessage}");
        Assert.IsNotNull(guestCreationResult.Result, "Guest response should not be null");
        Assert.IsNotEmpty(guestCreationResult.Result.guestKey, "Guest key should not be empty");

        // Act & Assert - Login with guest code
        yield return userManager.GuestLoginAsync(guestCreationResult.Result.guestKey, guestLoginResult);

        Assert.IsTrue(guestLoginResult.IsComplete, "Guest login should complete within timeout");
        Assert.IsTrue(guestLoginResult.IsSuccessful, $"Guest login should succeed: {guestLoginResult.ErrorMessage}");

        // Verify guest authentication state
        TestAssertions.AssertValidLoginResponse(guestLoginResult.Result);

        // Verify guest code is stored
        string storedGuestCode = LocalStorageService.GetString("guest-code");
        Assert.AreEqual(guestCreationResult.Result.guestKey, storedGuestCode, "Guest code should be stored");

        Debug.Log($"✓ Successfully completed guest workflow with key: {guestCreationResult.Result.guestKey}");
    }

    /// <summary>
    /// Tests the token refresh workflow.
    /// </summary>
    [UnityTest]
    public IEnumerator TokenRefreshWorkflow_ShouldProvideNewToken()
    {
        // Arrange - First create and login a user
        var registerForm = TestDataBuilder.CreateValidRegisterForm("refresh_test");
        var loginForm = TestDataBuilder.CreateLoginFormFromRegister(registerForm);

        var registrationResult = new AsyncOperationResult<bool>();
        var loginResult = new AsyncOperationResult<LoginResponse>();
        var refreshResult = new AsyncOperationResult<LoginResponse>();

        // Setup - Register and login to get initial tokens
        yield return userManager.RegisterAsync(registerForm, registrationResult);
        Assert.IsTrue(registrationResult.IsSuccessful, "Setup registration should succeed");

        yield return userManager.LoginAsync(loginForm, loginResult);
        Assert.IsTrue(loginResult.IsSuccessful, "Setup login should succeed");

        string originalToken = loginResult.Result.token;
        string refreshToken = loginResult.Result.refreshToken;

        // Simulate token expiration by clearing the access token
        LocalStorageService.Clear("token");

        // Act & Assert - Refresh token
        yield return userManager.RefreshTokenAsync(refreshToken, refreshResult);

        Assert.IsTrue(refreshResult.IsComplete, "Token refresh should complete within timeout");
        Assert.IsTrue(refreshResult.IsSuccessful, $"Token refresh should succeed: {refreshResult.ErrorMessage}");

        // Verify new token is different and valid
        TestAssertions.AssertValidLoginResponse(refreshResult.Result, registerForm.username);
        Assert.AreNotEqual(originalToken, refreshResult.Result.token, "New token should be different from original");

        // Verify new tokens are stored
        TestAssertions.AssertTokensStoredCorrectly(registerForm.username);

        Debug.Log($"✓ Successfully refreshed token for user: {registerForm.username}");
    }

    /// <summary>
    /// Tests multiple invalid registration scenarios in sequence.
    /// </summary>
    [UnityTest]
    public IEnumerator InvalidRegistrationScenarios_ShouldAllFail()
    {
        var testCases = new[]
        {
            new { form = TestScenarios.InvalidRegistrationData.EmptyUsername, description = "empty username" },
            new { form = TestScenarios.InvalidRegistrationData.EmptyEmail, description = "empty email" },
            new { form = TestScenarios.InvalidRegistrationData.EmptyPassword, description = "empty password" },
            new { form = TestScenarios.InvalidRegistrationData.InvalidEmail, description = "invalid email format" },
            new { form = TestScenarios.InvalidRegistrationData.WeakPassword, description = "weak password" }
        };

        foreach (var testCase in testCases)
        {
            var result = new AsyncOperationResult<bool>();

            // Act
            yield return userManager.RegisterAsync(testCase.form, result);

            // Assert
            Assert.IsTrue(result.IsComplete, $"Registration with {testCase.description} should complete within timeout");
            Assert.IsFalse(result.IsSuccessful, $"Registration with {testCase.description} should fail");
            Assert.IsNotEmpty(result.ErrorMessage, $"Error message should be provided for {testCase.description}");

            // Verify no tokens were stored
            TestAssertions.AssertNoTokensStored();

            Debug.Log($"✓ Correctly rejected registration with {testCase.description}: {result.ErrorMessage}");

            // Small delay between test cases
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Tests multiple invalid login scenarios.
    /// </summary>
    [UnityTest]
    public IEnumerator InvalidLoginScenarios_ShouldAllFail()
    {
        var testCases = new[]
        {
            new { form = TestScenarios.InvalidLoginData.EmptyUsername, description = "empty username" },
            new { form = TestScenarios.InvalidLoginData.EmptyPassword, description = "empty password" },
            new { form = TestScenarios.InvalidLoginData.NonexistentUser, description = "nonexistent user" },
            new { form = TestScenarios.InvalidLoginData.WrongPassword, description = "wrong password" }
        };

        foreach (var testCase in testCases)
        {
            var result = new AsyncOperationResult<LoginResponse>();

            // Act
            yield return userManager.LoginAsync(testCase.form, result);

            // Assert
            Assert.IsTrue(result.IsComplete, $"Login with {testCase.description} should complete within timeout");
            Assert.IsFalse(result.IsSuccessful, $"Login with {testCase.description} should fail");
            Assert.IsNotEmpty(result.ErrorMessage, $"Error message should be provided for {testCase.description}");

            // Verify no tokens were stored
            TestAssertions.AssertNoTokensStored();

            Debug.Log($"✓ Correctly rejected login with {testCase.description}: {result.ErrorMessage}");

            // Small delay between test cases
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Tests concurrent authentication operations to ensure thread safety.
    /// </summary>
    [UnityTest]
    public IEnumerator ConcurrentOperations_ShouldHandleGracefully()
    {
        // Arrange - Create multiple test users
        var user1Form = TestDataBuilder.CreateValidRegisterForm("concurrent_1");
        var user2Form = TestDataBuilder.CreateValidRegisterForm("concurrent_2");

        var result1 = new AsyncOperationResult<bool>();
        var result2 = new AsyncOperationResult<bool>();

        // Act - Start both registrations simultaneously
        var coroutine1 = userManager.RegisterAsync(user1Form, result1);
        var coroutine2 = userManager.RegisterAsync(user2Form, result2);

        // Start both coroutines
        yield return StartCoroutine(coroutine1);
        yield return StartCoroutine(coroutine2);

        // Assert - Both should complete successfully
        Assert.IsTrue(result1.IsComplete, "First registration should complete");
        Assert.IsTrue(result2.IsComplete, "Second registration should complete");
        Assert.IsTrue(result1.IsSuccessful, $"First registration should succeed: {result1.ErrorMessage}");
        Assert.IsTrue(result2.IsSuccessful, $"Second registration should succeed: {result2.ErrorMessage}");

        Debug.Log("✓ Successfully handled concurrent registration operations");
    }

    /// <summary>
    /// Tests the complete logout workflow (if your system supports it).
    /// </summary>
    [UnityTest]
    public IEnumerator LogoutWorkflow_ShouldClearAuthenticationState()
    {
        // Arrange - Setup authenticated user
        var registerForm = TestDataBuilder.CreateValidRegisterForm("logout_test");
        var loginForm = TestDataBuilder.CreateLoginFormFromRegister(registerForm);

        var registrationResult = new AsyncOperationResult<bool>();
        var loginResult = new AsyncOperationResult<LoginResponse>();

        // Setup user
        yield return userManager.RegisterAsync(registerForm, registrationResult);
        yield return userManager.LoginAsync(loginForm, loginResult);

        Assert.IsTrue(registrationResult.IsSuccessful && loginResult.IsSuccessful, "Setup should succeed");

        // Verify user is authenticated
        TestAssertions.AssertTokensStoredCorrectly(registerForm.username);

        // Act - Logout (manual cleanup since your UserManager doesn't have explicit logout)
        LocalStorageService.Clear("token");
        LocalStorageService.Clear("refresh-token");
        LocalStorageService.Clear("username");

        // Assert - Verify authentication state is cleared
        TestAssertions.AssertNoTokensStored();

        Debug.Log($"✓ Successfully logged out user: {registerForm.username}");
    }

    /// <summary>
    /// Helper coroutine to start another coroutine.
    /// </summary>
    private IEnumerator StartCoroutine(IEnumerator coroutine)
    {
        yield return coroutine;
    }
}