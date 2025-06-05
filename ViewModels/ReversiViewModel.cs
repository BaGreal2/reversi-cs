using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Reversi.Models;

namespace Reversi.ViewModels;

public class CellViewModel : INotifyPropertyChanged
{
    private Piece _piece;
    private bool _isValidMove;

    public int X { get; }
    public int Y { get; }

    public Piece Piece
    {
        get => _piece;
        set { _piece = value; OnPropertyChanged(); }
    }

    public bool IsValidMove
    {
        get => _isValidMove;
        set { _isValidMove = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public CellViewModel(int x, int y)
    {
        X = x; Y = y;
        Piece = Piece.None;
    }
}

public class ReversiViewModel : ViewModelBase
{
    private readonly ReversiGame _game = new();

    public ObservableCollection<CellViewModel> Cells { get; }

    public ICommand CellClickCommand { get; }
    public ICommand StartGameCommand { get; }
    public ICommand RestartGameCommand { get; }
    public ICommand ReturnToMenuCommand { get; }

    private bool _isMainMenuVisible = true;
    private bool _isGameOverVisible = false;
    private string _winner = "";

    public bool IsMainMenuVisible
    {
        get => _isMainMenuVisible;
        set => SetProperty(ref _isMainMenuVisible, value);
    }

    public bool IsGameOverVisible
    {
        get => _isGameOverVisible;
        set => SetProperty(ref _isGameOverVisible, value);
    }

    public bool IsGameBoardVisible => !IsMainMenuVisible && !IsGameOverVisible;

    public string Winner
    {
        get => _winner;
        set
        {
            if (SetProperty(ref _winner, value))
            {
                OnPropertyChanged(nameof(GameResultText));
            }
        }
    }

    public string GameResultText => Winner switch
    {
        "Black" => "Black wins!",
        "White" => "White wins!",
        _ => "It's a tie!"
    };

    public ReversiViewModel()
    {
        Cells = new ObservableCollection<CellViewModel>();
        for (int y = 0; y < ReversiGame.Size; y++)
            for (int x = 0; x < ReversiGame.Size; x++)
                Cells.Add(new CellViewModel(x, y));

        CellClickCommand = new RelayCommand<CellViewModel>(CellClicked);
        StartGameCommand = new RelayCommand(StartGame);
        RestartGameCommand = new RelayCommand(RestartGame);
        ReturnToMenuCommand = new RelayCommand(ReturnToMenu);

        RefreshBoard();
    }

    public void CellClicked(CellViewModel? cell)
    {
        if (cell is null || IsGameOverVisible) return;

        if (_game.MakeMove(cell.X, cell.Y))
        {
            RefreshBoard();

            if (_game.IsGameOver)
            {
                Winner = _game.GetWinner()?.ToString() ?? "None";
                IsGameOverVisible = true;
                OnPropertyChanged(nameof(IsGameBoardVisible));
            }
        }
    }

    private void RefreshBoard()
    {
        var validMoves = _game.GetValidMoves(_game.CurrentPlayer);
        foreach (var cell in Cells)
        {
            cell.Piece = _game.Board[cell.X, cell.Y];
            cell.IsValidMove = validMoves.Contains((cell.X, cell.Y));
        }
    }

    private void StartGame()
    {
        _game.Reset();
        RefreshBoard();
        IsMainMenuVisible = false;
        IsGameOverVisible = false;
        OnPropertyChanged(nameof(IsGameBoardVisible));
    }

    private void RestartGame()
    {
        _game.Reset();
        RefreshBoard();
        IsGameOverVisible = false;
        OnPropertyChanged(nameof(IsGameBoardVisible));
    }

    private void ReturnToMenu()
    {
        IsMainMenuVisible = true;
        IsGameOverVisible = false;
        OnPropertyChanged(nameof(IsGameBoardVisible));
    }
}
