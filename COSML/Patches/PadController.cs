#pragma warning disable CS0649, CS0414, IDE0052, IDE0044

using MonoMod;
using Rewired;
using UnityEngine;

namespace COSML.Patches
{
    [MonoModPatch("global::PadController")]
    public class PadController : global::PadController
    {
        [MonoModIgnore]
        private Cursors cursors;
        [MonoModIgnore]
        private AbstractPadInteractive selected;
        [MonoModIgnore]
        private AbstractPadInteractive previousFocus;
        [MonoModIgnore]
        private Place previousPlace;
        [MonoModIgnore]
        private OverableUI onUI;
        [MonoModIgnore]
        private AbstractPadInteractiveBrowser interactiveBrowser;
        [MonoModIgnore]
        private AbstractUIBrowser uiBrowser;
        [MonoModIgnore]
        private AbstractDragBrowser onDragBrowser;
        [MonoModIgnore]
        private AsbtractHideFlag previousFlag;
        [MonoModIgnore]
        private PadSpot padSpot;
        [MonoModIgnore]
        private WalkSpot previousHideSpot;
        [MonoModIgnore]
        private PadStick padStick;
        [MonoModIgnore]
        private bool onAction;
        [MonoModIgnore]
        private bool lockRadar;
        [MonoModIgnore]
        private bool needExitUI;

        public PadController(Cursors newCursors, Camera newMainCamera) : base(newCursors, newMainCamera) { }

        internal void SetUIBrowser(AbstractUIBrowser browser)
        {
            uiBrowser = browser;
        }

        public extern void orig_Loop();
        public override void Loop()
        {
            if (!(onDragBrowser?.TryQuitDrag(onAction, player.GetButton("Action"), IsShortcutEscape(), false) ?? false) && (onDragBrowser == null || onDragBrowser.AllowShortcut()))
            {
                GameController instance = (Patches.GameController)GameController.GetInstance();
                UIController uiController = (UIController)instance.GetUIController();
                if (uiController.keyboard?.IsOpen() ?? false)
                {
                    if (IsShortcutEscape())
                    {
                        instance.PlayGlobalSound("Play_generic_annul", false);
                        uiController.keyboard.Hide();
                        OnShortcuts();
                        return;
                    }
                    if (IsShortcutValid() || (GetControllerType() != ControllerType.MOUSE && IsShortcutPause()))
                    {
                        instance.PlayGlobalSound("Play_generic_valid", false);
                        uiController.keyboard.Valid();
                        OnShortcuts();
                        return;
                    }
                }
            }

            orig_Loop();
        }

