namespace Sha2Test {
    using System.Linq;
    using NUnit.Framework;
    using Sha2;

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

    internal class Program {
        private static void Main( string[] args ) {

            var test1 = "".ComputeHash().ArrayToString();
            Assert.True( test1.SequenceEqual( "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" ) );

            var test1b = " ".ComputeHash().ArrayToString();
            Assert.True( test1b.SequenceEqual( "36a9e7f1c95b82ffb99743e0c5c4ce95d83c9a430aac59f84ef3cbfab6145068" ) );

            var test2 = "The quick brown fox jumps over the lazy dog".ComputeHash().ArrayToString();
            Assert.True( test2.SequenceEqual( "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592" ) );

            var test3 = "The quick brown fox jumps over the lazy dog.".ComputeHash().ArrayToString();
            Assert.True( test3.SequenceEqual( "ef537f25c895bfa782526529a9b63d97aa631564d5d789c2b765448c8635fb6c" ) );
        }
    }
}
