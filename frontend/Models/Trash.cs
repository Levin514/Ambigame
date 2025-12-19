using Godot;
using System;
using System.Collections.Generic;

namespace Snake;

using System.Linq;

public partial class Trash : RefCounted
{
    public String category { get; private set; }
    public String trashName { get; private set; }

    private readonly Dictionary<String, List<String>> recycleObjects = new Dictionary<string, List<String>>()
	{
		{"paper", ["newspaper"]},
		{"plastic", ["plasticBottle"]},
		{"glass", ["glassJar"]}
	};

    public Trash()
    {
        Random rnd = new();
        int randomIndex = rnd.Next(0, recycleObjects.Count);
        String randomCategory = recycleObjects.Keys.ElementAt(randomIndex);
        List<String> categoryList = recycleObjects[randomCategory];
        randomIndex = rnd.Next(0, categoryList.Count);
        String randomTrashName = categoryList[randomIndex];
        this.category = randomCategory;
        this.trashName = randomTrashName;
    }

}