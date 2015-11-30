namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskDataBlobStorage : IBlobStorage<byte[]>
    {
        string GenerateBlobId(byte[] blob);
    }
}