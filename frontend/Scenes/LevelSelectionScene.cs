using Godot;
using System;

public partial class LevelSelectionScene : MarginContainer
{
	[Export] private Label _titleLabel;
	[Export] private Label _coinsLabel;

	public override void _Ready()
	{
		// Actualizar título y monedas si es necesario
		UpdateCoinsDisplay();
	}

	private void UpdateCoinsDisplay()
	{
		// Aquí puedes actualizar el contador de monedas desde el jugador
		// _coinsLabel.Text = "300"; // Ejemplo
	}

	public void SelectLevel(int levelNumber)
	{
		GD.Print($"Nivel seleccionado: {levelNumber}");
		// Ir a la escena de selección de mapa correspondiente al nivel
		switch (levelNumber)
		{
			case 1:
				GetTree().ChangeSceneToFile("res://Scenes/MapSelectionLevel1Scene.tscn");
				break;
			case 2:
				GetTree().ChangeSceneToFile("res://Scenes/MapSelectionLevel2Scene.tscn");
				break;
			case 3:
				GetTree().ChangeSceneToFile("res://Scenes/MapSelectionLevel3Scene.tscn");
				break;
		}
	}

	public void GoBack()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelectionScene.tscn");
	}
}
