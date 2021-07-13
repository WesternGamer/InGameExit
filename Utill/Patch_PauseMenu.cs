using HarmonyLib;
using Sandbox;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System;
using System.Text;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace InGameExit.Utill
{


	[HarmonyPatch(typeof(MyGuiScreenMainMenu), "CreateInGameMenu")]
	public static class Patch_CreateMainMenu
	{
		public static void Postfix(MyGuiScreenMainMenu __instance)
		{
			Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
			Vector2 leftButtonPositionOrigin = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM) + new Vector2(minSizeGui.X / 2f, 0f) + new Vector2(15f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;

			MyGuiControlButton openBtn = new MyGuiControlButton(new Vector2(leftButtonPositionOrigin.X + 0.2f, 0.999f), MyGuiControlButtonStyleEnum.Default, originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, text: new StringBuilder("Exit To Windows"), onButtonClick: ExitGame)
			{
				BorderEnabled = false,
				BorderSize = 0,
				BorderHighlightEnabled = false,
				BorderColor = Vector4.Zero
			};
			__instance.Controls.Add(openBtn);
		}

		private static void ExitGame(MyGuiControlButton btn)
		{
			
			MyGuiScreenMessageBox myGuiScreenMessageBox = ((!Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Are you sure you want to disconnect and exit the game?"), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback) : ((MySession.Static.Settings.EnableSaving && Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.MessageBoxTextSaveChangesBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuMessageBoxCallback) : MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextCampaignBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback)));
			myGuiScreenMessageBox.SkipTransition = true;
			myGuiScreenMessageBox.InstantClose = false;
			MyGuiSandbox.AddScreen(myGuiScreenMessageBox);
		}

		private static void OnExitToMainMenuMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
		{
			switch (callbackReturn)
			{
				case MyGuiScreenMessageBox.ResultEnum.YES:
					MyAudio.Static.Mute = true;
					MyAudio.Static.StopMusic();
					MyAsyncSaving.Start(delegate
					{
						MySandboxGame.Static.OnScreenshotTaken += UnloadAndExitAfterScreeshotWasTaken;
					});
					break;
				case MyGuiScreenMessageBox.ResultEnum.NO:
					MyAudio.Static.Mute = true;
					MyAudio.Static.StopMusic();
					UnloadAndExitGame();
					break;
				case MyGuiScreenMessageBox.ResultEnum.CANCEL:
					break;
			}
		}

		private static void OnExitToMainMenuFromCampaignMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
		{
			if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
			{
				MyAudio.Static.Mute = true;
				MyAudio.Static.StopMusic();
				UnloadAndExitGame();
			}
			
		}

		private static void UnloadAndExitAfterScreeshotWasTaken(object sender, EventArgs e)
		{
			MySandboxGame.Static.OnScreenshotTaken -= UnloadAndExitAfterScreeshotWasTaken;
			UnloadAndExitGame();
		}

		private static void UnloadAndExitGame()
        {
			MySessionLoader.Unload();
			MySandboxGame.Config.ControllerDefaultOnStart = MyInput.Static.IsJoystickLastUsed;
			MySandboxGame.Config.Save();
			MyScreenManager.CloseAllScreensNowExcept(null);
			MySandboxGame.ExitThreadSafe();
		}
	}
}
