using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Metadata.ViewStates;
using SharedLib;
using SharedLib.Exceptions;
using SharedLib.MetadataObjects;

namespace Metadata
{
    class MetadataServer : MarshalByRefObject, IMetaToPuppet, IMetaToClient, IMetaToMeta
    {

        public static Dictionary<int, MetaserverId> MetadataServerList = new Dictionary<int, MetaserverId>( );
        public static int ThisMetaserverId;
        public static int PortRecovery;
        //Request ID
        public static Int64 RequestIdMonotonic = 0;
        public static Int64 LastResponseSent = 0;
        public static MetaViewManager ViewManager;
        
        private static EventWaitHandle requestSeriesMonitor = new AutoResetEvent(true);

 

        //Metaserver Application Core
        private static MetaCore core = new MetaCore( );


        //Timeuntil timeout transaction
        //TODO reduzir para colocar em producao
        private static int _masterTimeout = 10000000;

        public Dictionary<long,List<MetaserverResponse>> Responses = new Dictionary<long, List<MetaserverResponse>>();

        public static void Main( string[] args )
        {
            System.Console.WriteLine( "Metadata Init...." );

        //  if (!Debugger.IsAttached)
          //     Debugger.Launch();
           // Debugger.Break();


            int i = 0;
            for ( i = 0; i < (args.Length - 2); i++ )
            {
                MetaserverId metaServer = new MetaserverId( );
                metaServer.Hostname = args[i++];
                metaServer.Port = Convert.ToInt32( args[i] );
                metaServer.Id = i / 2;
                MetadataServerList.Add( metaServer.Id, metaServer );
            }
            ThisMetaserverId = Convert.ToInt32( args[i++] );
            PortRecovery = Convert.ToInt32( args[i] ); ;
            System.Console.WriteLine("System ID: " + ThisMetaserverId);

            ViewManager = new MetaViewManager( ThisMetaserverId, MetadataServerList );

            System.Console.WriteLine( "Metadata servers list loaded..." );
            RemotingConfiguration.Configure( "../../App.config", true );
            System.Console.WriteLine( "Metadata Server init..." );

            CreateEmergencyChannel( );
            //CreateMetaserverChannel( );

            

            System.Console.WriteLine( "<enter> to exit..." );
            System.Console.ReadLine( );
        }

 


        /////////////////////////////////////////////////////////////// Thread Sync ///////////////////////////////////////////////
        public delegate MetaserverResponse ProcessRequestDelegate( MetaRequest request);


        public MetaserverResponse ClientProcessRequest( MetaRequest request )
        {
            //Forward to Master and wait for master ack. Then ack to client
              int master = ViewManager.GetMaster( );
              if ( master == ThisMetaserverId )
                return MasterReceiveRequest(request);
            else
            {
                IMetaToMeta server = ConnectToMetaserver( master );
                return server.MasterReceiveRequest( request );
            }
        }

 
        public MetaserverResponse MasterReceiveRequest( MetaRequest request )
        {
            //Stamp the request
            request.MasterReqStamp = GetNewSeriesRequestId();


            //TODO Verificar se ainda e master


            //Im the Master, forward to slaves and ack to front end
            foreach ( int serverId in ViewManager.GetSlaveList( ) )
            {
                if (serverId != ThisMetaserverId)
                {
                     IMetaToMeta server = ConnectToMetaserver( serverId );
                     ProcessRequestDelegate processDel = new ProcessRequestDelegate( server.SlaveReceiveRequest );
                     AsyncCallback processCb = new AsyncCallback( CbProcessRequest );
                     processDel.BeginInvoke( request, processCb, null );
                }
            }
             //Thread que também vai invocar o callback para simular o envio de uma resposta local
             ProcessRequestDelegate localDel = new ProcessRequestDelegate( SlaveReceiveRequest );
            AsyncCallback localCb = new AsyncCallback( CbProcessRequest );
            localDel.BeginInvoke( request, localCb, null );

            //TODO Criar outra thread que ao fim de X seg vai ver se já há resposta e se nao houver, declara falha
            return new MetaserverResponse( ResponseStatus.AckMaster, request);
        }



