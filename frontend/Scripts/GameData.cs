using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake;

public partial class GameData : Node
{
	public readonly Dictionary<String, String> recycleObjects = new Dictionary<string, string>()
	{
		{"paper", "newspaper"},
		{"plastic", "plasticBottle"},
		{"glass", "glassJar"}
	};

	public LinkedList<Trash> globalTrashList;

	public static GameData Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
