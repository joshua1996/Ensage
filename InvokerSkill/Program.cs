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
        private static bool _loaded, skilltrue = false, wait = false;
        private static Hero me;
        private static readonly Menu Menu = new Menu("Invoker Skill", "main", true);
        public static double startposx = HUDInfo.ScreenSizeX() * 0.340104167, startposy = HUDInfo.ScreenSizeY() * 0.75;
        public static float size = (float)Math.Sqrt(2500 * (HUDInfo.ScreenSizeX() * HUDInfo.ScreenSizeY() / 2073600));
        public static Vector2 vector_size = new Vector2(size, size);
        public static Dictionary<string, DotaTexture> _textureCache = new Dictionary<string, DotaTexture>();
        private static readonly Dictionary<string, SpellStruct> SpellInfo = new Dictionary<string, SpellStruct>();
        private static Ability[] spell = new Ability[10];
        private static Font FontArray;
        private static Ability q, w, e, temp;
        #endregion

        private static void Main()
        {
            Menu.AddItem(new MenuItem("enable", "On").SetValue(true));
            Menu.AddItem(new MenuItem("quickcast", "Quick Cast").SetValue(false));
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
            Menu.AddItem(
               new MenuItem("skill", "Skill Order").SetValue(
                   new PriorityChanger(
                       new List<string>(
                           new[]
                               {
                                    "invoker_tornado", "invoker_chaos_meteor", "invoker_emp",
                                    "invoker_deafening_blast", "invoker_cold_snap", "invoker_ghost_walk", "invoker_forge_spirit", "invoker_ice_wall",
                                    "invoker_alacrity", "invoker_sun_strike"
                                }),
                       "skillorder")));
            Menu.AddSubMenu(changekey);
            Menu.AddToMainMenu();
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
        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init
            me = ObjectManager.LocalHero;


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
                return;
            }

            if (Game.IsPaused || !Menu.Item("enable").GetValue<bool>())
            {
                return;
            }
            if (!skilltrue)
            {
                skilltrue = true;
                q = me.Spellbook.SpellQ;
                w = me.Spellbook.SpellW;
                e = me.Spellbook.SpellE;

                SpellInfo.Add("invoker_sun_strike", new SpellStruct(e, e, e));
                SpellInfo.Add("invoker_cold_snap", new SpellStruct(q, q, q));
                SpellInfo.Add("invoker_ghost_walk", new SpellStruct(q, q, w));
                SpellInfo.Add("invoker_ice_wall", new SpellStruct(q, q, e));
                SpellInfo.Add("invoker_tornado", new SpellStruct(w, w, q));
                SpellInfo.Add("invoker_deafening_blast", new SpellStruct(q, w, e));
                SpellInfo.Add("invoker_forge_spirit", new SpellStruct(e, e, q));
                SpellInfo.Add("invoker_emp", new SpellStruct(w, w, w));
                SpellInfo.Add("invoker_alacrity", new SpellStruct(w, w, e));
                SpellInfo.Add("invoker_chaos_meteor", new SpellStruct(e, e, w));
            }
            var spells = Menu.Item("skill").GetValue<PriorityChanger>().ItemList.OrderByDescending(
            xx => Menu.Item("skill").GetValue<PriorityChanger>().GetPriority(xx));
            int i = 0;
            foreach (var spellx in spells)
            {
                spell[i] = me.FindSpell(spellx);
                i++;
            }
            #endregion
            if (Utils.SleepCheck("waitpls"))
            {
                if (wait)
                {
                    var active1 = me.Spellbook.Spell4;
                    if (temp == active1)
                    {
                        wait = false;

                        if (Menu.Item("quickcast").GetValue<bool>())
                            Game.ExecuteCommand("dota_ability_quickcast 3");
                        else Game.ExecuteCommand("dota_ability_execute 3");
                        Utils.Sleep(150, "waitpls");
                    }
                }
                else if (Menu.Item("firstkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[0]);
                }
                else if (Menu.Item("secondkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[1]);
                }
                else if (Menu.Item("thirdkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[2]);
                }
                else if (Menu.Item("forthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[3]);
                }
                else if (Menu.Item("fifthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[4]);
                }
                else if (Menu.Item("sixthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[5]);
                }
                else if (Menu.Item("seventhkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[6]);
                }
                else if (Menu.Item("eighthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[7]);
                }
                else if (Menu.Item("ninthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[8]);
                }
                else if (Menu.Item("tenthkey").GetValue<KeyBind>().Active)
                {
                    UseSkill(spell[9]);
                }
                
            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectManager.LocalPlayer;
            if (player == null || player.Team == Team.Observer || !_loaded)
            {
                return;
            }
            if (ObjectManager.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Invoker || !Menu.Item("enable").GetValue<bool>()) return;
            
            float[] spellcd = new float[10];
            float[] spelltotalcd = new float[10];
            for (int j = 0; j < 10; j++)
            {
                if (spell[j] == null) continue;
                spellcd[j] = spell[j].Cooldown;
                spelltotalcd[j] = spell[j].CooldownLength;
            }
            
            
            var spells = Menu.Item("skill").GetValue<PriorityChanger>().ItemList.OrderByDescending(
                xx => Menu.Item("skill").GetValue<PriorityChanger>().GetPriority(xx));
            int i = 0;
            foreach (var spellx in spells)
            {
               // if (i >= 5 && i <= 10) continue;
                Drawing.DrawRect(new Vector2((float)startposx + size * i, (float)startposy), vector_size, GetTexture("materials/ensage_ui/spellicons/"+ spellx +".vmat"));
                Drawing.DrawRect(new Vector2((float)startposx + size * i, (float)startposy), vector_size, new Color(0, 0, 0, 150), true);
                i++;
            }

            
            Drawing.DrawRect(new Vector2((float)startposx, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[0] / spelltotalcd[0])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[1] / spelltotalcd[1])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 2, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[2] / spelltotalcd[2])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 3, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[3] / spelltotalcd[3])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 4, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[4] / spelltotalcd[4])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 5, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[5] / spelltotalcd[5])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 6, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[6] / spelltotalcd[6])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 7, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[7] / spelltotalcd[7])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 8, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[8] / spelltotalcd[8])) * 50), new Color(255, 255, 255, 70));
            Drawing.DrawRect(new Vector2((float)startposx + size * 9, (float)startposy), new Vector2(50, 50 - (1 - (spellcd[9] / spelltotalcd[9])) * 50), new Color(255, 255, 255, 70));
            
            //-----------------------------------------not learn skill dim--------------------------------------------------------------
            #region not_learn
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
            #endregion
            //------------------------------------------------------------------------------------------------------------------------------------
            #region cooldown
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
            #endregion
            
        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame || !Menu.Item("enable").GetValue<bool>())
                return;
            DrawShadowText(Convert.ToChar(Menu.Item("firstkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + 3, (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("secondkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("thirdkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 2 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("forthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 3 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("fifthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 4 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("sixthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 5 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("seventhkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 6 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("eighthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 7 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("ninthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 8 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
            DrawShadowText(Convert.ToChar(Menu.Item("tenthkey").GetValue<KeyBind>().Key).ToString(), (int)startposx + (int)(size * 9 + 3), (int)startposy + 1, Color.LightCyan, FontArray);
        }
        public static void UseSkill(Ability skill)
        {
            var active1 = me.Spellbook.Spell4;
            var active2 = me.Spellbook.Spell5;
            if (Equals(skill, active1)) //If the skill inside D
            {
                if (Menu.Item("quickcast").GetValue<bool>())
                    Game.ExecuteCommand("dota_ability_quickcast 3");
                else Game.ExecuteCommand("dota_ability_execute 3");
                Utils.Sleep(150, "waitpls");

            }
            else if (Equals(skill, active2)) //If the skill inside F
            {
                if (Menu.Item("quickcast").GetValue<bool>())
                    Game.ExecuteCommand("dota_ability_quickcast 4");
                else Game.ExecuteCommand("dota_ability_execute 4");
                Utils.Sleep(150, "waitpls");

            }
            else //If not inside D and F, invoke the skill
            {
                SpellStruct s;
                if (SpellInfo.TryGetValue(skill.Name, out s))
                {
                    var invoke = me.FindSpell("invoker_invoke");
                    var spells = s.GetNeededAbilities();
                    if (invoke.CanBeCasted() && me.CanCast())
                    {
                        if (spells[0] != null) spells[0].UseAbility();
                        if (spells[1] != null) spells[1].UseAbility();
                        if (spells[2] != null) spells[2].UseAbility();
                        invoke.UseAbility();
                        wait = true;
                        temp = skill;
                    }
                    else {
                        invoke.UseAbility();
                        Utils.Sleep(150, "waitpls");
                    }
                }
            }
        
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

    }
}
