using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// Performs search using Dijkstra's algorithm.
/// </summary>
public class Dijkstra : MonoBehaviour
{
    // Colors for the different search categories.
    public static Color openColor = Color.cyan;
    public static Color closedColor = Color.blue;
    public static Color activeColor = Color.yellow;
    public static Color pathColor = Color.yellow;

    // The stopwatch for timing search.
    private static Stopwatch watch = new Stopwatch();

    // open and close list
    public static List<NodeRecord> openList;
    public static List<NodeRecord> closedList;
    public static NodeRecord CurrentNodeRecord = null;

    private Graph graph;


    public static IEnumerator search(GameObject start, GameObject end, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Starts the stopwatch.
        watch.Start();

        // Add your Dijkstra code here.

        // initialize record for start
        NodeRecord startRecord = new NodeRecord();
        startRecord.SetNodeRecord(start, null, 0.0f);

        // initialize open and closed list
        openList = new List<NodeRecord>();
        closedList = new List<NodeRecord>();
        openList.Add(startRecord);

        // process each node
        while (openList.Count > 0)
        {
            // find smallest element
            CurrentNodeRecord = findSmallest(openList);

            // if coloring tiles, update tile color
            if (colorTiles && 
                CurrentNodeRecord.Tile != start && 
                CurrentNodeRecord.Tile != end) CurrentNodeRecord.ColorTile(activeColor);

            // pause animation to show new active tile
            yield return new WaitForSeconds(waitTime);

            // if goal node, terminate
            if(CurrentNodeRecord.Tile==end) 
                break;

            // otherwise get connections
            Dictionary<Direction, GameObject> connections = 
                CurrentNodeRecord.CurrentNode.Connections;

            // loop through each connection
            foreach (KeyValuePair<Direction, GameObject> connection in connections)
            {
                // get cost estimate for each to-node
                // it should be connection.getCost(), but the code is hard coded
                // why the connection is coded in node? It should be a seperate class
                // containing from - cost - to
                GameObject toNode = connection.Value;
                float toNodeCost = CurrentNodeRecord.CostSoFar + 1.0f;
                NodeRecord toNodeRecord = null;

                // skip if node is closed
                if (checkTileInList(toNode, closedList) != null)
                    continue;
                // if it is open and find a worse route
                else if (checkTileInList(toNode, openList) != null)
                {
                    // find record in open list
                    toNodeRecord = checkTileInList(toNode, openList);
                    if(toNodeRecord.CostSoFar<=toNodeCost)
                        continue;
                }
                // otherwise get unvisited node, record it
                else
                {
                    toNodeRecord = new NodeRecord();
                    toNodeRecord.SetNodeRecord(toNode);
                }

                // update node cost and connection
                toNodeRecord.SetNodeRecord(null, CurrentNodeRecord, toNodeCost);

                // if display costs, update tile display
                if (displayCosts)
                {
                    toNodeRecord.Display(toNodeCost);
                }

                // add it to open list if not in
                if(checkTileInList(toNode, openList) == null)
                {
                    openList.Add(toNodeRecord);
                }

                // if coloring tiles, update open tile color
                if (colorTiles &&
                toNodeRecord.Tile != start &&
                toNodeRecord.Tile != end)
                {
                    toNodeRecord.ColorTile(openColor);
                }

                // pause animation to show new open tile
                yield return new WaitForSeconds(waitTime);

            }

            // add it to close list and remove from open
            openList.Remove(CurrentNodeRecord);
            closedList.Add(CurrentNodeRecord);

            // if coloring tiles, update closed tile color
            if(colorTiles &&
                CurrentNodeRecord.Tile != start &&
                CurrentNodeRecord.Tile != end) CurrentNodeRecord.ColorTile(closedColor);
        }

        // Stops the stopwatch.
        watch.Stop();

        UnityEngine.Debug.Log("Seconds Elapsed: " + (watch.ElapsedMilliseconds / 1000f).ToString());
        UnityEngine.Debug.Log("Nodes Expanded: " + "print the number of nodes expanded here.");

        // Reset the stopwatch.
        watch.Reset();

        // Determine whether Dijkstra found a path and print it here.
        // UnityEngine.Debug.Log(CurrentNodeRecord.Tile.transform);
        if (CurrentNodeRecord.Tile != end)
            UnityEngine.Debug.Log("Search Failed");
        else
        {
            // work back along path
            // path = new Stack<NodeRecord>();
            while (CurrentNodeRecord.Tile != start)
            {
                if (path == null)
                    path = new Stack<NodeRecord>();
                path.Push(CurrentNodeRecord);
                CurrentNodeRecord = CurrentNodeRecord.LastNodeRecord;

                // if coloring tiles, update path tile color
                if (colorTiles && 
                    CurrentNodeRecord.Tile != start && 
                    CurrentNodeRecord.Tile != end) 
                    CurrentNodeRecord.ColorTile(pathColor);
                
                // pause animation to show new path tile
                yield return new WaitForSeconds (waitTime);
            }

            // print path length
            UnityEngine.Debug.Log("Path Length: " + path.Count.ToString());
        }


        yield return null;
    }

    // find noderecord with smallest cost so far
    public static NodeRecord findSmallest(List<NodeRecord> list)
    {
        float minCostSoFar = float.MaxValue;
        NodeRecord minNodeRecord = null;
        foreach (NodeRecord record in list)
        {
            if (record.CostSoFar < minCostSoFar)
            {
                minCostSoFar = record.CostSoFar;
                minNodeRecord = record;
            }
        }
        return minNodeRecord;
    }

    // check if a tile object is in noderecord list
    public static NodeRecord checkTileInList(GameObject tile, List<NodeRecord> list)
    {
        foreach(NodeRecord n in list)
        {
            if(tile==n.Tile)
                return n;
        }
        return null;
    }
}

/// <summary>
/// A class for recording search statistics.
/// </summary>
public class NodeRecord
{
    // The tile game object.
    public GameObject Tile { get; set; } = null;

    // Set the other class properties here.
    public Node CurrentNode { get; set; } = null;
    public float CostSoFar { get; set; } = 0.0f;
    public float EstimateTotalCost { get; set; } = 0.0f;

    // the Connection represents the previous node tile
    public NodeRecord LastNodeRecord { get; set; } = null;

    public void SetNodeRecord(
        GameObject tile = null,
        NodeRecord lastNodeRecord = null,
        float costSoFar = -1.0f, 
        float estimateTotalCost = -1.0f)
    {
        if (tile != null) { 
            this.Tile = tile;
            this.CurrentNode = tile.GetComponent<Node>();
        }
        if(lastNodeRecord != null) this.LastNodeRecord = lastNodeRecord;
        if(costSoFar!=-1.0f) this.CostSoFar = costSoFar;
        if(estimateTotalCost!=-1.0f) this.EstimateTotalCost = estimateTotalCost;
    }

    // Sets the tile's color.
    public void ColorTile (Color newColor)
    {
        SpriteRenderer renderer = Tile.GetComponentInChildren<SpriteRenderer>();
        renderer.material.color = newColor;
    }

    // Displays a string on the tile.
    public void Display (float value)
    {
        TextMesh text = Tile.GetComponent<TextMesh>();
        text.text = value.ToString();
    }

    
}