        public MetaserverResponse SlaveReceiveRequest(MetaRequest request)
                {
            //Processar o pedido, check type, unbox and process
        switch ( request.RequestType )
                    {
            case RequestType.Create:
                    return Create(request);
            case RequestType.Open:
                    return Open(request);
            case RequestType.Close:
                    return Close(request);
             case RequestType.Delete:
                    return Delete(request);
            case RequestType.Registry:
                    return Registry(request);
             default: 
                    return  new MetaserverResponse( ResponseStatus.InvalidRequest,request);
            }
        }



        /// <summary>
        /// Guarda a resposta e notifica o processo em espera (no master)
        /// </summary>
        /// <param name="ar"></param>
        public void CbProcessRequest( IAsyncResult ar )
         {
            ProcessRequestDelegate del = (ProcessRequestDelegate) ((AsyncResult) ar).AsyncDelegate;
            MetaserverResponse response = del.EndInvoke( ar );
            Console.WriteLine(response.Status);
            //Guardar a resposta
            lock (Responses)
            {
                List<MetaserverResponse> list;
                if (!(Responses.TryGetValue(response.OriginalRequest.MasterReqStamp, out list)))
                        list = new List<MetaserverResponse>();

                int numReplies = list.Count;
                
                if ( numReplies < (ViewManager.CountOnlineServers() - 1) )
                        {
                    list.Add(response);
                    Responses.Remove(response.OriginalRequest.MasterReqStamp);
                    Responses.Add(response.OriginalRequest.MasterReqStamp, list);
                        }
                    else
                    {
                     //Se ja recebeu todas as respostas, desbloqueia a thread para receber o pedido
                     Responses.Remove( response.OriginalRequest.MasterReqStamp );
                     Console.WriteLine( "Process Request" );
                     SendResponseToClient( response );
                }
            }
         }


        /// <summary>
        /// Send response back to client
        /// </summary>
        /// <param name="response"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        private void SendResponseToClient(MetaserverResponse response)
        {
            //Increment last ResponseSent
            Interlocked.Increment(ref LastResponseSent);


            String hostname = response.OriginalRequest.ClientHostname;
            int port = response.OriginalRequest.ClientPort;
            IClientToMeta client =
                (IClientToMeta)
                Activator.GetObject( typeof( IClientToMeta ), "tcp://" + hostname + ":" + port + "/PADIConnection" );
            client.ReceiveMetaserverResponse(response);
        }



        public BullyMsg BullyRequestsRetrival(BullyMsg msg)
        {
            return ViewManager.BullyRequestsRetrival(msg);
        }



        ////////////////////// Invocacaoes ao core ///////////////////////////
        /// 
        private MetaserverResponse Delete( MetaRequest request )
            {
            RequestDelete deleteRequest = (RequestDelete) request;
            MetaserverResponse response;
            //Invocar o servidor
            try
            {
                Boolean success = core.Delete(deleteRequest.Filename,request.ClientId);
                if (success)
                    response = new MetaserverResponse(ResponseStatus.Success, request);
                else
                {
                      response = new MetaserverResponse( ResponseStatus.Exception, request );
                      response.Exception = new PadiException( PadiExceptiontType.DeleteFile, "File not deleted" );
                }
            }
            catch (Exception ex)
            {
                response = new MetaserverResponse(ResponseStatus.Exception, request);
                response.Exception = new PadiException(PadiExceptiontType.DeleteFile, ex.Message);
            }

            return response;
            }

