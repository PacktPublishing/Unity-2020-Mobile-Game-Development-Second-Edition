using System; // DateTime
using UnityEngine;
using UnityEngine.Advertisements; // Advertisement class

public class UnityAdController : MonoBehaviour, IUnityAdsListener
{
    /// <summary>
    /// If we should show ads or not
    /// </summary>
    public static bool showAds = true;

    // Nullable type 
    public static DateTime? nextRewardTime = null;

    // For holding the obstacle for continuing the game 
    public static ObstacleBehaviour obstacle;

    /// <summary>
    /// Replace with your actual gameId
    /// </summary>
    private string gameId = "1234567";

    /// <summary>
    /// If the game is in test mode or not
    /// </summary>
    private bool testMode = true;

    /// <summary>
    /// Unity Ads must be initialized or else ads will not work properly
    /// </summary>
    private void Start()
    {
        // No need to initialize if it already is done
        if(!Advertisement.isInitialized)
        {
            // Use the functions provided by this to allow custom behavior
            // on the ads
            Advertisement.AddListener(this);
            Advertisement.Initialize(gameId, testMode);
            
        }
    }

    public static void ShowAd()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
    }

    public static void ShowRewardAd()
    {
        nextRewardTime = DateTime.Now.AddSeconds(15);

        ShowAd();
    }

    #region IUnityAdsListener Methods
    public void OnUnityAdsReady(string placementId)
    {
        // Actions to take when an Ad is ready to display, such as enabling
        // a rewards button
    }

    public void OnUnityAdsDidError(string message)
    {
        // If there was an error, display it
        Debug.LogError(message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Pause game while ad is shown 
        PauseScreenBehaviour.paused = true;
        Time.timeScale = 0f;
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // If there is an obstacle, we can remove it to continue the game
        if (obstacle != null && showResult == ShowResult.Finished)
        {
            obstacle.Continue();
        }

        // Unpause when ad is over 
        PauseScreenBehaviour.paused = false;
        Time.timeScale = 1f;
    }
    #endregion

}