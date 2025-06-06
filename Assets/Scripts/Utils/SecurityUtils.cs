// Assets/Scripts/Utils/SecurityUtils.cs
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class SecurityUtils
    {
        /* ──────────────────────────────────────────────────
           AES helpers — untouched
        ────────────────────────────────────────────────── */
        static readonly byte[] Key =
            Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 B

        public static string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] input = Encoding.UTF8.GetBytes(plain);
            byte[] enc = aes.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);

            byte[] outBytes = new byte[aes.IV.Length + enc.Length];
            Buffer.BlockCopy(aes.IV, 0, outBytes, 0, aes.IV.Length);
            Buffer.BlockCopy(enc, 0, outBytes, aes.IV.Length, enc.Length);
            return Convert.ToBase64String(outBytes);
        }

        public static string Decrypt(string base64)
        {
            byte[] combo = Convert.FromBase64String(base64);
            byte[] iv = new byte[16];
            byte[] enc = new byte[combo.Length - 16];

            Buffer.BlockCopy(combo, 0, iv, 0, 16);
            Buffer.BlockCopy(combo, 16, enc, 0, enc.Length);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] plain = aes.CreateDecryptor()
                               .TransformFinalBlock(enc, 0, enc.Length);
            return Encoding.UTF8.GetString(plain);
        }

        /* ──────────────────────────────────────────────────
           JWT helpers — fixed for Bearer prefix + URL-safe b64
        ────────────────────────────────────────────────── */
        [Serializable] public class JwtPayload { public long exp; }

        /// <summary>Decodes the middle part of a JWT (after stripping “Bearer ”).</summary>
        static byte[] FromBase64Url(string value)
        {
            value = value.Replace('-', '+').Replace('_', '/');
            switch (value.Length % 4)                       // pad to 4 bytes
            {
                case 2: value += "=="; break;
                case 3: value += "="; break;
            }
            return Convert.FromBase64String(value);
        }

        public static DateTime? GetExpiryFromJwt(string jwt)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt)) return null;

                string token = jwt.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                             ? jwt.Substring(7)
                             : jwt;

                string[] parts = token.Split('.');
                if (parts.Length < 2)                       // header.payload.sig
                {
                    Debug.LogError("Invalid JWT token format.");
                    return null;
                }

                byte[] payloadBytes = FromBase64Url(parts[1]);
                string json = Encoding.UTF8.GetString(payloadBytes);

                var data = JsonUtility.FromJson<JwtPayload>(json);
                return DateTimeOffset.FromUnixTimeSeconds(data.exp).UtcDateTime;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to decode JWT: " + ex.Message);
                return null;
            }
        }

        public static bool IsTokenValid(string jwt)
        {
            DateTime? exp = GetExpiryFromJwt(jwt);
            return exp != null && exp.Value > DateTime.UtcNow;
        }
    }
}
