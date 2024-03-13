using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using TerramazingGijinkaMadhouse.Content.NPCs.Hypnos;
using TerramazingGijinkaMadhouse.Content.Projectiles.Hypnos;

namespace TerramazingGijinkaMadhouse.Content.Items.Hypnos
{
	[LegacyName(["HypnoRadioTransmitter"])]
	public class HypnosCaller : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 27;
			Item.useAmmo = ModContent.ItemType<Indulgence>();

			Item.useTime = 15;
			Item.UseSound = SoundID.Item92;
			base.Item.useAnimation = 15;
			base.Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noMelee = true;

			Item.shoot = ModContent.ProjectileType<AergiaNeuron>();

			Item.rare = ItemRarityID.Cyan;
		}

		public override bool CanUseItem(Player player)
		{
			//JHypnos.Instance?.StrikeInstantKill();
			//return JHypnos.Instance == null;
			return NPC.FindFirstNPC(ModContent.NPCType<JHypnos>()) == -1;
		}

		public override bool? UseItem(Player player)
		{
			return base.UseItem(player);
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			base.UseStyle(player, heldItemFrame);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			JHypnos.SpawnTravelingMerchant(player);

			for (float num = 5f; num <= 18f; num += 3f)
			{
				float fadeIn = Main.rand.NextFloat(0.8f, 1.7f);
				for (int i = 0; i < 31; i++)
				{
					Dust dust = Dust.NewDustPerfect(player.Center, 267);
					dust.velocity = player.velocity + ((float)Math.PI * 2f * (float)i / 60f + (float)Math.PI).ToRotationVector2() * num;
					dust.noGravity = true;
					dust.color = Main.hslToRgb(Main.rand.NextFloat(), 0.7f, 0.625f);
					dust.fadeIn = fadeIn;
					dust.scale = 1.4f;
				}
			}

			return false;
		}

		public override void AddRecipes()
		{
			base.AddRecipes();

			CreateRecipe().AddIngredient<Indulgence>(77).AddTile(TileID.WorkBenches).Register();
		}
	}
}
