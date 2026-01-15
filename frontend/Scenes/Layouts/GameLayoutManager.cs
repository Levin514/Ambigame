using Godot;
using System;

public partial class GameLayoutManager : Control
{
	// Referencias a los indicadores
	[Export] private PanelContainer indicatorsPanel;
	[Export] private Control waterSystemNode;
	[Export] private Control lifeSystemNode;
	[Export] private Control plantedSystemNode;
	[Export] private Control seedsSystemNode;
	[Export] private Control waterDropsSystemNode;
	[Export] private Label scoreLabel;
	[Export] private Label timeLabel;
	[Export] private Label recycledLabel;
	[Export] private Button pauseButton;
	[Export] private MarginContainer pauseMenu;
	
	// Contenedor donde se cargarán los niveles
	[Export] private SubViewport gameContainer;
	
	// Pantallas de Game Over y Victoria
	[Export] private CanvasLayer gameOverScreen;
	[Export] private Label gameOverStatsLabel;
	[Export] private CanvasLayer victoryScreen;
	[Export] private Label victoryStatsLabel;
	
	private Node currentLevel;
	private string currentLevelType = ""; // Guardar el tipo de nivel actual

	public override void _Ready()
	{
		GD.Print("GameLayoutManager: Inicializado");
		
		// Por defecto, ocultar todos los indicadores específicos de nivel
		OcultarTodosLosIndicadores();
		
		// Mostrar solo indicadores comunes
		scoreLabel.Visible = true;
		timeLabel.Visible = true;
		
		// Conectar el botón de pausa
		if (pauseButton != null)
		{
			pauseButton.Pressed += OnPauseButtonPressed;
		}
		
		// Obtener información del nivel desde LevelManager
		var levelManager = GetNodeOrNull<LevelManager>("/root/LevelManager");
		if (levelManager != null && !string.IsNullOrEmpty(levelManager.LevelPath))
		{
			GD.Print($"GameLayoutManager: Cargando nivel desde LevelManager - {levelManager.LevelPath}");
			CargarNivel(levelManager.LevelPath, levelManager.LevelType);
		}
		else
		{
			// TESTING: Si no hay nivel configurado, cargar WaterLevel_Map1 por defecto
			GD.Print("GameLayoutManager: No hay nivel configurado, cargando nivel de prueba");
			CargarNivel("res://Scenes/Levels/Water/WaterLevel_Map1.tscn", "water");
		}
	}

	/// <summary>
	/// Maneja el evento cuando se presiona el botón de pausa
	/// </summary>
	private void OnPauseButtonPressed()
	{
		GD.Print("GameLayoutManager: Botón de pausa presionado");
		
		// Mostrar el menú de pausa (el menú se encarga de pausar el juego)
		if (pauseMenu != null && pauseMenu.HasMethod("Pause"))
		{
			pauseMenu.Call("Pause");
		}
		else
		{
			GD.PrintErr("GameLayoutManager: PauseMenu no encontrado o no tiene método Pause");
		}
	}

	/// <summary>
	/// Carga un nivel específico en el contenedor de juego
	/// </summary>
	public void CargarNivel(string rutaNivel, string tipoNivel)
	{
		GD.Print($"GameLayoutManager: Cargando nivel {rutaNivel} de tipo {tipoNivel}");
		
		// Guardar el tipo de nivel actual
		currentLevelType = tipoNivel;
		
		// Limpiar nivel anterior si existe
		if (currentLevel != null)
		{
			GD.Print("GameLayoutManager: Limpiando nivel anterior");
			currentLevel.QueueFree();
			currentLevel = null;
		}
		
		// Configurar indicadores según el tipo de nivel
		MostrarIndicadoresSegunTipo(tipoNivel);
		
		// Cargar el nuevo nivel
		var nivelScene = GD.Load<PackedScene>(rutaNivel);
		if (nivelScene != null)
		{
			currentLevel = nivelScene.Instantiate();
			gameContainer.AddChild(currentLevel);
			GD.Print($"GameLayoutManager: Nivel {rutaNivel} cargado exitosamente en SubViewport");
			
			// Conectar señales del nivel a los indicadores
			ConectarSeñalesDelNivel(tipoNivel);
			
			// Inicializar WaterSystem si es nivel de agua
			if (tipoNivel == "water" && waterSystemNode != null)
			{
				InitializeWaterSystem();
			}
		}
		else
		{
			GD.PrintErr($"GameLayoutManager: No se pudo cargar el nivel {rutaNivel}");
		}
	}

