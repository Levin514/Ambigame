using Godot;
using System;

public partial class MapSelectionLevel3Scene : MarginContainer
{
	[Export] private Label _titleLabel;
	[Export] private Label _coinsLabel;

	public override void _Ready()
	{
		UpdateCoinsDisplay();
	}

	private void UpdateCoinsDisplay()
	{
		// Actualizar contador de monedas
	}

	public void SelectMap(int mapNumber)
	{
		GD.Print($"Mapa {mapNumber} del Nivel 3 seleccionado");
		GetTree().ChangeSceneToFile("res://Scenes/MainGame.tscn");
	}

	public void SelectRandomMap()
	{
		GD.Print("Mapa aleatorio del Nivel 3 seleccionado");
		GetTree().ChangeSceneToFile("res://Scenes/MainGame.tscn");
	}

	public void GoBack()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelSelectionScene.tscn");
	}
}
