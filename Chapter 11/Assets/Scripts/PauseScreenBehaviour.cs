using System;
using UnityEngine;
using UnityEngine.Analytics;       // CustomEvent
using UnityEngine.SceneManagement; // SceneManager 

public class PauseScreenBehaviour : MainMenuBehaviour
{

    public static bool paused;

    [Tooltip("Reference to the pause menu object to turn on/off")]
    public GameObject pauseMenu;

    #region Share Score via Twitter 

    /// <summary>
    /// Web address in order to create a tweet 
    /// </summary>
    private const string tweetTextAddress =
                                "http://twitter.com/intent/tweet?text=";

    /// <summary>
    /// Where we want players to visit 
    /// </summary>
    private string appStoreLink = "http://johnpdoran.com/";

    [Tooltip("Reference to the player for the score")]
    public PlayerBehaviour player;

    /// <summary> 
    /// Will open Twitter with a prebuilt tweet. When called on iOS or 
    /// Android will open up Twitter app if installed 
    /// </summary> 
    public void TweetScore()
    {
        // Create contents of the tweet
        string tweet = "I got " + string.Format("{0:0}", player.Score)
                        + " points in Endless Roller! Can you do better?";

        // Creaft the entire message
        string message = tweet + "\n" + appStoreLink;

        //Ensures string is URL friendly
        string url = UnityEngine.Networking.UnityWebRequest.EscapeURL(message);

        // Open the URL to create the tweet 
        Application.OpenURL(tweetTextAddress + url);
    }

    #endregion

    /// <summary> 
    /// Reloads our current level, effectively "restarting" the     
    /// game 
    /// </summary> 
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary> 
    /// Will turn our pause menu on or off 
    /// </summary> 
    /// <param name="isPaused"></param> 
    public void SetPauseMenu(bool isPaused)
    {
        paused = isPaused;

        // If the game is paused, timeScale is 0, otherwise 1 
        Time.timeScale = (paused) ? 0 : 1;

        // No longer needed
        //pauseMenu.SetActive(paused);

        if (paused)
        {
            SlideMenuIn(pauseMenu);
        }
        else
        {
            SlideMenuOut(pauseMenu);
        }

        if (paused)
        {
            var result = Analytics.CustomEvent("Paused");

            if (result == AnalyticsResult.Ok)
            {
                Debug.Log("Event Sent: Paused");
            }
        }
    }

    protected override void Start()
    {
        // Initalize Ads if needed
        base.Start();

        if (!UnityAdController.showAds)
        {
            // If not showing ads, just start the game
            SetPauseMenu(false);
        }
    }

}