using Assets.Scripts.Dtos;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

/// <summary>
/// Mock implementation of RequestService for unit testing.
/// This allows testing UserManager without making actual network requests.
/// </summary>
public class MockRequestService
{
    // Flags to control mock responses
    public static bool SimulateNetworkFailure { get; set; } = false;
    public static bool SimulateServerError { get; set; } = false;
    public static bool SimulateTimeout { get; set; } = false;
    public static int ResponseDelayMilliseconds { get; set; } = 100; // Simulated network delay

    // Store predefined mock responses
    private static readonly Dictionary<string, MockResponse> MockResponses = new Dictionary<string, MockResponse>();

    // Keeps track of requests for verification
    public static List<MockRequest> ReceivedRequests { get; private set; } = new List<MockRequest>();

    /// <summary>
    /// Setup the mock service to intercept UnityWebRequests.
    /// </summary>
    public static void Setup()
    {
        // Reset state
        SimulateNetworkFailure = false;
        SimulateServerError = false;
        SimulateTimeout = false;
        ResponseDelayMilliseconds = 100;
        MockResponses.Clear();
        ReceivedRequests = new List<MockRequest>();

        // Configure default mock responses
        SetupDefaultResponses();

        // Override the RequestService
        RequestService.ConstructSimpleWebRequestOverride = HandleRequest;
    }

    /// <summary>
    /// Clean up and restore original behavior.
    /// </summary>
    public static void Cleanup()
    {
        RequestService.ClearTestOverride();
    }

    /// <summary>
    /// Adds or updates a mock response for a specific endpoint.
    /// </summary>
    public static void AddMockResponse(string endpointPattern, int statusCode, string responseContent, bool isNetworkError = false)
    {
        MockResponses[endpointPattern] = new MockResponse
        {
            StatusCode = statusCode,
            Content = responseContent,
            IsNetworkError = isNetworkError
        };
    }

    /// <summary>
    /// Sets up default mock responses for common endpoints.
    /// </summary>
    private static void SetupDefaultResponses()
    {
        // Registration - Success
        AddMockResponse("register", 201, "");

        // Registration - Username taken
        AddMockResponse("register/duplicate", 400, "Username already exists");

        // Login - Success
        var successLoginResponse = new LoginResponse
        {
            token = "mock-jwt-token",
            tokenType = "Bearer",
            refreshToken = "mock-refresh-token",
            username = "testuser"
        };
        AddMockResponse("login/success", 200, EncryptJsonResponse(JsonUtility.ToJson(successLoginResponse)));

        // Login - Invalid credentials
        AddMockResponse("login/fail", 401, "Invalid username or password");

        // Guest registration - Success
        var guestResponse = new GuestRegisterResponse
        {
            guestKey = "mock-guest-key"
        };
        AddMockResponse("register/guest", 200, JsonUtility.ToJson(guestResponse));

        // Guest login - Success
        var guestLoginResponse = new LoginResponse
        {
            token = "mock-guest-jwt-token",
            tokenType = "Bearer",
            refreshToken = "mock-guest-refresh-token",
            username = "guest"
        };
        AddMockResponse("login/guest", 200, EncryptJsonResponse(JsonUtility.ToJson(guestLoginResponse)));

        // Token refresh - Success
        var refreshResponse = new LoginResponse
        {
            token = "mock-refreshed-jwt-token",
            tokenType = "Bearer",
            refreshToken = "mock-new-refresh-token",
            username = "testuser"
        };
        AddMockResponse("refresh", 200, EncryptJsonResponse(JsonUtility.ToJson(refreshResponse)));
    }

