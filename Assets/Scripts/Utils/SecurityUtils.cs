using System.Text;
using System;
using UnityEngine;
using System.Security.Cryptography;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Utils
{
    public static class SecurityUtils
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes = 256-bit

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedBase64)
        {
            byte[] combinedBytes = Convert.FromBase64String(encryptedBase64);

            byte[] iv = new byte[16];
            byte[] cipherBytes = new byte[combinedBytes.Length - 16];

            Buffer.BlockCopy(combinedBytes, 0, iv, 0, 16);
            Buffer.BlockCopy(combinedBytes, 16, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)UnityEngine.Random.Range(0, 256);
            }
            return bytes;
        }

        [Serializable]
        public class JwtPayload
        {
            public long exp;
        }

        public static DateTime? GetExpiryFromJwt(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2)
                {
                    Debug.LogError("Invalid JWT token format.");
                    return null;
                }

                string payload = parts[1];
                byte[] payloadBytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(payloadBytes);

                JwtPayload payloadData = JsonUtility.FromJson<JwtPayload>(json);

                DateTime expiryDate = DateTimeOffset.FromUnixTimeSeconds(payloadData.exp).UtcDateTime;
                return expiryDate;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to decode JWT: " + ex.Message);
                return null;
            }
        }

        public static bool IsTokenValid(string jwt)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt)) { 
                    return false;
                }

                if (GetExpiryFromJwt(jwt) != null && GetExpiryFromJwt(jwt).Value > DateTime.UtcNow) { 
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to decode JWT: " + ex.Message);
                return false;
            }
        }
    }
}
