using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OnlineFPS
{
    public class UITransitionButton : MonoBehaviour,
                                      IPointerClickHandler
    {
        [SerializeField] CanvasSignature canvasSignature;
        [SerializeField] WindowSignature windowSignature;

        Transitioner transitioner;

        private void Awake() =>
            transitioner = Transitioner.MakeTransitioner(canvasSignature, windowSignature);

        public void OnPointerClick(PointerEventData eventData) =>
            transitioner.FireEvent();

        private abstract class Transitioner
        {
            public static Transitioner MakeTransitioner(CanvasSignature canvasSignature, WindowSignature windowSignature)
            {
                if (canvasSignature != CanvasSignature.NONE && windowSignature != WindowSignature.NONE)
                    return new CanvasWindowTransitioner(canvasSignature, windowSignature);
                if (canvasSignature != CanvasSignature.NONE)
                    return new CanvasTransitioner(canvasSignature);
                if (windowSignature != WindowSignature.NONE)
                    return new WindowTransitioner(windowSignature);

                return null;
            }

            protected Transitioner()
            { }

            public abstract void FireEvent();
        }

        private class CanvasTransitioner : Transitioner
        {
            CanvasSignature canvasSignature;

            public CanvasTransitioner(CanvasSignature canvasSignature) : base() =>
                this.canvasSignature = canvasSignature;

            public override void FireEvent() =>
                UIManager.Instance.PerformCanvasTransition(canvasSignature);
        }

        private class WindowTransitioner : Transitioner
        {
            WindowSignature windowSignature;

            public WindowTransitioner(WindowSignature windowSignature) : base() =>
                this.windowSignature = windowSignature;

            public override void FireEvent() =>
                UIManager.Instance.PerformWindowTransition(windowSignature);
        }

        private class CanvasWindowTransitioner : Transitioner
        {
            CanvasSignature canvasSignature;
            WindowSignature windowSignature;

            public CanvasWindowTransitioner(CanvasSignature canvasSignature, WindowSignature windowSignature) : base()
            {
                this.canvasSignature = canvasSignature;
                this.windowSignature = windowSignature;
            }

            public override void FireEvent() =>
                UIManager.Instance.PerformCanvasWindowTransition(canvasSignature, windowSignature);
        }
    }
}
