using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nativefier;
using Nativefier.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NativefierTest
{
    [TestClass]
    public class NativefierStringSerializerTest : IDiskCacheDelegate<string>, IMemoryCacheDelegate<string>
    {
        public static INativefier<string> Nativefier { get; set; }

        [TestInitialize]
        public void Start()
        {
            if (Nativefier == null)
            {
                Nativefier = new Nativefier<string>(4, "NativefierStringTest")
                {
                    DiskDelegate = this,
                    MemoryDelegate = this,
                    Serializer = new SerializerTest()
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
            var models = new List<string>();
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
            var models = new List<string>();
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

        private string GenerateTestModel()
        {
            return RandomString(50);
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
        public void OnClearDisk(INativefier<string> cache)
        {
            ClearDiskIsSuccess = cache == Nativefier;
        }

        private bool ClearMemoryIsSuccess = false;
        public void OnClearMemory(INativefier<string> cache)
        {
            ClearMemoryIsSuccess = cache == Nativefier;
        }

        private bool RemoveDiskIsSuccess = false;
        private string WhatBeingRemoveFromDisk;
        public void OnRemoveDisk(INativefier<string> cache, string forObj)
        {
            WhatBeingRemoveFromDisk = forObj;
            RemoveDiskIsSuccess = cache == Nativefier;
        }

        private bool RemoveMemoryIsSuccess = false;
        private string WhatBeingRemoveFromMemory;
        public void OnRemoveMemory(INativefier<string> cache, string forObj)
        {
            WhatBeingRemoveFromMemory = forObj;
            RemoveMemoryIsSuccess = cache == Nativefier;
        }
    }

    class SerializerTest : NativefierStringSerializer<string>
    {
        public override Encoding StringEncode => Encoding.UTF8;

        public override string DeserializeFromString(string str)
        {
            return str.Substring(0, str.Length - 10);
        }

        public override string SerializeToString(string obj)
        {
            return obj + "serialized";
        }
    }
}
