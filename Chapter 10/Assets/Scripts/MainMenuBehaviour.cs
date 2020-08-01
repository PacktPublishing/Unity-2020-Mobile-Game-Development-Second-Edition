﻿using UnityEngine;
using UnityEngine.SceneManagement; // LoadScene 
using System.Collections.Generic; // List 
using Facebook.Unity; // FB
using UnityEngine.UI; // Text / Image

public class MainMenuBehaviour : MonoBehaviour
{

    [Header("Object References")]
    public GameObject mainMenu;
    public GameObject facebookLogin;

    
    [Tooltip("Will display the user's Facebook profile pic")]
    public Image profilePic;

    [Tooltip("The text object used to display the greeting")]
    public Text greeting;


    /// <summary> 
    /// Will load a new scene upon being called 
    /// </summary> 
    /// <param name="levelName">The name of the level we want 
    /// to go to</param> 
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);

        if (UnityAdController.showAds)
        {
            // Show an ad 
            UnityAdController.ShowAd();
        }
    }

    public void DisableAds()
    {
        UnityAdController.showAds = false;

        // Used to store that we shouldn't show ads
        PlayerPrefs.SetInt("Show Ads", 0);
    }

    protected virtual void Start()
    {
        // Initialize the showAds variable
        UnityAdController.showAds = (PlayerPrefs.GetInt("Show Ads", 1) == 1);

        if (facebookLogin != null)
        {
            SlideMenuIn(facebookLogin);
        }

        // Unpause the game if needed
        Time.timeScale = 1;
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
    private void OnInitComplete()
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
    private void OnHideUnity(bool active)
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
    private void FacebookCallback(IResult result)
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

    public void ShowMainMenu()
    {
        if (facebookLogin != null && mainMenu != null)
        {
            mainMenu.SetActive(true);
            SlideMenuIn(mainMenu);
            SlideMenuOut(facebookLogin);

            // No longer needed as menus will be animating
            //facebookLogin.SetActive(false);
            //mainMenu.SetActive(true);


            if (FB.IsLoggedIn)
            {
                // Get information from Facebook profile 
                FB.API("/me?fields=name", HttpMethod.GET, SetName);
                FB.API("/me/picture?width=256&height=256",
                HttpMethod.GET, SetProfilePic);
            }
        }
    }

    private void SetName(IResult result)
    {
        if (result.Error != null)
        {
            print(result.Error);
            return;
        }

        string playerName = result.ResultDictionary["name"].ToString();

        if(greeting != null)
        {
            greeting.text = "Hello, " + playerName + "!";

            greeting.gameObject.SetActive(true);
        }

    }



    private void SetProfilePic(IGraphResult result)
    {
        if (result.Error != null)
        {
            print(result.Error);
            return;
        }

        Sprite fbImage = Sprite.Create(result.Texture,
                                        new Rect(0, 0, 256, 256),
                                        Vector2.zero);
        if(profilePic != null)
        {
            profilePic.sprite = fbImage;

            profilePic.gameObject.SetActive(true);
        }
    }

    #endregion

    /// <summary> 
    /// Will move an object from the left side of the screen 
    /// to the center 
    /// </summary> 
    /// <param name="obj">The UI element we would like to 
    /// move</param> 
    public void SlideMenuIn(GameObject obj)
    {
        obj.SetActive(true);

        var rt = obj.GetComponent<RectTransform>();

        if (rt)
        {
            // Set the object's position offscreen 
            var pos = rt.position;
            pos.x = -Screen.width / 2;
            rt.position = pos;

            // Move the object to the center of the screen (x of 0 is centered)
            var tween = LeanTween.moveX(rt, 0, 1.5f);
            tween.setEase(LeanTweenType.easeInOutExpo);
            tween.setIgnoreTimeScale(true);

            LeanTween.alpha(rt, 1, 0.5f);
        }
        
    }

    /// <summary> 
    /// Will move an object to the right offscreen
    /// </summary> 
    /// <param name="obj">The UI element we would like to 
    /// move </param> 
    public void SlideMenuOut(GameObject obj)
    {
        var rt = obj.GetComponent<RectTransform>();
        
        if(rt)
        {
            var tween = LeanTween.moveX(rt, Screen.width / 2, 0.5f);

            tween.setEase(LeanTweenType.easeOutQuad);

            tween.setIgnoreTimeScale(true);

            tween.setOnComplete(() =>
            {
                obj.SetActive(false);
            });

            LeanTween.alpha(rt, 0, 0.5f);
        }
    }

}