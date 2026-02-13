using Godot;
using Snake;
using System;
using System.Collections.Generic;
public partial class TreesInfoUi : Control
{
	private LinkedList<Card> cardList;
	private LinkedListNode<Card> currentCardNode;
	[Export] private TextureRect imageDisplay;
	[Export] private Label nameLabel;
	[Export] private Label descriptionLabel;
	[Export] private HBoxContainer buttonContainer;
	[Export] private Button previousButton;
	[Export] private Button nextButton;
	[Export] private Label cardCounterLabel;
	[Signal] public delegate void InfoCompletedEventHandler();
	private Camera2D _camera;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeUI();
	}

	private void InitializeUI()
	{

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

	public void ShowCards()
	{
		PopulateCards();
		DisplayCard();
	}
	
	private void PopulateCards()
	{
		if(GameData.Instance.globalcardSeedsList != null)
		{
			cardList = GameData.Instance.globalcardSeedsList;
		}
		// Initialize pointer to first card
		currentCardNode = cardList.First;
	}

	private void DisplayCard()
	{
		if (currentCardNode == null)
			return;

		Card currentCard = currentCardNode.Value;

		nameLabel.Text = currentCard.seedType;
		descriptionLabel.Text = currentCard.description;
		
		string seedTypeFormat = currentCard.seedType.ToLower().Replace(" ", "");
		Texture2D treeImage = GD.Load<Texture2D>("res://Assets/" + seedTypeFormat + "_tree" + ".jpg");
		imageDisplay.Texture = treeImage;

		
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
			EmitSignal(SignalName.InfoCompleted);
		}
	}

	
}
