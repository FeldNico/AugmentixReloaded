/*===============================================================================
Copyright (c) 2019 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Vuforia;

public class VoiceCommands : MonoBehaviour
{
    #region PRIVATE_MEMBERS

    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    KeywordRecognizer keywordRecognizer;
    CanvasGroup canvasGroup;
    UnityEngine.UI.Text voiceKeywordText;
    Animator voiceKeywordAnimator;
    AudioSource audioSource;
    HelpMenu helpMenu;
    System.Action keywordAction;
    readonly string composite = "\"{0}\"";
    Camera cam;

    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Awake()
    {
        // Get reference to HelpMenu before it is optionally disabled in HelpMenu.Start();
        this.helpMenu = FindObjectOfType<HelpMenu>();
    }

    void Start()
    {
        this.cam = Camera.main;
        this.audioSource = FindObjectOfType<AudioSource>();

        this.canvasGroup = GetComponentInChildren<CanvasGroup>();
        this.voiceKeywordText = this.canvasGroup.GetComponentInChildren<UnityEngine.UI.Text>();
        this.voiceKeywordAnimator = GetComponentInChildren<Animator>();

        // Setup the Keyword Commands
        SetupKeywordCommands();
        this.keywordRecognizer = new KeywordRecognizer(this.keywords.Keys.ToArray());
        this.keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        this.keywordRecognizer.Start();
    }

    #endregion //MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    void SetupKeywordCommands()
    {
        this.keywords.Add("Reset", () => { Reset(); });
        this.keywords.Add("Show Menu", () => { ToggleMenu(true); });
        this.keywords.Add("Close Menu", () => { ToggleMenu(false); });
        this.keywords.Add("Main Menu", () => { LoadScene("1-Menu"); });
        this.keywords.Add("Image Targets", () => { LoadScene("3-ImageTargets"); });
        this.keywords.Add("Model Targets", () => { LoadScene("3-ModelTargets"); });
        this.keywords.Add("VuMarks", () => { LoadScene("3-VuMarks"); });
        this.keywords.Add("Quit", () => { Application.Quit(); });
        this.keywords.Add("Standard", () => { SelectModelTargetDataSetType("Standard"); });
        this.keywords.Add("Advanced", () => { SelectModelTargetDataSetType("Advanced"); });
        this.keywords.Add("Three Sixty", () => { SelectModelTargetDataSetType("Three Sixty"); });
    }

    void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (this.keywords.TryGetValue(args.text, out this.keywordAction))
        {
            Debug.Log("Voice Command: " + args.text);

            this.voiceKeywordText.text = string.Format(this.composite, args.text);
            ToastVoiceCommand();
        }
    }

    void ToastVoiceCommand()
    {
        var distanceFromCamera = 1.0f; // adjust to preferred distance
        var toastPosition = this.cam.transform.position + this.cam.transform.forward * distanceFromCamera;
        var toastRotation = Quaternion.LookRotation(this.cam.transform.forward);
        this.transform.localPosition = Vector3.zero; // clear any local offsets
        this.transform.localEulerAngles = Vector3.zero; // clear any local rotations
        this.transform.SetPositionAndRotation(toastPosition, toastRotation);
        this.audioSource.Play();
        this.voiceKeywordAnimator.ResetTrigger("Toast");
        this.voiceKeywordAnimator.SetTrigger("Toast");
        StartCoroutine(InvokeKeywordAction());
    }

    IEnumerator InvokeKeywordAction()
    {
        // provide time for toast to display
        yield return new WaitForSeconds(1);
        this.keywordAction.Invoke();
    }

    #endregion //PRIVATE_METHODS


    #region COMMAND_ACTION_METHODS

    void LoadScene(string sceneName)
    {
        LoadingScreen.SceneToLoad = sceneName;
        LoadingScreen.Run();
    }

    void ToggleMenu(bool show)
    {
        if (this.helpMenu)
        {
            this.helpMenu.gameObject.SetActive(show);
        }
    }

    void SelectModelTargetDataSetType(string modelTargetType)
    {
        ModelTargetsManager mtm = FindObjectOfType<ModelTargetsManager>();
        if (mtm)
        {
            switch (modelTargetType)
            {
                case "Standard":
                    mtm.SelectDataSetStandard(true);
                    break;
                case "Advanced":
                    mtm.SelectDataSetAdvanced(true);
                    break;
                case "Three Sixty":
                    mtm.SelectDataSet360(true);
                    break;
            }
        }
    }

    void Reset()
    {
        var objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objTracker != null && objTracker.IsActive)
        {
            objTracker.Stop();

            List<DataSet> activeDataSets = objTracker.GetActiveDataSets().ToList();

            foreach (DataSet dataset in activeDataSets)
            {
                // The VuforiaEmulator.xml dataset (used by GroundPlane) is managed by Vuforia.
                if (!dataset.Path.Contains("VuforiaEmulator.xml"))
                {
                    VLog.Log("white", "Deactivating: " + dataset.Path);
                    objTracker.DeactivateDataSet(dataset);
                    VLog.Log("white", "Activating: " + dataset.Path);
                    objTracker.ActivateDataSet(dataset);
                }
            }

            objTracker.Start();
        }
    }

    #endregion // COMMAND_ACTION_METHODS
}
