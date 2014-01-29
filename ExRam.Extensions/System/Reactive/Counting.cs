// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Reactive
{
    public struct Counting<T>
    {
        private readonly T _value;
        private readonly ulong _number;

        public Counting(ulong number, T value)
        {
            this._value = value;
            this._number = number;
        }

        public T Value
        {
            get 
            {
                return this._value; 
            }
        }

        public ulong Number
        {
            get
            {
                return this._number; 
            }
        }

        public static bool operator ==(Counting<T> counting1, Counting<T> counting2)
        {
            return counting1.Equals(counting2);
        }

        public static bool operator !=(Counting<T> counting1, Counting<T> counting2)
        {
            return !(counting1 == counting2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Counting<T>)
            {
                var counting2 = (Counting<T>)obj;
                return ((this._number == counting2._number) && (object.Equals(this._value, counting2._value)));
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)Math.Pow(2, this._number) * (int)Math.Pow(3, this._value.GetHashCode());
            }
        }
    }
}
