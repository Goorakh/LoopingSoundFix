using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LoopingSoundFix
{
    static class EnsureSoundEventsStopOnDestroy
    {
        static readonly List<IDetour> _hooks = [];

        public static void ApplyPatches()
        {
            if (_hooks.Count > 0)
            {
                Log.Error("Patches already applied");
                return;
            }

            MethodInfo AkSoundEngine_UnregisterGameObj_MI = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.UnregisterGameObj(default));
            if (AkSoundEngine_UnregisterGameObj_MI != null)
            {
                _hooks.Add(new Hook(AkSoundEngine_UnregisterGameObj_MI, AkSoundEngine_UnregisterGameObj));
            }
            else
            {
                Log.Error("Failed to find method: AkSoundEngine.UnregisterGameObj");
            }

            // These are never actually called, but hook them just to be sure
            MethodInfo AkSoundEngine_UnregisterGameObjInternal_MI = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.UnregisterGameObjInternal(default(GameObject)));
            if (AkSoundEngine_UnregisterGameObjInternal_MI != null)
            {
                _hooks.Add(new Hook(AkSoundEngine_UnregisterGameObjInternal_MI, AkSoundEngine_UnregisterGameObjInternal));
            }
            else
            {
                Log.Error("Failed to find method: AkSoundEngine.UnregisterGameObjInternal");
            }

            MethodInfo AkSoundEngine_UnregisterAllGameObj_MI = SymbolExtensions.GetMethodInfo(() => AkSoundEngine.UnregisterAllGameObj());
            if (AkSoundEngine_UnregisterAllGameObj_MI != null)
            {
                _hooks.Add(new Hook(AkSoundEngine_UnregisterAllGameObj_MI, AkSoundEngine_UnregisterAllGameObj));
            }
            else
            {
                Log.Error("Failed to find method: AkSoundEngine.UnregisterAllGameObj");
            }
        }

        public static void UndoPatches()
        {
            foreach (IDetour hook in _hooks)
            {
                if (hook != null)
                {
                    hook.Undo();
                    hook.Dispose();
                }
            }

            _hooks.Clear();
        }

        static uint[] _sharedPlayingIdsBuffer = new uint[16];
        static void stopAllSoundEvents(GameObject gameObject)
        {
            if (!gameObject)
                return;

            uint numPlayingIds;
            do
            {
                numPlayingIds = (uint)_sharedPlayingIdsBuffer.Length;
                AKRESULT result = AkSoundEngine.GetPlayingIDsFromGameObject(gameObject, ref numPlayingIds, _sharedPlayingIdsBuffer);
                if (result != AKRESULT.AK_Success)
                    return;

                if (numPlayingIds >= _sharedPlayingIdsBuffer.Length)
                {
                    Array.Resize(ref _sharedPlayingIdsBuffer, _sharedPlayingIdsBuffer.Length * 2);
                }
                else
                {
                    break;
                }
            } while (true);

            if (numPlayingIds <= 0)
                return;

#if DEBUG
            Log.Debug($"Stopping {numPlayingIds} audio event(s) for {gameObject}");
#endif

            for (int i = 0; i < numPlayingIds; i++)
            {
                uint playingId = _sharedPlayingIdsBuffer[i];

                switch (Main.SoundCancelMode)
                {
                    case SoundCancelMode.Immediate:
                        AkSoundEngine.StopPlayingID(playingId);
                        break;
                    case SoundCancelMode.NextLoop:
                        AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Break, playingId);
                        break;
                    default:
                        Log.Warning($"Stop mode not implemented: {Main.SoundCancelMode}");
                        break;
                }
            }
        }

        delegate AKRESULT orig_AkSoundEngine_UnregisterGameObj(GameObject gameObject);
        static AKRESULT AkSoundEngine_UnregisterGameObj(orig_AkSoundEngine_UnregisterGameObj orig, GameObject gameObject)
        {
            AKRESULT result;
            try
            {
                stopAllSoundEvents(gameObject);
            }
            finally
            {
                result = orig(gameObject);
            }

            return result;
        }

        delegate AKRESULT orig_AkSoundEngine_UnregisterGameObjInternal(GameObject in_GameObj);
        static AKRESULT AkSoundEngine_UnregisterGameObjInternal(orig_AkSoundEngine_UnregisterGameObjInternal orig, GameObject in_GameObj)
        {
            AKRESULT result;
            try
            {
                stopAllSoundEvents(in_GameObj);
            }
            finally
            {
                result = orig(in_GameObj);
            }

            return result;
        }

        delegate AKRESULT orig_AkSoundEngine_UnregisterAllGameObj();
        static AKRESULT AkSoundEngine_UnregisterAllGameObj(orig_AkSoundEngine_UnregisterAllGameObj orig)
        {
            AKRESULT result = orig();

            if (result == AKRESULT.AK_Success)
            {
                AkSoundEngine.StopAll();
            }

            return result;
        }
    }
}
