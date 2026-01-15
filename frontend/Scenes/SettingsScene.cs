using Godot;
using System;

public partial class SettingsScene : MarginContainer
{
	private AudioManager audioManager;
	private HSlider musicVolumeSlider;
	private HSlider sfxVolumeSlider;
	private Label musicValueLabel;
	private Label sfxValueLabel;
	
	public override void _Ready()
	{
		// Obtener AudioManager
		audioManager = GetNode<AudioManager>("/root/AudioManager");
		
		// Obtener sliders
		musicVolumeSlider = GetNodeOrNull<HSlider>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/MusicSection/MusicVolumeSlider");
		sfxVolumeSlider = GetNodeOrNull<HSlider>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/SFXSection/SFXVolumeSlider");
		
		// Obtener labels de valor
		musicValueLabel = GetNodeOrNull<Label>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/MusicSection/MusicValueLabel");
		sfxValueLabel = GetNodeOrNull<Label>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/SFXSection/SFXValueLabel");
		
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
	
	public void MusicVolume_changed(float value)
	{
		if (audioManager != null)
		{
			audioManager.SetMusicVolume(value / 100.0f);
			UpdateMusicLabel(value);
			GD.Print($"SettingsScene: Volumen música cambiado a {value}%");
		}
	}
	
	public void SFXVolume_changed(float value)
	{
		if (audioManager != null)
		{
			audioManager.SetSFXVolume(value / 100.0f);
			UpdateSFXLabel(value);
			// Reproducir efecto de prueba
			audioManager.PlayTrashCollect();
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
	
	public void GoBack()
	{
		GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
	}
}
