// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ REPL
 *   REPL and remote code evaluating system provider of YACQ
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
 * All rights reserved.
 * 
 * This file is part of YACQ REPL.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * This code is originally in: https://github.com/takeshik/cs-util-codes/
 * 
 * This code is derivative work of BitArray.cs in Mono runtime library by:
 *   Ben Maurer (bmaurer@users.sourceforge.net)
 *   Marek Safar (marek.safar@gmail.com)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XSpect.Collections
{
    [Serializable()]
    public sealed class BitArray
        : IEnumerable<Boolean>,
          IEquatable<BitArray>,
          IEquatable<IEnumerable<Boolean>>,
          IEquatable<IEnumerable<Byte>>,
          IEquatable<IEnumerable<Int32>>
    {
        [Serializable()]
        private class Enumerator
            : IEnumerator<Boolean>
        {
            private readonly BitArray _bitArray;

            private readonly Int32 _version;

            private Boolean _current;

            private Int32 _index;

            public Enumerator(BitArray bitArray)
            {
                this._index = -1;
                this._bitArray = bitArray;
                this._version = bitArray._version;
            }

            public Boolean Current
            {
                get
                {
                    if (this._index == -1)
                    {
                        throw new InvalidOperationException("Enum not started");
                    }
                    if (this._index >= this._bitArray.Count)
                    {
                        throw new InvalidOperationException("Enum Ended");
                    }

                    return this._current;
                }
            }

            Object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public Boolean MoveNext()
            {
                this.CheckVersion();

                if (this._index < (this._bitArray.Count - 1))
                {
                    this._current = this._bitArray[++this._index];
                    return true;
                }

                this._index = this._bitArray.Count;
                return false;
            }

            public void Reset()
            {
                this.CheckVersion();
                this._index = -1;
            }

            public void Dispose()
            {
            }

            private void CheckVersion()
            {
                if (this._version != this._bitArray._version)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private Int32[] _array;

        private Int32 _length;

        private Int32 _version;

        public BitArray(BitArray bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }

            this._length = bits._length;
            this._array = new Int32[(this._length + 31) / 32];
            if (this._array.Length == 1)
            {
                this._array[0] = bits._array[0];
            }
            else
            {
                Array.Copy(bits._array, this._array, this._array.Length);
            }
        }

        public BitArray(Boolean[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            this._length = values.Length;
            this._array = new Int32[(this._length + 31) / 32];

            for (Int32 i = 0; i < values.Length; ++i)
            {
                this[i] = values[i];
            }
        }

        public BitArray(Byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            this._length = bytes.Length * 8;
            this._array = new Int32[(this._length + 31) / 32];

            for (Int32 i = 0; i < bytes.Length; ++i)
            {
                this.SetByte(i, bytes[i]);
            }
        }

        public BitArray(Int32[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            Int32 arrlen = values.Length;
            this._length = arrlen * 32;
            this._array = new Int32[arrlen];
            Array.Copy(values, this._array, arrlen);
        }

        public BitArray(Int32 length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            this._length = length;
            this._array = new Int32[(this._length + 31) / 32];
        }

        public BitArray(Int32 length, Boolean defaultValue)
            : this(length)
        {
            if (defaultValue)
            {
                for (Int32 i = 0; i < this._array.Length; ++i)
                {
                    this._array[i] = ~0;
                }
            }
        }

        public Boolean this[Int32 index]
        {
            get
            {
                if (index < 0 || index >= this._length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return (this._array[index >> 5] & (1 << (index & 31))) != 0;
            }
            set
            {
                if (index < 0 || index >= this._length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (value)
                {
                    this._array[index >> 5] |= (1 << (index & 31));
                }
                else
                {
                    this._array[index >> 5] &= ~(1 << (index & 31));
                }

                ++this._version;
            }
        }

        public Int32 Count
        {
            get
            {
                return this._length;
            }
            set
            {
                if (this._length == value)
                {
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                // Currently we never shrink the array
                if (value > this._length)
                {
                    Int32 numints = (value + 31) / 32;
                    Int32 oldNumints = (this._length + 31) / 32;
                    if (numints > this._array.Length)
                    {
                        var newArr = new Int32[numints];
                        Array.Copy(this._array, newArr, this._array.Length);
                        this._array = newArr;
                    }
                    else
                    {
                        Array.Clear(this._array, oldNumints, numints - oldNumints);
                    }

                    Int32 mask = this._length % 32;
                    if (mask > 0)
                    {
                        this._array[oldNumints - 1] &= (1 << mask) - 1;
                    }
                }

                // set the internal state
                this._length = value;
                ++this._version;
            }
        }

        public static BitArray operator !(BitArray operand)
        {
            Int32 ints = (operand._length + 31) / 32;
            for (Int32 i = 0; i < ints; ++i)
            {
                operand._array[i] = ~operand._array[i];
            }

            ++operand._version;
            return operand;
        }

        public static BitArray operator &(BitArray left, BitArray right)
        {
            left.CheckOperand(right);

            Int32 ints = (left._length + 31) / 32;
            for (Int32 i = 0; i < ints; ++i)
            {
                left._array[i] &= right._array[i];
            }

            ++left._version;
            return left;
        }

        public static BitArray operator &(BitArray left, Boolean[] right)
        {
            return left & new BitArray(right);
        }

        public static BitArray operator &(BitArray left, Byte[] right)
        {
            return left & new BitArray(right);
        }

        public static BitArray operator &(BitArray left, Int32[] right)
        {
            return left & new BitArray(right);
        }

        public static BitArray operator |(BitArray left, BitArray right)
        {
            left.CheckOperand(right);

            Int32 ints = (left._length + 31) / 32;
            for (Int32 i = 0; i < ints; ++i)
            {
                left._array[i] |= right._array[i];
            }

            ++left._version;
            return left;
        }

        public static BitArray operator |(BitArray left, Boolean[] right)
        {
            return left | new BitArray(right);
        }

        public static BitArray operator |(BitArray left, Byte[] right)
        {
            return left | new BitArray(right);
        }

        public static BitArray operator |(BitArray left, Int32[] right)
        {
            return left | new BitArray(right);
        }

        public static BitArray operator ^(BitArray left, BitArray right)
        {
            left.CheckOperand(right);

            Int32 ints = (left._length + 31) / 32;
            for (Int32 i = 0; i < ints; ++i)
            {
                left._array[i] ^= right._array[i];
            }

            ++left._version;
            return left;
        }

        public static BitArray operator ^(BitArray left, Boolean[] right)
        {
            return left ^ new BitArray(right);
        }

        public static BitArray operator ^(BitArray left, Byte[] right)
        {
            return left ^ new BitArray(right);
        }

        public static BitArray operator ^(BitArray left, Int32[] right)
        {
            return left ^ new BitArray(right);
        }

        public static BitArray operator <<(BitArray left, Int32 right)
        {
            for (int i = left.Count - 1; i >= 0; --i)
            {
                left[i] = i >= right && left[i - right];
            }
            return left;
        }

        public static BitArray operator >>(BitArray left, Int32 right)
        {
            for (int i = 0; i < left.Count; ++i)
            {
                left[i] = i < (left.Count - right) && left[i + right];
            }
            return left;
        }

        public static implicit operator BitArray(System.Collections.BitArray operand)
        {
            var array = new Boolean[operand.Count];
            operand.CopyTo(array, 0);
            return new BitArray(array);
        }

        public static implicit operator System.Collections.BitArray(BitArray operand)
        {
            return new System.Collections.BitArray(operand.ToByteArray());
        }

        public IEnumerator<Boolean> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Boolean Equals(BitArray other)
        {
            return this._array.SequenceEqual(other._array)
                && this.Count == other.Count;
        }

        public Boolean Equals(IEnumerable<Boolean> other)
        {
            return this.SequenceEqual(other);
        }

        public Boolean Equals(IEnumerable<Byte> other)
        {
            return Enumerable.Range(0, (this.Count + 7) / 8)
                .Select(this.GetByte)
                .SequenceEqual(other);
        }

        public Boolean Equals(IEnumerable<Int32> other)
        {
            return this._array.SequenceEqual(other);
        }

        private Byte GetByte(Int32 byteIndex)
        {
            Int32 index = byteIndex / 4;
            Int32 shift = (byteIndex % 4) * 8;

            Int32 theByte = this._array[index] & (0xff << shift);

            return (Byte) ((theByte >> shift) & 0xff);
        }

        private void SetByte(Int32 byteIndex, Byte value)
        {
            Int32 index = byteIndex / 4;
            Int32 shift = (byteIndex % 4) * 8;

            // clear the Byte
            this._array[index] &= ~(0xff << shift);
            // or in the new Byte
            this._array[index] |= value << shift;

            ++this._version;
        }

        private void CheckOperand(BitArray operand)
        {
            if (operand == null)
            {
                throw new ArgumentNullException();
            }
            if (operand._length != this._length)
            {
                throw new ArgumentException();
            }
        }

        public void Clear(Boolean value)
        {
            if (value)
            {
                for (Int32 i = 0; i < this._array.Length; ++i)
                {
                    this._array[i] = ~0;
                }
            }
            else
            {
                Array.Clear(this._array, 0, this._array.Length);
            }

            ++this._version;
        }

        public void CopyTo(Array array, Int32 index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("array", "Array rank must be 1");
            }

            if (index >= array.Length && this._length > 0)
            {
                throw new ArgumentException("index", "index is greater than array.Length");
            }

            // in each case, check to make sure enough space in array

            if (array is Boolean[])
            {
                this.CopyTo((Boolean[]) array, index);
            }
            else if (array is Byte[])
            {
                this.CopyTo((Byte[]) array, index);
            }
            else if (array is Int32[])
            {
                this.CopyTo((Int32[]) array, index);
            }
            else
            {
                throw new ArgumentException("Unsupported type", "array");
            }
        }

        public void CopyTo(Boolean[] array, Int32 index)
        {
            if (array.Length - index < this._length)
            {
                throw new ArgumentException();
            }

            // Copy the bits into the array
            for (Int32 i = 0; i < this._length; ++i)
            {
                array[index + i] = this[i];
            }
        }

        public void CopyTo(Byte[] array, Int32 index)
        {
            Int32 numbytes = (this._length + 7) / 8;

            if ((array.Length - index) < numbytes)
            {
                throw new ArgumentException();
            }

            // Copy the bytes into the array
            for (Int32 i = 0; i < numbytes; ++i)
            {
                array[index + i] = this.GetByte(i);
            }
        }

        public void CopyTo(Int32[] array, Int32 index)
        {
            Array.Copy(this._array, 0, array, index, (this._length + 31) / 32);
        }

        public Boolean[] ToArray()
        {
            var array = new Boolean[this.Count];
            this.CopyTo(array, 0);
            return array;
        }

        public Byte[] ToByteArray()
        {
            var array = new Byte[(this.Count + 7) / 8];
            this.CopyTo(array, 0);
            return array;
        }

        public Int32[] ToInt32Array()
        {
            var array = new Int32[this._array.Length];
            this.CopyTo(array, 0);
            return array;
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