        public override bool TrySwitchControllerType(ref InputsController oldInputs, bool isAnnoatationOpen, bool force)
        {
            GameController instance = (Patches.GameController)GameController.GetInstance();
            UIController uiController = (Patches.UIController)instance.GetUIController();
            if (
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2) ||
                (!uiController.annotationUI.IsOpen() && !isAnnoatationOpen && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace))) ||
                ((!uiController.keyboard?.IsOpen() ?? false) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Backspace))) ||
                HasTouch(instance.GetPlateformController()) || force
            )
            {
                selected = null;
                previousFocus = null;
                previousPlace = null;
                if (onUI != null)
                {
                    onUI.AutoExit(true);
                    onUI = null;
                }
                interactiveBrowser = null;
                if (uiBrowser != null)
                {
                    uiBrowser.QuitBrowser();
                    uiBrowser = null;
                }
                if (onDragBrowser != null)
                {
                    onDragBrowser.ForceQuit();
                    onDragBrowser = null;
                }
                previousFlag = null;
                previousHideSpot = null;
                padSpot = null;
                instance.GetUIController().toolTipUI.ForceExitRune();
                if (instance.GetPlateformController().HasTouch()) oldInputs = new TouchController(cursors, mainCamera, false);
                else oldInputs = new MouseController(cursors, mainCamera);
                return true;
            }
            return false;
        }

        private bool HasTouch(AbstractPlateform plateform)
        {
            if (!plateform.HasTouch()) return false;

            for (int i = 0; i < ReInput.touch.touchCount; i++)
            {
                Touch touch = ReInput.touch.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) return true;
            }

            return false;
        }

        public extern AbstractSpot orig_SearchNewSpot(Vector3 defaultPosition, out InteractiveCursorType outCursorType, out OverableInteractive newInteractive, out ItemReceiver itemReceiver);
        public new AbstractSpot SearchNewSpot(Vector3 defaultPosition, out InteractiveCursorType outCursorType, out OverableInteractive newInteractive, out ItemReceiver itemReceiver)
        {
            GameController instance = (GameController)GameController.GetInstance();
            bool flag = instance.IsInGame();
            PlayerController playerController = instance.GetPlayerController();
            HumanoidMove humanoidMove;
            AbstractSpot abstractSpot;
            AbstractPadUI abstractPadUI;
            if (playerController == null)
            {
                humanoidMove = null;
                abstractSpot = null;
                abstractPadUI = null;
            }
            else
            {
                humanoidMove = playerController.GetHumanoid();
                abstractSpot = humanoidMove.GetCurrentSpot();
                if (flag) abstractPadUI = abstractSpot.GetUIBrowser();
                else abstractPadUI = null;
            }
            if (flag && uiBrowser != null && abstractPadUI != null && uiBrowser.GetBrowserId() != abstractPadUI.GetBrowserId())
            {
                uiBrowser.QuitBrowser();
                uiBrowser = null;
            }
            if (onDragBrowser != null)
            {
                onDragBrowser.UpdateSelectedInteractive(ref selected, padStick, mainCamera, IsCursorOff());
                onDragBrowser.LoopDrag();
                newInteractive = null;
                outCursorType = InteractiveCursorType.MOVE;
                instance.GetUIController().radarUI.Hide();
                itemReceiver = null;
                return null;
            }
            if (!flag || abstractPadUI != null)
            {
                UIController uiController = (UIController)instance.GetUIController();
                if (abstractPadUI == null)
                {
                    if (uiController.keyboard?.IsOpen() ?? false)
                    {
                        abstractPadUI = uiController.keyboard;
                    }
                    else if (uiController.mainMenu.IsOpen())
                    {
                        abstractPadUI = uiController.mainMenu;
                    }
                    else if (uiController.annotationUI.IsOpen())
                    {
                        abstractPadUI = uiController.annotationUI;
                    }
                    else if (instance.GetJournal().IsOpened())
                    {
                        abstractPadUI = instance.GetJournal();
                    }
                    else if (instance.GetInventory().IsOpened())
                    {
                        abstractPadUI = instance.GetInventory();
                    }
                    else if (instance.GetCameraController().GetColorEffect().IsTerminal())
                    {
                        abstractPadUI = uiController.GetTerminalUI();
                    }
                    else if (uiController.GetVisiocodeUI().IsDisplayed())
                    {
                        abstractPadUI = uiController.GetVisiocodeUI();
                    }
                }
                if (abstractPadUI != null)
                {
                    if (uiBrowser == null) uiBrowser = abstractPadUI.GetBrowser();
                    else if (uiBrowser.GetBrowserId() != abstractPadUI.GetBrowserId())
                    {
                        padStick.ForceRestartTime();
                        uiBrowser.QuitBrowser();
                        uiBrowser = abstractPadUI.GetBrowser();
                    }
                    uiBrowser?.RefreshSelectedUI(onUI, padStick, abstractPadUI.GetBulles(), IsCursorOff());
                }
                else if (uiBrowser != null)
                {
                    uiBrowser.QuitBrowser();
                    uiBrowser = null;
                }
                newInteractive = null;
                outCursorType = InteractiveCursorType.MOVE;
                itemReceiver = null;
                return null;
            }
            AbstractPadInteractive abstractPadInteractive = selected;
            bool flag2 = instance.GetUIController().detectionHaloUI.IsOnAttack() && !previousPlace.allowUseOnAttack;
            bool flag3 = false;
            previousHideSpot = SearchHideSpot(instance, humanoidMove, abstractSpot, ref flag3);
            if (!humanoidMove.IsOnAction() && abstractSpot.IsFinish())
            {
                if (!playerController.IsOnCinematic() || !SearchPnjTalking())
                {
                    interactiveBrowser = padSpot.GetRefreshedBrowser(interactiveBrowser);
                }
            }
            else if (!abstractSpot.GetType().Equals(typeof(FollowSpot)) || !((FollowSpot)abstractSpot).IsInPosition() || !SearchPnjTalking())
            {
                interactiveBrowser = abstractSpot.GetRefreshedBrowser(interactiveBrowser);
            }
            if (selected == null || interactiveBrowser == null || !interactiveBrowser.IsInteractiveAlwaysAvailable(selected, mainCamera))
            {
                selected = null;
            }
            if (onUI != null && onUI.AutoExit(true) && needExitUI)
            {
                onUI = null;
                needExitUI = false;
            }
            interactiveBrowser?.UpdateSelectedInteractive(ref selected, padStick, mainCamera, IsCursorOff());
            bool flag4 = false;
            int radar = instance.GetPlateformController().GetOptions().GetRadar();
            if (radar != 0)
            {
                if (radar == 1)
                {
                    if (player.GetButtonDown("Next")) lockRadar = !lockRadar;
                    flag4 = lockRadar;
                }
            }
            else
            {
                flag4 = player.GetButton("Next");
                lockRadar = false;
            }
            if (interactiveBrowser != null && flag4 && !IsCursorOff() && !flag2)
            {
                instance.GetUIController().radarUI.DisplayInteractives(selected, interactiveBrowser.GetRadarInteractives(), false, false);
            }
            else
            {
                instance.GetUIController().radarUI.Hide();
            }
            if (selected == null || flag3)
            {
                onUI = null;
                if (selected == null)
                {
                    outCursorType = InteractiveCursorType.MOVE;
                }
                else
                {
                    outCursorType = selected.GetCursorType();
                }
                newInteractive = selected;
                if (previousFocus != null)
                {
                    instance.padPointer.transform.position = previousFocus.GetCursorScreenPosition(mainCamera);
                }
                itemReceiver = null;
                if (abstractSpot.GetSpotType(humanoidMove) == SpotType.FOLLOW)
                {
                    previousHideSpot = null;
                    return new WalkSpot(humanoidMove.transform.position, humanoidMove, false, true);
                }
                if (flag3 && previousHideSpot != null && previousHideSpot.GetWalkSpotParameters().onAngle)
                {
                    onAction = true;
                }
                return previousHideSpot;
            }
            else
            {
                if (flag2)
                {
                    outCursorType = InteractiveCursorType.MOVE;
                    itemReceiver = null;
                    previousHideSpot = null;
                    newInteractive = null;
                    return null;
                }
                PNJ pnj = selected.GetFollowable();
                if (pnj != null && pnj.FollowableByPlayer())
                {
                    pnj = pnj.GetEndQueue();
                    selected = pnj;
                }
                else
                {
                    pnj = null;
                }
                if (abstractPadInteractive == null || abstractPadInteractive.GetInstanceId() != selected.GetInstanceId())
                {
                    newInteractive = selected;
                    instance.padPointer.SetInteractiveHasChanged();
                }
                else
                {
                    newInteractive = null;
                }
                onUI = selected.TryEnterUI(newInteractive != null);
                if (selected.DisplayCursor())
                {
                    previousFocus = selected;
                }
                if (previousFocus != null)
                {
                    instance.padPointer.transform.position = previousFocus.GetCursorScreenPosition(mainCamera);
                }
                if (pnj == null)
                {
                    instance.targetCursor.Hide();
                    outCursorType = selected.GetCursorType();
                    itemReceiver = selected.GetItemReceiver();
                    previousHideSpot = null;
                    return selected.GetNewSpot(humanoidMove);
                }
                instance.targetCursor.Show(pnj);
                outCursorType = InteractiveCursorType.MOVE;
                itemReceiver = null;
                previousHideSpot = null;
                return new FollowSpot(pnj, true, 0f, -1f);
            }

        }

        private extern WalkSpot orig_SearchHideSpot(GameController gameController, HumanoidMove humanoid, AbstractSpot currentSpot, ref bool forceAction);
        private WalkSpot SearchHideSpot(GameController gameController, HumanoidMove humanoid, AbstractSpot currentSpot, ref bool forceAction)
        {
            return orig_SearchHideSpot(gameController, humanoid, currentSpot, ref forceAction);
        }

        private extern bool orig_SearchPnjTalking();
        private bool SearchPnjTalking()
        {
            return orig_SearchPnjTalking();
        }
    }
}
