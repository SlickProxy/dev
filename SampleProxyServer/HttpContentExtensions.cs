namespace SampleProxyServer
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SlickProxyLib;

    public static class HttpContentExtensions
    {
        public static void SaveResponseToFile(this ResponseInspection inspection)
        {
            if (inspection.StatusCode != HttpStatusCode.OK)
                return;

            var result = Task.Run(
                async () =>
                {
                    var uri = new Uri(inspection.To);
                    var path = uri.PathAndQuery.Split('?')[0].Trim();
                    if (string.IsNullOrEmpty(path) || path == "/" || path == "\\" || path.EndsWith("/")|| path.EndsWith("\\"))
                    {
                        path += "/index.html";
                    }
                    else if (!path.Contains("."))
                    {
                        if (inspection.ContentType == "text/html")
                        {
                            path = path + ".html";
                        }
                        else if (inspection.ContentType == "application/json")
                        {
                            path = path + ".json";
                        }
                    }

                    var fullPath = "Z://DownloadSite" + path;
                    var fileObj = new FileInfo(fullPath);
                    if (!Directory.Exists(fileObj.Directory.FullName))
                    {
                        Directory.CreateDirectory(fileObj.Directory.FullName);
                    }

                    await inspection.HttpContent.ReadAsFileAsync(fullPath, true);
                    return true;
                }).Result;
        }
        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite)
        {
            string pathname = Path.GetFullPath(filename);
            if (!overwrite && File.Exists(filename))
            {
                throw new InvalidOperationException(string.Format("File {0} already exists.", pathname));
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    (copyTask) =>
                    {
                        fileStream.Close();
                        if (!filename.EndsWith(".html"))
                            return;

                        string text = File.ReadAllText(filename);
                        text = text.Replace("http://www.abc.com", "");
                        File.WriteAllText(filename, text);
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                throw;
            }
        }
    }
}