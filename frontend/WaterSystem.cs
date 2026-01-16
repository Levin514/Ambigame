using Godot;
using System;
using Snake;

public partial class WaterSystem : Control
{
	[Export] private ProgressBar waterBar;
	[Export] private Label waterLabel;
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void VictoryEventHandler(int waterScore);
	
	private int totalPipes = 0;
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
		// Puntaje base: 1000 puntos
		// Multiplicador según agua salvada: (agua/100) × 1000
		// Ejemplos:
		// - 80% agua restante → 800 puntos
		// - 50% agua restante → 500 puntos  
		// - 5% agua restante → 50 puntos
		int baseScore = 1000;
		float waterPercentage = currentWater / maxWater;
		int finalScore = Mathf.RoundToInt(baseScore * waterPercentage);
		GD.Print($"WaterSystem: Puntaje calculado - Agua: {currentWater:F1}% → {finalScore} puntos");
		return finalScore;
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
