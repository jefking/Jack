using System;
using System.Collections.Generic;

using Jack.Core;
using Jack.Core.IO;
using Jack.Core.IO.Storage;

using Jack.Logger;

namespace Jack.Test
{
    /// <summary>
    /// Testor
    /// </summary>
    internal class Testor
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal Testor()
            : base()
        {
            using (var log = new TraceContext()) { }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Run Test Cases
        /// </summary>
        internal void Run()
        {
            using (var log = new TraceContext())
            {
                byte successes = 0
                    , failures = 0
                    , exceptions = 0;
                TestCase[] testcases = new TestCase[] {this.TestTestCasePattern
                    , this.CreateBlock
                    , this.SaveLocalBlock
                    , this.SaveMemoryBlock
                    , this.DeleteLocalBlock
                    , this.DeleteMemoryBlock
                    , this.ManifestStorage
                    , this.LoadServer
                    , this.ConnectToServer
                    , this.SaveFileOnServer
                    , this.ConnectToIsolatedServer
                    //, this.SaveToARetrieveFromB
                };
                log.Debug("Test Cases Starting;total={0}"
                    , testcases.Length);
                foreach (TestCase tc in testcases)
                {
                    try
                    {
                        if (tc())
                        {
                            log.Info("Success;Method={0},Class={1}"
                                , tc.GetInvocationList()[0].Method
                                , tc.GetInvocationList()[0].Target);
                            successes++;
                        }
                        else
                        {
                            log.Warn("Failure;Method={0},Class={1}"
                                , tc.GetInvocationList()[0].Method
                                , tc.GetInvocationList()[0].Target);
                            failures++;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failure Exception;Method={0},Class={1}"
                            , tc.GetInvocationList()[0].Method
                            , tc.GetInvocationList()[0].Target);
                        log.Error(ex);
                        exceptions++;
                    }

                    System.Threading.Thread.Sleep(100);//Sleep Thread
                }
                log.Info("Test Cases Completed;total={0},successes={1},failures={2},exceptions={3}"
                    , testcases.Length
                    , successes
                    , failures
                    , exceptions);
            }
        }

        #region Peers
        /// <summary>
        /// Connect to Isolated Server
        /// </summary>
        /// <remarks>
        /// Is dependant on having Installed service (or seperatly run application on port 15555)
        /// </remarks>
        /// <returns>Successful</returns>
        private bool ConnectToIsolatedServer()
        {
            //Connect from Client
            FileSystemClient client = FileSystemClient.Connect(Environment.MachineName
                , 15555);
            return (null != client
                && client.IsConnected());
        }
        /// <summary>
        /// Save to A, Retrieve from B
        /// </summary>
        /// <remarks>
        /// Is dependant on having Installed service (or seperatly run application on port 15555)
        /// </remarks>
        /// <returns>Successful</returns>
        private bool SaveToARetrieveFromB()
        {
            return false;
        }
        #endregion

        #region Communication
        /// <summary>
        /// Simple Test Case, to log or catch any exceptions
        /// </summary>
        /// <remarks>
        /// Doesn't Test functionality specifically
        /// </remarks>
        /// <returns></returns>
        private bool LoadServer()
        {
            using (ILifetime server = new Jack.Core.Windows.Services.JackService(Constants.ServerPort))
            {
                server.Initialize();
                server.Load();
                server.Unload();
            }
            return true;
        }
        /// <summary>
        /// Connect to Server
        /// </summary>
        /// <returns>Successful</returns>
        private bool ConnectToServer()
        {
            bool rtrn = false;
            using (Jack.Core.ILifetime server = new Jack.Core.Windows.Services.JackService(Constants.ServerPort))
            {
                server.Initialize();
                server.Load();

                //Connect from Client
                FileSystemClient client = FileSystemClient.Connect(Jack.Core.Network.Constants.Localhost
                    , Constants.ServerPort);
                rtrn = client.IsConnected();

                server.Unload();
            }
            return rtrn;
        }
        /// <summary>
        /// Save File On Server
        /// </summary>
        /// <returns>Success</returns>
        private bool SaveFileOnServer()
        {
            bool rtrn = false;
            using (Jack.Core.ILifetime server = new Jack.Core.Windows.Services.JackService(Constants.ServerPort))
            {
                server.Initialize();
                server.Load();

                FileSystemClient client = FileSystemClient.Connect(Jack.Core.Network.Constants.Localhost
                    , Constants.ServerPort);

                if (client.IsConnected())
                {
                    string file = string.Format(@"\\server\location\{0}.jpg"
                        , Guid.NewGuid());

                    byte[] orig = System.IO.File.ReadAllBytes(string.Format(@"{0}\Jack.Test.exe"
                        , FileHelper.BinDirectory));

                    client.Store(file
                        , orig);

                    IFile stored = client.Retrieve(file);

                    if (null != stored)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (IBlock block in stored.DataBlocks)
                        {
                            byteList.AddRange(block.Data);
                        }
                        byte[] fromStore = byteList.ToArray();

                        if (Jack.Core.Utility.Compare(fromStore
                            , orig))
                        {
                            rtrn = true;
                        }
                    }
                }
                server.Unload();
            }
            return rtrn;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Manifest Storage
        /// </summary>
        /// <returns>Success</returns>
        private bool ManifestStorage()
        {
            FileManifest fm = new FileManifest();
            fm.Identifier = Guid.NewGuid();
            fm.UniversalNamingConvention = string.Format(@"\\server\location\{0}.jpg"
                    , Guid.NewGuid());

            VersionManifest vm = new VersionManifest();
            vm.Identifier = Guid.NewGuid();
            fm.Versions.Push(vm);
            fm.Versions.Push(new VersionManifest());
            
            Jack.Core.XML.ManifestXml xml = new Jack.Core.XML.ManifestXml();
            xml.Store(fm);
            xml = null;//Just ensure that it is a clean read from file
            xml = new Jack.Core.XML.ManifestXml();

            IList<FileManifest> mans = xml.ReadAll();
            foreach (FileManifest gFm in mans)
            {
                if (fm.Identifier == gFm.Identifier)
                {
                    foreach (VersionManifest gVm in fm.Versions)
                    {
                        if (gVm.Identifier == vm.Identifier)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Save Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool SaveLocalBlock()
        {
            using (IFiler disk = new Local())
            {
                return this.SaveBlock(disk);
            }
        }
        /// <summary>
        /// Save Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool SaveMemoryBlock()
        {
            using (IFiler mem = new Memory())
            {
                return this.SaveBlock(mem);
            }
        }
        /// <summary>
        /// Save Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool SaveBlock(IFiler filer)
        {
            IList<IBlock> blocks = (List<IBlock>)Block.CreateBlocks(Helper.RandomBlock());
            foreach (Block block in blocks)
            {
                filer.SaveBlock(block.Identifier
                    , block.Data);
            }

            IList<Block> getBlocks = new List<Block>();
            byte[] data;
            foreach (Block block in blocks)
            {
                data = filer.GetBlock(block.Identifier);
                getBlocks.Add(new Block()
                {
                    Identifier = block.Identifier
                    , Data = data
                });
            }

            for (int i = 0; i < getBlocks.Count; i++)
            {
                if (!(Jack.Core.Utility.Compare(blocks[i].Data
                    , getBlocks[i].Data)))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Delete Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool DeleteLocalBlock()
        {
            using (IFiler disk = new Local())
            {
                return this.DeleteBlock(disk);
            }
        }
        /// <summary>
        /// Save Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool DeleteMemoryBlock()
        {
            using (IFiler mem = new Memory())
            {
                return this.DeleteBlock(mem);
            }
        }
        /// <summary>
        /// Save Local Block
        /// </summary>
        /// <returns>Success</returns>
        private bool DeleteBlock(IFiler filer)
        {
            IList<IBlock> blocks = (List<IBlock>)Block.CreateBlocks(Helper.RandomBlock());
            foreach (Block block in blocks)
            {
                filer.SaveBlock(block.Identifier
                    , block.Data);
            }

            foreach (Block block in blocks)
            {
                filer.DeleteBlock(block.Identifier);
            }

            foreach (Block block in blocks)
            {
                if (null != filer.GetBlock(block.Identifier))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Block Creation Test Case
        /// </summary>
        /// <returns></returns>
        private bool CreateBlock()
        {
            bool rtrn = false;
            byte[] orig = System.IO.File.ReadAllBytes(string.Format(@"{0}\Jack.Test.exe"
                , FileHelper.BinDirectory));

            IEnumerable<IBlock> blocks = Block.CreateBlocks(orig);

            if (null != blocks)
            {
                List<byte> byteList = new List<byte>();
                foreach (IBlock block in blocks)
                {
                    byteList.AddRange(block.Data);
                }
                byte[] fromStore = byteList.ToArray();

                if (Jack.Core.Utility.Compare(fromStore
                    , orig))
                {
                    rtrn = true;
                }
            }

            return rtrn;
        }
        #endregion

        #region Generic
        /// <summary>
        /// Test Test Case Pattern
        /// </summary>
        /// <returns></returns>
        private bool TestTestCasePattern()
        {
            return true;
        }
        #endregion
        #endregion
    }
}