using System.IO;
using Akka.Actor;

namespace WinTail.Actors
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;
    
        public FileValidatorActor(IActorRef consoleWriteActorRef)
        {
            consoleWriterActor = consoleWriteActorRef;
        }
        
        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if (IsFileUri(msg))
                {
                    consoleWriterActor.Tell(new Messages.InputSuccess($"Starting processing for {msg}"));

                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, consoleWriterActor));
                }
                else
                {
                    consoleWriterActor.Tell(new Messages.ValidationError($"{msg} is not an existing file URI on disk"));
                }
            }

            Sender.Tell(new Messages.ContinueProcessing());
        }

        private bool IsFileUri(string msg)
        {
            return File.Exists(msg);
        }
    }
}