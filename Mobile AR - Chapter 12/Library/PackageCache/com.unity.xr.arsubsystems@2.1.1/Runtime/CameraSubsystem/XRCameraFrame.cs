using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the properties included in the camera frame.
    /// </summary>
    [Flags]
    public enum XRCameraFrameProperties
    {
        /// <summary>
        /// The timestamp of the frame is included.
        /// </summary>
        [Description("Timestamp")]
        Timestamp               = (1 << 0),

        /// <summary>
        /// The average brightness of the frame is included.
        /// </summary>
        [Description("AverageBrightness")]
        AverageBrightness       = (1 << 1),

        /// <summary>
        /// The average color temperature of the frame is included.
        /// </summary>
        [Description("AverageColorTemperature")]
        AverageColorTemperature = (1 << 2),

        /// <summary>
        /// The color correction value of the frame is included.
        /// </summary>
        [Description("ColorCorrection")]
        ColorCorrection = (1 << 3),

        /// <summary>
        /// The project matrix for the frame is included.
        /// </summary>
        [Description("ProjectionMatrix")]
        ProjectionMatrix        = (1 << 4),

        /// <summary>
        /// The display matrix for the frame is included.
        /// </summary>
        [Description("DisplayMatrix")]
        DisplayMatrix           = (1 << 5),
    }

    /// <summary>
    /// Parameters of the Unity <c>Camera</c> that may be necessary/useful to the provider.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRCameraFrame : IEquatable<XRCameraFrame>
    {
        /// <summary>
        /// The timestamp, in nanoseconds, associated with this frame.
        /// </summary>
        /// <value>
        /// The timestamp, in nanoseconds, associated with this frame.
        /// </value>
        public long timestampNs
        {
            get { return m_TimestampNs; }
        }
        long m_TimestampNs;

        /// <summary>
        /// The estimated brightness of the scene.
        /// </summary>
        /// <value>
        /// The estimated brightness of the scene.
        /// </value>
        public float averageBrightness
        {
            get { return m_AverageBrightness; }
        }
        float m_AverageBrightness;

        /// <summary>
        /// The estimated color temperature of the scene.
        /// </summary>
        /// <value>
        /// The estimated color temperature of the scene.
        /// </value>
        public float averageColorTemperature
        {
            get { return m_AverageColorTemperature; }
        }
        float m_AverageColorTemperature;

        /// <summary>
        /// The estimated color correction value of the scene.
        /// </summary>
        /// <value>
        /// The estimated color correction value of the scene.
        /// </value>
        public Color colorCorrection
        {
            get { return m_ColorCorrection; }
        }
        Color m_ColorCorrection;

        /// <summary>
        /// The 4x4 projection matrix for the camera frame.
        /// </summary>
        /// <value>
        /// The 4x4 projection matrix for the camera frame.
        /// </value>
        public Matrix4x4 projectionMatrix
        {
            get { return m_ProjectionMatrix; }
        }
        Matrix4x4 m_ProjectionMatrix;

        /// <summary>
        /// The 4x4 display matrix for the camera frame.
        /// </summary>
        /// <value>
        /// The 4x4 display matrix for the camera frame.
        /// </value>
        public Matrix4x4 displayMatrix
        {
            get { return m_DisplayMatrix; }
        }
        Matrix4x4 m_DisplayMatrix;

        /// <summary>
        /// The <see cref="TrackingState"/> associated with the camera.
        /// </summary>
        /// <value>
        /// The tracking state associated with the camera.
        /// </value>
        public TrackingState trackingState
        {
            get { return m_TrackingState; }
        }
        TrackingState m_TrackingState;

        /// <summary>
        /// A native pointer associated with this frame. The data
        /// pointed to by this pointer is specific to provider implementation.
        /// </summary>
        /// <value>
        /// The native pointer associated with this frame.
        /// </value>
        public IntPtr nativePtr
        {
            get { return m_NativePtr; }
        }
        IntPtr m_NativePtr;

        /// <summary>
        /// The set of all flags indicating which properties are included in the frame.
        /// </summary>
        /// <value>
        /// The set of all flags indicating which properties are included in the frame.
        /// </value>
        public XRCameraFrameProperties properties
        {
            get { return m_Properties; }
        }
        XRCameraFrameProperties m_Properties;

        /// <summary>
        /// Whether the frame has a timestamp.
        /// </summary>
        /// <value>
        /// Whether the frame has a timestamp.
        /// </value>
        public bool hasTimestamp
        {
            get { return (m_Properties & XRCameraFrameProperties.Timestamp) != 0; }
        }

        /// <summary>
        /// Whether the frame has an average brightness.
        /// </summary>
        /// <value>
        /// Whether the frame has an average brightness.
        /// </value>
        public bool hasAverageBrightness
        {
            get { return (m_Properties & XRCameraFrameProperties.AverageBrightness) != 0; }
        }

        /// <summary>
        /// Whether the frame has an average color temperature.
        /// </summary>
        /// <value>
        /// Whether the frame has an average color temperature.
        /// </value>
        public bool hasAverageColorTemperature
        {
            get { return (m_Properties & XRCameraFrameProperties.AverageColorTemperature) != 0; }
        }

        /// <summary>
        /// Whether the frame has a color correction value.
        /// </summary>
        /// <value>
        /// Whether the frame has a color correction value.
        /// </value>
        public bool hasColorCorrection
        {
            get { return (m_Properties & XRCameraFrameProperties.ColorCorrection) != 0; }
        }

        /// <summary>
        /// Whether the frame has a projection matrix.
        /// </summary>
        /// <value>
        /// Whether the frame has a projection matrix.
        /// </value>
        public bool hasProjectionMatrix
        {
            get { return (m_Properties & XRCameraFrameProperties.ProjectionMatrix) != 0; }
        }

        /// <summary>
        /// Whether the frame has a display matrix.
        /// </summary>
        /// <value>
        /// Whether the frame has a display matrix.
        /// </value>
        public bool hasDisplayMatrix
        {
            get { return (m_Properties & XRCameraFrameProperties.DisplayMatrix) != 0; }
        }

        /// <summary>
        /// Provides timestamp of the camera frame.
        /// </summary>
        /// <param name="timestampNs">The timestamp of the camera frame.</param>
        /// <returns>
        /// <c>true</c> if the timestamp was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetTimestamp(out long timestampNs)
        {
            timestampNs = this.timestampNs;
            return hasTimestamp;
        }

        /// <summary>
        /// Provides brightness for the whole image as an average of all pixels' brightness.
        /// </summary>
        /// <param name="averageBrightness">An estimated average brightness for the environment.</param>
        /// <returns>
        /// <c>true</c> if average brightness was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAverageBrightness(out float averageBrightness)
        {
            averageBrightness = this.averageBrightness;
            return hasAverageBrightness;
        }

        /// <summary>
        /// Provides color temperature for the whole image as an average of all pixels' color temperature.
        /// </summary>
        /// <param name="averageColorTemperature">An estimated color temperature.</param>
        /// <returns>
        /// <c>true</c> if average color temperature was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetAverageColorTemperature(out float averageColorTemperature)
        {
            averageColorTemperature = this.averageColorTemperature;
            return hasAverageColorTemperature;
        }

        /// <summary>
        /// Provides projection matrix for the camera frame.
        /// </summary>
        /// <param name="projectionMatrix">The projection matrix used by the <c>XRCameraSubsystem</c>.</param>
        /// <returns>
        /// <c>true</c> if the projection matrix was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetProjectionMatrix(out Matrix4x4 projectionMatrix)
        {
            projectionMatrix = this.projectionMatrix;
            return this.hasProjectionMatrix;
        }

        /// <summary>
        /// Provides display matrix defining how texture is being rendered on the screen.
        /// </summary>
        /// <param name="displayMatrix">The display matrix for rendering.</param>
        /// <returns>
        /// <c>true</c> if the display matrix was provided. Otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetDisplayMatrix(out Matrix4x4 displayMatrix)
        {
            displayMatrix = this.displayMatrix;
            return hasDisplayMatrix;
        }

        public bool Equals(XRCameraFrame other)
        {
            return (m_TimestampNs.Equals(other.m_TimestampNs) && m_AverageBrightness.Equals(other.m_AverageBrightness)
                    && m_AverageColorTemperature.Equals(other.m_AverageColorTemperature)
                    && m_ProjectionMatrix.Equals(other.m_ProjectionMatrix)
                    && m_DisplayMatrix.Equals(other.m_DisplayMatrix)
                    && m_Properties.Equals(other.m_Properties));
        }

        public override bool Equals(System.Object obj)
        {
            return ((obj is XRCameraFrame) && Equals((XRCameraFrame)obj));
        }

        public static bool operator ==(XRCameraFrame lhs, XRCameraFrame rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(XRCameraFrame lhs, XRCameraFrame rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + m_TimestampNs.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AverageBrightness.GetHashCode();
                hashCode = (hashCode * 486187739) + m_AverageColorTemperature.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ColorCorrection.GetHashCode();
                hashCode = (hashCode * 486187739) + m_ProjectionMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + m_DisplayMatrix.GetHashCode();
                hashCode = (hashCode * 486187739) + m_NativePtr.GetHashCode();
                hashCode = (hashCode * 486187739) + m_Properties.GetHashCode();
            }
            return hashCode;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("properties:{0}\n   timestamp:{1}ns\n   avgBrightness:{2}\n"
                                       + "   avgColorTemp:{3}\n   colorCorrection:{4}\n   projection:\n{5}\n   display:\n{6}\n"
                                       + "   nativePtr{7}\n",
                                       m_Properties.ToString(), m_TimestampNs.ToString(),
                                       m_AverageBrightness.ToString("0.000"),
                                       m_AverageColorTemperature.ToString("0.000"),
                                       m_ColorCorrection.ToString(),
                                       m_ProjectionMatrix.ToString("0.000"), m_DisplayMatrix.ToString("0.000"),
                                       m_NativePtr.ToString("X16"));

            return stringBuilder.ToString();
        }
    }
}
