using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace stckytwl.BulletCrack;

[BepInPlugin("com.stckytwl.bulletcrack", "stckytwl.bulletcrack", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static readonly List<SonicBulletSoundPlayer.SonicAudio> SonicAudios = [];

    private void Awake()
    {
        new SonicBulletSoundPatch().Enable();
        new RandomizeSonicAudioPatch().Enable();

        LoadAudioClips();
    }

    private void LoadAudioClips()
    {
        var sonic9AudioDir= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\BulletCrack\Sounds\Sonic9");
        var sonic545AudioDir= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\BulletCrack\Sounds\Sonic545");
        var sonic762AudioDir= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\BulletCrack\Sounds\Sonic762");
        var sonicShotgunAudioDir= Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\BulletCrack\Sounds\SonicShotgun");

        for (var i = 0; i < sonic9AudioDir.Length; i++)
        {
            LoadAudioClip(sonic9AudioDir[i], SonicBulletSoundPlayer.SonicType.Sonic9);
            Logger.LogWarning($"Loaded {sonic9AudioDir[i]}");
        }
            
        for (var i = 0; i < sonic545AudioDir.Length; i++)
        {
            LoadAudioClip(sonic545AudioDir[i], SonicBulletSoundPlayer.SonicType.Sonic545);
            Logger.LogWarning($"Loaded {sonic545AudioDir[i]}");
        }
            
        for (var i = 0; i < sonic762AudioDir.Length; i++)
        {
            LoadAudioClip(sonic762AudioDir[i], SonicBulletSoundPlayer.SonicType.Sonic762);
            Logger.LogWarning($"Loaded {sonic762AudioDir[i]}");
        }
            
        for (var i = 0; i < sonicShotgunAudioDir.Length; i++)
        {
            LoadAudioClip(sonicShotgunAudioDir[i], SonicBulletSoundPlayer.SonicType.SonicShotgun);
            Logger.LogWarning($"Loaded {sonicShotgunAudioDir[i]}");
        }
    }

    private async void LoadAudioClip(string path, SonicBulletSoundPlayer.SonicType sonicType)
    {
        var audioClip = await RequestAudioClip(path);
        var sonicAudio = new SonicBulletSoundPlayer.SonicAudio
        {
            Clip = audioClip,
            Type = sonicType
        };
        SonicAudios.Add(sonicAudio);
    }

    private async Task<AudioClip> RequestAudioClip(string path)
    {
        var extension = Path.GetExtension(path);
        var audioType = AudioType.WAV;
        switch (extension)
        {
            case ".wav":
                audioType = AudioType.WAV;
                break;
            case ".ogg":
                audioType = AudioType.OGGVORBIS;
                break;
        }

        var uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        var sendWeb = uwr.SendWebRequest();

        while (!sendWeb.isDone)
            await Task.Yield();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Logger.LogError($"BulletCrack: Failed To Fetch Audio Clip {Path.GetFileNameWithoutExtension(path)}");
            return null;
        }

        var audioclip = DownloadHandlerAudioClip.GetContent(uwr);
        audioclip.name = Path.GetFileNameWithoutExtension(path);
        return audioclip;
    }
}