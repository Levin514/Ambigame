using Godot;
using System;

public partial class CustomizationScene : MarginContainer
{
	[Export] private VBoxContainer _hatsContainer;
	[Export] private VBoxContainer _outfitsContainer;
	[Export] private Sprite2D _playerPreview;
	[Export] private Button _hatsTabButton;
	[Export] private Button _outfitsTabButton;
	[Export] private Control _hatsPanel;
	[Export] private Control _outfitsPanel;

	private int _selectedHat = 0;
	private int _selectedOutfit = 0;

	public override void _Ready()
	{
		// Mostrar la pestaña de sombreros por defecto
		ShowHatsPanel();
		
		// Cargar preview del jugador - usando el icono del proyecto como placeholder
		LoadPlayerPreview();
	}

	private void LoadPlayerPreview()
	{
		// El sprite ya está configurado para mostrar el frame 0 (de frente)
		// Puedes cambiar el frame si necesitas otra pose
	}

	public void ShowHatsPanel()
	{
		_hatsPanel.Visible = true;
		_outfitsPanel.Visible = false;
		_hatsTabButton.Disabled = true;
		_outfitsTabButton.Disabled = false;
	}

	public void ShowOutfitsPanel()
	{
		_hatsPanel.Visible = false;
		_outfitsPanel.Visible = true;
		_hatsTabButton.Disabled = false;
		_outfitsTabButton.Disabled = true;
	}

	public void SelectHat(int hatId)
	{
		_selectedHat = hatId;
		// Actualizar preview del jugador con el sombrero seleccionado
		GD.Print($"Sombrero seleccionado: {hatId}");
	}

	public void SelectOutfit(int outfitId)
	{
		_selectedOutfit = outfitId;
		// Actualizar preview del jugador con el traje seleccionado
		GD.Print($"Traje seleccionado: {outfitId}");
	}

	public void GoBack()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}

	public void SaveCustomization()
	{
		// Guardar la personalización seleccionada
		GD.Print($"Guardando personalización - Sombrero: {_selectedHat}, Traje: {_selectedOutfit}");
		// Aquí puedes agregar la lógica para guardar en el modelo Player
		GoBack();
	}
}
