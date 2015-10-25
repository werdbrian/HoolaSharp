using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;


namespace HoolaLucian
{
    public class Program
    {
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static HpBarIndicator Indicator = new HpBarIndicator();
        private static Spell Q, Q1, W, E;
        private static bool AAPassive;
        private static bool HEXQ { get { return Menu.Item("HEXQ").GetValue<bool>(); } }
        private static bool KillstealQ { get { return Menu.Item("KillstealQ").GetValue<bool>(); } }
        static bool AutoQ { get { return Menu.Item("AutoQ").GetValue<KeyBind>().Active; } }
        private static int MinMana { get { return Menu.Item("MinMana").GetValue<Slider>().Value; } }

        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lucian") return;
            Game.PrintChat("Hoola Lucian - Loaded Successfully, Good Luck! :)");
            Q = new Spell(SpellSlot.Q, 675);
            Q1 = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 1200, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 475f);

            OnMenuLoad();

            Q.SetTargetted(0.25f, 1400f);
            Q1.SetSkillshot(0.5f, 65, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnDoCast += OnDoCast;
        }
        private static void OnMenuLoad()
        {
            Menu = new Menu("Hoola Lucian", "hoolalucian", true);

            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);


            var Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("Nocolision", "Nocolision W").SetValue(true));
            Menu.AddSubMenu(Misc);


            var Harass = new Menu("Harass", "Harass");
            Harass.AddItem(new MenuItem("HEXQ", "Use Extended Q").SetValue(true));
            Menu.AddSubMenu(Harass);

            var Auto = new Menu("Auto", "Auto");
            Auto.AddItem(new MenuItem("AutoQ", "Auto Extended Q (Toggle)").SetValue(new KeyBind('T', KeyBindType.Toggle)));
            Auto.AddItem(new MenuItem("MinMana", "Min Mana (%)").SetValue(new Slider(80)));
            Menu.AddSubMenu(Auto);

            var killsteal = new Menu("killsteal", "Killsteal");
            killsteal.AddItem(new MenuItem("KillstealQ", "Killsteal Q").SetValue(true));
            Menu.AddSubMenu(killsteal);

            Menu.AddToMainMenu();
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;

            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    Utility.DelayAction.Add(5, () => OnDoCastDelayed(args));
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && target.IsValid)
                {
                    Utility.DelayAction.Add(5, () => OnDoCastDelayed(args));
                }
            }
        }

        static void killsteal()
        {
            if (KillstealQ && Q.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Q.GetDamage2(target) && (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention")))
                        Q.Cast(target);
                }
            }
        }
        private static void OnDoCastDelayed(GameObjectProcessSpellCastEventArgs args)
        {
            AAPassive = false;
            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    if (ItemData.Youmuus_Ghostblade.GetItem().IsReady()) ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    if (E.IsReady() && !AAPassive) E.Cast(Game.CursorPos);
                    if (Q.IsReady() && !E.IsReady() && !AAPassive) Q.Cast(target);
                    if (!E.IsReady() && !Q.IsReady() && W.IsReady() && !AAPassive && Orbwalking.CanMove(10)) W.Cast(target.Position);
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && target.IsValid)
                {
                    if (E.IsReady() && !AAPassive) E.Cast(Game.CursorPos);
                    if (Q.IsReady() && !E.IsReady() && !AAPassive) Q.Cast(target);
                    if (!E.IsReady() && !Q.IsReady() && W.IsReady() && !AAPassive) W.Cast(target.Position);
                }
            }
        }

        static void Harass()
        {
            if (Q.IsReady() && HEXQ)
            {
                var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
                if (t1.IsValidTarget(Q1.Range) && Player.Distance(t1.ServerPosition) > Q.Range + 100)
                {
                    var qpred = Q.GetPrediction(t1, true);
                    var distance = Player.Distance(qpred.CastPosition);
                    var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                    foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
                    {
                        if (qpred.CastPosition.Distance(Player.Position.Extend(minion.Position, distance)) < 25)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }
            }
        }

        static void AutoUseQ()
        {
            if (Q.IsReady() && AutoQ && Player.ManaPercent > MinMana)
            {
                var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
                if (t1.IsValidTarget(Q1.Range) && Player.Distance(t1.ServerPosition) > Q.Range + 100)
                {
                    var qpred = Q.GetPrediction(t1, true);
                    var distance = Player.Distance(qpred.CastPosition);
                    var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                    foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
                    {
                        if (qpred.CastPosition.Distance(Player.Position.Extend(minion.Position, distance)) < 25)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            W.Collision = Menu.Item("Nocolision").GetValue<bool>();
            AutoUseQ();
            killsteal();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) Harass();
        }
        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                AAPassive = true;
            }
            if (args.Slot == SpellSlot.R && ItemData.Youmuus_Ghostblade.GetItem().IsReady())
            {
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }

        static float getComboDamage2(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                if (E.IsReady()) damage = damage + (float)Player.GetAutoAttackDamage2(enemy) * 2;
                if (W.IsReady()) damage = damage + W.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy);
                if (Q.IsReady())
                {
                    damage = damage + Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy);
                }
                damage = damage + (float)Player.GetAutoAttackDamage2(enemy);

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
                Indicator.unit = enemy;
                Indicator.drawDmg(getComboDamage2(enemy), new ColorBGRA(255, 204, 0, 160));

            }
        }
    }
}