	/// <summary>
	/// Muestra/oculta indicadores según el tipo de nivel
	/// </summary>
	private void MostrarIndicadoresSegunTipo(string tipoNivel)
	{
		// Primero ocultar todos
		OcultarTodosLosIndicadores();
		
		// Indicadores comunes siempre visibles
		scoreLabel.Visible = true;
		timeLabel.Visible = true;
		
		// Indicadores específicos por tipo
		switch (tipoNivel.ToLower())
		{
			case "water":
			case "waterlevel":
				waterSystemNode.Visible = true;
				GD.Print("GameLayoutManager: Mostrando indicador de agua");
				break;
				
			case "classify":
			case "classifylevel":
				lifeSystemNode.Visible = true;
				recycledLabel.Visible = true;
				GD.Print("GameLayoutManager: Mostrando indicadores de clasificación");
				break;
				
			case "recycling":
			case "recyclinglevel":
				lifeSystemNode.Visible = true;
				recycledLabel.Visible = true;
				GD.Print("GameLayoutManager: Mostrando indicadores de reciclaje");
				break;
				
			case "reforestation":
			case "reforestationlevel":
				plantedSystemNode.Visible = true;
				seedsSystemNode.Visible = true;
				waterDropsSystemNode.Visible = true;
				GD.Print("GameLayoutManager: Mostrando sistemas de reforestación (plantados, semillas, agua)");
				break;
				
			case "minigame":
				// Solo mostrar indicadores comunes (score, time)
				GD.Print("GameLayoutManager: Minijuego - solo indicadores comunes");
				break;
				
			default:
				GD.Print($"GameLayoutManager: Tipo de nivel desconocido: {tipoNivel}");
				break;
		}
	}

	/// <summary>
	/// Oculta todos los indicadores específicos de nivel
	/// </summary>
	private void OcultarTodosLosIndicadores()
	{
		waterSystemNode.Visible = false;
		lifeSystemNode.Visible = false;
		plantedSystemNode.Visible = false;
		seedsSystemNode.Visible = false;
		waterDropsSystemNode.Visible = false;
		recycledLabel.Visible = false;
		// Los labels comunes (score, time) no se ocultan aquí
	}

