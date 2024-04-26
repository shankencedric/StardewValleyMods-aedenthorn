using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Collections.Generic;

namespace QuickResponses
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
        }

        /// <summary> Sets the value of a specified field, if it exists, from Helper.Reflection into the `storage` variable.
        /// Assures `required` field is set to false, to be able to null-check.
        /// Returns bool for success.</summary>
        /// Made under the assumption that GetField method would return null. (Read as: I'm dumbo)
        private bool RetrieveFieldValue<T>(object obj, string name, ref T storage)
        {
            IReflectedField<T> field = Helper.Reflection.GetField<T>(obj, name, false);
            bool valid = field != null;
            if (valid)
                storage = field.GetValue();
            return valid;
        }

        private void Display_RenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (!Config.ShowNumbers || Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox))
                return;
            bool isQuestion = true;
            if (!RetrieveFieldValue(Game1.activeClickableMenu, "isQuestion", ref isQuestion))
                return;
            if (!isQuestion)
                return;

            DialogueBox db = Game1.activeClickableMenu as DialogueBox;
            int charIdxInDialogue = 0;
            bool isTransitioning = true;
            if (!RetrieveFieldValue(db, "characterIndexInDialogue", ref charIdxInDialogue) ||
                !RetrieveFieldValue(db, "transitioning", ref isTransitioning))
                return;
            if (charIdxInDialogue < db.getCurrentString().Length - 1 || isTransitioning)
                return;

            int x = 0;
            int y = 0;
            int heightForQuestions = 0;
            Response[] responses = {};
            if (!RetrieveFieldValue(Game1.activeClickableMenu, "x", ref x) ||
                !RetrieveFieldValue(Game1.activeClickableMenu, "y", ref y) ||
                !RetrieveFieldValue(Game1.activeClickableMenu, "heightForQuestions", ref heightForQuestions) ||
                !RetrieveFieldValue(Game1.activeClickableMenu, "responses", ref responses))
                return;

            int count = responses.Length;
            int responseY = y - (heightForQuestions - Game1.activeClickableMenu.height) + SpriteText.getHeightOfString((Game1.activeClickableMenu as DialogueBox).getCurrentString(), Game1.activeClickableMenu.width - 16) + 44;
            for (int i = 0; i < count; i++)
            {
                e.SpriteBatch.DrawString(Game1.dialogueFont, $"{i + 1}", new Vector2(x  - 0.69f, responseY - 6.9f), Config.NumberColor, 0, Vector2.Zero, Config.NumberScale, SpriteEffects.None, 0.86f);
                responseY += SpriteText.getHeightOfString(responses[i].responseText, Game1.activeClickableMenu.width) + 16;
            }

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox))
                return;
            bool isQuestion = true;
            if (!RetrieveFieldValue(Game1.activeClickableMenu, "isQuestion", ref isQuestion))
                return;
            if (!isQuestion)
                return;

            Response[] responses = {};
            if (!RetrieveFieldValue(Game1.activeClickableMenu, "responses", ref responses))
                return;

            List<SButton> sbs = new List<SButton> { SButton.D1, SButton.D2, SButton.D3, SButton.D4, SButton.D5, SButton.D6, SButton.D7, SButton.D8, SButton.D9, SButton.D0 };
            if (sbs.Contains(e.Button) && sbs.IndexOf(e.Button) < responses.Length)
            {
                Monitor.Log($"Pressed {e.Button} key on question dialogue");
                IReflectedField<int> selectedResponseField = Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "selectedResponse", false);
                if (selectedResponseField == null)
                    return;
                selectedResponseField.SetValue(sbs.IndexOf(e.Button));
                Game1.activeClickableMenu.receiveLeftClick(0, 0, true);
                Helper.Input.Suppress(e.Button);
                return;
            }

            if (e.Button == Config.SelectFirstResponseKey && responses.Length == 2)
            {
                Monitor.Log($"Pressed {Config.SelectFirstResponseKey} button key on question dialogue");
                IReflectedField<int> selectedResponseField = Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "selectedResponse", false);
                if (selectedResponseField == null)
                    return;
                selectedResponseField.SetValue(0);
                Game1.activeClickableMenu.receiveLeftClick(0, 0, true);
                Helper.Input.Suppress(e.Button);
                return;
            }
        }
    }
}
