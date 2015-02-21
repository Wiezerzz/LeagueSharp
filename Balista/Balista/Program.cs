using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;
using Color = System.Drawing.Color;

namespace Balista
{
    internal class Program
    {
        public static Spell R;
        private static Obj_AI_Hero Player;
        public static Menu menu;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Check to see if it is Kalista.
            if (Player.ChampionName != "Kalista") return;

            R = new Spell(SpellSlot.R, 1500f);
            R.SetSkillshot(0.50f, 1500, float.MaxValue, false, SkillshotType.SkillshotCircle);

            menu = new Menu("Balista", "Balista", true);
            {
                menu.AddItem(new MenuItem("useToggle", "Toggle").SetValue(false));
                menu.AddItem(new MenuItem("useOnComboKey", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            }
            Menu drawMenu = new Menu("Drawings", "Drawings");
            {
                drawMenu.AddItem(new MenuItem("minBRange", "Balista Min Range", true).SetValue(new Circle(false, Color.Chartreuse)));
                drawMenu.AddItem(new MenuItem("maxBRange", "Balista Max Range", true).SetValue(new Circle(false, Color.Green)));
            }
            Menu misc = new Menu("Misc", "misc");
            {
                misc.AddItem(new MenuItem("minRange", "Min Range to Balista", true).SetValue(new Slider(700, 100, 1400)));
                misc.AddItem(new MenuItem("maxRange", "Max Range to Balista", true).SetValue(new Slider(1400, 100, 1500)));
                misc.AddItem(new MenuItem("usePackets", "Use Packets").SetValue(false));
            }

            menu.AddSubMenu(drawMenu);
            menu.AddSubMenu(misc);
            menu.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#FF0000'>Balista</font> loaded! - <font color='#5882FA'>Wiezerzz</font>");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("minBRange", true).GetValue<Circle>().Active)
            Render.Circle.DrawCircle(Player.Position, menu.Item("minRange", true).GetValue<Slider>().Value, menu.Item("minBRange", true).GetValue<Circle>().Color, 3);
            if (menu.Item("maxBRange", true).GetValue<Circle>().Active)
                Render.Circle.DrawCircle(Player.Position, menu.Item("maxRange", true).GetValue<Slider>().Value, menu.Item("maxBRange", true).GetValue<Circle>().Color, 3);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || !menu.Item("useToggle").GetValue<bool>() &&
                !menu.Item("useOnComboKey").GetValue<KeyBind>().Active || !R.IsReady()) return;

            var blitzfriend =
                ObjectManager.Get<Obj_AI_Hero>()
                    .SingleOrDefault(
                        x =>
                            x.IsAlly && Player.Distance(x.ServerPosition) < menu.Item("maxRange", true).GetValue<Slider>().Value &&
                            Player.Distance(x.ServerPosition) >= menu.Item("minRange", true).GetValue<Slider>().Value && !x.IsMe &&
                            x.ChampionName == "Blitzcrank");

            if (blitzfriend == null)
                return;

            foreach (
                Obj_AI_Hero enem in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enem => enem.IsValid && enem.IsEnemy && enem.Distance(Player) <= 2450f)) //950f is blitz Q range.
            {
                if (enem != null)
                    foreach (BuffInstance buff in enem.Buffs)
                    {
                        if (enem.Buffs != null)
                        {
                            if (buff.Name == "rocketgrab2" && R.IsReady())
                            {
                                //Game.PrintChat("Grabbed!");
                                R.Cast(menu.Item("usePackets").GetValue<bool>());
                            }
                        }
                    }
            }
        }

    }
}
