using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plug.DJ4LeagueSharp
{
    public class JsonHandler
    {
        public string a { get; set; }
        public P p { get; set; }
        public string s { get; set; }
    }

    public class P
    {
        public string cid { get; set; }
        public string message { get; set; }
        public int uid { get; set; }
        public string un { get; set; }
        public int i { get; set; }
        public int v { get; set; }
        public string avatarID { get; set; }
        public object badge { get; set; }
        public int gRole { get; set; }
        public int id { get; set; }
        public string joined { get; set; }
        public string language { get; set; }
        public int level { get; set; }
        public int role { get; set; }
        public string slug { get; set; }
        public string username { get; set; }
        public int c { get; set; }
        public List<int> d { get; set; }
        public string h { get; set; }
        public M m { get; set; }
        public int p { get; set; }
        public string t { get; set; }
    }

    public class M
    {
        public string author { get; set; }
        public string cid { get; set; }
        public int duration { get; set; }
        public int format { get; set; }
        public int id { get; set; }
        public string image { get; set; }
        public string title { get; set; }
    }
}