        private MetaserverResponse Close( MetaRequest request )
            {
            RequestClose closeRequest = (RequestClose) request;
            MetaserverResponse response;
            //Invocar o servidor
            try
            {
            Boolean success = core.Close( closeRequest.Filename, request.ClientId );
                if ( success )
                    response = new MetaserverResponse( ResponseStatus.Success, request );
                else
                    {
                    response = new MetaserverResponse( ResponseStatus.Exception, request );
                    response.Exception = new PadiException( PadiExceptiontType.CloseFile, "File not closed" );
                    }
            }
            catch (Exception ex)
            {
               response = new MetaserverResponse(ResponseStatus.Exception, request);
               response.Exception = new PadiException(PadiExceptiontType.CloseFile, ex.Message);
            }

            return response;
        }



        private MetaserverResponse Create( MetaRequest request )
            {
            RequestCreate createRequest = (RequestCreate) request;
            MetaserverResponse response;
            //Invoca o servidor
            try
            {
                MetadataEntry entry = core.Create(createRequest.Filename, createRequest.NbDataServer, createRequest.ReadQuorum,
                        createRequest.WriteQuorum,request.ClientId);
                response = new MetaserverResponse(ResponseStatus.Success, request);
                response.MetaEntry = entry;
            }
            catch (Exception ex)
            {
                response = new MetaserverResponse(ResponseStatus.Exception, request);
                response.Exception = new PadiException(PadiExceptiontType.CreateFile, ex.Message);
            }
            return response;
        }

        private MetaserverResponse Open( MetaRequest request )
            {
            RequestOpen openRequest = (RequestOpen) request;
            MetaserverResponse response;
            try
            {
                MetadataEntry entry = core.Open(openRequest.Filename,request.ClientId);
                response = new MetaserverResponse(ResponseStatus.Success, request);
                response.MetaEntry = entry;
            }
            catch (Exception ex)
            {
               response = new MetaserverResponse(ResponseStatus.Exception, request);
                response.Exception = new PadiException(PadiExceptiontType.OpenFile, ex.Message);
            }
            return response;
        }

        private MetaserverResponse Registry(MetaRequest request)
        {
            RequestRegistry registryRequest = (RequestRegistry) request;
            MetaserverResponse response;
            try
            {
                core.Registry(registryRequest.ServerId, registryRequest.ServerIp, registryRequest.ServerPort);
                response = new MetaserverResponse(ResponseStatus.Success, request);
            }
            catch (Exception ex)
            {
                response = new MetaserverResponse(ResponseStatus.Exception, request);
                response.Exception = new PadiException(PadiExceptiontType.Registry, ex.Message);
            }

            return response;
        }








        public static  IMetaToMeta ConnectToMetaserver( int serverNumber )
        {
            MetaserverId serverId;
            if ( !MetadataServerList.TryGetValue( serverNumber, out serverId ) )
                throw new PadiException( PadiExceptiontType.InvalidRequest, "Can not connet to Metaserver: " + serverNumber );
            IMetaToMeta server;
            try
            {
                server =
                    (IMetaToMeta)
                    Activator.GetObject( typeof( IMetaToMeta ),
                                        "tcp://" + serverId.Hostname + ":" + serverId.Port + "/PADIConnection" );
                return server;
            }
            catch ( Exception e )
                {
                throw new PadiException( PadiExceptiontType.InvalidRequest, "Error connecting to Metaserver" + e.Message );
                }
            }






        //inteface ao puppet
        public String Dump( )
        {
            return core.Dump();
        }


        ////////////////////////// COPY MANAGEMENT ////////////////////////////////////////////////

        public static bool UpdateServer( CopyStructMetadata metaCopyStruct )
            {
                Console.WriteLine( "Start load process" );
                core.LoadFromStructMetadataCore( metaCopyStruct );
                RequestIdMonotonic = metaCopyStruct.RequestIdMonotonic;
                LastResponseSent = metaCopyStruct.LastResponseSent;
                Console.WriteLine( "Load done" );
                return true;
            }


