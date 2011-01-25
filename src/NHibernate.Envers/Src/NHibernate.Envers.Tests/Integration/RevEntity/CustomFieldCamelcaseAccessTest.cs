using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate.Envers.Exceptions;
using NHibernate.Envers.Tests.Entities;
using NHibernate.Envers.Tests.Entities.RevEntity;
using NUnit.Framework;

namespace NHibernate.Envers.Tests.Integration.RevEntity
{
	[TestFixture]
	public class CustomFieldCamelcaseAccessTest : TestBase
	{
		private int id;
		private long timestamp1;
		private long timestamp2;
		private long timestamp3;

		protected override IEnumerable<string> Mappings
		{
			get
			{
				return new[] { "Entities.Mapping.hbm.xml", "Entities.RevEntity.CustomRevEntityFieldCamelcaseAccess.hbm.xml" };
			}
		}

		protected override void Initialize()
		{
			var te = new StrTestEntity { Str = "x" };

			timestamp1 = DateTime.Now.AddSeconds(-1).Ticks;
			using (var tx = Session.BeginTransaction())
			{
				id = (int)Session.Save(te);
				tx.Commit();
			}

			timestamp2 = DateTime.Now.Ticks;
			Thread.Sleep(100);
			using (var tx = Session.BeginTransaction())
			{
				te.Str = "y";
				tx.Commit();
			}
			timestamp3 = DateTime.Now.Ticks;
		}

		[Test, ExpectedException(typeof(RevisionDoesNotExistException))]
		public void TooEarlyTimeStampShouldFireException()
		{
			AuditReader().GetRevisionNumberForDate(new DateTime(timestamp1));
		}

		[Test]
		public void VerifyTimestamps()
		{
			Assert.AreEqual(1, AuditReader().GetRevisionNumberForDate(new DateTime(timestamp2)));
			Assert.AreEqual(2, AuditReader().GetRevisionNumberForDate(new DateTime(timestamp3)));
		}


		[Test]
		public void VerifyDatesForRevisions()
		{
			Assert.AreEqual(1, AuditReader().GetRevisionNumberForDate(AuditReader().GetRevisionDate(1)));
			Assert.AreEqual(2, AuditReader().GetRevisionNumberForDate(AuditReader().GetRevisionDate(2)));
		}

		[Test]
		public void VerifyRevisionsForDates()
		{
			Assert.IsTrue(
				AuditReader().GetRevisionDate(AuditReader().GetRevisionNumberForDate(new DateTime(timestamp2))).Ticks <= timestamp2);
			Assert.IsTrue(
				AuditReader().GetRevisionDate(AuditReader().GetRevisionNumberForDate(new DateTime(timestamp2)) + 1).Ticks > timestamp2);
			Assert.IsTrue(
				AuditReader().GetRevisionDate(AuditReader().GetRevisionNumberForDate(new DateTime(timestamp3))).Ticks <= timestamp3);
		}

		[Test]
		public void VerifyFindRevision()
		{
			var rev1timestamp = AuditReader().FindRevision<CustomRevEntity>(1).CustomTimestamp;
			Assert.IsTrue(rev1timestamp > timestamp1);
			Assert.IsTrue(rev1timestamp <= timestamp2);

			var rev2timestamp = ((CustomRevEntity)AuditReader().FindRevision(typeof(CustomRevEntity), 2)).CustomTimestamp;
			Assert.IsTrue(rev2timestamp > timestamp2);
			Assert.IsTrue(rev2timestamp <= timestamp3);
		}

		[Test]
		public void VerifyRevisionCounts()
		{
			CollectionAssert.AreEquivalent(new[] { 1, 2 }, AuditReader().GetRevisions(typeof(StrTestEntity), id));
		}

		[Test]
		public void VerifyHistoryOfId()
		{
			var ver1 = new StrTestEntity { Id = id, Str = "x" };
			var ver2 = new StrTestEntity { Id = id, Str = "y" };

			Assert.AreEqual(ver1, AuditReader().Find<StrTestEntity>(id, 1));
			Assert.AreEqual(ver2, AuditReader().Find<StrTestEntity>(id, 2));
		}
	}
}