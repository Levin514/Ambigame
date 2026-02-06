using Godot;
using System;
using System.Collections.Generic;

public partial class WaterCatchLevel : Node2D
{
	[Signal] public delegate void GameOverEventHandler(int score, int recycled, int time);
	[Signal] public delegate void VictoryEventHandler(int score, int recycled, int time);
	[Signal] public delegate void ScoreUpdatedEventHandler(int score);
	[Signal] public delegate void TimeUpdatedEventHandler(int time);
	
	// Configuración de pantalla
	private float ScreenWidth;
	private float ScreenHeight;
	
	// Configuración de spawn de objetos
	[Export] public float SpawnRate = 1.5f;
	[Export] public float FallSpeed = 200f;
	[Export] public Texture2D FallingObjectTexture;
	[Export] public Vector2 FallingObjectScale = new Vector2(0.5f, 0.5f);
	
	// Configuración de canasta
	[Export] public float BasketSpeed = 300f;
	[Export] public Texture2D BasketTexture;
	[Export] public Vector2 BasketScale = new Vector2(1.5f, 1f);
	[Export] public float BasketWidth = 80f;
	[Export] public float BasketHeight = 30f;
	
	// Nodos
	private Node2D waterDropletContainer;
	private Node2D basketNode;
	private Node2D playerAnimationNode;
	private Sprite2D basketSprite;
	private CollisionShape2D basketCollision;
	
