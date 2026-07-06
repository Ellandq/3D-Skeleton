using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Model.Data.Save;

namespace Model.Data.Registry
{
    public class DiffRegistry
    {
        private readonly Dictionary<string, SavedPropChange> _byId = new();
        
        // Events
        public event Action<SavedPropChange> Registered;
        public event Action<SavedPropChange> Unregistered;

        public void Register(SavedPropChange propChange)
        {
            if (propChange == null)
                throw new ArgumentNullException();

            _byId[propChange.data.id] = propChange;
            
            Registered?.Invoke(propChange);
        }
        
        public void Unregister(string id)
        {
            if (!_byId.Remove(id, out var identifier))
                return;

            Unregistered?.Invoke(identifier);
        }
        
        public bool Unregister(SavedPropChange propChange)
        {
            if (propChange == null)
                return false;

            if (!_byId.ContainsKey(propChange.data.id))
                return false;

            Unregister(propChange.data.id);
            return true;
        }
        
        public bool IsRegistered(SavedPropChange propChange)
        {
            return propChange != null &&
                   _byId.TryGetValue(propChange.data.id, out var registered) &&
                   registered == propChange;
        }
        
        [CanBeNull]
        public SavedPropChange Get(string id)
        {
            _byId.TryGetValue(id, out var identifier);
            return identifier;
        }
        
        public bool TryGet(string id, out SavedPropChange identifier)
        {
            return _byId.TryGetValue(id, out identifier);
        }
        
        public IEnumerable<SavedPropChange> GetAll()
        {
            return _byId.Values;
        }
        
        public bool Contains(string id)
        {
            return _byId.ContainsKey(id);
        }

        public void Clear()
        {
            _byId.Clear();
        }
    }
}