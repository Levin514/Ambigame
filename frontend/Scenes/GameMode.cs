using Godot;
using System;
using Snakes.Models;
using Snake;

public partial class GameMode : MarginContainer
{
	private NavigationManager navigationManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/GameMode.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		navigationManager?.ClearHistory();
		GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
	}

	public void _on_history_button_pressed()
	{
		navigationManager?.NavigateTo("res://Scenes/MainScene.tscn");
	}
	
	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}
}
