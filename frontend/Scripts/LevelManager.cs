using Godot;
using System;

/// <summary>
/// Singleton global que gestiona la información del nivel actual a cargar.
/// Se usa para pasar parámetros entre el selector de mapas y el GameLayout.
/// </summary>
public partial class LevelManager : Node
{
	// Información del nivel actual
	public string LevelPath { get; set; } = "";
	public string LevelType { get; set; } = "";
	public string LevelName { get; set; } = "";
	public int MapNumber { get; set; } = 1;
	public string[] SlideKeys { get; set; } = null; // Claves de traducción para slides
	public string SlideTitleKey { get; set; } = "TUTORIAL_TITLE"; // Título de los slides
	
	/// <summary>
	/// Configura el nivel a cargar y cambia a la escena del GameLayout
	/// </summary>
	public void LoadLevel(string levelPath, string levelType, string levelName, int mapNumber = 1)
	{
		LoadLevelWithSlides(levelPath, levelType, levelName, mapNumber, null);
	}
	
	/// <summary>
	/// Configura el nivel a cargar con slides de tutorial y cambia a la escena del GameLayout
	/// </summary>
	public void LoadLevelWithSlides(string levelPath, string levelType, string levelName, int mapNumber = 1, string[] slideKeys = null, string slideTitleKey = "TUTORIAL_TITLE")
	{
		LevelPath = levelPath;
		LevelType = levelType;
		LevelName = levelName;
		MapNumber = mapNumber;
		SlideKeys = slideKeys;
		SlideTitleKey = slideTitleKey;
		
		GD.Print($"LevelManager: Configurado nivel - Path: {levelPath}, Type: {levelType}, Name: {levelName}, Map: {mapNumber}, Slides: {slideKeys?.Length ?? 0}");
		
		// Cambiar a la escena del GameLayout
		GetTree().ChangeSceneToFile("res://Scenes/Layouts/GameLayout.tscn");
	}
	
	/// <summary>
	/// Limpia los datos del nivel
	/// </summary>
	public void ClearLevel()
	{
		LevelPath = "";
		LevelType = "";
		LevelName = "";
		MapNumber = 1;
		SlideKeys = null;
		SlideTitleKey = "TUTORIAL_TITLE";
	}
}
