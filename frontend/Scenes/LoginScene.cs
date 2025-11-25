using Godot;
using System;
using System.Text;
using Newtonsoft.Json;
using Snakes.Models;

public partial class LoginScene : MarginContainer
{


	override public void _Ready()
	{
		
	}
	
	public void GoToMain()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameMode.tscn");
	}

	
}
