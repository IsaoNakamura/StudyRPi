using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Newtonsoft.Json;

namespace UtilityBasic
{
    public class UtilityJson
    {
        public static int saveToJson<T>(T saveObj, string dirPath, string fileName)
        {
            int result = 0;
            try
            {
                result = saveToJson<T>(saveObj, dirPath, fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public static int saveToJson<T>(T saveObj, string dirPath, string fileName, FileMode mode, FileAccess access, FileShare share)
        {
            int result = 0;
            try
            {
                if (saveObj == null)
                {
                    result = -1;
                    return result;
                }

                if (!Directory.Exists(dirPath))
                {
                    Console.WriteLine("cannot found directory({0}).", dirPath);
                    result = -1;
                    return result;
                }

                // シリアル化
                string buffer = JsonConvert.SerializeObject(saveObj, Formatting.Indented);
                if (buffer == null || buffer.Length <= 0)
                {
                    Console.WriteLine("JsonConvert.SerializeObject() error.");
                    result = -1;
                    return result;
                }

                string filePath = dirPath + "\\" + fileName;

                // ストリームの生成。
                //  既にファイルがある場合、再作成が必要であるためFileMode指定する。
                FileStream fileStream = new FileStream(filePath, mode, access, share);
                if (fileStream == null)
                {
                    Console.WriteLine("failed to create FileStream({0}) instance.", filePath);
                    result = -1;
                    return result;
                }

                // 書き込み
                StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false));
                if (streamWriter == null)
                {
                    Console.WriteLine("failed to create StreamWriter({0}) instance.", filePath);
                    fileStream.Close();
                    result = -1;
                    return result;
                }

                streamWriter.WriteLine(buffer);
                streamWriter.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = -1;
            }
            finally
            {
            }
            return result;
        }

        public static int loadFromJson<T>(out T loadObj, string filePath)
        {
            int result = 0;
            loadObj = default(T);
            try
            {
                result = loadFromJson<T>(out loadObj, filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    loadObj = default(T);
                }
            }
            return result;
        }

        public static int loadFromJson<T>(out T loadObj, string filePath, FileMode mode, FileAccess access, FileShare share)
        {
            int result = 0;
            loadObj = default(T);
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("cannot found file({0}).", filePath);
                    result = -1;
                    return result;
                }

                FileStream fileStream = new FileStream(filePath, mode, access, share);
                if (fileStream == null)
                {
                    result = -1;
                    return result;
                }

                StreamReader streamReader = new StreamReader(fileStream);
                if (streamReader == null)
                {
                    fileStream.Close();
                    result = -1;
                    return result;
                }

                string json = streamReader.ReadToEnd();
                streamReader.Close();
                fileStream.Close();

                // 逆シリアル化
                loadObj = JsonConvert.DeserializeObject<T>(json);
                if (loadObj == null)
                {
                    Console.WriteLine("JsonConvert.DeserializeObject() error.");
                    result = -1;
                    return result;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.Message);
                result = -1;
            }
            finally
            {
                if (result != 0)
                {
                    loadObj = default(T);
                }
            }
            return result;
        }

    }
}
