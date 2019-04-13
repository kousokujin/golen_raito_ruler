using System;

using System.IO;

namespace Golden_Raito_ruler
{
    static class XmlFileIO
    {
        public static File_Status xmlLoad(Type t, string filename, out object obj)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(t);

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    using (StreamReader reader = new System.IO.StreamReader(fs))
                    {
                        try
                        {
                            obj = serializer.Deserialize(reader);
                        }
                        catch (System.InvalidOperationException)
                        {
                            obj = null;
                            //logOutput.writeLog("設定ファイルの読み込みに失敗しました。");
                            return File_Status.read_failed;
                        }
                    }
                }
            }
            catch (IOException)
            {
                //logOutput.writeLog("設定ファイルの読み込みに失敗しました。");
                obj = null;
                return File_Status.read_failed;
            }
            catch (System.Security.SecurityException)
            {
                //logOutput.writeLog("設定ファイルへのアクセス権がありません。");
                obj = null;
                return File_Status.accsess_denyed;
            }

            return File_Status.sucsess;
        }

        public static File_Status xmlSave(Type t, string filename, object obj)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(t);

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        serializer.Serialize(sw, obj);
                    }
                }
            }
            catch (IOException)
            {
                //logOutput.writeLog("設定ファイルの書き込みに失敗しました。");
                return File_Status.save_failed;
            }
            catch (System.Security.SecurityException)
            {
                //logOutput.writeLog("設定ファイルへのアクセス権がありません。");
                return File_Status.accsess_denyed;
            }

            return File_Status.sucsess;
        }
    }

    //ファイルアクセスの成功・失敗
    enum File_Status
    {
        sucsess,
        read_failed,
        save_failed,
        accsess_denyed
    }
}
