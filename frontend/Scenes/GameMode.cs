using Godot;
using System;
using Snakes.Models;

public partial class GameMode : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
	}

	public void _on_history_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
    }
}
