using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Models
{
    public class CachedData<T>
    {
        public Func<T, T> AutoRefreshFunc { get; set; }

        public DateTime CacheTime { get; private set; }
        public TimeSpan ExpirationDuration { get; private set; }

        private T _data;
        public T Data
        {
            private set
            {
                _data = value;
                CacheTime = DateTime.Now;
            }
            get
            {
                if (IsExpired && AutoRefreshFunc != null)
                    Data = AutoRefreshFunc(_data);
                return _data;
            }
        }

        public CachedData(T Data, TimeSpan ExpirationDuration)
        {
            this.Data = Data;
            this.ExpirationDuration = ExpirationDuration;
        }

        public CachedData(T Data, int expireAfterSeconds)
        {
            this.Data = Data;
            ExpirationDuration = new TimeSpan(0, 0, expireAfterSeconds);
        }

        public bool IsExpired => DateTime.Now > CacheTime.Add(ExpirationDuration);
    }
}
