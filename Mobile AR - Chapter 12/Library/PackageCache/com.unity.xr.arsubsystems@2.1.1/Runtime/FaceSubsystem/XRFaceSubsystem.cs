using System;
using Unity.Collections;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif
namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// An abstract class that provides a generic API for low-level face tracking features.
    /// </summary>
    /// <remarks>
    /// This class can be used to access face tracking features in your app via accessing the generic API.
    /// It can also be extended to provide an implementation of a provider which provides the face tracking data
    /// to the higher level code.
    /// </remarks>
    public abstract class XRFaceSubsystem : TrackingSubsystem<XRFace, XRFaceSubsystemDescriptor>
    {
        /// <summary>
        /// Constructs a face subsystem. Do not invoked directly; call <c>Create</c> on the <see cref="XRFaceSubsystemDescriptor"/> instead.
        /// </summary>
        public XRFaceSubsystem()
        {
            m_Provider = CreateProvider();
            m_DefaultFace = XRFace.GetDefault();
        }

        /// <summary>
        /// Start the face subsystem, i.e., start tracking faces.
        /// </summary>
        public override void Start()
        {
            if (!m_Running)
                m_Provider.Start();

            m_Running = true;
        }

        /// <summary>
        /// Destroy the face subsystem.
        /// </summary>
#if UNITY_2019_3_OR_NEWER
        protected sealed override void OnDestroy()
#else
        public sealed override void Destroy()
#endif
        {
            if (m_Running)
                Stop();

            m_Provider.Destroy();
        }

        /// <summary>
        /// Stop the subsystem, i.e., stop tracking faces.
        /// </summary>
        public override void Stop()
        {
            if (m_Running)
                m_Provider.Stop();

            m_Running = false;
        }

        /// <summary>
        /// Returns <c>true</c> if face tracking is supported.
        /// </summary>
        public bool supported
        {
            get { return m_Provider.supported; }
        }

        /// <summary>
        /// Get the changes (added, updated, and removed) faces since the last call to <see cref="GetChanges(Allocator)"/>.
        /// </summary>
        /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
        /// <returns>
        /// <see cref="TrackableChanges{T}"/> describing the faces that have been added, updated, and removed
        /// since the last call to <see cref="GetChanges(Allocator)"/>. The caller owns the memory allocated with <c>Allocator</c>.
        /// </returns>
        public override TrackableChanges<XRFace> GetChanges(Allocator allocator)
        {
            var changes = m_Provider.GetChanges(m_DefaultFace, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Get the mesh data associated with the face with <paramref name="faceId"/>. The <paramref name="faceMesh"/>
        /// is reused if it is the correct size, otherwise, it is disposed and reallocated using <paramref name="allocator"/>.
        /// </summary>
        /// <param name="faceId">The <see cref="TrackableId"/> for a <see cref="XRFace"/>.</param>
        /// <param name="allocator">The allocator to use for the returned data if a resize is necessary. Must be <c>Allocator.TempJob</c> or <c>Allocator.Persistent</c>.</param>
        /// <param name="faceMesh">The container for the mesh data to either re-use or re-allocate.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"> is <c>Allocator.Temp</c></exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"> is <c>Allocator.None</c></exception>
        public virtual void GetFaceMesh(TrackableId faceId, Allocator allocator, ref XRFaceMesh faceMesh)
        {
            if (allocator == Allocator.Temp)
                throw new InvalidOperationException("Allocator.Temp is not supported. Use Allocator.TempJob if you wish to use a temporary allocator.");

            if (allocator == Allocator.None)
                throw new InvalidOperationException("Allocator.None is not a valid allocator.");

            m_Provider.GetFaceMesh(faceId, allocator, ref faceMesh);
        }

        /// <summary>
        /// Creates an instance of an implementation-specific <see cref="IProvider"/>.
        /// </summary>
        /// <returns>An implementation of the <see cref="IProvider"/> class.</returns>
        protected abstract IProvider CreateProvider();

        /// <summary>
        /// Class to be implemented by an implementor of the <see cref="XRFaceSubsystem"/>.
        /// </summary>
        protected class IProvider
        {
            /// <summary>
            /// Called by <see cref="XRFaceSubsystem.Start"/>. Only invoked if not already running.
            /// </summary>
            public virtual void Start()
            { }

            /// <summary>
            /// Called by <see cref="XRFaceSubsystem.Stop"/>. Only invoked if current running.
            /// </summary>
            public virtual void Stop()
            { }

            /// <summary>
            /// Called by <see cref="XRFaceSubsystem.Destroy"/> when the subsystem is destroyed.
            /// </summary>
            public virtual void Destroy()
            { }

            /// <summary>
            /// Called by <see cref="XRFaceSubsystem.supported"/>.
            /// Return <c>true</c> if face tracking is supported on the current device.
            /// </summary>
            public virtual bool supported
            {
                get { return false; }
            }

            /// <summary>
            /// Get the mesh data associated with the face with <paramref name="faceId"/>. The <paramref name="faceMesh"/>
            /// should be reused if it is the correct size, otherwise, its arrays should be reallocated with <paramref name="allocator"/>.
            /// Use <see cref="XRFaceMesh.Assign(XRFaceMesh)"/> to ensure unused <c>NativeArray</c>s are disposed properly and
            /// <see cref="CreateOrResizeNativeArrayIfNecessary"/> to resize individual arrays.
            /// </summary>
            /// <param name="faceId">The <see cref="TrackableId"/> for a <see cref="XRFace"/>.</param>
            /// <param name="allocator">The allocator to use for the returned data if a resize is necessary.</param>
            /// <param name="faceMesh">The container for the mesh data to either re-use or re-allocate.</param>
            /// <example>
            /// <code>
            /// var vertices = faceMesh.vertices;
            /// CreateOrResizeNativeArrayIfNecessary(numVertices, allocator, ref vertices);
            ///
            /// ...
            ///
            /// faceMesh.Assign(new XRFaceMesh
            /// {
            ///     vertices = vertices,
            ///     indices = ...
            /// });
            /// </code>
            /// </example>
            public virtual void GetFaceMesh(TrackableId faceId, Allocator allocator, ref XRFaceMesh faceMesh)
            {
                faceMesh.Dispose();
                faceMesh = default(XRFaceMesh);
            }

            /// <summary>
            /// Get the changes (added, updated, and removed) faces since the last call to <see cref="GetChanges(Allocator)"/>.
            /// </summary>
            /// <param name="defaultFace">
            /// The default face. This should be used to initialize the returned <c>NativeArray</c>s for backwards compatibility.
            /// See <see cref="TrackableChanges{T}.TrackableChanges(void*, int, void*, int, void*, int, T, int, Allocator)"/>.
            /// </param>
            /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
            /// <returns>
            /// <see cref="TrackableChanges{T}"/> describing the faces that have been added, updated, and removed
            /// since the last call to <see cref="GetChanges(Allocator)"/>. The changes should be allocated using
            /// <paramref name="allocator"/>.
            /// </returns>
            public virtual TrackableChanges<XRFace> GetChanges(
                XRFace defaultFace,
                Allocator allocator)
            {
                return default(TrackableChanges<XRFace>);
            }
        }

        IProvider m_Provider;

        XRFace m_DefaultFace;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRFace> m_ValidationUtility =
            new ValidationUtility<XRFace>();
#endif
    }
}
