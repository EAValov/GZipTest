using System;
using System.Collections.Generic;
using System.Text;

namespace GZipTest.DataModel.Enums
{
    /// <summary>
    /// Types of files in GZipTest arguments.
    /// </summary>
    public enum GZipTestParametersFileType
    { 
        /// <summary>
        /// File that is going to be processed.
        /// </summary>
        Original,

        /// <summary>
        /// File that is processed.
        /// </summary>
        Result
    }
}
