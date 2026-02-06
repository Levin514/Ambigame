using Godot;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Snake;

public partial class ReforestationSnake : Node2D
{
	// Señales para comunicarse con el GameLayout
	[Signal] public delegate void GameOverEventHandler(int score, int treesPlanted, int time, string reason);
	[Signal] public delegate void VictoryEventHandler(int score, int treesPlanted, int time);
	[Signal] public delegate void PlantAttemptEventHandler(string action); // "seed" o "water"
	[Signal] public delegate void PlantGrownEventHandler();
	[Signal] public delegate void CheckAvailableActionsEventHandler();
	
	// To generate random numbers.
	private static readonly Random rnd = new();

	[Export] DualGridTilemap DualGrid;
	[Export] private SnakeBody _snakeBody;
	[Export] private AudioStreamPlayer gameMusic;

	// We could use a Godot Timer too.
	private Timer timer;
	private bool isGameOver = false;
	
	// Sistema de tracking de plantas
	private int treesPlanted = 0; // Árboles completamente crecidos
	private int plantsDead = 0;
	private string gameOverReason = ""; // Razón de derrota
	private const int MAX_DEAD_PLANTS = 5; // Pierde si mueren 5 plantas
	private const int POINTS_PER_TREE = 20; // Puntos por cada árbol que crece completamente
	private const int POINTS_LOST_PER_DEATH = 10; // Puntos perdidos por cada planta muerta

	public override void _Ready()
	{
		isGameOver = false;
		
		// Generar 10 lugares de plantación separados
		DualGrid.GeneratePlantSpots(10);
		
		// Conectar señal de crecimiento de plantas
		DualGrid.PlantGrown += OnPlantGrown;
		
		// Conectar señal de muerte de plantas
		DualGrid.PlantDied += OnPlantDied;
		
		// Conectar señales del ReforestationSystem
		var reforestationSystem = GetNodeOrNull("../GameLayout/ReforestationSystem");
		if (reforestationSystem != null)
		{
			reforestationSystem.Connect("Victory", new Callable(this, nameof(OnVictory)));
			reforestationSystem.Connect("GameOver", new Callable(this, nameof(OnReforestationSystemGameOver)));
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
			EmitSignal(SignalName.GameOver, _snakeBody.Puntuacion, treesPlanted, (int)_snakeBody.juegoTime, gameOverReason);
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
			EmitSignal(SignalName.Victory, _snakeBody.Puntuacion, treesPlanted, (int)_snakeBody.juegoTime);
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
		
		// Solo emitir señal, no dar puntos al plantar
		EmitSignal(SignalName.PlantAttempt, action);
	}
	
	private void OnPlantGrown()
	{
		treesPlanted++;
		
		// Sumar puntos cuando el árbol crece completamente
		if (_snakeBody != null)
		{
			_snakeBody.AddScore(POINTS_PER_TREE);
			GD.Print($"ReforestationSnake: Árbol completamente crecido! +{POINTS_PER_TREE} puntos (Total árboles: {treesPlanted})");
		}
		
		EmitSignal(SignalName.PlantGrown);
	}
	
	private void OnPlantDied()
	{
		if (isGameOver) return;
		
		plantsDead++;
		
		// Restar puntos cuando una planta muere
		if (_snakeBody != null)
		{
			_snakeBody.AddScore(-POINTS_LOST_PER_DEATH);
			GD.Print($"ReforestationSnake: Planta murió! -{POINTS_LOST_PER_DEATH} puntos (Total muertas: {plantsDead}/{MAX_DEAD_PLANTS})");
		}
		
		// Si murieron 5 plantas, pierde el juego
		if (plantsDead >= MAX_DEAD_PLANTS)
		{
			gameOverReason = "plants_died";
			GD.Print("ReforestationSnake: Game Over - Demasiadas plantas muertas");
			OnGameOver();
		}

		EmitSignal(SignalName.CheckAvailableActions);
	}
	
	private void OnReforestationSystemGameOver(string reason)
	{
		gameOverReason = reason;
		GD.Print($"ReforestationSnake: Game Over desde ReforestationSystem - Razón: {reason}");
		OnGameOver();
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
