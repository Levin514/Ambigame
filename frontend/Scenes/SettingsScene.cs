using Godot;
using System;
using Snake;

public partial class SettingsScene : MarginContainer
{
	private AudioManager audioManager;
	private TranslationManager translationManager;
	private NavigationManager navigationManager;
	private HSlider musicVolumeSlider;
	private HSlider sfxVolumeSlider;
	private Label musicValueLabel;
	private Label sfxValueLabel;
	private OptionButton languageOptionButton;
	
	public override void _Ready()
	{
		// Obtener managers
		audioManager = GetNode<AudioManager>("/root/AudioManager");
		translationManager = GetNode<TranslationManager>("/root/TranslationManager");
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/SettingsScene.tscn");
		
		// Obtener sliders
		musicVolumeSlider = GetNodeOrNull<HSlider>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/MusicSection/MusicVolumeSlider");
		sfxVolumeSlider = GetNodeOrNull<HSlider>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/SFXSection/SFXVolumeSlider");
		
		// Obtener labels de valor
		musicValueLabel = GetNodeOrNull<Label>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/MusicSection/MusicHeader/MusicValueLabel");
		sfxValueLabel = GetNodeOrNull<Label>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/SFXSection/SFXHeader/SFXValueLabel");
		
		// Obtener selector de idioma
		languageOptionButton = GetNodeOrNull<OptionButton>("VBoxContainer/ContentPanel/MarginContainer/VBoxContainer/LanguageSection/LanguageOptionButton");
		
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
		
		// Inicializar selector de idioma
		if (languageOptionButton != null && translationManager != null)
		{
			string currentLocale = translationManager.GetCurrentLocale();
			languageOptionButton.Selected = currentLocale == "en" ? 1 : 0;
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
	
	public void Language_selected(int index)
	{
		if (translationManager != null)
		{
			string locale = index == 0 ? "es" : "en";
			translationManager.SetLocale(locale);
			GD.Print($"Language changed to: {locale}");
		}
	}
	
	public void GoBack()
	{
		navigationManager?.GoBack();
	}
}
