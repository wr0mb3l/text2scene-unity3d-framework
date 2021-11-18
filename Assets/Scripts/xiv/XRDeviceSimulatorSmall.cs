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
    public class XRDeviceSimulator : MonoBehaviour
    {
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

        Vector2 m_MouseDeltaInput;

        Vector3 m_CenterEyeEuler;

        XRSimulatedHMDState m_HMDState;

        XRSimulatedHMD m_HMDDevice;

        protected virtual void Awake()
        {
            m_HMDState.Reset();
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected virtual void OnEnable()
        {
            // Find the Camera if necessary
            if (m_CameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_CameraTransform = mainCamera.transform;
            }

            AddDevices();
            SubscribeMouseDeltaAction();
        }

        protected virtual void OnDisable()
        {
            RemoveDevices();

            UnsubscribeMouseDeltaAction();
        }

        protected virtual void Update()
        {
            ProcessPoseInput();

            if (m_HMDDevice != null)
            {
                InputState.Change(m_HMDDevice, m_HMDState);
            }
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated device(s)
        /// related to position and rotation.
        /// </summary>
        protected virtual void ProcessPoseInput()
        {
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
        }

        /// <summary>
        /// Remove simulated XR devices from the Input System.
        /// </summary>
        /// <seealso cref="InputSystem.RemoveDevice"/>
        protected virtual void RemoveDevices()
        {
            if (m_HMDDevice != null && m_HMDDevice.added)
                InputSystem.InputSystem.RemoveDevice(m_HMDDevice);
        }

        void SubscribeMouseDeltaAction() => Subscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
        void UnsubscribeMouseDeltaAction() => Unsubscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);

        void OnMouseDeltaPerformed(InputAction.CallbackContext context) => m_MouseDeltaInput = context.ReadValue<Vector2>();
        void OnMouseDeltaCanceled(InputAction.CallbackContext context) => m_MouseDeltaInput = Vector2.zero;

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