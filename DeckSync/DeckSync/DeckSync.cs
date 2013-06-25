using Mono.Cecil;
using ScrollsModLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using JsonFx.Json;
using UnityEngine;

namespace DeckSync
{
    public class DeckSync : BaseMod
    {

        private DeckBuilder2 deckBuilder = null;

        public static bool loaded = false;
		private bool inited = false;

		private GUISkin buttonSkin = (GUISkin)Resources.Load("_GUISkins/Lobby");

		private Dictionary<long, Card> allCardsDict = null;
		private Type deckBuilderType = typeof(DeckBuilder2);
		private string username = "";
		private string ingamename = "";

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.targetMethod.Equals("OnGUI"))
            {
				if (deckBuilder == null)
				{
					deckBuilder = (DeckBuilder2)info.target;
				}

				GUI.skin = buttonSkin;
                GUIPositioner positioner3 = App.LobbyMenu.getSubMenuPositioner(1f, 5);
                GUI.skin = buttonSkin;
                if (LobbyMenu.drawButton(positioner3.getButtonRect(3f), "Sync Collection"))
                {
					FieldInfo initedInfo = deckBuilderType.GetField("inited", BindingFlags.NonPublic | BindingFlags.Instance);

					inited = (bool)initedInfo.GetValue(deckBuilder);
					if (inited)
					{
						FieldInfo deckListInfo = deckBuilderType.GetField("allCardsDict", BindingFlags.NonPublic | BindingFlags.Instance);
						allCardsDict = (Dictionary<long, Card>)deckListInfo.GetValue(deckBuilder);
					}

					var writer = new JsonWriter();
					string json = writer.Write(allCardsDict);

					this.loadFromWeb(json);
                }
            }

			if (info.targetMethod.Equals("login"))
			{
				this.username = (string)typeof(Login).GetField ("username", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

			}

			if (info.targetMethod.Equals("profileinfomessage"))
			{
				this.ingamename = (string)typeof(ProfileInfo).GetField ("name", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

			}
        }

        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            returnValue = null;
            return false;
        }

        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
			try
			{
				return new MethodDefinition[] {
						scrollsTypes["DeckBuilder2"].Methods.GetMethod("OnGUI")[0], // to draw gui buttons on the deckbuilder screen
						scrollsTypes["Login"].Methods.GetMethod("login")[0], //this may seem scary, but i swear im just getting the username
						scrollsTypes["ProfileInfoMessage"].Methods.GetMethod("profileinfomessage")[0]
				};
			}
			catch
			{
				return new MethodDefinition[] { };
			}
        }

        public static string GetName()
        {
            return "CollectionSync";
        }

        public static int GetVersion()
        {
            return 1;
        }

        public void PopupCancel(string popupType)
        {

        }

		private void loadFromWeb(String collectionData)
        {

			WebClientTimeOut wc = new WebClientTimeOut();
			wc.DownloadStringCompleted += (sender, e) =>
			{
				App.Popups.ShowOk(null, "fail", "Import failed", "That deck does not exist, or is deleted.", "Ok");
			};
			wc.TimeOut = 5000;
			wc.DownloadStringAsync(new Uri("http://localhost:9000/collection/update?ingamename="+this.ingamename+"&inGameName="+this.username+"&data=" + collectionData));
        }
	
	}

}
