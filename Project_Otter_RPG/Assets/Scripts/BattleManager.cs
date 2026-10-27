using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{

    private static BattleManager battleManagerInstance;

    [SerializeField] private int playerCombatActions = 2;
    private PlayerCombat playerCombat;
    [SerializeField] private List<EnemyBase> enemyList;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("X = min amount of enemies, Y = Max amount of enemies")]
    [SerializeField] private Vector2 enemyRange;
    
    PlayerActions playerActions;
    private bool playersTurn = true;
    private bool canPerformActions = false;

    private List<ActionTypes> playerActionsTypes = new List<ActionTypes>();

    public enum ActionTypes
    {
        NO_ACTION,
        MOVE,
        ATTACK
    }

    // Creates an instance of the Game Manager if it is not set to null
    public static void CreateInstance()
    {
        if (battleManagerInstance != null)
        {
            battleManagerInstance = null;
        }
        battleManagerInstance = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    // Allows other scripts to get the game manager script
    public static BattleManager GetInstance()
    {
        return battleManagerInstance;
    }

    // Destroys the instance
    public static void DestroyInstance()
    {
        if (battleManagerInstance != null)
        {
            battleManagerInstance = null;
        }
    }

    private void Awake()
    {
        CreateInstance();

        // Gets the Player Actions for clicking
        playerActions = new PlayerActions();
        playerActions.Combat.ConfirmAction.performed += PerformAction;
        playerActions.Quit.QuitGame.performed += EndGame;
    }

    private void Start()
    {
        enemyList = new List<EnemyBase>();
        playerCombat = GameObject.FindAnyObjectByType<PlayerCombat>(FindObjectsInactive.Include);
        playerCombat.SetPlayerActionCount(playerCombatActions);
        List<GameObject> enemyObjects = new List<GameObject>();
        float numOfEnemies = Random.Range((int)enemyRange.x, ((int)enemyRange.y) + 1);
        for (int i = 0; i < numOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, new Vector2(0, 0), Quaternion.identity, GameObject.Find("Attack_Canvas").gameObject.transform);
            string newName = "Enemy" + i;
            enemy.name = newName;
            enemyList.Add(enemy.GetComponent<EnemyBase>());
        }
    }

    private void Update()
    {
        EndPlayerTurn();
    }

    // Performs actions based on the player's action list
    private void PerformAction(InputAction.CallbackContext ctx)
    {

        if (playerCombat.isAttacking)
        {
            if (playerCombat.Attack())
            {
                foreach (var enemy in enemyList)
                {
                    enemy.Death();
                }
                enemyList.RemoveAll(enemy => enemy.GetEnemyData().enemyCurrentHealth <= 0);
                ResetEnemyGrid();
                if (enemyList.Count <= 0)
                {
                    SceneManager.LoadScene("EndScene");
                }
            }
        }
    }

    private void EndPlayerTurn()
    {
        if (GetPlayerActionTypesList().Count == 0)
        {
            if (!playerCombat.GetSpriteInstance().currentAnim.looping)
            {
                if (playerCombat.GetSpriteInstance().currentAnim.hasEnded)
                {
                    playerCombat.SetCombatState(PlayerCombat.CombatState.IDLE_COMBAT);
                }
            }
            else if (playerCombat.GetSpriteInstance().currentAnim.name != PlayerCombat.CombatState.IDLE_COMBAT.ToString().ToLower())
            {
                playerCombat.GetSpriteInstance().Stop(PlayerCombat.CombatState.IDLE_COMBAT.ToString().ToLower());
            }
            BattleManager.GetInstance().SetCanPerformActions(false);
        }
    }

    private void PerformEnemyAction()
    {
        DetermineAction();
    }
    
    private void DetermineAction()
    {
        //if (!playersTurn)
        //{
        //    foreach (var enemy in enemyList)
        //    {
        //        enemy.visualizeAttack();
        //        if (enemy.GetEnemyActionTypes().Count != 0)
        //        {
        //            if (enemy.GetEnemyActionTypes()[0] == ActionTypes.MOVE)
        //            {
        //                enemy.MoveEnemyOnGrid();
        //                enemy.GetEnemyActionTypes().RemoveAt(0);
        //            }
        //            else if (enemy.GetEnemyActionTypes()[0] == ActionTypes.ATTACK)
        //            {
        //                enemy.Attack();
        //                
        //                ResetPlayerGrid();
        //                
        //                enemy.GetEnemyActionTypes().RemoveAt(0);
        //            }
        //            enemy.SetEnemyActionCount(-1);
        //        }
        //    }
        //    ResetEnemyGrid();
        //}
    }

    // Resets the color of the enemy grid
    public void ResetEnemyGrid()
    {
        foreach (var tile in gridManager.GetEnemyTileDictionary().Values)
        {
            if (tile.GetComponent<Image>().color != Color.red)
            {
                tile.GetComponent<Image>().color = Color.red;
            }
        }
    }

    public void ResetPlayerGrid()
    {
        foreach (var tile in gridManager.GetPlayerTileDictionary().Values)
        {
            if (tile.GetComponent<Image>().color != Color.green)
            {
                tile.GetComponent<Image>().color = Color.green;
            }
        }
    }

    private void EndGame(InputAction.CallbackContext context)
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // Sets the next action the player will perform
    public void SetPlayerAction(ActionTypes action)
    {
        int performedActions = playerActionsTypes.Count;
        if (performedActions < playerCombatActions)
        {
            playerActionsTypes.Add(action);
        }
        if (playerActionsTypes.Count == 2)
        {
            playerCombat.SetCombatState(PlayerCombat.CombatState.THINKING_COMBAT);
        }
    }

    public int GetPlayerActions()
    {
        return playerCombatActions;
    }

    public GridManager GetGridManager()
    {
        return gridManager;
    }

    public bool GetPlayersTurn()
    {
        return playersTurn;
    }

    public List<ActionTypes> GetPlayerActionTypesList()
    {
        return playerActionsTypes;
    }

    public List<EnemyBase> GetEnemyList()
    {
        return enemyList;
    }

    public bool GetCanPerformActions()
    {
        return canPerformActions;
    }

    public void SetCanPerformActions(bool truthValue)
    {
        canPerformActions = truthValue;
    }

    private void OnEnable()
    {

        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }
}
