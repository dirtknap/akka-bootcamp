using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using WinTail.Commons;

namespace WinTail.Actors
{
    class TailActor : UntypedActor
    {
        private readonly string filePath;
        private readonly IActorRef reporterActor;
        private FileObserver observer;
        private Stream fileStream;
        private StreamReader streamReader;


        public TailActor(string filePath, IActorRef reporterActor)
        {
            this.filePath = filePath;
            this.reporterActor = reporterActor;
            
        }

        protected override void PreStart()
        {
            observer = new FileObserver(Self, Path.GetFullPath(filePath));
            observer.Start();

            fileStream = new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(fileStream, Encoding.UTF8);

            var text = streamReader.ReadToEnd();
            Self.Tell(new InitialRead(filePath, text));
        }

        protected override void PostStop()
        {
            observer.Dispose();
            observer = null;
            streamReader.Close();
            streamReader.Dispose();
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = streamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    reporterActor.Tell(text);
                }
            }else if (message is FileError)
            {
                var fe = message as FileError;
                reporterActor.Tell($"Tail Error: {fe.Reason}");
            } else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                reporterActor.Tell(ir.Text);
            }
        }

        public class FileWrite
        {
            public string FileName { get; private set; }

            public FileWrite(string fileName)
            {
                FileName = fileName;
            }
        }

        public class FileError
        {
            public string FileName { get; private set; }
            public string Reason { get; private set; }

            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }
        }

        public class InitialRead
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }
    }
}
