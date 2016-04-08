using System;
using System.IO;
using Akka.Actor;
using WinTail.Actors;

namespace WinTail.Commons
{
    public class FileObserver : IDisposable
    {
        private readonly IActorRef tailActor;
        private readonly string absoluteFilePath;
        private FileSystemWatcher watcher;
        private readonly string fileDirectory;
        private readonly string fileName;

        public FileObserver(IActorRef tailActor, string absoluteFilePath)
        {
            this.tailActor = tailActor;
            this.absoluteFilePath = absoluteFilePath;
            fileDirectory = Path.GetDirectoryName(absoluteFilePath);
            fileName = Path.GetFileName(absoluteFilePath);
        }

        public void Start()
        {
            watcher = new FileSystemWatcher(fileDirectory, fileName);

            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            watcher.Changed += OnFileChanged;

            watcher.Error += OnFileError;

            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            watcher.Dispose();
        }

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }
        }

        void OnFileError(object sender, ErrorEventArgs e)
        {
            tailActor.Tell(new TailActor.FileError(fileName,  e.GetException().Message), ActorRefs.NoSender);
        }
    }
}