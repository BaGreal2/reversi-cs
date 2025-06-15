using System;
using System.Linq;
using System.Collections.Generic;

namespace Reversi.Models;

public sealed class ReversiAI
{
    public (int x, int y) GetMove(ReversiGame game, int depth)
    {
        var best = (x: -1, y: -1);
        var bestScore = int.MinValue;
        foreach (var m in game.GetValidMoves(game.CurrentPlayer))
        {
            var clone = game.Clone();
            clone.MakeMove(m.x, m.y);
            var score = Minimax(clone, depth - 1, false, int.MinValue, int.MaxValue, game.CurrentPlayer);
            if (score > bestScore)
            {
                bestScore = score;
                best = m;
            }
        }
        return best;
    }

    int Minimax(ReversiGame g, int depth, bool max, int alpha, int beta, Piece ai)
    {
        if (depth == 0 || g.IsGameOver) return Eval(g, ai);

        var moves = g.GetValidMoves(g.CurrentPlayer);
        if (moves.Count == 0)
        {
            var pass = g.Clone();
            pass.SkipTurn();                       // <- NEW helper
            return Minimax(pass, depth - 1, !max, alpha, beta, ai);
        }

        if (max)
        {
            var val = int.MinValue;
            foreach (var m in moves)
            {
                var c = g.Clone();
                c.MakeMove(m.x, m.y);
                val = Math.Max(val, Minimax(c, depth - 1, false, alpha, beta, ai));
                alpha = Math.Max(alpha, val);
                if (beta <= alpha) break;
            }
            return val;
        }
        else
        {
            var val = int.MaxValue;
            foreach (var m in moves)
            {
                var c = g.Clone();
                c.MakeMove(m.x, m.y);
                val = Math.Min(val, Minimax(c, depth - 1, true, alpha, beta, ai));
                beta = Math.Min(beta, val);
                if (beta <= alpha) break;
            }
            return val;
        }
    }

    int Eval(ReversiGame g, Piece ai)
    {
        var score = 0;
        for (var y = 0; y < ReversiGame.Size; y++)
            for (var x = 0; x < ReversiGame.Size; x++)
                score += g.Board[x, y] switch
                {
                    var p when p == ai => 1,
                    var p when p == g.Opponent(ai) => -1,
                    _ => 0
                };
        return score;
    }
}
