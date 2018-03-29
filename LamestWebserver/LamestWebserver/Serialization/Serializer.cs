using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace LamestWebserver.Serialization
{
    /// <summary>
    /// A class that contains various Serialization methods
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Retrieves XML-Serialized data from a file.
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="filename">The name of the file</param>
        /// <returns>The deserialized object</returns>
        public static T ReadXmlData<T>(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));
            
            return ReadXmlDataInMemory<T>(File.ReadAllText(filename));
        }

        /// <summary>
        /// Retrieves XML-Serialized data from a string.
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="xml">The serialized object</param>
        /// <returns>The deserialized object</returns>
        public static T ReadXmlDataInMemory<T>(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            using (MemoryStream memStream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                XmlSerializer serializer = XmlSerializationTools.GetXmlSerializer(typeof(T));
                return (T)serializer.Deserialize(memStream);
            }
        }

        /// <summary>
        /// Writes an Object to an XML-File.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        /// <param name="filename">The name of the file to write</param>
        public static void WriteXmlData<T>(T data, string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename) && !string.IsNullOrWhiteSpace(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            File.WriteAllText(filename, WriteXmlDataInMemory(data));
        }

        /// <summary>
        /// Serializes an Object to an XML-string.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        public static string WriteXmlDataInMemory<T>(T data)
        {
            StringBuilder output = new StringBuilder();

            using (StringWriter textWriter = new StringWriter(output))
            {
                XmlSerializer serializer = XmlSerializationTools.GetXmlSerializer(typeof(T));
                serializer.Serialize(textWriter, data);
            }

            return output.ToString();
        }

        /// <summary>
        /// Retrieves a JSON-Serialized object from a file.
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="filename">The name of the file</param>
        /// <returns>The deserialized object</returns>
        public static T ReadJsonData<T>(string filename) where T : new()
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            return ReadJsonDataInMemory<T>(File.ReadAllText(filename));
        }

        /// <summary>
        /// Retrieves JSON-Serialized data from a json string.
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="json">The serialized Object</param>
        /// <returns>The deserialized object</returns>
        public static T ReadJsonDataInMemory<T>(string json) where T : new()
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            return (T)JsonConvert.DeserializeObject(json, typeof(T));
        }

        /// <summary>
        /// Writes an Object to a JSON-File.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        /// <param name="filename">The name of the file to write</param>
        /// <param name="humanReadable">Shall the file contain linefeeds</param>
        public static void WriteJsonData<T>(T data, string filename, bool humanReadable)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename) && !string.IsNullOrWhiteSpace(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllText(filename, WriteJsonDataInMemory(data, humanReadable));
        }

        /// <summary>
        /// Writes an Object to a json string.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        /// <param name="humanReadable">Shall the file contain linefeeds</param>
        public static string WriteJsonDataInMemory<T>(T data, bool humanReadable)
        {
            return JsonConvert.SerializeObject(data, humanReadable ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Writes an Object to a JSON-File.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        /// <param name="filename">The name of the file to write</param>
        public static void WriteJsonData<T>(T data, string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            WriteJsonData(data, filename, false);
        }

        /// <summary>
        /// Writes an Object to a json string.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        public static string WriteJsonDataInMemory<T>(T data)
        {
            return WriteJsonDataInMemory(data, false);
        }

        /// <summary>
        /// Retrieves Binary-Serialized data from a file.
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="filename">The name of the file</param>
        /// <returns>The deserialized object</returns>
        public static T ReadBinaryData<T>(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            return ReadBinaryDataInMemory<T>(File.ReadAllBytes(filename));
        }

        /// <summary>
        /// Retrieves Binary-Serialized data from a byte[].
        /// </summary>
        /// <typeparam name="T">The Type of the data to deserialize</typeparam>
        /// <param name="data">The serizalized object</param>
        /// <returns>The deserialized object</returns>
        public static T ReadBinaryDataInMemory<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (MemoryStream memStream = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                return (T)formatter.Deserialize(memStream);
            }
        }

        /// <summary>
        /// Writes an Object to a Binary-File.
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        /// <param name="filename">The name of the file to write</param>
        public static void WriteBinaryData<T>(T data, string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename) && !string.IsNullOrWhiteSpace(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllBytes(filename, WriteBinaryDataInMemory(data));
        }

        /// <summary>
        /// Serializes an Object to a byte[].
        /// </summary>
        /// <typeparam name="T">The Type of the Object</typeparam>
        /// <param name="data">The Object</param>
        public static byte[] WriteBinaryDataInMemory<T>(T data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(ms, data);

                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// A class that contains a lot of XmlSerialization helpers
    /// </summary>
    public static class XmlSerializationTools
    {
        private static readonly Hashtable _cachedXmlSerialiazers = new Hashtable();

        /// <summary>
        /// Caches XMLSerializers to prevent MemoryLeaks.
        /// 
        /// Source: http://codereview.stackexchange.com/questions/24861/caching-xmlserializer-in-appdomain &amp; https://msdn.microsoft.com/en-us/library/system.xml.serialization.xmlserializer(v=vs.110).aspx
        /// </summary>
        /// <param name="type">type parameter of the Serializer</param>
        /// <returns>An XML-Serializer created with the given type argument</returns>
        public static XmlSerializer GetXmlSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var key = type;

            var serializer = _cachedXmlSerialiazers[key] as XmlSerializer;

            if (serializer == null)
            {
                serializer = new XmlSerializer(type);
                _cachedXmlSerialiazers[key] = serializer;
            }

            return serializer;
        }

        /// <summary>
        /// Source: http://stackoverflow.com/questions/2441673/reading-xml-with-xmlreader-in-c-sharp
        /// </summary>
        /// <param name="reader">the XMLReader</param>
        /// <param name="elementName">the name of the Element</param>
        /// <returns>All matching elements</returns>
        public static IEnumerable<XElement> GetElementsNamed(this XmlReader reader, string elementName)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (elementName == null)
                throw new ArgumentNullException(nameof(elementName));

            reader.MoveToContent(); // will not advance reader if already on a content node; if successful, ReadState is Interactive
            reader.Read();          // this is needed, even with MoveToContent and ReadState.Interactive
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                // corrected for bug noted by Wes below...
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName))
                {
                    // this advances the reader...so it's either XNode.ReadFrom() or reader.Read(), but not both
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                        yield return matchedElement;
                }
                else
                    reader.Read();
            }
        }

        /// <summary>
        /// Source: http://stackoverflow.com/questions/2441673/reading-xml-with-xmlreader-in-c-sharp
        /// </summary>
        /// <param name="reader">the XMLReader</param>
        /// <param name="elementName">the name of the Element</param>
        /// <returns>The first matching element</returns>
        public static XElement GetElementNamed(this XmlReader reader, string elementName)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (elementName == null)
                throw new ArgumentNullException(nameof(elementName));

            reader.MoveToContent(); // will not advance reader if already on a content node; if successful, ReadState is Interactive
            reader.Read();          // this is needed, even with MoveToContent and ReadState.Interactive
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                // corrected for bug noted by Wes below...
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName))
                {
                    // this advances the reader...so it's either XNode.ReadFrom() or reader.Read(), but not both
                    var matchedElement = XNode.ReadFrom(reader) as XElement;
                    if (matchedElement != null)
                        return matchedElement;
                }
                else
                    reader.Read();
            }

            return null;
        }

        /// <summary>
        /// Writes an object to the xmlWriter
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="writer">the current writer</param>
        /// <param name="name">the name of the object</param>
        /// <param name="value">the value of the object</param>
        public static void WriteElement<T>(this XmlWriter writer, string name, T value)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(name));
            serializer.Serialize(writer, value);
        }

        /// <summary>
        /// searches and reads an object from a xmlReader
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="reader">the current reader</param>
        /// <param name="name">the name of the object</param>
        public static T ReadElement<T>(this XmlReader reader, string name = null)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            while (name != null && reader.Name != name)
            {
                if (!reader.Read())
                    return default(T);
            }

            if (reader.GetAttribute("xsi:nil") == "true")
            {
                return default(T);
            }

            return ReadLowerElement<T>(reader);
        }

        /// <summary>
        /// searches and reads an object from a xmlReader
        /// </summary>
        /// <param name="reader">the current reader</param>
        /// <param name="T">The type of the element</param>
        /// <param name="name">the name of the object</param>
        public static object ReadElement(this XmlReader reader, Type T, string name = null)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            while (name != null && reader.Name != name)
            {
                if (!reader.Read())
                    return Activator.CreateInstance(T);
            }

            if (reader.GetAttribute("xsi:nil") == "true")
            {
                return Activator.CreateInstance(T);
            }

            return ReadLowerElement(reader, T);
        }

        /// <summary>
        /// reads an object from a xmlReader
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="reader">the current reader</param>
        public static T ReadLowerElement<T>(this XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.NodeType == XmlNodeType.Element)
            {
                XmlSerializer serializer;

                serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(reader.Name));

                T ret = (T)serializer.Deserialize(reader);

                return ret;
            }
            else
            {
                reader.Read();

                T ret = (T)reader.ReadContentAs(typeof(T), null);

                reader.Read();

                return ret;
            }
        }

        /// <summary>
        /// reads an object from a xmlReader
        /// </summary>
        /// <param name="reader">the current reader</param>
        /// <param name="T">The type of the element</param>
        public static object ReadLowerElement(this XmlReader reader, Type T)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.NodeType == XmlNodeType.Element)
            {
                XmlSerializer serializer;

                serializer = new XmlSerializer(T, new XmlRootAttribute(reader.Name));

                object ret = serializer.Deserialize(reader);

                return ret;
            }
            else
            {
                reader.Read();

                object ret = reader.ReadContentAs(T, null);

                reader.Read();

                return ret;
            }
        }

        /// <summary>
        /// Reads an XmlReader to a specified EndElement
        /// </summary>
        /// <param name="reader">the XmlReader</param>
        /// <param name="endElement">the name of the EndElement tag</param>
        public static void ReadToEndElement(this XmlReader reader, string endElement)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.Name != null && reader.NodeType == XmlNodeType.EndElement && reader.Name == endElement)
            {
                reader.ReadEndElement();
                return;
            }

            reader.ReadEndElement();
        }
    }
}
