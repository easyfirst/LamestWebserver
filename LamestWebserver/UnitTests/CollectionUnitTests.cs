﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LamestWebserver;
using LamestWebserver.Collections;
using System.Collections.Generic;
using LamestWebserver.Serialization;
using LamestWebserver.Core;

namespace UnitTests
{
    [TestClass]
    public class CollectionUnitTests
    {
        AVLHashMap<string, string> hashmap;
        AVLTree<string, string> tree;
        QueuedAVLTree<string, string> qtree;

        [Serializable]
        public class Person : IComparable, IEquatable<Person>
        {
            public int age;
            public string name;
            public int CompareTo(object obj) { if (!(obj is Person)) throw new NotImplementedException(); return this.age.CompareTo(((Person)obj).age); }
            public bool Equals(Person other) { return (other.name == name && other.age == age); }
            public override int GetHashCode()
            {
                return (age + name).GetHashCode();
            }
        };

        [Serializable]
        public class Couple : IEquatable<Couple> { public Person man, woman; public bool Equals(Couple other) { return man.Equals(other.man) && woman.Equals(other.woman); } };

        [TestMethod]
        public void TestSerializeClassAvlTree()
        {
            Console.WriteLine("Building Class AVLTree...");

            AVLTree<Person, Couple> tree = new AVLTree<Person, Couple>();
            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                Person a = new Person() { age = i, name = "a" + i };
                Person b = new Person() { age = i, name = "b" + i };
                Couple c = new Couple() { man = a, woman = b };

                tree.Add(a, c);
                tree.Add(b, c);
            }

            Console.WriteLine("Serializing...");
            Serializer.WriteXmlData(tree, "tree");

            Console.WriteLine("Deserializing...");
            tree = Serializer.ReadXmlData<AVLTree<Person, Couple>>("tree");

