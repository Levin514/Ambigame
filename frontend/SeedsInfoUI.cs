using Godot;
using System;
using System.Collections.Generic;

public partial class SeedsInfoUI : Control
{
	private LinkedList<Card> cardList = new LinkedList<Card>();
	private LinkedListNode<Card> currentCardNode;

	private VBoxContainer cardContainer;
	private TextureRect imageDisplay;
	private Label nameLabel;
	private Label descriptionLabel;
	private Button previousButton;
	private Button nextButton;
	private Label cardCounterLabel;

	public override void _Ready()
	{
		InitializeUI();
		PopulateCards();
		DisplayCard();
	}

	private void InitializeUI()
	{
		// Get the VBoxContainer from the scene tree
		cardContainer = GetNode<VBoxContainer>("PanelContainer/CardVBox");
		cardContainer.AddThemeConstantOverride("separation", 16);

		// Name label
		nameLabel = new Label();
		nameLabel.AddThemeFontSizeOverride("font_size", 24);
		nameLabel.AddThemeColorOverride("font_color", Colors.Black);
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		nameLabel.CustomMinimumSize = new Vector2(300, 50);
		cardContainer.AddChild(nameLabel);

		// Image display
		imageDisplay = new TextureRect();
		imageDisplay.CustomMinimumSize = new Vector2(300, 200);
		imageDisplay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		imageDisplay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		cardContainer.AddChild(imageDisplay);

		// Description label
		descriptionLabel = new Label();
		descriptionLabel.CustomMinimumSize = new Vector2(300, 100);
		descriptionLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		descriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		descriptionLabel.AddThemeColorOverride("font_color", Colors.Black);
		descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
		cardContainer.AddChild(descriptionLabel);

		// Card counter label
		cardCounterLabel = new Label();
		cardCounterLabel.HorizontalAlignment = HorizontalAlignment.Center;
		cardCounterLabel.AddThemeColorOverride("font_color", Colors.Black);
		cardContainer.AddChild(cardCounterLabel);

		// Navigation buttons container
		HBoxContainer buttonContainer = new HBoxContainer();
		buttonContainer.AddThemeConstantOverride("separation", 16);
		buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		cardContainer.AddChild(buttonContainer);

		// Previous button
		previousButton = new Button();
		previousButton.Text = "← Anterior";
		previousButton.Pressed += OnPreviousButtonPressed;
		buttonContainer.AddChild(previousButton);

		// Next button
		nextButton = new Button();
		nextButton.Text = "Siguiente →";
		nextButton.Pressed += OnNextButtonPressed;
		buttonContainer.AddChild(nextButton);
	}

	private void PopulateCards()
	{
		Texture2D sunflowerImage = GD.Load<Texture2D>("res://Assets/sunflower_seeds.jpg");
		// Add sample cards - modify with actual data as needed
		cardList.AddLast(new Card("Semilla de Girasol", sunflowerImage, "Una semilla de girasol es una semilla oleaginosa que produce un aceite de alta calidad."));
		cardList.AddLast(new Card("Semilla de Trigo", null, "El trigo es un cereal fundamental en la alimentación humana con alto contenido de carbohidratos."));
		cardList.AddLast(new Card("Semilla de Maíz", null, "El maíz es uno de los cultivos más importantes del mundo, utilizado para alimento y combustible."));
		cardList.AddLast(new Card("Semilla de Frijol", null, "Los frijoles son ricos en proteína y fibra, esenciales para una dieta equilibrada."));

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

		// Update button states
		previousButton.Disabled = (currentCardNode.Previous == null);
		nextButton.Disabled = (currentCardNode.Next == null);
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
		if (currentCardNode?.Previous != null)
		{
			currentCardNode = currentCardNode.Previous;
			DisplayCard();
		}
	}

	private void OnNextButtonPressed()
	{
		if (currentCardNode?.Next != null)
		{
			currentCardNode = currentCardNode.Next;
			DisplayCard();
		}
	}
}
