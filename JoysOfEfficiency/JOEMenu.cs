﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoysOfEfficiency.Options
{
    public class JOEMenu : IClickableMenu
    {
        private ModEntry mod;
        private ITranslationHelper translation;
        private IMonitor mon;

        private readonly List<MenuTab> tabs = new List<MenuTab>();

        private ClickableTextureComponent upCursor;
        private ClickableTextureComponent downCursor;

        private Rectangle tabEnabled;
        private Rectangle tabSliders;
        private Rectangle tabControls;

        private int tabIndex = 0;
        private int firstIndex = 0;

        private readonly SpriteFont font = Game1.smallFont;
        private string tabEnabledString;
        private string tabSlidersString;
        private string tabControlsString;

        private bool isListening;
        private ModifiedInputListener listener = null;

        public JOEMenu(int width, int height, ModEntry mod) : base (Game1.viewport.Width / 2 - width / 2, Game1.viewport.Height / 2 - height / 2, width, height, true)
        {

            this.mod = mod;
            translation = mod.Helper.Translation;
            upCursor = new ClickableTextureComponent("up-arrow", new Rectangle(xPositionOnScreen + this.width + Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            downCursor = new ClickableTextureComponent("down-arrow", new Rectangle(xPositionOnScreen + this.width + Game1.tileSize / 4, yPositionOnScreen + this.height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);

            tabEnabledString = translation.Get("tab.enabled");
            Vector2 Size = font.MeasureString(tabEnabledString);
            tabEnabled = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen, ((int)Size.X + 32), 64);

            tabSlidersString = translation.Get("tab.sliders");
            Size = font.MeasureString(tabSlidersString);
            tabSliders = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen + 68, ((int)Size.X + 32), 64);

            tabControlsString = translation.Get("tab.controls");
            Size = font.MeasureString(tabControlsString);
            tabControls = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen + 136, ((int)Size.X + 32), 64);

            {
                //Enabled Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new ModifiedCheckBox("HowManyStonesLeft", 0, ModEntry.Conf.HowManyStonesLeft, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("GiftInformation", 1, ModEntry.Conf.GiftInformation, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoWaterNearbyCrops", 2, ModEntry.Conf.AutoWaterNearbyCrops, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoPetNearbyAnimals", 3, ModEntry.Conf.AutoPetNearbyAnimals, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoAnimalDoor", 4, ModEntry.Conf.AutoAnimalDoor, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoFishing", 5, ModEntry.Conf.AutoFishing, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoReelRod", 6, ModEntry.Conf.AutoReelRod, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("MuchFasterBiting", 7, ModEntry.Conf.MuchFasterBiting, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("FishingInfo", 8, ModEntry.Conf.FishingInfo, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoGate", 9, ModEntry.Conf.AutoGate, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoEat", 10, ModEntry.Conf.AutoEat, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoHarvest", 11, ModEntry.Conf.AutoHarvest, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoDestroyDeadCrops", 12, ModEntry.Conf.AutoDestroyDeadCrops, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoRefillWateringCan", 13, ModEntry.Conf.AutoRefillWateringCan, OnCheckboxValueChanged));


                tabs.Add(tab);
            }
            {
                //Sliders Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new ModifiedSlider("CPUThresholdFishing", 0, (int)(ModEntry.Conf.CPUThresholdFishing * 10), 5, OnSliderValueChanged, (() => !ModEntry.Conf.AutoFishing), Format));
                tab.AddOptionsElement(new ModifiedSlider("StaminaToEatRatio", 1, (int)(ModEntry.Conf.StaminaToEatRatio * 10), 8, OnSliderValueChanged, (() => !ModEntry.Conf.AutoEat), Format));
                tab.AddOptionsElement(new ModifiedSlider("HealthToEatRatio", 2, (int)(ModEntry.Conf.HealthToEatRatio * 10), 8, OnSliderValueChanged, (() => !ModEntry.Conf.AutoEat), Format));
                tabs.Add(tab);
            }
            {
                //Controls Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new ModifiedInputListener(this, "Show Menu", 0, ModEntry.Conf.KeyShowMenu, translation, OnInputListnerChanged, OnStartListening));
                tab.AddOptionsElement(new ModifiedInputListener(this, "Show HowManyStones", 1, ModEntry.Conf.KeyShowStonesLeft, translation, OnInputListnerChanged, OnStartListening, (i => !ModEntry.Conf.HowManyStonesLeft)));
                tabs.Add(tab);
            }
            mon = mod.Monitor;
        }
        private void OnStartListening(int i, ModifiedInputListener listener)
        {
            isListening = true;
            this.listener = listener;
        }
        private void OnInputListnerChanged(int index, Keys value)
        {
            if(index == 0)
            {
                ModEntry.Conf.KeyShowMenu = value;
            }
            else if(index == 1)
            {
                ModEntry.Conf.KeyShowStonesLeft = value;
            }
            mod.WriteConfig();
            isListening = false;
            listener = null;
        }
        private void OnCheckboxValueChanged(int index, bool value)
        {
            switch (index)
            {
                case 0: ModEntry.Conf.HowManyStonesLeft = value; break;
                case 1: ModEntry.Conf.GiftInformation = value; break;
                case 2: ModEntry.Conf.AutoWaterNearbyCrops = value; break;
                case 3: ModEntry.Conf.AutoPetNearbyAnimals = value; break;
                case 4: ModEntry.Conf.AutoAnimalDoor = value; break;
                case 5: ModEntry.Conf.AutoFishing = value; break;
                case 6: ModEntry.Conf.AutoReelRod = value; break;
                case 7: ModEntry.Conf.MuchFasterBiting = value; break;
                case 8: ModEntry.Conf.FishingInfo = value; break;
                case 9: ModEntry.Conf.AutoGate = value; break;
                case 10: ModEntry.Conf.AutoEat = value; break;
                case 11: ModEntry.Conf.AutoHarvest = value; break;
                case 12: ModEntry.Conf.AutoDestroyDeadCrops = value; break;
                case 13: ModEntry.Conf.AutoRefillWateringCan = value; break;
                default:return;
            }
            mod.WriteConfig();
        }
        private void OnSliderValueChanged(int index, int value)
        {
            if(index == 0)
            {
                ModEntry.Conf.CPUThresholdFishing = value / 10.0f;
            }
            if(index == 1)
            {
                ModEntry.Conf.StaminaToEatRatio = value / 10.0f;
            }
            if(index == 2)
            {
                ModEntry.Conf.HealthToEatRatio = value / 10.0f;
            }

            mod.WriteConfig();
        }

        private string Format(int id, int value)
        {
            if(id == 0 || id == 1 || id == 2)
            {
                return string.Format("{0:f1}", value / 10f);
            }
            return value + "";
        }

        public override void update(GameTime time)
        {
            base.update(time);
            upCursor.visible = firstIndex > 0;
            downCursor.visible = !CanDrawAll();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (isListening)
            {
                return;
            }
            //Scroll with Mouse Wheel
            if (direction > 0 && upCursor.visible)
            {
                Game1.playSound("shwip");
                firstIndex--;
            }
            else if(direction < 0 && downCursor.visible)
            {
                Game1.playSound("shwip");
                firstIndex++;
            }
        }

        public override void draw(SpriteBatch b)
        {
            int x = 16, y = 16;


            drawTextureBox(b, tabEnabled.Left, tabEnabled.Top, tabEnabled.Width, tabEnabled.Height, Color.White * (tabIndex != 0 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabEnabledString, new Vector2(tabEnabled.Left + 16, tabEnabled.Top + (tabEnabled.Height - font.MeasureString(tabEnabledString).Y) / 2), Color.Black * (tabIndex != 0 ? 1.0f : 0.6f));

            drawTextureBox(b, tabSliders.Left, tabSliders.Top, tabSliders.Width, tabSliders.Height, Color.White * (tabIndex != 1 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabSlidersString, new Vector2(tabSliders.Left + 16, tabSliders.Top + (tabSliders.Height - font.MeasureString(tabSlidersString).Y) / 2), Color.Black * (tabIndex != 1 ? 1.0f : 0.6f));

            drawTextureBox(b, tabControls.Left, tabControls.Top, tabControls.Width, tabControls.Height, Color.White * (tabIndex != 2 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabControlsString, new Vector2(tabControls.Left + 16, tabControls.Top + (tabControls.Height - font.MeasureString(tabControlsString).Y) / 2), Color.Black * (tabIndex != 2 ? 1.0f : 0.6f));

            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen, yPositionOnScreen, this.width, this.height, Color.White, 1.0f, false);
            base.draw(b);

            {
                int X = (Game1.viewport.Width - 400) / 2;
                drawTextureBox(b, X, yPositionOnScreen - 108, 400, 100, Color.White);

                string str = "JOE Settings";
                Vector2 size = Game1.dialogueFont.MeasureString(str) * 1.1f;

                Utility.drawTextWithShadow(b, str, Game1.dialogueFont, new Vector2((Game1.viewport.Width - (int)size.X) / 2, yPositionOnScreen - 50 - (int)size.Y / 2), Color.Black, 1.1f);
            }

            foreach (OptionsElement element in GetElementsToShow())
            {
                element.draw(b, x + this.xPositionOnScreen, y + yPositionOnScreen);
                y += element.bounds.Height + 16;
            }
            upCursor.draw(b);
            downCursor.draw(b);

            if(isListening)
            {
                Point size = listener.GetListeningMessageWindowSize();
                drawTextureBox(b, (Game1.viewport.Width - size.X) / 2, (Game1.viewport.Height - size.Y) / 2, size.X, size.Y, Color.White);
                listener.DrawStrings(b, (Game1.viewport.Width - size.X) / 2, (Game1.viewport.Height - size.Y) / 2);
            }

            if (!Game1.options.hardwareCursor)
            {
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            upCursor.tryHover(x, y);
            downCursor.tryHover(x, y);
        }

        public override void leftClickHeld(int x, int y)
        {
            if (isListening)
            {
                return;
            }
            base.leftClickHeld(x, y);
            foreach (OptionsElement element in GetElementsToShow())
            {
                if (element.bounds.Contains(x - xPositionOnScreen - element.bounds.X / 2, y - yPositionOnScreen - element.bounds.Y / 2))
                {
                    element.leftClickHeld(x - element.bounds.X - xPositionOnScreen, y - element.bounds.Y - yPositionOnScreen);
                }
                y -= element.bounds.Height + 16;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (isListening)
            {
                foreach (OptionsElement element in tabs[tabIndex].GetElements())
                {
                    element.receiveKeyPress(key);
                }
            }
            else if(key == Keys.Escape)
            {
                Game1.playSound("bigDeSelect");
                Game1.activeClickableMenu = null;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (isListening)
            {
                return;
            }
            if(tabEnabled.Contains(x, y))
            {
                TryToChangeTab(0);
                return;
            }
            if (tabSliders.Contains(x, y))
            {
                TryToChangeTab(1);
                return;
            }
            if(tabControls.Contains(x, y))
            {
                TryToChangeTab(2);
                return;
            }
            if (upCursor.bounds.Contains(x, y) && upCursor.visible)
            {
                Game1.playSound("shwip");
                firstIndex--;
                return;
            }
            if (downCursor.bounds.Contains(x, y) && downCursor.visible)
            {
                Game1.playSound("shwip");
                firstIndex++;
                return;
            }
            foreach (OptionsElement element in GetElementsToShow())
            {
                if(element.bounds.Contains(x - xPositionOnScreen - element.bounds.X / 2, y - yPositionOnScreen - element.bounds.Y / 2))
                {
                    element.receiveLeftClick(x - element.bounds.X - xPositionOnScreen, y - element.bounds.Y - yPositionOnScreen);
                }
                y -= element.bounds.Height + 16;
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (isListening)
            {
                return;
            }
            foreach (OptionsElement element in GetElementsToShow())
            {
                if (element.bounds.Contains(x - xPositionOnScreen - element.bounds.X / 2, y - yPositionOnScreen - element.bounds.Y / 2))
                {
                    element.leftClickReleased(x - element.bounds.X - xPositionOnScreen, y - element.bounds.Y - yPositionOnScreen);
                }
                y -= element.bounds.Height + 16;
            }
        }

        public List<OptionsElement> GetElementsToShow()
        {
            List<OptionsElement> menuElements = tabs[tabIndex].GetElements();
            List<OptionsElement> elements = new List<OptionsElement>();
            int y = 16;
            for(int i = firstIndex; i < menuElements.Count;i++)
            {
                OptionsElement element = menuElements[i];
                if(y + element.bounds.Height < this.height)
                {
                    y += element.bounds.Height + 16;
                    elements.Add(element);
                }
            }
            return elements;
        }

        public bool CanDrawAll()
        {
            List<OptionsElement> menuElements = tabs[tabIndex].GetElements();
            int y = 16;
            for (int i = firstIndex; i < menuElements.Count; i++)
            {
                OptionsElement element = menuElements[i];
                if (y + element.bounds.Height < this.height)
                {
                    y += element.bounds.Height + 16;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void TryToChangeTab(int which)
        {
            if(tabIndex != which)
            {
                tabIndex = which;
                firstIndex = 0;
                Game1.playSound("drumkit6");
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true){}
    }
}
