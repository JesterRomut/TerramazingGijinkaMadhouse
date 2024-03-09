using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System;
using static Terraria.Player;
using System.Collections;
using TerramazingGijinkaMadhouse.Content.Projectiles.Hypnos;
using TerramazingGijinkaMadhouse.Content.NPCs.Hypnos;

namespace TerramazingGijinkaMadhouse.Common
{
    public class MadhousePlayer : ModPlayer
    {

        //public int musicBoxTrollAttempt = 0;

        //public Point? lastSleepingSpot = null;


        public override void PostUpdate()
        {
            //if (Player.sleeping.isSleeping)
            //{
            //    lastSleepingSpot = (Player.Bottom + new Vector2(0f, -2f)).ToTileCoordinates();
            //}
            UpdatePraisingHypnos();
        }

        //public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        //{
        //    ModPacket packet = Mod.GetPacket();
        //    packet.Write((byte)EverquartzMessageType.EverquartzSyncPlayer);
        //    packet.Write((byte)Player.whoAmI);
        //    packet.WriteVector2(lastSleepingSpot.GetValueOrDefault().ToVector2());
        //    packet.Send(toWho, fromWho);
        //}

        public override bool PreItemCheck()
        {
            if (IsPraisingHypnos)
            {
                PraisingHypnosAnimation();
                return false;
            }

            return true;
        }

        public override void ResetEffects()
        {
            ResetBuffs();
        }

        public override void UpdateDead()
        {
            ResetBuffs();
        }

        private void ResetBuffs()
        {
            blessed = false;
        }

		public override void ModifyLuck(ref float luck)
		{
			base.ModifyLuck(ref luck);

            if (blessed) luck += 0.5f;
		}

		//public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
		//{
		//    if (npc.Everquartz().mindcrashed > 0)
		//    {
		//        damage -= (int)Math.Floor(damage * 0.1);
		//    }
		//}

		//public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Item, consider using ModifyHitNPC instead */
		//{
		//    if (target.Everquartz().mindcrashed > 0)
		//    {
		//        modifiers.FinalDamage.Base += Player.GetWeaponDamage(item) / 2;
		//        //var debug = Player.GetWeaponDamage(item);
		//        //modifiers.SetCrit();
		//    }
		//}

		//public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Projectile, consider using ModifyHitNPC instead */
		//{
		//    if (target.Everquartz().mindcrashed > 0)
		//    {
		//        modifiers.FinalDamage.Base += proj.damage / 2;
		//    }
		//}


		#region Hypnos

		public int praisingTimer = 0;
        public bool IsPraisingHypnos => praisingTimer > 0;

        public bool blessed = false;

        public void InterruptPraisingHypnos()
        {
            praisingTimer = 0;
        }

        private void PraisingHypnosAnimation()
        {
            int num9 = Player.miscCounter % 14 / 7;
            CompositeArmStretchAmount stretch = CompositeArmStretchAmount.ThreeQuarters;
            float num2 = 0.3f;
            if (num9 == 1)
            {
                //stretch = CompositeArmStretchAmount.Full;
                num2 = 0.35f;
            }

            Player.SetCompositeArmBack(enabled: true, stretch, (float)Math.PI * -2f * num2 * Player.direction);
            Player.SetCompositeArmFront(enabled: true, stretch, (float)Math.PI * -2f * num2 * Player.direction);

        }

        public void DonePraisingHypnos()
        {
            //client side
            NPC hypnos = JHypnos.Instance;
            AergiaNeuron.AddElectricDusts(hypnos != null ? hypnos : Player);

            List<HypnosReward> rewards = JHypnos.GenerateRewards();


            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                JHypnos.HandleRewardsServer(Player, rewards);
            }
            else
            {
                int rewardCount = MadhouseUtils.EnumCount<HypnosReward>();
                //this.Mod.Logger.Info(rewards);
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MadhouseMessageType.HypnosReward);
                packet.Write(Player.whoAmI);
                bool[] rewardBools = new bool[rewardCount];
                rewards.ForEach(reward => rewardBools[(int)reward] = true);
                BitArray bitArray = new BitArray(rewardBools);
                packet.Write(bitArray.ToByteArray());

                packet.Send();
            }

            InterruptPraisingHypnos();
        }



        public void UpdatePraisingHypnos()
        {
            if (!IsPraisingHypnos)
            {
                return;
            }
            if (Player.talkNPC == -1)
            {
                InterruptPraisingHypnos();
                return;
            }
            int num = Math.Sign(Main.npc[Player.talkNPC].Center.X - Player.Center.X);
            if (Player.controlLeft || Player.controlRight || Player.controlUp || Player.controlDown || Player.controlJump || Player.pulley || Player.mount.Active || num != Player.direction)
            {
                InterruptPraisingHypnos();
                return;
            }
            praisingTimer--;
            if (praisingTimer <= 0)
            {
                DonePraisingHypnos();
            }
        }

        #endregion

    }

}