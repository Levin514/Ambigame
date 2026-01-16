using Godot;
using System;
using System.Text;
using Newtonsoft.Json;
using Snakes.Models;
using Snake;

public partial class LoginScene : MarginContainer
{
	private NavigationManager navigationManager;

	override public void _Ready()
	{
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		// LoginScene es la raíz, limpiamos historial
		navigationManager?.ClearHistory();
		navigationManager?.SetCurrentScene("res://Scenes/LoginScene.tscn");
	}
	
	public void GoToMain()
	{
		navigationManager?.NavigateTo("res://Scenes/GameMode.tscn");
	}

	public void GoToCustomization()
	{
		navigationManager?.NavigateTo("res://Scenes/CustomizationScene.tscn");
	}

	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}

	
}
