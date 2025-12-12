using Godot;
using System;
using System.Text;
using Snake;


public partial class LifeSystem : Control
{
	[Export] private Label healthLabel;
	[Signal] public delegate void GameOverEventHandler();
	[Export] private SnakeBody _snakeBody;
	private int actualHealth;
	private int maxHealth;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        actualHealth = 5;
		maxHealth = 5;
		SetHealth();
		_snakeBody.UpdateHealth += OnUpdateHealth;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnUpdateHealth()
    {
		if(actualHealth > 0)
        {
            actualHealth -= 1;
			SetHealth();
        }
		
		if(actualHealth == 0)
        {
            EmitSignal(SignalName.GameOver);
			GD.Print("Sin vida");
        }
		
    }
	
	public void SetHealth()
    {
		StringBuilder data = new StringBuilder();
		data.Append(actualHealth);
		data.Append("/");
		data.Append(maxHealth.ToString());
        healthLabel.Text =  data.ToString();	
    }


}
