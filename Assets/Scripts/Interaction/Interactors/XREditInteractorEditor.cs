using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Custom editor for an <see cref="XREditInteractor"/>.
/// </summary>
[CustomEditor(typeof(XREditInteractor), true), CanEditMultipleObjects]
public class XREditInteractorEditor : XRBaseControllerInteractorEditor
{
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.maxRaycastDistance"/>.</summary>
    protected SerializedProperty m_MaxRaycastDistance;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.hitDetectionType"/>.</summary>
    protected SerializedProperty m_HitDetectionType;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.sphereCastRadius"/>.</summary>
    protected SerializedProperty m_SphereCastRadius;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.raycastMask"/>.</summary>
    protected SerializedProperty m_RaycastMask;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.raycastTriggerInteraction"/>.</summary>
    protected SerializedProperty m_RaycastTriggerInteraction;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.hitClosestOnly"/>.</summary>
    protected SerializedProperty m_HitClosestOnly;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.hoverToSelect"/>.</summary>
    protected SerializedProperty m_HoverToSelect;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.hoverTimeToSelect"/>.</summary>
    protected SerializedProperty m_HoverTimeToSelect;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.enableUIInteraction"/>.</summary>
    protected SerializedProperty m_EnableUIInteraction;

    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.lineType"/>.</summary>
    protected SerializedProperty m_LineType;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.blendVisualLinePoints"/>.</summary>
    protected SerializedProperty m_BlendVisualLinePoints;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.endPointDistance"/>.</summary>
    protected SerializedProperty m_EndPointDistance;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.endPointHeight"/>.</summary>
    protected SerializedProperty m_EndPointHeight;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.controlPointDistance"/>.</summary>
    protected SerializedProperty m_ControlPointDistance;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.controlPointHeight"/>.</summary>
    protected SerializedProperty m_ControlPointHeight;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.sampleFrequency"/>.</summary>
    protected SerializedProperty m_SampleFrequency;

    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.velocity"/>.</summary>
    protected SerializedProperty m_Velocity;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.acceleration"/>.</summary>
    protected SerializedProperty m_Acceleration;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.additionalGroundHeight"/>.</summary>
    protected SerializedProperty m_AdditionalGroundHeight;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.additionalFlightTime"/>.</summary>
    protected SerializedProperty m_AdditionalFlightTime;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.referenceFrame"/>.</summary>
    protected SerializedProperty m_ReferenceFrame;

    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.keepSelectedTargetValid"/>.</summary>
    protected SerializedProperty m_KeepSelectedTargetValid;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.allowAnchorControl"/>.</summary>
    protected SerializedProperty m_AllowAnchorControl;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.useForceGrab"/>.</summary>
    protected SerializedProperty m_UseForceGrab;

    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.anchorControlMode"/>.</summary>
    protected SerializedProperty m_AnchorControlMode;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.anchorControlModeText"/>.</summary>
    protected SerializedProperty m_AnchorControlModeText;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.resetAnchorControlMode"/>.</summary>
    protected SerializedProperty m_ResetAnchorControlMode;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.rotateSpeed"/>.</summary>
    protected SerializedProperty m_RotateSpeed;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.translateSpeed"/>.</summary>
    protected SerializedProperty m_TranslateSpeed;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.scaleSpeed"/>.</summary>
    protected SerializedProperty m_ScaleSpeed;
    /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XREditInteractor.anchorRotateReferenceFrame"/>.</summary>
    protected SerializedProperty m_AnchorRotateReferenceFrame;

