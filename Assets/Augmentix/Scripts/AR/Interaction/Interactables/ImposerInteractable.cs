using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;

public class ImposerInteractable : AbstractInteractable
{
    private GameObject Object;

    // Start is called before the first frame update
    void Start()
    {
        OnInteractionStart += hand =>
        {
            if (Object == null)
            {
                Debug.LogError("Object not set");
                return;
            }

            var obj = PhotonNetwork.Instantiate(Object.name, transform.position, transform.rotation,
                (byte) TargetManager.Groups.PLAYERS);

            hand.CurrentInteractable = obj.GetComponent<AbstractInteractable>();
        };
    }
}
