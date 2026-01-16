using Godot;
using System;
using Snake;

public partial class WaterSavingMapSelector : MarginContainer
{
	private NavigationManager navigationManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/WaterSavingMapSelector.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_back_button_pressed()
	{
		navigationManager?.GoBack();
	}

	public void Casa_level_button_pressed()
	{
		// Cargar nivel de Casa (Map1) con slides de tutorial
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_WATER_1", 
			"SLIDE_WATER_2", 
			"SLIDE_WATER_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Water/WaterLevel_Map1.tscn", 
			"water", 
			"Casa", 
			1, 
			slides,
			"TUTORIAL_TITLE"
		);
	}

	public void Escuela_level_button_pressed()
	{
		// Cargar nivel de Escuela (Map2)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_WATER_1", 
			"SLIDE_WATER_2", 
			"SLIDE_WATER_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Water/WaterLevel_Map2.tscn", 
			"water", 
			"Escuela", 
			2,
			slides,
			"TUTORIAL_TITLE"
		);
	}

	public void Parque_level_button_pressed()
	{
		// Cargar nivel de Parque (Map3)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_WATER_1", 
			"SLIDE_WATER_2", 
			"SLIDE_WATER_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Water/WaterLevel_Map3.tscn", 
			"water", 
			"Parque", 
			3,
			slides,
			"TUTORIAL_TITLE"
		);
	}

	public void Random_level_button_pressed()
	{
		// Cargar nivel Bonus/Aleatorio
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_WATER_1", 
			"SLIDE_WATER_2", 
			"SLIDE_WATER_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Water/WaterLevel_Bonus.tscn", 
			"water", 
			"Bonus", 
			4,
			slides,
			"TUTORIAL_TITLE"
		);
	}
	
	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}
}
