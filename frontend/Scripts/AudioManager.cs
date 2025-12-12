using Godot;
using System;

public partial class AudioManager : Node
{
	private bool isMuted = false;
	private const string ConfigPath = "user://audio_settings.cfg";

	public override void _Ready()
	{
		LoadSettings();
	}

	public void ToggleMute()
	{
		isMuted = !isMuted;
		ApplyMuteState();
		SaveSettings();
	}

	public void SetMute(bool mute)
	{
		isMuted = mute;
		ApplyMuteState();
		SaveSettings();
	}

	public bool IsMuted()
	{
		return isMuted;
	}

	private void ApplyMuteState()
	{
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), isMuted);
		GD.Print($"AudioManager: Sonido {(isMuted ? "silenciado" : "activado")}");
	}

	private void SaveSettings()
	{
		var config = new ConfigFile();
		config.SetValue("audio", "muted", isMuted);
		config.Save(ConfigPath);
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		var error = config.Load(ConfigPath);
		
		if (error == Error.Ok)
		{
			isMuted = (bool)config.GetValue("audio", "muted", false);
			ApplyMuteState();
		}
	}
}
