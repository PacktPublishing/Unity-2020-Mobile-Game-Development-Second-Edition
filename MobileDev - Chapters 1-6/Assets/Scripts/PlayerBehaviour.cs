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


    [Tooltip("How far must the player swipe before we will execute the action (in pixel space)")]
    public float minSwipeDistance = 2f;

    /// <summary> 
    /// Stores the starting position of mobile touch events 
    /// </summary> 
    private Vector2 touchStart;

    // Start is called before the first frame update
    void Start()
    {
        // Get access to our Rigidbody component 
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is paused, don't do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }
            

        float horizontalSpeed = 0;

//Check if we are running either in the Unity editor or in a   
//standalone build. 
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
 
        // Check if we're moving to the side 
        horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

        // If the mouse is held down (or the screen is tapped  
        // on Mobile) 
        if (Input.GetMouseButton(0)) 
        { 
            horizontalSpeed = CalculateMovement(Input.mousePosition); 
        }

        //Check if we are running on a mobile device 
#elif UNITY_IOS || UNITY_ANDROID

        if(horizMovement == MobileHorizMovement.Accelerometer)
        {
            // Move player based on direction of the accelerometer
            horizontalSpeed = Input.acceleration.x * dodgeSpeed;
        }

        //Check if Input has registered more than zero touches 
        if (Input.touchCount > 0)
        {
            //Store the first touch detected. 
            Touch touch = Input.touches[0];

            if(horizMovement == MobileHorizMovement.ScreenTouch)
            {
                horizontalSpeed = CalculateMovement(touch.position); 
            }

            SwipeTeleport(touch); 

            TouchObjects(touch);
        }

#endif

        var movementForce = new Vector3(horizontalSpeed,
                                        0, 
                                        rollSpeed);

        // Time.deltaTime is the amount of time since the  
        // last frame (approx. 1/60 seconds) 
        movementForce *= (Time.deltaTime * 60);

        // Apply our auto-moving and movement forces 
        rb.AddForce(movementForce);
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
            if (Mathf.Abs(x) < minSwipeDistance)
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
    /// Will determine if we are touching a game object and if so  
    /// call events for it 
    /// </summary> 
    /// <param name="touch">Our touch event</param> 
    private static void TouchObjects(Touch touch)
    {
        // Convert the position into a ray 
        Ray touchRay = Camera.main.ScreenPointToRay(touch.position);

        RaycastHit hit;

        // Are we touching an object with a collider? 
        if (Physics.Raycast(touchRay, out hit))
        {
            // Call the PlayerTouch function if it exists on a  
            // component attached to this object 
            hit.transform.SendMessage("PlayerTouch",
                              SendMessageOptions.DontRequireReceiver);
        }
    }
}
