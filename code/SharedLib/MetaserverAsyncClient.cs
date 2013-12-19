using System;
using System.Collections.Generic;
using SharedLib;
using SharedLib.Exceptions;
using SharedLib.MetadataObjects;
using System.Threading;



namespace SharedLib
{
    /// <summary>
    /// Client which will interact with metaserver. Responses are handle by main object
    /// </summary>
    public class MetaserverAsyncClient : IClientToMeta
    {
        public  List<ServerId> MetadataServerList = new List<ServerId>();
        public  String ClientHostname;
        public  int ClientPort;

        //Response Array. Int: RequestID
        public Dictionary<int, MetaserverResponse> ResponseList = new Dictionary<int, MetaserverResponse>();

        //TODO Reduzir para colocar em producao
        public static int MetaServerResponseTimeout = 10000;

        public int ClientRequestId = 0;
        public int ClientId;

        public MetaserverAsyncClient(String hostname, int clientPort, List<ServerId> metaServerList, int clientId)
        {
            ClientHostname = hostname;
            ClientPort = clientPort;
            MetadataServerList = metaServerList;
            ClientId = clientId;
        }



        /// <summary>
        /// This method is called my metaserver to deliver a response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public Boolean ReceiveMetaserverResponse( MetaserverResponse response )
            {
            lock ( ResponseList )
                {
                ResponseList.Add( response.OriginalRequest.ClientReqStamp, response );
                EventWaitHandle requestLocker;
                if ( EventWaitHandle.TryOpenExisting( "Client" + ClientPort + ":" + Convert.ToString( response.OriginalRequest.ClientReqStamp ), out requestLocker ) )
                    requestLocker.Set( );
                }
            return true;
            }



        /// <summary>
        /// Send the request to metaserver, lock until response. If timeout, repeat request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public MetaserverResponse SendRequestToMetaserver( MetaRequest request )
            {
            //Fill the request source details:
            int requestId = GetNewRequestId( );
            request.ClientHostname = ClientHostname;
            request.ClientReqStamp = requestId;
            request.ClientPort = ClientPort;
            request.ClientId = ClientId;

            EventWaitHandle monitor = new EventWaitHandle( false, EventResetMode.AutoReset, "Client" + ClientPort + ":" + Convert.ToString( requestId ) );
            MetaserverResponse response;
            while ( true )
                {
                request.Attempt++;
                //Do remote sync request
                //TODO Lidar com a falha de um metaserver
                response = ConnectToMetaserver( ).ClientProcessRequest( request );
                //Pedido foi entregue ao master

                //Wait no monitor com o ID do pedido ate receber a resposta
                monitor.WaitOne( MetaServerResponseTimeout, true );

                //Este wait tem timeout e é desempedido a quando da resposta dos meta
                lock ( ResponseList )
                    {
                    if ( ResponseList.TryGetValue( request.ClientReqStamp, out response ) )
                        break;
                    }
                //Se a resposta nao veio, vamos realizar repeticao de pedido   
                }

             if ( response.Status.Equals( ResponseStatus.Exception ) )
                {
                    PadiException exception = response.Exception;
                    throw exception;
                }
            return response;
            }

        private int GetNewRequestId()
        {
            return  Interlocked.Increment(ref ClientRequestId);
        }

        //TODO. adicionar um critério de escolha do metaserver
        private IMetaToClient ConnectToMetaserver()
        {
            Console.WriteLine("ConnectToMetaserver....");
            foreach (ServerId server in MetadataServerList)
            {
                IMetaToClient serverInterface = (IMetaToClient)Activator.GetObject(
                    typeof(IMetaToClient),
                    "tcp://" + server.hostname + ":" + server.port + "/PADIConnection");

                return serverInterface;
            }
            throw new Exception("Client: No metadataserver available");
        }
    }
}