using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBFtoSQL
{
    class Operation
    {
        struct _type // структура с типами SQL 
        {
            public const string _int = "int";
            public const string _varchar = "varchar(255)";
            public const string _Datetime = "datetime";
        }
        private StringBuilder _str; // StringBuilder для работы с строками
        private string _newText = string.Empty; // Для создания нового текста 

        /// <summary>
        /// Преобрузет типы DBF в типы SQL 
        /// </summary>
        /// <param name="type">DBF тип в формате string</param>
        /// <returns></returns>
        public string  ConvertType(string type)
        {
            switch(type)
            {
                case "Double":
                    return _type._int; // если пришел Double отправляем int
                case "String":
                    return _type._varchar; // если пришел String отправляем varchar(255)
                case "DateTime":
                    return _type._Datetime; // если пришел DateTime, отправляем datetime
            }
            return type; // а тут мы ругаемся матом, но так НЕЛЬЗЯ!
        }

        public string AddLineString(StringBuilder paramter)
        {
            _str = new StringBuilder();
            paramter.ToString().TrimEnd(',');
            string[] _text = paramter.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i<_text.Length; i++) { _str.AppendLine(_text[i]+","); }
            _newText = _str.ToString();
            _newText = _newText.TrimEnd('\n', '\r',',');
            return _newText;
        }
    }
}
