using UnityEngine;

/// <summary> 
/// Responsible for moving the player automatically and 
/// reciving input. 
/// </summary> 
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    /// <summary> 
    /// A reference to the Rigidbody component 
    /// </summary> 
    private Rigidbody rb;

    [Tooltip("How fast the ball moves left/right")]
    public float dodgeSpeed = 5;

    [Tooltip("How fast the ball moves forwards automatically")]
    [Range(0, 10)]
    public float rollSpeed = 5;

    public enum MobileHorizMovement
    {
        Accelerometer,
        ScreenTouch
    }

    public MobileHorizMovement horizMovement = MobileHorizMovement.Accelerometer;

    [Header("Swipe Properties")]
    [Tooltip("How far will the player move upon swiping")]
    public float swipeMove = 2f;


    [Tooltip("How far must the player swipe before we will execute the action (in inches)")]
    public float minSwipeDistance = 0.25f;

    /// <summary>
    /// Used to hold the value that converts minSwipeDistance to pixels
    /// </summary>
    private float minSwipeDistancePixels;

    /// <summary> 
    /// Stores the starting position of mobile touch events 
    /// </summary> 
    private Vector2 touchStart;


    [Header("Scaling Properties")]

    [Tooltip("The minimum size (in Unity units) that the player should be")]
    public float minScale = 0.5f;

    [Tooltip("The maximum size (in Unity units) that the player should be")]
    public float maxScale = 3.0f;

    /// <summary>
    /// The current scale of the player
    /// </summary>
    private float currentScale = 1;

    // Start is called before the first frame update
    private void Start()
    {
        // Get access to our Rigidbody component 
        rb = GetComponent<Rigidbody>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;
    }

    /// <summary>
    /// FixedUpdate is called at a fixed framerate and is a prime place to put
    /// Anything based on time.
    /// </summary>
    private void FixedUpdate()
    {
        // If the game is paused, don't do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }

        // Check if we're moving to the side 
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

        // Check if we are running either in the Unity editor or in a 
        // standalone build. 
        #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
            // Check if we're moving to the side 
            horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

            // If the mouse is held down (or the screen is tapped 
            // on Mobile) 
            if (Input.GetMouseButton(0))
            {
                horizontalSpeed = CalculateMovement(Input.mousePosition);
            }

        // Check if we are running on a mobile device 
        #elif UNITY_IOS || UNITY_ANDROID

            if(horizMovement == MobileHorizMovement.Accelerometer)
            {
                // Move player based on direction of the accelerometer
                horizontalSpeed = Input.acceleration.x * dodgeSpeed;
            }

            //Check if Input has registered more than zero touches 
            if (Input.touchCount > 0)
            {
                if (horizMovement == MobileHorizMovement.ScreenTouch)
                {
                    //Store the first touch detected. 
                    Touch touch = Input.touches[0];
                    horizontalSpeed = CalculateMovement(touch.position);
                }
            }
        #endif

        rb.AddForce(horizontalSpeed, 0, rollSpeed);
    }

    /// <summary> 
    /// Update is called once per frame
    /// </summary> 
    private void Update()
    {
        // If the game is paused, don't do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }

        // Check if we are running on a mobile device 
        #if UNITY_IOS || UNITY_ANDROID
            //Check if Input has registered more than zero touches 
            if (Input.touchCount > 0)
            {
                //Store the first touch detected. 
                Touch touch = Input.touches[0];

                SwipeTeleport(touch);

                TouchObjects(touch);

                ScalePlayer();
            }
        #endif
    }

    /// <summary> 
    /// Will teleport the player if swiped to the left or right 
    /// </summary> 
    /// <param name="touch">Current touch event</param> 
    private void SwipeTeleport(Touch touch)
    {
        // Check if the touch just started 
        if (touch.phase == TouchPhase.Began)
        {
            // If so, set touchStart 
            touchStart = touch.position;
        }

        // If the touch has ended 
        else if (touch.phase == TouchPhase.Ended)
        {
            // Get the position the touch ended at 
            Vector2 touchEnd = touch.position;

            // Calculate the difference between the beginning and 
            // end of the touch on the x axis. 
            float x = touchEnd.x - touchStart.x;

            // If we are not moving far enough, don't do the teleport
            if (Mathf.Abs(x) < minSwipeDistancePixels)
            {
                return;
            }

            Vector3 moveDirection;

            // If moved negatively in the x axis, move left 
            if (x < 0)
            {
                moveDirection = Vector3.left;
            }
            else
            {
                // Otherwise we're on the right 
                moveDirection = Vector3.right;
            }

            RaycastHit hit;

            // Only move if we wouldn't hit something 
            if (!rb.SweepTest(moveDirection, out hit, swipeMove))
            {
                // Move the player 
                rb.MovePosition(rb.position + (moveDirection *
                                swipeMove));
            }
        }
    }

    /// <summary>
    /// Will change the player's scale via pinching and stretching two 
    /// touch events
    /// </summary>
    private void ScalePlayer()
    {
        // We must have two touches to check if we are scaling the object
        if (Input.touchCount != 2)
        {
            return;
        }
        else
        {
            //Store the touchs detected. 
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            // Find the position in the previous frame of each touch.
            Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
            Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

            // Find the the distance (or magnitude) between the touches 
            // in each frame.
            float prevTouchDeltaMag = (touch0Prev - touch1Prev).magnitude;

            float touchDeltaMag = (touch0.position - 
                                   touch1.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Keep the change consistent no matter what the framerate is
            float newScale = currentScale - (deltaMagnitudeDiff 
                                             * Time.deltaTime);
            
            // Ensure that it is valid
            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            // Update the player's scale
            transform.localScale = Vector3.one * newScale;

            // Set our current scale for the next frame
            currentScale = newScale;

        }
    }

    /// <summary> 
    /// Will figure out where to move the player horizontally 
    /// </summary> 
    /// <param name="pixelPos">The position the player has 
    /// touched/clicked on</param> 
    /// <returns>The direction to move in the x axis</returns> 
    float CalculateMovement(Vector3 pixelPos)
    {
        // Converts to a 0 to 1 scale 
        var worldPos = Camera.main.ScreenToViewportPoint(pixelPos);

        float xMove = 0;

        // If we press the right side of the screen 
        if (worldPos.x < 0.5f)
        {
            xMove = -1;
        }
        else
        {
            // Otherwise we're on the left 
            xMove = 1;
        }

        // replace horizontalSpeed with our own value 
        return xMove * dodgeSpeed;
    }

    /// <summary> 
    /// Will determine if we are touching a game object and if so 
    /// call events for it 
    /// </summary> 
    /// <param name="touch">Our touch event</param> 
    private static void TouchObjects(Touch touch)
    {
        // Convert the position into a ray 
        Ray touchRay = Camera.main.ScreenPointToRay(touch.position);

        RaycastHit hit;

        // Create a LayerMask that will collide with all possible channels
        int layerMask = ~0;

        // Are we touching an object with a collider? 
        if (Physics.Raycast(touchRay, out hit, Mathf.Infinity, 
                            layerMask, QueryTriggerInteraction.Ignore))
        {
            // Call the PlayerTouch function if it exists on a 
            // component attached to this object 
            hit.transform.SendMessage("PlayerTouch",
                                SendMessageOptions.DontRequireReceiver);
        }
    }

}