using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;
using System.Collections.Generic;


namespace WizzCaitlyn
{
    class Program
    {

        // declare shorthandle to access the player object
        // Properties http://msdn.microsoft.com/en-us/library/aa288470%28v=vs.71%29.aspx 
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        // declare  list of spells
        private static Spell Q, W, E, R;

        //orwalk
        private static Orbwalking.Orbwalker Orbwalker;

        // declare list of items
        private static Items.Item Bork;

        // declare menu
        private static Menu Menu;

        static void Main(string[] args)
        {
            // OnGameLoad event, gets fired after loading screen is over
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Caitlyn")
                return;

            Game.PrintChat("<font color='#FF0000'>WizzCaitlyn</font> loaded. - <font color='#5882FA'>Bruqaj</font>");


            Q = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 2000);

            Q.SetSkillshot(0.5f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);

            Bork = new Items.Item(3153, 450);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu.AddItem(new MenuItem("UltHelp", "Ult Target on R").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));

            // create submenu for TargetSelector used by Orbwalker
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));


            spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            spellMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            spellMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));


            Menu.AddToMainMenu();

            // subscribe to Drawing event
            Drawing.OnDraw += Drawing_OnDraw;

            // subscribe to Update event gets called every game update around 10ms
            Game.OnGameUpdate += Game_OnGameUpdate;

            //gapcloser
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            //trap
            GameObject.OnCreate += Trap_OnCreate;

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            R.Range = 500 * R.Level + 1500;


            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Peacemaker();
                YordleSnapTrap();
                CaliberNet();
            }

            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Peacemaker();
                YordleSnapTrap();
            }


            AceInTheHole();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if(Q.IsReady())
                Utility.DrawCircle(Player.Position, Q.Range, Color.ForestGreen);
            else if(!Q.IsReady())
                Utility.DrawCircle(Player.Position, Q.Range, Color.DarkRed);

            if (R.IsReady())
                Utility.DrawCircle(Player.Position, R.Range, Color.ForestGreen);
            else if (!R.IsReady())
                Utility.DrawCircle(Player.Position, R.Range, Color.DarkRed);
        }

        private static void Peacemaker()
        {
            if (!Menu.Item("useQ").GetValue<bool>())
                return;

            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (Q.IsReady() && target.IsValidTarget() && !Orbwalking.InAutoAttackRange(target))
                Q.CastOnUnit(target, true);
        }


        private static void CaliberNet()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(E.Range) && enemy.Distance((AttackableUnit)ObjectManager.Player) <= enemy.BoundingRadius + enemy.AttackRange + ObjectManager.Player.BoundingRadius && enemy.IsMelee())
                {
                    E.Cast(enemy);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }


        private static void YordleSnapTrap()
        {
            List<Obj_AI_Hero> enemBuffed = getEnemiesBuffs();
            foreach (Obj_AI_Hero enem in enemBuffed)
            {
                if (W.IsReady() && Menu.Item("useW").GetValue<bool>())
                {
                    W.CastOnUnit(enem, true);
                }

            }
        }

        public static List<Obj_AI_Hero> getEnemiesBuffs()
        {
            List<Obj_AI_Hero> enemBuffs = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero enem in ObjectManager.Get<Obj_AI_Hero>().Where(enem => enem.IsEnemy))
            {
                foreach (BuffInstance buff in enem.Buffs)
                {
                    if (buff.Name == "zhonyasringshield" || buff.Name == "caitlynyordletrapdebuff" || buff.Name == "powerfistslow" || buff.Name == "aatroxqknockup" || buff.Name == "ahriseducedoom" ||
                        buff.Name == "CurseoftheSadMummy" || buff.Name == "braumstundebuff" || buff.Name == "braumpulselineknockup" || buff.Name == "rupturetarget" || buff.Name == "EliseHumanE" ||
                        buff.Name == "HowlingGaleSpell" || buff.Name == "jarvanivdragonstrikeph2" || buff.Name == "karmaspiritbindroot" || buff.Name == "LuxLightBindingMis" || buff.Name == "lissandrawfrozen" ||
                        buff.Name == "lissandraenemy2" || buff.Name == "unstoppableforceestun" || buff.Name == "maokaiunstablegrowthroot" || buff.Name == "monkeykingspinknockup" || buff.Name == "DarkBindingMissile" ||
                        buff.Name == "namiqdebuff" || buff.Name == "nautilusanchordragroot" || buff.Name == "RunePrison" || buff.Name == "SonaR" || buff.Name == "sejuaniglacialprison" || buff.Name == "swainshadowgrasproot" ||
                        buff.Name == "threshqfakeknockup" || buff.Name == "VeigarStun" || buff.Name == "velkozestun" || buff.Name == "virdunkstun" || buff.Name == "viktorgravitonfieldstun" || buff.Name == "yasuoq3mis" ||
                        buff.Name == "zyragraspingrootshold" || buff.Name == "zyrabramblezoneknockup" || buff.Name == "katarinarsound" || buff.Name == "lissandrarself" || buff.Name == "AlZaharNetherGrasp" || buff.Name == "Meditate" ||
                        buff.Name == "missfortunebulletsound" || buff.Name == "AbsoluteZero" || buff.Name == "pantheonesound" || buff.Name == "VelkozR" || buff.Name == "infiniteduresssound" || buff.Name == "chronorevive" ||
                        buff.Type == BuffType.Suppression || buff.Name == "aatroxpassivedeath" || buff.Name == "zacrebirthstart")
                    {
                        enemBuffs.Add(enem);
                        break;
                    }
                }
            }
            return enemBuffs;
        }

        private static void Trap_OnCreate(LeagueSharp.GameObject Trap, EventArgs args)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.W) != SpellState.Ready || !Menu.Item("useW").GetValue<bool>())
                return;

            // Teleport
            if (Trap.Name.Contains("GateMarker_red") || Trap.Name == "Pantheon_Base_R_indicator_red.troy" || Trap.Name.Contains("teleport_target_red") ||
                Trap.Name == "LeBlanc_Displacement_Yellow_mis.troy" || Trap.Name == "Leblanc_displacement_blink_indicator_ult.troy" || Trap.Name.Contains("Crowstorm"))
            {
                if (Trap.IsEnemy)
                {

                    var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(enemy => enemy.IsEnemy && enemy.Distance(Trap.Position) < W.Range);
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, target);

                }
            }
            
        }

        private static void AceInTheHole()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (Menu.Item("useR").GetValue<bool>())
            {
                if (R.IsReady() && target.IsValidTarget())
                {
                    if (target.Health < R.GetDamage(target) && Orbwalking.InAutoAttackRange(target))
                    {
                        R.CastOnUnit(target, true);
                    }
                }
            }
            else
            {
                if (R.IsReady() && target.IsValidTarget())
                {
                    if (target.Health < R.GetDamage(target))
                    {
                        //from Marksman(Caitlyn.cs)
                        var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                        Drawing.DrawText(playerPos.X - 65, playerPos.Y + 20, Color.Yellow, "Hit R To kill: " + target.Name + "!");

                        if (Menu.Item("UltHelp").GetValue<KeyBind>().Active)
                        {
                            R.CastOnUnit(target, true);
                        }
                    }
                }
            }
        }
    }
}
