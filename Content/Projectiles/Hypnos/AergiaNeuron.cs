using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Utilities;
using Terraria.ID;
using Terraria.Audio;
using CalamityMod.Buffs.StatDebuffs;
using TerramazingGijinkaMadhouse.Content.NPCs.Hypnos;
using System.IO;

namespace TerramazingGijinkaMadhouse.Content.Projectiles.Hypnos
{
    internal static partial class CalamityWeakRef
    {
        internal static int MarkedForDeathBuff => ModContent.BuffType<MarkedforDeath>();
        internal static int KamiFluBuff => ModContent.BuffType<KamiFlu>();
        internal static int ArmorCrunchBuff => ModContent.BuffType<ArmorCrunch>();
    }

    public class AergiaNeuron : ModProjectile
    {
        #region ExtraAssets


        public static string NOTE => "The aergia neuron and blue exo laser's sprites can both be found on fandom";
        public static readonly Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("TerramazingGijinkaMadhouse/Content/Projectiles/Hypnos/AergiaNeuron_Glow");
        public static readonly Asset<Texture2D> glowRedTex = ModContent.Request<Texture2D>("TerramazingGijinkaMadhouse/Content/Projectiles/Hypnos/AergiaNeuron_GlowRed");
        //public static readonly Asset<Texture2D> tubeTex = ModContent.Request<Texture2D>("TerramazingGijinkaMadhouse/Projectiles/Hypnos/HypnosPlugCable");
        #endregion

        #region Consts
        public static readonly int laserTimer = 60;
        public static readonly short refreshTimeLeft = 300;
        public static readonly int alphaChange = 5;
        public static readonly int laserSpeed = 20;
        #endregion

        #region BuffInfo
        public static List<int> VanillaDebuffs => new List<int>()
        {
            BuffID.Ichor,
            BuffID.BetsysCurse
            //ModContent.BuffType<Mindcrashed>(),
        };

        public static List<int> CalamityDebuffs => new List<int>()
        {
                CalamityWeakRef.MarkedForDeathBuff,
            CalamityWeakRef.ArmorCrunchBuff,
                   CalamityWeakRef.KamiFluBuff
        };

        public static List<int> Debuffs => (ModCompatibility.calamityEnabled ? VanillaDebuffs.Union(CalamityDebuffs) : VanillaDebuffs).ToList();

        //public static int Debuff => ModContent.BuffType<Mindcrashed>();

        public static readonly int buffDuration = 300;

        #endregion

        #region Fields

        bool landed = false;
        public bool Landed
        {
            get
            {
                return landed;
            }
            set
            {
                landed = value;
            }
        }

        public int ShootCooldown
        {
            get
            {
                return (int)Projectile.ai[1];
            }
            set
            {
                Projectile.ai[1] = value;
            }
        }

        int AergiaIndex { get {

                return (int)Projectile.ai[0];

			}
        }

        short guaranteedLandedTime = 0;

		short timeLeft = refreshTimeLeft;
		#endregion

		#region Utils
		//public static List<Projectile> AllNeurons => Main.projectile.Where(proj => proj != null && proj.active && proj.type == ModContent.ProjectileType<AergiaNeuron>()).ToList();

        public static int CalcDamage(NPC target) => (int)Math.Floor((float)target.lifeMax / (target.boss ? 96000 : 6));
        public static void AddElectricDusts(Entity proj, int count = 3) // hey hypons or whatever your name u coded quite a lot maybe it is time to stop that is not healthy for you, i can tell by myself
        {
            if (Main.dedServ) return;
            for (int i = 0; i < count; i++)
            {
                Dust.NewDust(proj.position, proj.width, proj.height, DustID.Electric);
            }
        }

        public void AddElectricDusts()
        {
            AddElectricDusts(Projectile);
        }
        #endregion


