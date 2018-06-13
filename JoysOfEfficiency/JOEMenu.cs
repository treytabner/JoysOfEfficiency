﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

using JoysOfEfficiency.OptionsElements;
using JoysOfEfficiency.Utils;

namespace JoysOfEfficiency
{
    public class JOEMenu : IClickableMenu
    {
        private static readonly string[] fpsLocationStringKey = new string[] { "option.tl", "option.bl", "option.br", "option.tr" };

        private ModEntry mod;

        private ITranslationHelper translation;
        private readonly IMonitor mon;

        private readonly List<MenuTab> tabs = new List<MenuTab>();

        private ClickableTextureComponent upCursor;
        private ClickableTextureComponent downCursor;

        private Rectangle tabAutomation;
        private Rectangle tabCheats;
        private Rectangle tabControls;

        private int tabIndex = 0;
        private int firstIndex = 0;

        private readonly SpriteFont font = Game1.smallFont;
        private readonly string tabAutomationString;
        private readonly string tabCheatsString;
        private readonly string tabControlsString;

        private bool isListening;

        private bool isFirstTime;
        private ModifiedInputListener listener = null;

        public JOEMenu(int width, int height, ModEntry mod) : base(Game1.viewport.Width / 2 - width / 2, Game1.viewport.Height / 2 - height / 2, width, height, true)
        {

            this.mod = mod;
            translation = mod.Helper.Translation;
            upCursor = new ClickableTextureComponent("up-arrow", new Rectangle(xPositionOnScreen + this.width + Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            downCursor = new ClickableTextureComponent("down-arrow", new Rectangle(xPositionOnScreen + this.width + Game1.tileSize / 4, yPositionOnScreen + this.height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);

            tabAutomationString = translation.Get("tab.automation");
            Vector2 Size = font.MeasureString(tabAutomationString);
            tabAutomation = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen, ((int)Size.X + 32), 64);

            tabCheatsString = translation.Get("tab.cheats");
            Size = font.MeasureString(tabCheatsString);
            tabCheats = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen + 68, ((int)Size.X + 32), 64);

            tabControlsString = translation.Get("tab.controls");
            Size = font.MeasureString(tabControlsString);
            tabControls = new Rectangle(xPositionOnScreen - (int)Size.X - 20, yPositionOnScreen + 136, ((int)Size.X + 32), 64);

            {
                //Automation Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new LabelComponent("Balanced Mode"));
                tab.AddOptionsElement(new ModifiedCheckBox("BalancedMode", 20, ModEntry.Conf.BalancedMode, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Mine Info GUI"));
                tab.AddOptionsElement(new ModifiedCheckBox("MineInfoGUI", 0, ModEntry.Conf.MineInfoGUI, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Gift Information Tooltip"));
                tab.AddOptionsElement(new ModifiedCheckBox("GiftInformation", 1, ModEntry.Conf.GiftInformation, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Water Nearby Crops"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoWaterNearbyCrops", 2, ModEntry.Conf.AutoWaterNearbyCrops, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoWaterRadius", 3, ModEntry.Conf.AutoWaterRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoWaterNearbyCrops || ModEntry.Conf.BalancedMode)));
                tab.AddOptionsElement(new ModifiedCheckBox("FindCanFromInventory", 16, ModEntry.Conf.FindCanFromInventory, OnCheckboxValueChanged, (i => !(ModEntry.Conf.AutoWaterNearbyCrops || ModEntry.Conf.AutoRefillWateringCan))));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Pet Nearby Animals/Pets"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoPetNearbyAnimals", 3, ModEntry.Conf.AutoPetNearbyAnimals, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoPetNearbyPets", 24, ModEntry.Conf.AutoPetNearbyPets, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoPetRadius", 4, ModEntry.Conf.AutoPetRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoPetNearbyAnimals || ModEntry.Conf.BalancedMode)));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Animal Door"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoAnimalDoor", 4, ModEntry.Conf.AutoAnimalDoor, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Fishing"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoFishing", 5, ModEntry.Conf.AutoFishing, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("CPUThresholdFishing", 0, (int)(ModEntry.Conf.CPUThresholdFishing * 10), 0, 5, OnSliderValueChanged, (() => !ModEntry.Conf.AutoFishing), Format));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Fishing Tweaks"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoReelRod", 6, ModEntry.Conf.AutoReelRod, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Fishing Info"));
                tab.AddOptionsElement(new ModifiedCheckBox("FishingInfo", 8, ModEntry.Conf.FishingInfo, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Gate"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoGate", 9, ModEntry.Conf.AutoGate, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Eat"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoEat", 10, ModEntry.Conf.AutoEat, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("StaminaToEatRatio", 1, (int)(ModEntry.Conf.StaminaToEatRatio * 10), 3, 8, OnSliderValueChanged, (() => !ModEntry.Conf.AutoEat), Format));
                tab.AddOptionsElement(new ModifiedSlider("HealthToEatRatio", 2, (int)(ModEntry.Conf.HealthToEatRatio * 10), 3, 8, OnSliderValueChanged, (() => !ModEntry.Conf.AutoEat), Format));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Harvest"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoHarvest", 11, ModEntry.Conf.AutoHarvest, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoHarvestRadius", 5, ModEntry.Conf.AutoHarvestRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoHarvest || ModEntry.Conf.BalancedMode)));
                tab.AddOptionsElement(new ModifiedCheckBox("ProtectNectarProducingFlower", 25, ModEntry.Conf.ProtectNectarProducingFlower, OnCheckboxValueChanged, i => !ModEntry.Conf.AutoHarvest));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Destroy Dead Crops"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoDestroyDeadCrops", 12, ModEntry.Conf.AutoDestroyDeadCrops, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Refill Watering Can"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoRefillWateringCan", 13, ModEntry.Conf.AutoRefillWateringCan, OnCheckboxValueChanged));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Collect Collectibles"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoCollectCollectibles", 14, ModEntry.Conf.AutoCollectCollectibles, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoCollectRadius", 6, ModEntry.Conf.AutoCollectRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoCollectCollectibles || ModEntry.Conf.BalancedMode)));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Shake Fruited Plants"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoShakeFruitedPlants", 15, ModEntry.Conf.AutoShakeFruitedPlants, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoShakeRadius", 7, ModEntry.Conf.AutoShakeRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoShakeFruitedPlants || ModEntry.Conf.BalancedMode)));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Dig Artifact Spot"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoDigArtifactSpot", 17, ModEntry.Conf.AutoDigArtifactSpot, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("AutoDigRadius", 8, ModEntry.Conf.AutoDigRadius, 1, 3, OnSliderValueChanged, (() => !ModEntry.Conf.AutoDigArtifactSpot || ModEntry.Conf.BalancedMode)));
                tab.AddOptionsElement(new ModifiedCheckBox("FindHoeFromInventory", 18, ModEntry.Conf.FindHoeFromInventory, OnCheckboxValueChanged, i => !ModEntry.Conf.AutoDigArtifactSpot));