    /// <summary>
    /// Contents of GUI elements used by this editor.
    /// </summary>
    protected static class Contents
    {
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.maxRaycastDistance"/>.</summary>
        public static readonly GUIContent maxRaycastDistance = EditorGUIUtility.TrTextContent("Max Raycast Distance", "Max distance of ray cast. Increase this value will let you reach further.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.sphereCastRadius"/>.</summary>
        public static readonly GUIContent sphereCastRadius = EditorGUIUtility.TrTextContent("Sphere Cast Radius", "Radius of this Interactor's ray, used for sphere casting.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.raycastMask"/>.</summary>
        public static readonly GUIContent raycastMask = EditorGUIUtility.TrTextContent("Raycast Mask", "Layer mask used for limiting raycast targets.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.raycastTriggerInteraction"/>.</summary>
        public static readonly GUIContent raycastTriggerInteraction = EditorGUIUtility.TrTextContent("Raycast Trigger Interaction", "Type of interaction with trigger colliders via raycast.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.hitClosestOnly"/>.</summary>
        public static readonly GUIContent hitClosestOnly = EditorGUIUtility.TrTextContent("Hit Closest Only", "Consider only the closest Interactable as a valid target for interaction. Enable this to make only the closest Interactable receive hover events.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.hoverToSelect"/>.</summary>
        public static readonly GUIContent hoverToSelect = EditorGUIUtility.TrTextContent("Hover To Select", "Automatically select an Interactable after hovering over it for a period of time.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.hoverTimeToSelect"/>.</summary>
        public static readonly GUIContent hoverTimeToSelect = EditorGUIUtility.TrTextContent("Hover Time To Select", "Number of seconds for which this Interactor must hover over an Interactable to select it.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.enableUIInteraction"/>.</summary>
        public static readonly GUIContent enableUIInteraction = EditorGUIUtility.TrTextContent("Enable Interaction with UI GameObjects", "If checked, this interactor will be able to affect UI.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.lineType"/>.</summary>
        public static readonly GUIContent lineType = EditorGUIUtility.TrTextContent("Line Type", "Line type of the ray cast.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.blendVisualLinePoints"/>.</summary>
        public static readonly GUIContent blendVisualLinePoints = EditorGUIUtility.TrTextContent("Blend Visual Line Points", "Blend the line sample points used for raycasting with the current pose of the controller. Use this to make the line visual stay connected with the controller instead of lagging behind.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.endPointDistance"/>.</summary>
        public static readonly GUIContent endPointDistance = EditorGUIUtility.TrTextContent("End Point Distance", "Increase this value distance will make the end of curve further from the start point.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.controlPointDistance"/>.</summary>
        public static readonly GUIContent controlPointDistance = EditorGUIUtility.TrTextContent("Control Point Distance", "Increase this value will make the peak of the curve further from the start point.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.endPointHeight"/>.</summary>
        public static readonly GUIContent endPointHeight = EditorGUIUtility.TrTextContent("End Point Height", "Decrease this value will make the end of the curve drop lower relative to the start point.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.controlPointHeight"/>.</summary>
        public static readonly GUIContent controlPointHeight = EditorGUIUtility.TrTextContent("Control Point Height", "Increase this value will make the peak of the curve higher relative to the start point.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.sampleFrequency"/>.</summary>
        public static readonly GUIContent sampleFrequency = EditorGUIUtility.TrTextContent("Sample Frequency", "The number of sample points used to approximate curved paths. Larger values produce a better quality approximate at the cost of reduced performance due to the number of raycasts.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.velocity"/>.</summary>
        public static readonly GUIContent velocity = EditorGUIUtility.TrTextContent("Velocity", "Initial velocity of the projectile. Increase this value will make the curve reach further.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.acceleration"/>.</summary>
        public static readonly GUIContent acceleration = EditorGUIUtility.TrTextContent("Acceleration", "Gravity of the projectile in the reference frame.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.additionalGroundHeight"/>.</summary>
        public static readonly GUIContent additionalGroundHeight = EditorGUIUtility.TrTextContent("Additional Ground Height", "Additional height below ground level that the projectile will continue to. Increasing this value will make the end point drop lower in height.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.additionalFlightTime"/>.</summary>
        public static readonly GUIContent additionalFlightTime = EditorGUIUtility.TrTextContent("Additional Flight Time", "Additional flight time after the projectile lands at the adjusted ground level. Increasing this value will make the end point drop lower in height.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.referenceFrame"/>.</summary>
        public static readonly GUIContent referenceFrame = EditorGUIUtility.TrTextContent("Reference Frame", "The reference frame of the curve to define the ground plane and up. If not set at startup it will try to find the Rig GameObject, and if that does not exist it will use global up and origin by default.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.hitDetectionType"/>.</summary>
        public static readonly GUIContent hitDetectionType = EditorGUIUtility.TrTextContent("Hit Detection Type", "The type of hit detection used to hit interactable objects.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.keepSelectedTargetValid"/>.</summary>
        public static readonly GUIContent keepSelectedTargetValid = EditorGUIUtility.TrTextContent("Keep Selected Target Valid", "Keep selecting the target when not pointing to it after initially selecting it. It is recommended to set this value to true for grabbing objects, false for teleportation interactables.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.allowAnchorControl"/>.</summary>
        public static readonly GUIContent allowAnchorControl = EditorGUIUtility.TrTextContent("Anchor Control", "Allows the user to move the attach anchor point using the joystick.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.useForceGrab"/>.</summary>
        public static readonly GUIContent useForceGrab = EditorGUIUtility.TrTextContent("Force Grab", "Force grab moves the object to your hand rather than interacting with it at a distance.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.anchorControlMode"/>.</summary>
        public static readonly GUIContent anchorControlMode = EditorGUIUtility.TrTextContent("Anchor Control Mode", "Current Anchor Control Mode (Scale/Rotate/Move).");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.anchorControlModeText"/>.</summary>
        public static readonly GUIContent anchorControlModeText = EditorGUIUtility.TrTextContent("Anchor Control Mode Text", "The TMPro component to show this interactors anchor control mode.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.resetAnchorControlMode"/>.</summary>
        public static readonly GUIContent resetAnchorControlMode = EditorGUIUtility.TrTextContent("Reset Anchor Control Mode", "Reset this controllers anchor control mode on selecting an interactable.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.rotateSpeed"/>.</summary>
        public static readonly GUIContent rotateSpeed = EditorGUIUtility.TrTextContent("Rotate Speed", "Speed that the anchor is rotated.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.translateSpeed"/>.</summary>
        public static readonly GUIContent translateSpeed = EditorGUIUtility.TrTextContent("Translate Speed", "Speed that the anchor is translated.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.scaleSpeed"/>.</summary>
        public static readonly GUIContent scaleSpeed = EditorGUIUtility.TrTextContent("Scale Speed", "Speed that the anchor is scaled.");
        /// <summary><see cref="GUIContent"/> for <see cref="XREditInteractor.anchorRotateReferenceFrame"/>.</summary>
        public static readonly GUIContent anchorRotateReferenceFrame = EditorGUIUtility.TrTextContent("Rotate Reference Frame", "The optional reference frame to define the up axis when rotating the attach anchor point. When not set, rotates about the local up axis of the attach transform.");
    }

    /// <inheritdoc />
    protected override void OnEnable()
    {
        base.OnEnable();

        m_MaxRaycastDistance = serializedObject.FindProperty("m_MaxRaycastDistance");
        m_HitDetectionType = serializedObject.FindProperty("m_HitDetectionType");
        m_SphereCastRadius = serializedObject.FindProperty("m_SphereCastRadius");
        m_RaycastMask = serializedObject.FindProperty("m_RaycastMask");
        m_RaycastTriggerInteraction = serializedObject.FindProperty("m_RaycastTriggerInteraction");
        m_HitClosestOnly = serializedObject.FindProperty("m_HitClosestOnly");
        m_HoverToSelect = serializedObject.FindProperty("m_HoverToSelect");
        m_HoverTimeToSelect = serializedObject.FindProperty("m_HoverTimeToSelect");
        m_EnableUIInteraction = serializedObject.FindProperty("m_EnableUIInteraction");

        m_LineType = serializedObject.FindProperty("m_LineType");
        m_BlendVisualLinePoints = serializedObject.FindProperty("m_BlendVisualLinePoints");
        m_EndPointDistance = serializedObject.FindProperty("m_EndPointDistance");
        m_EndPointHeight = serializedObject.FindProperty("m_EndPointHeight");
        m_ControlPointDistance = serializedObject.FindProperty("m_ControlPointDistance");
        m_ControlPointHeight = serializedObject.FindProperty("m_ControlPointHeight");
        m_SampleFrequency = serializedObject.FindProperty("m_SampleFrequency");

        m_Velocity = serializedObject.FindProperty("m_Velocity");
        m_Acceleration = serializedObject.FindProperty("m_Acceleration");
        m_AdditionalGroundHeight = serializedObject.FindProperty("m_AdditionalGroundHeight");
        m_AdditionalFlightTime = serializedObject.FindProperty("m_AdditionalFlightTime");
        m_ReferenceFrame = serializedObject.FindProperty("m_ReferenceFrame");

        m_KeepSelectedTargetValid = serializedObject.FindProperty("m_KeepSelectedTargetValid");
        m_AllowAnchorControl = serializedObject.FindProperty("m_AllowAnchorControl");
        m_UseForceGrab = serializedObject.FindProperty("m_UseForceGrab");

        m_AnchorControlMode = serializedObject.FindProperty("m_AnchorControlMode");
        m_AnchorControlModeText = serializedObject.FindProperty("m_AnchorControlModeText");
        m_ResetAnchorControlMode = serializedObject.FindProperty("m_ResetAnchorControlMode");
        m_RotateSpeed = serializedObject.FindProperty("m_RotateSpeed");
        m_TranslateSpeed = serializedObject.FindProperty("m_TranslateSpeed");
        m_ScaleSpeed = serializedObject.FindProperty("m_ScaleSpeed");
        m_AnchorRotateReferenceFrame = serializedObject.FindProperty("m_AnchorRotateReferenceFrame");

        // Set default expanded for some foldouts
        const string initializedKey = "XRI." + nameof(XREditInteractorEditor) + ".Initialized";
        if (!SessionState.GetBool(initializedKey, false))
        {
            SessionState.SetBool(initializedKey, true);
            m_LineType.isExpanded = true;
            m_SelectActionTrigger.isExpanded = true;
        }
    }

    /// <inheritdoc />
    protected override void DrawProperties()
    {
        // Not calling base method to completely override drawn properties

        DrawInteractionManagement();

        EditorGUILayout.Space();

        DrawInteractionConfiguration();

        EditorGUILayout.Space();

        DrawRaycastConfiguration();

        EditorGUILayout.Space();

        DrawSelectionConfiguration();
    }

    /// <inheritdoc />
    protected override void DrawDerivedProperties()
    {
        EditorGUILayout.Space();
        base.DrawDerivedProperties();
    }

    /// <summary>
    /// Draw the property fields related to interaction configuration.
    /// </summary>
    protected virtual void DrawInteractionConfiguration()
    {
        EditorGUILayout.PropertyField(m_EnableUIInteraction, Contents.enableUIInteraction);
        EditorGUILayout.PropertyField(m_UseForceGrab, Contents.useForceGrab);
        EditorGUILayout.PropertyField(m_AllowAnchorControl, Contents.allowAnchorControl);
        if (m_AllowAnchorControl.boolValue)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_AnchorControlMode, Contents.anchorControlMode);
                EditorGUILayout.PropertyField(m_AnchorControlModeText, Contents.anchorControlModeText);
                EditorGUILayout.PropertyField(m_ResetAnchorControlMode, Contents.resetAnchorControlMode);
                EditorGUILayout.PropertyField(m_RotateSpeed, Contents.rotateSpeed);
                EditorGUILayout.PropertyField(m_TranslateSpeed, Contents.translateSpeed);
                EditorGUILayout.PropertyField(m_ScaleSpeed, Contents.scaleSpeed);
                EditorGUILayout.PropertyField(m_AnchorRotateReferenceFrame, Contents.anchorRotateReferenceFrame);
            }
        }

        EditorGUILayout.PropertyField(m_AttachTransform, BaseContents.attachTransform);
    }

    /// <summary>
    /// Draw the Raycast Configuration foldout.
    /// </summary>
    /// <seealso cref="DrawRaycastConfigurationNested"/>
    protected virtual void DrawRaycastConfiguration()
    {
        m_LineType.isExpanded = EditorGUILayout.Foldout(m_LineType.isExpanded, EditorGUIUtility.TrTempContent("Raycast Configuration"), true);
        if (m_LineType.isExpanded)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                DrawRaycastConfigurationNested();
            }
        }
    }

    /// <summary>
    /// Draw the nested contents of the Raycast Configuration foldout.
    /// </summary>
    /// <seealso cref="DrawRaycastConfiguration"/>
    protected virtual void DrawRaycastConfigurationNested()
    {
        EditorGUILayout.PropertyField(m_LineType, Contents.lineType);

        using (new EditorGUI.IndentLevelScope())
        {
            switch (m_LineType.enumValueIndex)
            {
                case (int)XREditInteractor.LineType.StraightLine:
                    EditorGUILayout.PropertyField(m_MaxRaycastDistance, Contents.maxRaycastDistance);
                    break;
                case (int)XREditInteractor.LineType.ProjectileCurve:
                    EditorGUILayout.PropertyField(m_ReferenceFrame, Contents.referenceFrame);
                    EditorGUILayout.PropertyField(m_Velocity, Contents.velocity);
                    EditorGUILayout.PropertyField(m_Acceleration, Contents.acceleration);
                    EditorGUILayout.PropertyField(m_AdditionalGroundHeight, Contents.additionalGroundHeight);
                    EditorGUILayout.PropertyField(m_AdditionalFlightTime, Contents.additionalFlightTime);
                    EditorGUILayout.PropertyField(m_SampleFrequency, Contents.sampleFrequency);
                    break;
                case (int)XREditInteractor.LineType.BezierCurve:
                    EditorGUILayout.PropertyField(m_ReferenceFrame, Contents.referenceFrame);
                    EditorGUILayout.PropertyField(m_EndPointDistance, Contents.endPointDistance);
                    EditorGUILayout.PropertyField(m_EndPointHeight, Contents.endPointHeight);
                    EditorGUILayout.PropertyField(m_ControlPointDistance, Contents.controlPointDistance);
                    EditorGUILayout.PropertyField(m_ControlPointHeight, Contents.controlPointHeight);
                    EditorGUILayout.PropertyField(m_SampleFrequency, Contents.sampleFrequency);
                    break;
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_RaycastMask, Contents.raycastMask);
        EditorGUILayout.PropertyField(m_RaycastTriggerInteraction, Contents.raycastTriggerInteraction);
        EditorGUILayout.PropertyField(m_HitDetectionType, Contents.hitDetectionType);
        if (m_HitDetectionType.enumValueIndex == (int)XREditInteractor.HitDetectionType.SphereCast)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_SphereCastRadius, Contents.sphereCastRadius);
            }
        }
        EditorGUILayout.PropertyField(m_HitClosestOnly, Contents.hitClosestOnly);
        EditorGUILayout.PropertyField(m_BlendVisualLinePoints, Contents.blendVisualLinePoints);
    }

    /// <summary>
    /// Draw the Selection Configuration foldout.
    /// </summary>
    /// <seealso cref="DrawSelectionConfigurationNested"/>
    protected virtual void DrawSelectionConfiguration()
    {
        m_SelectActionTrigger.isExpanded = EditorGUILayout.Foldout(m_SelectActionTrigger.isExpanded, EditorGUIUtility.TrTempContent("Selection Configuration"), true);
        if (m_SelectActionTrigger.isExpanded)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                DrawSelectionConfigurationNested();
            }
        }
    }

    /// <summary>
    /// Draw the nested contents of the Selection Configuration foldout.
    /// </summary>
    /// <seealso cref="DrawSelectionConfiguration"/>
    protected virtual void DrawSelectionConfigurationNested()
    {
        DrawSelectActionTrigger();
        EditorGUILayout.PropertyField(m_KeepSelectedTargetValid, Contents.keepSelectedTargetValid);
        EditorGUILayout.PropertyField(m_HideControllerOnSelect, BaseControllerContents.hideControllerOnSelect);
        EditorGUILayout.PropertyField(m_HoverToSelect, Contents.hoverToSelect);
        if (m_HoverToSelect.boolValue)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_HoverTimeToSelect, Contents.hoverTimeToSelect);
            }
        }
        EditorGUILayout.PropertyField(m_StartingSelectedInteractable, BaseContents.startingSelectedInteractable);
    }
}
