using Godot;
using System;

public partial class GameModeSelectionScene : MarginContainer
{
	public override void _Ready()
	{
	}

	public void SelectHistoryMode()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LevelSelectionScene.tscn");
	}

	public void SelectInfiniteMode()
	{
		// Modo infinito va directo al juego
		GetTree().ChangeSceneToFile("res://Scenes/MainGame.tscn");
	}

	public void GoBack()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}
}
