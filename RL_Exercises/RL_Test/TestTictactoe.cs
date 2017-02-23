using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RL_Exercises;

namespace RL_Test
{
    [TestClass]
    public class TestTictactoe
    {
        [TestMethod]
        public void testAnyNSumToK()
        {
            var ttt = new Tictactoe();
            Assert.IsTrue(ttt.anyNSumTok(0, 0, null));
            Assert.IsFalse(ttt.anyNSumTok(3, 15, null));
            Assert.IsFalse(ttt.anyNSumTok(0, -1, null));
            Assert.IsFalse(ttt.anyNSumTok(1, 0, null));

            Assert.IsTrue(ttt.anyNSumTok(3, 15, new List<int>() { 2, 9, 4 }));
            Assert.IsFalse(ttt.anyNSumTok(3, 15, new List<int>() { 2, 9, 5 }));
            Assert.IsTrue(ttt.anyNSumTok(3, 15, new List<int>() { 2, 5, 3, 1, 8 }));
            Assert.IsTrue(ttt.anyNSumTok(3, 15, new List<int>() { 2, 3, 7, 6 }));
        }

        [TestMethod]
        public void testCloningLists()
        {
            List<int> l = new List<int>() { 1, 2, 3, 4 };
            List<int> p = l.GetRange(0, l.Count);

            l.RemoveAt(2);
            Assert.IsFalse(l == p);
            Assert.AreEqual(p.Count, 4);

        }
    }
}
