using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes session relative data for a reference point.
    /// </summary>
    /// <seealso cref="XRReferencePointSubsystem"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRReferencePoint : ITrackable, IEquatable<XRReferencePoint>
    {
        /// <summary>
        /// Gets a default-initialized <see cref="XRReferencePoint"/>. This may be
        /// different from the zero-initialized version, e.g., the <see cref="pose"/>
        /// is <c>Pose.identity</c> instead of zero-initialized.
        /// </summary>
        /// <returns>A default <see cref="XRReferencePoint"/>.</returns>
        public static XRReferencePoint GetDefault()
        {
            return new XRReferencePoint(
                TrackableId.invalidId,
                Pose.identity,
                TrackingState.None,
                IntPtr.Zero);
        }

        /// <summary>
        /// Constructs the session relative data for reference point.
        /// This is typically provided by an implementation of the <see cref="XRReferencePointSubsystem"/>
        /// and not invoked directly.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this reference point.</param>
        /// <param name="pose">The <c>Pose</c>, in session space, of the reference point.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the reference point.</param>
        /// <param name="nativePtr">A native pointer associated with the reference point. The data pointed to by
        /// this pointer is implementation-specific.</param>
        public XRReferencePoint(
            TrackableId trackableId,
            Pose pose,
            TrackingState trackingState,
            IntPtr nativePtr)
        {
            m_Id = trackableId;
            m_Pose = pose;
            m_TrackingState = trackingState;
            m_NativePtr = nativePtr;
        }

        /// <summary>
        /// Get the <see cref="TrackableId"/> associated with this reference point.
        /// </summary>
        public TrackableId trackableId
        {
            get { return m_Id; }
        }

        /// <summary>
        /// Get the <c>Pose</c>, in session space, for this reference point.
        /// </summary>
        public Pose pose
        {
            get { return m_Pose; }
        }

        /// <summary>
        /// Get the <see cref="TrackingState"/> of this reference point.
        /// </summary>
        public TrackingState trackingState
        {
            get { return m_TrackingState; }
        }

        /// <summary>
        /// A native pointer associated with the reference point.
        /// The data pointed to by this pointer is implementation-specific.
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
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + m_TrackingState.GetHashCode();
                hashCode = hashCode * 486187739 + m_NativePtr.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(XRReferencePoint other)
        {
            return
                m_Id == other.m_Id &&
                m_Pose == other.m_Pose &&
                m_TrackingState == other.m_TrackingState &&
                m_NativePtr == other.m_NativePtr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return Equals((XRReferencePoint)obj);
        }

        public static bool operator==(XRReferencePoint lhs, XRReferencePoint rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(XRReferencePoint lhs, XRReferencePoint rhs)
        {
            return !lhs.Equals(rhs);
        }

        TrackableId m_Id;

        Pose m_Pose;

        TrackingState m_TrackingState;

        IntPtr m_NativePtr;
    }
}
