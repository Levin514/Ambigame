using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Classifier : Node2D
{
	[Export] Label puntuacionLabel;
	[Export] Label recicladosLabel;
	[Export] Label timerLabel;

	[Export] Sprite2D recycleSprite;

	private readonly Dictionary<String, String> recycleObjects = new Dictionary<string, string>()
	{
		{"paper", "newspaper"},
		{"plastic", "plasticBottle"},
		{"glass", "glassJar"}
	};

	private Random rnd = new();
	private List<String> keys;

	private readonly LinkedList<Vector2I> body;

	private String actualCategory;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		String actualCategory = GetRandomCategory();
		GenerateItemSprite(actualCategory);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public String GetRandomCategory()
	{
		keys = recycleObjects.Keys.ToList();
		String category = keys[rnd.Next() % keys.Count()];
		return category;
	}

	public void GenerateItemSprite(String category)
	{
		String item = recycleObjects[category];
		recycleSprite.Texture = GD.Load<Texture2D>("res://Assets/" + item + ".png");
		recycleSprite.Scale = new Vector2I(3,3);
	}

	public void UpdateItemSprite()
	{
		String newCategory = GetRandomCategory();
		GenerateItemSprite(newCategory);
		actualCategory = newCategory;
		GD.Print("Sprite Updated with: " + newCategory);
	}

	public override void _Input(InputEvent @event)
	{
		/* Vector2I element = body.First.Value;
		body.RemoveFirst(); */

		if (@event.IsActionPressed("ui_left"))
		{
			if(actualCategory == "paper")
			{
				GD.Print("Acierto");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_right") )
		{
			if(actualCategory == "glass")
			{
				GD.Print("Acierto");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_up"))
		{
			if(actualCategory == "plastic")
			{
				GD.Print("Acierto");
			}
			UpdateItemSprite();
		}
	}


}
