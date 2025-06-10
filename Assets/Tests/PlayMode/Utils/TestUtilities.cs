using System;
using System.Collections;
using UnityEngine;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using NUnit.Framework;

/// <summary>
/// Utility class for creating test data and common test operations.
/// This helps maintain consistency across test files and reduces code duplication.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a unique test username with timestamp to avoid collisions.
    /// </summary>
    public static string CreateUniqueUsername(string prefix = "testuser")
    {
        return $"{prefix}_{DateTime.Now.Ticks}";
    }

    /// <summary>
    /// Creates a unique test email with timestamp.
    /// </summary>
    public static string CreateUniqueEmail(string prefix = "test")
    {
        return $"{prefix}_{DateTime.Now.Ticks}@example.com";
    }

    /// <summary>
    /// Creates a test password that meets common password requirements.
    /// </summary>
    public static string CreateTestPassword()
    {
        return "TestPassword123!";
    }

    /// <summary>
    /// Creates a RegisterForm with unique, valid test data.
    /// </summary>
    public static RegisterForm CreateValidRegisterForm(string usernamePrefix = "testuser")
    {
        return new RegisterForm(
            CreateUniqueUsername(usernamePrefix),
            CreateUniqueEmail(),
            CreateTestPassword()
        );
    }

    /// <summary>
    /// Creates a LoginForm with the given credentials.
    /// </summary>
    public static LoginForm CreateLoginForm(string username, string password)
    {
        return new LoginForm(username, password);
    }

    /// <summary>
    /// Creates a LoginForm from a RegisterForm for testing registration -> login flow.
    /// </summary>
    public static LoginForm CreateLoginFormFromRegister(RegisterForm registerForm)
    {
        return new LoginForm(registerForm.username, registerForm.password);
    }
}

/// <summary>
/// Helper class for common test assertions and operations.
/// </summary>
public static class TestAssertions
{
    /// <summary>
    /// Asserts that a LoginResponse contains valid data.
    /// </summary>
    public static void AssertValidLoginResponse(LoginResponse response, string expectedUsername = null)
    {
        Assert.IsNotNull(response, "Login response should not be null");
        Assert.IsNotEmpty(response.token, "Token should not be empty");
        Assert.IsNotEmpty(response.refreshToken, "Refresh token should not be empty");
        Assert.IsNotEmpty(response.tokenType, "Token type should not be empty");

        if (!string.IsNullOrEmpty(expectedUsername))
        {
            Assert.AreEqual(expectedUsername, response.username, "Username should match expected value");
        }
    }

    /// <summary>
    /// Asserts that authentication tokens are properly stored in LocalStorage.
    /// </summary>
    public static void AssertTokensStoredCorrectly(string expectedUsername = null)
    {
        string storedToken = LocalStorageService.GetString("token");
        string storedRefreshToken = LocalStorageService.GetString("refresh-token");

        Assert.IsNotEmpty(storedToken, "Token should be stored in LocalStorage");
        Assert.IsNotEmpty(storedRefreshToken, "Refresh token should be stored in LocalStorage");

        if (!string.IsNullOrEmpty(expectedUsername))
        {
            string storedUsername = LocalStorageService.GetString("username");
            Assert.AreEqual(expectedUsername, storedUsername, "Username should be stored in LocalStorage");
        }
    }

    /// <summary>
    /// Asserts that no authentication tokens are stored (used after failed login attempts).
    /// </summary>
    public static void AssertNoTokensStored()
    {
        string storedToken = LocalStorageService.GetString("token");
        string storedRefreshToken = LocalStorageService.GetString("refresh-token");
        string storedUsername = LocalStorageService.GetString("username");

        Assert.IsTrue(string.IsNullOrEmpty(storedToken), "No token should be stored after failed operation");
        Assert.IsTrue(string.IsNullOrEmpty(storedRefreshToken), "No refresh token should be stored after failed operation");
        Assert.IsTrue(string.IsNullOrEmpty(storedUsername), "No username should be stored after failed operation");
    }
}

/// <summary>
/// Helper class for managing test timeouts and async operations.
/// </summary>
public static class TestTimeouts
{
    public const float DefaultTimeout = 5f;
    public const float ExtendedTimeout = 10f;
    public const float NetworkTimeout = 15f;

    /// <summary>
    /// Waits for a condition to be true or times out.
    /// Returns true if condition was met, false if timeout occurred.
    /// </summary>
    public static IEnumerator WaitForCondition(Func<bool> condition, float timeout = DefaultTimeout)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Waits for an async operation to complete (success or failure) or times out.
    /// </summary>
    public static IEnumerator WaitForAsyncOperation(Func<bool> isComplete, float timeout = DefaultTimeout)
    {
        return WaitForCondition(isComplete, timeout);
    }
}

/// <summary>
/// Test setup and cleanup utilities.
/// </summary>
public static class TestSetup
{
    /// <summary>
    /// Creates a test GameObject with UserManager component attached.
    /// </summary>
    public static (GameObject gameObject, UserManager userManager) CreateTestUserManager()
    {
        var gameObject = new GameObject("TestUserManager");
        var userManager = gameObject.AddComponent<UserManager>();
        return (gameObject, userManager);
    }

    /// <summary>
    /// Cleans up authentication data and test objects.
    /// </summary>
    public static void CleanupTest(GameObject testGameObject)
    {
        if (testGameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(testGameObject);
        }

        // Clear all stored authentication data
        LocalStorageService.ClearAll();
    }

    /// <summary>
    /// Sets up a clean test environment.
    /// </summary>
    public static (GameObject gameObject, UserManager userManager) SetupCleanTestEnvironment()
    {
        // Clear any existing data
        LocalStorageService.ClearAll();

        // Create fresh UserManager
        return CreateTestUserManager();
    }
}

