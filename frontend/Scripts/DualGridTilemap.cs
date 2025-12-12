using Godot;
using System;
using System.Collections.Generic;

namespace Snake;

using static TileType;

public partial class DualGridTilemap : Node2D {
	[Export] TileMapLayer worldMapLayer;
	[Export] TileMapLayer displayMapLayer;
	[Export] TileMapLayer plantMapLayer;
	[Export] public Vector2I grassPlaceholderAtlasCoord;
	[Export] public Vector2I dirtPlaceholderAtlasCoord;
	private Dictionary<Vector2I, Sprite2D> trashes = new();
	private Dictionary<Vector2I, Sprite2D> obstacles = new();
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

	public override void _Ready() {
		// Refresh all display tiles
		foreach (Vector2I coord in worldMapLayer.GetUsedCells()) {
			SetDisplayTile(coord);
		}
		GenerateObstacles(rnd.Next(0,10000), scale: 0.12f, threshold: 0.2f, octaves: 4, clearExisting: true);
		

		while(obstacleAmount < 5)
		{
			Vector2I randomCoord = new Vector2I(rnd.Next(0,32), rnd.Next(0,21));
			if(CellIsEmpty(randomCoord))
			{
				PlaceTetrisPiece(randomCoord, 6);
				obstacleAmount++;
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
		var sprite = new Sprite2D();
		sprite.Texture = GD.Load<Texture2D>("res://Assets/Plant.png");
		sprite.Position = plantMapLayer.MapToLocal(coords);
		sprite.ZIndex = 2;
		CallDeferred("add_child", sprite);
		GD.Print($"Added trash at {coords}");
		trashes[coords] = sprite;
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
		}
	}

	public void AddRock(Vector2I coords)
	{
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = GD.Load<Texture2D>("res://Assets/Stone.png");
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
		return !(HasTrashAt(coord) || HasRockAt(coord));
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
