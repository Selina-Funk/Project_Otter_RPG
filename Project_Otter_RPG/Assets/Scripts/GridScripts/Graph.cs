using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Rendering;

public class Graph
{
    private Dictionary<int, List<int>> adjacencyList;

    public Graph()
    {
        adjacencyList = new Dictionary<int, List<int>>();
    }

    public void AddEdge(int vertex1, int vertex2)
    {
        if (!adjacencyList.ContainsKey(vertex1))
        {
            adjacencyList.Add(vertex1, new List<int>());
        }
        if (!adjacencyList.ContainsKey(vertex2))
        {
            adjacencyList.Add(vertex2, new List<int>());
        }

        adjacencyList[vertex1].Add(vertex2);
    }

    public List<int> BFS(int startVertex, int goalVertex)
    {
        Dictionary<int, int> visited = new Dictionary<int, int>();
        visited[startVertex] = startVertex;
        List<int> paths = new List<int>();
        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startVertex);

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (current == goalVertex)
            {
                return pathDicToList(ref visited, ref goalVertex);
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    frontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                }
            }
        }

        return null;
    }

    public void BFS(int startVertex, int goalVertex, out List<int> keyList, out int weightWant, Enemy enemy)
    {
        Dictionary<int, int> visited = new Dictionary<int, int>();
        visited[startVertex] = startVertex;
        List<int> paths = new List<int>();
        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startVertex);

        //tileKeyWant = 0;
        keyList = new List<int>();
        weightWant = 0;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (current == goalVertex)
            {
                pathDicToList(ref visited, ref goalVertex, out List<int> tileKeys, out int weight, enemy);
                keyList = tileKeys;
                weightWant = weight;
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    frontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                }
            }
        }
    }

    public Queue<GameObject> Dikjstra(GameObject start, GameObject goal)
    {
        Dictionary<int, GameObject> enemyTileDic = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary();

        Dictionary<GameObject, GameObject> nextTileToGoal = new Dictionary<GameObject, GameObject>();
        Dictionary<GameObject, int> costToReachTile = new Dictionary<GameObject, int>();
        int tileIndex = 0;

        PriorityQueue<GameObject> frontier = new PriorityQueue<GameObject>();

        frontier.Enqueue(goal, 0);
        costToReachTile[goal] = 0;

        while (frontier.Count > 0)
        {
            GameObject currentTile = frontier.Dequeue();
            if (currentTile == start)
            {
                break;
            }

            for (int i = 1; i <= enemyTileDic.Count; i++)
            {
                if (enemyTileDic[i] == currentTile)
                {
                    tileIndex = i;
                    break;
                }
            }

            var neighbors = adjacencyList.ContainsKey(tileIndex) ? adjacencyList[tileIndex] : new List<int>();

            foreach (int neighbor in neighbors)
            {
                int newCost = costToReachTile[currentTile] + enemyTileDic[tileIndex].GetComponent<Tile>().GetTileWeight();

                GameObject tile = enemyTileDic[tileIndex];

                if (costToReachTile.ContainsKey(enemyTileDic[neighbor]) == false || newCost < costToReachTile[enemyTileDic[neighbor]])
                {
                    costToReachTile[enemyTileDic[neighbor]] = newCost;
                    int priority = newCost;
                    frontier.Enqueue(enemyTileDic[neighbor], priority);
                    nextTileToGoal[enemyTileDic[neighbor]] = currentTile;
                }
            }
        }

        if (!nextTileToGoal.ContainsKey(start))
        {
            return null;
        }

        Queue<GameObject> path = new Queue<GameObject>();
        GameObject currentPathTile = start;

        while (currentPathTile != goal)
        {
            currentPathTile = nextTileToGoal[currentPathTile];
            path.Enqueue(currentPathTile);
        }

        return path;
    }

    public void ChangeTileWeights(int startInt, bool playerGraph, int decreaseWeight)
    {
        Dictionary<int, int> visited = new Dictionary<int, int>();
        visited[startInt] = startInt;
        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startInt);
        int weightValue = 5;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (playerGraph)
            {
                BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weightValue * 2);
            }
            else
            {
                BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weightValue * 2);
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    frontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                }
            }
            ChangeWeightHelpFunction(weightValue, ref frontier, ref visited, playerGraph, decreaseWeight);
        }
        Debug.Log("VISITED COUNT: " + visited.Count);
    }

    public void ChangeTileWeights(int startInt, bool playerGraph, int desiredWeight, int decreaseWeight, ref int lowestValueTile)
    {
        Dictionary<int, int> visited = new Dictionary<int, int>();
        visited[startInt] = startInt;
        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startInt);
        int weightValue = desiredWeight;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (playerGraph)
            {
                BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weightValue * 2);
            }
            else
            {
                BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weightValue * 2);
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    frontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                    if (lowestValueTile > BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[current].GetComponent<Tile>().GetTileWeight())
                    {
                        lowestValueTile = current;
                    }
                }
            }
            ChangeWeightHelpFunction(weightValue, ref frontier, ref visited, playerGraph, decreaseWeight, ref lowestValueTile);
        }
        Debug.Log("VISITED COUNT: " + visited.Count);
    }

    private void ChangeWeightHelpFunction(int weight, ref Queue<int> oldFrontier, ref Dictionary<int, int> visited, bool playerGraph, int weightDecreaseValue)
    {
        weight -= weightDecreaseValue;
        Queue<int> newFrontier = new Queue<int>();
        
        while (oldFrontier.Count > 0)
        {
            int current = oldFrontier.Dequeue();

            if (playerGraph)
            {
                BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weight);
            }
            else
            {
                BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weight);
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    newFrontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                }
            }
        }
        if (newFrontier.Count > 0)
            ChangeWeightHelpFunction(weight, ref newFrontier, ref visited, playerGraph, weightDecreaseValue);
    }

    private void ChangeWeightHelpFunction(int weight, ref Queue<int> oldFrontier, ref Dictionary<int, int> visited, bool playerGraph, int weightDecreaseValue, ref int tileToMoveTo)
    {
        weight -= weightDecreaseValue;
        Queue<int> newFrontier = new Queue<int>();

        while (oldFrontier.Count > 0)
        {
            int current = oldFrontier.Dequeue();

            if (playerGraph)
            {
                BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weight);
            }
            else
            {
                BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary()[current].GetComponent<Tile>().AddTileWeight(weight);
            }

            if (BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[tileToMoveTo].GetComponent<Tile>().GetTileWeight() > BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary()[current].GetComponent<Tile>().GetTileWeight())
            {
                tileToMoveTo = current;
            }

            var neighbors = adjacencyList.ContainsKey(current) ? adjacencyList[current] : new List<int>();

            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor))
                {
                    continue;
                }
                else
                {
                    newFrontier.Enqueue(neighbor);
                    visited[neighbor] = current;
                }
            }
        }
        if (newFrontier.Count > 0)
            ChangeWeightHelpFunction(weight, ref newFrontier, ref visited, playerGraph, weightDecreaseValue, ref tileToMoveTo);
    }

    public void enemyAttackDecision(int startIndex, int endIndex, List<int> attackTiles, out int totalWeight, out int tileAddition)
    {
        totalWeight = 0;
        int tempWeight = 0;
        tileAddition = 0;
        int addition = 0;

        for (int i = startIndex; i <= endIndex; i++)
        {
            foreach (int tileKey in attackTiles)
            {
                BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary().TryGetValue(tileKey + addition, out GameObject tileObj);
                tempWeight += tileObj.GetComponent<Tile>().GetTileWeight();
            }
            if (tempWeight > totalWeight)
            {
                totalWeight = tempWeight;
                tileAddition = (i - 1);
            }
            tempWeight = 0;
            addition++;
        }
    }

    private List<int> pathDicToList(ref Dictionary<int, int> previousDic, ref int goal)
    {
        List<int> pathway = new List<int>();
        int current, previous;
        current = goal;
        do
        {
            pathway.Insert(0, current);
            previous = current;
            current = previousDic[current];
        } while (current != previous);
        return pathway;
    }

    private void pathDicToList(ref Dictionary<int, int> previousDic, ref int goal, out List<int> tileKeys, out int weight, Enemy enemy)
    {
        Dictionary<int, GameObject> enemyTileDictionary = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary();
        List<int> pathway = new List<int>();
        int current, previous;
        current = goal;

        weight = 0;
        //tileKey = 0;
        bool firstTime = true;

        do
        {
            weight += enemyTileDictionary[current].gameObject.GetComponent<Tile>().GetTileWeight();
            if (current != enemy.GetEnemyTileData().Key || firstTime)
            {
                pathway.Insert(0, current);
            }
            firstTime = false;
            previous = current;
            current = previousDic[current];
        } while (current != previous);
        tileKeys = pathway;
    }
}
