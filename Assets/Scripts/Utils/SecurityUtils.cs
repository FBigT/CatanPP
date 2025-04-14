using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class SecurityUtils
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private static byte[] Password { get; set; } = RandomBytes(KeySize);

        public static string Encrypt(string plainText)
        {
            byte[] nonce = RandomBytes(NonceSize);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aes = new AesGcm(Password))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(nonce, 0, nonce.Length);
                ms.Write(tag, 0, tag.Length);
                ms.Write(ciphertext, 0, ciphertext.Length);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string Decrypt(string encryptedBase64)
        {
            byte[] data = Convert.FromBase64String(encryptedBase64);
            byte[] nonce = data[..NonceSize];
            byte[] tag = data[NonceSize..(NonceSize + TagSize)];
            byte[] ciphertext = data[(NonceSize + TagSize)..];
            byte[] plaintextBytes = new byte[ciphertext.Length];

            using (var aes = new AesGcm(Password))
            {
                aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private static byte[] RandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
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
                // Split the token into its parts
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
    }
}
