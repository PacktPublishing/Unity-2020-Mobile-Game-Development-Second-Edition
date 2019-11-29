using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Creates, updates, and removes <c>GameObject</c>s with <see cref="ARFace"/> components under the <see cref="ARSessionOrigin"/>'s <see cref="ARSessionOrigin.trackablesParent"/>.
    /// </summary>
    /// <remarks>
    /// When enabled, this component subscribes to <see cref="ARSubsystemManager.faceAdded"/>,
    /// <see cref="ARSubsystemManager.faceUpdated"/>, and <see cref="ARSubsystemManager.faceRemoved"/>.
    /// If this component is disabled, and there are no other subscribers to those events,
    /// face detection will be disabled on the device.
    /// </remarks>
    [RequireComponent(typeof(ARSessionOrigin))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ARUpdateOrder.k_FaceManager)]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest?preview=1&subfolder=/api/UnityEngine.XR.ARFoundation.ARFaceManager.html")]
    public sealed class ARFaceManager : ARTrackableManager<
        XRFaceSubsystem,
        XRFaceSubsystemDescriptor,
        XRFace,
        ARFace>
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created face.")]
        GameObject m_FacePrefab;

        /// <summary>
        /// Getter/setter for the Face Prefab.
        /// </summary>
        public GameObject facePrefab
        {
            get { return m_FacePrefab; }
            set { m_FacePrefab = value; }
        }

        /// <summary>
        /// Not all devices support face tracking, even if the target platform generally does.
        /// Use this to check whether face tracking is supported at runtime.
        /// </summary>
        public bool supported
        {
            get
            {
                CreateSubsystemIfNecessary();
                return (subsystem != null) && subsystem.supported;
            }
        }

        /// <summary>
        /// Raised for each new <see cref="ARFace"/> detected in the environment.
        /// </summary>
        public event Action<ARFacesChangedEventArgs> facesChanged;

        /// <summary>
        /// Attempts to retrieve an <see cref="ARFace"/>.
        /// </summary>
        /// <param name="faceId">The <c>TrackableId</c> associated with the <see cref="ARFace"/>.</param>
        /// <returns>The <see cref="ARFace"/>if found. <c>null</c> otherwise.</returns>
        public ARFace TryGetFace(TrackableId faceId)
        {
            ARFace face;
            m_Trackables.TryGetValue(faceId, out face);

            return face;
        }

        protected override void OnEnable()
        {
            if (supported)
            {
                subsystem.Start();
            }
            else
            {
                enabled = false;
            }
        }

        protected override void OnAfterSetSessionRelativeData(
            ARFace face,
            XRFace sessionRelativeData)
        {
            face.UpdateMesh(subsystem);
        }

        protected override void OnTrackablesChanged(
            List<ARFace> added,
            List<ARFace> updated,
            List<ARFace> removed)
        {
            if (facesChanged != null)
            {
                facesChanged(
                    new ARFacesChangedEventArgs(
                        added,
                        updated,
                        removed));
            }
        }

        protected override GameObject GetPrefab()
        {
            return m_FacePrefab;
        }

        protected override string gameObjectName
        {
            get { return "ARFace"; }
        }
    }
}