    /// <summary>
    /// Handles a web request by returning a mocked response.
    /// </summary>
    private static UnityWebRequest HandleRequest(string endpoint, Methods method, bool requiresAuth, string jsonBody)
    {
        // Record the request for later verification
        ReceivedRequests.Add(new MockRequest
        {
            Endpoint = endpoint,
            Method = method,
            RequiresAuthorization = requiresAuth,
            RequestBody = jsonBody,
            Timestamp = DateTime.Now
        });

        // Create a mock request
        var request = new UnityWebRequest();
        request.url = endpoint;
        request.method = method.ToString();
        request.downloadHandler = new DownloadHandlerBuffer();

        // Simulate network conditions
        if (SimulateNetworkFailure)
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        if (SimulateTimeout)
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        if (SimulateServerError)
        {
            MockResponse serverError = new MockResponse
            {
                StatusCode = 500,
                Content = "Internal Server Error",
                IsNetworkError = false
            };

            ConfigureMockResponse(request, serverError);
            return request;
        }

        // Find matching mock response
        MockResponse response = null;

        // Special case handling based on endpoint and body content
        if (endpoint.Contains("register"))
        {
            // If registration contains a known username that should fail
            if (jsonBody != null && jsonBody.Contains("duplicate"))
            {
                response = MockResponses["register/duplicate"];
            }
            else if (endpoint.Contains("guest"))
            {
                response = MockResponses["register/guest"];
            }
            else
            {
                response = MockResponses["register"];
            }
        }
        else if (endpoint.Contains("login"))
        {
            // Parse login request to determine success/failure
            if (endpoint.Contains("guest"))
            {
                response = MockResponses["login/guest"];
            }
            else if (jsonBody != null &&
                    (jsonBody.Contains("testuser") || jsonBody.Contains("workflow_test") ||
                     jsonBody.Contains("refresh_test") || jsonBody.Contains("logout_test")))
            {
                response = MockResponses["login/success"];
            }
            else
            {
                response = MockResponses["login/fail"];
            }
        }
        else if (endpoint.Contains("refresh"))
        {
            response = MockResponses["refresh"];
        }
        else
        {
            // Default response for unhandled endpoints
            response = new MockResponse
            {
                StatusCode = 404,
                Content = "Not Found",
                IsNetworkError = false
            };
        }

        // Configure the response
        ConfigureMockResponse(request, response);

        return request;
    }

    /// <summary>
    /// Configures a UnityWebRequest with the specified mock response.
    /// </summary>
    private static void ConfigureMockResponse(UnityWebRequest request, MockResponse response)
    {
        // Simulate response code
        var requestOperation = new MockRequestOperation(request)
        {
            Status = UnityWebRequest.Result.Success,
            ResponseCode = response.StatusCode,
            Text = response.Content,
            Error = response.IsNetworkError ? "Network Error" : (response.StatusCode >= 400 ? "HTTP Error" : null)
        };

        // Set response code
        if (response.StatusCode >= 400)
        {
            requestOperation.Status = UnityWebRequest.Result.ProtocolError;
        }
    }

    /// <summary>
    /// Encrypts a JSON response for auth endpoints that use encryption.
    /// </summary>
    private static string EncryptJsonResponse(string json)
    {
        // Wrap in EncryptedMessage if needed
        var encryptedMessage = new EncryptedMessage("key", SecurityUtils.Encrypt(json));
        return JsonUtility.ToJson(encryptedMessage);
    }

