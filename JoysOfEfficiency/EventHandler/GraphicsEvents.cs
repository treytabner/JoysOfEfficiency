﻿using System;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Huds;
using JoysOfEfficiency.Utils;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace JoysOfEfficiency.EventHandler
{
    internal class GraphicsEvents
    {
        private static Config Conf => InstanceHolder.Config;

        public void OnPostRenderHud(object sender, EventArgs args)
        {
            if (Game1.currentLocation is MineShaft shaft && Conf.MineInfoGui)
            {
                Util.DrawMineGui(shaft);
            }
            if (Conf.GiftInformation)
            {
                GiftInformationTooltip.DrawTooltip();
            }
            if (Conf.FishingProbabilitiesInfo && Game1.player.CurrentTool is FishingRod rod && rod.isFishing)
            {
                FishingProbabilitiesBox.PrintFishingInfo();
            }
            if (UpdateEvents.Paused && Conf.PauseWhenIdle)
            {
                PausedHud.DrawPausedHud();
            }
        }

        public void OnPostRenderGui(object sender, EventArgs args)
        {
            if (Game1.activeClickableMenu is BobberBar bar)
            {
                if (Conf.FishingInfo)
                {
                    Util.DrawFishingInfoBox(Game1.spriteBatch, bar, Game1.dialogueFont);
                }
                if (Conf.AutoFishing)
                {
                    Util.AutoFishing(bar);
                }
            }
            if (Conf.EstimateShippingPrice && Game1.activeClickableMenu is ItemGrabMenu menu)
            {
                Util.DrawShippingPrice(menu, Game1.dialogueFont);
            }
        }
    }
}