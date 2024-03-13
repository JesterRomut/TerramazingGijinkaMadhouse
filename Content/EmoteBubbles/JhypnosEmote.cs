using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace TerramazingGijinkaMadhouse.Content.EmoteBubbles
{
	public class JHypnosEmote: ModEmoteBubble
	{
		public override void SetStaticDefaults()
		{
			// Add NPC emotes to "Town" category.
			AddToCategory(EmoteID.Category.Town);
		}
	}
}
