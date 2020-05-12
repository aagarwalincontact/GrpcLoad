using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcLoad;
using Grpc.Core;
using System.Threading;
using Newtonsoft.Json;



namespace GrpcServer
{
    class GrpcServer
    {
        static void Main(string[] args)
        {
            var _server = new Server(new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, Int32.MaxValue),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, Int32.MaxValue),
                new ChannelOption(ChannelOptions.MaxConcurrentStreams,Int32.MaxValue)
            });
            _server.Services.Add(GrpcMessage.BindService(new GRPCServerImpl(new int[] { 2000, 5000, 10000 })));
            _server.Ports.Add(new ServerPort("0.0.0.0", 9880, ServerCredentials.Insecure));
            _server.Start();

            Console.ReadKey();
        }
    }

    public class GRPCServerImpl : GrpcMessage.GrpcMessageBase
    {
        public GRPCServerImpl()
        {
        }
        int[] _delayCount;
        int delay=0;
        public GRPCServerImpl(int[] delayCounts)
        {
            _delayCount = delayCounts;
        }
        public override Task<Reply> GetData(Request request, ServerCallContext context)
        {
            
                if (_delayCount != null && _delayCount.Length > 0)
                {
                    var sleep = 0;
                    lock (_delayCount)
                    {
                        sleep = _delayCount[delay++ % _delayCount.Length];
                    }
                    Thread.Sleep(sleep);//cause timeout
                }
            Console.WriteLine("Server Responding");
            return Task.FromResult(new Reply { Payload = new string('*', 5000000)});
        }
    }
}
