using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{ 
    public enum CanvasSignature
    {
        NONE = -1,
        MAIN_MENU = 0,
        SETTINGS
    }

    public class UICanvas : MonoBehaviour
    {
        [SerializeField] CanvasSignature canvasSignature;
        [SerializeField] WindowSignature defaultWindowSignature;

        WindowSignature activeWindowSignature;
        Dictionary<WindowSignature, UIWindow> childWindows;

        Canvas canvas;

        public CanvasSignature CanvasSignature => canvasSignature;
        public WindowSignature ActiveWindowSignature => activeWindowSignature;

        public virtual void Init()
        {
            canvas = GetComponent<Canvas>();

            childWindows = new Dictionary<WindowSignature, UIWindow>();

            UIWindow[] windows = GetComponentsInChildren<UIWindow>();

            for (int i = 0; i < windows.Length; ++i)
            {
                childWindows.Add(windows[i].WindowSignature, windows[i]);
                windows[i].Init();
                windows[i].Close();
            }
        }

        public void Open()
        {
            OpenWithoutShow();
            Show();
        }

        public void OpenWithoutShow()
        {
            PrepareToOpen();
            activeWindowSignature = defaultWindowSignature;
            OpenWindow(defaultWindowSignature);
        }

        protected virtual void PrepareToOpen()
        {

        }

        public void OpenWindow(WindowSignature windowSignature)
        {
            CloseWindow(activeWindowSignature);
            childWindows[windowSignature].Open();
            activeWindowSignature = windowSignature;
        }

        public void Show() =>
            canvas.enabled = true;

        public void Close()
        {
            PrepareToClose();
            childWindows[activeWindowSignature].Close();
            Hide();
        }
        
        protected virtual void PrepareToClose()
        {

        }

        private void CloseWindow(WindowSignature windowSignature) =>
           childWindows[windowSignature].Close();

        public void Hide() =>
            canvas.enabled = false;
    }
}