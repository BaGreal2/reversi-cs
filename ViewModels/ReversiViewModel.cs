using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Reversi.Models;
using Avalonia.Threading;
using System.Linq;

namespace Reversi.ViewModels;

public enum Difficulty { Easy, Medium, Hard }

public class CellViewModel : INotifyPropertyChanged
{
    public int X { get; }
    public int Y { get; }

    Piece _piece;
    bool _valid;

    public Piece Piece
    {
        get => _piece;
        set { _piece = value; OnPropertyChanged(); }
    }
    public bool IsValidMove
    {
        get => _valid;
        set { _valid = value; OnPropertyChanged(); }
    }

    public CellViewModel(int x, int y) { X = x; Y = y; }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public sealed class ReversiViewModel : ViewModelBase
{
    readonly ReversiGame _game = new();
    readonly ReversiAI   _ai   = new();

    public ObservableCollection<CellViewModel> Cells { get; } = new();
    public ObservableCollection<Difficulty> Difficulties { get; } =
        new() { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };

    Difficulty _diff = Difficulty.Medium;
    public Difficulty SelectedDifficulty
    {
        get => _diff;
        set => SetProperty(ref _diff, value);
    }

    public ICommand CellClickCommand    { get; }
    public ICommand GiveUpCommand       { get; } 
    public ICommand StartGameCommand    { get; }
    public ICommand RestartGameCommand  { get; }
    public ICommand ReturnToMenuCommand { get; }

    bool _showMenu = true, _showGameOver;
    string _winner = "";

    public bool IsMainMenuVisible  { get => _showMenu;      set { SetProperty(ref _showMenu, value);      OnPropertyChanged(nameof(IsGameBoardVisible)); } }
    public bool IsGameOverVisible  { get => _showGameOver;   set { SetProperty(ref _showGameOver, value);  OnPropertyChanged(nameof(IsGameBoardVisible)); } }
    public bool IsGameBoardVisible => !IsMainMenuVisible && !IsGameOverVisible;

    public string Winner
    {
        get => _winner;
        set { if (SetProperty(ref _winner, value)) OnPropertyChanged(nameof(GameResultText)); }
    }
    public string GameResultText => Winner switch
    {
        "Black" => "Black wins!",
        "White" => "White wins!",
        _       => "It's a tie!"
    };

    public ReversiViewModel()
    {
        for (int y = 0; y < ReversiGame.Size; y++)
            for (int x = 0; x < ReversiGame.Size; x++)
                Cells.Add(new CellViewModel(x, y));

        CellClickCommand    = new RelayCommand<CellViewModel>(OnCell);
        GiveUpCommand       = new RelayCommand(GiveUp);
        StartGameCommand    = new RelayCommand(Start);
        RestartGameCommand  = new RelayCommand(Restart);
        ReturnToMenuCommand = new RelayCommand(ReturnToMenu);

        Refresh();
    }

    /* ---------- PLAYER CLICK ---------- */
    void OnCell(CellViewModel? c)
    {
        if (c is null || IsGameOverVisible) return;

        if (_game.MakeMove(c.X, c.Y))
        {
            Refresh();
            ContinueFlow();                // decide who moves next / pass if needed
        }
    }

    void GiveUp()
    {
        Winner            = "White";
        IsGameOverVisible = true;
    }

    /* ---------- AI TURN ---------- */
    async Task AiTurn()
    {
        // if AI has no legal moves, pass immediately
        if (!_game.GetValidMoves(_game.CurrentPlayer).Any())
        {
            _game.SkipTurn();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Refresh();
                ContinueFlow();
            });
            return;
        }

        var mv = _ai.GetMove(_game, Depth());
        if (mv.x >= 0) _game.MakeMove(mv.x, mv.y);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Refresh();
            ContinueFlow();
        });
    }

    /* ---------- GAME-FLOW CONTROL ---------- */
    void ContinueFlow()
    {
        if (_game.IsGameOver) { Finish(); return; }

        // if the side to move cannot play, auto-pass
        if (!_game.GetValidMoves(_game.CurrentPlayer).Any())
        {
            _game.SkipTurn();
            Refresh();

            if (_game.IsGameOver) { Finish(); return; }
        }

        // if it's now the AI’s turn, launch it
        if (_game.CurrentPlayer == Piece.White)
            _ = Task.Run(AiTurn);
    }

    /* ---------- UTIL ---------- */
    void Refresh()
    {
        var valids = _game.GetValidMoves(_game.CurrentPlayer);
        foreach (var cell in Cells)
        {
            cell.Piece       = _game.Board[cell.X, cell.Y];
            cell.IsValidMove = valids.Contains((cell.X, cell.Y));
        }
    }

    void Finish()
    {
        Winner = _game.GetWinner()?.ToString() ?? "None";
        IsGameOverVisible = true;
    }

    void Start()
    {
        _game.Reset();
        Refresh();
        IsGameOverVisible = false;
        IsMainMenuVisible = false;
        ContinueFlow();                    // in case Black starts with no moves (rare)
    }

    void Restart()      => Start();
    void ReturnToMenu() { IsMainMenuVisible = true; IsGameOverVisible = false; }

    int Depth() => SelectedDifficulty switch
    { Difficulty.Easy => 2, Difficulty.Medium => 4, _ => 6 };
}
