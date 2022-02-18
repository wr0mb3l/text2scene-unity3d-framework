using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Serialization;
using UnityEditor;

/// <summary>
/// Interactable component that allows basic "grab" functionality.
/// Can attach to a selecting Interactor and follow it around while obeying physics (and inherit velocity when released).
/// </summary>
[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("XR/XR Edit Interactable")]
[HelpURL(XRHelpURLConstants.k_XRGrabInteractable)]
public class XREditInteractable : XRBaseInteractable
{
    public enum AttachPointCompatibilityMode
    {
        Default,
        Legacy
    }

    private const float k_DefaultTighteningAmount = 0.5f;

    private const float k_DefaultSmoothingAmount = 5f;

    private const float k_VelocityDamping = 1f;

    private const float k_VelocityScale = 1f;

    private const float k_AngularVelocityDamping = 1f;

    private const float k_AngularVelocityScale = 1f;

    private const int k_ThrowSmoothingFrameCount = 20;

    private const float k_DefaultAttachEaseInTime = 0.15f;

    private const float k_DefaultThrowSmoothingDuration = 0.25f;

    private const float k_DefaultThrowVelocityScale = 1.5f;

    private const float k_DefaultThrowAngularVelocityScale = 1f;

    [SerializeField]
    private Transform m_AttachTransform;

    [SerializeField]
    private float m_AttachEaseInTime = 0.15f;

    [SerializeField]
    private MovementType m_MovementType = MovementType.Instantaneous;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_VelocityDamping = 1f;

    [SerializeField]
    private float m_VelocityScale = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_AngularVelocityDamping = 1f;

    [SerializeField]
    private float m_AngularVelocityScale = 1f;

    [SerializeField]
    private bool m_TrackPosition = true;

    [SerializeField]
    private bool m_SmoothPosition;

    [SerializeField]
    [Range(0f, 20f)]
    private float m_SmoothPositionAmount = 5f;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_TightenPosition = 0.5f;

    [SerializeField]
    private bool m_TrackRotation = true;

    [SerializeField]
    private bool m_SmoothRotation;

    [SerializeField]
    [Range(0f, 20f)]
    private float m_SmoothRotationAmount = 5f;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_TightenRotation = 0.5f;

    [SerializeField]
    private bool m_TrackScale = true;

    [SerializeField]
    private bool m_ThrowOnDetach = true;

    [SerializeField]
    private float m_ThrowSmoothingDuration = 0.25f;

    [SerializeField]
    private AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

    [SerializeField]
    private float m_ThrowVelocityScale = 1.5f;

    [SerializeField]
    private float m_ThrowAngularVelocityScale = 1f;

    [SerializeField]
    [FormerlySerializedAs("m_GravityOnDetach")]
    private bool m_ForceGravityOnDetach;

    [SerializeField]
    private bool m_RetainTransformParent = true;

    [SerializeField]
    private AttachPointCompatibilityMode m_AttachPointCompatibilityMode;

    private Vector3 m_InteractorLocalPosition;

    private Quaternion m_InteractorLocalRotation;

    private Vector3 m_TargetWorldPosition;

    private Quaternion m_TargetWorldRotation;

    private Vector3 m_TargetLocalScale;

    private Vector3 m_ParentLocalScale;

    private float m_CurrentAttachEaseTime;

    private MovementType m_CurrentMovementType;

    private bool m_DetachInLateUpdate;

    private Vector3 m_DetachVelocity;

    private Vector3 m_DetachAngularVelocity;

    private int m_ThrowSmoothingCurrentFrame;

    private readonly float[] m_ThrowSmoothingFrameTimes = new float[20];

    private readonly Vector3[] m_ThrowSmoothingVelocityFrames = new Vector3[20];

    private readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[20];

    private Rigidbody m_Rigidbody;

    private Vector3 m_LastPosition;

    private Quaternion m_LastRotation;

    private bool m_WasKinematic;

    private bool m_UsedGravity;

    private float m_OldDrag;

    private float m_OldAngularDrag;

