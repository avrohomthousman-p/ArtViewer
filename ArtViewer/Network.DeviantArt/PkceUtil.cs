using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Network.DeviantArt
{
    public static class PkceUtil
    {
        public static string GenerateCodeVerifier()
        {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Base64UrlEncode(bytes);
        }



        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(codeVerifier);
                var hash = sha256.ComputeHash(bytes);
                return Base64UrlEncode(hash);
            }
        }



        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
