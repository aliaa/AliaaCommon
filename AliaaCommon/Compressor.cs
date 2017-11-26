using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace AliaaCommon
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
            string viewState = request.Form["__VSTATE"];
            byte[] bytes = Convert.FromBase64String(viewState);
            bytes = Decompress(bytes);
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(Convert.ToBase64String(bytes));
        }

        /// <summary>
        /// Call this method on page's "SavePageStateToPersistenceMedium" method
        /// </summary>
        /// <param name="clientScript">ClientScript object of page</param>
        /// <param name="viewState">parameter of "SavePageStateToPersistenceMedium" method</param>
        public static void CompressViewState(ClientScriptManager clientScript, object viewState)
        {
            LosFormatter formatter = new LosFormatter();
            StringWriter writer = new StringWriter();
            formatter.Serialize(writer, viewState);
            string viewStateString = writer.ToString();
            byte[] bytes = Convert.FromBase64String(viewStateString);
            bytes = Compress(bytes);
            clientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes));
        }
    }
}