using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CameraCutsceneController : MonoBehaviour
{
    public Transform cameraStartLookAt;
    public Transform cameraEndLookAt;
    public Transform cam;

    void Start()
    {
        cam.LookAt(cameraStartLookAt);
        cam.DOLookAt(cameraEndLookAt.position, 10).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}
