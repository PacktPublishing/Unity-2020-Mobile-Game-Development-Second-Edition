using System;
using Unity.Collections;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// A subsystem for detecting and tracking a preconfigured set of images in the environment.
    /// </summary>
    /// <typeparam name="XRTrackedImage">Low level data describing a tracked image.</typeparam>
    /// <typeparam name="XRImageTrackingSubsystemDescriptor">The descriptor used by implementations to describe the feature set of the image tracking subsystem.</typeparam>
    public abstract class XRImageTrackingSubsystem
        : TrackingSubsystem<XRTrackedImage, XRImageTrackingSubsystemDescriptor>
    {
        /// <summary>
        /// Constructs a subsystem. Do not invoked directly; call <c>Create</c> on the <see cref="XRImageTrackingSubsystemDescriptor"/> instead.
        /// </summary>
        public XRImageTrackingSubsystem()
        {
            m_Provider = CreateProvider();
        }

        /// <summary>
        /// Starts the subsystem, that is, start detecting images in the scene. <see cref="imageLibrary"/> must not be null.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="imageLibrary"/> is null.</exception>
        public override void Start()
        {
            if (m_Running)
                return;

            if (m_ImageLibrary == null)
                throw new InvalidOperationException("Cannot start image tracking without an image library.");

            m_Provider.imageLibrary = m_ImageLibrary;
            m_Running = true;
        }

        /// <summary>
        /// Stops the subsystem, that is, stops detecting and tracking images.
        /// </summary>
        public override void Stop()
        {
            if (!m_Running)
                return;

            m_Provider.imageLibrary = null;
            m_Running = false;
        }

        /// <summary>
        /// Destroys the subsystem.
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
        /// Get or set the reference image library. This is the set of images to look for in the scene.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"/>Thrown if the subsystem has been started, and you attempt to set the image library to null.</exception>
        /// <seealso cref="Start"/>
        /// <seealso cref="Stop"/>
        /// <seealso cref="XRReferenceImageLibrary"/>
        public XRReferenceImageLibrary imageLibrary
        {
            get
            {
                return m_ImageLibrary;
            }
            set
            {
                if (m_ImageLibrary == value)
                    return;

                if (m_Running && value == null)
                    throw new ArgumentNullException("Cannot set imageLibrary to null while subsystem is running.");

                m_ImageLibrary = value;

                // If we are running, then we want to switch the current library
                if (m_Running)
                    m_Provider.imageLibrary = m_ImageLibrary;
            }
        }

        /// <summary>
        /// Retrieve the changes in the state of tracked images (added, updated, removed) since the last call to <c>GetChanges</c>.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned set of changes.</param>
        /// <returns>The set of tracked image changes (added, updated, removed) since the last call to this method.</returns>
        public override TrackableChanges<XRTrackedImage> GetChanges(Allocator allocator)
        {
            var changes = m_Provider.GetChanges(XRTrackedImage.GetDefault(), allocator);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            m_ValidationUtility.ValidateAndDisposeIfThrown(changes);
#endif
            return changes;
        }

        /// <summary>
        /// The maximum number of moving images to track.
        /// </summary>
        /// <exception cref="System.NotSupportedException"/>
        /// Thrown if the subsystem does not support tracking moving images.
        /// Check for support of this feature with <see cref="XRImageTrackingSubsystemDescriptor.supportsMovingImages"/>.
        /// </exception>
        public int maxNumberOfMovingImages
        {
            set
            {
                m_Provider.maxNumberOfMovingImages = value;
            }
        }

        /// <summary>
        /// Methods to implement by the implementing provider.
        /// </summary>
        protected class IProvider
        {
            /// <summary>
            /// Called when the subsystem is destroyed.
            /// </summary>
            public virtual void Destroy() {}

            /// <summary>
            /// Get the changes (added, updated, removed) to the tracked images since the last call to this method.
            /// </summary>
            /// <param name="defaultTrackedImage">An <see cref="XRTrackedImage"/> populated with default values.
            /// The implementation should first fill arrays of added, updated, and removed with copies of this
            /// before copying in its own values. This guards against addtional fields added to the <see cref="XRTrackedImage"/> in the future.</param>
            /// <param name="allocator">The allocator to use for the returned data.</param>
            /// <returns>The set of changes (added, updated, removed) tracked images since the last call to this method.</returns>
            public virtual TrackableChanges<XRTrackedImage> GetChanges(
                XRTrackedImage defaultTrackedImage,
                Allocator allocator)
            {
                return default(TrackableChanges<XRTrackedImage>);
            }

            /// <summary>
            /// Set the <see cref="XRReferenceImageLibrary"/>. Setting this to <c>null</c> implies the subsystem should stop detecting or tracking images.
            /// </summary>
            public virtual XRReferenceImageLibrary imageLibrary
            {
                set
                {
                    throw new NotSupportedException("This image tracking provider does not support image libraries.");
                }
            }

            /// <summary>
            /// The maximum number of moving images to track in realtime.
            /// </summary>
            public virtual int maxNumberOfMovingImages
            {
                set
                {
                    throw new NotSupportedException("This subsystem does not track moving images.");
                }
            }
        }

        /// <summary>
        /// Create an implementation of the <see cref="IProvider"/> class. This will only be called once.
        /// </summary>
        /// <returns>An instance of the <see cref="IProvider"/> interface.</returns>
        protected abstract IProvider CreateProvider();

        XRReferenceImageLibrary m_ImageLibrary;

        IProvider m_Provider;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ValidationUtility<XRTrackedImage> m_ValidationUtility =
            new ValidationUtility<XRTrackedImage>();
#endif
    }
}
