using Godot;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Snake;

public partial class Snake : Node2D
{
	// To generate random numbers.
	private static readonly Random rnd = new();

	[Export] DualGridTilemap DualGrid;
	// Scenes
	private Vector2I _gameSize;
	[Export] private SnakeBody _snakeBody;

	// We could use a Godot Timer too.
	private Timer timer;
	private bool isGameOver = false;

	public override void _Ready()
	{
		_gameSize = new Vector2I(33, 21);
		isGameOver = false;
		timer = new Timer(4000);
		timer.Elapsed += NewApple;
		timer.AutoReset = true;
		timer.Start();

		// We connect to the SnakeBody's GameOver Signal using C#
		// Lambda expression works too.
		_snakeBody.GameOver += OnGameOver;
	}

	public override void _Process(double delta)
	{
	}

	public void OnGameOver()
	{
		GD.Print("Game Over");
		isGameOver = true;
		timer.Stop();
		timer.Dispose();
	}

	public void NewApple(object src, ElapsedEventArgs e)
	{
		if (isGameOver) return;
		DualGrid.AddTrash(new Vector2I(rnd.Next(0, 32), rnd.Next(0, 21)));
	}

	public void OnAgainPressed()
	{
		GetTree().ReloadCurrentScene();
	}

	public void OnSalirPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}
}
