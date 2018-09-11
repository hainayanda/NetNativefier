using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nativefier;
using Nativefier.Model;
using NativefierTest.Model;

namespace NativefierTest
{
    [TestClass]
    public class NativefierTest : IDiskCacheDelegate<TestModel>, IMemoryCacheDelegate<TestModel>
    {
        public static INativefier<TestModel> Nativefier { get; set; }
        
        [TestInitialize]
        public void Start()
        {
            if(Nativefier == null)
            {
                Nativefier = new Nativefier<TestModel>(4, "NativefierTest")
                {
                    DiskDelegate = this,
                    MemoryDelegate = this
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
            var models = new List<TestModel>();
            for(int i = 0; i < 4; i++)
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
            for(int i = 0; i < 4; i++)
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
            var models = new List<TestModel>();
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

        private TestModel GenerateTestModel()
        {
            return new TestModel(
                RandomString(50), RandomInt(0, 50), RandomDouble(0, 50), 
                new SubTestModel(
                    RandomString(50), RandomInt(50, 100), RandomDouble(50, 100)));
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int RandomInt(int bottom, int top)
        {
            return random.Next(bottom, top);
        }

        public static double RandomDouble(int bottom, int top)
        {
            return RandomInt(bottom, top);
        }

        // DELEGATE

        private bool ClearDiskIsSuccess = false;
        public void OnClearDisk(INativefier<TestModel> cache)
        {
            ClearDiskIsSuccess = cache == Nativefier;
        }

        private bool ClearMemoryIsSuccess = false;
        public void OnClearMemory(INativefier<TestModel> cache)
        {
            ClearMemoryIsSuccess = cache == Nativefier;
        }

        private bool RemoveDiskIsSuccess = false;
        private TestModel WhatBeingRemoveFromDisk;
        public void OnRemoveDisk(INativefier<TestModel> cache, TestModel forObj)
        {
            WhatBeingRemoveFromDisk = forObj;
            RemoveDiskIsSuccess = cache == Nativefier;
        }

        private bool RemoveMemoryIsSuccess = false;
        private TestModel WhatBeingRemoveFromMemory;
        public void OnRemoveMemory(INativefier<TestModel> cache, TestModel forObj)
        {
            WhatBeingRemoveFromMemory = forObj;
            RemoveMemoryIsSuccess = cache == Nativefier;
        }
    }
}
