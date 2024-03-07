using System.Collections.Generic;
using Terraria.ModLoader;
using System.Linq;
using Terraria;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerramazingGijinkaMadhouse.NPCs;
using System;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using TerramazingGijinkaMadhouse.NPCs.Hypnos;
using System.Collections;
using TerramazingGijinkaMadhouse.Items;

namespace TerramazingGijinkaMadhouse
{
	public class TerramazingGijinkaMadhouseMod : Mod
	{
		public static TerramazingGijinkaMadhouseMod Instance { get; private set; }




		public override void PostSetupContent()
		{
			//ModCompatibility.calamityEnabled = ModLoader.HasMod("CalamityMod");
			//ModLoader.TryGetMod("Census", out Mod censusMod);
			//if (censusMod != null)
			//{
			//    censusMod.Call("TownNPCCondition", ModContent.NPCType<StarbornPrincess>(), "Brutally murder her mom");
			//}
			TryDoCensusSupport();
			//Logger.Info(TransmogrificationManager.Transmogrifications.Count());
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			EverquartzMessageType msgType = (EverquartzMessageType)reader.ReadByte();
			switch (msgType)
			{
				case EverquartzMessageType.HypnosReward:
					Player priest = Main.player[reader.ReadInt32()];
					byte[] byteArray = reader.ReadBytes((MadhouseUtils.EnumCount<HypnosReward>() - 1) / 8 + 1);
					BitArray bitArray = new BitArray(byteArray);
					List<HypnosReward> rewards = new List<HypnosReward>();
					for (int i = 0; i < bitArray.Length; i++)
					{
						if (bitArray.Get(i))
						{
							rewards.Add((HypnosReward)i);
						}
					}
					NPCs.Hypnos.JHypnos.HandleRewardsServer(priest, rewards);
					break;
				case EverquartzMessageType.HypnoCoinAdd:
					NPCs.Hypnos.JHypnos.HandleHypnoCoinAddServer();
					break;
				case EverquartzMessageType.HypnosDeparted:
					NPC hypnos = NPCs.Hypnos.JHypnos.Instance;
					if (hypnos != null)
					{
						NPCs.Hypnos.JHypnos.HandleDepartHypnosUniversal(hypnos);
					}
					break;
					//case EverquartzMessageType.EverquartzSyncPlayer:
					//    byte playernumber = reader.ReadByte();
					//    EverquartzPlayer ePlayer = Main.player[playernumber].GetModPlayer<EverquartzPlayer>();
					//    ePlayer.lastSleepingSpot = reader.ReadVector2().ToPoint();
					//    break;
			}
		}

		public override void Load()
		{
			base.Load();
			Instance = this;


			ModCompatibility.censusMod = null;
			ModLoader.TryGetMod("Census", out ModCompatibility.censusMod);
			ModCompatibility.hypnosMod = null;
			ModLoader.TryGetMod("HypnosMod", out ModCompatibility.hypnosMod);

			ModCompatibility.calamityEnabled = ModLoader.HasMod("CalamityMod");
			ModCompatibility.hypnosEnabled = ModLoader.HasMod("HypnosMod");
			ModCompatibility.calRemixEnabled = ModLoader.HasMod("CalRemix");


			//if (ModCompatibility.calamityEnabled)
			//{
			//    CalamityILChanges.Load();
			//}


		}



		public override void Unload()
		{
			base.Unload();

			//if (ModCompatibility.calamityEnabled)
			//{
			//    CalamityILChanges.Unload();
			//}

			ModCompatibility.calamityEnabled = false;
			ModCompatibility.hypnosEnabled = false;
			ModCompatibility.calRemixEnabled = false;

			ModCompatibility.censusMod = null;
			ModCompatibility.hypnosMod = null;



			Instance = null;

		}

		public override object Call(params object[] args)
		{
			if (args == null || args.Length == 0)
			{
				return null;
			}
			if (!(args[0] is string argStr))
			{
				return null;
			}
			switch (argStr)
			{
			//	case "Transmogrification":
			//	case "AddTransmogrification":
			//	case "AddTrans":
			//	case "RegisterTransmogrification":
			//	case "RegisterTrans":
			//		TransmogrificationManager.AddFromModCall(args.Skip(1));
			//		return null;
				default:
					return null;
			}
		}

		private void TryDoCensusSupport()
		{
			Mod censusMod = ModCompatibility.censusMod;
			//if (censusMod != null)
			//{
			//	censusMod.Call("TownNPCCondition", ModContent.NPCType<StarbornPrincess>(), Language.GetTextValue(StarbornPrincess.CensusConditionKey));
			//}
		}

	}


	public class MadhouseSystem : ModSystem
	{


		public override void OnWorldLoad()
		{
			NPCs.Hypnos.JHypnos.hypnoCoins = 0;
			NPCs.Hypnos.JHypnos.timePassed = 0;
			NPCs.Hypnos.JHypnos.spawnTime = Double.MaxValue;
		}
		public override void LoadWorldData(TagCompound tag)
		{
			NPCs.Hypnos.JHypnos.Load(tag.GetCompound("hypnos"));
		}
		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("hypnos", NPCs.Hypnos.JHypnos.Save());
		}

		public override void PreUpdateWorld()
		{
			NPCs.Hypnos.JHypnos.UpdateTravelingMerchant();
		}

		public static List<int> UniqueNPCs => new List<int>() {
			ModContent.NPCType<NPCs.Hypnos.JHypnos>(),
		};

		public override void PreUpdateNPCs()
		{
			UniqueNPCs.ForEach(AntiDupe);
		}



		public static void AntiDupe(int type)
		{
			IEnumerable<NPC> possiblyMultipleDeimi = Main.npc.Where(npc => npc != null && npc.active && npc.type == type);
			if (possiblyMultipleDeimi.Count() > 1)
			{
				possiblyMultipleDeimi.SkipLast(1).ToList().ForEach(npc => { npc.netUpdate = true; npc.active = false; });
			}
		}
	}




	public enum EverquartzMessageType
	{
		HypnosReward, // id, player.whoAmI, rewards(bytes)
		HypnoCoinAdd, // id
		HypnosDeparted, // id
						//EverquartzSyncPlayer // id, player.whoAmI (see EverquartzPlayer.SyncPlayer)
	}

	public static class ModCompatibility
	{
		public static bool calamityEnabled = false;
		public static bool hypnosEnabled = false;
		public static bool calRemixEnabled = false;
		public static Mod censusMod;
		public static Mod hypnosMod;
		private static int? hypnosBossType = null;
		public static int? HypnosBossType
		{
			get
			{
				if (!hypnosBossType.HasValue)
				{
					ModNPC hyNPC = null;
					hypnosMod?.TryFind<ModNPC>("HypnosBoss", out hyNPC);
					hypnosBossType = hyNPC?.Type;

				}
				return hypnosBossType;
			}
			set
			{
				hypnosBossType = value;
			}
		}
	}

	[JITWhenModsEnabled("CalamityMod")]
	internal static partial class CalamityWeakRef
	{

	}

	[JITWhenModsEnabled("Hypnos")]
	internal static partial class HypnosWeakRef
	{

	}

	[JITWhenModsEnabled("CalRemix")]
	internal static partial class CalRemixWeakRef
	{

	}



}