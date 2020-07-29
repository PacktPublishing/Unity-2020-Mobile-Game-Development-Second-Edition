using UnityEngine;

public class UISafeAreaHandler : MonoBehaviour
{
    RectTransform panel;

    // Start is called before the first frame update
    private void Start()
    {
        panel = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void Update()
    {
        Rect area = Screen.safeArea;

        // Pixel size in screen space of the whole screen
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        //For testing purposes
        if (Application.isEditor && Input.GetButton("Jump"))
        {
            // Use the notch properties of the iPhone XS Max
            if (Screen.height > Screen.width)
            {
                // Portrait
                area = new Rect(0f, 0.038f, 1f, 0.913f);
            }
            else
            {
                // Landscape
                area = new Rect(0.049f, 0.051f, 0.902f, 0.949f);
            }

            panel.anchorMin = area.position;
            panel.anchorMax = (area.position + area.size);

            return;
        }

        // Set anchors to percentages of the screen used.
        panel.anchorMin = area.position / screenSize;
        panel.anchorMax = (area.position + area.size) / screenSize;

    }
}