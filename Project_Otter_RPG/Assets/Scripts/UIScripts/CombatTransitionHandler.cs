using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CombatTransitionHandler : MonoBehaviour
{
    [SerializeField] private Material combatTransitionMat;

    [SerializeField] private GameObject testEnemy;

    [SerializeField] private float transitionTime = 2.0f;

    [SerializeField] private string propertyName = "_progress";

    public UnityEvent OnTransitionDone;

    public IEnumerator Transition()
    {
        float currentTime = transitionTime;

        while (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            combatTransitionMat.SetFloat(propertyName, Mathf.Clamp(currentTime, 0, transitionTime));
            yield return null;
        }

        Mathf.Clamp01(currentTime);
        testEnemy.GetComponent<OverworldEnemy>().SwitchToCombat();
        yield return new WaitForSeconds(0.2f);

        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            combatTransitionMat.SetFloat(propertyName, Mathf.Clamp(currentTime, 0, transitionTime));
            yield return null;
        }

        yield return null;
    }
}
