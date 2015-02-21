using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;
using Color = System.Drawing.Color;

namespace Balista
{
    class Program
    {
        public static Spell R;
        private static Obj_AI_Hero Player;
        public static Menu menu;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != "Kalista") return;

            R = new Spell(SpellSlot.R, 1500f);
            R.SetSkillshot(0.50f, 1500, float.MaxValue, false, SkillshotType.SkillshotCircle);

            menu = new Menu("Balista", "Balista", true);
            menu.AddItem(new MenuItem("useToggle", "Toggle").SetValue(true));
            menu.AddItem(new MenuItem("useOnComboKey", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));

            menu.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("Balista Loaded! - Bruqaj#");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            throw new NotImplementedException();
        }



    }
}
