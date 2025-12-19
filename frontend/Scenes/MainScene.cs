using Godot;
using System;
using Snakes.Models;

public partial class MainScene : MarginContainer
{
	[Export] private Label _userDataLabel;
	private Button soundButton;
	private AudioManager audioManager;
	
	public override void _Ready()
	{	
		if (Player.GetInstance() == null) 
			_userDataLabel.Text = "Bypass mode";
		else
			_userDataLabel.Text = Player.GetInstance().ToString();
		
		// Obtener referencia al AudioManager
		audioManager = GetNode<AudioManager>("/root/AudioManager");
		
		// Obtener referencia al botón de sonido
		soundButton = GetNode<Button>("ExitControl/SoundButton");
		UpdateSoundButtonText();
	}
	
	public void ToggleSound()
	{
		if (audioManager != null)
		{
			audioManager.ToggleMute();
			UpdateSoundButtonText();
		}
	}
	
	private void UpdateSoundButtonText()
	{
		if (soundButton != null && audioManager != null)
		{
			soundButton.Text = audioManager.IsMuted() ? "🔇 Sonido" : "🔊 Sonido";
		}
	}
	public void Level_1_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/BackgroundSelector.tscn");
	}

	public void Level_2_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/WaterSavingMapSelector.tscn");
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
