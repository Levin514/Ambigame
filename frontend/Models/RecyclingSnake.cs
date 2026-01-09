using Godot;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Snake;

/// <summary>
/// Script principal del nivel de reciclaje.
/// Emite señales para comunicarse con el GameLayout.
/// </summary>
public partial class RecyclingSnake : Node2D
{
	[Signal] public delegate void GameOverEventHandler(int score, int recycled, int time);
	[Signal] public delegate void VictoryEventHandler(int score, int recycled, int time);
	
	private static readonly Random rnd = new();

	[Export] private DualGridTilemap DualGrid;
	[Export] private SnakeBody _snakeBody;
	[Export] private AudioStreamPlayer gameMusic;

	private Timer timer;
	private bool isGameOver = false;

	public override void _Ready()
	{
		isGameOver = false;
		timer = new Timer(4000);
		timer.Elapsed += NewApple;
		timer.AutoReset = true;
		timer.Start();

		// Conectamos a las señales de GameOver del SnakeBody
		_snakeBody.GameOver += OnGameOver;
		
		// Detenemos la música del fondo
		var musicManager = GetNode<Node>("/root/MusicManager");
		if (musicManager != null)
		{
			var audioPlayer = musicManager.GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			if (audioPlayer != null)
			{
				audioPlayer.Stop();
			}
		}
	}

	public override void _Process(double delta)
	{
	}

	public void OnGameOver()
	{
		GD.Print("RecyclingSnake: OnGameOver llamado");
		if (isGameOver) return;
		
		isGameOver = true;
		timer.Stop();
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Emitir señal de Game Over con estadísticas
		if (_snakeBody != null)
		{
			EmitSignal(SignalName.GameOver, _snakeBody.Puntuacion, _snakeBody.Reciclados, (int)_snakeBody.juegoTime);
		}
	}

	public void NewApple(object src, ElapsedEventArgs e)
	{
		if (isGameOver) return;
		// No generar basura si el juego está pausado
		if (GetTree().Paused) return;
		
		var bounds = DualGrid.GetMapBounds();
		DualGrid.AddTrash(new Vector2I(rnd.Next(0, bounds.X + 1), rnd.Next(0, bounds.Y + 1)));
	}
}