/// <summary>
/// Helper for creating test scenarios and data.
/// </summary>
public static class TestScenarios
{
    /// <summary>
    /// Creates invalid test data scenarios for testing validation.
    /// </summary>
    public static class InvalidRegistrationData
    {
        public static RegisterForm EmptyUsername => new RegisterForm("", TestDataBuilder.CreateUniqueEmail(), TestDataBuilder.CreateTestPassword());
        public static RegisterForm EmptyEmail => new RegisterForm(TestDataBuilder.CreateUniqueUsername(), "", TestDataBuilder.CreateTestPassword());
        public static RegisterForm EmptyPassword => new RegisterForm(TestDataBuilder.CreateUniqueUsername(), TestDataBuilder.CreateUniqueEmail(), "");
        public static RegisterForm InvalidEmail => new RegisterForm(TestDataBuilder.CreateUniqueUsername(), "not-an-email", TestDataBuilder.CreateTestPassword());
        public static RegisterForm WeakPassword => new RegisterForm(TestDataBuilder.CreateUniqueUsername(), TestDataBuilder.CreateUniqueEmail(), "123");
        public static RegisterForm TooLongUsername => new RegisterForm(new string('a', 100), TestDataBuilder.CreateUniqueEmail(), TestDataBuilder.CreateTestPassword());
    }

    /// <summary>
    /// Creates invalid login data scenarios.
    /// </summary>
    public static class InvalidLoginData
    {
        public static LoginForm EmptyUsername => new LoginForm("", TestDataBuilder.CreateTestPassword());
        public static LoginForm EmptyPassword => new LoginForm(TestDataBuilder.CreateUniqueUsername(), "");
        public static LoginForm NonexistentUser => new LoginForm("nonexistentuser_" + DateTime.Now.Ticks, TestDataBuilder.CreateTestPassword());
        public static LoginForm WrongPassword => new LoginForm("testuser", "wrongpassword");
    }
}

/// <summary>
/// Wrapper for async operation results to make testing easier.
/// </summary>
public class AsyncOperationResult<T>
{
    public bool IsComplete { get; private set; }
    public bool IsSuccessful { get; private set; }
    public T Result { get; private set; }
    public string ErrorMessage { get; private set; }

    public void SetSuccess(T result)
    {
        IsComplete = true;
        IsSuccessful = true;
        Result = result;
        ErrorMessage = null;
    }

    public void SetFailure(string error)
    {
        IsComplete = true;
        IsSuccessful = false;
        Result = default(T);
        ErrorMessage = error;
    }

    public void Reset()
    {
        IsComplete = false;
        IsSuccessful = false;
        Result = default(T);
        ErrorMessage = null;
    }
}

/// <summary>
/// Extension methods to make testing UserManager operations easier.
/// </summary>
public static class UserManagerTestExtensions
{
    /// <summary>
    /// Performs a login operation and returns the result wrapped in an AsyncOperationResult.
    /// </summary>
    public static IEnumerator LoginAsync(this UserManager userManager, LoginForm loginForm, AsyncOperationResult<LoginResponse> result)
    {
        result.Reset();

        userManager.Login(
            loginForm,
            response => result.SetSuccess(response),
            error => result.SetFailure(error)
        );

        yield return TestTimeouts.WaitForCondition(() => result.IsComplete, TestTimeouts.DefaultTimeout);
    }

    /// <summary>
    /// Performs a registration operation and returns the result wrapped in an AsyncOperationResult.
    /// </summary>
    public static IEnumerator RegisterAsync(this UserManager userManager, RegisterForm registerForm, AsyncOperationResult<bool> result)
    {
        result.Reset();

        userManager.CreateUser(
            registerForm,
            () => result.SetSuccess(true),
            error => result.SetFailure(error)
        );

        yield return TestTimeouts.WaitForCondition(() => result.IsComplete, TestTimeouts.ExtendedTimeout);
    }

    /// <summary>
    /// Performs a guest creation operation and returns the result wrapped in an AsyncOperationResult.
    /// </summary>
    public static IEnumerator CreateGuestAsync(this UserManager userManager, AsyncOperationResult<GuestRegisterResponse> result)
    {
        result.Reset();

        userManager.CreateGuest(
            response => result.SetSuccess(response),
            error => result.SetFailure(error)
        );

        yield return TestTimeouts.WaitForCondition(() => result.IsComplete, TestTimeouts.DefaultTimeout);
    }

    /// <summary>
    /// Performs a guest login operation and returns the result wrapped in an AsyncOperationResult.
    /// </summary>
    public static IEnumerator GuestLoginAsync(this UserManager userManager, string guestCode, AsyncOperationResult<LoginResponse> result)
    {
        result.Reset();

        userManager.GuestLogin(
            guestCode,
            response => result.SetSuccess(response),
            error => result.SetFailure(error)
        );

        yield return TestTimeouts.WaitForCondition(() => result.IsComplete, TestTimeouts.DefaultTimeout);
    }

    /// <summary>
    /// Performs a token refresh operation and returns the result wrapped in an AsyncOperationResult.
    /// </summary>
    public static IEnumerator RefreshTokenAsync(this UserManager userManager, string refreshToken, AsyncOperationResult<LoginResponse> result)
    {
        result.Reset();

        userManager.RefreshToken(
            refreshToken,
            response => result.SetSuccess(response),
            error => result.SetFailure(error)
        );

        yield return TestTimeouts.WaitForCondition(() => result.IsComplete, TestTimeouts.DefaultTimeout);
    }
}