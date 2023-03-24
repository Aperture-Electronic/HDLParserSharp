using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace HDLElaborateRoslyn.RoslynDynamic
{
    public class DynamicIdentifierSet : DynamicObject
    {
        private readonly Dictionary<string, (Type type, object value)> items;

        public DynamicIdentifierSet() 
            => items = new Dictionary<string, (Type type, object value)>();

        public DynamicIdentifierSet(Dictionary<string, (Type type, object value)> items) 
            => this.items = items;

        public void AddOrModifyIdentifier<T>(string identifier, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("The value of an identifier can not be a null object");
            }

            if (items.ContainsKey(identifier))
            {
                items[identifier] = (typeof(T), value);
            }
            else
            {
                items.Add(identifier, (typeof(T), value));
            }   
        }

        public T GetIdentifier<T>(string identifier)
        {
            if (items.ContainsKey(identifier))
            {
                (Type type, object value) = items[identifier];
                if (type == typeof(T))
                {
                    return (T)value;
                }

                throw new Exception($"The type of existed identifier ({type}) is not matched of given type ({typeof(T)})");
            }

            throw new Exception($"Can not found the identifier \"{identifier}\" in the set");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (items.ContainsKey(binder.Name))
            {
                result = items[binder.Name].value;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (items.ContainsKey(binder.Name))
            {
                (Type type, object _) = items[binder.Name];
                if (type == value.GetType())
                {
                    items[binder.Name] = (type, value);
                    return true;
                }

                throw new Exception($"The type of existed identifier ({type}) is not matched of given type ({value.GetType()})");
            }
            else
            {
                items.Add(binder.Name, (value.GetType(), value));
                return true;
            }
        }
    }
}
