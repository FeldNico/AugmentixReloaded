/*===============================================================================
Copyright (c) 2019 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;

public class Versions : MonoBehaviour
{
    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        var versions = string.Format(
            "Vuforia Version: {0}\nUnity Version: {1}",
            Vuforia.VuforiaUnity.GetVuforiaLibraryVersion(), Application.unityVersion);

        var textComponent = GetComponent<TextMesh>();
        if (textComponent)
        {
            textComponent.text = versions;
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS
}
