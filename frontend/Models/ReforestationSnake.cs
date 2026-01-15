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
	[Signal] public delegate void PlantAttemptEventHandler(string action); // "seed" o "water"
	[Signal] public delegate void PlantGrownEventHandler();
	
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
		
		// Generar 10 lugares de plantación separados
		DualGrid.GeneratePlantSpots(10);
		
		// Conectar señal de crecimiento de plantas
		DualGrid.PlantGrown += OnPlantGrown;
		
		// Conectar señales del ReforestationSystem
		var reforestationSystem = GetNodeOrNull("../GameLayout/ReforestationSystem");
		if (reforestationSystem != null)
		{
			reforestationSystem.Connect("Victory", new Callable(this, nameof(OnVictory)));
			reforestationSystem.Connect("GameOver", new Callable(this, nameof(OnGameOver)));
			GD.Print("ReforestationSnake: Señales de ReforestationSystem conectadas");
		}
		else
		{
			GD.PrintErr("ReforestationSnake: No se encontró ReforestationSystem");
		}
		
		// NO generar huecos aleatorios en reforestación
		// timer = new Timer(4000);
		// timer.Elapsed += NewHole;
		// timer.AutoReset = true;
		// timer.Start();

		// Conectamos a las señales del SnakeBody
		_snakeBody.GameOver += OnGameOver;
		_snakeBody.PipeRepaired += OnPlantAction;
		
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
		if (timer != null)
		{
			timer.Stop();
		}
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Reproducir efecto de sonido de derrota
		var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
		if (audioManager != null)
		{
			audioManager.PlayDefeat();
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
		if (timer != null)
		{
			timer.Stop();
		}
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Reproducir efecto de sonido de victoria
		var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
		if (audioManager != null)
		{
			audioManager.PlayVictory();
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
	
	private void OnPlantAction(string action)
	{
		GD.Print($"ReforestationSnake: Acción de plantación detectada - {action}");
		EmitSignal(SignalName.PlantAttempt, action);
	}
	
	private void OnPlantGrown()
	{
		GD.Print("ReforestationSnake: Planta creció completamente");
		EmitSignal(SignalName.PlantGrown);
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
