/* 
 * Main center for event subscription, unsubscription, and notification.
 * Classes may listen to events and notify the listeners. There could only
 * be one signature for an event:
 * e.g eventName<int, string> != eventName<int, int>
 * In the case of multiple signatures, error is given.
 */

using System.Collections.Generic;
using System;
using UnityEngine;

public static class EventManager
{
    static Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();
    static Dictionary<string, Action> unparametrizedEvents = new Dictionary<string, Action>();

    #region Predefined Dynamic Events
    public static void NotifyWeaponChangeEvent(int weaponIndex)
    {
        if (events.ContainsKey(GameplayInputEvents.ON_WEAPON_CHANGE_BUTTON_CLICKED) && events[GameplayInputEvents.ON_WEAPON_CHANGE_BUTTON_CLICKED] != null)
        {
            ((Action<int>)events[GameplayInputEvents.ON_WEAPON_CHANGE_BUTTON_CLICKED])?.Invoke(weaponIndex);
        }
    }
    #endregion

    private static bool IsCompatible(Action a1, Action a2) =>
        a1.GetType() == a2.GetType();

    private static bool IsCompatible<T>(Action<T> a1, Action<T> a2) =>
        a1.GetType() == a2.GetType();

    private static bool IsCompatible<T, U>(Action<T, U> a1, Action<T, U> a2) =>
        a1.GetType() == a2.GetType();

    private static bool WasRegistered(string eventName)
    {
        bool result = events.ContainsKey(eventName);

        return result;
    }

    public static void AddListener(string eventName, Action action)
    {
        if (unparametrizedEvents.ContainsKey(eventName))
            unparametrizedEvents[eventName] += action;
        else
            unparametrizedEvents[eventName] = action;
    }

    public static void RemoveListener(string eventName, Action action)
    {
        unparametrizedEvents[eventName] -= action;
    }

    public static void NotifyEvent(string eventName)
    {
        if (!unparametrizedEvents.ContainsKey(eventName)) return;

        unparametrizedEvents[eventName]?.Invoke();
    }

    public static void AddListener<T>(string eventName, Action<T> action)
    {
        if (events.ContainsKey(eventName) && events[eventName] != null)
        {
            if (!IsCompatible((Action<T>)events[eventName], action))
            {
                Debug.LogError("Incompatible delegate types to combine");
                return;
            }

            events[eventName] = Delegate.Combine(events[eventName], action);
        }
        else
            events[eventName] = action;
    }

    public static void RemoveListener<T>(string eventName, Action<T> action)
    {
        if (!IsCompatible((Action<T>)events[eventName], action))
        {
            Debug.LogError("Incompatible delegate types to remove");
            return;
        }

        events[eventName] = (Action<T>)events[eventName] - action;
    }

    public static void NotifyEvent<T>(string eventName, T param1)
    {
        if (!WasRegistered(eventName)) return;

        events[eventName]?.DynamicInvoke(param1);
    }

    public static void AddListener<T, U>(string eventName, Action<T, U> action)
    {
        if (events.ContainsKey(eventName) && events[eventName] != null)
        {
            if (!IsCompatible((Action<T, U>)events[eventName], action))
            {
                Debug.LogError("Incompatible delegate types to combine");
                return;
            }
            events[eventName] = Delegate.Combine(events[eventName], action);
        }
        else
            events[eventName] = action;
    }

    public static void RemoveListener<T, U>(string eventName, Action<T, U> action)
    {
        if (!IsCompatible((Action<T, U>)events[eventName], action))
        {
            Debug.LogError("Incompatible delegate types to remove");
            return;
        }

        events[eventName] = (Action<T, U>)events[eventName] - action;
    }

    public static void NotifyEvent<T, U>(string eventName, T param1, U param2)
    {
        if (!WasRegistered(eventName)) return;

        Delegate[] listeners = events[eventName]?.GetInvocationList();

        foreach (Delegate listener in listeners)
            listener?.DynamicInvoke(param1, param2);
    }
}

public static class GameplayInputEvents
{
    public const string ON_FIRE_BUTTON_CLICKED = "ON_FIRE_BUTTON_CLICKED";
    public const string ON_RELOAD_BUTTON_CLICKED = "ON_RELOAD_BUTTON_CLICKED";
    public const string ON_WEAPON_CHANGE_BUTTON_CLICKED = "ON_WEAPON_CHANGE_BUTTON_CLICKED";
}