    /// <summary>
    /// Helper class to store mock response information.
    /// </summary>
    public class MockResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public bool IsNetworkError { get; set; }
    }

    /// <summary>
    /// Helper class to store information about received requests.
    /// </summary>
    public class MockRequest
    {
        public string Endpoint { get; set; }
        public Methods Method { get; set; }
        public bool RequiresAuthorization { get; set; }
        public string RequestBody { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Mock implementation of UnityWebRequestOperation.
    /// </summary>
    private class MockRequestOperation
    {
        private readonly UnityWebRequest _request;

        public UnityWebRequest.Result Status { get; set; }
        public long ResponseCode { get; set; }
        public string Text { get; set; }
        public string Error { get; set; }

        public MockRequestOperation(UnityWebRequest request)
        {
            _request = request;
        }
    }
}

/// <summary>
/// Example of a test class using the MockRequestService.
/// </summary>
[TestFixture]
public class UserManagerUnitTests
{
    private UserManager userManager;
    private GameObject testGameObject;

    [SetUp]
    public void SetUp()
    {
        // Setup mock service
        MockRequestService.Setup();

        // Create test objects
        testGameObject = new GameObject("TestUserManager");
        userManager = testGameObject.AddComponent<UserManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up mocks
        MockRequestService.Cleanup();

        // Clean up test objects
        if (testGameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(testGameObject);
        }

        LocalStorageService.ClearAll();
    }

    [UnityTest]
    public IEnumerator Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var loginForm = new LoginForm("invaliduser", "wrongpassword");
        bool loginSuccessful = false;
        string errorMessage = null;

        // Act
        userManager.Login(
            loginForm,
            result => { loginSuccessful = true; },
            error =>
            {
                loginSuccessful = false;
                errorMessage = error;
            }
        );

        // Wait a frame for the mock to process
        yield return null;

        // Assert
        Assert.IsFalse(loginSuccessful, "Login should fail with invalid credentials");
    }

    [UnityTest]
    public IEnumerator Login_WithNetworkFailure_ShouldHandleGracefully()
    {
        // Arrange
        MockRequestService.SimulateNetworkFailure = true;

        var loginForm = new LoginForm("testuser", "password");
        bool loginSuccessful = false;
        string errorMessage = null;

        // Act
        userManager.Login(
            loginForm,
            result => { loginSuccessful = true; },
            error =>
            {
                loginSuccessful = false;
                errorMessage = error;
            }
        );

        // Wait a frame for the mock to process
        yield return null;

        // Assert
        Assert.IsFalse(loginSuccessful, "Login should fail when network fails");
    }

    [UnityTest]
    public IEnumerator CreateUser_WithDuplicateUsername_ShouldFail()
    {
        // Arrange
        var registerForm = new RegisterForm("existinguser", "existing@example.com", "password123");
        bool firstRegistrationSuccessful = false;
        string firstErrorMessage = null;

        bool secondRegistrationSuccessful = false;
        string secondErrorMessage = null;

        // Act - First registration (should succeed)
        userManager.CreateUser(
            registerForm,
            () => { firstRegistrationSuccessful = true; },
            error =>
            {
                firstRegistrationSuccessful = false;
                firstErrorMessage = error;
            }
        );

        yield return null;

        // Act - Second registration (should fail)
        userManager.CreateUser(
            registerForm,
            () => { secondRegistrationSuccessful = true; },
            error =>
            {
                secondRegistrationSuccessful = false;
                secondErrorMessage = error;
            }
        );

        yield return null;

        // Assert - Second registration should fail due to duplicate username
        Assert.IsFalse(secondRegistrationSuccessful, "Second registration should fail for duplicate username");
    }


    [UnityTest]
    public IEnumerator RefreshToken_WithInvalidToken_ShouldFail()
    {
        // Arrange
        string refreshToken = "$$invalid_token$$";
        bool refreshSuccessful = false;
        LoginResponse response = null;

        // Act
        userManager.RefreshToken(
            refreshToken,
            result =>
            {
                refreshSuccessful = true;
                response = result;
            },
            error =>
            {
                refreshSuccessful = false;
            }
        );

        // Wait a frame for the mock to process
        yield return null;

        // Assert
        Assert.IsFalse(refreshSuccessful, "Token refresh should fail with invalid token");
        Assert.IsNull(response, "Response should be null when refresh fails");

        // Verify a request was made to the correct endpoint
        Assert.IsTrue(MockRequestService.ReceivedRequests.Exists(r => r.Endpoint.Contains("refresh")),
            "Should have made a request to the refresh endpoint");
    }

}