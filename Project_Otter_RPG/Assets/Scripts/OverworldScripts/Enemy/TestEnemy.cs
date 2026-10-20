using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using EnemyAI;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TestEnemy : MonoBehaviour
{
    [Header("Base Enemy")]
    [SerializeField] EnemyScriptableObject baseEnemyData;
    private EnemyScriptableObject instanceEnemyData;
    private GridManager gridManager;

    [Header("Navigation Path")]
    private New_Graph connectionGraph = new New_Graph();
    private Tile currentTile;
    private Tile endTile;

    [Header("Enemy AI")]
    private IEnemyState currentState;
    private MovementState movementState;
    private AttackState attackState;
    
    private bool canMove = false;
    public bool canAttack = false;
    public bool attackDebug = false;

    private float elapsedTime = 0.0f;
    private float timeToMove = 3.0f;
    private float timeToAttack = 2.0f;

    [Header("Enemy Attack")]
    private MoveData chosenMove;
    private bool attackVisualized = false;
    private int tileAttackAddition;

    private void Awake()
    {
        instanceEnemyData = baseEnemyData;

        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();
        movementState = new MovementState();
        attackState = new AttackState();
        SetEnemyState(movementState);
    }

    private void Start()
    {
        connectionGraph.ConnectEnemyTiles(this);
        connectionGraph.ConnectPlayerTiles(this);
        StartSpawn();
    }

    private void Update()
    {
        currentState.Update();
        if (!canAttack && attackDebug)
        {
            attackDebug = canAttack;
            SetEnemyState(movementState);
            StartCoroutine(GoThroughPath(0.6f));
        }
    }

    private void StartSpawn()
    {
        GameObject startSpawn;
        Vector2Int location = new Vector2Int(0,0);
        int randomNumber;
        do
        {
            randomNumber = Random.Range(1, 16);
            BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary().TryGetValue(randomNumber, out GameObject spawn);
            startSpawn = spawn;
        } while (startSpawn.GetComponent<Tile>().GetCharacterOn());

        location = startSpawn.GetComponent<Tile>().GetPosition();
        this.gameObject.transform.position = new Vector3(startSpawn.transform.position.x, startSpawn.transform.position.y, startSpawn.transform.position.z);
        this.gameObject.transform.rotation = startSpawn.transform.rotation;
        startSpawn.GetComponent<Tile>().SetCharacterOn(true);
        startSpawn.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);
        currentTile = startSpawn.GetComponent<Tile>();
        currentTile.SetCharacterOn(true);
        GraphBehavior.ChangeWeights(gridManager.FindTileKey(startSpawn, false), false, instanceEnemyData.weight, instanceEnemyData.weightDecreaseValue);
        StartCoroutine(GoThroughPath(1.0f));
        }

    public IEnumerator GoThroughPath(float duration)
    {
        if(canMove)
        {
            yield return new WaitForSeconds(duration);
            endTile = getEndTile();
            List<Tile> path = connectionGraph.AstarMove(currentTile, endTile);
            if (path.Count > 0)
            {
                currentTile.SetCharacterOn(false);
                currentTile.SetCharacterOnTile(null);
                currentTile = path.LastOrDefault();
                currentTile.SetCharacterOn(true);
                currentTile.SetCharacterOnTile(this.gameObject);
                this.gameObject.transform.position = currentTile.gameObject.transform.position;
                StartCoroutine(GoThroughPath(duration));
            }
            else
            {
                SetEnemyState(attackState);
            }
        }
        else
        {
            SetEnemyState(attackState);
        }
    }

    private Tile getEndTile()
    {
        Tile endTile = null;
        foreach (var gameObject in gridManager.GetEnemyTileDictionary().Values)
        {
            if (endTile == null)
            {
                endTile = gameObject.GetComponent<Tile>();
            }

            if (endTile.GetTileWeight() < gameObject.GetComponent<Tile>().GetTileWeight())
            {
                endTile = gameObject.GetComponent<Tile>();
            }
        }

        return endTile;
    }

    public void ChoseMove()
    {
        int randomAttack = UnityEngine.Random.Range(0, instanceEnemyData.moveList.Count);
        chosenMove = instanceEnemyData.moveList[randomAttack];
    }

    public void TilesToAttack()
    {
        int tempWeight = 0;
        int totalWeight = 0;

        for (int i = 0; i < gridManager.GetPlayerTileDictionary().Count; i++)
        {
            foreach (var tile in chosenMove.tileKeys)
            {
                if ((tile + i) % 4 == 0 && (((tile + i) - 1) % 4 == 3 || (tile + i) + 1 % 4 == 1)) break;
                if (gridManager.GetPlayerTileDictionary().TryGetValue((tile + i), out GameObject cell))
                {
                    tempWeight = cell.GetComponent<Tile>().GetTileWeight();
                }
            }
            if (tempWeight > totalWeight)
            {
                tileAttackAddition = i;
                totalWeight = tempWeight;
            }
            tempWeight = 0;
        }
    }

    public void VisualizeAttack()
    {
        foreach (int tileKey in chosenMove.tileKeys)
        {
            gridManager.GetPlayerTileDictionary()[tileKey + tileAttackAddition].gameObject.GetComponent<Image>().color = Color.pink;
        }
    }

    public void Attack()
    {
        foreach (var tile in chosenMove.tileKeys)
        {
            GameObject tileObj = gridManager.GetPlayerTileDictionary()[tile + tileAttackAddition].gameObject;
            if (tileObj.GetComponent<Tile>().GetCharacterOn())
            {
                tileObj.GetComponent<Tile>().GetCharacterOnTile().GetComponent<PlayerCombat>().TakeDamage(chosenMove.attackDamage);
            }
        }
        
        attackVisualized = false;
    }

    public void UnvisualizeAttack()
    {
        foreach (int tileKey in chosenMove.tileKeys)
        {
            gridManager.GetPlayerTileDictionary()[tileKey + tileAttackAddition].gameObject.GetComponent<Image>().color = Color.green;
        }
    }

    private void SetEnemyState(IEnemyState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter(this);
    }

    public void SetCanMove(bool truthValue)
    {
        canMove = truthValue;
    }

    public float GetTimeToMove()
    {
        return timeToMove;
    }

    public float GetTimeToAttack()
    {
        return timeToAttack;
    }
    
    public void SetElapsedTime(float value)
    {
        elapsedTime = value;
    }

    public void AddToElapsedTime(float time)
    {
        elapsedTime += time;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public Tile GetCurrentTile()
    {
        return currentTile;
    }

    public EnemyScriptableObject GetEnemyData()
    {
        return instanceEnemyData;
    }

    public New_Graph GetConnectionGraph()
    {
        return connectionGraph;
    }
}
