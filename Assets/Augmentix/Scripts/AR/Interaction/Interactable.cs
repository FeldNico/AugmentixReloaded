using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    public UnityAction<ARHand> OnPinchStart;
    public UnityAction<ARHand> OnPinchEnd;
    public bool IsGrabbed { private set; get; } = false;
    
    private IEnumerator _onPinchStayEnumerator(ARHand hand)
    {
        while (true)
        {
            OnPinchStay(hand);
            yield return new WaitForEndOfFrame();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        IEnumerator enumerator = null;
        
        OnPinchStart += hand =>
        {
            IsGrabbed = true;
            enumerator = _onPinchStayEnumerator(hand);
            StartCoroutine(enumerator);
        };
        OnPinchEnd += hand =>
        {
            IsGrabbed = false;
            if (enumerator != null)
                StopCoroutine(enumerator);
        };
    }
    
    abstract public void OnPinchStay(ARHand hand);
}
