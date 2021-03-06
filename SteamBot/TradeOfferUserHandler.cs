﻿using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;
using System;
using System.Collections.Generic;
using TradeAsset = SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset;

namespace SteamBot
{
    public class TradeOfferUserHandler : UserHandler
    {
        public TradeOfferUserHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override void OnNewTradeOffer(TradeOffer offer)
        {
            //receiving a trade offer 
			if (IsAdmin)
			{
				var myItems = offer.Items.GetMyItems();
				var theirItems = offer.Items.GetTheirItems();
				Log.Info("They want " + myItems.Count + " of my items.");
				Log.Info("And I will get " +  theirItems.Count + " of their items.");

				string tradeid;
				if (offer.Accept (out tradeid)) {
					Log.Success ("Accepted trade offer from Admin successfully : Trade ID: " + tradeid);
				}
			}
			else
			{
                //parse inventories of bot and other partner
                //either with webapi or generic inventory
                //Bot.GetInventory();
                //Bot.GetOtherInventory(OtherSID);

                var myItems = offer.Items.GetMyItems();
                var theirItems = offer.Items.GetTheirItems();
                Log.Info("They want " + myItems.Count + " of my items.");
                Log.Info("And I will get " +  theirItems.Count + " of their items.");

                //do validation logic etc
                if (DummyValidation(myItems, theirItems))
                {
                    string tradeid;
                    if (offer.Accept(out tradeid))
                    {
                        Log.Success("Accepted trade offer successfully : Trade ID: " + tradeid);
                    }
                }
                else
                {
                    // maybe we want different items or something

                    //offer.Items.AddMyItem(0, 0, 0);
                    //offer.Items.RemoveTheirItem(0, 0, 0);

					foreach (var item in offer.Items.GetMyItems())
					{
						offer.Items.RemoveMyItem((int)item.AppId, item.ContextId, item.AssetId);
					}

					foreach (var item in offer.Items.GetTheirItems())
					{
						
						offer.Items.RemoveTheirItem((int)item.AppId, item.ContextId, item.AssetId);
					}

                    if (offer.Items.NewVersion)
                    {
                        string newOfferId;
                        if (offer.CounterOffer(out newOfferId))
                        {
                            Log.Success("Counter offered successfully : New Offer ID: " + newOfferId);
                        }
                    }
                }
            }
        }

        public override void OnMessage(string message, EChatEntryType type)
        {
            if (IsAdmin)
            {
                //creating a new trade offer
                var offer = Bot.NewTradeOffer(OtherSID);

                //offer.Items.AddMyItem(0, 0, 0);
                if (offer.Items.NewVersion)
                {
                    string newOfferId;
                    if (offer.Send(out newOfferId))
                    {
                        Log.Success("Trade offer sent : Offer ID " + newOfferId);
                    }
                }

                //creating a new trade offer with token
                var offerWithToken = Bot.NewTradeOffer(OtherSID);

                //offer.Items.AddMyItem(0, 0, 0);
                if (offerWithToken.Items.NewVersion)
                {
                    string newOfferId;
                    // "token" should be replaced with the actual token from the other user
                    if (offerWithToken.SendWithToken(out newOfferId, "token"))
                    {
                        Log.Success("Trade offer sent : Offer ID " + newOfferId);
                    }
                }
            }
        }

        public override bool OnGroupAdd() { return false; }

        public override bool OnFriendAdd() { return IsAdmin; }

        public override void OnFriendRemove() { }

        public override void OnLoginCompleted() { }

        public override bool OnTradeRequest() { return false; }

        public override void OnTradeError(string error) { }

        public override void OnTradeTimeout() { }

        public override void OnTradeSuccess() { }

        public override void OnTradeInit() { }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeMessage(string message) { }

        public override void OnTradeReady(bool ready) { }

        public override void OnTradeAccept() { }

        private bool DummyValidation(List<TradeAsset> myAssets, List<TradeAsset> theirAssets)
        {
            //compare items etc
			if (myAssets.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
