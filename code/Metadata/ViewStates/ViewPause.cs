using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metadata.ViewStates;
using Metadata.ViewStatus;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStates
    {
        /// <summary>
        /// O servidor parou de receber pedidos. Vai deixar copiar e processeguir o protocolo de bully
        /// </summary>
        public class ViewPause : ViewState
        {

            public ViewPause(MetaViewManager manager)
                : base(manager, ServerStatus.Paused)
            {
                //Aguarda pedidos para que o bully avance
            }
            public override void Start( )
                {

                }

            /// <summary>
            /// Aguardar que o novo rowdy resolva a situacao
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public override BullyMsg NewRowdyMsgRequest( int source )
                {
                BullyMsg resp = new BullyMsg( BullyType.StatusMsg, Manager.ThisMetaserverId,Manager.GetStatus() );
                resp.Status = Status;
                return resp;
                }

            public override void ViewStatusChanged( int lastMaster, long lastId )
            {
                //TODO se for o ultimo tenho de passar a bully
            }

            public delegate void ChangeStateDelegate();
            public void ChangeStateCallback(IAsyncResult ia)
            {
                Console.WriteLine("State changed Async");
            }

            public override BullyMsg AreYouBiggerMsgRequest( int source )
                {
                if ( source < Manager.ThisMetaserverId )
                    {
                    Console.WriteLine( "Got an Are You Bigger from a smaller node: "+source );
                    ChangeStateDelegate del = new ChangeStateDelegate(Manager.ToBully);
                    AsyncCallback callback = new AsyncCallback(ChangeStateCallback);
                    del.BeginInvoke( callback, null );

                    return new BullyMsg( BullyType.ShutUp, Manager.ThisMetaserverId, Manager.GetStatus( ) );
                    }
                else
                    {
                    return new BullyMsg( BullyType.ImSmaller, Manager.ThisMetaserverId, Manager.GetStatus( ) );
                    }
                }

            /// <summary>
            /// Bully solved. New boss elected
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public override BullyMsg ImBossMsgRequest( int source )
            {
                if (source < Manager.ThisMetaserverId)
                {
                    Console.WriteLine("Error: The new master ("+source+") is smaller than a paused server: "+Manager.ThisMetaserverId);
                }
                Manager.ToSlave(source);
                return new BullyMsg( BullyType.AckBoss, Manager.ThisMetaserverId, Manager.GetStatus( ) );
            }


            public override void ShutUpMsgReply(int source)
            {
                Console.WriteLine("WARNING: Paused server got: ShutUpMsg");
             }

            public override void ImSmallerMsgReply(int source)
            {
               Console.WriteLine( "WARNING: Paused server got: ImSmallerMsg" );
            }

            public override void AckBossMsgReply(int source)
            {
                Console.WriteLine( "WARNING: Paused server got: AckBossMsg" );
            }
        }
    }
