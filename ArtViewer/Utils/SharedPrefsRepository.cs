using Android.Content;
using System;

namespace ArtViewer.Utils
{
    /// <summary>
    /// Utility class for saving and loading data from Shared Preferences easily
    /// </summary>
    public class SharedPrefsRepository
    {
        private const string PREFERENCES_KEY = "MyPrefs";
        private const string REFRESH_TOKEN_KEY = "refresh_token";
        private const string REFESH_TOKEN_EXPIRATION_DATE_KEY = "refresh_token_expiration_date";
        public const string PKCE_CODE_VERIFIER_KEY = "pkce_code_verifier";

        private readonly ISharedPreferences _prefs;


        public SharedPrefsRepository()
        {
            this._prefs = Application.Context.GetSharedPreferences(PREFERENCES_KEY, FileCreationMode.Private)
                ?? throw new InvalidOperationException("SharedPreferences Unavialible");
        }



        public string? RefreshToken
        {
            get => _prefs.GetString(REFRESH_TOKEN_KEY, null);
            set
            {
                ISharedPreferencesEditor? editor = _prefs.Edit();
                if (String.IsNullOrEmpty(value))
                {
                    editor?.Remove(REFRESH_TOKEN_KEY);
                    editor?.Remove(REFESH_TOKEN_EXPIRATION_DATE_KEY);
                }
                else
                {
                    editor?.PutString(REFRESH_TOKEN_KEY, value);
                }

                editor?.Apply();
            }
        }


        public DateTime? RefreshTokenExpirationDate
        {
            get
            {
                string? savedDate = _prefs.GetString(REFESH_TOKEN_EXPIRATION_DATE_KEY, null);

                if (String.IsNullOrEmpty(savedDate))
                    return null;

                if (!DateTime.TryParse(savedDate, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expirationDate))
                    return null;

                return expirationDate;
            }
            set
            {
                ISharedPreferencesEditor? editor = _prefs.Edit();
                if (value == null)
                {
                    editor?.Remove(REFRESH_TOKEN_KEY);
                    editor?.Remove(REFESH_TOKEN_EXPIRATION_DATE_KEY);
                }
                else
                {
                    editor?.PutString(REFESH_TOKEN_EXPIRATION_DATE_KEY, value.Value.ToString("O")); //ISO 8601 format
                }

                editor?.Apply();
            }
        }


        public string? PkceCodeVerifier
        {
            get => _prefs.GetString(PKCE_CODE_VERIFIER_KEY, null);
            set
            {
                ISharedPreferencesEditor? editor = _prefs.Edit();

                if (String.IsNullOrEmpty(value))
                {
                    editor?.Remove(PKCE_CODE_VERIFIER_KEY);
                }
                else
                {
                    editor?.PutString(PKCE_CODE_VERIFIER_KEY, value);
                }

                editor?.Apply();
            }
        }
    }
}
