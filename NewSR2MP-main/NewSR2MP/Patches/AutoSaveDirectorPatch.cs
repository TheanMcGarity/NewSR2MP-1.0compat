using HarmonyLib;


using Il2CppMonomiPark.SlimeRancher.Persist;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using NewSR2MP.SaveModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using UnityEngine;


namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Load))]
    public class AutoSaveDirectorLoadSave
    {
        public static void Postfix(AutoSaveDirector __instance, GameSaveIdentifier identifier, bool reloadAllCoreScenes, Stream stream = null)
        {
            if (ClientActive()) return;
            MultiplayerManager.CheckForMPSavePath();
            var path = Path.Combine(GameContext.Instance.AutoSaveDirector._storageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves", $"{__instance.CurrentSaveGameName()}.srmp");
            var networkGame = new NetworkV01();

            if (File.Exists(path))
            {
                FileStream fs = File.Open(path, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                
                try { networkGame.ReadData(br); } catch (Exception e) { SRMP.Error($"Error loading multiplayer save file: {e}"); }
                
                br.Dispose();
                fs.Dispose();
            }
            
            savedGame = networkGame;
            savedGamePath = path;
        }
    }
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.StartNewGame))]
    public class AutoSaveDirectorLoadNewGame
    {
        public static void Postfix(AutoSaveDirector __instance, int saveSlotIndex, GameSettingsModel gameSettingsModel)
        {
            MultiplayerManager.CheckForMPSavePath();
            var path = Path.Combine(GameContext.Instance.AutoSaveDirector._storageProvider.TryCast<FileStorageProvider>().savePath, "MultiplayerSaves", $"{__instance.CurrentSaveGameName()}.srmp");
            var networkGame = new NetworkV01();
            
            FileStream fs = File.Create(path);
            BinaryWriter bw = new BinaryWriter(fs);
            
            try { networkGame.WriteData(bw); } catch { }
            
            bw.Dispose();
            fs.Dispose();
            
            savedGame = networkGame;
            savedGamePath = path;
        }
    }
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.SaveGame))]
    public class AutoSaveDirectorSaveGame
    {
        public static bool Prefix(AutoSaveDirector __instance)
        {
            return !ClientActive();
        }
        public static void Postfix(AutoSaveDirector __instance)
        {
            if (ClientActive()) return;
            
            try
            {
                MultiplayerManager.DoNetworkSave();
            }
            catch (Exception ex)
            {
                SRMP.Error($"Error occured during saving multiplayer data!\n{ex}");
            }
        }
    }
    
    
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Awake))]
    public class AutoSaveDirectorAwake
    {
        public static void Postfix(AutoSaveDirector __instance)
        {
            foreach (var ident in __instance._saveReferenceTranslation._identifiableTypeLookup)
            {
                identifiableTypes.TryAdd(GetIdentID(ident.Value), ident.Value);
            }
            foreach (var pedia in Resources.FindObjectsOfTypeAll<PediaEntry>()) // SavedGame's list doesnt include some pedia entries.
            {
                pediaEntries.TryAdd(pedia.name, pedia); 
            }

            foreach (var scene in __instance._saveReferenceTranslation._sceneGroupTranslation.RawLookupDictionary)
            {
                sceneGroups.TryAdd(__instance._saveReferenceTranslation._sceneGroupTranslation.InstanceLookupTable._reverseIndex[scene.key], scene.value);
                sceneGroupsReverse.TryAdd(scene.key.TrimStart('S','c','e','n','e','G','r','o','u','p','.'), __instance._saveReferenceTranslation._sceneGroupTranslation.InstanceLookupTable._reverseIndex[scene.key]);
            }
            
            CreateWeatherLookup(__instance._saveReferenceTranslation);
        }
    }
}
