namespace NetHook.Core
{
    public class FrameInstractions
    {
        public byte[] Value { get; }
        public MemoryInstractions MemInstractions { get; }
        public FrameInstractions(byte[] array, MemoryInstractions memInstractions)
        {
            Value = array;
        }

        public int Offset => MemInstractions.GetOffset(this);
    }
}
