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
		previousButton.Text = TranslationManager.Tr("BTN_PREVIOUS");
		previousButton.Pressed += OnPreviousButtonPressed;

		// Next button
		nextButton.Text = TranslationManager.Tr("BTN_NEXT");
		nextButton.Pressed += OnNextButtonPressed;
	}

	private void PopulateCards()
	{
		Texture2D sunflowerImage = GD.Load<Texture2D>("res://Assets/sunflower_seeds.jpg");
		Texture2D ceiboImage = GD.Load<Texture2D>("res://Assets/ceibo_seeds.jpg");
		Texture2D guayacanImage = GD.Load<Texture2D>("res://Assets/guayacan_seeds.jpg");
		Texture2D algarroboImage = GD.Load<Texture2D>("res://Assets/algarrobo_seeds.jpg");
		Texture2D paloSantoImage = GD.Load<Texture2D>("res://Assets/palosanto_seeds.jpg");
		Texture2D cascolImage = GD.Load<Texture2D>("res://Assets/cascol_seeds.jpg");

		// Add sample cards - modify with actual data as needed
		cardList.AddLast(new Card(TranslationManager.Tr("SUNFLOWER_SEED"), TranslationManager.Tr("SUNFLOWER_NAME"), sunflowerImage, TranslationManager.Tr("SUNFLOWER_DESC")));
		cardList.AddLast(new Card(TranslationManager.Tr("CEIBO_SEED"), TranslationManager.Tr("CEIBO_NAME"), ceiboImage, TranslationManager.Tr("CEIBO_DESC")));
		cardList.AddLast(new Card(TranslationManager.Tr("GUAYACAN_SEED"), TranslationManager.Tr("GUAYACAN_NAME"), guayacanImage, TranslationManager.Tr("GUAYACAN_DESC")));
		cardList.AddLast(new Card(TranslationManager.Tr("ALGARROBO_SEED"), TranslationManager.Tr("ALGARROBO_NAME"), algarroboImage, TranslationManager.Tr("ALGARROBO_DESC")));
		cardList.AddLast(new Card(TranslationManager.Tr("PALOSANTO_SEED"), TranslationManager.Tr("PALOSANTO_NAME"), paloSantoImage, TranslationManager.Tr("PALOSANTO_DESC")));
		cardList.AddLast(new Card(TranslationManager.Tr("CASCOL_SEED"), TranslationManager.Tr("CASCOL_NAME"), cascolImage, TranslationManager.Tr("CASCOL_DESC")));

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
		cardCounterLabel.Text = $"{TranslationManager.Tr("UI_CARD_COUNTER")} {cardIndex + 1} / {cardList.Count}";
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
