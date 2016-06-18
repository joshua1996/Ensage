//using WindowsInput;
using System;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

namespace DrawingOnMap
{
    class Program
    {
        private static Hero me;
        private static bool flag = false, step2 = false, step3 = false;
        private static Vector2 startloc = new Vector2(150, 950), central1, central2;
        private static double theta = 0, step, x, y;
        private static int r = 100;
        private static double r2;
        private static readonly Ensage.Common.Menu.Menu Menu = new Ensage.Common.Menu.Menu("DrawingOnMap", "main", true);




        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);
        static void Main(string[] args)
        {
            Ensage.Common.Menu.Menu Circle = new Ensage.Common.Menu.Menu("Circle", "circle");
            Ensage.Common.Menu.Menu Star = new Ensage.Common.Menu.Menu("Star", "star");
            Ensage.Common.Menu.Menu Heart = new Ensage.Common.Menu.Menu("Heart", "heart");
            Circle.AddItem(new Ensage.Common.Menu.MenuItem("circlekey", "CTRL +").SetValue(new KeyBind('1', KeyBindType.Press)));
            Circle.AddItem(new Ensage.Common.Menu.MenuItem("radius", "Radius").SetValue(new Slider(100,15,150)));
            Star.AddItem(new Ensage.Common.Menu.MenuItem("starkey", "CTRL +").SetValue(new KeyBind('2', KeyBindType.Press)));
            Star.AddItem(new Ensage.Common.Menu.MenuItem("starsize", "Size").SetValue(new Slider(100, 15, 150)));
            Heart.AddItem(new Ensage.Common.Menu.MenuItem("heartkey", "CTRL +").SetValue(new KeyBind('3', KeyBindType.Press)));
            Heart.AddItem(new Ensage.Common.Menu.MenuItem("heartsize", "Size").SetValue(new Slider(100, 15, 150)));
            Menu.AddSubMenu(Circle);
            Menu.AddSubMenu(Star);
            Menu.AddSubMenu(Heart);
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
        }
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;
            if (!Game.IsInGame || me == null) return;

            if (Menu.Item("circlekey").GetValue<KeyBind>().Active)
            {
                r = Menu.Item("radius").GetValue<Slider>().Value;
                step = 0.1;
                flag = true;
                //draw
                mouse_event(0x02, 0, 0, 0, 0);
                if (theta < 6.28318530718)
                {
                    x = startloc.X + r * Math.Cos(theta);
                    y = startloc.Y + r * Math.Sin(theta);
                    SetCursorPos((int)x, (int)y);
                    theta += step;
                }
                else theta = 0;
            }
            else if (Menu.Item("starkey").GetValue<KeyBind>().Active)
            {
                r = Menu.Item("starsize").GetValue<Slider>().Value;
                step = 2.51327412287;
                flag = true;
                //draw
                mouse_event(0x02, 0, 0, 0, 0);
                if (Utils.SleepCheck("star"))
                {
                    if (theta < 12.5663706144)
                    {
                        x = startloc.X + r * Math.Cos(theta);
                        y = startloc.Y + r * Math.Sin(theta);
                        SetCursorPos((int)x, (int)y);
                        theta += step;
                        Utils.Sleep(100, "star");
                    }
                    else theta = 0;
                }
            }
            else if (Menu.Item("heartkey").GetValue<KeyBind>().Active)
            {
                r = Menu.Item("heartsize").GetValue<Slider>().Value;
                r2 = Math.Sqrt((double)(r * r * 2)) / 2;
                central1 = startloc - new Vector2((r/2),(r/2));
                central2 = startloc - new Vector2((-r / 2), (r / 2));
                flag = true;
                //draw
                mouse_event(0x02, 0, 0, 0, 0);
                //step 1 square
                if (Utils.SleepCheck("heart") && !step2 && !step3)
                {
                    step = 1.57079632679;
                    if (theta < 3.142)
                    {
                        x = startloc.X + r * Math.Cos(theta);
                        y = startloc.Y + r * Math.Sin(theta);
                        SetCursorPos((int)x, (int)y);
                        theta += step;
                        Utils.Sleep(100, "heart");
                    }
                    else
                    {
                        theta = 0;
                        step2 = true;
                    }
                }
                else if (!step3 && step2) //step2 
                {
                    step = 0.1;
                    if (theta < 3.14)
                    {
                        x = central1.X + r2 * Math.Cos(theta+2.4);
                        y = central1.Y + r2 * Math.Sin(theta+2.4);
                        SetCursorPos((int)x, (int)y);
                        theta += step;
                    }
                    else
                    {
                        theta = 0;
                        step3 = true;
                    }
                }
                else if (step2 && step3)
                {
                    if (theta < 3.14)
                    {
                        x = central2.X + r2 * Math.Cos(theta+3.8);
                        y = central2.Y + r2 * Math.Sin(theta+3.8);
                        SetCursorPos((int)x, (int)y);
                        theta += step;
                    }
                    else
                    {
                        theta = 0;
                        step2 = false;
                        step3 = false;
                    }

                }

            }
            else if (flag)
            {
                theta = 0;
                flag = false;
                mouse_event(0x04, 0, 0, 0, 0);
            }
        }
    }
}
