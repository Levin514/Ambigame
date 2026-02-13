using Godot;
using System;

public partial class AudioManager : Node
{
	// Buses de audio
	private int musicBusIndex;
	private int sfxBusIndex;
	
	// Volúmenes (0.0 a 1.0)
	private float musicVolume = 0.7f;
	private float sfxVolume = 0.8f;
	private bool isMuted = false;
	
	// Reproductores de audio
	private AudioStreamPlayer musicPlayer;
	private AudioStreamPlayer sfxPlayer;
	
	// Efectos de sonido precargados
	private AudioStream trashCollectSound;
	private AudioStream repairPipeSound;
	private AudioStream breakPipeSound;
	private AudioStream plantSeedSound;
	private AudioStream waterPlantSound;
	private AudioStream victorySound;
	private AudioStream defeatSound;
	
	private const string ConfigPath = "user://audio_settings.cfg";

	public override void _Ready()
	{
		// Obtener índices de buses de audio (ya existen en default_bus_layout.tres)
		musicBusIndex = AudioServer.GetBusIndex("Music");
		sfxBusIndex = AudioServer.GetBusIndex("SFX");
		
		// Verificar que los buses existan
		if (musicBusIndex == -1 || sfxBusIndex == -1)
		{
			GD.PrintErr("AudioManager: Error - Los buses Music/SFX no existen. Verifica default_bus_layout.tres");
		}
		else
		{
			GD.Print($"AudioManager: Buses correctamente inicializados - Music: {musicBusIndex}, SFX: {sfxBusIndex}");
		}
		
		// Crear reproductores de audio
		musicPlayer = new AudioStreamPlayer();
		musicPlayer.Bus = "Music";
		AddChild(musicPlayer);
		
		sfxPlayer = new AudioStreamPlayer();
		sfxPlayer.Bus = "SFX";
		AddChild(sfxPlayer);
		
		// Precargar efectos de sonido
		trashCollectSound = GD.Load<AudioStream>("res://Assets/Recolet.mp3");
		repairPipeSound = GD.Load<AudioStream>("res://Assets/RepairPipe.mp3");
		breakPipeSound = GD.Load<AudioStream>("res://Assets/RepairPipe.mp3"); // Reutilizar o crear nuevo
		plantSeedSound = GD.Load<AudioStream>("res://Assets/Plant.mp3");
		waterPlantSound = GD.Load<AudioStream>("res://Assets/Water.mp3");
		victorySound = GD.Load<AudioStream>("res://Assets/Victory.mp3");
		defeatSound = GD.Load<AudioStream>("res://Assets/Losing.mp3");
		
		// Verificar que los sonidos se hayan cargado
		int soundsLoaded = 0;
		if (trashCollectSound != null) soundsLoaded++;
		if (repairPipeSound != null) soundsLoaded++;
		if (breakPipeSound != null) soundsLoaded++;
		if (plantSeedSound != null) soundsLoaded++;
		if (waterPlantSound != null) soundsLoaded++;
		if (victorySound != null) soundsLoaded++;
		if (defeatSound != null) soundsLoaded++;
		GD.Print($"AudioManager: {soundsLoaded}/7 efectos de sonido cargados correctamente");
		
		// Cargar configuración y aplicar volúmenes de forma diferida
		LoadSettings();
		CallDeferred(nameof(ApplyAllVolumes));
	}
	
	// Aplicar todos los volúmenes de forma segura después de la inicialización completa
	private void ApplyAllVolumes()
	{
		ApplyMuteState();
		ApplyMusicVolume();
		ApplySFXVolume();
		GD.Print("AudioManager: Volúmenes aplicados después de inicialización completa");
	}

	// ========== CONTROL DE VOLUMEN ==========
	
	public void SetMusicVolume(float volume)
	{
		musicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
		ApplyMusicVolume();
		SaveSettings();
	}
	
	public void SetSFXVolume(float volume)
	{
		sfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
		ApplySFXVolume();
		SaveSettings();
	}
	
	public float GetMusicVolume()
	{
		return musicVolume;
	}
	
