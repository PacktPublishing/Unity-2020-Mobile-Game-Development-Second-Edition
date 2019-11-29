using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRSessionSubsystemImpl : XRSessionSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new IProvider();
        }
    }

    [TestFixture]
    public class XRSessionSubsystemTestFixture
    {
         [Test]
        public void RunningStateTests()
        {
            XRSessionSubsystem subsystem = new XRSessionSubsystemImpl();
            
            // Initial state is not running
            Assert.That(subsystem.running == false);

            // After start subsystem is running
            subsystem.Start();
            Assert.That(subsystem.running == true);

            // After start subsystem is running
            subsystem.Stop();
            Assert.That(subsystem.running == false);
        }
    }
}