// GENERATED AUTOMATICALLY FROM 'Assets/Packages/DebuggingConsole/ConsoleInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ConsoleInput : IInputActionCollection
{
    private InputActionAsset asset;
    public ConsoleInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ConsoleInput"",
    ""maps"": [
        {
            ""name"": ""Console"",
            ""id"": ""390a8ee8-774f-405f-a893-5fd77c9fbbf7"",
            ""actions"": [
                {
                    ""name"": ""Submit"",
                    ""id"": ""57ec5125-64f1-4c4c-b99c-6aa1968d6895"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""AutoComplete"",
                    ""id"": ""dfab1f24-7b58-49a1-9526-9027647a4f8b"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""ShowHide"",
                    ""id"": ""9c7a5c4b-47d3-4b39-a67c-d809f3033c0b"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""DeleteWord"",
                    ""id"": ""2ea02632-9d81-4acc-a419-d6bec0902d63"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""HistoryUp"",
                    ""id"": ""424f5efd-6d01-455a-b400-69c7b895d614"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                },
                {
                    ""name"": ""HistoryDown"",
                    ""id"": ""36fd4266-69fb-48be-a988-a6deaab7bba0"",
                    ""expectedControlLayout"": """",
                    ""continuous"": false,
                    ""passThrough"": false,
                    ""initialStateCheck"": false,
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""bindings"": []
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""fd966bc3-d202-4f62-8f83-9567c35988a4"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""cdba1694-ae23-4da4-8e85-b6e1445f329c"",
                    ""path"": ""<Keyboard>/numpadEnter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""c92e81ae-1727-4caf-8f43-2f309e270086"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AutoComplete"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""d5f50501-a0d6-43ff-bf21-2e403f09c23e"",
                    ""path"": ""<Keyboard>/backquote"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShowHide"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""9327807d-63a8-43b1-8a87-2e102cbd87d0"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DeleteWord"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""ffc26124-e5a2-42d9-906d-c0a8a688f6fb"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HistoryUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                },
                {
                    ""name"": """",
                    ""id"": ""9a08b2ad-21c2-4822-9a5c-807c55c87ce8"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HistoryDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false,
                    ""modifiers"": """"
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Console
        m_Console = asset.GetActionMap("Console");
        m_Console_Submit = m_Console.GetAction("Submit");
        m_Console_AutoComplete = m_Console.GetAction("AutoComplete");
        m_Console_ShowHide = m_Console.GetAction("ShowHide");
        m_Console_DeleteWord = m_Console.GetAction("DeleteWord");
        m_Console_HistoryUp = m_Console.GetAction("HistoryUp");
        m_Console_HistoryDown = m_Console.GetAction("HistoryDown");
    }

    ~ConsoleInput()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes
    {
        get => asset.controlSchemes;
    }

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Console
    private InputActionMap m_Console;
    private IConsoleActions m_ConsoleActionsCallbackInterface;
    private InputAction m_Console_Submit;
    private InputAction m_Console_AutoComplete;
    private InputAction m_Console_ShowHide;
    private InputAction m_Console_DeleteWord;
    private InputAction m_Console_HistoryUp;
    private InputAction m_Console_HistoryDown;
    public struct ConsoleActions
    {
        private ConsoleInput m_Wrapper;
        public ConsoleActions(ConsoleInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Submit { get { return m_Wrapper.m_Console_Submit; } }
        public InputAction @AutoComplete { get { return m_Wrapper.m_Console_AutoComplete; } }
        public InputAction @ShowHide { get { return m_Wrapper.m_Console_ShowHide; } }
        public InputAction @DeleteWord { get { return m_Wrapper.m_Console_DeleteWord; } }
        public InputAction @HistoryUp { get { return m_Wrapper.m_Console_HistoryUp; } }
        public InputAction @HistoryDown { get { return m_Wrapper.m_Console_HistoryDown; } }
        public InputActionMap Get() { return m_Wrapper.m_Console; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(ConsoleActions set) { return set.Get(); }
        public void SetCallbacks(IConsoleActions instance)
        {
            if (m_Wrapper.m_ConsoleActionsCallbackInterface != null)
            {
                Submit.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnSubmit;
                Submit.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnSubmit;
                Submit.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnSubmit;
                AutoComplete.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnAutoComplete;
                AutoComplete.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnAutoComplete;
                AutoComplete.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnAutoComplete;
                ShowHide.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnShowHide;
                ShowHide.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnShowHide;
                ShowHide.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnShowHide;
                DeleteWord.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnDeleteWord;
                DeleteWord.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnDeleteWord;
                DeleteWord.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnDeleteWord;
                HistoryUp.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryUp;
                HistoryUp.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryUp;
                HistoryUp.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryUp;
                HistoryDown.started -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryDown;
                HistoryDown.performed -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryDown;
                HistoryDown.canceled -= m_Wrapper.m_ConsoleActionsCallbackInterface.OnHistoryDown;
            }
            m_Wrapper.m_ConsoleActionsCallbackInterface = instance;
            if (instance != null)
            {
                Submit.started += instance.OnSubmit;
                Submit.performed += instance.OnSubmit;
                Submit.canceled += instance.OnSubmit;
                AutoComplete.started += instance.OnAutoComplete;
                AutoComplete.performed += instance.OnAutoComplete;
                AutoComplete.canceled += instance.OnAutoComplete;
                ShowHide.started += instance.OnShowHide;
                ShowHide.performed += instance.OnShowHide;
                ShowHide.canceled += instance.OnShowHide;
                DeleteWord.started += instance.OnDeleteWord;
                DeleteWord.performed += instance.OnDeleteWord;
                DeleteWord.canceled += instance.OnDeleteWord;
                HistoryUp.started += instance.OnHistoryUp;
                HistoryUp.performed += instance.OnHistoryUp;
                HistoryUp.canceled += instance.OnHistoryUp;
                HistoryDown.started += instance.OnHistoryDown;
                HistoryDown.performed += instance.OnHistoryDown;
                HistoryDown.canceled += instance.OnHistoryDown;
            }
        }
    }
    public ConsoleActions @Console
    {
        get
        {
            return new ConsoleActions(this);
        }
    }
    public interface IConsoleActions
    {
        void OnSubmit(InputAction.CallbackContext context);
        void OnAutoComplete(InputAction.CallbackContext context);
        void OnShowHide(InputAction.CallbackContext context);
        void OnDeleteWord(InputAction.CallbackContext context);
        void OnHistoryUp(InputAction.CallbackContext context);
        void OnHistoryDown(InputAction.CallbackContext context);
    }
}
