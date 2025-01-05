using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintDetection : MonoBehaviour
{
    public LayerMask paint;
    public Transform cameraPos;
    public float pointerDistance;
    bool frontDetection;

    // Update is called once per frame
    void Update()
    {
        frontDetection = Physics.Raycast(cameraPos.position, cameraPos.forward, pointerDistance, paint);
        // Affiche le raycast dans la scène
        Debug.DrawRay(cameraPos.position, cameraPos.forward * pointerDistance, frontDetection ? Color.green : Color.red);
    }
}
