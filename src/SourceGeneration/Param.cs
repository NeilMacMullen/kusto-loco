﻿namespace SourceGeneration
{
    public class Param
    {
        public readonly string Name;
        public readonly string Type;

        public Param(int index, string name, string type)
        {
            ColumnIndex = index;
            Name = name.Trim();
            Type = type.Trim();
        }

        /// <summary>
        ///     The index into the table that is being processed
        /// </summary>
        public int ColumnIndex { get; }

        public bool IsString => Type.Contains("string");

        /// <summary>
        ///     Transforms a Param ColumnIndex
        /// </summary>
        /// <remarks>
        ///     When we have a context parameter at the beginning, we need
        ///     to shift all the columnIndices so that it's ignored
        /// </remarks>
        public Param ShiftIndexLeft() => new Param(ColumnIndex - 1, Name, Type);
    }
}