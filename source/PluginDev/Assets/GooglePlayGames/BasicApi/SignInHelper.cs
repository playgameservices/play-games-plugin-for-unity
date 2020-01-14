using System.Collections.Generic;
using UnityEngine;

namespace GooglePlayGames.BasicApi
{
    public class SignInHelper
    {
        private static int True = 0;
        private static int False = 1;
        private const string PromptSignInKey = "prompt_sign_in";

        public static SignInStatus ToSignInStatus(int code)
        {
            Dictionary<int, SignInStatus> dictionary = new Dictionary<int, SignInStatus>()
            {
                {
                    /* CommonUIStatus.UI_BUSY */ -12, SignInStatus.AlreadyInProgress
                },
                {
                    /* CommonStatusCodes.SUCCESS */ 0, SignInStatus.Success
                },
                {
                    /* CommonStatusCodes.SIGN_IN_REQUIRED */ 4, SignInStatus.UiSignInRequired
                },
                {
                    /* CommonStatusCodes.NETWORK_ERROR */ 7, SignInStatus.NetworkError
                },
                {
                    /* CommonStatusCodes.INTERNAL_ERROR */ 8, SignInStatus.InternalError
                },
                {
                    /* CommonStatusCodes.DEVELOPER_ERROR */ 10, SignInStatus.DeveloperError
                },
                {
                    /* CommonStatusCodes.CANCELED */ 16, SignInStatus.Canceled
                },
                {
                    /* CommonStatusCodes.API_NOT_CONNECTED */ 17, SignInStatus.Failed
                },
                {
                    /* GoogleSignInStatusCodes.SIGN_IN_FAILED */ 12500, SignInStatus.Failed
                },
                {
                    /* GoogleSignInStatusCodes.SIGN_IN_CANCELLED */ 12501, SignInStatus.Canceled
                },
                {
                    /* GoogleSignInStatusCodes.SIGN_IN_CURRENTLY_IN_PROGRESS */ 12502, SignInStatus.AlreadyInProgress
                },
            };

            return dictionary.ContainsKey(code) ? dictionary[code] : SignInStatus.Failed;
        }

        /// <summary>
        /// Used during authentication to save if the user should be prompted to interactive sign in next time they
        /// try to authenticate with SignInInteractivity.CanPromptOnce.
        /// </summary>
        /// <param name="value"></param>
        public static void SetPromptUiSignIn(bool value)
        {
            PlayerPrefs.SetInt(PromptSignInKey, value ? True : False);
        }

        /// <summary>
        /// Used during authentication with SignInInteractivity.CanPromptOnce to understand whether or not the user should be
        /// prompted to interactive sign in.
        /// </summary>
        /// <returns></returns>
        public static bool ShouldPromptUiSignIn()
        {
            return PlayerPrefs.GetInt(PromptSignInKey, True) != False;
        }

    }
}
