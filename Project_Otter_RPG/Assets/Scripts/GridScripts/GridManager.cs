using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    // Variables for creating enemy grid
    [SerializeField] int eWidth = 3, eHeight = 3;
    [SerializeField] Vector2 enemyGridStart = new Vector2(0.20f, 0.80f);
    [SerializeField] float offset = 10.0f;

    [SerializeField] int pWidth = 3, pHeight = 3;
    [SerializeField] Vector2 playerGridStart = new Vector2(-1.0f, 3.0f);
    
    // Variables for the tiles themselves
    [SerializeField] GameObject tile;
    [SerializeField] LayerMask mask;
    [SerializeField] string enemyTileTag;
    [SerializeField] string playerTileTag;

    // Variables for the player actions
    private PlayerActions playerActions;
    private Vector2 mouseLocation;

    // Variables for storing the tiles
    private Dictionary<int, GameObject> playerTileDictionary = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> enemyTileDictionary = new Dictionary<int, GameObject>();

    private int baseTileWeight = 5;
    List<int> enemyTilePathway;
    List<int> playerTilePathway;

    private void Awake()
    {
        // Creates a new PlayerActions
        playerActions = new PlayerActions();

        // Gets the current mouse position
        playerActions.MouseActions.MouseLocation.performed += ctx => mouseLocation = ctx.ReadValue<Vector2>();
        
        createGrids();

        playerTilePathway = new List<int>();
    }

    public int FindTileKey(GameObject tileObj, bool playerGraph)
    {
        if (playerGraph)
        {
            foreach (var tile in playerTileDictionary)
            {
                if (tile.Value == tileObj.gameObject)
                {
                    return tile.Key;
                }
            }

            return -1;
        }
        else
        {
            foreach (var tile in enemyTileDictionary)
            {
                if (tile.Value == tileObj.gameObject)
                {
                    return tile.Key;
                }
            }
        }

            return -1;
    }

    private void createGrids()
    {
        enemyGrid();
        playerGrid();
    }

    private void enemyGrid()
    {
        GameObject enemyGridContainer = GameObject.Find("Enemy_Grid_Container");
        int eTileCount = 1;

        RectTransform rt = tile.GetComponent<RectTransform>();
        // Generates a grid based on width and height for the enemy
        for (int i = 0; i < eWidth; i++)
        {
            for (int j = 0; j < eHeight; j++)
            {
                // Instantiates the tile object and renames it based on it's position, then adds it to the dictionary
                var currentTile = Instantiate(tile, GameObject.Find("Attack_Canvas").transform.Find("Enemy_Grid_Container").transform);
                currentTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(enemyGridStart.x + ((rt.rect.width + offset) * i), enemyGridStart.y + ((rt.rect.height + offset) * j));
                currentTile.name = $"EnemyTile({i},{j})";
                currentTile.tag = enemyTileTag;
                currentTile.GetComponent<Tile>().SetPosition(new Vector2Int(i, j));
                currentTile.GetComponent<Tile>().init(true);
                currentTile.GetComponent<Tile>().SetTileWeight(baseTileWeight);
                enemyTileDictionary.Add(eTileCount, currentTile);
                eTileCount++;
            }
        }
    }

    private void playerGrid()
    {
        GameObject playerGridContainer = GameObject.Find("Player_Grid_Container");
        int pTileCount = 1;

        RectTransform rt = tile.GetComponent<RectTransform>();
        // Generates a grid based on width and height for the player
        for (int i = 0; i < pWidth; i++)
        {
            for (int j = 0; j < pHeight; j++)
            {
                // Instantiates the tile object and renames it based on it's position, then adds it to the dictionary
                var currentTile = Instantiate(tile, GameObject.Find("Attack_Canvas").transform.Find("Player_Grid_Container").transform);
                currentTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(playerGridStart.x + ((rt.rect.width + offset) * i), playerGridStart.y + ((rt.rect.height + offset) * j));
                currentTile.name = $"PlayerTile({i},{j})";
                currentTile.tag = playerTileTag;
                currentTile.GetComponent<Tile>().init(false);
                currentTile.GetComponent<Tile>().SetTileWeight(baseTileWeight);
                playerTileDictionary.Add(pTileCount, currentTile);
                pTileCount++;
            }
        }
    }

    public void ResetPlayerTileWeight()
    {
        foreach (var tile in playerTileDictionary.Values)
        {
            tile.GetComponent<Tile>().SetTileWeight(baseTileWeight);
        }
    }

    public void ResetEnemyTileWeight()
    {
        foreach (var tile in enemyTileDictionary.Values)
        {
            tile.GetComponent<Tile>().SetTileWeight(baseTileWeight);
        }
    }

    // Gets the dictionary containing the enemy grid tiles
    public Dictionary<int, GameObject> GetEnemyTileDictionary()
    {
        return enemyTileDictionary;
    }

    // Gets the dictionary containing the player grid tiles
    public Dictionary<int, GameObject> GetPlayerTileDictionary()
    {
        return playerTileDictionary;
    }

    // Gets the enemy tile tag name
    public string GetEnemyTileTag()
    {
        return enemyTileTag;
    }

    // Gets the player tile tag name
    public string GetPlayerTag()
    {
        return playerTileTag;
    }

    // Gets player grid width
    public int GetPlayerGridWidth()
    {
        return pWidth;
    }

    // Gets player grid height
    public int GetPlayerGridHeight()
    {
        return pHeight;
    }

    // Gets enemy grid width
    public int GetEnemyGridWidth()
    {
        return eWidth;
    }


    // Gets enemy grid height
    public int GetEnemyGridHeight()
    {
        return eHeight;
    }

    public int GetBaseTileWeight()
    {
        return baseTileWeight;
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
