using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace LTG.Deployment.DbDeploy.DataAccess.Helpers
{
    public interface IMd5Service
    {
        string CalculateMd5Hash(string path);
    }

    public class Md5Service : IMd5Service
    {
        public IFileSystem FileSystem { get; }

        public Md5Service(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public string CalculateMd5Hash(string path)
        {
            var data = FileSystem.File.ReadAllBytes(path);
            var md5 = MD5.Create();

            var hash = md5.ComputeHash(data);

            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}