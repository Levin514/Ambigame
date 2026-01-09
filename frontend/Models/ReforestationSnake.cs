using Godot;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Snake;

public partial class ReforestationSnake : Node2D
{
	// Señales para comunicarse con el GameLayout
	[Signal] public delegate void GameOverEventHandler(int score, int time);
	[Signal] public delegate void VictoryEventHandler(int score, int time);
	[Signal] public delegate void PlantAttemptEventHandler();
	
	// To generate random numbers.
	private static readonly Random rnd = new();

	[Export] DualGridTilemap DualGrid;
	[Export] private SnakeBody _snakeBody;
	[Export] private AudioStreamPlayer gameMusic;

	// We could use a Godot Timer too.
	private Timer timer;
	private bool isGameOver = false;

	public override void _Ready()
	{
		isGameOver = false;
		timer = new Timer(4000);
		timer.Elapsed += NewHole;
		timer.AutoReset = true;
		timer.Start();

		// Conectamos a las señales del SnakeBody
		_snakeBody.GameOver += OnGameOver;
		_snakeBody.PipeRepaired += OnPlantAttempt; // Reutilizamos la señal para intentos de plantación
		
		// Detenemos la música del menú
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
		GD.Print("ReforestationSnake: OnGameOver llamado");
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
			EmitSignal(SignalName.GameOver, _snakeBody.Puntuacion, (int)_snakeBody.juegoTime);
		}
	}

	public void OnVictory()
	{
		GD.Print("ReforestationSnake: OnVictory llamado");
		if (isGameOver) return;
		
		isGameOver = true;
		timer.Stop();
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Emitir señal de Victoria con estadísticas
		if (_snakeBody != null)
		{
			EmitSignal(SignalName.Victory, _snakeBody.Puntuacion, (int)_snakeBody.juegoTime);
		}
	}

	public void NewHole(object src, ElapsedEventArgs e)
	{
		if (isGameOver) return;
		// No generar huecos si el juego está pausado
		if (GetTree().Paused) return;
		var bounds = DualGrid.GetMapBounds();
		DualGrid.AddTrash(new Vector2I(rnd.Next(0, bounds.X + 1), rnd.Next(0, bounds.Y + 1)));
	}
	
	private void OnPlantAttempt()
	{
		GD.Print("ReforestationSnake: Intento de plantar, emitiendo señal");
		EmitSignal(SignalName.PlantAttempt);
	}

	public void OnContinuarPressed()
	{
		// Regresar al menú principal
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}
	
	public void OnAgainPressed()
	{
		GetTree().ReloadCurrentScene();
	}

	public void OnSalirPressed()
	{
		// Detener la música del juego
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Reanudar música del menú
		var musicManager = GetNode<Node>("/root/MusicManager");
		if (musicManager != null)
		{
			var audioPlayer = musicManager.GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			if (audioPlayer != null && !audioPlayer.Playing)
			{
				audioPlayer.Play();
			}
		}
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}
}
