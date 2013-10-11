using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
using System.Reflection;
using System.Collections.Generic;

namespace RandomDeck
{
    public class RandomDeck : BaseMod, IOkCancelCallback
    {
        private GUISkin buttonSkin = (GUISkin)Resources.Load("_GUISkins/Lobby");
        List<DeckInfo> decks;
        private bool randomDeck = false;
        private int randomDeckNumber = 0;

        //initialize everything here, Game is loaded at this point
        public RandomDeck()
        {
            List<DeckInfo> decks = new List<DeckInfo>();
        }


        public static string GetName()
        {
            return "RandomDeck";
        }

        public static int GetVersion()
        {
            return 1;
        }

        //only return MethodDefinitions you obtained through the scrollsTypes object
        //safety first! surround with try/catch and return an empty array in case it fails
        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["Lobby"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["GameActionManager"].Methods.GetMethod("ChooseDeck")[0]
                };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
        }


        public override void BeforeInvoke(InvocationInfo info)
        {
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.targetMethod.Equals("OnGUI") && info.target.GetType().ToString() == "Lobby")
            {
                GUI.skin = buttonSkin;
                GUIPositioner positioner = App.LobbyMenu.getSubMenuPositioner(1f, 1);

                if (LobbyMenu.drawButton(positioner.getButtonRect(3f), "Random Deck - " + randomDeck))
                {
                    randomDeck = !randomDeck;
                }
            }
            else if (info.targetMethod.Equals("ChooseDeck") && info.target.GetType().ToString() == "GameActionManager")
            {
                if (randomDeck)
                {
                    decks = (List<DeckInfo>)typeof(GameActionManager).GetField("decks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

                    if (decks.Count > 0)
                    {
                        bool existsValidDeck = false;
                        for (int i = 0; i < decks.Count; i++)
                        {
                            if (decks[i].valid)
                            {
                                existsValidDeck = true;
                                break;
                            }
                        }
                        if (!existsValidDeck)
                        {
                            App.Popups.ShowOk(this, "randeck_Prob", "Random Deck", "There is no valid Deck in your decklist.", "OK");
                        }

                        do
                        {
                            randomDeckNumber = UnityEngine.Random.Range(0, decks.Count - 1);
                        } while (decks[randomDeckNumber].valid == false);

                        App.Popups.ShowOkCancel(this, "randeck_OK", "Random Deck", "A random deck has been chosen\n" + decks[randomDeckNumber].name, "Start Game", "Cancel");

                        //typeof(Popups).GetField("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(App.Popups, 0);
                        //typeof(Popups).GetMethod("HidePopup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(App.Popups, new object[] {});   
                    }
                    else
                    {
                        App.Communicator.SelectedDeck = null;
                        App.Popups.ShowOk(this, "randeck_Prob", "Random Deck", "A random deck could not been chosen\n Game couldn't be started", "OK");
                    }
                }
            }

        }

        //override only when needed
        /*

        public override void ReplaceMethod (InvocationInfo info, out object returnValue)
        {
            returnValue = null;
        }

        public override bool WantsToReplace (InvocationInfo info)
        {
            return false;
        }

        */

        public void PopupOk(string popupType)
        {
            if (popupType.Equals("randeck_OK"))
            {
                App.GameActionManager.PopupDeckChosen(decks[randomDeckNumber]);
            }
        }

        public void PopupCancel(string popupType)
        {
        }
    }
}

