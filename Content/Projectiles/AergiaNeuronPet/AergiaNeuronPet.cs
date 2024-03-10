using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TerramazingGijinkaMadhouse.Content.Buffs.AergiaNeuronPet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace TerramazingGijinkaMadhouse.Content.Projectiles.AergiaNeuronPet
{
	public abstract class AergiaNeuronPet : ModProjectile
	{
		public override string Texture => "TerramazingGijinkaMadhouse/Content/Projectiles/AergiaNeuronPet/AergiaNeuronPet";

		public abstract int BuffType { get; }

		//public abstract int OtherVariantType { get; }

		Vector2 hoveringPos;
		bool hovering;
		float hoveringY;
		int hoveringTime;

		int AergiaIndex { get
			{
				return (int)Projectile.ai[0] - 1;
			}
			set
			{
				Projectile.ai[0] = value + 1;
				Projectile.netUpdate = true;
			}
		}

		//int aergiaIndex = -1;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		// Token: 0x06004568 RID: 17768 RVA: 0x002042DC File Offset: 0x002024DC
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 15;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 3600;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.scale = 0.95f;

			AergiaIndex = -1;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active)
			{
				player.ClearBuff(BuffType);
			}
			if (player.HasBuff(BuffType))
			{
				Projectile.timeLeft = 4;
			}

			//if (AergiaIndex == -1) AergiaIndex =  Main.projectile.Where(proj => proj.active && proj.owner == Projectile.owner && proj.ModProjectile is AergiaNeuronPet).ToList().IndexOf(Projectile);

			if (AergiaIndex == -1) AergiaIndex = Array.IndexOf(Array.FindAll(Main.projectile, proj => proj != null && proj.active && proj.owner == Projectile.owner && proj.ModProjectile is AergiaNeuronPet), Projectile);

			Vector2 idealPos = player.Center;
			idealPos.X -= player.width / 1.5f;
			idealPos.X += -player.direction * 45 * (AergiaIndex+1);
			idealPos.Y -= player.height;

			Vector2 vectorToIdlePosition = idealPos - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();

			float speed = MathHelper.Max(8f, player.velocity.Length() * 1.1f);

			float inertia = 20f;

			if (distanceToIdlePosition > 1000)
			{
				Projectile.Center = idealPos;
			}

			//speed = player.accRunSpeed * speed;

			//Projectile.position = idealPos;

			//if (distanceToIdlePosition > 20f)
			//{
			//	// The immediate range around the player (when it passively floats about)

			//	// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement


			//}
			//else
			//{
			//	Projectile.velocity /= 1.1f;
			//}

			if (hovering)
			{
				Projectile.velocity /= 1.1f;

				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.2f);

				//Projectile.spriteDirection = -player.direction;

				Projectile.frame = 0; // Daedream Pose

				Projectile.position.Y = hoveringY + (float)Math.Sin(++hoveringTime / 10) * 2;

				//hoveringTime++;

				if ((idealPos - hoveringPos).Length() > 20f)
				{
					hovering = false;
					hoveringTime = 0;
				}
			}
			else
			{

				Projectile.frame = 1;

				Projectile.rotation = Projectile.velocity.X * 0.05f;

				if (Projectile.velocity.X != 0f)
				{
					Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
				}

				//if (Projectile.velocity.Y != 0f && Main.rand.NextBool(5))
				//{
				//	Dust.NewDustDirect(new Vector2(Projectile.Center.X, Projectile.Center.Y + Projectile.height / 2), Projectile.width, Projectile.height, DustID.Electric, -Projectile.velocity.X / 2, -Projectile.velocity.Y, 0, default, 0.5f);

				//}

				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;

				if (distanceToIdlePosition < 20f)
				{
					hovering = true;
					hoveringPos = idealPos;
					hoveringY = Projectile.position.Y;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			SpriteEffects spriteEffects = 0;
			if (Projectile.spriteDirection == -1)
			{
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			Color colorArea = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
			Texture2D texture2D3 = ModContent.Request<Texture2D>(this.Texture, AssetRequestMode.AsyncLoad).Value;
			int textureArea = ModContent.Request<Texture2D>(this.Texture, AssetRequestMode.AsyncLoad).Value.Height / Main.projFrames[Projectile.type];
			int y3 = textureArea * Projectile.frame;
			Rectangle rectangle = new Rectangle(0, y3, texture2D3.Width, textureArea);
			Vector2 halfRect = rectangle.Size() / 2f;

			Vector2 drawPos = Projectile.position + Projectile.Size - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

			if (Projectile.isAPreviewDummy)
			{
				

				drawPos.Y += (float)Math.Sin(++hoveringTime / 10) * 2;
				drawPos.Y -= 20;
				drawPos.X += 5;

				Main.EntitySpriteDraw(ModContent.Request<Texture2D>(this.Texture, AssetRequestMode.AsyncLoad).Value, drawPos, new Rectangle?(rectangle), lightColor, Projectile.rotation, halfRect, Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
				return false;
			}


			if (!hovering)
			{
				int eightCompare = 8;
				int twoConst = 2;
				int counter = 1;
				while ((twoConst > 0 && counter < eightCompare) || (twoConst < 0 && counter > eightCompare))
				{
					Color colorAlpha = colorArea;
					colorAlpha = Projectile.GetAlpha(colorAlpha);
					float trailColorChange = eightCompare - counter;
					if (twoConst < 0)
					{
						trailColorChange = 1 - counter;
					}
					colorAlpha *= trailColorChange / (ProjectileID.Sets.TrailCacheLength[Projectile.type] * 1.5f);
					Vector2 oldDrawPos = Projectile.oldPos[counter];
					float projRotate = Projectile.rotation;
					SpriteEffects effects = spriteEffects;
					Main.spriteBatch.Draw(texture2D3, oldDrawPos + Projectile.Size - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), colorAlpha, projRotate + Projectile.rotation * 0f * (counter - 1) * Projectile.spriteDirection, halfRect, Projectile.scale, effects, 0f);
					counter += twoConst;
				}
			}
			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value, Projectile.position + Projectile.Size - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), lightColor, Projectile.rotation, halfRect, Projectile.scale, spriteEffects, 0f);


			Main.spriteBatch.SetBlendState(BlendState.NonPremultiplied);

			Main.spriteBatch.Draw(ModContent.Request<Texture2D>($"{Texture}_Glow", AssetRequestMode.AsyncLoad).Value, Projectile.position + Projectile.Size - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), new Color(1, 1, 1, 0.6f + (float)Math.Sin(Main.GameUpdateCount / 10) * 0.4f), Projectile.rotation, halfRect, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
			return false;
		}
		//}
		//else if (Projectile.velocity == Vector2.Zero)
		//{
		//	// If there is a case where it's not moving at all, give it a little "poke"
		//	Projectile.velocity.X = -0.15f;
		//	Projectile.velocity.Y = -0.05f;
		//}
	}

	public class AergiaNeuronPetVanity : AergiaNeuronPet
	{
		public override int BuffType => ModContent.BuffType<AergiaNeuronMonitoringVanity>();

		//public override int OtherVariantType => ModContent.ProjectileType<AergiaNeuronPetLight>();
	}

	public class AergiaNeuronPetLight : AergiaNeuronPet
	{
		public override int BuffType => ModContent.BuffType<AergiaNeuronMonitoringLight>();

		//public override int OtherVariantType => ModContent.ProjectileType<AergiaNeuronPetVanity>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.LightPet[Projectile.type] = true;
		}

		public override void PostAI()
		{
			base.PostAI();
			Lighting.AddLight(Main.player[Projectile.owner].Center, new Vector3(0.8f, 1, 1) * 5);

			Lighting.AddLight(Projectile.Center, new Vector3(1, 0.2f, 1) * 3);

			if (ModCompatibility.calamityEnabled)
			{
				CalamityWeakRef.AddAbyssLightInAbyss(Main.player[Projectile.owner], 5);
			}
		}
	}
}

