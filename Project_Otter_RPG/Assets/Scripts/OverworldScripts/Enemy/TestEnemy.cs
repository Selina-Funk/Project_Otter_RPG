using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EnemyAI;
using System.Runtime.CompilerServices;
using Unity.Hierarchy;

public class TestEnemy : MonoBehaviour
{
    private GridManager gridManager;
    private New_Graph connectionGraph = new New_Graph();
    private Tile currentTile;
    private Tile endTile;

    private IEnemyState currentState;
    private MovementState movementState;
    private AttackState attackState;
    
    private bool canMove = false;
    public bool canAttack = false;
    public bool attackDebug = false;

    private float elapsedTime = 0.0f;
    private float timeToMove = 3.0f;
    private float timeToAttack = 5.0f;

    private void Awake()
    {
        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();
        movementState = new MovementState();
        attackState = new AttackState();
        SetEnemyState(movementState);
    }

    private void Start()
    {
        connectionGraph.ConnectEnemyTiles();
        connectionGraph.ConnectPlayerTiles();
        StartSpawn();
    }

    private void Update()
    {
        currentState.Update();
        if (!canAttack && attackDebug)
        {
            attackDebug = canAttack;
            SetEnemyState(movementState);
            StartCoroutine(GoThroughPath(1.0f));
        }
    }

    private void StartSpawn()
    {
        GameObject startSpawn;
        Vector2Int location = new Vector2Int(0,0);
        BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary().TryGetValue(1, out GameObject spawn);
        startSpawn = spawn;

        location = startSpawn.GetComponent<Tile>().GetPosition();
        this.gameObject.transform.position = new Vector3(startSpawn.transform.position.x, startSpawn.transform.position.y, startSpawn.transform.position.z);
        this.gameObject.transform.rotation = startSpawn.transform.rotation;
        startSpawn.GetComponent<Tile>().SetCharacterOn(true);
        startSpawn.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);
        currentTile = startSpawn.GetComponent<Tile>();
        int randomNumber = UnityEngine.Random.Range(16, 16);
        endTile = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[randomNumber].gameObject.GetComponent<Tile>();
        StartCoroutine(GoThroughPath(1.0f));
        }

    public IEnumerator GoThroughPath(float duration)
    {
        if(canMove)
        {
            yield return new WaitForSeconds(duration);
            List<Tile> path = connectionGraph.AstarMove(currentTile, endTile);
            if (path.Count > 0)
            {
                currentTile = path.LastOrDefault();
                this.gameObject.transform.position = currentTile.gameObject.transform.position;
                StartCoroutine(GoThroughPath(duration));
            }
            else
            {
                Debug.Log("ALL DONE");
            }
        }
        else
        {
            SetEnemyState(attackState);
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
}
