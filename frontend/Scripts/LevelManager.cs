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
	
	/// <summary>
	/// Configura el nivel a cargar y cambia a la escena del GameLayout
	/// </summary>
	public void LoadLevel(string levelPath, string levelType, string levelName, int mapNumber = 1)
	{
		LevelPath = levelPath;
		LevelType = levelType;
		LevelName = levelName;
		MapNumber = mapNumber;
		
		GD.Print($"LevelManager: Configurado nivel - Path: {levelPath}, Type: {levelType}, Name: {levelName}, Map: {mapNumber}");
		
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
	}
}
