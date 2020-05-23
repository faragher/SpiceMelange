//NOTICE:
//I am an idiot, and in less than a week, I managed to overwrite the source
//for this DLL. This is reconstructed from decompiling the release DLL, and 
//this is exactly why this is going up on GitHub. 
//
//In my defense, I'm coding on Linux and using RDP to run Windows on a remote 
//machine for dnSpy, and Remmina's shared folders are buggy.
//
//But now it's future proofed if I'm unable to continue. Win/Win


using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace BTBSpiceMelange
{
	// Token: 0x02000005 RID: 5
	public class CompProperties_SpiceDryer : CompProperties
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002439 File Offset: 0x00000639
		public CompProperties_SpiceDryer()
		{
			this.compClass = typeof(CompSpiceDryer);
		}

		// Token: 0x04000008 RID: 8
		public float DaystoDry = 1f;
	}

	// Token: 0x02000004 RID: 4
	public class CompSpiceDryer : ThingComp
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002259 File Offset: 0x00000459
		public CompProperties_SpiceDryer Props
		{
			get
			{
				return (CompProperties_SpiceDryer)this.props;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002266 File Offset: 0x00000466
		private CompTemperatureRuinable FreezerComp
		{
			get
			{
				return this.parent.GetComp<CompTemperatureRuinable>();
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000A RID: 10 RVA: 0x00002273 File Offset: 0x00000473
		public bool TemperatureDamaged
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002276 File Offset: 0x00000476
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<float>(ref this.dryingProgress, "dryingProgress", 0f, false);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002294 File Offset: 0x00000494
		public override void CompTick()
		{
			if (this.parent.Map != null && GenTemperature.GetTemperatureForCell(this.parent.Position, this.parent.Map) > this.minimumDryingTemp)
			{
				float num = 1f / (this.Props.DaystoDry * 60000f);
				this.dryingProgress += num;
				if (this.dryingProgress > 1f)
				{
					this.Dry();
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000230C File Offset: 0x0000050C
		public void Dry()
		{
			try
			{
				for (int i = 0; i < this.parent.stackCount; i++)
				{
					Thing thing = ThingMaker.MakeThing(M_DefOf.SpiceMelange, null);
					thing.stackCount = this.parent.stackCount;
					GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
				}
			}
			finally
			{
				this.parent.Destroy(DestroyMode.Vanish);
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002394 File Offset: 0x00000594
		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(this.parent.stackCount + count);
			float b = ((ThingWithComps)otherStack).GetComp<CompSpiceDryer>().dryingProgress;
			this.dryingProgress = Mathf.Lerp(this.dryingProgress, b, t);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000023D7 File Offset: 0x000005D7
		public override void PostSplitOff(Thing piece)
		{
			((ThingWithComps)piece).GetComp<CompSpiceDryer>().dryingProgress = this.dryingProgress;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000023EF File Offset: 0x000005EF
		public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PrePreTraded(action, playerNegotiator, trader);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000023FA File Offset: 0x000005FA
		public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
			base.PostPostGeneratedForTrader(trader, forTile, forFaction);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002405 File Offset: 0x00000605
		public override string CompInspectStringExtra()
		{
			if (!this.TemperatureDamaged)
			{
				return "Drying progress: : " + this.dryingProgress.ToStringPercent();
			}
			return null;
		}

		// Token: 0x04000006 RID: 6
		private float dryingProgress;

		// Token: 0x04000007 RID: 7
		private float minimumDryingTemp = 25f;
	}
	
	// Token: 0x02000003 RID: 3
	public class IncidentWorker_SpiceBlow : IncidentWorker
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002054 File Offset: 0x00000254
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow && this.TryFindRootCell(map, out intVec);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00000298
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!this.TryFindRootCell(map, out intVec))
			{
				return false;
			}
			Thing thing = null;
			int randomInRange = IncidentWorker_SpiceBlow.CountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 root = intVec;
				Map map2 = map;
				int radius = 3;
				IntVec3 intVec2;
				if (!CellFinder.TryRandomClosewalkCellNear(root, map2, radius, out intVec2, null))
				{
					break;
				}
				Plant plant = intVec2.GetPlant(map);
				if (plant != null)
				{
					plant.Destroy(DestroyMode.Vanish);
				}
				Thing thing2 = GenSpawn.Spawn(M_DefOf.PreSpice, intVec2, map, WipeMode.Vanish);
				if (thing == null)
				{
					thing = thing2;
				}
			}
			if (thing == null)
			{
				return false;
			}
			base.SendStandardLetter(parms, thing, new NamedArgument[0]);
			return true;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000213C File Offset: 0x0000033C
		private bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => this.CanSpawnAt(x, map) && x.GetRoom(map, RegionType.Set_Passable).CellCount >= 64, map, out cell);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002178 File Offset: 0x00000378
		private bool CanSpawnAt(IntVec3 c, Map map)
		{
			Log.Message("Trying to spawn on " + c.GetTerrain(map).ToString(), false);
			if (!c.Standable(map) || (c.GetTerrain(map) != TerrainDefOf.Sand && !c.GetTerrain(map).ToString().Contains("SoftSand")))
			{
				Log.Message("Failed spawn on " + c.GetTerrain(map).ToString(), false);
				return false;
			}
			Plant plant = c.GetPlant(map);
			if (plant != null && plant.def.plant.growDays > 10f)
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == ThingDefOf.Plant_Ambrosia)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04000003 RID: 3
		private static readonly IntRange CountRange = new IntRange(10, 20);

		// Token: 0x04000004 RID: 4
		private const int MinRoomCells = 64;

		// Token: 0x04000005 RID: 5
		private const int SpawnRadius = 6;
	}

	// Token: 0x02000002 RID: 2
	[DefOf]
	public static class M_DefOf
	{
		// Token: 0x04000001 RID: 1
		public static ThingDef SpiceMelange;

		// Token: 0x04000002 RID: 2
		public static ThingDef PreSpice;
	}
}
