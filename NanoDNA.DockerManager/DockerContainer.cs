
namespace NanoDNA.DockerManager
{
    internal class DockerContainer
    {

        /// <summary>
        /// Name of the Docker Container
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID of the Docker Container
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// Image of the Docker Container
        /// </summary>
        public string Image { get; set; }


        public DockerContainer ()
        {

        }

        public void Execute ()
        {
            //Executes Docker Command
        }






    }
}
