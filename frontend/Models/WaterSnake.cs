using Godot;
using System;

namespace Snake;

public partial class WaterSnake : Node2D
{
	// Señales para comunicarse con el GameLayout
	[Signal] public delegate void GameOverEventHandler(int score, int pipesRepaired, int time);
	[Signal] public delegate void VictoryEventHandler(int score, int pipesRepaired, int time);
	[Signal] public delegate void PipeRepairedEventHandler(string action);
	[Signal] public delegate void PipeBrokenEventHandler();
	
	// To generate random numbers.
	private static readonly Random rnd = new();
	
	// Contador de tuberías reparadas
	private int pipesRepaired = 0;

	[Export] DualGridTilemap DualGrid;
	// Scenes
	private Vector2I _gameSize;
	[Export] private SnakeBody _snakeBody;
	[Export] private AudioStreamPlayer gameMusic;

	private bool isGameOver = false;
	private Timer degradationTimer;

	public override void _Ready()
	{
		isGameOver = false;
		
		// Generar 3-4 redes de tuberías conectadas desde el inicio
		var bounds = DualGrid.GetMapBounds();
		int pipeNetworks = rnd.Next(3, 5); // 3 a 4 redes
		GD.Print($"WaterSnake: Generando {pipeNetworks} redes de tuberías");
		
		for (int i = 0; i < pipeNetworks; i++)
		{
			Vector2I start = new Vector2I(rnd.Next(0, bounds.X + 1), rnd.Next(0, bounds.Y + 1));
			DualGrid.PlacePipeNetwork(start, rnd.Next(5, 9)); // 5-8 segmentos cada red
		}

		// Conectamos a las señales de GameOver del SnakeBody
		_snakeBody.GameOver += OnGameOver;
		_snakeBody.PipeRepaired += OnPipeRepaired;
		
		// Inicializar timer de degradación de tuberías
		degradationTimer = new Timer();
		degradationTimer.WaitTime = rnd.Next(8, 13); // 8-12 segundos
		degradationTimer.Timeout += OnDegradationTimeout;
		AddChild(degradationTimer);
		degradationTimer.Start();
		
		// Conectar señal de tubería rota del DualGrid
		DualGrid.PipeBroken += OnPipeBroken;
		
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
		if (degradationTimer != null)
		{
			degradationTimer.Stop();
		}
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Emitir señal de Game Over con estadísticas (puntaje, tuberías reparadas, tiempo)
		if (_snakeBody != null)
		{
			EmitSignal(SignalName.GameOver, _snakeBody.Puntuacion, pipesRepaired, (int)_snakeBody.juegoTime);
		}
	}

	public void OnVictory(int waterScore)
	{
		GD.Print($"WaterSnake: OnVictory llamado con waterScore: {waterScore}");
		if (isGameOver) return; // Evitar ejecutar múltiples veces
		
		isGameOver = true;
		if (degradationTimer != null)
		{
			degradationTimer.Stop();
		}
		
		isGameOver = true;
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Emitir señal de Victoria con estadísticas (puntaje, tuberías reparadas, tiempo)
		// El waterScore ya incluye la bonificación del WaterSystem
		if (_snakeBody != null)
		{
			EmitSignal(SignalName.Victory, waterScore, pipesRepaired, (int)_snakeBody.juegoTime);
		}
	}

	private void OnPipeRepaired(string action)
	{
		GD.Print("WaterSnake: Tubería reparada, emitiendo señal");
		pipesRepaired++;
		GD.Print($"WaterSnake: Total tuberías reparadas: {pipesRepaired}");
		EmitSignal(SignalName.PipeRepaired, action);
	}

	private void OnPipeBroken()
	{
		GD.Print("WaterSnake: Tubería rota, emitiendo señal");
		EmitSignal(SignalName.PipeBroken);
	}

	private void OnDegradationTimeout()
	{
		if (isGameOver) return;
		
		// Romper una tubería buena aleatoria
		Vector2I? goodPipe = DualGrid.GetRandomGoodPipe();
		if (goodPipe.HasValue)
		{
			DualGrid.BreakPipe(goodPipe.Value);
			GD.Print($"WaterSnake: Tubería degradada automáticamente en {goodPipe.Value}");
		}
		
		// Reiniciar timer con nuevo intervalo aleatorio
		degradationTimer.WaitTime = rnd.Next(8, 13);
		degradationTimer.Start();
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