                tab.AddOptionsElement(new EmptyLabel());
                tab.AddOptionsElement(new LabelComponent("Auto Deposit/Pull Machines"));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoDepositIngredient", 22, ModEntry.Conf.AutoDepositIngredient, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedCheckBox("AutoPullMachineResult", 23, ModEntry.Conf.AutoPullMachineResult, OnCheckboxValueChanged));
                tab.AddOptionsElement(new ModifiedSlider("MachineRadius", 10, ModEntry.Conf.MachineRadius, 1, 3, OnSliderValueChanged, (() => !(ModEntry.Conf.AutoPullMachineResult || ModEntry.Conf.AutoDepositIngredient) || ModEntry.Conf.BalancedMode)));

                tabs.Add(tab);
            }
            {
                //Cheats Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new LabelComponent("Fishing Tweaks"));
                tab.AddOptionsElement(new ModifiedCheckBox("MuchFasterBiting", 7, ModEntry.Conf.MuchFasterBiting, OnCheckboxValueChanged));
                tabs.Add(tab);
            }
            {
                //Controls Tab
                MenuTab tab = new MenuTab();
                tab.AddOptionsElement(new LabelComponent("Settings Menu"));
                tab.AddOptionsElement(new ModifiedInputListener(this, "KeyShowMenu", 0, ModEntry.Conf.KeyShowMenu, translation, OnInputListnerChanged, OnStartListening));
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
            if (index == 0)
            {
                ModEntry.Conf.KeyShowMenu = value;
            }
            mod.WriteConfig();
            isListening = false;
            listener = null;
        }
        private void OnCheckboxValueChanged(int index, bool value)
        {
            switch (index)
            {
                case 0: ModEntry.Conf.MineInfoGUI = value; break;
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
                case 14: ModEntry.Conf.AutoCollectCollectibles = value; break;
                case 15: ModEntry.Conf.AutoShakeFruitedPlants = value; break;
                case 16: ModEntry.Conf.FindCanFromInventory = value; break;
                case 17: ModEntry.Conf.AutoDigArtifactSpot = value; break;
                case 18: ModEntry.Conf.FindHoeFromInventory = value; break;
                case 20: ModEntry.Conf.BalancedMode = value; break;
                case 22: ModEntry.Conf.AutoDepositIngredient = value; break;
                case 23: ModEntry.Conf.AutoPullMachineResult = value; break;
                case 24: ModEntry.Conf.AutoPetNearbyPets = value; break;
                case 25: ModEntry.Conf.ProtectNectarProducingFlower = value; break;
                default: return;
            }
            mod.WriteConfig();
        }
        private void OnSliderValueChanged(int index, int value)
        {
            if (index == 0)
            {
                ModEntry.Conf.CPUThresholdFishing = value / 10.0f;
            }
            if (index == 1)
            {
                ModEntry.Conf.StaminaToEatRatio = value / 10.0f;
            }
            if (index == 2)
            {
                ModEntry.Conf.HealthToEatRatio = value / 10.0f;
            }
            if (index == 3)
            {
                ModEntry.Conf.AutoWaterRadius = value;
            }
            if (index == 4)
            {
                ModEntry.Conf.AutoPetRadius = value;
            }
            if (index == 5)
            {
                ModEntry.Conf.AutoHarvestRadius = value;
            }
            if (index == 6)
            {
                ModEntry.Conf.AutoCollectRadius = value;
            }
            if (index == 7)
            {
                ModEntry.Conf.AutoShakeRadius = value;
            }
            if (index == 8)
            {
                ModEntry.Conf.AutoDigRadius = value;
            }
            if (index == 10)
            {
                ModEntry.Conf.MachineRadius = value;
            }
            mod.WriteConfig();
        }

