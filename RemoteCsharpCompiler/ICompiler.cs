using System.ServiceModel;

namespace RemoteCsharpCompiler
{
    [ServiceContract] // контракт службы
    interface ICompiler
    {
        [OperationContract] // контракт операции
        DataCompile Compiler(string source); // метод компилирует строку с исходным 
        // кодом source и возвращает результат в виде объекта типа DataCompile
    }
}