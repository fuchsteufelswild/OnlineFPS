using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [SerializeField] private Joystick m_Joystick; // Used for mobile movement inputs

    // Used for mobile rotation inputs
    private bool m_MouseDown;
    private Vector3 m_InitialMousePosition;
    private Vector3 m_MoveDifference;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;

        // m_Joystick = FindObjectOfType<Joystick>();
    }

    public (float, float) MovementInput
    {
        get
        {
#if UNITY_ANDROID
            return (m_Joystick.Horizontal, m_Joystick.Vertical);
#else
            return (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif
        }
    }

    public float HorizontalRotationInput
    {
        get
        {
#if UNITY_ANDROID
            if(m_MouseDown) return m_MoveDifference.x;
                else return 0;
#else
            return Input.GetAxis("Mouse X");
#endif
        }
    }

    public float VerticalRotationInput
    {
        get
        {
#if UNITY_ANDROID
            if(m_MouseDown) return m_MoveDifference.y;
            else return 0;
            
#else
            return Input.GetAxis("Mouse Y");
#endif
        }
    }

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetMouseButtonDown(1))
        {
            m_InitialMousePosition = Input.mousePosition;
            m_MouseDown = true;
        }
        if (Input.GetMouseButtonUp(1))
            m_MouseDown = false;

        if (Input.GetMouseButton(1))
        {
            m_MoveDifference = Input.mousePosition - m_InitialMousePosition;
            m_InitialMousePosition = Input.mousePosition;
        }
#endif
    }
}