	// Control
	private float spawnTimer = 0f;
	private int waterCollected = 0;
	private int waterCollectedStreak= 0;
	private int _score = 0;
	private int baseScore = 100;
	public int Score
	{
		get => _score;
		set
		{
			_score = value;
			EmitSignal(SignalName.ScoreUpdated, _score);
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

	private float timeAccumulator = 0f;
	private List<Node2D> fallingObjects = new List<Node2D>();
	private Vector2 basketPosition;
	private AnimatedSprite2D playerSprite;
	private float lastBasketX = 0f;

	private Camera2D _camera;
	
	public override void _Ready()
	{
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		ScreenHeight = screenSize.Y;
		ScreenWidth = screenSize.X;

		//Temporal solution
		_camera = GetNode<Camera2D>("/root/GameLayout/VBoxContainer/GameContainer/SubViewportContainer/SubViewport/Camera2D");
		_camera.Zoom = new Vector2I(1,1);
		
		ElapsedTime = 60;

		// Obtener referencias a los contenedores
		waterDropletContainer = GetNode<Node2D>("WaterDroplet");
		basketNode = GetNode<Node2D>("Basket");
		playerAnimationNode = GetNode<Node2D>("PlayerAnimation");
		
		// Obtener referencia al AnimatedSprite2D del jugador
		playerSprite = playerAnimationNode.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		// Crear sprite y colisión para la canasta
		basketSprite = new Sprite2D();
		basketSprite.Texture = BasketTexture;
		basketSprite.Scale = BasketScale;
		basketNode.AddChild(basketSprite);
		
		basketCollision = new CollisionShape2D();
		RectangleShape2D rectShape = new RectangleShape2D();
		rectShape.Size = new Vector2(BasketWidth, BasketHeight);
		basketCollision.Shape = rectShape;
		basketNode.AddChild(basketCollision);
		
		// Posición inicial de la canasta (centro inferior)
		basketPosition = new Vector2(ScreenWidth / 2f, ScreenHeight - ScreenHeight * 0.15f);
		basketNode.Position = basketPosition;
		lastBasketX = basketPosition.X;
		
		// Posición inicial del jugador (debajo de la canasta)
		playerAnimationNode.Position = new Vector2(basketPosition.X, basketPosition.Y + 25f);
		
		// Inicializar timer
		spawnTimer = SpawnRate;
		
		GD.Print("WaterCatchLevel iniciado. Pantalla: " + ScreenWidth + "x" + ScreenHeight);
	}

	public void InitializeSignals()
	{
		EmitSignal(SignalName.ScoreUpdated, Score);
		EmitSignal(SignalName.TimeUpdated, ElapsedTime);
		GD.Print("WaterCatchLevel: Señales inicializadas");
	}

	public override void _Process(double delta)
	{
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
		
		float deltaF = (float)delta;
		
		// Lógica de spawn
		spawnTimer -= deltaF;
		if (spawnTimer <= 0)
		{
			SpawnFallingObject();
			spawnTimer = SpawnRate;
		}
		
		// Control de la canasta
		HandleBasketInput(deltaF);
		
		// Actualizar posición visual de la canasta
		basketNode.Position = basketPosition;
		
		// Actualizar posición del jugador (debajo de la canasta)
		playerAnimationNode.Position = new Vector2(basketPosition.X, basketPosition.Y + 50f);
		
		// Voltear sprite según la dirección de movimiento
		if (basketPosition.X < lastBasketX)
		{
			// Movimiento a la izquierda
			playerSprite.Play("walk_left");
		}
		else if (basketPosition.X > lastBasketX)
		{
			// Movimiento a la derecha
			playerSprite.Play("walk_right");
		}
		else
		{
			playerSprite.Stop();
		}
		lastBasketX = basketPosition.X;
		
		// Verificar colisiones con objetos cayendo
		CheckCollisions(deltaF);
	}

	private void SpawnFallingObject()
	{
		// Generar posición aleatoria (eje 0 a 1)
		float normalizedX = (float)GD.Randf();
		float spawnX = normalizedX * ScreenWidth;
		
		// Crear nodo contenedor para el objeto
		Node2D fallingObjectNode = new Node2D();
		fallingObjectNode.Position = new Vector2(spawnX, -50f);
		waterDropletContainer.AddChild(fallingObjectNode);
		
		// Crear sprite del objeto
		Sprite2D fallingSprite = new Sprite2D();
		fallingSprite.Texture = FallingObjectTexture;
		fallingSprite.Scale = FallingObjectScale;
		fallingObjectNode.AddChild(fallingSprite);
		
		// Crear área de colisión
		Area2D fallingArea = new Area2D();
		fallingObjectNode.AddChild(fallingArea);
		
		CircleShape2D circleShape = new CircleShape2D();
		circleShape.Radius = 16f;
		
		CollisionShape2D collisionShape = new CollisionShape2D();
		collisionShape.Shape = circleShape;
		fallingArea.AddChild(collisionShape);
		
		// Almacenar en lista
		fallingObjects.Add(fallingObjectNode);
	}

	private void HandleBasketInput(float delta)
	{
		float moveDirection = 0f;
		
		// Entrada de teclado
		if (Input.IsActionPressed("ui_left"))
		{
			moveDirection = -1f;
		}
		else if (Input.IsActionPressed("ui_right"))
		{
			moveDirection = 1f;
		}
		
		// Entrada analógica
		float axisInput = Input.GetAxis("ui_left", "ui_right");
		if (axisInput != 0)
		{
			moveDirection = axisInput;
		}
		
		// Aplicar movimiento
		basketPosition.X += moveDirection * BasketSpeed * delta;
		
		// Limitar posición dentro de pantalla
		float minX = BasketWidth / 2f;
		float maxX = ScreenWidth - (BasketWidth / 2f);
		basketPosition.X = Mathf.Clamp(basketPosition.X, minX, maxX);
	}

	private void CheckCollisions(float delta)
	{
		List<int> objectsToRemove = new List<int>();
		
		for (int i = fallingObjects.Count - 1; i >= 0; i--)
		{
			Node2D obj = fallingObjects[i];
			
			// Hacer caer el objeto
			obj.Position += new Vector2(0, FallSpeed * delta);
			
			// Verificar si salió de pantalla (perdido)
			if (obj.Position.Y > ScreenHeight + 100f)
			{
				GD.Print("Objeto perdido");
				waterCollectedStreak = 0;
				objectsToRemove.Add(i);
				UpdateUI();
				continue;
			}
			
			// Verificar colisión con canasta
			if (IsObjectCaughtByBasket(obj))
			{
				GD.Print("¡Objeto atrapado!");
				UpdateScore();
				waterCollected += 1;
				waterCollectedStreak += 1;
				objectsToRemove.Add(i);
				UpdateUI();
			}
		}
		
		// Remover objetos
		foreach (int i in objectsToRemove)
		{
			if (i < fallingObjects.Count)
			{
				fallingObjects[i].QueueFree();
				fallingObjects.RemoveAt(i);
			}
		}
	}

	private bool IsObjectCaughtByBasket(Node2D obj)
	{
		// Rango de la canasta (eje normalizado)
		float basketMinX = basketPosition.X - (BasketWidth / 2f);
		float basketMaxX = basketPosition.X + (BasketWidth / 2f);
		float basketMinY = basketPosition.Y - (BasketHeight / 2f);
		float basketMaxY = basketPosition.Y + (BasketHeight / 2f);
		
		// Posición del objeto
		float objX = obj.Position.X;
		float objY = obj.Position.Y;
		
		// Verificar si está dentro del rango de la canasta
		return objX >= basketMinX && objX <= basketMaxX && 
		       objY >= basketMinY && objY <= basketMaxY;
	}
	
	private void UpdateScore()
	{
		if(waterCollectedStreak > 0)
		{
			Score += baseScore + baseScore * waterCollectedStreak / 2;
		}
		else
		Score += baseScore;
	}

	private void UpdateUI()
	{
		GD.Print($"Puntuación: {Score} | Tiempo: {ElapsedTime}");
	}

	private void TriggerGameOver()
	{
		GD.Print($"¡JUEGO TERMINADO! Puntuación final: {Score}");
		EmitSignal(SignalName.GameOver, Score, waterCollected, ElapsedTime);
	}
}
