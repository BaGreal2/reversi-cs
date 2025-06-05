using System.Collections.Generic;
using System.Linq;

namespace Reversi.Models;

public enum Piece { None, Black, White }

public class ReversiGame
{
    public const int Size = 8;
    public Piece[,] Board { get; private set; }
    public Piece CurrentPlayer { get; private set; } = Piece.Black;

    private static readonly (int dx, int dy)[] Directions = new (int, int)[]
    {
        (-1,-1), (-1,0), (-1,1),
        (0,-1),         (0,1),
        (1,-1), (1,0), (1,1)
    };

    public ReversiGame()
    {
        Board = new Piece[Size, Size];
        InitBoard();
    }

    private void InitBoard()
    {
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                Board[x, y] = Piece.None;

        Board[3, 3] = Piece.White;
        Board[4, 4] = Piece.White;
        Board[3, 4] = Piece.Black;
        Board[4, 3] = Piece.Black;

        CurrentPlayer = Piece.Black;
    }

    public bool IsValidMove(int x, int y, Piece player)
    {
        if (Board[x, y] != Piece.None) return false;

        foreach (var (dx, dy) in Directions)
        {
            int nx = x + dx, ny = y + dy;
            bool hasOpponent = false;
            while (InBounds(nx, ny) && Board[nx, ny] == Opponent(player))
            {
                nx += dx; ny += dy;
                hasOpponent = true;
            }
            if (hasOpponent && InBounds(nx, ny) && Board[nx, ny] == player)
                return true;
        }
        return false;
    }

    public List<(int x, int y)> GetValidMoves(Piece player)
    {
        var moves = new List<(int, int)>();
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                if (IsValidMove(i, j, player))
                    moves.Add((i, j));
        return moves;
    }

    public bool MakeMove(int x, int y)
    {
        if (!IsValidMove(x, y, CurrentPlayer)) return false;

        Board[x, y] = CurrentPlayer;
        foreach (var (dx, dy) in Directions)
        {
            int nx = x + dx, ny = y + dy;
            var toFlip = new List<(int, int)>();
            while (InBounds(nx, ny) && Board[nx, ny] == Opponent(CurrentPlayer))
            {
                toFlip.Add((nx, ny));
                nx += dx; ny += dy;
            }
            if (InBounds(nx, ny) && Board[nx, ny] == CurrentPlayer)
            {
                foreach (var (fx, fy) in toFlip)
                    Board[fx, fy] = CurrentPlayer;
            }
        }

        CurrentPlayer = Opponent(CurrentPlayer);
        return true;
    }

    public bool IsGameOver =>
        !GetValidMoves(Piece.Black).Any() && !GetValidMoves(Piece.White).Any();

    public Piece? GetWinner()
    {
        int black = 0, white = 0;
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                if (Board[x, y] == Piece.Black) black++;
                else if (Board[x, y] == Piece.White) white++;
            }

        if (black == white) return null;
        return black > white ? Piece.Black : Piece.White;
    }

    public void Reset()
    {
        InitBoard();
    }

    public Piece Opponent(Piece p) =>
        p == Piece.Black ? Piece.White : Piece.Black;

    private bool InBounds(int x, int y) =>
        x >= 0 && y >= 0 && x < Size && y < Size;
}
