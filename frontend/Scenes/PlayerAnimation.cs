using Godot;
using System;

public partial class PlayerAnimation : Node2D
{
	[Export] private int TileSize = 16;
	[Export] private float MoveTime = 0.15f; // how long each step takes
	private AnimatedSprite2D animatedSprite;

	private bool isMoving = false;
	private Vector2 targetPosition;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Position = Position.Snapped(Vector2.One * TileSize);
		targetPosition = Position;
	}

	public override void _Process(double delta)
	{
	}

	public void ChangeAnimation(string animationName)
	{
			animatedSprite.Play(animationName);
	}

	public void MoveSprite(Vector2I newPosition, double delta)
	{
		Vector2 targetPos = newPosition * TileSize;
		float distance = Position.DistanceTo(targetPos);
		
		// Si la distancia es muy grande (teletransporte/wrap-around), mover instantáneamente
		if (distance > TileSize * 5) // umbral para detectar teletransporte
		{
			Position = targetPos;
		}
		else
		{
			Position = Position.MoveToward(targetPos, 100);
		}
	}
}
