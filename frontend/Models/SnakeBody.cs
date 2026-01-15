using Godot;
using Newtonsoft.Json;
using Snakes.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Godot.TextServer;

namespace Snake;

public partial class SnakeBody : Sprite2D
{
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void UpdateHealthEventHandler();
	[Signal] public delegate void PipeRepairedEventHandler(string action);
	[Signal] public delegate void ScoreUpdatedEventHandler(int score);
	[Signal] public delegate void RecycledUpdatedEventHandler(int recycled);
	[Signal] public delegate void TimeUpdatedEventHandler(int time);
	
	[Export] DualGridTilemap DualGrid;

	//[Export] CanvasLayer gameOverScreen;
	[Export] PlayerAnimation player_ani;
	[Export] LifeSystem life_system;
	[Export] public AudioStream repairSound; // Sonido al reparar tubería
	[Export] public bool HasWalls = false; // Si true, chocar con bordes causa Game Over
	[Export] public bool HideBody = false; // Si true, no dibuja cuerpo ni crece (solo cabeza)
	[Export] public bool FreeMovement = false; // Si true, movimiento libre sin serpiente (reforestación)

	private LinkedList<Vector2I> _body;
	private LinkedList<Trash> trashList;
	private AudioStreamPlayer audioPlayer; // Reproductor de audio
	private bool _crash;
	private Direction _direction;
	private Direction _nextDirection;  // Buffer para la próxima dirección
	private double _time;

	private int reciclados = 0;
	public int Reciclados
	{
		get => reciclados;
		set
		{
			reciclados = value;
			EmitSignal(SignalName.RecycledUpdated, reciclados);
		}
	}

	private double puntuacionBase = 100.0;
	private int puntuacion = 0;
	public int Puntuacion
	{
		get => puntuacion;
		set
		{
			puntuacion = value;
			EmitSignal(SignalName.ScoreUpdated, puntuacion);
		}
	}

	private double elapsedTime = 0;
	public double juegoTime = 0;
	private void UpdateTimerLabel()
	{
		EmitSignal(SignalName.TimeUpdated, (int)juegoTime);
	}

	public override void _Ready()
	{
		// Inicializar reproductor de audio
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		
		trashList = new();
		DualGrid.TrashCollector += AddToTrashList;
		_direction = Direction.RIGHT;
		_nextDirection = Direction.RIGHT;  // Inicializar buffer
		
		// Inicializar posición según modo de movimiento
		if (FreeMovement)
		{
			// Modo libre: empezar en el centro del mapa
			var bounds = DualGrid.GetMapBounds();
			Vector2I centerPos = new Vector2I(bounds.X / 2, bounds.Y / 2);
			_body = new([centerPos]);
			GD.Print($"SnakeBody: Modo movimiento libre - Posición inicial: {centerPos}");
		}
		else
		{
			// Modo serpiente: posición tradicional
			_body = new([new(1, 0), new(0, 0)]);
		}
		
		ZIndex = 1;
		
		// Conectar al LifeSystem solo si existe (nivel de limpieza)
		if (life_system != null)
		{
			life_system.GameOver += OnLifeSystemGameOver;
		}
	}
	
	private void OnLifeSystemGameOver()
	{
		GD.Print("SnakeBody: OnLifeSystemGameOver - deteniendo movimiento");
		_crash = true;
		EmitSignal(SignalName.GameOver);
	}

	public override void _Draw()
	{
		if (HideBody) return; // No dibujar cuerpo si está oculto
		
		foreach (var pos in _body.Skip(1))
		{
			Vector2I coords = new() { X = pos.X, Y = pos.Y };
			DualGrid.SetTile(coords, DualGrid.grassPlaceholderAtlasCoord);
		}
	}

