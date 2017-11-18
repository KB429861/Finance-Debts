using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace Finance.Managers
{
    public abstract class FileManager
    {
        public static void WriteFile(string folderName, string fileName, string text)
        {
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!file.DirectoryExists(folderName))
                    file.CreateDirectory(folderName);
                using (
                    var writer =
                        new StreamWriter(new IsolatedStorageFileStream(folderName + "\\" + fileName, FileMode.Create,
                            FileAccess.Write, file)))
                    writer.WriteLine(text);
            }
        }

        public static string ReadFile(string folderName, string fileName)
        {
            using (var file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (
                    var reader =
                        new StreamReader(new IsolatedStorageFileStream(folderName + "\\" + fileName, FileMode.Open,
                            FileAccess.Read, file)))
                    return reader.ReadToEnd();
            }
        }

        public static string ExportTable<T>(IEnumerable<T> list)
        {
            var csv = new StringBuilder();
            const string delimiter = ",";
            const string newline = "\n";
            var type = typeof (T);
            var properties = type.GetProperties();
            foreach (var obj in list)
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj) != null
                        ? Convert.ToString(property.GetValue(obj), CultureInfo.InvariantCulture)
                        : string.Empty;
                    value = value.Replace(',', '.');
                    csv.Append(value).Append(delimiter);
                }
                csv.Append(newline);
            }
            return csv.ToString();
        }

        public static List<T> ImportTable<T>(string text)
        {
            const char delimiter = ',';
            const char newline = '\n';
            var type = typeof (T);
            var properties = type.GetProperties();
            var list = new List<T>();
            var lines = text.Split(newline);
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line) || line == "\r") continue;
                var obj = (T) Activator.CreateInstance(type);
                var columns = line.Split(delimiter);
                var length = columns.Length >= properties.Length ? properties.Length : columns.Length;
                for (var i = 0; i < length; i++)
                {
                    object column;
                    var t = properties[i].PropertyType;
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (Nullable<>))
                    {
                        if (string.IsNullOrEmpty(columns[i]) || columns[i] == "\r")
                            column = t.IsValueType ? Activator.CreateInstance(t) : null;
                        else
                            column = Convert.ChangeType(columns[i], t.GetGenericArguments()[0],
                                CultureInfo.InvariantCulture);
                        properties[i].SetValue(obj, column);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(columns[i]) || columns[i] == "\r")
                            column = t.IsValueType ? Activator.CreateInstance(t) : null;
                        else
                            column = Convert.ChangeType(columns[i], t, CultureInfo.InvariantCulture);
                        properties[i].SetValue(obj, column);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}