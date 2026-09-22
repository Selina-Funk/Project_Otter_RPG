using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject.Find("OverworldPlayer").SetActive(false);
        //GameObject.Find("OverworldPlayer").GetComponent<OverworldPlayerMovement>().DisableMovement();
        GameObject.FindAnyObjectByType<BattleManager>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
}
