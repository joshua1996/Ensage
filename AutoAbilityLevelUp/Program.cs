using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;

namespace AutoAbilityLevelUp
{
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
    internal class Program
    {
        #region member
        private static Hero me;
        private static uint abilitypoint = 0;
        private const int WmKeyup = 0x0101;
        private static int skillnum = 5, count = 0;
        private static Ability[] spell = new Ability[5];
        private static int[] sequence = new int[25];
        private static bool loaded = false, skilltrue = false, hold = false;
        private static bool[,] skill2d = new bool[5, 25], skill2dcasche = new bool[5, 25];
        private static bool _leftMouseIsPress, leftMouseIsHold, move;
        private static bool[] offskill1 = new bool[25];
        private static readonly Menu Menu = new Menu("AutoAbilityLevelUp", "rootmenu", true);
        private static Vector2 startloc = new Vector2(450, 110);
        private static readonly Dictionary<string, DotaTexture> TextureDictionary = new Dictionary<string, DotaTexture>();
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
        private static void Game_OnUpdate(EventArgs args)
        {
            #region Init
            me = ObjectMgr.LocalHero;
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
                for (int i = 0; i < 25; i++)
                {
                    offskill1[i] = true;
                }
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
                spell[1] = me.Spellbook.SpellW;
                spell[2] = me.Spellbook.SpellE;
                spell[3] = me.Spellbook.SpellR;
                spell[4] = me.Spellbook.Spells.FirstOrDefault(x => x.AbilityType == AbilityType.Attribute);
                count = (int)(me.Level - me.AbilityPoints);
                for (int i = 0; i < count; i++) offskill1[i] = false;
                for (int i = 0; i < 20 + skillnum; i++)
                {
                    sequence[i] = -1;
                }
                if (ObjectMgr.LocalHero.ClassID == ClassID.CDOTA_Unit_Hero_Invoker)
                {
                    skillnum = 4;
                    sequence[24] = 4;
                }
                for (int i = 0; i < skillnum; i++) name[i] = NameManager.Name(spell[i]);
                skilltrue = true;
            }
            if (_leftMouseIsPress && Menu.Item("table").GetValue<KeyBind>().Active)
            {
                for (int i = 0; i < 20 + skillnum; i++)
                {
                    sequence[i] = getskill(i);
                }
            }

            if (Utils.SleepCheck("WaitPLSyoujerk"))
            {
                if(abilitypoint > me.AbilityPoints)
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
            if (move)
            {
                startloc = Game.MouseScreenPosition + new Vector2(0, 6);
            }
            if (Menu.Item("table").GetValue<KeyBind>().Active)
            {
                DotaTexture texture;

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
                DragButton(startloc - new Vector2(25, 12), 50, 12, ref move);
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

        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, bool isActive, Color @on, Color off, string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (isActive)
            {
                if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
                {
                    clicked = !clicked;
                    Utils.Sleep(250, "ClickButtonCd");
                }
                var newColor = isIn
                    ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                    : clicked ? @on : off;
                Drawing.DrawRect(a, new Vector2(w, h), newColor);
                Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
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
        private static void DragButton(Vector2 a, float w, float h, ref bool clicked)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (leftMouseIsHold && Utils.SleepCheck("HoldButtonCd") && isIn)
            {
                hold = true;
                Utils.Sleep(250, "HoldButtonCd");
            }
            else if(!leftMouseIsHold) hold = false;
            if (hold) clicked = true;
            else clicked = false;
            var newColor = isIn
                ? new Color(255, 255, 255, 50) : clicked ? new Color(0, 0, 0, 100) : new Color(0, 0, 0, 50);
            Drawing.DrawRect(a, new Vector2(w, h), newColor);
            Drawing.DrawRect(a, new Vector2(w, h), new Color(0, 0, 0, 255), true);
        }
    }
}
