using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfAppClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int _count = 1; // для генерации названий файлов - результатов компиляции на клиенте
        ImageSource _iconStart;
        ImageSource _iconStop;
        bool _isCompilling; // флаг выполнения запроса к службе
        CancellationTokenSource _cancelTokenSource; // источник и токен отмены используется для 
        CancellationToken _token; // подачи запроса на отмену выполнения компиляции        

        public MainWindow()
        {
            InitializeComponent();
            _cancelTokenSource = new CancellationTokenSource();
            _token = _cancelTokenSource.Token;

            this.Closed += (s, e) => _cancelTokenSource.Cancel(); // на событие закрытия окна передаём запрос на отмену
            // это сделано для корректной обработки закрытия окна пользователем во время взаимодействия со службой

            Uri iconUriStart = new Uri("pack://application:,,,/Images/Start.ico", UriKind.RelativeOrAbsolute);
            Uri iconUriStop = new Uri("pack://application:,,,/Images/Stop.ico", UriKind.RelativeOrAbsolute);
            _iconStart = BitmapFrame.Create(iconUriStart);
            _iconStop = BitmapFrame.Create(iconUriStop);           

            ServiceOnline(false, "Служба недоступна"); // первоначально устанавливаем недоступность службы

            Task.Factory.StartNew(() => // в отдельной задаче в бесконечном цикле проверяем доступность службы
            {
                while (true)
                {
                    if (_token.IsCancellationRequested) // запрос на отмену поступит, если пользователь нажмёт
                    { // кнопку закрытия окна, в этом случае приложение завершает свою работу
                        Environment.Exit(1); 
                    }
                    try
                    {
                        string uri = Dispatcher.Invoke(() => TbUri.Text); // получаем uri службы
                        // объект для загрузки метаданных службы
                        MetadataExchangeClient mexClient = new MetadataExchangeClient(new Uri(uri), MetadataExchangeClientMode.HttpGet);
                        MetadataSet metadata = mexClient.GetMetadata(); // пытаемся получить метаданные                    
                        Dispatcher.Invoke(() => ServiceOnline(true, "Служба доступна")); // данные получены, служба доступна
                    }
                    catch (Exception ex) // ошибка 
                    {                            
                        Dispatcher.Invoke(() => ServiceOnline(false, ex.Message)); // служба недоступна
                    }                        
                }
            }, _token /* эта задача может быть отменена при закрытии окна*/);

            TbSource.Text = File.ReadAllText(@"../../WindowsForms.cs"); // исходный код компилируемого файла
        }         
       
        private void ServiceOnline(bool enabled, string message) // метод информирует о доступности/недосупности службы
        {
            MainWindow1.Title = message;
            MainWindow1.Icon = enabled ? _iconStart : _iconStop;
            ButCompileFile.IsEnabled = enabled && !_isCompilling;
        }

        private async void ButCompileFile_ClickAsync(object sender, RoutedEventArgs e) // асинхронный метод компиляции
        {            
            try
            {
                EnableControls(false); // блокируем элементы управления на время взаимодействия со службой

                ServiceReference3.CompilerClient _proxy = new ServiceReference3.CompilerClient(); // получаем сылку на прокси класс службы
                ListBox1.Items.Clear(); // очищаем сообщения об ошибках

                _isCompilling = true; // устанавливаем флаг компиляции
                Progress1.IsIndeterminate = true; // запускаем прогресс-бар
                ServiceReference3.DataCompile result = await _proxy.CompilerAsync(TbSource.Text); // асинхронный вызов метода службы
                ListBox1.Items.Clear(); 

                if (result.Errors != null) // если есть ошибки
                {
                   
                    foreach (string error in result.Errors)
                    {
                        ListBox1.Items.Add(error); // выводим их
                    }
                }
                else // ошибок нет
                {                    
                    ListBox1.Items.Add("Компиляция выполнена успешно. Запуск файла...");
                    string exeFile = string.Format("tmp{0}.exe", _count++); // формируем название исполняемого файла
                    File.WriteAllBytes(exeFile, result.Output); // записываем его на диск
                    Process.Start(exeFile); // запускаем на выполнение
                }               
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                _isCompilling = false; // сбрасываем флаг компиляции              
                Progress1.IsIndeterminate = false; // останавливаем прогресс-бар
                EnableControls(true); // делаем доступными элементы управления
            }
        }

        private void ButSelectFile_Click(object sender, RoutedEventArgs e) // выбор исходного кода для компиляции
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSharp files (*.cs)|*.cs|All files (*.*)|*.*",                
            };

            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbSource.Text = File.ReadAllText(openFileDialog.FileName);
                TbFile.Text = openFileDialog.FileName;
            }
        }

        private void EnableControls(bool enabled) // метод блокировки/разблокировки элементов управления
        {
            ButSelectFile.IsEnabled = enabled;
            TbSource.IsEnabled = enabled;
            TbUri.IsEnabled = enabled;
        }
    }
}