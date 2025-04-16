using System;
using UnityEngine;
using Assets.Scripts.User;
using Assets.Scripts.Utils;

namespace Catan.UI.Test
{
    public class AutoLogin : MonoBehaviour
    {
        private UserManager userManager;
        public static event Action OnLoginComplete;

        void Awake()
        {
            userManager = gameObject.AddComponent<UserManager>();
            userManager.CreateGuest(GuestOnSuccess, OnError);
        }

        private void GuestOnSuccess(GuestRegisterResponse guestResponse)
        {
            LocalStorageService.SetVariable("guest-code", guestResponse.guestKey);
            userManager.GuestLogin(guestResponse.guestKey, LoginSuccess, OnError);
        }

        private void LoginSuccess(LoginResponse response)
        {
            LocalStorageService.SetVariable("token", response.tokenType + " " + response.token);
            LocalStorageService.SetVariable("refresh-token", response.refreshToken);
            OnLoginComplete?.Invoke();
        }

        private void OnError(string error)
        {
            Debug.LogError("[AutoLogin] " + error);
        }
    }
}