using Godot;
using System;
using System.Collections.Generic;

namespace Snake;

using static TileType;

public partial class DualGridTilemap : Node2D {

	[Signal] public delegate void TrashCollectorEventHandler(Trash trash);
	[Signal] public delegate void PipeBrokenEventHandler();
	[Signal] public delegate void PlantGrownEventHandler(); // Nueva señal cuando una planta crece completamente
	[Export] TileMapLayer worldMapLayer;
	[Export] TileMapLayer displayMapLayer;
	[Export] TileMapLayer plantMapLayer;
	[Export] public Vector2I grassPlaceholderAtlasCoord;
	[Export] public Vector2I dirtPlaceholderAtlasCoord;
	[Export] public bool generateObstacles = true; // Controla si se generan rocas (false para nivel de agua)
	[Export] public Texture2D obstacleSprite;
	private Dictionary<Vector2I, Sprite2D> trashes = new();
	private Dictionary<Vector2I, Trash> trashItems = new();
	private Dictionary<Vector2I, Sprite2D> obstacles = new();
	private Dictionary<Vector2I, Sprite2D> pipes = new();
	private Dictionary<Vector2I, bool> pipeStates = new(); // true = arreglada, false = rota
	
	// Sistema de plantación para reforestación
	private Dictionary<Vector2I, Sprite2D> plantSpots = new(); // Lugares de plantación
	private Dictionary<Vector2I, PlantState> plantStates = new(); // Estado de cada lugar
	private Dictionary<Vector2I, Timer> plantTimers = new(); // Timers para crecimiento
	private Dictionary<Vector2I, Label> plantTimerLabels = new(); // Labels de temporizadores
	
