using System.Collections.Generic;
using Terraria.ModLoader;
using System.Linq;
using Terraria;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using System.Collections;
using TerramazingGijinkaMadhouse.Content.NPCs.Hypnos;
using CalamityMod;
using TerramazingGijinkaMadhouse.Common;

namespace TerramazingGijinkaMadhouse
{
	public class TerramazingGijinkaMadhouse : Mod
	{
		public static TerramazingGijinkaMadhouse Instance { get; private set; }




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
			MadhouseMessageType msgType = (MadhouseMessageType)reader.ReadByte();
			switch (msgType)
			{
				case MadhouseMessageType.HypnosReward: // server
													   //Player prayer = Main.player[reader.ReadInt32()];
					Player prayer = Main.player[reader.ReadByte()];
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
					JHypnos.HandleRewardsServer(prayer, rewards);
					break;
				case MadhouseMessageType.HypnoCoinAdd:
					JHypnos.HandleHypnoCoinAddServer();
					break;
				case MadhouseMessageType.HypnosDeparted:
					int hypnos = NPC.FindFirstNPC(ModContent.NPCType<JHypnos>());
					if (hypnos != -1)
					{
						JHypnos.HandleDepartHypnosUniversal(Main.npc[hypnos]);
					}
					break;
				case MadhouseMessageType.HypnosBlessingReceived: // client
					JHypnos.AddBlessingVisuals(Main.player[Main.myPlayer].Center);
					break;
				case MadhouseMessageType.HypnosArrived: // server
					byte player = reader.ReadByte();
					JHypnos.SpawnTravelingMerchant(player == 0? null : Main.player[player - 1]);
					break;
				case MadhouseMessageType.ItemStackAdded:// client to server
														//JHypnos.AddIndulgenceUniversal(Main.player[reader.ReadInt32()]);
					byte player2 = reader.ReadByte();
					byte slot = reader.ReadByte();

					MadhouseUtils.AddItemStackUniversal(Main.player[player2], slot);
					if (Main.netMode == NetmodeID.Server)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte)MadhouseMessageType.ItemStackAdded);
						packet.Write((byte)player2);
						packet.Write((byte)slot);
						packet.Send(-1, player2);
					}
					break;
				case MadhouseMessageType.MadhousePlayerSync:
					byte playerNumber = reader.ReadByte();
					MadhousePlayer examplePlayer = Main.player[playerNumber].Madhouse();
					examplePlayer.ReceivePlayerSync(reader);

					if (Main.netMode == NetmodeID.Server)
					{
						// Forward the changes to the other clients
						examplePlayer.SyncPlayer(-1, whoAmI, false);
					}
					break;
			}
		}

		public override void Load()
		{
			base.Load();
			Instance = this;

			ModCompatibility.Load();
		}
		public override void Unload()
		{
			base.Unload();

			ModCompatibility.Unload();

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
				default:
					return null;
			}
		}

		private void TryDoCensusSupport()
		{
			Mod censusMod = ModCompatibility.censusMod;
			//if (censusMod != null)
			//{
			//}
		}

	}


	public class MadhouseSystem : ModSystem
	{


		public override void OnWorldLoad()
		{
			JHypnos.hypnoCoins = 0;
			JHypnos.timePassed = 0;
			JHypnos.spawnTime = Double.MaxValue;
			//JHypnos.ResetInstance();
		}
		public override void LoadWorldData(TagCompound tag)
		{
			JHypnos.Load(tag.GetCompound("hypnos"));
		}
		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("hypnos", JHypnos.Save());
		}

		public override void PreUpdateWorld()
		{
			JHypnos.UpdateTravelingMerchant();
		}

		//public static List<int> UniqueNPCs => new List<int>() {
		//	ModContent.NPCType<JHypnos>(),
		//};

		public override void PreUpdateNPCs()
		{
			//UniqueNPCs.ForEach(AntiDupe);
		}
	}




	public enum MadhouseMessageType
	{
		HypnosReward, // player.whoAmI, rewards(bytes)
		HypnoCoinAdd, // 
		HypnosDeparted, // 
		HypnosBlessingReceived, // 
		HypnosArrived, // id, calledPlayer: int
		ItemStackAdded, // id, player: int, slot: int
		MadhousePlayerSync
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

		public static void Load()
		{
			censusMod = null;
			ModLoader.TryGetMod("Census", out censusMod);
			hypnosMod = null;
			ModLoader.TryGetMod("HypnosMod", out hypnosMod);

			calamityEnabled = ModLoader.HasMod("CalamityMod");
			hypnosEnabled = ModLoader.HasMod("HypnosMod");
			calRemixEnabled = ModLoader.HasMod("CalRemix");
		}

		public static void Unload()
		{
			calamityEnabled = false;
			hypnosEnabled = false;
			calRemixEnabled = false;

			censusMod = null;
			hypnosMod = null;

			hypnosBossType = null;
		}
	}

	[JITWhenModsEnabled("CalamityMod")]
	internal static partial class CalamityWeakRef
	{
		public static void AddAbyssLightInAbyss(Player p, int add)
		{

			ModCalls.AddAbyssLightStrength(p, add);


		}
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