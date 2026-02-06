using Godot;
using System;
using Snakes.Models;
using Snake;

public partial class MainScene : MarginContainer
{
	[Export] private Label _userDataLabel;
	private NavigationManager navigationManager;
	
	public override void _Ready()
	{	
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/MainScene.tscn");
		
		if (Player.GetInstance() == null) 
			_userDataLabel.Text = "Bypass mode";
		else
			_userDataLabel.Text = Player.GetInstance().ToString();
	}
	public void Level_1_button_pressed()
	{
		navigationManager?.NavigateTo("res://Scenes/BackgroundSelector.tscn");
	}

	public void Level_2_button_pressed()
	{
		navigationManager?.NavigateTo("res://Scenes/WaterSavingMapSelector.tscn");
	}

	public void Level_3_button_pressed()
	{
		// Cargar nivel de reforestación directamente
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_REFORESTATION_1", 
			"SLIDE_REFORESTATION_2", 
			"SLIDE_REFORESTATION_3",
			"SLIDE_REFORESTATION_4" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Reforestation/ReforestationLevel_Map1.tscn", 
			"reforestation", 
			"Reforestación", 
			1,
			slides,
			"TUTORIAL_TITLE"
		);
	}

	public void GoToTutorial()
	{
		navigationManager?.NavigateTo("res://Scenes/TutorialScene.tscn");
	}
	
	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		navigationManager?.ClearHistory();
		GetTree().ChangeSceneToFile("res://Scenes/GameMode.tscn");
	}
}
