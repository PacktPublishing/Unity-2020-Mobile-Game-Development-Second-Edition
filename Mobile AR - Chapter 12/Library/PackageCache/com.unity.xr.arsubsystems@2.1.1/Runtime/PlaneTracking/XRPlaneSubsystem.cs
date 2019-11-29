using System;
using Unity.Collections;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Base class for plane subsystems.
    /// </summary>
    /// <remarks>
    /// This subsystem surfaces information regarding plane (i.e., flat surface) detection in the physical environment.
    /// Implementations are typically found in other provider or platform-specific packages.
    /// </remarks>
    public abstract class XRPlaneSubsystem : TrackingSubsystem<BoundedPlane, XRPlaneSubsystemDescriptor>
    {
        /// <summary>
        /// Constructs a plane subsystem. Do not invoked directly; call <c>Create</c> on the <see cref="XRPlaneSubsystemDescriptor"/> instead.
        /// </summary>
        public XRPlaneSubsystem()
        {
            m_Provider = CreateProvider();
            m_DefaultPlane = BoundedPlane.GetDefault();
        }

        /// <summary>
        /// Start the plane subsystem, i.e., start tracking planes.
        /// </summary>
        public override void Start()
        {
            if (!m_Running)
                m_Provider.Start();

            m_Running = true;
        }

        /// <summary>
        /// Destroy the plane subsystem.
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
        /// Stop the subsystem, i.e., stop tracking planes.
        /// </summary>
        public override void Stop()
        {
            if (m_Running)
                m_Provider.Stop();

            m_Running = false;
        }

        /// <summary>
        /// Set <see cref="PlaneDetectionMode"/>, e.g., to enable different modes of detection.
        /// </summary>
        public PlaneDetectionMode planeDetectionMode
        {
            set
            {
                m_Provider.planeDetectionMode = value;
            }
        }

        /// <summary>
        /// Get the changes (added, updated, and removed) planes since the last call to <see cref="GetChanges(Allocator)"/>.
        /// </summary>
        /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
        /// <returns>
        /// <see cref="TrackableChanges{T}"/> describing the planes that have been added, updated, and removed
        /// since the last call to <see cref="GetChanges(Allocator)"/>. The caller owns the memory allocated with <c>Allocator</c>.
        /// </returns>
        public override TrackableChanges<BoundedPlane> GetChanges(Allocator allocator)
        {
            var changes = m_Provider.GetChanges(m_DefaultPlane, allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// Gets the boundary polygon describing the plane.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with the plane of which to retrieve the boundary.</param>
        /// <param name="allocator">An <c>Allocator</c> to use if <paramref name="boundary"/> needs to be created. <c>Allocator.Temp</c> is not supported; use <c>Allocator.TempJob</c> if you need temporary memory.</param>
        /// <param name="boundary">The boundary will be stored here. If <c>boundary</c> is the same length as the new boundary,
        /// it is simply overwritten with the new data. Otherwise, it is disposed and recreated with the corect length.</param>
        public void GetBoundary(
            TrackableId trackableId,
            Allocator allocator,
            ref NativeArray<Vector2> boundary)
        {
            if (allocator == Allocator.Temp)
                throw new InvalidOperationException("Allocator.Temp is not supported. Use Allocator.TempJob if you wish to use a temporary allocator.");

            if (allocator == Allocator.None)
                throw new InvalidOperationException("Allocator.None is not a valid allocator.");

            m_Provider.GetBoundary(trackableId, allocator, ref boundary);
        }

        /// <summary>
        /// Concrete classes must implement this to provide the provider-specific implementation.
        /// </summary>
        /// <returns></returns>
        protected abstract IProvider CreateProvider();

        /// <summary>
        /// Creates or resizes the <paramref name="array"/> if necessary. If <paramref name="array"/>
        /// has been allocated and its length is equal to <paramref name="length"/>, then this method
        /// does nothing. If its length is different, then it is first disposed before being assigned
        /// to a new <c>NativeArray</c>.
        /// </summary>
        /// <param name="length">The length that <paramref name="array"/> will have after this method returns.</param>
        /// <param name="allocator">If allocation is necessary, this allocator will be used to create the new <c>NativeArray</c>.</param>
        /// <param name="array">The array to create or resize.</param>
        protected static void CreateOrResizeNativeArrayIfNecessary<T>(
            int length,
            Allocator allocator,
            ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated)
            {
                if (array.Length != length)
                {
                    array.Dispose();
                    array = new NativeArray<T>(length, allocator);
                }
            }
            else
            {
                array = new NativeArray<T>(length, allocator);
            }
        }

        /// <summary>
        /// The API that derived classes must implement.
        /// </summary>
        protected class IProvider
        {
            /// <summary>
            /// Start the plane subsystem, i.e., start tracking planes.
            /// </summary>
            public virtual void Start()
            { }

            /// <summary>
            /// Stop the subsystem, i.e., stop tracking planes.
            /// </summary>
            public virtual void Stop()
            { }

            /// <summary>
            /// Destroy the plane subsystem. <see cref="Stop"/> is always called first.
            /// </summary>
            public virtual void Destroy()
            { }

            /// <summary>
            /// Retrieves the boundary points of the plane with <paramref name="trackableId"/>.
            /// </summary>
            /// <param name="trackableId">The id of the plane.</param>
            /// <param name="allocator">An <c>Allocator</c> to use for the returned <c>NativeArray</c>.</param>
            /// <param name="boundary">An existing <c>NativeArray</c> to update or recreate if necessary.
            /// See <see cref="CreateOrResizeNativeArrayIfNecessary{T}(int, Allocator, ref NativeArray{T})"/>.</param>
            /// <returns>A <c>NativeArray</c> of 2D plane-space (relative to <see cref="BoundedPlane.pose"/>) positions describing
            /// the plane's boundary. <c>NativeArray</c> should be allocated using <paramref name="allocator"/>.</returns>
            public virtual void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                throw new NotSupportedException("Boundary vertices are not supported.");
            }

            /// <summary>
            /// Get the changes (added, updated, and removed) planes since the last call to <see cref="GetChanges(Allocator)"/>.
            /// </summary>
            /// <param name="defaultPlane">
            /// The default plane. This should be used to initialize the returned <c>NativeArray</c>s for backwards compatibility.
            /// See <see cref="TrackableChanges{T}.TrackableChanges(void*, int, void*, int, void*, int, T, int, Allocator)"/>.
            /// </param>
            /// <param name="allocator">An <c>Allocator</c> to use when allocating the returned <c>NativeArray</c>s.</param>
            /// <returns>
            /// <see cref="TrackableChanges{T}"/> describing the planes that have been added, updated, and removed
            /// since the last call to <see cref="GetChanges(Allocator)"/>. The changes should be allocated using
            /// <paramref name="allocator"/>.
            /// </returns>
            public virtual TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane,
                Allocator allocator)
            {
                return default(TrackableChanges<BoundedPlane>);
            }

            /// <summary>
            /// Set the <see cref="PlaneDetectionMode"/>.
            /// </summary>
            public virtual PlaneDetectionMode planeDetectionMode
            {
                set { }
            }
        }

        IProvider m_Provider;

        BoundedPlane m_DefaultPlane;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<BoundedPlane> m_ValidationUtility =
            new ValidationUtility<BoundedPlane>();
#endif
    }
}
