using System;
using NUnit.Framework;

namespace NanoDNA.DockerManager.Tests
{
    internal class DockerTests
    {
        /// <summary>
        /// Default Hello World Image
        /// </summary>
        public const string DEFAULT_IMAGE = "hello-world";

        /// <summary>
        /// Default Ubuntu Image
        /// </summary>
        public const string DEFAULT_INTERACTIVE_IMAGE = "ubuntu:22.04";

        /// <summary>
        /// Default Bad Ubuntu Image that will cause an Error
        /// </summary>
        public const string BAD_DEFAULT_INTERACTIVE_IMAGE = "ubuntu:22.04__";

        #region StopTests

        /// <summary>
        /// Tests if we can Stop a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void StopContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            container.Start(true);

            Assert.IsTrue(Docker.ContainerRunning(name), "Container isn't running");

            Docker.StopContainer(name);
            Docker.RemoveContainer(name);

            Assert.IsFalse(Docker.ContainerRunning(name), "Container is still running");

            container.Start(true);

            Assert.IsTrue(Docker.ContainerRunning(name), "Container isn't running");

            Docker.RemoveContainer(name, true);
        }

        /// <summary>
        /// Tests if the Stop function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void StopContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            Assert.Throws<Exception>(() => Docker.StopContainer(name));

            container.Start(true);

            Assert.IsTrue(Docker.ContainerRunning(name), "Container isn't running");

            Docker.StopContainer(name);

            Assert.IsFalse(Docker.ContainerRunning(name), "Container is still running");

            Assert.Throws<Exception>(() => Docker.StopContainer(name));

            Docker.RemoveContainer(name, true);
        }

        #endregion

        #region KillTests

        /// <summary>
        /// Tests if we can Kill a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void KillContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            container.Start(true);

            Assert.IsTrue(Docker.ContainerRunning(name), "Container isn't running");

            Docker.KillContainer(name);

            Assert.IsFalse(Docker.ContainerRunning(name), "Container doesn't exist");

            Docker.RemoveContainer(name, true);
        }

        /// <summary>
        /// Tests if the Kill function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void KillContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            Assert.Throws<Exception>(() => Docker.KillContainer(name));

            container.Start(true);

            Assert.IsTrue(Docker.ContainerRunning(name), "Container isn't running");

            Docker.KillContainer(name);

            Assert.IsFalse(Docker.ContainerRunning(name), "Container doesn't exist");

            Docker.RemoveContainer(name);
        }

        #endregion

        #region RemoveTests

        /// <summary>
        /// Tests if we can Start a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void RemoveContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            container.Start(true);

            Assert.IsTrue(Docker.ContainerExists(name), "Container doesn't exist");

            Docker.RemoveContainer(name, true);

            Assert.IsFalse(Docker.ContainerExists(name), "Container still exists");

            container.Start(true);

            Assert.IsTrue(Docker.ContainerExists(name), "Container doesn't exist");

            Docker.StopContainer(name);
            Docker.RemoveContainer(name);

            Assert.IsFalse(Docker.ContainerExists(name), "Container still exists");
        }

        /// <summary>
        /// Tests if the Remove function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_INTERACTIVE_IMAGE)]
        public void RemoveContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image);

            Assert.Throws<Exception>(() => Docker.RemoveContainer(name));

            container.Start(true);

            Assert.IsTrue(Docker.ContainerExists(name), "Container doesn't exist");

            Assert.Throws<Exception>(() => Docker.RemoveContainer(name));

            Docker.StopContainer(name);
            Docker.RemoveContainer(name);
        }

        #endregion
    }
}
