using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;


namespace HoolaTalon
{
    public class Program
    {
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static HpBarIndicator Indicator = new HpBarIndicator();
        private static Spell Q, W, E, R;
        static bool Dind { get { return Menu.Item("Dind").GetValue<bool>(); } }
        static bool DW { get { return Menu.Item("DW").GetValue<bool>(); } }
        static bool DE { get { return Menu.Item("DE").GetValue<bool>(); } }
        static bool DR { get { return Menu.Item("DR").GetValue<bool>(); } }
        static bool KSW { get { return Menu.Item("KSW").GetValue<bool>(); } }
        static bool KSEW { get { return Menu.Item("KSEW").GetValue<bool>(); } }
        static bool KSR { get { return Menu.Item("KSR").GetValue<bool>(); } }
        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Talon") return;
            Game.PrintChat("Hoola Talon - Loaded Successfully, Good Luck! :)");

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 720f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 600f) { Delay = 0.1f, Speed = 902f * 2 };

            W.SetSkillshot(0.25f, 52f * (float)Math.PI / 180, 902f * 2, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.25f, float.MaxValue);

            Game.OnUpdate += OnTick;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Spellbook.OnCastSpell += OnCast;
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;

            OnMenuLoad();
        }

        private static void OnDraw(EventArgs args)
        {
            if(DR)Render.Circle.DrawCircle(Player.Position, E.Range + W.Range, E.IsReady() && W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if(DW)Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if(DE)Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
        }

        private static void OnCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.E)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && args.Target is Obj_AI_Hero) CastYoumoo();
            }
            if (args.Slot == SpellSlot.Q) Orbwalking.LastAATick = 0;
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (args.Target is Obj_AI_Hero)
                {
                    var target = (Obj_AI_Hero) args.Target;
                    if (!target.IsDead)
                    {
                        if (Q.IsReady()) Q.Cast();
                        UseCastItem(300);
                        if (!Q.IsReady()) W.Cast(target.ServerPosition);
                    }
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (args.Target is Obj_AI_Minion)
                {
                    var target = (Obj_AI_Minion)args.Target;
                    if (!target.IsDead)
                    {
                        if (Q.IsReady()) Q.Cast();
                        UseCastItem(300);
                    }
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            Killsteal();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) Combo();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) Harass();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) LaneClear();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) JungleClear();
        }

        private static void Killsteal()
        {
            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && Player.Distance(x.ServerPosition) <= E.Range + W.Range && !x.IsZombie && !x.IsDead);
            if (W.IsReady() && KSW)
            {
                foreach (var target in targets)
                {
                    if (target.Health <= W.GetDamage2(target) * 2)
                    {
                        if (Player.Distance(target.ServerPosition) < W.Range && Player.Mana >= W.ManaCost)
                        {
                            W.Cast(target.ServerPosition);
                        }
                        else if (E.IsReady() && Player.Distance(target.ServerPosition) > W.Range && Player.Mana >= E.ManaCost + W.ManaCost && KSEW)
                        {
                            var minions = MinionManager.GetMinions(E.Range);
                            if (minions.Count != 0)
                            {
                                foreach (var minion in minions)
                                {
                                    if (minion.IsValidTarget(E.Range) &&
                                        minion.Distance(target.ServerPosition) <= W.Range)
                                    {
                                        Game.ShowPing(PingCategory.Normal, minion.Position);
                                        E.Cast(minion);
                                        W.Cast(target.ServerPosition);
                                    }

                                }
                            }
                            if (minions.Count == 0)
                            {
                                var heros = HeroManager.Enemies;
                                if (heros.Count != 0)
                                {
                                    foreach (var hero in heros)
                                    {
                                        if (hero.IsValidTarget(E.Range) &&
                                            hero.Distance(target.ServerPosition) <= W.Range)
                                        {
                                            Game.ShowPing(PingCategory.Normal, hero.Position);
                                            E.Cast(hero);
                                            W.Cast(target.ServerPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (!Orbwalker.InAutoAttackRange(target) && E.IsReady()) E.Cast(target);
            if (!E.IsReady() && !Orbwalker.InAutoAttackRange(target) && Player.Distance(target) <= W.Range) W.Cast(target.ServerPosition);
            if (target.Health < R.GetDamage2(target) && Player.Distance(target.Position) <= R.Range - 50 && KSR) R.Cast();
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget(W.Range)) W.Cast(target.ServerPosition);
        }

        private static void JungleClear()
        {
            var Mobs = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (W.IsReady())
            {

                List<Vector2> minionVec2List = new List<Vector2>();

                foreach (var Mob in Mobs)
                    minionVec2List.Add(Mob.ServerPosition.To2D());

                var MaxHit = MinionManager.GetBestCircularFarmLocation(minionVec2List, 200f, W.Range);

                if (MaxHit.MinionsHit >= 1)
                    W.Cast(MaxHit.Position);
            }
        }

        private static void LaneClear()
        {
            var Minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (W.IsReady())
            {

                List<Vector2> minionVec2List = new List<Vector2>();

                foreach (var Minion in Minions)
                    minionVec2List.Add(Minion.ServerPosition.To2D());

                var MaxHit = MinionManager.GetBestCircularFarmLocation(minionVec2List, 200f, W.Range);
                    if (MaxHit.MinionsHit >= 3 && W.IsReady()) W.Cast(MaxHit.Position);
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
        static bool HasItem()
        {
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                return true;
            }
            return false;
        }
        
        
        private static void OnMenuLoad()
        {
            Menu = new Menu("Hoola Talon", "hoolatalon", true);

            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);


            var Draw = new Menu("Draw", "Draw");
            Draw.AddItem(new MenuItem("Dind", "Draw Damage Indicator").SetValue(true));
            Draw.AddItem(new MenuItem("DW", "Draw W Range").SetValue(true));
            Draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(true));
            Draw.AddItem(new MenuItem("DR", "Draw E W Killsteal Range").SetValue(true));
            Menu.AddSubMenu(Draw);


            var Killsteal = new Menu("Killsteal", "Killsteal");
            Killsteal.AddItem(new MenuItem("KSW", "Killsteal W").SetValue(true));
            Killsteal.AddItem(new MenuItem("KSEW", "Killsteal EW").SetValue(true));
            Killsteal.AddItem(new MenuItem("KSR", "Killsteal R (While Combo Only)").SetValue(true));
            Menu.AddSubMenu(Killsteal);

            Menu.AddToMainMenu();
        }

        private static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                if (Q.IsReady()) damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy);
                if (W.IsReady()) damage += W.GetDamage2(enemy) * 2;
                if (R.IsReady()) damage += R.GetDamage2(enemy) * 2;
                if (HasItem()) damage += (float)Player.GetAutoAttackDamage2(enemy) * 0.7f;

                return damage;
            }
            return 0;
        }
        private static void OnEndScene(EventArgs args)
        {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                if (Dind)
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage(enemy), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }

            }
        }
    }
}