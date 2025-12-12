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
		// Aquí se cargará el nivel de Casa
		GetTree().ChangeSceneToFile("res://Scenes/WaterGame.tscn");
	}

	public void Escuela_level_button_pressed()
	{
		// Aquí se cargará el nivel de Escuela
		GetTree().ChangeSceneToFile("res://Scenes/WaterGame.tscn");
	}

	public void Parque_level_button_pressed()
	{
		// Aquí se cargará el nivel de Parque
		GetTree().ChangeSceneToFile("res://Scenes/WaterGame.tscn");
	}

	public void Random_level_button_pressed()
	{
		// Aquí se cargará el nivel Aleatorio
		GetTree().ChangeSceneToFile("res://Scenes/WaterGame.tscn");
	}
}
