using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace OnlineFPS
{
    public class UIErrorWindow : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] TextMeshProUGUI errorText;

        System.Action okButtonCallback;

        public void Open(string error, System.Action callback)
        {
            okButtonCallback = callback;

            errorText.text = error;
            canvas.enabled = true;
        }

        public void Close() =>
            canvas.enabled = false;

        public void OnOkButtonClicked()
        {
            okButtonCallback?.Invoke();
        }
    }

}