using System;
using NanoDNA.ProcessRunner;

namespace NanoDNA.DockerManager
{
    /// <summary>
    /// Global Static Docker Class to interact with the Docker Service on the Device without Containers
    /// </summary>
    public static class Docker
    {
        /// <summary>
        /// Used to Debug some if the Wait Statements, internal for development purposes
        /// </summary>
        internal static bool DEBUG = false;

        /// <summary>
        /// Checks if the Docker Service is Running on the Device
        /// </summary>
        /// <returns>True if the Docker Service is Started, False otherwise</returns>
        public static bool Running()
        {
            CommandRunner runner = new CommandRunner();

            runner.TryRun("docker info");

            return !(string.Join("\n", runner.STDError).Contains("ERROR: error during connect"));
        }

        /// <summary>
        /// Checks if a Docker Container Exists on the Device
        /// </summary>
        /// <param name="containerName">Name of the Docker Container to check</param>
        /// <returns>True if the Docker Container</returns>
        public static bool ContainerExists(string containerName)
        {
            if (!Running())
                throw new InvalidOperationException("Docker Service is not Running");

            CommandRunner runner = new CommandRunner();

            runner.TryRun($"docker inspect {containerName}");

            return runner.STDError.Length == 0;
        }

        /// <summary>
        /// Checks if a Docker Container is Running on the Device
        /// </summary>
        /// <param name="containerName">Name of the Container</param>
        /// <returns>True if the Container is Running, False otherwise</returns>
        public static bool ContainerRunning(string containerName)
        {
            if (!Running())
                throw new InvalidOperationException("Docker Service is not Running");

            CommandRunner runner = new CommandRunner();
            string stateStr = "\"{{.State.Running}}\"";

            runner.TryRun($"docker inspect -f {stateStr} {containerName}");

            return runner.STDOutput[runner.STDOutput.Length - 1] == "true";
        }

        /// <summary>
        /// Stops a Docker Container that is Running on the Device
        /// </summary>
        /// <param name="containerName">Name of the Container</param>
        /// <param name="time">Time for the Container to Stop, default is ~10 seconds</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public static void StopContainer(string containerName, int time = 0)
        {
            if (!Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!ContainerExists(containerName) || !ContainerRunning(containerName))
                throw new Exception("Container Doesn't Exist or is Not Running, cannot Stop a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            string timeArg = time != 0 ? $"--time {time}" : "";

            runner.TryRun($"docker stop {timeArg} {containerName}");

            if (runner.STDError.Length != 0)
                throw new Exception($"Error Stopping Docker Container : {string.Join("\n", runner.STDError)}");
        }

        /// <summary>
        /// Forcefully Kills the Docker Container, stopping it Immediately
        /// </summary>
        /// <param name="containerName">Name of the Container</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public static void KillContainer(string containerName)
        {
            if (!Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!ContainerExists(containerName))
                throw new Exception("Container Doesn't Exist, cannot Stop a Non Existent Container");

            CommandRunner runner = new CommandRunner();

            runner.TryRun($"docker kill {containerName}");

            if (runner.STDError.Length != 0)
                throw new Exception($"Error Killing Docker Container : {string.Join("\n", runner.STDError)}");
        }

        /// <summary>
        /// Removes the Docker Container from the Device
        /// </summary>
        /// <param name="containerName">Name of the Container</param>
        /// <param name="force">Force the Removal of the Container</param>
        /// <exception cref="InvalidOperationException">Thrown if Docker Service is not Started</exception>
        /// <exception cref="Exception">Thrown if the Container doesn't Exist</exception>
        public static void RemoveContainer(string containerName, bool force = false)
        {
            if (!Docker.Running())
                throw new InvalidOperationException("Docker Service is not Running");

            if (!ContainerExists(containerName))
                throw new Exception("Container Doesn't Exist, cannot Remove a Non Existent Container");

            if (ContainerRunning(containerName) && !force)
                throw new Exception("Cannot Remove a Running Container, set Force to True to Remove the Container");

            CommandRunner runner = new CommandRunner();

            string forceArg = force ? "-f" : "";

            runner.TryRun($"docker rm {forceArg} {containerName}");

            if (runner.STDError.Length != 0)
                throw new Exception($"Error Removing Docker Container : {string.Join("\n", runner.STDError)}");
        }
    }
}
