// Assets/Scripts/Utils/SecurityUtils.cs
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Assets.Scripts.Dtos;
using static UnityEditor.PlayerSettings;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;

namespace Assets.Scripts.Utils
{
    public static class SecurityUtils
    {
        /* ──────────────────────────────────────────────────
           AES helpers — untouched
        ────────────────────────────────────────────────── */
        static readonly byte[] Key =
            Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 B

        static byte[] CurrentKey;

        private static readonly string rsaPublicKeyXml = @"<RSAKeyValue>
  <Modulus>nmJ9lMXCqMhUo9DotRngBZEANKp0E+plE+QL6ZjtNrQV4flHIguU60jBWxCNR6hM7JRvmY2aQcwCNSGxYR0ywlSg+h21eTLQ52/fONrtrA/SwO7JPSp0RtOCSCt2j+XnVi37J6bh9m26G0V6tIJXAFHNdlmyr2CY65DBiERby5dbXxWoEXAVE+aCgBGw3OUUIYGe7c4qm8eov5go3XmrqHdNIRybUwsUA9UuwojDhZpmkV+rt8CUCxCq7LryjtW9ksbUWOPQiIEMct+jtCV6DtYNfLasiOCiP72V2nfBO7YW/G2km/4uXDd4GwzkSu/SzfXQC+y+njVH7o/k6W0c5w==</Modulus>
  <Exponent>AQAB</Exponent>
</RSAKeyValue>";

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

        public static byte[] createKey() {
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            return aes.Key;
        }

        public static string EncryptKey(byte[] key) {
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(rsaPublicKeyXml);
            byte[] encryptedKey = rsa.Encrypt(key, false);

            return Convert.ToBase64String(encryptedKey);
        }

        public static string EncryptRequest(string json, out string encryptedAesKeyB64, out byte[] aesKey)
        {
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aesKey = aes.Key;
            byte[] iv = aes.IV;

            byte[] plainBytes = Encoding.UTF8.GetBytes(json);
            byte[] encryptedPayload;
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                encryptedPayload = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            byte[] ivAndPayload = new byte[iv.Length + encryptedPayload.Length];
            Buffer.BlockCopy(iv, 0, ivAndPayload, 0, iv.Length);
            Buffer.BlockCopy(encryptedPayload, 0, ivAndPayload, iv.Length, encryptedPayload.Length);

            encryptedAesKeyB64 = EncryptKey(aesKey);

            return Convert.ToBase64String(ivAndPayload);
        }

        public static string DecryptResponse(string base64EncryptedResponse, byte[] aesKey)
        {
            byte[] ivAndCipher = Convert.FromBase64String(base64EncryptedResponse);

            byte[] iv = new byte[16];
            byte[] cipherBytes = new byte[ivAndCipher.Length - 16];
            Buffer.BlockCopy(ivAndCipher, 0, iv, 0, 16);
            Buffer.BlockCopy(ivAndCipher, 16, cipherBytes, 0, cipherBytes.Length);

            using Aes aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
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
