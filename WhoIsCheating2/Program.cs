using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LeagueSharp;

namespace WhoIsCheating2
{
    internal class Hero
    {
        public int Count;
        public int Detections;
        public int NetworkId;
    }

    internal class Program
    {
        private static bool _lookUp;
        private static bool _isDetecting;
        private static int _lastTick;
        private static List<Hero> _heroList;
        private static TimeSpan _ts;
        private static DateTime _start;

        private static void Main(string[] args)
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnInput += Game_OnInput;
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.ToLower() == "/startdetection")
            {
                _isDetecting = true;
                Game.PrintChat("<font color = \"#EEAD0E\">WhoIsCheating2 is now detecting players.</font>");
                args.Process = false;
            }

            if (args.Input.ToLower() == "/stopdetection")
            {
                _isDetecting = false;
                Game.PrintChat("<font color = \"#EEAD0E\">WhoIsCheating2 has stopped detecting players.</font>");
                args.Process = false;
            }
        }

        private static void DebugStatus(string message, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Check();
        }

        private static void Check()
        {
            if (Environment.TickCount <= _lastTick + 200)
            {
                return;
            }

            if (!_lookUp)
            {
                _heroList = new List<Hero>();
                using (var enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current == null || !current.IsValid)
                        {
                            continue;
                        }

                        _heroList.Add(new Hero {NetworkId = current.NetworkId, Count = 0, Detections = 0});
                        DebugStatus(String.Format("Added NId: {0}", current.NetworkId), ConsoleColor.White);
                    }
                }

                Game.PrintChat(
                    "<font color = \"#00E5EE\">WhoIsCheating2 by</font> <font color = \"#FF3300\">Mistejk</font> <font color = \"#00E5EE\">loaded and initialised.</font>");
                Game.PrintChat(
                    "<font color = \"#00EE00\">Type /StartDetection in order to start detecting players!</font>");
                _lookUp = true;
            }

            if (!_isDetecting)
            {
                return;
            }

            _ts = DateTime.Now - _start;
            if (_ts.TotalMilliseconds > 1000.0)
            {
                WhoIsCheatingHuehue();
            }

            _lastTick = Environment.TickCount;
        }

        private static void WhoIsCheatingHuehue()
        {
            using (var enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var hero = enumerator.Current;
                    if (hero == null || !hero.IsValid)
                    {
                        continue;
                    }

                    if (_heroList.Find(y => y.NetworkId == hero.NetworkId).Count >= 10)
                    {
                        ++_heroList.Find(y => y.NetworkId == hero.NetworkId).Detections;
                        if (hero.ChampionName != ObjectManager.Player.ChampionName)
                        {
                            DebugStatus(
                                String.Format(
                                    "Cheater detected! NId: {0}, Count: {1}, CN: {2}, Detections: {3}", hero.NetworkId,
                                    _heroList.Find(y => y.NetworkId == hero.NetworkId).Count, hero.ChampionName,
                                    _heroList.Find(y => y.NetworkId == hero.NetworkId).Detections), ConsoleColor.Red);
                            Game.PrintChat(
                                "Cheater detected: <font color = \"#FF0000\">{0} ({3})</font>. Detection {1}. Count {2}.",
                                hero.ChampionName, _heroList.Find(y => y.NetworkId == hero.NetworkId).Detections,
                                _heroList.Find(y => y.NetworkId == hero.NetworkId).Count, hero.Name);
                        }
                    }
                    _heroList.Find(y => y.NetworkId == hero.NetworkId).Count = 0;
                }
            }
            _start = DateTime.Now;
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !_lookUp || !_isDetecting)
            {
                return;
            }

            ++_heroList.Find(hero => hero.NetworkId == sender.NetworkId).Count;
        }
    }
}