using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider),typeof(Rigidbody))]
public abstract class AbstractInteractable : MonoBehaviour
{
    public UnityAction<ARHand> OnInteractionStart;
    public UnityAction<ARHand> OnInteractionEnd;
    public UnityAction<ARHand> OnInteractionKeep;
    public bool IsInteractedWith { private set; get; } = false;
    
    private IEnumerator _onPinchStayEnumerator(ARHand hand)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (IsInteractedWith)
                OnInteractionKeep?.Invoke(hand);
        }
    }
    
    // Start is called before the first frame update
    public void Start()
    {
        IEnumerator enumerator = null;
        
        OnInteractionStart += hand =>
        {
            IsInteractedWith = true;
            enumerator = _onPinchStayEnumerator(hand);
            StartCoroutine(enumerator);
        };
        OnInteractionEnd += hand =>
        {
            IsInteractedWith = false;
            if (enumerator != null)
                StopCoroutine(enumerator);
        };
    }
}
