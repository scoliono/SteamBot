using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using SteamTrade.TradeOffer;
using SteamTrade.TradeWebAPI;
using TradeAsset = SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset;

namespace SteamBot
{
    public class SimpleUserHandler : UserHandler
    {
        public TF2Value AmountAdded;

        public SimpleUserHandler (Bot bot, SteamID sid) : base(bot, sid) {}

        public override bool OnGroupAdd()
        {
            return false;
        }

        public override bool OnFriendAdd () 
        {
            return true;
        }

        public override void OnLoginCompleted()
        {
        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

        public override void OnFriendRemove () {}
        
        public override void OnMessage (string message, EChatEntryType type) 
        {
            SendChatMessage(Bot.ChatResponse);
        }

        public override bool OnTradeRequest() 
        {
            return true;
        }
        
        public override void OnTradeError (string error) 
        {
            SendChatMessage("There was an error: {0}.", error);
            Log.Warn (error);
        }
        
        public override void OnTradeTimeout () 
        {
            SendChatMessage("Sorry, but you were AFK and the trade was canceled.");
            Log.Info ("User was kicked because he was AFK.");
        }
        
        public override void OnTradeInit() 
        {
            SendTradeMessage("Success. Please put up your items.");
        }
        
        public override void OnTradeAddItem (Schema.Item schemaItem, Inventory.Item inventoryItem)
		{
			if (Validate())
			{
				Trade.SetReady(true);
			}
		}
        
        public override void OnTradeRemoveItem (Schema.Item schemaItem, Inventory.Item inventoryItem) {}
        
        public override void OnTradeMessage (string message)
		{
			Log.Info("New TradeMessage: "+message);
		}
        
        public override void OnTradeReady (bool ready) 
        {
            if (!ready)
            {
                Trade.SetReady (false);
            }
            else
            {
                if(Validate ())
                {
                    Trade.SetReady (true);
                }
                //SendTradeMessage("Scrap: {0}", AmountAdded.ScrapTotal);
            }
        }

        public override void OnTradeSuccess()
        {
            // Trade completed successfully
            Log.Success("Trade Complete.");
        }

        public override void OnTradeAccept() 
        {
            if (Validate() || IsAdmin)
            {
                //Even if it is successful, AcceptTrade can fail on
                //trades with a lot of items so we use a try-catch
                try {
                    if (Trade.AcceptTrade())
                        Log.Success("Trade Accepted!");
                }
                catch {
                    Log.Warn ("The trade might have failed, but we can't be sure.");
                }
            }
        }

        public bool Validate ()
        {            
            AmountAdded = TF2Value.Zero;
            
            List<string> errors = new List<string> ();
            
            foreach (TradeUserAssets asset in Trade.OtherOfferedItems)
            {
                var item = Trade.OtherInventory.GetItem(asset.assetid);
				if (item.Defindex == 5000)
					AmountAdded += TF2Value.Scrap;
				else if (item.Defindex == 5001)
					AmountAdded += TF2Value.Reclaimed;
				else if (item.Defindex == 5002)
					AmountAdded += TF2Value.Refined;
				else if (item.Defindex == 5021)
					AmountAdded += (TF2Value.Refined * 19);
                else
                {
                    var schemaItem = Trade.CurrentSchema.GetItem (item.Defindex);
                    errors.Add (schemaItem.Name + " is not a metal or key.");
                }
            }
            
            if (AmountAdded == TF2Value.Zero)
            {
                errors.Add ("You must put up metal/keys.");
            }
            
            // send the errors
            if (errors.Count != 0)
                SendTradeMessage("There were errors in your trade: ");
            foreach (string error in errors)
            {
                SendTradeMessage(error);
            }
            
            return errors.Count == 0;
        }
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

