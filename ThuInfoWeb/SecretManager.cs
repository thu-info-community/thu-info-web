namespace ThuInfoWeb
{
    public class SecretManager
    {
        public SecretManager(string createVersionKey)
        {
            CreateVersionKey = createVersionKey;
        }

        public string CreateVersionKey { get; }
    }
}
