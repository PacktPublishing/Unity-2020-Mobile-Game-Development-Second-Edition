using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Contains low-level data for a tracked image in the environment.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRTrackedImage : ITrackable, IEquatable<XRTrackedImage>
    {
        /// <summary>
        /// Constructs an <see cref="XRTrackedImage"/>.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this tracked image.</param>
        /// <param name="sourceImageId">A <c>GUID</c> associated with the source image.</param>
        /// <param name="pose">The <c>Pose</c> associated with the detected image.</param>
        /// <param name="size">The size (i.e., dimensions) of the detected image.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the detected image.</param>
        /// <param name="nativePtr">A native pointer associated with the detected image.</param>
        public XRTrackedImage(
            TrackableId trackableId,
            Guid sourceImageId,
            Pose pose,
            Vector2 size,
            TrackingState trackingState,
            IntPtr nativePtr)
        {
            m_Id = trackableId;
            m_SourceImageId = sourceImageId;
            m_Pose = pose;
            m_Size = size;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
        }

        /// <summary>
        /// Generates a <see cref="XRTrackedImage"/> populated with default values.
        /// </summary>
        /// <returns>A default <see cref="XRTrackedImage"/>.</returns>
        public static XRTrackedImage GetDefault()
        {
            return new XRTrackedImage(
                TrackableId.invalidId,
                default(Guid),
                Pose.identity,
                default(Vector2),
                TrackingState.None,
                IntPtr.Zero);
        }

        /// <summary>
        /// The <see cref="TrackableId"/> associated with this tracked image.
        /// </summary>
        public TrackableId trackableId
        {
            get { return m_Id; }
        }

        /// <summary>
        /// The <c>GUID</c> associated with the source image.
        /// </summary>
        public Guid sourceImageId
        {
            get { return m_SourceImageId; }
        }

        /// <summary>
        /// The <c>Pose</c> associated with this tracked image.
        /// </summary>
        public Pose pose
        {
            get { return m_Pose; }
        }

        /// <summary>
        /// The size (i.e., dimensions) of this tracked image.
        /// </summary>
        public Vector2 size
        {
            get { return m_Size; }
        }

        /// <summary>
        /// The <see cref="TrackingState"/> associated with this tracked image.
        /// </summary>
        public TrackingState trackingState
        {
            get { return m_TrackingState; }
        }

        /// <summary>
        /// A native pointer associated with this tracked image.
        /// The data pointed to by this pointer is implementation-defined.
        /// While its lifetime is also implementation-defined, it should be
        /// valid at least until the next call to
        /// <see cref="XRImageTrackingSubsystem.GetChanges(Allocator)"/>.
        /// </summary>
        public IntPtr nativePtr
        {
            get { return m_NativePtr; }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = hashCode * 486187739 + m_SourceImageId.GetHashCode();
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + m_Size.GetHashCode();
                hashCode = hashCode * 486187739 + m_TrackingState.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(XRTrackedImage other)
        {
            return
                m_Id == other.m_Id &&
                m_SourceImageId == other.m_SourceImageId &&
                m_Pose == other.m_Pose &&
                m_Size == other.m_Size &&
                m_TrackingState == other.m_TrackingState;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return Equals((XRTrackedImage)obj);
        }

        public static bool operator==(XRTrackedImage lhs, XRTrackedImage rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(XRTrackedImage lhs, XRTrackedImage rhs)
        {
            return !lhs.Equals(rhs);
        }

        TrackableId m_Id;
        Guid m_SourceImageId;
        Pose m_Pose;
        Vector2 m_Size;
        TrackingState m_TrackingState;
        IntPtr m_NativePtr;
    }
}
