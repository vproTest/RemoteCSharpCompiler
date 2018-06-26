using System.CodeDom.Compiler;  // содержит типы для управления созданием и компиляцией исходного кода
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RemoteCsharpCompiler
{
    class RemoteCompiler : ICompiler
    {
        public DataCompile Compiler(string source)
        {
            Thread.Sleep(5000); // добавлено время для иммитации длительности выполнения метода
            CompilerParameters parameters = new CompilerParameters(); // создали экземпляр компилятора
            parameters.GenerateExecutable = true; // генерируем исполнимый файл
            parameters.GenerateInMemory = false; 
            parameters.OutputAssembly = "Output.exe"; // имя результирующей сборки 
            parameters.ReferencedAssemblies.Add("System.dll"); // добавление сборок в 
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll"); // проект
            // получаем результат компиляции
            CompilerResults result =  CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameters, source);
            // создали объект, который будет отправлен клиенту, как результат компиляции
            DataCompile dataCompile = new DataCompile();            
            if(result.Errors.Count > 0) // если есть ошибки компиляции
            {
                dataCompile.Errors = new List<string>();
                foreach (CompilerError error in result.Errors)  // сохраняем их в списке для отправки клиенту
                {
                    dataCompile.Errors.Add(string.Format("{0}: {1}; Line: {2}",
                        error.ErrorNumber, error.ErrorText, error.Line));
                }
            }
            else // ошибок нет
            {
                dataCompile.Output = File.ReadAllBytes(parameters.OutputAssembly); // сохраняем байты результата 
                // компиляции (исполнимый файл)
            }
            return dataCompile;           
        }
    }
}