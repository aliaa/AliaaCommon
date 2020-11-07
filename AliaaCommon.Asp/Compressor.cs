using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Web.UI;

namespace AliaaCommon.Asp
{
    public static class Compressor
    {
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            GZipStream gzip = new GZipStream(output,
                              CompressionMode.Compress, true);
            gzip.Write(data, 0, data.Length);
            gzip.Close();
            return output.ToArray();
        }
        
        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream();
            input.Write(data, 0, data.Length);
            input.Position = 0;
            GZipStream gzip = new GZipStream(input,
                              CompressionMode.Decompress, true);
            MemoryStream output = new MemoryStream();
            byte[] buff = new byte[64];
            int read = -1;
            read = gzip.Read(buff, 0, buff.Length);
            while (read > 0)
            {
                output.Write(buff, 0, read);
                read = gzip.Read(buff, 0, buff.Length);
            }
            gzip.Close();
            return output.ToArray();
        }

        /// <summary>
        /// Call this method on page's "LoadPageStateFromPersistenceMedium" method
        /// </summary>
        /// <param name="request">Request object of page</param>
        /// <returns>return this as "LoadPageStateFromPersistenceMedium" method result </returns>
        public static object DecompressViewState(HttpRequest request)
        {
            StringBuilder viewStateBase64 = new StringBuilder();
            int i = 0;
            string viewStateChunk;
            while((viewStateChunk = request.Form["__VSTATE" + i]) != null)
            {
                viewStateBase64.Append(viewStateChunk);
                i++;
            }
            byte[] bytes = Convert.FromBase64String(viewStateBase64.ToString());
            bytes = Decompress(bytes);
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(Convert.ToBase64String(bytes));
        }

        /// <summary>
        /// Call this method on page's "SavePageStateToPersistenceMedium" method
        /// </summary>
        /// <param name="clientScript">ClientScript object of page</param>
        /// <param name="viewState">parameter of "SavePageStateToPersistenceMedium" method</param>
        public static void CompressViewState(ClientScriptManager clientScript, object viewState, int splitSize = 4000)
        {
            LosFormatter formatter = new LosFormatter();
            StringWriter writer = new StringWriter();
            formatter.Serialize(writer, viewState);
            string viewStateString = writer.ToString();
            byte[] bytes = Convert.FromBase64String(viewStateString);
            bytes = Compress(bytes);
            int byteSplitSize = splitSize / 4 * 3;
            for (int i = 0; i <= bytes.Length / byteSplitSize; i++)
            {
                int offset = i * byteSplitSize;
                string base64 = Convert.ToBase64String(bytes, offset, Math.Min(byteSplitSize, bytes.Length - offset));
                clientScript.RegisterHiddenField("__VSTATE"+i, base64);
            }
        }
    }
}