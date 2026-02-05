using Godot;

/// <summary>
/// Sistema global para mostrar destellos de pantalla en cualquier nivel del juego.
/// Se puede llamar desde cualquier script: ScreenFlash.Show() o ScreenFlash.ShowColor()
/// </summary>
public partial class ScreenFlash : CanvasLayer
{
	private static ScreenFlash _instance;
	private ColorRect _flashRect;
	private Tween _currentTween; // Guardar referencia al tween activo

	public override void _Ready()
	{
		_instance = this;
		Layer = 100; // Sobre todo lo demás
		
		// CRÍTICO: Procesar siempre, incluso cuando el juego está en pausa
		ProcessMode = ProcessModeEnum.Always;
		
		_flashRect = new ColorRect
		{
			Color = new Color(1, 0.3f, 0.2f, 0.4f),
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Visible = false
		};
		
		_flashRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(_flashRect);
	}

	/// <summary>
	/// Muestra un destello rojo en pantalla
	/// </summary>
	public static void Show(float duration = 0.3f)
	{
		if (_instance?._flashRect == null) return;
		
		// Matar tween anterior si existe
		if (_instance._currentTween != null && _instance._currentTween.IsValid())
		{
			_instance._currentTween.Kill();
		}
		
		var rect = _instance._flashRect;
		rect.Color = new Color(1, 0.3f, 0.2f, 0.4f);
		rect.Modulate = new Color(1, 1, 1, 1);
		rect.Visible = true;
		
		_instance._currentTween = rect.CreateTween();
		_instance._currentTween.SetProcessMode(Tween.TweenProcessMode.Idle);
		_instance._currentTween.TweenProperty(rect, "modulate:a", 0.0f, duration);
		_instance._currentTween.TweenCallback(Callable.From(() => rect.Visible = false));
	}

	/// <summary>
	/// Muestra un destello de color personalizado
	/// </summary>
	public static void ShowColor(Color color, float duration = 0.3f, float alpha = 0.5f)
	{
		if (_instance?._flashRect == null) return;
		
		// Matar tween anterior si existe
		if (_instance._currentTween != null && _instance._currentTween.IsValid())
		{
			_instance._currentTween.Kill();
		}
		
		var rect = _instance._flashRect;
		rect.Color = new Color(color.R, color.G, color.B, alpha);
		rect.Modulate = new Color(1, 1, 1, 1);
		rect.Visible = true;
		
		_instance._currentTween = rect.CreateTween();
		_instance._currentTween.SetProcessMode(Tween.TweenProcessMode.Idle);
		_instance._currentTween.TweenProperty(rect, "modulate:a", 0.0f, duration);
		_instance._currentTween.TweenCallback(Callable.From(() => rect.Visible = false));
	}

	/// <summary>
	/// Oculta el destello inmediatamente (usado cuando aparece Game Over)
	/// </summary>
	public static void HideFlash()
	{
		if (_instance?._flashRect == null) return;
		
		// Matar el tween activo si existe
		if (_instance._currentTween != null && _instance._currentTween.IsValid())
		{
			_instance._currentTween.Kill();
			_instance._currentTween = null;
		}
		
		// Ocultar inmediatamente
		var rect = _instance._flashRect;
		rect.Visible = false;
		rect.Modulate = new Color(1, 1, 1, 0);
	}
}
