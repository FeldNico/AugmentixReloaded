/*===============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class HelpMenu : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    public bool displayOnLoad;
    #endregion // PUBLIC_MEMBERS

    #region PRIVATE_MEMBERS
    GestureRecognizer gestureRecognizer;
    AudioSource audioButtonPress;
    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        this.audioButtonPress = FindObjectOfType<AudioSource>();

        this.gestureRecognizer = new GestureRecognizer();
        GestureSettings gestureSettings = this.gestureRecognizer.GetRecognizableGestures();
        gestureSettings |= GestureSettings.DoubleTap;
        this.gestureRecognizer.SetRecognizableGestures(gestureSettings);
        SetupGestureEvents();
        this.gestureRecognizer.StartCapturingGestures();

        if (!this.displayOnLoad)
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        this.gestureRecognizer.StopCapturingGestures();
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS
    void SetupGestureEvents()
    {
        this.gestureRecognizer.Tapped += (args) =>
        {
            if (args.tapCount > 1)
            {
                Debug.Log("Double-Air-Tap Recognized");
                LoadingScreen.SceneToLoad = "1-Menu";
                LoadingScreen.Run();
            }
            else
            {
                Debug.Log("Air-Tap Recognized");
                if (gameObject.activeInHierarchy)
                {
                    // Play audio before game object is deactivated
                    this.audioButtonPress.Play();
                    gameObject.SetActive(!gameObject.activeInHierarchy);
                }
                else
                {
                    // Play audio after game object is actived
                    gameObject.SetActive(!gameObject.activeInHierarchy);
                    this.audioButtonPress.Play();
                }
            }
        };
    }
    #endregion // PRIVATE_METHODS
}
