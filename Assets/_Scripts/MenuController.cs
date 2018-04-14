using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    public Transform cameraStartLookAt;
    public Transform cameraEndLookAt;
    public Transform cam;

    void Start()
    {
        cam.LookAt(cameraStartLookAt);
        cam.DOLookAt(cameraEndLookAt.position, 10).SetLoops(-1, LoopType.Yoyo);
    }

    public void ButtonHandlerStartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void ButtonHandlerQuitGame()
    {
        Application.Quit();
    }
}
