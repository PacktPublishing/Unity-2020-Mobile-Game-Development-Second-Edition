using UnityEngine;
using UnityEngine.UI; // Button
using System; // DateTime
using System.Collections; // IEnumerator

public class ObstacleBehaviour : MonoBehaviour
{

    [Tooltip("How long to wait before restarting the game")]
    public float waitTime = 2.0f;

    public GameObject explosion;

    private GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        // First check if we collided with the player 
        if (collision.gameObject.GetComponent<PlayerBehaviour>())
        {
            // Destroy (Hide) the player
            collision.gameObject.SetActive(false);

            player = collision.gameObject;

            // Call the function ResetGame after waitTime has passed 
            Invoke("ResetGame", waitTime);
        }
    }

    /// <summary> 
    /// Will restart the currently loaded level 
    /// </summary> 
    private void ResetGame()
    {
        //Bring up restart menu
        var go = GetGameOverMenu();
        go.SetActive(true);

        // Get our continue button
        var buttons = go.transform.GetComponentsInChildren<Button>();
        Button continueButton = null;

        foreach (var button in buttons)
        {
            if (button.gameObject.name == "Continue Button")
            {
                continueButton = button;
                break;
            }
        }

        // If we found the button we can use it
        if (continueButton)
        {
            //if (UnityAdController.showAds)
            {
                // If player clicks on button we want to play ad and
                // then continue
                StartCoroutine(ShowContinue(continueButton));
            }
            //else if(Application.platform)
            //{
            //    // If can't play an ad, no need for continue button
            //    continueButton.gameObject.SetActive(false);
            //}

        }
    }

    /// <summary> 
    /// If the object is tapped, we spawn an explosion and 
    /// destroy this object 
    /// </summary> 
    private void PlayerTouch()
    {
        if (explosion != null)
        {
            var particles = Instantiate(explosion, transform.position,
            Quaternion.identity);
            Destroy(particles, 1.0f);
        }

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Retrieves the Game Over menu game object
    /// </summary>
    /// <returns>The Game Over menu object</returns>
    GameObject GetGameOverMenu()
    {
        var canvas = GameObject.Find("Canvas").transform;
        return canvas.Find("Game Over").gameObject;
    }

    /// <summary>
    /// Handles resetting the game if needed
    /// </summary>
    public void Continue()
    {
        var go = GetGameOverMenu();
        go.SetActive(false);
        player.SetActive(true);

        // Explode this as well (So if we respawn player can continue)
        PlayerTouch();
    }

    public IEnumerator ShowContinue(Button contButton)
    {
        while (true)
        {
            var btnText = contButton.GetComponentInChildren<Text>();

            // Check if we haven't reached the next reward time yet 
            // (if one exists) 
            var rewardTime = UnityAdController.nextRewardTime;

            bool validTime = rewardTime.HasValue;
            bool timePassed = true;

            if (validTime)
            {
                timePassed = DateTime.Now > rewardTime.Value;
            }

            if (!timePassed)
            {
                // Unable to click on the button 
                contButton.interactable = false;

                // Get the time remaining until we get to the next 
                // reward time 
                TimeSpan remaining = rewardTime.Value - DateTime.Now;

                // Get the time left in the following format 99:99 
                var countdownText = string.Format("{0:D2}:{1:D2}",
                                    remaining.Minutes,
                                    remaining.Seconds);

                // Set our button's text to reflect the new time 
                btnText.text = countdownText;

                // Come back after 1 second and check again 
                yield return new WaitForSeconds(1f);
            }
            else if (!UnityAdController.showAds)
            {
                // It's valid to click the button now
                contButton.interactable = true;

                // If player clicks on button we want to just continue
                contButton.onClick.AddListener(Continue);

                UnityAdController.obstacle = this;

                // Change text to allow continue
                btnText.text = "Free Continue";

                // We can now leave the coroutine
                break;
            }
            else
            {
                // It's valid to click the button now 
                contButton.interactable = true;

                // If player clicks on button we want to play ad and 
                // then continue 
                contButton.onClick.AddListener(UnityAdController.ShowRewardAd);
                UnityAdController.obstacle = this;

                // Change text to its original version 
                btnText.text = "Continue (Play Ad)";

                // We can now leave the coroutine 
                break;
            }
        }
    }

}