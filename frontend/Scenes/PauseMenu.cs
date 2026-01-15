using Godot;
using System;


public partial class PauseMenu : MarginContainer
{
	private AudioManager audioManager;
	private CheckBox soundCheckBox;
	private HSlider musicVolumeSlider;
	private HSlider sfxVolumeSlider;
	private Label musicValueLabel;
	private Label sfxValueLabel;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        Visible = false;
		audioManager = GetNode<AudioManager>("/root/AudioManager");
		soundCheckBox = GetNodeOrNull<CheckBox>("PauseMenu/PanelContainer/VBoxContainer/SoundCheckBox");
		
		// Intentar obtener los sliders (pueden no existir aún en la escena)
		musicVolumeSlider = GetNodeOrNull<HSlider>("PauseMenu/PanelContainer/VBoxContainer/MusicVolumeContainer/MusicVolumeSlider");
		sfxVolumeSlider = GetNodeOrNull<HSlider>("PauseMenu/PanelContainer/VBoxContainer/SFXVolumeContainer/SFXVolumeSlider");
		
		// Obtener labels de valor
		musicValueLabel = GetNodeOrNull<Label>("PauseMenu/PanelContainer/VBoxContainer/MusicVolumeContainer/MusicValueLabel");
		sfxValueLabel = GetNodeOrNull<Label>("PauseMenu/PanelContainer/VBoxContainer/SFXVolumeContainer/SFXValueLabel");
		
		// Inicializar valores de sliders si existen
		if (audioManager != null)
		{
			if (musicVolumeSlider != null)
			{
				musicVolumeSlider.Value = audioManager.GetMusicVolume() * 100;
				UpdateMusicLabel((float)musicVolumeSlider.Value);
			}
			
			if (sfxVolumeSlider != null)
			{
				sfxVolumeSlider.Value = audioManager.GetSFXVolume() * 100;
				UpdateSFXLabel((float)sfxVolumeSlider.Value);
			}
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

	public void Resume()
    {
		Visible = false;
        GetTree().Paused = false;
    }

	public void Pause()
    {
		Visible = true;
        GetTree().Paused = true;
		GD.Print(GetTree());
		
		// Actualizar estado del CheckBox según el estado actual del sonido
		if (soundCheckBox != null && audioManager != null)
		{
			soundCheckBox.ButtonPressed = !audioManager.IsMuted();
		}
		
		// Actualizar sliders
		if (audioManager != null)
		{
			if (musicVolumeSlider != null)
			{
				musicVolumeSlider.Value = audioManager.GetMusicVolume() * 100;
				UpdateMusicLabel((float)musicVolumeSlider.Value);
			}
			
			if (sfxVolumeSlider != null)
			{
				sfxVolumeSlider.Value = audioManager.GetSFXVolume() * 100;
				UpdateSFXLabel((float)sfxVolumeSlider.Value);
			}
		}
    }


	public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GD.Print("ESC");

            if (GetTree().Paused == false)
            {
                Pause();   
            }

            else if (GetTree().Paused == true)
            {
                Resume();
            }
			
            
        }
    }

	public void Continue_button_pressed()
    {
        Resume();
    }

	public void Sound_toggled(bool buttonPressed)
	{
		if (audioManager != null)
		{
			audioManager.SetMute(!buttonPressed);
		}
	}
	
	public void MusicVolume_changed(float value)
	{
		if (audioManager != null)
		{
			audioManager.SetMusicVolume(value / 100.0f);
			UpdateMusicLabel(value);
		}
	}
	
	public void SFXVolume_changed(float value)
	{
		if (audioManager != null)
		{
			audioManager.SetSFXVolume(value / 100.0f);
			UpdateSFXLabel(value);
		}
	}
	
	private void UpdateMusicLabel(float value)
	{
		if (musicValueLabel != null)
		{
			musicValueLabel.Text = $"{(int)value}%";
		}
	}
	
	private void UpdateSFXLabel(float value)
	{
		if (sfxValueLabel != null)
		{
			sfxValueLabel.Text = $"{(int)value}%";
		}
	}

	public void Main_menu_button_pressed()
    {
		GetTree().Paused = false;
		
		// Detener la música del juego si está sonando
		var gameMusic = GetTree().Root.GetNode<AudioStreamPlayer>("Node2D/AudioStreamPlayer");
		if (gameMusic != null && gameMusic.Playing)
		{
			gameMusic.Stop();
		}
		
		// Reanudar la música del menú principal
		var musicManager = GetNode<Node>("/root/MusicManager");
		if (musicManager != null)
		{
			var audioPlayer = musicManager.GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			if (audioPlayer != null && !audioPlayer.Playing)
			{
				audioPlayer.Play();
			}
		}
		
        GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
    }

}
