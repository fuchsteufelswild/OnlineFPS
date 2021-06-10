using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    public enum WindowSignature
    {
        NONE = -1,
        MENU = 0,
        SETTINGS,
        ROOM_LIST,
        ROOM
    }

    public class UIWindow : MonoBehaviour
    {
        [SerializeField] WindowSignature windowSignature;

        CanvasGroup canvasGroup;
        
        public WindowSignature WindowSignature => windowSignature;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Init()
        {
        }

        public virtual void Open()
        {
            PrepareToOpen();
            Show();
        }

        protected virtual void PrepareToOpen()
        {

        }

        private void Show()
        {
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Close()
        {
            PrepareToClose();
            Hide();
        }

        protected virtual void PrepareToClose()
        {

        }

        private void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}