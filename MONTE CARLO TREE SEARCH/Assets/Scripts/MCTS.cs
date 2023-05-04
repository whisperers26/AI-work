using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MCTS
{
    private Dictionary<string, (int, int)> nodeScores = new Dictionary<string, (int, int)>();
    public int TotalIterations { get; set; } = 0;
    public int gamesPlayed = 0;
    public int wins = 0;

    public int Search(GameModel root, int iterations)
    {
        // initialize
        if (!nodeScores.ContainsKey(root.GetState()))
        {
            nodeScores.Add(root.GetState(), (0, 0));
        }

        //move and iter define
        int move = -1;
        int iteration = 0;

        // iterate through tree
        while (iteration < iterations)
        {
            Stack<GameModel> path = Traverse(root);
            GameModel leaf = path.Peek();
            bool didRedWin = Rollout(leaf);
            move = Backpropagate(path, didRedWin);
            iteration++;
            TotalIterations++;
        }
        return move;
    }

    private int Backpropagate (Stack<GameModel> path, bool didRedWin)
    {
        int move = -1;

        // until path traversed
        while (path.Count > 0)
        {
            GameModel node = path.Pop();
            if (path.Count == 1)
                move = node.LastMove;
            (int, int) prevScore = nodeScores[node.GetState()];
            (int, int) newScore;
            if (didRedWin != node.IsRed)
                newScore = (prevScore.Item1 + 1, prevScore.Item2 + 1);
            else
                newScore = (prevScore.Item1, prevScore.Item2 + 1);
            nodeScores[node.GetState()] = newScore;
        }

        return move;
    }

    private bool Rollout (GameModel node)
    {
        // continue until reach leaf
        int depth = 0;
        while (!LeafNode(node))
        {
            depth++;
            node = RolloutNode(node);
        }
        return node.RedWin;
    }

    private GameModel RolloutNode (GameModel node)
    {
        int[] moves = GetRandomMoves();
        foreach (int move in moves)
        {
            if (node.ValidMove(move))
            {
                GameModel child = node.Clone();
                child.AddPiece(move);
                return child;
            }
        }
        return null;
    }

    private bool FullyExplored (GameModel node)
    {
        int[] moves = GetMoves();

        if (node.GameOver)
            return false;
        foreach (int move in moves)
        {
            if (node.ValidMove(move))
            {
                GameModel child = node.Clone();
                child.AddPiece(move);
                if (!nodeScores.ContainsKey(child.GetState()))
                    return false;
            }
        }

        return true;
    }

    private GameModel BestChild (GameModel node)
    {
        int[] moves = GetMoves();
        GameModel bestChild = null;
        double bestScore = double.MinValue;

        foreach (int move in moves)
        {
            if (node.ValidMove(move))
            {
                GameModel child = node.Clone();
                child.AddPiece(move);
                double score = GetScore(node, child);
                if (score > bestScore)
                {
                    bestChild = child;
                    bestScore = score;
                }
            }
        }
        return bestChild;
    }

    private bool LeafNode (GameModel node)
    {
        return node.GameOver;
    }

    private GameModel Expand (GameModel node)
    {
        int[] moves = GetRandomMoves();
        foreach (int move in moves)
        {
            if (node.ValidMove(move))
            {
                GameModel child = node.Clone();
                child.AddPiece(move);
                if (!nodeScores.ContainsKey(child.GetState()))
                {
                    nodeScores.Add(child.GetState(), (0, 0));
                    return child;
                }
            }
        }
        return null;
    }

    private Stack<GameModel> Traverse(GameModel node)
    {
        Stack<GameModel> path = new Stack<GameModel>();
        while (FullyExplored(node))
        {
            path.Push(node);
            node = BestChild(node);
        }
        path.Push(node);
        if (!LeafNode(node))
            path.Push(Expand(node));

        return path;
    }

    private double GetScore(GameModel parent, GameModel child)
    {
        (int, int) parentScore = nodeScores[parent.GetState()];
        (int, int) childScore = nodeScores[child.GetState()];

        double winRatio = childScore.Item1 / childScore.Item2;
        double visitRatio = Math.Log(parentScore.Item2) / childScore.Item2;
        double k = Math.Sqrt(2);

        return winRatio + k * Math.Sqrt(visitRatio);
    }

    // get possible moves
    private int[] GetMoves()
    {
        return new int[] { 0, 1, 2, 3, 4, 5, 6 };
    }

    // get random moves
    private int[] GetRandomMoves()
    {
        int[] arr = GetMoves();
        Random rand = new Random();
        arr = arr.OrderBy(x => rand.Next()).ToArray();
        return arr;
    }

    public string GetStats()
    {
        return "Total Expanded: " + nodeScores.Count + " Total Iterations: " + TotalIterations + " Games Played: " + gamesPlayed + " Wins: " + wins;
    }
}
