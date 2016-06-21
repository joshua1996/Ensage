using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Ensage;
using System.IO;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;
using System.Security.Permissions;

namespace AutoAbilityLevelUp
{
    #region classes
    internal class NameManager
    {
        #region Static Fields

        public static Dictionary<float, string> NameDictionary = new Dictionary<float, string>();

        #endregion

        #region Public Methods and Operators

        public static string Name(Entity entity)
        {
            var handle = entity.Handle;
            string name;
            if (NameDictionary.TryGetValue(handle, out name))
            {
                return name;
            }
            name = entity.Name;
            NameDictionary.Add(handle, name);
            return name;
        }

        #endregion
    }

    #endregion

    internal class Program
    {
        #region member
        private static Hero me;
        private static uint abilitypoint = 0;
        private const int WmKeyup = 0x0101;
        private static int skillnum = 5, count = 0;
        private static Ability[] spell = new Ability[5];
        private static int[] sequence = new int[25];
        private static bool loaded = false, skilltrue = false, hold = false, save = false, delete = false, start = false, startcache = false;
        private static bool[,] skill2d = new bool[5, 25], skill2dcasche = new bool[5, 25];
        private static bool _leftMouseIsPress, leftMouseIsHold;
        private static bool[] offskill1 = new bool[25];
        private static readonly Menu Menu = new Menu("AutoAbilityLevelUp", "rootmenu", true);
        private static Vector2 startloc = new Vector2(450, 110), diff;
        private static readonly Dictionary<string, DotaTexture> TextureDictionary = new Dictionary<string, DotaTexture>();
        private static readonly string MyPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\AutoAbilityLevelUp\\";
        private static string[] name = new string[5];
        #endregion


        private static void Main()
        {
            Menu.AddItem(new MenuItem("table", "Show Table").SetValue(new KeyBind('K', KeyBindType.Toggle)));
            Menu.AddToMainMenu();
            for (int i = 0; i < 25; i++)
            {
                offskill1[i] = true;
            }
            TextureDictionary.Add("itembg1", Drawing.GetTexture("materials/ensage_ui/menu/itembg1.vmat"));
            Directory.CreateDirectory(MyPath);
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen)
            {
                _leftMouseIsPress = false;
            }
            else
                _leftMouseIsPress = true;
            if (args.Msg != WmKeyup && args.WParam == 1)
                leftMouseIsHold = true;
            else leftMouseIsHold = false;
        }
        [PermissionSetAttribute(SecurityAction.Assert, Unrestricted = true)]
        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init
            me = ObjectManager.LocalHero;
            if (!loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                loaded = true;

            }

