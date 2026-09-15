using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject.FindAnyObjectByType<BattleManager>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
}
