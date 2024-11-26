using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;

// ReSharper disable all InconsistentNaming
namespace stckytwl.BetterBulletCracks;

public class ReplaceSonicBulletSoundsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SonicBulletSoundPlayer), nameof(SonicBulletSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static void Prefix(ref List<SonicBulletSoundPlayer.SonicAudio> ____sources)
    {
        ____sources = Plugin.SonicAudios;
    }
}

public class RandomizeSonicAudioPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SonicBulletSoundPlayer), nameof(SonicBulletSoundPlayer.method_0));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref List<SonicBulletSoundPlayer.SonicAudio> ____sources, ref SonicBulletSoundPlayer.SonicAudio __result)
    {
        var resultType = __result.Type;
        var sorted = ____sources.Where(i => i.Type == resultType).ToList();
        var num = UnityEngine.Random.Range(0, sorted.Count);
        var randomSonicAudio = sorted[num];

        __result = randomSonicAudio;
    }
}

public class ModifySonicAudioVolumePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SonicBulletSoundPlayer.Class522), nameof(SonicBulletSoundPlayer.Class522.Initialize));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref float ___float_1)
    {
        var newVolume = ___float_1 * Plugin.PluginVolume.Value / 100f;
        ___float_1 = newVolume;
    }
}