        public CopyStructMetadata RequestUpdate( )
            {
            Console.WriteLine( "Start Copy Process" );
            while ( Thread.VolatileRead( ref RequestIdMonotonic ) != Thread.VolatileRead( ref LastResponseSent ) )
                {
                //Wait to finish all requests
                Console.WriteLine( "Wait for end of all requests..." );
                Thread.Sleep( 500 );
                }
            CopyStructMetadata metaCopyStruct = core.Copy( );
            metaCopyStruct.RequestIdMonotonic = RequestIdMonotonic;
            metaCopyStruct.LastResponseSent = LastResponseSent;
            Console.WriteLine( "Sending Copy" );
            return metaCopyStruct;
          }


      



        private static long GetNewSeriesRequestId( )
        {
            //Wait if the master is stop
            requestSeriesMonitor.WaitOne();
            long id = Interlocked.Increment( ref RequestIdMonotonic );
            //Unlock next number
            requestSeriesMonitor.Set( );
            return id;
        }

        public static long LockSeriesRequestsId()
        {
            //Lock monitor
            requestSeriesMonitor.Reset( );
            return Thread.VolatileRead(ref RequestIdMonotonic);
        }

        public static void UnlockRequestId( )
        {
            requestSeriesMonitor.Set( );
        }


    
        public static Boolean IsMaster()
        {
            return (ViewManager.GetMaster() == ThisMetaserverId);
        }

        /////////////////////////////// Channel Manager /////////////////////////////////
        public static TcpChannel MetaServerChannel;
        public static TcpChannel EmergencyChannel;

        public String Fail( )
        {
            Console.WriteLine( "FAIL!!!!!!!!!" );
            //Unbind current channel
            if ( MetaServerChannel == null )
                return "";
        ChannelServices.UnregisterChannel( MetaServerChannel );
            //Bind object launchmeta
            CreateEmergencyChannel( );
         return "Metaserver "+ThisMetaserverId+ "fail";
        }

        public static void CreateEmergencyChannel( )
        {
            EmergencyChannel = new TcpChannel( PortRecovery );
            ChannelServices.RegisterChannel( EmergencyChannel, false );
            RemotingConfiguration.RegisterWellKnownServiceType(
                  typeof( MetaLauncher ),
                  "PADIConnection",
                  WellKnownObjectMode.Singleton );
        }


        public static void CreateMetaserverChannel( )
        {
            MetaserverId thisServer;
            if ( !MetadataServerList.TryGetValue( ThisMetaserverId, out thisServer ) )
                throw new PadiException( PadiExceptiontType.InvalidRequest, "Cannot create channel to metaserver. Invalid ID: " + ThisMetaserverId );


            MetaServerChannel = new TcpChannel( thisServer.Port );
            ChannelServices.RegisterChannel( MetaServerChannel, false );
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof( MetadataServer ),
                "PADIConnection",
                WellKnownObjectMode.Singleton );
        }



        public static void DestroyEmergencyChannel( )
        {
           if ( EmergencyChannel == null )
                return;
            ChannelServices.UnregisterChannel( EmergencyChannel );
        }


        public override object InitializeLifetimeService( )
        {
            return null;
        }

        }




     
          

    public class MetaLauncher : MarshalByRefObject, IRecovery
    {
        public String Recover( )
            {
            Console.WriteLine( "Recovered!!!!!!!!!" );
            //Unbind Old
            MetadataServer.DestroyEmergencyChannel( );
            //Create Channel again
            MetadataServer.CreateMetaserverChannel( );
            //TODO Desbloquear os pedidos agora
            MetadataServer.ViewManager.UpdateView( );
            Console.Write( MetadataServer.ViewManager );
            return "Metadataserver " + MetadataServer.ThisMetaserverId + " recovered \r\n " + MetadataServer.ViewManager;
            }
        }

    public struct MetaserverId
    {
        public int Id;
        public int Port;
        public String Hostname;
    }

   
    }
