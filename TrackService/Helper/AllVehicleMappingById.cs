using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackService.Helper
{
    public class AllVehiclesMappingById<T>
    {
        private readonly Dictionary<T, string> _connections = new Dictionary<T, string>();
        
        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public bool HasKey(T key)
        {
            return _connections.ContainsKey(key);
        }

        public void Add(T key, string value)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(key))
                    _connections.Add(key, value);
            }
        }

        public List<T> GetAllData()
        {
            List<T> values = _connections.Where(x => x.Value == "ALL").Select(x => x.Key).ToList();
            return values;
        }

        public string GetAllVehiclesId(T key)
        {
            string value;
            if (_connections.TryGetValue(key, out value))
                return value;

            return null;
        }

        public void Remove(T key)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(key))
                    return;

                _connections.Remove(key);
            }
        }
    }
}
