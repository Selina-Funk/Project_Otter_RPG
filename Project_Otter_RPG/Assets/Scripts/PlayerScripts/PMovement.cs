using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class PMovement : PlayerManager
{
    private GridManager gridManager;
    [SerializeField] private Canvas attackCanvas;

    private KeyValuePair<int, GameObject> playerTile;
    Dictionary<int, GameObject> potentialMoveTiles = new Dictionary<int, GameObject>();

    private float moveTime = 0.25f;
    [SerializeField] private int playerActionCount = 0;
    private GameObject chosenMoveLocation;

    private enum DirectionToMoveTile
    {
        LEFT = 1,
        DOWN = 2,
        UP = 3,
        RIGHT = 4
    };

    public override void Init(PlayerSystems system)
    {
        // Gets the GridManager script
        base.Init(system);
        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();
        base.playerActions.Combat.AddTileMovement.performed += AddTileMovement;
    }

    public override void Tick()
    {
        base.Tick();
        Debug.Log("PLAYER ACTION COUNT: " + playerActionCount);
    }

    public override void FixedTick()
    {
        base.FixedTick();
        VisualizeMovement();
        BattleManager.GetInstance().VisualizeEnemyAttacks();
    }

    public void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        StartSpawn();
    }

    private void AddTileMovement(InputAction.CallbackContext context)
    {
        if (BattleManager.GetInstance().GetCanPerformActions() && BattleManager.GetInstance().GetPlayerActionTypesList()[0] == BattleManager.ActionTypes.MOVE)
        {
            if (context.control.name == PlayerManager.InputKeyNames.upArrow.ToString())
            {
                if(potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.UP))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.UP];
                }
            }
            else if (context.control.name == PlayerManager.InputKeyNames.downArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.DOWN))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.DOWN];
                }
            }
            else if (context.control.name == PlayerManager.InputKeyNames.rightArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.RIGHT))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.RIGHT];
                }
            }
            else if (context.control.name == PlayerManager.InputKeyNames.leftArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.LEFT))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.LEFT];
                }
            }
        }
    }

    private void StartSpawn()
    {
        // Places the player on a random tile
        int randomNumber = Random.Range(1, gridManager.GetPlayerTileDictionary().Count);
        Vector3 targetPosition = gridManager.GetPlayerTileDictionary()[randomNumber].gameObject.transform.position;
        GameObject startSpawn = BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[randomNumber];
        this.gameObject.transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        startSpawn.GetComponent<Tile>().SetCharacterOn(true);
        startSpawn.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);
        playerTile = new KeyValuePair<int, GameObject>(randomNumber, startSpawn.gameObject);

        GraphBehavior.ChangeWeights(gridManager.FindTileKey(startSpawn, true), true, 1);
    }

    public void VisualizeMovement()
    {
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count != 0 && BattleManager.GetInstance().GetPlayerActionTypesList()[0] == BattleManager.ActionTypes.MOVE)
        {
            if (potentialMoveTiles.Count == 0)
            {
                Dictionary<int, GameObject> playerTiles = BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary();
                int playerKey = playerTile.Key;

                // Adds the potential keys to a list of integers
                List<int> potentialKeys = new List<int>();
                potentialKeys.Add(playerKey - gridManager.GetPlayerGridWidth());

                if ((playerTile.Key - 1) % 4 != 0)
                {
                    potentialKeys.Add(playerKey - 1);
                }
                else
                {
                    potentialKeys.Add(0);
                }
                if (playerTile.Key % 4 != 0)
                {
                    potentialKeys.Add(playerKey + 1);
                }
                else
                {
                    potentialKeys.Add(0);
                }
                potentialKeys.Add(playerKey + gridManager.GetPlayerGridWidth());

                // Checks to see if each potential key is valid and adds the tile to the dictionary if the key exists
                int key = 1;
                foreach (var potentailKey in potentialKeys)
                {
                    if (playerTiles.TryGetValue(potentailKey, out GameObject tileObject))
                    {
                        potentialMoveTiles.Add(key, tileObject);
                    }
                    key++;
                }
            }

            foreach (var tile in potentialMoveTiles.Values)
            {
                tile.GetComponent<Image>().color = Color.magenta;
            }
        }
    }

    // Moves the player when a tile is clicked
    public void MovePlayerOnGrid()
    {
        transform.DOMove(chosenMoveLocation.transform.position, moveTime).SetUpdate(UpdateType.Fixed);
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();

        // Changing where the player is on the tiles
        playerTile.Value.gameObject.GetComponent<Tile>().SetCharacterOn(false);
        playerTile.Value.gameObject.GetComponent<Tile>().SetCharacterOnTile(null);
        chosenMoveLocation.gameObject.GetComponent<Tile>().SetCharacterOn(true);
        chosenMoveLocation.gameObject.GetComponent<Tile>().SetCharacterOnTile(this.gameObject);

        gridManager.ResetPlayerTileWeight();
        GraphBehavior.ChangeWeights(gridManager.FindTileKey(chosenMoveLocation.gameObject, true), true, 1);

        playerTile = new KeyValuePair<int, GameObject>(BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary().FirstOrDefault(x => x.Value == chosenMoveLocation.gameObject).Key, chosenMoveLocation.gameObject);
        playerActionCount--;

        List<Enemy> eList = BattleManager.GetInstance().GetEnemyList();
        foreach (var removeTile in potentialMoveTiles)
        {
            removeTile.Value.gameObject.GetComponent<Image>().color = Color.green;
            foreach (Enemy enemy in eList)
            {
                foreach (var eAttackTile in enemy.GetAttackTiles())
                {
                    if (removeTile.Value.gameObject == eAttackTile)
                    {
                        removeTile.Value.gameObject.GetComponent<Image>().color = Color.orange;
                        break;
                    }
                }
            }
        }
        potentialMoveTiles = new Dictionary<int, GameObject>();
    }

    public void MovePlayer()
    {
        // Removes the first action from the player action list if the player has moved
        if (chosenMoveLocation != null)
        {
            MovePlayerOnGrid();
            BattleManager.GetInstance().GetPlayerActionTypesList().RemoveAt(0);
            chosenMoveLocation = null;
        }
    }

    // Gets the Player Action Count
    public int getPlayerActionCount()
    {
        return playerActionCount;
    }

    // Sets the Player Action Count
    public void SetPlayerActionCount(int countValue)
    {
        playerActionCount += countValue;
    }

    public Dictionary<int, GameObject> GetPotentialMoveTiles()
    {
        return potentialMoveTiles;
    }

    private void OnDisable()
    {
        base.playerActions.Disable();
    }
}
