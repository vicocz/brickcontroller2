using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BrickController2
{
    internal static class FileWorkaround
    {
        public static async Task<string> ReadAllTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (StreamReader reader = new StreamReader(sourceStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task WriteAllTextAsync(string path, string contents)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrEmpty(contents))
            {
                new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read).Dispose();
                return;
            }

            FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);

            using (var sw = new StreamWriter(stream, Encoding.Unicode))
            {
                await sw.WriteAsync(contents).ConfigureAwait(false);
                await sw.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
