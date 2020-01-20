using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableInteractable : Interactable
{
    public override void OnPinchStay(ARHand hand)
    {
        transform.position = hand.GetPinchPosition();
    }
    
}
