using UnityEngine;
using UnityEngine.SceneManagement; // LoadScene 

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

}
