using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake;

public partial class GameData : Node
{
	public string recycleBackground = "forest";
	public Dictionary<string, Dictionary<string,List<String>>> recycleObjects;
	public readonly Dictionary<String, List<String>> forestRecycleObjects = new Dictionary<string, List<String>>()
	{
		{"paper", ["newspaper"]},
		{"plastic", ["plasticBottle"]},
		{"glass", ["glassJar"]}
	};

	public readonly Dictionary<String, List<String>> beachRecycleObjects = new Dictionary<string, List<String>>()
	{
		{"paper", ["cardboardBox"]},
		{"plastic", ["plasticBaskets"]},
		{"glass", ["glassJar"]}
	};

	public readonly Dictionary<String, List<String>> manglarRecycleObjects = new Dictionary<string, List<String>>()
	{
		{"paper", ["cardboardBox"]},
		{"plastic", ["plasticBaskets", "plasticBottle"]},
		{"glass", ["glassJar", "metalCan"]}
	};

	public LinkedList<Trash> globalTrashList;
	public LinkedList<Card> globalcardSeedsList;
	public int globalSeedsAmount = 0;
	public int globalWaterAmount = 0;
	public int globalScoreDivisor = 250; // Obsoleto - usar divisores específicos
	public int globalSeedsDivisor = 100; // Divisor para cálculo de semillas
	public int globalWaterDivisor = 250; // Divisor para cálculo de agua

	// Niveles completados
	public bool recyclingLevelCompleted = false;
	public bool classifyLevelCompleted = false;
	public bool waterLevelCompleted = false;
	public bool waterCatchLevelCompleted = false;

	public static GameData Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
		recycleObjects = new Dictionary<string, Dictionary<string,List<String>>>()
		{
			{"forest", forestRecycleObjects},
			{"beach", beachRecycleObjects},
			{"manglar", manglarRecycleObjects}
		};
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
