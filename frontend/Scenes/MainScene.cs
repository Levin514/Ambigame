using Godot;
using System;
using Snakes.Models;
using Snake;

public partial class MainScene : MarginContainer
{
	[Export] private Label _userDataLabel;
	private NavigationManager navigationManager;
	
	// Referencias a botones de niveles
	private Button level1Button;
	private Button level2Button;
	private Button level3Button;
	private TextureButton level1TextureButton;
	private TextureButton level2TextureButton;
	private TextureButton level3TextureButton;
	
	public override void _Ready()
	{	
		navigationManager = GetNode<NavigationManager>("/root/NavigationManager");
		navigationManager?.SetCurrentScene("res://Scenes/MainScene.tscn");
		
		if (Player.GetInstance() == null) 
			_userDataLabel.Text = Tr("UI_USER_PLACEHOLDER");
		else
			_userDataLabel.Text = Player.GetInstance().ToString();

		// Obtener referencias a botones
		level1Button = GetNodeOrNull<Button>("MarginContainer2/VBoxContainer/MainButtons/Level1Container/Level1Button");
		level2Button = GetNodeOrNull<Button>("MarginContainer2/VBoxContainer/MainButtons/Level2Container/Level2Button");
		level3Button = GetNodeOrNull<Button>("MarginContainer2/VBoxContainer/MainButtons/Level3Container/Level3Button");
		level1TextureButton = GetNodeOrNull<TextureButton>("MarginContainer2/VBoxContainer/MainButtons/Level1Container/Level1TextureButton");
		level2TextureButton = GetNodeOrNull<TextureButton>("MarginContainer2/VBoxContainer/MainButtons/Level2Container/Level2TextureButton");
		level3TextureButton = GetNodeOrNull<TextureButton>("MarginContainer2/VBoxContainer/MainButtons/Level3Container/Level3TextureButton");

		// Configurar estado de botones según progreso
		UpdateLevelButtons();
	}

	private void UpdateLevelButtons()
	{
		// Nivel 1 (Reciclaje) siempre desbloqueado
		SetButtonEnabled(level1Button, level1TextureButton, true);

		// Nivel 2 (Agua) requiere haber completado Clasificación
		bool waterUnlocked = GameData.Instance.classifyLevelCompleted;
		SetButtonEnabled(level2Button, level2TextureButton, waterUnlocked);

		// Nivel 3 (Reforestación) requiere haber completado minijuego de agua
		bool reforestationUnlocked = GameData.Instance.waterCatchLevelCompleted;
		SetButtonEnabled(level3Button, level3TextureButton, reforestationUnlocked);
	}

	private void SetButtonEnabled(Button button, TextureButton textureButton, bool enabled)
	{
		if (button != null)
		{			button.Disabled = !enabled;
			// Aplicar efecto visual de sombra/disabled
			if (!enabled)
			{
				button.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gris oscurecido
			}
			else
			{
				button.Modulate = new Color(1f, 1f, 1f, 1f); // Color normal
			}
		}

		if (textureButton != null)
		{
			textureButton.Disabled = !enabled;
			// Aplicar efecto visual de sombra/disabled
			if (!enabled)
			{
				textureButton.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gris oscurecido
			}
			else
			{
				textureButton.Modulate = new Color(1f, 1f, 1f, 1f); // Color normal
			}
		}
	}
	public void Level_1_button_pressed()
	{
		navigationManager?.NavigateTo("res://Scenes/BackgroundSelector.tscn");
	}

	public void Level_2_button_pressed()
	{
		// Verificar que el nivel esté desbloqueado
		if (!GameData.Instance.classifyLevelCompleted)
		{
			var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
			audioManager?.PlayBreakPipe();
			GD.Print("Nivel de Agua bloqueado: Completa el minijuego de Clasificación primero");
			return;
		}

		navigationManager?.NavigateTo("res://Scenes/WaterSavingMapSelector.tscn");
	}

	public void Level_3_button_pressed()
	{
		// Verificar que el nivel esté desbloqueado
		if (!GameData.Instance.waterCatchLevelCompleted)
		{
			var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
			audioManager?.PlayBreakPipe();
			GD.Print("Nivel de Reforestación bloqueado: Completa el minijuego de Agua primero");
			return;
		}

		// Cargar nivel de reforestación directamente
		var levelManager = GetNode<LevelManager>("/root/LevelManager");
		string[] slides = new string[] 
		{ 
			"SLIDE_REFORESTATION_1", 
			"SLIDE_REFORESTATION_2", 
			"SLIDE_REFORESTATION_3",
			"SLIDE_REFORESTATION_4" 
		};
		levelManager.LoadLevelWithSlides(
			"res://Scenes/Levels/Reforestation/ReforestationLevel_Map1.tscn", 
			"reforestation", 
			"Reforestación", 
			1,
			slides,
			"TUTORIAL_TITLE"
		);
	}

	public void GoToTutorial()
	{
		navigationManager?.NavigateTo("res://Scenes/TutorialScene.tscn");
	}
	
	public void GoToSettings()
	{
		navigationManager?.NavigateTo("res://Scenes/SettingsScene.tscn");
	}

	public void LogOut()
	{
		Player.SetInstance(null);
		navigationManager?.ClearHistory();
		GetTree().ChangeSceneToFile("res://Scenes/GameMode.tscn");
	}
}
