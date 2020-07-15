﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fabrik.SimpleBus.Demo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            var bus = new InProcessBus();

            // Delegate Handler
            bus.Subscribe<string>(message => Console.WriteLine("Delegate Handler Received: {0}", message));
            bus.Subscribe<string>(async (message, token) => await WriteMessageAsync(message, token));

            // Strongly typed handler
            bus.Subscribe(() => new MessageHandler());

            // Strongly typed async handler
            bus.Subscribe(() => new AsyncMessageHandler()); // will automatically be passed a cancellation token

            Console.WriteLine("Enter a message\n");

            string input;
            while ((input = Console.ReadLine()) != "q")
            {
                var t2 = bus.SendAsync(input);
                var t1 = bus.SendAsync(new Message { Body = input });

                Task.WaitAll(t1, t2);

                Console.WriteLine("\nEnter another message\n");
            }
        }

        private Task WriteMessageAsync(string message, CancellationToken cancellationToken)
        {
            return Task.Delay(2000, cancellationToken)
                .ContinueWith(task => Console.WriteLine("Delegate Async Handler Received: {0}", message), cancellationToken);
        }
    }

    public class Message
    {
        public string Body { get; set; }
    }

    public class MessageHandler : IHandle<Message>
    {
        public void Handle(Message message)
        {
            Console.WriteLine("{0} Received message type: {1}", GetType().Name, nameof(Message));
        }
    }

    public class AsyncMessageHandler : IHandleAsync<Message>
    {
        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            Console.WriteLine("{0} Received message type: {1}", GetType().Name, nameof(Message));
        }
    }
}