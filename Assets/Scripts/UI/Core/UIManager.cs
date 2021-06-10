using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [SerializeField] CanvasSignature defaultCanvasSignature;
        [SerializeField] Canvas loadingWindow;
        [SerializeField] UIErrorWindow errorWindow;
        
        UITransitionStack transitionStack;

        Dictionary<CanvasSignature, UICanvas> childCanvases;
        [SerializeField] CanvasSignature activeCanvasSignature; // serialize for debug

        UITransitionStack.UITransitionInfo reusedTransitionInfo;

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);

            Instance = this;
            
        }

        public void Init()
        {
            childCanvases = new Dictionary<CanvasSignature, UICanvas>();
            transitionStack = new UITransitionStack();

            UICanvas[] canvases = GetComponentsInChildren<UICanvas>();
            for(int i = 0; i < canvases.Length; ++i)
            {
                childCanvases.Add(canvases[i].CanvasSignature, canvases[i]);
                canvases[i].Init();
                canvases[i].Close();
            }

            activeCanvasSignature = defaultCanvasSignature;
            childCanvases[activeCanvasSignature].Open();
        }

        public void OpenLoadingWindow() =>
            loadingWindow.enabled = true;
        public void CloseLoadingWindow() =>
            loadingWindow.enabled = false;

        public void OpenErrorWindow(string error, System.Action callback) =>
            errorWindow.Open(error, callback);
        public void CloseErrorWindow() =>
            errorWindow.Close();

        public void OnBackButtonClicked()
        {
            if (!transitionStack.Empty)
            {
                UITransitionStack.UITransitionInfo transitionInfo = transitionStack.PopTransition();
                if (transitionInfo.IsCanvasWindowTransition)
                    CanvasWindowTransitionImpl(transitionInfo.canvas, transitionInfo.window);
                else if (transitionInfo.IsCanvasTransition)
                    CanvasTransitionImpl(transitionInfo.canvas);
                else if (transitionInfo.IsWindowTransition)
                    WindowTransitionImpl(transitionInfo.window);
            }
        }

        public void PerformWindowTransition(WindowSignature windowSignature)
        {
            UICanvas activeCanvas = childCanvases[activeCanvasSignature];

            reusedTransitionInfo.window = activeCanvas.ActiveWindowSignature;
            reusedTransitionInfo.canvas = CanvasSignature.NONE;
            
            transitionStack.PushTransition(reusedTransitionInfo);

            WindowTransitionImpl(windowSignature);
        }

        private void WindowTransitionImpl(WindowSignature windowSignature)
        {
            UICanvas activeCanvas = childCanvases[activeCanvasSignature];
            activeCanvas.OpenWindow(windowSignature);

            CloseErrorWindow();
        }
            
        public void PerformCanvasTransition(CanvasSignature canvasSignature)
        {
            reusedTransitionInfo.window = WindowSignature.NONE;
            reusedTransitionInfo.canvas = activeCanvasSignature;

            transitionStack.PushTransition(reusedTransitionInfo);

            CanvasTransitionImpl(canvasSignature);
        }

        private void CanvasTransitionImpl(CanvasSignature canvasSignature)
        {
            UICanvas activeCanvas = childCanvases[activeCanvasSignature];
            activeCanvas.Close();
            activeCanvasSignature = canvasSignature;
            childCanvases[activeCanvasSignature].Open();

            CloseErrorWindow();
        }

        public void PerformCanvasWindowTransition(CanvasSignature canvasSignature, WindowSignature windowSignature)
        {
            UICanvas activeCanvas = childCanvases[activeCanvasSignature];
            reusedTransitionInfo.window = activeCanvas.ActiveWindowSignature;
            reusedTransitionInfo.canvas = activeCanvasSignature;

            transitionStack.PushTransition(reusedTransitionInfo);

            CanvasWindowTransitionImpl(canvasSignature, windowSignature);
        }

        private void CanvasWindowTransitionImpl(CanvasSignature canvasSignature, WindowSignature windowSignature)
        {
            UICanvas activeCanvas = childCanvases[activeCanvasSignature];

            activeCanvas.Close();
            activeCanvasSignature = canvasSignature;
            activeCanvas = childCanvases[activeCanvasSignature];
            activeCanvas.OpenWithoutShow();
            activeCanvas.OpenWindow(windowSignature);
            activeCanvas.Show();

            CloseErrorWindow();
        }
    }

    public class UITransitionStack
    {
        List<UITransitionInfo> transitionStack;

        public bool Empty =>
            transitionStack.Count == 0;

        public UITransitionInfo MostRecentTransition =>
            transitionStack.GetLastElement();

        public UITransitionStack() =>
            transitionStack = new List<UITransitionInfo>();

        public void PushTransition(UITransitionInfo transitionInfo) =>
            transitionStack.Add(transitionInfo);

        public UITransitionInfo PopTransition()
        {
            UITransitionInfo mostRecentTransition = MostRecentTransition;
            transitionStack.RemoveLast();

            return mostRecentTransition;
        }

        public struct UITransitionInfo
        {
            public CanvasSignature canvas;
            public WindowSignature window;

            public bool IsCanvasWindowTransition =>
                canvas != CanvasSignature.NONE && window != WindowSignature.NONE;

            public bool IsCanvasTransition =>
                canvas != CanvasSignature.NONE;

            public bool IsWindowTransition =>
                window != WindowSignature.NONE;
        }
    }

}