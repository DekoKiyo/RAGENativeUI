namespace RAGENativeUI.Memory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 416)]
    internal unsafe struct CAnimPostFX
    {
        [FieldOffset(0x0000)] public uint Name;

        [FieldOffset(0x0008)] public Stack FXStack;


        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct CSimpleArray
        {
            public CAnimPostFX* Offset;
            public short Count;
            public short Size;

            public CAnimPostFX* Get(short index)
            {
                if (index >= Size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"The size of this {nameof(CAnimPostFX)}.{nameof(CSimpleArray)} is {Size}, the index {index} is out of range.");
                }

                return &Offset[index];
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 408)]
        internal unsafe struct Stack
        {
            [FieldOffset(0x0000)] public LayerBlend LayerBlend;
            [FieldOffset(0x0030)] private Layer layersArrayStart;

            [FieldOffset(0x0168)] public int LayersCount;

            public Layer* GetLayer(int index)
            {
                fixed(Layer* array = &layersArrayStart)
                {
                    return &array[index];
                }
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 48)]
        internal unsafe struct LayerBlend
        {
            [FieldOffset(0x0000)] public Layer* LayerA;
            [FieldOffset(0x0008)] public Layer* LayerB;
            [FieldOffset(0x0010)] public float FrequencyNoise;
            [FieldOffset(0x0014)] public float AmplitudeNoise;
            [FieldOffset(0x0018)] public float Frequency;
            [FieldOffset(0x001C)] public float Bias;

            [FieldOffset(0x0024)] public uint LayerAModifierName;
            [FieldOffset(0x0028)] public uint LayerBModifierName;
            [FieldOffset(0x002C)] public byte Disabled;
        }

        [StructLayout(LayoutKind.Explicit, Size = 52)]
        internal unsafe struct Layer
        {
            [FieldOffset(0x0000)] public uint ModifierName;
            [FieldOffset(0x0004)] public uint StartDelayDuration;
            [FieldOffset(0x0008)] public uint InDuration;
            [FieldOffset(0x000C)] public uint HoldDuration;
            [FieldOffset(0x0010)] public uint OutDuration;

            [FieldOffset(0x0024)] public uint AnimMode;

            [FieldOffset(0x002C)] public uint LoopMode;
        }
    }
}
