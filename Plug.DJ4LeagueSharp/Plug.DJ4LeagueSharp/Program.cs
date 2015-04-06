using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;

using WebSocket4Net;
using SharpDX;


namespace Plug.DJ4LeagueSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr h, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr h, uint dwVolume);

        private static WebSocket wsClient = null;
        private static bool isLoggedIn = false;
        private static WebClientEx webClient = new WebClientEx();


        private static string curPlaylistID = "0";
        private static string vid_cID = "0";
        private static string vid_author = "0";
        private static string vid_title = "0";
        private static int vid_format = 0;
        private static int vid_duration = 0;


        /// <summary>
        /// A custom WebClient featuring a cookie container
        /// </summary>
        public class WebClientEx : WebClient
        {
            public CookieContainer CookieContainer { get; set; }

            public WebClientEx()
            {
                CookieContainer = new CookieContainer();
                
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = CookieContainer;
                }
                return request;
            }
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnInput += Game_OnInput;
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (!String.IsNullOrEmpty(args.Input))
            {
                if (args.Input.ToLower().StartsWith("/plug"))
                {
                    args.Process = false;

                    //Showing information, Commands.
                    switch (args.Input.ToLower())
                    {
                        case "/plug":
                        case "/plug ":
                        case "/plug help":
                            Game.PrintChat("TODO: showing help");
                            break;
                        case "/plug woot":
                        case "/plug woot!":
                            new Thread(() => Vote(1, webClient.CookieContainer)).Start();
                            break;
                        case "/plug meh":
                        case "/plug meh!":
                            new Thread(() => Vote(-1, webClient.CookieContainer)).Start();
                            break;
                    }

                    //Login and stuff
                    if (args.Input.ToLower().StartsWith("/plug login"))
                    {
                        string[] loginStrings = args.Input.Split(' ');
                        switch (loginStrings.Count())
                        {
                            case 2:
                                PlugPrint("You forgot to enter email & password.");
                                break;
                            case 3:
                                PlugPrint("You forgot to enter a password.");
                                break;
                            case 4:
                                PlugLogin(loginStrings[2], loginStrings[3]);
                                break;
                        }
                    }
                    if (args.Input.ToLower().StartsWith("/plug msg") && isLoggedIn)
                    {
                        string[] msgStrings = args.Input.Split(' ');
                        if (msgStrings.Count() >= 3)
                            wsClient.Send(
                                "{\"a\":\"chat\",\"p\":\"" + args.Input.Substring(10, args.Input.Length - 10) +
                                "\",\"t\":0}");
                    }
                    else if (!isLoggedIn)
                    {
                        PlugPrint("You are not logged in!");
                    }
                }
            }
        }

        private static void PlugLogin(string email, string pass)
        {
            if (!email.Contains("@"))
            {
                PlugPrint("Invalid email.");
                return;
            }

            wsClient = new WebSocket(
                "wss://godj.plug.dj:443/socket", "", null, null,
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2342.2 Safari/537.36",
                "https://plug.dj", WebSocketVersion.Rfc6455);

            wsClient.Opened += new EventHandler(websocket_Opened);
            wsClient.Closed += new EventHandler(wsClient_Closed);
            wsClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(wsClient_MessageReceived);
            wsClient.EnableAutoSendPing = true;
            wsClient.AutoSendPingInterval = 20;
            wsClient.Open();


            MatchCollection matches = null;
            matches = new Regex(@"var _csrf = ""(.*)"", _fb").Matches(webClient.DownloadString("https://plug.dj"));
            var Token = matches[0].Groups[1].Value;

            var request = (HttpWebRequest)WebRequest.Create("https://plug.dj/_/auth/login");

            var postData = "{\"csrf\":\"" + Token + "\",\"email\":\"" + email + "\",\"password\":\"" + pass + "\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.CookieContainer = webClient.CookieContainer;
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            MatchCollection matches2 = null;
            matches2 = new Regex(@",_jm=""(.*)"",_st=").Matches(webClient.DownloadString("https://plug.dj/dashboard"));
                
            wsClient.Send("{\"a\":\"auth\",\"p\":\"" + matches2[0].Groups[1].Value + "\",\"t\":0}");
            webClient.CookieContainer = request.CookieContainer;
            ChangeRoom("joduskame", webClient.CookieContainer);
        }

        private static void websocket_Opened(object sender, EventArgs e)
        {
            PlugPrint("Connecting to Plug.DJ server...");
        }

        private static void ChangeRoom(string room, CookieContainer cookieContainer)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://plug.dj/_/rooms/join");

            var postData = "{\"slug\":\"" + room + "\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.CookieContainer = cookieContainer;
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        private static void Vote(int direction, CookieContainer cookieContainer)
        {
            if (direction != 1 && direction != -1)
                return;

            var request = (HttpWebRequest)WebRequest.Create("https://plug.dj/_/votes");

            var postData = "{\"direction\":" + direction.ToString() + ",\"historyID\":\"" + curPlaylistID + "\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.CookieContainer = cookieContainer;
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        static void wsClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Plug.DJ pinging us, I think?
            if (e.Message == "h")
            return;

            //Just someone who left, nothing important and made the program crash cause JsonHandler.p is NOT an interger NOR a list.
            if(e.Message.StartsWith("[{\"a\":\"userLeave") || e.Message.StartsWith("[{\"a\":\"djListUpdate"))
                return;

            //Succesfull login
            if (e.Message == "[{\"a\":\"ack\",\"p\":\"1\",\"s\":\"dashboard\"}]")
            {
                PlugPrint("Succesfully logged in - Connected.");
                isLoggedIn = true;
                return;
            }

            string chatMsg = e.Message.Substring(1, e.Message.Length - 2);
            JsonHandler jsonLoLKing = new JavaScriptSerializer().Deserialize<JsonHandler>(chatMsg);

            switch (jsonLoLKing.a)
            {
                case "chat":
                    PlugPrint("<" + jsonLoLKing.p.un + "> " + jsonLoLKing.p.message);
                    break;
                case "advance":
                    vid_cID = jsonLoLKing.p.m.cid;
                    vid_author = jsonLoLKing.p.m.author;
                    vid_title = jsonLoLKing.p.m.title;
                    vid_format = jsonLoLKing.p.m.format;
                    vid_duration = jsonLoLKing.p.m.duration;
                    curPlaylistID = jsonLoLKing.p.h;
                    RunWithTimeout(runBrowserThread, TimeSpan.FromSeconds(vid_duration));
                    break;
            }

        }

        static void wsClient_Closed(object sender, EventArgs e)
        {
            PlugPrint("Connection closed - Disconnected.");
        }

        private static void PlugPrint(string text)
        {
            Game.PrintChat("[Plug.DJ] " + text);
        }


        #region THIS IS SO BAD OMFG KILL ME, SORRY GUYS I SUCK BALLS :(
        private static void runBrowserThread()
        {

            using (WebBrowser br = new WebBrowser())
            {
                br.DocumentCompleted += browser_DocumentCompleted;
                if(vid_format == 1)
                br.Navigate("http://www.booyoutube.com/watch/?v=" + vid_cID);
                else
                {
                    br.Navigate("https://w.soundcloud.com/player/?url=https://api.soundcloud.com/tracks/" + vid_cID + "&show_artwork=false&auto_play=true");
                }
                Console.WriteLine("start thread");
                Application.Run();
            }
        }

        private static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var br = sender as WebBrowser;

            if (br.ReadyState != WebBrowserReadyState.Complete && e.Url.AbsolutePath != br.Url.AbsolutePath)
                return;

            if (br.Url == e.Url)
            {
                Console.WriteLine("Navigated to {0}.", e.Url);
            }
        }

        private static void RunWithTimeout(ThreadStart threadStart, TimeSpan timeout)
        {
            Thread workerThread = new Thread(threadStart);
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();

            bool finished = workerThread.Join(timeout);
            if (!finished)
                workerThread.Abort();
        }
        #endregion
    }
}
