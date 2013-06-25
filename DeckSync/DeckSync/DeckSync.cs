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

namespace CollectionSync
{
    public class CollectionSync : BaseMod
    {

        private DeckBuilder2 deckBuilder = null;

        public static bool loaded = false;
		private bool inited = false;

		private GUISkin buttonSkin = (GUISkin)Resources.Load("_GUISkins/Lobby");

		private Dictionary<long, Card> allCardsDict = null;
		private Type deckBuilderType = typeof(DeckBuilder2);

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
						scrollsTypes["DeckBuilder2"].Methods.GetMethod("OnGUI")[0] // to draw gui buttons on the deckbuilder screen
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

				JsonReader reader = new JsonReader();
				var template = new { error=Boolean.FalseString, msg=String.Empty };
				var msg = reader.Read(e.Result,template);
					var hdr = "";
					if (msg.error == "true")
					{
						hdr = "Error Syncing Collection";
					} else {
						hdr = "Import Succeeded";
					}
				App.Popups.ShowOk(null, "fail", hdr, msg.msg, "Ok");
			};
			wc.TimeOut = 5000;
			//wc.DownloadStringAsync(new Uri("http://localhost:9000/collection/update?inGameName="+App.MyProfile.ProfileInfo.name+"&data=" + collectionData));
			wc.DownloadStringAsync(new Uri("http://www.scrollstoolbox.com:9000/collection/update?inGameName="+App.MyProfile.ProfileInfo.name+"&data=" + collectionData));
        }
	
	}

}
