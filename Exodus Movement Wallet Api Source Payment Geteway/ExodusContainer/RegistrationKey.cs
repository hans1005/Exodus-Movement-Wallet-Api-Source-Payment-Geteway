using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akeraiotitasoft.ExodusContainer
{
    public class RegistrationKey
    {
        public Type Type { get; set; }

        public string Name { get; set; }

        public override int GetHashCode()
        {
            int typeHash = (Type?.GetHashCode() ?? 0);
            int nameHash = 0;
            if (!string.IsNullOrEmpty(Name)) nameHash = Name.GetHashCode();
            return typeHash ^ nameHash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RegistrationKey input = obj as RegistrationKey;
            if (input == null) return false;
            bool stillCouldMatch = true;
            if (Type == null)
            {
                if (input.Type != null)
                {
                    stillCouldMatch = false;
                }
            }
            else if (!Type.Equals(input.Type))
            {
                stillCouldMatch = false;
            }
            if (!stillCouldMatch) return false;
            if (string.IsNullOrEmpty(Name))
            {
                if (!string.IsNullOrEmpty(input.Name))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (Name.Equals(input.Name))
            {
                return true;
            }
            return false;
        }
    }
}
