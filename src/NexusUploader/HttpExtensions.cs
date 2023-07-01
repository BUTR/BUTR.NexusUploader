using System.Net.Http;

namespace NexusUploader
{
    public static class HttpExtensions
    {
        public static StringContent ToContent(this int i) => new(i.ToString());

        public static StringContent ToContent(this string s) => new(string.IsNullOrWhiteSpace(s) ? string.Empty : s);

        public static StringContent ToContent(this long l) => new(l.ToString());
    }
}