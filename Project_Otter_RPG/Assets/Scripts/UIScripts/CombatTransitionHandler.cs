using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CombatTransitionHandler : MonoBehaviour
{
    [SerializeField] private Material combatTransitionMat;

    [SerializeField] private float transitionTime = 1.0f;

    [SerializeField] private string propertyName = "_progress";

    public UnityEvent OnTransitionDone;

    public IEnumerator Transition()
    {
        float currentTime = 0.0f;
        float propertyValue = 1.0f;

        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            combatTransitionMat.SetFloat(propertyName, Mathf.Clamp01( (propertyValue - currentTime) / transitionTime));
        }
        propertyValue = 0.0f;
        OnTransitionDone?.Invoke();
        new WaitForSeconds(0.1f);

        while (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            combatTransitionMat.SetFloat(propertyName, Mathf.Clamp01((propertyValue + currentTime) / transitionTime));
        }
        propertyValue = 1.0f;

        yield return null;
    }
}
