using System;
using System.Collections;
using GlobalEnums;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UnityEngine.UI
{
    public class MenuSelectable : Selectable, ISelectHandler, IEventSystemHandler, IDeselectHandler, ICancelHandler, IPointerExitHandler
    {
        public delegate void OnSelectedEvent(MenuSelectable self);

        [Header("On Cancel")]
        public CancelAction cancelAction;

        [Header("Fleurs")]
        public Animator leftCursor;

        public Animator rightCursor;

        [Header("Highlight")]
        public Animator selectHighlight;

        public bool playSubmitSound = true;

        [Header("Description Text")]
        public Animator descriptionText;

        //protected MenuAudioController uiAudioPlayer;

        protected GameObject prevSelectedObject;

        protected bool dontPlaySelectSound;

        protected bool deselectWasForced;

        //private MenuButtonList parentList;

        public bool DontPlaySelectSound
        {
            get
            {
                return dontPlaySelectSound;
            }
            set
            {
                dontPlaySelectSound = value;
            }
        }

        public event OnSelectedEvent OnSelected;

        private new void Awake()
        {
            base.transition = Transition.None;
            if (base.navigation.mode != Navigation.Mode.Explicit)
            {
                Navigation navigation = default(Navigation);
                navigation.mode = Navigation.Mode.Explicit;
                base.navigation = navigation;
            }
        }

        private new void Start()
        {
            HookUpAudioPlayer();
        }

        public new void OnSelect(BaseEventData eventData)
        {
            if (!base.interactable)
            {
                return;
            }
            if (this.OnSelected != null)
            {
                this.OnSelected(this);
            }
            if (leftCursor != null)
            {
                leftCursor.ResetTrigger("hide");
                leftCursor.SetTrigger("show");
            }
            if (rightCursor != null)
            {
                rightCursor.ResetTrigger("hide");
                rightCursor.SetTrigger("show");
            }
            if (selectHighlight != null)
            {
                selectHighlight.ResetTrigger("hide");
                selectHighlight.SetTrigger("show");
            }
            if (descriptionText != null)
            {
                descriptionText.ResetTrigger("hide");
                descriptionText.SetTrigger("show");
            }
            if (!DontPlaySelectSound)
            {
                try
                {
                    //uiAudioPlayer.PlaySelect();
                }
                catch (Exception ex)
                {
                    Debug.LogError(base.name + " doesn't have a select sound specified. " + ex);
                }
            }
            else
            {
                dontPlaySelectSound = false;
            }
        }

        public new void OnDeselect(BaseEventData eventData)
        {
            StartCoroutine(ValidateDeselect());
        }

        public void ForceDeselect()
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                deselectWasForced = true;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (cancelAction != 0)
            {
                ForceDeselect();
            }
            /*if (!parentList)
            {
                parentList = GetComponentInParent<MenuButtonList>();
            }
            if ((bool)parentList)
            {
                parentList.ClearLastSelected();
            }*/
            /*if (cancelAction != 0)
            {
                if (cancelAction == CancelAction.GoToMainMenu)
                {
                    UIManager.instance.UIGoToMainMenu();
                }
                else if (cancelAction == CancelAction.GoToOptionsMenu)
                {
                    UIManager.instance.UIGoToOptionsMenu();
                }
                else if (cancelAction == CancelAction.GoToVideoMenu)
                {
                    UIManager.instance.UIGoToVideoMenu();
                }
                else if (cancelAction == CancelAction.GoToPauseMenu)
                {
                    UIManager.instance.UIGoToPauseMenu();
                }
                else if (cancelAction == CancelAction.LeaveOptionsMenu)
                {
                    UIManager.instance.UILeaveOptionsMenu();
                }
                else if (cancelAction == CancelAction.GoToExitPrompt)
                {
                    UIManager.instance.UIShowQuitGamePrompt();
                }
                else if (cancelAction == CancelAction.GoToProfileMenu)
                {
                    UIManager.instance.UIGoToProfileMenu();
                }
                else if (cancelAction == CancelAction.GoToControllerMenu)
                {
                    UIManager.instance.UIGoToControllerMenu();
                }
                else if (cancelAction == CancelAction.ApplyRemapGamepadSettings)
                {
                    UIManager.instance.ApplyRemapGamepadMenuSettings();
                }
                else if (cancelAction == CancelAction.ApplyAudioSettings)
                {
                    UIManager.instance.ApplyAudioMenuSettings();
                }
                else if (cancelAction == CancelAction.ApplyVideoSettings)
                {
                    UIManager.instance.ApplyVideoMenuSettings();
                }
                else if (cancelAction == CancelAction.ApplyGameSettings)
                {
                    UIManager.instance.ApplyGameMenuSettings();
                }
                else if (cancelAction == CancelAction.ApplyKeyboardSettings)
                {
                    UIManager.instance.ApplyKeyboardMenuSettings();
                }
                else if (cancelAction == CancelAction.ApplyControllerSettings)
                {
                    UIManager.instance.ApplyControllerMenuSettings();
                }
                else if (cancelAction == CancelAction.GoToExtrasMenu)
                {
                    if ((bool)ContentPackDetailsUI.Instance)
                    {
                        ContentPackDetailsUI.Instance.UndoMenuStyle();
                    }
                    UIManager.instance.UIGoToExtrasMenu();
                }
                else if (cancelAction == CancelAction.GoToExplicitSwitchUser)
                {
                    UIManager.instance.UIGoToEngageMenu();
                }
                else if (cancelAction == CancelAction.ReturnToProfileMenu)
                {
                    UIManager.instance.UIReturnToProfileMenu();
                }
                else
                {
                    Debug.LogError("CancelAction not implemented for this control");
                }
            }*/
            if (cancelAction != 0)
            {
                PlayCancelSound();
            }
        }

        private IEnumerator ValidateDeselect()
        {
            prevSelectedObject = EventSystem.current.currentSelectedGameObject;
            yield return new WaitForEndOfFrame();
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                if (leftCursor != null)
                {
                    leftCursor.ResetTrigger("show");
                    leftCursor.SetTrigger("hide");
                }
                if (rightCursor != null)
                {
                    rightCursor.ResetTrigger("show");
                    rightCursor.SetTrigger("hide");
                }
                if (selectHighlight != null)
                {
                    selectHighlight.ResetTrigger("show");
                    selectHighlight.SetTrigger("hide");
                }
                if (descriptionText != null)
                {
                    descriptionText.ResetTrigger("show");
                    descriptionText.SetTrigger("hide");
                }
                deselectWasForced = false;
            }
            else if (deselectWasForced)
            {
                if (leftCursor != null)
                {
                    leftCursor.ResetTrigger("show");
                    leftCursor.SetTrigger("hide");
                }
                if (rightCursor != null)
                {
                    rightCursor.ResetTrigger("show");
                    rightCursor.SetTrigger("hide");
                }
                if (selectHighlight != null)
                {
                    selectHighlight.ResetTrigger("show");
                    selectHighlight.SetTrigger("hide");
                }
                if (descriptionText != null)
                {
                    descriptionText.ResetTrigger("show");
                    descriptionText.SetTrigger("hide");
                }
                deselectWasForced = false;
            }
            else
            {
                deselectWasForced = false;
                dontPlaySelectSound = true;
                EventSystem.current.SetSelectedGameObject(prevSelectedObject);
            }
        }

        protected void HookUpAudioPlayer()
        {
            /*if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Pre_Menu_Intro")
            {
                uiAudioPlayer = Object.FindObjectOfType<MenuAudioController>();
            }
            else
            {
                uiAudioPlayer = UIManager.instance.uiAudioPlayer;
            }*/
        }

        protected void PlaySubmitSound()
        {
            if (playSubmitSound)
            {
                //uiAudioPlayer.PlaySubmit();
            }
        }

        protected void PlayCancelSound()
        {
            //uiAudioPlayer.PlayCancel();
        }

        protected void PlaySelectSound()
        {
            //uiAudioPlayer.PlaySelect();
        }
    }
}
