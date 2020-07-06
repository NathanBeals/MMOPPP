// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""PlayerControls"",
            ""id"": ""594d475f-3b28-4322-9882-3192ca3749c3"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""PassThrough"",
                    ""id"": ""64d391fd-a5c5-4918-8617-f0897014d89a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""PassThrough"",
                    ""id"": ""cef2f1eb-73ba-44f8-a281-4905c8fe73b0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""f75d7617-30fb-4c83-a009-681003224061"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Strafe"",
                    ""type"": ""Value"",
                    ""id"": ""f936fa1f-9e71-4da7-9f5a-c64c9817b71e"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Value"",
                    ""id"": ""b1121191-203d-405c-a4ed-49c92aaba42c"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""392da6a4-5738-4a43-a7d1-f4264d79483c"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""c69805ef-9254-4f81-af9f-a0388d82cfbf"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b1722164-9756-445f-b954-4611e0a81ebd"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d4fd3eac-f4b9-485f-9582-4fc4237abdb6"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""63466b5c-ddfe-4ae1-b82e-fb6278ede135"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""63942cdc-1d19-4f47-a0f7-9d3f4d9875d0"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.1,y=0.1)"",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b071ab66-2e9d-4aba-a799-796505e8068c"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b68addf0-01f6-4e75-aafb-0c9dd9ea852d"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Strafe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9cc1c982-cbb0-4e28-8f9e-5bd612460181"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""DebugControls"",
            ""id"": ""6907a8da-2d0d-455f-97b4-8d94ed170fa6"",
            ""actions"": [
                {
                    ""name"": ""SaveWorldState"",
                    ""type"": ""Button"",
                    ""id"": ""0f5dbe1b-d0fb-4e17-9107-fd82df370fb2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LoadWorldState"",
                    ""type"": ""Button"",
                    ""id"": ""ccf1f8c3-6c15-43bc-b727-ac732afcb02d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SpawnRandomCharacter"",
                    ""type"": ""Button"",
                    ""id"": ""9c3d5722-7324-4d65-a515-c00ed8ed6a46"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ExitGame"",
                    ""type"": ""Button"",
                    ""id"": ""218e4afc-37e7-49f2-a9ce-f55b656ab01d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""1b554468-590e-40bc-9a44-94c4dd90c93f"",
                    ""path"": ""<Keyboard>/numpad7"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SaveWorldState"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bdaf8695-a35c-468d-a0e2-ac4c3cd12c25"",
                    ""path"": ""<Keyboard>/numpad8"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LoadWorldState"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6cfee56e-0d97-4d73-8808-9d65aab2a02d"",
                    ""path"": ""<Keyboard>/numpad9"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpawnRandomCharacter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7a8b4f93-ff73-4ecb-bb7a-b3b5db1e219b"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ExitGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerControls
        m_PlayerControls = asset.FindActionMap("PlayerControls", throwIfNotFound: true);
        m_PlayerControls_Move = m_PlayerControls.FindAction("Move", throwIfNotFound: true);
        m_PlayerControls_Rotate = m_PlayerControls.FindAction("Rotate", throwIfNotFound: true);
        m_PlayerControls_Jump = m_PlayerControls.FindAction("Jump", throwIfNotFound: true);
        m_PlayerControls_Strafe = m_PlayerControls.FindAction("Strafe", throwIfNotFound: true);
        m_PlayerControls_Sprint = m_PlayerControls.FindAction("Sprint", throwIfNotFound: true);
        // DebugControls
        m_DebugControls = asset.FindActionMap("DebugControls", throwIfNotFound: true);
        m_DebugControls_SaveWorldState = m_DebugControls.FindAction("SaveWorldState", throwIfNotFound: true);
        m_DebugControls_LoadWorldState = m_DebugControls.FindAction("LoadWorldState", throwIfNotFound: true);
        m_DebugControls_SpawnRandomCharacter = m_DebugControls.FindAction("SpawnRandomCharacter", throwIfNotFound: true);
        m_DebugControls_ExitGame = m_DebugControls.FindAction("ExitGame", throwIfNotFound: true);
    }

    public void Dispose()
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

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

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

    // PlayerControls
    private readonly InputActionMap m_PlayerControls;
    private IPlayerControlsActions m_PlayerControlsActionsCallbackInterface;
    private readonly InputAction m_PlayerControls_Move;
    private readonly InputAction m_PlayerControls_Rotate;
    private readonly InputAction m_PlayerControls_Jump;
    private readonly InputAction m_PlayerControls_Strafe;
    private readonly InputAction m_PlayerControls_Sprint;
    public struct PlayerControlsActions
    {
        private @PlayerInputActions m_Wrapper;
        public PlayerControlsActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_PlayerControls_Move;
        public InputAction @Rotate => m_Wrapper.m_PlayerControls_Rotate;
        public InputAction @Jump => m_Wrapper.m_PlayerControls_Jump;
        public InputAction @Strafe => m_Wrapper.m_PlayerControls_Strafe;
        public InputAction @Sprint => m_Wrapper.m_PlayerControls_Sprint;
        public InputActionMap Get() { return m_Wrapper.m_PlayerControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerControlsActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerControlsActions instance)
        {
            if (m_Wrapper.m_PlayerControlsActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMove;
                @Rotate.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnRotate;
                @Rotate.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnRotate;
                @Rotate.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnRotate;
                @Jump.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnJump;
                @Strafe.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnStrafe;
                @Strafe.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnStrafe;
                @Strafe.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnStrafe;
                @Sprint.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnSprint;
            }
            m_Wrapper.m_PlayerControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Rotate.started += instance.OnRotate;
                @Rotate.performed += instance.OnRotate;
                @Rotate.canceled += instance.OnRotate;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Strafe.started += instance.OnStrafe;
                @Strafe.performed += instance.OnStrafe;
                @Strafe.canceled += instance.OnStrafe;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
            }
        }
    }
    public PlayerControlsActions @PlayerControls => new PlayerControlsActions(this);

    // DebugControls
    private readonly InputActionMap m_DebugControls;
    private IDebugControlsActions m_DebugControlsActionsCallbackInterface;
    private readonly InputAction m_DebugControls_SaveWorldState;
    private readonly InputAction m_DebugControls_LoadWorldState;
    private readonly InputAction m_DebugControls_SpawnRandomCharacter;
    private readonly InputAction m_DebugControls_ExitGame;
    public struct DebugControlsActions
    {
        private @PlayerInputActions m_Wrapper;
        public DebugControlsActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @SaveWorldState => m_Wrapper.m_DebugControls_SaveWorldState;
        public InputAction @LoadWorldState => m_Wrapper.m_DebugControls_LoadWorldState;
        public InputAction @SpawnRandomCharacter => m_Wrapper.m_DebugControls_SpawnRandomCharacter;
        public InputAction @ExitGame => m_Wrapper.m_DebugControls_ExitGame;
        public InputActionMap Get() { return m_Wrapper.m_DebugControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DebugControlsActions set) { return set.Get(); }
        public void SetCallbacks(IDebugControlsActions instance)
        {
            if (m_Wrapper.m_DebugControlsActionsCallbackInterface != null)
            {
                @SaveWorldState.started -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSaveWorldState;
                @SaveWorldState.performed -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSaveWorldState;
                @SaveWorldState.canceled -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSaveWorldState;
                @LoadWorldState.started -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnLoadWorldState;
                @LoadWorldState.performed -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnLoadWorldState;
                @LoadWorldState.canceled -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnLoadWorldState;
                @SpawnRandomCharacter.started -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSpawnRandomCharacter;
                @SpawnRandomCharacter.performed -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSpawnRandomCharacter;
                @SpawnRandomCharacter.canceled -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnSpawnRandomCharacter;
                @ExitGame.started -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnExitGame;
                @ExitGame.performed -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnExitGame;
                @ExitGame.canceled -= m_Wrapper.m_DebugControlsActionsCallbackInterface.OnExitGame;
            }
            m_Wrapper.m_DebugControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SaveWorldState.started += instance.OnSaveWorldState;
                @SaveWorldState.performed += instance.OnSaveWorldState;
                @SaveWorldState.canceled += instance.OnSaveWorldState;
                @LoadWorldState.started += instance.OnLoadWorldState;
                @LoadWorldState.performed += instance.OnLoadWorldState;
                @LoadWorldState.canceled += instance.OnLoadWorldState;
                @SpawnRandomCharacter.started += instance.OnSpawnRandomCharacter;
                @SpawnRandomCharacter.performed += instance.OnSpawnRandomCharacter;
                @SpawnRandomCharacter.canceled += instance.OnSpawnRandomCharacter;
                @ExitGame.started += instance.OnExitGame;
                @ExitGame.performed += instance.OnExitGame;
                @ExitGame.canceled += instance.OnExitGame;
            }
        }
    }
    public DebugControlsActions @DebugControls => new DebugControlsActions(this);
    public interface IPlayerControlsActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnStrafe(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
    }
    public interface IDebugControlsActions
    {
        void OnSaveWorldState(InputAction.CallbackContext context);
        void OnLoadWorldState(InputAction.CallbackContext context);
        void OnSpawnRandomCharacter(InputAction.CallbackContext context);
        void OnExitGame(InputAction.CallbackContext context);
    }
}
