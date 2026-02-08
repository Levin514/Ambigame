using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake;

public partial class ClassifyLevel : Node2D
{
	[Signal] public delegate void GameOverEventHandler(int score, int recycled, int time);
	[Signal] public delegate void VictoryEventHandler(int score, int recycled, int time);
	[Signal] public delegate void ScoreUpdatedEventHandler(int score);
	[Signal] public delegate void TimeUpdatedEventHandler(int time);
	[Signal] public delegate void RecycledUpdatedEventHandler(int recycled);

	// Referencias a nodos
	private Sprite2D container1;
	private Sprite2D container2;
	private Sprite2D container3;
	private Sprite2D container4;
	private Node2D playerAnimation;
	[Export] private Classifier classifier;
	[Export] private AudioStreamPlayer gameMusic;

	// Propiedades con signals
	private int _score = 0;
	public int Score
	{
		get => _score;
		set
		{
			_score = value;
			EmitSignal(SignalName.ScoreUpdated, _score);
		}
	}

	private int _recycled = 0;
	public int Recycled
	{
		get => _recycled;
		set
		{
			_recycled = value;
			EmitSignal(SignalName.RecycledUpdated, _recycled);
		}
	}

	private int _elapsedTime = 0;
	public int ElapsedTime
	{
		get => _elapsedTime;
		set
		{
			_elapsedTime = value;
			EmitSignal(SignalName.TimeUpdated, _elapsedTime);
		}
	}

	// Control
	private float timeAccumulator = 0f;
	private bool gameOver = false;
	private Vector2 initialPosition;
	private bool isMoving = false;
	private Vector2 targetPosition;
	private float movementSpeed = 300f;
	private AnimatedSprite2D playerSprite;
	private String currentDirection = "";
	private bool isWaiting = false;

	public bool GetIsMoving()
	{
		return isMoving;
	}

	public bool GetIsWaiting()
	{
		return isWaiting;
	} 

	private Camera2D _camera;

	public override void _Ready()
	{
		//Temporal solution
		_camera = GetNode<Camera2D>("/root/GameLayout/VBoxContainer/GameContainer/SubViewportContainer/SubViewport/Camera2D");
		_camera.Zoom = new Vector2I(1,1);

		// Detener la música del menú
		var musicManager = GetNode<Node>("/root/MusicManager");
		if (musicManager != null)
		{
			var audioPlayer = musicManager.GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			if (audioPlayer != null)
			{
				audioPlayer.Stop();
			}
		}

		// Obtener referencias
		container1 = GetNode<Sprite2D>("Container1");
		container2 = GetNode<Sprite2D>("Container2");
		container3 = GetNode<Sprite2D>("Container3");
		container4 = GetNode<Sprite2D>("Container4");
		playerAnimation = GetNode<Node2D>("PlayerAnimation");
		classifier = GetNode<Classifier>("Node2D");

		// Obtener el AnimatedSprite2D del playerAnimation
		playerSprite = playerAnimation.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Posición inicial (Container4)
		initialPosition = container4.Position;
		playerAnimation.Position = initialPosition;

		Score = 0;
		Recycled = 0;
		ElapsedTime = 60;

		// Mostrar sprite mirando hacia abajo
		if (playerSprite != null)
		{
			playerSprite.Animation = "walk_down";
			playerSprite.Frame = 0;
		}

		GD.Print("ClassifyLevel iniciado");
	}

