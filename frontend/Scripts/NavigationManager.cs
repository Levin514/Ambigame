using Godot;
using System.Collections.Generic;

namespace Snake;

/// <summary>
/// Singleton global que gestiona el historial de navegación entre escenas.
/// Permite implementar un botón "Volver" que regresa a la escena anterior.
/// </summary>
public partial class NavigationManager : Node
{
	public static NavigationManager Instance { get; private set; }
	
	private Stack<string> navigationHistory = new Stack<string>();
	private string currentScene = "";

	public override void _Ready()
	{
		Instance = this;
	}

	/// <summary>
	/// Navega a una nueva escena y guarda la actual en el historial.
	/// </summary>
	/// <param name="scenePath">Ruta de la escena a la que navegar</param>
	public void NavigateTo(string scenePath)
	{
		if (!string.IsNullOrEmpty(currentScene))
		{
			navigationHistory.Push(currentScene);
			GD.Print($"NavigationManager: Guardando escena actual: {currentScene}");
		}
		
		currentScene = scenePath;
		GD.Print($"NavigationManager: Navegando a: {scenePath}");
		GetTree().ChangeSceneToFile(scenePath);
	}

	/// <summary>
	/// Vuelve a la escena anterior en el historial.
	/// Si no hay historial, va a la escena por defecto (LoginScene).
	/// </summary>
	public void GoBack()
	{
		if (navigationHistory.Count > 0)
		{
			string previousScene = navigationHistory.Pop();
			currentScene = previousScene;
			GD.Print($"NavigationManager: Volviendo a: {previousScene}");
			GetTree().ChangeSceneToFile(previousScene);
		}
		else
		{
			GD.Print("NavigationManager: No hay historial, volviendo a LoginScene");
			GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
		}
	}

	/// <summary>
	/// Limpia todo el historial de navegación.
	/// Útil al hacer logout o resetear la aplicación.
	/// </summary>
	public void ClearHistory()
	{
		navigationHistory.Clear();
		currentScene = "";
		GD.Print("NavigationManager: Historial limpiado");
	}

	/// <summary>
	/// Establece la escena actual sin navegar.
	/// Útil para inicializar el sistema al cargar el juego.
	/// </summary>
	public void SetCurrentScene(string scenePath)
	{
		currentScene = scenePath;
	}
}
