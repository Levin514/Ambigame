using Godot;
using System;


public partial class PauseMenu : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        Visible = false;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

	public void Resume()
    {
		Visible = false;
        GetTree().Paused = false;
    }

	public void Pause()
    {
		Visible = true;
        GetTree().Paused = true;
		GD.Print(GetTree());
    }


	public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GD.Print("ESC");

            if (GetTree().Paused == false)
            {
                Pause();   
            }

            else if (GetTree().Paused == true)
            {
                Resume();
            }
			
            
        }
    }

	public void Continue_button_pressed()
    {
        Resume();
    }

	public void Main_menu_button_pressed()
    {
		GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/LoginScene.tscn");
    }

}
