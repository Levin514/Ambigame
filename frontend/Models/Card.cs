using Godot;
using System;

public partial class Card : Node
{
	public string cardName { get; set; }
	public Texture2D image { get; set; }
	public string description { get; set; }

	public Card() { }

	public Card(string name, Texture2D image, string description)
	{
		this.cardName = name;
		this.image = image;
		this.description = description;
	}
}
