using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Google.Protobuf.WellKnownTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static BattleManager;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyScriptableObject baseEnemyData;
    private EnemyScriptableObject instanceEnemyData;

    private GridManager gridManager;

    private KeyValuePair<int, GameObject> enemyTile;

    private List<GameObject> attackTiles = new List<GameObject>();
    private List<int> tilesToMoveToo = new List<int>();
    private List<MoveData> moves = new List<MoveData>();
    private List<MoveData> chosenMove = new List<MoveData>();
    private bool attackVisualized = false;
    private int tileAttackAddition;

    private List<ActionTypes> enemyActionTypes = new List<ActionTypes>();

    private float moveTime = 0.25f;
    private int lowestValueTile = 1000;
    private int enemyActionCount = 0;
    private GameObject tileToMoveTo;

    private float baseDecisionValue = 10000.0f;
    private float moveMultiplier;
    private float attackMultiplier;

    private List<int> pathwayKeys = new List<int>();

    private void Awake()
    {
        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();

        instanceEnemyData = Instantiate(baseEnemyData);

        instanceEnemyData.enemyCurrentHealth = instanceEnemyData.enemyMaxHealth;

        moves = instanceEnemyData.moveList;

        moveMultiplier = instanceEnemyData.attackRate;
        attackMultiplier = (1.0f - instanceEnemyData.attackRate);

        StartSpawn();
    }

    private void Start()
    {
        
    }

    // Places the enemy on a random spot on their grid
    private void StartSpawn()
    {
        GameObject startSpawn;
        int randomNumber;
        do
        {
            randomNumber = Random.Range(1, (gridManager.GetEnemyGridHeight() * gridManager.GetEnemyGridHeight()));
            startSpawn = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[randomNumber];
        } while (startSpawn.GetComponent<Tile>().GetCharacterOn());
        this.gameObject.transform.position = new Vector3(startSpawn.transform.position.x, startSpawn.transform.position.y, -1.0f);
        startSpawn.GetComponent<Tile>().SetCharacterOn(true);
        startSpawn.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);
        GraphBehavior.ChangeWeights(gridManager.FindTileKey(startSpawn, false), false, instanceEnemyData.weight ,instanceEnemyData.weightDecreaseValue, ref this.lowestValueTile);
        enemyTile = new KeyValuePair<int, GameObject>(randomNumber, startSpawn);
    }

    public float DetermineAttackWeightModifier()
    {
        KeyValuePair<MoveData, int> highestWeight = new KeyValuePair<MoveData, int>();
        foreach (var move in moves)
        {
            GraphBehavior.GetEnemyAttackWeight(move.centerTileKey, (16 - (move.rightMostTileKey - move.leftMostTileKey)), move.tileKeys, out int weight, out int addToTile);
            if (highestWeight.Key != move)
            {
                highestWeight = new KeyValuePair<MoveData, int>(move, weight);
                tileAttackAddition = addToTile;
            }
        }
        chosenMove.Add(highestWeight.Key);
        return (baseDecisionValue - (attackMultiplier * highestWeight.Value));
    }

    public float DetermineMoveWeightModifier()
    {
        tilesToMoveToo.Clear();
        tilesToMoveToo = new List<int>();
        Queue<GameObject> thePath = new Queue<GameObject>();
        Dictionary<int, GameObject> enemyGrid = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary();
        

        if (enemyGrid.TryGetValue(enemyTile.Key - 4, out GameObject tileObjectLeft))
        {
            tilesToMoveToo.Add(enemyTile.Key - 4);
        }
        if (enemyGrid.TryGetValue(enemyTile.Key - 1, out GameObject tileObjectDown) && (this.enemyTile.Key - 1) % 4 != 0)
        {
            tilesToMoveToo.Add(enemyTile.Key - 1);
        }
        if (enemyGrid.TryGetValue(enemyTile.Key + 1, out GameObject tileObjectUp) && (this.enemyTile.Key % 4) != 0)
        {
            tilesToMoveToo.Add(enemyTile.Key + 1);
        }
        if (enemyGrid.TryGetValue(enemyTile.Key + 4, out GameObject tileObjectRight))
        {
            tilesToMoveToo.Add(enemyTile.Key + 4);
        }

        gridManager.ResetEnemyTileWeight();

        foreach (var enemy in BattleManager.GetInstance().GetEnemyList())
        {
            if (enemy != this.gameObject)
            {
                GraphBehavior.ChangeWeights(enemy.GetComponent<Enemy>().GetEnemyTileData().Key, false, instanceEnemyData.weight, instanceEnemyData.weightDecreaseValue, ref lowestValueTile);
            }
        }

        foreach (var enemy in BattleManager.GetInstance().GetEnemyList())
        {
            if (enemy.gameObject != this.gameObject)
            {
                foreach (var tile in tilesToMoveToo)
                {
                    GraphBehavior.GetEnemyMoveWeight(BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[tile], BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[lowestValueTile], out Queue<GameObject> path);
                    thePath = path;
                }
            }
        }
        tileToMoveTo = thePath.Dequeue();
        tileToMoveTo.GetComponent<Tile>().SetCharacterOn(true);
        tileToMoveTo.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);
        return baseDecisionValue - (moveMultiplier * tileToMoveTo.GetComponent<Tile>().GetTileWeight());

    }

    public void MoveEnemyOnGrid()
    {
        if (enemyActionCount > 0)
        {
            // Dictionary of enemy Tiles
            Dictionary<int, GameObject> enemyTiles = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary();
            
            // Moves the enemy object to the desired tile
            Vector3 endPosition = new Vector3(tileToMoveTo.transform.position.x, tileToMoveTo.gameObject.transform.position.y, -1.0f);
            transform.DOMove(endPosition, moveTime).SetUpdate(UpdateType.Fixed);
            enemyTile.Value.gameObject.GetComponent<Tile>().SetCharacterOn(false);
            enemyTile.Value.gameObject.GetComponent<Tile>().SetCharacterOnTile(null);
            lowestValueTile = 1000;
        }
    }

    public void visualizeAttack()
    {
        if (enemyActionTypes.Count != 0 && enemyActionTypes[0] == BattleManager.ActionTypes.ATTACK && !attackVisualized)
        {

            if (attackTiles.Count == 0)
            {
                foreach (var key in chosenMove[0].tileKeys)
                {
                    GameObject tile = BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[key + tileAttackAddition];
                    attackTiles.Add(tile);
                }
            }

            foreach (var tile in attackTiles)
            {
                tile.gameObject.GetComponent<Image>().color = Color.orange;
            }
        }
    }

    public bool Attack()
    {
        bool hitPlayableCharacter = false;
        foreach (var tile in attackTiles)
        {
            if (tile.GetComponent<Tile>().GetCharacterOn())
            {
                tile.GetComponent<Tile>().GetCharacterOnTile().GetComponent<PlayerCombat>().TakeDamage(chosenMove.FirstOrDefault().attackDamage);
                hitPlayableCharacter = true;
            }
        }
        attackTiles = new List<GameObject>();
        attackVisualized = false;
        chosenMove.RemoveAt(chosenMove.IndexOf(chosenMove.FirstOrDefault()));

        if (hitPlayableCharacter)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Death()
    {
        if (instanceEnemyData.enemyCurrentHealth <= 0)
        {
            enemyTile.Value.gameObject.GetComponent<Tile>().SetCharacterOn(false);
            enemyTile.Value.gameObject.GetComponent<Tile>().SetCharacterOnTile(null);
            Destroy(this.gameObject);
        }
    }

    public EnemyScriptableObject GetEnemyScriptableObject()
    {
        return instanceEnemyData;
    }

    // Gets the enemy action count
    public int GetEnemyActionCount()
    {
        return enemyActionCount;
    }

    // Sets the enemy action count
    public void SetEnemyActionCount(int actionCount)
    {
        enemyActionCount += actionCount;
    }

    private void SetChosenMove()
    {
        int moveKey = Random.Range(0, moves.Count - 1);
        chosenMove.Add(moves[moveKey]);
    }

    public void SetAttackVisualized(bool truthValue)
    {
        attackVisualized = truthValue;
    }

    public List<GameObject> GetAttackTiles()
    {
        return attackTiles;
    }

    public void SetEnemyActionTypes(BattleManager.ActionTypes action)
    {
        enemyActionTypes.Add(action);
    }

    public List<BattleManager.ActionTypes> GetEnemyActionTypes()
    {
        return enemyActionTypes;
    }

    public KeyValuePair<int, GameObject> GetEnemyTileData()
    {
        return enemyTile;
    }

    public int GetTileKeyToMoveTo()
    {
        return lowestValueTile;
    }
}
