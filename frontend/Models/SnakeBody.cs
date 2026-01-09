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
	[Signal] public delegate void PipeRepairedEventHandler();
	[Signal] public delegate void ScoreUpdatedEventHandler(int score);
	[Signal] public delegate void RecycledUpdatedEventHandler(int recycled);
	[Signal] public delegate void TimeUpdatedEventHandler(int time);
	
	[Export] DualGridTilemap DualGrid;

	//[Export] CanvasLayer gameOverScreen;
	[Export] PlayerAnimation player_ani;
	[Export] LifeSystem life_system;
	[Export] public bool HasWalls = false; // Si true, chocar con bordes causa Game Over
	[Export] public bool HideBody = false; // Si true, no dibuja cuerpo ni crece (solo cabeza)

	private LinkedList<Vector2I> _body;
	private LinkedList<Trash> trashList;
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
		trashList = new();
		DualGrid.TrashCollector += AddToTrashList;
		_direction = Direction.RIGHT;
		_nextDirection = Direction.RIGHT;  // Inicializar buffer
		_body = new([new(1, 0), new(0, 0)]);
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
		if (DualGrid.HasTrashAt(headPosition))
		{
			// Emitir señal de tubería reparada (el GameLayout/WaterSystem lo manejará)
			EmitSignal(SignalName.PipeRepaired);
			
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
		_time += delta;
		elapsedTime += delta;
		if (elapsedTime > 1 && !_crash)
		{
			juegoTime++;
			UpdateTimerLabel();
			elapsedTime = 0;
		}
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
		// Guardar en buffer la próxima dirección (validando que no sea opuesta a la ACTUAL)
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