        #region Overrides
        public override void SetStaticDefaults()
        {
            // base.DisplayName.SetDefault("Aergia Neuron");
            //DisplayName.AddTranslation(7, "埃吉亚神经元");
            //DisplayName.AddTranslation(7, "Нейрон Агерии");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.damage = 1;
			Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.npcProj = true;
            Projectile.DamageType = DamageClass.MagicSummonHybrid;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0;
            Projectile.timeLeft = refreshTimeLeft;

            timeLeft = refreshTimeLeft;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //damage = CalcDamage(target);
            //crit = true;
            modifiers.FinalDamage.Flat = CalcDamage(target);
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D sprite = Landed ? glowTex.Value : glowRedTex.Value;
            float originOffsetX = (sprite.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f + DrawOriginOffsetX;

            Rectangle frame = new Rectangle(0, 0, sprite.Width, sprite.Height);

            Vector2 origin = new Vector2(originOffsetX, Projectile.height / 2 - DrawOriginOffsetY);

            Color color = Color.White;
            color.A = (byte)(255 - Projectile.alpha);

			Main.spriteBatch.SetBlendState(BlendState.NonPremultiplied);


			Main.EntitySpriteDraw(sprite, Projectile.position - Main.screenPosition + new Vector2(originOffsetX + DrawOffsetX, Projectile.height / 2 + Projectile.gfxOffY), (Rectangle?)frame, color, Projectile.rotation, origin, Projectile.scale, default, 0);

			Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
            writer.Write(guaranteedLandedTime);
            writer.Write(timeLeft);

			base.SendExtraAI(writer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
            guaranteedLandedTime = reader.ReadInt16();
            timeLeft = reader.ReadInt16();

			base.ReceiveExtraAI(reader);
		}

		#endregion

		#region AI
		public override void AI()
        {
            NPC hypnos = JHypnos.Instance;

            if (hypnos == null)
            {
                Projectile.Kill();
                return;
            }

            //if (AergiaIndex == -1)
            //{
            if (AergiaIndex >= 12)
            {
                Projectile.Kill();
                return;
            }

            if (--timeLeft <= 0)
            {
				Projectile.Kill();
				return;
			}

			//    AergiaIndex = AllNeurons.Count;
			//}
			Projectile.timeLeft = 10;

			int aergiaIndex = AergiaIndex;

            int neuronCount = 12;
            float offset = Main.GlobalTimeWrappedHourly * 80;

            double rad6 = (double)(360f / neuronCount * aergiaIndex + offset) * (Math.PI / 180.0);
            double dist4 = 200.0;
            float hyposx4 = hypnos.Center.X - (int)(Math.Cos(rad6) * dist4) - Projectile.width / 2;
            float hyposy4 = hypnos.Center.Y - (int)(Math.Sin(rad6) * dist4) - Projectile.height / 2;

            float dist = Vector2.Distance(
                Projectile.Center,
                ((float)Math.PI * 2f * aergiaIndex / neuronCount + offset).ToRotationVector2() * (float)dist4 + hypnos.Center
                );

            if (Landed == false && (dist < 20f  || guaranteedLandedTime > 100))
            {
                Landed = true;
                AddElectricDusts();
                Projectile.netUpdate = true;
            }else if (guaranteedLandedTime <= 100)   
            {
                guaranteedLandedTime++;
            }

            float idealx8;
            float idealy8;
            if (!Landed)
            {
                idealx8 = MathHelper.Lerp(Projectile.position.X, hyposx4, 0.4f);
                idealy8 = MathHelper.Lerp(Projectile.position.Y, hyposy4, 0.4f);
            }
            else
            {
                idealx8 = MathHelper.Lerp(Projectile.position.X, hyposx4, 0.8f);
                idealy8 = MathHelper.Lerp(Projectile.position.Y, hyposy4, 0.8f);

                //int targetin = -1;
                //Projectile.Minion_FindTargetInRange(800, ref targetin, false);
                //NPC target = Main.npc[targetin]; 
                NPC target = Projectile.Center.NearestEnemy(800f);
                if (target != null && target.active)
                {
					//CombatText.NewText(Projectile.Hitbox, Color.White, $"{target.FullName} {target.CanBeChasedBy()} {target.chaseable}");
					timeLeft = refreshTimeLeft;
                    if (ShootCooldown > 0)
                    {
                        ShootCooldown--;
                    }
                    else
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.SafeDirectionTo(target.Center) * laserSpeed, ModContent.ProjectileType<BlueExoPulseLaser>(), 1, 0, 0, target.whoAmI);
                        SoundStyle style = JHypnos.IPutTheSoundFileInLocalBecauseICouldntKnowCalamitysPathOfThis;
                        style.Volume = JHypnos.IPutTheSoundFileInLocalBecauseICouldntKnowCalamitysPathOfThis.Volume - 0.1f;
                        SoundEngine.PlaySound(in style, Projectile.Center);
                        ShootCooldown = laserTimer;

                    }

                }
            }

			
			Projectile.position = new Vector2(idealx8, idealy8);

            if (timeLeft < 200)
            {
                Projectile.alpha = Math.Min(Projectile.alpha + alphaChange, 255);
            }
            else
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha = Math.Max(Projectile.alpha - alphaChange, 0);
                }
            }

