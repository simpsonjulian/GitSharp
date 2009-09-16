/*
 * Copyright (C) 2008, Google Inc.
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
using GitSharp.Tests.Util;
using GitSharp.TreeWalk.Filter;
using Xunit;

namespace GitSharp.Tests.TreeWalk
{
	public class TreeWalkBasicDiffTest : RepositoryTestCase
	{
		[Fact]
		public void testMissingSubtree_DetectFileAdded_FileModified()
		{
			var ow = new ObjectWriter(db);
			ObjectId aFileId = ow.WriteBlob(Constants.CHARSET.GetBytes("a"));
			ObjectId bFileId = ow.WriteBlob(Constants.CHARSET.GetBytes("b"));
			ObjectId cFileId1 = ow.WriteBlob(Constants.CHARSET.GetBytes("c-1"));
			ObjectId cFileId2 = ow.WriteBlob(Constants.CHARSET.GetBytes("c-2"));

			// Create sub-a/empty, sub-c/empty = hello.
			Func<ObjectId> oldTree = () =>
			                         	{
			                         		var root = new Tree(db);

			                         		Tree subA = root.AddTree("sub-a");
			                         		subA.AddFile("empty").Id = aFileId;
			                         		subA.Id = ow.WriteTree(subA);

			                         		Tree subC = root.AddTree("sub-c");
			                         		subC.AddFile("empty").Id = cFileId1;
			                         		subC.Id = ow.WriteTree(subC);

			                         		return ow.WriteTree(root);
			                         	};

			// Create sub-a/empty, sub-b/empty, sub-c/empty.
			Func<ObjectId> newTree = () =>
			                         	{
			                         		var root = new Tree(db);

			                         		Tree subA = root.AddTree("sub-a");
			                         		subA.AddFile("empty").Id = aFileId;
			                         		subA.Id = ow.WriteTree(subA);

			                         		Tree subB = root.AddTree("sub-b");
			                         		subB.AddFile("empty").Id = bFileId;
			                         		subB.Id = ow.WriteTree(subB);

			                         		Tree subC = root.AddTree("sub-c");
			                         		subC.AddFile("empty").Id = cFileId2;
			                         		subC.Id = ow.WriteTree(subC);

			                         		return ow.WriteTree(root);
			                         	};

			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			tw.reset(new[] { oldTree.Invoke(), newTree.Invoke() });
			tw.Recursive = true;
			tw.setFilter(TreeFilter.ANY_DIFF);

			Assert.True(tw.next());
			Assert.Equal("sub-b/empty", tw.getPathString());
			Assert.Equal(FileMode.Missing, tw.getFileMode(0));
			Assert.Equal(FileMode.RegularFile, tw.getFileMode(1));
			Assert.Equal(ObjectId.ZeroId, tw.getObjectId(0));
			Assert.Equal(bFileId, tw.getObjectId(1));

			Assert.True(tw.next());
			Assert.Equal("sub-c/empty", tw.getPathString());
			Assert.Equal(FileMode.RegularFile, tw.getFileMode(0));
			Assert.Equal(FileMode.RegularFile, tw.getFileMode(1));
			Assert.Equal(cFileId1, tw.getObjectId(0));
			Assert.Equal(cFileId2, tw.getObjectId(1));

			Assert.False(tw.next());
		}
	}
}