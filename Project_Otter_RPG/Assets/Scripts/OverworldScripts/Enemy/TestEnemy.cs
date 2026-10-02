using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    GridManager gridManager;
    New_Graph connectionGraph = new New_Graph();
    Tile currentTile;
    Tile endTile;

    private void Awake()
    {
        gridManager = GameObject.Find("BattleManager").GetComponent<GridManager>();
    }

    private void Start()
    {
        connectionGraph.ConnectEnemyTiles();
        connectionGraph.ConnectPlayerTiles();
        StartSpawn();
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
        Debug.Log("CURRENT TILE IS: " + currentTile);
        int randomNumber = UnityEngine.Random.Range(11, 16);
        endTile = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[randomNumber].gameObject.GetComponent<Tile>();
        StartCoroutine(GoThroughPath(1.0f));
    }

    private IEnumerator GoThroughPath(float duration)
    {
        Debug.Log("INSIDE COROUTINE");
        yield return new WaitForSeconds(duration);
        List<Tile> path = connectionGraph.AstarMove(currentTile, endTile);
        if (path.Count > 0)
        {
            currentTile = path.LastOrDefault();
            Debug.Log("CURRENT TILE IS: " + currentTile);
            this.gameObject.transform.position = currentTile.gameObject.transform.position;
            Debug.Log("NEW TILE SELECTED");
            StartCoroutine(GoThroughPath(duration));
        }
        else
        {
            Debug.Log("ALL DONE");
        }
    }
}
