using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bare.WebServer
{
    /// <summary>
    /// Sessions are associated with the client IP.
    /// </summary>
    public class Session
    {
        public DateTime LastConnection { get; set; }

        /// <summary>
        /// Is the user authenticated?
        /// </summary>
        public bool Authenticated { get; set; }

        /// <summary>
        /// Can be used by controllers to add additional information that needs to persist in the session.
        /// </summary>
        private Dictionary<string, object> Objects { get; set; }

        // Indexer for accessing session objects.  If an object isn't found, null is returned.
        public object this[string objectKey]
        {
            get
            {
                object val = null;
                Objects.TryGetValue(objectKey, out val);

                return val;
            }

            set { Objects[objectKey] = value; }
        }

        /// <summary>
        /// Object collection getter with type conversion.
        /// Note that if the object does not exist in the session, the default value is returned.
        /// Therefore, session objects like "isAdmin" or "isAuthenticated" should always be true for their "yes" state.
        /// </summary>
        public T GetObject<T>(string objectKey)
        {
            object val = null;
            T ret = default(T);

            if (Objects.TryGetValue(objectKey, out val))
            {
                ret = (T)Converter.Convert(val, typeof(T));
            }

            return ret;
        }

        public Session()
        {
            Objects = new Dictionary<string, object>();
            UpdateLastConnectionTime();
        }

        public void UpdateLastConnectionTime()
        {
            LastConnection = DateTime.Now;
        }

        /// <summary>
        /// Returns true if the last request exceeds the specified expiration time in seconds.
        /// </summary>
        public bool IsExpired(int expirationInSeconds)
        {
            return (DateTime.Now - LastConnection).TotalSeconds > expirationInSeconds;
        }

        /// <summary>
        /// De-authorize the session.
        /// </summary>
        public void Expire()
        {
            Authenticated = false;
            // Don't remove the validation token, as we still essentially have a session, we just want the user to log in again.
        }
    }
}
