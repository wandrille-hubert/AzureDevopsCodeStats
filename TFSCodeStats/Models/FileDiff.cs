using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSCodeStats.Models
{
    class FileDiff
    {
        public Block[] blocks { get; set; }
        public Linecharblock[] lineCharBlocks { get; set; }
        public Modifiedfile modifiedFile { get; set; }
        public Originalfile originalFile { get; set; }
    }

    public class Modifiedfile
    {
        public string __type { get; set; }
        public Contentmetadata contentMetadata { get; set; }
        public string serverItem { get; set; }
        public string version { get; set; }
        public string versionDescription { get; set; }
        public object commitId { get; set; }
        public int gitObjectType { get; set; }
        public Objectid objectId { get; set; }
    }

    public class Contentmetadata
    {
        public string contentType { get; set; }
        public int encoding { get; set; }
        public string extension { get; set; }
        public string fileName { get; set; }
        public string vsLink { get; set; }
    }

    public class Objectid
    {
        public string full { get; set; }
        public string _short { get; set; }
    }

    public class Originalfile
    {
        public string __type { get; set; }
        public Contentmetadata1 contentMetadata { get; set; }
        public string serverItem { get; set; }
        public string version { get; set; }
        public string versionDescription { get; set; }
        public object commitId { get; set; }
        public int gitObjectType { get; set; }
        public Objectid1 objectId { get; set; }
    }

    public class Contentmetadata1
    {
        public string contentType { get; set; }
        public int encoding { get; set; }
        public string extension { get; set; }
        public string fileName { get; set; }
        public string vsLink { get; set; }
    }

    public class Objectid1
    {
        public string full { get; set; }
        public string _short { get; set; }
    }

    public class Block
    {
        public int changeType { get; set; }
        public int mLine { get; set; }
        public string[] mLines { get; set; }
        public int mLinesCount { get; set; }
        public int oLine { get; set; }
        public string[] oLines { get; set; }
        public int oLinesCount { get; set; }
        public bool truncatedBefore { get; set; }
        public bool truncatedAfter { get; set; }
    }

    public class Linecharblock
    {
        public object charChange { get; set; }
        public Linechange lineChange { get; set; }
    }

    public class Linechange
    {
        public int changeType { get; set; }
        public int mLine { get; set; }
        public string[] mLines { get; set; }
        public int mLinesCount { get; set; }
        public int oLine { get; set; }
        public string[] oLines { get; set; }
        public int oLinesCount { get; set; }
        public bool truncatedBefore { get; set; }
        public bool truncatedAfter { get; set; }
    }

}
