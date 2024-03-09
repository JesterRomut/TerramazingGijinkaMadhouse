using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TerramazingGijinkaMadhouse.Content.Buffs.AergiaNeuronPet;
using TerramazingGijinkaMadhouse.Content.NPCs.Hypnos;
using TerramazingGijinkaMadhouse.Content.Projectiles.AergiaNeuronPet;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerramazingGijinkaMadhouse.Content.Items.AergiaNeuronPet
{
	public abstract class AergiaCaller: ModItem
	{
		//public override string Texture => "TerramazingGijinkaMadhouse/Content/Items/Indulgence";

		public abstract int ProjectileType { get; }
		public abstract int BuffType { get; }

		public abstract int RightClickTransform { get; }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.ZephyrFish);
			Item.damage = 0;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.width = 13;
			Item.height = 23;
			Item.UseSound = SoundID.NPCHit9;
			Item.shoot = ProjectileType;
			Item.buffType = BuffType;
			Item.value = Item.buyPrice(0, 4, 0, 0);
			Item.rare = ItemRarityID.Orange;
			Item.consumable = false;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
			{
				player.AddBuff(Item.buffType, 3600, true, false);
			}
		}

		public override void RightClick(Player player)
		{
			//if (player.HasBuff(BuffType)) return;
			//player.PutItemInInventoryFromItemUsage(RightClickTransform);
			//Item.TurnToAir();

			Item.SetDefaults(RightClickTransform);
			//Item.stack = 1;

			//Main.mouseItem.SetDefaults(RightClickTransform);
			


			//JHypnos.neurons = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }; 
			//JHypnos.Instance?.StrikeInstantKill();
			//JHypnos.Instance.StrikeInstantKill();
		}

		public override bool ConsumeItem(Player player)
		{
			return false;
		}

		public override bool CanRightClick()
		{
			return true;
		}
	}

	public class AergiaCallerVanity: AergiaCaller
	{
		public override int ProjectileType => ModContent.ProjectileType<AergiaNeuronPetVanity>();
		public override int BuffType => ModContent.BuffType<AergiaNeuronMonitoringVanity>();

		public override int RightClickTransform => ModContent.ItemType<AergiaCallerLight>();

		public override void AddRecipes()
		{
			base.AddRecipes();

			CreateRecipe().AddIngredient(ModContent.ItemType<Indulgence>(), 12)
				.AddIngredient(ItemID.Wire, 77)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class AergiaCallerLight: AergiaCaller {
		public override int ProjectileType => ModContent.ProjectileType<AergiaNeuronPetLight>();
		public override int BuffType => ModContent.BuffType<AergiaNeuronMonitoringLight>();

		public override int RightClickTransform => ModContent.ItemType<AergiaCallerVanity>();

		public override void AddRecipes()
		{
			base.AddRecipes();

			//CreateRecipe(1).AddIngredient(ModContent.ItemType<Indulgence>(), 20);
		}
	}
}
