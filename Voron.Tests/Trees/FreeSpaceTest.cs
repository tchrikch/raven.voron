﻿using System;
using System.IO;
using Xunit;

namespace Voron.Tests.Trees
{
	public class FreeSpaceTest : StorageTest
	{
		[Fact]
		public void WillBeReused()
		{
			var random = new Random();
			var buffer = new byte[512];
			random.NextBytes(buffer);

			Env.FreeSpaceRepository.MinimumFreePagesInSection = 1;

			using (var tx = Env.NewTransaction(TransactionFlags.ReadWrite))
			{
				for (int i = 0; i < 25; i++)
				{
					Env.Root.Add(tx, i.ToString("0000"), new MemoryStream(buffer));
				}

				tx.Commit();
			}
			var before = Env.Stats();

			using (var tx = Env.NewTransaction(TransactionFlags.ReadWrite))
			{
				for (int i = 0; i < 25; i++)
				{
					Env.Root.Delete(tx, i.ToString("0000"));
				}

				tx.Commit();
			}

			var old = Env.NextPageNumber;
			using (var tx = Env.NewTransaction(TransactionFlags.ReadWrite))
			{
				for (int i = 0; i < 25; i++)
				{
					Env.Root.Add(tx, i.ToString("0000"), new MemoryStream(buffer));
				}

				tx.Commit();
			}

			var after = Env.Stats();

			Assert.Equal(after.RootPages, before.RootPages);

			Assert.True(Env.NextPageNumber - old < 2);
		}
	}
}