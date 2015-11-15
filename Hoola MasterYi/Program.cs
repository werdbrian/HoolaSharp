using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;


namespace HoolaMasterYi
{
    public class Program
    {
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static HpBarIndicator Indicator = new HpBarIndicator();
        private static Spell Q, W, E, R;
        private static bool KsQ { get { return Menu.Item("KsQ").GetValue<bool>(); } }
        private static bool KsT { get { return Menu.Item("KsT").GetValue<bool>(); } }
        private static bool KsB { get { return Menu.Item("KsB").GetValue<bool>(); } }
        private static bool CQ { get { return Menu.Item("CQ").GetValue<bool>(); } }
        private static bool CW { get { return Menu.Item("CW").GetValue<bool>(); } }
        private static bool CE { get { return Menu.Item("CE").GetValue<bool>(); } }
        private static bool CR { get { return Menu.Item("CR").GetValue<bool>(); } }
        private static bool CT { get { return Menu.Item("CT").GetValue<bool>(); } }
        private static bool CY { get { return Menu.Item("CY").GetValue<bool>(); } }
        private static bool CB { get { return Menu.Item("CB").GetValue<bool>(); } }
        private static bool HQ { get { return Menu.Item("HQ").GetValue<bool>(); } }
        private static bool HW { get { return Menu.Item("HW").GetValue<bool>(); } }
        private static bool HE { get { return Menu.Item("HE").GetValue<bool>(); } }
        private static bool HT { get { return Menu.Item("HT").GetValue<bool>(); } }
        private static bool HY { get { return Menu.Item("HY").GetValue<bool>(); } }
        private static bool HB { get { return Menu.Item("HB").GetValue<bool>(); } }
        private static bool LW { get { return Menu.Item("LW").GetValue<bool>(); } }
        private static bool LE { get { return Menu.Item("LE").GetValue<bool>(); } }
        private static bool LI { get { return Menu.Item("LI").GetValue<bool>(); } }
        private static bool JQ { get { return Menu.Item("JQ").GetValue<bool>(); } }
        private static bool JW { get { return Menu.Item("JW").GetValue<bool>(); } }
        private static bool JE { get { return Menu.Item("JE").GetValue<bool>(); } }
        private static bool JI { get { return Menu.Item("JI").GetValue<bool>(); } }
        private static bool AutoY { get { return Menu.Item("AutoY").GetValue<bool>(); } }
        private static bool DQ { get { return Menu.Item("DQ").GetValue<bool>(); } }
        private static bool Dind { get { return Menu.Item("Dind").GetValue<bool>(); } }
        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "MasterYi") return;
            Game.PrintChat("Hoola Master Yi - Loaded Successfully, Good Luck! :)");
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            OnMenuLoad();

