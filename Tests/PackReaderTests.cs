﻿/*
 * Copyright (C) 2007, Robin Rosenberg <robin.rosenberg@dewire.com>
 * Copyright (C) 2008, Shawn O. Pearce <spearce@spearce.org>
 * Copyright (C) 2009, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using GitSharp.Tests.Util;
using Xunit;

namespace GitSharp.Tests
{
	public class PackReaderTests : RepositoryTestCase
	{
		private const string PackName = "pack-34be9032ac282b11fa9babdc2b2a93ca996c9c2f";
		private static readonly string TestPack = "Resources/" + GitSharp.Transport.IndexPack.GetPackFileName(PackName);
		private static readonly string TestIdx = "Resources/" + GitSharp.Transport.IndexPack.GetIndexFileName(PackName);

		[Fact]
		public void test003_lookupCompressedObject()
		{
			ObjectId id = ObjectId.FromString("902d5476fa249b7abc9d84c611577a81381f0327");
			var pr = new PackFile(TestIdx, TestPack);
			PackedObjectLoader or = pr.Get(new WindowCursor(), id);
			Assert.NotNull(or);
			Assert.Equal(Constants.OBJ_TREE, or.Type);
			Assert.Equal(35, or.Size);
			Assert.Equal(7738, or.DataOffset);
			pr.Close();
		}

		[Fact]
		public void test004_lookupDeltifiedObject()
		{
			ObjectId id = ObjectId.FromString("5b6e7c66c276e7610d4a73c70ec1a1f7c1003259");
			ObjectLoader or = db.OpenObject(id);
			Assert.NotNull(or);
			Assert.True(or is PackedObjectLoader);
			Assert.Equal(Constants.OBJ_BLOB, or.Type);
			Assert.Equal(18009, or.Size);
			Assert.Equal(537, ((PackedObjectLoader)or).DataOffset);
		}

		[Fact]
        public void test005_todopack()
        {
            var todopack = new FileInfo("Resources/todopack");
            if (!todopack.Exists)
            {
                Console.WriteLine("Skipping \"test005_todopack\": no " + todopack.FullName);
                return;
            }

            var packDir = new FileInfo(Path.Combine(db.ObjectsDirectory.FullName, "pack"));
            const string packName = "pack-2e71952edc41f3ce7921c5e5dd1b64f48204cf35";

			new FileInfo(Path.Combine(todopack.FullName, GitSharp.Transport.IndexPack.GetPackFileName(packName)))
				.CopyTo(Path.Combine(packDir.FullName, GitSharp.Transport.IndexPack.GetPackFileName(packName)));

			new FileInfo(Path.Combine(todopack.FullName, GitSharp.Transport.IndexPack.GetIndexFileName(packName)))
				.CopyTo(Path.Combine(packDir.FullName, GitSharp.Transport.IndexPack.GetIndexFileName(packName)));

			Tree t = db.MapTree(ObjectId.FromString("aac9df07f653dd18b935298deb813e02c32d2e6f"));
            Assert.NotNull(t);
            var x = t.MemberCount;

            t = db.MapTree(ObjectId.FromString("6b9ffbebe7b83ac6a61c9477ab941d999f5d0c96"));
            Assert.NotNull(t);
            x = t.MemberCount;
        }
	}
}