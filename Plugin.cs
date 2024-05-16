using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Common.Utils;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;

namespace stckytwl.BulletCrack;

[BepInPlugin("com.stckytwl.bulletcrack", "stckytwl.BulletCrack", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static readonly List<SonicBulletSoundPlayer.SonicAudio> SonicAudios = [];
    private string _soundFilesDirectory;
    private static readonly Dictionary<string, SonicBulletSoundPlayer.SonicType> SubDirMappings = new()
    {
        { "Sonic9", SonicBulletSoundPlayer.SonicType.Sonic9 },
        { "Sonic545", SonicBulletSoundPlayer.SonicType.Sonic545 },
        { "Sonic762", SonicBulletSoundPlayer.SonicType.Sonic762 },
        { "SonicShotgun", SonicBulletSoundPlayer.SonicType.SonicShotgun }
    };

    public static ConfigEntry<float> PluginVolume;

    private void Awake()
    {
        _soundFilesDirectory = Assembly.GetExecutingAssembly().Location.GetDirectory() + @"\";
        PluginVolume = Config.Bind("", "Sonic Crack Volume", 100f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 200f)));

        new ReplaceSonicBulletSoundsPatch().Enable();
        new RandomizeSonicAudioPatch().Enable();
        new ModifySonicAudioPatch().Enable();

        LoadAudioClips();
    }

    private void LoadAudioClips()
    {
        var bulletCrackSoundsDir = Path.Combine(_soundFilesDirectory, "Sounds");
        
        if (!Directory.Exists(bulletCrackSoundsDir))
        {
            Logger.LogError($"Directory {bulletCrackSoundsDir} does not exist.");
            return;
        }
        
        foreach (var entry in SubDirMappings)
        {
            var subDirPath = Path.Combine(bulletCrackSoundsDir, entry.Key);
            if (!Directory.Exists(subDirPath))
            {
                Logger.LogError($"Directory {subDirPath} does not exist.");
                continue;
            }

            var audioFiles = Directory.GetFiles(subDirPath);
            LoadAudioClips(audioFiles, entry.Value);
            Logger.LogInfo($"Loading {audioFiles.Length} files from {subDirPath}");
        }
        
        var sharedDirPath = Path.Combine(bulletCrackSoundsDir, "Shared");
        if (!Directory.Exists(sharedDirPath))
        {
            Logger.LogWarning($"Directory {sharedDirPath} does not exist.");
            return;
        }

        var sharedAudioFiles = Directory.GetFiles(sharedDirPath);
        foreach (var i in SubDirMappings.Values)
        {
            LoadAudioClips(sharedAudioFiles, i);
        }
    }

    private async void LoadAudioClips(IEnumerable<string> files, SonicBulletSoundPlayer.SonicType sonicType)
    {
        foreach (var path in files)
        {
            var audioClip = await RequestAudioClip(path);
            if (audioClip is null) continue;
            
            var sonicAudio = new SonicBulletSoundPlayer.SonicAudio
            {
                Clip = audioClip,
                Type = sonicType
            };
            SonicAudios.Add(sonicAudio);
        }
    }

    private async Task<AudioClip> RequestAudioClip(string path)
    {
        var extension = Path.GetExtension(path);
        AudioType audioType;
        switch (extension)
        {
            case ".wav":
                audioType = AudioType.WAV;
                break;
            case ".ogg":
                audioType = AudioType.OGGVORBIS;
                break;
            default:
                Logger.LogWarning($"BulletCrack: \"{Path.GetFileNameWithoutExtension(path)}\" is not a valid audio file! {extension}");
                return null;
        }

        var uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        var sendWeb = uwr.SendWebRequest();

        while (!sendWeb.isDone)
            await Task.Yield();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Logger.LogError($"BulletCrack: Failed To Fetch Audio Clip \"{Path.GetFileNameWithoutExtension(path)}\"");
            return null;
        }

        var audioclip = DownloadHandlerAudioClip.GetContent(uwr);
        audioclip.name = Path.GetFileNameWithoutExtension(path);
        return audioclip;
    }
}