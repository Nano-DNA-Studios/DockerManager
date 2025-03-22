using NanoDNA.ProcessRunner;
using System;

namespace NanoDNA.DockerManager
{
    public static class Docker
    {
        internal static bool DEBUG = false;

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

            runner.RunCommand($"docker inspect {containerName}");

            return runner.StandardError.Length == 0;
        }

        /// <summary>
        /// Checks if the Docker Service is Running on the Device
        /// </summary>
        /// <returns>True if the Docker Service is Started, False otherwise</returns>
        public static bool Running()
        {
            CommandRunner runner = new CommandRunner();

            runner.RunCommand("docker info");

            //Console.WriteLine($"STDOut : {string.Join("\n", runner.StandardOutput)}");
            //Console.WriteLine($"STDErr : {string.Join("\n", runner.StandardError)}");

            return !(string.Join("\n", runner.StandardError).Contains("ERROR: error during connect"));
        }






    }
}
