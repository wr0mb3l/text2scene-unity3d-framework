using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation
{
    public class XRDeviceSimulatorSmall : MonoBehaviour
    {
        /// <summary>
        /// The coordinate space in which to operate.
        /// </summary>
        /// <seealso cref="keyboardTranslateSpace"/>
        public enum Space
        {
            /// <summary>
            /// Applies translations of HMD relative to its own coordinate space, considering its own rotations.
            /// </summary>
            Local,

            /// <summary>
            /// Applies translations of HMD relative to the world coordinate space.
            /// </summary>
            World,
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the x-axis (left/right) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardXTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the x-axis (left/right) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardXTranslateAction
        {
            get => m_KeyboardXTranslateAction;
            set
            {
                UnsubscribeKeyboardXTranslateAction();
                m_KeyboardXTranslateAction = value;
                SubscribeKeyboardXTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the y-axis (up/down) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardYTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the y-axis (up/down) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardYTranslateAction
        {
            get => m_KeyboardYTranslateAction;
            set
            {
                UnsubscribeKeyboardYTranslateAction();
                m_KeyboardYTranslateAction = value;
                SubscribeKeyboardYTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the z-axis (forward/back) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardZTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the z-axis (forward/back) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardZTranslateAction
        {
            get => m_KeyboardZTranslateAction;
            set
            {
                UnsubscribeKeyboardZTranslateAction();
                m_KeyboardZTranslateAction = value;
                SubscribeKeyboardZTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes. Must be a Value Vector2 Control.")]
        InputActionReference m_MouseDeltaAction;
        /// <summary>
        /// The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to the screen-space motion delta of the mouse in pixels.
        /// </remarks>
        /// <seealso cref="mouseScrollAction"/>
        public InputActionReference mouseDeltaAction
        {
            get => m_MouseDeltaAction;
            set
            {
                UnsubscribeMouseDeltaAction();
                m_MouseDeltaAction = value;
                SubscribeMouseDeltaAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Grip control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_GripAction;
        /// <summary>
        /// The Input System Action used to control the Grip control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference gripAction
        {
            get => m_GripAction;
            set
            {
                UnsubscribeGripAction();
                m_GripAction = value;
                SubscribeGripAction();
            }
        }

        [SerializeField]
        [Tooltip("The coordinate space in which keyboard translation should operate.")]
        Space m_KeyboardTranslateSpace = Space.Local;
        /// <summary>
        /// The coordinate space in which keyboard translation should operate.
        /// </summary>
        /// <seealso cref="Space"/>
        /// <seealso cref="mouseTranslateSpace"/>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        public Space keyboardTranslateSpace
        {
            get => m_KeyboardTranslateSpace;
            set => m_KeyboardTranslateSpace = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the x-axis (left/right) when triggered by keyboard input.")]
        float m_KeyboardXTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the x-axis (left/right) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateSpeed"/>
        /// <seealso cref="keyboardZTranslateSpeed"/>
        public float keyboardXTranslateSpeed
        {
            get => m_KeyboardXTranslateSpeed;
            set => m_KeyboardXTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the y-axis (up/down) when triggered by keyboard input.")]
        float m_KeyboardYTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the y-axis (up/down) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardXTranslateSpeed"/>
        /// <seealso cref="keyboardZTranslateSpeed"/>
        public float keyboardYTranslateSpeed
        {
            get => m_KeyboardYTranslateSpeed;
            set => m_KeyboardYTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the z-axis (forward/back) when triggered by keyboard input.")]
        float m_KeyboardZTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the z-axis (forward/back) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardZTranslateAction"/>
        /// <seealso cref="keyboardXTranslateSpeed"/>
        /// <seealso cref="keyboardYTranslateSpeed"/>
        public float keyboardZTranslateSpeed
        {
            get => m_KeyboardZTranslateSpeed;
            set => m_KeyboardZTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.")]
        float m_MouseXRotateSensitivity = 0.1f;
        /// <summary>
        /// Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseYRotateSensitivity"/>
        /// <seealso cref="mouseScrollRotateSensitivity"/>
        public float mouseXRotateSensitivity
        {
            get => m_MouseXRotateSensitivity;
            set => m_MouseXRotateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.")]
        float m_MouseYRotateSensitivity = 0.1f;
        /// <summary>
        /// Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseXRotateSensitivity"/>
        /// <seealso cref="mouseScrollRotateSensitivity"/>
        public float mouseYRotateSensitivity
        {
            get => m_MouseYRotateSensitivity;
            set => m_MouseYRotateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input." +
                "\nA false value (default) means typical FPS style where moving the mouse up/down pitches up/down." +
                "\nA true value means flight control style where moving the mouse up/down pitches down/up.")]
        bool m_MouseYRotateInvert;
        /// <summary>
        /// A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input.
        /// A <see langword="false"/> value (default) means typical FPS style where moving the mouse up/down pitches up/down.
        /// A <see langword="true"/> value means flight control style where moving the mouse up/down pitches down/up.
        /// </summary>
        public bool mouseYRotateInvert
        {
            get => m_MouseYRotateInvert;
            set => m_MouseYRotateInvert = value;
        }

        [SerializeField]
        [Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR rigs. Automatically set to the first enabled camera tagged MainCamera if unset.")]
        Transform m_CameraTransform;
        /// <summary>
        /// The <see cref="Transform"/> that contains the <see cref="Camera"/>. This is usually the "Head" of XR rigs.
        /// Automatically set to <see cref="Camera.main"/> if unset.
        /// </summary>
        public Transform cameraTransform
        {
            get => m_CameraTransform;
            set => m_CameraTransform = value;
        }

        float m_KeyboardXTranslateInput;
        float m_KeyboardYTranslateInput;
        float m_KeyboardZTranslateInput;

        Vector2 m_MouseDeltaInput;
        
        bool m_GripInput;

        Vector3 m_CenterEyeEuler;

        XRSimulatedHMDState m_HMDState;
        XRSimulatedControllerState m_LeftControllerState;

        XRSimulatedHMD m_HMDDevice;
        XRSimulatedController m_LeftControllerDevice;
        private CharacterController controller;

        protected virtual void Awake()
        {
            m_HMDState.Reset();
            m_LeftControllerState.Reset();
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected virtual void OnEnable()
        {
            // Find the Camera if necessary
            if (m_CameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    mainCamera.gameObject.AddComponent<CharacterController>();
                    m_CameraTransform = mainCamera.transform;
            }

            AddDevices();

            SubscribeKeyboardXTranslateAction();
            SubscribeKeyboardYTranslateAction();
            SubscribeKeyboardZTranslateAction();
            SubscribeGripAction();
            SubscribeMouseDeltaAction();
        }

        protected virtual void OnDisable()
        {
            RemoveDevices();

            UnsubscribeKeyboardXTranslateAction();
            UnsubscribeKeyboardYTranslateAction();
            UnsubscribeKeyboardZTranslateAction();
            UnsubscribeGripAction();
            UnsubscribeMouseDeltaAction();
        }

        protected virtual void Update()
        {
            ProcessPoseInput();
            ProcessControlInput();

            if (m_HMDDevice != null)
            {
                InputState.Change(m_HMDDevice, m_HMDState);
            }
            if (m_LeftControllerDevice != null)
            {
                InputState.Change(m_LeftControllerDevice, m_LeftControllerState);
            }
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated device(s)
        /// related to position and rotation.
        /// </summary>
        protected virtual void ProcessPoseInput()
        {
            // Determine frame of reference
            GetAxes(m_KeyboardTranslateSpace, m_CameraTransform, out var right, out var up, out var forward);

            // Keyboard translation
            Vector3 deltaPosition = Time.deltaTime * (
                m_KeyboardZTranslateInput * m_KeyboardZTranslateSpeed * forward +
                m_KeyboardXTranslateInput * m_KeyboardXTranslateSpeed * right +
                m_KeyboardYTranslateInput * m_KeyboardYTranslateSpeed * up);
            m_HMDState.centerEyePosition += deltaPosition;
            m_HMDState.devicePosition = m_HMDState.centerEyePosition;

            // Mouse rotation
            var scaledMouseDeltaInput = new Vector3(m_MouseDeltaInput.x * m_MouseXRotateSensitivity,
                            m_MouseDeltaInput.y * m_MouseYRotateSensitivity * (m_MouseYRotateInvert ? 1f : -1f),
                            0);

            Vector3 anglesDelta;
            anglesDelta = new Vector3(scaledMouseDeltaInput.y, scaledMouseDeltaInput.x, 0f);

            m_CenterEyeEuler += anglesDelta;
            m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to input controls.
        /// </summary>
        protected virtual void ProcessControlInput()
        {
            ProcessButtonControlInput(ref m_LeftControllerState);
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to button input controls.
        /// </summary>
        /// <param name="controllerState">The controller state that will be processed.</param>
        protected virtual void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
        {
            controllerState.grip = m_GripInput ? 1f : 0f;
            controllerState.WithButton(ControllerButton.GripButton, m_GripInput);
            // controllerState.trigger = m_TriggerInput ? 1f : 0f;
            // controllerState.WithButton(ControllerButton.TriggerButton, m_TriggerInput);
            // controllerState.WithButton(ControllerButton.PrimaryButton, m_PrimaryButtonInput);
            // controllerState.WithButton(ControllerButton.SecondaryButton, m_SecondaryButtonInput);
            // controllerState.WithButton(ControllerButton.MenuButton, m_MenuInput);
            // controllerState.WithButton(ControllerButton.Primary2DAxisClick, m_Primary2DAxisClickInput);
            // controllerState.WithButton(ControllerButton.Secondary2DAxisClick, m_Secondary2DAxisClickInput);
            // controllerState.WithButton(ControllerButton.Primary2DAxisTouch, m_Primary2DAxisTouchInput);
            // controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, m_Secondary2DAxisTouchInput);
            // controllerState.WithButton(ControllerButton.PrimaryTouch, m_PrimaryTouchInput);
            // controllerState.WithButton(ControllerButton.SecondaryTouch, m_SecondaryTouchInput);
        }


        /// <summary>
        /// Add simulated XR devices to the Input System.
        /// </summary>
        /// <seealso cref="InputSystem.AddDevice{TDevice}"/>
        protected virtual void AddDevices()
        {
            m_HMDDevice = InputSystem.InputSystem.AddDevice<XRSimulatedHMD>();
            if (m_HMDDevice == null)
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedHMD)}.");
            }

            m_LeftControllerDevice = InputSystem.InputSystem.AddDevice<XRSimulatedController>($"{nameof(XRSimulatedController)} - {InputSystem.CommonUsages.LeftHand}");
            if (m_LeftControllerDevice != null)
            {
                InputSystem.InputSystem.SetDeviceUsage(m_LeftControllerDevice, InputSystem.CommonUsages.LeftHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {InputSystem.CommonUsages.LeftHand}.", this);
            }
        }

        /// <summary>
        /// Remove simulated XR devices from the Input System.
        /// </summary>
        /// <seealso cref="InputSystem.RemoveDevice"/>
        protected virtual void RemoveDevices()
        {
            if (m_HMDDevice != null && m_HMDDevice.added)
                InputSystem.InputSystem.RemoveDevice(m_HMDDevice);

            if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
                InputSystem.InputSystem.RemoveDevice(m_LeftControllerDevice);
        }

        static void GetAxes(Space translateSpace, Transform cameraTransform, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            if (cameraTransform == null)
                throw new ArgumentNullException(nameof(cameraTransform));

            switch (translateSpace)
            {
                case Space.Local:
                    right = cameraTransform.right;
                    up = cameraTransform.up;
                    forward = cameraTransform.forward;
                    break;
                case Space.World:
                    right = cameraTransform.right;
                    up = Vector3.up;
                    forward = Vector3.Normalize(new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z));
                    break;
                default:
                    right = Vector3.right;
                    up = Vector3.up;
                    forward = Vector3.forward;
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return;
            }
        }

        void SubscribeKeyboardXTranslateAction() => Subscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);
        void UnsubscribeKeyboardXTranslateAction() => Unsubscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);

        void SubscribeKeyboardYTranslateAction() => Subscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);
        void UnsubscribeKeyboardYTranslateAction() => Unsubscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);

        void SubscribeKeyboardZTranslateAction() => Subscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);
        void UnsubscribeKeyboardZTranslateAction() => Unsubscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);

        void SubscribeMouseDeltaAction() => Subscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
        void UnsubscribeMouseDeltaAction() => Unsubscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);

        void SubscribeGripAction() => Subscribe(m_GripAction, OnGripPerformed, OnGripCanceled);
        void UnsubscribeGripAction() => Unsubscribe(m_GripAction, OnGripPerformed, OnGripCanceled);

        void OnKeyboardXTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = context.ReadValue<float>();
        void OnKeyboardXTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = 0f;

        void OnKeyboardYTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = context.ReadValue<float>();
        void OnKeyboardYTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = 0f;

        void OnKeyboardZTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = context.ReadValue<float>();
        void OnKeyboardZTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = 0f;

        void OnMouseDeltaPerformed(InputAction.CallbackContext context) => m_MouseDeltaInput = context.ReadValue<Vector2>();
        void OnMouseDeltaCanceled(InputAction.CallbackContext context) => m_MouseDeltaInput = Vector2.zero;

        void OnGripPerformed(InputAction.CallbackContext context) => m_GripInput = true;
        void OnGripCanceled(InputAction.CallbackContext context) => m_GripInput = false;

        static void Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed += performed;
                if (canceled != null)
                    action.canceled += canceled;
            }
        }

        static void Unsubscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed -= performed;
                if (canceled != null)
                    action.canceled -= canceled;
            }
        }

        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
    }
}