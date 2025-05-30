﻿using System;
using System.Threading;
using NanoDNA.ProcessRunner;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace NanoDNA.DockerManager
{
    /// <summary>
    /// Controls a Docker Container and it's Operations through C#
    /// </summary>
    public class DockerContainer
    {
        /// <summary>
        /// Name of the Docker Container
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// ID of the Docker Container
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// Image of the Docker Container, includes the Image Tag
        /// </summary>
        public string Image { get; private set; }

        /// <summary>
        /// Environment Variables for the Docker Container
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; private set; }

        /// <summary>
        /// Toggle for the Container to Ignore Errors during Operations
        /// </summary>
        public bool IgnoreContainerErrors { get; private set; }

        /// <summary>
        /// Toggle for Allowing Docker In Docker
        /// </summary>
        public bool DockerInDocker { get; private set; }

        /// <summary>
        /// Initializes a new Instance of a <see cref="DockerContainer"/>.
        /// </summary>
        /// <param name="name">Name of the Container once Started</param>
        /// <param name="image">Name of the Docker Image the Container will use</param>
        /// <param name="dockerInDocker">Toggle for Allowing Docker In Docker</param>
        /// <param name="ignoreContainerErrors">Toggle for Ignoring Container Operation Errors</param>
        /// <exception cref="ArgumentException">Thrown if the Name if Invalid</exception>
        public DockerContainer(string name, string image, bool dockerInDocker, bool ignoreContainerErrors = false)
        {
            Name = name;
            Image = image;
            IgnoreContainerErrors = ignoreContainerErrors;
            EnvironmentVariables = new Dictionary<string, string>();
            DockerInDocker = dockerInDocker;

            if (Name != Name.ToLower())
                throw new ArgumentException("Name must be lowercase");

            if (Name != Name.Trim() || Name != Name.Replace(" ", ""))
                throw new ArgumentException("Name must not contain spaces");
        }

        /// <summary>
        /// Initializes a new Instance of a <see cref="DockerContainer"/> with predefined Environment Variables.
        /// </summary>
        /// <param name="name">Name of the Container once Started</param>
        /// <param name="image">Name of the Docker Image the Container will use</param>
        /// <param name="environmentVariables">Dictionary of predefined Environment Variables</param>
        /// <param name="dockerInDocker">Toggle for Allowing Docker In Docker</param>
        /// <param name="ignoreContainerErrors">Toggle for Ignoring Container Operation Errors</param>
        /// <exception cref="ArgumentException">Thrown if the Name if Invalid</exception>
        public DockerContainer(string name, string image, Dictionary<string, string> environmentVariables, bool dockerInDocker, bool ignoreContainerErrors = false)
        {
            Name = name;
            Image = image;
            IgnoreContainerErrors = ignoreContainerErrors;
            EnvironmentVariables = environmentVariables;
            DockerInDocker = dockerInDocker;

            if (Name != Name.ToLower())
                throw new ArgumentException("Name must be lowercase");

            if (Name != Name.Trim() || Name != Name.Replace(" ", ""))
                throw new ArgumentException("Name must not contain spaces");
        }

        /// <summary>
        /// Adds Environment Variables to the Docker Container before Startup
        /// </summary>
        /// <param name="name">Name of the Environment Variable</param>
        /// <param name="value">Value to Set the Environment Variable</param>
        /// <exception cref="InvalidOperationException">Thrown if the Container already Exists or is Running</exception>
        /// <exception cref="ArgumentException">Thrown if the Environment Variable already exists</exception>
        public void AddEnvironmentVariable(string name, string value)
        {
            if (Exists() || Running())
                throw new InvalidOperationException("Cannot Add Environment Variables to a Running Container");

            if (!EnvironmentVariables.ContainsKey(name))
                EnvironmentVariables.Add(name, value);
            else
                throw new ArgumentException("Environment Variable Already Exists");
        }

        /// <summary>
        /// Sets the Value of an Environment Variable in the Docker Container before Startup
        /// </summary>
        /// <param name="name">Name of the Environment Variable</param>
        /// <param name="value">Value to Set the Environment Variable</param>
        /// <exception cref="InvalidOperationException">Thrown if the Container already Exists or is Running</exception>
        /// <exception cref="ArgumentException">Thrown if the Environment Variable doesn't exist</exception>
        public void SetEnvironmentVariable(string name, string value)
        {
            if (Exists() || Running())
                throw new InvalidOperationException("Cannot Add Environment Variables to a Running Container");

            if (!EnvironmentVariables.ContainsKey(name))
                throw new ArgumentException("Environment Variable Doesn't Exist");

            EnvironmentVariables[name] = value;
        }

        /// <summary>
        /// Executes a Command in a Running Docker Container
        /// </summary>
        /// <param name="command">Command to run in the Running Container</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exists or if an Error Occured while getting the logs</exception>
        public void Execute(string command)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Running())
                throw new Exception("Container Doesn't Exist, cannot Execute a Command in a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            if (!runner.TryRunCommand($"docker exec {Name} {command}"))
                throw new Exception($"Error Executing Command : ({command}) -> {string.Join("\n", runner.StandardError)}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Executing Command : ({command}) -> {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Gets the Logs of the Docker Container as a String.
        /// </summary>
        /// <returns>All Container logs as a single string</returns>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exists or if an Error Occured while getting the logs</exception>
        public string GetLogs()
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Exists())
                throw new Exception("Container Doesn't Exist, cannot get Logs of a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            runner.TryRunCommand($"docker logs {Name}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Getting Docker Logs : {string.Join("\n", runner.StandardError)}");

            return string.Join("\n", runner.StandardOutput);
        }

        /// <summary>
        /// Removes the Docker Container from the Device
        /// </summary>
        /// <param name="force">Force the Removal of the Container</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public void Remove(bool force = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Exists())
                throw new Exception("Container Doesn't Exist, cannot Remove a Non Existent Container");

            if (Running() && !force)
                throw new Exception("Cannot Remove a Running Container, set Force to True to Remove the Container");

            CommandRunner runner = new CommandRunner();

            string forceArg = force ? "-f" : "";

            runner.TryRunCommand($"docker rm {forceArg} {Name}");

            if (runner.StandardError.Length != 0)
                throw new Exception($"Error Removing Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Removes the Docker Container from the Device Asynchronously
        /// </summary>
        /// <param name="force">Force the Removal of the Container</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public async Task RemoveAsync(bool force = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Exists())
                throw new Exception("Container Doesn't Exist, cannot Remove a Non Existent Container");

            if (Running() && !force)
                throw new Exception("Cannot Remove a Running Container, set Force to True to Remove the Container");

            CommandRunner runner = new CommandRunner();

            string forceArg = force ? "-f" : "";

            await runner.TryRunCommandAsync($"docker rm {forceArg} {Name}");

            if (runner.StandardError.Length != 0)
                throw new Exception($"Error Removing Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Stops the Docker Container
        /// </summary>
        /// <param name="time">Time for the Container to Stop, default is ~10 seconds</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public void Stop(int time = 0)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Exists() || !Running())
                throw new Exception("Container Doesn't Exist or is Not Running, cannot Stop a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            string timeArg = time != 0 ? $"--time {time}" : "";

            runner.TryRunCommand($"docker stop {timeArg} {Name}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Stopping Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Stops the Docker Container Asynchronously
        /// </summary>
        /// <param name="time">Time for the Container to Stop, default is ~10 seconds</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public async Task StopAsync(int time = 0)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Exists() || !Running())
                throw new Exception("Container Doesn't Exist or is Not Running, cannot Stop a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            string timeArg = time != 0 ? $"--time {time}" : "";

            await runner.TryRunCommandAsync($"docker stop {timeArg} {Name}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Stopping Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Forcefully Kills the Docker Container, stopping it Immediately
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public void Kill()
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!Running())
                throw new Exception("Container Doesn't Exist, cannot Stop a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            runner.TryRunCommand($"docker kill {Name}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Killing Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Checks if the Docker Container Exists
        /// </summary>
        /// <returns>True if the Container already Exists, False otherwise</returns>
        public bool Exists()
        {
            return Docker.ContainerExists(Name);
        }

        /// <summary>
        /// Runs a Docker Container with the specified Arguments on Startup Asynchronously, Removes itself once finished
        /// </summary>
        /// <param name="command">Command to run on Startup</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists or if an Error Occured in the container</exception>
        /// <returns>Nothing</returns>
        public async Task RunAsync(string command)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            await runner.TryRunCommandAsync($"docker run --name {Name} --rm {GetAdditionalArguments(true, false)} {Image} {command}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Runs a Docker Container with the specified Arguments on Startup, Removes itself once finished
        /// </summary>
        /// <param name="command">Command to run on Startup</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists or if an Error Occured in the container</exception>
        public void Run(string command)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            if (!runner.TryRunCommand($"docker run --name {Name} --rm {GetAdditionalArguments(false, false)} {Image} {command}"))
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Starts the Docker Container in Detached Mode
        /// </summary>
        /// <param name="interactive">Run the Container in Interactive Mode, method to artificially leave a container "Hanging", wasteful method</param>
        /// <exception cref="InvalidOperationException">Thrown if the Docker Service is not Running</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists</exception>
        public void Start(bool interactive = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            string command = $"docker run --name {Name} {GetAdditionalArguments(true, interactive)} {Image}";

            runner.TryRunCommand(command);

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Starting Docker Container : (Command : {command}) \n{string.Join("\n", runner.StandardError)} \n {string.Join("\n", runner.StandardOutput)}");
        }

        /// <summary>
        /// Starts the Docker Container in Detached Mode Asynchronously
        /// </summary>
        /// <param name="interactive">Run the Container in Interactive Mode, method to artificially leave a container "Hanging", wasteful method</param>
        /// <exception cref="InvalidOperationException">Thrown if the Docker Service is not Running</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists</exception>
        /// <returns>Nothing</returns>
        public async Task StartAsync(bool interactive = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            await runner.TryRunCommandAsync($"docker run --name {Name} {GetAdditionalArguments(true, interactive)} {Image}");

            if (runner.StandardError.Length != 0 && !IgnoreContainerErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Generic Function for Repeatedly checking a Condition until it is met or a Timeout Occurs
        /// </summary>
        /// <param name="condition">Function representing the Condition to repeatedly Check. Function will block thread until the condition is equivalent to <paramref name="expectedResult"/></param>
        /// <param name="expectedResult">The expected result of the condition function before unblocking the thread</param>
        /// <param name="label">Debugging Label to print</param>
        /// <param name="maxWaitSec">Maximum number of Seconds to wait for before continuing as a Timeout error</param>
        private void WaitUntil(Func<bool> condition, bool expectedResult, string label, int maxWaitSec)
        {
            int waitCount = maxWaitSec * 10;
            int count = 0;
            while (condition() != expectedResult && count < waitCount)
            {
                Thread.Sleep(100);
                count++;
            }

            if (Docker.DEBUG)
                Console.WriteLine($"Docker Container {label} : {condition()} ({count})");
        }

        /// <summary>
        /// Waits for the Container to Exist for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to Exist</returns>
        public void WaitUntilExists(int maxWaitSec = 10) => WaitUntil(Exists, true, "Exists", maxWaitSec);

        /// <summary>
        /// Waits for the Container to be Running for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Running</returns>
        public void WaitUntilRunning(int maxWaitSec = 10) => WaitUntil(Running, true, "Running", maxWaitSec);

        /// <summary>
        /// Waits for the Container to be Ready for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Ready</returns>
        public void WaitUntilReady(int maxWaitSec = 10) => WaitUntil(Ready, true, "Ready", maxWaitSec);

        /// <summary>
        /// Waits for the Container to Not Exist for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to Exist</returns>
        public void WaitUntilRemoved(int maxWaitSec = 10) => WaitUntil(Exists, false, "Exists", maxWaitSec);

        /// <summary>
        /// Waits for the Container to Not be Running for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Running</returns>
        public void WaitUntilStopped(int maxWaitSec = 10) => WaitUntil(Running, false, "Running", maxWaitSec);

        /// <summary>
        /// Waits for the Container to Not be Ready for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Ready</returns>
        public void WaitUntilUnready(int maxWaitSec = 10) => WaitUntil(Ready, false, "Running", maxWaitSec);

        /// <summary>
        /// Checks if the Container is Ready to be used
        /// </summary>
        /// <returns>True if the Container Exists and is Running</returns>
        public bool Ready() => Exists() && Running();

        /// <summary>
        /// Checks if the Docker Container is Running
        /// </summary>
        /// <returns>True if the Container is Running, False otherwise</returns>
        public bool Running()
        {
            CommandRunner runner = new CommandRunner();
            string stateStr = "\"{{.State.Running}}\"";

            runner.TryRunCommand($"docker inspect -f {stateStr} {Name}");

            return runner.StandardOutput[runner.StandardOutput.Length - 1] == "true";
        }

        /// <summary>
        /// Checks if the Docker Container is Paused
        /// </summary>
        /// <returns>True if the Container is Paused, False otherwise</returns>
        public bool Paused()
        {
            CommandRunner runner = new CommandRunner();
            string stateStr = "\"{{.State.Paused}}\"";

            runner.TryRunCommand($"docker inspect -f {stateStr} {Name}");

            return runner.StandardOutput[runner.StandardOutput.Length - 1] == "true";
        }

        /// <summary>
        /// Gets the Environment Variables Arguments for the Containers Startup
        /// </summary>
        /// <returns>Arguments to set the Environment Variables on Startup</returns>
        private string GetEnvironmentVariables()
        {
            string env = "";

            foreach (var item in EnvironmentVariables)
                env += $"-e {item.Key}={item.Value} ";

            return env;
        }

        /// <summary>
        /// Adds the Extra Arguments for Docker In Docker capabilities
        /// </summary>
        /// <returns>Arguments for Docker In Docker</returns>
        private string GetDockerInDocker()
        {
            if (!DockerInDocker)
                return "";

            CommandRunner runner = new CommandRunner();

            if (OperatingSystem.IsLinux() && File.Exists("/var/run/docker.sock") && runner.TryRunCommand("(getent group docker | cut -d: -f3)"))
                return $"--privileged --group-add {runner.StandardOutput[0]} -v /var/run/docker.sock:/var/run/docker.sock ";

            if (!EnvironmentVariables.ContainsKey("DOCKER_HOST"))
                AddEnvironmentVariable("DOCKER_HOST", "tcp://host.docker.internal:2375");

            return "";
        }

        /// <summary>
        /// Gets the Addityional Arguments for the Containers Startup
        /// </summary>
        /// <param name="detached">Toggle for Container to Startup in Detached Mode</param>
        /// <param name="interactive">Toggle for Container to Startup in Interactive Mode</param>
        /// <returns>Arguments to Add to the Container Startup</returns>
        private string GetAdditionalArguments(bool detached, bool interactive)
        {
            string args = "";

            args += interactive ? "-it " : "";
            args += detached ? "-d " : "";
            args += GetDockerInDocker();
            args += GetEnvironmentVariables();

            return args;
        }
    }
}