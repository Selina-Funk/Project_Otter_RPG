using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    // Variables for the base combat character
    [Header("Base Character")]
    private PlayableCharacterData characterData;
    [SerializeField] private static HealthBarUI healthBar;
    private PlayerActions playerActions;
    private CombatState combatState;
    private bool isDead = false;

    private static SpriteInstance spriteInstance;

    [Header("Grid and Canvas")]
    private GridManager gridManager;
    [SerializeField] private Canvas attackCanvas;

    [Header("Combat Movement")]
    private KeyValuePair<int, GameObject> playerTile;
    Dictionary<int, GameObject> potentialMoveTiles = new Dictionary<int, GameObject>();

    private float moveTime = 0.25f;
    [SerializeField] private int playerActionCount = 0;
    private GameObject chosenMoveLocation;

    [Header("Combat Attack")]
    [SerializeField] List<MoveData> moves = new List<MoveData>();
    private List<GameObject> attackTiles = new List<GameObject>();

    private List<MoveData> chosenMove = new List<MoveData>();
    private int tileAddition = 0;

    public enum CombatState
    {
        IDLE_COMBAT, THINKING_COMBAT, THINKING_COMBAT_END, ATTACK_COMBAT, HIT_COMBAT, DEATH_COMBAT
    }

    public enum InputKeyNames
    {
        upArrow,
        downArrow,
        rightArrow,
        leftArrow
    }

    private enum DirectionToMoveTile
    {
        LEFT = 1,
        DOWN = 2,
        UP = 3,
        RIGHT = 4
    };

    private void Awake()
    {
        spriteInstance = gameObject.GetComponent<SpriteInstance>();
        characterData = Resources.Load<PlayableCharacterData>("ScriptableObjects/PlayableCharacterData/HarteData");
        characterData.characterCurrentHealth = characterData.characterMaxHealth;
        healthBar = GameObject.Find("Harte_Health_Bar").GetComponent<HealthBarUI>();
        healthBar.SetHaxHealth(characterData.characterMaxHealth);
        playerActions = new PlayerActions();

        // Movement Stuff
        playerActions.Combat.AddTileMovement.performed += AddTileMovement;

        // Attack Stuff
        playerActions.Combat.AddTileAtk.performed += ChangeTileValue;
    }

    private void Start()
    {
        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();
        StartGame();
    }

    private void Update()
    {
        VisualizeMovement();
        BattleManager.GetInstance().VisualizeEnemyAttacks();
        SeeAttackPattern();
        Death();
    }


    #region Movement Combat

    private void StartGame()
    {
        StartSpawn();
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
    private void AddTileMovement(InputAction.CallbackContext context)
    {
        if (BattleManager.GetInstance().GetCanPerformActions() && BattleManager.GetInstance().GetPlayerActionTypesList()[0] == BattleManager.ActionTypes.MOVE)
        {
            if (context.control.name == PlayerCombat.InputKeyNames.upArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.UP))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.UP];
                }
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.downArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.DOWN))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.DOWN];
                }
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.rightArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.RIGHT))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.RIGHT];
                }
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.leftArrow.ToString())
            {
                if (potentialMoveTiles.ContainsKey((int)DirectionToMoveTile.LEFT))
                {
                    chosenMoveLocation = potentialMoveTiles[(int)DirectionToMoveTile.LEFT];
                }
            }
        }
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
        //playerActionCount--;

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

    #endregion

    #region Attack Combat

    public MoveData ChosenMove(string nameOfMove)
    {
        foreach (var move in moves)
        {
            if (move.moveName == nameOfMove)
            {
                return move;
            }
        }
        return null;
    }

    private void ChangeTileValue(InputAction.CallbackContext context)
    {
        if (BattleManager.GetInstance().GetCanPerformActions() && BattleManager.GetInstance().GetPlayerActionTypesList()[0] == BattleManager.ActionTypes.ATTACK && chosenMove.Count != 0)
        {
            MoveData atk = chosenMove[0];
            if (context.control.name == PlayerCombat.InputKeyNames.upArrow.ToString() && (atk.rightMostTileKey + tileAddition) % 4 != 0)
            {
                tileAddition += 1;
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.downArrow.ToString() && chosenMove.Count != 0 && (((atk.leftMostTileKey + tileAddition) - 1) % 4) != 0)
            {
                tileAddition -= 1;
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.rightArrow.ToString() && chosenMove.Count != 0 && (atk.rightMostTileKey + tileAddition) <= (16 - 4))
            {
                tileAddition += 4;
            }
            else if (context.control.name == PlayerCombat.InputKeyNames.leftArrow.ToString() && chosenMove.Count != 0 && (atk.leftMostTileKey + tileAddition) > 4)
            {
                tileAddition -= 4;
            }
        }
    }

    public void SeeAttackPattern()
    {
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count != 0 && BattleManager.GetInstance().GetPlayerActionTypesList()[0] == BattleManager.ActionTypes.ATTACK)
        {
            if (attackTiles.Count == 0)
            {
                foreach (var key in chosenMove[0].tileKeys)
                {
                    GameObject tile = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[key];
                    tile.GetComponent<Image>().color = Color.hotPink;
                    attackTiles.Add(tile);
                }
            }

            // Sets up variables for setting the correct colors
            GridManager gridManager = BattleManager.GetInstance().GetGridManager();

            // Gets the tiles based on the mouse's position
            if (chosenMove[0].tileKeys[0] >= 1 && tileAddition <= gridManager.GetEnemyTileDictionary().Count)
            {
                attackTiles.Clear();

                // Resets tile color to red
                BattleManager.GetInstance().ResetEnemyGrid();

                // Sets the desired tiles to hotpink for visualization purposes
                foreach (var tileKey in gridManager.GetEnemyTileDictionary().Keys)
                {
                    foreach (var moveKey in chosenMove[0].tileKeys)
                    {

                        attackTiles.Add(gridManager.GetEnemyTileDictionary()[(moveKey + tileAddition)]);
                        gridManager.GetEnemyTileDictionary()[(moveKey + tileAddition)].gameObject.GetComponent<Image>().color = Color.hotPink;
                        continue;
                    }
                    break;
                }
            }
        }
    }

    public bool Attack()
    {
        Debug.Log("Chosen Move: " + chosenMove.FirstOrDefault().name);

        spriteInstance.Play(PlayerCombat.CombatState.ATTACK_COMBAT.ToString().ToLower());

        // Checks to make sure the tile is acceptable
        if (BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary().ContainsValue(attackTiles.FirstOrDefault()))
        {
            foreach (var tile in attackTiles)
            {
                if (tile.GetComponent<Tile>().GetCharacterOn())
                {
                    tile.GetComponent<Tile>().GetCharacterOnTile().GetComponent<Enemy>().GetEnemyScriptableObject().enemyCurrentHealth -= chosenMove.FirstOrDefault().attackDamage;
                    attackTiles = new List<GameObject>();
                }
            }
            chosenMove.RemoveAt(chosenMove.IndexOf(chosenMove.FirstOrDefault()));
            tileAddition = 0;
            return true;
        }

        return false;
    }

    public void TakeDamage(int damage)
    {
        spriteInstance.Play(CombatState.HIT_COMBAT.ToString().ToLower());
        characterData.characterCurrentHealth -= damage;
    }

    public void Death()
    {
        if (GetPlayableCharacterData().characterCurrentHealth <= 0)
        {
            spriteInstance.Stop(CombatState.DEATH_COMBAT.ToString().ToLower());
            StartCoroutine(Dead(spriteInstance.currentAnim.uniformFrameDelay * spriteInstance.currentAnim.frameSets[0].frames.Length));
        }
    }

    private IEnumerator Dead(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("EndScene");
    }

    #endregion

    #region Getters
    public PlayableCharacterData GetPlayableCharacterData()
    {
        return characterData;
    }

    public PlayerActions GetPlayerActions()
    {
        return playerActions;
    }

    public void SetCombatState(CombatState newCombatState, bool setPreviousAnim = false)
    {
        if (combatState == newCombatState)
        {
            return;
        }

        combatState = newCombatState;
        spriteInstance.Stop(combatState.ToString().ToLower());
    }

    public CombatState GetCombatState()
    {
        return combatState;
    }

    protected IEnumerator WaitForAnimation(float animationTime)
    {
        yield return new WaitForSeconds(animationTime);
    }

    public SpriteInstance GetSpriteInstance()
    {
        return spriteInstance;
    }

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

    public void SetChosenMoveData(MoveData move)
    {
        chosenMove.Add(move);
    }

    public List<MoveData> GetMoves()
    {
        return moves;
    }

    #endregion

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }
}