using Godot;
using System;
using Snake;

public partial class WaterSystem : Control
{
	[Export] private ProgressBar waterBar;
	[Export] private Label waterLabel;
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void VictoryEventHandler(int waterScore);
	[Signal] public delegate void PipesRepairedUpdatedEventHandler(int pipesRepaired);
	[Signal] public delegate void ScoreUpdatedEventHandler(int score); // Nueva señal para actualizar UI
	
	private int totalPipes = 0;
	private int pipesRepaired = 0;
	private int pointsPerPipe = 10; // Puntos por cada tubería reparada
	private int currentScore = 0;
	private Node dualGridTilemap = null; // Referencia al DualGridTilemap
	private float currentWater;
	private float maxWater = 100f;
	private float baseWaterLossRate = 3f; // Tasa base de pérdida por segundo
	private float currentWaterLossRate = 0f; // Tasa actual (dinámica)
	
	private bool hasEmittedGameOver = false;
	private bool hasEmittedVictory = false;
	private bool isGameOver = false;

	public override void _Ready()
	{
        GD.Print("WaterSystem: Inicializando sistema de agua");
		currentWater = maxWater; // Iniciar en 100%
		UpdateWaterDisplay();
		// Emitir señales iniciales
		EmitSignal(SignalName.PipesRepairedUpdated, pipesRepaired);
		EmitSignal(SignalName.ScoreUpdated, currentScore);
		GD.Print($"WaterSystem: Score inicial: {currentScore}");
	}

	public void SetTotalPipes(int total)
	{
		totalPipes = total;
		GD.Print($"WaterSystem: Total de tuberías configurado a {totalPipes}");
		UpdateWaterDisplay();
	}

	public void SetDualGridTilemap(Node dualGrid)
	{
		dualGridTilemap = dualGrid;
		GD.Print("WaterSystem: Referencia a DualGridTilemap configurada");
	}

	public void InitializeBrokenPipes()
	{
		if (dualGridTilemap != null)
		{
			int brokenPipes = (int)dualGridTilemap.Call("GetBrokenPipesCount");
			GD.Print($"WaterSystem: Tuberías rotas iniciales: {brokenPipes}/{totalPipes}");
			UpdateWaterLossRate();
		}
	}

	public override void _Process(double delta)
	{
		if (isGameOver) return; // No procesar si el juego terminó
		
		// Disminuir agua según la tasa dinámica (basada en tuberías rotas)
		currentWater -= currentWaterLossRate * (float)delta;
		
		// Limitar entre 0 y maxWater
		currentWater = Mathf.Clamp(currentWater, 0, maxWater);
		
		UpdateWaterDisplay();
		CheckGameStatus();
	}

	public void OnPipeRepaired()
	{
		GD.Print("WaterSystem: Tubería reparada, recalculando pérdida de agua");
		pipesRepaired++;
		
		// Calcular puntos según el porcentaje de agua ACTUAL
		int pointsEarned = 0;
		if (currentWater >= 85f)
		{
			pointsEarned = 50;
		}
		else if (currentWater >= 50f)
		{
			pointsEarned = 30;
		}
		else if (currentWater >= 25f)
		{
			pointsEarned = 10;
		}
		else
		{
			pointsEarned = 5;
		}
		
		// Sumar puntos al SnakeBody directamente
		var snake = GetTree().Root.FindChild("Snake", true, false);
		if (snake != null)
		{
			var snakeBody = snake.GetNodeOrNull("SnakeBody");
			if (snakeBody != null && snakeBody.HasMethod("AddScore"))
			{
				snakeBody.Call("AddScore", pointsEarned);
				GD.Print($"WaterSystem: Tubería reparada con {currentWater:F1}% de agua → +{pointsEarned} puntos sumados al SnakeBody");
			}
			else
			{
				GD.PrintErr("WaterSystem: No se pudo encontrar SnakeBody para sumar puntos");
			}
		}
		
		// Mantener currentScore para referencia interna
		currentScore += pointsEarned;
		GD.Print($"WaterSystem: Tuberías reparadas: {pipesRepaired}/{totalPipes}");
		
		EmitSignal(SignalName.PipesRepairedUpdated, pipesRepaired);
		UpdateWaterLossRate();
		CheckGameStatus(); // Verificar victoria inmediatamente
	}

	public void OnPipeBroken()
	{
		GD.Print("WaterSystem: Tubería rota, recalculando pérdida de agua");
		UpdateWaterLossRate();
	}

	private void UpdateWaterDisplay()
	{
		if (waterBar != null)
		{
			waterBar.Value = currentWater;
		}
		
		if (waterLabel != null)
		{
			waterLabel.Text = $"{Mathf.RoundToInt(currentWater)}%";
		}
	}

	private void UpdateWaterLossRate()
	{
		if (dualGridTilemap != null && totalPipes > 0)
		{
			// Consultar el número REAL de tuberías rotas del DualGridTilemap
			int brokenPipes = (int)dualGridTilemap.Call("GetBrokenPipesCount");
			
			// Pérdida proporcional: baseRate × (tuberías rotas / total)
			float lossRatio = (float)brokenPipes / totalPipes;
			currentWaterLossRate = baseWaterLossRate * lossRatio;
			GD.Print($"WaterSystem: Tuberías rotas: {brokenPipes}/{totalPipes}, pérdida: {currentWaterLossRate:F2}/s");
		}
		else
		{
			currentWaterLossRate = 0f;
		}
	}

	private int CalculateScore()
	{
		// El puntaje ya está calculado según el momento de cada reparación
		// Ya no hay bonificación adicional al final
		GD.Print($"WaterSystem: Puntaje final: {currentScore} (sin bonus adicional)");
		return currentScore;
	}

	private void CheckGameStatus()
	{
		if (currentWater <= 0 && !hasEmittedGameOver)
		{
			isGameOver = true;
			hasEmittedGameOver = true;
			GD.Print("WaterSystem: Emitiendo GameOver - Agua agotada");
			EmitSignal(SignalName.GameOver);
		}
		else if (dualGridTilemap != null && totalPipes > 0 && !hasEmittedVictory)
		{
			// Consultar el número REAL de tuberías rotas
			int brokenPipes = (int)dualGridTilemap.Call("GetBrokenPipesCount");
			
			if (brokenPipes <= 0)
			{
				isGameOver = true;
				hasEmittedVictory = true;
				int score = CalculateScore();
				GD.Print($"WaterSystem: Emitiendo Victory - Todas las tuberías reparadas, Puntaje: {score}");
				// Emitir con puntaje como parámetro
				EmitSignal(SignalName.Victory, score);
			}
		}
	}
}
