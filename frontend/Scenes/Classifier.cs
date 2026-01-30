using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake;
public partial class Classifier : Node2D
{
	[Export] Label puntuacionLabel;
	[Export] Label recicladosLabel;
	[Export] Label timerLabel;

	[Export] Sprite2D recycleSprite;
	[Export] Sprite2D sprite;
	private ClassifyLevel classifyLevel;

	private Dictionary<String, List<String>> recycleObjects;

	private Random rnd = new();
	private List<String> keys;

	private LinkedList<Trash> body;
	private bool hasTrashList;
	private bool hasTrashElements;

	private String actualCategory;

	private bool isCorrect;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Obtener ClassifyLevel del nodo padre
		classifyLevel = GetParent<ClassifyLevel>();

		recycleObjects = GameData.Instance.recycleObjects[GameData.Instance.recycleBackground];

		if(GameData.Instance.globalTrashList != null)
		{
			body = GameData.Instance.globalTrashList;
		}
		
		hasTrashList = body != null;
		if(hasTrashList)
		{
			hasTrashElements = body.Count != 0;
		}

		UpdateItemSprite();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public bool GetHasTrashElements()
	{
		return hasTrashElements;
	}

	public bool GetCorrectClassification()
	{
		return isCorrect;
	}

	public String GetRandomCategory()
	{
		String category;
		if(!hasTrashList)
		{
			keys = GameData.Instance.recycleObjects.Keys.ToList();
			category = keys[rnd.Next() % keys.Count()];
		}
		else if(hasTrashElements)
		{
			Trash trash = body.First.Value;
			body.RemoveFirst();
			category = trash.category;
			hasTrashElements = body.Count != 0;
		}
		else
		{
			category = null;	
		}

		GD.Print(category);
		GD.Print(hasTrashList);
		return category;
	}

	public void GenerateItemSprite(String category)
	{
		String item = recycleObjects[category][rnd.Next(0, recycleObjects[category].Count)];
		recycleSprite.Texture = GD.Load<Texture2D>("res://Assets/" + item + ".png");
		recycleSprite.Scale = new Vector2I(3,3);
	}

	public void GenerateSprite(String name)
	{
		sprite.Texture = GD.Load<Texture2D>("res://Assets/" + name + ".png");
		sprite.Scale = new Vector2(0.25f,0.25f);
	}

	public void UpdateItemSprite()
	{
		String newCategory = GetRandomCategory();
		if(newCategory != null)
		{
			GenerateItemSprite(newCategory);
			actualCategory = newCategory;
			GD.Print("Sprite Updated with: " + newCategory);
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Bloquear input si el jugador está en movimiento o esperando
		if (classifyLevel != null && (classifyLevel.GetIsMoving() || classifyLevel.GetIsWaiting()))
			return;

		/* Vector2I element = body.First.Value;
		body.RemoveFirst(); */
		if (@event.IsActionPressed("ui_left"))
		{
			isCorrect = actualCategory == "paper";
			if(isCorrect)
			{
				GD.Print("Acierto");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_right") )
		{
			isCorrect = actualCategory == "glass";
			if(isCorrect)
			{
				GD.Print("Acierto");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_up"))
		{
			isCorrect = actualCategory == "plastic";
			if(isCorrect)
			{
				GD.Print("Acierto");
			}
			
			UpdateItemSprite();
		}
	}


}
