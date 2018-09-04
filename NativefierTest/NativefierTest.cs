using System;
using System.Linq;
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
                Nativefier = new Nativefier<TestModel>(4, "NativefierTest");
            }
        }

        [TestMethod]
        public void TestStoreOneItem()
        {
            var model = GenerateTestModel();
            Nativefier.Put(model, "one");
        }

        [TestMethod]
        public void TestStoreFourItem()
        {
        }

        [TestMethod]
        public void TestStoreFiveItem()
        {
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
