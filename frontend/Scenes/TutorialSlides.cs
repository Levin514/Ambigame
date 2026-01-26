using Godot;
using System;
using Snake;

/// <summary>
/// Sistema de slides para tutoriales antes de iniciar un nivel.
/// Muestra una serie de textos explicativos que el jugador puede navegar.
/// </summary>
public partial class TutorialSlides : Control
{
	[Export] private Label titleLabel;
	[Export] private Label contentLabel;
	[Export] private Label counterLabel;
	[Export] private Button continueButton;
	
	private string[] slideKeys = Array.Empty<string>(); // Claves de traducción de los slides
	private int currentSlideIndex = 0;
	
	/// <summary>
	/// Señal emitida cuando se completan todos los slides
	/// </summary>
	[Signal] public delegate void SlidesCompletedEventHandler();
	
	public override void _Ready()
	{
		if (continueButton != null)
		{
			continueButton.Pressed += OnContinuePressed;
		}
	}
	
	/// <summary>
	/// Configura los slides con las claves de traducción
	/// </summary>
	/// <param name="keys">Array de claves de traducción para cada slide</param>
	/// <param name="titleKey">Clave de traducción para el título (opcional)</param>
	public void SetupSlides(string[] keys, string titleKey = "TUTORIAL_TITLE")
	{
		slideKeys = keys;
		currentSlideIndex = 0;
		
		if (titleLabel != null)
		{
			titleLabel.Text = TranslationManager.Tr(titleKey);
		}
		
		ShowCurrentSlide();
	}
	
	/// <summary>
	/// Muestra el slide actual
	/// </summary>
	private void ShowCurrentSlide()
	{
		if (slideKeys.Length == 0)
		{
			GD.PrintErr("TutorialSlides: No hay slides configurados");
			return;
		}
		
		// Actualizar contenido del slide
		if (contentLabel != null)
		{
			contentLabel.Text = TranslationManager.Tr(slideKeys[currentSlideIndex]);
		}
		
		// Actualizar contador (1/3, 2/3, etc.)
		if (counterLabel != null)
		{
			counterLabel.Text = $"{currentSlideIndex + 1}/{slideKeys.Length}";
		}
		
		// Actualizar texto del botón
		if (continueButton != null)
		{
			bool isLastSlide = currentSlideIndex >= slideKeys.Length - 1;
			continueButton.Text = isLastSlide 
				? TranslationManager.Tr("BTN_START") 
				: TranslationManager.Tr("BTN_CONTINUE");
		}
	}
	
	/// <summary>
	/// Maneja el clic en el botón continuar
	/// </summary>
	private void OnContinuePressed()
	{
		currentSlideIndex++;
		
		if (currentSlideIndex >= slideKeys.Length)
		{
			// Se completaron todos los slides
			EmitSignal(SignalName.SlidesCompleted);
		}
		else
		{
			// Mostrar siguiente slide
			ShowCurrentSlide();
		}
	}
}
