using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using Unity.VisualScripting;
using UnityEngine;

public class New_Graph : MonoBehaviour
{
    private Dictionary<Tile, List<Tile>> enemyAdjacencyList = new Dictionary<Tile, List<Tile>>(); // Enemy tiles that are next to each other and connect 
    private Dictionary<Tile, List<Tile>> playerAdjacencyList = new Dictionary<Tile, List<Tile>>(); // Player tiles that are next to each other and connect
    
    private Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>(); // Second tile is the tile it came from
    private Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int>(); // total cost at the tile
    
    private PriorityQueue<Tile> frontier;

    private GridManager gridManager;
    private TestEnemy enemy;

    public void ConnectEnemyTiles(TestEnemy theEnemy)
    {
        enemy = theEnemy;
        gridManager = BattleManager.GetInstance().GetGridManager();
        foreach (var obj in gridManager.GetEnemyTileDictionary())
        {
            List<Tile> neighbors = new List<Tile>();

            if (gridManager.GetEnemyTileDictionary().ContainsKey(obj.Key - 4))
            {
                neighbors.Add(gridManager.GetEnemyTileDictionary()[obj.Key - 4].GetComponent<Tile>());
            }
            if (gridManager.GetEnemyTileDictionary().ContainsKey(obj.Key - 1) && obj.Value.GetComponent<Tile>().GetPosition().y != 0)
            {
                neighbors.Add(gridManager.GetEnemyTileDictionary()[obj.Key - 1].GetComponent<Tile>());
            }
            if (gridManager.GetEnemyTileDictionary().ContainsKey(obj.Key + 1) && obj.Value.GetComponent<Tile>().GetPosition().y != 3)
            {
                neighbors.Add(gridManager.GetEnemyTileDictionary()[obj.Key + 1].GetComponent<Tile>());
            }
            if (gridManager.GetEnemyTileDictionary().ContainsKey(obj.Key + 4))
            {
                neighbors.Add(gridManager.GetEnemyTileDictionary()[obj.Key + 4].GetComponent<Tile>());
            }

            if (!enemyAdjacencyList.ContainsKey(obj.Value.GetComponent<Tile>()))
            {
                enemyAdjacencyList.Add(obj.Value.GetComponent<Tile>(), neighbors);
            }
        }
    }

    public void ConnectPlayerTiles(TestEnemy theEnemy)
    {
        enemy = theEnemy;
        gridManager = BattleManager.GetInstance().GetGridManager();
        foreach (var obj in gridManager.GetPlayerTileDictionary())
        {
            List<Tile> neighbors = new List<Tile>();

            if (gridManager.GetPlayerTileDictionary().ContainsKey(obj.Key - 4))
            {
                neighbors.Add(gridManager.GetPlayerTileDictionary()[obj.Key - 4].GetComponent<Tile>());
            }
            if (gridManager.GetPlayerTileDictionary().ContainsKey(obj.Key - 1))
            {
                neighbors.Add(gridManager.GetPlayerTileDictionary()[obj.Key - 1].GetComponent<Tile>());
            }
            if (gridManager.GetPlayerTileDictionary().ContainsKey(obj.Key + 1))
            {
                neighbors.Add(gridManager.GetPlayerTileDictionary()[obj.Key + 1].GetComponent<Tile>());
            }
            if (gridManager.GetPlayerTileDictionary().ContainsKey(obj.Key + 4))
            {
                neighbors.Add(gridManager.GetPlayerTileDictionary()[obj.Key + 4].GetComponent<Tile>());
            }

            if (!playerAdjacencyList.ContainsKey(obj.Value.GetComponent<Tile>()))
            {
                playerAdjacencyList.Add(obj.Value.GetComponent<Tile>(), neighbors);
            }
        }
    }

    public List<Tile> GetNeighbors(Tile tile, bool isPlayerGrid)
    {
        if (!isPlayerGrid)
        {
            if (enemyAdjacencyList.ContainsKey(tile))
            {
                return enemyAdjacencyList[tile];
            }
        }
        else
        {
            if (playerAdjacencyList.ContainsKey(tile))
            {
                return playerAdjacencyList[tile];
            }
        }
        return null;
    }

    public List<Tile> AstarMove(Tile start, Tile end)
    {
        frontier = new PriorityQueue<Tile>();
        List<Tile> path = new List<Tile>();
        Tile current = start;
        frontier.Enqueue(current, 0);
        costSoFar[current] = 0;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == end)
            {
                break;
            }

            if (current.GetCharacterOn() && current != enemy.GetCurrentTile())
            {
                continue;
            }

            List<Tile> neighbors = enemyAdjacencyList[current];

            foreach (var neighbor in neighbors)
            {
                int newCost = costSoFar[current] + neighbor.GetTileWeight();
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Enqueue(neighbor, newCost);
                    cameFrom[neighbor] = current;
                }
            }
        }

        while (cameFrom.ContainsKey(current) && cameFrom[current] != start)
        {
            path.Add(cameFrom[current]);
            current = cameFrom[current];
        }

        return path;
    }
}
