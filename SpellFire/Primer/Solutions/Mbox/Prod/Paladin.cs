﻿using SpellFire.Well.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpellFire.Primer.Gui;

namespace SpellFire.Primer.Solutions.Mbox.Prod
{
	public partial class ProdMbox : MultiboxSolution
	{
		private class Paladin : Solution
		{
			private ProdMbox mbox;

			private static string PaladinBuffsForClass(UnitClass unitClass)
			{
				return unitClass switch
				{
					UnitClass.Paladin => "Blessing of Sanctuary",
					_ => "Blessing of Kings"
				};
			}

			private static readonly string[] PartyBuffs = { };
			private static readonly string[] SelfBuffs =
			{
				//"Seal of Light",
				"Righteous Fury"
			};
			public Paladin(Client client, ProdMbox mbox) : base(client)
			{
				this.mbox = mbox;

				const int viewDistanceMax = 1250;
				me.ExecLua($"SetCVar('farclip', {viewDistanceMax})");
			}

			public override void Tick()
			{
				Thread.Sleep(ProdMbox.ClientSolutionSleepMs);
				me.RefreshLastHardwareEvent();

				if (me.CastPrioritySpell())
				{
					return;
				}

				if (!me.GetObjectMgrAndPlayer())
				{
					return;
				}

				if (!mbox.masterAI)
				{
					return;
				}

				LootAround(me);

				if (me.IsOnCooldown("Seal of Light")) /* global cooldown check */
				{
					return;
				}

				if (mbox.buffingAI)
				{
					BuffUp(me, mbox, PartyBuffs, SelfBuffs, PaladinBuffsForClass);
				}

				long targetGuid = me.GetTargetGUID();
				if (targetGuid == 0)
				{
					return;
				}
				GameObject target = me.ObjectManager.FirstOrDefault(obj => obj.GUID == targetGuid);
				bool validTarget = target != null
				             && target.Health > 0
				             && me.ControlInterface.remoteControl
					             .CGUnit_C__UnitReaction(me.Player.GetAddress(), target.GetAddress()) <= UnitReaction.Neutral;
				if (!validTarget)
				{
					return;
				}

				if (me.Player.GetDistance(target) > MeleeAttackRange || me.Player.IsMounted())
				{
					return;
				}
				else
				{
					FaceTowards(me, target);
				}

				if (!me.Player.IsCastingOrChanneling())
				{
					if (!me.Player.IsAutoAttacking())
					{
						me.ExecLua("AttackTarget()");
					}

					if (mbox.complexRotation)
					{
						if (!me.IsOnCooldown("Hammer of Wrath") && target.HealthPct < 20)
						{
							me.CastSpell("Hammer of Wrath");
						}

						if (!me.IsOnCooldown("Judgement of Light"))
						{
							me.CastSpell("Judgement of Light");
						}

						if (!me.IsOnCooldown("Hammer of the Righteous"))
						{
							me.CastSpell("Hammer of the Righteous");
						}

						if (!me.IsOnCooldown("Shield of Righteousness"))
						{
							me.CastSpell("Shield of Righteousness");
						}

						bool isHSUp = me.HasAura(me.Player, "Holy Shield", null);
						if (!isHSUp)
						{
							me.CastSpell("Holy Shield");
						}
					}
					else
					{
						if (!me.IsOnCooldown("Hammer of the Righteous"))
						{
							me.CastSpell("Hammer of the Righteous");
						}

						if (!me.IsOnCooldown("Shield of Righteousness"))
						{
							me.CastSpell("Shield of Righteousness");
						}
					}
				}
			}

			public override void RenderRadar(RadarCanvas radarCanvas, Bitmap radarBackBuffer)
			{
				if (mbox.masterAI && mbox.radarOn)
				{
					base.RenderRadar(radarCanvas, radarBackBuffer);
				}
				else
				{
					Thread.Sleep(ProdMbox.ClientSolutionSleepMs);
				}

			}

			public override void Dispose()
			{
				me.LuaEventListener.Dispose();
			}
		}
	}
}