	public bool TryEat()
	{
		Debug.Assert(_body != null, nameof(_body) + " != null");
		var headPosition = _body.First.Value;
		
		// Detectar lugar de plantación (para nivel de reforestación)
		if (DualGrid.HasPlantSpotAt(headPosition))
		{
			var plantState = (int)DualGrid.Call("GetPlantState", headPosition);
			
			// 0 = Empty (puede plantar), 2 = NeedsWater (puede regar)
			if (plantState == 0) // PlantState.Empty
			{
				bool planted = (bool)DualGrid.Call("TryPlantSeed", headPosition);
				if (planted)
				{
					GD.Print("SnakeBody: Semilla plantada");
					EmitSignal(SignalName.PipeRepaired, "seed"); // Pasar "seed" como parámetro
					return true;
				}
			}
			else if (plantState == 2) // PlantState.NeedsWater
			{
				bool watered = (bool)DualGrid.Call("TryWaterPlant", headPosition);
				if (watered)
				{
					GD.Print("SnakeBody: Planta regada");
					EmitSignal(SignalName.PipeRepaired, "water"); // Pasar "water" como parámetro
					return true;
				}
			}
			
			return false;
		}
		
		// Detectar tubería y actuar según su estado
		if (DualGrid.HasPipeAt(headPosition))
		{
			if (DualGrid.IsPipeRepairedAt(headPosition))
			{
				// Tubería BUENA → ROMPERLA (penalización)
				DualGrid.BreakPipe(headPosition);
				GD.Print("SnakeBody: ¡Rompiste una tubería buena! Penalización.");
				// No reproducir sonido de reparación (es una penalización)
				return true;
			}
			else
			{
				// Tubería ROTA → REPARARLA (objetivo)
				DualGrid.RepairPipe(headPosition);
				
				// Reproducir sonido de reparación
				if (repairSound != null && audioPlayer != null)
				{
					audioPlayer.Stream = repairSound;
					audioPlayer.Play();
				}
				
				// Emitir señal de tubería reparada
				EmitSignal(SignalName.PipeRepaired, "pipe");
				GD.Print("SnakeBody: Tubería reparada correctamente.");
				
				return true;
			}
		}
		
		// Detectar basura (para niveles de reciclaje)
		if (DualGrid.HasTrashAt(headPosition))
		{
			Reciclados++;
			Puntuacion += (int)(puntuacionBase * (_body.Count / 10.0));
			DualGrid.RemoveTrashAt(headPosition);
			return true;
		}
		return false;
	}

	public void TryObstacle()
	{
		Debug.Assert(_body != null, nameof(_body) + " != null");
		var headPosition = _body.First.Value;
		if (DualGrid.HasRockAt(headPosition))
		{
			EmitSignal(SignalName.UpdateHealth);
			DualGrid.RemoveRockAt(headPosition);
		}
	}

	public bool Crash()
	{
		return _body
			.Skip(1)
			.Any(t =>
			{
				return t.X == _body.First.Value.X && t.Y == _body.First.Value.Y;
			});
	}

	public void AddToTrashList(Trash trash)
	{
		trashList.AddLast(trash);
		GD.Print(trashList.Count);
	}

