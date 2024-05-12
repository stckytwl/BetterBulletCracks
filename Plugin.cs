using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace stckytwl.BulletCrack
{
    [BepInPlugin("com.stckytwl.bulletcrack", "stckytwl.bulletcrack", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AudioClip[] AudioClips;

        private void Awake()
        {
            new FlyingBulletSoundPatch().Enable();

            LoadAudioClips();
        }

        private void LoadAudioClips()
        {
            var audioFilesDir =
                Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins\BulletCrack\Sounds\");
            AudioClips = new AudioClip[audioFilesDir.Length];

            for (var i = 0; i < audioFilesDir.Length; i++)
            {
                LoadAudioClip(audioFilesDir[i], i);
                Logger.LogWarning($"Loaded {audioFilesDir[i]}");
            }
        }

        private async void LoadAudioClip(string path, int index)
        {
            AudioClips[index] = await RequestAudioClip(path);
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

    public class FlyingBulletSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FlyingBulletSoundPlayer).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        // ReSharper disable all InconsistentNaming
        public static void Prefix(FlyingBulletSoundPlayer __instance, ref AudioClip[] ____sources)
        {
            ____sources = Plugin.AudioClips;
        }
    }
}