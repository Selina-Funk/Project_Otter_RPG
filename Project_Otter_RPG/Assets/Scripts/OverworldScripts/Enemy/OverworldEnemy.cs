using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(GameObject.Find("CombatTransitionManager").GetComponent<CombatTransitionHandler>().Transition());
    }

    public void SwitchToCombat()
    {
        GameObject.Find("OverworldPlayer").SetActive(false);
        GameObject.FindAnyObjectByType<BattleManager>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
}
