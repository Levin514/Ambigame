using Godot;
using System;
using Snake;

public partial class SeedsSystem : PanelContainer
{
	[Export] private Label seedsLabel;
	
	[Signal] public delegate void NoSeedsEventHandler();
	
	private int currentSeeds = 10;
	private int initialSeeds = 10;

	public override void _Ready()
	{
		GD.Print("SeedsSystem: Inicializando sistema de semillas");
		currentSeeds = initialSeeds;
		UpdateDisplay();
	}

	public bool HasSeeds(int amount = 1)
	{
		return currentSeeds >= amount;
	}

	public bool ConsumeSeeds(int amount = 1)
	{
		if (currentSeeds < amount)
		{
			GD.Print($"SeedsSystem: No hay suficientes semillas ({currentSeeds}/{amount})");
			EmitSignal(SignalName.NoSeeds);
			return false;
		}
		
		currentSeeds -= amount;
		GD.Print($"SeedsSystem: Consumidas {amount} semillas - Restantes: {currentSeeds}");
		UpdateDisplay();
		return true;
	}

	public void AddSeeds(int amount)
	{
		currentSeeds += amount;
		GD.Print($"SeedsSystem: Añadidas {amount} semillas - Total: {currentSeeds}");
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if (seedsLabel != null)
		{
			seedsLabel.Text = $"{TranslationManager.Tr("UI_SEEDS")}: {currentSeeds}";
		}
	}

	public void Reset()
	{
		currentSeeds = initialSeeds;
		UpdateDisplay();
	}
}
