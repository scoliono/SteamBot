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
            //SendChatMessage(Bot.ChatResponse);
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
						Log.Success("Accepted trade offer successfully from "+offer.PartnerSteamId.ToString()+" : Trade ID: " + tradeid);
					}
				}
				else
				{
					if(offer.Decline()) Log.Success("Declined trade offer successfully");
				}
			}
		}
		private bool DummyValidation(List<TradeAsset> myAssets, List<TradeAsset> theirAssets)
		{
			if (myAssets.Count == 0)
			{
				GetOtherInventory();
				foreach (var item in theirAssets)
				{
					ushort i = OtherInventory.GetItem((ulong)item.AssetId).Defindex;
					if (i == 5000 || i == 5001 || i == 5002 || i == 5021)
					{
						continue;
					}
					else
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
    }
 
}

