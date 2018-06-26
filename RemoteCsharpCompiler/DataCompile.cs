using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RemoteCsharpCompiler
{
    [DataContract] // контракт данных
    class DataCompile // класс для отправки клиенту, представляет результат компиляции
    {
        [DataMember] // член контракта данных
        public byte[] Output { get; set; } // список возможных ошибок

        [DataMember] // член контракта данных
        public List<string> Errors { get; set; } // результат компиляции (исполнимый файл)
    }
}