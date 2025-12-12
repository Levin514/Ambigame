using Godot;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Snake;

public partial class WaterSnake : Node2D
{
	// To generate random numbers.
	private static readonly Random rnd = new();

	[Export] DualGridTilemap DualGrid;
	// Scenes
	private Vector2I _gameSize;
	[Export] private SnakeBody _snakeBody;
	[Export] private AudioStreamPlayer gameMusic;
	[Export] private CanvasLayer gameOverScreen;
	[Export] private CanvasLayer victoryScreen;
	[Export] private Label gameOverStatsLabel;
	[Export] private Label victoryStatsLabel;

	// We could use a Godot Timer too.
	private Timer timer;
	private bool isGameOver = false;

	public override void _Ready()
	{
		_gameSize = new Vector2I(33, 21);
		isGameOver = false;
		timer = new Timer(4000);
		timer.Elapsed += NewApple;
		timer.AutoReset = true;
		timer.Start();

		// Conectamos a las señales de GameOver del SnakeBody y WaterSystem
		_snakeBody.GameOver += OnGameOver;
		
		// Conectar a las señales del WaterSystem
		var waterSystem = GetNode<WaterSystem>("WaterSystemScreen/WaterSystem");
		if (waterSystem != null)
		{
			GD.Print("WaterSystem encontrado, conectando señales");
			waterSystem.GameOver += OnGameOver;
			waterSystem.Victory += OnVictory;
		}
		else
		{
			GD.Print("ERROR: WaterSystem no encontrado!");
		}
		
		// Detenemos la música sólo al momento de jugar
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
		GD.Print("WaterSnake: OnGameOver llamado");
		if (isGameOver) return; // Evitar ejecutar múltiples veces
		
		isGameOver = true;
		timer.Stop();
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Mostrar pantalla de Game Over
		if (gameOverScreen != null)
		{
			GD.Print("WaterSnake: Mostrando GameOverScreen");
			gameOverScreen.Visible = true;
			if (gameOverStatsLabel != null && _snakeBody != null)
			{
				gameOverStatsLabel.Text = $"Puntuación: {_snakeBody.Puntuacion}\nTiempo: {_snakeBody.juegoTime} segundos";
			}
		}
		else
		{
			GD.Print("WaterSnake: ERROR - gameOverScreen es null!");
		}
	}

	public void OnVictory()
	{
		GD.Print("WaterSnake: OnVictory llamado");
		if (isGameOver) return; // Evitar ejecutar múltiples veces
		
		isGameOver = true;
		timer.Stop();
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Mostrar pantalla de Victoria
		if (victoryScreen != null)
		{
			GD.Print("WaterSnake: Mostrando VictoryScreen");
			victoryScreen.Visible = true;
			if (victoryStatsLabel != null && _snakeBody != null)
			{
				victoryStatsLabel.Text = $"Puntuación: {_snakeBody.Puntuacion}\nTiempo: {_snakeBody.juegoTime} segundos";
			}
		}
		else
		{
			GD.Print("WaterSnake: ERROR - victoryScreen es null!");
		}
	}

	public void NewApple(object src, ElapsedEventArgs e)
	{
		if (isGameOver) return;
		// No generar basura si el juego está pausado
		if (GetTree().Paused) return;
		DualGrid.AddTrash(new Vector2I(rnd.Next(0, 32), rnd.Next(0, 21)));
	}


	public void OnContinuarPressed()
	{
		// Por ahora regresa al menú principal, después irá al minijuego
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
		
		// Al salir, reanudamos la música como que no ha pasado nada
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
