using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class FileDataController : FileDataControllerBase
    {
        public FileDataController(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters, IFileDataStorage fileDataStorage)
            : base(remoteTaskQueueControllerBaseParameters, fileDataStorage)
        {
        }
    }
}