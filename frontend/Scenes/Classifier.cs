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

	private Dictionary<String, String> recycleObjects;

	private Random rnd = new();
	private List<String> keys;

	private LinkedList<Trash> body;
	private bool hasTrashList;
	private bool hasTrashElements;

	private String actualCategory;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		recycleObjects = GameData.Instance.recycleObjects;

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

	public String GetRandomCategory()
	{
		String category;
		if(hasTrashList && hasTrashElements)
		{
			Trash trash = body.First.Value;
			body.RemoveFirst();
			category = trash.category;
			hasTrashList = body.Count != 0;
		}
		else
		{
			keys = GameData.Instance.recycleObjects.Keys.ToList();
			category = keys[rnd.Next() % keys.Count()];
		}
		GD.Print(category);
		GD.Print(hasTrashList);
		return category;
	}

	public void GenerateItemSprite(String category)
	{
		String item = recycleObjects[category];
		recycleSprite.Texture = GD.Load<Texture2D>("res://Assets/" + item + ".png");
		recycleSprite.Scale = new Vector2I(3,3);
	}

	public void GenerateSprite(String name)
	{
		sprite.Texture = GD.Load<Texture2D>("res://Assets/" + name + ".png");
		sprite.Scale = new Vector2(0.05f,0.05f);
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
				
				GenerateSprite("greencheck");
			}
			else
			{
				GenerateSprite("redcross");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_right") )
		{
			if(actualCategory == "glass")
			{
				GD.Print("Acierto");
				GenerateSprite("greencheck");
			}
			else
			{
				GenerateSprite("redcross");
			}
			UpdateItemSprite();
		}

		if (@event.IsActionPressed("ui_up"))
		{
			if(actualCategory == "plastic")
			{
				GD.Print("Acierto");
				GenerateSprite("greencheck");
			}
			
			UpdateItemSprite();
		}
	}


}
