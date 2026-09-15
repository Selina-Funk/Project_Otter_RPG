using System.Collections.Generic;
using UnityEngine;

public class GraphBehavior : MonoBehaviour
{
    private static Graph playerTileConnection;
    private static Graph enemyTileConnection;

    private static void ConnectEnemyTiles()
    {
        enemyTileConnection = new Graph();
        Dictionary<int, GameObject> enemyDictionary = BattleManager.GetInstance().GetGridManager().GetEnemyTileDictionary();
        foreach (var obj in enemyDictionary)
        {
            if (enemyDictionary.ContainsKey(obj.Key - 4))
            {
                enemyTileConnection.AddEdge(obj.Key, obj.Key - 4);
            }
            if (enemyDictionary.ContainsKey(obj.Key - 1))
            {
                enemyTileConnection.AddEdge(obj.Key, obj.Key - 1);
            }
            if (enemyDictionary.ContainsKey(obj.Key + 1))
            {
                enemyTileConnection.AddEdge(obj.Key, obj.Key + 1);
            }
            if (enemyDictionary.ContainsKey(obj.Key + 4))
            {
                enemyTileConnection.AddEdge(obj.Key, obj.Key + 4);
            }
        }
    }

    private static void ConnectPlayerTiles()
    {
        playerTileConnection = new Graph();
        Dictionary<int, GameObject> enemyDictionary = BattleManager.GetInstance().GetGridManager().GetPlayerTileDictionary();
        foreach (var obj in enemyDictionary)
        {
            if (enemyDictionary.ContainsKey(obj.Key - 4))
            {
                playerTileConnection.AddEdge(obj.Key, obj.Key - 4);
            }
            if (enemyDictionary.ContainsKey(obj.Key - 1) && ((obj.Key - 1) % 4 != 0))
            {
                playerTileConnection.AddEdge(obj.Key, obj.Key - 1);
            }
            if (enemyDictionary.ContainsKey(obj.Key + 1) && (obj.Key % 4 != 0))
            {
                playerTileConnection.AddEdge(obj.Key, obj.Key + 1);
            }
            if (enemyDictionary.ContainsKey(obj.Key + 4))
            {
                playerTileConnection.AddEdge(obj.Key, obj.Key + 4);
            }
        }
    }

    //public static List<int> GetPlayerGridPath(int start, int end)
    //{
    //    if (enemyTileConnection == null && enemyTileConnection == null)
    //    {
    //        ConnectEnemyTiles();
    //        ConnectPlayerTiles();
    //    }
    //    return playerTileConnection.BFS(start, end);
    //}

    public static void ChangeWeights(int start, bool playerGraph, int decreaseWeight)
    {
        if (playerGraph)
        {
            ConnectPlayerTiles();
            playerTileConnection.ChangeTileWeights(start, playerGraph, decreaseWeight);
        }
        else
        {
            ConnectEnemyTiles();
            enemyTileConnection.ChangeTileWeights(start, playerGraph, decreaseWeight);
        }
    }

    public static void ChangeWeights(int start, bool playerGraph, int weight, int decreaseWeight, ref int lowestValueTile)
    {
        if (playerGraph)
        {
            ConnectPlayerTiles();
            playerTileConnection.ChangeTileWeights(start, playerGraph, decreaseWeight);
        }
        else
        {
            ConnectEnemyTiles();
            enemyTileConnection.ChangeTileWeights(start, playerGraph, weight, decreaseWeight, ref lowestValueTile);
        }
    }

    public static void GetEnemyAttackWeight(int startIndex, int endIndex, List<int> moveList, out int totalWeight, out int tileAddition)
    {
        ConnectPlayerTiles();
        enemyTileConnection.enemyAttackDecision(startIndex, endIndex, moveList, out totalWeight, out tileAddition);
    }

    public static void GetEnemyMoveWeight(int startIndex, int endIndex, out List<int> listOfKeys, out int weight, Enemy enemyScript)
    {
        ConnectEnemyTiles();
        enemyTileConnection.BFS(startIndex, enemyScript.GetTileKeyToMoveTo(), out listOfKeys, out weight, enemyScript);
    }

    // GetMoveWeight using dijkstra
    public static void GetEnemyMoveWeight(GameObject startTile, GameObject endTile, out Queue<GameObject> pathway)
    {
        ConnectEnemyTiles();
        pathway = enemyTileConnection.Dikjstra(startTile, endTile);
    }
}
