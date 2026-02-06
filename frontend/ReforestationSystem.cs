using Godot;
using System;
using Snake;

public partial class ReforestationSystem : Control
{
	[Export] private ProgressBar plantedBar;
	[Export] private Label plantedLabel;
	[Export] private Label seedsLabel;
	[Export] private Label waterLabel;
	
	[Signal] public delegate void GameOverEventHandler(string reason);
	[Signal] public delegate void VictoryEventHandler();
	
	private int maxPlanted = 10;
	private int currentSeeds = 10; // 10 semillas iniciales
	private int currentWater = 10; // 10 gotas iniciales
	private int waterPerPlant = 1; // 1 gotas por planta
	
	private Node dualGridTilemap = null;
	
	private bool hasEmittedGameOver = false;
	private bool hasEmittedVictory = false;
	private bool isGameOver = false;
	private float timeAccumulator = 0f;

	public override void _Ready()
	{
		GD.Print("ReforestationSystem: Inicializando sistema de reforestación");
		currentSeeds = 10;
		currentWater = 10;
		UpdateDisplay();
	}

    public override void _Process(double delta)
    {
        timeAccumulator += (float)delta;
		if (timeAccumulator >= 1f)
		{
			timeAccumulator = 0f;

			// Verificar derrota por no tener recursos
			if (currentSeeds == 0 || currentWater == 0)
			{
				CheckVictory();
				CheckGameOver();
			}
		}
    }

	
	public void SetDualGridTilemap(Node dualGrid)
	{
		dualGridTilemap = dualGrid;
		GD.Print("ReforestationSystem: Referencia a DualGridTilemap configurada");
	}

	public void OnPlantSeed()
	{
		if (isGameOver) return;
		
		GD.Print($"ReforestationSystem: Intento de plantar semilla - Semillas: {currentSeeds}");
		
		// Verificar si tiene semillas
		if (currentSeeds < 1)
		{
			GD.Print("ReforestationSystem: Sin semillas - No se puede plantar");
			return;
		}
		
		// Consumir semilla
		currentSeeds -= 1;
		GD.Print($"ReforestationSystem: Semilla plantada - Quedan: {currentSeeds}");
		UpdateDisplay();
		CheckVictory();
		CheckGameOver(); // Verificar si puede continuar
	}
	
	public void OnWaterPlant()
	{
		if (isGameOver) return;
		
		GD.Print($"ReforestationSystem: Intento de regar - Agua: {currentWater}");
		
		// Verificar si tiene agua
		if (currentWater < waterPerPlant)
		{
			GD.Print("ReforestationSystem: Sin agua - No se puede regar");
			return;
		}
		
		// Consumir agua
		currentWater -= waterPerPlant;
		GD.Print($"ReforestationSystem: Planta regada - Queda agua: {currentWater}");
		UpdateDisplay();
		CheckVictory();
		CheckGameOver(); // Verificar si puede continuar
	}
	
	public void CheckVictory()
	{
		if (isGameOver || hasEmittedVictory) return;
		
		if (dualGridTilemap != null)
		{
			int fullyGrown = (int)dualGridTilemap.Call("GetFullyGrownCount");
			int total = (int)dualGridTilemap.Call("GetTotalPlantSpots");
			
			GD.Print($"ReforestationSystem: {fullyGrown}/{total} plantas completamente crecidas");
			
			if (fullyGrown >= total && total > 0)
			{
				isGameOver = true;
				hasEmittedVictory = true;
				GD.Print("ReforestationSystem: ¡Victoria! Todas las plantas han crecido");
				EmitSignal(SignalName.Victory);
			}
		}
	}
	
	public void CheckGameOver()
	{
		if (isGameOver || hasEmittedGameOver) return;
		
		if (dualGridTilemap != null)
		{
			int fullyGrown = (int)dualGridTilemap.Call("GetFullyGrownCount");
			int total = (int)dualGridTilemap.Call("GetTotalPlantSpots");
			
			// Si no tiene semillas y no puede ganar
			if (currentSeeds == 0 && fullyGrown < total)
			{
				isGameOver = true;
				hasEmittedGameOver = true;
				GD.Print($"ReforestationSystem: Game Over - Sin semillas ({fullyGrown}/{total} árboles)");
				EmitSignal(SignalName.GameOver, "no_seeds");
			}
		}
	}

	private void UpdateDisplay()
	{
		if (dualGridTilemap != null)
		{
			int fullyGrown = (int)dualGridTilemap.Call("GetFullyGrownCount");
			int total = (int)dualGridTilemap.Call("GetTotalPlantSpots");
			
			if (plantedBar != null)
			{
				plantedBar.MaxValue = total;
				plantedBar.Value = fullyGrown;
			}
			
			if (plantedLabel != null)
			{
				plantedLabel.Text = $"{fullyGrown}/{total}";
			}
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
	
	public bool HasSeeds(int amount)
	{
		return currentSeeds >= amount;
	}
	
	public bool HasWater(int amount)
	{
		return currentWater >= amount;
	}
}
