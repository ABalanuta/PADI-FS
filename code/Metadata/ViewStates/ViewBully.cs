using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metadata.ViewStates;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStatus
    {
    /// <summary>
    /// Executa o protocolo de bully
    /// </summary>
    public class ViewBully : ViewState
    {
        private int _biggers = 0;
        private Object _waitBiggers = new object();
        private Object _waitSlaves = new object();
        private int _slaveAcks = 1;

    /// <summary>
    /// Start bully algorithm
    /// </summary>
        public ViewBully( MetaViewManager manager )
            : base( manager,ServerStatus.Bully )
        {
        }

        public override void Start()
        {

        //Multicast to biggers (I agree with me)
        Console.WriteLine("Send: are you bigger from"+Manager.ThisMetaserverId);
        Manager.MulticastMsg( new BullyMsg( BullyType.AreYouBigger, Manager.ThisMetaserverId,Manager.GetStatus() ), Manager.ThisMetaserverId, 2 );

        //Wait that all biggers can reply
        lock ( _waitBiggers )
            {
            Monitor.Wait( _waitBiggers, 1000 );
            if ( _biggers > 0 )
                {
                //There is a bigger men, he will take care
                Manager.ToPause( );
                return;
                }
            }

        //Timeout para os maiores acabou, sou o maior.
        //Multicast all anouncing that I'm the boss
        _slaveAcks = 1;

        //TODO Temos de tornar os OK atomicos porque um newbie pode votar contra e por isso os restantes ao podem recuperar

        Manager.MulticastMsg( new BullyMsg( BullyType.ImBoss, Manager.ThisMetaserverId,ServerStatus.Master), 0, 2);

        //Aguardo que todos os online facam ack
        lock ( _waitSlaves )
            {
            while ( _slaveAcks != Manager.CountOnlineServers( ) )
                {
                Monitor.Wait( _waitSlaves, 2000 );
                }
            }

        Console.WriteLine( "Im the boss!!!!!" );
        Manager.ToMaster( );
        }


        /// <summary>
        /// Se recebemos uma new Rowdy e estamos em Bully, vamos passar a pausa para que o novo rowdy possa copiar os dados e depois ele inicia
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override BullyMsg NewRowdyMsgRequest( int source )
         {
         BullyMsg resp = new BullyMsg( BullyType.StatusMsg, Manager.ThisMetaserverId, Manager.GetStatus( ) );
            resp.Status = ServerStatus.Paused;
            Manager.ToPause();
            return resp;
         }


        public override void ViewStatusChanged( int lastMaster, long lastId )
             {
                    //TODO Ver se e preciso
             }


         public override BullyMsg AreYouBiggerMsgRequest( int source )
             {
             if ( source < Manager.ThisMetaserverId )
                 {
                 Console.WriteLine( "Got an Are You Bigger from a smaller during bully: "+source );
                 return new BullyMsg( BullyType.ShutUp, Manager.ThisMetaserverId, Manager.GetStatus( ) );
                 }
             else
                 {
                 //Abort. Go to pause
                 Manager.ToPause( );
                 return new BullyMsg( BullyType.ImSmaller, Manager.ThisMetaserverId, Manager.GetStatus( ) );
                 }
             }



        /// <summary>
        /// There is a bigger bully on street
        /// </summary>
        /// <param name="source"></param>
        public override void ShutUpMsgReply(int source)
        {
        Console.WriteLine( "Node "+source+" shutup me" );
            Interlocked.Increment( ref _biggers );
            lock (_waitBiggers)
            {
                Monitor.PulseAll(_waitBiggers);
            }
        }

        public override void ImSmallerMsgReply(int source)
        {
            Console.WriteLine("Server: "+source+" is smaller");
        }

        public override BullyMsg ImBossMsgRequest(int source)
        {
              if ( source < Manager.ThisMetaserverId )
                {
                Console.WriteLine( "Error: The new master (" + source + ") is smaller than a bully server: " + Manager.ThisMetaserverId );
                }
              Manager.ToSlave( source );
             return new BullyMsg( BullyType.AckBoss, Manager.ThisMetaserverId,Manager.GetStatus());
        }

        public override void AckBossMsgReply(int source)
        {
            lock (_waitSlaves)
            {
                Interlocked.Increment(ref _slaveAcks);
                lock (_waitSlaves)
                {
                    Monitor.PulseAll( _waitSlaves );
                }
            }
        }


     }
    }
