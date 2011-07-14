namespace Nancy
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Nancy.Cookies;

    /// <summary>
    /// Provides strongly-typed access to HTTP request headers.
    /// </summary>
    public class RequestHeaders
    {
        private readonly IDictionary<string, IEnumerable<string>> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestHeaders"/> class.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public RequestHeaders(IDictionary<string, IEnumerable<string>> headers)
        {
            this.headers = new Dictionary<string, IEnumerable<string>>(headers, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Content-types that are acceptable.
        /// </summary>
        public IEnumerable<string> Accept
        {
            get { return this.GetValue("Accept"); }
        }

        /// <summary>
        /// Character sets that are acceptable.
        /// </summary>
        public IEnumerable<string> AcceptCharset
        {
            get { return this.GetValue("Accept-Charset"); }
        }

        /// <summary>
        /// Acceptable encodings.
        /// </summary>
        public IEnumerable<string> AcceptEncoding
        {
            get { return this.GetValue("Accept-Encoding"); }
        }

        /// <summary>
        /// Acceptable languages for response.
        /// </summary>
        public IEnumerable<string> AcceptLanguage
        {
            get { return this.GetValue("Accept-Language"); }
        }

        /// <summary>
        /// Acceptable languages for response.
        /// </summary>
        public string Authorization
        {
            get { return this.GetValue("Authorization", x => x.First()); }
        }

        /// <summary>
        /// Used to specify directives that MUST be obeyed by all caching mechanisms along the request/response chain.
        /// </summary>
        public IEnumerable<string> CacheControl
        {
            get { return this.GetValue("Cache-Control"); }
        }
        
        /// <summary>
        /// Contains name/value pairs of information stored for that URL.
        /// </summary>
        public IEnumerable<INancyCookie> Cookie
        {
            get { return this.GetValue("Cookie", GetNancyCookies); }
        }

        /// <summary>
        /// What type of connection the user-agent would prefer.
        /// </summary>
        public string Connection
        {
            get { return this.GetValue("Connection", x => x.First()); }
        }

        /// <summary>
        /// The length of the request body in octets (8-bit bytes).
        /// </summary>
        public long ContentLength
        {
            get { return this.GetValue("Content-Length", x => Convert.ToInt64(x.First())); }
        }

        /// <summary>
        /// The mime type of the body of the request (used with POST and PUT requests).
        /// </summary>
        public string ContentType
        {
            get { return this.GetValue("Content-Type", x => x.First()); }
        }

        /// <summary>
        /// The date and time that the message was sent.
        /// </summary>
        public DateTime Date
        {
            get { return this.GetValue("Date", x => ParseDateTime(x.First())); }
        }

        /// <summary>
        /// The domain name of the server (for virtual hosting), mandatory since HTTP/1.1
        /// </summary>
        public string Host
        {
            get { return this.GetValue("Host", x => x.First()); }
        }

        /// <summary>
        /// Only perform the action if the client supplied entity matches the same entity on the server. This is mainly for methods like PUT to only update a resource if it has not been modified since the user last updated it.
        /// </summary>
        public IEnumerable<string> IfMatch
        {
            get { return this.GetValue("If-Match"); }
        }

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged
        /// </summary>
        public DateTime IfModifiedSince
        {
            get { return this.GetValue("If-Modified-Since", x => ParseDateTime(x.First())); }
        }

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged
        /// </summary>
        public IEnumerable<string> IfNoneMatch
        {
            get { return this.GetValue("If-None-Match"); }
        }

        /// <summary>
        /// If the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity.
        /// </summary>
        public string IfRange
        {
            get { return this.GetValue("If-Range", x => x.First()); }
        }

        /// <summary>
        /// Only send the response if the entity has not been modified since a specific time.
        /// </summary>
        public DateTime IfUnmodifiedSince
        {
            get { return this.GetValue("If-Unmodified-Since", x => ParseDateTime(x.First())); }
        }

        /// <summary>
        /// Limit the number of times the message can be forwarded through proxies or gateways.
        /// </summary>
        public int MaxForwards
        {
            get { return this.GetValue("Max-Forwards", x => Convert.ToInt32(x.First())); }
        }

        /// <summary>
        /// This is the address of the previous web page from which a link to the currently requested page was followed.
        /// </summary>
        public string Referrer
        {
            get { return this.GetValue("Referer", x => x.First()); }
        }

        /// <summary>
        /// The user agent string of the user agent
        /// </summary>
        public string UserAgent
        {
            get { return this.GetValue("User-Agent", x => x.First()); }
        }

        public IEnumerable<string> this[string name]
        {
            get
            {
                return (this.headers.ContainsKey(name)) ?
                    this.headers[name] :
                    Enumerable.Empty<string>();
            }
        }

        private static object GetDefaultValue(Type T)
        {
            if (IsGenericEnumerable(T))
            {
                var enumerableType = T.GetGenericArguments().First();
                var x = typeof(List<>).MakeGenericType(new[] { enumerableType });
                return Activator.CreateInstance(x);
            }

            if (T.Equals(typeof(DateTime)))
            {
                return DateTime.MinValue;
            }

            return T.Equals(typeof(string)) ?
                string.Empty :
                null;
        }

        private static IEnumerable<INancyCookie> GetNancyCookies(IEnumerable<string> cookies)
        {
            if(cookies == null)
            {
                return Enumerable.Empty<INancyCookie>();
            }

            return from cookie in cookies
                   let pair = cookie.Split('=')
                   select new NancyCookie(pair[0], pair[1]);
        }

        private IEnumerable<string> GetValue(string name)
        {
            return this.GetValue(name, x => x);
        }

        private T GetValue<T>(string name, Func<IEnumerable<string>, T> converter)
        {
            if (!this.headers.ContainsKey(name))
            {
                return (T)(GetDefaultValue(typeof(T)) ?? default(T));
            }

            return converter.Invoke(this.headers[name]);
        }

        private static bool IsGenericEnumerable(Type T)
        {
            return !(T.Equals(typeof(string))) && T.IsGenericType && T.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>));
        }

        private static DateTime ParseDateTime(string value)
        {
            return DateTime.ParseExact(
                value,
                "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
        }

    }
}