        private string Format(int id, int value)
        {
            if (id >= 0 && id < 3)
            {
                return string.Format("{0:f1}", value / 10f);
            }
            else if (id == 11)
            {
                return mod.Helper.Translation.Get(fpsLocationStringKey[value]);
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
            else if (direction < 0 && downCursor.visible)
            {
                Game1.playSound("shwip");
                firstIndex++;
            }
        }

        public override void draw(SpriteBatch b)
        {
            int x = 16, y = 16;


            drawTextureBox(b, tabAutomation.Left, tabAutomation.Top, tabAutomation.Width, tabAutomation.Height, Color.White * (tabIndex == 0 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabAutomationString, new Vector2(tabAutomation.Left + 16, tabAutomation.Top + (tabAutomation.Height - font.MeasureString(tabAutomationString).Y) / 2), Color.Black * (tabIndex == 0 ? 1.0f : 0.6f));

            drawTextureBox(b, tabCheats.Left, tabCheats.Top, tabCheats.Width, tabCheats.Height, Color.White * (tabIndex == 1 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabCheatsString, new Vector2(tabCheats.Left + 16, tabCheats.Top + (tabCheats.Height - font.MeasureString(tabCheatsString).Y) / 2), Color.Black * (tabIndex == 1 ? 1.0f : 0.6f));

            drawTextureBox(b, tabControls.Left, tabControls.Top, tabControls.Width, tabControls.Height, Color.White * (tabIndex == 2 ? 1.0f : 0.6f));
            b.DrawString(Game1.smallFont, tabControlsString, new Vector2(tabControls.Left + 16, tabControls.Top + (tabControls.Height - font.MeasureString(tabControlsString).Y) / 2), Color.Black * (tabIndex == 2 ? 1.0f : 0.6f));

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

            if (isListening)
            {
                Point size = listener.GetListeningMessageWindowSize();
                drawTextureBox(b, (Game1.viewport.Width - size.X) / 2, (Game1.viewport.Height - size.Y) / 2, size.X, size.Y, Color.White);
                listener.DrawStrings(b, (Game1.viewport.Width - size.X) / 2, (Game1.viewport.Height - size.Y) / 2);
            }

            Util.DrawCursor();
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
            else if (key == Keys.Escape)
            {
                CloseMenu();
            }
            else if (key == ModEntry.Conf.KeyShowMenu)
            {
                if (!isFirstTime)
                {
                    isFirstTime = true;
                    return;
                }
                CloseMenu();
            }
        }

        private void CloseMenu()
        {
            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (isListening)
            {
                return;
            }
            if (tabAutomation.Contains(x, y))
            {
                TryToChangeTab(0);
                return;
            }
            if (tabCheats.Contains(x, y))
            {
                if (!ModEntry.Conf.BalancedMode)
                    TryToChangeTab(1);
                else
                    Game1.playSound("coin");
                return;
            }
            if (tabControls.Contains(x, y))
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
                if (element.bounds.Contains(x - xPositionOnScreen - element.bounds.X / 2, y - yPositionOnScreen - element.bounds.Y / 2))
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
            for (int i = firstIndex; i < menuElements.Count; i++)
            {
                OptionsElement element = menuElements[i];
                int hElem = element is ModifiedSlider ? element.bounds.Height + 4 : element.bounds.Height;
                if (y + hElem < height)
                {
                    y += hElem + 16;
                    elements.Add(element);
                }
                else
                {
                    break;
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
                int hElem = element is ModifiedSlider ? element.bounds.Height + 4 : element.bounds.Height;
                if (y + hElem < height)
                {
                    y += hElem + 16;
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
            if (tabIndex != which)
            {
                tabIndex = which;
                firstIndex = 0;
                Game1.playSound("drumkit6");
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) { }
    }
}