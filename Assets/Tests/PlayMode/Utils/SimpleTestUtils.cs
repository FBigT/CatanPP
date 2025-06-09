using UnityEngine;
using System;

/// <summary>
/// Simplified test utilities that don't require external dependencies
/// </summary>
public static class SimpleTestUtils
{
    /// <summary>
    /// Creates a unique test username
    /// </summary>
    public static string CreateUniqueUsername(string prefix = "testuser")
    {
        return $"{prefix}_{DateTime.Now.Ticks}";
    }

    /// <summary>
    /// Creates a unique test email
    /// </summary>
    public static string CreateUniqueEmail(string prefix = "test")
    {
        return $"{prefix}_{DateTime.Now.Ticks}@example.com";
    }

    /// <summary>
    /// Standard test password
    /// </summary>
    public static string GetTestPassword()
    {
        return "TestPassword123!";
    }

    /// <summary>
    /// Logs test success message
    /// </summary>
    public static void LogTestSuccess(string message)
    {
        Debug.Log($"<color=green>✓ {message}</color>");
    }

    /// <summary>
    /// Logs test failure message
    /// </summary>
    public static void LogTestFailure(string message)
    {
        Debug.LogError($"<color=red>✗ {message}</color>");
    }
}
