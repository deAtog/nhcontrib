using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Validator.Constraints;
using NUnit.Framework;

namespace NHibernate.Validator.Tests.ValidatorsTest
{
	[TestFixture]
	public class AssertFalseFixture
	{
		[Test]
		public void IsValid()
		{
			AssertFalseValidator v = new AssertFalseValidator();
			Assert.IsTrue(v.IsValid(false));
			Assert.IsFalse(v.IsValid(null));
			Assert.IsFalse(v.IsValid(true));
			Assert.IsFalse(v.IsValid(new object()));
		}
	}
}
