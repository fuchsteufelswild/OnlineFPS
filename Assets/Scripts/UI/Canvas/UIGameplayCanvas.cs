using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameplayCanvas : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_ANDROID
        ;
#else
        gameObject.SetActive(false);
#endif

    }

    public void FireButtonEvent()
    {
        EventManager.NotifyEvent(GameplayInputEvents.ON_FIRE_BUTTON_CLICKED);
    }

    public void ReloadButtonEvent()
    {
        EventManager.NotifyEvent(GameplayInputEvents.ON_RELOAD_BUTTON_CLICKED);
    }

    public void HandgunEvent()
    {
        EventManager.NotifyWeaponChangeEvent(0);
    }

    public void AutomaticRifleEvent()
    {
        EventManager.NotifyWeaponChangeEvent(1);
    }
}
