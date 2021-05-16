using System;

namespace NeeView
{
    // Immutable
    public sealed class CommandNameSource : IEquatable<CommandNameSource>, IComparable<CommandNameSource>
    {
        public CommandNameSource(string name)
        {
            Name = name;
            Number = 0;
        }

        public CommandNameSource(string name, int number)
        {
            Name = name;
            Number = number;
        }


        public string Name { get; }

        public int Number { get; }

        public string FullName => Number == 0 ? Name : Name + ":" + Number.ToString();

        public bool IsClone => Number != 0;


        #region IEqutable

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CommandNameSource);
        }

        public bool Equals(CommandNameSource p)
        {
            if (p is null)
            {
                return false;
            }

            if (ReferenceEquals(this, p))
            {
                return true;
            }

            return (Name == p.Name) && (Number == p.Number);
        }

        public override int GetHashCode()
        {
            return (Name, Number).GetHashCode();
        }

        public static bool operator ==(CommandNameSource lhs, CommandNameSource rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CommandNameSource lhs, CommandNameSource rhs)
        {
            return !(lhs == rhs);
        }

        #endregion IEqutable

        #region IComparable

        public int CompareTo(CommandNameSource other)
        {
            var result = Name.CompareTo(other.Name);
            if (result != 0) return result;

            return Number.CompareTo(other.Number);
        }

        #endregion IComparable

        public override string ToString()
        {
            return FullName;
        }

        public static CommandNameSource Parse(string name)
        {
            var tokens = name.Split(':');
            if (tokens.Length == 2)
            {
                return new CommandNameSource(tokens[0], int.Parse(tokens[1]));
            }
            else
            {
                return new CommandNameSource(name);
            }
        }


    }
}

