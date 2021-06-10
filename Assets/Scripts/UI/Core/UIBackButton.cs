using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OnlineFPS
{
    public class UIBackButton : MonoBehaviour,
                                IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) =>
            UIManager.Instance.OnBackButtonClicked();
    }
}