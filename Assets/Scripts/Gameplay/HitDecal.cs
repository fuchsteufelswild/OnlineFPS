using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDecal : MonoBehaviour
{
    public static Vector3 defaultPosition = Vector3.one * 100;

    public bool IsInUse { get; private set; } = false;

    public void InitAt(Vector3 position, Vector3 normal)
    {
        IsInUse = true;
        transform.forward = normal;
        transform.position = position;
        Invoke("PutBackIntoBag", 4.0f);
    }

    public void PutBackIntoBag()
    {
        transform.position = defaultPosition;
        IsInUse = false;
    }
}
