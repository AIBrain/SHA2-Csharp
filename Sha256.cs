namespace Sha2 {
    /*
     * Copyright (c) 2010 Yuri K. Schlesner
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
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
     * THE SOFTWARE.
     */
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using NUnit.Framework;

    public class Sha256 {
        private static readonly UInt32[] K = { 0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5, 0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174, 0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA, 0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967, 0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85, 0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070, 0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3, 0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2 };
        private readonly UInt32[] _h = { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        private readonly byte[] _pendingBlock = new byte[ 64 ];
        private readonly UInt32[] _uintBuffer = new UInt32[ 16 ];

        private UInt64 _bitsProcessed;

        private bool _closed;
        private uint _pendingBlockOff;

        private static UInt32 ROTL( UInt32 x, byte n ) {
            Assert.True( n < 32 );
            return ( x << n ) | ( x >> ( 32 - n ) );
        }

        private static UInt32 ROTR( UInt32 x, byte n ) {
            Assert.True( n < 32 );
            return ( x >> n ) | ( x << ( 32 - n ) );
        }

        private static UInt32 Ch( UInt32 x, UInt32 y, UInt32 z ) {
            return ( x & y ) ^ ( ( ~x ) & z );
        }

        private static UInt32 Maj( UInt32 x, UInt32 y, UInt32 z ) {
            return ( x & y ) ^ ( x & z ) ^ ( y & z );
        }

        private static UInt32 Sigma0( UInt32 x ) {
            return ROTR( x, 2 ) ^ ROTR( x, 13 ) ^ ROTR( x, 22 );
        }

        private static UInt32 Sigma1( UInt32 x ) {
            return ROTR( x, 6 ) ^ ROTR( x, 11 ) ^ ROTR( x, 25 );
        }

        private static UInt32 sigma0( UInt32 x ) {
            return ROTR( x, 7 ) ^ ROTR( x, 18 ) ^ ( x >> 3 );
        }

        private static UInt32 sigma1( UInt32 x ) {
            return ROTR( x, 17 ) ^ ROTR( x, 19 ) ^ ( x >> 10 );
        }

        private void ProcessBlock( IList<uint> m ) {
            Assert.True( m.Count == 16 );

            // 1. Prepare the message schedule (W[t]):
            var w = new UInt32[ 64 ];
            for ( var t = 0; t < 16; ++t ) {
                w[ t ] = m[ t ];
            }

            for ( var t = 16; t < 64; ++t ) {
                w[ t ] = sigma1( w[ t - 2 ] ) + w[ t - 7 ] + sigma0( w[ t - 15 ] ) + w[ t - 16 ];
            }

            // 2. Initialize the eight working variables with the (i-1)-st hash value:
            UInt32 a = this._h[ 0 ], b = this._h[ 1 ], c = this._h[ 2 ], d = this._h[ 3 ], e = this._h[ 4 ], f = this._h[ 5 ], g = this._h[ 6 ], h = this._h[ 7 ];

            // 3. For t=0 to 63:
            for ( var t = 0; t < 64; ++t ) {
                var t1 = h + Sigma1( e ) + Ch( e, f, g ) + K[ t ] + w[ t ];
                var t2 = Sigma0( a ) + Maj( a, b, c );
                h = g;
                g = f;
                f = e;
                e = d + t1;
                d = c;
                c = b;
                b = a;
                a = t1 + t2;
            }

            // 4. Compute the intermediate hash value H:
            this._h[ 0 ] = a + this._h[ 0 ];
            this._h[ 1 ] = b + this._h[ 1 ];
            this._h[ 2 ] = c + this._h[ 2 ];
            this._h[ 3 ] = d + this._h[ 3 ];
            this._h[ 4 ] = e + this._h[ 4 ];
            this._h[ 5 ] = f + this._h[ 5 ];
            this._h[ 6 ] = g + this._h[ 6 ];
            this._h[ 7 ] = h + this._h[ 7 ];
        }

        public void AddData( byte[] data, uint offset, uint len ) {
            if ( this._closed ) {
                throw new InvalidOperationException( "Adding data to a closed hasher." );
            }

            if ( len == 0 ) {
                return;
            }

            this._bitsProcessed += len * 8;

            while ( len > 0 ) {
                uint amountToCopy;

                if ( len < 64 ) {
                    if ( this._pendingBlockOff + len > 64 ) {
                        amountToCopy = 64 - this._pendingBlockOff;
                    }
                    else {
                        amountToCopy = len;
                    }
                }
                else {
                    amountToCopy = 64 - this._pendingBlockOff;
                }

                Array.Copy( data, offset, this._pendingBlock, this._pendingBlockOff, amountToCopy );
                len -= amountToCopy;
                offset += amountToCopy;
                this._pendingBlockOff += amountToCopy;

                if ( this._pendingBlockOff != 64 ) {
                    continue;
                }
                ToUintArray( this._pendingBlock, this._uintBuffer );
                this.ProcessBlock( this._uintBuffer );
                this._pendingBlockOff = 0;
            }
        }

        public ReadOnlyCollection<byte> GetHash() {
            return ToByteArray( this.GetHashUInt32() );
        }

        public byte[] GetHash256() {
            return ToBytes( this.GetHashUInt32() );
        }

        public ReadOnlyCollection<UInt32> GetHashUInt32() {
            if ( this._closed ) {
                return Array.AsReadOnly( this._h );
            }

            var sizeTemp = this._bitsProcessed;

            this.AddData( new byte[] { 0x80 }, 0, 1 );

            var available_space = 64 - this._pendingBlockOff;

            if ( available_space < 8 ) {
                available_space += 64;
            }

            // 0-initialized
            var padding = new byte[ available_space ];
            // Insert lenght uint64
            for ( uint i = 1; i <= 8; ++i ) {
                padding[ padding.Length - i ] = ( byte )sizeTemp;
                sizeTemp >>= 8;
            }

            this.AddData( padding, 0u, ( uint )padding.Length );

            Assert.True( this._pendingBlockOff == 0 );

            this._closed = true;

            return Array.AsReadOnly( this._h );
        }

        private static void ToUintArray( byte[] src, UInt32[] dest ) {
            for ( uint i = 0, j = 0; i < dest.Length; ++i, j += 4 ) {
                dest[ i ] = ( ( UInt32 )src[ j + 0 ] << 24 ) | ( ( UInt32 )src[ j + 1 ] << 16 ) | ( ( UInt32 )src[ j + 2 ] << 8 ) | src[ j + 3 ];
            }
        }

        private static ReadOnlyCollection<byte> ToByteArray( ICollection<uint> src ) {
            var dest = new byte[ src.Count * 4 ];
            var pos = 0;

            foreach ( var t in src ) {
                dest[ pos++ ] = ( byte )( t >> 24 );
                dest[ pos++ ] = ( byte )( t >> 16 );
                dest[ pos++ ] = ( byte )( t >> 8 );
                dest[ pos++ ] = ( byte )( t );
            }

            return Array.AsReadOnly( dest );
        }

        private static byte[] ToBytes( ICollection<uint> src ) {
            var dest = new byte[ src.Count * sizeof( uint ) ];
            var pos = 0;

            foreach ( var t in src ) {
                dest[ pos++ ] = ( byte )( t >> 24 );
                dest[ pos++ ] = ( byte )( t >> 16 );
                dest[ pos++ ] = ( byte )( t >> 8 );
                dest[ pos++ ] = ( byte )( t );
            }

            return dest;
        }

        public static ReadOnlyCollection<byte> ComputeHash( String data ) {
            var sha = new Sha256();
            var buf = Encoding.UTF8.GetBytes( data );
            sha.AddData( buf, 0, ( uint )buf.Length );
            return sha.GetHash();
        }

        public static byte[] ComputeHash256( String data ) {
            var sha = new Sha256();
            var buf = Encoding.UTF8.GetBytes( data );
            sha.AddData( buf, 0, ( uint )buf.Length );
            return sha.GetHash256();
        }

        public static ReadOnlyCollection<byte> HashFile( Stream fs ) {
            var sha = new Sha256();
            var buf = new byte[ 8196 ];

            uint bytes_read;
            do {
                bytes_read = ( uint )fs.Read( buf, 0, buf.Length );
                if ( bytes_read == 0 ) {
                    break;
                }

                sha.AddData( buf, 0, bytes_read );
            } while ( bytes_read == 8196 );

            return sha.GetHash();
        }
    }
}
