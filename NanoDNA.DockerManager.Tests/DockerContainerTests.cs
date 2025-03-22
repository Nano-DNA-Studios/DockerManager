using NUnit.Framework;
using NanoDNA.ProcessRunner;
using System;

namespace NanoDNA.DockerManager.Tests
{
    internal class DockerContainerTests
    {

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void StartDockerContainer()
        {
            DockerContainer container = new DockerContainer("testing", "ubuntu:22.04");

            container.AddEnvironmentVariable("REPO", "https:/");
            container.AddEnvironmentVariable("TOKEN", "git_");

            container.Start();

            container.WaitForReady();

            Assert.IsTrue(container.Exists());
            Assert.IsTrue(container.Running());
            Assert.IsTrue(container.Ready());
        }

        [Test]
        public void Testing ()
        {
            CommandRunner runner = new CommandRunner();

            runner.RunCommand($"docker inspect testing");

            Console.WriteLine($"STDOut : {string.Join("\n", runner.StandardOutput)}");
            Console.WriteLine($"STDErr : {string.Join("\n", runner.StandardError)}");
        }

        [Test]
        public void ContainerExists ()
        {
            DockerContainer container = new DockerContainer("testing", "hello-world");

            Assert.IsFalse(Docker.ContainerExists("testing"), "Docker Container Doesn't Exist yet");

            container.Start();

            Assert.IsTrue(Docker.ContainerExists("testing"), "Docker Container does exist");

        }

        [Test]
        public void DockerServiceRunning()
        {
            Assert.IsTrue(Docker.Running(), "Docker Service is Running");
        }



    }
}
