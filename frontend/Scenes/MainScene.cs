using Godot;
using System;
using Snakes.Models;

public partial class MainScene : MarginContainer
{
	[Export] private Label _userDataLabel;
	public override void _Ready()
	{	
		if (Player.GetInstance() == null) 
			_userDataLabel.Text = "Bypass mode";
		else
			_userDataLabel.Text = Player.GetInstance().ToString();
	}
	public void Level_1_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/BackgroundSelector.tscn");
	}

	public void GoToTutorial()
	{
		GetTree().ChangeSceneToFile("res://Scenes/TutorialScene.tscn");
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		GetTree().ChangeSceneToFile("res://Scenes/GameMode.tscn");
	}
}
