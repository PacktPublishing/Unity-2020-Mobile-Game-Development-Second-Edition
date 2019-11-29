using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A struct describing face data that is stored in the <see cref="XRFaceSubsystem"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRFace : ITrackable, IEquatable<XRFace>
    {
        // Fields to marshall/serialize from native code
        TrackableId m_TrackableId;
        Pose m_Pose;
        TrackingState m_TrackingState;
        IntPtr m_NativePtr;

        /// <summary>
        /// Get a <see cref="XRFace"/> with reasonable default values.
        /// </summary>
        /// <returns>A new <see cref="XRFace"/> populated with default values.</returns>
        public static XRFace GetDefault()
        {
            var face = default(XRFace);
            face.m_Pose = Pose.identity;
            return face;
        }

        /// <summary>
        /// The unique <see cref="TrackableId"/> of the face as a trackable within the <see cref="XRFaceSubsystem"/>.
        /// </summary>
        /// <remarks>
        /// With this, you are able to extract more data about this particular face from the <see cref="XRFaceSubsystem"/>.
        /// </remarks>
        public TrackableId trackableId
        {
            get { return m_TrackableId; }
        }

        /// <summary>
        /// The <see cref="pose"/> of the face describes its position and rotation in session space.
        /// </summary>
        public Pose pose
        {
            get { return m_Pose; }
        }

        /// <summary>
        /// The tracking state associated with this <see cref="XRFace"/>.
        /// </summary>
        public TrackingState trackingState
        {
            get { return m_TrackingState; }
        }

        /// <summary>
        /// A native pointer associated with this <see cref="XRFace"/>.
        /// </summary>
        /// <remarks>
        /// The data pointed to by this pointer is implementation-defined.
        /// </remarks>
        public IntPtr nativePtr
        {
            get { return m_NativePtr; }
        }

        // IEquatable boilerplate
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return (obj is XRFace) && Equals((XRFace)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = trackableId.GetHashCode();
                hashCode = (hashCode * 486187739) + pose.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)trackingState).GetHashCode();
                hashCode = (hashCode * 486187739) + nativePtr.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator==(XRFace lhs, XRFace rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(XRFace lhs, XRFace rhs)
        {
            return !lhs.Equals(rhs);
        }

        public bool Equals(XRFace other)
        {
            return
                trackableId.Equals(other.trackableId) &&
                pose.Equals(other.pose) &&
                (trackingState == other.trackingState) &&
                (nativePtr == other.nativePtr);
        }
    };
}
