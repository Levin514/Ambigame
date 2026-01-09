using Godot;
using System;

public partial class PlantedSystem : Control
{
	[Export] private ProgressBar plantedBar;
	[Export] private Label plantedLabel;
	
	[Signal] public delegate void VictoryEventHandler();
	
	private int currentPlanted = 0;
	private int maxPlanted = 10;
	private bool hasEmittedVictory = false;

	public override void _Ready()
	{
		GD.Print("PlantedSystem: Inicializando sistema de plantados");
		currentPlanted = 0;
		UpdateDisplay();
	}

	public void OnPlantSuccessful()
	{
		if (hasEmittedVictory) return;
		
		currentPlanted += 1;
		GD.Print($"PlantedSystem: Plantado exitoso - {currentPlanted}/{maxPlanted}");
		UpdateDisplay();
		CheckVictory();
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
	}

	private void CheckVictory()
	{
		if (currentPlanted >= maxPlanted && !hasEmittedVictory)
		{
			hasEmittedVictory = true;
			GD.Print("PlantedSystem: ¡Victoria! Todos los árboles plantados");
			EmitSignal(SignalName.Victory);
		}
	}

	public void Reset()
	{
		currentPlanted = 0;
		hasEmittedVictory = false;
		UpdateDisplay();
	}
}
