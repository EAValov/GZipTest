using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.DataModel
{
    /// <summary>
    /// Chunk of data to operate.
    /// </summary>
    public class DataBlock
    {
        /// <summary>
        /// Identity.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Data itself.
        /// </summary>
        public byte[] DataBytes { get; set; }

        /// <summary>
        /// <see cref="DataBlock"/>
        /// </summary>
        /// <param name="id"><see cref="DataBytes"/></param>
        /// <param name="data"><see cref="ID"/></param>
        public DataBlock(int id, byte[] data)
        {
            this.ID = id;
            this.DataBytes = data;
        }
    }
}
