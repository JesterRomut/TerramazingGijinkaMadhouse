using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerramazingGijinkaMadhouse.Content.Projectiles.AergiaNeuronPet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ReLogic.Content;

namespace TerramazingGijinkaMadhouse.Content.Buffs.AergiaNeuronPet
{
	public abstract class AergiaNeuronMonitoring: ModBuff
	{
		public override string Texture => "TerramazingGijinkaMadhouse/Content/Buffs/AergiaNeuronPet/AergiaNeuronMonitoring";

		public abstract int ProjectileType { get; }

		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 18000;
			if (player.ownedProjectileCounts[ProjectileType] <= 0 && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, ProjectileType, 0, 0f, player.whoAmI, 0f, 0f, 0f);
			}
		}

		private Vector2 ClosestPointOnCircle(Vector2 center, float radius, Vector2 p)
		{
			var dx = p.X - center.X;
			var dy = p.Y - center.Y;
			var dist = Math.Sqrt(dx * dx + dy * dy);
			var t = dist == 0 ? 0 : radius / dist;

			Vector2 closestPt = new Vector2(
					(float)(center.X + dx * t),
					(float)(center.Y + dy * t)
				);
			return closestPt;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
		{
			spriteBatch.Draw(ModContent.Request<Texture2D>("TerramazingGijinkaMadhouse/Content/Buffs/AergiaNeuronPet/AergiaNeuronMonitoring_Frame", AssetRequestMode.AsyncLoad).Value, drawParams.Position, drawParams.DrawColor);

			Vector2 mousePos = Main.MouseWorld.ToScreenPosition();

			Vector2 eyePos = ClosestPointOnCircle(drawParams.Position, MathHelper.Clamp((mousePos - drawParams.Position).Length() / 100, 0, 2), mousePos);

			

			spriteBatch.Draw(ModContent.Request<Texture2D>("TerramazingGijinkaMadhouse/Content/Buffs/AergiaNeuronPet/AergiaNeuronMonitoring_Eye", AssetRequestMode.AsyncLoad).Value, eyePos, drawParams.DrawColor);
			return false;
		}
	}

	public class AergiaNeuronMonitoringVanity: AergiaNeuronMonitoring
	{
		public override int ProjectileType => ModContent.ProjectileType<AergiaNeuronPetVanity>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.vanityPet[Type] = true;
		}
	}

	public class AergiaNeuronMonitoringLight: AergiaNeuronMonitoring
	{
		public override int ProjectileType => ModContent.ProjectileType<AergiaNeuronPetLight>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.lightPet[Type] = true;
		}
	}
}
