#if UNITY_STANDALONE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Application;

public class GoogleLogIn : MonoBehaviour
{
    public void GoogleLoginButtonClick()
    {
        string clientId = "1069658341853-s2r0s6f0bsj2ihfvo7097rfbi0fj2h5p.apps.googleusercontent.com";
        string redirectUri = "https://metashopgooglelogin.netlify.app/.netlify/functions/googleIdToken";//코드를 가져온 uri와 같은 uri사용해야함.
        string authorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        string responseType = "code";
        string scope = "email";

        string authUrl = $"{authorizationUrl}?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}";
        Application.OpenURL(authUrl);
        Application.Quit();
    }
}
#endif
