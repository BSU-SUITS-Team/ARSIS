using ARSIS.EventManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using MixedReality.Toolkit.UX;
using UnityEngine.Rendering.VirtualTexturing;

public class NotificationDisplayManager : MonoBehaviour, IRenderable
{
    private Boolean changed = true;
    private List<BaseArsisEvent> data = new();
    [SerializeField]
    public GameObject MainNotifObj;
    public GameObject mainParentObject;

    private float cooldownTimer = 0f;
    private float cooldownDuration = 5f; // Cooldown duration in seconds

    public void Render(List<BaseArsisEvent> data)
    {
        this.data = data;
        changed = true;
    }
    void Start()
    {
        EventDatastore eventDatastore = EventDatastore.Instance;
        eventDatastore.AddHandler("notification", this);
    }

    void Update()
    {
        // Update the cooldown timer
        cooldownTimer += Time.deltaTime;

        // Check if the cooldown duration has passed
        if (cooldownTimer >= cooldownDuration)
        {
            // Reset the cooldown timer
            cooldownTimer = 0f;

            // Check for new notifications
            CheckForNotifications();
        }
    }

    void CheckForNotifications()
    {
        foreach (BaseArsisEvent baseArsisEvent in data)
        {
            if (baseArsisEvent is ARSIS.EventManager.Notifications notification)
            {
                Debug.Log(notification.data.content);

                // Instantiate the prefab with mainParentObject as the parent
                GameObject mainNotifObj = Instantiate(MainNotifObj, mainParentObject.transform);
            }
        }
    }
}
