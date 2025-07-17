using Android.Content;
using Android.Security.Keystore;
using Android.Util;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using System;
using System.Text;

namespace ArtViewer.Network.DeviantArt
{

    /// <summary>
    /// Convienece tools for storing the appID in encrypted shared preferences.
    /// </summary>
    public static class SecurePreferences
    {
        private const string KeyAlias = "MySecureKey";
        private const string PrefName = "SecurePrefs";
        private const string EncryptedKey = "encrypted_data";
        private const string IVKey = "encryption_iv";

        private static KeyStore keyStore;



        static SecurePreferences()
        {
            keyStore = KeyStore.GetInstance("AndroidKeyStore");
            keyStore.Load(null);

            if (!keyStore.ContainsAlias(KeyAlias))
            {
                var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
                keyGenerator.Init(new KeyGenParameterSpec.Builder(KeyAlias,
                    KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(KeyProperties.BlockModeGcm)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
                    .Build());
                keyGenerator.GenerateKey();
            }
        }




        /// <summary>
        /// Encrypts the specified appID and puts it in the shared prefs.
        /// </summary>
        /// <param name="appID">The appID to encrpt and save.</param>
        public static void EncryptAppID(string appID)
        {
            var secretKey = ((KeyStore.SecretKeyEntry)keyStore.GetEntry(KeyAlias, null)).SecretKey;
            var cipher = Cipher.GetInstance("AES/GCM/NoPadding");
            cipher.Init(CipherMode.EncryptMode, secretKey);

            byte[] iv = cipher.GetIV();
            byte[] encrypted = cipher.DoFinal(Encoding.UTF8.GetBytes(appID));

            var prefs = Android.App.Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString(EncryptedKey, Convert.ToBase64String(encrypted));
            editor.PutString(IVKey, Convert.ToBase64String(iv));
            editor.Apply();

        }



        /// <summary>
        /// Decrypts the AppID stored in the app shared preferences and returns it.
        /// </summary>
        /// <returns>The decrypted appID or null if the appID is not saved in shared preferences.</returns>
        public static string? DecryptAppID()
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            string encryptedBase64 = prefs.GetString(EncryptedKey, null);
            string ivBase64 = prefs.GetString(IVKey, null);

            if (encryptedBase64 == null || ivBase64 == null)
                return null;

            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
            byte[] iv = Convert.FromBase64String(ivBase64);

            var secretKey = ((KeyStore.SecretKeyEntry)keyStore.GetEntry(KeyAlias, null)).SecretKey;
            var cipher = Cipher.GetInstance("AES/GCM/NoPadding");
            var spec = new GCMParameterSpec(128, iv);
            cipher.Init(CipherMode.DecryptMode, secretKey, spec);

            byte[] decryptedBytes = cipher.DoFinal(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);

        }



        /// <summary>
        /// Checks if the App ID is currently in the shared preferences.
        /// </summary>
        /// <returns>true if the AppID is saved locally and false otherwise</returns>
        public static bool AppIDExists()
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            return prefs.Contains(EncryptedKey) && prefs.Contains(IVKey);
        }



        /// <summary>
        /// Deletes the AppID from shared preferences.
        /// </summary>
        public static void DeleteStoredAppID()
        {
            var prefs = Android.App.Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.Remove(EncryptedKey);
            editor.Remove(IVKey);
            editor.Apply();
        }

    }
}
