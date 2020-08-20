using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinThumbsPreloader
{
    class DirectoryScanner
    {
        private string path;
        private bool includeNestedDirectories;

        public DirectoryScanner(string path, bool includeNestedDirectories)
        {
            this.path = path;
            this.includeNestedDirectories = includeNestedDirectories;
        }

        public IEnumerable<string> GetItems()
        {
            if (includeNestedDirectories)
            {
                foreach (string item in GetItemsNested()) yield return item;
            }
            else
            {
                foreach (string item in GetItemsOnlyFirstLevel()) yield return item;
            }
        }

        public List<string> GetItemsBulk() {
            List<string> items = new List<string>();
            if (includeNestedDirectories)
            {
                foreach (string item in GetItemsNested()) items.Add(item);
            }
            else
            {
                foreach (string item in GetItemsOnlyFirstLevel()) items.Add(item);
            }
            return items;
        }

        private IEnumerable<string> GetItemsOnlyFirstLevel()
        {
            string[] items = null;
            try
            {
                items = Directory.GetFileSystemEntries(path).ToArray();
            }
            catch (Exception)
            {
                //Do nothing
            }
            if (items != null)
            {
                for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                {
                    yield return items[itemIndex];
                }
            }
        }

        private IEnumerable<string> GetItemsNested()
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            string currentPath;
            while (queue.Count > 0)
            {
                currentPath = queue.Dequeue();
                yield return currentPath;
                string[] files = null;
                try
                {
                    foreach (string subDirectory in Directory.GetDirectories(currentPath)) queue.Enqueue(subDirectory);
                    files = Directory.GetFiles(currentPath);
                }
                catch (Exception)
                {
                    //Do nothing
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++) yield return files[i];
                }
            }
        }

        public IEnumerable<int> GetItemsCount()
        {
            if (includeNestedDirectories)
            {
                foreach (int itemsCount in GetItemsCountNested()) yield return itemsCount;
            }
            else
            {
                foreach (int itemsCount in GetItemsCountOnlyFirstLevel()) yield return itemsCount;
            }
        }

        private IEnumerable<int> GetItemsCountOnlyFirstLevel()
        {
            int itemsCount = 0;
            try
            {
                itemsCount = Directory.GetFileSystemEntries(path).Length;
            }
            catch (Exception)
            {
                //Do nothing
            }
           if (itemsCount > 0) yield return itemsCount;
        }

        private IEnumerable<int> GetItemsCountNested()
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            string currentPath;
            int itemsCount;
            while (queue.Count > 0)
            {
                currentPath = queue.Dequeue();
                itemsCount = 0;
                try
                {
                    foreach (string subDir in Directory.GetDirectories(currentPath))
                    {
                        queue.Enqueue(subDir);
                        itemsCount++;
                    }
                    itemsCount += Directory.GetFiles(currentPath).Length;
                }
                catch (Exception)
                {
                    //Do nothing
                }
                if (itemsCount > 0) yield return itemsCount;
            }
        }
    }
}
