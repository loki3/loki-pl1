using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_ParseChars
	{
		[Test]
		public void TestBasic()
		{
			{
				string[] strs = ParseChars.Do("one");
				Assert.AreEqual(1, strs.Length);
				Assert.AreEqual("one", strs[0]);
			}

			{
				string[] strs = ParseChars.Do("one two");
				Assert.AreEqual(2, strs.Length);
				Assert.AreEqual("one", strs[0]);
				Assert.AreEqual("two", strs[1]);
			}

			{
				string[] strs = ParseChars.Do("one two 3 four 5");
				Assert.AreEqual(5, strs.Length);
				Assert.AreEqual("one", strs[0]);
				Assert.AreEqual("two", strs[1]);
				Assert.AreEqual("3", strs[2]);
				Assert.AreEqual("four", strs[3]);
				Assert.AreEqual("5", strs[4]);
			}
		}

		[Test]
		public void TestStrings()
		{
			{
				string[] strs = ParseChars.Do("'one'");
				Assert.AreEqual(3, strs.Length);
				Assert.AreEqual("'", strs[0]);
				Assert.AreEqual("one", strs[1]);
				Assert.AreEqual("'", strs[2]);
			}

			{
				string[] strs = ParseChars.Do("' one '");
				Assert.AreEqual(3, strs.Length);
				Assert.AreEqual("'", strs[0]);
				Assert.AreEqual(" one ", strs[1]);
				Assert.AreEqual("'", strs[2]);
			}

			{
				string[] strs = ParseChars.Do("test ' \tone  ' \"more \" end");
				Assert.AreEqual(8, strs.Length);
				Assert.AreEqual("test", strs[0]);
				Assert.AreEqual("'", strs[1]);
				Assert.AreEqual(" \tone  ", strs[2]);
				Assert.AreEqual("'", strs[3]);
				Assert.AreEqual("\"", strs[4]);
				Assert.AreEqual("more ", strs[5]);
				Assert.AreEqual("\"", strs[6]);
				Assert.AreEqual("end", strs[7]);
			}
		}
	}
}