	/// <summary>
	/// Conecta las señales del nivel cargado a los indicadores correspondientes
	/// </summary>
	private void ConectarSeñalesDelNivel(string tipoNivel)
	{
		GD.Print($"GameLayoutManager: Conectando señales para nivel tipo {tipoNivel}");
		
		if (tipoNivel.ToLower() == "water" || tipoNivel.ToLower() == "waterlevel")
		{
			// Conectar señales del WaterSnake (nodo raíz del nivel)
			if (currentLevel.HasSignal("GameOver"))
			{
				currentLevel.Connect("GameOver", new Callable(this, nameof(OnLevelGameOver)));
				GD.Print("GameLayoutManager: Señal GameOver conectada");
			}
			
			if (currentLevel.HasSignal("Victory"))
			{
				currentLevel.Connect("Victory", new Callable(this, nameof(OnLevelVictory)));
				GD.Print("GameLayoutManager: Señal Victory conectada");
			}
			
			if (currentLevel.HasSignal("PipeRepaired"))
			{
				currentLevel.Connect("PipeRepaired", new Callable(this, nameof(OnPipeRepaired)));
				GD.Print("GameLayoutManager: Señal PipeRepaired conectada");
			}
		
		if (currentLevel.HasSignal("PipeBroken"))
		{
			currentLevel.Connect("PipeBroken", new Callable(this, nameof(OnPipeBroken)));
			GD.Print("GameLayoutManager: Señal PipeBroken conectada");
		}
		
		// Buscar el SnakeBody dentro del nivel
		var snakeBody = currentLevel.GetNodeOrNull("Snake/SnakeBody");
		if (snakeBody != null)
		{
				
				if (snakeBody.HasSignal("TimeUpdated"))
				{
					snakeBody.Connect("TimeUpdated", new Callable(this, nameof(OnTimeUpdated)));
					GD.Print("GameLayoutManager: Señal TimeUpdated conectada");
				}
				
				if (snakeBody.HasSignal("RecycledUpdated"))
				{
					snakeBody.Connect("RecycledUpdated", new Callable(this, nameof(OnRecycledUpdated)));
					GD.Print("GameLayoutManager: Señal RecycledUpdated conectada");
				}
				
				if (snakeBody.HasSignal("PipeRepaired"))
				{
					snakeBody.Connect("PipeRepaired", new Callable(this, nameof(OnPipeRepaired)));
					GD.Print("GameLayoutManager: Señal PipeRepaired (SnakeBody) conectada");
				}
			}
			else
			{
				GD.PrintErr("GameLayoutManager: No se encontró SnakeBody en el nivel");
			}
			
			// Conectar señales del WaterSystem a señales del nivel
			if (waterSystemNode != null && waterSystemNode.HasSignal("GameOver"))
			{
				waterSystemNode.Connect("GameOver", new Callable(currentLevel, "OnGameOver"));
				GD.Print("GameLayoutManager: WaterSystem.GameOver conectado a nivel");
			}
			
			if (waterSystemNode != null && waterSystemNode.HasSignal("Victory"))
			{
				waterSystemNode.Connect("Victory", new Callable(currentLevel, "OnVictory"));
				GD.Print("GameLayoutManager: WaterSystem.Victory conectado a nivel");
			}
		}
		else if (tipoNivel.ToLower() == "recycling" || tipoNivel.ToLower() == "recyclinglevel")
		{
			// Conectar señales del RecyclingSnake (nodo raíz del nivel)
			if (currentLevel.HasSignal("GameOver"))
			{
				currentLevel.Connect("GameOver", new Callable(this, nameof(OnLevelGameOver)));
				GD.Print("GameLayoutManager: Señal GameOver conectada");
			}
			
			if (currentLevel.HasSignal("Victory"))
			{
				currentLevel.Connect("Victory", new Callable(this, nameof(OnLevelVictory)));
				GD.Print("GameLayoutManager: Señal Victory conectada");
			}
			
			// Buscar el SnakeBody dentro del nivel
			var snakeBody = currentLevel.GetNodeOrNull("Snake/SnakeBody");
			if (snakeBody != null)
			{
				if (snakeBody.HasSignal("ScoreUpdated"))
				{
					snakeBody.Connect("ScoreUpdated", new Callable(this, nameof(OnScoreUpdated)));
					GD.Print("GameLayoutManager: Señal ScoreUpdated conectada");
				}
				
				if (snakeBody.HasSignal("TimeUpdated"))
				{
					snakeBody.Connect("TimeUpdated", new Callable(this, nameof(OnTimeUpdated)));
					GD.Print("GameLayoutManager: Señal TimeUpdated conectada");
				}
				
				if (snakeBody.HasSignal("RecycledUpdated"))
				{
					snakeBody.Connect("RecycledUpdated", new Callable(this, nameof(OnRecycledUpdated)));
					GD.Print("GameLayoutManager: Señal RecycledUpdated conectada");
				}
			}
			else
			{
				GD.PrintErr("GameLayoutManager: No se encontró SnakeBody en el nivel de reciclaje");
			}
			
			// Conectar SnakeBody.UpdateHealth al LifeSystem y LifeSystem.GameOver al SnakeBody
			var body = currentLevel.GetNodeOrNull("Snake/SnakeBody");
			if (body != null && lifeSystemNode != null)
			{
				if (body.HasSignal("UpdateHealth"))
				{
					body.Connect("UpdateHealth", new Callable(lifeSystemNode, "OnUpdateHealth"));
					GD.Print("GameLayoutManager: SnakeBody.UpdateHealth conectado a LifeSystem");
				}
				
				if (lifeSystemNode.HasSignal("GameOver"))
				{
					lifeSystemNode.Connect("GameOver", new Callable(body, "OnLifeSystemGameOver"));
					GD.Print("GameLayoutManager: LifeSystem.GameOver conectado a SnakeBody");
				}
			}
		}
		else if (tipoNivel.ToLower() == "reforestation" || tipoNivel.ToLower() == "reforestationlevel")
		{
			// Conectar señales del ReforestationSnake (nodo raíz del nivel)
			if (currentLevel.HasSignal("GameOver"))
			{
				currentLevel.Connect("GameOver", new Callable(this, nameof(OnReforestationGameOver)));
				GD.Print("GameLayoutManager: Señal GameOver (Reforestation) conectada");
			}
			
			if (currentLevel.HasSignal("Victory"))
			{
				currentLevel.Connect("Victory", new Callable(this, nameof(OnReforestationVictory)));
				GD.Print("GameLayoutManager: Señal Victory (Reforestation) conectada");
			}
			
			if (currentLevel.HasSignal("PlantAttempt"))
			{
				currentLevel.Connect("PlantAttempt", new Callable(this, nameof(OnPlantAttempt)));
				GD.Print("GameLayoutManager: Señal PlantAttempt conectada");
			}
			
			// Buscar el SnakeBody dentro del nivel
			var snakeBody = currentLevel.GetNodeOrNull("Snake/SnakeBody");
			if (snakeBody != null)
			{
				if (snakeBody.HasSignal("ScoreUpdated"))
				{
					snakeBody.Connect("ScoreUpdated", new Callable(this, nameof(OnScoreUpdated)));
					GD.Print("GameLayoutManager: Señal ScoreUpdated conectada");
				}
				
				if (snakeBody.HasSignal("TimeUpdated"))
				{
					snakeBody.Connect("TimeUpdated", new Callable(this, nameof(OnTimeUpdated)));
					GD.Print("GameLayoutManager: Señal TimeUpdated conectada");
				}
			}
			else
			{
				GD.PrintErr("GameLayoutManager: No se encontró SnakeBody en el nivel de reforestación");
			}
			
			// Conectar señales del PlantedSystem
			if (plantedSystemNode != null && plantedSystemNode.HasSignal("Victory"))
			{
				plantedSystemNode.Connect("Victory", new Callable(currentLevel, "OnVictory"));
				GD.Print("GameLayoutManager: PlantedSystem.Victory conectado a nivel");
			}
			
			// Conectar señales de los sistemas de recursos (para Game Over)
			if (seedsSystemNode != null && seedsSystemNode.HasSignal("NoSeeds"))
			{
				seedsSystemNode.Connect("NoSeeds", new Callable(currentLevel, "OnGameOver"));
				GD.Print("GameLayoutManager: SeedsSystem.NoSeeds conectado a nivel");
			}
			
			if (waterDropsSystemNode != null && waterDropsSystemNode.HasSignal("NoWater"))
			{
				waterDropsSystemNode.Connect("NoWater", new Callable(currentLevel, "OnGameOver"));
				GD.Print("GameLayoutManager: WaterDropsSystem.NoWater conectado a nivel");
			}
		}
		else if (tipoNivel.ToLower() == "minigame")
		{
			// Conectar señales del minijuego (WaterCatchLevel)
			if (currentLevel.HasSignal("GameOver"))
			{
				currentLevel.Connect("GameOver", new Callable(this, nameof(OnLevelGameOver)));
				GD.Print("GameLayoutManager: Señal GameOver (minijuego) conectada");
			}
			
			if (currentLevel.HasSignal("Victory"))
			{
				currentLevel.Connect("Victory", new Callable(this, nameof(OnLevelVictory)));
				GD.Print("GameLayoutManager: Señal Victory (minijuego) conectada");
			}
		}
	}

