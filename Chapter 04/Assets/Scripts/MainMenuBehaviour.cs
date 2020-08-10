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
    }
}