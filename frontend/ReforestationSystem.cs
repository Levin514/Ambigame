using Godot;
using System;
using Snake;

public partial class ReforestationSystem : Control
{
	[Export] private ProgressBar plantedBar;
	[Export] private Label plantedLabel;
	[Export] private Label seedsLabel;
	[Export] private Label waterLabel;
	
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void VictoryEventHandler();
	
	private int currentPlanted = 0;
	private int maxPlanted = 10;
	private int currentSeeds = 5;
	private int currentWater = 30;
	private int waterPerPlant = 5;
	
	private bool hasEmittedGameOver = false;
	private bool hasEmittedVictory = false;
	private bool isGameOver = false;

	public override void _Ready()
	{
		GD.Print("ReforestationSystem: Inicializando sistema de reforestación");
		currentPlanted = 0;
		currentSeeds = 5;
		currentWater = 30;
		UpdateDisplay();
	}

	public void OnPlantAttempt()
	{
		if (isGameOver) return;
		
		GD.Print($"ReforestationSystem: Intento de plantar - Semillas: {currentSeeds}, Agua: {currentWater}");
		
		// Verificar si tiene suficientes recursos
		if (currentSeeds < 1 || currentWater < waterPerPlant)
		{
			isGameOver = true;
			hasEmittedGameOver = true;
			GD.Print("ReforestationSystem: Sin recursos suficientes - Emitiendo GameOver");
			EmitSignal(SignalName.GameOver);
			return;
		}
		
		// Consumir recursos y plantar
		currentSeeds -= 1;
		currentWater -= waterPerPlant;
		currentPlanted += 1;
		
		GD.Print($"ReforestationSystem: Plantado exitoso - {currentPlanted}/{maxPlanted}");
		UpdateDisplay();
		CheckGameStatus();
	}

	private void UpdateDisplay()
	{
		if (plantedBar != null)
		{
			plantedBar.MaxValue = maxPlanted;
			plantedBar.Value = currentPlanted;
		}
		
		if (plantedLabel != null)
		{
			plantedLabel.Text = $"{currentPlanted}/{maxPlanted}";
		}
		
		if (seedsLabel != null)
		{
			seedsLabel.Text = $"Semillas: {currentSeeds}";
		}
		
		if (waterLabel != null)
		{
			waterLabel.Text = $"Agua: {currentWater}";
		}
	}

	private void CheckGameStatus()
	{
		if (currentPlanted >= maxPlanted && !hasEmittedVictory)
		{
			isGameOver = true;
			hasEmittedVictory = true;
			GD.Print("ReforestationSystem: ¡Victoria! 10 semillas plantadas");
			EmitSignal(SignalName.Victory);
		}
	}
	
	public bool CanPlant()
	{
		return currentSeeds >= 1 && currentWater >= waterPerPlant && !isGameOver;
	}
}
