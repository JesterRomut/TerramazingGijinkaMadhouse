using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

namespace TerramazingGijinkaMadhouse.Content.Items.Hypnos
{
	public class HypnoRadioTransmitter: ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 27;
			Item.useAmmo = ModContent.ItemType<Indulgence>();

			Item.useTime = 15;
			base.Item.useAnimation = 15;
			base.Item.useStyle = ItemUseStyleID.Guitar;

			Item.rare = ItemRarityID.Cyan;
		}

		public override bool CanUseItem(Player player)
		{
			return base.CanUseItem(player);
		}

		public override bool? UseItem(Player player)
		{
			return base.UseItem(player);
		}

		public override void AddRecipes()
		{
			base.AddRecipes();

			CreateRecipe().AddIngredient<Indulgence>(77).AddTile(TileID.WorkBenches).Register();
		}
	}
}