    private Transform m_OriginalSceneParent;

    public Transform attachTransform
    {
        get
        {
            return m_AttachTransform;
        }
        set
        {
            m_AttachTransform = value;
        }
    }

    public float attachEaseInTime
    {
        get
        {
            return m_AttachEaseInTime;
        }
        set
        {
            m_AttachEaseInTime = value;
        }
    }

    public MovementType movementType
    {
        get
        {
            return m_MovementType;
        }
        set
        {
            m_MovementType = value;
        }
    }

    public float velocityDamping
    {
        get
        {
            return m_VelocityDamping;
        }
        set
        {
            m_VelocityDamping = value;
        }
    }

    public float velocityScale
    {
        get
        {
            return m_VelocityScale;
        }
        set
        {
            m_VelocityScale = value;
        }
    }

    public float angularVelocityDamping
    {
        get
        {
            return m_AngularVelocityDamping;
        }
        set
        {
            m_AngularVelocityDamping = value;
        }
    }

    public float angularVelocityScale
    {
        get
        {
            return m_AngularVelocityScale;
        }
        set
        {
            m_AngularVelocityScale = value;
        }
    }

    public bool trackPosition
    {
        get
        {
            return m_TrackPosition;
        }
        set
        {
            m_TrackPosition = value;
        }
    }

    public bool smoothPosition
    {
        get
        {
            return m_SmoothPosition;
        }
        set
        {
            m_SmoothPosition = value;
        }
    }

    public float smoothPositionAmount
    {
        get
        {
            return m_SmoothPositionAmount;
        }
        set
        {
            m_SmoothPositionAmount = value;
        }
    }

    public float tightenPosition
    {
        get
        {
            return m_TightenPosition;
        }
        set
        {
            m_TightenPosition = value;
        }
    }

    public bool trackRotation
    {
        get
        {
            return m_TrackRotation;
        }
        set
        {
            m_TrackRotation = value;
        }
    }

    public bool smoothRotation
    {
        get
        {
            return m_SmoothRotation;
        }
        set
        {
            m_SmoothRotation = value;
        }
    }

    public float smoothRotationAmount
    {
        get
        {
            return m_SmoothRotationAmount;
        }
        set
        {
            m_SmoothRotationAmount = value;
        }
    }

    public float tightenRotation
    {
        get
        {
            return m_TightenRotation;
        }
        set
        {
            m_TightenRotation = value;
        }
    }

    public bool trackScale
    {
        get
        {
            return m_TrackScale;
        }
        set
        {
            m_TrackScale = value;
        }
    }

    public bool throwOnDetach
    {
        get
        {
            return m_ThrowOnDetach;
        }
        set
        {
            m_ThrowOnDetach = value;
        }
    }

    public float throwSmoothingDuration
    {
        get
        {
            return m_ThrowSmoothingDuration;
        }
        set
        {
            m_ThrowSmoothingDuration = value;
        }
    }

    public AnimationCurve throwSmoothingCurve
    {
        get
        {
            return m_ThrowSmoothingCurve;
        }
        set
        {
            m_ThrowSmoothingCurve = value;
        }
    }

    public float throwVelocityScale
    {
        get
        {
            return m_ThrowVelocityScale;
        }
        set
        {
            m_ThrowVelocityScale = value;
        }
    }

    public float throwAngularVelocityScale
    {
        get
        {
            return m_ThrowAngularVelocityScale;
        }
        set
        {
            m_ThrowAngularVelocityScale = value;
        }
    }

    public bool forceGravityOnDetach
    {
        get
        {
            return m_ForceGravityOnDetach;
        }
        set
        {
            m_ForceGravityOnDetach = value;
        }
    }

    [Obsolete("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach")]
    public bool gravityOnDetach
    {
        get
        {
            return forceGravityOnDetach;
        }
        set
        {
            forceGravityOnDetach = value;
        }
    }

    public bool retainTransformParent
    {
        get
        {
            return m_RetainTransformParent;
        }
        set
        {
            m_RetainTransformParent = value;
        }
    }

