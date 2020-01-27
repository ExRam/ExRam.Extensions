// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Reactive
{
    public struct Counting<T>
    {
        public Counting(ulong number, T value)
        {
            Value = value;
            Number = number;
        }

        public T Value { get; }

        public ulong Number { get; }

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
            return obj is Counting<T> counting2 && Number == counting2.Number && Equals(Value, counting2.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)Math.Pow(2, Number) * (int)Math.Pow(3, Value.GetHashCode());
            }
        }
    }
}