            if (Projectile.alpha >= 255)
            {
				Projectile.netUpdate = true;
				Projectile.Kill();
                return;
            }




        }

		public override bool PreKill(int timeLeft)
		{

			//JHypnos.neurons[AergiaIndex] = -1;
   //         Projectile.netUpdate = true;

			return base.PreKill(timeLeft);
		}

		public override void OnKill(int timeLeft)
		{
			JHypnos.neurons[AergiaIndex] = -1;
			Projectile.netUpdate = true;

			base.OnKill(timeLeft);

		}
		#endregion




	}

    public class BlueExoPulseLaser : ModProjectile
    {
        #region Fields
        public int TargetInt => (int)Projectile.ai[0];
        public NPC Target
        {
            get
            {
                NPC npc = Main.npc.ElementAtOrDefault((int)Projectile.ai[0]);
                if (npc != null && npc.active && npc.CanBeChasedBy() && npc.chaseable)
                {
                    return npc;
                }
                return null;
            }
        }
        #endregion

        #region Overrides
        public override void SetStaticDefaults()
        {
            // base.DisplayName.SetDefault("Blue Exo Pulse Laser");
            //DisplayName.AddTranslation(7, "蓝色星流脉冲激光");
            //DisplayName.AddTranslation(6, "Синий Экзо-Пульсовой Лазер");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 480;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.npcProj = true;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0;
            Projectile.DamageType = DamageClass.MagicSummonHybrid;
        }



        public override void OnKill(int timeLeft)
        {
            AergiaNeuron.AddElectricDusts(Projectile, 1);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //damage = AergiaNeuron.CalcDamage(target);
            modifiers.FinalDamage.Flat += AergiaNeuron.CalcDamage(target);
            //crit = true;
            AergiaNeuron.Debuffs.ForEach(buff => { target.AddBuff(buff, AergiaNeuron.buffDuration); });

            NPC target2 = Projectile.Center.NearestEnemy(800f);
            if (target2 != null && target2.whoAmI != TargetInt)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Projectile.SafeDirectionTo(target2.Center) * AergiaNeuron.laserSpeed, ModContent.ProjectileType<BlueExoPulseLaser>(), 1, 0, 0, target.whoAmI);



            }
        }
        #endregion

        #region AI
        public override void AI()
        {
            

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }
            Projectile.alpha -= 30;
            Lighting.AddLight(Projectile.Center, 0f, 0f, 0.6f);
            NPC target = Target;
            if (target == null)
            {
                if (Projectile.timeLeft > 60)
                {
                    Projectile.timeLeft = 60;
                }

            }
            else
            {
                Projectile.velocity = Projectile.SafeDirectionTo(target.Center) * AergiaNeuron.laserSpeed;
            }
            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(0.0 - Projectile.velocity.Y, 0.0 - Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }
            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha += 10;
            }
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
        }
        #endregion
    }
}
