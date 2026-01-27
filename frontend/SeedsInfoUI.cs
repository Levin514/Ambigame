using Godot;
using Snake;
using System;
using System.Collections.Generic;

public partial class SeedsInfoUI : Control
{
	private LinkedList<Card> cardList = new LinkedList<Card>();
	private LinkedListNode<Card> currentCardNode;

	[Export] private VBoxContainer cardContainer;
	[Export] private TextureRect imageDisplay;
	[Export] private Label nameLabel;
	[Export] private Label descriptionLabel;
	[Export] private HBoxContainer buttonContainer;
	[Export] private Button previousButton;
	[Export] private Button nextButton;
	[Export] private Label cardCounterLabel;
	private Camera2D _camera;
	[Signal] public delegate void SlidesCompletedEventHandler();

	public override void _Ready()
	{
		//Temporal solution
		_camera = GetNode<Camera2D>("/root/GameLayout/VBoxContainer/GameContainer/SubViewportContainer/SubViewport/Camera2D");
		_camera.Zoom = new Vector2I(1,1);

		InitializeUI();
		PopulateCards();
		DisplayCard();
	}

	private void InitializeUI()
	{
		cardContainer.AddThemeConstantOverride("separation", 16);

		// Name label
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		nameLabel.CustomMinimumSize = new Vector2(300, 50);

		// Image display
		imageDisplay.CustomMinimumSize = new Vector2(300, 200);
		imageDisplay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		imageDisplay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

		// Description label
		descriptionLabel.CustomMinimumSize = new Vector2(300, 100);
		descriptionLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		descriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;

		// Card counter label
		cardCounterLabel.HorizontalAlignment = HorizontalAlignment.Center;

		// Navigation buttons container
		buttonContainer.AddThemeConstantOverride("separation", 16);
		buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		// Previous button
		previousButton.Text = "← Anterior";
		previousButton.Pressed += OnPreviousButtonPressed;

		// Next button
		nextButton.Text = "Siguiente →";
		nextButton.Pressed += OnNextButtonPressed;
	}

	private void PopulateCards()
	{
		Texture2D sunflowerImage = GD.Load<Texture2D>("res://Assets/sunflower_seeds.jpg");
		// Add sample cards - modify with actual data as needed
		cardList.AddLast(new Card("Semilla de Girasol", "Girasol", sunflowerImage, "Una semilla de girasol es una semilla oleaginosa que produce un aceite de alta calidad."));
		cardList.AddLast(new Card("Semilla de Ceibo", "Ceibo", null, "El ceibo es un árbol característico de la costa ecuatoriana, puede alcanzar gran altura y es símbolo de identidad cultural."));
		cardList.AddLast(new Card("Semilla de Guayacán", "Guayacán", null, "El guayacán es un árbol típico de los bosques secos de la costa, reconocido por su intensa floración amarilla."));
		cardList.AddLast(new Card("Semilla de Algarrobo", "Algarrobo", null, "El algarrobo es un árbol resistente a la sequía, común en la costa, cuyas vainas sirven de alimento para animales."));
		cardList.AddLast(new Card("Semilla de Palo Santo", "Palo Santo", null, "El palo santo es un árbol nativo de la costa ecuatoriana, apreciado por su aroma y usos medicinales y espirituales."));
		cardList.AddLast(new Card("Semilla de Cascol", "Cascol", null, "El cascol es un árbol propio de la costa, utilizado tradicionalmente como cerca viva y para la recuperación de suelos."));

		// Initialize pointer to first card
		currentCardNode = cardList.First;
	}

	private void DisplayCard()
	{
		if (currentCardNode == null)
			return;

		Card currentCard = currentCardNode.Value;

		nameLabel.Text = currentCard.cardName;
		descriptionLabel.Text = currentCard.description;
		
		if (currentCard.image != null)
		{
			imageDisplay.Texture = currentCard.image;
		}
		
		// Update counter
		int cardIndex = GetCardIndex();
		cardCounterLabel.Text = $"Tarjeta {cardIndex + 1} de {cardList.Count}";
	}

	private int GetCardIndex()
	{
		int index = 0;
		foreach (var card in cardList)
		{
			if (card == currentCardNode.Value)
				return index;
			index++;
		}
		return 0;
	}

	private void OnPreviousButtonPressed()
	{
		if (currentCardNode.Previous != null)
		{
			currentCardNode = currentCardNode.Previous;
			DisplayCard();
		}
	}

	private void OnNextButtonPressed()
	{
		if (currentCardNode.Next != null)
		{
			currentCardNode = currentCardNode.Next;
			DisplayCard();
		}
		else
		{
			_camera.Zoom = new Vector2I(2,2);
			GameData.Instance.globalcardSeedsList = cardList;
			EmitSignal(SignalName.SlidesCompleted);
		}
	}
}
