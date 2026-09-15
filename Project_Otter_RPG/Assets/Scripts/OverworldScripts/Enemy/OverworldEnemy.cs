using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    private GameObject attackCanvas;

    private void Awake()
    {
        attackCanvas = GameObject.Find("Attack_Canvas");
    }
    private void OnTriggerEnter(Collider other)
    {
        attackCanvas.SetActive(true);
    }
}
