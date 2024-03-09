using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TerramazingGijinkaMadhouse.Content.Items.Hypnos;

namespace TerramazingGijinkaMadhouse.Content.Items
{
	public class Indulgence : ModItem
	{

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Indulgence");
			//DisplayName.AddTranslation(7, "赎罪券");
			//DisplayName.AddTranslation(6, "Снисхождение");

			//// Tooltip.SetDefault("You are atoned from your sins");
			//Tooltip.AddTranslation(7, "你已经免除了你的罪");
			//Tooltip.AddTranslation(6, "Вы искуплены от своих грехов");
		}
		public override void SetDefaults()
		{
			Item.width = 13;
			Item.height = 23;
			Item.value = 0;
			Item.rare = ItemRarityID.Gray;
			Item.maxStack = 9999;
			Item.ammo = Type;
			Item.consumable = true;
		}
	}
}