	/// <summary>
	/// Actualiza el puntaje en el label común
	/// </summary>
	public void ActualizarPuntaje(int puntos)
	{
		scoreLabel.Text = $"Puntos: {puntos}";
	}

	/// <summary>
	/// Actualiza el tiempo en el label común
	/// </summary>
	public void ActualizarTiempo(int tiempo)
	{
		int minutos = tiempo / 60;
		int segundos = tiempo % 60;
		timeLabel.Text = $"Tiempo: {minutos:00}:{segundos:00}";
	}

	/// <summary>
	/// Actualiza el contador de reciclados
	/// </summary>
	public void ActualizarReciclados(int cantidad)
	{
		recycledLabel.Text = $"Reciclados: {cantidad}";
	}
	
	// ========== Manejadores de Señales del Nivel ==========
	
	private void OnLevelGameOver(int score, int recycled, int time)
	{
		GD.Print($"GameLayoutManager: Game Over - Score: {score}, Recycled: {recycled}, Time: {time}");
		
		// Pausar el juego
		GetTree().Paused = true;
		
		// Actualizar estadísticas
		if (gameOverStatsLabel != null)
		{
			gameOverStatsLabel.Text = $"Puntos: {score}\nReciclados: {recycled}\nTiempo: {time}s";
		}
		
		// Mostrar pantalla de Game Over
		if (gameOverScreen != null)
		{
			gameOverScreen.Visible = true;
		}
	}
	
