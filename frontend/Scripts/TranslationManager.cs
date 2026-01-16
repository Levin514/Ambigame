using Godot;
using System;

namespace Snake;

/// <summary>
/// Gestor global de traducciones para el juego.
/// Maneja el cambio de idioma y proporciona métodos auxiliares para las traducciones.
/// </summary>
public partial class TranslationManager : Node
{
	public static TranslationManager Instance { get; private set; }
	
	private const string LOCALE_SETTING_KEY = "user/locale";
	private string currentLocale = "es"; // Idioma por defecto: español

	public override void _Ready()
	{
		Instance = this;
		LoadSavedLocale();
	}

	/// <summary>
	/// Carga el idioma guardado en la configuración del usuario.
	/// Si no hay idioma guardado, detecta el idioma del sistema.
	/// </summary>
	private void LoadSavedLocale()
	{
		// Intentar cargar el idioma guardado
		if (FileAccess.FileExists("user://settings.cfg"))
		{
			var config = new ConfigFile();
			var error = config.Load("user://settings.cfg");
			
			if (error == Error.Ok && config.HasSectionKey("settings", LOCALE_SETTING_KEY))
			{
				currentLocale = config.GetValue("settings", LOCALE_SETTING_KEY).AsString();
				TranslationServer.SetLocale(currentLocale);
				GD.Print($"Idioma cargado: {currentLocale}");
				return;
			}
		}
		
		// Si no hay idioma guardado, detectar el idioma del sistema
		DetectSystemLocale();
	}

	/// <summary>
	/// Detecta y aplica el idioma del sistema operativo.
	/// </summary>
	private void DetectSystemLocale()
	{
		string systemLocale = OS.GetLocale();
		GD.Print($"Idioma del sistema detectado: {systemLocale}");
		
		// Extraer solo el código de idioma (ej: "es_MX" -> "es")
		string languageCode = systemLocale.Split('_')[0].ToLower();
		
		// Verificar si el idioma está soportado
		if (languageCode == "es" || languageCode == "en")
		{
			currentLocale = languageCode;
		}
		else
		{
			// Por defecto, usar español
			currentLocale = "es";
		}
		
		TranslationServer.SetLocale(currentLocale);
		GD.Print($"Idioma configurado: {currentLocale}");
	}

	/// <summary>
	/// Cambia el idioma del juego.
	/// </summary>
	/// <param name="locale">Código del idioma ("es" o "en")</param>
	public void SetLocale(string locale)
	{
		if (locale != "es" && locale != "en")
		{
			GD.PrintErr($"Idioma no soportado: {locale}");
			return;
		}

		currentLocale = locale;
		TranslationServer.SetLocale(locale);
		SaveLocale();
		
		GD.Print($"Idioma cambiado a: {locale}");
		
		// Emitir señal para notificar el cambio (si se necesita actualizar UI)
		EmitSignal(SignalName.LocaleChanged, locale);
	}

	/// <summary>
	/// Guarda el idioma actual en la configuración del usuario.
	/// </summary>
	private void SaveLocale()
	{
		var config = new ConfigFile();
		
		// Cargar configuración existente si existe
		if (FileAccess.FileExists("user://settings.cfg"))
		{
			config.Load("user://settings.cfg");
		}
		
		config.SetValue("settings", LOCALE_SETTING_KEY, currentLocale);
		config.Save("user://settings.cfg");
	}

	/// <summary>
	/// Obtiene el idioma actual.
	/// </summary>
	/// <returns>Código del idioma actual ("es" o "en")</returns>
	public string GetCurrentLocale()
	{
		return currentLocale;
	}

	/// <summary>
	/// Obtiene la traducción de una clave.
	/// Método auxiliar que envuelve TranslationServer.Translate()
	/// </summary>
	/// <param name="key">Clave de traducción</param>
	/// <returns>Texto traducido</returns>
	public static string Tr(string key)
	{
		return TranslationServer.Translate(key);
	}

	/// <summary>
	/// Obtiene la traducción con formato (interpolación).
	/// </summary>
	/// <param name="key">Clave de traducción</param>
	/// <param name="args">Argumentos para formatear</param>
	/// <returns>Texto traducido y formateado</returns>
	public static string TrFormat(string key, params object[] args)
	{
		string translation = TranslationServer.Translate(key);
		return string.Format(translation, args);
	}

	/// <summary>
	/// Señal emitida cuando cambia el idioma.
	/// </summary>
	[Signal]
	public delegate void LocaleChangedEventHandler(string newLocale);
}
