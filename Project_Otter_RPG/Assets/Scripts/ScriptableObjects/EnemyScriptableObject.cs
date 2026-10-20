using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "Scriptable Objects/EnemyScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Enemy Name")]
    public string enemyName = "";

    [Header("Enemy Health")]
    public int enemyMaxHealth = 0;
    [HideInInspector] public int enemyCurrentHealth = 0;

    [Header("Enemy Movement")]
    public Vector2 movementTimes = Vector2.zero;

    [Header("Attack-to-Move Ratio")]
    [Tooltip("The higher the value, the more likey the enemy is to attack")]
    [Range(0.001f, 1.0f)]
    public float attackRate = 0.5f;

    [Header("Enemy Attacks")]
    [SerializeField] public List<MoveData> moveList = new List<MoveData>();

    [Header("Enemy Weight Value")]
    [SerializeField] public int weight = 5;
    [SerializeField] public int weightDecreaseValue = 1;
}
