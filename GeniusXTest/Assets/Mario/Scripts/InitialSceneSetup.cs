using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

/// <summary>
/// For some reason even mirror's run in background setting isn't enough for dissonance, probably execution order, so I set it up here beforehand
/// also took care of android's mic permissions
/// </summary>
public class InitialSceneSetup : MonoBehaviour
{

    bool wentToNextScene = false;

    public void Start()
    {
        Application.runInBackground = true;
    }
    void Update()
    {
        if (!wentToNextScene)
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
            else
            {
                wentToNextScene = true;
                SceneManager.LoadScene(1);
                
            }
#else
            wentToNextScene = true;
            SceneManager.LoadScene(1);
#endif
        }

    }
}
