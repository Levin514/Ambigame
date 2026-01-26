using Godot;
using System;

public partial class Card : Node
{
	public string cardName { get; set; }
	public string seedType { get; set; }
	public Texture2D image { get; set; }
	public string description { get; set; }

	public Card() { }

	public Card(string name, string seedType, Texture2D image, string description)
	{
		this.cardName = name;
		this.seedType = seedType;
		this.image = image;
		this.description = description;
	}
}
