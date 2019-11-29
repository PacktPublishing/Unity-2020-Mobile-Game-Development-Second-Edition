using System;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Describes the capabilities of an <see cref="XRImageTrackingSubsystem"/>.
    /// </summary>
    public class XRImageTrackingSubsystemDescriptor : SubsystemDescriptor<XRImageTrackingSubsystem>
    {
        /// <summary>
        /// Construction information for the <see cref="XRImageTrackingSubsystemDescriptor"/>.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// A string identifier used to name the subsystem provider.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// The <c>System.Type</c> of the provider implementation, used to instantiate the class.
            /// </summary>
            public Type subsystemImplementationType { get; set; }

            /// <summary>
            /// Whether the subsystem supports tracking the poses of moving images in realtime.
            /// </summary>
            public bool supportsMovingImages { get; set; }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = id == null ? 0 : id.GetHashCode();
                    hashCode = hashCode * 486187739 + (subsystemImplementationType == null ? 0 : subsystemImplementationType.GetHashCode());
                    return hashCode;
                }
            }

            public bool Equals(Cinfo other)
            {
                return
                    (id == other.id) &&
                    (subsystemImplementationType == subsystemImplementationType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return Equals((Cinfo)obj);
            }

            public static bool operator==(Cinfo lhs, Cinfo rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator!=(Cinfo lhs, Cinfo rhs)
            {
                return !lhs.Equals(rhs);
            }
        }

        /// <summary>
        /// Whether the subsystem supports tracking the poses of moving images in realtime.
        /// </summary>
        public bool supportsMovingImages { get; private set; }

        /// <summary>
        /// Registers a new descriptor with the <c>SubsystemManager</c>.
        /// </summary>
        /// <param name="cinfo">The construction information for the new descriptor.</param>
        public static void Create(Cinfo cinfo)
        {
            SubsystemRegistration.CreateDescriptor(new XRImageTrackingSubsystemDescriptor(cinfo));
        }

        XRImageTrackingSubsystemDescriptor(Cinfo cinfo)
        {
            this.id = cinfo.id;
            this.subsystemImplementationType = cinfo.subsystemImplementationType;
            this.supportsMovingImages = cinfo.supportsMovingImages;
        }
    }
}
