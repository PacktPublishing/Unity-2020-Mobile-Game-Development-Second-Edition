using UnityEngine;
using UnityEngine.SceneManagement; // LoadScene 
using System.Collections.Generic; // List 
using Facebook.Unity; // FB
using UnityEngine.UI; // Text / Image 

public class MainMenuBehaviour : MonoBehaviour
{
    /// <summary> 
    /// Will load a new scene upon being called 
    /// </summary> 
    /// <param name="levelName">The name of the level we want 
    /// to go to</param> 
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);

#if UNITY_ADS
        if (UnityAdController.showAds)
        {
            // Show an ad 
            UnityAdController.ShowAd();
        }
#endif

    }

    public void DisableAds()
    {
        UnityAdController.showAds = false;

        // Used to store that we shouldn't show ads 
        PlayerPrefs.SetInt("Show Ads", 0);
    }

    virtual protected void Start()
    {
        // Initialize the showAds variable 
        UnityAdController.showAds = (PlayerPrefs.GetInt("Show Ads", 1) == 1);
    }

    #region Facebook 

    public void Awake()
    {
        // We only call FB Init once, so check if it has been called  
        // already 
        if (!FB.IsInitialized)
        {
            FB.Init(OnInitComplete, OnHideUnity);
        }
    }
    /// <summary> 
    /// Once initialized, will inform if logged in on Facebook 
    /// </summary> 
    void OnInitComplete()
    {
        if (FB.IsLoggedIn)
        {
            print("Logged into Facebook");

            // Close Login and open Main Menu 
            ShowMainMenu();
        }
    }

    /// <summary> 
    /// Called whenever Unity loses focus 
    /// </summary> 
    /// <param name="active">If the gmae is currently active</param> 
    void OnHideUnity(bool active)
    {
        // Set TimeScale based on if the game is paused 
        Time.timeScale = (active) ? 1 : 0;
    }

    /// <summary> 
    /// Attempts to log in on Facebook 
    /// </summary> 
    public void FacebookLogin()
    {
        List<string> permissions = new List<string>();

        // Add permissions we want to have here 
        permissions.Add("public_profile");

        FB.LogInWithReadPermissions(permissions, FacebookCallback);
    }

    /// <summary> 
    /// Called once facebook has logged in, or not 
    /// </summary> 
    /// <param name="result">The result of our login request</param> 
    void FacebookCallback(IResult result)
    {
        if (result.Error == null)
        {
            OnInitComplete();
        }
        else
        {
            print(result.Error);
        }
    }

    [Header("Object References")]
    public GameObject mainMenu;
    public GameObject facebookLogin;

    public void ShowMainMenu()
    {
        if (facebookLogin != null && mainMenu != null)
        {
            facebookLogin.SetActive(false);
            mainMenu.SetActive(true);

            if (FB.IsLoggedIn)
            {
                // Get information from Facebook profile 
                FB.API("/me?fields=name", HttpMethod.GET, SetName);
                FB.API("/me/picture?width=256&height=256",
                HttpMethod.GET, SetProfilePic);
            }
        }

    }



public Text greeting;

void SetName(IResult result)
{
    if (result.Error != null)
    {
        print(result.Error);
        return;
    }

    string playerName = result.ResultDictionary["name"].ToString();
    greeting.text = "Hello, " + playerName + "!";

    greeting.gameObject.SetActive(true);
}

public Image profilePic;

void SetProfilePic(IGraphResult result)
{
    if (result.Error != null)
    {
        print(result.Error);
        return;
    }

    Sprite fbImage = Sprite.Create(result.Texture,
                                    new Rect(0, 0, 256, 256),
                                    Vector2.zero);
    profilePic.sprite = fbImage;

    profilePic.gameObject.SetActive(true);
}

    #endregion

}