	private void OnLevelVictory(int score, int recycled, int time)
	{
		GD.Print($"GameLayoutManager: Victory! - Score: {score}, Recycled: {recycled}, Time: {time}");
		
		// Pausar el juego
		GetTree().Paused = true;
		
		// Actualizar estadísticas
		if (victoryStatsLabel != null)
		{
			victoryStatsLabel.Text = $"Puntos: {score}\nReciclados: {recycled}\nTiempo: {time}s";
		}
		
		// Mostrar pantalla de Victoria
		if (victoryScreen != null)
		{
			victoryScreen.Visible = true;
		}
	}
	
	private void OnPipeRepaired()
	{
		GD.Print("GameLayoutManager: Tubería reparada, actualizando WaterSystem");
		if (waterSystemNode != null && waterSystemNode.HasMethod("OnPipeRepaired"))
		{
			waterSystemNode.Call("OnPipeRepaired");
		}
	}
	
	private void OnPipeBroken()
	{
		GD.Print("GameLayoutManager: Tubería rota, actualizando WaterSystem");
		if (waterSystemNode != null && waterSystemNode.HasMethod("OnPipeBroken"))
		{
			waterSystemNode.Call("OnPipeBroken");
		}
	}
	
	private void InitializeWaterSystem()
	{
		GD.Print("GameLayoutManager: Inicializando WaterSystem con total de tuberías");
		
		// Buscar el DualGridTilemap en el nivel actual
		var dualGrid = currentLevel.FindChild("TileMapLayers", true, false);
		
		if (dualGrid != null && waterSystemNode != null)
		{
			// Obtener el total de tuberías
			int totalPipes = (int)dualGrid.Call("GetTotalPipes");
			int brokenPipes = (int)dualGrid.Call("GetBrokenPipesCount");
			GD.Print($"GameLayoutManager: Total de tuberías: {totalPipes}, Rotas: {brokenPipes}");
			
			// Configurar el WaterSystem
			waterSystemNode.Call("SetTotalPipes", totalPipes);
			waterSystemNode.Call("SetDualGridTilemap", dualGrid); // Pasar referencia al DualGrid
			waterSystemNode.Call("InitializeBrokenPipes"); // Inicializar sin parámetro
		}
		else
		{
			GD.PrintErr("GameLayoutManager: No se encontró DualGridTilemap o WaterSystem");
		}
	}
	
