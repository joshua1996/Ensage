using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;
using Ensage.Common.Menu;

namespace InvokerSkill
{
    internal class Program
    {
        #region Members

        private static bool _loaded;
        private static bool skilltrue = false;
        private static readonly Dictionary<string, SpellStruct> SpellInfo = new Dictionary<string, SpellStruct>();
        private static Ability q, w, e, ss, coldsnap, ghostwalk, icewall, tornado, deafblast, forge, emp, alacrity, chaosmeteor;
        private static Ability[] spell = new Ability[11];
        private static int startspell = 10;
        private static Vector2 _sizer = new Vector2(265, 300);
        private static Hero me;
        private static Font FontArray;
        public static Dictionary<string, DotaTexture> _textureCache = new Dictionary<string, DotaTexture>();
        public static double startposx = HUDInfo.ScreenSizeX() * 0.340104167, startposy = HUDInfo.ScreenSizeY() * 0.75;
        public static float size = (float)Math.Sqrt(2500 * (HUDInfo.ScreenSizeX() * HUDInfo.ScreenSizeY() / 2073600));
        public static Vector2 vector_size = new Vector2(size, size);
        private static readonly Menu Menu = new Menu("Invoker Skill", "invokerskill", true);

        private static char firstkey, secondkey, thirdkey, forthkey, fifthkey, sixthkey, seventhkey, eighthkey, ninthkey, tenthkey;
        #endregion

