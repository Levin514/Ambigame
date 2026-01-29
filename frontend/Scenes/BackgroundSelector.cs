using Godot;
using System;
using Snake;

public partial class BackgroundSelector : MarginContainer
{
	private NavigationManager navigationManager;
	private Random rnd = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/BackgroundSelector.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_back_button_pressed()
    {
        navigationManager?.GoBack();
    }

	public void Forest_level_button_pressed()
    {
        // Cargar nivel de Bosque (Map1 del nivel de Reciclaje)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_RECYCLING_1", 
			"SLIDE_RECYCLING_2", 
			"SLIDE_RECYCLING_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Recycling/RecyclingLevel_Map1.tscn", 
			"recycling", 
			"Bosque", 
			1,
			slides,
			"TUTORIAL_TITLE"
		);
    }

	public void Beach_level_button_pressed()
    {
        // Cargar nivel de Escuela (Map2)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_RECYCLING_1", 
			"SLIDE_RECYCLING_2", 
			"SLIDE_RECYCLING_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Recycling/RecyclingLevel_Map2.tscn", 
			"recycling", 
			"Escuela", 
			2,
			slides,
			"TUTORIAL_TITLE"
		);
    }

	public void Manglar_level_button_pressed()
    {
        // Cargar nivel de Parque (Map3)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_RECYCLING_1", 
			"SLIDE_RECYCLING_2", 
			"SLIDE_RECYCLING_3" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Recycling/RecyclingLevel_Map3.tscn", 
			"recycling", 
			"Parque", 
			3,
			slides,
			"TUTORIAL_TITLE"
		);
    }

	public void Random_level_button_pressed()
    {
		int rndNumber = rnd.Next(0,3);
		if(rndNumber == 0)
		{
			Forest_level_button_pressed();
		}
		else if(rndNumber == 1)
		{
			Beach_level_button_pressed();
		}
		else
		{
			Manglar_level_button_pressed();
		}
    }
	
	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}
}
