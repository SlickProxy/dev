namespace SampleProxyServer
{
    using SlickProxyLib;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpContentExtensions
    {
        public static void SaveResponsesToFolder(this ResponseInspection inspection, string directory, string defaultFileNameWithExtension, params Tuple<string, string>[] fileTypeFromContentType)
        {
            if (inspection.StatusCode != HttpStatusCode.OK)
                return;

            bool result = Task.Run(
                async () =>
                {
                    var uri = new Uri(inspection.To);
                    string path = uri.PathAndQuery.Split('?')[0].Trim();
                    if (string.IsNullOrEmpty(path) || path == "/" || path == "\\" || path.EndsWith("/") || path.EndsWith("\\"))
                        path += "/" + defaultFileNameWithExtension;
                    else if (!path.Contains("."))
                        foreach (Tuple<string, string> tuple in fileTypeFromContentType)
                            if (tuple.Item1 == inspection.ContentType && !path.ToLower().EndsWith(tuple.Item2.ToLower()))
                                path = path + tuple.Item2;

                    string fullPath = directory + path;
                    var fileObj = new FileInfo(fullPath);
                    if (!Directory.Exists(fileObj.Directory.FullName))
                        Directory.CreateDirectory(fileObj.Directory.FullName);
                    await inspection.HttpResponseMessage.Content.ReadAsFileAsync(fullPath, true);
                    return true;
                }).Result;
        }

        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite)
        {
            string pathname = Path.GetFullPath(filename);
            if (!overwrite && File.Exists(filename))
                throw new InvalidOperationException(string.Format("File {0} already exists.", pathname));

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    copyTask =>
                    {
                        fileStream.Close();
                        if (!filename.EndsWith(".html"))
                            return;

                        string text = File.ReadAllText(filename);
                        text = text.Replace("forums.asp.net", "");
                        File.WriteAllText(filename, text);
                    });
            }
            catch
            {
                if (fileStream != null)
                    fileStream.Close();

                throw;
            }
        }
    }
}