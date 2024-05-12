using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;

// ReSharper disable all InconsistentNaming
namespace stckytwl.BulletCrack;

public class SonicBulletSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SonicBulletSoundPlayer).GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance);
    }

    [PatchPrefix]
    public static void Prefix(SonicBulletSoundPlayer __instance, ref List<SonicBulletSoundPlayer.SonicAudio> ____sources)
    {
        ____sources = Plugin.SonicAudios;
    }
}

public class RandomizeSonicAudioPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SonicBulletSoundPlayer).GetMethod("method_0", BindingFlags.Public | BindingFlags.Instance);
    }

    [PatchPostfix]
    public static void PatchPostFix(ref List<SonicBulletSoundPlayer.SonicAudio> ____sources, ref SonicBulletSoundPlayer.SonicAudio __result)
    {
        var resultType = __result.Type;
        var sorted = ____sources.Where(i => i.Type == resultType).ToList();
        var num = UnityEngine.Random.Range(0, sorted.Count - 1);
        var randomSonicAudio = sorted[num];

        __result = randomSonicAudio;
    }
}