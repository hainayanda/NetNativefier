using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nativefier;
using Nativefier.Model;
using NativefierTest.Model;

namespace NativefierTest
{
    [TestClass]
    public class NativefierTest : IDiskCacheDelegate<TestModel>, IMemoryCacheDelegate<TestModel>
    {
        public static Nativefier<TestModel> Nativefier { get; set; }
        
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
        }

        [TestMethod]
        public void TestStoreFourItem()
        {
        }

        [TestMethod]
        public void TestStoreFiveItem()
        {
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
