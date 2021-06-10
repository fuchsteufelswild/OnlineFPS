using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    public class GameManager : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            UIManager.Instance.Init();
        }
    }
}