            Q.SetTargetted(0.25f, float.MaxValue);
            
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
            Spellbook.OnCastSpell += OnCast;
            Obj_AI_Base.OnDoCast += OnDoCastJC;
            Obj_AI_Base.OnProcessSpellCast += BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += BeforeAttackJC;
            Drawing.OnDraw += OnDraw;
        }

        static void OnDraw(EventArgs args)
        {
            if (DQ) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
        }

        static void BeforeAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !args.Target.IsValid || !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Hero)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (CR) R.Cast();
                    if (CY) CastYoumoo();
                    if (CE) E.Cast();
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (HY) CastYoumoo();
                    if (HE) E.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Minions = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range);
                    if (Minions[0].IsValid && Minions.Count != 0) if (LE) E.Cast(); 
                }
            }
        }

        static void BeforeAttackJC(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !args.Target.IsValid || !Orbwalking.IsAutoAttack(args.SData.Name)) return;
            
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Mobs = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs[0].IsValid && Mobs.Count != 0) if (JE) E.Cast();
                }
            }
        }

        private static void OnCast(Spellbook Sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && AutoY) CastYoumoo();
        }

        static void WCancel()
        {
            var target = Orbwalker.GetTarget();
            Orbwalking.LastAATick = 0;
            if (Orbwalking.InAutoAttackRange(target)) Utility.DelayAction.Add(3,()=>Player.IssueOrder(GameObjectOrder.AttackUnit, Orbwalker.GetTarget()));
            else Utility.DelayAction.Add(3,()=>Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(Game.CursorPos, 50)));
        }

        private static void OnPlay(Obj_AI_Base Sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!Sender.IsMe) return;
            if (args.Animation.Contains("Spell2"))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && CW)
                {
                    WCancel();
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && HW)
                {
                    WCancel();
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && LW)
                {
                    var Minions = MinionManager.GetMinions(Player.AttackRange);
                    if (Minions.Count != 0 && Minions[0].IsValid) WCancel();
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && JW)
                {
                    var Mobs = MinionManager.GetMinions(Player.AttackRange, MinionTypes.All, MinionTeam.Neutral);
                    if (Mobs.Count != 0 && Mobs[0].IsValid) WCancel();
                }
            }
        }

        static void UseCastItem(int t)
        {
            for (int i = 0; i < t; i = i + 1)
            {
                if (HasItem())
                    Utility.DelayAction.Add(i, () => CastItem());
            }
        }
        static void CastItem()
        {

            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
        }
        static void CastYoumoo()
        {
            if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
        }
        static void CastBOTRK(Obj_AI_Hero target)
        {
            if (ItemData.Blade_of_the_Ruined_King.GetItem().IsReady())
                ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
            if (ItemData.Bilgewater_Cutlass.GetItem().IsReady())
                ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
        }
        static bool HasItem()
        {
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                return true;
            }
            return false;
        }
        

        private static void OnDoCast(Obj_AI_Base Sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !args.Target.IsValid && !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Hero && args.Target.IsValid)
            {
                var target = (Obj_AI_Hero)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (CB) CastBOTRK(target);
                    if (CT) UseCastItem(300);
                    if (CW) Utility.DelayAction.Add(1, () => W.Cast());
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (HB) CastBOTRK(target);
                    if (HT) UseCastItem(300);
                    if (HW) Utility.DelayAction.Add(1, () => W.Cast());
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                var Minions = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    if (Minions.Count != 0 && Minions[0].IsValid)
                    {
                        if (LI) UseCastItem(300);
                        if (LW) Utility.DelayAction.Add(1, () => W.Cast());
                    }
                }
            }

        }
        private static void OnDoCastJC(Obj_AI_Base Sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !args.Target.IsValid && !Orbwalking.IsAutoAttack(args.SData.Name)) return;
            
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                var Mobs = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range, MinionTypes.All, MinionTeam.Neutral);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    if (Mobs[0].IsValid && Mobs.Count != 0)
                    {
                        if (Q.IsReady() && JQ) Q.Cast(Mobs[0]);
                        if (!Q.IsReady() || (Q.IsReady() && !JQ))
                        {
                            if (JI) UseCastItem(300);
                            if (JW) Utility.DelayAction.Add(1, () => W.Cast());
                        }
                    }
                }
            }

        }
        private static void OnMenuLoad()
        {
            Menu = new Menu("Hoola Master Yi", "hoolamasteryi", true);

            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);


            var Combo = new Menu("Combo", "Combo");
            Combo.AddItem(new MenuItem("CQ", "Use Q").SetValue(false));
            Combo.AddItem(new MenuItem("CW", "Use W").SetValue(true));
            Combo.AddItem(new MenuItem("CE", "Use E").SetValue(true));
            Combo.AddItem(new MenuItem("CR", "Use R").SetValue(false));
            Combo.AddItem(new MenuItem("CT", "Use Tiamat/Hydra").SetValue(true));
            Combo.AddItem(new MenuItem("CY", "Use Youmoo").SetValue(true));
            Combo.AddItem(new MenuItem("CB", "Use BOTRK").SetValue(false));
            Menu.AddSubMenu(Combo);


            var Harass = new Menu("Harass", "Harass");
            Harass.AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
            Harass.AddItem(new MenuItem("HW", "Use W").SetValue(true));
            Harass.AddItem(new MenuItem("HE", "Use E").SetValue(true));
            Harass.AddItem(new MenuItem("HT", "Use Tiamat/Hydra").SetValue(true));
            Harass.AddItem(new MenuItem("HY", "Use Youmoo").SetValue(true));
            Harass.AddItem(new MenuItem("HB", "Use BOTRK").SetValue(true));
            Menu.AddSubMenu(Harass);

            var Laneclear = new Menu("Laneclear", "Laneclear");
            Laneclear.AddItem(new MenuItem("LW", "Use W").SetValue(false));
            Laneclear.AddItem(new MenuItem("LE", "Use E").SetValue(false));
            Laneclear.AddItem(new MenuItem("LI", "Use Tiamat/Hydra").SetValue(false));
            Menu.AddSubMenu(Laneclear);

            var Jungleclear = new Menu("Jungleclear", "Jungleclear");
            Jungleclear.AddItem(new MenuItem("JQ", "Use Q").SetValue(true));
            Jungleclear.AddItem(new MenuItem("JW", "Use W").SetValue(false));
            Jungleclear.AddItem(new MenuItem("JE", "Use E").SetValue(true));
            Jungleclear.AddItem(new MenuItem("JI", "Use Tiamat/Hydra").SetValue(true));
            Menu.AddSubMenu(Jungleclear);

            var killsteal = new Menu("killsteal", "Killsteal");
            killsteal.AddItem(new MenuItem("KsQ", "Ks Q").SetValue(false));
            killsteal.AddItem(new MenuItem("KsT", "Ks Tiamat/Hydra").SetValue(true));
            killsteal.AddItem(new MenuItem("KsB", "Ks BOTRK").SetValue(true));
            Menu.AddSubMenu(killsteal);

            var Draw = new Menu("Draw", "Draw");
            Draw.AddItem(new MenuItem("Dind", "Draw Damage Indicator").SetValue(true));
            Draw.AddItem(new MenuItem("DQ", "Draw Q").SetValue(true));
            Menu.AddSubMenu(Draw);

            var Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("AutoY", "Use Youmoo While R").SetValue(true));
            Menu.AddSubMenu(Misc);

            Menu.AddToMainMenu();
        }

        static void killsteal()
        {
            if (KsQ && Q.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.IsValid && target.Health < Q.GetDamage(target) && (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") || !target.HasBuff("JudicatorIntervention")) && (!Orbwalking.InAutoAttackRange(target) || !Orbwalking.CanAttack()))
                        Q.Cast(target);
                }
            }
            if (KsB &&
                (ItemData.Bilgewater_Cutlass.GetItem().IsReady() ||
                 ItemData.Blade_of_the_Ruined_King.GetItem().IsReady()))
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget(ItemData.Blade_of_the_Ruined_King.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Damage.GetItemDamage2(Player,target,Damage.DamageItems.Bilgewater)) ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
                    if (target.Health < Damage.GetItemDamage2(Player, target, Damage.DamageItems.Botrk)) ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
                }
            }
            if (KsT &&
                (ItemData.Tiamat_Melee_Only.GetItem().IsReady() ||
                 ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady()))
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget(ItemData.Ravenous_Hydra_Melee_Only.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Damage.GetItemDamage2(Player, target, Damage.DamageItems.Tiamat)) ItemData.Tiamat_Melee_Only.GetItem().Cast();
                    if (target.Health < Damage.GetItemDamage2(Player, target, Damage.DamageItems.Hydra)) ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            killsteal();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && CQ) Combo();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && HQ) Harass();
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && target.IsValid) Q.Cast(target);
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && target.IsValid) Q.Cast(target);
        }
        
        static float getComboDamage2(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;

                if (Q.IsReady())
                    damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);

                if (E.IsReady())
                    damage += E.GetDamage2(enemy);

                if (W.IsReady())
                    damage += (float)Player.GetAutoAttackDamage2(enemy, true);

                if (!Player.IsWindingUp)
                    damage += (float)Player.GetAutoAttackDamage2(enemy, true);

                return damage;
            }
            return 0;
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                if (Dind)
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage2(enemy), new ColorBGRA(255, 204, 0, 160));
                }
                

            }
        }
    }
}