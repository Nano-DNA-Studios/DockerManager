using NanoDNA.ProcessRunner;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

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
        /// Toggle for the Container to be run in Interactive Mode
        /// </summary>
        //public bool Interactive { get; private set; }

        /// <summary>
        /// Toggle for the Container to be run in Detached Mode
        /// </summary>
        //public bool Detached { get; private set; }

        /// <summary>
        /// Environment Variables for the Docker Container
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; private set; }

        /// <summary>
        /// Initializes a new Instance of a <see cref="DockerContainer"/>.
        /// </summary>
        /// <param name="name">Name of the Container once Started</param>
        /// <param name="image">Name of the Docker Image the Container will use</param>
        /// <exception cref="ArgumentException">Thrown if the Name if Invalid</exception>
        public DockerContainer(string name, string image)
        {
            Name = name;
            Image = image;
            EnvironmentVariables = new Dictionary<string, string>();

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
        /// <exception cref="ArgumentException">Thrown if the Name if Invalid</exception>
        public DockerContainer(string name, string image, Dictionary<string, string> environmentVariables)
        {
            Name = name;
            Image = image;
            EnvironmentVariables = environmentVariables;

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

            EnvironmentVariables.Add(name, value);
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

        public void Execute()
        {
            //Executes Docker Command
        }

        public string GetLogs()
        {
            ///Return the docker logs as a string
            return "";
        }

        public void Remove()
        {
            //Removes the Docker Container
        }

        public void Stop()
        {

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
        /// <param name="arguments">Arguments to run on Startup</param>
        /// <param name="ignoreErrors">Ignores Errors the Container encouters and doesn't throw them</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists or if an Error Occured in the container</exception>
        /// <returns>Nothing</returns>
        public async Task RunAsync(string arguments, bool ignoreErrors = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            await runner.RunCommandAsync($"docker run --name {Name} --rm {GetAdditionalArguments(true, false)} {Image} {arguments}");

            if (runner.StandardError.Length != 0 && !ignoreErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Runs a Docker Container with the specified Arguments on Startup, Removes itself once finished
        /// </summary>
        /// <param name="arguments">Arguments to run on Startup</param>
        /// <param name="ignoreErrors">Ignores Errors the Container encouters and doesn't throw them</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists or if an Error Occured in the container</exception>
        public void Run(string arguments, bool ignoreErrors = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            runner.RunCommand($"docker run --name {Name} --rm {GetAdditionalArguments(false, false)} {Image} {arguments}");

            if (runner.StandardError.Length != 0 && !ignoreErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Starts the Docker Container in Detached Mode
        /// </summary>
        /// <param name="interactive">Run the Container in Interactive Mode, method to artificially leave a container "Hanging", wasteful method</param>
        /// <param name="ignoreErrors">Ignores Errors the Container encouters and doesn't throw them</param>
        /// <exception cref="InvalidOperationException">Thrown if the Docker Service is not Running</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists</exception>
        public void Start(bool interactive = false, bool ignoreErrors = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            CommandRunner runner = new CommandRunner();

            runner.RunCommand($"docker run --name {Name} {GetAdditionalArguments(true, interactive)} {Image}");

            if (runner.StandardError.Length != 0 && !ignoreErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Starts the Docker Container in Detached Mode Asynchronously
        /// </summary>
        /// <param name="interactive">Run the Container in Interactive Mode, method to artificially leave a container "Hanging", wasteful method</param>
        /// <param name="ignoreErrors">Ignores Errors the Container encouters and doesn't throw them</param>
        /// <exception cref="InvalidOperationException">Thrown if the Docker Service is not Running</exception>
        /// <exception cref="Exception">Thrown if the Container already Exists</exception>
        /// <returns>Nothing</returns>
        public async Task StartAsync(bool interactive = false, bool ignoreErrors = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (Exists())
                throw new Exception("Container Already Exists, Rename the Container or Remove the Old Container");

            string interactiveFlag = interactive ? "-it" : "";

            CommandRunner runner = new CommandRunner();

            await runner.RunCommandAsync($"docker run --name {Name} {GetAdditionalArguments(true, interactive)} {Image}");

            if (runner.StandardError.Length != 0 && !ignoreErrors)
                throw new Exception($"Error Starting Docker Container : {string.Join("\n", runner.StandardError)}");
        }

        /// <summary>
        /// Waits for the Container to Exist for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to Exist</returns>
        public Task WaitForExists(int maxWaitSec = 5)
        {
            return Task.Run(() =>
            {
                int waitCount = maxWaitSec * 10;
                int count = 0;
                while (!Exists() && count < waitCount)
                {
                    Thread.Sleep(100);
                    count++;
                }

                if (Docker.DEBUG)
                    Console.WriteLine($"Docker Container Exists : {Exists()} ({count})");
            });
        }

        /// <summary>
        /// Waits for the Container to be Running for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Running</returns>
        public Task WaitForRunning(int maxWaitSec = 5)
        {
            return Task.Run(() =>
            {
                int waitCount = maxWaitSec * 10;
                int count = 0;
                while (!Running() && count < waitCount)
                {
                    Thread.Sleep(100);
                    count++;
                }

                if (Docker.DEBUG)
                    Console.WriteLine($"Docker Container Running : {Running()} ({count})");
            });
        }

        /// <summary>
        /// Waits for the Container to be Ready for a specified amount of time
        /// </summary>
        /// <param name="maxWaitSec">Max number of Seconds to Wait</param>
        /// <returns>Task Waiting for the Container to be Ready</returns>
        public Task WaitForReady(int maxWaitSec = 5)
        {
            return Task.Run(() =>
            {
                int waitCount = maxWaitSec * 10;
                int count = 0;
                while (!Ready() && count < 50)
                {
                    Thread.Sleep(100);
                    count++;
                }

                if (Docker.DEBUG)
                    Console.WriteLine($"Docker Container Ready : {Ready()} ({count})");
            });
        }

        /// <summary>
        /// Checks if the Container is Ready to be used
        /// </summary>
        /// <returns>True if the Container Exists and is Running</returns>
        public bool Ready()
        {
            return Exists() && Running();
        }

        /// <summary>
        /// Checks if the Docker Container is Running
        /// </summary>
        /// <returns>True if the Container is Running, False otherwise</returns>
        public bool Running()
        {
            CommandRunner runner = new CommandRunner();
            string stateStr = "\"{{.State.Running}}\"";

            runner.RunCommand($"docker inspect -f {stateStr} {Name}");

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

            runner.RunCommand($"docker inspect -f {stateStr} {Name}");

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

        private string GetAdditionalArguments(bool detached, bool interactive)
        {
            string args = "";

            args += interactive ? "-it " : "";
            args += detached ? "-d " : "";
            args += GetEnvironmentVariables();

            return args;
        }
    }
}
