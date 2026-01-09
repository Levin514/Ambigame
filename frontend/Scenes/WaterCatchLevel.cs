using Godot;
using System;
using System.Collections.Generic;

public partial class WaterCatchLevel : Node2D
{
	// Configuración de pantalla
	[Export] public float ScreenWidth = 800f;
	[Export] public float ScreenHeight = 600f;
	
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
	private Sprite2D basketSprite;
	private CollisionShape2D basketCollision;
	
	// Control
	private float spawnTimer = 0f;
	private int score = 0;
	private int lives = 3;
	private List<Node2D> fallingObjects = new List<Node2D>();
	private Vector2 basketPosition;
	
	public override void _Ready()
	{
		// Obtener referencias a los contenedores
		waterDropletContainer = GetNode<Node2D>("WaterDroplet");
		basketNode = GetNode<Node2D>("Basket");
		
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
		basketPosition = new Vector2(ScreenWidth / 2f, ScreenHeight - 50f);
		basketNode.Position = basketPosition;
		
		// Inicializar timer
		spawnTimer = SpawnRate;
		
		GD.Print("WaterCatchLevel iniciado. Pantalla: " + ScreenWidth + "x" + ScreenHeight);
	}

	public override void _Process(double delta)
	{
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
				lives--;
				objectsToRemove.Add(i);
				UpdateUI();
				continue;
			}
			
			// Verificar colisión con canasta
			if (IsObjectCaughtByBasket(obj))
			{
				GD.Print("¡Objeto atrapado!");
				score += 10;
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
		
		// Verificar Game Over
		if (lives <= 0)
		{
			GameOver();
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

	private void UpdateUI()
	{
		GD.Print($"Puntuación: {score} | Vidas: {lives}");
	}

	private void GameOver()
	{
		GD.Print($"¡JUEGO TERMINADO! Puntuación final: {score}");
		GetTree().Paused = true;
	}
}
