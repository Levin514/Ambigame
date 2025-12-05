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
	public void Play()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelectionScene.tscn");
	}

	public void GoToCustomization()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CustomizationScene.tscn");
	}

	public void GoToRanking()
	{
		GetTree().ChangeSceneToFile("res://Scenes/RankingScene.tscn");
	}
	
	public void GoToHistorial()
	{
		GetTree().ChangeSceneToFile("res://Scenes/HistorialScene.tscn");
	}

	public void GoToTutorial()
	{
		GetTree().ChangeSceneToFile("res://Scenes/TutorialScene.tscn");
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
	}
}
