using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Will show or remove a button depending on if we can restore /// ads or not
/// </summary>
public class RestoreAdsChecker : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        bool canRestore = false;

        switch (Application.platform)
        {
            // Windows Store
            case RuntimePlatform.WSAPlayerX86:
            case RuntimePlatform.WSAPlayerX64:
            case RuntimePlatform.WSAPlayerARM:

            // iOS, OSX, tvOS
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.tvOS:

                canRestore = true;
                break;

            // Android
            case RuntimePlatform.Android:
                switch (StandardPurchasingModule.Instance().appStore)
                {
                    case AppStore.SamsungApps:
                    case AppStore.CloudMoolah:
                        canRestore = true;
                        break;

                }
                break;
        }

        gameObject.SetActive(canRestore);
    }

}