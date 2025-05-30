﻿using System;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NanoDNA.DockerManager.Tests
{
    internal class DockerContainerTests
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

        #region ConstructorTests

        /// <summary>
        /// Tests the Constructor of the Docker Container
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_IMAGE)]
        [TestCase("testing_", DEFAULT_IMAGE)]
        [TestCase("testing_hello", DEFAULT_IMAGE)]
        [TestCase("testing2", DEFAULT_INTERACTIVE_IMAGE)]
        public void ConstructorTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, false);

            Assert.That(container.Name, Is.EqualTo(name), "Container Name is Wrong");
            Assert.That(container.Image, Is.EqualTo(image), "Container Image is Wrong");
            Assert.That(container.EnvironmentVariables.Count, Is.EqualTo(0), "Container has Environment Variables");
        }

        /// <summary>
        /// Tests if the Constructor throws an Exception when the Container Name is Invalid
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("Testing", DEFAULT_IMAGE)]
        [TestCase("testing ", DEFAULT_IMAGE)]
        [TestCase(" testing", DEFAULT_IMAGE)]
        [TestCase("tes ting", DEFAULT_IMAGE)]
        public void InvalidContainerName(string name, string image)
        {
            Assert.Throws<ArgumentException>(() => new DockerContainer(name, image, false), "Invalid Container Name");
        }

        /// <summary>
        /// Tests the Constructor of the Docker Container with Environment Variables
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing", DEFAULT_IMAGE)]
        [TestCase("testing_", DEFAULT_IMAGE)]
        [TestCase("testing_hello", DEFAULT_IMAGE)]
        [TestCase("testing2", DEFAULT_INTERACTIVE_IMAGE)]
        public void ConstructorEnvironmentVariableTest(string name, string image)
        {
            Dictionary<string, string> environmentVariables = new Dictionary<string, string>
            {
                { "REPO", "https://github.com"},
                { "PATH", "/bin/bash"}
            };

            DockerContainer container = new DockerContainer(name, image, environmentVariables, false);

            Assert.That(container.Name, Is.EqualTo(name), "Container Name is Wrong");
            Assert.That(container.Image, Is.EqualTo(image), "Container Image is Wrong");
            Assert.That(container.EnvironmentVariables.Count, Is.EqualTo(2), "Container doesn't have 2 Environment Variables");
            Assert.That(container.EnvironmentVariables["REPO"], Is.EqualTo("https://github.com"), "REPO Environment Variable is Wrong");
            Assert.That(container.EnvironmentVariables["PATH"], Is.EqualTo("/bin/bash"), "PATH Environment Variable is Wrong");
        }

        /// <summary>
        /// Tests if the Constructor throws an Exception when the Container Name is Invalid
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("Testing", DEFAULT_IMAGE)]
        [TestCase("testing ", DEFAULT_IMAGE)]
        [TestCase(" testing", DEFAULT_IMAGE)]
        [TestCase("tes ting", DEFAULT_IMAGE)]
        public void InvalidContainerNameEnvironmentVariable(string name, string image)
        {
            Dictionary<string, string> environmentVariables = new Dictionary<string, string>
            {
                { "REPO", "https://github.com"},
                { "PATH", "/bin/bash"}
            };

            Assert.Throws<ArgumentException>(() => new DockerContainer(name, image, environmentVariables, false), "Invalid Container Name");
        }

        #endregion

        #region EnvironmentVariableTests

        /// <summary>
        /// Tests if we can add Environment Variables to the Container
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing1", DEFAULT_IMAGE)]
        public void AddEnvironmentVariableTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.AddEnvironmentVariable("REPO", "dummy");
            container.AddEnvironmentVariable("TOKEN", "dummy");

            Assert.That(container.EnvironmentVariables.Count, Is.EqualTo(2), "Container doesn't have 2 Environment Variables");
            Assert.That(container.EnvironmentVariables["REPO"], Is.EqualTo("dummy"), "REPO Environment Variable is Wrong");
            Assert.That(container.EnvironmentVariables["TOKEN"], Is.EqualTo("dummy"), "TOKEN Environment Variable is Wrong");

            Assert.Throws<ArgumentException>(() => container.AddEnvironmentVariable("REPO", "dummy"));

            container.Start();

            Assert.Throws<InvalidOperationException>(() => container.AddEnvironmentVariable("IDK", "dummy"));

            container.Remove(true);
        }

        /// <summary>
        /// Tests if we can set Environment Variables to the Container
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing2", DEFAULT_IMAGE)]
        public void SetEnvironmentVariableTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.AddEnvironmentVariable("REPO", "");
            container.AddEnvironmentVariable("TOKEN", "");

            container.SetEnvironmentVariable("REPO", "dummy");
            container.SetEnvironmentVariable("TOKEN", "dummy");

            Assert.That(container.EnvironmentVariables.Count, Is.EqualTo(2), "Container doesn't have 2 Environment Variables");
            Assert.That(container.EnvironmentVariables["REPO"], Is.EqualTo("dummy"), "REPO Environment Variable is Wrong");
            Assert.That(container.EnvironmentVariables["TOKEN"], Is.EqualTo("dummy"), "TOKEN Environment Variable is Wrong");

            Assert.Throws<ArgumentException>(() => container.SetEnvironmentVariable("IDK", "value"));

            container.Start();

            Assert.Throws<InvalidOperationException>(() => container.SetEnvironmentVariable("REPO", "dummy"));

            container.Remove(true);
        }

        #endregion

        #region StartTests

        /// <summary>
        /// Tests if we can Start a Container 
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing3", DEFAULT_INTERACTIVE_IMAGE)]
        public void StartContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");
            Assert.IsTrue(container.Running(), "Container isn't running");
            Assert.IsTrue(container.Ready(), "Container isn't ready");

            container.Remove(true);
        }

        /// <summary>
        /// Tests if the Start function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing4", DEFAULT_INTERACTIVE_IMAGE)]
        public void StartContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");
            Assert.IsTrue(container.Running(), "Container isn't running");
            Assert.IsTrue(container.Ready(), "Container isn't ready");

            Assert.Throws<Exception>(() => container.Start());

            container.Remove(true);
        }

        /// <summary>
        /// Tests if the Start function throws Errors when we provide a Bad Image Name
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing5", BAD_DEFAULT_INTERACTIVE_IMAGE)]
        public void StartContainerFailTest2(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Start());
        }

        /// <summary>
        /// Tests if we can Start a Container Asynchronously
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing6", DEFAULT_INTERACTIVE_IMAGE)]
        public void StartAsyncTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.StartAsync(true);
            });
            Assert.IsFalse(container.Exists(), "Container exists");
            container.WaitUntilExists();
            Assert.IsTrue(container.Exists(), "Container doesn't exist");
            container.Remove(true);

            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.StartAsync(true);
            });
            Assert.IsFalse(container.Running(), "Container exists");
            container.WaitUntilRunning();
            Assert.IsTrue(container.Running(), "Container doesn't exist");
            container.Remove(true);

            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.StartAsync(true);
            });
            Assert.IsFalse(container.Ready(), "Container exists");
            container.WaitUntilReady();
            Assert.IsTrue(container.Ready(), "Container doesn't exist");
            container.Remove(true);
        }

        #endregion

        #region RunTests

        /// <summary>
        /// Tests if Run a Single Command in the Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing7", DEFAULT_INTERACTIVE_IMAGE)]
        public void RunContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Run("bash -c \"echo 'Hello from exec' | tee /proc/1/fd/1\"");

            Assert.IsFalse(container.Ready(), "Container is ready");
            Assert.IsFalse(container.Running(), "Container is running");
            Assert.IsFalse(container.Exists(), "Container exists");
        }

        /// <summary>
        /// Tests if the Run function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing8", DEFAULT_INTERACTIVE_IMAGE)]
        public void RunContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Run("echoe 'Hello from exec' >> /proc/1/fd/1"));

            container.Start(true);

            Assert.Throws<Exception>(() => container.Run("echoe 'Hello from exec' >> /proc/1/fd/1"));

            container.Remove(true);
        }

        /// <summary>
        /// Tests if we can Run a Command Asynchronously in the Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing9", DEFAULT_INTERACTIVE_IMAGE)]
        public void RunAsyncContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.RunAsync("sleep 3");

            container.WaitUntilReady();

            Assert.IsTrue(container.Ready(), "Container isn't ready");
            Assert.IsTrue(container.Running(), "Container is running");
            Assert.IsTrue(container.Exists(), "Container doesn't exist");

            container.WaitUntilUnready();

            Assert.IsFalse(container.Ready(), "Container is ready");
            Assert.IsFalse(container.Running(), "Container is running");
            Assert.IsFalse(container.Exists(), "Container exists");
        }

        #endregion

        #region StopTests

        /// <summary>
        /// Tests if we can Stop a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing10", DEFAULT_INTERACTIVE_IMAGE)]
        public void StopContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Assert.IsTrue(container.Running(), "Container isn't running");

            container.Stop();
            container.Remove();

            Assert.IsFalse(container.Running(), "Container is still running");

            container.Start(true);

            Assert.IsTrue(container.Running(), "Container isn't running");

            container.Remove(true);
        }

        /// <summary>
        /// Tests if the Stop function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing11", DEFAULT_INTERACTIVE_IMAGE)]
        public void StopContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Stop());

            container.Start(true);

            Assert.IsTrue(container.Running(), "Container isn't running");

            container.Stop();

            Assert.IsFalse(container.Running(), "Container is still running");

            Assert.Throws<Exception>(() => container.Stop());

            container.Remove(true);
        }

        /// <summary>
        /// Tests if we can Stop a Container Asynchronously
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing12", DEFAULT_INTERACTIVE_IMAGE)]
        public void StopAsyncTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.StopAsync();
            });
            Assert.IsTrue(container.Running(), "Container isn't running");
            container.WaitUntilStopped();
            Assert.IsFalse(container.Running(), "Container is running");
            container.Remove(true);

            container.Start(true);
            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.StopAsync();
            });
            Assert.IsTrue(container.Ready(), "Container isn't running");
            container.WaitUntilStopped();
            Assert.IsFalse(container.Ready(), "Container is running");
            container.Remove(true);
        }

        #endregion

        #region RemoveTests

        /// <summary>
        /// Tests if we can Start a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing13", DEFAULT_INTERACTIVE_IMAGE)]
        public void RemoveContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");

            container.Remove(true);

            Assert.IsFalse(container.Exists(), "Container still exists");

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");

            container.Stop();
            container.Remove();

            Assert.IsFalse(container.Exists(), "Container still exists");
        }

        /// <summary>
        /// Tests if the Remove function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing14", DEFAULT_INTERACTIVE_IMAGE)]
        public void RemoveContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Remove());

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");

            Assert.Throws<Exception>(() => container.Remove());

            container.Stop();
            container.Remove();
        }

        /// <summary>
        /// Tests if we can Remove a Container Asynchronously
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing15", DEFAULT_INTERACTIVE_IMAGE)]
        public void RemoveAsyncTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.RemoveAsync(true);
            });
            Assert.IsTrue(container.Exists(), "Container doesn't exist");
            container.WaitUntilRemoved();
            Assert.IsFalse(container.Exists(), "Container still exists");

            container.Start(true);
            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.RemoveAsync(true);
            });
            Assert.IsTrue(container.Running(), "Container isn't running");
            container.WaitUntilRemoved();
            Assert.IsFalse(container.Running(), "Container is still running");

            container.Start(true);
            Task.Run(() =>
            {
                Thread.Sleep(500);
                container.RemoveAsync(true);
            });
            Assert.IsTrue(container.Ready(), "Container isn't ready");
            container.WaitUntilRemoved();
            Assert.IsFalse(container.Ready(), "Container is still ready");
        }

        #endregion

        #region KillTests

        /// <summary>
        /// Tests if we can Kill a Container and Remove it afterwards
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing16", DEFAULT_INTERACTIVE_IMAGE)]
        public void KillContainerTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            Assert.IsTrue(container.Running(), "Container isn't running");

            container.Kill();

            Assert.IsFalse(container.Running(), "Container doesn't exist");

            container.Remove(true);
        }

        /// <summary>
        /// Tests if the Kill function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing17", DEFAULT_INTERACTIVE_IMAGE)]
        public void KillContainerFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Kill());

            container.Start(true);

            Assert.IsTrue(container.Running(), "Container isn't running");

            container.Kill();

            Assert.IsFalse(container.Running(), "Container doesn't exist");

            container.Remove(true);
        }

        #endregion

        #region ExecuteTests

        /// <summary>
        /// Tests if we can Execute a Command in the Container
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing18", DEFAULT_INTERACTIVE_IMAGE)]
        public void ExecuteCommandTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            container.Execute("bash -c \"echo 'Hello from exec' | tee /proc/1/fd/1\"");
            container.Remove(true);
            Assert.Pass("Command was Executed");
        }

        /// <summary>
        /// Tests if the Execute function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing19", DEFAULT_INTERACTIVE_IMAGE)]
        public void ExecuteCommandFail(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.Throws<Exception>(() => container.Execute("echo hello"));

            container.Start(true);

            Assert.Throws<Exception>(() => container.Execute("nonexistentcommand"));

            container.Remove(true);
        }

        #endregion

        #region LogsTests

        /// <summary>
        /// Tests if we can Get the Logs of the Container
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing20", DEFAULT_IMAGE)]
        public void GetLogsTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            container.Start(true);

            container.WaitUntilReady();

            Assert.That(container.GetLogs().Contains("Hello from Docker!"), "Command didn't execute correctly");

            container.Remove(true);
        }

        /// <summary>
        /// Tests if the GetLogs function throws Errors when necessary
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing21", DEFAULT_INTERACTIVE_IMAGE)]
        public void GetLogsFailTest(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, false);

            Assert.Throws<Exception>(() => container.GetLogs());
        }

        #endregion

        /// <summary>
        /// Tests if the State Checking Functions work correctly
        /// </summary>
        /// <param name="name">Name of the Container</param>
        /// <param name="image">Name of the Image</param>
        [Test]
        [TestCase("testing22", DEFAULT_INTERACTIVE_IMAGE)]
        public void StateTests(string name, string image)
        {
            DockerContainer container = new DockerContainer(name, image, true);

            Assert.IsFalse(container.Exists(), "Container exists");
            Assert.IsFalse(container.Running(), "Container is running");
            Assert.IsFalse(container.Ready(), "Container is ready");

            container.Start(true);

            Assert.IsTrue(container.Exists(), "Container doesn't exist");
            Assert.IsTrue(container.Running(), "Container isn't running");
            Assert.IsTrue(container.Ready(), "Container isn't ready");

            container.Remove(true);
        }
    }
}