using Godot;
using System;
using System.Collections.Generic;

public partial class Classifier : Node2D
{
	[Export] Label puntuacionLabel;
	[Export] Label recicladosLabel;
	[Export] Label timerLabel;

	private readonly LinkedList<Vector2I> body;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/*
	public override void _Input(InputEvent @event)
	{
		Vector2I element = body.First.Value;
		body.RemoveFirst();

		if (@event.IsAction("ui_left"))
		{
			
		}

		if (@event.IsAction("ui_right") )
		{
			
		}

		if (@event.IsAction("ui_up"))
		{
			_
		}
			
	}
	*/

}
