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
    [SerializeField] private int enemyCombatActions = 1;
    private PlayerCombat playerCombat;
    [SerializeField] private List<Enemy> enemyList;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private float enemyActionDelayTime = 1.0f;
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

        GameObject attackCanvas = GameObject.FindAnyObjectByType<ButtonManager>(FindObjectsInactive.Include).gameObject;
        attackCanvas.SetActive(true);

        // Gets the Player Actions for clicking
        playerActions = new PlayerActions();
        playerActions.Combat.ConfirmAction.performed += PerformAction;
        playerActions.Quit.QuitGame.performed += EndGame;
    }

    private void Start()
    {
        enemyList = new List<Enemy>();
        playerCombat = GameObject.FindAnyObjectByType<PlayerCombat>(FindObjectsInactive.Include);
        playerCombat.SetPlayerActionCount(playerCombatActions);
        List<GameObject> enemyObjects = new List<GameObject>();
        float numOfEnemies = Random.Range((int)enemyRange.x, ((int)enemyRange.y) + 1);
        for (int i = 0; i < numOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, new Vector2(0, 0), Quaternion.identity, GameObject.Find("Attack_Canvas").gameObject.transform);
            enemyList.Add(enemy.GetComponent<Enemy>());
        }

        SetEnemyAction();
    }

    public void VisualizeEnemyAttacks()
    {
        foreach (var enemy in enemyList)
        {
            enemy.visualizeAttack();
        }
    }

    private void Update()
    {
        UpdateTurn();

        VisualizeEnemyAttacks();

        EndPlayerTurn();

        Debug.Log("CAN PERFORM ACTION: " + canPerformActions);
    }

    // Updates whether it is the player or enemies turn
    private void UpdateTurn()
    {
        if (playersTurn == true && playerCombat.getPlayerActionCount() <= 0)
        {
            foreach (var enemyScript in enemyList)
            {
                enemyScript.SetEnemyActionCount(enemyCombatActions);
                playersTurn = false;
            }
            Invoke("PerformEnemyAction", enemyActionDelayTime);
        }
        if (playersTurn == false)
        {
            bool shouldBePlayersTurn = false;
            foreach (var enemyScript in enemyList)
            {
                if (enemyScript.GetEnemyActionCount() <= 0 && enemyScript.GetEnemyActionCount() == 0)
                {
                    shouldBePlayersTurn = true;
                }
                else
                {
                    shouldBePlayersTurn = false;
                    break;
                }
            }
            if (shouldBePlayersTurn)
            {
                SetEnemyAction();
                playersTurn = true;
                playerCombat.SetPlayerActionCount(playerCombatActions);
                ButtonManager buttonManager = GameObject.Find("Attack_Canvas").GetComponent<ButtonManager>();
                buttonManager.ShowUIMenu(true);
            }
        }
    }

    public void SetEnemyAction()
    {
        foreach (var enemy in enemyList)
        {
            if (enemy.GetEnemyActionTypes().Count != enemyCombatActions)
            {
                float attackWeight = enemy.DetermineAttackWeightModifier();
                float moveWeight = enemy.DetermineMoveWeightModifier();
                ActionTypes enemyAction = ActionTypes.NO_ACTION;
                if (attackWeight > moveWeight)
                {
                    enemyAction = ActionTypes.ATTACK;
                }
                else if (moveWeight > attackWeight)
                {
                    enemyAction = ActionTypes.MOVE;
                }
                enemy.SetEnemyActionTypes(enemyAction);
                enemy.visualizeAttack();
            }
        }
    }

    // Performs actions based on the player's action list
    private void PerformAction(InputAction.CallbackContext ctx)
    {
        // Checks to make sure list is not empty
        if (canPerformActions && playersTurn)
        {
            // What to do with the movement action
            if (playerActionsTypes[0] == ActionTypes.MOVE)
            {
                playerCombat.MovePlayer();
            }
            // What to do with the attack action
            else if (playerActionsTypes[0] == ActionTypes.ATTACK)
            {
                if (playerCombat.Attack())
                {
                    foreach (var enemy in enemyList)
                    {
                        enemy.Death();
                    }
                    enemyList.RemoveAll(enemy => enemy.GetEnemyScriptableObject().enemyCurrentHealth <= 0);
                    ResetEnemyGrid();
                    playerActionsTypes.RemoveAt(0);
                    if (enemyList.Count <= 0)
                    {
                        SceneManager.LoadScene("EndScene");
                    }
                }
            }
            playerCombat.SetPlayerActionCount(-1);
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
        if (!playersTurn)
        {
            foreach (var enemy in enemyList)
            {
                enemy.visualizeAttack();
                if (enemy.GetEnemyActionTypes().Count != 0)
                {
                    if (enemy.GetEnemyActionTypes()[0] == ActionTypes.MOVE)
                    {
                        enemy.MoveEnemyOnGrid();
                        enemy.GetEnemyActionTypes().RemoveAt(0);
                    }
                    else if (enemy.GetEnemyActionTypes()[0] == ActionTypes.ATTACK)
                    {
                        enemy.Attack();
                        
                        ResetPlayerGrid();
                        
                        enemy.GetEnemyActionTypes().RemoveAt(0);
                    }
                    enemy.SetEnemyActionCount(-1);
                }
            }
            ResetEnemyGrid();
        }
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

    public List<Enemy> GetEnemyList()
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
