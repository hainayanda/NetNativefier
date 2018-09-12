using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nativefier;
using Nativefier.Model;
using NativefierTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NativefierTest
{
    [TestClass]
    public class NativefierBytesSerializerTest : INativefierSerializer<TestModelBytes>, IDiskCacheDelegate<TestModelBytes>, IMemoryCacheDelegate<TestModelBytes>
    {
        public static INativefier<TestModelBytes> Nativefier { get; set; }

        [TestInitialize]
        public void Start()
        {
            if (Nativefier == null)
            {
                Nativefier = new Nativefier<TestModelBytes>(4, "NativefierBytesTest")
                {
                    DiskDelegate = this,
                    MemoryDelegate = this,
                    Serializer = this
                };
            }
        }

        [TestMethod]
        public void TestStoreOneItem()
        {
            var model = GenerateTestModel();
            Nativefier.Put(model, "one");
            var stored = Nativefier["one"];
            Assert.AreEqual(model, stored);
            var asyncResult = Nativefier.AsyncGet("one").Result;
            Assert.AreEqual(model, asyncResult);
            Nativefier.Clear();
            Thread.Sleep(100);
            Assert.IsTrue(ClearMemoryIsSuccess);
            Assert.IsTrue(ClearDiskIsSuccess);
            Thread.Sleep(100);
        }

        [TestMethod]
        public void TestStoreFourItem()
        {
            var models = new List<TestModelBytes>();
            for (int i = 0; i < 4; i++)
            {
                var model = GenerateTestModel();
                Nativefier[i.ToString()] = model;
                Thread.Sleep(100);
                if (i > 1)
                {
                    Assert.IsTrue(RemoveMemoryIsSuccess);
                    Assert.IsTrue(models.Contains(WhatBeingRemoveFromMemory));
                }
                models.Add(model);
            }
            for (int i = 0; i < 4; i++)
            {
                var stored = Nativefier[i.ToString()];
                Assert.IsTrue(models.Contains(stored));
                var asyncResult = Nativefier.AsyncGet(i.ToString()).Result;
                Assert.IsTrue(models.Contains(asyncResult));
            }
            Nativefier.Clear();
            Thread.Sleep(100);
            Assert.IsTrue(ClearMemoryIsSuccess);
            Assert.IsTrue(ClearDiskIsSuccess);
            Thread.Sleep(100);
        }

        [TestMethod]
        public void TestStoreEightItem()
        {
            var models = new List<TestModelBytes>();
            for (int i = 0; i < 8; i++)
            {
                var model = GenerateTestModel();
                Nativefier[i.ToString()] = model;
                Thread.Sleep(100);
                if (i >= 2)
                {
                    Assert.IsTrue(RemoveMemoryIsSuccess);
                    Assert.IsTrue(models.Contains(WhatBeingRemoveFromMemory));
                }
                if (i >= 4)
                {
                    Assert.IsTrue(RemoveDiskIsSuccess);
                    Assert.IsTrue(models.Contains(WhatBeingRemoveFromDisk));
                }
                models.Add(model);
            }
            for (int i = 4; i < 8; i++)
            {
                var stored = Nativefier[i.ToString()];
                Assert.IsTrue(models.Contains(stored));
                var asyncResult = Nativefier.AsyncGet(i.ToString()).Result;
                Assert.IsTrue(models.Contains(asyncResult));
            }
            for (int i = 0; i < 4; i++)
            {
                var shouldNull = Nativefier[i.ToString()];
                Assert.IsNull(shouldNull);
                var asyncNull = Nativefier.AsyncGet(i.ToString()).Result;
                Assert.IsNull(asyncNull);
            }
            Nativefier.Clear();
            Thread.Sleep(100);
            Assert.IsTrue(ClearMemoryIsSuccess);
            Assert.IsTrue(ClearDiskIsSuccess);
            Thread.Sleep(100);
        }

        private TestModelBytes GenerateTestModel()
        {
            return new TestModelBytes
            {
                Content = RandomBytes(10)
            };
        }

        private static Random random = new Random();
        public static byte[] RandomBytes(int length)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }

        // DELEGATE

        private bool ClearDiskIsSuccess = false;
        public void OnClearDisk(INativefier<TestModelBytes> cache)
        {
            ClearDiskIsSuccess = cache == Nativefier;
        }

        private bool ClearMemoryIsSuccess = false;
        public void OnClearMemory(INativefier<TestModelBytes> cache)
        {
            ClearMemoryIsSuccess = cache == Nativefier;
        }

        private bool RemoveDiskIsSuccess = false;
        private TestModelBytes WhatBeingRemoveFromDisk;
        public void OnRemoveDisk(INativefier<TestModelBytes> cache, TestModelBytes forObj)
        {
            WhatBeingRemoveFromDisk = forObj;
            RemoveDiskIsSuccess = cache == Nativefier;
        }

        private bool RemoveMemoryIsSuccess = false;
        private TestModelBytes WhatBeingRemoveFromMemory;
        public void OnRemoveMemory(INativefier<TestModelBytes> cache, TestModelBytes forObj)
        {
            WhatBeingRemoveFromMemory = forObj;
            RemoveMemoryIsSuccess = cache == Nativefier;
        }

        public byte[] Serialize(TestModelBytes obj)
        {
            return obj.Content;
        }

        public TestModelBytes Deserialize(byte[] bytes)
        {
            return new TestModelBytes() { Content = bytes };
        }
    }
  
}
