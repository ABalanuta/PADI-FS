using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;



using SharedLib.Exceptions;
using SharedLib.MetadataObjects;
using SharedLib;



namespace Metadata
    {
    // Middleware to receive clients requests
        internal class MetaCore
        {
            private static String StorageFileLocation = System.Environment.SpecialFolder.ApplicationData.ToString() + "/";

            //ServerState: Ficheiro vs Metadados
            private ConcurrentDictionary<String, MetadataEntry> _metaTable =
                new ConcurrentDictionary<String, MetadataEntry>();

            //Lista de dataservers
            private List<DataserverInfo> _dataServerTable = new List<DataserverInfo>();
       
            //TODO o nome so nao pode ser repetido dentro do mesmo dataserver
            private List<String> _used = new List<string>();


            public MetadataEntry Open(String filename,int clientId)
            {
                Console.WriteLine("Opening file " + filename);

                MetadataEntry entry;
                Mutex locker = new Mutex(false, filename);
                locker.WaitOne();
                Console.WriteLine("Openfile: " + filename);
                Boolean status = _metaTable.TryGetValue(filename, out entry);
                if (status)
                    entry.AddClient(clientId);
                else
                    throw new PadiException(PadiExceptiontType.OpenFile, "Can not open the file: " + filename);
                locker.ReleaseMutex();
                return entry;
            }


            public MetadataEntry Create(String filename, int nbDataServers, int readQuorum, int writeQuorum,int clientId)
            {
                Console.WriteLine("Create file: " + filename);
                MetadataEntry entry;
                Mutex locker = new Mutex( false, filename );
                locker.WaitOne( );
                    if (_metaTable.TryGetValue(filename, out entry))
                    {
                        locker.ReleaseMutex();
                        throw new PadiException(PadiExceptiontType.CreateFile, "File already exists: " + filename);
                    }
                    entry = new MetadataEntry(filename, nbDataServers, readQuorum, writeQuorum);

                    if ( MetadataServer.IsMaster())
                   {
                    //Alocar tantos servidores quanto possivel, o que faltar devolve como falta
                    AllocFileOnDataServer(entry);
                     }

                if (!(_metaTable.TryAdd(entry.FileName, entry)))
                        throw new Exception("Cannot save file: " + filename);
                  locker.ReleaseMutex();
                //TODO  se falhar tens de retirar o alocamento dos Dataservers
                return entry;
            }


       




            public Boolean Delete(String filename,int clientId)
            {
                Console.WriteLine("Try delete: " + filename);
                //Lock the file access until delete it. No one can open it
                MetadataEntry entry;
                Mutex locker = new Mutex(false, filename);
                locker.WaitOne();

                //Exclusive access:
                Boolean status = _metaTable.TryGetValue(filename, out entry);
                if (!status)
                {
                    Console.WriteLine( "File "+filename+" is still open" );
                    locker.ReleaseMutex();
                    return false;
                }
                if (entry.IsOpen())
                {
                   // EventWaitHandle monitor = new EventWaitHandle(false, EventResetMode.AutoReset, "padi"+MetadataServer.ThisMetaserverId+filename + "delete");
                   // locker.ReleaseMutex();
                   // Console.WriteLine("Delete operation locked: " + filename);
                   // monitor.WaitOne();
                   // Console.WriteLine("Delete operation unlocked: " + filename);
                   // status = _metaTable.TryRemove(filename, out entry);
                   // monitor.Set();
                    locker.ReleaseMutex( );
                    Console.WriteLine( "File " + filename + " is still open" );
                    return false;
                }
                Console.WriteLine("File deleted, no user before: " + filename);
                status = _metaTable.TryRemove(filename, out entry);
                if ( status )
                    Console.WriteLine( "File " + filename + " has been deleted" );
                locker.ReleaseMutex( );
                return status;
            }


            /// <summary>
            ///  Remove the entry from read clients
            /// </summary>
            public Boolean Close( String filename, int clientId )
            {
                Console.WriteLine("Try close: " + filename);
                MetadataEntry entry;
                Mutex locker = new Mutex(false, filename);
                locker.WaitOne();

                Console.WriteLine("Go close file");
                Boolean status = _metaTable.TryGetValue(filename, out entry);
                if (status)
                {
                    if (!entry.RemoveClient(clientId))
                    {
                        Console.WriteLine("This client didnt open this file");
                        return false;
                    }
                    //Last client, so auth delete
                    //if (!entry.IsOpen())
                    //{
                    //    Console.WriteLine("File can be delete: " + filename);
                    //    //Allow to delete this file
                    //    if (
                    //        EventWaitHandle.TryOpenExisting(
                    //            "padi" + MetadataServer.ThisMetaserverId + filename + "delete", out lockerDelete))
                    //    {
                    //        lockerDelete.Set();
                    //        //Lock until other thread delete this element to keep the locker mutex, this ensures no other thread will modify it
                    //        lockerDelete.WaitOne();
                    //    }
                    //}
                }
                locker.ReleaseMutex();
                if(status)
                    Console.WriteLine("File has been closed");
                return status;
            }



            public String Dump()
            {
                StringBuilder builder = new StringBuilder("Metadata table: \n");

                foreach (KeyValuePair<String, MetadataEntry> entry in _metaTable)
                {
                    builder.Append(entry.Value.ToString());
                }
                return builder.ToString();
            }





            ////////////////////////////// Dataserver's interface ///////////////
            /// Alloc "serversToAlloc" dataservers to file in entry. 
            private int AllocFileOnDataServer(MetadataEntry entry)
            {
                //A entry tem de estar trancada com o lock do filename
                lock (_dataServerTable)
                {
                    int serversLeft = entry.NBDataServers - entry.ServerFileList.Count;

                    if (serversLeft <= 0)
                        return 0;

                    foreach (DataserverInfo dataserverInfo in _dataServerTable)
                    {
                        if (entry.ServerFileList.ContainsKey(dataserverInfo.IdStruct)) continue;
                        if (serversLeft == 0) return 0;

                        // Generate Fresh LocalNames
                        String localFilename = GenerateLocalFileName(serversLeft);
                        CreateEmptyFileOnDataServer(dataserverInfo.IdStruct.hostname,
                                                    dataserverInfo.IdStruct.port, localFilename);
                        _used.Add(localFilename);
                        entry.ServerFileList.TryAdd(dataserverInfo.IdStruct, localFilename);
                        
                        serversLeft--;
                    }
                    if(serversLeft > 0)
                        Console.WriteLine("Metadata cant alloc enough dataservers : " + entry.FileName + "missing: " +
                                      serversLeft);

                    return serversLeft;
                }
        }

               // Cria um ficheiro vazio no servidor de dados
                private void CreateEmptyFileOnDataServer(string hostname, int port, string localFilename)
            {
                IDataToMeta dateserver = (IDataToMeta)Activator.GetObject(
                    typeof(IDataToMeta), "tcp://" + hostname + ":" + port + "/PADIConnection");

                dateserver.CreateEmptyFile(localFilename);
                //TODO Lidar com falta
            }




            public Boolean Registry(String serverId, String serverIP, int serverPort)
            {
                Console.WriteLine("Tentativa de registo do servidor id: " + serverId + " IP: " + serverIP + ":" +
                                  serverPort);
                lock (_dataServerTable)
                {
                    foreach (DataserverInfo info in _dataServerTable)
                    {
                        if (info.IdStruct.id.Equals(serverId))
                            return false;
                    }
                    _dataServerTable.Add(new DataserverInfo(serverId, serverIP, serverPort));
                }

             if ( MetadataServer.IsMaster())
                {
                //Check if there is some file with dataservers missing
                foreach ( KeyValuePair<String, MetadataEntry> keyValuePair in _metaTable )
                    {
                    Mutex locker = new Mutex( false, keyValuePair.Key );
                    locker.WaitOne( );
                    AllocFileOnDataServer( keyValuePair.Value );
                    locker.ReleaseMutex( );
                    }
                }
                return true;
            }



            ///////////////////////// copy & Load ////////////////////////////////
            public CopyStructMetadata Copy()
            {
                //TODO optimizar para so copiar as diferencas
                CopyStructMetadata dataStruct = new CopyStructMetadata();
                dataStruct.DataServerTable = new List<DataserverInfo>(_dataServerTable.ToArray());
                dataStruct.MetaTable = new ConcurrentDictionary<string, MetadataEntry>(_metaTable);
                dataStruct.Used = new List<string>(_used);
                return dataStruct;
            }

            public void LoadFromStructMetadataCore( CopyStructMetadata dataStruct )
            {
            _dataServerTable = new List<DataserverInfo>( dataStruct.DataServerTable );
            _metaTable = new ConcurrentDictionary<string, MetadataEntry>( dataStruct.MetaTable );
            _used = new List<string>( dataStruct.Used );
            }


            ////////////////////////////////// STORAGE /////////////////////////
            //TODO Refazer
            public  void SaveStateToDisk(MetadataServer server)
            {
            FileStream fs = new FileStream( StorageFileLocation + "metaServer" + MetadataServer.ThisMetaserverId + ".xml",
                                               FileMode.Create);
                BinaryFormatter formater = new BinaryFormatter();
                try
                {
                    formater.Serialize(fs, server);
                    Console.WriteLine("Metadata table written to file");
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize Metatable" + e.Message);
                }
                finally
                {
                    fs.Close();
                }
            }


            public  MetadataServer LoadStateFromDisk(String filename)
            {
                MetadataServer metaServer = null;

                FileStream fs = new FileStream( StorageFileLocation + "metaServer" + MetadataServer.ThisMetaserverId + ".xml",
                                               FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    metaServer = (MetadataServer) formatter.Deserialize(fs);
                    Console.WriteLine("MetadataServer loaded from: " + filename);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Metadata: Failed to deserialize. Reason: " + e.Message);
                }
                finally
                {
                    fs.Close();
                }
                return metaServer;
            }







            public String GenerateLocalFileName(int t)
            {
                Random r = new Random(DateTime.Now.Millisecond + t);
                var data = new byte[16];
                char[] chars = "1234567890ABCDEFGHJKLMNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz".ToCharArray();
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte) chars[r.Next(0, chars.Length)];
                }
                var encoding = new ASCIIEncoding();
                string name = encoding.GetString(data);
                // System.Console.WriteLine("Generated Name: " + name);
                if(_used.Contains( name))
                {
                    System.Console.WriteLine( "# FileLocalname Already in Use Generating Other localname" );
                    return GenerateLocalFileName(t);
                }
                return name;
             }


        }
    }