	public override void _Process(double delta)
	{
		if (gameOver)
			return;

		// Actualizar tiempo
		timeAccumulator += (float)delta;
		if (timeAccumulator >= 1f)
		{
			ElapsedTime--;
			timeAccumulator = 0f;

			// Verificar derrota por tiempo
			if (ElapsedTime <= 0)
			{
				TriggerGameOver();
				return;
			}
		}

		// Mover el jugador hacia el objetivo
		if (isMoving)
		{
			MovePlayerToTarget((float)delta);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (gameOver || isMoving || isWaiting)
			return;

		String direction = "";

		if (@event.IsActionPressed("ui_left"))
		{
			direction = "left";
		}
		else if (@event.IsActionPressed("ui_right"))
		{
			direction = "right";
		}
		else if (@event.IsActionPressed("ui_up"))
		{
			direction = "up";
		}

		if (direction != "")
		{
			// Iniciar movimiento
			currentDirection = direction;
			targetPosition = GetContainerPosition(direction);
			isMoving = true;
			
			if (playerSprite != null)
			{
				playerSprite.Play("walk_" + direction);
			}
		}
	}

	private Vector2 GetContainerPosition(String direction)
	{
		return direction switch
		{
			"left" => container1.Position,
			"right" => container2.Position,
			"up" => container3.Position,
			_ => initialPosition
		};
	}

	private void MovePlayerToTarget(float delta)
	{
		Vector2 direction = (targetPosition - playerAnimation.Position).Normalized();
		playerAnimation.Position += direction * movementSpeed * delta;

		// Verificar si llegó al objetivo
		if (playerAnimation.Position.DistanceTo(targetPosition) < 10f)
		{
			playerAnimation.Position = targetPosition;
			OnContainerHit();
		}
	}

	private void OnContainerHit()
	{
		isMoving = false;
		isWaiting = true;
		
		// Detener la animación
		if (playerSprite != null)
		{
			playerSprite.Stop();
		}
		
		// Verificar si la clasificación es correcta
		bool isCorrect = classifier.GetCorrectClassification();
		
		// Aplicar puntuación
		if (isCorrect)
		{
			Score += 100;
			Recycled += 1;
			// Reproducir efecto de acierto
			var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
			if (audioManager != null)
			{
				audioManager.PlayTrashCollect();
			}
		}
		else
		{
			Score -= 50;
			// Reproducir efecto de error
			var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
			if (audioManager != null)
			{
				audioManager.PlayBreakPipe();
			}
		}
		
		// Mostrar sprite de resultado (greencheck o redcross)
		ShowResultSprite(isCorrect);

		// Volver a posición inicial después de 1 segundo
		GetTree().CreateTimer(1.0).Timeout += ReturnToInitialPosition;
	}


	private void ShowResultSprite(bool isCorrect)
	{
		// Crear sprite temporal
		Sprite2D resultSprite = new Sprite2D();
		String spriteName = isCorrect ? "greencheck" : "redcross";
		resultSprite.Texture = GD.Load<Texture2D>("res://Assets/" + spriteName + ".png");
		resultSprite.Scale = new Vector2(0.025f, 0.025f);
		
		// Posicionar en la cabeza del jugador (arriba del playerAnimation)
		Vector2 spritePosition = new Vector2(0, -20f); // Relativo al playerAnimation
		resultSprite.Position = spritePosition;
		
		// Añadir como hijo del playerAnimation para que se mueva con él
		playerAnimation.AddChild(resultSprite);
		
		// Animar: elevarse y desvanecerse en paralelo
		Tween tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(resultSprite, "position:y", spritePosition.Y - 15f, 0.5f); // Elevarse
		tween.TweenProperty(resultSprite, "modulate:a", 0f, 1f); // Desvanecerse
		
		GetTree().CreateTimer(1).Timeout += () =>
		{
			if (tween != null)
			{
				tween.Kill();
			}
			
			if (resultSprite != null && IsInstanceValid(resultSprite))
			{
				resultSprite.QueueFree();
			}
		};
	}

	private void ReturnToInitialPosition()
	{
		playerAnimation.Position = initialPosition;
		isWaiting = false;
		
		// Mostrar sprite mirando hacia abajo
		if (playerSprite != null)
		{
			playerSprite.Animation = "walk_down";
			playerSprite.Frame = 0;
		}

		// Verificar condición de victoria después de regresar
		if (classifier != null && classifier.GetHasTrash())
		{
			TriggerVictory();
		}
	}

	public void OnVictory(int finalScore, int totalRecycled, int time)
	{
		gameOver = true;
		GD.Print($"ClassifyLevel: ¡VICTORIA! Score: {finalScore}, Reciclados: {totalRecycled}, Tiempo: {time}");
		GameData.Instance.globalSeedsAmount += CalculateSeedsAmount();
		EmitSignal(SignalName.Victory, finalScore, totalRecycled, time);
	}

	private int CalculateSeedsAmount()
	{
		double puntuacionFinal = Score;
		double divisor = GameData.Instance.globalSeedsDivisor;
		return (int)Math.Ceiling(puntuacionFinal / divisor);
	}

	private void TriggerGameOver()
	{
		gameOver = true;
		
		// Detener música del nivel
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		GD.Print($"ClassifyLevel: ¡JUEGO TERMINADO! Score: {Score}, Reciclados: {Recycled}, Tiempo: {ElapsedTime}");
		GameData.Instance.globalSeedsAmount += CalculateSeedsAmount();
		GameData.Instance.classifyLevelCompleted = true; // Marcar nivel como completado
		EmitSignal(SignalName.GameOver, Score, Recycled, ElapsedTime);
	}

	private void TriggerVictory()
	{
		gameOver = true;
		
		// Detener música del nivel
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		GD.Print($"ClassifyLevel: ¡VICTORIA! Score: {Score}, Reciclados: {Recycled}, Tiempo: {ElapsedTime}");
		GameData.Instance.classifyLevelCompleted = true; // Marcar nivel como completado
		EmitSignal(SignalName.Victory, Score, Recycled, ElapsedTime);
	}

	public void InitializeSignals()
	{
		EmitSignal(SignalName.ScoreUpdated, Score);
		EmitSignal(SignalName.RecycledUpdated, Recycled);
		EmitSignal(SignalName.TimeUpdated, ElapsedTime);
		GD.Print("ClassifyLevel: Señales inicializadas");
	}
}
