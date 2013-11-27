using System.Collections.ObjectModel;
using System.Text;

namespace Sha2
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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
    public static class Util
    {
        public static string ArrayToString( this ReadOnlyCollection< byte > arr)
        {
            var s = new StringBuilder(arr.Count * 2);
            foreach ( var t in arr ) {
                s.AppendFormat("{0:x2}", t);
            }

            return s.ToString();
        }
        
        public static string ArrayToString( this byte[] arr)
        {
            var s = new StringBuilder(arr.Length * 2);
            foreach ( var t in arr ) {
                s.AppendFormat("{0:x2}", t);
            }

            return s.ToString();
        }

        public static ReadOnlyCollection<byte> ToByteArray( this ICollection<uint> src ) {
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

        public static byte[] ToBytes( ICollection<uint> src ) {
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

        public static ReadOnlyCollection<byte> ComputeHasher( this String data ) {
            var sha = new Sha256();
            var buf = Encoding.UTF8.GetBytes( data );
            sha.AddData( buf, 0, ( uint )buf.Length );
            return sha.GetHash();
        }

        public static byte[] ComputeHash( this String data ) {
            var sha = new Sha256();
            var buf = Encoding.UTF8.GetBytes( data );
            sha.AddData( buf, 0, ( uint )buf.Length );
            return sha.GetHash256();
        }

        public static ReadOnlyCollection<byte> HashFile( this Stream fs ) {
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
