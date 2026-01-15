using Godot;
using System;

public partial class WaterDropsSystem : PanelContainer
{
	[Export] private Label waterLabel;
	
	[Signal] public delegate void NoWaterEventHandler();
	
	private int currentWater = 20;
	private int initialWater = 20;
	private int waterPerPlant = 2;

	public override void _Ready()
	{
		GD.Print("WaterDropsSystem: Inicializando sistema de gotas de agua");
		currentWater = initialWater;
		UpdateDisplay();
	}

	public bool HasWater(int amount)
	{
		return currentWater >= amount;
	}

	public bool ConsumeWater(int amount)
	{
		if (currentWater < amount)
		{
			GD.Print($"WaterDropsSystem: No hay suficiente agua ({currentWater}/{amount})");
			EmitSignal(SignalName.NoWater);
			return false;
		}
		
		currentWater -= amount;
		GD.Print($"WaterDropsSystem: Consumidas {amount} gotas de agua - Restantes: {currentWater}");
		UpdateDisplay();
		return true;
	}

	public void AddWater(int amount)
	{
		currentWater += amount;
		GD.Print($"WaterDropsSystem: Añadidas {amount} gotas de agua - Total: {currentWater}");
		UpdateDisplay();
	}

	public int GetWaterPerPlant()
	{
		return waterPerPlant;
	}

	private void UpdateDisplay()
	{
		if (waterLabel != null)
		{
			waterLabel.Text = $"Agua: {currentWater}";
		}
	}

	public void Reset()
	{
		currentWater = initialWater;
		UpdateDisplay();
	}
}