            if (!Game.IsInGame || me == null)
            {
                loaded = false;
                skilltrue = false;
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            #endregion
            if (!skilltrue)
            {
                spell[0] = me.Spellbook.SpellQ;
                if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore) spell[1] = me.Spellbook.SpellD;
                else spell[1] = me.Spellbook.SpellW;
                if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Chen || ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Beastmaster) spell[2] = me.Spellbook.SpellD;
                else if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore) spell[2] = me.Spellbook.SpellF;
                else spell[2] = me.Spellbook.SpellE;
                if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Ursa) spell[3] = me.Spellbook.Spells.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Ability_Ursa_Enrage);
                else if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Ogre_Magi) spell[3] = me.Spellbook.Spells.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Ability_Ogre_Magi_Multicast);
                else spell[3] = me.Spellbook.SpellR;
                spell[4] = me.Spellbook.Spells.FirstOrDefault(x => x.AbilityType == AbilityType.Attribute);
                count = (int)(me.Level - me.AbilityPoints);
                if (ObjectManager.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Invoker)
                {
                    skillnum = 4;
                    sequence[24] = 4;
                }
                else skillnum = 5;

                try
                {

                    LoadThis("" + ObjectManager.LocalHero.ClassID);
                    copy2darray();
                    for (int i = 0; i < 20 + skillnum; i++)
                    {
                        sequence[i] = getskill(i);
                    }
                }
                catch
                {
                    for (int i = 0; i < 20 + skillnum; i++)
                    {
                        sequence[i] = -1;
                        for (int j = 0; j < skillnum; j++)
                        {
                            skill2d[j, i] = false;
                            skill2dcasche[j, i] = false;
                        }
                    }
                 }
            for (int i = 0; i < skillnum; i++) name[i] = NameManager.Name(spell[i]);
                skilltrue = true;
            }
            if (save)
            {
                save = false;
                SaveThis("" + ObjectManager.LocalHero.ClassID);
            }
            if (delete)
            {
                delete = false;
                File.Delete(MyPath + ObjectManager.LocalHero.ClassID + ".txt");
                for (int i = 0; i < 20 + skillnum; i++)
                {
                    sequence[i] = -1;
                    for (int j = 0; j < skillnum; j++)
                    {
                        skill2d[j, i] = false;
                        skill2dcasche[j, i] = false;
                    }
                }
            }
            if (_leftMouseIsPress && Menu.Item("table").GetValue<KeyBind>().Active)
            {
                for (int i = 0; i < 20 + skillnum; i++)
                {
                    sequence[i] = getskill(i);
                }
            }
            if (start)
            {
                if (startcache == false)
                {
                    startcache = true;
                    for (int i = 0; i < count; i++) offskill1[i] = false;
                }
                if (Utils.SleepCheck("WaitPLSyoujerk"))
                {
                    if (abilitypoint > me.AbilityPoints)
                    {
                        abilitypoint = 0;
                        count++;
                        offskill1[count - 1] = false;
                    }
                    if (me.AbilityPoints > 0 && sequence[count] != -1)
                    {
                        abilitypoint = me.AbilityPoints;
                        Player.UpgradeAbility(me, spell[sequence[count]]);
                        Utils.Sleep(500, "WaitPLSyoujerk");
                    }
                }
            }
            else if (startcache == true)
            {
                startcache = false;
                for (int i = 0; i < count; i++)
                {
                    offskill1[i] = true;
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || me == null || !loaded || Game.IsPaused) return;
            if (_leftMouseIsPress && Menu.Item("table").GetValue<KeyBind>().Active)
            {
                for (int i = 0; i < 20 + skillnum; i++)
                {
                    for (int j = 0; j < skillnum; j++)
                    {
                        if (skill2d[j, i] != skill2dcasche[j, i])
                        {
                            updatearray(j, i);
                            copy2darray();
                        }
                    }
                }
            }
            if (Menu.Item("table").GetValue<KeyBind>().Active)
            {
                DotaTexture texture;
                if (TextureDictionary.TryGetValue("itembg1", out texture))
                    Drawing.DrawRect(startloc - new Vector2(25,12), new Vector2(725, 137), texture);
                for (int i = 0; i < skillnum; i++)
                {

                    if (!TextureDictionary.TryGetValue(name[i], out texture))
                    {
                        texture = Drawing.GetTexture("materials/ensage_ui/spellicons/" + name[i] + ".vmat");
                        TextureDictionary.Add(name[i], texture);
                    }
                    Drawing.DrawRect(startloc - new Vector2(25, -i * 25), new Vector2(25, 25), texture);
                }

                for (int i = 0; i < 20 + skillnum; i++)
                {
                    for (int j = 0; j < skillnum; j++)
                    {
                        DrawButton(startloc + new Vector2((i * 25), (j * 25)), 25, 25, ref skill2d[j, i], offskill1[i], new Color(0, 255, 0, 25), new Color(0, 0, 0, 50), "" + (i + 1));
                    }
                }
                DrawButton(startloc + new Vector2(((20 + skillnum) * 25), ((skillnum-1) * 25)), 75, 25, ref save, true, new Color(0, 255, 0, 25), new Color(0, 0, 0, 50), "SAVE");
                DrawButton(startloc + new Vector2(((20 + skillnum) * 25), ((skillnum-2) * 25)), 75, 25, ref delete, true, new Color(0, 255, 0, 25), new Color(0, 0, 0, 50), "DELETE");
                DrawButton(startloc + new Vector2(((20 + skillnum) * 25), 0), 75, 25 * (skillnum-2), ref start, true, new Color(0, 255, 0, 25), new Color(0, 0, 0, 50), "START");
                DragButton(startloc - new Vector2(25, 12), 725, 12);
            }
        }
        private static void updatearray(int j, int i)
        {
            for (int a = 0; a < 5; a++)
            {
                if (a == j) continue;
                skill2d[a, i] = false;
            }
        }
        private static void copy2darray()
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    skill2dcasche[j, i] = skill2d[j, i];
                }
            }
        }
        private static int getskill(int x)
        {
            for (int i = 0; i < 5; i++)
            {
                if (skill2d[i, x] == true) return i;
            }
            return -1;
        }
        private static void LoadThis(string section)
        {
            string temp, first;
            int equal;
            FileStream fileStream = File.OpenRead(MyPath + section + ".txt");
            TextReader textReader = new StreamReader(fileStream);
            for (int i = 0; i < 20 + skillnum; i++)
            {
                for (int j = 0; j < skillnum; j++)
                {
                    //skill2d[j, i] = Convert.ToBoolean(SaveLoadSysHelper.IniReadValue(section, "" + j + ", " + i));
                    temp = textReader.ReadLine();
                    while (temp != null)
                    {
                        equal = temp.IndexOf('=');
                        first = temp.Substring(0, equal);
                        if (first.Equals("" + j + ", " + i))
                        {
                            skill2d[j, i] = Convert.ToBoolean(temp.Substring(equal + 1));
                            break;
                        }
                        temp = textReader.ReadLine();
                    }
                }
            }
            textReader.Close();
        }

        private static void SaveThis(string section)
        {

            FileStream fileStream = File.OpenWrite(MyPath + section + ".txt");
            TextWriter textWriter = new StreamWriter(fileStream);
            for (int i = 0; i < 20 + skillnum; i++)
                for (int j = 0; j < skillnum; j++)
                {
                    //SaveLoadSysHelper.IniWriteValue(section, "" + j + ", " + i, skill2d[j, i].ToString());
                    textWriter.WriteLine("" + j + ", " + i + "=" + skill2d[j, i].ToString());
                }

            textWriter.Flush();
            textWriter.Close();
        }
        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn && !hold)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(255, 255, 255, 10), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
            else
            {
                var newColor2 = clicked ? @on : Color.Gray;
                Drawing.DrawRect(a, new Vector2(w, h), newColor2);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
                if (drawOnButtonText != "")
                {
                    Drawing.DrawText(drawOnButtonText, a + new Vector2(10, 2), Color.White,
                        FontFlags.AntiAlias | FontFlags.DropShadow);
                }
            }
        }
        private static void DragButton(Vector2 loc, float w, float h)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, loc.X, loc.Y, w, h);

            if (leftMouseIsHold && Utils.SleepCheck("HoldButtonCd") && isIn && !hold)
            {
                hold = true;
                diff = Game.MouseScreenPosition - startloc;
                Utils.Sleep(250, "HoldButtonCd");
            }
            else if (!leftMouseIsHold) hold = false;
            if (hold)
            {
                startloc = Game.MouseScreenPosition - diff;
            }
            Drawing.DrawRect(loc, new Vector2(w, h), new Color(0, 0, 0, 150));
        }
    }
}