	private readonly Vector2I[] NEIGHBOURS = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];

	readonly Dictionary<Tuple<TileType, TileType, TileType, TileType>, Vector2I> neighboursToAtlasCoord = new() {
		{new (TrashBag, TrashBag, TrashBag, TrashBag), new Vector2I(2, 1)}, // All corners
		{new (Dirt, Dirt, Dirt, TrashBag), new Vector2I(1, 3)}, // Outer bottom-right corner
		{new (Dirt, Dirt, TrashBag, Dirt), new Vector2I(0, 0)}, // Outer bottom-left corner
		{new (Dirt, TrashBag, Dirt, Dirt), new Vector2I(0, 2)}, // Outer top-right corner
		{new (TrashBag, Dirt, Dirt, Dirt), new Vector2I(3, 3)}, // Outer top-left corner
		{new (Dirt, TrashBag, Dirt, TrashBag), new Vector2I(1, 0)}, // Right edge
		{new (TrashBag, Dirt, TrashBag, Dirt), new Vector2I(3, 2)}, // Left edge
		{new (Dirt, Dirt, TrashBag, TrashBag), new Vector2I(3, 0)}, // Bottom edge
		{new (TrashBag, TrashBag, Dirt, Dirt), new Vector2I(1, 2)}, // Top edge
		{new (Dirt, TrashBag, TrashBag, TrashBag), new Vector2I(1, 1)}, // Inner bottom-right corner
		{new (TrashBag, Dirt, TrashBag, TrashBag), new Vector2I(2, 0)}, // Inner bottom-left corner
		{new (TrashBag, TrashBag, Dirt, TrashBag), new Vector2I(2, 2)}, // Inner top-right corner
		{new (TrashBag, TrashBag, TrashBag, Dirt), new Vector2I(3, 1)}, // Inner top-left corner
		{new (Dirt, TrashBag, TrashBag, Dirt), new Vector2I(2, 3)}, // Bottom-left top-right corners
		{new (TrashBag, Dirt, Dirt, TrashBag), new Vector2I(0, 1)}, // Top-left down-right corners
		{new (Dirt, Dirt, Dirt, Dirt), new Vector2I(0, 3)}, // No corners
	};

	private Random rnd = new();
	private int obstacleAmount = 0;

	// Obtener límites reales del mapa basado en las celdas usadas
	public Vector2I GetMapBounds()
	{
		var usedCells = worldMapLayer.GetUsedCells();
		if (usedCells.Count == 0)
			return new Vector2I(32, 21); // Valores por defecto si no hay celdas
		
		int maxX = 0;
		int maxY = 0;
		foreach (var cell in usedCells)
		{
			if (cell.X > maxX) maxX = cell.X;
			if (cell.Y > maxY) maxY = cell.Y;
		}
		return new Vector2I(maxX, maxY);
	}

	public override void _Ready() {
		// Refresh all display tiles
		foreach (Vector2I coord in worldMapLayer.GetUsedCells()) {
			SetDisplayTile(coord);
		}
		
		// Generar obstáculos solo si está habilitado (niveles de reciclaje, reforestación)
		if (generateObstacles)
		{
			GenerateObstacles(rnd.Next(0,10000), scale: 0.12f, threshold: 0.2f, octaves: 4, clearExisting: true);
			
			while(obstacleAmount < 5)
			{
				Vector2I randomCoord = new Vector2I(rnd.Next(0, GetMapBounds().X + 1), rnd.Next(0, GetMapBounds().Y + 1));
				if(CellIsEmpty(randomCoord))
				{
					PlaceTetrisPiece(randomCoord, 6);
					obstacleAmount++;
				}
			}
		}
	}

	/// <summary>
	/// <para>Returns the map coordinates of the cell containing the given <paramref name="localPosition"/>. If <paramref name="localPosition"/> is in global coordinates, consider using <see cref="Godot.Node2D.ToLocal(Vector2)"/> before passing it to this method. See also <see cref="Godot.TileMapLayer.MapToLocal(Vector2I)"/>.</para>
	/// </summary>
	public Vector2I LocalToMap(Vector2 pos)
	{
		return worldMapLayer.LocalToMap(pos);
	}

	public void SetTile(Vector2I coords, Vector2I atlasCoords) {
		worldMapLayer.SetCell(coords, 0, atlasCoords);
		SetDisplayTile(coords);
	}

	void SetDisplayTile(Vector2I pos) {
		// loop through 4 display neighbours
		for (int i = 0; i < NEIGHBOURS.Length; i++) {
			Vector2I newPos = pos + NEIGHBOURS[i];
			displayMapLayer.SetCell(newPos, 1, CalculateDisplayTile(newPos));
		}
	}

	Vector2I CalculateDisplayTile(Vector2I coords) {
		// get 4 world tile neighbours
		TileType botRight = GetWorldTile(coords - NEIGHBOURS[0]);
		TileType botLeft = GetWorldTile(coords - NEIGHBOURS[1]);
		TileType topRight = GetWorldTile(coords - NEIGHBOURS[2]);
		TileType topLeft = GetWorldTile(coords - NEIGHBOURS[3]);

		// return tile (atlas coord) that fits the neighbour rules
		return neighboursToAtlasCoord[new(topLeft, topRight, botLeft, botRight)];
	}

	TileType GetWorldTile(Vector2I coords) {
		Vector2I atlasCoord = worldMapLayer.GetCellAtlasCoords(coords);
		if (atlasCoord == grassPlaceholderAtlasCoord)
			return TrashBag;
		else
			return Dirt;
	}

	public void AddTrash(Vector2I coords)
	{
		Trash trash = new Trash();
		var sprite = new Sprite2D();
		sprite.Texture = GD.Load<Texture2D>("res://Assets/" + trash.trashName +".png");
		sprite.Position = plantMapLayer.MapToLocal(coords);
		sprite.Scale = new Vector2(0.5f,0.5f);
		sprite.ZIndex = 2;
		CallDeferred("add_child", sprite);
		GD.Print($"Added trash at {coords}");
		trashes[coords] = sprite;
		trashItems[coords] = trash;
	}

	public bool HasTrashAt(Vector2I coords)
	{
		return trashes.ContainsKey(coords);
	}

	public Sprite2D GetTrashAt(Vector2I coords)
	{
		return trashes.TryGetValue(coords, out var sprite) ? sprite : null;
	}
	public void RemoveTrashAt(Vector2I coords)
	{
		if (trashes.TryGetValue(coords, out var sprite))
		{
			sprite.QueueFree(); // elimina el nodo de la escena
			trashes.Remove(coords); // elimina la referencia del diccionario
			EmitSignal(SignalName.TrashCollector,trashItems[coords]);
			trashItems.Remove(coords);
		}
	}

	public void AddRock(Vector2I coords)
	{
		Sprite2D sprite = new Sprite2D();
		if(obstacleSprite == null)
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/Stone.png");
		}
		else
		{
			sprite.Texture = obstacleSprite;
		}
		sprite.Position = plantMapLayer.MapToLocal(coords);
		sprite.ZIndex = 3;
		CallDeferred("add_child", sprite);
		GD.Print($"Added stone at {coords}");
		
		obstacles[coords] = sprite;
	}

	public bool HasRockAt(Vector2I coords)
	{
		return obstacles.ContainsKey(coords);
	}

	public Sprite2D GetRockAt(Vector2I coords)
	{
		return obstacles.TryGetValue(coords, out var sprite) ? sprite : null;
	}

	public void RemoveRockAt(Vector2I coords)
	{
		if (obstacles.TryGetValue(coords, out var sprite))
		{
			sprite.QueueFree(); // elimina el nodo de la escena
			obstacles.Remove(coords); // elimina la referencia del diccionario
		}
	}

	// ===== MÉTODOS PARA TUBERÍAS =====
	public void AddPipe(Vector2I coords, bool isBroken = true)
	{
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = isBroken 
			? GD.Load<Texture2D>("res://Assets/BrokenPipe.png") 
			: GD.Load<Texture2D>("res://Assets/Pipe.png");
		sprite.Position = plantMapLayer.MapToLocal(coords);
		sprite.Scale = new Vector2(0.5f, 0.5f);
		sprite.ZIndex = 2;
		CallDeferred("add_child", sprite);
		GD.Print($"Added {(isBroken ? "broken" : "fixed")} pipe at {coords}");
		pipes[coords] = sprite;
		pipeStates[coords] = !isBroken; // true si está arreglada
	}

	public bool HasPipeAt(Vector2I coords)
	{
		return pipes.ContainsKey(coords);
	}

	public bool IsPipeRepairedAt(Vector2I coords)
	{
		return pipeStates.TryGetValue(coords, out bool isRepaired) && isRepaired;
	}

	public void RepairPipe(Vector2I coords)
	{
		if (pipes.TryGetValue(coords, out var sprite))
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/Pipe.png");
			pipeStates[coords] = true;
			GD.Print($"Pipe repaired at {coords}");
		}
	}

	public void BreakPipe(Vector2I coords)
	{
		if (pipes.TryGetValue(coords, out var sprite))
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/BrokenPipe.png");
			pipeStates[coords] = false;
			GD.Print($"Pipe broken at {coords}");
			EmitSignal(SignalName.PipeBroken);
		}
	}

	public void RemovePipeAt(Vector2I coords)
	{
		if (pipes.TryGetValue(coords, out var sprite))
		{
			sprite.QueueFree();
			pipes.Remove(coords);
			pipeStates.Remove(coords);
		}
	}

	public int GetTotalPipes()
	{
		return pipes.Count;
	}

	public int GetRepairedPipesCount()
	{
		int count = 0;
		foreach (var kvp in pipeStates)
		{
			if (kvp.Value == true)
				count++;
		}
		return count;
	}

	public int GetBrokenPipesCount()
	{
		int count = 0;
		foreach (var kvp in pipeStates)
		{
			if (kvp.Value == false)
				count++;
		}
		return count;
	}

	public Vector2I? GetRandomGoodPipe()
	{
		var goodPipes = new System.Collections.Generic.List<Vector2I>();
		foreach (var kvp in pipeStates)
		{
			if (kvp.Value == true) // Tubería buena
				goodPipes.Add(kvp.Key);
		}

		if (goodPipes.Count > 0)
		{
			int randomIndex = rnd.Next(goodPipes.Count);
			return goodPipes[randomIndex];
		}

		return null; // No hay tuberías buenas
	}

	public void PlacePipeNetwork(Vector2I startCoord, int pipeLength)
	{
		Stack<Vector2I> pipeCoords = GenerateObstacleWithBacktracking(startCoord, pipeLength);
		
		GD.Print($"Red de tuberías generada con {pipeCoords.Count} segmentos");
		
		while (pipeCoords.Count > 0) {
			Vector2I coord = pipeCoords.Pop();
			// 50% probabilidad de estar rota o buena
			bool isBroken = rnd.Next(2) == 0;
			AddPipe(coord, isBroken: isBroken);
		}
	}

	// ===== MÉTODOS PARA PLANTACIÓN =====
	
	public void AddPlantSpot(Vector2I coords)
	{
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = GD.Load<Texture2D>("res://Assets/SownField.png");
		sprite.Position = plantMapLayer.MapToLocal(coords);
		sprite.Scale = new Vector2(0.5f, 0.5f);
		sprite.ZIndex = 2;
		plantMapLayer.CallDeferred("add_child", sprite);
		plantSpots[coords] = sprite;
		plantStates[coords] = PlantState.Empty;
		GD.Print($"Added plant spot at {coords}");
	}

	public bool HasPlantSpotAt(Vector2I coords)
	{
		return plantSpots.ContainsKey(coords);
	}

	public PlantState GetPlantState(Vector2I coords)
	{
		return plantStates.TryGetValue(coords, out PlantState state) ? state : PlantState.Empty;
	}

	public bool TryPlantSeed(Vector2I coords)
	{
		if (!HasPlantSpotAt(coords) || GetPlantState(coords) != PlantState.Empty)
			return false;

		// Cambiar sprite a plant1
		if (plantSpots.TryGetValue(coords, out var sprite))
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/Plant1.png");
			plantStates[coords] = PlantState.Seeded;
			
			// Crear timer y label
			CreatePlantTimer(coords, 10f, () => OnSeedGrowthComplete(coords));
			GD.Print($"Planted seed at {coords}");
			return true;
		}
		return false;
	}

	public bool TryWaterPlant(Vector2I coords)
	{
		if (!HasPlantSpotAt(coords) || GetPlantState(coords) != PlantState.NeedsWater)
			return false;

		// Mantener sprite plant2, iniciar timer
		plantStates[coords] = PlantState.Watered;
		CreatePlantTimer(coords, 10f, () => OnWaterGrowthComplete(coords));
		GD.Print($"Watered plant at {coords}");
		return true;
	}

	private void CreatePlantTimer(Vector2I coords, float duration, Action onComplete)
	{
		// Crear timer
		Timer timer = new Timer();
		timer.WaitTime = duration;
		timer.OneShot = true;
		timer.Timeout += onComplete;
		AddChild(timer);
		timer.Start();
		plantTimers[coords] = timer;

		// Crear label con temporizador visible
		Label label = new Label();
		label.Position = plantMapLayer.MapToLocal(coords) + new Vector2(-15, -40);
		label.Scale = new Vector2(0.8f, 0.8f);
		label.ZIndex = 10;
		label.AddThemeColorOverride("font_color", Colors.White);
		label.AddThemeColorOverride("font_outline_color", Colors.Black);
		label.AddThemeConstantOverride("outline_size", 2);
		CallDeferred("add_child", label);
		plantTimerLabels[coords] = label;
	}

	public override void _Process(double delta)
	{
		// Actualizar labels de temporizadores
		foreach (var kvp in plantTimers)
		{
			Vector2I coords = kvp.Key;
			Timer timer = kvp.Value;
			
			if (plantTimerLabels.TryGetValue(coords, out Label label) && !timer.IsStopped())
			{
				float timeLeft = (float)timer.TimeLeft;
				label.Text = $"{Mathf.CeilToInt(timeLeft)}s";
			}
		}
	}

	private void OnSeedGrowthComplete(Vector2I coords)
	{
		GD.Print($"Seed growth complete at {coords}");
		
		// Cambiar a plant2 (necesita agua)
		if (plantSpots.TryGetValue(coords, out var sprite))
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/Plant2.png");
			plantStates[coords] = PlantState.NeedsWater;
		}
		
		// Limpiar timer y label
		CleanupTimer(coords);
	}

	private void OnWaterGrowthComplete(Vector2I coords)
	{
		GD.Print($"Water growth complete at {coords} - Plant fully grown!");
		
		// Cambiar a plant3 (completamente crecida)
		if (plantSpots.TryGetValue(coords, out var sprite))
		{
			sprite.Texture = GD.Load<Texture2D>("res://Assets/Plant3.png");
			plantStates[coords] = PlantState.FullyGrown;
			
			// Emitir señal de que una planta creció completamente
			EmitSignal(SignalName.PlantGrown);
		}
		
		// Limpiar timer y label
		CleanupTimer(coords);
	}

	private void CleanupTimer(Vector2I coords)
	{
		if (plantTimers.TryGetValue(coords, out Timer timer))
		{
			timer.QueueFree();
			plantTimers.Remove(coords);
		}
		
		if (plantTimerLabels.TryGetValue(coords, out Label label))
		{
			label.QueueFree();
			plantTimerLabels.Remove(coords);
		}
	}

	public int GetTotalPlantSpots()
	{
		return plantSpots.Count;
	}

	public int GetFullyGrownCount()
	{
		int count = 0;
		foreach (var kvp in plantStates)
		{
			if (kvp.Value == PlantState.FullyGrown)
				count++;
		}
		return count;
	}

	public void GeneratePlantSpots(int count)
	{
		var bounds = GetMapBounds();
		int added = 0;
		int attempts = 0;
		int maxAttempts = 1000;

		while (added < count && attempts < maxAttempts)
		{
			attempts++;
			Vector2I coords = new Vector2I(rnd.Next(2, bounds.X - 1), rnd.Next(2, bounds.Y - 1));
			
			// Verificar que esté vacío y separado de otros lugares
			if (CellIsEmpty(coords) && !HasPlantSpotAt(coords) && IsIsolated(coords, 3))
			{
				AddPlantSpot(coords);
				added++;
			}
		}
		
		GD.Print($"Generated {added} plant spots");
	}

	private bool IsIsolated(Vector2I coords, int minDistance)
	{
		// Verificar que no haya otros lugares de plantación cerca
		foreach (var spot in plantSpots.Keys)
		{
			int dist = Mathf.Abs(coords.X - spot.X) + Mathf.Abs(coords.Y - spot.Y);
			if (dist < minDistance)
				return false;
		}
		return true;
	}

	public int GenerateObstacles(int seed = 0, float scale = 0.1f, float threshold = 0.6f, int octaves = 4, bool clearExisting = true)
	{
		GD.Print("Generating obstacles...");
		// Limpiar obstáculos actuales si se solicita
		if (clearExisting)
		{
			var keys = new List<Vector2I>(trashes.Keys);
			foreach (var k in keys)
				RemoveTrashAt(k);
		}
	
		// Configurar FastNoiseLite
		var noise = new FastNoiseLite();
		noise.Seed = seed;
		noise.Frequency = scale; // escala de muestreo (input * Frequency)
		noise.FractalOctaves = Math.Max(1, octaves);
		noise.FractalGain = 0.5f;
		noise.FractalLacunarity = 2f;
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin; // opcional: tipo de ruido
	
		int added = 0;
	
		// Iterar por las celdas 
		for(int x = 0; x <= 33; x++)
		{
			for(int y = 0; y <= 21; y++)
			{
				Vector2I coord = new Vector2I(x,y);
				// Muestreo del ruido: FastNoiseLite devuelve en [-1, 1]
				float raw = noise.GetNoise2D(coord.X, coord.Y);
				float t = (raw + 1f) * 0.5f; // normalizar a [0,1]
				float biased = Mathf.Pow(t, 4); // elevar a la 4ª potencia
				GD.Print(biased);
				if (biased > threshold)
				{
					if (!HasTrashAt(coord))
					{
						AddRock(coord);
						added++;
					}
				}
			}
		}
	
		GD.Print($"GenerateObstacles: added {added} obstacles (seed={seed}, scale={scale}, threshold={threshold}, octaves={octaves})");
		return added;
	}

	private bool CellIsEmpty(Vector2I coord)
	{
		return !(HasTrashAt(coord) || HasRockAt(coord) || HasPipeAt(coord));
	}

	private Stack<Vector2I> GenerateObstacleWithBacktracking(Vector2I startCoord, int pieceSize)
	{
		Stack<Vector2I> piece = new Stack<Vector2I>();
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		Vector2I[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
		
		// Iniciar recorrido desde la coordenada de inicio
		Stack<Vector2I> toExplore = new Stack<Vector2I>();
		toExplore.Push(startCoord);
		
		while (toExplore.Count > 0 && piece.Count < pieceSize) {
			Vector2I current = toExplore.Pop();
			
			// Si no ha sido visitada, marcarla
			if (!visited.Contains(current))
			{
				// Marcar como visitada y agregar a la pieza
				visited.Add(current);
				piece.Push(current);
				
				// Barajar direcciones para que sea aleatorio
				Vector2I[] shuffledDirections = new Vector2I[directions.Length];
				Array.Copy(directions, shuffledDirections, directions.Length);
				ShuffleArray(shuffledDirections);
				
				// Explorar vecinos aleatorios
				foreach (Vector2I dir in shuffledDirections) {
					Vector2I neighbor = current + dir;
					
					// Verificar que no esté visitado
					if (!visited.Contains(neighbor) || CellIsEmpty(neighbor)) {
						toExplore.Push(neighbor);
					}
				}
			}
		}
		
		return piece;
	}
	
	void ShuffleArray<T>(T[] array)
	{
		Random random = new Random();
		for (int i = array.Length - 1; i > 0; i--) {
			int randomIndex = random.Next(i + 1);
			// Swap
			T temp = array[i];
			array[i] = array[randomIndex];
			array[randomIndex] = temp;
		}
	}
	
	
	public void PlaceTetrisPiece(Vector2I startCoord, int pieceSize)
	{
		Stack<Vector2I> piece = GenerateObstacleWithBacktracking(startCoord, pieceSize);
		
		GD.Print($"Pieza generada con {piece.Count} celdas");
		
		while (piece.Count > 0) {
			Vector2I coord = piece.Pop();
			AddRock(coord);
		}
	}
}

public enum TileType {
	None,
	TrashBag,
	Dirt,
	Terrain,
}

public enum PlantState
{
	Empty,          // sown_field - puede plantar semilla
	Seeded,         // plant1 - esperando timer para crecer
	NeedsWater,     // plant2 - necesita agua
	Watered,        // plant2 - esperando timer para crecer
	FullyGrown      // plant3 - planta completamente crecida
}
