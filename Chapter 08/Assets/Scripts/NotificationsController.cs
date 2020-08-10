using UnityEngine;
using NotificationSamples; // GameNotificationManager
using System; // DateTime

public class NotificationsController : MonoBehaviour
{
    private GameNotificationsManager notificationsManager;

    private static bool addedReminder = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Get access to the notifications manager
        notificationsManager = GetComponent<GameNotificationsManager>();


        // Create a channel to use for it (required for Android)
        GameNotificationChannel channel = new GameNotificationChannel("channel0",
                                            "Default Channel", "Generic Notifications");

        // Initalize the manager so it can be used.
        notificationsManager.Initialize(channel);

        // Check if the notification hasn't been added yet
        if (!addedReminder)
        {
            // Remind the player to come back tomorrow to play the game
            ShowNotification("Endless Runner",
                             "Come back and try to beat your score!",
                             DateTime.Now.AddDays(1));

            // Cannot be added again until the user comes back
            addedReminder = true;
        }
    }

    public void ShowNotification(string title,
                                string body,
                                DateTime deliveryTime)
    {
        IGameNotification notification =
                                notificationsManager.CreateNotification();

        if (notification != null)
        {
            notification.Title = title;
            notification.Body = body;
            notification.DeliveryTime = deliveryTime;

            notificationsManager.ScheduleNotification(notification);
        }
    }

}
