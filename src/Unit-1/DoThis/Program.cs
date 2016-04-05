﻿using System;
﻿using Akka.Actor;
using Akka.Actor.Internal;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));

            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            MyActorSystem.AwaitTermination();
        }
    }
    #endregion
}
