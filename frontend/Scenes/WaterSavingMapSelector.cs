using Godot;
using System;

public partial class WaterSavingMapSelector : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}

	public void Casa_level_button_pressed()
	{
		// Cargar nivel de Casa (Map1)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		levelManager.LoadLevel("res://Scenes/Levels/Water/WaterLevel_Map1.tscn", "water", "Casa", 1);
	}

	public void Escuela_level_button_pressed()
	{
		// Cargar nivel de Escuela (Map2)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		levelManager.LoadLevel("res://Scenes/Levels/Water/WaterLevel_Map2.tscn", "water", "Escuela", 2);
	}

	public void Parque_level_button_pressed()
	{
		// Cargar nivel de Parque (Map3)
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		levelManager.LoadLevel("res://Scenes/Levels/Water/WaterLevel_Map3.tscn", "water", "Parque", 3);
	}

	public void Random_level_button_pressed()
	{
		// Cargar nivel Bonus/Aleatorio
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		levelManager.LoadLevel("res://Scenes/Levels/Water/WaterLevel_Bonus.tscn", "water", "Bonus", 4);
	}
}