	public float GetSFXVolume()
	{
		return sfxVolume;
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

	private void ApplyMusicVolume()
	{
		// Verificar que el índice del bus sea válido
		int currentMusicBusIndex = AudioServer.GetBusIndex("Music");
		if (currentMusicBusIndex != -1)
		{
			musicBusIndex = currentMusicBusIndex;
			float db = LinearToDb(musicVolume);
			AudioServer.SetBusVolumeDb(musicBusIndex, db);
			GD.Print($"AudioManager: Volumen música: {musicVolume:F2} ({db:F1} dB)");
		}
		else
		{
			GD.PrintErr("AudioManager: No se pudo aplicar volumen de música - Bus no encontrado");
		}
	}
	
	private void ApplySFXVolume()
	{
		// Verificar que el índice del bus sea válido
		int currentSfxBusIndex = AudioServer.GetBusIndex("SFX");
		if (currentSfxBusIndex != -1)
		{
			sfxBusIndex = currentSfxBusIndex;
			float db = LinearToDb(sfxVolume);
			AudioServer.SetBusVolumeDb(sfxBusIndex, db);
			GD.Print($"AudioManager: Volumen efectos: {sfxVolume:F2} ({db:F1} dB)");
		}
		else
		{
			GD.PrintErr("AudioManager: No se pudo aplicar volumen de efectos - Bus no encontrado");
		}
	}

	private void ApplyMuteState()
	{
		int masterBusIndex = AudioServer.GetBusIndex("Master");
		if (masterBusIndex != -1)
		{
			AudioServer.SetBusMute(masterBusIndex, isMuted);
			GD.Print($"AudioManager: Sonido {(isMuted ? "silenciado" : "activado")}");
		}
		else
		{
			GD.PrintErr("AudioManager: No se pudo aplicar estado de mute - Bus Master no encontrado");
		}
	}
	
	private float LinearToDb(float linear)
	{
		if (linear <= 0.0f)
			return -80.0f;
		return Mathf.LinearToDb(linear);
	}

	// ========== REPRODUCCIÓN DE MÚSICA ==========
	
	public void PlayMusic(AudioStream music, bool loop = true)
	{
		if (musicPlayer != null && music != null)
		{
			musicPlayer.Stream = music;
			
			// Configurar loop si es AudioStreamMP3
			if (music is AudioStreamMP3 mp3Stream)
			{
				mp3Stream.Loop = loop;
			}
			
			musicPlayer.Play();
			GD.Print($"AudioManager: Reproduciendo música (Loop: {loop})");
		}
		else
		{
			if (musicPlayer == null)
				GD.PrintErr("AudioManager: musicPlayer es null");
			if (music == null)
				GD.PrintErr("AudioManager: AudioStream de música es null");
		}
	}
	
	public void StopMusic()
	{
		if (musicPlayer != null && musicPlayer.Playing)
		{
			musicPlayer.Stop();
		}
	}

	// ========== REPRODUCCIÓN DE EFECTOS ==========
	
	public void PlayTrashCollect()
	{
		PlaySFX(trashCollectSound);
	}
	
	public void PlayRepairPipe()
	{
		PlaySFX(repairPipeSound);
	}
	
	public void PlayBreakPipe()
	{
		PlaySFX(breakPipeSound);
	}
	
	public void PlayPlantSeed()
	{
		PlaySFX(plantSeedSound);
	}
	
	public void PlayWaterPlant()
	{
		PlaySFX(waterPlantSound);
	}
	
	public void PlayVictory()
	{
		PlaySFX(victorySound);
	}
	
	public void PlayDefeat()
	{
		PlaySFX(defeatSound);
	}
	
	private void PlaySFX(AudioStream sound)
	{
		if (sound == null)
		{
			GD.PrintErr("AudioManager: Efecto de sonido no encontrado (null)");
			return;
		}
		
		// Verificar que el bus SFX exista
		int sfxBus = AudioServer.GetBusIndex("SFX");
		if (sfxBus == -1)
		{
			GD.PrintErr("AudioManager: Bus SFX no existe");
			return;
		}
		
		// Crear un reproductor temporal para permitir múltiples efectos simultáneos
		var player = new AudioStreamPlayer();
		player.Bus = "SFX";
		player.Stream = sound;
		player.ProcessMode = ProcessModeEnum.Always; // Reproducir incluso cuando el juego está pausado
		AddChild(player);
		player.Play();
		
		// Eliminar el reproductor cuando termine
		player.Finished += () => {
			player.QueueFree();
		};
	}

	// ========== GUARDAR/CARGAR CONFIGURACIÓN ==========
	
	private void SaveSettings()
	{
		var config = new ConfigFile();
		config.SetValue("audio", "muted", isMuted);
		config.SetValue("audio", "music_volume", musicVolume);
		config.SetValue("audio", "sfx_volume", sfxVolume);
		config.Save(ConfigPath);
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		var error = config.Load(ConfigPath);
		
		if (error == Error.Ok)
		{
			isMuted = (bool)config.GetValue("audio", "muted", false);
			musicVolume = (float)config.GetValue("audio", "music_volume", 0.7f);
			sfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);
			GD.Print($"AudioManager: Configuración cargada - Música: {musicVolume:F2}, SFX: {sfxVolume:F2}, Muted: {isMuted}");
		}
		else
		{
			GD.Print("AudioManager: Configuración no encontrada, usando valores por defecto");
		}
		// NO aplicar volúmenes aquí - se aplican en ApplyAllVolumes() llamado con CallDeferred
	}
}