        private static void Main()
        {
            MenuItem quickcastoption = new MenuItem("quickcast", "Quick Cast").SetValue(false);
            Menu changekey = new Menu("Change Key", "changekey");
            changekey.AddItem(new MenuItem("firstkey", "First Key").SetValue(new KeyBind('1', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("secondkey", "Second Key").SetValue(new KeyBind('2', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("thirdkey", "Third Key").SetValue(new KeyBind('3', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("forthkey", "Forth Key").SetValue(new KeyBind('4', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("fifthkey", "Fifth Key").SetValue(new KeyBind('5', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("sixthkey", "Sixth Key").SetValue(new KeyBind('6', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("seventhkey", "Seventh Key").SetValue(new KeyBind('7', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("eighthkey", "Eighth Key").SetValue(new KeyBind('8', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("ninthkey", "Ninth Key").SetValue(new KeyBind('9', KeyBindType.Press)));
            changekey.AddItem(new MenuItem("tenthkey", "Tenth Key").SetValue(new KeyBind('0', KeyBindType.Press)));
            Menu.AddItem(quickcastoption);
            Menu.AddSubMenu(changekey);
            Menu.AddToMainMenu();
            firstkey = Convert.ToChar(Menu.Item("firstkey").GetValue<KeyBind>().Key);
            secondkey = Convert.ToChar(Menu.Item("secondkey").GetValue<KeyBind>().Key);
            thirdkey = Convert.ToChar(Menu.Item("thirdkey").GetValue<KeyBind>().Key);
            forthkey = Convert.ToChar(Menu.Item("forthkey").GetValue<KeyBind>().Key);
            fifthkey = Convert.ToChar(Menu.Item("fifthkey").GetValue<KeyBind>().Key);
            sixthkey = Convert.ToChar(Menu.Item("sixthkey").GetValue<KeyBind>().Key);
            seventhkey = Convert.ToChar(Menu.Item("seventhkey").GetValue<KeyBind>().Key);
            eighthkey = Convert.ToChar(Menu.Item("eighthkey").GetValue<KeyBind>().Key);
            ninthkey = Convert.ToChar(Menu.Item("ninthkey").GetValue<KeyBind>().Key);
            tenthkey = Convert.ToChar(Menu.Item("tenthkey").GetValue<KeyBind>().Key);
            FontArray = new Font(
                    Drawing.Direct3DDevice9,
                    new FontDescription
                    {
                        FaceName = "Tahoma",
                        Height = 15,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });
            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }
        struct SpellStruct
        {
            private readonly Ability _oneAbility;
            private readonly Ability _twoAbility;
            private readonly Ability _threeAbility;

            public SpellStruct(Ability oneAbility, Ability twoAbility, Ability threeAbility)
            {
                _oneAbility = oneAbility;
                _twoAbility = twoAbility;
                _threeAbility = threeAbility;
            }

            public Ability[] GetNeededAbilities()
            {
                return new[] { _oneAbility, _twoAbility, _threeAbility };
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen)
                return;
            //  if(Game.IsKeyDown(0x11)){
            if (args.WParam == firstkey)
                startspell = 1;
            else if (args.WParam == secondkey)
                startspell = 2;
            else if (args.WParam == thirdkey)
                startspell = 3;
            else if (args.WParam == forthkey)
                startspell = 4;
            else if (args.WParam == fifthkey)
                startspell = 5;
            else if (args.WParam == sixthkey)
                startspell = 6;
            else if (args.WParam == seventhkey)
                startspell = 7;
            else if (args.WParam == eighthkey)
                startspell = 8;
            else if (args.WParam == ninthkey)
                startspell = 9;
            else if (args.WParam == tenthkey)
                startspell = 0;
            else startspell = 10;
            // }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init
            me = ObjectMgr.LocalHero;


            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker)
                {
                    return;
                }
                _loaded = true;

            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                PrintInfo("> Invorker unLoaded");
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            #endregion



            if (startspell < 10)
            {

                if (!skilltrue)
                {
                    skilltrue = true;
                    q = me.Spellbook.SpellQ;
                    w = me.Spellbook.SpellW;
                    e = me.Spellbook.SpellE;

                    ss = me.FindSpell("invoker_sun_strike");
                    coldsnap = me.FindSpell("invoker_cold_snap");
                    ghostwalk = me.FindSpell("invoker_ghost_walk");
                    icewall = me.FindSpell("invoker_ice_wall");
                    tornado = me.FindSpell("invoker_tornado");
                    deafblast = me.FindSpell("invoker_deafening_blast");
                    forge = me.FindSpell("invoker_forge_spirit");
                    emp = me.FindSpell("invoker_emp");
                    alacrity = me.FindSpell("invoker_alacrity");
                    chaosmeteor = me.FindSpell("invoker_chaos_meteor");

                    SpellInfo.Add(ss.Name, new SpellStruct(e, e, e));
                    SpellInfo.Add(coldsnap.Name, new SpellStruct(q, q, q));
                    SpellInfo.Add(ghostwalk.Name, new SpellStruct(q, q, w));
                    SpellInfo.Add(icewall.Name, new SpellStruct(q, q, e));
                    SpellInfo.Add(tornado.Name, new SpellStruct(w, w, q));
                    SpellInfo.Add(deafblast.Name, new SpellStruct(q, w, e));
                    SpellInfo.Add(forge.Name, new SpellStruct(e, e, q));
                    SpellInfo.Add(emp.Name, new SpellStruct(w, w, w));
                    SpellInfo.Add(alacrity.Name, new SpellStruct(w, w, e));
                    SpellInfo.Add(chaosmeteor.Name, new SpellStruct(e, e, w));
                    spell[1] = tornado;
                    spell[2] = chaosmeteor;
                    spell[3] = emp;
                    spell[4] = deafblast;
                    spell[5] = coldsnap;
                    spell[6] = ghostwalk;
                    spell[7] = forge;
                    spell[8] = icewall;
                    spell[9] = alacrity;
                    spell[0] = ss;
                    spell[10] = chaosmeteor; //extra one just for checking condition, will not be used
                }

                SpellStruct s;
                var active1 = me.Spellbook.Spell4;
                var active2 = me.Spellbook.Spell5;
                if (Equals(spell[startspell], active1)) //If the skill inside D
                {
                    spell[startspell].UseAbility();
                    if (Utils.SleepCheck("spell1sleep"))
                    {
                        if (!Menu.Item("quickcast").GetValue<bool>())
                            Game.ExecuteCommand("dota_ability_execute 3");
                        else Game.ExecuteCommand("dota_ability_quickcast 3");
                        Utils.Sleep(150, "spell1sleep");
                    }

                }
                else if (Equals(spell[startspell], active2)) //If the skill inside F
                {
                    spell[startspell].UseAbility();
                    if (Utils.SleepCheck("spell2sleep"))
                    {
                        if (!Menu.Item("quickcast").GetValue<bool>())
                            Game.ExecuteCommand("dota_ability_execute 4");
                        else Game.ExecuteCommand("dota_ability_quickcast 4");
                        Utils.Sleep(150, "spell2sleep");
                    }
                }
                else //If not inside D and F, invoke the skill
                {
                    if (SpellInfo.TryGetValue(spell[startspell].Name, out s))
                    {
                        var invoke = me.FindSpell("invoker_invoke");
                        var spells = s.GetNeededAbilities();
                        if (spells[0] != null) spells[0].UseAbility();
                        if (spells[1] != null) spells[1].UseAbility();
                        if (spells[2] != null) spells[2].UseAbility();
                        invoke.UseAbility();
                    }
                }
                startspell = 10;
            }


        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Invoker) return;
            if (!skilltrue)
            {
                skilltrue = true;
                q = me.Spellbook.SpellQ;
                w = me.Spellbook.SpellW;
                e = me.Spellbook.SpellE;

                ss = me.FindSpell("invoker_sun_strike");
                coldsnap = me.FindSpell("invoker_cold_snap");
                ghostwalk = me.FindSpell("invoker_ghost_walk");
                icewall = me.FindSpell("invoker_ice_wall");
                tornado = me.FindSpell("invoker_tornado");
                deafblast = me.FindSpell("invoker_deafening_blast");
                forge = me.FindSpell("invoker_forge_spirit");
                emp = me.FindSpell("invoker_emp");
                alacrity = me.FindSpell("invoker_alacrity");
                chaosmeteor = me.FindSpell("invoker_chaos_meteor");

                SpellInfo.Add(ss.Name, new SpellStruct(e, e, e));
                SpellInfo.Add(coldsnap.Name, new SpellStruct(q, q, q));
                SpellInfo.Add(ghostwalk.Name, new SpellStruct(q, q, w));
                SpellInfo.Add(icewall.Name, new SpellStruct(q, q, e));
                SpellInfo.Add(tornado.Name, new SpellStruct(w, w, q));
                SpellInfo.Add(deafblast.Name, new SpellStruct(q, w, e));
                SpellInfo.Add(forge.Name, new SpellStruct(e, e, q));
                SpellInfo.Add(emp.Name, new SpellStruct(w, w, w));
                SpellInfo.Add(alacrity.Name, new SpellStruct(w, w, e));
                SpellInfo.Add(chaosmeteor.Name, new SpellStruct(e, e, w));
                spell[1] = tornado;
                spell[2] = chaosmeteor;
                spell[3] = emp;
                spell[4] = deafblast;
                spell[5] = coldsnap;
                spell[6] = ghostwalk;
                spell[7] = forge;
                spell[8] = icewall;
                spell[9] = alacrity;
                spell[0] = ss;
                spell[10] = chaosmeteor; //extra one just for checking condition, will not be used
            }
            float[] spellcd = new float[10];
            float[] spelltotalcd = new float[10];
            for (int i = 0; i < 10; i++)
            {
                if (spell[i] == null) continue;
                spellcd[i] = spell[i].Cooldown;
                spelltotalcd[i] = spell[i].CooldownLength;
            }
            Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_tornado.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_chaos_meteor.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_emp.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_deafening_blast.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_cold_snap.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_ghost_walk.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_forge_spirit.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_ice_wall.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_alacrity.vmat"));
            Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/invoker_sun_strike.vmat"));


            //draw box
            Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
            Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);

            Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[1] / spelltotalcd[1])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[2] / spelltotalcd[2])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[3] / spelltotalcd[3])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[4] / spelltotalcd[4])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[5] / spelltotalcd[5])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[6] / spelltotalcd[6])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[7] / spelltotalcd[7])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[8] / spelltotalcd[8])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[9] / spelltotalcd[9])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[0] / spelltotalcd[0])) * 50), new Color(255, 255, 255, 70));
            //-----------------------------------------not learn skill dim--------------------------------------------------------------
            if (q.AbilityState == AbilityState.NotLearned || w.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), vector_size, new Color(0, 0, 0, 200));
                Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            if (w.AbilityState == AbilityState.NotLearned || e.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), vector_size, new Color(0, 0, 0, 200));
                Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            if (e.AbilityState == AbilityState.NotLearned || q.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), vector_size, new Color(0, 0, 0, 200));
                Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }

            if (w.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            if (q.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            if (e.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            if (e.AbilityState == AbilityState.NotLearned || q.AbilityState == AbilityState.NotLearned || w.AbilityState == AbilityState.NotLearned)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), vector_size, new Color(0, 0, 0, 200));
            }
            //------------------------------------------------------------------------------------------------------------------------------------
            if (spellcd[1] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[2] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[3] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[4] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[5] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[6] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[7] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[8] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[9] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }
            if (spellcd[0] != 0)
            {
                Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), vector_size, new Color(0, 0, 0, 150));
            }

        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;
            if (ObjectMgr.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Invoker) return;
            DrawShadowText(firstkey.ToString(), (int)startposx + 3, (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(secondkey.ToString(), (int)startposx + (int)(size + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(thirdkey.ToString(), (int)startposx + (int)(size * 2 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(forthkey.ToString(), (int)startposx + (int)(size * 3 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(fifthkey.ToString(), (int)startposx + (int)(size * 4 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(sixthkey.ToString(), (int)startposx + (int)(size * 5 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(seventhkey.ToString(), (int)startposx + (int)(size * 6 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(eighthkey.ToString(), (int)startposx + (int)(size * 7 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(ninthkey.ToString(), (int)startposx + (int)(size * 8 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(tenthkey.ToString(), (int)startposx + (int)(size * 9 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
        }
        public static DotaTexture GetTexture(string name)
        {
            if (_textureCache.ContainsKey(name)) return _textureCache[name];

            return _textureCache[name] = Drawing.GetTexture(name);
        }
        public static void DrawShadowText(string stext, int x, int y, Color color, Font f)
        {
            f.DrawText(null, stext, x + 1, y + 1, Color.Black);
            f.DrawText(null, stext, x, y, color);
        }
        #region Helpers
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }

        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion

    }
}
