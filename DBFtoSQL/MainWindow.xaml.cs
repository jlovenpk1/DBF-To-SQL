using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBFtoSQL
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OdbcConnection odbc; // ODBC коннект, сюда мы кинем наш путь подключения
        private OpenFileDialog openFile; // Работа с проводником для открытия файла
        private SaveFileDialog saveFile; // Работа с проводником для сохранения
        private OdbcCommand _command; // SQL команду пихнем сюда
        private OdbcDataReader _reader; // Для считывания
        private StreamWriter _fileStream; // Для работы с файлами и запись в них
        private Operation _operation;  // Класс, в котором мы заныкали функционал 
        private DataTable dataTable;
        private StringBuilder str;
        private string _select = "SELECT * from "; // Запрос SQL
        private string dbfDirectory = string.Empty; // папка где хранится DBF файл
        private string dbfName = string.Empty; // Имя файла
        private string _namefile = string.Empty; // Имя без .DBF
        private string sqlDirectory = string.Empty; // путь до папки сохранения
        private string sqlName = string.Empty; // имя файла
        private string _columnName = string.Empty; // Имя колонки
        private string _columnType = string.Empty; // Тип колонки(int, varchar, DateTime)
        private string _insertText = string.Empty; // текст Insert операции
        

        public MainWindow()
        {
            InitializeComponent();
            _start.IsEnabled = false; // вырубаем кнопку конвертации
        }

        private void _DBFLoad_Click(object sender, RoutedEventArgs e)
        {
            openFile = new OpenFileDialog(); // создаем экземпляр OpenFileDialog
            if (openFile.ShowDialog() == true) // если окно проводника открыто и файл выбран
            {
                _DBF.Text = openFile.FileName; // загоняем имя файла документа в TextBox 
                dbfDirectory = System.IO.Path.GetDirectoryName(openFile.FileName); // Загоняем путь к файлу в переменую
                dbfName = openFile.SafeFileName; // загоняем имя файла в переменную 
            }
            openFile.Reset(); // обнуляемся
            _namefile = dbfName.Remove(dbfName.IndexOf('.')); // получаем имя файла без расширения
        }

        private void _SQLLoad_Click(object sender, RoutedEventArgs e)
        {
            saveFile = new SaveFileDialog(); // создаем экземпляр SaveFileDialog
            saveFile.FileName = _namefile+".sql"; // Имя сохраняемого файла  = Имя DBF файла + расширение *.SQL
            if(saveFile.ShowDialog() == true) // окно сохранения все дела
            {
                _SQL.Text = saveFile.FileName; // сохраняем в TextBox путь к сохраняемому файлу
                sqlDirectory = System.IO.Path.GetDirectoryName(saveFile.FileName); // путь к файлу 
                sqlName = "\\" + saveFile.SafeFileName; // имя файла
            }
            saveFile.Reset(); // обнуляемся 
            if(dbfName != null && sqlName != null) // если данные не пустые
            {
                _start.IsEnabled = true; // включаем кнопку конвертации 
            }
        }

        private void _start_Click(object sender, RoutedEventArgs e)
        {

            odbc = new OdbcConnection(@"Driver={Microsoft dBase Driver (*.dbf)}; SourceType=DBF;DefaultDir="+dbfDirectory+";Exclusive=No; Collate=Machine;NULL=NO;DELETED=NO; BACKGROUNDFETCH=NO"); // Подключение к DBF
            try
            {
                odbc.Open(); // Открываем подключение 
                _command = new OdbcCommand(_select + dbfName, odbc); // Отпавляем команду
                _reader = _command.ExecuteReader(); // Считываем ответ
                dataTable = _reader.GetSchemaTable(); // Считываем Схему таблицы 
                _fileStream = new StreamWriter(sqlDirectory + sqlName, true); // файл куда будем писать и сохранять
                _fileStream.WriteLine("CREATE TABLE "+ _dataBaseName.Text +"." + _namefile +"("); // Шаблон первой строки SQL
                str = new StringBuilder(); // Экземпляр StringBuilder 

                foreach (DataRow row in dataTable.Rows) // Прыгаем по всей схеме таблицы
                {
                    _columnName = row.Field<string>("ColumnName"); // Имя колонки в DBF файле
                    _columnType = new Operation().ConvertType(row.Field<Type>("DataType").Name); // Тип Колонки в DBF
                    str.Append(_columnName + "     " + _columnType + ","); // Загоняем все в StringBuilder 
                }

                var _newText = new Operation().AddLineString(str); // Загоняем все в один текст через AddLineString разбиваем на строки
                _fileStream.WriteLine(_newText); // записываем в файл
                _fileStream.WriteLine(");"); // добавляем закрывающую скобку в последней строке 
                while (_reader.Read()) // Читаем
                {
                   for (int i = 0; i < _reader.FieldCount; i++) // Пока читаем
                    {
                        var ss = _reader[i]; // записываем во временную переменную данные
                        if (ss is double) // если тип double 
                        {
                            _insertText += ss + ","; // загоняем инфу
                        }
                        if (ss is string) // если тип string 
                        {
                            _insertText += "\'" + ss + "\',"; // загоняем инфу и добавляем кавычки
                        }
                        if (ss is DateTime) // если тип DateTime 
                        {
                            string date = ss.ToString(); // загоняем в string 
                            DateTime _date = DateTime.Parse(date); // потом парсим в DateTime
                            date = _date.ToString("yyyy-MM-dd"); // потом в дату все дело выводим
                            
                            _insertText += "\'" + date + "\',"; // загоняем инфу 
                            // а все выше ИНДУСКИЙ КОД :D 
                        }
                        if (ss.ToString() == "") // ну и если пустые строки
                        {
                            _insertText += "\' \',"; // загоняем пустоту, без нее никак :D
                        }
                        else // иначе трах-ба-бах 
                        {
                            _insertText += ss + ","; // тоже загоняем
                         }
                        
                    }
                    _insertText = _insertText.TrimEnd(','); // удаляем запятую в конце
                    _fileStream.WriteLine("insert into " + _dataBaseName.Text + "."+ _namefile + " values (" + _insertText + ");"); // загоняем в наш документ
                    _insertText = string.Empty; // обнуляемся
                }

                _fileStream.Close(); // как все загнали, закрываемся
            }
            catch (Exception er) // Мало ли
            {
                MessageBox.Show("ОШИБКА ЕПТ ТВОЮ МАТЬ: " + er.ToString());
            }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTip = "Имя Вашей базы данных, в которую будет загружаться данная таблица";
        }
    }
}
