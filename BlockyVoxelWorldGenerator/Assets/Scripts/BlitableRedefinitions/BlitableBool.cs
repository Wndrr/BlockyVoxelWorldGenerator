namespace BlitableRedefinitions
{
    public struct BlitableBool
    {
        private readonly byte _value;
        public BlitableBool(bool value) { _value = (byte)(value ? 1 : 0); }
        public static implicit operator BlitableBool(bool value) { return new BlitableBool(value); }
        public static implicit operator bool(BlitableBool value) { return value._value != 0; }
    }
}