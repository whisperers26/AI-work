using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs search using A*.
/// </summary>
public class AStar : MonoBehaviour
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

    static private Graph graph;
    static private float scale;

    public static IEnumerator search(GameObject start, GameObject end, Heuristic heuristic, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Store the graph game object.
        GameObject graphGO = GameObject.Find("Graph");

        // Grab the graph script.
        graph = graphGO.GetComponent<Graph>();
        scale = graph.scale;

        // Starts the stopwatch.
        watch.Start();

        // Add your A* code here.

        // initialize record for start
        NodeRecord startRecord = new NodeRecord();
        startRecord.SetNodeRecord(start, null, 0.0f, heuristic(start, start, end));

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
            if (CurrentNodeRecord.Tile == end)
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
                float toNodeHeuristic = 0.0f;

                // if node is closed, skip or remove
                if (checkTileInList(toNode, closedList) != null)
                {
                    toNodeRecord = checkTileInList(toNode, closedList);

                    // no shorter route, skip
                    if (toNodeRecord.CostSoFar <= toNodeCost)
                        continue;
                    // otherwise, remove
                    closedList.Remove(toNodeRecord);

                    // get heuristic
                    toNodeHeuristic = toNodeRecord.EstimateTotalCost - toNodeRecord.CostSoFar;
                }
                // skip if node in open and not found shorter route
                else if (checkTileInList(toNode, openList) != null)
                {
                    toNodeRecord = checkTileInList(toNode, openList);

                    // no shorter route, skip
                    if (toNodeRecord.CostSoFar <= toNodeCost)
                        continue;

                    // get heuristic
                    toNodeHeuristic = toNodeRecord.EstimateTotalCost - toNodeRecord.CostSoFar;
                }
                // otherwise unvisited node and record
                else
                {
                    toNodeRecord = new NodeRecord();
                    toNodeRecord.SetNodeRecord(toNode);

                    // calculate heuristic
                    toNodeHeuristic = heuristic(start, toNode, end);
                }

                // update cost, estimate and connection
                toNodeRecord.SetNodeRecord(null, CurrentNodeRecord, toNodeCost, toNodeHeuristic + toNodeCost);

                // if display cost, update tile display
                if (displayCosts)
                {
                    toNodeRecord.Display(Mathf.Round((toNodeHeuristic + toNodeCost)*10)/10.0f);
                }

                // add it to open list if not in
                if (checkTileInList(toNode, openList) == null)
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
            if (colorTiles &&
                CurrentNodeRecord.Tile != start &&
                CurrentNodeRecord.Tile != end) CurrentNodeRecord.ColorTile(closedColor);

        }


        // Stops the stopwatch.
        watch.Stop();

        UnityEngine.Debug.Log("Seconds Elapsed: " + (watch.ElapsedMilliseconds / 1000f).ToString());
        UnityEngine.Debug.Log("Nodes Expanded: " + "print the number of nodes expanded here.");

        // Reset the stopwatch.
        watch.Reset();

        // Determine whether A* found a path and print it here.
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
                yield return new WaitForSeconds(waitTime);
            }

            // print path length
            UnityEngine.Debug.Log("Path Length: " + path.Count.ToString());
        }

        yield return null;
    }

    // find noderecord with smallest estimate cost
    public static NodeRecord findSmallest(List<NodeRecord> list)
    {
        float minCostSoFar = float.MaxValue;
        NodeRecord minNodeRecord = null;
        foreach (NodeRecord record in list)
        {
            if (record.EstimateTotalCost < minCostSoFar)
            {
                minCostSoFar = record.EstimateTotalCost;
                minNodeRecord = record;
            }
        }
        return minNodeRecord;
    }

    // check if a tile object is in noderecord list
    public static NodeRecord checkTileInList(GameObject tile, List<NodeRecord> list)
    {
        foreach (NodeRecord n in list)
        {
            if (tile == n.Tile)
                return n;
        }
        return null;
    }

    public delegate float Heuristic(GameObject start, GameObject tile, GameObject goal);

    public static float Uniform (GameObject start, GameObject tile, GameObject goal)
    {
        return 0f;
    }

    public static float Manhattan (GameObject start, GameObject tile, GameObject goal)
    {
        float dx = (Mathf.Abs(tile.transform.position.x - goal.transform.position.x)) * scale;
        float dy = (Mathf.Abs(tile.transform.position.y - goal.transform.position.y)) * scale;
        return 1.1f*(dx + dy);
    }

    public static float CrossProduct (GameObject start, GameObject tile, GameObject goal)
    {
        float dx = (Mathf.Abs(tile.transform.position.x - goal.transform.position.x)) * scale;
        float dy = (Mathf.Abs(tile.transform.position.y - goal.transform.position.y)) * scale;
        float crossHeuristic = 1.1f*(dx + dy);

        float dx1 = (tile.transform.position.x - goal.transform.position.x) * scale;
        float dy1 = (tile.transform.position.y - goal.transform.position.y) * scale;
        float dx2 = (start.transform.position.x - goal.transform.position.x) * scale;
        float dy2 = (start.transform.position.y - goal.transform.position.y) * scale;
        float cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
        crossHeuristic += cross * 0.001f;

        return crossHeuristic;
    }
}
