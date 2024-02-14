using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SchoolHelper.Bot.Structs
{
    public struct Post
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public string Content { get; set; }
        public bool Approved { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }

        public static string CompressContent(string content)
        {
            using var output = new MemoryStream();
            using (var gzip = new DeflateStream(output, CompressionMode.Compress))
            {
                using (var writer = new StreamWriter(gzip, Encoding.UTF8))
                {
                    writer.Write(content);           
                }
            }
            
            var compressedContentBytes = output.ToArray();
            var compressedContent = Convert.ToBase64String(compressedContentBytes);

            return compressedContent;
        }
        
        public static string DecompressContent(string content)
        {
            var bytes = Convert.FromBase64String(content);
            
            using var output = new MemoryStream(bytes);
            using var gzip = new DeflateStream(output, CompressionMode.Decompress);
            using var decStream = new MemoryStream();
            
            gzip.CopyTo(decStream);
            var decompressedBytes = decStream.ToArray();
            return Encoding.UTF8.GetString(decompressedBytes);
        }
    }
}