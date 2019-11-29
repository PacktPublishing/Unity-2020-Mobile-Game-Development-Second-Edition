using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation; // ARRaycastManager
using UnityEngine.XR.ARSubsystems; // TrackableType

public class PlaceARObject : MonoBehaviour
{
    /// <summary>
    /// A reference to the Raycast Manager for being able to perform raycasts
    /// </summary>
    ARRaycastManager raycastManager;

    /// <summary>
    /// A reference to the AR camera tp know where to draw raycasts from
    /// </summary>
    Camera arCamera;

    [Tooltip("The object to spawn when the screen is tapped")]
    public GameObject objectToSpawn;

    /// <summary>
    /// Start is called before the first frame update. Initalize our
    /// private variables
    /// </summary>
    void Start()
    {
        raycastManager = GameObject.FindObjectOfType<ARRaycastManager>();
        arCamera = GameObject.FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Figure out where the center of the screen is
        var viewportCenter = new Vector2(0.5f, 0.5f);
        var screenCenter = arCamera.ViewportToScreenPoint(viewportCenter);

        // Check if there is something in front of the center of the screen
        // and update the placement indicator if needed
        UpdateIndicator(screenCenter);

        // If we tap on the screen, spawn an object
        if(Input.GetMouseButtonDown(0))
        {
            // Spawn the object above the floor to see it fall
            Vector3 objPos = transform.position + Vector3.up;

            Instantiate(objectToSpawn, objPos, transform.rotation);
        }
    }

    /// <summary>
    /// Will update the placement indicator's position and rotation
    /// to be on the floor of any plane surface
    /// </summary>
    /// <param name="screenPosition">A position in screen space</param>
    void UpdateIndicator(Vector2 screenPosition)
    {
        var hits = new List<ARRaycastHit>();

        raycastManager.Raycast(screenPosition,
                               hits,
                               TrackableType.Planes);

        // If there is at least one hit position
        if (hits.Count > 0)
        {
            // Get the pose data
            var placementPose = hits[0].pose;

            var camForward = arCamera.transform.forward;

            // We want the object to be flat
            camForward.y = 0;

            // Scale the vector be have a size of 1
            camForward = camForward.normalized;

            // Rotate to face in front of the camera
            placementPose.rotation = Quaternion.LookRotation(camForward);

            transform.SetPositionAndRotation(placementPose.position,
                                             placementPose.rotation);
        }
    }
}
