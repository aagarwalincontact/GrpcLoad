using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcLoad;
using Grpc.Core;
using System.Threading;

namespace GRPCLoadTest
{
    public class PollingClient
    {

        static void Main(string[] args)
        {

            Poller _poller = new Poller();
            _poller.StartTimer();
            Console.ReadKey();
            
        }
        
    }

    public class Poller
    {
        public List<Timer> _queryPollTimer;
        public Channel _grpcchannel;
        public bool _isCurrentlyRunning = false;
        public Poller()
        {
            _grpcchannel = new Channel("localhost", 9880, ChannelCredentials.Insecure, new List<ChannelOption>
                                                {
                                                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, Int32.MaxValue),
                                                    new ChannelOption(ChannelOptions.MaxSendMessageLength, Int32.MaxValue),
                                                    new ChannelOption(ChannelOptions.MaxConcurrentStreams,Int32.MaxValue)
                                                });
            _queryPollTimer = new List<Timer>();

        }

        public void StartTimer()
        {
            for (int i = 0; i < 10; i++)
                _queryPollTimer.Add(new Timer(PollTimerCallback, this, 2000, 2000));
        }

        private async void PollTimerCallback(object timerObject)
        {
            try
            {

                List<Task> pollTasks = new List<Task>();

                pollTasks.Add(RequestAsync().ContinueWith(task =>
                {
                    Reply result = task.Result;
                    if (task.Exception != null)
                        Console.WriteLine("Result Status :{0}", task.Exception);
                    else
                        Console.WriteLine("Result Status:{0}", "Success");

                }));

                await Task.WhenAll(pollTasks);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Polling Client PollTimerCallback threw exception.", ex);
            }
        }

        private async Task<Reply> RequestAsync()
        {
            var _client = new GrpcMessage.GrpcMessageClient(_grpcchannel);
            return await _client.GetDataAsync(new Request(),null, DateTime.UtcNow.AddMilliseconds(3000));
        }
    }
}