	public override void _Process(double delta)
	{
		// Actualizar timer del juego
		elapsedTime += delta;
		if (elapsedTime > 1 && !_crash)
		{
			juegoTime++;
			UpdateTimerLabel();
			elapsedTime = 0;
		}
		
		// En modo FreeMovement, movimiento continuo mientras se mantiene presionada la tecla
		if (FreeMovement && !_crash)
		{
			_time += delta;
			
			// Velocidad de movimiento (ajustable)
			if (_time > 0.1) // Moverse cada 0.1 segundos
			{
				Direction newDirection = _direction;
				bool hasMoved = false;
				
				// Detectar qué tecla está presionada
				if (Input.IsActionPressed("ui_left"))
				{
					newDirection = Direction.LEFT;
					hasMoved = true;
				}
				else if (Input.IsActionPressed("ui_right"))
				{
					newDirection = Direction.RIGHT;
					hasMoved = true;
				}
				else if (Input.IsActionPressed("ui_up"))
				{
					newDirection = Direction.UP;
					hasMoved = true;
				}
				else if (Input.IsActionPressed("ui_down"))
				{
					newDirection = Direction.DOWN;
					hasMoved = true;
				}
				
				if (hasMoved)
				{
					// Aplicar movimiento
					_direction = newDirection;
					var translation = _direction switch
					{
						Direction.RIGHT => new Vector2I(1, 0),
						Direction.LEFT => new Vector2I(-1, 0),
						Direction.UP => new Vector2I(0, -1),
						Direction.DOWN => new Vector2I(0, 1),
						_ => new Vector2I(0, 0)
					};
					
					var mapBounds = DualGrid.GetMapBounds();
					var newVect = new Vector2I(_body.First.Value.X, _body.First.Value.Y);
					newVect += translation;
					
					// Limitar a los bordes del mapa
					if (newVect.X < 0) newVect.X = 0;
					if (newVect.X > mapBounds.X) newVect.X = mapBounds.X;
					if (newVect.Y < 0) newVect.Y = 0;
					if (newVect.Y > mapBounds.Y) newVect.Y = mapBounds.Y;
					
					// Actualizar animación
					if (_direction == Direction.RIGHT)
						player_ani.ChangeAnimation("walk_right");
					if (_direction == Direction.LEFT)
						player_ani.ChangeAnimation("walk_left");
					if (_direction == Direction.UP)
						player_ani.ChangeAnimation("walk_up");
					if (_direction == Direction.DOWN)
						player_ani.ChangeAnimation("walk_down");
					
					// Mover personaje
					_body.Clear();
					_body.AddFirst(newVect);
					TryEat(); // Procesar interacciones
					player_ani.MoveSprite(_body.First.Value, delta);
					
					_time = 0;
				}
			}
			
			return;
		}
		
		// Modo serpiente: movimiento automático con timer
		_time += delta;
		if (_time > 0.2 && !_crash)
		{
			// Aplicar la dirección del buffer
			_direction = _nextDirection;
			
			var translation = _direction switch
			{
				Direction.RIGHT => new Vector2I(1, 0),
				Direction.LEFT => new Vector2I(-1, 0),
				Direction.UP => new Vector2I(0, -1),
				Direction.DOWN => new Vector2I(0, 1),
				_ => new Vector2I(0, 0)
			};
			if (_body.Count > 0)
			{
				var mapBounds = DualGrid.GetMapBounds();
				var newVect = new Vector2I(_body.First.Value.X, _body.First.Value.Y);
				newVect += translation;
			
			// Si tiene paredes, detectar colisión con bordes
			if (HasWalls)
			{
				if (newVect.X < 0 || newVect.X > mapBounds.X || newVect.Y < 0 || newVect.Y > mapBounds.Y)
				{
					ShowGameOverScreen();
					return;
				}
			}
			else // Teletransporte (comportamiento original)
			{
				if (newVect.X < 0)
					newVect = new Vector2I(mapBounds.X, newVect.Y);
				if (newVect.X > mapBounds.X)
					newVect = new Vector2I(0, newVect.Y);
				if (newVect.Y < 0)
					newVect = new Vector2I(newVect.X, mapBounds.Y);
				if (newVect.Y > mapBounds.Y)
					newVect = new Vector2I(newVect.X, 0);
			}

			if (_direction == Direction.RIGHT)
				player_ani.ChangeAnimation("walk_right");
			if (_direction == Direction.LEFT)
				player_ani.ChangeAnimation("walk_left");
			if (_direction == Direction.UP)
				player_ani.ChangeAnimation("walk_up");
			if (_direction == Direction.DOWN)
				player_ani.ChangeAnimation("walk_down");

			// En modo movimiento libre, solo mover la posición actual sin cuerpo
			if (FreeMovement)
			{
				_body.Clear();
				_body.AddFirst(newVect);
				TryEat(); // Procesar interacciones
				player_ani.MoveSprite(_body.First.Value, delta);
			}
			else
			{
				_body.AddFirst(newVect);
				
				// Si HideBody está activo, siempre eliminar el último segmento (no crecer)
				if (HideBody)
				{
					TryEat(); // Procesar colección pero sin crecer
					var last = _body.Last.Value;
					_body.RemoveLast();
					player_ani.MoveSprite(_body.First.Value, delta);
					DualGrid.SetTile(last, DualGrid.dirtPlaceholderAtlasCoord);
				}
				else if (!TryEat())
				{
					var last = _body.Last.Value;
					_body.RemoveLast();
					player_ani.MoveSprite(_body.First.Value, delta);
					DualGrid.SetTile(last, DualGrid.dirtPlaceholderAtlasCoord);
				}
			}

			TryObstacle();

			if (Crash())
			{
				
				ShowGameOverScreen();
			}
		}
		if (!_crash)
			QueueRedraw();
		_time = 0;
	}
}

public void ShowGameOverScreen()
	{
		_crash = true;
		//gameOverScreen.Visible = true;
		GameData.Instance.globalTrashList = trashList;
		
		EmitSignal(SignalName.GameOver);
	}

	public override void _Input(InputEvent @event)
	{
		// En modo FreeMovement, el movimiento se maneja en _Process
		if (FreeMovement)
		{
			return;
		}
		
		// Modo serpiente: guardar en buffer la próxima dirección (validando que no sea opuesta a la ACTUAL)
		if (@event.IsAction("ui_left") && _direction != Direction.RIGHT)
		{
			_nextDirection = Direction.LEFT;
			return;
		}

		if (@event.IsAction("ui_right") && _direction != Direction.LEFT)
		{
			_nextDirection = Direction.RIGHT;
			return;
		}

		if (@event.IsAction("ui_up") && _direction != Direction.DOWN)
		{
			_nextDirection = Direction.UP;
			return;
		}

		if (@event.IsAction("ui_down") && _direction != Direction.UP)
		{
			_nextDirection = Direction.DOWN;
			return;
		}

		if (@event.IsActionPressed("ui_accept"))
		{
			EmitSignal(SignalName.UpdateHealth);
			GD.Print("SPACE");
		}
	}

	public void on_test_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/ClassifyLevel.tscn");
		GD.Print(GameData.Instance.globalTrashList);
	}

	private enum Direction
	{
		LEFT,
		RIGHT,
		UP,
		DOWN,
	}
}