            Console.WriteLine("Validating...");
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(tree[new Person() { age = i, name = "a" + i }].woman.Equals(new Person() { age = i, name = "b" + i }));
                Assert.IsTrue(tree[new Person() { age = i, name = "b" + i }].woman.Equals(new Person() { age = i, name = "b" + i }));
                Assert.IsTrue(tree[new Person() { age = i, name = "a" + i }].man.Equals(new Person() { age = i, name = "a" + i }));
                Assert.IsTrue(tree[new Person() { age = i, name = "b" + i }].man.Equals(new Person() { age = i, name = "a" + i }));
            }
        }

        [TestMethod]
        public void TestSerializeClassAvlHashMap()
        {
            Console.WriteLine("Building Class AVLHashMap...");

            AVLHashMap<Person, Couple> hashmap = new AVLHashMap<Person, Couple>();
            int count = 1000;

            for (int i = 0; i < count; i++)
            {
                Person a = new Person() { age = i, name = "a" + i };
                Person b = new Person() { age = i, name = "b" + i };
                Couple c = new Couple() { man = a, woman = b };

                hashmap.Add(a, c);
                hashmap.Add(b, c);
            }

            Console.WriteLine("Serializing...");
            Serializer.WriteXmlData(hashmap, "hashmap");

            Console.WriteLine("Deserializing...");
            hashmap = Serializer.ReadXmlData<AVLHashMap<Person, Couple>>("hashmap");

            Console.WriteLine("Validating...");
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(hashmap[new Person() { age = i, name = "a" + i }].woman.Equals(new Person() { age = i, name = "b" + i }));
                Assert.IsTrue(hashmap[new Person() { age = i, name = "b" + i }].woman.Equals(new Person() { age = i, name = "b" + i }));
                Assert.IsTrue(hashmap[new Person() { age = i, name = "a" + i }].man.Equals(new Person() { age = i, name = "a" + i }));
                Assert.IsTrue(hashmap[new Person() { age = i, name = "b" + i }].man.Equals(new Person() { age = i, name = "a" + i }));
            }
        }

        public class ContainerClass
        {
            public string one = "1";
            public AVLTree<Person, Couple> tree = new AVLTree<Person, Couple>();
            public string two = "2";
            public AVLHashMap<Person, Couple> hashmap = new AVLHashMap<Person, Couple>();
            public string three = "3";
        }

        [TestMethod]
        public void TestSerializeMultiple()
        {
            Console.WriteLine("Testing embedded behaviour of AVLTree, AVLHashmap, QueuedAVLTree");

            Person anna = new Person() { name = "Anna", age = 30 };
            Person berta = new Person() { name = "Berta", age = 23 };
            Person carla = new Person() { name = "Carla", age = 26 };
            Person diane = new Person() { name = "Diane", age = 27 };

            ContainerClass container = new ContainerClass();

            container.tree.Add(anna, new Couple() { man = anna, woman = berta });

            container.hashmap.Add(anna, new Couple() { man = anna, woman = berta });
            container.hashmap.Add(berta, new Couple() { man = berta, woman = carla });
            container.hashmap.Add(carla, new Couple() { man = carla, woman = anna });

            Console.WriteLine("Serializing...");
            Serializer.WriteXmlData(container, "container");

            Console.WriteLine("Deserializing...");
            container = null;
            container = Serializer.ReadXmlData<ContainerClass>("container");

            Console.WriteLine("Validating...");

            Assert.IsTrue(container.tree.Count == 1);
            Assert.IsTrue(container.hashmap.Count == 3);

            Assert.IsTrue(container.one == "1");
            Assert.IsTrue(container.two == "2");
            Assert.IsTrue(container.three == "3");

            Assert.IsTrue(container.tree[anna].Equals(new Couple() { man = anna, woman = berta }));

            Assert.IsTrue(container.hashmap[anna].Equals(new Couple() { man = anna, woman = berta }));
            Assert.IsTrue(container.hashmap[berta].Equals(new Couple() { man = berta, woman = carla }));
            Assert.IsTrue(container.hashmap[carla].Equals(new Couple() { man = carla, woman = anna }));
        }

        [TestMethod]
        public void TestAvlHashMaps()
        {
            hashmap = new AVLHashMap<string, string>(1);
            ExecuteTestHashMap();
            hashmap.Clear();
            
            hashmap = new AVLHashMap<string, string>(10);
            ExecuteTestHashMap();
            hashmap.Clear();

            hashmap = new AVLHashMap<string, string>(1024);
            ExecuteTestHashMap();
            hashmap.Clear();
        }

        [TestMethod]
        public void TestAvlTrees()
        {
            tree = new AVLTree<string, string>();
            ExecuteTestTree();
            tree.Clear();
        }

        [TestMethod]
        public void TestQueuedAvlTreesError()
        {
            Console.Write("--- THE BUG HAS BEEN: ");

            var qt = new QueuedAVLTree<string, string>(10);

            for (int i = 0; i < 20; i += 2)
                qt.Add(new KeyValuePair<string, string>(i.ToString("00"), i.ToString("00")));

            Assert.IsTrue(qt.Count == 10);

            for (int i = 1; i < 21; i += 2)
            {
                qt.Add(new KeyValuePair<string, string>(i.ToString("00"), i.ToString("00")));
                Assert.IsTrue(qt[i.ToString("00")] == i.ToString("00"));
                qt.Validate();
                Assert.IsTrue(qt.Count == 10);
            }

            Console.WriteLine("FIXED! --- ");
        }

        [TestMethod]
        public void TestQueuedAvlTrees()
        {
            qtree = new QueuedAVLTree<string, string>(1);
            List<string> hashes = new List<string>();
            List<string> values = new List<string>();

            Assert.IsTrue(qtree.Count == 0);
            qtree.Validate();
            qtree.Add("wolo", "123");
            Assert.AreEqual(qtree["wolo"], "123");
            Assert.IsTrue(qtree.Count == 1);
            Assert.IsTrue(qtree.ContainsKey("wolo"));
            Assert.IsTrue(qtree.Contains(new KeyValuePair<string, string>("wolo", "123")));
            Assert.IsFalse(qtree.ContainsKey("wolo1"));
            Assert.IsFalse(qtree.Contains(new KeyValuePair<string, string>("wolo", "1234")));
            Assert.IsFalse(qtree.Contains(new KeyValuePair<string, string>("wolo1", "123")));
            Assert.IsFalse(qtree.Remove(new KeyValuePair<string, string>("wolo", "1234")));
            Assert.IsFalse(qtree.Remove(new KeyValuePair<string, string>("wolo1", "123")));
            Assert.IsTrue(qtree.Count == 1);
            qtree.Validate();
            qtree.Add("yolo", "0123");
            Assert.AreEqual(qtree["yolo"], "0123");
            Assert.IsTrue(qtree.Count == 1);
            Assert.IsTrue(qtree.ContainsKey("yolo"));
            Assert.IsTrue(qtree.Contains(new KeyValuePair<string, string>("yolo", "0123")));
            Assert.IsFalse(qtree.Contains(new KeyValuePair<string, string>("wolo", "123")));
            Assert.IsFalse(qtree.ContainsKey("wolo"));
            qtree.Validate();
            Assert.IsTrue(qtree.Remove(new KeyValuePair<string, string>("yolo", "0123")));
            Assert.IsTrue(qtree.Count == 0);
            Assert.IsFalse(qtree.Contains(new KeyValuePair<string, string>("yolo", "0123")));
            Assert.IsFalse(qtree.ContainsKey("yolo"));
            qtree.Validate();
            qtree["wolo"] = "abc";
            Assert.IsTrue(qtree.Count == 1);
            Assert.IsTrue(qtree.Remove("wolo"));
            qtree.Clear();
            Assert.IsTrue(qtree.Count == 0);
            qtree.Validate();

            qtree = new QueuedAVLTree<string, string>(10);
            ExecuteTestQueuedTree(10);
            qtree.Clear();

            qtree = new QueuedAVLTree<string, string>(1024);
            ExecuteTestQueuedTree(1024);
            qtree.Clear();
        }

        public void ExecuteTestHashMap()
        {
            List<string> hashes = new List<string>();
            List<string> values = new List<string>();
            const int size = 1000;

            Console.WriteLine("Small Tests...");
            Assert.IsTrue(hashmap.Count == 0);
            hashmap.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(hashmap.ContainsKey("key"));
            Assert.IsFalse(hashmap.ContainsKey("value"));
            Assert.IsTrue(hashmap["key"] == "value");
            Assert.IsTrue(hashmap.Count == 1);
            hashmap.Validate();
            hashmap.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(hashmap.ContainsKey("key"));
            Assert.IsFalse(hashmap.ContainsKey("value"));
            Assert.IsTrue(hashmap["key"] == "value2");
            Assert.IsTrue(hashmap.Count == 1);
            hashmap.Validate();
            hashmap.Add(new KeyValuePair<string, string>("key", "avalue"));
            Assert.IsTrue(hashmap.ContainsKey("key"));
            Assert.IsFalse(hashmap.ContainsKey("value"));
            Assert.IsTrue(hashmap["key"] == "avalue");
            Assert.IsTrue(hashmap.Count == 1);
            hashmap.Validate();
            Assert.IsFalse(hashmap.Remove("value"));
            Assert.IsTrue(hashmap.Remove("key"));
            hashmap.Validate();
            Assert.IsTrue(hashmap.Count == 0);
            hashmap.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(hashmap.Count == 1);
            hashmap.Clear();
            Assert.IsTrue(hashmap.Count == 0);
            hashmap.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(hashmap.ContainsKey("key"));
            Assert.IsFalse(hashmap.ContainsKey("value"));
            Assert.IsTrue(hashmap["key"] == "value");
            Assert.IsTrue(hashmap.Count == 1);
            hashmap.Validate();
            hashmap.Clear();
            Assert.IsFalse(hashmap.Remove(""));
            Assert.IsFalse(hashmap.Remove(new KeyValuePair<string, string>("", "")));

            Console.WriteLine("Adding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(hashmap.Count == i);
                hashes.Add(Hash.GetHash());
                values.Add(Hash.GetHash());
                hashmap[hashes[i]] = values[i];
                Assert.IsTrue(hashmap[hashes[i]] == values[i]);
                Assert.IsTrue(hashmap.Keys.Contains(hashes[i]));
                Assert.IsTrue(hashmap.Values.Contains(values[i]));
                hashmap.Validate();
            }

            Console.WriteLine("Overriding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(hashmap[hashes[i]] == values[i]);
                values[i] = Hash.GetHash();
                hashmap[hashes[i]] = values[i];
                Assert.IsTrue(hashmap.Count == size);
            }

            Console.WriteLine("Checking...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(hashmap[hashes[i]] == values[i]);
                Assert.IsTrue(hashmap.Keys.Contains(hashes[i]));
                Assert.IsTrue(hashmap.Values.Contains(values[i]));
            }

            Console.WriteLine("Validating...");

            hashmap.Validate();

            Serializer.WriteXmlData(hashmap, nameof(hashmap));
            hashmap = Serializer.ReadXmlData<AVLHashMap<string, string>>(nameof(hashmap));

            hashmap.Validate();

            Console.WriteLine("Deleting...");
            
            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(hashmap.Count == size - i);
                Assert.IsTrue(hashmap.ContainsKey(hashes[i]));
                Assert.IsTrue(hashmap[hashes[i]] != default(string));
                Assert.IsTrue(hashmap.Remove(hashes[i]));
                Assert.IsFalse(hashmap.Keys.Contains(hashes[i]));
                Assert.IsFalse(hashmap.Values.Contains(values[i]));

                if (true)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        Assert.IsFalse(hashmap[hashes[j]].Contains(hashes[j]));
                    }

                    for (int j = 0; j < i; j++)
                    {
                        Assert.IsFalse(hashmap.Remove(hashes[j]));
                    }
                }

                Assert.IsTrue(hashmap[hashes[i]] == default(string));
                hashmap.Validate();
            }

            Serializer.WriteXmlData(hashmap, nameof(hashmap));
            hashmap = Serializer.ReadXmlData<AVLHashMap<string, string>>(nameof(hashmap));

            hashmap.Validate();
        }

        public void ExecuteTestTree()
        {
            List<string> hashes = new List<string>();
            List<string> values = new List<string>();
            const int size = 1000;

            Console.WriteLine("Small Tests...");
            Assert.IsTrue(tree.Count == 0);
            tree.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(tree.ContainsKey("key"));
            Assert.IsFalse(tree.ContainsKey("value"));
            Assert.IsTrue(tree["key"] == "value");
            Assert.IsTrue(tree.Count == 1);
            tree.Validate();
            tree.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(tree.ContainsKey("key"));
            Assert.IsFalse(tree.ContainsKey("value"));
            Assert.IsTrue(tree["key"] == "value2");
            Assert.IsTrue(tree.Count == 1);
            tree.Validate();
            tree.Add(new KeyValuePair<string, string>("key", "avalue"));
            Assert.IsTrue(tree.ContainsKey("key"));
            Assert.IsFalse(tree.ContainsKey("value"));
            Assert.IsTrue(tree["key"] == "avalue");
            Assert.IsTrue(tree.Count == 1);
            tree.Validate();
            Assert.IsFalse(tree.Remove("value"));
            Assert.IsTrue(tree.Remove("key"));
            tree.Validate();
            Assert.IsTrue(tree.Count == 0);
            tree.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(tree.Count == 1);
            tree.Clear();
            Assert.IsTrue(tree.Count == 0);
            tree.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(tree.ContainsKey("key"));
            Assert.IsFalse(tree.ContainsKey("value"));
            Assert.IsTrue(tree["key"] == "value");
            Assert.IsTrue(tree.Count == 1);
            tree.Validate();
            tree.Clear();
            Assert.IsFalse(tree.Remove(""));
            Assert.IsFalse(tree.Remove(new KeyValuePair<string, string>("", "")));

            Console.WriteLine("Adding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(tree.Count == i);
                hashes.Add(Hash.GetHash());
                values.Add(Hash.GetHash());
                tree[hashes[i]] = values[i];
                Assert.IsTrue(tree[hashes[i]] == values[i]);
                Assert.IsTrue(tree.Keys.Contains(hashes[i]));
                Assert.IsTrue(tree.Values.Contains(values[i]));
                tree.Validate();
            }

            Console.WriteLine("Overriding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(tree[hashes[i]] == values[i]);
                values[i] = Hash.GetHash();
                tree[hashes[i]] = values[i];
                Assert.IsTrue(tree.Count == size);
            }

            Console.WriteLine("Checking...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(tree[hashes[i]] == values[i]);
                Assert.IsTrue(tree.Keys.Contains(hashes[i]));
                Assert.IsTrue(tree.Values.Contains(values[i]));
            }

            Console.WriteLine("Validating...");

            tree.Validate();

            Console.WriteLine("Deleting...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(tree.Count == size - i);
                Assert.IsTrue(tree.ContainsKey(hashes[i]));
                Assert.IsTrue(tree[hashes[i]] != default(string));
                Assert.IsTrue(tree.Remove(hashes[i]));
                Assert.IsFalse(tree.Keys.Contains(hashes[i]));
                Assert.IsFalse(tree.Values.Contains(values[i]));

                if (true)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        Assert.IsFalse(tree[hashes[j]].Contains(hashes[j]));
                    }

                    for (int j = 0; j < i; j++)
                    {
                        Assert.IsFalse(tree.Remove(hashes[j]));
                    }
                }

                Assert.IsTrue(tree[hashes[i]] == default(string));
                tree.Validate();
            }

            Serializer.WriteXmlData(tree, nameof(tree));
            tree = Serializer.ReadXmlData<AVLTree<string, string>>(nameof(tree));

            tree.Validate();
        }

        public void ExecuteTestQueuedTree(int size)
        {
            List<string> hashes = new List<string>();
            List<string> values = new List<string>();

            Console.WriteLine("Small Tests...");
            Assert.IsTrue(qtree.Count == 0);
            qtree.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(qtree.ContainsKey("key"));
            Assert.IsFalse(qtree.ContainsKey("value"));
            Assert.IsTrue(qtree["key"] == "value");
            Assert.IsTrue(qtree.Count == 1);
            qtree.Validate();
            qtree.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(qtree.ContainsKey("key"));
            Assert.IsFalse(qtree.ContainsKey("value"));
            Assert.IsTrue(qtree["key"] == "value2");
            Assert.IsTrue(qtree.Count == 1);
            qtree.Validate();
            qtree.Add(new KeyValuePair<string, string>("key", "avalue"));
            Assert.IsTrue(qtree.ContainsKey("key"));
            Assert.IsFalse(qtree.ContainsKey("value"));
            Assert.IsTrue(qtree["key"] == "avalue");
            Assert.IsTrue(qtree.Count == 1);
            qtree.Validate();
            Assert.IsFalse(qtree.Remove("value"));
            Assert.IsTrue(qtree.Remove("key"));
            qtree.Validate();
            Assert.IsTrue(qtree.Count == 0);
            qtree.Add(new KeyValuePair<string, string>("key", "value2"));
            Assert.IsTrue(qtree.Count == 1);
            qtree.Clear();
            Assert.IsTrue(qtree.Count == 0);
            qtree.Add(new KeyValuePair<string, string>("key", "value"));
            Assert.IsTrue(qtree.ContainsKey("key"));
            Assert.IsFalse(qtree.ContainsKey("value"));
            Assert.IsTrue(qtree["key"] == "value");
            Assert.IsTrue(qtree.Count == 1);
            qtree.Validate();
            qtree.Clear();
            Assert.IsFalse(qtree.Remove(""));
            Assert.IsFalse(qtree.Remove(new KeyValuePair<string, string>("", "")));

            Console.WriteLine("Adding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree.Count == i);
                hashes.Add(Hash.GetHash());
                values.Add(Hash.GetHash());
                qtree[hashes[i]] = values[i];
                Assert.IsTrue(qtree[hashes[i]] == values[i]);
                Assert.IsTrue(qtree.Keys.Contains(hashes[i]));
                Assert.IsTrue(qtree.Values.Contains(values[i]));
                qtree.Validate();
            }

            Console.WriteLine("Overriding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree[hashes[i]] == values[i]);
                values[i] = Hash.GetHash();
                qtree[hashes[i]] = values[i];
                Assert.IsTrue(qtree.Count == size);
            }

            Console.WriteLine("Checking...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree[hashes[i]] == values[i]);
                Assert.IsTrue(qtree.Keys.Contains(hashes[i]));
                Assert.IsTrue(qtree.Values.Contains(values[i]));
            }

            Console.WriteLine("Validating...");

            qtree.Validate();

            Console.WriteLine("Deleting...");
            
            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree.Count == size - i);
                Assert.IsTrue(qtree.ContainsKey(hashes[i]));
                Assert.IsTrue(qtree[hashes[i]] != default(string));
                Assert.IsTrue(qtree.Remove(hashes[i]));
                Assert.IsFalse(qtree.Keys.Contains(hashes[i]));
                Assert.IsFalse(qtree.Values.Contains(values[i]));

                if (true)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        Assert.IsFalse(qtree[hashes[j]].Contains(hashes[j]));
                    }

                    for (int j = 0; j < i; j++)
                    {
                        Assert.IsFalse(qtree.Remove(hashes[j]));
                    }
                }

                Assert.IsTrue(qtree[hashes[i]] == default(string));
                qtree.Validate();
            }

            hashes.Clear();
            values.Clear();

            Console.WriteLine("Adding...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree.Count == i);
                hashes.Add(Hash.GetHash());
                values.Add(Hash.GetHash());
                qtree[hashes[i]] = values[i];
                Assert.IsTrue(qtree[hashes[i]] == values[i]);
                Assert.IsTrue(qtree.Keys.Contains(hashes[i]));
                Assert.IsTrue(qtree.Values.Contains(values[i]));
                qtree.Validate();
            }

            Console.WriteLine("Overflowing...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree.Count == size);
                hashes.Add(Hash.GetHash());
                values.Add(Hash.GetHash());
                qtree.Validate();
                qtree[hashes[size + i]] = values[size + i];
                qtree.Validate();
                Assert.IsTrue(qtree[hashes[size + i]] == values[size + i]);
                Assert.IsTrue(qtree.Keys.Contains(hashes[size + i]));
                Assert.IsTrue(qtree.Values.Contains(values[size + i]));
                qtree.Validate();
            }

            Console.WriteLine("Overriding...");

            for (int i = 0; i < size; i++)
            {
                values[i] = Hash.GetHash();
                qtree[hashes[size + i]] = values[size + i];
                Assert.IsTrue(qtree[hashes[size + i]] == values[size + i]);
                Assert.IsTrue(qtree.Keys.Contains(hashes[size + i]));
                Assert.IsTrue(qtree.Values.Contains(values[size + i]));
                Assert.IsTrue(qtree.Count == size);
                qtree.Validate();
            }

            Console.WriteLine("Validating...");

            qtree.Validate();

            Console.WriteLine("Deleting...");

            for (int i = 0; i < size; i++)
            {
                Assert.IsTrue(qtree.Count == size - i);
                Assert.IsTrue(qtree.ContainsKey(hashes[size + i]));
                Assert.IsTrue(qtree[hashes[size + i]] != default(string));
                Assert.IsTrue(qtree.Remove(hashes[size + i]));
                Assert.IsFalse(qtree.Keys.Contains(hashes[size + i]));
                Assert.IsFalse(qtree.Values.Contains(values[size + i]));

                if (true)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        Assert.IsFalse(qtree[hashes[size + j]].Contains(hashes[size + j]));
                    }

                    for (int j = 0; j < i; j++)
                    {
                        Assert.IsFalse(qtree.Remove(hashes[size + j]));
                    }
                }

                Assert.IsTrue(qtree[hashes[size + i]] == default(string));
                qtree.Validate();
            }
        }
    }
}
