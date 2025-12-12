using Godot;
using System;
using Snake;

public partial class WaterSystem : Control
{
	[Export] private ProgressBar waterBar;
	[Export] private Label waterLabel;
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void VictoryEventHandler();
	
	private float currentWater;
	private float maxWater = 100f;
	private float waterDecreaseRate = 2f; // 1 cada 0.5 segundos (2 por segundo)
	private float waterIncreaseAmount = 15f; // Porcentaje por tubería reparada
	
	private bool hasEmittedGameOver = false;
	private bool hasEmittedVictory = false;
	private bool isGameOver = false;

	public override void _Ready()
	{
        GD.Print("WaterSystem: Inicializando sistema de agua");
		currentWater = maxWater / 2f;
		UpdateWaterDisplay();
	}

	public override void _Process(double delta)
	{
		if (isGameOver) return; // No procesar si el juego terminó
		
		// Disminuir agua constantemente
		currentWater -= waterDecreaseRate * (float)delta;
		
		// Limitar entre 0 y maxWater
		currentWater = Mathf.Clamp(currentWater, 0, maxWater);
		
		UpdateWaterDisplay();
		CheckGameStatus();
	}

	public void OnPipeRepaired()
	{
        GD.Print("WaterSystem: Tubería reparada, aumentando agua");
		currentWater += waterIncreaseAmount;
		currentWater = Mathf.Clamp(currentWater, 0, maxWater);
		UpdateWaterDisplay();
		CheckGameStatus(); // Verificar victoria inmediatamente
	}

	private void UpdateWaterDisplay()
	{
		if (waterBar != null)
		{
			waterBar.Value = currentWater;
		}
		
		if (waterLabel != null)
		{
			waterLabel.Text = $"Agua: {Mathf.RoundToInt(currentWater)}%";
		}
	}

	private void CheckGameStatus()
	{
		if (currentWater <= 0 && !hasEmittedGameOver)
		{
			isGameOver = true;
			hasEmittedGameOver = true;
			GD.Print("WaterSystem: Emitiendo GameOver");
			EmitSignal(SignalName.GameOver);
		}
		else if (currentWater >= maxWater && !hasEmittedVictory)
		{
			isGameOver = true;
			hasEmittedVictory = true;
			GD.Print("WaterSystem: Emitiendo Victory");
			EmitSignal(SignalName.Victory);
		}
	}
}
