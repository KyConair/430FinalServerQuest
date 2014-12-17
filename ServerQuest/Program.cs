using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServerQuest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Welcome to ServerQuest!\n\n");

            UDPServer server = new UDPServer();
            server.startup();

            Console.Write("How many clients should we start with? ");
            int numClients = int.Parse(Console.ReadLine());
            Console.Write("\nHow often should each client fire a packet (in ms)? (50 is 'average') ");
            int packetDelay = int.Parse(Console.ReadLine());
            Console.Write("\nHow long should the barrage run for (in seconds)? ");
            int time = int.Parse(Console.ReadLine());

            System.Timers.Timer stopwatch = new System.Timers.Timer(time * 1000);

            bool isFiring = false;

            //Once 'time' is passed, 'Elapsed' is called as an event, allowing us to call a delegate that simply sets 'isFiring' to false
            stopwatch.Elapsed += delegate { isFiring = false; };
            
            //Set up lists to hold our clients, and the task process controlling said client(used during firing)
            List<UDPC> clients = new List<UDPC>();
            List<Task> clientTasks = new List<Task>();
            //Make a cancellationtoken for stopping our tasks
            var clientsCancellationToken = new CancellationTokenSource();
            for(int i = 0; i < numClients; i++) {
                UDPC client = new UDPC();
                client.startup(i+1, packetDelay);
                clients.Add(client);
            }

            Console.Write("\n\nAll clients are ready to fire, cap'n! \nPress any key to fire!");
            Console.ReadKey();
            Console.Write("\n\nFIRE!!!");

            isFiring = true;
            stopwatch.Enabled = true;
            
            //Start each client in its own task, each client knows to keep sending packets from its beginSending method
            for (int i = 0; i < numClients; i++)
            {
                clientTasks.Add(Task.Factory.StartNew(() => 
                {
                    clients[i-1].beginSending(i+1);
                }, clientsCancellationToken.Token));
            }


            // Run no other code here until the timer has switched isFiring to false;
            while (isFiring);

            Console.Write("\n\nCEASING FIRE");
            stopwatch.Enabled = false;
            //Cleanup after ourselves and cancel the tasks holding the packet sending
            clientsCancellationToken.Cancel();

            Console.Write("\n\nNumber of packets calculated to have been sent: " + (time * (1000/packetDelay) * numClients));
            server.reportLastRun();

            // Hold the console open
            Console.ReadLine();
        }
    }
}
