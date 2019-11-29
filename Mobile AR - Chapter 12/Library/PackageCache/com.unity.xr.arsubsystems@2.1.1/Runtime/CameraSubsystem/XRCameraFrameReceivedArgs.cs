using System;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Encapsulates all information provided in an event callback for when the camera frame is received.
    /// </summary>
    public struct XRCameraFrameReceivedArgs : IEquatable<XRCameraFrameReceivedArgs>
    {
        /// <summary>
        /// The camera subsystem that is raising the event.
        /// </summary>
        /// <value>
        /// The camera subsystem that is raising the event.
        /// </value>
        public XRCameraSubsystem cameraSubsystem { get; private set; }

        /// <summary>
        /// The camera frame that raised this event.
        /// </summary>
        /// <value>
        /// The camera frame that raised this event.
        /// </value>
        public XRCameraFrame cameraFrame { get; private set; }

        /// <summary>
        /// Constructs a <c>XRCameraFrameReceivedArgs</c>.
        /// </summary>
        /// <param name="cameraSubsystem">The camera subsystem that is raising the event.</param>
        internal XRCameraFrameReceivedArgs(XRCameraSubsystem cameraSubsystem, XRCameraFrame cameraFrame)
        {
            this.cameraSubsystem = cameraSubsystem;
            this.cameraFrame = cameraFrame;
        }

        public bool Equals(XRCameraFrameReceivedArgs other)
        {
            return cameraSubsystem.Equals(other.cameraSubsystem) && cameraFrame.Equals(other.cameraFrame);
        }

        public override bool Equals(System.Object obj)
        {
            return ((obj is XRCameraFrameReceivedArgs) && Equals((XRCameraFrameReceivedArgs)obj));
        }

        public static bool operator ==(XRCameraFrameReceivedArgs lhs, XRCameraFrameReceivedArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(XRCameraFrameReceivedArgs lhs, XRCameraFrameReceivedArgs rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + cameraSubsystem.GetHashCode();
                hashCode = (hashCode * 486187739) + cameraFrame.GetHashCode();
            }
            return hashCode;
        }

        public override string ToString()
        {
            return string.Format("frame received {0}", cameraFrame.ToString());
        }
    }
}