	private void OnPlantAttempt()
	{
		GD.Print("GameLayoutManager: Intento de plantar");
		
		// Verificar y consumir recursos
		bool hasSeeds = seedsSystemNode != null && (bool)seedsSystemNode.Call("HasSeeds", 1);
		int waterNeeded = waterDropsSystemNode != null ? (int)waterDropsSystemNode.Call("GetWaterPerPlant") : 5;
		bool hasWater = waterDropsSystemNode != null && (bool)waterDropsSystemNode.Call("HasWater", waterNeeded);
		
		if (!hasSeeds || !hasWater)
		{
			GD.Print("GameLayoutManager: Sin recursos suficientes - Game Over");
			if (currentLevel != null && currentLevel.HasMethod("OnGameOver"))
			{
				currentLevel.Call("OnGameOver");
			}
			return;
		}
		
		// Consumir recursos
		if (seedsSystemNode != null)
		{
			seedsSystemNode.Call("ConsumeSeeds", 1);
		}
		
		if (waterDropsSystemNode != null)
		{
			waterDropsSystemNode.Call("ConsumeWater", waterNeeded);
		}
		
		// Actualizar plantados
		if (plantedSystemNode != null && plantedSystemNode.HasMethod("OnPlantSuccessful"))
		{
			plantedSystemNode.Call("OnPlantSuccessful");
		}
	}
	
	private void OnReforestationGameOver(int score, int time)
	{
		GD.Print($"GameLayoutManager: Game Over (Reforestation) - Score: {score}, Time: {time}");
		
		// Pausar el juego
		GetTree().Paused = true;
		
		// Actualizar estadísticas
		if (gameOverStatsLabel != null)
		{
			gameOverStatsLabel.Text = $"Puntos: {score}\nTiempo: {time}s";
		}
		
		// Mostrar pantalla de Game Over
		if (gameOverScreen != null)
		{
			gameOverScreen.Visible = true;
		}
	}
	
	private void OnReforestationVictory(int score, int time)
	{
		GD.Print($"GameLayoutManager: Victory! (Reforestation) - Score: {score}, Time: {time}");
		
		// Pausar el juego
		GetTree().Paused = true;
		
		// Actualizar estadísticas
		if (victoryStatsLabel != null)
		{
			victoryStatsLabel.Text = $"Puntos: {score}\nTiempo: {time}s";
		}
		
		// Mostrar pantalla de Victoria
		if (victoryScreen != null)
		{
			victoryScreen.Visible = true;
		}
	}
	
	private void OnScoreUpdated(int score)
	{
		ActualizarPuntaje(score);
	}
	
	private void OnTimeUpdated(int time)
	{
		ActualizarTiempo(time);
	}
	
	private void OnRecycledUpdated(int recycled)
	{
		ActualizarReciclados(recycled);
	}
	
	// ========== Manejadores de Botones de Pantallas ==========
	
	/// <summary>
	/// Reinicia el nivel actual (botón "Volver a jugar")
	/// </summary>
	private void OnAgainPressed()
	{
		GD.Print("GameLayoutManager: Reiniciando nivel");
		
		// Ocultar pantallas
		if (gameOverScreen != null) gameOverScreen.Visible = false;
		if (victoryScreen != null) victoryScreen.Visible = false;
		
		// Despausar
		GetTree().Paused = false;
		
		// Recargar la escena actual
		GetTree().ReloadCurrentScene();
	}
	
	/// <summary>
	/// Vuelve al menú principal (botón "Salir" / "Menú Principal")
	/// </summary>
	private void OnSalirPressed()
	{
		GD.Print("GameLayoutManager: Volviendo al menú principal");
		
		// Despausar antes de cambiar de escena
		GetTree().Paused = false;
		
		// Cambiar a la escena del menú principal
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}
	
	/// <summary>
	/// Continúa al siguiente nivel (botón "Siguiente Nivel" en pantalla de Victoria)
	/// </summary>
	private void OnContinuarPressed()
	{
		GD.Print("GameLayoutManager: Continuar al siguiente nivel");
		
		// Ocultar pantalla de victoria
		if (victoryScreen != null) victoryScreen.Visible = false;
		
		// Despausar
		GetTree().Paused = false;
		
		// Si es nivel de agua, cargar el minijuego en el contenedor actual
		if (currentLevelType.ToLower() == "water" || currentLevelType.ToLower() == "waterlevel")
		{
			GD.Print("GameLayoutManager: Cargando minijuego de agua en el layout");
			CargarNivel("res://Scenes/WaterCatchLevel.tscn", "minigame");
		}
		else
		{
			// Para otros niveles, volver al menú principal
			GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
		}
	}
}