    public AttachPointCompatibilityMode attachPointCompatibilityMode
    {
        get
        {
            return m_AttachPointCompatibilityMode;
        }
        set
        {
            m_AttachPointCompatibilityMode = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_CurrentMovementType = m_MovementType;
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            Debug.LogError("Grab Interactable does not have a required Rigidbody.", this);
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        switch (updatePhase)
        {
            case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                if (base.isSelected)
                {
                    if (m_CurrentMovementType == MovementType.Kinematic)
                    {
                        PerformKinematicUpdate(updatePhase);
                    }
                    else if (m_CurrentMovementType == MovementType.VelocityTracking)
                    {
                        PerformVelocityTrackingUpdate(Time.deltaTime, updatePhase);
                    }
                }

                break;
            case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                if (base.isSelected)
                {
                    if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default)
                    {
                        UpdateInteractorLocalPose(base.selectingInteractor);
                    }

                    UpdateTarget(Time.deltaTime);
                    SmoothVelocityUpdate();
                    if (m_CurrentMovementType == MovementType.Instantaneous)
                    {
                        PerformInstantaneousUpdate(updatePhase);
                    }
                }

                break;
            case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                if (base.isSelected)
                {
                    if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default)
                    {
                        UpdateInteractorLocalPose(base.selectingInteractor);
                    }

                    UpdateTarget(Time.deltaTime);
                    if (m_CurrentMovementType == MovementType.Instantaneous)
                    {
                        PerformInstantaneousUpdate(updatePhase);
                    }
                }

                break;
            case XRInteractionUpdateOrder.UpdatePhase.Late:
                if (m_DetachInLateUpdate)
                {
                    if (base.selectingInteractor == null)
                    {
                        Detach();
                    }

                    m_DetachInLateUpdate = false;
                }

                break;
        }
    }

    private Vector3 GetWorldAttachPosition(XRBaseInteractor interactor)
    {
        return interactor.attachTransform.position + interactor.attachTransform.rotation * m_InteractorLocalPosition;
    }

    private Quaternion GetWorldAttachRotation(XRBaseInteractor interactor)
    {
        return interactor.attachTransform.rotation * m_InteractorLocalRotation;
    }
    private Vector3 GetWorldAttachScale(XRBaseInteractor interactor)
    {
        return Vector3.Scale(interactor.attachTransform.localScale, m_ParentLocalScale);
    }

    private void UpdateTarget(float timeDelta)
    {
        Vector3 worldAttachPosition = GetWorldAttachPosition(base.selectingInteractor);
        Quaternion worldAttachRotation = GetWorldAttachRotation(base.selectingInteractor);
        Vector3 worldAttachScale = GetWorldAttachScale(base.selectingInteractor);

        if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
        {
            float t = m_CurrentAttachEaseTime / m_AttachEaseInTime;
            m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, t);
            m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, t);
            m_CurrentAttachEaseTime += timeDelta;
            return;
        }

        if (m_SmoothPosition)
        {
            m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, m_SmoothPositionAmount * timeDelta);
            m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, m_TightenPosition);
        }
        else
        {
            m_TargetWorldPosition = worldAttachPosition;
        }

        if (m_SmoothRotation)
        {
            m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, m_SmoothRotationAmount * timeDelta);
            m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, m_TightenRotation);
        }
        else
        {
            m_TargetWorldRotation = worldAttachRotation;
        }

        m_TargetLocalScale = worldAttachScale;
    }

    private void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
        {
            if (m_TrackPosition)
            {
                base.transform.position = m_TargetWorldPosition;
            }

            if (m_TrackRotation)
            {
                base.transform.rotation = m_TargetWorldRotation;
            }

            if (m_TrackScale)
            {
                base.transform.localScale = m_TargetLocalScale;
            }
        }
    }

    private void PerformKinematicUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
        {
            if (m_TrackPosition)
            {
                Vector3 position = (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default) ? m_TargetWorldPosition : (m_TargetWorldPosition - m_Rigidbody.worldCenterOfMass + m_Rigidbody.position);
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.MovePosition(position);
            }

            if (m_TrackRotation)
            {
                m_Rigidbody.angularVelocity = Vector3.zero;
                m_Rigidbody.MoveRotation(m_TargetWorldRotation);
            }

            if (m_TrackScale)
            {
                base.transform.localScale = m_TargetLocalScale;
            }
        }
    }

    private void PerformVelocityTrackingUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase != 0)
        {
            return;
        }

        if (m_TrackPosition)
        {
            m_Rigidbody.velocity *= 1f - m_VelocityDamping;
            Vector3 a = ((m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default) ? (m_TargetWorldPosition - base.transform.position) : (m_TargetWorldPosition - m_Rigidbody.worldCenterOfMass)) / timeDelta;
            if (!float.IsNaN(a.x))
            {
                m_Rigidbody.velocity += a * m_VelocityScale;
            }
        }

        if (m_TrackScale)
        {
            base.transform.localScale = m_TargetLocalScale;
        }

        if (!m_TrackRotation)
        {
            return;
        }

        m_Rigidbody.angularVelocity *= 1f - m_AngularVelocityDamping;
        (m_TargetWorldRotation * Quaternion.Inverse(base.transform.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
        {
            angle -= 360f;
        }

        if (Mathf.Abs(angle) > Mathf.Epsilon)
        {
            Vector3 a2 = axis * (angle * ((float)Math.PI / 180f)) / timeDelta;
            if (!float.IsNaN(a2.x))
            {
                m_Rigidbody.angularVelocity += a2 * m_AngularVelocityScale;
            }
        }
    }

    private void UpdateInteractorLocalPose(XRBaseInteractor interactor)
    {
        if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Legacy)
        {
            UpdateInteractorLocalPoseLegacy(interactor);
            return;
        }

        Transform transform = (m_AttachTransform != null) ? m_AttachTransform : base.transform;
        Vector3 direction = base.transform.position - transform.position;
        m_InteractorLocalPosition = transform.InverseTransformDirection(direction);
        m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(base.transform.rotation) * transform.rotation);
    }

    private void UpdateInteractorLocalPoseLegacy(XRBaseInteractor interactor)
    {
        Transform transform = (m_AttachTransform != null) ? m_AttachTransform : base.transform;
        Vector3 direction = m_Rigidbody.worldCenterOfMass - transform.position;
        Vector3 interactorLocalPosition = transform.InverseTransformDirection(direction);
        Vector3 lossyScale = interactor.attachTransform.lossyScale;
        lossyScale = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
        interactorLocalPosition.Scale(lossyScale);
        m_InteractorLocalPosition = interactorLocalPosition;
        m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(base.transform.rotation) * transform.rotation);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        Grab();
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        Drop();
    }

    protected virtual void Grab()
    {
        m_OriginalSceneParent = base.transform.parent;
        base.transform.SetParent(null);
        m_CurrentMovementType = (base.selectingInteractor.selectedInteractableMovementTypeOverride ?? m_MovementType);
        SetupRigidbodyGrab(m_Rigidbody);
        m_DetachVelocity = Vector3.zero;
        m_DetachAngularVelocity = Vector3.zero;
        m_TargetWorldPosition = ((m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default) ? base.transform.position : m_Rigidbody.worldCenterOfMass);
        m_TargetWorldRotation = base.transform.rotation;
        m_TargetLocalScale = base.transform.lossyScale;
        m_ParentLocalScale = base.transform.localScale;
        m_CurrentAttachEaseTime = 0f;
        UpdateInteractorLocalPose(base.selectingInteractor);
        SmoothVelocityStart();
    }

    protected virtual void Drop()
    {
        if (m_RetainTransformParent && m_OriginalSceneParent != null && !m_OriginalSceneParent.gameObject.activeInHierarchy)
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Retain Transform Parent is set to true, and has a non-null Original Scene Parent. However, the old parent is deactivated so we are choosing not to re-parent upon dropping.", this);
            }
        }
        else if (m_RetainTransformParent && base.gameObject.activeInHierarchy)
        {
            base.transform.SetParent(m_OriginalSceneParent);
        }

        SetupRigidbodyDrop(m_Rigidbody);
        m_CurrentMovementType = m_MovementType;
        m_DetachInLateUpdate = true;
        SmoothVelocityEnd();
    }

    protected virtual void Detach()
    {
        if (m_ThrowOnDetach)
        {
            m_Rigidbody.velocity = m_DetachVelocity;
            m_Rigidbody.angularVelocity = m_DetachAngularVelocity;
        }
    }

    protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
    {
        m_WasKinematic = rigidbody.isKinematic;
        m_UsedGravity = rigidbody.useGravity;
        m_OldDrag = rigidbody.drag;
        m_OldAngularDrag = rigidbody.angularDrag;
        rigidbody.isKinematic = (m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous);
        rigidbody.useGravity = false;
        rigidbody.drag = 0f;
        rigidbody.angularDrag = 0f;
    }

    protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
    {
        rigidbody.isKinematic = m_WasKinematic;
        rigidbody.useGravity = (m_UsedGravity | m_ForceGravityOnDetach);
        rigidbody.drag = m_OldDrag;
        rigidbody.angularDrag = m_OldAngularDrag;
    }

    private void SmoothVelocityStart()
    {
        if (!(base.selectingInteractor == null))
        {
            m_LastPosition = base.selectingInteractor.attachTransform.position;
            m_LastRotation = base.selectingInteractor.attachTransform.rotation;
            Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
            Array.Clear(m_ThrowSmoothingVelocityFrames, 0, m_ThrowSmoothingVelocityFrames.Length);
            Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
            m_ThrowSmoothingCurrentFrame = 0;
        }
    }

    private void SmoothVelocityEnd()
    {
        if (m_ThrowOnDetach)
        {
            Vector3 smoothedVelocityValue = GetSmoothedVelocityValue(m_ThrowSmoothingVelocityFrames);
            Vector3 smoothedVelocityValue2 = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
            m_DetachVelocity = smoothedVelocityValue * m_ThrowVelocityScale;
            m_DetachAngularVelocity = smoothedVelocityValue2 * m_ThrowAngularVelocityScale;
        }
    }

    private void SmoothVelocityUpdate()
    {
        if (!(base.selectingInteractor == null))
        {
            m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
            m_ThrowSmoothingVelocityFrames[m_ThrowSmoothingCurrentFrame] = (base.selectingInteractor.attachTransform.position - m_LastPosition) / Time.deltaTime;
            Quaternion quaternion = base.selectingInteractor.attachTransform.rotation * Quaternion.Inverse(m_LastRotation);
            m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = new Vector3(Mathf.DeltaAngle(0f, quaternion.eulerAngles.x), Mathf.DeltaAngle(0f, quaternion.eulerAngles.y), Mathf.DeltaAngle(0f, quaternion.eulerAngles.z)) / Time.deltaTime * ((float)Math.PI / 180f);
            m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % 20;
            m_LastPosition = base.selectingInteractor.attachTransform.position;
            m_LastRotation = base.selectingInteractor.attachTransform.rotation;
        }
    }

    private Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
    {
        Vector3 a = default(Vector3);
        float num = 0f;
        for (int i = 0; i < 20; i++)
        {
            int num2 = ((m_ThrowSmoothingCurrentFrame - i - 1) % 20 + 20) % 20;
            if (m_ThrowSmoothingFrameTimes[num2] == 0f)
            {
                break;
            }

            float num3 = (Time.time - m_ThrowSmoothingFrameTimes[num2]) / m_ThrowSmoothingDuration;
            float num4 = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - num3, 0f, 1f));
            a += velocityFrames[num2] * num4;
            num += num4;
            if (Time.time - m_ThrowSmoothingFrameTimes[num2] > m_ThrowSmoothingDuration)
            {
                break;
            }
        }

        if (num > 0f)
        {
            return a / num;
        }

        return Vector3.zero;
    }
}
